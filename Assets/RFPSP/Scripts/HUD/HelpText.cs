//HelpText.cs by Azuline Studios© All Rights Reserved
using UnityEngine;
using System.Collections;

public class HelpText : MonoBehaviour {
	//draw ammo amount on screen
	[Tooltip("Color of GUIText.")]
	public Color textColor;
	[Tooltip("Offset from total screen width to position GUIText.")]
	public float horizontalOffset = 0.0425f;
	[Tooltip("Offset from total screen height to position GUIText.")]
	public float verticalOffset = 0.075f;
	[Tooltip("Scaled GUIText size, relative to screen size.")]
	public float fontScale = 0.029f;
	private bool helpTextState = true;
	private bool helpTextEnabled = false;
	private float startTime = 0.0f;
	private bool initialHide = true;
	private bool moveState = true;
	private bool F1pressed = false;
	private bool fadeState = false;
	private float moveTime = 0.0f;
	private float fadeTime = 5.0f;
	[HideInInspector]
	public GameObject playerObj;
	private GUIText guiTextComponent;
	private float oldWidth;//to track screen size change
	
	void Start(){
		guiTextComponent = GetComponent<GUIText>();
		playerObj = Camera.main.transform.GetComponent<CameraControl>().playerObj;
		guiTextComponent.pixelOffset = new Vector2 (Screen.width * horizontalOffset, Screen.height * verticalOffset);
		guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
		guiTextComponent.text = "Press F1 for controls";
		guiTextComponent.material.color = textColor;
		this.GetComponent<GUIText>().enabled = true;
		startTime = Time.time;
		oldWidth = Screen.width;
	}
	
	void Update (){
		//Initialize script references
		FPSRigidBodyWalker FPSWalkerComponent = playerObj.GetComponent<FPSRigidBodyWalker>();
		InputControl InputComponent = playerObj.GetComponent<InputControl>();
		float horizontal = FPSWalkerComponent.inputX;//Get input from player movement script
		float vertical = FPSWalkerComponent.inputY;
		
		Color tempColor = GetComponent<GUIText>().material.color; 
		
		if(oldWidth != Screen.width){
			guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
			guiTextComponent.pixelOffset = new Vector2 (Screen.width * horizontalOffset, Screen.height * verticalOffset);
			oldWidth = Screen.width;
		}
		
		if(moveState &&(Mathf.Abs(horizontal) > 0.75f || Mathf.Abs(vertical) > 0.75f)){
			moveState = false;	
			if(startTime + fadeTime < Time.time){
				moveTime = Time.time;//fade F1 message if moved after fadeTime
			}else{
				moveTime = startTime + fadeTime;//if moved before fade started, start fade at fadeTime	
			}
		}
		
		//if F1 is pressed before fade, bypass fading of F1 message and show help text
		if(InputComponent.helpPress && (moveState || moveTime > Time.time)){
			moveState = false;	
			F1pressed = true;
			moveTime = Time.time;
		}
		
		if(!fadeState && !F1pressed){
			if(!moveState && (startTime + fadeTime < Time.time)){
				if(moveTime + 1.0f > Time.time){
					tempColor.a -= Time.deltaTime;//fade out color alpha element for one second
					guiTextComponent.material.color = tempColor;
				}else{
					fadeState = true;//F1 message has faded, allow controls to be shown with F1 press
				}
			}
		}else{
			
			if(initialHide){
				guiTextComponent.text = "Mouse 1 : fire weapon\nMouse 2 : raise sights or block\nMouse 3 or C : toggle camera mode\nW : forward\nS : backward\nA : strafe left\nD : strafe right\nLeft Shift : sprint\nLeft Ctrl : crouch\nX : prone\nQ : lean left\nE : lean right\nSpace : jump\nF : use item, move item, move NPC\nR : reload\nB : fire mode\nH : holster weapon\nBackspace : drop weapon\nG : throw grenade\nBackslash : select grenade\nV : melee attack\nT : enter bullet time\nZ : toggle deadzone aiming\nEsc or F2: Main Menu\nTab : Pause\n";
				guiTextComponent.enabled = false;
				tempColor.a = 1.0f;//reset alpha to opaque
				guiTextComponent.material.color = tempColor;
				initialHide = false;//do these actions only once after F1 help notice has faded out
			}
			
			if(InputComponent.helpPress){
				if(helpTextState){
					if(!helpTextEnabled){
						guiTextComponent.enabled = true;
						helpTextEnabled = true;
					}else{
						guiTextComponent.enabled = false;
						helpTextEnabled = false;
					}
					guiTextComponent.fontSize = Mathf.RoundToInt(Screen.height * fontScale);
					guiTextComponent.pixelOffset = new Vector2 (Screen.width * horizontalOffset, Screen.height * verticalOffset);
					helpTextState = false;
				}
			}else{
				helpTextState = true;		
			}
		}
		
	}
	
}