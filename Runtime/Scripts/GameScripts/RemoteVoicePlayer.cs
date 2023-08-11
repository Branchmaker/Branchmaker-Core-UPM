using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class RemoteVoicePlayer : MonoBehaviour
{
    private static RemoteVoicePlayer _player;
    private UnityWebRequest _webRequest;
    private AudioSource _audioSource;
    private void Awake()
    {
        _player = this;
        _audioSource = GetComponent<AudioSource>();
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
        _webRequest = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
        _webRequest.SendWebRequest();

        while (!_webRequest.isDone)
        {
            // Check for errors during streaming
            if (_webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error streaming audio from " + _webRequest.url);
                Debug.Log(_webRequest.error);
                yield break;
            }

            // Process the downloaded audio data
            var audioHandler = (DownloadHandlerAudioClip)_webRequest.downloadHandler;
            var audioClip = audioHandler.audioClip;

            // Make sure the audio source is set and ready
            if (_audioSource.clip == null)
            {
                _audioSource.clip = AudioClip.Create("StreamingAudioClip", audioClip.samples, audioClip.channels, audioClip.frequency, false);
            }

            // Get the samples from the downloaded audio clip
            var samples = new float[audioClip.samples * audioClip.channels];
            audioClip.GetData(samples, 0);

            // Stream the samples to the audio source
            _audioSource.clip.SetData(samples, _audioSource.timeSamples);
            _audioSource.Play();

            // Wait for a short time before streaming the next portion
            yield return new WaitForSecondsRealtime(0.1f);
        }
    }
}
