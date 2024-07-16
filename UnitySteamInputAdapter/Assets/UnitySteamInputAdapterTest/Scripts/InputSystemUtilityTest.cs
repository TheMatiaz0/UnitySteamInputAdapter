using NUnit.Framework;
using UnityEngine;
using UnitySteamInputAdapter.Utils;

namespace UnitySteamInputAdapterTest
{
    public class InputSystemUtilityTest : MonoBehaviour
    {
        [TestCase("/XInputController/buttonSouth", "buttonSouth")]
        [TestCase("/XInputController/dpad/right", "dpad/right")]
        [TestCase("/XInputController/<Button>", "<Button>")]
        [TestCase("/XInputController/{Submit}", "{Submit}")]
        [TestCase("/XInputController/#(a)", "#(a)")]
        [TestCase("/<Gamepad>/buttonSouth", "buttonSouth")]
        [TestCase("/<Gamepad>/buttonSouth", "buttonSouth")]
        [TestCase("XInputController/buttonSouth", "buttonSouth")]
        [TestCase("*/buttonSouth", "buttonSouth")]
        public void RemoveRootFromPath(string input, string expected)
        {
            var result = InputSystemUtility.RemoveRootFromPath(input);
            Assert.AreEqual(expected, result);
        }

        [TestCase("/XInputController/buttonSouth", false)]
        [TestCase("/XInputController/dpad/right", false)]
        [TestCase("/XInputController/<Button>", true)]
        [TestCase("/XInputController/{Submit}", true)]
        [TestCase("/XInputController/#(a)", true)]
        public void HasPathComponent(string input, bool expected)
        {
            var result = InputSystemUtility.HasPathComponent(input);
            Assert.AreEqual(expected, result);
        }
    }
}
