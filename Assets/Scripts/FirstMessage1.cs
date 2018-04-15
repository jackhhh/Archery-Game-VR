using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FirstMessage1 : MonoBehaviour {

    public Text MessageBox;
    /*
    public enum MsgType
    {
        Near = 0,
        Lose = 1,
        SpawnLeft = 2,
        SpawnRight = 3
    }*/

    // Use this for initialization
    void Start () {
        MessageBox.text = string.Format("游戏开始\n如果你是新手\n请先看游戏说明");
    }
}
