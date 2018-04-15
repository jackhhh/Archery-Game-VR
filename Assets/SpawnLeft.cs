using UnityEngine;
using System.Collections;



public class SpawnLeft : MonoBehaviour {

    SteamVR_TrackedObject trackedObj;
    public GameObject[] SpawnList;

    public GameObject Msg;

    //public Component MessageScript;


    private void SetChildrenActive(GameObject obj, bool active)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            GameObject childObj = obj.transform.GetChild(i).gameObject;
            childObj.SetActive(active);
            SetChildrenActive(childObj, active);
        }
    }

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }
    // Use this for initialization
    void Start () {
        //MessageScript = Msg.GetComponent<Message>();
    }

	// Update is called once per frame
	void FixedUpdate() {
        var device = SteamVR_Controller.Input((int)trackedObj.index);

        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Axis0))
        //if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            Thei.i++;
            Debug.Log("前进切换，现在是位置——" + Thei.i);
            if (Thei.i > SpawnList.Length-1)
            {
                SpawnList[0].SetActive(true);
                SetChildrenActive(SpawnList[0], true);
                SpawnList[Thei.i - 1].SetActive(false);
                SetChildrenActive(SpawnList[Thei.i - 1], false);
                Thei.i = 0;
            }
            else
            {
                SpawnList[Thei.i].SetActive(true);
                SetChildrenActive(SpawnList[Thei.i], true);
                SpawnList[Thei.i - 1].SetActive(false);
                SetChildrenActive(SpawnList[Thei.i - 1], false);
            }
            //发送消息提示
            Debug.Log("马上就应该显示了");
            Msg.GetComponent<Message>().Show(Message.MsgType.SpawnLeft);
            //MessageScript.Show(Message.MsgType.SpawnLeft);
            Debug.Log("显示完毕");
           
        }
    }
}
