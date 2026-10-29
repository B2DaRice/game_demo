# Editor (`AstroIdle.Editor`)

Editor-only tooling (assembly `AstroIdle.Editor`, excluded from player builds). Adds the **Astro Idle** menu to
Unity's menu bar.

| File | Purpose |
|------|---------|
| `ProjectBootstrapper.cs` | `[InitializeOnLoad]`. Once per editor session: creates `Assets/_Project/Scenes/Main.unity` if missing (Main Camera + `Game` object with `GameBootstrap`, camera reference assigned), makes it the first build scene, and applies player settings if they're still Unity defaults (company `B2DaRice`, product `Astro Idle`, package `com.b2darice.astroidle`, version `0.1.0`, portrait, IL2CPP, ARM64). |
| `AstroIdleMenu.cs` | Menu items listed below. |

## Menu

| Menu item | What it does |
|-----------|--------------|
| **Astro Idle → Setup → Run Project Setup** | Re-runs the bootstrapper and **force**-applies player settings. Safe to repeat. |
| **Astro Idle → Open Main Scene** | Opens `Main.unity` (prompts to save the current scene). |
| **Astro Idle → Validate Content** | Runs `ContentDatabase.Validate()` and logs results to the Console. |
| **Astro Idle → Save Data → Delete Save** | Deletes the local save (only while not playing). |
| **Astro Idle → Save Data → Open Save Folder** | Reveals `Application.persistentDataPath`. |

## How to...

**Change the app name / package id / version:** edit the constants at the top of `ProjectBootstrapper.cs`, then run
**Run Project Setup**. Or edit them directly in *Edit → Project Settings → Player* (and update the constants to match).
The package id must stay the same once the app is published on Google Play.

**Add a menu item:** add a `[MenuItem("Astro Idle/…")]` static method to `AstroIdleMenu.cs` and list it in the table
above.
