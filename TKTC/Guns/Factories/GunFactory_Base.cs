using System.Collections.Generic;
using UnityEngine;

namespace TKTC.Guns.Factories
{
	internal class GunFactory_Base : MonoBehaviour
	{
		

		// List because we want to be able to add or rpemove guns during runtime, these are starting settings
		private List<GunType> spawnableGuns = new List<GunType>( [
			new GunType("VN1", typeof(Gun_VN1) )
		] );

		// VARIABLES
		// TODO: day - configs, how?


		// METHODS
		public GameObject SpawnGun(string name, Transform newParent)
		{
			GunType? toSpawn = null;
			foreach (GunType tempType in spawnableGuns)
			{
				if (tempType.name == name) toSpawn = tempType;
				break;
			}

			if (toSpawn is null || toSpawn?.type is null || !toSpawn.type.IsSubclassOf(typeof(Gun_Base))) return null;

			GameObject newGun = new();
			newGun.AddComponent(toSpawn?.type);

			newGun.transform.SetParent(newParent);
			return newGun;
		}


		

		public bool AddGunToSpawn(string newName, System.Type newType)
		{
			return AddGunToSpawn(new GunType(newName, newType));
		}

		public bool AddGunToSpawn(GunType newGun)
		{
			// Sanity check, make sure name is valid and the class type is a gun
			if (newGun.name is null || !newGun.type.IsSubclassOf(typeof(Gun_Base))) return false;
			newGun.name = newGun.name.Trim();
			if (newGun.name == "") return false;

			// Look for existing spawn with same name
			foreach (GunType tempType in spawnableGuns) if (tempType.name == newGun.name) return false;
			
			// Add spawn option
			spawnableGuns.Add(newGun);
			return true;
		}
	}

	public struct GunType
	{
		public string name;
		public System.Type type;

		public GunType(string newName, System.Type newType)
		{
			name = newName;
			type = newType;
		}
	}
}
