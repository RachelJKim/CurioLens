using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class FirestoreManager : MonoBehaviour
{
    public static FirestoreManager Instance { get; private set; }

    private FirebaseFirestore db;

    [SerializeField]
    private int currentUserId = -1;

    private void Awake()
    {
        Instance = this;
    }

    private async void Start()
    {
        await InitiateAsync();
    }

    public async Task InitiateAsync()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Firebase initialization failed: " + task.Exception);
            }
            else if (task.IsCompleted)
            {

                db = FirebaseFirestore.DefaultInstance;
#if UNITY_EDITOR
                db.Settings.PersistenceEnabled = false;
#endif
                Debug.Log("initialize success");
                return;
            }
        });

        List<ObjectData> objectDataList = new List<ObjectData>();
        if (currentUserId != -1)
        {
            objectDataList = await LoadObjectListByUserId(currentUserId);
        }
        string jsonString = await LoadDataFromFirestoreAsync();

        try
        {
            List<UserData> userDataList = JsonConvert.DeserializeObject<List<UserData>>(jsonString);
            Debug.Log("UserData list parsed successfully!");

            DataManager.Instance.SetData(currentUserId, objectDataList, userDataList);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to parse JSON: " + e.Message);
        }
    }

    private async Task<string> LoadDataFromFirestoreAsync() // data 가져오기 (모든 user data 거 ) 
    {
        try
        {
            CollectionReference userDataRef = db.Collection("user_data");
      
            List<Dictionary<string, object>> userDataList = new List<Dictionary<string, object>>();


            await userDataRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    QuerySnapshot snapshot = task.Result;

                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        if (document.Exists)
                        {
                            Dictionary<string, object> docData = document.ToDictionary();

                            if (!docData.ContainsKey("currentId"))
                            {
                                userDataList.Add(docData);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Error loading data: " + task.Exception);
                }
            });

            string jsonData = JsonConvert.SerializeObject(userDataList, Formatting.Indented);
            Debug.Log("json Data" + jsonData);

            return jsonData;
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading data from Firestore: " + e.Message);
            return null;
        }
    }

    public async Task<List<ObjectData>> LoadObjectListByUserId(int userId)
    {
        try
        {
            DocumentReference docRef = FirebaseFirestore.DefaultInstance
                .Collection("user_data")
                .Document(userId.ToString());

            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> docData = snapshot.ToDictionary();

                if (docData.ContainsKey("objectList"))
                {
                    var rawList = docData["objectList"] as List<object>;
                    List<ObjectData> resultList = new List<ObjectData>();

                    foreach (var item in rawList)
                    {
                        Dictionary<string, object> objDict = item as Dictionary<string, object>;

                        ObjectData obj = new ObjectData
                        {
                            Name = objDict["Name"].ToString(),
                            Concept = objDict["Concept"].ToString(),
                            Description = objDict["Description"].ToString(),
                            Effect = objDict["Effect"].ToString(),
                            Question = objDict["Question"].ToString()
                        };

                        resultList.Add(obj);
                    }

                    Debug.Log($"Loaded {resultList.Count} ObjectData for user {userId}");
                    return resultList;
                }
                else
                {
                    Debug.LogWarning("objectList not found in user data.");
                    return null;
                }
            }
            else
            {
                Debug.LogWarning($"No document found for user ID: {userId}");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error while loading object list: " + e.Message);
            return null;
        }
    }

    public async void StoreData(List<ObjectData> objectDataList)
    {
        UserData userData = new UserData();
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        long newId = 0;
        string jsonData = "";

        if (currentUserId == -1) // 생성 
        {
            await db.RunTransactionAsync(async transaction =>
            {
                DocumentReference counterRef = db.Collection("user_data").Document("counter");

                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(counterRef);

                if (!snapshot.Exists)
                {
                    transaction.Set(counterRef, new Dictionary<string, object> { { "currentId", 0L } }); 
                    newId = 1; 
                }
                else
                {
                    long currentId = snapshot.GetValue<long>("currentId");
                    newId = currentId + 1;
                    transaction.Update(counterRef, "currentId", newId);
                }

                userData.SetUserData((int)newId, objectDataList);
                jsonData = JsonUtility.ToJson(userData, true);
            });
        }
        else
        {
            userData.SetUserData(currentUserId, objectDataList);
            jsonData = JsonUtility.ToJson(userData, true);
        }
        
        try
        {
            List<Dictionary<string, object>> convertedList = new List<Dictionary<string, object>>();

            foreach (var obj in userData.ObjectList)
            {
                var objDict = new Dictionary<string, object>
                {
                    { "Name", obj.Name },
                    { "Concept", obj.Concept },
                    { "Description", obj.Description },
                    { "Effect", obj.Effect },
                    { "Question", obj.Question }
                };

                convertedList.Add(objDict);
            }

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "id", userData.Id },
                { "objectList", convertedList }
            };

            DocumentReference docRef = FirebaseFirestore.DefaultInstance
                .Collection("user_data")
                .Document(userData.Id.ToString());

            await docRef.SetAsync(data);
            Debug.Log("User data stored successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to store user data: " + e.Message);
        }

    }
}
