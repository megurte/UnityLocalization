using UnityEngine;

namespace Localization
{
    public static class LocalizationBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            var go = new GameObject("[Localization]");
            go.AddComponent<LocalizationHandler>();
            Object.DontDestroyOnLoad(go);
        }
    }
}