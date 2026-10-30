# Tests

Edit-mode unit tests (assembly `AstroIdle.Tests.EditMode`, editor-only, NUnit via Unity Test Framework).

**Run:** *Window → General → Test Runner → EditMode → Run All.*

| File | Covers |
|------|--------|
| `EditMode/NumberFormatterTests.cs` | Suffixes, truncation, boundaries, scientific notation, rates, durations. |
| `EditMode/ProgressionMathTests.cs` | Upgrade cost curve and rounding, weapon unlock pricing, random index picking, stat stacking, offline earnings cap and efficiency. |
| `EditMode/WaveMathTests.cs` | Enemy count cap, spawn interval floor, wave scaling, boss cadence, weighted spawn picking. |
| `EditMode/SaveDataTests.cs` | Upgrade level helpers, JSON round trip of progress (including starting weapon and auto mode settings). |
| `EditMode/AutoBuyPlannerTests.cs` | Auto mode rule: cheapest cost ÷ weight wins, zero weights are skipped, ship stat → category mapping. |
| `EditMode/ContentValidationTests.cs` | Loads the **shipped** content and fails on any validation error or missing core asset. |

## Conventions
- File per class under test: `<Class>Tests.cs`, namespace `AstroIdle.Tests`.
- Test pure logic (static math classes, services with `SaveData`). For randomness, inject the random value (see
  `SpawnPicker.Pick(…, roll01)`).
- `WaveMathTests` uses `ScriptableObject.CreateInstance<WaveConfig>()`, which picks up the field defaults in
  `WaveConfig.cs`. Tests assert relationships rather than hard-coded balance numbers so tuning doesn't break them.
- When you add a formula or rule, add a test here. When you add a content cross-reference, add a validation rule
  to `ContentDatabase.Validate()`. `ContentValidationTests` picks it up automatically.

## Adding Play Mode tests later
Create `Tests/PlayMode/` with an asmdef like the EditMode one but without `"includePlatforms": ["Editor"]`, and
document it here.
