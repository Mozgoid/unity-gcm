using UnityEngine;
using System.Collections;
using Parse;

public class ParseNotifications : MonoBehaviour
{

#if UNITY_ANDROID
    private void Awake()
    {
        GCM.Initialize(); //create GCMReceiver

        // Set callbacks
        GCM.SetErrorCallback(errorId => Debug.Log("Parse notification error: " + errorId));
        GCM.SetUnregisteredCallback(registrationId => Debug.Log("Parse notification Unregistered: " + registrationId));
        GCM.SetDeleteMessagesCallback(total => Debug.Log("Parse notification DeleteMessages " + total));
        GCM.SetMessageCallback(HandleMessage);
    }
#elif UNITY_IPHONE
    void Update()
    {
        foreach (var notification in NotificationServices.remoteNotifications)
        {
            HandleMessage(notification.userInfo);
        }
        NotificationServices.ClearRemoteNotifications();
    }
#endif

    //You can get data that tou send in notification's JSON from dict param.
    //Warrning!
    //Not all messages come here.
    //On iOS messages will be handled if you tap on notification, also it will be handled if you be in game when notification will arrive.
    //But message won't be handled if notification arrive when you not in game and you not tap the notification.
    //On Android it will be handled only if notification arrives when game running on foreground.
    void HandleMessage(IDictionary dict)
    {
        foreach (var key in dict.Keys)
        {
            Debug.Log(key + " : " + dict[key]);
        }
    }


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        StartCoroutine(PrepareNotifications());
    }

    private IEnumerator PrepareNotifications()
    {
        //wait for internet connection
        while (Application.internetReachability == NetworkReachability.NotReachable)
        {
            yield return null;
        }

        if (PlayerPrefs.HasKey("currentInstallation"))
        {
            RefreshInstalliation();
        }
        else
        {
            Debug.Log("creating parse notification installiation");
#if UNITY_IPHONE
            yield return StartCoroutine(CreateIosInstalliation());
#elif UNITY_ANDROID
            CreateAndroidInstalliation();
#endif
        }
    }

    //update installiation object in case you change version or something else
    private void RefreshInstalliation()
    {
        ParseObject.GetQuery("_Installation")
            .GetAsync(PlayerPrefs.GetString("currentInstallation"))
            .ContinueWith(t =>
            {
                if (!t.IsFaulted && t.Result != null)
                {
                    UpdateObject(t.Result);
                    t.Result.SaveAsync();
                }
            });
    }


#if UNITY_IPHONE
    IEnumerator CreateIosInstalliation()
    {
        NotificationServices.RegisterForRemoteNotificationTypes(RemoteNotificationType.Alert
            | RemoteNotificationType.Badge
            | RemoteNotificationType.Sound);

        //wait for token
        while (NotificationServices.deviceToken == null)
        {
            yield return null;
        }

        Debug.Log("parse notification Token gotten");
        var token = NotificationServices.deviceToken;
        var tokenString = System.BitConverter.ToString(token).Replace("-", "").ToLower();
        var obj = CreateInstallationObject(tokenString, "ios");
        obj.SaveAsync().ContinueWith(t =>
        {
            if (t.IsFaulted || obj.ObjectId == null)
            {
                Debug.LogError("save parse installiation failed");
            }
            else
            {
                Debug.LogError("save parse installiation ok");
                PlayerPrefs.SetString("currentInstallation", obj.ObjectId);
            }
        });
    }
#elif UNITY_ANDROID
    private void CreateAndroidInstalliation()
    {
        const string ProjectID = "1234567890"; //replace with yours
        GCM.SetRegisteredCallback(registrationId =>
        {
            Debug.Log("Parse notification Registered: " + registrationId);
            var obj = CreateInstallationObject(registrationId, "android");
            obj["pushType"] = "gcm";
            obj["GCMSenderId"] = ProjectID;
            obj.SaveAsync().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Debug.LogError("error on parse push installiation ");
                }
                else if (obj.ObjectId != null)
                {
                    PlayerPrefs.SetString("currentInstallation", obj.ObjectId);
                }
            });
        });

        string[] senderIds = { ProjectID };
        GCM.Register(senderIds);
    }
#endif

    private ParseObject CreateInstallationObject(string token, string deviceType)
    {
        var obj = new ParseObject("_Installation");
        obj["deviceToken"] = token;
        obj["appIdentifier"] = "com.example.parsenotifications"; //replace with yours
        obj["deviceType"] = deviceType;
        obj["timeZone"] = "UTC";
        obj["appName"] = "SUPER AWESOME GAME 4"; //replace with yours
        obj.AddToList("channels", "chanel1");
        obj.AddToList("channels", "chanel2");
        UpdateObject(obj);
        return obj;
    }


    private void UpdateObject(ParseObject obj)
    {
        obj["appVersion"] = "1.0";
        obj["parseVersion"] = "1.3.1";
        obj["language"] = Application.systemLanguage.ToString();
        //change other data that you want
    }
}