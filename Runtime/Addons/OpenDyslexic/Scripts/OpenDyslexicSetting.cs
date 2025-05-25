using UnityEngine;
using UnityEngine.UI;

namespace BranchMaker.Addons.OpenDyslexic
{
    public class OpenDyslexicSetting : MonoBehaviour
    {
        private static bool _enabled;
        public TMPro.TMP_FontAsset openDysFont;
        public static TMPro.TMP_FontAsset globalDysFont;

        private void Awake()
        {
            _enabled = PlayerPrefs.GetInt("DyslexicFont", 0) == 1;
            globalDysFont = openDysFont;
            GetComponent<Toggle>().isOn = _enabled;
        }

        public void ToggleFontSwap(Toggle toggleElement)
        {
            _enabled = toggleElement.isOn;
            PlayerPrefs.SetInt("DyslexicFont", _enabled ? 1 : 0);
            UpdateAllLabels();
        }

        void UpdateAllLabels()
        {
            foreach (var openDyslexicFontReplacer in GameObject.FindObjectsByType<OpenDyslexicFontReplacer>(FindObjectsSortMode.None))
            {
                openDyslexicFontReplacer.CheckForFontUpdate();
            }
        }

        public static bool ChangeFonts()
        {
            return _enabled;
        }
    }
}
