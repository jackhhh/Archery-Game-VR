//AppleFall.cs by Azuline Studios© All Rights Reserved
//Script to make apples fall from trees if damaged or picked.
using UnityEngine;
using System.Collections;

public class AppleFall : MonoBehaviour {
	private Transform myTransform;

	void Start () {
		myTransform = transform;
	}
	
	public void ApplyDamage ( float damage ){
		if(!myTransform.GetComponent<Rigidbody>().useGravity){
			GetComponent<AudioSource>().pitch = Random.Range(0.75f * Time.timeScale, 1.0f * Time.timeScale);//add slight random value to sound pitch for variety
			GetComponent<AudioSource>().Play();//play "picking" sound effect
			myTransform.GetComponent<Rigidbody>().useGravity = true;	
		}
	}
	
	public void OnCollisionEnter(){
		if(!myTransform.GetComponent<Rigidbody>().useGravity){
			GetComponent<AudioSource>().pitch = Random.Range(0.75f * Time.timeScale, 1.0f * Time.timeScale);//add slight random value to sound pitch for variety
			GetComponent<AudioSource>().Play();//play "picking" sound effect
			myTransform.GetComponent<Rigidbody>().useGravity = true;	
		}
	}
	
}
