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
namespace AltController.Core
{
    public class VirtualKeyData
    {
        // Members
        private System.Windows.Forms.Keys _keyCode = System.Windows.Forms.Keys.None;
        private ushort _windowsScanCode;
        private string _keyName = "";
        private string _tinyName = null;

        // Properties
        public System.Windows.Forms.Keys KeyCode { get { return _keyCode; } set { _keyCode = value; } }
        public ushort WindowsScanCode { get { return _windowsScanCode; } set { _windowsScanCode = value; } }
        public string Name { get { return _keyName; } set { _keyName = value; } }
        public string TinyName
        { 
            get
            {
                if (_tinyName != null)
                {
                    return _tinyName;
                }
                else
                {
                    // Default to key name if tiny name isn't set
                    return _keyName;
                }
            } 
            set { _tinyName = value; } 
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public VirtualKeyData()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        /// <param name="keyCode"></param>
        /// <param name="windowsScanCode"></param>
        /// <param name="keyName"></param>
        public VirtualKeyData(System.Windows.Forms.Keys keyCode,
                                ushort windowsScanCode,
                                string keyName,
                                string tinyName)
        {
            _keyCode = keyCode;
            _windowsScanCode = windowsScanCode;
            _keyName = keyName;
            _tinyName = tinyName;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="keyData"></param>
        public VirtualKeyData(VirtualKeyData vk)
        {
            _keyCode = vk._keyCode;
            _windowsScanCode = vk._windowsScanCode;
            if (vk._keyName != null)
            {
                _keyName = string.Copy(vk._keyName);
            }
            if (vk._tinyName != null)
            {
                _tinyName = string.Copy(vk._tinyName);
            }
        }
    }
}
