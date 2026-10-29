# Build & Run

How to open, play, test and ship Astro Idle. Written for someone new to Unity.

- [1. Prerequisites (one-time)](#1-prerequisites-one-time)
- [2. Open the project (first time)](#2-open-the-project-first-time)
- [3. Play in the editor](#3-play-in-the-editor)
- [4. Run the unit tests](#4-run-the-unit-tests)
- [5. Build to an Android phone](#5-build-to-an-android-phone)
- [6. Day-to-day tips](#6-day-to-day-tips)
- [7. Troubleshooting](#7-troubleshooting)

---

## 1. Prerequisites (one-time)

| Need | Why | How |
|------|-----|-----|
| **Unity 6000.4.6f1** | The version this project is pinned to (`ProjectSettings/ProjectVersion.txt`). | Already installed via Unity Hub. |
| **Android Build Support** module | Required to build to a phone (not needed just to play in the editor). | Unity Hub → **Installs** → gear icon on *6000.4.6f1* → **Add modules** → tick **Android Build Support** *and its children* **OpenJDK** and **Android SDK & NDK Tools** → Install. |
| An Android phone + USB cable | To test on a device. | See [section 5](#5-build-to-an-android-phone). |
| (Optional) Visual Studio / Rider / VS Code | Editing C#. | Unity → **Edit → Preferences → External Tools → External Script Editor**. |

## 2. Open the project (first time)

1. Unity Hub → **Projects** → **Add** (dropdown) → **Add project from disk** → choose this repository's root folder
   (the one containing `Assets/`, `Packages/` and `ProjectSettings/`).
2. Click the project to open it. The first import takes a few minutes while Unity builds its `Library/` cache.
3. **Input System prompt:** Unity asks whether to enable the new Input System backends. Click **Yes**. The editor
   restarts. (The game's UI works with either input backend; this just avoids a warning.)
4. **Automatic setup:** once scripts compile, an editor script (`ProjectBootstrapper`) runs once and:
   - creates `Assets/_Project/Scenes/Main.unity` (a camera plus the `GameBootstrap` object),
   - puts that scene first in the build scene list,
   - sets the app name, Android package id (`com.b2darice.astroidle`), portrait orientation and IL2CPP/ARM64.

   The Console shows `[Astro Idle] Created Assets/_Project/Scenes/Main.unity ...`. If you missed it, run
   **Astro Idle → Setup → Run Project Setup** from the menu bar at any time. It's safe to run repeatedly.
5. **Commit what Unity generated.** Unity creates `.meta` files for every asset and folder, plus files in
   `ProjectSettings/`. These belong in git (the `.gitignore` already excludes `Library/`, `Temp/`, etc.):

   ```bash
   git add -A
   ```

   ```bash
   git commit -m "Initial Unity import: generated meta files, settings and main scene"
   ```

## 3. Play in the editor

1. Open the scene: **Astro Idle → Open Main Scene** (or double-click `Assets/_Project/Scenes/Main.unity`).
2. Make the Game view look like a phone. Either:
   - **Recommended:** in the Game view's top-left dropdown switch from **Game** to **Simulator**, then pick a phone
     (e.g. a Pixel or Galaxy). This also simulates the notch / safe area. Or:
   - stay in **Game** view, open the resolution dropdown, click **+** and add a fixed **1080 x 2400** resolution.
3. Press **Play** (▶ at the top of the editor).

What you should see: a starfield with your cyan ship, asteroids drifting in, green bolts firing automatically, a HUD at
the top (credits, wave, hull) and the upgrade panel at the bottom with **SHIP / WEAPONS / GOALS / DEV** tabs.

The **DEV** tab only appears in the editor and in *Development Builds*. It can grant credits, skip waves, speed up the
game (x2/x4/x8) and reset all progress. Use it to test balance quickly.

**Save data in the editor:** progress is saved to your user profile (`Application.persistentDataPath`) and survives
play sessions. To start fresh, use DEV → *Reset ALL progress* while playing, or **Astro Idle → Save Data → Delete Save**
while stopped.

## 4. Run the unit tests

1. **Window → General → Test Runner**.
2. Select the **EditMode** tab → **Run All**.

Tests live in `Assets/_Project/Tests/EditMode/` ([README](../Assets/_Project/Tests/README.md)). They cover the number
formatter, cost and stat formulas, offline earnings, wave scaling, the spawn table, save serialization, and a
validation pass over all shipped content.

You can also check content without the test runner: **Astro Idle → Validate Content**.

## 5. Build to an Android phone

### Phone setup (one-time)
1. On the phone: **Settings → About phone** → tap **Build number** 7 times to enable Developer options.
2. **Settings → System → Developer options** → enable **USB debugging**.
3. Plug the phone into the PC. When the phone asks *"Allow USB debugging?"*, tick *Always allow* and tap **Allow**.

### Build & install
1. In Unity: **File → Build Profiles**.
2. Select **Android** in the platform list. If it's not active yet, click **Switch Platform** and wait for the
   re-import (only needed once).
3. Check that **Scene List** contains `Assets/_Project/Scenes/Main` (the setup script adds it).
4. For test builds, tick **Development Build**. This enables the DEV tab and better error logs. Untick it for a
   release build.
5. Under **Run Device**, pick your phone (click **Refresh** if it isn't listed).
6. Click **Build And Run**. Choose a location for the `.apk` (e.g. a `Builds/` folder in the repo, which is
   gitignored). Unity builds, installs and launches the game on the phone.

The first IL2CPP build takes several minutes. Later builds are faster. Builds use **IL2CPP + ARM64**, which
Google Play requires. To iterate faster on device you can temporarily switch *Player Settings → Other Settings →
Scripting Backend* to **Mono**, but switch back to IL2CPP before any release.

### Reading logs from the device
With a *Development Build*, use **Window → Analysis → Android Logcat** (install the *Android Logcat* package from the
Package Manager if the menu is missing) to see `Debug.Log` output and errors from the phone.

## 6. Day-to-day tips

- **Tuning balance:** select any asset in `Assets/_Project/Resources/Content/` and edit its numbers in the
  Inspector. Changes made **during Play mode are reverted when you stop**, so note the values you like, or edit
  while stopped. See the [content guide](../Assets/_Project/Resources/Content/README.md).
- **Fast-forward:** DEV tab → *Game speed* (x8 makes reaching wave 10 quick).
- **Offline earnings:** shown when you return after more than 2 minutes away. To test in the editor, stop Play
  mode, wait 2+ minutes, then press Play again.

## 7. Troubleshooting

| Symptom | Fix |
|---------|-----|
| Console: `[Content] No ShipDefinition found...` or assets show *"The associated script can not be loaded"* | A ScriptableObject script's `.meta` GUID changed. The `.cs.meta` files in `Scripts/Runtime/Content/` must keep their committed GUIDs; restore them from git. Then run **Astro Idle → Validate Content**. |
| No `Main.unity` scene / scene missing from build | **Astro Idle → Setup → Run Project Setup**. |
| Pressing Play shows only a blue/grey screen | You're in an empty scene. Open **Astro Idle → Open Main Scene**. |
| Buttons don't respond to clicks | Make sure there's no second EventSystem in the scene; the game creates its own. |
| Package Manager error resolving `com.unity.inputsystem` or `com.unity.ide.visualstudio` | Open **Window → Package Manager**, select the package, click **Update** to the version it recommends. |
| Phone not listed under Run Device | Re-plug the cable, accept the USB-debugging prompt on the phone, set USB mode to *File transfer*, click **Refresh**. |
| Build fails mentioning SDK/NDK/JDK | Re-run the Unity Hub module install making sure *OpenJDK* and *Android SDK & NDK Tools* are ticked. |
| Want a totally clean start | Delete the save (above). If Unity misbehaves, close it and delete the `Library/` folder; Unity rebuilds it. |
