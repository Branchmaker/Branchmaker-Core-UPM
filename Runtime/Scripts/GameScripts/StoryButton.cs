using BranchMaker.Story;
using System.Collections.Generic;
using BranchMaker;
using UnityEngine;
using UnityEngine.UI;

public class StoryButton : MonoBehaviour
{
    public static List<string> playerkeys = new List<string>();
    public string gotoNode = string.Empty;

    public string needKey = string.Empty;

    bool notFirsttime = false;
    Button btn;
    public BranchNodeBlock designatedAction;

    private void Awake()
    {
        btn = GetComponent<Button>();
        UpdateClickable();
    }
    public void GoToMyNode()
    {
        if (designatedAction != null) StoryManager.PerformAction(designatedAction);
        else StoryManager.LoadNodeKey(gotoNode);
    }

    private void OnEnable()
    {
        UpdateClickable();
    }

    public void UpdateClickable()
    {
        if (!CanBeClicked())
        {
            btn.interactable = false;
            btn.enabled = false;
            GetComponent<Image>().CrossFadeColor(Color.black, 0f, false, false);
        } else
        {
            btn.enabled = true;
            btn.interactable = true;
            if (!notFirsttime)
            {
                SoundeffectsManager.PlayEffect("ClueChime",true,false);
                GetComponent<Image>().CrossFadeColor(Color.black, 0f, false, false);
                GetComponent<Image>().CrossFadeColor(btn.colors.normalColor, 2f, false, false);
                //GetComponent<Image>().CrossFadeAlpha(1f, 2f,false);
                notFirsttime = true;

            }
            else
            {
                GetComponent<Image>().CrossFadeColor(btn.colors.normalColor, 0f, false, false);
            }
        }
    }

    private bool CanBeClicked() {
        if (string.IsNullOrEmpty(needKey)) return true;
        needKey = needKey.ToLower();
        return playerkeys.Contains(needKey);
    } 
}
