//HealthText.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class HungerText : MonoBehaviour {
	//draw hunger amount on screen
	[HideInInspector]
	public float hungerGui;
	private float oldHungerGui = -512;
	[Tooltip("Color of GUIText.")]
	public Color textColor;
	[Tooltip("Offset from total screen width to position GUIText.")]
	public float horizontalOffset = 0.0425f;
	[Tooltip("Offset from total screen height to position GUIText.")]
	public float verticalOffset = 0.075f;
	[Tooltip("Scaled GUIText size, relative to screen size.")]
	public float fontScale = 0.032f;
	private GUIText guiTextComponent;
	private float oldWidth;//to track screen size change
	
	void Start(){
		guiTextComponent = GetComponent<GUIText>();
		guiTextComponent.material.color = textColor;
		guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
		oldHungerGui = -512;
		oldWidth = Screen.width;
	}
	
	void Update (){
		//only update GUIText if value to be displayed has changed
		if(hungerGui != oldHungerGui || oldWidth != Screen.width){
			guiTextComponent.text = "Hunger : "+ hungerGui.ToString();
			guiTextComponent.pixelOffset = new Vector2 (Screen.width * horizontalOffset, Screen.height * verticalOffset);
			guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
			oldWidth = Screen.width;
			oldHungerGui = hungerGui;
		}
	}
	
}