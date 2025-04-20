using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualObjectsController : MonoBehaviour
{
    // 현재 활성화 되어 있는 정보들 update 
    private void Awake()
    {

    }

    public void UpdateObjectInfo(int userId)
    {
        foreach(Transform virtualObjectTransform in transform)
        {
            virtualObjectTransform.GetComponent<VirtualObject>().UpdateObjectData(userId);
        }
    }


}
