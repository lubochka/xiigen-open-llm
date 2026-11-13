# API/Dashboard QA HOWTO

This document explains how a person uses the dashboard/API explorer for XIIGen methods. It describes dashboard-to-core documentation and QA coverage only. Dashboard/API pass cannot prove live Claude/Cursor/Codex runtime, token savings, quality improvement, learned model readiness, target promotion, locator update or final readiness.

## Total / Done / Now / Remaining

| Field | Status |
|---|---|
| Total | Clear API examples are needed, explaining input/output/debug/raw JSON and scope boundaries. |
| Done | This how-to describes the working path and three primary examples; the docs QA review checks the examples only as documentation coverage. |
| Now | Dashboard/API is only supporting evidence; token pairs 0/9, quality rows 0/9, DPO rows 0/9, lifecycle rows 0/9, final reviews 0/10. |
| Remaining | After docs QA, live host runtime/token/quality/learnability evidence packets remain; dashboard examples must stay narrow support evidence. |

## How to use the dashboard

1. Open the dashboard in a local environment where the WPF UI is allowed.
2. Select an API row by its human-readable name.
3. Read the method description, example input and safety note.
4. Paste or type text into the input parameter box.
5. Enable debug/raw JSON only when it is needed for evidence.
6. Click Run.
7. Read the result, warnings, errors, debug trace, raw JSON and safe history.
8. Record the evidence scope: dashboard/API only, not live host runtime.

## API examples

| Human-readable API name | Method id | Input field | Example input | Returned result | Warning/error/debug/raw JSON | Limitation |
|---|---|---|---|---|---|---|
| Understand a user prompt as a structured task | `prompt-to-task` | User prompt | "Draft a plan to improve skill selection; do not implement." | Status, chat display text, structured task refs, check-set results. | Warnings/errors, optional debug trace, raw JSON, correlation id and redaction status. | Proves only the dashboard/API path for this method. Does not prove Claude/Cursor/Codex runtime or semantic quality. |
| Find an orchestration plan for a structured task | `orchestration-plan` | Structured task text | "Task: docs QA review; gates: no-overclaim, JSON parse, diff check." | Scenario refs, orchestration family, warnings and check-set results. | Debug may show a redacted task ref, selected scenario fields and reason codes. | Does not prove token savings, quality improvement or learned model readiness. |
| Select relevant skills for a task | `skill-selection` | Task, plan, available/requested skills | "Task: docs QA; available skills: documentation-sync, test-integrity, output-readability-gate." | Selected skills, rejected skills, evidence features, warnings. | Raw/debug panels may show selected/rejected ids and scaffold evidence. | Does not prove DPO training, checkpoint promotion, learnability or final readiness. |

## Returned result and debug panels

The result panel must show a clear answer/status. Warnings explain degraded or missing proof conditions. Errors explain validation or controlled failure. The debug trace must show redacted method/input/output/correlation details. Raw JSON must be visible only when it is explicitly requested and safe. Safe history must use a prompt hash or redacted refs, not raw private prompt text.

## Screenshot/transcript/evidence fields for later QA

A future dashboard QA packet must record:

- method id and human API name;
- example input or redacted input ref;
- result text and status;
- warnings/errors/debug/raw JSON state;
- correlation id;
- redaction status;
- evidence scope label;
- limitation statement;
- timestamp/freshness.

## Short code anchors

`43.SmartSkillsDomain.Dashboard/ApiDashboardRunner.cs`:

```csharp
public const string DashboardEvidenceScope =
    "dashboard-to-core invocation only; not extension host runtime proof";
```

`43.SmartSkillsDomain.Dashboard/ApiDashboardResultPresenter.cs`:

```csharp
RawJsonText,
DebugTraceText,
DebugTraceSuppressionReason,
```

These anchors explain why dashboard QA can inspect result/debug/raw panels while keeping scope narrow.

## Evidence scope and limitations

This example proves only the dashboard/API path for the named method. It does not prove live Claude/Cursor/Codex runtime, token savings, quality improvement, DPO training, checkpoint promotion, target promotion, locator update or final readiness.

## Phase 13 command

Phase 13/14: QA automation, documentation, and no-overclaim evidence.

What this step accomplishes: it gives the operator a single verifiable command for dashboard/API QA, host packet scope checks, docs coverage and a no-overclaim scan.

Run:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\tools\phase13-qa-docs-no-overclaim.ps1 -Json
```

What this proves:

- The existing dashboard/API QA script passes.
- Existing Claude/Cursor/Codex runtime packets preserve the exact host/scenario boundary.
- Dashboard/API docs explain how to run the API explorer and read result/debug/raw JSON fields.
- The changed Phase 13 wording contains no positive readiness overclaims.

What this does not prove:

- A dashboard/API pass does not prove live Claude/Cursor/Codex runtime.
- It does not prove final product readiness, full host UI readiness, full semantic quality, learnability/training success, checkpoint promotion, locator update or Phase 14 final reviews.
