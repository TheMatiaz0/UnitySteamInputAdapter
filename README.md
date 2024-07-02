# Steam Input Adapter
Change controller input from Unity(InputSystem) to Steam(InputAction). You can get SteamInputActionOrigin from InputControl.

This package is useful if you want to use the InputSystem but partially use SteamInput.
For example, you can generate button information to pass to `SteamInput.GetGlyphPNGForActionOrigin()`.

![image](https://github.com/eviltwo/UnitySteamInputAdapter/assets/7721151/73e78a15-4096-4467-8a72-d89027b821fb)

Please note that we have not tested this with all controllers. Xbox controllers and PS5 controllers have been tested.

# Require packages
- InputSystem (Unity)
- [Steamworks.NET](https://github.com/rlabrecque/Steamworks.NET)

# Install with UPM
```
https://github.com/eviltwo/UnitySteamInputAdapter.git?path=UnitySteamInputAdapter/Assets/UnitySteamInputAdapter
```

# Code example
```
var unityInputControl = _inputActionReference.action.controls[0];
var steamInputAction = SteamInputAdapter.GetSteamInputAction(unityInputControl);
Debug.Log($"{unityInputControl.name}: {steamInputAction}");

// Output
// buttonNorth: k_EInputActionOrigin_PS5_Triangle
```
