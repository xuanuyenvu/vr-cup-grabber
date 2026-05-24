# VR Cup Grabber

A Unity VR experiment for a wine/tea cup-grabbing user study on Meta Quest.
Participants rate cup appearances and answer Presence Questionnaire (PQ) items via in-VR Visual Analogue Scale (VAS) forms.

Built with **Unity 2022.3.58f1**, **Meta XR SDK 72**, and **URP 14**.

---

## Table of Contents

- [Requirements](#requirements)
- [Setup](#setup)
- [Scenes](#scenes)
- [User Study Flow](#user-study-flow)
- [CSV Output](#csv-output)
- [Project Structure](#project-structure)
- [Git Notes](#git-notes)
- [Troubleshooting](#troubleshooting)

---

## Requirements

| Component              | Version / Name       |
| ---------------------- | -------------------- |
| Unity Editor           | 2022.3.58f1          |
| Render Pipeline        | URP 14.0.11          |
| Meta XR SDK            | 72.0.0               |
| XR Interaction Toolkit | 2.6.3                |
| Target Platform        | Android (Meta Quest) |

### External Package

The local package `com.meta.movement` is referenced via a relative path:

```json
"com.meta.movement": "file:../Unity-Movement-72.0.0"
```

Place the `Unity-Movement-72.0.0` folder as a **sibling** of the project root:

```
├── vr-cup-grabber/          ← this repo
├── Unity-Movement-72.0.0/   ← local package
```

---

## Setup

1. Clone this repository.
2. Open **Unity Hub** → **Add project from disk** → select `vr-cup-grabber/`.
3. Ensure **Unity 2022.3.58f1** is installed (Hub will prompt if missing).
4. Ensure the `Unity-Movement-72.0.0` sibling folder exists (see [Requirements](#requirements)).
5. Let Unity restore all packages.
6. Open a main scene (see [Scenes](#scenes)).
7. Switch build target to **Android** (File → Build Settings → Android → Switch Platform).
8. Build and run on Meta Quest.

> **Note:** The project uses the **Universal Render Pipeline (URP)**. If you see pink materials, verify URP assets are assigned in `Edit → Project Settings → Graphics`.

---

## Scenes

### Main (Build)

| Scene                               | Purpose                 |
| ----------------------------------- | ----------------------- |
| `Assets/Scenes/Neutral Scene.unity` | Primary study scene     |
| `Assets/Scenes/Royal Scene.unity`   | Alternative study scene |

### Development / Utility

| Scene                                    | Purpose                           |
| ---------------------------------------- | --------------------------------- |
| `Assets/Scenes/Garden/GardenScene.unity` | Environment preview (Garden)      |
| `Assets/Scenes/Terminal/Terminal.unity`  | Scene teleport hub                |
| `Assets/Scenes/LiquidScene.unity`        | Liquid rendering test             |
| `Assets/Scenes/TCP Client.unity`         | Odor-delivery TCP backend testing |

---

## User Study Flow

### Experiment Types

The enum `UserStudyFormManager.ExperimentType` defines three modes:

1. **ColorTaste** — Rate cup appearance + taste.
2. **ColorTastePureWater** — Same as above, with plain water as control.
3. **VisualFruitScentPureWater** — Fruit scent + visual pairing, no taste.

### Questionnaire

| Section | Items             | Scale     | Order          |
| ------- | ----------------- | --------- | -------------- |
| Q1      | Like / Dislike    | −50 to 50 | Always first   |
| Taste   | 5 taste VAS items | 0 to 100  | **Randomized** |
| PQ      | 10 Presence items | 0 to 100  | Fixed sequence |

- **VAS (Visual Analogue Scale):** participants drag a slider along a bar.
- **Taste questions** are shuffled each trial.
- **PQ questions** stay in a fixed order.

### CSV Output

After each session a CSV file is saved to:

```
Assets/UserData/
```

This folder is **ignored by Git** to avoid uploading local study data.

See [CSV Output](#csv-output) for the schema.

---

## CSV Output

### Naming

Files follow the pattern `{experiment}_{participant}_pq.csv` (e.g. `CT_P01_pq.csv`).

### Header Columns

```
Q1,Q2,Q3,Q4,Q5,Q6,PQ1,PQ2,PQ3,PQ4,PQ5,PQ6,PQ7,PQ8,PQ9,PQ10
```

- **Q1:** Like/dislike (−50 to 50)
- **Q2–Q6:** Taste VAS items (0 to 100)
- **PQ1–PQ10:** Presence Questionnaire VAS items (0 to 100)

---

## Project Structure

```
Assets/
├── Scenes/
│   ├── Neutral Scene.unity          ← Main study scene
│   ├── Royal Scene.unity            ← Alternative scene
│   ├── Garden/                      ← Garden environment
│   ├── Terminal/                    ← Teleport hub
│   └── ...
├── Scripts/
│   ├── UserStudyFormManager.cs      ← Main study logic
│   ├── DissolveController.cs        ← Cup dissolve effect
│   ├── MarkerRigCalibrator.cs       ← Rig calibration
│   └── ScriptableObjects/
│       └── MarkerRigOffsetData.cs
├── Data/
│   └── ScriptableObjects/           ← Config assets
├── Materials/
├── Models/
├── Shaders/
├── StreamingAssets/                 ← Odor config (JSON)
└── UserData/                        ← CSV output (git ignored)

Packages/
└── manifest.json                    ← Package dependencies
```

---

## Git Notes

### Ignored (local‑only)

These files stay on your machine and are **not pushed** to the repo:

```text
.idea/                    # JetBrains Rider settings
.vscode/                  # VS Code settings
caveman.json              # Local config
debug.log                 # Unity editor log
Assets/UserData/          # Study CSV output
.beads/                   # AI task tracking state
.opencode/                # AI context files
AGENTS.md                 # AI agent rules
```

### Branches

- **`main`** — stable release
- **`feat/*`** — feature branches

---

## Troubleshooting

### "Unity-Movement-72.0.0" package not found

Verify the folder exists next to the project root (see [Requirements](#requirements)).
Then restart Unity or re-import (`Packages/manifest.json` will trigger resolution).

### Pink / missing materials

URP render pipeline assets may not be assigned.

1. Go to `Edit → Project Settings → Graphics`.
2. Under `Scriptable Render Pipeline Settings`, assign the URP asset.
3. If missing, create one: `Create → Rendering → URP Asset (with Universal Renderer)`.

### Scenes show broken references after merge

Meta `.meta` files may conflict during Git merges. Use:

```
git checkout --theirs Assets/Scenes/<scene>.unity.meta
```

Then re‑open the scene in Unity.

### Cannot build for Android

- Install Android build support via Unity Hub.
- Ensure `Project Settings → Player → Other Settings → Color Space` is set to **Linear**.
- Set `Texture Compression` to **ASTC** in Android Player Settings.

---

## License

Internal research project.
