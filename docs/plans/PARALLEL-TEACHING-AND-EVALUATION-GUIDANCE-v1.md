# PARALLEL-TEACHING-AND-EVALUATION-GUIDANCE-v1.md
# Version: 1.5 — 2026-05-28
# Purpose: Protocol for parallel training and evaluation across all five ILearningMachine directions.
# Authority: ARCHITECT-PLAN-LEARNING-MACHINE-EVOLUTION-v1.md
# Prerequisite: TECHNICAL-DEBT-CLOSURE-PHASE-0.md complete (TD1, TD2, TD3 all closed).
# Dependency map: PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md

---

## 0. Session Start — Mandatory Read Order

**Binding rule (from ORCHESTRATION-TRAINING-PROTOCOL-v1.md §4, line 149):**
> Never start from a generic guide alone. The guide can be stale relative to the latest run.

Before opening any section of this guidance, the executor must read the domain
source-of-truth files for the direction being continued. Generic guidance is
**secondary**. The domain state file is **authoritative**.

### Required reads before starting any direction

| Direction | Authoritative state file | What to read first |
|---|---|---|
| Dir 1 — Translation | Latest `STREAM-STATE.json` for the active RU_EN/EN_RU/HE_EN/EN_HE stream, if it exists. Also read `docs/plans/TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md` before any chat/post/prompt batch. | Current batch id, last gate result, next action; use case matrix per chat/post/prompt; parallel executor strategy |
| Dir 2 — CodeWiki | `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\PROJECT-LOCAL-WIKI-MODEL-STATE.json` | `status`, `next_action`, `last_gate`, `current_session` — **do not start from this guidance until these are read** |
| Dir 3 — Affiliate | `STATE-PARALLEL-DOMAIN-TRAINING.json` until a domain-specific state file exists. For the Achoti first-party source, also read `docs/plans/ACHOTI-ENTREPRENEURSHIP-AFFILIATE-TRAINING-GUIDE-v1.md`. | Current status, next action, Achoti source boundary, held-out prompt suite |
| Dir 4 — Design Conversion | `STATE-PARALLEL-DOMAIN-TRAINING.json` and Dir 2 export availability | Whether first CodeWikiMachine export exists |
| Dir 5 — Model Server | `STATE-PARALLEL-DOMAIN-TRAINING.json` | Current implementation status |

### Dir 2 specific read order (ORCHESTRATION-TRAINING-PROTOCOL-v1.md §4)

Before any Dir 2 execution:

1. `PROJECT-LOCAL-WIKI-MODEL-STATE.json` — current `status` + `next_action` + `last_gate`
2. The active session file named in `current_session` (e.g. `PROJECT-LOCAL-WIKI-MODEL-SESSION-01-BASE-MODEL.md`)
3. `ORCHESTRATION-TRAINING-PROTOCOL-v1.md` Section 4 read-order list
4. The latest runner report artifact named in `latest_phase_report_artifact`
5. Section 22.0 (gate rules) and Section 22.0A (metrics persistence rules) of the protocol

**Current Dir 2 state (as of last known session):**
- `status`: `SESSION_01_QUERY_PLANNING_BATCH_030_REJECTED_MOVE_TO_TRACE_AGGREGATOR`
- `next_action`: Do not continue `QUERY_PLANNING_BATCH_030`. Train `WikiTraceAggregator`
  cross-unit trace rows proving `ProjectWikiQueryPlanningFlow → WikiContextRoutingUnit
  → WikiAnswerComposer` order and blocking answer-before-evidence-analysis.
  Persist CPU/RAM/accuracy report before user-facing output.

This guidance does not override the `next_action` field in `PROJECT-LOCAL-WIKI-MODEL-STATE.json`.
If they conflict, the external state file wins.

---

## 1. Core Principle — One Machine Per Direction

Each direction is an independent `ILearningMachine<TDomain>` instance (TD3 contract,
`00.Core/LearningMachineContracts.cs`).

**Isolation rules (non-negotiable):**
- One `TrainingRunner` per machine. Runners do not share mutable state.
- Each machine owns its private DPO (Direct Preference Optimization) pair store
  and held-out evaluation set.
- `25.ModelServer` is the only shared boundary. The current implemented boundary is
  `IModelRepository.UploadAsync`, `IModelRepository.LoadWeightsAsync`,
  `IRemoteUnitLocator`, and `RemoteUnitManifest`. `ILearningMachineRepository`
  is a Direction 5 deliverable, not an existing execution surface.
- No direct file sharing between machines or training runners.

**Conflict avoidance:**
- Domain IDs are globally unique (tag format: `domain-name/direction-adapter`).
- Versioned manifests prevent stale-import confusion.
- Resource cap: maximum 3 concurrent `TrainBatchAsync` calls (configurable via
  a scheduler flag in the runner host; default 3).

---

## 2. Standard Training Pipeline (applies to every machine)

Every `ILearningMachine<TDomain>` implementation follows this sequence:

