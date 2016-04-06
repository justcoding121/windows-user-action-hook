namespace EventHook.Hooks.Keyboard
{
    /// <summary>
    ///     Key event
    /// </summary>
    internal enum KeyEvent
    {
        /// <summary>
        ///     Key Down
        /// </summary>
        WM_KEYDOWN = 256,

        /// <summary>
        ///     Key Up
        /// </summary>
        WM_KEYUP = 257,

        /// <summary>
        ///     System key Up
        /// </summary>
        WM_SYSKEYUP = 261,

        /// <summary>
        ///     System key Down
        /// </summary>
        WM_SYSKEYDOWN = 260
    }
}