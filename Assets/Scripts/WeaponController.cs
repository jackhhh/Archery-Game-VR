using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour
{

    [SerializeField]
    private Transform Bow;

    [SerializeField]
    private Transform ArrowLeft;

    [SerializeField]
    private GameObject ArrowRight;

    [SerializeField]
    private Transform ControllerLeft;

    [SerializeField]
    private Transform ControllerRight;

    SteamVR_TrackedObject trackedObj;


    RaycastHit hit; //射线检测

    //private LineRenderer laserAim; //激光

    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void FixedUpdate()
    {

        var device = SteamVR_Controller.Input((int)trackedObj.index);

        //获取两个手柄的距离
        Vector3 PointControllerLeft = ControllerLeft.position;
        Vector3 PointControllerRight = ControllerRight.position;
        float Distance = Vector3.Distance(PointControllerLeft, PointControllerRight);

        if (device.GetTouch(SteamVR_Controller.ButtonMask.Trigger)) //按住了扳机
        {
            //根据两手柄间的距离调整右手震动大小
            var deviceIndex2 = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Rightmost);
            SteamVR_Controller.Input(deviceIndex2).TriggerHapticPulse(System.Convert.ToUInt16(Distance / 0.6f * 1000));

            //根据两个手柄间的距离来对pull这个动画设置规范化时间
            Bow.GetComponent<Animation>()["pull"].normalizedTime = (Distance / 0.6f);
            Bow.GetComponent<Animation>().Play("pull");

        }

        if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger)) //按了扳机
        {
            //把右手的箭隐藏
            ArrowRight.SetActive(false);
        }
        if (device.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger)) //松开了扳机
        {
            Bow.GetComponent<Animation>().Play("release");

            //左手震一下
            var deviceIndex1 = SteamVR_Controller.GetDeviceIndex(SteamVR_Controller.DeviceRelation.Leftmost);
            SteamVR_Controller.Input(deviceIndex1).TriggerHapticPulse(2000);

            //射线检测到僵尸就扣1格血
            if (Physics.Raycast(ArrowLeft.position, ArrowLeft.right, out hit, Mathf.Infinity))
            {
                print(hit.collider.name);
                if (hit.collider.name.Contains("Zombie"))
                    hit.collider.GetComponent<EnemyController>().Hit(1);
                //上面可以根据部位修改不同扣血量
            }

            //当释放动画播放完之后再显示右手的箭
            //while (Bow.GetComponent<Animation>()["release"].normalizedTime <= 1f) ;
            ArrowRight.SetActive(true);
        }
        
    }

    // Use this for initialization
    void Start()
    {
        //Bow.GetComponent<Animation>()["pulled"].wrapMode = WrapMode.Once;
        //laserAim = Bow.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //laserAim.SetPosition(0, ArrowLeft.position);
        //laserAim.SetPosition(1, ArrowLeft.position + ArrowLeft.forward * 200);
    }
}

