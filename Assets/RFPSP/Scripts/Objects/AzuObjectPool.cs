//AzuObjectPool.cs by Azuline Studios© All Rights Reserved
//Creates object pools and contains functions for instantiating and recycling pooled objects
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MultiDimensionalPool{
	[Tooltip("The gameobject used to create an object pool.")]
	public GameObject prefab;
	[Tooltip("Number of this type of object to store in object pool.")] 
	public int poolSize;
	[HideInInspector]
	public int nextActive = 0;//index of next pooled object to spawn
	[Tooltip("True if spawned pooled object should ignore collision with player.")]
	public bool ignorePlayerCollision;
	[Tooltip("List of pooled game objects (should never have any missing list entries at runtime).")]
	public List<GameObject> pooledObjs = new List<GameObject>();
}

[System.Serializable]
public class AzuObjectPool : MonoBehaviour {

	private Collider FPSWalkerCapsule;
	public static AzuObjectPool instance;
	private Transform myTransform;

	[Tooltip("List of object pools that are active in the scene. Index numbers of pools in this list are used by other scripts to identify which pooled objects to spawn.")]
	public List<MultiDimensionalPool> objRegistry = new List<MultiDimensionalPool>();

	void Awake(){
		instance = this;
	}

	void Start () {

		FPSWalkerCapsule = Camera.main.transform.GetComponent<CameraControl>().FPSWalkerComponent.GetComponent<Collider>();
		myTransform = transform;

		//create the object pools
		for(int i = 0; i < objRegistry.Count; i++){
			for(int n = 0; n < objRegistry[i].poolSize; n++){
				if(objRegistry[i].prefab){
					GameObject obj;
					obj = Instantiate(objRegistry[i].prefab, myTransform.position, myTransform.rotation) as GameObject;//instantiate a pooled object
					if(objRegistry[i].ignorePlayerCollision){//ignore collision with player if ignorePlayerCollision is true
						Physics.IgnoreCollision(obj.GetComponent<Collider>(), FPSWalkerCapsule, true);
					}
					obj.SetActive(false);
					objRegistry[i].pooledObjs.Add(obj);//add object to pool
					obj.transform.parent = myTransform;
				}
			}
		}
		
	}

	//spawn an object from a pool
	public GameObject SpawnPooledObj(int objRegIndex, Vector3 spawnPosition, Quaternion spawnRotation ){
		GameObject spawnObj = objRegistry[objRegIndex].pooledObjs[objRegistry[objRegIndex].nextActive];//identify which pool and object to spawn from with objRegIndex
		spawnObj.SetActive(true);
		//set spawned object's position and rotation
		spawnObj.transform.position = spawnPosition;
		spawnObj.transform.rotation = spawnRotation;
		//find index of object in pool to spawn after this one
		if(objRegistry[objRegIndex].nextActive == objRegistry[objRegIndex].pooledObjs.Count - 1){
			objRegistry[objRegIndex].nextActive = 0;//start from beginning if this is the last object in the pool
		}else{
			objRegistry[objRegIndex].nextActive++; 
		}

		return spawnObj;
		
	}

	//recycle spawned pool object (visibly remove from scene and return to its object pool)
	public GameObject RecyclePooledObj(int objRegIndex, GameObject obj){
		GameObject recycleObj = objRegistry[objRegIndex].pooledObjs[objRegistry[objRegIndex].pooledObjs.IndexOf(obj)];//identify which pool and object to recycle with objRegIndex
		recycleObj.transform.parent = myTransform;
		recycleObj.SetActive(false);

		return recycleObj;
		
	}

}
