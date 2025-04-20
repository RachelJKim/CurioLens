using Oculus.Interaction;
using UnityEngine;
using UnityEngine.Events;

public enum PoseType
{
    ThumbsUp, ThumbsDown
}
public class PoseDetector : MonoBehaviour
{
    private SelectorUnityEventWrapper thumbsUpLeftSelectorWrapper;
    private SelectorUnityEventWrapper thumbsUpRightSelectorWrapper;
    private SelectorUnityEventWrapper thumbsDownLeftSelectorWrapper;
    private SelectorUnityEventWrapper thumbsDownRightSelectorWrapper;

    public UnityEvent<PoseType> OnPoseDetected;


    private void Awake()
    {
        thumbsUpLeftSelectorWrapper = transform.Find("ThumbsUpLeft").GetComponent<SelectorUnityEventWrapper>();
        thumbsUpRightSelectorWrapper = transform.Find("ThumbsUpRight").GetComponent<SelectorUnityEventWrapper>();
        thumbsDownLeftSelectorWrapper = transform.Find("ThumbsDownLeft").GetComponent<SelectorUnityEventWrapper>();
        thumbsDownRightSelectorWrapper = transform.Find("ThumbsDownRight").GetComponent<SelectorUnityEventWrapper>();
    }

    private void Start()
    {
        thumbsUpLeftSelectorWrapper.WhenSelected.AddListener(DetectThumbsUp);
        thumbsUpRightSelectorWrapper.WhenSelected.AddListener(DetectThumbsUp);   
        thumbsDownLeftSelectorWrapper.WhenSelected.AddListener(DetectThumbsDown);   
        thumbsDownRightSelectorWrapper.WhenSelected.AddListener(DetectThumbsDown);   
    }

    private void DetectThumbsUp()
    {
        OnPoseDetected.Invoke(PoseType.ThumbsUp);
    }

    private void DetectThumbsDown()
    {
        OnPoseDetected.Invoke(PoseType.ThumbsDown);
    }
}
