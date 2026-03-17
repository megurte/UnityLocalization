# Unity CSV Localization

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
| menu_play | Play | Играть | プレイ |
| menu_settings | Settings | Настройки | 設定 |
| card_damage | Damage {0} | Урон {0} | ダメージ {0} |

The system supports formatted arguments using standard string formatting.

Exapmle of table structure in Google Sheets:
// TODO

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

### 1. Create a Localization CSV file

Create a CSV file that contains all localized strings.

Example structure:

```csv
alias,EN,FR,JP
card_damage,Card has {0} damage,La carte a {0} dégâts,カードは{0}ダメージです
start_game,Start Game,Démarrer le jeu,ゲーム開始
```

- `alias` — unique identifier of the text
- other columns represent supported languages
- `{0}`, `{1}`, etc. are placeholders for runtime parameters

#### File location

The CSV file must be placed inside the **Resources** folder:

```
Assets/Resources/Localization/loc.csv
```

By default the system loads the file from:

```
Resources/Localization/loc.csv
```

If you want to change the path, open:

```
LocalizationConstants.cs
```

and modify the value of:

```
LocalizationCsvPath
```

---

### 2. Localize static UI text

To automatically localize **static UI text** when the scene loads add the **LocalizationTextContainer** component to a GameObject with **TextMeshPro** or **TextMeshProUGUI**
In the component, specify the localization key:

```
alias = start_game
```

When the scene loads, the text will be automatically replaced with the localized value.

---

### 3. Localize dynamic text in code

For dynamic text use:

```csharp
LocalizationProvider.Localize()
```

Example CSV entry:

```csv
card_damage,Card has {0} damage,La carte a {0} dégâts
```

C# usage:

```csharp
string text = LocalizationProvider.Localize("card_damage", "10");
```

Result:

```
Card has 10 damage
```

Arguments are automatically inserted into `{0}`, `{1}`, `{2}`, etc.

Example with multiple parameters:

```csharp
LocalizationProvider.Localize("battle_result", playerName, score);
```

CSV:

```csv
battle_result,{0} scored {1} points
```

---

### 4. Change language at runtime

To change the current language:

```csharp
LocalizationProvider.SetLocalizationKey("FR");
```

After calling this method:

- the language changes globally
- all cached localized texts update automatically

Language keys must match the CSV column names.
### Example

```csharp
button.OnClickAsObservable().Subscribe(_ =>
{
    LocalizationProvider.SetLocalizationKey(keyLocal);
    LocalizationProvider.ForceReload();
}).AddTo(DisposeOnDestroy);
```
---

### 5. Get the current language

You can access the current language key using:

```csharp
string locale = LocalizationProvider.CurrentLocaleKey;
```
---

### Example

```csharp
using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializableField] private TextMeshProUGUI damageText;

    public void UpdateDamageText(int damage)
    {
        damageText.text = LocalizationProvider.Localize("card_damage", damage);
    }
}
```
