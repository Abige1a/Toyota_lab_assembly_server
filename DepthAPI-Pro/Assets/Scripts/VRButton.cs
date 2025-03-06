using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VRButton : MonoBehaviour
{
    public UnityEvent events;
    public UnityEvent hanging_events;
    public UnityEvent not_hanging_events;

    private float minInvokeDelay = 1.0f;
    private bool canInvoke = true;           // 是否允许触发事件
    private bool triggeredThisFrame = false; // 本帧内是否已触发
    private float lastInvokeTime = -9999f;   // 上次成功触发的时间点


    void Start()
    {

    }

    void Update()
    {

    }
    private void LateUpdate()
    {
        
        triggeredThisFrame = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerHand"))
        {
            TryInvoke(events);
        }
    }

    public void InvokeEvents()
    {
        TryInvoke(events);
    }

    private IEnumerator ResetInvokeFlag()
    {
        canInvoke = false;
        // wait for seconds to invoke
        yield return new WaitForSeconds(minInvokeDelay);
        canInvoke = true;
    }

    private void TryInvoke(UnityEvent unityEvent)
    {
        // 1) 检查本帧是否已经触发过
        if (triggeredThisFrame)
        {
            return;
        }

        // 2) 检查冷却时间
        if (!canInvoke || Time.time - lastInvokeTime < minInvokeDelay)
        {
            return;
        }

        // 满足条件才触发
        unityEvent.Invoke();
        lastInvokeTime = Time.time;
        canInvoke = false;
        triggeredThisFrame = true;
        StartCoroutine(ResetInvokeFlag());
    }

    public void Invoke()
    {
        events.Invoke();
    }

    public void HangingInvoke() 
    {
        hanging_events.Invoke();
    }

    public void NotHangingInvoke()
    {
        not_hanging_events.Invoke();
    }

}
