#!/bin/zsh
# Публикация APK на GitHub Releases.  Использование: ./release.sh 1.1 "Что нового"
set -e
VER="$1"; NOTES="${2:-Исправления и улучшения}"
[ -z "$VER" ] && { echo "Укажите версию, например: ./release.sh 1.1 \"Что нового\""; exit 1; }
HERE=~/CODE/neon-arena
GH=~/CODE/hay-hunter/tools/gh/bin/gh
U=/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity
REPO="$($GH api user -q .login)/neon-arena"
CODE=$(python3 -c "import sys; p=(sys.argv[1].split('.')+['0','0'])[:3]; print(int(p[0])*10000+int(p[1])*100+int(p[2]))" "$VER")
cd $HERE/UnityProject
if [ "$SKIP_BUILD" != "1" ]; then
  $U -batchmode -nographics -projectPath "$PWD" -executeMethod BuildScript.BuildAndroid -appVersion "$VER" -versionCode "$CODE" -quit -logFile $HERE/release.log
  grep -E "\[Build\] Android: Succeeded" $HERE/release.log >/dev/null || { echo "Сборка не удалась — см. release.log"; exit 1; }
fi
mkdir -p $HERE/release/$VER
cp Builds/Android/NeonArena.apk $HERE/release/$VER/NeonArena.apk
$GH release create "v$VER" "$HERE/release/$VER/NeonArena.apk" --repo "$REPO" --title "Неон Арена $VER" --notes "$NOTES"
echo "Готово: https://github.com/$REPO/releases/tag/v$VER"
