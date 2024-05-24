using System.Collections;
using System.Collections.Generic;
using TKTC.Guns.Projectiles;
using UnityEngine;
using UnityEngine.Pool;

namespace TKTC.Guns.AmmoPools
{
	public class AmmoPool_Base : MonoBehaviour
	{
        // VARIABLES
        protected bool autoRequestRefillFromParentCollections;
        internal List<AmmoPool_Collection> parentCollections = new();

		private float muzzleVelocity = 700f;
		internal IObjectPool<Projectile_Base> projectilePool;

		private IEnumerator projectileSpawnRoutine;
		private int poolSize;
        const int MAX_POOL_SIZE = 256;
        protected byte minimumPoolSize = 2; // We generally want to base the pool size off of clipSize but in cases where clipSize is small and reload times are small, like rocket launchers, this will be necessary to set
		protected bool usingProjectilePool;
		protected int lastPoolIterator;

        protected System.Type projectileType = typeof(Projectile_Base); // Id like to use null, but nullable types suck, so check for projectileType == Projectile_Base as a failure state
		public System.Type ProjectileType
		{
			get { return projectileType; }
			internal set { projectileType = value; }
		}

		internal Projectile_Base? projectileToClone;
		internal Projectile_Base? ProjectileToClone
		{
			get
			{
                // Get from ammoFactory
			}
			private set { }
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

		// EVENTS
		void Awake()
		{

		}

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

		private Projectile_Base CreateProjectile()
		{
			Projectile_Base projectileInstance = Instantiate(ProjectileToClone);
			projectileInstance.parentPool = projectilePool;
			return projectileInstance;
		}

		// invoked when returning an item to the object pool
		private void OnReleaseToPool(Projectile_Base pooledObject)
		{
			pooledObject.gameObject.SetActive(false);
		}

		// invoked when retrieving the next item from the object pool
		private void OnGetFromPool(Projectile_Base pooledObject)
		{
			pooledObject.gameObject.SetActive(true);
		}

		// invoked when we exceed the maximum number of pooled items (i.e. destroy the pooled object)
		private void OnDestroyPooledObject(Projectile_Base pooledObject)
		{
			Destroy(pooledObject.gameObject);
		}

		public void EnableProjectilePool()
		{
			projectilePool = new ObjectPool<Projectile_Base>(CreateProjectile,
				OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
				true, poolSize, 120);
		}

		public void DisableProjectilePool()
		{
			projectilePool.Clear();
		}
    }
}