```
Step 1  DPO pair generation
        Domain-specific *DpoPairBuilder (one per domain).
        Output: LearningBatch<TDomain> with BatchId, DomainTag, TrainingItems[],
                SourceIds[], and a held-out set governed by the domain-specific
                protocol table below.

        Held-out split by domain (authoritative — no exceptions without explicit
        per-domain protocol approval):

        | Domain | Train rows | Held-out rows | Diagnostic probe allowed |
        |---|---|---|---|
        | CodeWiki (Dir 2) | 50 | 50 | 16-row probe only when WikiTraceAggregator or answer-analysis baseline explicitly permits it |
        | Documentation (Dir 2 D1/D2/D3) | 50 | 50 | Never — documentation accuracy floor depends on full 50 held-out |
        | Translation (Dir 1) | 50 | 50 | Only for direction-adapter diagnostic runs, with state file approval |
        | Affiliate personal/global (Dir 3) | 50 | 50 | Privacy constraint makes smaller sets invalid |
        | Design Conversion (Dir 4) | 50 | 50 | Until Dir 2 export available — then inherit Dir 2 split |
        | New domain with no protocol | 80% train | 20% held-out | 20% fallback — only for genuinely novel domains |

        **The generic 20% held-out split does NOT apply to CodeWiki, documentation,
        local-wiki, or translation domains. Using 20% on these domains under-measures
        accuracy and will cause false promotion.** WikiTraceAggregatorTrainingRunner
        enforces 50 held-out in code; do not override.

Step 2  TrainBatchAsync(LearningBatch<TDomain>)
        Calls the domain runner, which must perform ALL of the following
        (not just calling DpoTrainer.Step):

        a. Source authority check: verify every row's `source_status` is ACCEPTED
           by the domain source registry before training. Reject rows from
           unregistered or BLOCKED sources.
        b. Manifest check: if a checkpoint exists, load it via the current
           IModelRepository/RemoteUnitManifest path before writing new weights.
        c. Train: call `DpoTrainer.Step`, `DpoTrainer.StepMany`, `EvaluateMany`,
           or the domain-specific training pipeline as appropriate.
        d. For CodeWiki and documentation domains: the batch cannot be marked
           accepted without at least one runtime-flow inference call through the
           corresponding flow (e.g. `project-wiki-orchestration-flow`). A pure
           tensor/loss pass is not sufficient — the flow must return a non-empty
           `FinalHumanResult` on a representative prompt.
        e. Persist a runner report artifact to disk before returning. The report
           must contain: AccuracyBefore, AccuracyAfter, RewardMargin, CpuMs,
           PeakRamMb, ElapsedMs, RowsTrained, HeldOutRows, CheckpointRef,
           ManifestRef, BatchId, DomainTag, MachineId, GatePassed, RejectionReason.
           (See Section 22.0A of ORCHESTRATION-TRAINING-PROTOCOL-v1.md.)
        f. Return LearningBatchResult only after the report is persisted to disk.

Step 3  EvaluateAsync(LearningEvaluationSet<TDomain>)
        Runs on held-out set.
        Returns LearningEvaluationResult with Accuracy, RewardMargin, PassesGate.

Step 4  Promotion gate check (see Section 3)
        If PassesGate for 2 consecutive batches → proceed to Step 5.
        If gate fails → revert to previous checkpoint; record rejection batch.

Step 5  ExportAsync() → LearningMachineExport
        Produces: MachineId, DomainTag, Version, CheckpointRef, ManifestRef,
                  AccuracyAtExport, ExportedAtUtc.

Step 6  Publish the export through the current ModelServer boundary.
        Before Direction 5 is implemented this means the existing manifest,
        locator, checkpoint, and `IModelRepository` path. After Direction 5,
        this becomes `ILearningMachineRepository.PublishAsync(export)`.
        Consuming machines use `ImportAsync(export)` only after the export is
        located, loaded, and validated.
```

---

## 3. Evaluation and Promotion Gates

### Gate B (per batch — required before next batch starts)

| Metric | Threshold | Source |
|---|---|---|
| `AccuracyAfter` | ≥ `AccuracyBefore + 0.02` | `LearningBatchResult` |
| `RewardMargin` | ≥ 0.025 | `LearningBatchResult` |
| `RowsTrained` | 50 for CodeWiki/documentation/translation (see domain table in §2 Step 1) | `LearningBatchResult` |
| Held-out set size | 50 for CodeWiki/documentation/translation; 20% only for new domains without a protocol (see §2 Step 1) | `LearningBatch.TrainingItems` count |
| Runner report artifact | Persisted to disk before `TrainBatchAsync` returns (Step 2e) | File path in runner output |

The 16-row shape is a diagnostic probe only for sensitive orchestration baselines.
It is not a replacement for the 50-row training unit and cannot be promoted by
itself unless the active domain protocol explicitly allows that probe to become
part of a later combo baseline.

If Gate B fails: mark batch as rejected, record batch id and rejection reason in
the runner report, do NOT promote, do NOT increment the consecutive-pass counter.

### Promotion gate (required before ExportAsync + PublishAsync)

| Condition | Requirement |
|---|---|
| Consecutive Gate B passes | 2 batches back-to-back |
| Standalone gate | `EvaluateAsync` on full held-out set passes Gate B thresholds |
| Orchestrator A/B (optional) | Swap flow, measure `FinalHumanResult` quality on 10 live prompts; accepted if quality ≥ pre-swap baseline |

Rejection handling: revert `CheckpointRef` to the last export that passed the promotion
gate. Record the rejected export id in the runner report as `rollback_to_checkpoint`.
Note: `RemoteUnitManifest.rollback_ref` and `.upgrade_policy` are planned fields that
do not yet exist in `25.ModelServer/Remote/RemoteUnitManifest.cs` (current fields:
UnitId, UnitRole, DomainTag, Version, ServerBaseUrl, UnitEndpoint, ManifestSha256,
TrainingBranch, TrainingRunId, SourceDatasetSha256, ModelConfig, LoraConfig,
TokenizerPath, RegisteredAtUtc). Direction 5 (Section 8.5) must add these fields
before upgrade/rollback automation is active.

### Metrics every runner report must persist

```
AccuracyBefore, AccuracyAfter, RewardMargin
CpuMs, PeakRamMb, ElapsedMs
RowsTrained, HeldOutRows
CheckpointRef, ManifestRef
BatchId, DomainTag, MachineId
GatePassed (bool), RejectionReason (string | null)
RunnerVersion, ProducedAtUtc
```

UNKNOWN rules — three tiers:

| Report type | UNKNOWN permitted? | Effect |
|---|---|---|
| Promotion/completion claim | **Never** — blocks immediately | Cannot call a batch accepted, promoted, or done |
| Status-only checkpoint report | **Conditionally** — only when `next_action` = "add instrumentation + rerun" | Records gap; does not block checkpoint commit; does not advance the consecutive-pass counter |
| Diagnostic probe report | Permitted for in-progress diagnosis | Does not count toward promotion; must be labeled `diagnostic_only: true` |

