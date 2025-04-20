using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private int myLensId;
    private int currentLensId;

    private List<ObjectData> objectDataList;
    private List<UserData> userDataList; // TODO: user Data List에서 읽어가서 scene에 표시해주기

    public List<UserData> UserDataList => userDataList;

    public UnityEvent DataInitialized;

    public int MyLensId => myLensId;
    public int CurrentLensId => currentLensId;

    private void Awake()
    {
        Instance = this;

        objectDataList = new List<ObjectData>();
        userDataList = new List<UserData>();

        DataInitialized = new UnityEvent();
    }
    
    public UserData GetUserDataById(int userId)
    {
        return userDataList.FirstOrDefault(u => u.Id == userId);
    }


    public void SetData(int currentUserId, List<ObjectData> objectDataList, List<UserData> userDataList)
    {
        myLensId = currentUserId;
        currentLensId = currentUserId;
        if (objectDataList != null)
        {
            this.objectDataList = objectDataList;
        }
        
        this.userDataList = userDataList;

        DataInitialized.Invoke();
    }

    public void AddObjectData(string objectName, string concept, string description, string effect, string question)
    {
        ObjectData newObjectData = new ObjectData
        {
            Name = objectName,
            Concept = concept,
            Description = description,
            Effect = effect,
            Question = question
        };

        objectDataList.Add(newObjectData);

        FirestoreManager.Instance.StoreData(objectDataList);
    }

    public void ChangeSelectedId(int selectedId)
    {
        currentLensId = selectedId;
    }
}
