---
artifact_type: execution_gates_open_questions
authority_source: latest unquoted Luba work order for DPO legal build library
status: populated_now_questions_final_section
last_updated: 2026-06-23
write_scope: C:\Users\lubao\Documents\xiigen\llm mvp\dpo legal build\claude-*.md
---

# Execution Gates And Open Questions

## Current Boundary

This library is a planning artifact. It does not authorize code implementation, training, tests, builds, git, external browsing, or provider terms checks.

## Stage Queue

| stage | action | status | exit evidence |
|---|---|---|---|
| S0 | Populate planning library in target folder | done | Eight `claude-*.md` files exist |
| S1 | Parent review of exact library files | remaining | Exact file list, headings, line counts |
| S2 | Luba acceptance of library and plan direction | blocked_on_luba | Current explicit acceptance quote |
| S3 | 20-pass external-doc and provider evidence collection | remaining | Terms, license, model-card, source registry packet |
| S4 | 40-pass `llm_mvp_core` core-doc library collection | remaining | Full source map and gap ledger |
| S5 | Architecture plan repair or final plan write | remaining | Approved architecture plan artifact |
| S6 | Implementation work order through authorized executor or sub-agent | not_authorized_now | Fresh implementation approval quote |
| S7 | DPO pair builder and legal gate implementation | not_authorized_now | Code, unit tests, source gates |
| S8 | Training runner and measurement implementation | not_authorized_now | Held-out metrics and lifecycle proof |
| S9 | Promotion review | not_authorized_now | Legal, metric, ablation, fresh-load gates pass |

## Hard Gates Before Implementation

| gate | pass condition |
|---|---|
| library_review_gate | Exact target folder contains only accepted library docs for this task |
| authority_gate | Current Luba instruction explicitly approves implementation |
| legal_gate | Provider terms and model license checks completed before model use |
| source_gate | DPO rows have source ids, provenance, and allowed-use status |
| hierarchy_gate | Training remains on `ILearningMachine`, not on flow or unit |
| graph_helper_gate | Deterministic helpers are not presented as learned behavior |
| async_gate | Branch dependencies and parallel levels are explicit |
| metric_gate | DPO and graph metrics are numeric and source-disjoint |
| lifecycle_gate | Checkpoint save, fresh-load, import, export, and continue-training are verified |
| privacy_gate | Raw project code or private data does not leave local boundary without approval |

## Acceptance Criteria For This Library

| criterion | expected result |
|---|---|
| Required files | Files 00 through 07 exist in target folder |
| Prefix | Every created basename starts with `claude-` |
| Non-empty | Every file has substantive Markdown |
| Header block | Every file includes `artifact_type`, `authority_source`, `status`, `last_updated`, `write_scope` |
| Future honesty | Future evidence phases are marked future or not done |
| Question placement | All questions for Luba appear only in final section of this file |
| No git | No git commands run |
| No tests or builds | No tests or builds run |
| No code edits | Repository code is unchanged |

## Resolved Questions (Решение Luba 2026-06-29)

Перенесено из «Open Questions For Luba» как RESOLVED. Значения дословно из решений Luba 2026-06-29 (мастер-план §E.0). Это plan-only пометка статуса; счётчики стадий S0–S9 не меняются.

| вопрос (исходный) | решение Luba 2026-06-29 | статус |
|---|---|---|
| #2 — один common `ILearningMachine<DynamicGraphDomain>` или `ILearningMachinePair` с самого начала? | v1 = ОДНА common-нога через `25.ModelServer`/`IModelRepository`/`IRemoteUnitLocator`; двухплечевой `ILearningMachinePair<DynamicGraphPersonal, DynamicGraphGlobal>` отложен на будущую фазу (Q6). | RESOLVED |
| #9 — какой продуктовый UI/runtime потребляет первый чекпойнт динам-графа? | Шов figma→html+css: публичный API-хост = `xiigen-platform-dot-net-angular` как фасад, делегирует во `xiigen-flows/FLOW-31`; FigmaToCode/CanvaToCode = клиенты (Q-host). Реализация фасада→FLOW-31 = execution (рекон-backing R16 = MISSING). | RESOLVED по выбору хоста; реализация — execution |

