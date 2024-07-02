#if SUPPORT_INPUTSYSTEM && SUPPORT_STEAMWORKS && !DISABLESTEAMWORKS
using Steamworks;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
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
        /// Get SteamInputType from UnityInputDevice.
        /// </summary>
        /// <param name="inputDevice">Unity InputDevice</param>
        /// <returns>Steam InputType. If conversion fails, <see cref="ESteamInputType.k_ESteamInputType_Unknown"/> is returned.</returns>
        public static ESteamInputType GetSteamInputDevice(InputDevice inputDevice)
        {
            switch (inputDevice)
            {
                case XInputController:
                    return ESteamInputType.k_ESteamInputType_XBox360Controller;

                case DualSenseGamepadHID:
                    return ESteamInputType.k_ESteamInputType_PS5Controller;

                case DualShockGamepad:
                    return ESteamInputType.k_ESteamInputType_PS4Controller;

                case SwitchProControllerHID:
                    return ESteamInputType.k_ESteamInputType_SwitchProController;

                case Gamepad:
                    return ESteamInputType.k_ESteamInputType_GenericGamepad;

                default:
                    return ESteamInputType.k_ESteamInputType_Unknown;
            }
        }

        /// <summary>
        /// Get SteamInputActionOrigin from UnityInputControl for input translation.
        /// Result is almost like XInput.
        /// </summary>
        /// <remarks>
        /// Unity InputControl names: No define documents. You can check the name from the Input Debugger.
        /// Steam InputActionOrigin names: https://partner.steamgames.com/doc/api/ISteamInput#EInputActionOrigin
        /// </remarks>
        private static EInputActionOrigin GetBaseSteamInputAction(InputControl inputControl)
        {
            var controlLocalPath = InputSystemUtility.GetInputControlLocalPath(inputControl);
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

                default:
                    return EInputActionOrigin.k_EInputActionOrigin_None;
            }
        }
    }
}
#endif