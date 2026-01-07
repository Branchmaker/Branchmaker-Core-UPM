using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.UI
{
    
public class DialogueButton : MonoBehaviour
{
    public TMPro.TextMeshProUGUI TMLabel;
    public virtual void SetLabel(string newLabel, BranchNodeBlock fromBlock)
    {
        if (TMLabel != null) TMLabel.text = newLabel;
        if (GetComponent<Text>() != null) GetComponent<Text>().text = newLabel;
    }

}

}