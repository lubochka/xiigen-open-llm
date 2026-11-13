---
artifact_type: planning_library_index
authority_source: latest unquoted Luba work order for DPO legal build library
status: populated_now_recon_pass_1_done
last_updated: 2026-06-30
recon_pass: B-21 (R12-recon-libs) — first substantial pass recorded in claude-02 §RECON v1 + claude-03 §RECON
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
---

# ILearningDynamicGraph DPO Planning Library Index

## Purpose

Эта библиотека собирает planning evidence для `ILearningDynamicGraph` и DPO над `ILearningMachine` / `ILogicalOrchestrator` / `ILogicalFlow` / `ILogicalUnit`. Она нужна не для реализации, а для следующего архитектурного планирования: отделить текущие требования Luba от evidence-only материалов, зафиксировать legal gates, описать learning hierarchy и подготовить stage queue без запуска code, tests, builds, training или git.

## Document Map

| file | use in planning | status |
|---|---|---|
| `claude-00-library-index.md` | Навигация по библиотеке, статус заполнения, будущие evidence tasks | populated_now |
| `claude-01-authority-requirements-ledger.md` | Binding requirements ledger из текущего work order и mapping to gates | populated_now |
| `claude-02-source-inventory-and-reference-map.md` | Source categories + **RECON v1**: named references R-CORE-* (14) / R-MVP-* (10) с путями + честный Gate B-счётчик прохода | recon_pass_1_done |
| `claude-03-ilearningmachine-pattern-notes.md` | Pattern notes по hierarchy + **RECON v1**: 12 file:line якорей канона графа | recon_pass_1_done |
| `claude-04-dpo-legal-model-policy.md` | Legal model policy, closed API prohibitions, provenance and provider-terms gate | populated_now |
| `claude-05-dpo-data-training-measurement-guide.md` | DPO pair generation, judging, measurement, anti-deterministic-training warnings | populated_now |
| `claude-06-ilearningdynamicgraph-architecture-brief.md` | Architecture brief, data flow, graph state, async routing, Mermaid diagrams, solved subtasks | populated_now |
| `claude-07-execution-gates-open-questions.md` | Gates, stage queue, acceptance criteria, and final Luba questions section only | populated_now |

## Library Status

| area | collected now | future or not done |
|---|---|---|
| Current Luba authority | Current work order captured in `claude-01` | No implementation approval invented |
| Existing plan evidence | Existing plan path recorded as evidence-only | Old plan commands not reactivated |
| Core contracts | Exact contract paths recorded from `00.Core` and related project files | Full 40-pass core-doc pass still future |
| ILearning pattern | Main hierarchy and lifecycle summarized | Full cross-repo class audit still future |
| DPO mechanics | Existing `DpoPair`, `DpoTrainer`, routing DPO precedent summarized | No training run performed |
| Legal model policy | Closed API prohibition and provider-terms gate defined | No internet or provider terms check performed |
| External docs library | Future collection phase named | 20-pass external-doc library not completed |
| RECON knowledge library (B-21) | **Pass 1 done**: per-file named refs R-CORE-* (14) + R-MVP-* (10) + 12 file:line якорей, пути проверены на диске, формат-счётчик = Gate B | full per-file audit (pass 2..N) = future; pass_count_claimed honestly == 1, NOT >=40 |
| Execution gates | Stage queue and acceptance criteria written | Luba approval still required before implementation |

## Usage Order

1. Read `claude-01` to determine what is binding now.
2. Read `claude-02` to separate collected evidence from future collection.
3. Read `claude-03` and `claude-06` together for architecture shape.
4. Read `claude-04` before accepting any teacher, judge, provider, or training source.
5. Read `claude-05` before creating DPO pairs or metrics.
6. Read `claude-07` before issuing any execution work order.

## Completion Boundary

The library is populated now as a planning artifact set. It does not claim that product implementation, model training, provider legal checks, 20-pass external docs, or 40-pass core docs are complete.
