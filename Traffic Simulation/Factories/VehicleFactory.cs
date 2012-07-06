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

#endregion

namespace Traffic_Simulation
{
    [Serializable]
    public class VehicleFactory
    {
        private int fractionsum;
        private Dictionary<Type, int> iddictionary;

        private double ordersum;
        private List<VehicleProductionItem> productionitemlist;

        public VehicleFactory()
        {
            List<Type> vehicletypelist = Global.GetClassesInNameSpace("Traffic_Simulation.Vehicles");
            productionitemlist = new List<VehicleProductionItem>();

            Reset();

            ParameterPanel.VehicleSetChanged += vehiclesetchanged;
            ParameterPanel.VehicleFractionChanged += vehiclefractionchanged;

            vehiclesetchanged(null, EventArgs.Empty);
        }

        public int ProductionCount { get; private set; }

        private void vehiclefractionchanged(object o, EventArgs ea)
        {
            fractionsum = 0;

            foreach (VehicleProductionItem productionitem in productionitemlist)
            {
                productionitem.Fraction = ParameterPanel.GetVehicleFraction(productionitem.VehicleType);
                fractionsum += productionitem.Fraction;
            }
        }

        private void vehiclesetchanged(object o, EventArgs ea)
        {
            productionitemlist = VehicleSetData.GetVehicleSet(ParameterPanel.VehicleSet);
            iddictionary = new Dictionary<Type, int>();

            foreach (VehicleProductionItem productionitem in productionitemlist)
                iddictionary.Add(productionitem.VehicleType, 0);

            vehiclefractionchanged(o, ea);
        }

        public void Reset()
        {
            vehiclesetchanged(null, EventArgs.Empty);
            ordersum = 0;
            ProductionCount = 0;
        }

        public Vehicle[] OrderVehicles(double seconds, int vehiclecount, int capacity)
        {
            int amount =
                Math.Max(
                    Math.Min(Math.Min(GetSpawnAmount(seconds), ParameterPanel.VehicleCap - vehiclecount), capacity), 0);

            var vehicles = new Vehicle[amount];

            for (int i = 0; i < amount; i++)
                vehicles[i] = MakeVehicle();

            return vehicles;
        }

        private int GetSpawnAmount(double seconds)
            // http://en.wikipedia.org/wiki/Sum_of_normally_distributed_random_variables
        {
            // Determine how much vehicles need to be spawned based on μSpawn en σSpawn .
            // Because this mostly is near 0 we keep track of an ordersum to determine
            // cumulative how much vehicles need to be spawned.

            double cμ = ParameterPanel.μSpawn*seconds;
            double cσ = Math.Sqrt(ParameterPanel.σSpawn*ParameterPanel.σSpawn*seconds);

            ordersum += Randomizer.NextNormal(cμ, cσ);

            var amount = (int) Math.Max(ordersum, 0);
            ordersum -= amount;

            return amount;
        }

        private Vehicle MakeVehicle()
        {
            VehicleProductionItem productionitem = chooseproductionitem();
            VehicleModelData vehiclemodeldata = choosemodel(productionitem);

            iddictionary[productionitem.VehicleType]++;
            ProductionCount++;

            return (Vehicle) Activator.CreateInstance(productionitem.VehicleType, ProductionCount, vehiclemodeldata);
        }

        private VehicleProductionItem chooseproductionitem()
        {
            int sum = Randomizer.Next(0, fractionsum);

            foreach (VehicleProductionItem t in productionitemlist.Where(t => (sum -= t.Fraction) < 0))
                return t;

            return productionitemlist[productionitemlist.Count - 1];
        }

        private static VehicleModelData choosemodel(VehicleProductionItem productionitem)
        {
            return productionitem.ModelDataList[Randomizer.Next(0, productionitem.ModelDataList.Count)];
        }
    }
}