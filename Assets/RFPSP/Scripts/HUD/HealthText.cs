//HealthText.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class HealthText : MonoBehaviour {
	//draw health amount on screen
	[HideInInspector]
	public float healthGui;
	private float oldHealthGui = -512;
	[Tooltip("Color of GUIText.")]
	public Color textColor;
	[Tooltip("Offset from total screen width to position GUIText.")]
	public float horizontalOffset = 0.0425f;
	[Tooltip("Offset from total screen height to position GUIText.")]
	public float verticalOffset = 0.075f;
	[Tooltip("Scaled GUIText size, relative to screen size.")]
	public float fontScale = 0.032f;
	[Tooltip("True if negative HP should be shown, otherwise, clamp at zero.")]
	public bool showNegativeHP = true;
	private GUIText guiTextComponent;
	private float oldWidth;//to track screen size change
	
	void Start(){
		guiTextComponent = GetComponent<GUIText>();
		guiTextComponent.material.color = textColor;
		guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
		oldHealthGui = -512;
		oldWidth = Screen.width;
	}
	
	void Update (){
		//only update GUIText if value to be displayed has changed
		if(healthGui != oldHealthGui || oldWidth != Screen.width){
			if(healthGui < 0.0f && !showNegativeHP){
				guiTextComponent.text = "Health : 0";
			}else{
				guiTextComponent.text = "Health : "+ healthGui.ToString();
			}
			guiTextComponent.pixelOffset = new Vector2 (Screen.width * horizontalOffset, Screen.height * verticalOffset);
			guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
			oldWidth = Screen.width;
			oldHealthGui = healthGui;
		}
	}
	
}