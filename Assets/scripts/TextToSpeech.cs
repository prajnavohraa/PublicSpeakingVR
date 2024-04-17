using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.IO;
using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;

public class TextToSpeech : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource; // Reference to the AudioSource component

    private AmazonPollyClient client;

    void Start()
    {
        // Initialize AWS credentials and Polly client
        var credentials = new BasicAWSCredentials("AKIAYS2NRKLVHZPVGT5N", "OPmB9QHBP8xu99Igd1OZCq5tmfwYBuHSQtR63A6s");
        client = new AmazonPollyClient(credentials, RegionEndpoint.EUCentral1);
    }

    public async Task Speak(string textToSpeak)
    {
        // Create a speech synthesis request using Amazon Polly
        textToSpeak="Hello, I am Aria. How can I help you today?";
        var request = new SynthesizeSpeechRequest
        {
            Text = textToSpeak,
            Engine = Engine.Neural,
            VoiceId = VoiceId.Aria, // You can change the voice ID based on your preferences
            OutputFormat = OutputFormat.Mp3
        };

        try
        {
            var response = await client.SynthesizeSpeechAsync(request);
            WriteIntoFile(response.AudioStream);
            await PlayAudioClip();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to synthesize speech: " + e.Message);
        }
    }

    private void WriteIntoFile(Stream stream)
    {
        // Write the received audio stream to a file
        using (var fileStream = new FileStream($"{Application.persistentDataPath}/audio.mp3", FileMode.Create))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
        }
    }

    private async Task PlayAudioClip()
    {
        // Load the audio file as an AudioClip and play it
        string path = $"{Application.persistentDataPath}/audio.mp3";
        using (var www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG))
        {
            var op = www.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                audioSource.Play();
            }
            else
            {
                Debug.LogError("Failed to load audio clip: " + www.error);
            }
        }
    }
}
