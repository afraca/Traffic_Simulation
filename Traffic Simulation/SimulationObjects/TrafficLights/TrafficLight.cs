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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (TrafficLight))]
    public class TrafficLight : SimulationObject
    {
        private static readonly Dictionary<TrafficLightState, Brush> colordictionary;

        [OptionalField] private readonly Lane entrylane;
        [OptionalField] private readonly List<Lane> exitlanes;
        [OptionalField] private readonly PointD location;
        [OptionalField] private PointF[] drawpoint;

        [OptionalField] private float size;
        [OptionalField] private TrafficLightState trafficlightstate;

        static TrafficLight()
        {
            colordictionary = new Dictionary<TrafficLightState, Brush>
                {
                    {TrafficLightState.Green, Brushes.Green},
                    {TrafficLightState.Red, Brushes.Red},
                    {TrafficLightState.Yellow, Brushes.Orange}
                };
        }

        public TrafficLight(Lane entrylane, PointD location)
        {
            this.entrylane = entrylane;
            this.location = location;
            exitlanes = new List<Lane>();
        }

        public TrafficLightState TrafficLightState
        {
            get { return trafficlightstate; }
            set { trafficlightstate = value; }
        }

        public override void CalculateDrawPoints()
        {
            Matrix matrix = ParameterPanel.Matrix;
            drawpoint = new PointF[] {location};
            size = matrix.Elements[0]*(float) entrylane.Width/4;
            matrix.TransformPoints(drawpoint);
        }

        public void AddExitLane(Lane lane)
        {
            exitlanes.Add(lane);
        }

        public override void Draw(PaintEventArgs pea)
        {
            pea.Graphics.FillEllipse(colordictionary[TrafficLightState], drawpoint[0].X - size, drawpoint[0].Y - size,
                                     2*size, 2*size);
        }
    }
}