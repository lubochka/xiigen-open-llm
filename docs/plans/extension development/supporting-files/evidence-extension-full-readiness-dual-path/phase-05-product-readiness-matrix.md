# Phase 05.01 - Product readiness matrix intake/downgrade

CURRENT TASK: Phase 05.01 Product readiness matrix intake/downgrade. Build a human-readable matrix from existing evidence only.

active_task_sentence_source: Luba current unquoted instruction: "CURRENT TASK: Phase 05.01 Product readiness matrix intake/downgrade. Build a human-readable matrix from existing evidence only."

NOT THE TASK: no new probes/tests/builds, no GUI launch, no prompts/messages, no model/training/checkpoint/locator mutation, no code edit, no product/full-extension/Codex-chat/token/semantic/learnability/checkpoint/locator readiness claim, no git stage/commit/push/switch.

EVIDENCE-ONLY: Phase 01 inventory, Phase 02 dual-path contract, Phase 03 Automated boundary report/command-output records, Phase 04 local desktop downgrade summary/status/packets, execution state/log, and plan review ledger.

## Фаза и шаг

Фаза 6 из 17: Product readiness matrix. Смысл фазы: собрать понятную матрицу исходных product/readiness requirements и честно показать, где есть только narrow evidence, где claim понижен, где claim не готов, где есть blocker, а где работа не запускалась.

Шаг 05.01: intake/downgrade. Смысл шага: использовать только существующие evidence files Phase 01-04, не запускать новые проверки, не делать readiness claim и для каждой строки записать статус, ссылки, недостающие доказательства, следующий bounded phase/step и запрет overclaim.

Статус: DONE_AS_MATRIX_INTAKE_ONLY. Это не product readiness, не full extension readiness и не joined pass.

## Top summary

| Metric | Count |
|---|---:|
| total rows | 14 |
| proven narrow count | 3 |
| downgraded count | 4 |
| not ready count | 3 |
| blocked count | 2 |
| not run count | 2 |
| formal joined pass rows | 0 |
| formal local desktop pass rows | 0 |
| readiness claims opened | 0 |

Почему product readiness нельзя заявлять: Phase 03 даёт только boundary-only evidence для 12 из 28 scenario rows; Phase 04 даёт только Cursor marker-only evidence для одного local row, Claude/Codex имеют `BLOCKED_WITH_REASON`, formal `PASS_LOCAL_DESKTOP` = 0, formal `PASS_JOINED` = 0, а product workflow rows имеют `BOUNDARY_COMMAND_INSUFFICIENT`. Поэтому evidence supports only narrow guards, blockers, and downgrades.

## Evidence base

- [Phase 01 scenario/claim inventory](phase-01-scenario-claim-inventory.md): 28 scenario rows; claim families and required evidence fields.
- [Phase 02 dual-path evidence contract](phase-02-dual-path-evidence-contract.md): PASS evidence gate, join rules, and dashboard/documentation substitution rejection.
- [Phase 03 Automated boundary report](phase-03-automated-boundary-report.md): 12 accepted boundary rows, 16 needs-verification rows, 28 local desktop rows still not run.
- [Phase 04 completion/downgrade summary](phase-04-completion-downgrade-summary.md): Cursor marker-only, Claude blocked, Codex blocked, zero formal local/joined pass.
- [Phase 04 local desktop status board](phase-04-local-desktop-status-board.md): final local desktop board and counts.
- [Phase 04 local desktop packets JSON](phase-04-local-desktop-packets.json): machine-readable host rollup.
- [Execution state](../../EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md) and [execution log](../../EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md): rolling phase status.
- [Plan review ledger](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.review-ledger.md): plan-review/governance evidence only.

## Matrix overview

