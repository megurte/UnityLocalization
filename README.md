# UnityLocalization

UnityLocalization is a lightweight localization system for Unity built around a simple concept: alias-based translations stored in a CSV file.

The system focuses on simplicity, performance, and easy integration into existing Unity projects. It automatically detects UI elements on scene load and applies translations using a centralized localization provider.

---

## Features

- CSV-based localization workflow
- Alias-based translation keys
- Automatic localization on scene load
- Instant language switching at runtime
- Works with TextMeshPro
- No dependency on Unity Localization Package
- Lightweight and fast
- Can treat existing UI text as a localization alias
- Global static localization provider accessible from anywhere in code
- Easy integration into existing projects

---

## How It Works

Localization entries are defined in a CSV table.

Each row represents a localization alias and each column represents a language.

Example:

| alias | EN | RU | JP |
|------|------|------|------|
| MENU_PLAY | Play | Играть | プレイ |
| MENU_SETTINGS | Settings | Настройки | 設定 |
| CARD_DAMAGE | Damage {0} | Урон {0} | ダメージ {0} |

The system supports formatted arguments using standard string formatting.

Exapmle of table structure in Google Sheets:


Example:

```csharp
LocalizationProvider.Localize("card_damage", "10");
```

## Installation

UPM
- Install via Unity Package Manager (Git URL)
- Open Unity Package Manager.
- Select Add package from git URL
- Then paste: ```https://github.com/megurte/UnityLocalization.git#latest```

## Usage

1. Create a Localization CSV
Place the CSV file inside of `Resource` folder with directory: `Localization/loc` where loc is a csv file.
You can specify path to your loc file by typing path to `LocalizationConstants` file and `LocalizationCsvPath` field.
