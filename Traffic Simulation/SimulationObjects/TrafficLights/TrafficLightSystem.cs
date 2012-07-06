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
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    [KnownType(typeof (TrafficLightSystem))]
    public class TrafficLightSystem
    {
        private const int checkdistance = 200;
        private readonly Crossing crossing;
        private readonly List<Road> roadlist;
        private readonly Dictionary<Road, List<TrafficLight>> trafficlightdictionary;
        private int roadindex;
        private double time;
        private TrafficLightState trafficlightstate;

        public TrafficLightSystem(Crossing crossing)
        {
            this.crossing = crossing;
            roadindex = 0;
            roadlist = new List<Road>();
            time = 0;
            trafficlightdictionary = new Dictionary<Road, List<TrafficLight>>();
            trafficlightstate = TrafficLightState.Yellow;
        }

        public void AddTrafficLight(Road road, TrafficLight trafficlight)
        {
            if (!roadlist.Contains(road))
            {
                roadlist.Add(road);
                trafficlightdictionary.Add(road, new List<TrafficLight>());
            }

            trafficlightdictionary[road].Add(trafficlight);
        }

        public void Next(double seconds)
        {
            time += seconds;

            while ((trafficlightstate == TrafficLightState.Green && time > ParameterPanel.GreenTime) ||
                   (trafficlightstate == TrafficLightState.Yellow && time > ParameterPanel.YellowTime))
            {
                int current = roadlist[roadindex].GetVehiclesAtLastOfRoadToward(crossing.Location, checkdistance);

                int maximum;
                if (trafficlightstate == TrafficLightState.Green && (time > ParameterPanel.GreenTime || current == 0))
                {
                    maximum = roadlist.Max(road => road.GetVehiclesAtLastOfRoadToward(crossing.Location, checkdistance));

                    if (current == maximum)
                    {
                        if (current > 0)
                            time -= 1;
                        return;
                    }

                    foreach (TrafficLight trafficlight in trafficlightdictionary[roadlist[roadindex]])
                        trafficlight.TrafficLightState = TrafficLightState.Yellow;

                    trafficlightstate = TrafficLightState.Yellow;
                    time -= Math.Max(ParameterPanel.GreenTime, time);
                }

                if (trafficlightstate != TrafficLightState.Yellow || !(time > ParameterPanel.YellowTime)) continue;
                var subroadlist = roadlist.Where(road => road != roadlist[roadindex]).ToList();
                maximum = subroadlist.Max(
                    road => road.GetVehiclesAtLastOfRoadToward(crossing.Location, checkdistance));
                roadindex =
                    roadlist.IndexOf(
                        subroadlist.First(
                            road =>
                            road.GetVehiclesAtLastOfRoadToward(crossing.Location, checkdistance) == maximum));

                for (int i = 0; i < roadlist.Count; i++)
                    foreach (var trafficlight in trafficlightdictionary[roadlist[i]])
                        trafficlight.TrafficLightState = i != roadindex
                                                             ? TrafficLightState.Red
                                                             : TrafficLightState.Green;

                trafficlightstate = TrafficLightState.Green;
                time -= ParameterPanel.YellowTime;
            }
        }

        public void DrawTrafficLights(PaintEventArgs pea)
        {
            foreach (var trafficlight in roadlist.SelectMany(t => trafficlightdictionary[t]))
                trafficlight.Draw(pea);
        }

        public void CalculateTrafficLightDrawPoints()
        {
            foreach (
                var trafficlight in
                    trafficlightdictionary.Values.SelectMany(trafficlightlist => trafficlightlist))
                trafficlight.CalculateDrawPoints();
        }
    }
}