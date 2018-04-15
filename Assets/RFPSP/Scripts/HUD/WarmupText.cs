//WarmupText.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class WarmupText : MonoBehaviour {
	//draw ammo amount on screen
	[HideInInspector]
	public float warmupGui;//bullets remaining in clip
	private float oldWarmup = -512;
	[Tooltip("Color of GUIText.")]
	public Color textColor;
	[Tooltip("Offset from total screen width to position GUIText.")]
	public float horizontalOffset = 0.65f;
	[Tooltip("Offset from total screen height to position GUIText.")]
	public float verticalOffset = 0.96f;
	[HideInInspector]
	public float horizontalOffsetAmt = 0.78f;
	[HideInInspector]
	public float verticalOffsetAmt = 0.1f;
	[Tooltip("Scaled GUIText size, relative to screen size.")]
	public float fontScale = 0.032f;
	[HideInInspector]
	public bool waveBegins;
	[HideInInspector]
	public bool waveComplete;
	private GUIText guiTextComponent;
	private float oldWidth;//to track screen size change
	
	void OnEnable(){
		guiTextComponent = GetComponent<GUIText>();
		horizontalOffsetAmt = horizontalOffset;
		verticalOffsetAmt = verticalOffset;	
		guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
		oldWarmup = -512;
		oldWidth = Screen.width;
	}
	
	void Update(){
		//only update GUIText if value to be displayed has changed
		if(warmupGui != oldWarmup || oldWidth != Screen.width) {

			if(!waveComplete){
				if(!waveBegins){
					guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
					guiTextComponent.text = "Warmup Time : "+  Mathf.Round(warmupGui).ToString();
				}else{
					guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * (fontScale * 1.5f));
					guiTextComponent.text = "INCOMING WAVE";
				}
			}else{
				guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * (fontScale * 1.5f));
				guiTextComponent.text = "WAVE COMPLETE";
			}
			
			guiTextComponent.pixelOffset = new Vector2 (Screen.width * horizontalOffsetAmt, Screen.height * verticalOffsetAmt);
			guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
			oldWidth = Screen.width;
			
			guiTextComponent.material.color = textColor;
			oldWarmup = warmupGui;
	    }
	
	}
}