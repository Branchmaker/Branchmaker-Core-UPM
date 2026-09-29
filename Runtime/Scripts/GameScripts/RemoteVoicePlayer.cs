using System.Collections;
using BranchMaker.Utility;
using UnityEngine;
using UnityEngine.Networking;

namespace BranchMaker.GameScripts
{
    [RequireComponent(typeof(AudioSource))]
    public class RemoteVoicePlayer : BaseController<RemoteVoicePlayer>
    {
        private UnityWebRequest _webRequest;
        protected AudioSource AudioSource;

        protected override void Awake()
        {
            base.Awake();
            AudioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            StoryManager.Instance.OnBlockChange.AddListener(ProcessBlock);
        }

        protected virtual void ProcessBlock(BranchNodeBlock block)
        {
            StopSpeaking();
            if (!string.IsNullOrEmpty(block.voice_file)) PlayRemoteOgg(block.voice_file);
        }

        protected virtual void PlayRemoteOgg(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return;
            if (uri.EndsWith(".jpg") || uri.EndsWith(".jpeg")) return;
            if (!uri.ToLower().EndsWith(".mp3") && !uri.ToLower().EndsWith(".ogg") &&
                !uri.ToLower().EndsWith(".ogx")) return;
            StartCoroutine(PlayFile(uri));
        }

        public virtual void StopSpeaking()
        {
            StopAllCoroutines();
            GetComponent<AudioSource>().Stop();
        }

        protected virtual IEnumerator PlayFile(string path)
        {
            const int maxAttempts = 3; // Initial attempt + 2 retries
            const float retryDelay = 0.5f;

            if (IsHLSFormat(path))
            {
                for (var attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    _webRequest = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN);

                    yield return _webRequest.SendWebRequest();

                    if (_webRequest.result == UnityWebRequest.Result.Success)
                    {
                        var audioClip = DownloadHandlerAudioClip.GetContent(_webRequest);

                        if (audioClip)
                        {
                            AudioSource.clip = audioClip;
                            AudioSource.Play();
                            yield break;
                        }
                    }

                    Debug.LogWarning(
                        $"Failed to load voice file '{path}' " +
                        $"(attempt {attempt}/{maxAttempts}): {_webRequest.error}"
                    );

                    _webRequest.Dispose();
                    _webRequest = null;

                    if (attempt < maxAttempts)
                        yield return new WaitForSeconds(retryDelay);
                }

                Debug.LogError($"Could not load voice file '{path}' after {maxAttempts} attempts.");
                yield break;
            }

            var audioType = GetAudioTypeFromPath(path);

#if UNITY_WEBGL
    if (audioType == AudioType.OGGVORBIS)
        yield break;
#endif

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                using (var www = UnityWebRequestMultimedia.GetAudioClip(path, audioType))
                {
                    if (audioType == AudioType.MPEG)
                    {
                        var downloadHandler = new DownloadHandlerAudioClip(
                            string.Empty,
                            AudioType.MPEG
                        );

                        downloadHandler.streamAudio = true;
                        www.downloadHandler = downloadHandler;
                    }

                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        var audioClip = DownloadHandlerAudioClip.GetContent(www);

                        if (audioClip)
                        {
                            AudioSource.clip = audioClip;
                            AudioSource.Play();
                            yield break;
                        }
                    }

                    Debug.LogWarning(
                        $"Failed to load voice file '{path}' " +
                        $"(attempt {attempt}/{maxAttempts}): {www.error}"
                    );
                }

                if (attempt < maxAttempts)
                    yield return new WaitForSeconds(retryDelay);
            }

            Debug.LogError($"Could not load voice file '{path}' after {maxAttempts} attempts.");
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
            if (_webRequest != null && !_webRequest.isDone) _webRequest.Abort();
        }
    }
}