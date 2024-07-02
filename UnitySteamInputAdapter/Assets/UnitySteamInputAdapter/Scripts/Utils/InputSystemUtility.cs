#if SUPPORT_INPUTSYSTEM
using UnityEngine.InputSystem;

namespace UnitySteamInputAdapter.Utils
{
    public static class InputSystemUtility
    {
        public static string GetInputControlLocalPath(InputControl inputControl)
        {
            return inputControl.path.Substring(inputControl.device.path.Length + 1);
        }
    }
}
#endif
