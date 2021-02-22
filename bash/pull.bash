#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
ROOT="$DIR/.."

echo "Choose a project:"
echo "[1] king_of_the_hat"
echo "[2] Package Party"
printf "=> "
read;

case $REPLY in
	1)
		projectName="king_of_the_hat"
		;;
	2)
		projectName="Package Party"
		;;
	*)
		echo "You entered ${REPLY} but we looking for a number on the list"
		exit
		;;
esac

cp -r "$HOME/work/${projectName}/Packages/violetui/" $ROOT
tee Runtime/Navigation/ScreenId.cs &> /dev/null <<EOF
// Names are automatically added through ScreenIdGenerator.cs, deletions are done manually :)
public enum ScreenId {
	None = 0,
}
EOF
