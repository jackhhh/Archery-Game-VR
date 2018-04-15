//MovingElevator.cs by Azuline Studios© All Rights Reserved
//script for moving an elevator vertically up and down
using UnityEngine;
using System.Collections;

public class MovingElevator : MonoBehaviour {
	[Tooltip("Destination point for elevator to cycle to.")]
	public Transform pointB;
	private Vector3 pointA;
	[Tooltip("Speed of elevator movement.")]
	public float speed = 1.0f;
	private float direction = 1.0f;
	private Transform myTransform;
	
	IEnumerator Start (){
		myTransform = transform;
		pointA = transform.position;
		while (true) {
		
			if (myTransform.position.y > pointA.y){
				direction = -1;
			}else{
				if(myTransform.position.y < pointB.position.y){direction = 1;}
			}
			float delta = Time.deltaTime * 60;
			myTransform.Translate(0f, direction  * speed * delta, 0f, Space.World);
			
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