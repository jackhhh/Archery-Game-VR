//LeanColliderDamage.cs by Azuline Studios© All Rights Reserved
//Applies damage to player when they are leaning.
using UnityEngine;
using System.Collections;

public class LeanColliderDamage : MonoBehaviour {
	
	private FPSPlayer FPSPlayerComponent; 

	void Start () {
		FPSPlayerComponent =  Camera.main.GetComponent<CameraControl>().playerObj.GetComponent<FPSPlayer>();
	}
	
	public void ApplyDamage ( float damage ){
		FPSPlayerComponent.ApplyDamage(damage);
	}	
}
