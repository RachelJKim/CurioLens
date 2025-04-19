using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    ConvectionFlow, Diffusion, Resistance
}

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;
    private Dictionary<EffectType, GameObject> effectPrefabDict;
    private List<GameObject> effectList; // todo: scene 바꿀 때 한 번에 제거하기 

    private void Awake()
    {
        Instance = this;

        effectPrefabDict = new Dictionary<EffectType, GameObject>();

        foreach (EffectType type in System.Enum.GetValues(typeof(EffectType)))
        {
            string path = $"Prefabs/Effects/{type}";

            GameObject prefab = Resources.Load<GameObject>(path);

            if (prefab != null)
            {
                effectPrefabDict[type] = prefab;
            }
        }
    }

    public void CreateEffect(EffectType effectType, Transform objectTransform)
    {
        Vector3 effectPosition = objectTransform.position + Vector3.right * 1f;

        GameObject effectObject = GameObject.Instantiate(effectPrefabDict[effectType], objectTransform);
        effectObject.transform.position = effectPosition;
        effectList.Add(effectObject);
    }
}