In all cases, a report with `UNKNOWN` metric fields must not count toward the 2-consecutive-pass
promotion gate. The gap must be closed before the next promotion attempt.

---

## 4. Shared Infrastructure (what to reuse, what to add)

### Exists and ready to use

| Asset | Location | Role in training pipeline |
|---|---|---|
| `ILearningMachine<TDomain>` | `00.Core/LearningMachineContracts.cs` | Top-level contract — implement per domain |
| `ILearningMachinePair<TP,TG>` | `00.Core/LearningMachineContracts.cs` | Affiliate pair contract (Direction 3) |
| `LearningBatch<TDomain>` + siblings | `00.Core/LearningMachineContracts.cs` | Standard batch/result/export records |
| `DpoTrainer.Step` / `StepMany` / `EvaluateMany` | `11.DPO/DpoTrainer.cs` | DPO runner surface (Rule 10 — no external ML) |
| `LoraApplicator.Apply` | `10.LoRA/LoraApplicator.cs` | LoRA update in each batch |
| `IModelRepository` | `25.ModelServer/Contracts/IModelRepository.cs` | Upload checkpoint + load weights |
| `IRemoteUnitLocator` | `25.ModelServer/Contracts/IRemoteUnitLocator.cs` | Locate unit by id |
| `RemoteUnitManifest` | `25.ModelServer/Remote/RemoteUnitManifest.cs` | Checkpoint transfer record |
| `InMemoryLogicalFlowRegistry` | `15.OrchestratorModel` | Flow registration via 4-arg `Register<TInput>` |
| `LogicalOrchestratorAssistant.CreateDefault()` | `15.OrchestratorModel/Logical/` | Registers flows by ScenarioId |
| 6 Bootstrap `ILogicalUnit` impls | `42.ProjectWikiDomain/BootstrapOrchestrationUnits.cs` | Starting point for Direction 2 DPO training |

### Must be added (tracked as open items)

| Item | Location | Needed by |
|---|---|---|
| `ILearningMachineRepository` | `25.ModelServer/Contracts/` | **Direction 5 adds this interface.** Directions 1–4 use the existing `IModelRepository.UploadAsync` + `IRemoteUnitLocator` path until Direction 5 is complete. Do not block Dir 1–4 training on this interface's absence. |
| `RemoteUnitManifest.rollback_ref` | `25.ModelServer/Remote/RemoteUnitManifest.cs` | Direction 5 upgrade/rollback automation |
| `RemoteUnitManifest.upgrade_policy` | `25.ModelServer/Remote/RemoteUnitManifest.cs` | Direction 5 upgrade/rollback automation |
| `27.TranslationDomain` project | new | Direction 1 |
| `31.AffiliateDomain` project | new | Direction 3 |
| `30.DesignConversionDomain` project | new | Direction 4 |
| `CodeWikiMachine` wrapper class | `42.ProjectWikiDomain/` | Direction 2 |

---

## 5. Parallel Execution Rules

1. **One runner per machine.** Never share a `TrainingRunner` instance across domain tags.
2. **Serialise writes per domain.** A domain's own batches run sequentially (Batch N+1
   starts only after Batch N gate result is recorded). Different domains run in parallel.
3. **Resource cap.** Maximum 3 concurrent `TrainBatchAsync` calls across all machines.
   Configured via `ParallelTrainingScheduler.MaxConcurrent` (to be added in the runner host).
4. **ModelServer is the only handoff point.** Before Direction 5 exists, handoff
   uses the existing checkpoint, manifest, locator, and `IModelRepository` path.
   After Direction 5 exists, a machine publishes via `ILearningMachineRepository.PublishAsync`
   only after its promotion gate passes. Another machine imports via `ImportAsync`
   and runs its own gate before accepting the import.
5. **No shared mutable state.** DPO pair files, held-out sets, checkpoint files, and
   runner reports are private to each machine's `43.LearningLocalWikiData/artifacts/`
   sub-folder or equivalent external training data folder.
6. **Domain ID uniqueness.** Every `LearningBatch.DomainTag` must be globally unique in
   format `domain/adapter` (e.g., `translation/ru_en`, `code-wiki/wiki-trace-aggregator`).

---

## 6. Testing Teaching Results — Verification Protocol

For any training claim to be accepted, the following evidence must be present. A
`passed: true` JSON field alone is not enough (Rule: completion evidence must prove
actual capability, not only artifact existence).

### Required for each batch claim

- Runner-emitted `LearningBatchResult` with numeric `AccuracyBefore`, `AccuracyAfter`,
  `RewardMargin` (no UNKNOWN values).
- Held-out set used for `EvaluateAsync` call — must be separate from training rows.
- Rejection batch log if any batch was rejected.
- Gate B comparison: `AccuracyAfter ≥ AccuracyBefore + 0.02` AND `RewardMargin ≥ 0.025`.

### Required for promotion claim

- 2 consecutive Gate B passes on the same machine instance.
- `EvaluateAsync` on full held-out set: `LearningEvaluationResult.PassesGate = true`.
- `ExportAsync` succeeded: `LearningMachineExport` record with non-empty `CheckpointRef`.
- `ILearningMachineRepository.PublishAsync` returned `IsSuccess = true` (once interface exists).

### Required for end-to-end direction claim

- `GetOrchestratorAsync()` returns a live `ILogicalOrchestrator`.
- The orchestrator's `RunAsync` on a representative prompt returns a non-empty `FinalHumanResult`.
- No `TASK_FULLY_COMPLETE` until the above two runtime checks pass.

---

## 7. Concrete Successful-Learning Scenarios

A direction is not considered successfully trained because a scaffold test passes,
a file exists, or a report says `passed: true`. A direction is considered
successfully trained only when at least one scenario below is proven by all four
evidence layers:

