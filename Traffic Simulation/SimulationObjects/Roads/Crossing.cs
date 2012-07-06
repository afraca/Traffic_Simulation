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
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (Crossing))]
    public class Crossing : SimulationObject, ILaneContainer
    {
        [OptionalField] private readonly PointD[] cornerpoints;
        [OptionalField] private readonly List<CrossingLane> lanelist;
        [OptionalField] private readonly double linewidth;
        [OptionalField] private readonly PointD location;
        [OptionalField] private readonly List<Road> roadlist;
        [OptionalField] private readonly TrafficLightSystem trafficlightsystem;

        [NonSerialized] private GraphicsPath drawgraphicspath;
        [OptionalField] private PointF[] drawpoints;
        [NonSerialized] private GraphicsPath graphicspath;
        [NonSerialized] private Pen linepen;

        public Crossing(double x, double y, CrossingData crossingdata, params RoadConnection[] roadconnections)
            : this(new PointD(x, y), crossingdata, roadconnections)
        {
        }

        private Crossing(PointD location, CrossingData crossingdata, params RoadConnection[] roadconnections)
        {
            cornerpoints = new PointD[2*roadconnections.Length];
            lanelist = new List<CrossingLane>();
            linewidth = crossingdata.LineWidth;
            this.location = location;
            roadlist = new List<Road>();
            trafficlightsystem = new TrafficLightSystem(this);

            #region SortRoads

            List<RoadConnection> roadconnectionlist = roadconnections.ToList();
            // Bepaal het SortingPoint van elke Road. Dit is het uiteinde van de Road dat bij de Crossing uitkomt.
            foreach (RoadConnection t in roadconnectionlist)
                t.Road.SortPoint = t.Road.GetTowardCoordinate(location);

            // Sorteer de Roads die op de Crossing samenkomen tegen de klik in.
            roadconnectionlist.Sort(
                (r1, r2) => Math.Atan2(r2.Road.SortPoint.Y - location.Y, r2.Road.SortPoint.X - location.X).CompareTo(
                    Math.Atan2(r1.Road.SortPoint.Y - location.Y, r1.Road.SortPoint.X - location.X)));

            #endregion

            #region MakeCrossingLanes

            for (int i = 0; i < roadconnectionlist.Count; i++)
            {
                Road road = roadconnectionlist[i].Road;
                roadlist.Add(road);

                // Bepaal de hoekpunten van de Crossing bij elke Road.
                if (road.IsToward(location))
                {
                    cornerpoints[2*i] = road.GetLastCoordinate() +
                                        road.GetLastDirection().GetLeftHandNormal()*road.Width/2;
                    cornerpoints[2*i + 1] = road.GetLastCoordinate() +
                                            road.GetLastDirection().GetRightHandNormal()*road.Width/2;
                    road.SetNext(this);
                }
                else
                {
                    cornerpoints[2*i] = road.GetFirstCoordinate() +
                                        road.GetFirstDirection().GetRightHandNormal()*road.Width/2;
                    cornerpoints[2*i + 1] = road.GetFirstCoordinate() +
                                            road.GetFirstDirection().GetLeftHandNormal()*road.Width/2;
                    road.SetPrevious(this);
                }

                Dictionary<int, HashSet<RoadLaneIndex>> connectiondictionary =
                    roadconnectionlist[i].ConnectionDictionary;
                List<RoadLane> road1lanelist = roadconnectionlist[i].Road.GetTowardLanes(location);

                // Verbind de Lanes van de Roads met elkaar zoals beschreven in de PDF
                foreach (int lane1 in connectiondictionary.Keys)
                {
                    var vehiclelist = new List<Vehicle>();
                    HashSet<RoadLaneIndex> roadlaneindexhashset = connectiondictionary[lane1];
                    var trafficlight = new TrafficLight(road1lanelist[lane1], road1lanelist[lane1].GetLastCoordinate());

                    foreach (RoadLaneIndex roadlaneindex in roadlaneindexhashset)
                    {
                        int road2index = (i + roadlaneindex.Road)%roadconnectionlist.Count;
                        List<RoadLane> road2lanelist = roadconnectionlist[road2index].Road.GetOffwardLanes(location);

                        trafficlight.AddExitLane(road2lanelist[roadlaneindex.Lane]);

                        lanelist.Add(new CrossingLane(this, road1lanelist[lane1], road2lanelist[roadlaneindex.Lane],
                                                      road1lanelist[lane1].Width, vehiclelist));
                    }

                    if (road1lanelist[lane1].LaneType == LaneType.Shoulder) continue;
                    trafficlightsystem.AddTrafficLight(road, trafficlight);
                    road1lanelist[lane1].TrafficLight = trafficlight;
                }
            }

            #endregion

            MakeCrossingGraphicsPath();
        }

        public PointD Location
        {
            get { return location; }
        }


        private void MakeCrossingGraphicsPath()
        {
            int length = cornerpoints.Length;

            graphicspath = new GraphicsPath();

            for (int i = 0; i < cornerpoints.Length; i++)
            {
                if (i%2 == 0)
                    graphicspath.AddLine(cornerpoints[i], cornerpoints[i + 1]);
                else
                {
                    VectorD vector1 =
                        new VectorD(cornerpoints[(i - 1)%length], cornerpoints[(i)%length]).GetRightHandNormal();
                    VectorD vector2 =
                        new VectorD(cornerpoints[(i + 1)%length], cornerpoints[(i + 2)%length]).GetRightHandNormal();

                    if (vector1.IsCoLinearWith(vector2))
                        graphicspath.AddLine(cornerpoints[(i)%length], cornerpoints[(i + 1)%length]);
                    else
                    {
                        var line1 = new LineD(cornerpoints[(i)%length], vector1);
                        var line2 = new LineD(cornerpoints[(i + 1)%length], vector2);

                        Bezier bezier = new QuadraticBezier(cornerpoints[(i)%length], line1.GetCrossingPoint(line2),
                                                            cornerpoints[(i + 1)%length]);

                        PointF[] points = bezier.GetDrawPoints();

                        graphicspath.AddBezier(points[0], points[1], points[2], points[3]);
                    }
                }
            }
        }

        public void TrafficLightNext(double seconds)
        {
            trafficlightsystem.Next(seconds);
        }

        #region Navigation

        private Road GetRoadTo(Road source, Point point)
        {
            var roaddistancelist = new List<RoadDistance>();

            foreach (Road road in roadlist.Where(road => road != source))
            {
                double distance = road.GetOffwardCoordinate(location).DistanceTo(point);

                if (distance < road.Width)
                    return road;

                bool endofmap = (road.IsToward(location) && road.Previous == null) ||
                                (!road.IsToward(location) && road.Next == null);
                roaddistancelist.Add(new RoadDistance(road, distance, endofmap));
            }

            roaddistancelist = roaddistancelist.Where(roaddistance => !roaddistance.EndOfMap).ToList();
            double min = roaddistancelist.Min(roaddistance => roaddistance.Distance);
            roaddistancelist =
                roaddistancelist.Where(roaddistance => roaddistance.Distance < 1.2*min).ToList();

            if (roaddistancelist.Count > 1)
                roaddistancelist.Sort(
                    (r1, r2) => r1.Road.VehicleCount.CompareTo(r2.Road.VehicleCount));

            return roaddistancelist[0].Road;
        }

        public int GetLaneIndexTo(Road source, Point point)
        {
            Road exit = GetRoadTo(source, point);
            List<CrossingLane> sublanelist =
                lanelist.Where(
                    crossinglane =>
                    crossinglane.PreviousLane.Road == source && crossinglane.NextLane.Road == exit).ToList();
            return (sublanelist[0].PreviousLane.LaneIndex);
        }

        #endregion

        #region Drawing

        public override void CalculateDrawPoints()
        {
            Matrix matrix = ParameterPanel.Matrix;

            linepen = new Pen(Color.White, (float) linewidth*matrix.Elements[0]);
            if (graphicspath == null)
            {
                DeserializationRepopulation();
            }
            if (graphicspath != null) drawgraphicspath = (GraphicsPath) graphicspath.Clone();
            drawpoints = new PointF[cornerpoints.Length];

            for (int i = 0; i < drawpoints.Length; i++)
                drawpoints[i] = cornerpoints[i];

            drawgraphicspath.Transform(matrix);
            matrix.TransformPoints(drawpoints);
        }

        public void CalculateLaneDrawPoints()
        {
            foreach (CrossingLane lane in lanelist)
                lane.CalculateDrawPoints();
        }

        public void CalculateTrafficLightDrawPoints()
        {
            trafficlightsystem.CalculateTrafficLightDrawPoints();
        }

        public override void Draw(PaintEventArgs pea)
        {
            pea.Graphics.FillPath(Brushes.Gray, drawgraphicspath);

            for (int i = 0; i < drawpoints.Length; i += 2)
                pea.Graphics.DrawLine(linepen, drawpoints[i], drawpoints[i + 1]);
        }

        public void DrawLaneCurves(PaintEventArgs pea)
        {
            foreach (CrossingLane lane in lanelist)
                lane.DrawCurve(pea);
        }

        public void DrawLanePoints(PaintEventArgs pea)
        {
            foreach (CrossingLane lane in lanelist)
                lane.DrawPoints(pea);
        }

        public void DrawTrafficLights(PaintEventArgs pea)
        {
            trafficlightsystem.DrawTrafficLights(pea);
        }

        #endregion

        #region Deserialization fixes

        // Deserialization fixes for nonserializable object properties
        private void DeserializationRepopulation()
        {
            graphicspath = new GraphicsPath();
            MakeCrossingGraphicsPath();
        }

        #endregion
    }
}