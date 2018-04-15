//AmmoText.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class AmmoText : MonoBehaviour {
	//draw ammo amount on screen
	[HideInInspector]
	public int ammoGui;//bullets remaining in clip
	[HideInInspector]
	public int ammoGui2;//total ammo in inventory
	[HideInInspector]
	public bool showMags = true;
	private int oldAmmo = -512;
	private int oldAmmo2 = -512;
	[Tooltip("Color of GUIText.")]
	public Color textColor;
	[Tooltip("Offset from total screen width to position GUIText.")]
	public float horizontalOffset = 0.95f;
	[Tooltip("Offset from total screen height to position GUIText.")]
	public float verticalOffset = 0.075f;
	[HideInInspector]
	public float horizontalOffsetAmt = 0.78f;
	[HideInInspector]
	public float verticalOffsetAmt = 0.1f;
	[Tooltip("Scaled GUIText size, relative to screen size.")]
	public float fontScale = 0.032f;
	[HideInInspector]
	public GUIText guiTextComponent;
	private float oldWidth;//to track screen size change
	
	void OnEnable(){
		horizontalOffsetAmt = horizontalOffset;
		verticalOffsetAmt = verticalOffset;	
		guiTextComponent = GetComponent<GUIText>();
		guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
		oldAmmo = -512;
		oldAmmo2 = -512;
		oldWidth = Screen.width;
	}
	
	void Update(){
		//only update GUIText if value to be displayed has changed
		if(ammoGui != oldAmmo || ammoGui2 != oldAmmo2 || oldWidth != Screen.width) {

			if(showMags){
				guiTextComponent.text = "Ammo : "+ ammoGui.ToString()+" / "+ ammoGui2.ToString();
			}else{
				guiTextComponent.text = "Ammo : "+ ammoGui2.ToString();
			}
			
			guiTextComponent.material.color = textColor;
		    oldAmmo = ammoGui;
			oldAmmo2 = ammoGui2;
			
			guiTextComponent.pixelOffset = new Vector2 (Screen.width * horizontalOffsetAmt, Screen.height * verticalOffsetAmt);
			guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
			oldWidth = Screen.width;
			
	    }
	
	}
}