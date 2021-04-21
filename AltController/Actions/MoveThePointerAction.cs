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
using System;
using System.Xml;
using System.Windows;
using System.Windows.Forms;
using AltController.Core;
using AltController.Event;

namespace AltController.Actions
{
    /// <summary>
    /// Move the mouse pointer
    /// </summary>
    public class MoveThePointerAction : BaseAction
    {
        // Members
        private Utils _utils = new Utils();
        private bool _absoluteMove = true;
        private EDisplayArea _relativeTo = EDisplayArea.ActiveWindow;
        private bool _percentOrPixels = true;
        private double _x = 50;
        private double _y = 1;

        // State
        private Size _relativeToSize = Size.Empty;
        private Size _desktopSize = Size.Empty;

        // Properties
        public bool AbsoluteMove { get { return _absoluteMove; } set { _absoluteMove = value; Updated(); } }
        public EDisplayArea RelativeTo { get { return _relativeTo; } set { _relativeTo = value; Updated(); } }
        public bool PercentOrPixels { get { return _percentOrPixels; } set { _percentOrPixels = value; Updated(); } }
        public double X { get { return _x; } set { _x = value; Updated(); } }
        public double Y { get { return _y; } set { _y = value; Updated(); } }
        private Size RelativeToSize
        {
            get
            {
                if (_relativeToSize == Size.Empty)
                {
                    _relativeToSize = GUIUtils.GetDisplayAreaSize(_relativeTo);
                }
                return _relativeToSize;
            }
        }
        private Size DesktopSize
        {
            get
            {
                if (_desktopSize == Size.Empty)
                {
                    _desktopSize = GUIUtils.GetDisplayAreaSize(EDisplayArea.Desktop);
                }
                return _desktopSize;
            }
        }

        /// <summary>
        /// Type of action
        /// </summary>
        public override EActionType ActionType
        {
            get { return EActionType.MoveThePointer; }
        }

        /// <summary>
        /// Name of action
        /// </summary>
        public override string Name
        {
            get
            {
                string name = _absoluteMove ? Properties.Resources.String_Move_pointer_to : Properties.Resources.String_Move_pointer_by;
                name += string.Format(" ({0}, {1})",
                    _percentOrPixels ? string.Format("{0:0.0}%", _x) : string.Format("{0}px", (int)_x),
                    _percentOrPixels ? string.Format("{0:0.0}%", _y) : string.Format("{0}px", (int)_y));
                if (_percentOrPixels)
                {
                    name += " " + string.Format(Properties.Resources.String_relative_to_X, _utils.GetDisplayAreaName(_relativeTo).ToLower());
                }
                
                return name;
            }
        }

        /// <summary>
        /// Short name of action
        /// </summary>
        public override string ShortName
        {
            get
            {
                string name = _absoluteMove ? Properties.Resources.String_Move_to : Properties.Resources.String_Move_by;
                name += string.Format(" ({0}, {1})",
                    _percentOrPixels ? string.Format("{0:0.0}%", _x) : string.Format("{0}px", (int)_x),
                    _percentOrPixels ? string.Format("{0:0.0}%", _y) : string.Format("{0}px", (int)_y));
                return name;
            }
        }

        /// <summary>
        /// Action updated
        /// </summary>
        protected override void Updated()
        {
            base.Updated();

            _relativeToSize = Size.Empty;   // Triggers recalculation
        }

        /// <summary>
        /// Initialise from xml
        /// </summary>
        /// <param name="element"></param>
        public override void FromXml(XmlElement element)
        {
            _absoluteMove = bool.Parse(element.GetAttribute("isabsolutemove"));
            _percentOrPixels = bool.Parse(element.GetAttribute("ispercent"));
            _x = double.Parse(element.GetAttribute("x"), System.Globalization.CultureInfo.InvariantCulture);
            _y = double.Parse(element.GetAttribute("y"), System.Globalization.CultureInfo.InvariantCulture);
            if (element.HasAttribute("relativeto"))
            {
                // Introduced in v1.82
                _relativeTo = (EDisplayArea)Enum.Parse(typeof(EDisplayArea), element.GetAttribute("relativeto"));
            }
            else if (element.HasAttribute("iswindowcoord"))
            {
                // Legacy
                _relativeTo = bool.Parse(element.GetAttribute("iswindowcoord")) ? EDisplayArea.ActiveWindow : EDisplayArea.Desktop;
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

            element.SetAttribute("x", _x.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("y", _y.ToString(System.Globalization.CultureInfo.InvariantCulture));
            element.SetAttribute("isabsolutemove", _absoluteMove.ToString());
            element.SetAttribute("ispercent", _percentOrPixels.ToString());
            element.SetAttribute("relativeto", _relativeTo.ToString());
        }

        /// <summary>
        /// Start the action
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="args"></param>
        public override void StartAction(IStateManager parent, AltControlEventArgs args)
        {
            // Get the amount to move in pixels
            double x;
            double y;
            if (_percentOrPixels)
            {
                // Convert percent to pixels
                if (_relativeTo == EDisplayArea.ActiveWindow)
                {
                    x = 0.01 * _x * (parent.CurrentWindowRect.Right - parent.CurrentWindowRect.Left);
                    y = 0.01 * _y * (parent.CurrentWindowRect.Bottom - parent.CurrentWindowRect.Top);
                }
                else
                {
                    x = 0.01 * _x * RelativeToSize.Width;
                    y = 0.01 * _y * RelativeToSize.Height;
                }
            }
            else
            {
                // Pixels
                x = _x;
                y = _y;
            }

            // Add window / desktop / current pointer position offset
            if (_absoluteMove)
            {
                if (_relativeTo == EDisplayArea.ActiveWindow)
                {
                    x += parent.CurrentWindowRect.Left;
                    y += parent.CurrentWindowRect.Top;
                }                
            }
            else
            {
                System.Drawing.Point curPos = Cursor.Position;
                x += curPos.X / parent.DPI_X;
                y += curPos.Y / parent.DPI_Y;
            }

            // Convert to normalised co-ords
            x = (x * 0xFFFF) / DesktopSize.Width;
            y = (y * 0xFFFF) / DesktopSize.Height;

            // Check bounds
            x = Math.Max(0, Math.Min(0xFFFF, x));
            y = Math.Max(0, Math.Min(0xFFFF, y));

            // Move the mouse
            parent.MouseStateManager.MoveMouse((int)x, (int)y, true);
        }
    }
}
