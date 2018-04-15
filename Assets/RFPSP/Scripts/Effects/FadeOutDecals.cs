//FadeOutDecals.cs by Azuline Studios© All Rights Reserved
//Fades out decals like bullet marks smoothly
using UnityEngine;
using System.Collections;

public class FadeOutDecals : MonoBehaviour {
	[HideInInspector]
	public float startTime;
	[Tooltip("How long should this mark stay in the scene before fading.")]
	public int markDuration = 10;
	private Renderer RendererComponent;
	[HideInInspector]
	public Color tempColor;
	[HideInInspector]
	public GameObject parentObj;
	[HideInInspector]
	public Transform parentObjTransform;
	[HideInInspector]
	public Transform myTransform;

	void Awake(){
		myTransform = transform;
		RendererComponent = GetComponent<Renderer>();
		parentObj = myTransform.parent.gameObject;
		parentObjTransform = parentObj.transform;
	}
		
	public void InitializeDecal(){
		startTime = Time.time + markDuration;
		tempColor =  RendererComponent.material.color;
		tempColor.a = 1.0f;
		RendererComponent.material.color = tempColor;
	}
	
	void Update (){
		
		if(startTime < Time.time){
			tempColor.a -= Time.deltaTime;//fade out the color's alpha amount
			RendererComponent.material.color = tempColor;//set the guiTexture's color to the value(s) of our temporary color vector
			tempColor.a = Mathf.Clamp(tempColor.a, 0.0f, 255.0f);//prevent alpha from going into negative values 
		}

		if(startTime < (Time.time - markDuration)){
			parentObjTransform.parent = AzuObjectPool.instance.transform;
			parentObj.SetActive(false);
		}
		
	}
}