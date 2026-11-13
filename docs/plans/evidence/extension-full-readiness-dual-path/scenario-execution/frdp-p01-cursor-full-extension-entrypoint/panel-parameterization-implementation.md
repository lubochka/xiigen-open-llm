# FRDP-P01 Cursor Panel Parameterization Implementation

CURRENT TASK: Implement minimal Cursor panel parameterization for FRDP-P01 same-marker UI display evidence.
NOT THE TASK: Host UI run, product readiness, full Cursor readiness, joined pass, online prompt send, or git action.
EVIDENCE-ONLY: Existing runtime-proof and marker-file artifacts show the desired marker/correlation shape but do not by themselves prove panel display.

## Change

`xiigenSmartSkills.panel` now accepts the same `RuntimeProofCommandInput` style payload used by `xiigenSmartSkills.runtimeProof.invoke`.

Before this change, the panel command always used a hard-coded prompt and called:

```text
runCursorSmartSkillsRequest(context, prompt, "xiigenSmartSkills.panel", "panel")
```

After this change, the panel command normalizes an optional payload through `normalizeRuntimeProofCommandInput`, then calls:

```text
runCursorSmartSkillsRequest(context, panelInput.prompt || "", "xiigenSmartSkills.panel", "panel", panelInput)
```

The panel still renders through the existing `buildPanelHtml(result)` and `getDisplayText(result)` path. Cursor remains a thin host; llm_mvp_core still owns the core request, response, and orchestration behavior.

## Evidence Fields Added

The `hostPanelUiRef` JSON now records:

- `correlationId`
- `runtimeProofScenario`
- `marker` from `redactedInput.marker`
- `sameMarkerInPrompt`
- `sameMarkerInCorrelation`
- `panelCommandAcceptedRuntimeProofInput`
- `sameSharedRequestPathAsRuntimeProofInvoke`

These fields are implementation support for the next safe no-send panel evidence run. They do not prove host UI display until that separate run is performed.

## Verification Boundary

Safe local check run:

```text
node --check dist\extension.js
```

Result: passed with exit code 0.

No package-level TypeScript compile or unit-test command was available in `package.json`; the package exposes only `install-smoke`, and the existing Extension Host harnesses would start host/runtime paths that are outside this no-host-UI step.

No Cursor host UI was run in this step. No product readiness, full Cursor readiness, or joined pass is claimed.
