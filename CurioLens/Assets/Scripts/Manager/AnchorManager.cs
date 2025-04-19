using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;

public class AnchorManager : MonoBehaviour
{
    [Header("Placeable Prefabs")]
    public List<GameObject> placeablePrefabs;

    [Header("Placement Point (e.g., Index Tip)")]
    public Transform placementPoint;

    private int currentPrefabIndex = 0;

    [Serializable]
    public class AnchorSaveData
    {
        public string uuid;
        public int prefabIndex;
        public Vector3 position;
        public Quaternion rotation;
    }

    [Serializable]
    public class AnchorSaveWrapper
    {
        public List<AnchorSaveData> anchors;
    }

    public List<AnchorSaveData> placedAnchors = new List<AnchorSaveData>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N)) // Place prefab at index tip
        {
            Debug.Log("[HERE] NNNNNNNNNNNNN");
            PlaceCurrentPrefab();
        }

        if (Input.GetKeyDown(KeyCode.Tab)) // Switch prefab
        {
            currentPrefabIndex = (currentPrefabIndex + 1) % placeablePrefabs.Count;
            Debug.Log($"Switched to prefab index: {currentPrefabIndex}");
        }
    }

    public void PlaceCurrentPrefab()
    {
        if (placementPoint == null)
        {
            Debug.LogWarning("Placement point is not assigned.");
            return;
        }

        var prefab = placeablePrefabs[currentPrefabIndex];
        var obj = Instantiate(prefab, placementPoint.position, placementPoint.rotation);
        var anchor = obj.GetComponent<OVRSpatialAnchor>();

        if (anchor == null)
        {
            Debug.LogWarning("Prefab does not have OVRSpatialAnchor.");
            return;
        }
        StartCoroutine(SaveAnchorWhenReady(anchor, currentPrefabIndex, obj.transform));


    }

    private IEnumerator SaveAnchorWhenReady(OVRSpatialAnchor anchor, int index, Transform transform)
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (!anchor.Created && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (!anchor.Created)
        {
            Debug.LogWarning("Anchor not ready to be saved.");
            yield break;
        }

        anchor.Save((a, success) =>
        {
            if (success)
            {
                RegisterAnchor(a.Uuid, index, transform);
                Debug.Log($"Successfully saved anchor: {a.Uuid}");
            }
            else
            {
                Debug.LogWarning("Anchor save failed.");
            }
        });
    }


    public void RegisterAnchor(Guid uuid, int prefabIndex, Transform transform)
    {
        placedAnchors.Add(new AnchorSaveData
        {
            uuid = uuid.ToString(),
            prefabIndex = prefabIndex,
            position = transform.position,
            rotation = transform.rotation
        });
    }

    public void SaveToDisk()
    {
        string json = JsonUtility.ToJson(new AnchorSaveWrapper { anchors = placedAnchors }, true);
        string path = Path.Combine(Application.persistentDataPath, "anchors.json");
        File.WriteAllText(path, json);
        Debug.Log($"Anchors saved to {path}");
    }

    public void LoadFromDisk()
    {
        string path = Path.Combine(Application.persistentDataPath, "anchors.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("No anchor save file found.");
            return;
        }

        string json = File.ReadAllText(path);
        AnchorSaveWrapper wrapper = JsonUtility.FromJson<AnchorSaveWrapper>(json);
        placedAnchors = wrapper.anchors;

        foreach (var data in placedAnchors)
        {
            if (data.prefabIndex < 0 || data.prefabIndex >= placeablePrefabs.Count)
            {
                Debug.LogWarning($"Invalid prefab index: {data.prefabIndex}");
                continue;
            }

            var prefab = placeablePrefabs[data.prefabIndex];

            var uuid = new Guid(data.uuid);
            var options = new OVRSpatialAnchor.LoadOptions
            {
                Timeout = 0,
                StorageLocation = OVRSpace.StorageLocation.Local,
                Uuids = new Guid[] { uuid }
            };

            OVRSpatialAnchor.LoadUnboundAnchors(options, (anchors) =>
            {
                foreach (var unbound in anchors)
                {
                    if (unbound.Uuid.ToString() != data.uuid) continue;

                    void OnLocalized(OVRSpatialAnchor.UnboundAnchor ub, bool success)
                    {
                        if (!success) return;

                        var obj = Instantiate(prefab, data.position, data.rotation);
                        var anchor = obj.GetComponent<OVRSpatialAnchor>();
                        if (anchor != null)
                        {
                            ub.BindTo(anchor);
                            Debug.Log($"Loaded and bound anchor: {data.uuid}");
                        }
                    }

                    if (unbound.Localized)
                        OnLocalized(unbound, true);
                    else
                        unbound.Localize(OnLocalized);
                }
            });
        }
    }
}
