using System.Collections.Generic;
using UnityEngine;

public class DataManager
{
    public static DataManager Instance { get; private set; }

    private List<ObjectData> objectDataList;
    private List<UserData> userDataList; // TODO: user Data List에서 읽어가서 scene에 표시해주기

    public static void CreateInstance(List<ObjectData> objectDataList, List<UserData> userDataList)
    {
        Instance = new DataManager(objectDataList, userDataList);
    }

    public DataManager(List<ObjectData> objectDataList, List<UserData> userDataList)
    {
        this.objectDataList = objectDataList;
        this.userDataList = userDataList;
    }

    // TODO: object Data 저장 할 떄 마다 호출 필요 
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
