//MainMenu.cs by Azuline Studios© All Rights Reserved
//displays main menu gui and manages button states
using UnityEngine;
using System.Collections;

public class MainMenu : MonoBehaviour {
	private FPSPlayer FPSPlayerComponent;
//	private InputControl InputComponent;
	
	[Tooltip("True if the menu size should scale with screen size.")]
	public bool scaleWithScreenSize;
	[Tooltip("The NPC parent objects to enable/disable for game modes in the order of: single player, faction war, wave survival.")]
	public GameObject[] NpcGroups;
	public Font font;
	public Color GuiTint;
	[Tooltip("Color of normal button text.")]
	public Color btnTextColor;
	[Tooltip("Color of text for buttons with active states.")]
	public Color btnActiveTextColor;
	
	private bool initSyles;
	public float fontSizeScaled = 0.025f;
	public float titleFontSizeScaled;
	public int fontSize = 14;
	public int titleFontSize = 14;

	public float menuPosV = 0.3f;
	public float menuPosH = 0.5f;
	public float buttonHeight;
	public float buttonWidth;
	public float buttonHeightScaled = 0.075f;
	public float buttonWidthScaled = 0.31f;
	private float buttonHeightAmt;
	private float buttonWidthAmt;
	public float buttonSpacing = 10f;
	private float buttonV;
	private float buttonH;
	private bool menuDisplayed;
	private bool resumePress;
	
	private bool dzState;
	private bool dzButtonState;
	private bool invulButtonState;
	private bool giveAllButtonState;
	private AudioSource aSource;
	public AudioClip buttonClickFx;
	public float buttonFxVol = 1.0f;
	public AudioClip invulClickFx;
	public float invulFxVol = 1.0f;
	public AudioClip giveAllClickFx;
	public float giveAllFxVol = 1.0f;
	public AudioClip beepClickFx;
	public float beepFxVol = 1.0f;
	
	private GUIStyleState buttonActive;
	private GUIStyleState buttonInActive;
	private GUIStyle toggleButtonStyle1;
	private GUIStyle toggleButtonStyle2;
	private GUIStyle toggleButtonStyle3;
	private GUIStyle mainButtonSyle;
	private GUIStyle mapButtonStyle;
	private bool npcGoupsNull;
	private GUIStyle titleStyle;
	private bool hungerThirstActive;
	
	void Start () {
		//set up menu components and get object references
		FPSPlayerComponent =  Camera.main.GetComponent<CameraControl>().playerObj.GetComponent<FPSPlayer>();
//		InputComponent = FPSPlayerComponent.InputComponent;
		
		aSource = gameObject.AddComponent<AudioSource>(); 
		aSource.spatialBlend = 0.0f;
		aSource.volume = 1.0f;
		aSource.playOnAwake = false;
		
		hungerThirstActive = FPSPlayerComponent.usePlayerThirst;
		
		if(NpcGroups.Length > 0 && NpcGroups[0] != null && NpcGroups[1] != null && NpcGroups[2] != null){
			foreach (GameObject obj in NpcGroups){
				obj.SetActive(false);
			}
			//get the number of the NpcGroup from Game Type player pref and make it active on scene start
			NpcGroups[(PlayerPrefs.GetInt("Game Type"))].SetActive(true);
		}else{
			npcGoupsNull = true;
		}
		
		this.enabled = false;
		
	}
	
	void Update() {
		//resume game from button press in Update() loop because resuming from OnGUI() causes a lag in smoothing in other scripts
		if(resumePress){
			Time.timeScale = FPSPlayerComponent.menuTime;
			resumePress = false;
			FPSPlayerComponent.paused = false;
			FPSPlayerComponent.menuDisplayed = false;
			this.enabled = false;//deactivate this script to avoid running OnGUI when not needed
		}
	}
	
