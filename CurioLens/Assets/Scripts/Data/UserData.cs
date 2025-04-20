using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;


[System.Serializable]
public class UserData
{
    [JsonProperty("id")]
    private int id;
    [JsonProperty("objectList")]
    private List<ObjectData> objectList;

    public int Id => id;
    public List<ObjectData> ObjectList => objectList;
    
    // TODO: description default 값 제거 
    public void SetUserData(int id, List<ObjectData> objectList)
    {
        this.id = id;
        this.objectList = objectList;
    }
}
