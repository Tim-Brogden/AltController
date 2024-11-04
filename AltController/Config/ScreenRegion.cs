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
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using AltController.Core;

namespace AltController.Config
{
    /// <summary>
    /// Defines a region of the screen
    /// </summary>
    public class ScreenRegion : NamedItem
    {
        // Members
        private EShape _shape = EShape.Rectangle;
        private Rect _rect = new Rect(0.0, 0.0, 1.0, 1.0);
        private double _holeSizeFraction = 0.5;
        private double _startAngleDeg = 0.0;
        private double _sweepAngleDeg = 90.0;
        private string _colour = "";
        private string _backgroundColour = "";
        private string _bgImage = "";
        private double _translucency = -1.0;    // Between 0 and 1 if set, or -1 to signify default translucency
        private LogicalState _showInState = new LogicalState();

        // Properties
        public EShape Shape { get { return _shape; } set { _shape = value; } }
        public Rect Rectangle { get { return _rect; } set { _rect = value; } }
        public double HoleSize { get { return _holeSizeFraction; } set { _holeSizeFraction = value; } }
        public double StartAngle { get { return _startAngleDeg; } set { _startAngleDeg = value; } }
        public double SweepAngle { get { return _sweepAngleDeg; } set { _sweepAngleDeg = value; } }
        public string Colour { get { return _colour; } set { _colour = value; } }
        public string BackgroundColour { get { return _backgroundColour; } set { _backgroundColour = value; } }
        public string BackgroundImage { get { return _bgImage; } set { _bgImage = value; } }
        public double Translucency { get { return _translucency; } set { _translucency = value; } }
        public LogicalState ShowInState { get { return _showInState; } set { _showInState = value; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ScreenRegion()
        {
        }

        /// <summary>
        /// Full constructor
        /// </summary>
        public ScreenRegion(long id, 
                            string name,
                            EShape shape,
                            Rect rect,
                            double holeSizeFraction,
                            double startAngleDeg,
                            double sweepAngleDeg,
                            string colour,
                            string backgroundColour,
                            string bgImage,
                            double translucency,
                            LogicalState showInState)
            :base(id,name)
        {
            _shape = shape;
            _rect = rect;
            _holeSizeFraction = holeSizeFraction;
            _startAngleDeg = startAngleDeg;
            _sweepAngleDeg = sweepAngleDeg;
            _colour = colour;
            _backgroundColour = backgroundColour;
            _bgImage = bgImage;
            _translucency = translucency;
            _showInState = showInState;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="region"></param>
        public ScreenRegion(ScreenRegion region)
            : base(region.ID, region.Name)
        {
            _shape = region._shape;
            _rect = region._rect;
            _holeSizeFraction = region._holeSizeFraction;
            _startAngleDeg = region._startAngleDeg;
            _sweepAngleDeg = region._sweepAngleDeg;
            _colour = region._colour;
            _backgroundColour = region._backgroundColour;
            _bgImage = region._bgImage;
            _translucency = region._translucency;
            _showInState = new LogicalState(region._showInState);
        }
        
        /// <summary>
        /// Return whether the specified region is the same as this one
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public bool IsSameShapeAs(ScreenRegion region)
        {
            bool isSame = (_shape == region.Shape) && (_rect == region.Rectangle);
            if (isSame)
            {
                switch (_shape)
                {
                    case EShape.Annulus:
                        isSame &= (_holeSizeFraction == region.HoleSize); break;
                    case EShape.AnnulusSector:
                        isSame &= (_holeSizeFraction == region.HoleSize) &&
                                    (_startAngleDeg == region.StartAngle) &&
                                    (_sweepAngleDeg == region.SweepAngle); break;
                    case EShape.EllipseSector:
                        isSame &= (_startAngleDeg == region.StartAngle) &&
                                    (_sweepAngleDeg == region.SweepAngle); break;
                }
            }

            return isSame;
        }

        /// <summary>
        /// Return whether a point is inside the region or not
        /// </summary>
        /// <returns></returns>
        public bool Contains(Point point)
        {
            bool isInside = false;
            switch (_shape)
            {
                case EShape.Rectangle:
                default:
                    isInside = _rect.Contains(point);
                    break;
                case EShape.Ellipse:
                    {
                        double radius = CalcRadius(_rect, point);
                        isInside = (radius < 1.0);
                    }
                    break;
                case EShape.EllipseSector:
                    {
                        double radius = CalcRadius(_rect, point);
                        if (radius < 1.0)
                        {
                            // Inside ellipse - now see if inside sector
                            double xRadius = 0.5 * _rect.Width;
                            if (radius > 0.0 && xRadius > 0.0)
                            {
                                double cos = Math.Max(-1.0, Math.Min(0.999999, (point.X - (_rect.X + xRadius)) / (radius * xRadius)));
                                double angleDeg = Math.Acos(cos) * 180.0 / Math.PI;
                                if (point.Y > _rect.Y + 0.5 * _rect.Height)
                                {
                                    angleDeg = 360.0 - angleDeg;
                                }

                                double endAngle = _startAngleDeg + _sweepAngleDeg;
                                if (endAngle < 360.0)
                                {
                                    isInside = (angleDeg > _startAngleDeg) && (angleDeg < endAngle);
                                }
                                else
                                {
                                    isInside = (angleDeg > _startAngleDeg) || (angleDeg < endAngle - 360.0);
                                }
                            }
                            else
                            {
                                isInside = true;
                            }
                        }
                    }
                    break;
                case EShape.Annulus:
                    {
                        double radius = CalcRadius(_rect, point);
                        isInside = (radius < 1.0) && (radius > _holeSizeFraction);
                    }
                    break;
                case EShape.AnnulusSector:
                    {
                        double radius = CalcRadius(_rect, point);
                        if ((radius < 1.0) && (radius > _holeSizeFraction))
                        {
                            // Inside annulus - now see if inside sector
                             double xRadius = 0.5 * _rect.Width;
                             if (xRadius > 0.0)
                             {
                                 double cos = Math.Max(-1.0, Math.Min(0.999999, (point.X - (_rect.X + xRadius)) / (radius * xRadius)));
                                 double angleDeg = Math.Acos(cos) * 180.0 / Math.PI;
                                 if (point.Y > _rect.Y + 0.5 * _rect.Height)
                                 {
                                     angleDeg = 360.0 - angleDeg;
                                 }

                                 double endAngle = _startAngleDeg + _sweepAngleDeg;
                                 if (endAngle < 360.0)
                                 {
                                     isInside = (angleDeg > _startAngleDeg) && (angleDeg < endAngle);
                                 }
                                 else
                                 {
                                     isInside = (angleDeg > _startAngleDeg) || (angleDeg < endAngle - 360.0);
                                 }
                             }                             
                        }
                    }
                    break;
            }

            return isInside;
        }

        /// <summary>
        /// Calculate the (x / xRadius) ^ 2 + (y / yRadius) ^ 2 for the vector from the centre of an ellipse (defined by a rectangle) to a point
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        private double CalcRadius(Rect rect, Point point)
        {
            double xRadius = 0.5 * rect.Width;
            double yRadius = 0.5 * rect.Height;
            double radius = 0.0;
            if (xRadius > 0.0 && yRadius > 0.0)
            {
                double xNorm = (point.X - (rect.X + xRadius)) / xRadius;
                double yNorm = (point.Y - (rect.Y + yRadius)) / yRadius;
                radius = xNorm * xNorm + yNorm * yNorm;
                if (radius > 0.0)
                {
                    radius = Math.Sqrt(radius);
                }
            }
            
            return radius;
        }
       
        /// <summary>
        /// Get a geometry that represents the shape
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public Geometry GetGeometry(Rect rect)
        {
            Geometry geometry;
            switch (_shape)
            {
                case EShape.Rectangle:
                default:
                    geometry = new RectangleGeometry(rect);
                    break;
                case EShape.Ellipse:
                    geometry = new EllipseGeometry(rect);
                    break;
                case EShape.EllipseSector:
                    geometry = CreateEllipseSectorGeometry(rect);
                    break;
                case EShape.Annulus:
                    geometry = CreateAnnulusGeometry(rect);
                    break;
                case EShape.AnnulusSector:
                    geometry = CreateAnnulusSectorGeometry(rect);
                    break;
            }

            if (geometry.CanFreeze)
            {
                geometry.Freeze();
            }

            return geometry;
        }

        /// <summary>
        /// Return the normalised position where the region text should be centred
        /// </summary>
        /// <returns></returns>
        public Point GetTextPosition()
        {
            Point textPos;

            switch (_shape)
            {
                case EShape.Rectangle:
                case EShape.Ellipse:
                default:
                    {
                        textPos = new Point(0.5, 0.5);
                    }
                    break;
                case EShape.EllipseSector:
                    {
                        double textAngle = _startAngleDeg + 0.5 * _sweepAngleDeg;
                        textPos = GetPointOnStandardEllipse(new Point(0.5, 0.5), 0.25, 0.25, textAngle);
                    }
                    break;
                case EShape.Annulus:
                    {
                        // Display text near the top middle
                        double radiusFraction = 0.25 * (1.0 - _holeSizeFraction);
                        textPos = new Point(0.5, radiusFraction);
                    }
                    break;
                case EShape.AnnulusSector:
                    {
                        double textAngle = _startAngleDeg + 0.5 * _sweepAngleDeg;
                        double radiusFraction = 0.25 * (1.0 + _holeSizeFraction);
                        textPos = GetPointOnStandardEllipse(new Point(0.5, 0.5), radiusFraction, radiusFraction, textAngle);
                    }
                    break;
            }

            return textPos;
        }

        /// <summary>
        /// Create a geometry to draw an ellipse sector
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        /// <returns></returns>
        private Geometry CreateEllipseSectorGeometry(Rect rect)
        {
            Point centrePoint = new Point(rect.X + 0.5 * rect.Width, rect.Y + 0.5 * rect.Height);
            Point ellipseStart = GetPointOnEllipse(rect, _startAngleDeg);
            Point ellipseEnd = GetPointOnEllipse(rect, _startAngleDeg + _sweepAngleDeg);

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();
            figure.IsClosed = true;
            figure.StartPoint = centrePoint;
            figure.Segments.Add(new LineSegment(ellipseStart, true));
            figure.Segments.Add(new ArcSegment(ellipseEnd,
                new Size(0.5 * rect.Width, 0.5 * rect.Height), 0.0, _sweepAngleDeg > 180, SweepDirection.Counterclockwise, true));
            geometry.Figures.Add(figure);

            return geometry;
        }

        /// <summary>
        /// Create a geometry to draw an annulus
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        private Geometry CreateAnnulusGeometry(Rect rect)
        {
            Rect innerRect = new Rect();
            innerRect.X = rect.X + 0.5 * (1.0 - _holeSizeFraction) * rect.Width;
            innerRect.Y = rect.Y + 0.5 * (1.0 - _holeSizeFraction) * rect.Height;
            innerRect.Width = _holeSizeFraction * rect.Width;
            innerRect.Height = _holeSizeFraction * rect.Height;

            CombinedGeometry geometry = new CombinedGeometry();
            geometry.Geometry1 = new EllipseGeometry(rect);
            geometry.Geometry2 = new EllipseGeometry(innerRect);
            geometry.GeometryCombineMode = GeometryCombineMode.Exclude;

            return geometry;
        }

        /// <summary>
        /// Create a geometry to draw an ellipse sector
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="startAngle"></param>
        /// <param name="sweepAngle"></param>
        /// <returns></returns>
        private Geometry CreateAnnulusSectorGeometry(Rect rect)
        {
            Rect innerRect = new Rect();
            innerRect.X = rect.X + 0.5 * (1.0 - _holeSizeFraction) * rect.Width;
            innerRect.Y = rect.Y + 0.5 * (1.0 - _holeSizeFraction) * rect.Height;
            innerRect.Width = _holeSizeFraction * rect.Width;
            innerRect.Height = _holeSizeFraction * rect.Height;

            Point outerStart = GetPointOnEllipse(rect, _startAngleDeg);
            Point outerSweep = GetPointOnEllipse(rect, _startAngleDeg + _sweepAngleDeg);
            Point innerStart = GetPointOnEllipse(innerRect, _startAngleDeg);
            Point innerSweep = GetPointOnEllipse(innerRect, _startAngleDeg + _sweepAngleDeg);

            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();
            figure.IsClosed = true;
            figure.StartPoint = outerStart;
            figure.Segments.Add(new ArcSegment(outerSweep,
                new Size(0.5 * rect.Width, 0.5 * rect.Height), 0.0, _sweepAngleDeg > 180, SweepDirection.Counterclockwise, true));
            figure.Segments.Add(new LineSegment(innerSweep, true));
            figure.Segments.Add(new ArcSegment(innerStart,
                new Size(0.5 * innerRect.Width, 0.5 * innerRect.Height), 0.0, _sweepAngleDeg > 180, SweepDirection.Clockwise, true));
            geometry.Figures.Add(figure);

            return geometry;
        }

        /// <summary>
        /// Return a point on an ellipse
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        private Point GetPointOnEllipse(Rect rect, double angleDeg)
        {
            double xRadius = 0.5 * rect.Width;
            double yRadius = 0.5 * rect.Height;
            Point point = GetPointOnStandardEllipse(new Point(rect.Left + xRadius, rect.Top + yRadius), xRadius, yRadius, angleDeg);

            return point;
        }

        /// <summary>
        /// Return a point on an ellipse
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        private Point GetPointOnStandardEllipse(Point centre, double xRadius, double yRadius, double angleDeg)
        {
            double angleRad = Math.PI * angleDeg / 180.0;            
            Point point = new Point(centre.X + xRadius * Math.Cos(angleRad), centre.Y - yRadius * Math.Sin(angleRad));  

            return point;
        }

        /// <summary>
        /// Initialise from Xml
        /// </summary>
        /// <param name="modeElement"></param>
        public override void FromXml(XmlElement element)
        {
            base.FromXml(element);

            _shape = (EShape)Enum.Parse(typeof(EShape), element.GetAttribute("shape"));
            _rect = new Rect(double.Parse(element.GetAttribute("x"), CultureInfo.InvariantCulture),
                            double.Parse(element.GetAttribute("y"), CultureInfo.InvariantCulture),
                            double.Parse(element.GetAttribute("width"), CultureInfo.InvariantCulture),
                            double.Parse(element.GetAttribute("height"), CultureInfo.InvariantCulture));
            _holeSizeFraction = double.Parse(element.GetAttribute("holesize"), CultureInfo.InvariantCulture);
            _startAngleDeg = double.Parse(element.GetAttribute("startangle"), CultureInfo.InvariantCulture);
            _sweepAngleDeg = double.Parse(element.GetAttribute("sweepangle"), CultureInfo.InvariantCulture);
            _colour = element.GetAttribute("colour");
            if (element.HasAttribute("backgroundcolour"))   // Added v2.0
            {
                _backgroundColour = element.GetAttribute("backgroundcolour");
            }
            _bgImage = element.GetAttribute("bgimage");
            _translucency = double.Parse(element.GetAttribute("translucency"), CultureInfo.InvariantCulture);
            _showInState.FromXml(element);
        }

        /// <summary>
        /// Write out to Xml
        /// </summary>
        /// <param name="element"></param>
        public override void ToXml(XmlElement element, XmlDocument doc)
        {
            base.ToXml(element, doc);

            element.SetAttribute("shape", _shape.ToString());
            element.SetAttribute("x", _rect.X.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("y", _rect.Y.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("width", _rect.Width.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("height", _rect.Height.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("holesize", _holeSizeFraction.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("startangle", _startAngleDeg.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("sweepangle", _sweepAngleDeg.ToString(CultureInfo.InvariantCulture));
            element.SetAttribute("colour", _colour);
            element.SetAttribute("backgroundcolour", _backgroundColour);
            element.SetAttribute("bgimage", _bgImage);
            element.SetAttribute("translucency", _translucency.ToString(CultureInfo.InvariantCulture));
            _showInState.ToXml(element, doc);
        }
    }
}
