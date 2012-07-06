#region COPYING

// Copyright 2011, 2012 Stefan Ottevanger & Sije Harkema
// 
// This file is part of Traffic Simulation.
// 
// Traffic Simulation is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Traffic Simulation is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Traffic Simulation.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (Building))]
    public class Building : SimulationObject
    {
        [OptionalField] private readonly Color color;
        [OptionalField] private readonly double[] coordinates;
        [OptionalField] private readonly PointD[] points;
        [NonSerialized] private GraphicsPath drawgraphicspath;
        [NonSerialized] private GraphicsPath graphicspath;
        [NonSerialized] private SolidBrush solidbrush;
        //Deserialization causes nullreference-exceptions, dummy variables required

        public Building(Color buildingcolor, params double[] coordinates)
        {
            solidbrush = new SolidBrush(buildingcolor);
            this.coordinates = coordinates;
            color = buildingcolor;

            graphicspath = new GraphicsPath();
            for (int i = 0; i < coordinates.Length - 3; i += 2)
                graphicspath.AddLine(new PointD(coordinates[i], coordinates[i + 1]),
                                     new PointD(coordinates[i + 2], coordinates[i + 3]));
        }

        public Building(Color buildingcolor, params PointD[] points)
        {
            solidbrush = new SolidBrush(buildingcolor);
            this.points = points;

            graphicspath = new GraphicsPath();
            for (int i = 0; i < points.Length - 1; i++)
                graphicspath.AddLine(points[i], points[i + 1]);
        }

        public override void CalculateDrawPoints()
        {
            Matrix matrix = ParameterPanel.Matrix;
            if (graphicspath == null)
            {
                DeserializationRepopulation();
            }
            if (graphicspath != null) drawgraphicspath = graphicspath.Clone() as GraphicsPath;
            if (drawgraphicspath != null) drawgraphicspath.Transform(matrix);
        }

        public override void Draw(PaintEventArgs pea)
        {
            pea.Graphics.FillPath(solidbrush, drawgraphicspath);
        }

        #region Deserialization fixes for nonserializable object properties

        public void DeserializationRepopulation()
        {
            solidbrush = new SolidBrush(color);
            graphicspath = new GraphicsPath();
            if (coordinates != null)
            {
                for (int i = 0; i < coordinates.Length - 3; i += 2)
                    graphicspath.AddLine(new PointD(coordinates[i], coordinates[i + 1]),
                                         new PointD(coordinates[i + 2], coordinates[i + 3]));
            }
            else
            {
                for (int i = 0; i < points.Length - 1; i++)
                    graphicspath.AddLine(points[i], points[i + 1]);
            }
        }

        #endregion
    }
}