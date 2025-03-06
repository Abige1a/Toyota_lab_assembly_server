using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowController : MonoBehaviour
{
    public Transform handtransform;
    private bool canGrab = false; // 是否可以抓取面板

    void Update()
    {
        // 检测右手触发器是否按下
       
        if (canGrab)
        {
            FollowHand(); // 如果可以抓取，跟随手柄
        }
        if (canGrab && OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
        {
            moveButtonClicked();
        }
        
        
    }

    // 按钮点击事件
    public void moveButtonClicked()
    {
        canGrab = !canGrab; // 切换抓取状态
    }

    // 跟随手柄
    private void FollowHand()
    {
        // 获取右手手柄的位置
        Vector3 handForward = handtransform.rotation * Vector3.forward;
        transform.position = handtransform.position+handForward; // 更新面板位置
        transform.rotation = handtransform.rotation;
    }
}
