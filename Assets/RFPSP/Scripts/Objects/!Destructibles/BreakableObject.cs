//BreakableObject.cs by Azuline Studios© All Rights Reserved
//Attach to object with a particle emitter and box collider to create breakable objects.
using UnityEngine;
using System.Collections;
//this script used to create breakable glass objects
public class BreakableObject : MonoBehaviour {
	[Tooltip("When hitpoints are depleted, object is destroyed.")]
	public float hitPoints = 150;
	private ParticleEmitter breakParticles;
	private bool broken;
	private Transform myTransform;
	
	void Start () {
		myTransform = transform;
		breakParticles = myTransform.GetComponent<ParticleEmitter>();
	}
	
	IEnumerator DetectBroken () {
		while(true){
			if(broken){//remove breakable object if it is broken and particles have faded
				//prevent attached hitmarks from being destroyed with game object
				FadeOutDecals[] decals = gameObject.GetComponentsInChildren<FadeOutDecals>(true);
				foreach (FadeOutDecals dec in decals) {
					dec.parentObjTransform.parent = AzuObjectPool.instance.transform;
					dec.parentObj.SetActive(false);
					dec.gameObject.SetActive(false);
				}
				//drop arrows if object is destroyed
				ArrowObject[] arrows = gameObject.GetComponentsInChildren<ArrowObject>(true);
				foreach (ArrowObject arr in arrows) {
					arr.transform.parent = null;
					arr.myRigidbody.isKinematic = false;
					arr.myBoxCol.isTrigger = false;
					arr.gameObject.tag = "Usable";
					arr.falling = true;
				}
				if(breakParticles && breakParticles.particleCount == 0.0f){
					Destroy(myTransform.gameObject);
					yield break;
				}
			}
			yield return new WaitForSeconds(0.1f);
		}
	}
	
	public void ApplyDamage (float damage){
		hitPoints -= damage;
		if(hitPoints <= 0 && !broken){
			if(breakParticles){
				breakParticles.Emit();//emit broken object particles
			}
			if(GetComponent<AudioSource>()){
				GetComponent<AudioSource>().pitch = Random.Range(0.95f * Time.timeScale, 1.0f * Time.timeScale);//add slight random value to breaking sound pitch for variety
				GetComponent<AudioSource>().Play();//play break sound
			}
			//disable mesh and collider of glass object untill object is deleted after sound effect finishes playing
			myTransform.GetComponent<MeshRenderer>().enabled = false;
			myTransform.GetComponent<BoxCollider>().enabled = false;//can use other collider types if needed
			broken = true;
			StartCoroutine(DetectBroken());
		}
	}
}