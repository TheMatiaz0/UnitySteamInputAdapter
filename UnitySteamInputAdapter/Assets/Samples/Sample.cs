using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnitySteamInputAdapter;
using UnitySteamInputAdapter.Utils;

public class Sample : MonoBehaviour
{
    [SerializeField]
    private string[] _controlPaths = null;

    private Rect _deviceRect = new Rect(10, 10, 400, 0);
    private Rect _actionRect = new Rect(10, 200, 400, 0);
    private Rect _buttonRect = new Rect(400, 10, 400, 0);

    private void Awake()
    {
        SteamAPI.Init();
        SteamInput.Init(false);
    }

    private void OnDestroy()
    {
        SteamInput.Shutdown();
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

        // Show actions
        _actionRect = GUILayout.Window(1, _actionRect, id =>
        {
            for (int i = 0; i < _controlPaths.Length; i++)
            {
                var path = _controlPaths[i];
                var steamAction = SteamInputAdapter.GetSteamInputAction(Gamepad.current, path);
                GUILayout.Label($"Unity:{path}\nSteam:{steamAction}");
            }
            GUI.DragWindow();
        }, "Action");

        // Show input controls. Convert Unity InputControl to Steam InputActionOrigin.
        _buttonRect = GUILayout.Window(2, _buttonRect, id =>
        {
            var devices = InputSystem.devices;
            foreach (var device in devices)
            {
                if (device is Gamepad gamepad)
                {
                    foreach (var control in gamepad.allControls)
                    {
                        var steamAction = SteamInputAdapter.GetSteamInputAction(control);
                        GUILayout.Label($"{InputSystemUtility.RemoveRootFromPath(control.path)}({control.displayName}):\n{steamAction}");
                    }
                }
            }
            GUI.DragWindow();
        }, "Unity to Steam");
    }
}
