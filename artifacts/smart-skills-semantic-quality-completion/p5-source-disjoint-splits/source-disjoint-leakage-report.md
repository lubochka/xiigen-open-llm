# P5 Source-Disjoint Leakage Report

Статус: `INSUFFICIENT_SOURCE_FAMILIES`.

Source-disjoint train/eval split сейчас невозможен. В `PromptToTask` есть две draft-строки, но обе происходят из одной source family: `basic prompt.txt`. В `OrchestrationReasoning` тоже есть две draft-строки, но обе происходят из одной source family: `PATCH-phase-transition-protocol-SKILL.md`.

Это не PASS и не semantic quality claim. Это честное P5 evidence: later P6/P8 training/eval должны ждать дополнительных независимых accepted rows или идти по явно обозначенному no-training/no-op path.

## PromptToTask

- rows: `P3-PTT-DRAFT-001`, `P3-PTT-DRAFT-002`
- source family: `sf-prompt-to-task-basic-prompt-77A175A9`
- source: `basic prompt.txt`
- split: невозможен
- training_allowed: `false`
- eval_allowed: `false`

Почему нельзя делить: одна строка не может быть train, а другая held-out eval, если обе являются same-source variants из одного `basic prompt.txt`. Такой eval проверял бы риск запоминания источника, filename/path, keyword-only или source-title shortcut, а не переносимую PromptToTask способность.

Нужно дальше: дополнительная независимая PromptToTask source family, принятая parent review, или демоция/слияние одного same-source variant.

## OrchestrationReasoning

- rows: `P4-ORCH-DPO-DRAFT-001`, `P4-ORCH-DPO-DRAFT-002`
- source family: `sf-orchestration-phase-transition-8CD45F23`
- source: `PATCH-phase-transition-protocol-SKILL.md`
- split: невозможен
- training_allowed: `false`
- eval_allowed: `false`

Почему нельзя делить: обе строки являются variants одного phase-transition source. Если одну считать train, а другую eval, модель могла бы пройти по WordPress/plugin title, path, static phase keywords или exact protocol text lookup.

Нужно дальше: дополнительная независимая OrchestrationReasoning source family, принятая parent review, или демоция/слияние одного same-source variant.

## No-Claim Boundary

В P5 не запускались code edits, tests, build, training, eval, checkpoint/model rewrite, commit, push или branch switch. Semantic quality не заявлена.
