using UnityEngine;
using System.Collections;

public class TimedDestroy : MonoBehaviour {

	[SerializeField]
	float _destroyDelay = 5;

	void OnEnable()
	{
		StartCoroutine (DelayDestroy ());
	}

	private IEnumerator DelayDestroy()
	{
		yield return new WaitForSeconds(_destroyDelay);
		Destroy (gameObject);
	}
}
