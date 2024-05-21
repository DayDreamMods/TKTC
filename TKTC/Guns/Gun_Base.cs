using System;
using System.Collections.Generic;
using System.Text;
using TKTC.Guns.AmmoPools;

// Were gonna be cheaky and as a design choice start guns unchambered, unloaded, with the safety on.

namespace TKTC.Guns
{
    internal class Gun_Base
    {
        // VARIABLES
        private string _name;
        private bool roundChambered;
        private AmmoPool_Base magezine;
        private AmmoPool_Base reloadSource;

        public override string ToString() { return _name; }
	}
}
