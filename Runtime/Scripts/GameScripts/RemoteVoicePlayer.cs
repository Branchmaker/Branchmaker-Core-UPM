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
        if (IsHLSFormat(path))
        {
            webRequest = UnityWebRequest.Get(path);
            yield return webRequest.SendWebRequest();
            // If it's HLS format, stream the audio
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(webRequest);
            if (audioClip == null) yield break;

            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            // If it's OGG format, download and play
            var audioType = GetAudioTypeFromPath(path);
            
            #if UNITY_WEBGL
            if (audioType == AudioType.OGGVORBIS) yield break;  
                #endif
                
                using (var www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
            {
                if (audioType == AudioType.MPEG)
                {
                    DownloadHandlerAudioClip dHA = new DownloadHandlerAudioClip(string.Empty, AudioType.MPEG);
                    dHA.streamAudio = true;
                    www.downloadHandler = dHA;
                    
                    www.SendWebRequest();
                    while (www.downloadProgress < 1) {
                        yield return new WaitForSeconds(.1f);
                    }
                    if (www.responseCode != 200 || www.result == UnityWebRequest.Result.ConnectionError) {
                        Debug.Log("error");
                    } else {
                        audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                        audioSource.Play();
                    }

                    yield break;

                }

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

    private bool IsHLSFormat(string path)
    {
        return path.ToLower().EndsWith(".hls");
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
            case false when path.ToLower().EndsWith(".mp4"):
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
