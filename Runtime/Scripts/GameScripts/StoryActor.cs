using System.Collections;
using System.Collections.Generic;
using BranchMaker.Actors;
using UnityEngine;
using UnityEngine.UI;

public class StoryActor : MonoBehaviour
{
    public static Dictionary<string, StoryActor> actorpool = new Dictionary<string, StoryActor>();

    public ActorObject sockPuppet;
    public string actorKey;
    static Color unspokenColor = new Color(0.3f, 0.3f, 0.3f,1f);

    static StoryActor currentlySpeaking;

    static float speakFadeSpeed = 0.3f;

    public List<StoryActor> blocksOutActors = new List<StoryActor>();

    public RawImage rawImage;

    Image characterImage;

    private void Awake()
    {
        if (string.IsNullOrEmpty(actorKey)) return;
        if (!actorpool.ContainsKey(actorKey)) actorpool.Add(actorKey,this);
        actorpool[actorKey] = this;
        characterImage = GetComponent<Image>();
        characterImage.enabled = false;
        if (rawImage != null) rawImage.enabled = false;
    }

    private void OnDisable()
    {
        if (characterImage != null) characterImage.enabled = false;
        currentlySpeaking = null;
    }

    private void OnEnable()
    {
        if (characterImage != null) characterImage.CrossFadeColor(UnspokenColorTint(), 0f, false, false);
        if (string.IsNullOrEmpty(actorKey)) return;
        if (!actorpool.ContainsKey(actorKey)) actorpool.Add(actorKey, this);
        actorpool[actorKey] = this;
    }

    Color UnspokenColorTint() {
        if (sockPuppet == null) return Color.black;
        var tinted = sockPuppet.themeColor * 0.3f;

        var f = 0.5f; // desaturate by 20%
        var L = 0.3f * tinted.r + 0.6f * tinted.g + 0.1f * tinted.b;
        var newR = tinted.r + f * (L - tinted.r);
        var newG = tinted.g + f * (L - tinted.g);
        var newB = tinted.b + f * (L - tinted.b);

        return new Color(newR,newG,newB);
    }

    public static void FadeInSpeaker(StoryActor newshown)
    {
        currentlySpeaking = newshown;
        foreach (var blocked in newshown.blocksOutActors)
        {
            blocked.characterImage.enabled = false;
        }

        if (newshown.characterImage != null)
        {
            newshown.characterImage.enabled = true;
            newshown.characterImage.CrossFadeColor(Color.white, speakFadeSpeed, false, false);
        }
    }

    public static void ShowSpeaker(string actor)
    {
        if (!actorpool.ContainsKey(actor)) return;
        FadeInSpeaker(actorpool[actor]);
    }
    public static void NewSpeaker(string actor)
    {
        if (currentlySpeaking != null) {

            if (currentlySpeaking.actorKey == actor) return;
        }
        if (!actorpool.ContainsKey(actor)) return;

        if (currentlySpeaking != null)
        {
            if (currentlySpeaking.gameObject.activeInHierarchy)
            {
                if (currentlySpeaking.characterImage.enabled)
                {
                    currentlySpeaking.characterImage.CrossFadeColor(currentlySpeaking.UnspokenColorTint(), speakFadeSpeed, false, false);
                }
            }
        }
        
        FadeInSpeaker(actorpool[actor]);

    }
}