Связанные числовые/политические решения той же даты (детали в мастер-плане §E.0): Q3 (TeacherProvenance → боковой DPO-манифест через `LearningBatch.SourceIds`, без правки 00.Core); Q8 (`IQueryable<GraphNodeCandidate>`, candidate_order_invariant=true); Q11 (`ablation_threshold_locked=0.02`, единый unit/edge/graph); Q14 (детерминистический baseline порядка рёбер, обучаемость в `EdgeAuthorityUnit`); Q30 (held-out `>=0.95` И delta `>=+0.02` И RewardMargin `>=0.025`, единый порог); Q37 (policy by authority: teacher только open-weight permissive, closed = judge-only, a_side_style_only); Q38 (client-выходы `LegScope=PersonalAdaptive`, QUARANTINE до клиренса, per-client terms-snapshot).

**B-22 / R13-reconcile (2026-06-30) — закрытие механизма #4 (gate `graph_helper_gate`).** Гейт «Deterministic helpers are not presented as learned behavior» (таблица Hard Gates выше) ранее имел одну незакрытую диспозицию: механизм #4 no-hidden-determinism (`OrchestrationScenarioPhasePlanner.PlanPhases`, план фаз сценария) стоял **DEFERRED без блюпринта** — контрадикция с matrix R5 (`covered`) и с каноном «каждый из 9 механизмов имеет вердикт». Снято применением прецедента Q14: рекоменд. дефолт v1 = **`BASELINE_MUST_BE_LABELLED`** (honest deterministic baseline без claim «обученный план фаз») + инстанцированный обучаемый блюпринт `SemanticPhasePlanUnit` (typed `PhasePlanState` + DPO training-update → checkpoint `graph-phase-plan/1` + ablation `≥0.02` + forbidden-leakage). Где спроектировано: `claude-plan-12` §13 Q1 / §8.2 / §7 / Gate B п.6; мастер §E.0 Q-mech4 + §E.2 «Блок 12»; matrix R5 синхронизирован. После B-22 `classified_mechanism_count==9`, немаркированных финалов — 0. `graph_helper_gate` теперь измерим: deterministic helper #4 помечен baseline, не выдаётся за learned. **RESOLVED(Luba 2026-06-30): обучаемый план фаз НЕ активируется в v1** — остаётся `BASELINE_MUST_BE_LABELLED`, обучаемая замена = deferred-to-later-phase.

## Open Questions For Luba

1. Should the first future implementation target be a standalone `ILearningDynamicGraph` project, an addition beside `21.AgentInfrastructure\Logical`, or a domain beside `42.ProjectWikiDomain`?
2. **[RESOLVED — Luba 2026-06-29, см. раздел Resolved Questions]** Should `ILearningDynamicGraph` start as one common `ILearningMachine<DynamicGraphDomain>` or as `ILearningMachinePair<TPersonal,TGlobal>` from the beginning? → v1 = одна common-нога; pair отложен (Q6).
3. Which first DPO target matters most: project-local wiki graph answers, code-documentation routing, smart-skills orchestration, or another xiigen flow?
4. Should closed hosted APIs remain completely forbidden even for evaluation notes, or can they be considered after terms and privacy review?
5. **[RESOLVED — Luba 2026-06-30]** Which open-weight candidate should be checked first for legal teacher or judge use? → teacher v1 = **Qwen3 Apache-2.0 через нейтральный хостинг Together** (`provider_config_id=qwen3-together-v1`, `model_id=Qwen/Qwen3-235B`); judge-allowlist = тот же нейтральный набор провайдеров; closed = только judge; `goalConfidenceFloor=0.80`. Расширение allowlist иными open-weight permissive = deferred-to-later-phase.
6. What is the minimum first proof threshold: one 50 train and 50 held-out pilot, a 20-batch progression, or a different Luba-defined target?
7. Should provider terms verification be handled by a legal/source sub-agent, by Luba directly, or by both with separate evidence packets?
8. **[RESOLVED — Luba 2026-06-30]** Should personal adaptive graph learning be allowed to train on local private user data if only anonymized signals sync to global? → v1 = **ТОЛЬКО common-нога**; adaptive/personal-leg (`DynamicGraphMachinePair`) и client-local модель = deferred-to-later-phase; personal-store = `<project>/.xiigen`; политика анонимного sync = deferred-to-later-phase (активируется вместе с adaptive-ногой).
9. **[RESOLVED — Luba 2026-06-29, см. раздел Resolved Questions]** What product UI or runtime should consume the first trained dynamic graph checkpoint? → шов figma→html+css, публичный фасад `xiigen-platform-dot-net-angular` → `xiigen-flows/FLOW-31` (Q-host); реализация = execution.
10. Should future library expansion preserve these eight files as canonical, or add separate `claude-08+` evidence packets?
