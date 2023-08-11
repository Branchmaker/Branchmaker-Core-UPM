using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class RemoteVoicePlayer : MonoBehaviour
{
    private static RemoteVoicePlayer _player;
    private UnityWebRequest webRequest;
    private AudioSource audioSource;
    private void Awake()
    {
        _player = this;
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlayRemoteOgg(string uri)
    {
        if (string.IsNullOrEmpty(uri)) return;
        if (uri.EndsWith(".jpg") || uri.EndsWith(".jpeg")) return;
        _player.StartCoroutine(_player.PlayFile(uri));
    }

    public static void StopSpeaking()
    {
        if (_player == null) return;
        _player.StopAllCoroutines();
        _player.GetComponent<AudioSource>().Stop();
    }
    
    private IEnumerator PlayFile(string path)
    {
        webRequest = UnityWebRequest.Get(path);
        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Could not load " + webRequest.url);
            Debug.Log(webRequest.error);
            yield break;
        }

        if (IsHLSFormat(webRequest))
        {
            // If it's HLS format, stream the audio
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
            if (audioClip == null) yield break;

            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else if (IsOGGFormat(path))
        {
            // If it's OGG format, download and play
            var audioType = GetAudioTypeFromPath(path);
            using (var www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Could not load " + www.url);
                    Debug.Log(www.error);
                }
                else
                {
                    var myClip = DownloadHandlerAudioClip.GetContent(www);
                    if (myClip == null) yield break;
                    audioSource.clip = myClip;
                    audioSource.Play();
                }
            }
        }
    }

    private bool IsHLSFormat(UnityWebRequest request)
    {
        return request.url.ToLower().EndsWith(".hls");
    }

    private bool IsOGGFormat(string path)
    {
        return path.ToLower().EndsWith(".ogg");
    }

    private static AudioType GetAudioTypeFromPath(string path)
    {
        switch (path.ToLower().EndsWith(".ogg"))
        {
            case false when path.ToLower().EndsWith(".mp3"):
                return AudioType.MPEG;
            case false when path.ToLower().EndsWith(".s3m"):
                return AudioType.S3M;
            case false:
                break;
            default:
                return AudioType.OGGVORBIS;
        }

        return AudioType.OGGVORBIS;
    }

    private void OnDestroy()
    {
        // Clean up the web request when the object is destroyed
        if (webRequest != null && !webRequest.isDone)
        {
            webRequest.Abort();
        }
    }
}
