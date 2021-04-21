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
using AltController.Core;

namespace AltController.Event
{
    /// <summary>
    /// Container for a control event source and parameter index
    /// </summary>
    public class ParameterSourceArgs
    {
        private int _paramIndex;
        private string _paramName;
        private EDataType _dataType;
        private AltControlEventArgs _controlArgs;

        public int ParamIndex { get { return _paramIndex; } }
        public string ParamName { get { return _paramName; } }
        public EDataType DataType { get { return _dataType; } }
        public AltControlEventArgs ControlArgs { get { return _controlArgs; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="paramIndex"></param>
        /// <param name="dataType"></param>
        /// <param name="controlArgs"></param>
        public ParameterSourceArgs(int paramIndex, 
                                    string paramName,
                                    EDataType dataType, 
                                    AltControlEventArgs controlArgs)
        {
            _paramIndex = paramIndex;
            _paramName = paramName;
            _dataType = dataType;
            _controlArgs = controlArgs;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="paramSource"></param>
        public ParameterSourceArgs(ParameterSourceArgs paramSource)
        {
            _paramIndex = paramSource.ParamIndex;
            _paramName = paramSource.ParamName;
            _dataType = paramSource.DataType;
            if (paramSource.ControlArgs != null)
            {
                _controlArgs = new AltControlEventArgs(paramSource.ControlArgs);
            }
        }
        
        /// <summary>
        /// Get the full name of this source
        /// </summary>
        /// <returns></returns>
        public string GetFullName()
        {
            string name;
            if (_controlArgs == null)
            {
                name = Properties.Resources.String_Event + " - " + _paramName;
            }
            else
            {
                name = string.Format("{0} - {1}", _controlArgs.GetControlTypeName(), _paramName);
            }

            return name;
        }
    }
}
