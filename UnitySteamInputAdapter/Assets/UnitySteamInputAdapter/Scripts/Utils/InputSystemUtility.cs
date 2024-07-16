#if SUPPORT_INPUTSYSTEM
using UnityEngine.InputSystem;

namespace UnitySteamInputAdapter.Utils
{
    public static class InputSystemUtility
    {
        public static string RemoveRootFromPath(string inputControlPath)
        {
            if (string.IsNullOrEmpty(inputControlPath))
            {
                return string.Empty;
            }
            var startIndex = inputControlPath[0] == InputControlPath.Separator ? 1 : 0;
            var separationIndex = inputControlPath.IndexOf(InputControlPath.Separator, startIndex);
            if (separationIndex == -1)
            {
                return inputControlPath;
            }

            if (separationIndex == inputControlPath.Length)
            {
                return string.Empty;
            }

            return inputControlPath.Substring(separationIndex + 1);
        }

        public static bool HasPathComponent(string path)
        {
            return path.IndexOf('<') >= 0
                || path.IndexOf('{') >= 0
                || path.IndexOf('(') >= 0;
        }
    }
}
#endif
