# XIIGen debug trace HOWTO

The debug trace helps verify that the request/response path is clear and redacted. This is a verification aid only. Debug trace alone does not prove runtime readiness, token savings, quality improvement, semantic quality, learnability, target promotion, locator update or final readiness.

## Total / Done / Now / Remaining

| Field | Status |
|---|---|
| Total | A single debug trace format is needed for dashboard, Claude, Cursor and Codex evidence packets. |
| Done | This how-to names the fields, a positive example, a controlled error example and the redaction boundary. |
| Now | Debug docs applied; live UI/chat readiness is not closed; token pairs 0/9; quality rows 0/9; DPO rows 0/9; lifecycle rows 0/9; final reviews 0/10. |
| Remaining | Future runtime packets must attach actual same-run debug/error/correlation evidence. |

## Debug trace fields

Every useful trace should show:

- xiigen method id;
- host name/type;
- redacted input payload or input ref;
- redacted output payload or output ref;
- error or warning when present;
- correlation id;
- run timestamp/freshness;
- redaction status;
- evidence-scope label;
- raw/private refs kept separate from redacted report.

## Positive example

Human-readable summary:

```text
Method: prompt-to-task
Correlation id: claude-prompt-to-task-20260619-001
Input: redacted_input_ref=...
Output: structured_task_ref=...
Debug scope: dashboard_api_only
Redaction: raw private content not copied
```

What this proves: a named request created a trace with correlation and redaction fields.

What this does not prove: live host runtime, if the trace did not come from a real host surface. By itself, a trace never proves token/quality/learning.

## Controlled error example

```text
Method: skill-selection
Correlation id: cursor-skill-selection-20260619-err01
Error: missing available skills
Visible to user: yes
Debug scope: live_host_runtime_candidate
Redaction: no raw private text copied
```

Controlled errors are important because a happy path alone does not prove error handling. The packet must show that the error was visible to the user in the same host surface.

## Where debug appears

| Surface | Where the human should see it | Boundary |
|---|---|---|
| Dashboard | debug panel and raw JSON panel when toggled. | Dashboard/API only. |
| Claude | Claude-visible answer/error/debug block or attachment. | Requires live Claude surface packet. |
| Cursor | Extension Host logs plus command/chat/panel-visible result. | Requires safe launch and live UI evidence. |
| Codex | Plugin/app/chat/app-server output and evidence packet. | Plugin smoke is narrow until live surface proof exists. |

## Short code anchors

`43.SmartSkillsDomain/XiigenExtensionApiContracts.cs`:

```csharp
public sealed record XiigenDebugTrace(
public static string FormatDebugTraceForChat(
```

`43.SmartSkillsDomain/XiigenExtensionApiService.cs`:

```csharp
private static XiigenDebugTrace BuildDebugTrace(
```

These anchors identify core debug formatting/building. They do not prove any host UI displayed the trace.

## Raw/private vs redacted refs

Raw private content should stay out of human status reports unless a specific approved packet allows it. A safe report should point to redacted refs, hashes or private artifact locators. If redaction status is unclear, the evidence stays narrow and cannot be upgraded.

## Debug evidence boundary for Phase 13

Phase 13/14: QA automation, documentation, and no-overclaim evidence.

What this step accomplishes: Phase 13 verifies that the debug text helps to understand method/input/output/correlation, but does not become standalone proof of readiness.

Phase 13 QA must read debug evidence as follows:

- method id and host/surface are named;
- redacted input and redacted output are present in the text or refs;
- the correlation id links the same run;
- controlled error or negative path are visible when the packet claims runtime proof;
- dashboard debug stays dashboard/API scope only.

Debug trace by itself does not prove runtime readiness, token savings, semantic quality, learnability, checkpoint promotion, locator update or final readiness.
