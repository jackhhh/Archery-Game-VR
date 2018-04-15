using UnityEngine;
using System.Collections;
using UltraReal.Utilities;

namespace UltraReal.WeaponSystem
{
    /// <summary> 
    /// Abstract base class for ammo.  Contains the bare essential data for ammo. 
    /// </summary> 
    public abstract class UltraRealAmmoBase : UltraRealMonobehaviorBase
    {

        /// <summary> 
        /// Reference to an AudioClip for the firing sound.
        /// </summary> 
        [SerializeField]
        protected AudioClip _fireSound = null;

        /// <summary> 
        /// Reference shell casing prefab.  Must have the script derived from the UltraRealShellCasingBase class.
        /// This prefab will usually be fired from the weapons ejector. 
        /// </summary> 
        [SerializeField]
        protected UltraRealShellCasingBase _shellCasing = null;

        /// <summary> 
        /// Reference to an GameObject for the Muzzle flash.
        /// </summary> 
        [SerializeField]
        protected GameObject _muzzleFlashPrefab = null;

        /// <summary> 
        /// The prefab that will be spawned at the hit location
        /// </summary>
        [SerializeField]
        protected GameObject _hitPrefab = null;

        /// <summary> 
        /// The amound of damage caused by a shot.
        /// </summary>
        [SerializeField]
        protected float _damageAmount = 1f;

        /// <summary> 
        /// Spawns the ammo using the parameter info.
        /// </summary> 
        public virtual UltraRealAmmoBase SpawnAmmo(Transform muzzleTransform, Transform ejectorTransform, float shellCasingEjectForce, float shellCasingEjectTorque, float spread, AudioSource gunAudioSource)
        {
            if (muzzleTransform != null) {

                GameObject _ammoGameObject = GameObject.Instantiate(gameObject, muzzleTransform.position, muzzleTransform.rotation) as GameObject;

                UltraRealAmmoBase _newAmmo = _ammoGameObject.GetComponent<UltraRealAmmoBase>();

                _newAmmo.CachedTransform.RotateAround(_newAmmo.CachedTransform.position, Vector3.right, Random.Range(-spread, spread));

                if (_shellCasing != null && _shellCasing.gameObject != null && ejectorTransform != null) {
                    GameObject _newCasing = GameObject.Instantiate(_shellCasing.gameObject, ejectorTransform.position, ejectorTransform.rotation) as GameObject;

                    Rigidbody _newRigid = _newCasing.GetComponent<Rigidbody>();

                    if (_newRigid != null) {
                        _newRigid.AddRelativeForce(new Vector3(
                            Random.Range(-shellCasingEjectForce, shellCasingEjectForce) * 0.1f,
                            Random.Range(-shellCasingEjectForce, shellCasingEjectForce) * 0.1f,
                            Random.Range(shellCasingEjectForce * 0.5f, shellCasingEjectForce)));

                        _newRigid.AddTorque(new Vector3(
                            Random.Range(-shellCasingEjectTorque, shellCasingEjectTorque),
                            Random.Range(-shellCasingEjectTorque, shellCasingEjectTorque),
                            Random.Range(-shellCasingEjectTorque, shellCasingEjectTorque)));
                    }
                }

                if (_muzzleFlashPrefab != null)
                    GameObject.Instantiate(_muzzleFlashPrefab, muzzleTransform.position, muzzleTransform.rotation);

                if (gunAudioSource != null)
                    gunAudioSource.PlayOneShot(_newAmmo._fireSound);

                return _newAmmo;
            }
            else
                return null;
        }

        /// <summary> 
        /// Spawns the hit effect at the hit location.
        /// </summary> 
        protected virtual void SpawnHit(Collider col, Vector3 position, Quaternion rotation)
        {
            GameObject.Instantiate(_hitPrefab, position, rotation);
            DamageTarget(col.gameObject);
        }

        /// <summary> 
        /// Damages target through a send message. You can overright this if you want to use something
        /// like events instead.
        /// </summary> 
        protected virtual void DamageTarget(GameObject target)
        {
            target.SendMessage("ApplyDamage", _damageAmount, SendMessageOptions.DontRequireReceiver);
        }
    }
}
