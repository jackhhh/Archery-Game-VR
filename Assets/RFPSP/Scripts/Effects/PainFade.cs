//PainFade.cs by Azuline Studios© All Rights Reserved
//script to make screen flash red when damage taken
using UnityEngine;
using System.Collections;
public class PainFade : MonoBehaviour {
	
	[HideInInspector]
	public GameObject painFadeObj;
	
	public void FadeIn ( Color color, Texture2D fadeTexture, float fadeLength ){
		
		fadeTexture.SetPixel(0, 0, color);
		fadeTexture.Apply();

		painFadeObj.layer = 14;//set fade object's layer to one not ignored by weapon camera
		painFadeObj.AddComponent<GUITexture>();
		painFadeObj.transform.position = new Vector3 (0.5f, 0.5f, 1000);
		painFadeObj.GetComponent<GUITexture>().texture = fadeTexture;
		StartCoroutine(DoFade(fadeLength, true, color.a));
		
	}
		
	IEnumerator DoFade ( float fadeLength, bool destroyTexture, float alpha ){

		//Create a temporary Color var and make alpha of color = 0 (transparent for starting fade out)
		Color tempColor = GetComponent<GUITexture>().color; 
   		tempColor.a = 0.0f;//store the color's alpha amount
    	GetComponent<GUITexture>().color = tempColor;//set the guiTexture's color to the value of our temporary color var
		alpha = Mathf.Clamp01(alpha);
		
		//Fade texture out
		float time = 0.0f;
		while (time < fadeLength){
			time += Time.deltaTime;
			tempColor.a = Mathf.InverseLerp(fadeLength, 0.0f, time) * alpha;
			GetComponent<GUITexture>().color = tempColor;
			yield return null;
		}
	
		Destroy (gameObject);
		
	}
}