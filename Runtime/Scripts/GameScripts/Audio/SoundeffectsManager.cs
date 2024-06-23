using System.Collections.Generic;
using UnityEngine;

namespace BranchMaker.GameScripts.Audio
{
    public class SoundeffectsManager : MonoBehaviour {
        static List<SoundeffectsManager> soundlibs = new List<SoundeffectsManager>();
        List<AudioSource> sources;
        public AudioClip[] clips;

        Dictionary<string, AudioClip> clipdics = new Dictionary<string, AudioClip>();

        static public float volume = 0.5f;

        public bool isVoiceCharacter;

        public static void forceVolume(float newvolume, float reduction = 1f) {
            volume = newvolume;
            foreach (SoundeffectsManager man in soundlibs)
            {
                foreach (AudioSource sur in man.sources)
                {
                    sur.volume = volume * reduction;
                }
            }
            PlayerPrefs.SetFloat("sfx", volume);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            soundlibs.Clear();
        }

        public static void stopSpeech() {

            foreach (SoundeffectsManager man in soundlibs)
            {
                if (!man.isVoiceCharacter) continue;
                foreach (AudioSource source in man.sources)
                {
                    if (source == null) continue;
                    if (source.isPlaying)
                    {
                        source.Stop();
                    }
                }
            }
        }

        // Use this for initialization
        void Awake () {

            if (clips.Length == 0) return;
            if (!soundlibs.Contains(this)) soundlibs.Add(this);
            //ToggleSettingBox.preloadSettings();
            sources = new List<AudioSource>(GetComponents<AudioSource>());
            forceVolume(PlayerPrefs.GetFloat("sfx", 0.5f));
        
            foreach (AudioSource sur in sources) {
                sur.volume = volume;
            }

            if (clips == null) return;
            foreach (AudioClip clip in clips)
            {
                if (clip == null) continue;
                if (clipdics.ContainsKey(clip.name)) continue;
                clipdics.Add(clip.name, clip);
            }

        }
        /*
    public static void preload(AudioClip clip)
    {
        if (clipdics.ContainsKey(clip.name)) return;
        clipdics.Add(clip.name, clip);
    }
    */

        /*
    static public bool playingSound()
    {
        foreach (AudioSource source in sources)
        {
            if (source.isPlaying) return true;
        }
        return false;
    }
    */

        // Update is called once per frame
        static public void PlayEffect (string key,bool jiggleSound = true, bool playStacked = true)
        {
            foreach (SoundeffectsManager man in soundlibs)
            {
                if (!man.gameObject.activeInHierarchy) continue;
                if (!man.clipdics.ContainsKey(key)) continue;

                if (!playStacked)
                {
                    foreach (AudioSource source in man.sources)
                    {
                        if (!source.isPlaying) continue;
                        if (source.clip == man.clipdics[key]) return;
                    }
                }


                foreach (AudioSource source in man.sources)
                {
                    if (source == null) continue;
                    if (source.isPlaying) continue;
                    if (!source.enabled) continue;
                    source.clip = man.clipdics[key];
                    source.volume = volume;
                    if (jiggleSound) source.pitch = 1f + (Random.Range(-100, 100) * 0.001f);
                    else source.pitch = 1f;
                    //if (key.Contains("zen_")) source.volume = 1f;
                    source.Play();
                    return;
                }
            }
        }
        /*
    static public void StopEffect(string key)
    {
        if (sources == null) return;
        if (!clipdics.ContainsKey(key)) return;

        foreach (AudioSource source in sources)
        {
            if (!source.isPlaying) continue;
            if (source.clip == clipdics[key]) {
                source.Stop();
                return; }
        }
    }


    static public void PlayClip(AudioClip clip, bool jiggleSound = true)
    {
        if (clip == null) return;
        if (sources == null) return;
        if (sources.Count == 0) return; 
        sources.RemoveAll(a => a == null);
        foreach (AudioSource source in sources)
        {
            if (source.isPlaying) continue;
            source.clip = clip;
            source.volume = volume;
            if (jiggleSound) source.pitch = 1f + (Random.Range(-100, 100) * 0.001f);
            else source.pitch = 1f;
            source.Play();
            return;
        }
    }
    */
    }
}
