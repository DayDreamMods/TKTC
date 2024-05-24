using System;
using System.Collections.Generic;
using System.Text;

namespace TKTC.Guns
{
    internal class Gun_VN1 : Gun_Base
    {
        void Awake()
        {
            _name = "VN1 Rifle";
			isBlowBack = true;
			selectorStates.Add(new("Automatic", 0.3f, 0.0f, 1f, false, true));

		}
    }
}
