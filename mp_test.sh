#!/bin/zsh
# Сетевой тест: две копии игры (лидер + друг) через интернет-реле.  ./mp_test.sh <папка>
cd ~/CODE/neon-arena
D=${1:-shots_mp}; rm -rf $D; mkdir -p $D/host $D/guest
APP=$(ls UnityProject/Builds/Mac/NeonArena.app/Contents/MacOS/*)
ROOM=T$(date +%H%M%S)
"$APP" -mptest host -id $ROOM -name Хост -shots $PWD/$D/host -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -logFile $PWD/$D/host/player.log &
sleep 3
"$APP" -mptest guest -id G$(date +%S)XYZ -room $ROOM -name Гость -shots $PWD/$D/guest -screen-width 1280 -screen-height 720 -screen-fullscreen 0 -logFile $PWD/$D/guest/player.log
wait
echo ЛИДЕР; cat $D/host/autotest.txt; echo ДРУГ; cat $D/guest/autotest.txt
