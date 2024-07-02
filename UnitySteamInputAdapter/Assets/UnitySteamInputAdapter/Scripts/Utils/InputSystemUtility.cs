#if SUPPORT_INPUTSYSTEM
using System.Text;
using UnityEngine.InputSystem;

namespace UnitySteamInputAdapter.Utils
{
    public static class InputSystemUtility
    {
        private static StringBuilder _stringBuilder = new StringBuilder();

        public static string GetInputControlLocalPath(InputControl inputControl)
        {
            return GetInputControlLocalPath(inputControl.path);
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

            _stringBuilder.Clear();
            while (enumerator.MoveNext())
            {
                if (_stringBuilder.Length > 0)
                {
                    _stringBuilder.Append(InputControlPath.Separator);
                }
                _stringBuilder.Append(enumerator.Current.name);
            }
            return _stringBuilder.ToString();
        }
    }
}
#endif
