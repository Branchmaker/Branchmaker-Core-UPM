using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.UI
{
    
public class DialogueButton : MonoBehaviour
{
    public TMPro.TextMeshPro TMLabel;
    public void SetLabel(string newLabel)
    {
        if (TMLabel != null) TMLabel.text = newLabel;
        if (GetComponent<Text>() != null) GetComponent<Text>().text = newLabel;
    }

}

}