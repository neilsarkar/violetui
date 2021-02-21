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

cp -r ./ "$HOME/work/${projectName}/Packages/violetui/"
rm -rf "$HOME/work/${projectName}/Packages/violetui/"
rm -rf "$HOME/work/${projectName}/Packages/violetui/bash"
