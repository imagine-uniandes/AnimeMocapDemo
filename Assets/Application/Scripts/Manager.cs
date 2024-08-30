using System;
using System.IO;
using UnityEngine;
using TMPro;
using UniRx;

public class Manager : MonoBehaviour
{    
    [SerializeField]
    public Animator animator;
    // OSC
    [SerializeField]
    private OscServer oscServer;
    [SerializeField]
    private OscClient oscClient;
    // UI
    [SerializeField]
    private TMP_Text versionText;
    [SerializeField]
    private TMP_InputField receivePort;
    [SerializeField]
    private TMP_InputField sendIP;
    [SerializeField]
    private TMP_InputField sendPort;

    private const string SettingsPath = "./Settings.json";
    private Settings settings = new Settings();
    
    private bool LoadSettings()
    {
        if (!File.Exists(SettingsPath))
        {
            return false;
        }
        var contents = File.ReadAllText(SettingsPath);

        try
        {
            JsonUtility.FromJsonOverwrite(contents, this.settings);
        }catch(Exception)
        {
            return false;
        }

        return true;
    }
    private void SaveSettings()
    {
        var contents = JsonUtility.ToJson(this.settings, true);
        File.WriteAllText(SettingsPath, contents);
    }
    private void Start()
    {
        if (!LoadSettings())
        {
            SaveSettings();
        }

        this.versionText.text = Const.Version;

        SetupUI();

        SetupMotionEvent();

        this.oscServer.Run(this.settings.ReceivePort);
        this.oscClient.Run(this.settings.SendIP, this.settings.SendPort);
    }
    private void OnApplicationQuit()
    {
        End();
    }
    private void OnDestroy()
    {
        End();
    }
    private void End()
    {
        this.oscClient.Stop();
        this.oscServer.Stop();
    }
    private void SetupUI()
    {
        SetupReceivePortUI();
        SetupSendIPUI();
        SetupSendPortUI();
    }
    private void SetupReceivePortUI()
    {
        IDisposable disposable = null;
        this.receivePort.onValueChanged.AddListener((text) => 
        {
            var number = 0;
            if (!int.TryParse(text, out number))
            {
                return;
            }
            this.settings.ReceivePort = number;
            SaveSettings();

            disposable?.Dispose();
            disposable = Observable.Timer(TimeSpan.FromSeconds(Const.SaveSettingsDelay))
            .TakeUntilDestroy(this)
            .Subscribe(_ =>
            {
                this.oscServer.Stop();
                this.oscServer.Run(this.settings.ReceivePort);
            });
        });
        this.receivePort.SetTextWithoutNotify(this.settings.ReceivePort.ToString());
    }
    private void SetupSendIPUI()
    {
        IDisposable disposable = null;
        this.sendIP.onValueChanged.AddListener((text) => 
        {
            this.settings.SendIP = text;
            SaveSettings();

            disposable?.Dispose();
            disposable = Observable.Timer(TimeSpan.FromSeconds(Const.SaveSettingsDelay))
            .TakeUntilDestroy(this)
            .Subscribe(_ =>
            {
                this.oscClient.Stop();
                this.oscClient.Run(this.settings.SendIP, this.settings.SendPort);
            });
        });
        this.sendIP.SetTextWithoutNotify(this.settings.SendIP);
    }
    private void SetupSendPortUI()
    {
        IDisposable disposable = null;
        this.sendPort.onValueChanged.AddListener((text) => 
        {
            var number = 0;
            if (!int.TryParse(text, out number))
            {
                return;
            }
            this.settings.SendPort = number;
            SaveSettings();

            disposable?.Dispose();
            disposable = Observable.Timer(TimeSpan.FromSeconds(Const.SaveSettingsDelay))
            .TakeUntilDestroy(this)
            .Subscribe(_ =>
            {
                this.oscClient.Stop();
                this.oscClient.Run(this.settings.SendIP, this.settings.SendPort);
            });
        });
        this.sendPort.SetTextWithoutNotify(this.settings.SendPort.ToString());
    }
    private void SetupMotionEvent()
    {
        this.oscServer.onDataReceived.AddListener((message) => 
        {
            try
            {
                switch (message.address)
                {
                    case "/VMC/Ext/Root/Pos":
                    {
                        OnReceivedVMCRoot((string)message.values[0], new Vector3((float)message.values[1], (float)message.values[2], (float)message.values[3]), new Quaternion((float)message.values[4], (float)message.values[5], (float)message.values[6], (float)message.values[7]));
                        break;
                    }
                    case "/VMC/Ext/Bone/Pos":
                    {
                        OnReceivedVMCBone((string)message.values[0], new Vector3((float)message.values[1], (float)message.values[2], (float)message.values[3]), new Quaternion((float)message.values[4], (float)message.values[5], (float)message.values[6], (float)message.values[7]));
                        break;
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e.ToString());
            }
        });
    }
    private void OnReceivedVMCRoot(string name, Vector3 position, Quaternion rotation)
    {
        this.animator.transform.localPosition = position;
        this.animator.transform.localRotation = rotation;
    }
    private void OnReceivedVMCBone(string name, Vector3 position, Quaternion rotation)
    {
        foreach (var bone in Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (bone.ToString() == name)
            {
                var boneTransform = this.animator.GetBoneTransform((HumanBodyBones)bone);
                boneTransform.localPosition = position;
                boneTransform.localRotation = rotation;
                return;
            }
        }
    }
}
