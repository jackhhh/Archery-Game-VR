using UnityEngine;
using System.Collections;

public class ZombieCount : MonoBehaviour
{
    public static int level=0;
    public GameObject[] zombie_level;
    public static int[] zombieCount1= { 5, 10, 20, 30 };//5  10
    public GameObject[] Msg;
    // Use this for initialization
    void Start ()
    {
        //gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
    {
        //如果当前关卡僵尸数数小于等于0
        if (zombieCount1[level] <= 0)
        {
            //如果关卡还没打完
            if(level <= zombie_level.Length - 1)
            {
                Debug.Log("OK");
                zombie_level[level].SetActive(false);
                level++;
                if(level == 1)
                {
                    for (int i = 0; i < Msg.Length; i++)
                    {
                        Msg[i].GetComponent<Message>().Show(Message.MsgType.Level0_1);
                    }
                }
                else if(level == 2)
                {
                    for (int i = 0; i < Msg.Length; i++)
                    {
                        Msg[i].GetComponent<Message>().Show(Message.MsgType.Level1_2);
                    }
                }
                else
                {
                    for (int i = 0; i < Msg.Length; i++)
                    {
                        Msg[i].GetComponent<Message>().Show(Message.MsgType.Level99_);
                    }
                    //Application.Quit();
                }
                /*else if(level == 3)
                {
                    for (int i = 0; i < Msg.Length; i++)
                    {
                        Msg[i].GetComponent<Message>().Show(Message.MsgType.Level2_3);
                    }
                }*/
                if (level <= zombie_level.Length - 1)
                    zombie_level[level].SetActive(true);
            }
            else
            {
                for(int i = 0; i < Msg.Length; i++)
                {
                    Msg[i].GetComponent<Message>().Show(Message.MsgType.Level3_);
                }
            }
            
        }
	}
}
