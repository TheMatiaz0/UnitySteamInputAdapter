# Changelog

## [1.0.1] - 2024-07-13
### Added
- Added processing to convert indirect paths to direct paths.
  - Example: "XInputController/{Submit}" -> "XInputController/buttonSouth"
### Changed
- Renamed `GetInputControlLocalPath()` to `RemoveRootFromPath()`. Internal processing was also changed.

## [1.0.0] - 2024-07-13
### Fixed
- Fixed to recognize gamepads when the user has Steam Input enabled.

## [0.9.2] - 2024-07-03
### Added
- Convert steam input action from control path.
- Fix control path parse

## [0.9.0] - 2024-07-02
### Added
- SteamInputAdapter
