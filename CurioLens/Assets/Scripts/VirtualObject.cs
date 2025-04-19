using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Oculus.Interaction;
using TMPro;
using UnityEngine;

public class VirtualObject : MonoBehaviour
{
    private InteractableUnityEventWrapper interactableUnityEventWrapper;

    private void Awake()
    {
        interactableUnityEventWrapper = transform.GetComponent<InteractableUnityEventWrapper>();
    }

    private void Start()
    {
        interactableUnityEventWrapper.WhenHover.AddListener(PlaceQuestionUI);
        interactableUnityEventWrapper.WhenUnhover.AddListener(RemoveQuestionUI);
        interactableUnityEventWrapper.WhenUnselect.AddListener(GetAnswerData);
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

        LLMAnswerData llmAnswerData = await LLMManager.Instance.GetDescriptionJson(gameObject.name, question);

        Debug.Log("science description 입니다" + llmAnswerData.Concept + " : " + llmAnswerData.Description + " : " + llmAnswerData.Effect);

        GameObject answerUI = InteractionManager.Instance.PlaceUI(UIType.Answer, transform);
        answerUI.transform.Find("Dialog_Text/text").GetComponent<TextMeshProUGUI>().text = llmAnswerData.Description;

        if (llmAnswerData.Effect != "none")
        {
            EffectType effectType = (EffectType)System.Enum.Parse(typeof(EffectType), llmAnswerData.Effect);

            EffectManager.Instance.CreateEffect(effectType, transform);
        }

        // TODO: ui에 띄워주기 + 관련 개념 있으면 prefeab 생성 =
        // TODO: pose 기반 firebase 저장 연동하기 
    }
}
