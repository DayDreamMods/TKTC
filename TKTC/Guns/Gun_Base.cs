using System;
using System.Collections;
using System.Collections.Generic;
using TKTC.Guns.AmmoPools;
using TKTC.Guns.Projectiles;
using UnityEngine;

// Were gonna be cheaky and as a design choice start guns unchambered, unloaded, with the safety on.

namespace TKTC.Guns
{
	public class Gun_Base : MonoBehaviour
	{
		// SELECTOR SWITCH
		
		public class SelectorState
		{
			public string name;
			public float burstWait; // the total time between shots that are not in the same burst is burstWait + autoWait, so when burstSize == 1 consider setting 
			public float autoWait;
			public float blowBackWait;
			public float burstSize;
			public bool triggerBlocked;
			public bool isAutomatic;
			public SelectorState(string newName, float newAutoRate, float newBurstRate, float newburstSize, bool newTriggerBlocked, bool newAuto)
			{
				name = newName;
				burstWait = newBurstRate;
				autoWait = newAutoRate;
				burstSize = newburstSize;
				triggerBlocked = newTriggerBlocked;
				isAutomatic = newAuto;
				blowBackWait = Mathf.Min(burstWait, autoWait) / 3f; // needs to be reworked when burstSize == 0 to disregard burstWait
			}
		}

		protected List<SelectorState> selectorStates = new();
		protected SelectorState? currentState;
		protected SelectorState? CurrentState
		{
			get { return currentState; }
			set
			{
				firing = false;
				if (value == null) return;
				switch (currentState) // Old state
				{
					
				}
				switch (value) // New value
				{

				}
				currentState = value;
			}
		}

		// VARIABLES
		protected string _name = "ERROR_DEFAULT";
		protected bool firing;

		protected IEnumerator? fireRoutine;
		protected float nextFireTime;
		public bool isBlowBack;

		internal Projectile_Base? chamberedProjectile;
		protected bool roundChambered, roundChamberedAfterLastCycle;
		public bool RoundChambered
		{
			get { return roundChambered; }
			private set { }
		}

		protected Transform? muzzleTip;
		protected float RoundsToFire
		{
			get
			{
				if (currentState is null) return 0f;
				if (magezine is null) return roundChambered ? 1f : 0f;
				if (!isBlowBack) return 1f;
				if (currentState.isAutomatic) return magezine.Amount;
				return Mathf.Min(currentState.burstSize, magezine.Amount);
			}
			set { }
		}

		protected bool hasBolt = true;
		protected bool canDropBolt;
		protected AmmoPool_Base? magezine;

		// EVENTS
		void Start()
		{
			magezine = new AmmoPool_Base();
			selectorStates.Add(new("Safe", 0.3f, 0.0f, 1f, true, true));
			currentState = selectorStates[1];
		}

		void Update()
		{
		}

		void OnDestroy()
		{
			magezine?.DisableProjectilePool();
        }

		// METHODS
		public override string ToString() { return _name; }

		public virtual void PullTrigger(bool sustained = false)
		{
			if (!canDropBolt || currentState is null || currentState.triggerBlocked)
			{
				firing = false;
				return;
			}

			if (!roundChambered)
			{
				if (canDropBolt)
				{
					canDropBolt = false;
                    // TODO: sound effect of bolt dropping
                }
				firing = false;
				return;
			}

			firing = true;
			fireRoutine = Fire(RoundsToFire, currentState);
		}

		protected virtual IEnumerator Fire(float toFire, SelectorState stateToFireIn) // this is a sexy routine if i do say so myself
		{
			nextFireTime = Mathf.Max(Time.time, nextFireTime); // we're not updating this value when the routine isnt running, so we have to catch it up at the begining
			
			if (stateToFireIn is null) yield break;

			float shotsFired = 0f;
			while (shotsFired < toFire)
			{
				for (int i = 0; i < stateToFireIn.burstSize; i++)
				{
					shotsFired++;
					chamberedProjectile?.Fire();
					nextFireTime += stateToFireIn.burstWait;
					if (isBlowBack)
					{
						yield return new WaitForSeconds(stateToFireIn.blowBackWait); // ok to use waitforseconds here as long as were sure that bbWait < burstWawit or autoWait depending on gun
						if (!CycleChamber() || !firing || shotsFired >= toFire)
						{
							// Smoke from open bolt? maybe keep track of number of shots fired in last few seconds?
							nextFireTime += stateToFireIn.autoWait;
							yield break;
						}
					}
					else
					{
						nextFireTime += stateToFireIn.burstWait;
						yield break;
					}
					yield return new WaitUntil(() => Time.time >= nextFireTime);
				}
				if (stateToFireIn.isAutomatic)
				{
					nextFireTime += stateToFireIn.autoWait;
					yield return new WaitUntil(() => Time.time >= nextFireTime);
				} else
				{
					yield break;
				}
			}
		}

		public virtual void ReleaseTrigger()
		{
			firing = false;
		}

		protected bool CycleChamber()
		{
			if (roundChamberedAfterLastCycle) ; // TODO: trigger ejection animation
			canDropBolt = true;
			if (magezine is not null)
			{
				roundChambered = magezine.TryRemoveOne();
				if (roundChambered)
				{
					chamberedProjectile = magezine.projectilePool.Get();
					chamberedProjectile?.transform.SetParent(muzzleTip, false);
				}
				roundChamberedAfterLastCycle = roundChambered;
			}
			else
			{
				roundChambered = false;
			}
			if (hasBolt)
			{
				canDropBolt = true;
				// TODO: dont depend on currentState here. probably fine but a bit unsafe
				if (!isBlowBack) nextFireTime = Time.time + currentState!.autoWait; // if it has blowback the timer is handled in Fire(), if its manual cocking it gets handled here
			}

            return roundChambered;
		}
	}
}
