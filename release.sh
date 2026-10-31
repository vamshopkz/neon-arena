#!/bin/zsh
# Публикация APK на GitHub Releases.  Использование: ./release.sh 1.1 "Что нового"
set -e
VER="$1"; NOTES="${2:-Исправления и улучшения}"
[ -z "$VER" ] && { echo "Укажите версию, например: ./release.sh 1.1 \"Что нового\""; exit 1; }
HERE=~/CODE/neon-arena
GH=~/CODE/hay-hunter/tools/gh/bin/gh
U=/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity
REPO="$($GH api user -q .login)/neon-arena"
# where the game looks for updates (baked into the build)
printf "%s" "https://github.com/$REPO/releases/latest/download/" > $HERE/UnityProject/Assets/Resources/update_base.txt
CODE=$(python3 -c "import sys; p=(sys.argv[1].split('.')+['0','0'])[:3]; print(int(p[0])*10000+int(p[1])*100+int(p[2]))" "$VER")
cd $HERE/UnityProject
if [ "$SKIP_BUILD" != "1" ]; then
  $U -batchmode -nographics -projectPath "$PWD" -executeMethod BuildScript.BuildAndroid -appVersion "$VER" -versionCode "$CODE" -quit -logFile $HERE/release.log
  grep -E "\[Build\] Android: Succeeded" $HERE/release.log >/dev/null || { echo "Сборка не удалась — см. release.log"; exit 1; }
fi
OUT=$HERE/release/$VER
mkdir -p $OUT
cp Builds/Android/NeonArena.apk $OUT/NeonArena.apk
# version.json: the game compares it with its own version and shows «ОБНОВИТЬ»; size + sha256 reject a broken download
python3 - "$VER" "$NOTES" "$OUT/version.json" "$OUT/NeonArena.apk" <<'PY'
import hashlib, json, os, sys
apk = sys.argv[4]
json.dump({"version": sys.argv[1], "notes": sys.argv[2], "apk": "NeonArena.apk", "size": os.path.getsize(apk),
           "sha256": hashlib.sha256(open(apk, "rb").read()).hexdigest()}, open(sys.argv[3], "w"), ensure_ascii=False)
PY
$GH release create "v$VER" "$OUT/NeonArena.apk" "$OUT/version.json" --repo "$REPO" --title "Неон Арена $VER" --notes "$NOTES"
echo "Готово: https://github.com/$REPO/releases/tag/v$VER"
