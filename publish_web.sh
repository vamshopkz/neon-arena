#!/bin/zsh
# Браузерная версия на GitHub Pages (после BuildScript.BuildWebGL → UnityProject/Builds/WebGL).
set -e
HERE=~/CODE/neon-arena
GH=~/CODE/hay-hunter/tools/gh/bin/gh
SRC=$HERE/UnityProject/Builds/WebGL
[ -f $SRC/index.html ] || { echo "Нет сборки $SRC"; exit 1; }
LOGIN=$($GH api user -q .login)
REPO="$LOGIN/neon-arena"
DIR=$HERE/web-publish
rm -rf $DIR; mkdir -p $DIR
cp -R $SRC/ $DIR/
touch $DIR/.nojekyll
cd $DIR
git init -q -b gh-pages
git add -A
git -c user.name="$LOGIN" -c user.email="$LOGIN@users.noreply.github.com" commit -q \
  -m "Браузерная версия $(date +%Y-%m-%d\ %H:%M)" -m "Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>"
git -c credential.helper= -c credential.helper="!$GH auth git-credential" push -q -f "https://github.com/$REPO.git" gh-pages
$GH api "repos/$REPO/pages" >/dev/null 2>&1 || $GH api -X POST "repos/$REPO/pages" -f "source[branch]=gh-pages" -f "source[path]=/" >/dev/null
echo "Готово: https://$LOGIN.github.io/neon-arena/"
