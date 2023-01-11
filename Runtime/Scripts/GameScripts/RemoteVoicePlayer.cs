using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class RemoteVoicePlayer : MonoBehaviour
{
    private static RemoteVoicePlayer player;
    // Start is called before the first frame update
    void Awake()
    {
        player = this;
    }

    public static void PlayRemoteOgg(string uri)
    {
        if (string.IsNullOrEmpty(uri)) return;
        if (uri.EndsWith(".jpg") || uri.EndsWith(".jpeg")) return;
        player.StartCoroutine(player.PlayFile(uri));
    }

    public static void StopSpeaking()
    {
        if (player == null) return;
        player.StopAllCoroutines();
        player.GetComponent<AudioSource>().Stop();
    }
    
    IEnumerator PlayFile(string path)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                var myClip = DownloadHandlerAudioClip.GetContent(www);
                if (myClip != null)
                {
                    GetComponent<AudioSource>().clip = myClip;
                    GetComponent<AudioSource>().Play();
                }
            }
        }
    }
}
