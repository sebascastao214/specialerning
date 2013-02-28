using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IHCSClassLibrary
{
    public class MetersToFeets
    {
        public double Feet;
        public void Set_MetersToFeedConvertion (double Height)
        {
            Feet = Math.Round ((Height * 3.2808399),2);
        }

        public double Get_Meter_Feet_Convertion ()
        {
            return Feet;
        }
    }
}
