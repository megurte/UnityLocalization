using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Localization
{
    public static class LocalizationProvider
    {
        private static readonly Dictionary<string, string[]> rowsByAlias = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, int> langColumnIndex = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> currentLangCache = new(StringComparer.OrdinalIgnoreCase);

        public static string CurrentLocaleKey { get; private set; }

        private static bool _initialized;

        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            LoadAndParseCsv();
            SetLocalizationKey(LocalizationConstants.DefaultLocale);
        }

        public static void SetLocalizationKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                key = LocalizationConstants.DefaultLocale;

            key = key.Trim().ToUpperInvariant();

            if (CurrentLocaleKey == key)
                return;

            CurrentLocaleKey = key;

            currentLangCache.Clear();

            LocalizationEvents.RaiseLanguageChanged();
        }

        public static string Localize(string alias)
        {
            if (string.IsNullOrEmpty(alias))
                return string.Empty;

            EnsureInitialized();

            if (currentLangCache.TryGetValue(alias, out var cached))
                return cached;

            if (rowsByAlias.TryGetValue(alias, out var row))
            {
                var text = GetTextFromRow(row, CurrentLocaleKey);

                text = string.IsNullOrEmpty(text)
                    ? GetFallbackText(row)
                    : text;

                currentLangCache[alias] = text;

                return text;
            }

            return $"#{alias}";
        }

        public static string Localize(string alias, params string[] args)
        {
            var raw = Localize(alias);

            if (args == null || args.Length == 0)
                return raw;

            try
            {
                return string.Format(raw, args);
            }
            catch (FormatException)
            {
                return raw;
            }
        }

        public static void ForceReload()
        {
            rowsByAlias.Clear();
            langColumnIndex.Clear();
            currentLangCache.Clear();

            LoadAndParseCsv();
            SetLocalizationKey(CurrentLocaleKey ?? LocalizationConstants.DefaultLocale);
        }

        private static void EnsureInitialized()
        {
            if (!_initialized)
                Initialize();
        }

        private static void LoadAndParseCsv()
        {
            var csvFile = Resources.Load<TextAsset>(LocalizationConstants.LocalizationCsvPath);

            if (csvFile == null)
            {
                Debug.LogError(
                    $"[LocalizationProvider] CSV not found at Resources/{LocalizationConstants.LocalizationCsvPath}.csv");
                return;
            }

            var text = csvFile.text;

            if (string.IsNullOrEmpty(text))
                return;

            using var reader = new StringReader(text);

            var header = ReadCsvRow(reader);

            if (header == null || header.Count < 2)
            {
                Debug.LogError(
                    "[LocalizationProvider] CSV header invalid. Expected: alias,<LANG1>,<LANG2>...");
                return;
            }

            for (int i = 1; i < header.Count; i++)
            {
                var lang = header[i]?.Trim();

                if (!string.IsNullOrEmpty(lang))
                    langColumnIndex.TryAdd(lang.ToUpperInvariant(), i);
            }

            if (langColumnIndex.Count == 0)
            {
                Debug.LogError("[LocalizationProvider] No language columns found.");
                return;
            }

            List<string> row;

            while ((row = ReadCsvRow(reader)) != null)
            {
                if (row.Count == 0)
                    continue;

                var alias = (row[0] ?? string.Empty).Trim();

                if (string.IsNullOrEmpty(alias))
                    continue;

                if (row.Count < header.Count)
                {
                    var fixedRow = new string[header.Count];

                    for (int i = 0; i < row.Count; i++)
                        fixedRow[i] = row[i];

                    for (int i = row.Count; i < header.Count; i++)
                        fixedRow[i] = string.Empty;

                    rowsByAlias[alias] = fixedRow;
                }
                else
                {
                    rowsByAlias[alias] = row.ToArray();
                }
            }
        }

        private static string GetTextFromRow(IReadOnlyList<string> row, string localeKey)
        {
            if (row == null || string.IsNullOrEmpty(localeKey))
                return null;

            if (langColumnIndex.TryGetValue(localeKey, out var idx))
            {
                if (idx >= 0 && idx < row.Count)
                    return row[idx];
            }

            if (!string.Equals(localeKey,
                    LocalizationConstants.DefaultLocale,
                    StringComparison.OrdinalIgnoreCase) &&
                langColumnIndex.TryGetValue(
                    LocalizationConstants.DefaultLocale,
                    out var defIdx) &&
                defIdx >= 0 &&
                defIdx < row.Count)
            {
                return row[defIdx];
            }

            return null;
        }

        private static string GetFallbackText(IReadOnlyList<string> row)
        {
            foreach (var kv in langColumnIndex)
            {
                var idx = kv.Value;

                if (idx >= 0 && idx < row.Count)
                {
                    var value = row[idx];

                    if (!string.IsNullOrEmpty(value))
                        return value;
                }
            }

            return row.Count > 0 ? row[0] : string.Empty;
        }

        private static List<string> ReadCsvRow(StringReader reader)
        {
            var line = ReadCsvPhysicalLine(reader);

            if (line == null)
                return null;

            var cells = new List<string>();
            var sb = new StringBuilder();

            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        bool hasNext = i + 1 < line.Length;

                        if (hasNext && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        cells.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            cells.Add(sb.ToString());

            return cells;
        }

        private static string ReadCsvPhysicalLine(TextReader reader)
        {
            var raw = reader.ReadLine();

            if (raw == null)
                return null;

            var sb = new StringBuilder(raw);

            var quotesCount = CountQuotes(raw);

            while (quotesCount % 2 != 0)
            {
                var next = reader.ReadLine();

                if (next == null)
                    break;

                sb.AppendLine();
                sb.Append(next);

                quotesCount += CountQuotes(next);
            }

            return sb.ToString();

            static int CountQuotes(string s)
            {
                var cnt = 0;

                for (var i = 0; i < s.Length; i++)
                {
                    if (s[i] == '"')
                        cnt++;
                }

                return cnt;
            }
        }
    }
}