using System;
using System.Collections.Generic;
using System.Linq;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class VirtualObject : MonoBehaviour
{
    private InteractableUnityEventWrapper interactableUnityEventWrapper;

    private LLMAnswerData currentLLMAnswerData;
    private string question;

    private void Awake()
    {
        interactableUnityEventWrapper = transform.GetComponent<InteractableUnityEventWrapper>();
    }

    private void Start()
    {
        interactableUnityEventWrapper.WhenHover.AddListener(PlaceQuestionUI);
        interactableUnityEventWrapper.WhenUnhover.AddListener(RemoveQuestionUI);
        interactableUnityEventWrapper.WhenSelect.AddListener(StartLoadingAnimation);
        interactableUnityEventWrapper.WhenUnselect.AddListener(FinishLoadingAnimation);

        GameObject.Find("Poses").GetComponent<PoseDetector>().OnPoseDetected.AddListener(UpdateObjectData);
        STTManager.Instance.VoiceTextUpdated.AddListener(GetAnswerData);
    }

    public void UpdateObjectData(int userId)
    {
        List<ObjectData> objectDataList = DataManager.Instance.GetUserDataById(userId).ObjectList;

        ObjectData found = objectDataList.FirstOrDefault(obj => obj.Name == transform.name);

        if (found.Effect != "none")
        {
            EffectType effectType = (EffectType)System.Enum.Parse(typeof(EffectType), found.Effect);
            EffectManager.Instance.CreateEffect(effectType, transform);
        }
    }

    private void PlaceQuestionUI()
    {
        if (DataManager.Instance.CurrentLensId != DataManager.Instance.MyLensId)
        {
            GameObject questionFromOtherUI = InteractionManager.Instance.PlaceUI(UIType.QuestionFromOther, transform);
            questionFromOtherUI.transform.Find("QuestionFromOther/Dialog_Text/text").GetComponent<TextMeshProUGUI>().text = DataManager.Instance.GetUserDataById(DataManager.Instance.CurrentLensId).ObjectList.FirstOrDefault(obj => obj.Name == transform.name).Question;
        }
        else
        {
            InteractionManager.Instance.PlaceUI(UIType.Question, transform);
        }
    }

    private async void GetAnswerData(string question)
    {
        this.question = question;
        currentLLMAnswerData = await LLMManager.Instance.GetDescriptionJson(gameObject.name, question);

        Debug.Log("퀘스천은" + question);

        Debug.Log("science description 입니다" + currentLLMAnswerData.Concept + " : " + currentLLMAnswerData.Description + " : " + currentLLMAnswerData.Effect);

        GameObject answerUI = InteractionManager.Instance.PlaceUI(UIType.Answer, transform);
        answerUI.transform.Find("Concept (Text)").GetComponent<TextMeshProUGUI>().text = currentLLMAnswerData.Concept;
        answerUI.transform.Find("Description (Text)").GetComponent<TextMeshProUGUI>().text = currentLLMAnswerData.Description;

        EffectType effectType = (EffectType)System.Enum.Parse(typeof(EffectType), currentLLMAnswerData.Effect);
        EffectManager.Instance.CreateEffect(effectType, transform);
    }

    private void RemoveQuestionUI()
    {
        InteractionManager.Instance.RemoveUI(UIType.Question);
    }

    private void StartLoadingAnimation()
    {
        transform.Find("Question/Loading Bar").gameObject.SetActive(true);

        STTManager.Instance.startRecording();
    }

    private void FinishLoadingAnimation()
    {
        transform.Find("Question/Loading Bar").gameObject.SetActive(false);
        STTManager.Instance.stopRecording();
    }

    private void UpdateObjectData(PoseType poseType)
    {
        switch (poseType)
        {
            case PoseType.ThumbsUp:
                DataManager.Instance.AddObjectData(gameObject.name, currentLLMAnswerData.Concept, currentLLMAnswerData.Description, currentLLMAnswerData.Effect, question);
                break;
            case PoseType.ThumbsDown: // UI 제거
                InteractionManager.Instance.RemoveUI(UIType.Question);
                InteractionManager.Instance.RemoveUI(UIType.Answer);
                break;
        }
    }
}
