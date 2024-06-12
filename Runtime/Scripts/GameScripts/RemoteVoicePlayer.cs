using System.Collections;
using BranchMaker;
using BranchMaker.Runtime;
using BranchMaker.Runtime.Utility;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class RemoteVoicePlayer : BaseController<RemoteVoicePlayer>
{
    private UnityWebRequest _webRequest;
    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake();
        _audioSource = GetComponent<AudioSource>();
    }
    
    void Start()
    {
        StoryManager.Instance.OnBlockChange.AddListener(ProcessBlock);
    }

    private void ProcessBlock(BranchNodeBlock block)
    {
        StopSpeaking();
        if (!string.IsNullOrEmpty(block.voice_file)) PlayRemoteOgg(block.voice_file);
    }

    private void PlayRemoteOgg(string uri)
    {
        if (string.IsNullOrEmpty(uri)) return;
        if (uri.EndsWith(".jpg") || uri.EndsWith(".jpeg")) return;
        StartCoroutine(PlayFile(uri));
    }

    public void StopSpeaking()
    {
        StopAllCoroutines();
        GetComponent<AudioSource>().Stop();
    }
    
    private IEnumerator PlayFile(string path)
    {
        if (IsHLSFormat(path))
        {
            _webRequest = UnityWebRequest.Get(path);
            yield return _webRequest.SendWebRequest();
            // If it's HLS format, stream the audio
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(_webRequest);
            if (!audioClip) yield break;

            _audioSource.clip = audioClip;
            _audioSource.Play();
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
                        _audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
                        _audioSource.Play();
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
                    if (!myClip) yield break;
                    _audioSource.clip = myClip;
                    _audioSource.Play();
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
        if (_webRequest != null && !_webRequest.isDone)
        {
            _webRequest.Abort();
        }
    }
}
