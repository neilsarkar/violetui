cp -r ~/work/King_of_the_Hat/Packages/violetui/ .
tee Runtime/Navigation/ScreenId.cs &> /dev/null <<EOF
// Names are automatically added through ScreenIdGenerator.cs, deletions are done manually :)
namespace VioletUI {
	public enum ScreenId {
		None = 0,
	}
}
EOF
