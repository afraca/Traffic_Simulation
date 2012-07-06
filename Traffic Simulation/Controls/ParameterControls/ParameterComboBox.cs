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
using System.Windows.Forms;

#endregion

namespace Traffic_Simulation
{
    public class ParameterComboBox : ComboBox, IParameterBox
    {
        protected IParameterContainer iparametercontainer;

        public ParameterComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList;
            SelectedIndexChanged += selectedindexchanged;
        }

        #region IParameterBox Members

        public event EventHandler<StatusEventArgs> StatusChanged;

        public void SetIParameterContainer(IParameterContainer iparametercontainer)
        {
            this.iparametercontainer = iparametercontainer;
        }

        public void SetValue(object o)
        {
            if (InvokeRequired)
                Invoke(new SetParameterDelegate(setvalue), o);
            else
                setvalue(o);
        }

        #endregion

        protected virtual void setvalue(object o)
        {
            SelectedItem = o.ToString();
        }

        private void selectedindexchanged(object o, EventArgs ea)
        {
            StatusChanged(this, new StatusEventArgs("{0} updated to: '{1}'.", Name, SelectedItem.ToString()));
            iparametercontainer.UpdateParameter(this, SelectedItem);
        }
    }
}