| requirement_id | Required row name | Русское имя | Status |
|---|---|---|---|
| PRM-01 | Product readiness | Готовность продукта | `NOT_READY` |
| PRM-02 | Full extension readiness: Claude, Cursor, Codex | Полная готовность расширения по Claude/Cursor/Codex | `BLOCKED_WITH_REASON` |
| PRM-03 | Full Codex chat UI readiness | Полная готовность Codex chat UI | `BLOCKED_WITH_REASON` |
| PRM-04 | Token savings before/after | Экономия токенов до/после | `DOWNGRADED` |
| PRM-05 | Semantic quality improvement: PromptToTask, OrchestrationReasoning, SkillSelection | Улучшение semantic quality по PromptToTask/OrchestrationReasoning/SkillSelection | `DOWNGRADED` |
| PRM-06 | Learnability/training and DPO creation | Learnability/training и создание DPO | `NOT_READY` |
| PRM-07 | Checkpoint mutation/promotion | Мутация и promotion checkpoint | `DOWNGRADED` |
| PRM-08 | Locator update | Обновление locator | `NOT_RUN` |
| PRM-09 | Local user model export/import/backup/continue-training | Локальный user model lifecycle: export/import/backup/continue-training | `NOT_RUN` |
| PRM-10 | Dashboard/API boundary proof | Dashboard/API boundary proof | `PROVEN_NARROW` |
| PRM-11 | Local desktop host proof | Local desktop host proof | `DOWNGRADED` |
| PRM-12 | Governance/skills/human-readable reporting repair | Governance/skills/reporting repair | `PROVEN_NARROW` |
| PRM-13 | No hidden deterministic mechanisms/boundaries | Нет скрытых deterministic mechanisms/boundaries | `NOT_READY` |
| PRM-14 | Documentation/readability evidence | Documentation/readability evidence | `PROVEN_NARROW` |

## Detailed matrix rows

### PRM-01 - Product readiness

- `requirement_id`: `PRM-01`
- Russian human-readable name: Готовность продукта.
- What this requirement means: продукт можно считать готовым только когда product workflow rows имеют joined evidence: Automated boundary + Local desktop path + EvidenceJoiner без открытых gaps.
- Current status: `NOT_READY`.
- Evidence links: [Phase 01 inventory](phase-01-scenario-claim-inventory.md), [Phase 03 report](phase-03-automated-boundary-report.md), [Phase 04 summary](phase-04-completion-downgrade-summary.md), [plan Phase 05](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md).
- Missing evidence: no joined product workflow rows; product-readiness rows are `BOUNDARY_COMMAND_INSUFFICIENT`; no formal local desktop pass; no formal joined pass.
- Next bounded phase/step that can improve it: Phase 05 follow-up after accepted joined evidence exists, otherwise Phase 15 final readiness review remains closed.
- Overclaim ban sentence: Нельзя заявлять product readiness, пока product workflow rows не имеют accepted joined evidence; текущая матрица доказывает только отсутствие такой готовности.

### PRM-02 - Full extension readiness: Claude, Cursor, Codex

- `requirement_id`: `PRM-02`
- Russian human-readable name: Полная готовность расширения отдельно для Claude, Cursor и Codex.
- What this requirement means: каждый host должен отдельно доказать real host entrypoint/runtime, boundary proof, local desktop marker/log/screenshot evidence, learning/lifecycle/QA rows and no host substitution.
- Current status: `BLOCKED_WITH_REASON`.
- Evidence links: [Phase 01 inventory rows 02/11/20](phase-01-scenario-claim-inventory.md), [Phase 03 accepted boundary interpretation](phase-03-automated-boundary-report.md), [Phase 04 summary](phase-04-completion-downgrade-summary.md), [Phase 04 status board](phase-04-local-desktop-status-board.md).
- Missing evidence: Claude and Codex have no safe local marker route; Cursor has only marker-only local evidence for one scenario; no host has full joined extension evidence.
- Next bounded phase/step that can improve it: Phase 06 full extension readiness matrix after host-specific Phase 04 blockers are repaired.
- Overclaim ban sentence: Нельзя заявлять full extension readiness for Claude/Cursor/Codex from API/core boundary evidence, marker-only Cursor evidence, or blocker records.

### PRM-03 - Full Codex chat UI readiness

- `requirement_id`: `PRM-03`
- Russian human-readable name: Полная готовность Codex chat UI.
- What this requirement means: нужен реальный Codex app/chat UI marker evidence or exact blocker, not helper script, not active conversation/workspace side-effect, and joined boundary evidence.
- Current status: `BLOCKED_WITH_REASON`.
- Evidence links: [Phase 01 Codex chat row](phase-01-scenario-claim-inventory.md), [Phase 03 row 21](phase-03-automated-boundary-report.md), [Phase 04 Codex blocker](phase-04-completion-downgrade-summary.md), [plan Phase 07](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md).
- Missing evidence: no non-host Codex chat UI boundary command; no safe marker-only surface separate from active Codex conversation/workspace; no screenshot/video/log joined evidence.
- Next bounded phase/step that can improve it: Phase 07 Full Codex chat UI readiness path and downgrade rules.
- Overclaim ban sentence: Нельзя заявлять full Codex chat UI readiness from Codex helper/API/plugin boundary evidence or from the current Codex conversation.

