using UnityEngine;
using System.Collections;
using UltraReal.Utilities;
using UltraReal.WeaponSystem;

/// <summary> 
/// Rigid Body ammo.  This will use force for movement and will collide with colliders.
/// Use for slow moving projectiles only.  Faster moving projectiles will require increasing
/// Higher physics sampling rates that can adversly impact perfomance.  Use BasicAmmoRayCast for 
/// very fast moving projectiles instead.
/// </summary>
public class BasicAmmoRigidBody : UltraRealAmmoBase
{

    /// <summary> 
    /// The amount of force applied to the ammo RigidBody.
    /// </summary>
    [SerializeField]
    private float _forwardForce = 1000f;

    protected override void OnStart()
    {
        base.OnStart();

        if (CachedRigidbody == null)
            CachedRigidbody = gameObject.AddComponent<Rigidbody>();

        CachedRigidbody.AddRelativeForce(new Vector3(0f, 0f, _forwardForce));

    }

    /// <summary> 
    /// Applies force to the Rigid body ammo.
    /// </summary>
    protected override void OnUpdate()
    {
        base.OnUpdate();


    }

    /// <summary> 
    /// Tests for collision and spawns hit effect.
    /// </summary>
    protected override void OnCollisionEnter(Collision col)
    {
        base.OnCollisionEnter(col);

        if (_hitPrefab != null) {
            foreach (ContactPoint _contactPoint in col.contacts) {
                SpawnHit(col.collider, _contactPoint.point, Quaternion.FromToRotation(Vector3.forward, _contactPoint.normal));
            }
        }

        Destroy(gameObject);
    }
}

