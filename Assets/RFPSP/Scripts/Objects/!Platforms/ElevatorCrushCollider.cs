//ElevatorCrushCollider.cs by Azuline Studios© All Rights Reserved
//script for instant death collider which kills player on contact
using UnityEngine;
using System.Collections;

public class ElevatorCrushCollider : MonoBehaviour {
	[Tooltip("Sound effect to play when player is crushed under elevator.")]
	public AudioClip squishSnd;
	private bool fxPlayed;
	private float crushTime;
	
	void OnTriggerEnter ( Collider col  ){
		if(col.gameObject.tag == "Player"){
			FPSPlayer player = col.GetComponent<FPSPlayer>();
			if (player && !fxPlayed) {
				player.ApplyDamage(player.maximumHitPoints + 1.0f);
				PlayAudioAtPos.PlayClipAt(squishSnd, player.transform.position, 0.75f);
				crushTime = Time.time;
				fxPlayed = true;
			}
		}
	}
	
	void OnTriggerExit ( Collider col  ){
		if(col.gameObject.tag == "Player" && crushTime + 1.0f < Time.time){
			fxPlayed = false;
		}
	}
}