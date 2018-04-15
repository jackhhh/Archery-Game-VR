using UnityEngine;
using System.Collections;
using UltraReal.Utilities;

public class CurserControl : UltraRealMonobehaviorBase {

    [SerializeField]
    CursorLockMode _lockMode = CursorLockMode.None;

    [SerializeField]
    bool _cursorVisible = true;

	// Use this for initialization
	protected override void OnUpdate()
    {
 	    base.OnUpdate();

        Cursor.lockState = _lockMode;
        Cursor.visible = _cursorVisible;
	}
}
