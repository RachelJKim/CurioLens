using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public class InteractionManager : MonoBehaviour
{
    [Header("UI Prefabs")]
    public GameObject questionUIPrefab;
    public GameObject answerUIPrefab;

    [Header("Pinch Hand (Left)")]
    public OVRHand leftHand;

    [SerializeField]
    private GameObject questionUI = null;
    [SerializeField]
    private GameObject answerUI = null;

    public GameObject currentlyHoveredObject = null;  // 🧠 Track hovered object
    private bool isPinchTriggered = false;

    void Update()
    {
        HandlePinchInteraction();
    }

    // Called from InteractableUnityEventWrapper → OnUnhover(GameObject hoveredObject)
    public void OnUnhoveredObject(GameObject hoveredObject)
    {
        if (questionUI == null && hoveredObject != null)
        {
            currentlyHoveredObject = hoveredObject;  // ✅ Save it
            //PlaceUI(UIType.Question);
        }
    }

    private void HandlePinchInteraction()
    {
        if (questionUI == null || isPinchTriggered)
            return;

        if (leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
        {
            OnPinchStart();
        }
    }

    public void PlaceUI(string type)
    {
        Debug.Log("HERE");
        Vector3 basePosition = currentlyHoveredObject != null
            ? currentlyHoveredObject.transform.position + Vector3.up * 1f
            : transform.position; // fallback

        switch (type)
        {
            case "question":
                if (questionUI == null)
                {
                    questionUI = Instantiate(questionUIPrefab, basePosition, Quaternion.identity);
                    Debug.Log("Question UI placed.");
                }
                break;

            case "answer":
                if (answerUI == null)
                {
                    answerUI = Instantiate(answerUIPrefab, basePosition, Quaternion.identity);
                    Debug.Log("Answer UI placed.");
                }
                break;
        }
    }

    public void RemoveUI(string type)
    {
        switch (type)
        {
            case "question":
                if (questionUI != null)
                {
                    Destroy(questionUI);
                    questionUI = null;
                    Debug.Log("Question UI removed.");
                }
                break;

            case "answer":
                if (answerUI != null)
                {
                    Destroy(answerUI);
                    answerUI = null;
                    Debug.Log("Answer UI removed.");
                }
                break;
        }
    }

    private void OnPinchStart()
    {
        isPinchTriggered = true;

        Debug.Log("Pinch detected on left hand – triggering next action.");

        if (questionUI != null)
        {
            Vector3 uiPos = questionUI.transform.position;
            //RemoveUI(UIType.Question);
            //PlaceUI(UIType.Answer); // Will reuse last hovered object position
        }
    }
}

public enum UIType
{
    Question,
    Answer
}
