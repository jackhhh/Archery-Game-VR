using UnityEngine;
using System.Collections;
using UltraReal.Utilities;

namespace UltraReal.WeaponSystem
{

    /// <summary> 
    /// Base class used for all damage scripts used in the weapon system. 
    /// </summary>
    public abstract class UltraRealDamageBase : UltraRealMonobehaviorBase
    {
        /// <summary> 
        /// Health value that will be subtracted from when damaged
        /// </summary>
        [SerializeField]
        protected float _health = 100f;

        /// <summary> 
        /// Health value that will be subtracted from when damaged
        /// </summary>
        public float Health
        {
            get { return _health; }
            set
            {
                _health = value;
                if (_health < 0) {
                    _health = 0;
                    OnDeath();
                }
            }
        }

        /// <summary> 
        /// Applies damage to the health value.
        /// </summary>
        protected virtual void ApplyDamage(float damageAmount)
        {
            Health -= damageAmount;
            OnDamaged(damageAmount);
        }

        /// <summary> 
        /// Virtual method for death events.
        /// </summary>
        protected virtual void OnDeath() { }

        /// <summary> 
        /// Virtual method for damage events.
        /// </summary>
        protected virtual void OnDamaged(float damage) { }
    }
}
