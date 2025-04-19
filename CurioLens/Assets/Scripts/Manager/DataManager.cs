using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private List<ObjectData> objectDataList;
    private List<UserData> userDataList; // TODO: user Data List에서 읽어가서 scene에 표시해주기

    private void Awake()
    {
        Instance = this;

        objectDataList = new List<ObjectData>();
        userDataList = new List<UserData>();
    }

    public void SetData(List<ObjectData> objectDataList, List<UserData> userDataList)
    {
        if (objectDataList != null)
        {
            this.objectDataList = objectDataList;
        }
        
        this.userDataList = userDataList;
    }

    public void AddObjectData(string objectName, string concept, string description)
    {
        ObjectData newObjectData = new ObjectData
        {
            Name = objectName,
            Concept = concept,
            Description = description
        };

        objectDataList.Add(newObjectData);

        FirestoreManager.Instance.StoreData(objectDataList);
    }
}
