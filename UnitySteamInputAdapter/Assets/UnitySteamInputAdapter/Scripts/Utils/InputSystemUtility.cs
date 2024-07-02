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

        public static string GetInputControlLocalPath(string inputControlPath)
        {
            if (string.IsNullOrEmpty(inputControlPath))
            {
                return null;
            }
            var pathComponents = InputControlPath.Parse(inputControlPath);
            var enumerator = pathComponents.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                return null;
            }
            var device = enumerator.Current.name;
            if (inputControlPath.Length <= device.Length + 2)
            {
                return null;
            }
            return inputControlPath.Substring(device.Length + 2);
        }
    }
}
#endif
