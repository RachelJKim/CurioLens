using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendListUIController : MonoBehaviour
{    private void Start()
    {
        DataManager.Instance.DataInitialized.AddListener(Initialize);
    }

    private void Initialize()
    {
        Debug.Log("Initialize UI friend list @@@@@@@@@@@@");
        ToggleGroup friendLensToggleGroup = transform.Find("Friend Lens (Toggle Group)").GetComponent<ToggleGroup>();
        GameObject friendLensElementPrefab = Resources.Load<GameObject>("Prefabs/UI/Friend Lens (Toggle)");

        foreach(UserData userData in DataManager.Instance.UserDataList)
        {
            Toggle friendLensToggle = Instantiate(friendLensElementPrefab, friendLensToggleGroup.transform).GetComponent<Toggle>();
            friendLensToggle.group = friendLensToggleGroup;
            friendLensToggle.transform.Find("Background/Name (Text)").GetComponent<TextMeshProUGUI>().text = userData.Id.ToString();
            friendLensToggle.onValueChanged.AddListener((isOn) => UpdateSelectedFriendLens(isOn, userData.Id));
        }        
    }

    private void UpdateSelectedFriendLens(bool isOn, int userId)
    {
        if (isOn == false)
        {
            return;
        }

        EffectManager.Instance.RemoveEffects();
        GameObject.Find("Virtual Objects").GetComponent<VirtualObjectsController>().UpdateObjectInfo(userId);
    }
}
