using UnityEngine;
using System.Collections;

public class ShowcaseAnimation : MonoBehaviour {

	private float seconds = 1f;
	private float pause = 3f;

	void Update () {
		seconds -= 1 * Time.deltaTime;
		if (seconds <= 0) {
			GetComponent<Animation>().Play("empty");
			GetComponent<Animation>().PlayQueued("loaded");
			GetComponent<Animation>().PlayQueued("pull");
			GetComponent<Animation>().PlayQueued("pulled");
			GetComponent<Animation>().PlayQueued("release");
			seconds = pause;
		}
	}
}
