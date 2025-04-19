using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

public enum UIType
{
    Question,
    Answer
}

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("UI Prefabs")]
    public GameObject questionUIPrefab;
    public GameObject answerUIPrefab;

    [Header("Pinch Hand (Left)")]
    public OVRHand leftHand;

    public GameObject questionUI = null;
    public GameObject answerUI = null;

    public GameObject currentlyHoveredObject = null;  // Track hovered object
    private bool isPinchTriggered = false;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        HandlePinchInteraction();
    }

    // Called from InteractableUnityEventWrapper → OnUnhover(GameObject hoveredObject)
    public void OnUnhoveredObject(GameObject hoveredObject)
    {
        if (questionUI == null && hoveredObject != null)
        {
            currentlyHoveredObject = hoveredObject;  // Save it
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

    public GameObject PlaceUI(UIType uiType, Transform hoveredObjectTransfrom)
    {
        Vector3 basePosition = hoveredObjectTransfrom.transform.position + Vector3.up * 0.7f;

        switch (uiType)
        {
            case UIType.Question:
                if (questionUI == null)
                {
                    questionUI = Instantiate(questionUIPrefab, hoveredObjectTransfrom);
                    questionUI.transform.position = basePosition;
                    questionUI.transform.rotation = Quaternion.identity;
                    questionUI.name = uiType.ToString();
                    Debug.Log("Question UI placed.");

                    return questionUI;
                }
                break;

            case UIType.Answer:
                if (answerUI == null)
                {
                    answerUI = Instantiate(answerUIPrefab, hoveredObjectTransfrom);
                    answerUI.transform.position = basePosition;
                    answerUI.transform.rotation = Quaternion.identity;
                    answerUI.name = uiType.ToString();
                    Debug.Log("Answer UI placed.");

                    return answerUI;
                }
                break;
        }

        return null;
    }

    public void RemoveUI(UIType uiType)
    {
        switch (uiType)
        {
            case UIType.Question:
                if (questionUI != null)
                {
                    Destroy(questionUI);
                    questionUI = null;
                    Debug.Log("Question UI removed.");
                }
                break;

            case UIType.Answer:
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


