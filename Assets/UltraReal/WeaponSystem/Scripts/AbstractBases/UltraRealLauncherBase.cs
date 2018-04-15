using UnityEngine;
using System.Collections;
using UltraReal.Utilities;

namespace UltraReal.WeaponSystem
{
    /// <summary> 
    /// Base class used for all Launcher scripts used in the weapon system. 
    /// </summary>
    public abstract class UltraRealLauncherBase : UltraRealWeaponSystemBase
    {

        /// <summary> 
        /// Reference to an ammo GameObject that is assigned a script derived from UltraRealAmmoBase.
        /// </summary> 
        [SerializeField]
        protected UltraRealAmmoBase _ammo;

    }
}