### PRM-04 - Token savings before/after

- `requirement_id`: `PRM-04`
- Russian human-readable name: Экономия токенов до/после.
- What this requirement means: same workload/tokenizer/version before-after traces, quality guard, repeatability, and host-visible access are required before any savings claim.
- Current status: `DOWNGRADED`.
- Evidence links: [Phase 01 token rows](phase-01-scenario-claim-inventory.md), [Phase 03 command table](phase-03-automated-boundary-report.md), [token trace command output](phase-03-command-output/03-dotnet-token-trace.txt).
- Missing evidence: no accepted before/after savings report; no joined host-visible candidate behavior; no product-level token savings calculation.
- Next bounded phase/step that can improve it: Phase 08 token savings before/after.
- Overclaim ban sentence: Нельзя заявлять token savings; existing evidence proves only token trace/accounting guard tests passed and explicitly leaves savings unclaimed.

### PRM-05 - Semantic quality improvement: PromptToTask, OrchestrationReasoning, SkillSelection

- `requirement_id`: `PRM-05`
- Russian human-readable name: Semantic quality improvement for PromptToTask, OrchestrationReasoning, SkillSelection.
- What this requirement means: held-out rubric comparison must show baseline/candidate improvement for PromptToTask, OrchestrationReasoning, and SkillSelection, with leakage/hardcoding checks and host-visible answer evidence.
- Current status: `DOWNGRADED`.
- Evidence links: [Phase 01 semantic rows](phase-01-scenario-claim-inventory.md), [Phase 03 command table](phase-03-automated-boundary-report.md), [quality scoring command output](phase-03-command-output/04-dotnet-quality-scoring.txt), [plan Phase 09](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md).
- Missing evidence: no held-out baseline/candidate improvement report; no layer-specific PromptToTask/OrchestrationReasoning/SkillSelection improvement packet; no local desktop answer evidence; hidden determinism audit is not executed.
- Next bounded phase/step that can improve it: Phase 09 semantic quality improvement eval plus Phase 13 deterministic shortcut review.
- Overclaim ban sentence: Нельзя заявлять semantic quality improvement; current evidence proves only scoring guard behavior, not actual improvement.

### PRM-06 - Learnability/training and DPO creation

- `requirement_id`: `PRM-06`
- Russian human-readable name: Learnability/training и DPO creation.
- What this requirement means: local training/continue-training, DPO provenance, split/leakage gates, before/after behavior, checkpoint diff/rollback, and host-visible changed behavior must all be joined.
- Current status: `NOT_READY`.
- Evidence links: [Phase 01 learnability/DPO rows](phase-01-scenario-claim-inventory.md), [Phase 03 failed/needs-verification interpretation](phase-03-automated-boundary-report.md), [DPO dataset command output](phase-03-command-output/05-dotnet-dpo-dataset.txt), [blocker audit](phase-03-command-output/07-blocker-audit-excluded-mutating-probes.txt).
- Missing evidence: learnability-training has `NO_BOUNDARY_COMMAND_DISCOVERED`; DPO train/eval/quality metrics were not run; mutating training/checkpoint probes were excluded; no host-visible changed behavior.
- Next bounded phase/step that can improve it: Phase 10 learnability/training/DPO quality.
- Overclaim ban sentence: Нельзя заявлять learnability, training success, or DPO creation/quality from DPO dataset loader tests alone.

### PRM-07 - Checkpoint mutation/promotion

- `requirement_id`: `PRM-07`
- Russian human-readable name: Checkpoint mutation/promotion.
- What this requirement means: candidate checkpoint mutation/promotion needs hash, compatibility/load, quality/token gates, promotion owner, rollback, and host reload using the promoted candidate.
- Current status: `DOWNGRADED`.
- Evidence links: [Phase 01 checkpoint rows](phase-01-scenario-claim-inventory.md), [Phase 03 command table](phase-03-automated-boundary-report.md), [checkpoint/locator command output](phase-03-command-output/06-dotnet-checkpoint-locator-readback.txt).
- Missing evidence: no checkpoint mutation was performed; no promotion was performed; no host reload proof; no rollback proof tied to promoted candidate.
- Next bounded phase/step that can improve it: Phase 12 checkpoint mutation/promotion and locator update.
- Overclaim ban sentence: Нельзя заявлять checkpoint mutation or promotion; existing evidence only proves readback/fresh-load/direct-promotion rejection guards.

