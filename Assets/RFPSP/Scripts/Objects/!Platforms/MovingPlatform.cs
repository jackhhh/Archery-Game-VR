//MovingPlatform.cs by Azuline Studios© All Rights Reserved
//script for moving a platform horizontally back and forth
using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour {
	[Tooltip("Destination point for elevator to cycle to.")]
	public Transform pointB;
	[Tooltip("Destination point for elevator to cycle to.")]
	public Transform pointA;
	[Tooltip("Speed of elevator movement.")]
	public float speed = 1.0f;
	private float direction = 1.0f;
	private Transform myTransform;

	IEnumerator Start (){
		myTransform = transform;
	    while (true) {
	    
			if (myTransform.position.z < pointA.position.z){
				direction = 1;
			}else{
				if(myTransform.position.z > pointB.position.z){direction = -1;}
			}
			float delta = Time.deltaTime * 60;
			myTransform.Translate(direction  * -speed * delta, 0, direction  * speed * delta, Space.World);
				
			yield return null;
	    }
	}
	
	void OnCollisionEnter(Collision collision){
		//remove shells if they collide with a moving object like an elevator
		if(collision.gameObject.layer == 12 && collision.gameObject.GetComponent<ShellEjection>()){
			AzuObjectPool.instance.RecyclePooledObj(collision.gameObject.GetComponent<ShellEjection>().RBPoolIndex, collision.gameObject);
		}
	}
	
}