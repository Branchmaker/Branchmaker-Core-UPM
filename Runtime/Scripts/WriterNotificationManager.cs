using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
//using Steamworks;

public class WriterNotificationManager : MonoBehaviour
{

    static public bool pendingNotification;
    /*

        public GameObject notificationModal;
        public Text notificationText;

        public bool playCelebrationTrack = false;

        float countDown = 0f;

        // Use this for initialization
        void Awake () {
            notificationModal.SetActive(false);
        }

        void Start()
        {
            if (!SteamManager.Initialized) return;
            StartCoroutine(GetServerNotifications());
            // Pusher stuff

            // TODO: Replace these with your app values
            PusherSettings.Verbose = false;
            PusherSettings.AppKey = "ab2797db6bb66eb86ec7";
            PusherSettings.HttpAuthUrl = "https://branchmaker.com/api/pusher/auth";

            PusherClient.PusherOptions opts = new PusherClient.PusherOptions();
            opts.Encrypted = true;
            opts.Authorizer = new PusherClient.HttpAuthorizer(PusherSettings.HttpAuthUrl);

            pusherClient = new PusherClient.Pusher(PusherSettings.AppKey, opts);
            pusherClient.Connected += HandleConnected;
            pusherClient.ConnectionStateChanged += HandleConnectionStateChanged;
            pusherClient.Connect();
        }

        IEnumerator GetServerNotifications() {

            WWWForm formData = new WWWForm();
            formData.AddField("steam_user", SteamUser.GetSteamID().ToString());
            formData.AddField("identity", SteamFriends.GetPersonaName());

            UnityWebRequest webcall = UnityWebRequest.Post("https://branchmaker.com/client/suggestions/status", formData);
            webcall.SetRequestHeader("Cache-Control", "max-age=0, no-cache, no-store");
            webcall.SetRequestHeader("Pragma", "no-cache");
            yield return webcall.SendWebRequest();

            if (webcall.downloadHandler.text != string.Empty) {
                notificationText.text = webcall.downloadHandler.text;

                if (!OptionGridGenerator.waitingForResponse) notificationModal.SetActive(true);
                if (playCelebrationTrack) BGMManager.playHiddenTrack(0);
                SoundeffectsManager.PlayEffect("cluebell");
                pendingNotification = false;

                if (StoryManager.manager != null) {
                    StoryManager.manager.forceReloadFromServer();
                    if (!OptionGridGenerator.waitingForResponse) LoadSaveManager.readFromSteam();
                   // StoryManager.manager.StartCoroutine(StoryManager.manager.getAllTheNodes());
                }
            }
        }

        //* PUSHER BIT


        PusherClient.Pusher pusherClient = null;
        PusherClient.Channel pusherChannel = null;

        private void Update()
        {
            if (pendingNotification) {
                pendingNotification = false;
                StartCoroutine(GetServerNotifications());
            }

        }

        // Initialize
        void HandleConnected(object sender)
        {
            Debug.Log("Pusher client connected, now subscribing to story updates channel");
            pusherChannel = pusherClient.Subscribe("story-updates");
            pusherChannel.BindAll(HandleChannelEvent);
        }

        void OnDestroy()
        {
            if (pusherClient != null)
                pusherClient.Disconnect();
        }

        void HandleChannelEvent(string eventName, object evData)
        {
            pendingNotification = true;
            Debug.Log("Received event on channel, event name: " + eventName + ", data: " + JsonHelper.Serialize(evData));
        }

        void HandleConnectionStateChanged(object sender, PusherClient.ConnectionState state)
        {
            //Debug.Log("Pusher connection state changed to: " + state);
        }
        */
}