### PRM-08 - Locator update

- `requirement_id`: `PRM-08`
- Russian human-readable name: Locator update.
- What this requirement means: locator update requires real runtime source-of-truth, locator diff, fresh-load, rollback locator, ownership, and per-host reload evidence.
- Current status: `NOT_RUN`.
- Evidence links: [Phase 01 locator rows](phase-01-scenario-claim-inventory.md), [Phase 03 locator interpretation](phase-03-automated-boundary-report.md), [plan Phase 12](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md).
- Missing evidence: no locator update/diff/fresh-load/rollback path was run or authorized; no per-host reload evidence.
- Next bounded phase/step that can improve it: Phase 12 locator update substep after checkpoint promotion gate.
- Overclaim ban sentence: Нельзя заявлять locator update or host reload from locator; the update itself was not run.

### PRM-09 - Local user model export/import/backup/continue-training

- `requirement_id`: `PRM-09`
- Russian human-readable name: Локальный user model export/import/backup/continue-training.
- What this requirement means: local model lifecycle proof needs backup/export/import/share/continue-training/rollback roundtrip, package manifest/readback, and host-visible restored or continued behavior.
- Current status: `NOT_RUN`.
- Evidence links: [Phase 01 asset lifecycle rows](phase-01-scenario-claim-inventory.md), [Phase 03 failed/needs-verification interpretation](phase-03-automated-boundary-report.md), [blocker audit](phase-03-command-output/07-blocker-audit-excluded-mutating-probes.txt), [plan Phase 11](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md).
- Missing evidence: asset-lifecycle rows have `NO_BOUNDARY_COMMAND_DISCOVERED`; portability/lifecycle probes were excluded because they write outside allowed evidence scope or perform lifecycle writes; no local desktop lifecycle evidence.
- Next bounded phase/step that can improve it: Phase 11 backup/export/import/share/continue-training/rollback lifecycle.
- Overclaim ban sentence: Нельзя заявлять local user model lifecycle readiness; export/import/backup/continue-training was not executed in accepted evidence.

### PRM-10 - Dashboard/API boundary proof

- `requirement_id`: `PRM-10`
- Russian human-readable name: Dashboard/API boundary proof.
- What this requirement means: API/core boundary may be narrow evidence for pre-host behavior, while dashboard/reporting can display accepted evidence but cannot replace boundary or local desktop paths.
- Current status: `PROVEN_NARROW`.
- Evidence links: [Phase 02 contract](phase-02-dual-path-evidence-contract.md), [Phase 02 validator output](phase-03-command-output/08-phase02-validator.txt), [Phase 03 command table](phase-03-automated-boundary-report.md), [plan Phase 14](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md).
- Missing evidence: no dashboard runtime proof; no dashboard row-count UI proof; no dashboard can claim model semantic quality or host runtime.
- Next bounded phase/step that can improve it: Phase 14 documentation/dashboard/user-visible status after accepted evidence rows exist.
- Overclaim ban sentence: Нельзя заявлять readiness from dashboard/API proof; the evidence supports only schema/API boundary and substitution rejection.

### PRM-11 - Local desktop host proof

- `requirement_id`: `PRM-11`
- Russian human-readable name: Local desktop host proof.
- What this requirement means: real installed Claude/Cursor/Codex host evidence must show marker/log/screenshot/video or exact blocker for the same scenario, then join with boundary proof.
- Current status: `DOWNGRADED`.
- Evidence links: [Phase 04 completion summary](phase-04-completion-downgrade-summary.md), [Phase 04 status board](phase-04-local-desktop-status-board.md), [Phase 04 rolling packets](phase-04-local-desktop-packets.json).
- Missing evidence: only Cursor has one marker-only local row; Claude and Codex are blocked; formal local desktop pass rows = 0; joined pass rows = 0.
- Next bounded phase/step that can improve it: Phase 04 follow-up route/probe work per host before any join.
- Overclaim ban sentence: Нельзя заявлять Local desktop host pass; current evidence is marker-only/blocker evidence, not formal `PASS_LOCAL_DESKTOP`.

