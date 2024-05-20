using System.Collections.Generic;

namespace TKTC.Guns.AmmoPools
{
	internal class AmmoPool_Collection
	{
		// VARIABLES
		private List<AmmoPool_Base> pools = new();

		// METHODS
		public void AddAmmo(AmmoPool_Base toMerge)
		{
			// Even if a pool of the same type is present first check to see if allowance has changed, eg a weapon was dropped
			if (!AllowedToAddPool(toMerge)) return;

			// Check if there are any existing pools to distribute the ammo between
			bool foundSimilar = false;
			foreach (AmmoPool_Base tempPool in pools)
			{
				if (!toMerge.IsEmpty && !tempPool.IsFull && toMerge.GetType().IsAssignableFrom(tempPool.GetType()))
				{
					tempPool.AddAmmo(toMerge);
					foundSimilar = true;
				}
			}
			if (foundSimilar) return;

			// Add the pool if were allowed to, via clone, then empty the original
			if (AllowedToAddPool(toMerge))
			{
				AddPool(toMerge.Clone());
				toMerge.Amount = 0f;
			}
		}

		internal virtual bool AllowedToAddPool(AmmoPool_Base testPool) // Override to limit what ammo can be stored, for instance limiting to what the equipped guns can use
		{
			return true;
		}

		internal bool AddPool(AmmoPool_Base toAdd)
		{
			if (pools.Contains(toAdd)) return false;
			pools.Add(toAdd);
			toAdd.AddedToCollection(this);
			return true;
		}

		internal bool RemovePool(AmmoPool_Base toRemove)
		{
			if (!pools.Contains(toRemove)) return false;
			pools.Remove(toRemove);
			toRemove.RemovedFromCollection(this);
			return true;
		}
	}
}
