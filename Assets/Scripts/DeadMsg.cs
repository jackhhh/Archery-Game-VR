using UnityEngine;
using System.Collections;

public class DeadMsg : MonoBehaviour
{

    public Message Msg;

    void OnTriggerEnter(Collider collider)
    {
        //如果碰撞体所在物体名字包含Zombie
        if (collider.gameObject.name.Contains("Zombie"))
        {
            //发送消息提示 
            Msg.Show(Message.MsgType.Lose);
        }

    }
}
