using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyLight : MonoBehaviour
{
    public float lifeTime = 1.1f;
    void Start() => Destroy(gameObject, lifeTime);
}
