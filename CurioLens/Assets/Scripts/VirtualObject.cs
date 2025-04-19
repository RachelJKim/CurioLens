using System;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class VirtualObject : MonoBehaviour
{
    private InteractableUnityEventWrapper interactableUnityEventWrapper;

    private LLMAnswerData currentLLMAnswerData;

    private void Awake()
    {
        interactableUnityEventWrapper = transform.GetComponent<InteractableUnityEventWrapper>();
    }

    private void Start()
    {
        interactableUnityEventWrapper.WhenHover.AddListener(PlaceQuestionUI);
        interactableUnityEventWrapper.WhenUnhover.AddListener(RemoveQuestionUI);
        interactableUnityEventWrapper.WhenUnselect.AddListener(GetAnswerData);

        GameObject.Find("Poses").GetComponent<PoseDetector>().OnPoseDetected.AddListener(UpdateObjectData);
    }

    private void PlaceQuestionUI()
    {
        InteractionManager.Instance.PlaceUI(UIType.Question, transform);
    }

    private void RemoveQuestionUI()
    {
        InteractionManager.Instance.RemoveUI(UIType.Question);
    }

    private async void GetAnswerData()
    {
        string question = transform.Find("Question/Dialog_Text/text").GetComponent<TextMeshProUGUI>().text;
        if (question == String.Empty)
        {
            return;
        }

        currentLLMAnswerData = await LLMManager.Instance.GetDescriptionJson(gameObject.name, question);

        Debug.Log("science description 입니다" + currentLLMAnswerData.Concept + " : " + currentLLMAnswerData.Description + " : " + currentLLMAnswerData.Effect);

        GameObject answerUI = InteractionManager.Instance.PlaceUI(UIType.Answer, transform);
        answerUI.transform.Find("Dialog_Text/text").GetComponent<TextMeshProUGUI>().text = currentLLMAnswerData.Description;

        if (currentLLMAnswerData.Effect != "none")
        {
            EffectType effectType = (EffectType)System.Enum.Parse(typeof(EffectType), currentLLMAnswerData.Effect);
            EffectManager.Instance.CreateEffect(effectType, transform);
        }
    }

    private void UpdateObjectData(PoseType poseType)
    {
        switch (poseType)
        {
            case PoseType.ThumbsUp:
                DataManager.Instance.AddObjectData(gameObject.name, currentLLMAnswerData.Concept, currentLLMAnswerData.Description);
                break;
            case PoseType.ThumbsDown: // UI 제거
                break;
        }
    }
}
