#!/bin/bash

printf 'Copy code from sample project? [y/N]: '
read answer
if [ "$answer" = "y" ]; then
	cp -r ~/work/King_of_the_Hat/Packages/violetui/ .
fi

tee Runtime/Navigation/ScreenId.cs &> /dev/null <<EOF
// Names are automatically added through ScreenIdGenerator.cs, deletions are done manually :)
namespace VioletUI {
	public enum ScreenId {
		None = 0,
	}
}
EOF

npm version patch

git commit -am "Bump version number"
git push

