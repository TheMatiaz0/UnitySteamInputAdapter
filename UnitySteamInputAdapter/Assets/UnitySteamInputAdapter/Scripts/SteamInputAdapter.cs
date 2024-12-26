#if SUPPORT_INPUTSYSTEM && SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
using System;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
#if UNITY_SWITCH
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
        public static EInputActionOrigin GetSteamInputAction(InputControl inputControl)
        {
            // Get target device
            var device = GetSteamInputDevice(inputControl.device);
            if (device == ESteamInputType.k_ESteamInputType_Unknown)
            {
                return EInputActionOrigin.k_EInputActionOrigin_None;
            }

            // Get base input action (almost like XInput)
            var baseInputActionOrigin = GetBaseSteamInputAction(inputControl);
            if (baseInputActionOrigin == EInputActionOrigin.k_EInputActionOrigin_None)
            {
                return EInputActionOrigin.k_EInputActionOrigin_None;
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
        public static EInputActionOrigin GetSteamInputAction(InputDevice inputDevice, string controlPath)
        {
            // Get target device
            var device = GetSteamInputDevice(inputDevice);
            if (device == ESteamInputType.k_ESteamInputType_Unknown)
            {
                return EInputActionOrigin.k_EInputActionOrigin_None;
            }

            if (string.IsNullOrEmpty(controlPath))
            {
                return EInputActionOrigin.k_EInputActionOrigin_None;
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
                return EInputActionOrigin.k_EInputActionOrigin_None;
            }

            // Get base input action (almost like XInput)
            var baseInputActionOrigin = GetBaseSteamInputAction(controlLocalPath);
            if (baseInputActionOrigin == EInputActionOrigin.k_EInputActionOrigin_None)
            {
                return EInputActionOrigin.k_EInputActionOrigin_None;
            }

            // Translate base input to target device input
            return SteamInput.TranslateActionOrigin(device, baseInputActionOrigin);
        }

        /// <summary>
        /// Get SteamInputType from UnityInputDevice.
        /// </summary>
        /// <param name="inputDevice">Unity InputDevice</param>
        /// <returns>Steam InputType. If conversion fails, <see cref="ESteamInputType.k_ESteamInputType_Unknown"/> is returned.</returns>
        public static ESteamInputType GetSteamInputDevice(InputDevice inputDevice)
        {
            if (TryGetHijackedSteamInputDevice(inputDevice, out var result))
            {
                return result;
            }

            switch (inputDevice)
            {
                case XInputController:
                    return ESteamInputType.k_ESteamInputType_XBox360Controller;

                case DualSenseGamepadHID:
                    return ESteamInputType.k_ESteamInputType_PS5Controller;

                case DualShockGamepad:
                    return ESteamInputType.k_ESteamInputType_PS4Controller;
#if UNITY_SWITCH
                case SwitchProControllerHID:
                    return ESteamInputType.k_ESteamInputType_SwitchProController;
#endif
                case Gamepad:
                    return ESteamInputType.k_ESteamInputType_GenericGamepad;

                default:
                    return ESteamInputType.k_ESteamInputType_Unknown;
            }
        }

        private static readonly InputHandle_t[] InputHandleBuffer = new InputHandle_t[Constants.STEAM_INPUT_MAX_COUNT];

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
        public static bool TryGetHijackedSteamInputDevice(InputDevice inputDevice, out ESteamInputType result)
        {
            if (inputDevice is not XInputController)
            {
                result = ESteamInputType.k_ESteamInputType_Unknown;
                return false;
            }

            var steamDeviceCount = SteamInput.GetConnectedControllers(InputHandleBuffer);
            if (steamDeviceCount == 0)
            {
                result = ESteamInputType.k_ESteamInputType_Unknown;
                return false;
            }

            var capabilities = inputDevice.description.capabilities;
            if (string.IsNullOrEmpty(capabilities))
            {
                result = ESteamInputType.k_ESteamInputType_Unknown;
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
                result = ESteamInputType.k_ESteamInputType_Unknown;
                return false;
            }

            if (capabilities != null && capabilitiesValue.userIndex != Capabilities.InvalidValue)
            {
                for (int i = 0; i < steamDeviceCount; i++)
                {
                    var inputHandle = InputHandleBuffer[i];
                    var steamDeviceIndex = SteamInput.GetGamepadIndexForController(inputHandle);
                    if (steamDeviceIndex == capabilitiesValue.userIndex)
                    {
                        result = SteamInput.GetInputTypeForHandle(inputHandle);
                        return true;
                    }
                }
            }

            result = ESteamInputType.k_ESteamInputType_Unknown;
            return false;
        }

        /// <summary>
        /// Get SteamInputActionOrigin from UnityInputControl for input translation.
        /// Result is almost like XInput.
        /// </summary>
        private static EInputActionOrigin GetBaseSteamInputAction(InputControl inputControl)
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
        private static EInputActionOrigin GetBaseSteamInputAction(string controlLocalPath)
        {
            switch (controlLocalPath)
            {
                // Common controls
                case "buttonSouth":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_A;

                case "buttonEast":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_B;

                case "buttonWest":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_X;

                case "buttonNorth":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_Y;

                case "leftShoulder":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftBumper;

                case "rightShoulder":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_RightBumper;

                case "start":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_Start;

                case "select":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_Back;

                case "leftTrigger":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftTrigger_Pull;

                case "leftTriggerButton":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftTrigger_Click;

                case "rightTrigger":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_RightTrigger_Pull;

                case "rightTriggerButton":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_RightTrigger_Click;

                case "leftStick":
                case "leftStick/x":
                case "leftStick/y":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftStick_Move;

                case "leftStickPress":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftStick_Click;

                case "leftStick/up":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftStick_DPadNorth;

                case "leftStick/down":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftStick_DPadSouth;

                case "leftStick/left":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftStick_DPadWest;

                case "leftStick/right":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_LeftStick_DPadEast;

                case "rightStick":
                case "rightStick/x":
                case "rightStick/y":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_RightStick_Move;

                case "rightStickPress":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_RightStick_Click;

                case "rightStick/up":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_RightStick_DPadNorth;

                case "rightStick/down":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_RightStick_DPadSouth;

                case "rightStick/left":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_RightStick_DPadWest;

                case "rightStick/right":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_RightStick_DPadEast;

                case "dpad/up":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_DPad_North;

                case "dpad/down":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_DPad_South;

                case "dpad/left":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_DPad_West;

                case "dpad/right":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_DPad_East;

                case "dpad":
                case "dpad/x":
                case "dpad/y":
                    return EInputActionOrigin.k_EInputActionOrigin_XBox360_DPad_Move;

                // DualSense controls
                case "micButton":
                    return EInputActionOrigin.k_EInputActionOrigin_PS5_Mute;

                case "touchpadButton":
                    return EInputActionOrigin.k_EInputActionOrigin_PS5_CenterPad_Click;

                // Switch Pro controls
                case "capture":
                    return EInputActionOrigin.k_EInputActionOrigin_Switch_Capture;

                default:
                    return EInputActionOrigin.k_EInputActionOrigin_None;
            }
        }
    }
}
#endif
