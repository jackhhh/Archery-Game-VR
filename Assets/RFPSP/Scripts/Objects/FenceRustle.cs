//FenceRustle.cs by Azuline Studios© All Rights Reserved
//Plays fence rustling sounds on trigger when objects enter trigger area.
using UnityEngine;
using System.Collections;

public class FenceRustle : MonoBehaviour {
	[Tooltip("Sound effects to play when player runs up against this object.")]
	public AudioClip[] fenceRustles;
	[Tooltip("Volume of sound effects when player runs up against this object.")]
	public float rustleVol = 1.0f;

	void OnTriggerEnter(Collider col){
		if(fenceRustles.Length > 0 && (col.gameObject.layer == 11 || col.gameObject.layer == 13)){
			PlayAudioAtPos.PlayClipAt(fenceRustles[Random.Range(0, fenceRustles.Length)], transform.position, rustleVol);
		}
	}

}
