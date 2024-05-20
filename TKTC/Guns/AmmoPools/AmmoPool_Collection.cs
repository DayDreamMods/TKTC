using System;
using System.Collections.Generic;
using System.Text;
using TKTC.Guns.AmmoPools;

namespace TKTC.Guns.AmmoPools
{
    internal class AmmoPool_Collection
    {
        private List<AmmoPool_Base> pools; // We dont want duplicate references, but were expecting small List lengths and frequent iteration

        public float AddAmmo(float inAmmo) // return overflow amount
        {

        }

        public void AddAmmo(ref AmmoPool_Base toMerge)
        {

        }
    }
}
