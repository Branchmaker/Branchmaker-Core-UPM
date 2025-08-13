using TMPro;
using UnityEngine;

namespace BranchMaker.Addons.OpenDyslexic
{
    public class OpenDyslexicFontReplacer : MonoBehaviour
    {
        private TMP_Text _label;
        private TMP_FontAsset _basicFont;
        private void Awake()
        {
            _label = GetComponent<TMP_Text>();
            if (_label == null) return;
            _basicFont = _label.font;
        }

        private void OnEnable()
        {
            CheckForFontUpdate();
        }

        public void CheckForFontUpdate()
        {
            if (_label == null) return;
            if (OpenDyslexicSetting.ChangeFonts())
            {
                if (_label.font != OpenDyslexicSetting.globalDysFont) _label.font = OpenDyslexicSetting.globalDysFont;
            }
            else
            {
                if (_label.font != _basicFont) _label.font = _basicFont;
            }
        }
    }
}
