#!/bin/bash

printf 'Copy code from sample project? [y/N]: '
read answer
if [ "$answer" = "y" ]; then
	cp -r ~/work/King_of_the_Hat/Packages/violetui/ .
fi

tee Runtime/Navigation/ScreenId.cs &> /dev/null <<EOF
// Names are automatically added through ScreenIdGenerator.cs, deletions are done manually :)
public enum ScreenId {
	None = 0,
}
EOF

[[ -z "$1" ]] && { echo "Not updating without changelog."; exit 0; }

{ printf "## [0.1.]"; printf "$1"; cat CHANGELOG.md; } > tmp.md && mv tmp.md CHANGELOG.md
# $(date +%F)
# npm version patch
# git push