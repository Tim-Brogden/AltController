using System;
using System.Windows.Data;
using System.Windows.Forms;
using System.Globalization;
using AltController.Sys;

namespace AltController.Core
{
    [ValueConversion(typeof(long), typeof(String))]
    public class HotkeyBindingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long modifiers = ((long)value) & 0xFFFF;
            string str = "";
            if ((modifiers & WindowsAPI.MOD_ALT) != 0)
            {
                str += "Alt+";
            }
            if ((modifiers & WindowsAPI.MOD_CONTROL) != 0)
            {
                str += "Ctrl+";
            }
            if ((modifiers & WindowsAPI.MOD_SHIFT) != 0)
            {
                str += "Shift+";
            }
            if ((modifiers & WindowsAPI.MOD_WIN) != 0)
            {
                str += "Win+";
            }

            Keys keyCode = (Keys)(((long)value) >> 16);
            VirtualKeyData vk = KeyUtils.GetVirtualKeyByKeyCode(keyCode);
            if (vk != null)
            {
                str += vk.Name;
            }
            return str;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0L;   // Converting back is not supported
        }
    }
}
