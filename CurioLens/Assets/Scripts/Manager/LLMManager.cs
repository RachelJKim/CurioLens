using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class ScienceDescription
{
    public string Concept { get; set; }
    public string Description { get; set; }
}

public class LLMManager
{
    public static LLMManager Instance { get; private set; }

    private string systemPrompt;

    public static void CreateInstance()
    {
        Instance = new LLMManager();
    }

    public LLMManager()
    {
        TextAsset systemPromptFile = Resources.Load<TextAsset>("Prompt/SystemPrompt");

        systemPrompt = systemPromptFile.text;
    }

    // output: string (science notion, description for science notion )
    public async Task<ScienceDescription> GetDescriptionJson(string objectName, string question)
    {
        string userPrompt = $@"
        Selected Object: {objectName},
        Question: {question}
        ";
        
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Environment.API_KEY}");

            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                max_tokens = 1000,
                temperature = 0.7
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(Environment.API_URL, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error: {response.StatusCode}, {response.ReasonPhrase}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            Debug.Log("LLM Response: " + responseString);

            ScienceDescription scienceDescription = new ScienceDescription();

            try
            {
                var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseString);

                if (responseObject.ContainsKey("choices"))
                {
                    var choices = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(responseObject["choices"].ToString());

                    if (choices.Count > 0)
                    {
                        var messageContent = JsonConvert.DeserializeObject<Dictionary<string, object>>(choices[0]["message"].ToString());
                        string responseContent = messageContent["content"].ToString();

                        string jsonContent = ExtractJsonFromResponse(responseContent);
                        scienceDescription = JsonConvert.DeserializeObject<ScienceDescription>(jsonContent);
                    }
                }
            }
            catch (JsonReaderException ex)
            {
                Debug.LogError("JSON Parsing Error: " + ex.Message);
            }

            return scienceDescription;
        }
    }

    private string ExtractJsonFromResponse(string content)
    {
        int jsonStart = content.IndexOf("{");
        int jsonEnd = content.LastIndexOf("}");
        
        if (jsonStart != -1 && jsonEnd != -1 && jsonEnd > jsonStart)
        {
            return content.Substring(jsonStart, jsonEnd - jsonStart + 1);
        }

        throw new Exception("Invalid JSON structure in response content.");
    }
}
