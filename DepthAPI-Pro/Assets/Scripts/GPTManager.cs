using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OpenAI
{
    public class GPTManager : MonoBehaviour
    {
        public Text message;
        public Image progressBar;
        public GameObject recordButton;
        public GameObject sendButton;
        public GameObject testSendButton;
        public GameObject microphoneButtonsContainer;
        public GameObject microphoneButtonPrefab;
        public Text microphoneText;

        public SendGPTService service;

        private readonly int duration = 5;
        private readonly string fileName = "output.wav";

        private List<string> deviceNames = new List<string>();
        private bool isRecording;
        private AudioClip clip;


        //private OpenAIApi openai = new OpenAIApi("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");
        private OpenAIApi openai = new OpenAIApi("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");

        private float time = 0.0f;
        // Start is called before the first frame update
        void Start()
        {
            ReadMicrophones();
            //ShowMicrophones();
            PlayerPrefs.SetInt("user-mic-device-index", 0);
            sendButton.SetActive(false);
            testSendButton.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                progressBar.fillAmount = time / duration;

                if (time >= duration)
                {
                    time = 0;
                    isRecording = false;
                    EndRecord();
                }
            }
        }

        void ShowMicrophones()
        {
            deviceNames.Clear();
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                int index = i;
                deviceNames.Add(Microphone.devices[i]);
                GameObject button = Instantiate(microphoneButtonPrefab, pos, Quaternion.identity, microphoneButtonsContainer.transform);
                button.transform.localPosition = pos;
                pos += new Vector3(0, -30, 0);
                button.GetComponent<VRButton>().events.AddListener(() => SetMicrophone(index));
                button.GetComponentInChildren<Text>().text = Microphone.devices[i];
            }
        }

        void ReadMicrophones()
        {
            deviceNames.Clear();
            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                deviceNames.Add(Microphone.devices[i]);
            }
        }

        public void SetMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
            microphoneText.text = "Selected: " + deviceNames[index];
        }

        public void RecordAudio()
        {

            isRecording = true;
            recordButton.SetActive(false);

            var index = PlayerPrefs.GetInt("user-mic-device-index");

            #if !UNITY_WEBGL
            clip = Microphone.Start(deviceNames[index], false, duration, 44100);
            #endif

        }

        public async void EndRecord()
        {
            message.text = "Transcripting...";

#if !UNITY_WEBGL
            Microphone.End(null);
#endif

            message.text = "Transcripting... Microphone ended";
            byte[] data = SaveWav.Save(fileName, clip);

            message.text = "Transcripting... File saved";
            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = data, Name = "audio.wav" },
                // File = Application.persistentDataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "en"
            };
            message.text = "Transcripting... Request created";
            var res = await openai.CreateAudioTranscription(req);

            message.text = "Transcripted";
            progressBar.fillAmount = 0;
            message.text = res.Text;
            recordButton.SetActive(true);
            sendButton.SetActive(true);
            testSendButton.SetActive(true);
        }

        public void SendAudio()
        {
            service.call_service(message.text);
            sendButton.SetActive(false);
            testSendButton.SetActive(false);
        }

        public void SendAudioToStage1()
        {
            service.call_service_stage(message.text);
            sendButton.SetActive(false);
            testSendButton.SetActive(false);
        }

        public void SendAudioToTest()
        {
            service.call_service_test(13, message.text);
            sendButton.SetActive(false);
            testSendButton.SetActive(false);
        }
    }
}