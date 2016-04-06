using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace EventHook.Hooks.Keyboard
{
    internal class KeyboardListener : IDisposable
    {
        private readonly Dispatcher _dispatcher;

        //http://stackoverflow.com/questions/6193711/call-has-been-made-on-garbage-collected-delegate-in-c
        private readonly InterceptKeys.LowLevelKeyboardProc _hookProcDelegateToAvoidGC;
        /// <summary>
        ///     Creates global keyboard listener.
        /// </summary>
        internal KeyboardListener()
        {
            // Dispatcher thread handling the KeyDown/KeyUp events.
            _dispatcher = Dispatcher.CurrentDispatcher;

            // We have to store the LowLevelKeyboardProc, so that it is not garbage collected runtime
            _hookProcDelegateToAvoidGC = LowLevelKeyboardProc;
            // Set the hook
            _hookId = InterceptKeys.SetHook(_hookProcDelegateToAvoidGC);

            // Assign the asynchronous callback event
            _hookedKeyboardCallbackAsync = KeyboardListener_KeyboardCallbackAsync;
        }

        #region IDisposable Members

        /// <summary>
        ///     Disposes the hook.
        ///     <remarks>This call is required as it calls the UnhookWindowsHookEx.</remarks>
        /// </summary>
        public void Dispose()
        {
            InterceptKeys.UnhookWindowsHookEx(_hookId);
        }

        #endregion

        /// <summary>
        ///     Destroys global keyboard listener.
        /// </summary>
        ~KeyboardListener()
        {
            Dispose();
        }

        /// <summary>
        ///     Fired when any of the keys is pressed down.
        /// </summary>
        internal event RawKeyEventHandler KeyDown;

        /// <summary>
        ///     Fired when any of the keys is released.
        /// </summary>
        internal event RawKeyEventHandler KeyUp;

        #region Inner workings

        /// <summary>
        ///     Hook ID
        /// </summary>
        private readonly IntPtr _hookId;

        /// <summary>
        ///     Asynchronous callback hook.
        /// </summary>
        /// <param name="character">Character</param>
        /// <param name="keyEvent">Keyboard event</param>
        /// <param name="vkCode">VKCode</param>
        private delegate void KeyboardCallbackAsync(InterceptKeys.KeyEvent keyEvent, int vkCode, string character);

        /// <summary>
        ///     Actual callback hook.
        ///     <remarks>Calls asynchronously the asyncCallback.</remarks>
        /// </summary>
        /// <param name="nCode"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
                if (wParam.ToUInt32() == (int) InterceptKeys.KeyEvent.WM_KEYDOWN ||
                    wParam.ToUInt32() == (int) InterceptKeys.KeyEvent.WM_KEYUP ||
                    wParam.ToUInt32() == (int) InterceptKeys.KeyEvent.WM_SYSKEYDOWN ||
                    wParam.ToUInt32() == (int) InterceptKeys.KeyEvent.WM_SYSKEYUP)
                {
                    // Captures the character(s) pressed only on WM_KEYDOWN
                    var chars = InterceptKeys.VkCodeToString((uint) Marshal.ReadInt32(lParam),
                        (wParam.ToUInt32() == (int) InterceptKeys.KeyEvent.WM_KEYDOWN ||
                         wParam.ToUInt32() == (int) InterceptKeys.KeyEvent.WM_SYSKEYDOWN));

                    _hookedKeyboardCallbackAsync.BeginInvoke((InterceptKeys.KeyEvent) wParam.ToUInt32(),
                        Marshal.ReadInt32(lParam), chars, null, null);
                }

            return InterceptKeys.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        /// <summary>
        ///     Event to be invoked asynchronously (BeginInvoke) each time key is pressed.
        /// </summary>
        private readonly KeyboardCallbackAsync _hookedKeyboardCallbackAsync;

        /// <summary>
        ///     HookCallbackAsync procedure that calls accordingly the KeyDown or KeyUp events.
        /// </summary>
        /// <param name="keyEvent">Keyboard event</param>
        /// <param name="vkCode">VKCode</param>
        /// <param name="character">Character as string.</param>
        private void KeyboardListener_KeyboardCallbackAsync(InterceptKeys.KeyEvent keyEvent, int vkCode,
            string character)
        {
            switch (keyEvent)
            {
                // KeyDown events
                case InterceptKeys.KeyEvent.WM_KEYDOWN:
                    if (KeyDown != null)
                        _dispatcher.BeginInvoke(new RawKeyEventHandler(KeyDown), this,
                            new RawKeyEventArgs(vkCode, false, character, 0));
                    break;
                case InterceptKeys.KeyEvent.WM_SYSKEYDOWN:
                    if (KeyDown != null)
                        _dispatcher.BeginInvoke(new RawKeyEventHandler(KeyDown), this,
                            new RawKeyEventArgs(vkCode, true, character, 0));
                    break;

                // KeyUp events
                case InterceptKeys.KeyEvent.WM_KEYUP:
                    if (KeyUp != null)
                        _dispatcher.BeginInvoke(new RawKeyEventHandler(KeyUp), this,
                            new RawKeyEventArgs(vkCode, false, character, 1));
                    break;
                case InterceptKeys.KeyEvent.WM_SYSKEYUP:
                    if (KeyUp != null)
                        _dispatcher.BeginInvoke(new RawKeyEventHandler(KeyUp), this,
                            new RawKeyEventArgs(vkCode, true, character, 1));
                    break;
            }
        }

        #endregion
    }
}