1. Training evidence: accepted train rows and held-out rows from the correct source state.
2. Runner evidence: numeric runner metrics, CPU, RAM, elapsed time, checkpoint ref.
3. Runtime evidence: the trained machine is loaded through `GetOrchestratorAsync()`
   or the domain runtime flow and answers the scenario prompt.
4. Regression evidence: the same scenario rejects the known wrong behavior named
   in the training pair, not only the happy-path answer.

### 7.1 Scenario Rules Shared By All Directions

| Rule | Required behavior |
|---|---|
| Scenario rows must match runtime shape | Pair A-side is the actual unit input shape, not a ledger line or a prose summary. |
| State is read first | Each direction restores its current state before selecting rows. Generic guidance is never the first source. |
| 50/50 default | Normal accepted batch uses 50 train rows and 50 held-out rows. Diagnostic 16-row probes are allowed only where the active protocol says so. |
| Test is not metric | `dotnet test` proves contracts. Accuracy, reward margin, CPU/RAM/time come only from runner artifacts. |
| Wrong behavior must be named | Each scenario has a specific reject condition, usually from `[WRONG: ...]` contrast rows. |
| Runtime claim needs runtime path | End-to-end success needs the trained flow or orchestrator path, not only static validation. |

### 7.2 Direction 1 - Translation Scenario Set

| Scenario id | Real user prompt / input | Successful learned behavior | Evidence required | Reject if |
|---|---|---|---|---|
| TR-R1-DIRECTION | `Переведи это на английский для Codex: проверь последние изменения в проекте и скажи, что может сломаться.` | Direction router selects RU_EN, preserves intent, and outputs English suitable for downstream orchestration. | 50 train + 50 held-out rows from the active RU_EN stream state; runner report with numeric accuracy/reward margin; runtime `TranslationRoutingFlow` result. | Router flips direction, leaves Russian in output, or changes review intent into implementation intent. |
| TR-R2-TOPIC | `Сделай английскую версию поста: мы добавили xiigen affiliate library, теперь партнёры смогут обучать свои домены и делиться ими.` | Topic router keeps product terms stable and selects the translation adapter without treating product names as natural-language words. | Topic-router held-out evaluation plus runtime flow output with unchanged product/entity names. | Product terms are translated as ordinary words or topic selection changes the target adapter. |
| TR-R3-BACKTRANSLATION | `Переведи на английский, но сохрани смысл: завтра нужно показать клиенту, что импорт домена не раскрывает личные данные аффилиата.` | Runtime check translates RU_EN and internally back-translates EN_RU; request type, named entities, dates, privacy constraint, and user intent remain stable. | Held-out back-translation report with mismatch ledger and numeric pass/fail counts. | Back-translation preserves surface words but loses action, privacy constraint, entity, or date. |
| TR-R4-CHAT-SAFETY | `Можешь перевести на иврит: привет, я только что вступил в сеть, хочу понять с чего начать и кому задать вопросы.` | Translation is fluent enough for chat, but does not add promises, facts, or moderation decisions. | Runtime chat scenario report with source text, translated text, and reviewer verdict per row. | Output adds commitments, changes tone into a formal support answer, or drops important user context. |

Minimum first acceptance for Direction 1: `TR-R1-DIRECTION` and `TR-R2-TOPIC`
both pass on held-out rows and runtime flow output. `TR-R3-BACKTRANSLATION` is
required before promotion beyond the first adapter. `TR-R4-CHAT-SAFETY` is
required before chat-product use.

### 7.3 Direction 2 - Code Wiki Machine Scenario Set

Source of truth before any Direction 2 run:

```
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\PROJECT-LOCAL-WIKI-MODEL-STATE.json
```

Current required next action from that state: do not continue
`QUERY_PLANNING_BATCH_030`; move remediation to `WikiTraceAggregator` and train
cross-unit trace rows proving:

```
ProjectWikiQueryPlanningFlow -> WikiContextRoutingUnit -> WikiAnswerComposer
```

The model must also block answer-before-evidence-analysis.

| Scenario id | Real user prompt / input | Successful learned behavior | Evidence required | Reject if |
|---|---|---|---|---|
| CW-T1-TRACE-ORDER | `Где у нас создаётся задача после нажатия Save? Покажи файлы и порядок вызовов, прежде чем делать вывод.` | Trace output shows planning first, context routing second, answer composition last. Answer includes evidence path before conclusion. | `WikiTraceAggregator` 50 train + 50 held-out rows; runner metrics with heldout accuracy and reward margin; runtime `project-wiki-orchestration-flow` result. | Answer is produced before evidence analysis, or the trace skips `WikiContextRoutingUnit`. |
| CW-T2-MULTIROOT | `Почему заказ появляется в админке, но не виден в клиентском кабинете? Проверь клиент, API и SQL, если они связаны.` | System map links all configured roots and names which evidence came from each root. | Multi-root prompt suite result, system-map artifact, and runner report for the trained wiki unit. | Only the first project/root is indexed, SQL or server evidence is ignored, or output pretends one project is the whole system. |
| CW-T3-AUTHORITY | `Скажи точно, где считается комиссия партнёра. Если в проекте этого не видно, лучше так и скажи.` | The machine abstains or asks for missing evidence instead of hallucinating. | Held-out rows containing insufficient-evidence prompts and runtime prompt report. | It invents implementation details or cites non-existent files/classes. |
| CW-T4-DOC-LEVEL-D1 | `Объясни метод CalculateBonus: что он принимает, что возвращает, какие ошибки и side effects у него есть.` | Output documents local behavior, parameters, return value, side effects, and errors without caller-chain claims. | Documentation stream D1 accepted batch: 50 train + 50 held-out, `metric_mode=actual`, factual accuracy >= 0.80. | It includes caller workflow claims not visible from method-level evidence. |
| CW-T5-DOC-LEVEL-D3 | `Проследи, как login проходит от контроллера до базы, и где может потеряться session id.` | Output uses caller/callee propagation and includes the relevant call-chain evidence. | D3 stream accepted only after D1/D2 evidence exists for same stack/artifact stream. | It starts D3 before lower levels are accepted, or drops side effects from callees. |
| CW-T6-PLUGIN-PROMPT | `Перед тем как я попрошу Cursor менять checkout flow, дай ему контекст из local wiki: какие файлы трогать и чего нельзя ломать.` | Plugin-facing adapter injects evidence-bound context and displays `advanced_wiki_ready` only when registry/import-export/system-map/prompt-suite conditions pass. | Host or adapter prompt-suite evidence, not only internal helper execution. | UI claims ready while registry, import/export, multi-root, or advanced prompt suite is missing. |

