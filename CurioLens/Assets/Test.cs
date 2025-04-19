using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private GameObject convectionObject;
    private void Awake()
    {
        convectionObject = Resources.Load<GameObject>("Prefabs/Effects/Convection");
        // FirestoreManager.CreateInstance();
    }

    private void Start()
    {
        GameObject.Instantiate(convectionObject, transform);
    }

    // private async void Start()
    // {
    //     await FirestoreManager.Instance.InitiateAsync();
    //     // string Test = await FirestoreManager.Instance.LoadDataFromFirestoreAsync();
    //     // Debug.Log("test~" + Test);
    //     // 내가 이미 있는 유저일 때 => 데이터 불러오기 
    //     // 어떤 pointing 할 때 data 저장할 수 있게끔 함수 만들어두기 
    //     // 모든 user data load 해와서 처음에 읽어올 수 있게 매핑 해두기? => 읽어오면 데이터 적용 가능하게끔? 
    //     List<ObjectData> objectDataList = new List<ObjectData>();

    //     ObjectData obj1 = new ObjectData
    //     {
    //         Name = "테스트 객체1",
    //         Concept = "테스트 개념1",
    //         Description = "테스트 설명1"
    //     };
    //     ObjectData obj2 = new ObjectData
    //     {
    //         Name = "테스트 객체2",
    //         Concept = "테스트 개념2",
    //         Description = "테스트 설명2"
    //     };
    //     ObjectData obj3 = new ObjectData
    //     {
    //         Name = "테스트 객체3",
    //         Concept = "테스트 개념3",
    //         Description = "테스트 설명3"
    //     };


    //     objectDataList.Add(obj1);
    //     objectDataList.Add(obj2);
    //     objectDataList.Add(obj3);

    //     FirestoreManager.Instance.StoreData(objectDataList); // todo: 나중에 데이터 만드는 용 
    // }
}
