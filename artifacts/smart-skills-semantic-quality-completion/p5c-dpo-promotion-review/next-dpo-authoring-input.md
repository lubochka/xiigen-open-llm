# Next DPO authoring input

Этот файл передает только три строки, которые P5C считает кандидатами для следующего DPO (Direct Preference Optimization) authoring. Это не training set, не eval set и не checkpoint input. У всех строк `training_ready=false`.

## Строки для следующего authoring

| Row | Machine | Что переписать | Что обязательно удалить | Ablation controls |
|---|---|---|---|---|
| `P3-PTT-DRAFT-001` | PromptToTask | Контракт задачи до исполнения: цель, scope, artifacts, validation, no-claim boundary. | Raw prompt text, exact path, filename/title cues. | exact prompt lookup; filename/path lookup; keyword-only classifier; source-title shortcut |
| `P4-ORCH-DPO-DRAFT-001` | OrchestrationReasoning | Phase-boundary handoff: evidence, self-check, parent review, next-phase boundary отдельно. | WordPress/plugin path, exact protocol wording, source title cues. | filename/path lookup; WordPress/plugin title shortcut; static phase keyword shortcut; exact protocol text lookup |
| `P5A-CAND-PTT-AUTHORITY-RULES-001` | PromptToTask | Rules artifact превращается в task contract: inputs, result, boundaries, forbidden actions, allowed verification. | AGENTS.md filename as signal, XIIGen/.NET/C#, service/class/index names, code snippets. | filename/path shortcut; stack leakage; service/class-name leakage; exact code snippet replay; source-state shortcut |

## Требования к следующему authoring

1. Написать новые chosen/rejected rows, а не переносить draft JSON дословно.
2. Rejected должен быть близким near miss, а не абсурдным отказом или checklist-only вариантом.
3. Source refs остаются provenance-only.
4. Same-source variants P3/P4 не считать independent examples.
5. Source-disjoint grouping должен держать семьи отдельно: `basic prompt`, `phase-transition protocol`, `authority rules`.
6. Training/eval остаются запрещены до отдельного parent acceptance, actual DPO rows, source-disjoint split, ablations и lifecycle gates.
