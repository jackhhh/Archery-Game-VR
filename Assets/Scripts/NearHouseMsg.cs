using UnityEngine;
using System.Collections;

public class NearHouseMsg : MonoBehaviour {

    public Message Msg;

    void OnTriggerEnter(Collider collider)
    {
        //如果碰撞体所在物体名字包含Zombie
        Debug.Log("void OnTriggerstay(Collider collider)"+ collider.gameObject.name);
        if (collider.gameObject.tag=="Zombie")
        //if (collider.gameObject.name.Contains("Zombie"))
        {
            Debug.Log(" if (collider.gameObject.name.Contains(Zombie))");
            //发送消息提示 
            Msg.Show(Message.MsgType.Near);
        }
        
    }
}
