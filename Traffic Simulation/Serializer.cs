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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

#endregion

namespace Traffic_Simulation
{
    public static class Serializer
    {
        private static readonly BinaryFormatter binaryformatter = new BinaryFormatter();
        private static readonly DataContractSerializer datacontractformatter;

        static Serializer()
        {
            var types = GetSerializableTypes().ToList();
            types.Add(typeof (Bezier[]));
            types.Add(typeof (List<Road>));
            types.Add(typeof (List<Building>));
            types.Add(typeof (List<Crossing>));
            types.Add(typeof (List<RoadConnection>));
            datacontractformatter = new DataContractSerializer(typeof (object), types);
        }

        //get classes with serializable attribute: http://stackoverflow.com/questions/607178/c-sharp-how-enumerate-all-classes-with-custom-class-attribute
        private static IEnumerable<Type> GetSerializableTypes()
        {
            return
                Assembly.GetExecutingAssembly().GetTypes().Where(
                    type => type.GetCustomAttributes(typeof (SerializableAttribute), true).Length > 0);
        }

        public static void Serialize(string location, object o, bool binary = true)
        {
            using (Stream stream = File.Open(location, FileMode.Create))
            {
                try
                {
                    if (binary)
                        binaryformatter.Serialize(stream, o);
                    else
                        datacontractformatter.WriteObject(stream, o);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public static object Deserialize(string location, bool binary = true)
        {
            using (Stream stream = File.Open(location, FileMode.Open))
            {
                try
                {
                    return binary ? binaryformatter.Deserialize(stream) : datacontractformatter.ReadObject(stream);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return null;
                }
            }
        }
    }
}