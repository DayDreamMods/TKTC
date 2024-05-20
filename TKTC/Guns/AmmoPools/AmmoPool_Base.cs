using System.Collections.Generic;
using UnityEngine;

namespace TKTC.Guns.AmmoPools
{
	internal abstract class AmmoPool_Base
	{
		// VARIABLES
		private System.Type? projectileType; // need to decide if this should be nullable or assign a default projectile_bullet class
		public System.Type ProjectileType
		{
			get { return projectileType; }
			internal set { projectileType = value; }
		}

		private float amount;
		public float Amount
		{
			get { return amount; }
			internal set { amount = value; }
		}

		private float capacity;
		public float Capacity
		{
			get { return capacity; }
			internal set { capacity = value; }
		}

		private List<AmmoPool_Collection> parentCollections = new();

		// CONSTRUCTORS
		public AmmoPool_Base()
		{
			// access current round settings to determine initial fill amounts
		}

		public AmmoPool_Base(float initialAmount, float newCapacity, System.Type newProjectileType)
		{
			SetValues(initialAmount, newCapacity, newProjectileType);
#if DEBUG
			if (capacity == 0f) TKTC.Logger.LogDebug("AmmoPool created with zero capacity!");
#endif
		}

		public AmmoPool_Base(AmmoPool_Base copyFrom)
		{
			SetValues(copyFrom.Amount, copyFrom.Capacity, copyFrom.ProjectileType);
		}

		// METHODS
		public bool IsEmpty => amount <= 0f;
		public bool IsFull => amount >= 0f;

		private float AddAmmo(float toAdd)
		{
			float willAdd = Mathf.Min(capacity - amount, toAdd);
			amount += willAdd;
			toAdd -= willAdd;
			return toAdd; // Return remainder
		}

		public bool AddAmmo(AmmoPool_Base toMerge)
		{
			// Check if the type we are pulling from is of the same type or a base type of this pool, only allow loading into a same or derived ammo pool
			if (IsFull || !toMerge.GetType().IsAssignableFrom(this.GetType())) return false;

			toMerge.amount = AddAmmo(toMerge.Amount);
			return true;
		}

		public bool TryRemoveOne() // Just for single shot reloading
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

		internal abstract AmmoPool_Base Clone(); // needs to return a new object of the same type as 'this'
		/*{
			return new AmmoPool_Base(this);
		}*/

		protected virtual void SetValues(float newAmount, float newCapacity, System.Type newProjectileType)
		{
			capacity = Mathf.Min(newCapacity, 0f);
			amount = Mathf.Clamp(newAmount, 0f, capacity);
			projectileType = newProjectileType;
		}

		internal void AddedToCollection(AmmoPool_Collection newParent)
		{
			if (!parentCollections.Contains(newParent)) parentCollections.Add(newParent);
		}

		internal void RemovedFromCollection(AmmoPool_Collection oldParent)
		{
			parentCollections.Remove(oldParent);
		}
	}
}
