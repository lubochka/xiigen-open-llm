# XIIGen debug trace HOWTO

Debug trace помогает проверять, что request/response path понятен и redacted. Это verification aid only. Debug trace alone does not prove runtime readiness, token savings, quality improvement, semantic quality, learnability, target promotion, locator update or final readiness.

## Всего / Сделано / Сейчас / Осталось

| Поле | Статус |
|---|---|
| Всего | Нужен единый формат debug trace для dashboard, Claude, Cursor и Codex evidence packets. |
| Сделано | Этот how-to называет fields, positive example, controlled error example and redaction boundary. |
| Сейчас | Debug docs applied; live UI/chat readiness не закрыта; token pairs 0/9; quality rows 0/9; DPO rows 0/9; lifecycle rows 0/9; final reviews 0/10. |
| Осталось | Future runtime packets must attach actual same-run debug/error/correlation evidence. |

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

Что это доказывает: named request создал trace с correlation и redaction fields.

Что это не доказывает: live host runtime, если trace не пришел из real host surface. Сам по себе trace никогда не доказывает token/quality/learning.

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

## Граница debug evidence для Phase 13

Фаза 13/14: QA automation, documentation, and no-overclaim evidence.

Что решает этот шаг: Phase 13 проверяет, что debug text помогает понять method/input/output/correlation, но не становится самостоятельным proof of readiness.

Phase 13 QA должна читать debug evidence так:

- method id и host/surface названы;
- redacted input и redacted output есть в тексте или refs;
- correlation id связывает тот же run;
- controlled error или negative path видны, когда packet claims runtime proof;
- dashboard debug остается только dashboard/API scope.

Debug trace сам по себе не доказывает runtime readiness, token savings, semantic quality, learnability, checkpoint promotion, locator update или final readiness.
