//WaveText.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class WaveText : MonoBehaviour {
	//draw ammo amount on screen
	[HideInInspector]
	public int waveGui;//bullets remaining in clip
	[HideInInspector]
	public int waveGui2;//total ammo in inventory
	private int oldWave = -512;
	private int oldWave2 = -512;
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
	private GUIText guiTextComponent;
	private float oldWidth;//to track screen size change
	
	void OnEnable(){
		guiTextComponent = GetComponent<GUIText>();
		horizontalOffsetAmt = horizontalOffset;
		verticalOffsetAmt = verticalOffset;	
		guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
		oldWave = -512;
		oldWave2 = -512;
		oldWidth = Screen.width;
	}
	
	void Update(){
		//only update GUIText if value to be displayed has changed
		if(waveGui != oldWave || waveGui2 != oldWave2 || oldWidth != Screen.width) {
			
			guiTextComponent.text = "Wave "+ waveGui.ToString()+" - Remaining : "+ waveGui2.ToString();
			
			guiTextComponent.pixelOffset = new Vector2 (Screen.width * horizontalOffsetAmt, Screen.height * verticalOffsetAmt);
			guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
			oldWidth = Screen.width;
			
			guiTextComponent.material.color = textColor;
			oldWave = waveGui;
			oldWave2 = waveGui2;
	    }
	
	}
}