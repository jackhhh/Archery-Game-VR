using UnityEngine;
using System.Collections;
using UltraReal.Utilities;
using UltraReal.WeaponSystem;

public class BasicAmmoRayCast : UltraRealAmmoBase {

	/// <summary> 
	/// Did this ammo already hit something.
	/// </summary>
	bool _hit = false;

	/// <summary> 
	/// The distance the raycast will fire to the target.
	/// </summary>
	[SerializeField]
	protected float _fireDistance = 100f;

	/// <summary> 
	/// Projects a ray to the target and spawns the hit effect if required.
	/// </summary>
	protected override void OnUpdate ()
	{
		base.OnUpdate ();

		RaycastHit hit = new RaycastHit();
		Ray ray = new Ray(CachedTransform.position,transform.TransformDirection (Vector3.forward));
		if (!_hit && _hitPrefab != null && Physics.Raycast (ray,out hit,_fireDistance)){
			SpawnHit(hit.collider,hit.point,Quaternion.FromToRotation (Vector3.forward, hit.normal));
			_hit = true;
		}
	}	
}
