using UnityEngine;
using System.Collections;
using UltraReal.Utilities;
using UltraReal.WeaponSystem;

/// <summary> 
/// This is just a dirt simple imput script.  This can easily be replaced with a custom version.
/// </summary>
public class BasicInputExample : UltraRealMonobehaviorBase {

	/// <summary> 
	/// Reference to the launcher script
	/// </summary>
	private UltraRealLauncherBase launcher;

	/// <summary> 
	/// Finds the launcher script.
	/// </summary>
	protected override void OnStart ()
	{
		base.OnStart ();

		launcher = GetComponent<UltraRealLauncherBase> ();
	}

	/// <summary> 
	/// Tests to see if the player is pressing the fire button, or the reload button.  Activateds
	/// methods on launcher accordingly.
	/// </summary>
	protected override void OnUpdate ()
	{
		base.OnUpdate ();

		if (Input.GetButton ("Fire1") && launcher != null)
			launcher.Fire ();

		if (Input.GetKeyDown (KeyCode.R))
			launcher.Reload ();
	}
}
