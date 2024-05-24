using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Pool;

namespace TKTC.Guns.Projectiles
{
	internal class Projectile_Base : MonoBehaviour
	{
		// deactivate after delay
		Rigidbody rBody;
		private float timeoutDelay = 2f; // TODO: dynamically determine based on muzzle velocity at Fire()
		private float poolBorrowTimer;
		internal IObjectPool<Projectile_Base>? parentPool;

		void Awake()
		{
			rBody = GetComponent<Rigidbody>();
		}

		private IEnumerator TracePath(float velocity, Transform location)
		{
			poolBorrowTimer = timeoutDelay;
			RaycastHit objectHit;
			Vector3 fwd;
			while (poolBorrowTimer > 0f)
			{
				poolBorrowTimer -= Time.deltaTime;
				fwd = location.TransformDirection(Vector3.forward);

				// Debug.DrawRay(location.position, fwd * velocity, Color.green);
				if (Physics.Raycast(location.position, fwd, out objectHit, velocity))
				{
					TKTC.Logger.LogDebug($"You shot: {objectHit.transform.gameObject.name}");
				}
				yield return 0; // wait for next frame
			}
			Deactivate();
		}

		public void Deactivate()
		{
			rBody.velocity = new Vector3(0f, 0f, 0f);
			rBody.angularVelocity = new Vector3(0f, 0f, 0f);
			parentPool?.Release(this);
		}

		internal void Fire(float newVelocity)
        {
			transform.SetParent(null, true);

		}
    }
}
