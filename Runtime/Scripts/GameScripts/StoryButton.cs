using BranchMaker.Story;
using System.Collections.Generic;
using BranchMaker;
using UnityEngine;
using UnityEngine.UI;

public class StoryButton : MonoBehaviour
{
    public static List<string> playerkeys = new();
    public string gotoNode = string.Empty;

    public string needKey = string.Empty;

    bool notFirsttime = false;
    private Button _btn;
    private Image _image;
    public BranchNodeBlock designatedAction;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.RemoveAllListeners();
        _btn.onClick.AddListener(GoToMyNode);
        _image = GetComponent<Image>();
        UpdateClickable();
    }

    private void GoToMyNode()
    {
        if (designatedAction != null)
        {
            StoryEventManager.ParseBlockscript(designatedAction);
            StoryManager.LoadNodeKey(designatedAction.target_node);
        }
        else Debug.Log("Missing action for "+gotoNode);
    }

    private void OnEnable() => UpdateClickable();

    public void UpdateClickable()
    {
        if (!CanBeClicked())
        {
            _btn.interactable = false;
            _btn.enabled = false;
            _image.CrossFadeColor(Color.black, 0f, false, false);
        } else
        {
            _btn.enabled = true;
            _btn.interactable = true;
            if (!notFirsttime)
            {
                SoundeffectsManager.PlayEffect("ClueChime",true,false);
                _image.CrossFadeColor(Color.black, 0f, false, false);
                _image.CrossFadeColor(_btn.colors.normalColor, 2f, false, false);
                notFirsttime = true;
            }
            else
            {
                _image.CrossFadeColor(_btn.colors.normalColor, 0f, false, false);
            }
        }
    }

    private bool CanBeClicked() {
        if (string.IsNullOrEmpty(needKey)) return true;
        needKey = needKey.ToLower();
        return playerkeys.Contains(needKey);
    } 
}
