using UnityEngine;
using System.Collections;

public class SpawnRight : MonoBehaviour {

    SteamVR_TrackedObject trackedObj;
    public GameObject[] SpawnList;

    //public Message Msg;

    public GameObject Msg;

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
	}

    // Update is called once per frame
    void FixedUpdate() {

        var device = SteamVR_Controller.Input((int)trackedObj.index);
 
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Axis0))
        {
            Thei.i--;
            Debug.Log("回退切换，现在是位置——"+ Thei.i);
            if (Thei.i < 0 )
            {
                Debug.Log("SpawnList.Length是——" + SpawnList.Length);
                SpawnList[SpawnList.Length - 1].SetActive(true);
                SetChildrenActive(SpawnList[SpawnList.Length - 1], true);
                SpawnList[0].SetActive(false);
                SetChildrenActive(SpawnList[0], false);
                Thei.i = SpawnList.Length - 1;
            }
            else
            {
                Debug.Log("你刚刚切换了");
                SpawnList[Thei.i].SetActive(true);
                SetChildrenActive(SpawnList[Thei.i], true);
                SpawnList[Thei.i + 1].SetActive(false);
                SetChildrenActive(SpawnList[Thei.i + 1], false);
            }
            //发送消息提示
            //Msg.Show(Message.MsgType.SpawnRight);
            Msg.GetComponent<Message>().Show(Message.MsgType.SpawnRight);
        }
    }
}
