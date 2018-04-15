using UnityEngine;
using System.Collections;
using UltraReal.Utilities;
using UltraReal.WeaponSystem;

/// <summary> 
/// Basic launcher class.  This would be your gun script.
/// </summary>
public class BasicLauncher : UltraRealLauncherBase {

	/// <summary> 
	/// Time before the next shot can be fired.
	/// </summary>
	float _nextShotTime;

	/// <summary> 
	/// keeps track of the current ammo count before a reload is requred.
	/// </summary>
	int _ammoCount;

	/// <summary> 
	/// Time delay before next shot can be fired.  A machine gun would use a really small shot delay.
	/// </summary>
	[SerializeField] 
	private float _shotDelay = 0.1f;

	/// <summary> 
	/// How long it takes to reload the wepaon.
	/// </summary>
	[SerializeField]
	private float _reloadDelay = 1f;

	/// <summary> 
	/// The amount of ammo a magazine(clip) can hold.
	/// </summary>
	[SerializeField]
	private int _magazineSize = 20;

	/// <summary> 
	/// Reference to the Transform for the shell ejector.
	/// </summary>
	[SerializeField]
	private Transform _ejectorTransform = null;

	/// <summary> 
	/// Reference to the Transform for the muzzle position.
	/// </summary>
	[SerializeField]
	private Transform _muzzleTransform = null;

	/// <summary> 
	/// Reference to the AudoClip for reloading.
	/// </summary>
	[SerializeField]
	private AudioClip _reloadSound = null;

	
	/// <summary> 
	/// Reference to the AudoClip for missfiring.
	/// </summary>
	[SerializeField]
	private AudioClip _missfireSound = null;

	/// <summary> 
	/// Force applied to ejected shells.
	/// </summary>
	[SerializeField]
	protected float _ejectorForce = 100f;

	/// <summary> 
	/// Torque applied to ejected shells.
	/// </summary>
	[SerializeField]
	protected float _ejectorTorque = 100f;

    /// <summary> 
    /// Reference to the AudioSource for the gun.  If it's null, it will create one.
    /// </summary>
    [SerializeField]
    protected AudioSource _audioSource = null;

    /// <summary> 
    /// Reference to animator for the shooting, and reloading
    /// </summary>
    [SerializeField]
    protected Animator _animator = null;

    /// <summary> 
    /// Name of the trigger in the animation controller for Firing.
    /// </summary>
    [SerializeField]
    protected string _fireAnimTriggerName = "Fire";

    /// <summary> 
    /// Name of the trigger in the animation controller for Reloading.
    /// </summary>
    [SerializeField]
    protected string _reloadAnimTriggerName = "Reload";

	/// <summary> 
	///	Sets defaults for weapon
	/// </summary>
	protected override void OnStart ()
	{
		base.OnStart ();

		_nextShotTime = Time.time;
		_ammoCount = _magazineSize;

        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();
	}

	/// <summary> 
	///	Fires the weapon
	/// </summary>
	public override void Fire ()
	{
		if (_nextShotTime <= Time.time){
			if (_ammoCount > 0 && _ammo != null) {

				if(_muzzleTransform != null)
                    _ammo.SpawnAmmo(_muzzleTransform, _ejectorTransform, _ejectorForce, _ejectorTorque, 2f, _audioSource);

				_ammoCount--;

                if (_animator != null)
                    _animator.SetTrigger(_fireAnimTriggerName);

				base.Fire ();
			}
			else
				MissFire();
			_nextShotTime = Time.time + _shotDelay;
		}
	}

	/// <summary> 
	///	Forces a weapon missfire.
	/// </summary>
	public override void MissFire()
	{
		base.MissFire ();

		if (_missfireSound != null && _audioSource != null)
			_audioSource.PlayOneShot (_missfireSound);
	}

	/// <summary> 
	/// Reloads the weapon
	/// </summary>
	public override void Reload ()
	{
		base.Reload ();

		if (_reloadSound != null && _audioSource != null)
			_audioSource.PlayOneShot (_reloadSound);

        if (_animator != null)
            _animator.SetTrigger(_reloadAnimTriggerName);

		_nextShotTime = Time.time + _reloadDelay;
		_ammoCount = _magazineSize;
	}
}
