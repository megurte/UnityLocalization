using System;

namespace Localization
{
    public static class LocalizationEvents
    {
        public static event Action LanguageChanged;

        public static void RaiseLanguageChanged()
        {
            LanguageChanged?.Invoke();
        }
    }
}