# Steam Input Adapter
Change controller input from Unity(InputSystem) to Steam(InputAction). You can get SteamInputActionOrigin from InputControl.

This package is useful if you want to use the InputSystem but partially use SteamInput.
For example, you can generate button information to pass to `SteamInput.GetGlyphPNGForActionOrigin()`.

Check out [InputGlyphs](https://github.com/eviltwo/InputGlyphs) that use this package!

![adapter](https://github.com/user-attachments/assets/5bc4d381-531d-4b30-ba14-d36b3643ec96)

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