Minimum first acceptance for Direction 2: `CW-T1-TRACE-ORDER` must pass with an
actual runner report and runtime `project-wiki-orchestration-flow` prompt result.
`CW-T2-MULTIROOT` and `CW-T6-PLUGIN-PROMPT` are required before plugin readiness
claims. `CW-T4` and `CW-T5` are required before documentation-learning claims.

### 7.4 Direction 3 - Affiliate Scenario Set

Direction 3 starts from scaffold and privacy guarantees. It cannot claim trained
business capability until real affiliate-domain train and held-out rows exist.

| Scenario id | Real user prompt / input | Successful learned behavior | Evidence required | Reject if |
|---|---|---|---|---|
| AF-P1-PERSONAL | `Я третий день не могу объяснить клиенту ценность продукта. Посмотри мои прошлые попытки и дай следующий шаг на сегодня.` | Personal machine uses local private state and produces a specific learning action without exposing private data. | Personal held-out scenario set and local runtime result. | It gives generic marketing advice or leaks raw personal notes. |
| AF-G1-GLOBAL | `Как простыми словами объяснить владельцу маленького бизнеса, зачем ему xiigen, если он не технарь?` | Global machine answers from shared XIIGen/marketing domain without using personal data. | Global train/held-out rows and runtime global machine result. | It uses personal facts or unregistered product claims. |
| AF-S1-SYNC | `Я хочу поделиться тем, что этот скрипт продаж сработал, но без имён клиентов и переписки. Что уйдёт в общую модель?` | Payload contains anonymized signal fields only: no raw conversation, no customer text, no identifiable data. | Sync contract test plus serialized payload sample stored in runner report. | Payload includes raw text, names, emails, account ids, or private notes. |
| AF-M1-MERGE | `Обнови мой локальный ассистент новыми материалами по продажам, но не трогай мои личные заметки и историю клиентов.` | Global knowledge improves shared-domain answer while personal state remains local and private. | Import report, compatibility check, runtime before/after comparison on held-out prompts. | Import overwrites personal profile or changes private local facts. |

Minimum first acceptance for Direction 3: `AF-S1-SYNC` passes before any networked
affiliate sync is allowed. `AF-P1-PERSONAL` and `AF-G1-GLOBAL` are required before
claiming the assistant can teach affiliates.

### 7.5 Direction 4 - Design Conversion Scenario Set

Direction 4 can scaffold in parallel, but training success depends on Direction 2
because design-to-code needs existing-code analysis from `CodeWikiMachine`.

| Scenario id | Real user prompt / input | Successful learned behavior | Evidence required | Reject if |
|---|---|---|---|---|
| DC-R1-REUSE | `Вот новый экран из Figma для профиля партнёра. Проверь, есть ли уже похожий компонент, и не генерируй дубль.` | Flow maps the design to the existing component and proposes reuse or extension, not duplicate generation. | CodeWikiMachine export loaded; design flow runtime report with component mapping. | It generates a new component while an existing one matches the evidence. |
| DC-G1-GAP | `Дизайнер поменял экран заказа. Скажи, это только вёрстка, редизайн существующего flow или новая функциональность?` | Gap analysis classifies change as skin-only/redesign/new-feature with evidence. | Runtime `DesignConversionFlow` result with at least 3 stages and non-empty gap analysis. | It treats every design as a full new feature. |
| DC-F1-FRAMEWORK | `Сделай этот экран сначала для Angular web, а потом скажи, что изменится для React Native.` | Framework adapter changes component shape appropriately while preserving domain behavior. | Held-out framework-adapter rows and runtime outputs for two target frameworks. | It emits Angular-specific concepts in React Native output or loses business behavior. |
| DC-S1-SAFETY | `В дизайне есть кнопка Approve payout. Проверь, есть ли в бэкенде такой action, прежде чем писать код.` | Flow flags missing backend/API evidence instead of inventing endpoints. | Runtime report with missing-evidence ledger. | It invents API endpoints, service methods, or stored procedures. |

Minimum first acceptance for Direction 4: `DC-G1-GAP` scaffold passes first.
Training acceptance requires `DC-R1-REUSE` and `DC-S1-SAFETY` after Direction 2
has exported a usable `CodeWikiMachine`.

### 7.6 Direction 5 - Domain Library Scenario Set

Direction 5 proves that trained units become project assets: exportable,
importable, locatable, and validated through infrastructure.

| Scenario id | Real user prompt / input | Successful learned behavior | Evidence required | Reject if |
|---|---|---|---|---|
| DL-E1-EXPORT | `Я дообучил домен для риэлторов. Подготовь его так, чтобы другой партнёр мог установить и проверить качество.` | Export contains machine id, domain tag, version, checkpoint ref, manifest ref, metric summary, source dataset hash. | `LearningMachineExport` artifact plus manifest/checkpoint files. | Export exists but checkpoint cannot be loaded or metrics are missing. |
| DL-I1-IMPORT | `Установи мне домен "real-estate-leads" от другого партнёра и проверь, что он реально отвечает на тестовый вопрос.` | Import succeeds, locator resolves it, and `GetOrchestratorAsync()` returns a usable orchestrator. | Export -> publish -> find -> import -> runtime prompt report. | Registry fields exist but loaded orchestrator cannot run. |
| DL-U1-UPGRADE | `Обнови мой домен продаж до новой версии, но сначала проверь, что старые сценарии не ухудшатся.` | Compatibility probe passes before import and records result. | `IsCompatibleAsync` report and before/after runtime comparison. | Upgrade is accepted based only on version string or manifest presence. |
| DL-R1-ROLLBACK | `После обновления ответы стали хуже. Верни предыдущую рабочую версию и докажи, что она снова проходит проверку.` | Rollback ref resolves, previous checkpoint loads, and previous runtime scenario passes again. | Rollback/load probe report and runtime prompt report after rollback. | Rollback ref exists but cannot be loaded or invoked. |
| DL-S1-SHARING | `Хочу взять домен по стоматологии из сети партнёров. Проверь, совместим ли он с моей локальной библиотекой и политикой данных.` | Imported domain is listed as compatible and does not bypass local policy or privacy checks. | Compatibility, policy, import, and runtime report. | Shared model imports without compatibility/policy probe. |

