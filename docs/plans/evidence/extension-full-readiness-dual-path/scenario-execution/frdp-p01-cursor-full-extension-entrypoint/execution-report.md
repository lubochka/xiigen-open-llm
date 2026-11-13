# Scenario execution report - Cursor full-extension entrypoint

CURRENT TASK: advance one concrete product-readiness scenario with real evidence, not another diagnostic-only report.
NOT THE TASK: product readiness claim, full Cursor readiness claim, real Cursor chat or extension UI claim, token savings claim, semantic quality improvement claim, training, checkpoint mutation, locator update, commit, push, or stage.
EVIDENCE-ONLY: Phase 01 inventory, Phase 03 automated boundary packet/report, Phase 04 Cursor marker-only local evidence, and this scenario-specific core API harness run.

## Scenario

| Field | Value |
|---|---|
| Scenario number | 11 of 28 |
| Scenario identifier | `frdp-p01-cursor-full-extension-entrypoint` |
| Host | Cursor |
| Claim family | full extension readiness for Cursor |
| Marker | `FRDP-P01-frdp-p01-cursor-full-extension-entrypoint` |
| Selected because | It already had accepted automated boundary evidence and one Cursor marker-only local artifact, while formal local desktop and joined evidence were still missing. |

## Command actually run

```text
powershell.exe -NoProfile -ExecutionPolicy Bypass -File tools/xiigen-extension-core-api.ps1 -RequestPath docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-entrypoint\request.json -ResponsePath docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-entrypoint\response.json -TracePath docs\plans\evidence\extension-full-readiness-dual-path\scenario-execution\frdp-p01-cursor-full-extension-entrypoint\trace.json -CoreRoot C:\Projects\llm_mvp_core
```

| Evidence item | Value |
|---|---|
| Started UTC | `2026-06-24T15:40:47.4110125Z` |
| Completed UTC | `2026-06-24T15:41:17.4588466Z` |
| Exit code | `0` |
| Command result | [command-result.json](command-result.json) |
| Command output | [command-output.txt](command-output.txt) |
| Request | [request.json](request.json) |
| Response | [response.json](response.json) |
| Trace | [trace.json](trace.json) |

## Concrete result numbers

| Check | Result |
|---|---|
| Core API target called | `SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest` |
| Response success | `true` |
| Response status | `core_boundary_partial_no_semantic_quality_claim` |
| Host name in trace | `Cursor` |
| Host type in trace | `cursor-local-adapter-harness` |
| Method in trace | `orchestration-plan` |
| Selected skills count | `2` |
| Selected skills | `test-integrity`, `plan-review` |
| Requested skills count | `4` |
| Available skills count | `7` |
| Structured task references | `1` |
| Scenario references | `1` |
| Common assets loaded | `true` |
| Prompt-to-task asset loaded | `true` |
| Orchestration reasoning asset loaded | `true` |
| Skill selection asset loaded | `true` |
| Semantic quality claimed | `false` |
| Runtime extension claimed | `false` |
| Host runtime claimed | `false` |
| External AI used | `false` |
| Training started | `false` |
| Raw private prompt persisted | `false` |
| Contains secrets | `false` |

## Fresh local Cursor marker-file proof

This packet added a no-send local Cursor program check for the same marker. The worker created [local-cursor-marker.txt](local-cursor-marker.txt), then invoked the installed local Cursor program with that file path. The Cursor process command line contained the marker file path, and a `Cursor` main window was observed.

Evidence: [local-cursor-marker-proof.md](local-cursor-marker-proof.md) and [local-cursor-marker-proof.json](local-cursor-marker-proof.json).

This is accepted only as a fresh local marker-file route. It does not prove prompt intake, extension command activation, host-visible answer display, or that the Cursor extension command reached the same application programming interface / программный интерфейс приложения handler path.

## What moved

This scenario moved from board-only/status-only evidence to a scenario-specific partial evidence packet. The fresh run proves that a Cursor-shaped thin-adapter request with the scenario marker can enter `llm_mvp_core`, call `XiigenExtensionApiService`, produce a structured task reference, produce an orchestration scenario reference, select skills, load common assets, and keep semantic-quality/runtime-extension claims closed.

The local side also moved narrowly: the same marker now has a fresh local Cursor program marker-file proof, not only older Phase 04 marker-only evidence.

This does not prove real Cursor UI, real Cursor extension command activation, prompt intake in Cursor, host-visible answer display, full Cursor extension readiness, or product readiness.

## Joined status after this run

| Path | Status |
|---|---|
| Automated boundary / core / Application Programming Interface evidence | Accepted before Phase 03 for this scenario, and refreshed here with a scenario-specific core API harness run. |
| Local real-program evidence | Fresh no-send Cursor marker-file proof plus existing Phase 04 marker-only evidence plus Phase 1B extension-host/core correlation proof; no user interface / пользовательский интерфейс answer-display proof. |
| Joined evidence | Partial only; no formal joined pass because user interface / пользовательский интерфейс answer display is still missing. |

Counts stay unchanged: full joined pass `0 -> 0`, partial `1 -> 1`, not moved `8 -> 8`, blocked with next action `19 -> 19`, readiness claims opened `0 -> 0`.

## Next concrete action

Capture safe no-send Cursor user interface / пользовательский интерфейс answer-display evidence for the same row 11 marker, then join it to the accepted extension-host/core correlation proof without opening product readiness, full Cursor product readiness, or a formal joined-pass claim before that evidence exists.
