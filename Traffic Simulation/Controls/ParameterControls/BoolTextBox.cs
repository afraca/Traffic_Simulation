﻿#region COPYING

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

namespace Traffic_Simulation
{
    public class BoolTextBox : ParameterTextBox
    {
        private bool value;

        public BoolTextBox()
        {
            ValueType = typeof (bool);
        }

        public bool Value
        {
            get { return value; }
        }

        protected override bool valid()
        {
            if (!bool.TryParse(Text, out value))
                return typeerror();

            ValueDynamic = value;

            return true;
        }

        protected override void reset()
        {
            bool.TryParse(text, out value);
        }

        public override T GetValue<T>()
        {
            return (T) (object) value;
        }
    }
}