using TMPro;
using UnityEngine;

namespace BranchMaker.Addons.OpenDyslexic
{
    public class OpenDyslexicFontReplacer : MonoBehaviour
    {
        private TMPro.TextMeshProUGUI label;
        private TMPro.TMP_FontAsset basicFont;
        private void Awake()
        {
            label = GetComponent<TextMeshProUGUI>();
            if (label == null) return;
            basicFont = label.font;
        }

        private void OnEnable()
        {
            CheckForFontUpdate();
        }

        public void CheckForFontUpdate()
        {
            if (label == null) return;
            if (OpenDyslexicSetting.ChangeFonts())
            {
                if (label.font != OpenDyslexicSetting.globalDysFont) label.font = OpenDyslexicSetting.globalDysFont;
            }
            else
            {
                if (label.font != basicFont) label.font = basicFont;
            }
        }
    }
}
