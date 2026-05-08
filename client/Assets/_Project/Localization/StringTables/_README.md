# String tables

Slice 0 ships an empty Localization folder. Once Unity Editor is open and the
`com.unity.localization` package is imported:

1. Window → Asset Management → Localization Tables → Create New
2. Pick this folder (`_Project/Localization/StringTables`).
3. Add a `String Table Collection` named **`UI`** with English (en) locale.
4. Seed these keys for slice 0 (Login screen):

| Key                            | English value                                  |
|--------------------------------|------------------------------------------------|
| `login.idle`                   | Sign in to begin                                |
| `login.guest.in_progress`      | Signing in as guest…                            |
| `login.google.unavailable`     | Google sign-in is not available in this build  |
| `login.error`                  | Login failed. Please try again.                 |

Adding more keys per slice:

- Combat strings → `Combat` table
- Inventory → `Inventory` table
- Always reference via `IdleMmo.Shared.Localization.LocKey`. Never hardcode user-facing text.
