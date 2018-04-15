using UnityEngine;
using System.Collections;

public class EnemySpawnController : MonoBehaviour {
	[SerializeField]
	private GameObject zombie;//僵尸
	[SerializeField]
	private float interval = 5f;//生成间隔 未设定为5秒
	private float timeCount = 0;//计时

	// Use this for initialization
	void Start () {
		timeCount = 2f;
	}
	
	// Update is called once per frame
	void Update () {
		//如果倒计时结束则生成一个僵尸并且倒计时重置
		if (timeCount > 0) {
			timeCount -= Time.deltaTime;
		} 
		else {
			Instantiate (zombie, this.transform.position, this.transform.rotation);
			timeCount = interval;
		}
	}
}