### PRM-12 - Governance/skills/human-readable reporting repair

- `requirement_id`: `PRM-12`
- Russian human-readable name: Governance/skills/human-readable reporting repair.
- What this requirement means: status reports must be readable to Luba, preserve phase/step/count/review/fix/next-action sections, and keep readiness overclaims closed.
- Current status: `PROVEN_NARROW`.
- Evidence links: [plan review ledger](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.review-ledger.md), [Phase 04 summary reporting contract check](phase-04-completion-downgrade-summary.md), [execution log](../../EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md), [execution state](../../EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md).
- Missing evidence: future propagation to every relevant doc/skill/guide/agent file is still a later Phase 16 task; reporting repair does not prove product/runtime behavior.
- Next bounded phase/step that can improve it: Phase 16 documents, skills, guides, and agent-file propagation.
- Overclaim ban sentence: Нельзя заявлять product/runtime readiness from governance or reporting repair; it proves only a narrow reporting/governance artifact condition.

### PRM-13 - No hidden deterministic mechanisms/boundaries

- `requirement_id`: `PRM-13`
- Russian human-readable name: Нет скрытых deterministic mechanisms/boundaries.
- What this requirement means: exact-prompt lookup, regex-only routing, fixture leakage, marker hardcoding, hidden judge/teacher/tool use, and deterministic shortcuts must be audited before quality/learning/routing claims open.
- Current status: `NOT_READY`.
- Evidence links: [plan Phase 13](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md), [plan review ledger RC-16/RC-23](../../EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.review-ledger.md), [Phase 03 quality guard context](phase-03-automated-boundary-report.md).
- Missing evidence: no Phase 13 source/runtime hidden-determinism audit has been executed; no held-out host behavior variants; no linked hidden-determinism report.
- Next bounded phase/step that can improve it: Phase 13 no hidden deterministic shortcut review.
- Overclaim ban sentence: Нельзя заявлять semantic quality, learnability, or routing improvement until hidden deterministic shortcut review is accepted.

### PRM-14 - Documentation/readability evidence

- `requirement_id`: `PRM-14`
- Russian human-readable name: Documentation/readability evidence.
- What this requirement means: user-visible reports should be readable, linked to accepted artifacts, and clear about remaining work; documentation must not replace evidence.
- Current status: `PROVEN_NARROW`.
- Evidence links: [Phase 04 status board](phase-04-local-desktop-status-board.md), [Phase 04 summary](phase-04-completion-downgrade-summary.md), [Phase 05 JSON matrix](phase-05-product-readiness-matrix.json), [execution log](../../EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md).
- Missing evidence: no full Phase 14 dashboard/readability pass; no final user dashboard; no propagated docs/skills/guides completion packet.
- Next bounded phase/step that can improve it: Phase 14 documentation/dashboard/user-visible status and Phase 16 propagation.
- Overclaim ban sentence: Нельзя заявлять readiness from readable documentation; it proves only that current evidence is presented in a readable non-claim format.

## Counts and downgrade interpretation

| Status | Rows | Meaning |
|---|---:|---|
| `PROVEN_NARROW` | 3 | Narrow evidence exists for API/schema/reporting/readability boundaries only. |
| `DOWNGRADED` | 4 | Some guard/boundary/marker evidence exists, but the original readiness claim is not proven. |
| `NOT_READY` | 3 | The row lacks required joined or audit evidence and cannot be claimed. |
| `BLOCKED_WITH_REASON` | 2 | Exact current blocker prevents local/host claim from opening. |
| `NOT_RUN` | 2 | The requirement's actual runtime/lifecycle/update action was not executed in accepted evidence. |

## Verification plan for this artifact

Required local checks for this Phase 05.01 packet:

- JSON parse `phase-05-product-readiness-matrix.json`.
- Exact search Markdown/JSON for each required row name.
- Search unsupported readiness claim terms; every hit must be requirement name, non-claim, not-proven, downgrade, blocker, or overclaim-ban context.
- `git diff --check` for the four changed files in the allowed write scope.

## Changed files intended by this step

- `docs/plans/evidence/extension-full-readiness-dual-path/phase-05-product-readiness-matrix.md`
- `docs/plans/evidence/extension-full-readiness-dual-path/phase-05-product-readiness-matrix.json`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-STATE.md`
- `docs/plans/EXTENSION-FULL-READINESS-DUAL-PATH-EXECUTION-LOG.md`
