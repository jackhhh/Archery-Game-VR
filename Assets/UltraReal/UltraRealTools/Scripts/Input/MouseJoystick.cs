using UnityEngine;
using System.Collections;
using UltraReal.Utilities;

public class MouseJoystick : UltraRealMonobehaviorBase {

	protected Vector2 _axis;

	[SerializeField]
	protected Camera _inputCamera = null;

	[SerializeField]
	protected float _inputDistanceFromCamera = 2f;

	public Vector2 Axis
	{
		get{ return _axis; }
		set{ _axis = value; }
	}

	protected override void OnStart ()
	{
		base.OnStart ();

		if (_inputCamera == null)
			_inputCamera = GetComponent<Camera> ();

		if (_inputCamera == null)
			_inputCamera = Camera.main;

		if (_inputCamera == null)
			Debug.LogError ("MouseJoystick need a Camera reference");
	}

	protected override void OnUpdate ()
	{
		base.OnUpdate ();
	
		float x = Input.mousePosition.x;
		float y = Input.mousePosition.y;
		Vector3 p = Camera.main.ScreenToViewportPoint(new Vector3(x, y, _inputDistanceFromCamera));
		_axis = new Vector2 (Mathf.Clamp(p.x * 2f - 1f,-1f,1f),Mathf.Clamp(p.y * 2f - 1f,-1f,1f));
	}
}