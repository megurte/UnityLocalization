using TMPro;
using UnityEngine;

namespace Localization
{
    [DisallowMultipleComponent]
    public class LocalizationTextContainer : MonoBehaviour
    {
        [Tooltip("Key alias in CSV")]
        public string alias;

        [Tooltip("Arguments for alias {0}, {1}, ...")]
        public string[] args;

        [SerializeField, HideInInspector] private TMP_Text label;

        public TMP_Text Label => label ? label : label = GetComponent<TMP_Text>();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!label) label = GetComponent<TMP_Text>();
        }
#endif
    }
}