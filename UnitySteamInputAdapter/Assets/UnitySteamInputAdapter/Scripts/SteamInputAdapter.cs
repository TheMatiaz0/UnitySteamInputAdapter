#if SUPPORT_INPUTSYSTEM && SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
using System;
using System.Linq;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_WSA
using UnityEngine.InputSystem.Switch;
#endif
using UnityEngine.InputSystem.XInput;
using UnitySteamInputAdapter.Utils;

namespace UnitySteamInputAdapter
{
    /// <summary>
    /// This class changes the definition of controller input from the InputSystem to the SteamInputAPI.
    /// </summary>
    /// <remarks>
    /// Require SteamAPI.Init() to be called before using this class.
    /// </remarks>
    public static class SteamInputAdapter
    {
        /// <summary>
        /// Get SteamInputActionOrigin from UnityInputControl.
        /// </summary>
        /// <remarks>
        /// Require SteamAPI.Init() to be called before call this function.
        /// </remarks>
        /// <param name="inputControl">Unity InputControl</param>
        /// <returns>Steam InputActionOrigin. If conversion fails, <see cref="EInputActionOrigin.k_EInputActionOrigin_None"/> is returned.</returns>
        public static InputActionOrigin GetSteamInputAction(InputControl inputControl)
        {
            // Get target device
            var device = GetSteamInputDevice(inputControl.device);
            if (device == InputType.Unknown)
            {
                return InputActionOrigin.None;
            }

            // Get base input action (almost like XInput)
            var baseInputActionOrigin = GetBaseSteamInputAction(inputControl);
            if (baseInputActionOrigin == InputActionOrigin.None)
            {
                return InputActionOrigin.None;
            }

            // Translate base input to target device input
            return SteamInput.TranslateActionOrigin(device, baseInputActionOrigin);
        }

        /// <summary>
        /// Get SteamInputActionOrigin from UnityInputDevice and controlPath.
        /// </summary>
        /// <remarks>
        /// Require SteamAPI.Init() to be called before call this function.
        /// </remarks>
        /// <param name="inputDevice">Unity InputDevice</param>
        /// <param name="controlPath">Unity path of InputControl</param>
        /// <returns>Steam InputActionOrigin. If conversion fails, <see cref="EInputActionOrigin.k_EInputActionOrigin_None"/> is returned.</returns>
        public static InputActionOrigin GetSteamInputAction(InputDevice inputDevice, string controlPath)
        {
            // Get target device
            var device = GetSteamInputDevice(inputDevice);
            if (device == InputType.Unknown)
            {
                return InputActionOrigin.None;
            }

            if (string.IsNullOrEmpty(controlPath))
            {
                return InputActionOrigin.None;
            }

            // Get path without device name.
            // Example: "XInputController/buttonSouth" -> "buttonSouth"
            var controlLocalPath = InputSystemUtility.RemoveRootFromPath(controlPath);

            // Convert indirectory path to directory path.
            // Example: "XInputController/{Submit}" -> "XInputController/buttonSouth"
            if (InputSystemUtility.HasPathComponent(controlLocalPath))
            {
                var control = inputDevice.TryGetChildControl(controlPath);
                if (control != null)
                {
                    controlPath = control.path;
                    controlLocalPath = InputSystemUtility.RemoveRootFromPath(controlPath);
                }
            }

            if (string.IsNullOrEmpty(controlLocalPath))
            {
                return InputActionOrigin.None;
            }

            // Get base input action (almost like XInput)
            var baseInputActionOrigin = GetBaseSteamInputAction(controlLocalPath);
            if (baseInputActionOrigin == InputActionOrigin.None)
            {
                return InputActionOrigin.None;
            }

            // Translate base input to target device input
            return SteamInput.TranslateActionOrigin(device, baseInputActionOrigin);
        }

        /// <summary>
        /// Get SteamInputType from UnityInputDevice.
        /// </summary>
        /// <param name="inputDevice">Unity InputDevice</param>
        /// <returns>Steam InputType. If conversion fails, <see cref="ESteamInputType.k_ESteamInputType_Unknown"/> is returned.</returns>
        public static InputType GetSteamInputDevice(InputDevice inputDevice)
        {
            if (TryGetHijackedSteamInputDevice(inputDevice, out var result))
            {
                return result;
            }

            switch (inputDevice)
            {
                case XInputController:
                    return InputType.XBox360Controller;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_WSA
                case DualSenseGamepadHID:
                    return InputType.PS5Controller;
#endif

                case DualShockGamepad:
                    return InputType.PS4Controller;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_WSA
                case SwitchProControllerHID:
                    return InputType.SwitchProController;
#endif

                case Gamepad:
                    return InputType.GenericGamepad;

                default:
                    return InputType.Unknown;
            }
        }

        [Serializable]
        private class Capabilities
        {
            public const int InvalidValue = -1;
            public int userIndex = InvalidValue;
        }

