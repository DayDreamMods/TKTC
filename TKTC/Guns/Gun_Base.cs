using System;
using System.Collections.Generic;
using System.Text;
using TKTC.Guns.AmmoPools;
using TKTC.Guns.Projectiles;
using UnityEngine;

// Were gonna be cheaky and as a design choice start guns unchambered, unloaded, with the safety on.

namespace TKTC.Guns
{
	public class Gun_Base : MonoBehaviour
	{
		public struct SelectorSwitch
		{
			public enum SwitchState
			{
				Safe,
				Single,
				Burst,
				Auto
			}
			public SwitchState state;
		}

		// VARIABLES
		private string _name;

		private bool roundChambered;
		public bool RoundChambered
		{
			get { return roundChambered; }
			private set { }
		}

		private Projectile_Base? chamberedProjectile;
		private SelectorSwitch selectorSwitch;
		private bool waitOnTriggerToCycleChamber;

		private float singleRoF, burstRoF, automaticRoF;
		private float burstRoundsCount; // its weird its a float, but energy weapons

		private bool roundChamberedAfterLastCycle;
		private bool hasBolt = true;
		private bool canDropBoltUnchambered;
        private AmmoPool_Base? magezine;

		// EVENTS
		void Start()
		{
			magezine = new AmmoPool_Base();
		}

		void OnDestroy()
		{
			magezine?.DisableProjectilePool();
        }

		// METHODS
		public override string ToString() { return _name; }

		public virtual void PullTrigger(bool sustained = false)
		{
			if (!roundChambered)
			{
				if (canDropBoltUnchambered)
				{
					canDropBoltUnchambered = false;
                    // TODO: sound effect of bolt dropping
                }
				return;
			}

			bool didFire;
			switch (selectorSwitch.state)
			{
				case SelectorSwitch.SwitchState.Single:
					didFire = Fire();
					break;
				case SelectorSwitch.SwitchState.Burst:
                    StartCoroutine(FireBurst((int)magezine.TryRemoveUpTo(burstRoundsCount)));
                    break;
                case SelectorSwitch.SwitchState.Auto:
					break;
                default:
					break;
			}
			// Cycle chamber immediately if atuomatic blowback bolt
			if (!waitOnTriggerToCycleChamber) CycleChamber();
		}

		protected IEnumerable FireBurst(int numToFire)
		{
            yield return new WaitForSeconds(burstRoF);
        }

		public virtual void ReleaseTrigger(bool firedOnPull)
		{
			// Cycle chamber on mouse release if manual bolt action
            if (waitOnTriggerToCycleChamber) CycleChamber();
		}

		protected virtual bool Fire()
		{
			if (roundChambered)
			{
				chamberedProjectile.Fire();
			}
			return false;
		}

		protected bool CycleChamber()
		{
            // if (roundChamberedAfterLastCycle) TODO: trigger ejection animation
            roundChambered = magezine.TryRemoveOne();
            roundChamberedAfterLastCycle = roundChambered;
			if (hasBolt) canDropBoltUnchambered = true;

            return roundChambered;
		}
	}
}
