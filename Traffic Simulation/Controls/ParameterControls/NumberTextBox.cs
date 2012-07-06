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

using System.Media;

#endregion

namespace Traffic_Simulation
{
    public abstract class NumberTextBox : ParameterTextBox
    {
        protected NumberTextBox()
        {
            Factor = 1;
            Max = 1;
            Min = 0;
        }

        public double Factor { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }

        protected bool maxerror()
        {
            SystemSounds.Beep.Play();
            onstatuschanged(
                new StatusEventArgs(
                    string.Format(
                        "The value entered at {0}, {1}, is larger than the maximum value for this field, which is {2}.",
                        Name, Text, Max)));
            return false;
        }

        protected bool minerror()
        {
            SystemSounds.Beep.Play();
            onstatuschanged(
                new StatusEventArgs(
                    string.Format(
                        "The value entered at {0}, {1}, is smaller than the minimum value for this field, which is {2}.",
                        Name, Text, Min)));
            return false;
        }
    }
}