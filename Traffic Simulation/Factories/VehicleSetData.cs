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

using System.Collections.Generic;
using Traffic_Simulation.ModelData;
using Traffic_Simulation.Vehicles;

#endregion

namespace Traffic_Simulation
{
    public static class VehicleSetData
    {
        public static List<VehicleProductionItem> GetVehicleSet(string vehicleset)
        {
            switch (vehicleset)
            {
                default:
                    return getdefaultvehicleset();
                case "Porsche 911 (993)":
                    return getporsche993vehicleset();
            }
        }

        private static List<VehicleProductionItem> getdefaultvehicleset()
        {
            var productionitemlist = new List<VehicleProductionItem>();

            // Buses
            var modeldatalist = new List<VehicleModelData>
                {
                    new BusModelData("Van Hool AGG300", new SizeD(2.550, 24.785), 25, 2, 8),
                    new BusModelData("Van Hool AG300", new SizeD(2.550, 18.390), 25, 2, 8),
                    new BusModelData("Van Hool A330", new SizeD(2.550, 11.995), 25, 2, 8)
                };
            productionitemlist.Add(new VehicleProductionItem(typeof (Bus), modeldatalist));

            // Cars
            modeldatalist = new List<VehicleModelData>
                {
                    new CarModelData("Porsche 911 Turbo S (997)", new SizeD(1.831, 4.435), 87.5, 8.6, 10),
                    new CarModelData("Porsche Panamera Turbo (970)", new SizeD(1.931, 4.970), 84.2, 8.1,
                                     10),
                    new CarModelData("Porsche Cayenne Turbo (958)", new SizeD(1.938, 4.846), 77.2, 5.9,
                                     10),
                    new CarModelData("Porsche Cayman R (987)", new SizeD(1.801, 4.376), 78.3, 5.6, 10),
                    new CarModelData("Porsche Boxster Spyder (987)", new SizeD(1.816, 4.369), 74.2, 5.4,
                                     10),
                    new CarModelData("Mini Cooper Saloon", new SizeD(1.35, 3.05), 38, 2, 6),
                    new CarModelData("Ford Fiesta Mark V", new SizeD(1.685, 3.924), 44.4, 2.8, 8)
                };
            productionitemlist.Add(new VehicleProductionItem(typeof (Car), modeldatalist));

            // Motorcycles
            modeldatalist = new List<VehicleModelData>
                {new MotorcycleModelData("Yamaha YZF-R6", new SizeD(0.701, 2.040), 71.1, 9.6, 10)};
            productionitemlist.Add(new VehicleProductionItem(typeof (Motorcycle), modeldatalist));

            // Trucks
            modeldatalist = new List<VehicleModelData>
                {new TruckModelData("Unimog 435", new SizeD(2.300, 5.500), 23.6, 2, 10)};
            productionitemlist.Add(new VehicleProductionItem(typeof (Truck), modeldatalist));

            return productionitemlist;
        }

        private static List<VehicleProductionItem> getporsche993vehicleset()
        {
            var productionitemlist = new List<VehicleProductionItem>();
            var modeldatalist = new List<VehicleModelData>
                {
                    new CarModelData("911 (993) Carrera 2", new SizeD(1.651, 4.275), 270/3.6,
                                     100/3.6/5.6, 10),
                    new CarModelData("911 (993) Carrera S", new SizeD(1.735, 4.260), 270/3.6,
                                     100/3.6/5.7, 10),
                    new CarModelData("911 (993) GT 2", new SizeD(1.651, 4.275), 296/3.6, 96/3.6/4.0,
                                     10),
                    new CarModelData("911 (993) Turbo", new SizeD(1.651, 4.275), 290/3.6,
                                     100/3.6/4.5, 10),
                    new CarModelData("911 (993) Turbo S", new SizeD(1.735, 4.260), 303/3.6,
                                     96/3.6/3.7, 10)
                };

            productionitemlist.Add(new VehicleProductionItem(typeof (Car), modeldatalist));

            return productionitemlist;
        }
    }
}