        /// <summary>
        /// If the user enables Steam Input, all gamepads will be overridden to XInput.
        /// This function retrieves the type of gamepad before it is overridden.
        /// </summary>
        /// <remarks>
        /// If multiple gamepads are connected, the type of gamepad returned by this function might be swapped.
        /// Only Steam can improve this, and there is nothing that Unity or we can do about it.
        /// Users can resolve this issue by disabling Steam Input. Alternatively, restarting the game or unplugging and replugging all the gamepads may solve the problem.
        /// </remarks>
        public static bool TryGetHijackedSteamInputDevice(InputDevice inputDevice, out InputType result)
        {
            if (inputDevice is not XInputController)
            {
                result = InputType.Unknown;
                return false;
            }

            var steamDeviceCount = SteamInput.Controllers.Count();
            if (steamDeviceCount == 0)
            {
                result = InputType.Unknown;
                return false;
            }

            var capabilities = inputDevice.description.capabilities;
            if (string.IsNullOrEmpty(capabilities))
            {
                result = InputType.Unknown;
                return false;
            }

            Capabilities capabilitiesValue;
            try
            {
                capabilitiesValue = JsonUtility.FromJson<Capabilities>(capabilities);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                result = InputType.Unknown;
                return false;
            }

            if (capabilities != null && capabilitiesValue.userIndex != Capabilities.InvalidValue)
            {
                var controller = SteamInput.Controllers.FirstOrDefault(c => c.GamepadIndex == capabilitiesValue.userIndex);

                if (controller != null)
                {
                    result = controller.InputType;
                    return true;
                }
            }

            result = InputType.Unknown;
            return false;
        }

        /// <summary>
        /// Get SteamInputActionOrigin from UnityInputControl for input translation.
        /// Result is almost like XInput.
        /// </summary>
        private static InputActionOrigin GetBaseSteamInputAction(InputControl inputControl)
        {
            var controlLocalPath = InputSystemUtility.RemoveRootFromPath(inputControl.path);
            return GetBaseSteamInputAction(controlLocalPath);
        }

        /// <summary>
        /// Get SteamInputActionOrigin from path for input translation.
        /// Result is almost like XInput.
        /// </summary>
        /// <remarks>
        /// Unity InputControl names: No define documents. You can check the name from the Input Debugger.
        /// Steam InputActionOrigin names: https://partner.steamgames.com/doc/api/ISteamInput#EInputActionOrigin
        /// </remarks>
        private static InputActionOrigin GetBaseSteamInputAction(string controlLocalPath)
        {
            switch (controlLocalPath)
            {
                // Common controls
                case "buttonSouth":
                    return InputActionOrigin.XBox360_A;

                case "buttonEast":
                    return InputActionOrigin.XBox360_B;

                case "buttonWest":
                    return InputActionOrigin.XBox360_X;

                case "buttonNorth":
                    return InputActionOrigin.XBox360_Y;

                case "leftShoulder":
                    return InputActionOrigin.XBox360_LeftBumper;

                case "rightShoulder":
                    return InputActionOrigin.XBox360_RightBumper;

                case "start":
                    return InputActionOrigin.XBox360_Start;

                case "select":
                    return InputActionOrigin.XBox360_Back;

                case "leftTrigger":
                    return InputActionOrigin.XBox360_LeftTrigger_Pull;

                case "leftTriggerButton":
                    return InputActionOrigin.XBox360_LeftTrigger_Click;

                case "rightTrigger":
                    return InputActionOrigin.XBox360_RightTrigger_Pull;

                case "rightTriggerButton":
                    return InputActionOrigin.XBox360_RightTrigger_Click;

                case "leftStick":
                case "leftStick/x":
                case "leftStick/y":
                    return InputActionOrigin.XBox360_LeftStick_Move;

                case "leftStickPress":
                    return InputActionOrigin.XBox360_LeftStick_Click;

                case "leftStick/up":
                    return InputActionOrigin.XBox360_LeftStick_DPadNorth;

                case "leftStick/down":
                    return InputActionOrigin.XBox360_LeftStick_DPadSouth;

                case "leftStick/left":
                    return InputActionOrigin.XBox360_LeftStick_DPadWest;

                case "leftStick/right":
                    return InputActionOrigin.XBox360_LeftStick_DPadEast;

                case "rightStick":
                case "rightStick/x":
                case "rightStick/y":
                    return InputActionOrigin.XBox360_RightStick_Move;

                case "rightStickPress":
                    return InputActionOrigin.XBox360_RightStick_Click;

                case "rightStick/up":
                    return InputActionOrigin.XBox360_RightStick_DPadNorth;

                case "rightStick/down":
                    return InputActionOrigin.XBox360_RightStick_DPadSouth;

                case "rightStick/left":
                    return InputActionOrigin.XBox360_RightStick_DPadWest;

                case "rightStick/right":
                    return InputActionOrigin.XBox360_RightStick_DPadEast;

                case "dpad/up":
                    return InputActionOrigin.XBox360_DPad_North;

                case "dpad/down":
                    return InputActionOrigin.XBox360_DPad_South;

                case "dpad/left":
                    return InputActionOrigin.XBox360_DPad_West;

                case "dpad/right":
                    return InputActionOrigin.XBox360_DPad_East;

                case "dpad":
                case "dpad/x":
                case "dpad/y":
                    return InputActionOrigin.XBox360_DPad_Move;

                // DualSense controls
                case "micButton":
                    return InputActionOrigin.PS5_Mute;

                case "touchpadButton":
                    return InputActionOrigin.PS5_CenterPad_Click;

                // Switch Pro controls
                case "capture":
                    return InputActionOrigin.Switch_Capture;

                default:
                    return InputActionOrigin.None;
            }
        }
    }
}
#endif
