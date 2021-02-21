## [0.1.75] - 2020-02-21

### Added
- IRepeatView
- X button to discard edits on a menu
- Navigator.GoBack
- Navigator.DebugVisit
- VioletButton.showModal
- VioletButton.closeModal
- VioletScreen.isEditing

## [0.1.74] - 2020-11-10

### Added
- Navigator 'Lose' button to discard changes and revert currently edited prefab
- Navigator 'Delete unused screens'

### Changed
- ScreenId is now serialized to json instead of binary so that it can be edited by hand

## [0.1.73] - 2020-10-13

### Added
- Navigator.ShowOverlay
- Navigator.HideOverlay

### Changed
- View now uses EditorUpdate instead of magic method Update, allowing subclasses to define the method themselves without using an override in editor.
- PrefabApplier no longer breaks builds :grimacing:
- StateMonoBehaviour makes all magic methods virtual and overridable

## [0.1.72] - 2020-09-04

### Changed
- Fixed compilation error on making builds

## [0.1.72] - 2020-08-16

### Changed
- ClodeModal => HideModal

## [0.1.71] - 2020-08-11

### Changed
- RepeatView adds the prefab at the beginning, duplicates index 0
- ScreenId uses the path Assets/Plugins/VioletUI/ScreenId.bytes

## [0.1.70] - 2020-08-11

### Added
- ScreenId now uses Assets/Menus/ScreenId.bytes as a persistent source of truth for package

## [0.1.69] - 2020-08-10

### Changed
- RepeatView now adds an inactive prefab to the scene to make it easier to edit

## [0.1.68] - 2020-08-10

### Added
- Navigator.OnScreenAdded

### Changed
- Screen => VioletScreen
- Fix user-initiated render to always use forceRender

## [0.1.60] - 2020-07-26

### Added
- VioletButton
- Violet.Log

## [0.1.50] - 2020-07-25

### Added
- Navigator
- Screen

## [0.1.42] - 2020-07-21

### Added
- ChildView
- StateMonobehaviour

## [0.1.0] - 2020-07-19

### Added
- View
- RepeatView