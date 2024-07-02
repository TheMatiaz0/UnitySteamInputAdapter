using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnitySteamInputAdapter;
using UnitySteamInputAdapter.Utils;

public class Sample : MonoBehaviour
{
    private Rect _deviceRect = new Rect(10, 10, 400, 0);
    private Rect _buttonRect = new Rect(400, 10, 400, 0);

    private void Awake()
    {
        SteamAPI.Init();
    }

    private void OnDestroy()
    {
        SteamAPI.Shutdown();
    }

    private void OnGUI()
    {
        // Show connected devices
        _deviceRect = GUILayout.Window(0, _deviceRect, id =>
        {
            var devices = InputSystem.devices;
            for (int i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                var steamDevice = SteamInputAdapter.GetSteamInputDevice(device);
                GUILayout.Label($"{i}:\nUnity:{device.name}\nSteam:{steamDevice}");
            }
            GUI.DragWindow();
        }, "Connected devices");

        // Show input controls. Convert Unity InputControl to Steam InputActionOrigin.
        _buttonRect = GUILayout.Window(1, _buttonRect, id =>
        {
            var devices = InputSystem.devices;
            foreach (var device in devices)
            {
                if (device is Gamepad gamepad)
                {
                    foreach (var control in gamepad.allControls)
                    {
                        var steamAction = SteamInputAdapter.GetSteamInputAction(control);
                        GUILayout.Label($"{InputSystemUtility.GetInputControlLocalPath(control)}({control.displayName}):\n{steamAction}");
                    }
                }
            }
            GUI.DragWindow();
        }, "Unity to Steam");
    }
}