	void OnGUI() {
	
		GUI.color = GuiTint;
			
		if(!initSyles){//initialize menu styles
			toggleButtonStyle1 = new GUIStyle(GUI.skin.button);
			toggleButtonStyle2 = new GUIStyle(GUI.skin.button);
			toggleButtonStyle3 = new GUIStyle(GUI.skin.button);
			titleStyle = new GUIStyle(GUI.skin.box);
			mainButtonSyle = new GUIStyle(GUI.skin.button);
			
			mapButtonStyle = new GUIStyle(GUI.skin.button);
			
			if(!npcGoupsNull){
				mapButtonStyle.normal.textColor = btnTextColor;
				mapButtonStyle.active.textColor = btnTextColor;
				mapButtonStyle.hover.textColor = Color.white;
			}else{
				mapButtonStyle.normal.textColor = Color.gray;
				mapButtonStyle.active.textColor = Color.gray;
				mapButtonStyle.hover.textColor = Color.gray;
			}

			GUI.skin.font = font;
			
			//initialize toggle button colors based on player states
			if(FPSPlayerComponent.invulnerable){
				toggleButtonStyle1.normal.textColor = btnActiveTextColor;
				toggleButtonStyle1.active.textColor = btnActiveTextColor;
				toggleButtonStyle1.hover.textColor = btnActiveTextColor;
			}else{
				toggleButtonStyle1.normal.textColor = Color.gray;
				toggleButtonStyle1.active.textColor = Color.gray;
				toggleButtonStyle1.hover.textColor = Color.gray;
			}
			
			if(FPSPlayerComponent.WeaponPivotComponent.deadzoneZooming){
				toggleButtonStyle3.normal.textColor = btnActiveTextColor;
				toggleButtonStyle3.active.textColor = btnActiveTextColor;
				toggleButtonStyle3.hover.textColor = btnActiveTextColor;
			}else{
				toggleButtonStyle3.normal.textColor = Color.gray;
				toggleButtonStyle3.active.textColor = Color.gray;
				toggleButtonStyle3.hover.textColor = Color.gray;
			}
			
			if(hungerThirstActive){
				toggleButtonStyle2.normal.textColor = btnActiveTextColor;
				toggleButtonStyle2.active.textColor = btnActiveTextColor;
				toggleButtonStyle2.hover.textColor = btnActiveTextColor;
			}else{
				toggleButtonStyle2.normal.textColor = Color.gray;
				toggleButtonStyle2.active.textColor = Color.gray;
				toggleButtonStyle2.hover.textColor = Color.gray;
			}
			
			initSyles = true;
		}
		
		mainButtonSyle.normal.textColor = btnTextColor;
		mainButtonSyle.active.textColor = btnTextColor;
		mainButtonSyle.hover.textColor = Color.white;
		
		if(scaleWithScreenSize){//scale gui objects if scaleWithScreenSize is true
			buttonHeightAmt = Screen.height * buttonHeightScaled;
			buttonWidthAmt = Screen.width * buttonWidthScaled;
			int scaledFontSize = Mathf.RoundToInt(Screen.width * fontSizeScaled);
			mainButtonSyle.fontSize = scaledFontSize;
			mapButtonStyle.fontSize = scaledFontSize;
			toggleButtonStyle1.fontSize = scaledFontSize;
			toggleButtonStyle2.fontSize = scaledFontSize;
			toggleButtonStyle3.fontSize = scaledFontSize;
			titleStyle.fontSize = Mathf.RoundToInt(Screen.width * titleFontSizeScaled);
			titleStyle.normal.textColor = Color.white;
		}else{
			buttonHeightAmt = buttonHeight;
			buttonWidthAmt = buttonWidth;
			mainButtonSyle.fontSize = fontSize;
			mapButtonStyle.fontSize = fontSize;
			toggleButtonStyle1.fontSize = fontSize;
			toggleButtonStyle2.fontSize = fontSize;
			toggleButtonStyle3.fontSize = fontSize;
			titleStyle.fontSize = titleFontSize;
			titleStyle.normal.textColor = Color.white;
		}
		
		//find button vertical and horizontal positions
		buttonH = (Screen.width * menuPosH) - buttonWidthAmt - (buttonSpacing * 0.5f);
		buttonV = Screen.height * menuPosV;
		
		//menu title and background
		GUI.Box(new Rect(buttonH - buttonSpacing, buttonV - buttonHeightAmt - buttonSpacing, (buttonWidthAmt * 2.0f) + buttonSpacing * 3.0f, (buttonHeightAmt + buttonSpacing) * 6.0f), "");
		GUI.Box(new Rect(buttonH - buttonSpacing, buttonV - buttonHeightAmt - buttonSpacing, (buttonWidthAmt * 2.0f) + buttonSpacing * 3.0f, buttonHeightAmt), "Realistic FPS Prefab - Main Menu", titleStyle);

//			GUI.Button(new Rect(Screen.width * 0.5f - ((buttonWidthAmt * 1.5f) * 0.5f), 0.0f + buttonSpacing, buttonWidthAmt * 1.5f, buttonHeightAmt * 0.5f), 
//					  " Level : " + level.ToString() + " XP : " + kills.ToString() + " / " + nextLevelKills.ToString());
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Left Buttons
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		if (GUI.Button(new Rect(buttonH, buttonV, buttonWidthAmt, buttonHeightAmt), "Resume Game", mainButtonSyle)){		
			resumePress = true;
			PlayButtonFx(buttonClickFx, buttonFxVol);
		}
		
		if (GUI.Button(new Rect(buttonH, buttonV + buttonHeightAmt + buttonSpacing, buttonWidthAmt, buttonHeightAmt), "Restart Map", mainButtonSyle)){		
			FPSPlayerComponent.RestartMap();
			PlayButtonFx(buttonClickFx, buttonFxVol);
			this.enabled = false;
		}

		if (GUI.Button(new Rect(buttonH, buttonV + ((buttonHeightAmt + buttonSpacing) * 2), buttonWidthAmt, buttonHeightAmt), "Story Mode", mapButtonStyle)){
			if(!npcGoupsNull){	
				PlayerPrefs.SetInt("Game Type", 0);
				FPSPlayerComponent.RestartMap();
				PlayButtonFx(buttonClickFx, buttonFxVol);
				this.enabled = false;
			}else{
				PlayButtonFx(beepClickFx, beepFxVol);
			}
		}
		
		if (GUI.Button(new Rect(buttonH, buttonV + ((buttonHeightAmt + buttonSpacing) * 3), buttonWidthAmt, buttonHeightAmt), "Faction War", mapButtonStyle)){		
			if(!npcGoupsNull){	
				PlayerPrefs.SetInt("Game Type", 1);
				FPSPlayerComponent.RestartMap();
				PlayButtonFx(buttonClickFx, buttonFxVol);
				this.enabled = false;
			}else{
				PlayButtonFx(beepClickFx, beepFxVol);
			}
		}
		
		if (GUI.Button(new Rect(buttonH, buttonV + ((buttonHeightAmt + buttonSpacing) * 4), buttonWidthAmt, buttonHeightAmt), "Wave Survival", mapButtonStyle)){		
			if(!npcGoupsNull){	
				PlayerPrefs.SetInt("Game Type", 2);
				FPSPlayerComponent.RestartMap();
				PlayButtonFx(buttonClickFx, buttonFxVol);
				this.enabled = false;
			}else{
				PlayButtonFx(beepClickFx, beepFxVol);
			}
		}
		
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		//Right Buttons
		/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		if (GUI.Button(new Rect(buttonH + buttonWidthAmt + buttonSpacing, buttonV, buttonWidthAmt, buttonHeightAmt), "Give All Weapons", mainButtonSyle)){		
			FPSPlayerComponent.PlayerWeaponsComponent.GiveAllWeaponsAndAmmo();
			PlayButtonFx(giveAllClickFx, giveAllFxVol);
		}

		if (GUI.Button(new Rect(buttonH + buttonWidthAmt + buttonSpacing, buttonV + buttonHeightAmt + buttonSpacing, buttonWidthAmt, buttonHeightAmt), "Invulnerable", toggleButtonStyle1)){		
			if(!FPSPlayerComponent.invulnerable){
				FPSPlayerComponent.invulnerable = true;
				PlayButtonFx(invulClickFx, invulFxVol);
				toggleButtonStyle1.normal.textColor = btnActiveTextColor;
				toggleButtonStyle1.active.textColor = btnActiveTextColor;
				toggleButtonStyle1.hover.textColor = btnActiveTextColor;
			}else{
				PlayButtonFx(beepClickFx, beepFxVol);
				FPSPlayerComponent.invulnerable = false;
				toggleButtonStyle1.normal.textColor = Color.gray;
				toggleButtonStyle1.active.textColor = Color.gray;
				toggleButtonStyle1.hover.textColor = Color.gray;
			}
		}

		if (GUI.Button(new Rect(buttonH + buttonWidthAmt + buttonSpacing, buttonV + ((buttonHeightAmt + buttonSpacing) * 2), buttonWidthAmt, buttonHeightAmt), "Hunger and Thirst", toggleButtonStyle2)){		
			if(hungerThirstActive){
				hungerThirstActive = false;
				FPSPlayerComponent.usePlayerHunger = false;
				FPSPlayerComponent.usePlayerThirst = false;
				PlayButtonFx(beepClickFx, beepFxVol);
				toggleButtonStyle2.normal.textColor = Color.gray;
				toggleButtonStyle2.active.textColor = Color.gray;
				toggleButtonStyle2.hover.textColor = Color.gray;
			}else{
				PlayButtonFx(buttonClickFx, buttonFxVol);
				FPSPlayerComponent.usePlayerHunger = true;
				FPSPlayerComponent.usePlayerThirst = true;
				FPSPlayerComponent.UpdateHunger(-FPSPlayerComponent.maxHungerPoints);
				FPSPlayerComponent.UpdateThirst(-FPSPlayerComponent.maxThirstPoints);
				hungerThirstActive = true;
				toggleButtonStyle2.normal.textColor = btnActiveTextColor;
				toggleButtonStyle2.active.textColor = btnActiveTextColor;
				toggleButtonStyle2.hover.textColor = btnActiveTextColor;
			}
		}

		if (GUI.Button(new Rect(buttonH + buttonWidthAmt + buttonSpacing, buttonV + ((buttonHeightAmt + buttonSpacing) * 3), buttonWidthAmt, buttonHeightAmt), "Free Aim Zooming", toggleButtonStyle3)){		
			FPSPlayerComponent.WeaponPivotComponent.ToggleDeadzoneZooming();
			if(FPSPlayerComponent.WeaponPivotComponent.deadzoneZooming){
				PlayButtonFx(buttonClickFx, buttonFxVol);
				toggleButtonStyle3.normal.textColor = btnActiveTextColor;
				toggleButtonStyle3.active.textColor = btnActiveTextColor;
				toggleButtonStyle3.hover.textColor = btnActiveTextColor;
			}else{
				PlayButtonFx(beepClickFx, beepFxVol);
				toggleButtonStyle3.normal.textColor = Color.gray;
				toggleButtonStyle3.active.textColor = Color.gray;
				toggleButtonStyle3.hover.textColor = Color.gray;
			}
			aSource.Play();
		}


		if (GUI.Button(new Rect(buttonH + buttonWidthAmt + buttonSpacing, buttonV + ((buttonHeightAmt + buttonSpacing) * 4), buttonWidthAmt, buttonHeightAmt), "Exit Game", mainButtonSyle)){		
			PlayerPrefs.SetInt("Game Type", 0);
			PlayButtonFx(buttonClickFx, buttonFxVol);
			Application.Quit();
			#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
			#endif
		
		}
		
	}
	
	//play button click sound effects
	private void PlayButtonFx(AudioClip clip, float vol){
		aSource.volume = vol;
		aSource.clip = clip;
		aSource.Play();
	}
}
