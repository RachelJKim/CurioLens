using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class STTData : MonoBehaviour
{
    public TextMeshProUGUI resultText;

    public void UpdateText(string recognizedText)
    {
        resultText.text = recognizedText;
    }
}
