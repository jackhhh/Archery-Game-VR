//PlayAudioAtPos.cs 
//Plays audio at a point with several parameters for customization of playback
using UnityEngine;
using System.Collections;

public static class PlayAudioAtPos{

	public static AudioSource PlayClipAt(AudioClip clip,//audio clip to play
										 Vector3 pos,//position to player audio clip
										 float vol,//volume of played audio clip
										 float blend = 1.0f,//3d factor of sound effect
										 float pitch = 1.0f,//pitch of sound effect
										 float minDist = 1.0f,//minimum distance player can hear sound
										 float maxDist = 500.0f){
										 
		//temp audio object must be index 0 (first) in object pool
		GameObject tempGO = AzuObjectPool.instance.SpawnPooledObj(0, pos, Quaternion.identity) as GameObject;
		TempAudioTimer TimerComponent = tempGO.GetComponent<TempAudioTimer>();
		AudioSource aSource = TimerComponent.aSource; // add an audio source
		aSource.clip = clip; // define the clip
		// set other aSource properties here, if desired
		aSource.spatialBlend = blend;
		aSource.minDistance = minDist;
		aSource.maxDistance = maxDist;
		aSource.volume = vol;
		aSource.pitch = pitch;
		aSource.Play(); // start the sound
		TimerComponent.StartCoroutine(TimerComponent.DeactivateTimer());
		return aSource; // return the AudioSource reference
	}
}