Minimum first acceptance for Direction 5: `DL-E1-EXPORT` and `DL-I1-IMPORT`
must pass with a real Direction 1 or Direction 2 machine export. `DL-U1` and
`DL-R1` are required before upgrade/rollback automation claims.

---

## 8. Per-Direction Setup

### 8.1 Direction 1 — Translation (27.TranslationDomain)

**Status:** ACTIVE — 44 tests passing as of phase 24 (2026-05-29, commit `a5e74154`). Scaffold steps below are for reference; the domain is already implemented.

**Before continuing any training run — read state file first (§0 rule):**
```
docs/plans/STATE-PARALLEL-DOMAIN-TRAINING.json        ← authoritative batch id, gate history, next action
docs/plans/TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md    ← domain-specific protocol and phase records
```
The state file is the authoritative source for the current batch id and last gate result.
Generic guidance in this section is secondary. If `STATE-PARALLEL-DOMAIN-TRAINING.json` and this section disagree, the state file wins.

**Existing data:**
```
C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\training-pairs-train-ready.jsonl
```

**Scaffold steps (historical reference — domain is already scaffolded):**
1. Create `27.TranslationDomain/TranslationDomain.csproj` (references 00.Core, 11.DPO, 26.TrainableRouter).
2. Add domain marker: `public sealed class TranslationDomain { }`.
3. Add input record: `TranslationRoutingRequest(SourceLanguage, TargetLanguage, TopicDomain, InputText)`.
4. Implement `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>`.
5. Implement `TranslationMachine : ILearningMachine<TranslationDomain>`.
6. Register `TranslationRoutingFlow` in `LogicalOrchestratorAssistant.CreateDefault()` with
   flow id `translation-routing-flow`.
7. Add `Tests/TranslationDomainTests.cs` — 3 tests minimum:
   - Flow id `translation-routing-flow` registered.
   - `TranslationMachine.TrainBatchAsync` returns `AccuracyAfter > 0`.
   - `GetOrchestratorAsync` returns non-null orchestrator.

**Gate B:** targeted contract tests pass, then scenarios `TR-R1-DIRECTION` and
`TR-R2-TOPIC` pass with runner-emitted metrics and runtime flow output. A test
run alone is not training-success evidence.

**Adapters:** RU_EN, EN_RU, HE_EN, EN_HE. Start with RU_EN — largest existing corpus.

---

### 8.2 Direction 2 — Code Wiki Machine (42.ProjectWikiDomain)

**Status:** READY TO START — TD1 and TD2 complete.

**Existing assets:**
- 6 Bootstrap `ILogicalUnit` impls in `BootstrapOrchestrationUnits.cs`.
- `ProjectWikiOrchestrationFlow` registered as `project-wiki-orchestration-flow`.
- Full DPO infrastructure for 10+ wiki units in `43.LearningLocalWikiData`.

**Scaffold steps:**
1. Add `CodeWikiMachine : ILearningMachine<CodeWikiDomain>` to `42.ProjectWikiDomain/`.
   `TrainBatchAsync` dispatches to existing training runners based on `batch.DomainTag`.
2. Add `public sealed class CodeWikiDomain { }` domain marker.
3. Restore state from
   `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\PROJECT-LOCAL-WIKI-MODEL-STATE.json`.
4. Continue `WikiTraceAggregatorUnit` training — do not continue
   `QUERY_PLANNING_BATCH_030_REJECTED`; train cross-unit trace rows for
   `ProjectWikiQueryPlanningFlow -> WikiContextRoutingUnit -> WikiAnswerComposer`.
5. **Gate B — two separate checks (do not conflate):**

   **Check A — Targeted contract tests** (`dotnet test --filter CodeWikiMachineTests`):
   - `CodeWikiMachine` implements `ILearningMachine<CodeWikiDomain>` (static contract)
   - `TrainBatchAsync` accepts a batch and returns `AccuracyAfter > 0`
   - `GetOrchestratorAsync` returns a non-null orchestrator
   These are contract/static verification only. A passing test suite is NOT training-success evidence.

   **Check B — WikiTraceAggregator runner report** (separate from test run):
   - `WikiTraceAggregatorUnit` runner invoked with 50 train rows + 50 held-out rows
   - Runner persists a JSON artifact with: heldout accuracy, reward margin, CPU/RAM/elapsed,
     batch id, checkpoint ref
   - heldout accuracy ≥ 0.75, reward margin ≥ 0.025 (from runner report, not from test output)
   - Runtime `project-wiki-orchestration-flow` executed on at least one representative prompt;
     `FinalHumanResult` is non-empty and trace order proves planning→context→answer
   Check B cannot be satisfied by a test assertion. It requires a real runner execution and
   a persisted JSON artifact named in the direction state file.

---

### 8.3 Direction 3 — Affiliate (31.AffiliateDomain)

**Status:** READY TO START — TD3 complete.

**First-party source now available:** `C:\Users\lubao\Documents\אחותי.zip`.
Use the dedicated guide before any Achoti-related training:
`docs/plans/ACHOTI-ENTREPRENEURSHIP-AFFILIATE-TRAINING-GUIDE-v1.md`.
This source targets `ILearningMachine<EntrepreneurshipBootstrappingDomain>` under
Direction 3 / AffiliateGlobal first. Raw private source files must not be committed.

