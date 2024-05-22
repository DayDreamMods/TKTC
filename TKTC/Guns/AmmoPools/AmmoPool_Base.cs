using System.Collections.Generic;
using TKTC.Guns.Projectiles;
using UnityEngine;

namespace TKTC.Guns.AmmoPools
{
	internal class AmmoPool_Base
	{
        // VARIABLES
        protected bool autoRequestRefillFromParentCollections;
        protected List<AmmoPool_Collection> parentCollections = new();

		private int poolSize;
        const int MAX_POOL_SIZE = 256;
        protected byte minimumPoolSize = 2; // We generally want to base the pool size off of clipSize but in cases where clipSize is small and reload times are small, like rocket launchers, this will be necessary to set
		protected bool usingProjectilePool;
		protected List<Projectile_Base> projectilePool;

        protected System.Type projectileType = typeof(Projectile_Base); // Id like to use null, but nullable types suck, so check for projectileType == Projectile_Base as a failure state
		public System.Type ProjectileType
		{
			get { return projectileType; }
			internal set { projectileType = value; }
		}

        protected float amount;
		public float Amount
		{
			get { return amount; }
			internal set { amount = value; }
		}

        protected float clipSize;
		public float ClipSize
		{
			get { return clipSize; }
			internal set { clipSize = value; }
		}

        protected float capacity;
		public float Capacity
		{
			get { return capacity; }
			internal set { capacity = value - (value % clipSize); } // When setting with Property enforce multiple of clipsize
		}

		// CONSTRUCTORS
		public AmmoPool_Base()
		{
			// access current round settings to determine initial fill amounts
		}

		public AmmoPool_Base(float initialAmount, float newCapacity, System.Type newProjectileType)
		{
			SetValues(initialAmount, newCapacity, newProjectileType);
			if (capacity < 1f) TKTC.Logger.LogDebug("AmmoPool created with <1.0f capacity!");
		}

		public AmmoPool_Base(AmmoPool_Base copyFrom)
		{
			SetValues(copyFrom.Amount, copyFrom.Capacity, copyFrom.ProjectileType);
		}

        protected virtual void SetValues(float newAmount, float newCapacity, System.Type newProjectileType)
        {
            capacity = Mathf.Min(newCapacity, 0f);
            amount = Mathf.Clamp(newAmount, 0f, capacity);
            projectileType = newProjectileType;
        }

        // METHODS
        public bool IsEmpty => amount <= 0f;
		public bool IsFull => amount >= 0f;

        protected float AddAmmo(float toAdd)
		{
			float willAdd = Mathf.Min(capacity - amount, toAdd);
			amount += willAdd;
			toAdd -= willAdd;
			return toAdd; // Return remainder
		}

		internal bool AddAmmo(AmmoPool_Base toMerge)
		{
			// Check if the type we are pulling from is of the same type or a base type of this pool, only allow loading into a same or derived ammo pool
			if (IsFull || !toMerge.GetType().IsAssignableFrom(this.GetType())) return false;

			toMerge.amount = AddAmmo(toMerge.Amount);
			return true;
		}

		public bool TryRemoveOne() // Just for single shot reloading, itty bit more efficient
		{
			if (amount < 1f) return false;
			amount -= 1f;
			return true;
		}

		public bool TryRemoveExactly(float toRemove)
		{
			if (amount < toRemove) return false;
			amount -= toRemove;
			return true;
		}

		public float TryRemoveUpTo(float toRemove)
		{
			toRemove = Mathf.Min(amount, toRemove);
			amount -= toRemove;
			return toRemove;
		}

		internal virtual AmmoPool_Base Clone()
		{
			// This needs to be overridden in child classes to return an object of their own type cast to AmmoPool_Base
			return new AmmoPool_Base(this);
		}

		internal void AddedToCollection(AmmoPool_Collection newParent)
		{
			if (!parentCollections.Contains(newParent)) parentCollections.Add(newParent);
		}

		internal void RemovedFromCollection(AmmoPool_Collection oldParent)
		{
			parentCollections.Remove(oldParent);
		}

        public void EnableProjectilePool()
		{
			usingProjectilePool = true;
			projectilePool = new();

			poolSize = Mathf.Clamp((int)Mathf.Ceil(clipSize * 1.1f), minimumPoolSize, MAX_POOL_SIZE); // we do clipsize * 1.1 so that a high fire rate high speed reload gun can mag dump and get 10% into their next clip before the first projetile is available again in the pool

			for (int i = 0; i < poolSize)
			{

			}
		}

		public void DisableProjectilePool()
		{

		}

		private IEnumerable SpawnProjectilesInPool(GameObject toClone, int numToSpawn) // Spawn one projectile into the pool per frame, instead of say 70 at once for one gun when youre adding multiple guns at once.
		{
			int numSpawned = 0;
			while (numSpawned < numToSpawn)
			{
				GameObject newProjectile = GameObject.Instantiate(toClone);
				projectilePool.Add(newProjectile.GetComponent<Projectile_Base>());
				yield return new WaitForSeconds(0.00001f);
			}
		}
    }
}
