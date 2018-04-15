using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Message : MonoBehaviour {

    public Text MessageBox;

    public enum MsgType
    {
        Near = 0,
        Lose = 1,
        SpawnLeft = 2,
        SpawnRight = 3,
        Level0_1 = 41,
        Level1_2 = 42,
        Level2_3 = 43,
        Level3_  = 44,
        Level99_ = 99
    }

    [SerializeField]
    private float interval = 5f;//消息显示时间 未设定为5秒
    private float timeCount = 5;//计时器
    private float ENDTime = -1;//最后时刻

    // Use this for initialization
    void Start () {
        //MessageBox.text = string.Format("游戏开始，骚年保重~");
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        //如果倒计时结束则清空显示
        if (timeCount > 0)
        {
            timeCount -= Time.deltaTime;
        }
        else
        {
            MessageBox.text = string.Format("");
            timeCount = interval;
        }

        if(ENDTime>0)
        {
            Debug.Log("正在减，别急");
            ENDTime -= Time.deltaTime;
        }
        if(ENDTime > 0 && ENDTime < 1)
        {
            Debug.Log("这时候应该关了吧");
            Application.Quit();
        }
    }

    public void Show(MsgType _MsgType)
    {
        Debug.Log("已经调用show函数了");
        //让每个消息显示interval秒
        timeCount = interval;

        switch (_MsgType)
        {
            case MsgType.Near:
                {
                    MessageBox.text = string.Format("僵尸到你家门口了！");
                }
                break;
            case MsgType.Lose:
                {
                    MessageBox.text = string.Format("僵尸吃掉了你的脑子！\n\n下次再来吧");
                    ENDTime = 5;
                }
                break;
            case MsgType.SpawnLeft:
                {
                    MessageBox.text = string.Format("你切换到了后一个攻击点！");
                }
                break;
            case MsgType.SpawnRight:
                {
                    MessageBox.text = string.Format("你切换到了前一个攻击点！");
                }
                break;
            case MsgType.Level0_1://恭喜你通过新手关卡，下面进入第一关
                {
                    MessageBox.text = string.Format("恭喜你通过新手关卡\n下面进入第一关");
                }
                break;
            case MsgType.Level1_2:
                {
                    MessageBox.text = string.Format("恭喜你通过第一关\n下面进入第二关");
                }
                break;
            case MsgType.Level2_3:
                {
                    MessageBox.text = string.Format("恭喜你通过第二关\n下面进入第三关");
                }
                break;
            case MsgType.Level3_:
                {
                     MessageBox.text = string.Format("恭喜你通过第二关\n下面进入第四关");
                }
                break;
            case MsgType.Level99_:
                {
                    MessageBox.text = string.Format("恭喜你通过所有关卡\n~~独乐乐不如众乐乐~~\n把这个游戏分享出去吧");
                    timeCount = 10;
                    ENDTime = 10;
                }
                break;
        }
    }
}
