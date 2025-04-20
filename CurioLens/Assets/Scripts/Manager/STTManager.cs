using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using System.Text;
using System.IO;
using System;
using UnityEngine.Events;

public class STTManager : MonoBehaviour
{
    public static STTManager Instance;
    private STTData sttData;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingLengthSec = 15;
    private int _recordingHZ = 22050;

    // 받아온 값에 간편하게 접근하기 위한 JSON 선언
    [Serializable]
    public class VoiceRecognize
    {
        public string text;
    }

    // 사용할 언어(Kor)를 맨 뒤에 붙임
    string url = "https://naveropenapi.apigw.ntruss.com/recog/v1/stt?lang=Kor";

    public UnityEvent<string> VoiceTextUpdated;

    private void Awake()
    {
        Instance = this;

        VoiceTextUpdated = new UnityEvent<string>();
    }

    private IEnumerator PostVoice(string url, byte[] data)
    {
        Debug.Log("[HERE] " + data.Length);
        // request 생성
        WWWForm form = new WWWForm();
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        
        // 요청 헤더 설정
        request.SetRequestHeader("X-NCP-APIGW-API-KEY-ID", "enweo60kx9");
        request.SetRequestHeader("X-NCP-APIGW-API-KEY", "AzANNJwgRpccP5mNyxIw2Ed614kYzuXRbHKV329k");
        request.SetRequestHeader("Content-Type", "application/octet-stream");
        
        // 바디에 처리과정을 거친 Audio Clip data를 실어줌
        request.uploadHandler = new UploadHandlerRaw(data);
        
        // 요청을 보낸 후 response를 받을 때까지 대기
        yield return request.SendWebRequest();
        
        // 만약 response가 비어있다면 error
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request failed: " + request.error);
        }
        else
        {
            string message = request.downloadHandler.text;
            VoiceRecognize voiceRecognize = JsonUtility.FromJson<VoiceRecognize>(message);
            Debug.Log("Server response: " + voiceRecognize.text);
            VoiceTextUpdated.Invoke(voiceRecognize.text);
            
            if (sttData != null)
            {
                sttData.UpdateText(voiceRecognize.text);
            }
        }
    }

    private void Start()
    {
        sttData = FindObjectOfType<STTData>();

        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone found!");
        }
        foreach (var device in Microphone.devices)
        {
            Debug.Log("🎤 Detected Microphone: " + device);
        }

        _microphoneID = Microphone.devices[0];
        Debug.Log("Using mic: " + _microphoneID);
    }

    public void startRecording()
    {
        Debug.Log("Recording started...");
        _recording = Microphone.Start(_microphoneID, false, _recordingLengthSec, _recordingHZ);
    }

    public void stopRecording()
    {
        if (Microphone.IsRecording(_microphoneID))
        {
            Microphone.End(_microphoneID);

            Debug.Log("Recording ended. Sending to server...");

            if (_recording == null)
            {
                Debug.LogError("nothing recorded");
                return;
            }
            // audio clip to byte array
            byte[] byteData = GetByteFromAudioClip(_recording);

            // 녹음된 audioclip api 서버로 보냄
            StartCoroutine(PostVoice(url, byteData));
        }
        // return;
    }

    private const int HEADER_SIZE = 44;
    private const ushort BITS_PER_SAMPLE = 16;
    private const int BLOCK_SIZE = BITS_PER_SAMPLE / 8;

    private byte[] GetByteFromAudioClip(AudioClip clip)
    {
        MemoryStream stream = new MemoryStream();

        int sampleCount = clip.samples * clip.channels;
        float[] samples = new float[sampleCount];
        clip.GetData(samples, 0);

        int byteCount = sampleCount * BLOCK_SIZE;
        int fileSize = HEADER_SIZE + byteCount;

        // Write WAV Header
        WriteWavHeader(stream, clip, fileSize);

        // Write PCM Data
        foreach (float sample in samples)
        {
            short intSample = (short)(sample * short.MaxValue);
            byte[] byteData = BitConverter.GetBytes(intSample);
            stream.Write(byteData, 0, byteData.Length);
        }

        return stream.ToArray();
    }

    private void WriteWavHeader(Stream stream, AudioClip clip, int fileSize)
    {
        int sampleRate = clip.frequency;
        short channels = (short)clip.channels;
        int byteRate = sampleRate * channels * BLOCK_SIZE;
        short blockAlign = (short)(channels * BLOCK_SIZE);

        // ChunkID "RIFF"
        stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(fileSize - 8), 0, 4);
        stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);

        // Subchunk1ID "fmt "
        stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4); // Subchunk1Size (16 for PCM)
        stream.Write(BitConverter.GetBytes((short)1), 0, 2); // AudioFormat (1 for PCM)
        stream.Write(BitConverter.GetBytes(channels), 0, 2);
        stream.Write(BitConverter.GetBytes(sampleRate), 0, 4);
        stream.Write(BitConverter.GetBytes(byteRate), 0, 4);       // fixed
        stream.Write(BitConverter.GetBytes(blockAlign), 0, 2);
        stream.Write(BitConverter.GetBytes(BITS_PER_SAMPLE), 0, 2);

        // Subchunk2ID "data"
        stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(fileSize - HEADER_SIZE), 0, 4);
    }
}