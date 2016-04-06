using System.Text;
using System.Windows.Forms;

namespace EventHook
{
    public class HotkeyInfo
    {
        private readonly Keys _hotkeyCombo;

        public HotkeyInfo(Keys hotkeyCombo)
        {
            _hotkeyCombo = hotkeyCombo;
        }

        public Keys HotkeyCombo { get { return _hotkeyCombo; } }
        public Keys HotkeyKey { get { return _hotkeyCombo & Keys.KeyCode; } }
        public Keys HotkeyModifiers { get { return _hotkeyCombo & Keys.Modifiers;} }

        public override int GetHashCode()
        {
            return _hotkeyCombo.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return _hotkeyCombo.Equals(obj);
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (_hotkeyCombo.HasFlag(Keys.Alt))
            {
                builder.Append("Alt");
                builder.Append('+');
            }
            if (_hotkeyCombo.HasFlag(Keys.Control))
            {
                builder.Append("Ctrl");
                builder.Append('+');
            }
            if (_hotkeyCombo.HasFlag(Keys.Shift))
            {
                builder.Append("Shift");
                builder.Append('+');
            }
            builder.Append(HotkeyKey);
            return builder.ToString();
        }
    }
}
