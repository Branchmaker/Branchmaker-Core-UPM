using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.UI
{
    
public class DialogueButton : MonoBehaviour
{
    public void SetLabel(string newLabel)
    {
        if (GetComponent<TMPro.TextMeshPro>() != null) GetComponent<TMPro.TextMeshPro>().text = newLabel;
        if (GetComponent<Text>() != null) GetComponent<Text>().text = newLabel;
    }

}

}