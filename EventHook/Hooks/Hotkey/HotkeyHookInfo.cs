using System.Collections.Generic;
using System.Windows.Forms;

namespace EventHook.Hooks.Hotkey
{
    internal class HotkeyHookInfo : HotkeyInfo
    {
        internal const string KeyModifierAlt = "%";
        internal const string KeyModifierCtrl = "^";
        internal const string KeyModifierShift = "+";

        public HotkeyHookInfo(HotkeyInfo info) : base(info.HotkeyCombo)
        {
        }

        internal string HookString
        {
            get
            {
                return string.Format("{0}{1}{2}{3}",
                    HotkeyModifiers.HasFlag(Keys.Control) ? KeyModifierCtrl : string.Empty,
                    HotkeyModifiers.HasFlag(Keys.Shift) ? KeyModifierShift : string.Empty,
                    HotkeyModifiers.HasFlag(Keys.Alt) ? KeyModifierAlt : string.Empty,
                    HotkeyKey);
            }
        }

        private static readonly IDictionary<char, uint> Modifiers = new Dictionary<char, uint>
        {
            { '+', (uint)KeyModifier.SHIFT },
            { '%', (uint)KeyModifier.ALT },
            { '^', (uint)KeyModifier.CONTROL },
        };

        internal uint HookModifier
        {
            get
            {
                var value = HookString;
                uint result = 0;

                for (var i = 0; i < 3; i++)
                {
                    var firstChar = value[0];
                    if (Modifiers.ContainsKey(firstChar))
                    {
                        result |= Modifiers[firstChar];
                    }
                    else
                    {
                        // first character isn't a modifier symbol:
                        break;
                    }

                    // truncate first character for next iteration:
                    value = value.Substring(1);
                }

                return result;
            }
        }

        internal uint HookKey { get { return (uint)HotkeyKey; } }
    }
}