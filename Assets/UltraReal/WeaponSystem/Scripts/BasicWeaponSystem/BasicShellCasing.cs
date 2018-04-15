using UnityEngine;
using System.Collections;
using UltraReal.Utilities;
using UltraReal.WeaponSystem;

/// <summary> 
/// All weapon launchers must derive from this class.  This would be your gun script.
/// </summary>
public class BasicShellCasing : UltraRealShellCasingBase {

	/// <summary> 
	/// Reference to a sound effect when shell collides with objects.
	/// </summary>
	[SerializeField]
	AudioClip _collisionSound = null;

	/// <summary> 
	/// Plays sound effect when shell collides with objects.
	/// </summary>
	protected override void OnCollisionEnter (Collision col)
	{
		base.OnCollisionEnter (col);

		if (CachedAudioSource != null && _collisionSound != null) {
			CachedAudioSource.PlayOneShot (_collisionSound);
		}
	}
}
