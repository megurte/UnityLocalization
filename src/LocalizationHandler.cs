using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Localization
{
    [DefaultExecutionOrder(-500)]
    public class LocalizationHandler : MonoBehaviour
    {
        [Header("Strategy")]
        public bool includeInactive = true;
        public bool checkAllTextInSceneForAliases = false;
        public bool verboseLogs = true;

        private readonly List<Entry> _entries = new(256);

        private struct Entry
        {
            public TMP_Text Label;
            public string Alias;
            public string[] Args;
        }

        private static LocalizationHandler _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;

            DontDestroyOnLoad(gameObject);

            LocalizationProvider.Initialize();

            SceneManager.sceneLoaded += OnSceneLoaded;
            LocalizationEvents.LanguageChanged += ApplyAll;

            CacheAll();
            ApplyAll();
        }

        private void OnDestroy()
        {
            if (_instance != this)
                return;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            LocalizationEvents.LanguageChanged -= ApplyAll;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            CacheAll();
            ApplyAll();
        }

        public void CacheAll()
        {
            _entries.Clear();

            var localized = FindObjectsOfType<LocalizationTextContainer>(includeInactive);

            foreach (var lt in localized)
            {
                if (lt == null || lt.Label == null)
                    continue;

                var alias = lt.alias?.Trim();

                if (string.IsNullOrEmpty(alias))
                {
                    if (verboseLogs)
                        Debug.LogWarning($"[LocalizationHandler] '{lt.name}' has LocalizedText but empty Alias");

                    continue;
                }

                _entries.Add(new Entry
                {
                    Label = lt.Label,
                    Alias = alias,
                    Args = lt.args
                });
            }

            if (checkAllTextInSceneForAliases)
            {
                var allTexts = FindObjectsOfType<TMP_Text>(includeInactive);

                foreach (var t in allTexts)
                {
                    if (t == null)
                        continue;

                    if (t.GetComponent<LocalizationTextContainer>() != null)
                        continue;

                    var alias = t.text?.Trim();

                    if (string.IsNullOrEmpty(alias))
                        continue;

                    _entries.Add(new Entry
                    {
                        Label = t,
                        Alias = alias,
                        Args = null
                    });
                }
            }
        }

        public void ApplyAll()
        {
            foreach (var entry in _entries)
            {
                if (!entry.Label)
                    continue;

                string text;

                if (entry.Args != null && entry.Args.Length > 0)
                {
                    text = LocalizationProvider.Localize(entry.Alias, entry.Args);
                }
                else
                {
                    text = LocalizationProvider.Localize(entry.Alias);
                }

                if (string.IsNullOrEmpty(text) || text.StartsWith("#"))
                {
                    if (verboseLogs)
                        Debug.LogWarning(
                            $"[LocalizationHandler] Missing alias '{entry.Alias}' for '{entry.Label.name}'");
                }

                entry.Label.text = text;
            }
        }

        public static void LocalizeAll()
        {
            if (_instance == null)
                return;

            _instance.CacheAll();
            _instance.ApplyAll();
        }
    }
}