**Project number:** `31` — number `28` is occupied by `28.SemanticNlp` (decision A-LM-05 analogue).

**Scaffold steps:**
1. Create `31.AffiliateDomain/AffiliateDomain.csproj` (references 00.Core, 25.ModelServer).
2. Add domain markers: `AffiliatePersonalDomain`, `AffiliateGlobalDomain`.
3. Implement `AffiliatePersonalMachine : ILearningMachine<AffiliatePersonalDomain>`.
4. Implement `AffiliateGlobalMachine : ILearningMachine<AffiliateGlobalDomain>`.
5. Implement `AffiliateMachinePair : ILearningMachinePair<AffiliatePersonalDomain, AffiliateGlobalDomain>`.
6. `SyncPersonalToGlobalAsync`: anonymise signal before sending — no raw user content,
   no conversation text, no identifiable data in sync payload (decision A-LM-08).
7. Add `Tests/AffiliateDomainTests.cs`:
   - Sync payload contains zero raw user content.
   - `ILearningMachinePair` contract fully satisfied.

**Gate B:** targeted contract tests pass, then scenario `AF-S1-SYNC` proves the
serialized sync payload contains no raw user content or identifiable personal data.
Training-success claims require `AF-P1-PERSONAL` and `AF-G1-GLOBAL` held-out rows.

---

### 8.4 Direction 4 — Design Conversion (30.DesignConversionDomain)

**Status:** BLOCKED until Direction 2 `CodeWikiMachine` produces its first export.
Scaffold can start now; training cannot start until Direction 2's first export is available.

**Scaffold steps (parallel with Direction 2):**
1. Create `30.DesignConversionDomain/DesignConversionDomain.csproj` (references 00.Core, 42.ProjectWikiDomain).
2. Add `DesignConversionRequest(DesignArtifactPath, TargetFramework, ActiveProjectRoot, LinkedProjectRoots[])`.
3. Implement flow skeleton: `DesignConversionFlow : ILogicalFlow<DesignConversionRequest>` with
   stub units (DesignAnalysisUnit, ExistingCodeMapUnit, GapAnalysisUnit, ConversionPlanUnit,
   CodeGenerationUnit).
4. Implement `DesignConversionMachine : ILearningMachine<DesignConversionDomain>`.
5. Add scaffold test: flow returns a result with ≥ 3 stages and non-empty gap analysis.

**Training data source:** Bootstrap DPO pairs from `C:\Projects\GenieDynamicApp` and
`C:\Projects\bizguide-client` (existing Angular/React projects).

**Gate B:** targeted scaffold tests pass, then scenario `DC-G1-GAP` returns a
structured conversion plan. Training-success claims require `DC-R1-REUSE` and
`DC-S1-SAFETY` after a real `CodeWikiMachine` export is available.

---

### 8.5 Direction 5 — Domain Library (25.ModelServer extension)

**Status:** READY TO START — TD3 complete (`LearningMachineExport` type exists in 00.Core).

**Required additions to `25.ModelServer/Contracts/`:**

```csharp
// New interface — does not yet exist; must be added
public interface ILearningMachineRepository
{
    Task<OperationResult<string>>                  PublishAsync(LearningMachineExport export);
    Task<OperationResult<LearningMachineExport>>   FindAsync(string machineId, string version);
    Task<OperationResult<LearningMachineExport[]>> ListCompatibleAsync(string domainTag);
    Task<OperationResult<bool>>                    IsCompatibleAsync(
        LearningMachineExport from, LearningMachineExport to);
}
```

**Required additions to `RemoteUnitManifest`** (fields currently absent):

```csharp
// Add to existing RemoteUnitManifest record in 25.ModelServer/Remote/RemoteUnitManifest.cs
string UpgradePolicy,   // e.g. "require_probe", "auto", "manual"
string RollbackRef      // CheckpointRef of the last known-good export for this machine
```

**Implementation class:** `LocalLearningMachineRepository` in `25.ModelServer/Local/`.

**Upgrade and rollback probe** (required before any `ImportAsync` on a version change):
- Upgrade probe: `IsCompatibleAsync(from, to)` must return `true`.
- Rollback probe: `FindAsync(machineId, export.RollbackRef)` must succeed and
  `ImportAsync(rollback)` must return `IsSuccess = true` on a test instance.
- Both probes must be recorded in a runner report before the new version is promoted.

**Gate B:** targeted repository tests pass, then scenarios `DL-E1-EXPORT` and
`DL-I1-IMPORT` pass with a real machine export. Upgrade/rollback claims require
`DL-U1-UPGRADE` and `DL-R1-ROLLBACK`.

---

## 9. Recommended Execution Order

```
Week 1 (parallel):
  Direction 1 scaffold + RU_EN first batch
  Direction 2 WikiTraceAggregator next batch + CodeWikiMachine wrapper
  Direction 5 ILearningMachineRepository interface + RemoteUnitManifest fields
  Direction 3 scaffold + Achoti first-party source inventory and first DPO batch plan

Week 2+ (after Direction 2 first export):
  Direction 4 training begins (reuses CodeWikiMachine analysis)
  Directions 1 + 3 continue batch-by-batch
  Direction 5 end-to-end cycle test with real Direction 1 export
```

One `TrainingRunner` per direction. Do not bundle multiple directions into a single runner.

---

## 10. Governance

**The domain state file is always authoritative over this guidance file.**
This guidance is a secondary reference. If the domain state file's `next_action`
differs from a step in this guidance, the domain state file wins.

For Direction 2 the authoritative source is:
`C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\PROJECT-LOCAL-WIKI-MODEL-STATE.json`

Read it before any Direction 2 session. If its `next_action` does not match what
this guidance says, update this guidance — not the state file.

