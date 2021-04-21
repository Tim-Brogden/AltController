/*
Alt Controller
--------------
Copyright 2013 Tim Brogden
http://altcontroller.net

Description
-----------
A free program for mapping computer inputs, such as pointer movements and button presses, 
to actions, such as key presses. The aim of this program is to help make third-party programs,
such as computer games, more accessible to users with physical difficulties.

License
-------
This file is part of Alt Controller. 
Alt Controller is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Alt Controller is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Alt Controller.  If not, see <http://www.gnu.org/licenses/>.
*/
using System.Xml;
using System.Windows.Forms;
using AltController.Core;

namespace AltController.Actions
{
    public abstract class BaseKeyAction : BaseAction
    {
        private VirtualKeyData _virtualKey;

        public VirtualKeyData VirtualKey { get { return _virtualKey; } }
        public long UniqueKeyID
        {
            get
            {
                return _virtualKey != null ? (long)_virtualKey.KeyCode : (long)Keys.A;
            }
            set
            {
                _virtualKey = KeyUtils.GetVirtualKeyByKeyCode((Keys)value);
            }
        }

        /// <summary>
        /// Return a name for the key
        /// </summary>
        public string KeyName
        {
            get
            {
                string name;
                if (_virtualKey != null)
                {
                    name = _virtualKey.Name;
                }
                else
                {
                    name = "N/K";
                }

                return name;
            }
        }

        /// <summary>
        /// Return a tiny name for the key
        /// </summary>
        public override string TinyName
        {
            get
            {
                string tinyName;
                if (_virtualKey != null)
                {
                    tinyName = _virtualKey.TinyName;
                }
                else
                {
                    tinyName = "N/K";
                }

                return tinyName;
            }
        }


        public BaseKeyAction()
            : base()
        {
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            Keys keyCode = Keys.None;
            ushort scanCode = 0;
            if (element.HasAttribute("key"))
            {
                keyCode = (Keys)int.Parse(element.GetAttribute("key"), System.Globalization.CultureInfo.InvariantCulture);
            }
            if (element.HasAttribute("scancode"))
            {
                scanCode = ushort.Parse(element.GetAttribute("scancode"), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture);
            }

            // Find virtual key.
            // Let the scan code take precendence over the virtual key code, in order to adapt to the user's keyboard layout.
            _virtualKey = null;
            if (scanCode != 0)
            {
                _virtualKey = KeyUtils.GetVirtualKeyByScanCode(scanCode);
            }
            if (_virtualKey == null)
            {
                _virtualKey = KeyUtils.GetVirtualKeyByKeyCode(keyCode);
            }

            base.FromXml(element);
        }

        /// <summary>
        /// Convert to xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            ushort scanCode = 0;
            Keys keyCode = Keys.None;
            if (_virtualKey != null)
            {
                scanCode = _virtualKey.WindowsScanCode;
                keyCode = _virtualKey.KeyCode;
            }
            element.SetAttribute("scancode", scanCode.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("key", ((int)keyCode).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

    }
}
