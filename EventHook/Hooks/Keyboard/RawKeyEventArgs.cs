using System;
using System.Windows.Input;

namespace EventHook.Hooks.Keyboard
{
    /// <summary>
    ///     Raw KeyEvent arguments.
    /// </summary>
    internal class RawKeyEventArgs : EventArgs
    {
        /// <summary>
        ///     Unicode character of key pressed.
        /// </summary>
        internal string Character;

        /// <summary>
        ///     Released/Up(1) or Pressed/Down(0)
        /// </summary>
        internal int KeyState;

        /// <summary>
        ///     Is the hitted key system key.
        /// </summary>
        internal bool IsSysKey;

        /// <summary>
        ///     WPF Key of the key.
        /// </summary>
        internal Key Key;

        /// <summary>
        ///     VKCode of the key.
        /// </summary>
        internal int VkCode;


        /// <summary>
        ///     Create raw keyevent arguments.
        /// </summary>
        /// <param name="vkCode"></param>
        /// <param name="isSysKey"></param>
        /// <param name="character">Character</param>
        /// <param name="type"></param>
        internal RawKeyEventArgs(int vkCode, bool isSysKey, string character, int type)
        {
            VkCode = vkCode;
            IsSysKey = isSysKey;
            Character = character;
            Key = KeyInterop.KeyFromVirtualKey(vkCode);
            KeyState = type;
        }

        /// <summary>
        ///     Convert to string.
        /// </summary>
        /// <returns>Returns string representation of this key, if not possible empty string is returned.</returns>
        public override string ToString()
        {
            return Character;
        }
    }
}