Every training session for any direction must maintain a `STATE-<DIRECTION>-<DATE>.json`
state file following the schema in `STATE-TECHNICAL-DEBT-CLOSURE-PHASE-0.json`.
This state file supplements the domain state file — it does not replace it. It must link
to the domain source of truth first, for example `PROJECT-LOCAL-WIKI-MODEL-STATE.json`
for Direction 2 or `STREAM-STATE.json` for translation streams.

Required state fields:
- `plan_id`, `main_focus_goal`, `goal_reached`, `gate_status`
- `current_batch_id`, `last_gate_b_result`, `consecutive_gate_b_passes`
- `next_action` (non-empty when `goal_reached = false`)
- `commit_status.last_commit_pushed`

Branch rule:
- Single-executor sessions use the current branch. Do not create or switch branches
  unless Luba explicitly asks for branch work in the current task.
- Three-executor execution must use separate branches and preferably separate
  local worktrees. Luba's question authorizes this protocol; it does not authorize
  this executor to create or switch branches without a direct branch instruction.
- Use the `codex/` branch prefix unless Luba explicitly requests another prefix.

Allowed verification per batch: targeted `dotnet test --filter <DirectionTests>`
plus the direction's scenario runner/report named in Section 7. Full `dotnet test`
is not allowed as a ritual; it is only permitted when a plan Gate B explicitly
requires it or Luba explicitly asks for it.

---

## 11. Parallel Execution With Three Executors

This section answers how three executors can train and test different directions
in parallel without corrupting the code branch or the model registry.

### 11.1 Truth boundary: what exists now vs what Direction 5 must add

Already exists:
- `ILearningMachine<TDomain>.ExportAsync()` and `ImportAsync(...)` in
  `00.Core/LearningMachineContracts.cs`.
- `LearningMachineExport` as the top-level transfer record.
- `IModelRepository.UploadAsync(...)`, `IModelRepository.LoadWeightsAsync(...)`,
  `IRemoteUnitLocator`, and `RemoteUnitManifest` in `25.ModelServer`.

Not yet implemented:
- `ILearningMachineFactory`.
- A concrete `ParallelTrainingScheduler` runner host.
- `ILearningMachineRepository`.
- `RemoteUnitManifest.rollback_ref`.
- `RemoteUnitManifest.upgrade_policy`.
- Executed upgrade, compatibility, and rollback probes.

Therefore Directions 1, 2, and 3 can train in parallel as isolated domain work,
but full "download/upload trained machine between affiliates" is complete only
after Direction 5 implements the repository, manifest fields, and probes.

### 11.2 Recommended branch layout

Do not run three executors in one dirty checkout. Use one worktree and one branch
per executor:

| Executor | Branch | Owns | Must not edit |
|---|---|---|---|
| Translation executor | `codex/translation-training` | `27.TranslationDomain/`, translation tests, translation state/report artifacts | `00.Core`, `25.ModelServer`, unrelated docs |
| CodeWiki executor | `codex/codewiki-training` | `42.ProjectWikiDomain/` CodeWikiMachine slice, WikiTraceAggregator runner/report artifacts | Translation and Affiliate domain folders |
| Achoti/Affiliate executor | `codex/achoti-entrepreneurship` | `31.AffiliateDomain/`, Achoti source inventory, Achoti pair builder/tests/state | Translation and CodeWiki folders |
| ModelServer coordinator | `codex/modelserver-export-import` | `25.ModelServer/`, shared export/import contracts, compatibility/rollback probes | Domain-specific training logic |

If all three executors run on the same machine, create separate local worktrees
after Luba gives explicit branch approval. Example commands, not to be run by
an executor unless branch creation is explicitly authorized:

```bash
git worktree add C:/Projects/llm_mvp_core-translation -b codex/translation-training main
git worktree add C:/Projects/llm_mvp_core-codewiki -b codex/codewiki-training main
git worktree add C:/Projects/llm_mvp_core-achoti -b codex/achoti-entrepreneurship main
git worktree add C:/Projects/llm_mvp_core-modelserver -b codex/modelserver-export-import main
```

### 11.3 Git stores code; ModelServer stores trained assets

Git contains:
- source code;
- tests;
- source inventories and source ledgers;
- DPO row definitions when they are allowed to be committed;
- held-out prompt definitions;
- runner reports;
- manifests and checkpoint references.

Git does not contain:
- raw private archives such as `C:\Users\lubao\Documents\אחותי.zip`;
- private PDFs/audio/video extracted from Achoti;
- model weights when the checkpoint belongs in ModelServer/artifact storage;
- affiliate personal data.

The trained machine handoff is:

```text
TrainBatchAsync
  -> EvaluateAsync
  -> Gate B / promotion gate
  -> ExportAsync() -> LearningMachineExport
  -> publish via current ModelServer path
  -> another instance locates/loads export
  -> ImportAsync(export)
  -> GetOrchestratorAsync()
  -> runtime held-out prompt verification
```

Before Direction 5 is complete, "publish via current ModelServer path" means
the existing checkpoint/manifest/locator plus `IModelRepository` path. After
Direction 5, it means `ILearningMachineRepository.PublishAsync(export)`.

### 11.4 Merge order

1. Merge the ModelServer coordinator PR first when export/import infrastructure
   changes shared contracts.
2. Rebase or merge `origin/main` into each domain branch after the coordinator
   PR lands.
3. Merge domain PRs one at a time. Each PR must pass its own targeted Gate B and
   must name the exported model/checkpoint reference when training was run.
4. If two branches modify the same shared file, the coordinator branch owns the
   final shape. Domain branches rebase to it instead of resolving shared contract
   conflicts independently.
5. Raw training assets stay outside git even when the PR is merged. The PR links
   to source inventories, runner reports, and ModelServer manifest/checkpoint refs.

Recommended first merge order:

```text
codex/modelserver-export-import
  -> codex/translation-training
  -> codex/achoti-entrepreneurship
  -> codex/codewiki-training
```

`codex/codewiki-training` can be merged earlier if it does not depend on the
ModelServer export/import slice for its current Gate B.
