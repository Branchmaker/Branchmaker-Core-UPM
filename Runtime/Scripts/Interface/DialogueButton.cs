using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.Interface
{
    [RequireComponent(typeof(Button))]
    public abstract class DialogueButton : MonoBehaviour
    {
        public TextMeshProUGUI TMLabel;
        public Button button;

        private void Awake()
        {
            button ??= GetComponent<Button>();
        }

        public virtual void SetLabel(string newLabel, BranchNodeBlock fromBlock)
        {
            if (TMLabel != null) TMLabel.text = newLabel;
            if (GetComponent<Text>() != null) GetComponent<Text>().text = newLabel;
        }
    }
}