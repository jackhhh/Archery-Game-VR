//FoliageRustle.cs by Azuline Studios© All Rights Reserved
//Plays foliage rustling sounds on trigger when objects enter trigger area.
using UnityEngine;
using System.Collections;

public class FoliageRustle : MonoBehaviour {
	private AudioSource rustleFx;
	private Footsteps FootstepsComponent;

	void Start () {
		FootstepsComponent = Camera.main.GetComponent<CameraControl>().playerObj.GetComponent<Footsteps>();	
		rustleFx = transform.gameObject.AddComponent<AudioSource>();
		rustleFx.spatialBlend = 1.0f;
		rustleFx.volume = FootstepsComponent.foliageRustleVol;
		rustleFx.pitch = 1.0f;
		rustleFx.dopplerLevel = 0.0f;
		rustleFx.maxDistance = 10.0f;
		rustleFx.rolloffMode = AudioRolloffMode.Linear;
	}

	void OnTriggerEnter(Collider col){
		if(FootstepsComponent.foliageRustles.Length > 0 && (col.gameObject.layer == 11 || col.gameObject.layer == 13)){
			rustleFx.clip = FootstepsComponent.foliageRustles[Random.Range(0, FootstepsComponent.foliageRustles.Length)];
			rustleFx.PlayOneShot(rustleFx.clip);
		}
	}

}
