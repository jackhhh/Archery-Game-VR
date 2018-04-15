using UnityEngine;
using System.Collections;
using UltraReal.Utilities;

namespace UltraReal.WeaponSystem
{
    /// <summary> 
    /// Base class for all ammo scripts used in the weapon system.  
    /// </summary>
    public abstract class UltraRealWeaponSystemBase : UltraRealMonobehaviorBase
    {

        /// <summary> 
        /// Fires the weapon.  
        /// </summary> 
        public virtual void Fire() { }

        /// <summary> 
        /// Reloads the weapon.  
        /// </summary> 
        public virtual void Reload() { }

        /// <summary> 
        /// Missfires the weapon.  
        /// </summary> 
        public virtual void MissFire() { }

    }
}
