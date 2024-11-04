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
using AltController.Core;
using AltController.Event;

namespace AltController.Actions
{
    /// <summary>
    /// Base class for all actions
    /// </summary>
    public abstract class BaseAction
    {
        private bool _isOngoing = false;
        private ParamDefinition[] _requiredParameters;
//        private ParameterSourceArgs[] _parameterSources;
//        private ParamValueGetter[] _paramValueGetters;

        abstract public EActionType ActionType { get; }
        public ParamDefinition[] RequiredParameters { get { return _requiredParameters; } }
//        public ParameterSourceArgs[] ParameterSources { get { return _parameterSources; } }
        public bool IsOngoing { get { return _isOngoing; } protected set { _isOngoing = value; } }

        /// <summary>
        /// Name of action
        /// </summary>
        public abstract string Name { get; }
        public virtual string ShortName { get { return Name; } }
        public virtual string TinyName { get { return ""; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseAction()
        {
        }

        /// <summary>
        /// Configure the method to call to get a particular parameter value
        /// </summary>
        /// <param name="paramIndex"></param>
        /// <param name="getterMethod"></param>
        //public void ConfigureParamValueGetter(int paramIndex, ParamValueGetter getterMethod, bool enable)
        //{
        //    if (_paramValueGetters != null && paramIndex > -1 && paramIndex < _paramValueGetters.Length)
        //    {
        //        if (enable && _paramValueGetters[paramIndex] == null)
        //        {
        //            // Add method
        //            _paramValueGetters[paramIndex] += getterMethod;
        //        }
        //        else if (!enable && _paramValueGetters[paramIndex] != null)
        //        {
        //            // Remove method
        //            _paramValueGetters[paramIndex] -= getterMethod;
        //        }
        //    }
        //}

        /// <summary>
        /// Set the parameters required by the action
        /// </summary>
        /// <param name="parametersTypes"></param>
        protected void SetRequiredParameters(ParamDefinition[] requiredParameters)
        {
            // Initialise arrays for parameter types and sources and value getting methods
            _requiredParameters = requiredParameters;
            //if (requiredParameters != null)
            //{
            //    _parameterSources = new ParameterSourceArgs[requiredParameters.Length];
            //    _paramValueGetters = new ParamValueGetter[requiredParameters.Length];
            //}
            //else
            //{
            //    _parameterSources = null;
            //    _paramValueGetters = null;
            //}
        }

        /// <summary>
        /// Update internal data after a property has changed
        /// </summary>
        protected virtual void Updated()
        {
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public virtual void FromXml(XmlElement element)
        {
            // Read in the parameter sources
            //if (_parameterSources != null)
            //{
            //    XmlNodeList paramNodes = element.GetElementsByTagName("param");
            //    if (paramNodes != null)
            //    {
            //        int i = 0;
            //        foreach (XmlNode paramNode in paramNodes)
            //        {
            //            if (i < _parameterSources.Length)
            //            {
            //                XmlElement paramElement = (XmlElement)paramNode;
            //                int paramIndex = int.Parse(paramElement.GetAttribute("paramindex"));
            //                string paramName = paramElement.GetAttribute("paramname");
            //                EDataType dataType = (EDataType)Enum.Parse(typeof(EDataType), paramElement.GetAttribute("datatype"));
            //                AltControlEventArgs controlArgs = AltControlEventArgs.FromXml(paramElement);

            //                _parameterSources[i++] = new ParameterSourceArgs(paramIndex, paramName, dataType, controlArgs);
            //            }
            //            else
            //            {
            //                break;
            //            }
            //        }
            //    }
            //}

            Updated();
        }        

        /// <summary>
        /// Convert to xml representation
        /// </summary>
        /// <param name="element"></param>
        public virtual void ToXml(XmlElement element, XmlDocument doc)
        {
            // Write param sources
            //if (_parameterSources != null)
            //{
            //    foreach (ParameterSourceArgs parameterSource in _parameterSources)
            //    {
            //        XmlElement paramElement = doc.CreateElement("param");
            //        paramElement.SetAttribute("paramindex", parameterSource.ParamIndex.ToString());
            //        paramElement.SetAttribute("paramname", parameterSource.ParamName.ToString());
            //        paramElement.SetAttribute("datatype", parameterSource.DataType.ToString());
            //        AltControlEventArgs.ToXml(parameterSource.ControlArgs, paramElement, doc);

            //        element.AppendChild(paramElement);
            //    }
            //}
        }

        /// <summary>
        /// Get the values of the parameters needed by the action
        /// </summary>
        /// <param name="paramIndex"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected ParamValue GetParamValue(int paramIndex,
                                            AltControlEventArgs args)
        {
            ParamValue paramValue;
            if (_requiredParameters != null && paramIndex > -1 && paramIndex < _requiredParameters.Length)
            {
                // Get the source of the parameter value and the delegate method for getting it
                //ParameterSourceArgs paramSource = _parameterSources[paramIndex];
                //ParamValueGetter paramGetter = _paramValueGetters[paramIndex];

                // If a method has been configured for getting the value from a control, call it.
                // Otherwise, get the value from the event that has been provided.
                //if (paramGetter != null)
                //{
                //    paramValue = paramGetter(parent, paramSource);
                //}
                //else
                //{
                paramValue = args.GetParamValue(paramIndex);
                //}

                // See whether the type of value returned is what the action requires
                EDataType requiredDataType = _requiredParameters[paramIndex].DataType;                
                if (paramValue.DataType != requiredDataType)
                {
                    /*
                    object convertedValue = null;

                    if (paramValue.DataType == EDataType.Pixel && requiredDataType == EDataType.Float)
                    {
                        // Allow conversion from pixel to float
                        RECT windowRect = parent.CurrentWindowRect;
                        if (paramIndex == 0)
                        {
                            // Treat value as X screen co-ord
                            int width = windowRect.Right - windowRect.Left;
                            if (width > 0)
                            {
                                convertedValue = ((int)paramValue.Value - windowRect.Left) / (float)width;
                            }
                        }
                        else if (paramIndex == 1)
                        {
                            // Treat value as Y screen co-ord
                            int height = windowRect.Bottom - windowRect.Top;
                            if (height > 0)
                            {
                                convertedValue = ((int)paramValue.Value - windowRect.Top) / (float)height;
                            }
                        }
                    }
                    else if (paramValue.DataType == EDataType.Float && requiredDataType == EDataType.Pixel)
                    {
                        // Allow conversion from float to pixel
                        RECT windowRect = parent.CurrentWindowRect;
                        if (paramIndex == 0)
                        {
                            // Treat value as normalised X screen co-ord
                            convertedValue = windowRect.Left + (int)((windowRect.Right - windowRect.Left) * (float)paramValue.Value);
                        }
                        else if (paramIndex == 1)
                        {
                            // Treat value as normalised Y screen co-ord (top = 0, bottom = 1)
                            convertedValue = windowRect.Top + (int)((windowRect.Bottom - windowRect.Top) * (float)paramValue.Value);
                        }
                    }
                     */

                    // If we are able to do some type conversion, return the converted value, otherwise null
                    paramValue.DataType = requiredDataType;
                    paramValue.Value = null;
                }
            }
            else
            {
                // Can't return value
                paramValue = new ParamValue();
            }

            return paramValue;
        }

        // Start performing the action
        public virtual void StartAction(IStateManager parent, AltControlEventArgs args)
        {
            // Set whether or not the action needs to be continued further
            IsOngoing = false;
        }

        // Continue the action
        public virtual void ContinueAction(IStateManager parent, AltControlEventArgs args)
        {
            // Set whether or not the action needs to be continued further
            IsOngoing = false;
        }

        // Stop the action
        public virtual void StopAction(IStateManager parent)
        {
            // Set whether or not the action needs to be continued further
            IsOngoing = false;
        }
    }
}
