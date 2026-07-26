# DesignConversionDomain (DCM) Training Runner Guide

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

**Version:** 1.0
**Status:** working guide (companion to `dcm-dpo-construction-GUIDE.md`)
**Language:** the operator's language (plain language; Russian for the original author) — primary; code and identifiers in English
**Audience:** architect, implementer, reviewer within llm_mvp_core

This guide describes the **runner protocol, checkpoint lifecycle, metrics, and weight promotion**
for DesignConversionDomain. Questions of DPO-pair construction, defect taxonomy, Render-QA, and
the AI judge are described in `dcm-dpo-construction-GUIDE.md`.

---

## §0 How this skill is used

Use this guide when you need to design or review the DCM training runner: the order of `LoadCheckpointAsync`,
`TrainBatchAsync`, `SaveCheckpointAsync`, `ExportAsync`, backup, fresh-load, and continue-training.
It does not describe the unit implementation and does not authorize a product-code phase; it defines the runner contract for architectural
review, work orders, and the reviewer packet.

Before any claim like "trained", "ready for promotion", "improves HTML+CSS", or "replaces Bootstrap", the runner report
must close the source/license gate from the DPO guide, the metrics from the measurement guide, and the lifecycle below. If there is no evidence,
the correct status is `OPEN`, `PROMOTION_BLOCKED`, or an honest scaffold, not a trained feature.

---

## §1 Purpose and context

### What this guide does

`dcm-dpo-construction-GUIDE.md` answers the question "**how to build a training pair**" —
the structure of `DesignConversionTrainReadyPair`, the chosen/rejected rules, quarantine, license.

This guide answers the question "**how to train and save the model**":

- in what order to call the machine's methods;
- how `SaveCheckpointAsync` differs from `ExportAsync`;
- how shared state connects training and inference;
- which metrics to record after each batch;
- how to tell that the model is ready for promotion to the repository.

### Relation to the DPO guide

Section 9 of `dcm-dpo-construction-GUIDE.md` states explicitly:

> "Details of the runner protocol, metrics, and lifecycle (load → fallback → export/import →
> continue-training) are described in a separate document for the domain's training runner."

This document is exactly that companion.

---

## §2 Architecture of the trainable domain

### ILearningMachinePair — two legs

DesignConversionDomain implements the interface:

```
ILearningMachinePair<DesignConversionPersonal, DesignConversionGlobal>
```

This means two independent legs:

| Leg | Type | Where it lives | What it learns |
|------|-----|-----------|-------------|
| **Personal** | `ILearningMachine<DesignConversionPersonal>` | on the user's side, on their device | the specific user's preferences, their projects |
| **Global** | `ILearningMachine<DesignConversionGlobal>` | `25.ModelServer` + endpoint in `15.OrchestratorModel` | common design-conversion patterns from sources with a permitted license/consent |

Both legs are full `ILearningMachine` instances, each with its own checkpoint, its own
DPO pairs, and its own `SaveCheckpointAsync`.

### Rule A-LM-07 for DCM

The global leg publishes its checkpoint via `IModelRepository.UploadAsync` to `25.ModelServer`.
The endpoint is registered in `15.OrchestratorModel`. The personal leg is stored on the user's
side and is **not published** to the shared repository.

### Rule A-LM-08 for DCM

Synchronization from Personal to Global is allowed only through **anonymized signals**.
Specific design files, URLs, project names, or any user identifiers
do not enter the global training set without explicit consent.

### Units inside each leg

Each leg contains two trainable units:

```
CriticUnit   : ILogicalUnit<CriticInput, QualityVerdict>
ImproverUnit : ILogicalUnit<ImproverInput, ImprovedHtml>
```

`Train*` methods exist **only** on `ILearningMachine` (rule A-LM-02).
Units (`ILogicalUnit`) are pure inference contracts; adding `Train*` to them is forbidden.

Allowed trainable levels for DCM descriptions: `ILearningMachinePair`, `ILearningMachine`, `ILogicalOrchestrator`,
`ILogicalFlow`, `ILogicalUnit`. But ownership of saving, loading, promotion, and calling `Train*` stays with the Machine/Pair.
A regex/BM25 lookup, static validator, deterministic render helper, or ad-hoc service cannot be called a trained unit.
They are allowed only as bootstrap/scaffold evidence and must pass the anti-shortcut check in the measurement guide.

---

## §3 Shared state — the only bridge

### Why this matters

Between `TrainBatchAsync` (training) and `GetOrchestratorAsync` / inference there is no direct
call. The only mechanism for transferring knowledge is **shared state**,
which is read by units during inference and updated during training.

Analog in TranslationDomain (comment in `TranslationTrainedUnits.cs`, line 253):

> «Share the KNN store — this is the sole bridge between TrainBatchAsync and inference.»

Analog in `TranslationUnitContracts.cs`, line 177:

> «Shared state is the only mechanism that connects TrainBatchAsync with GetOrchestratorAsync.»

### For DCM this means

After each `TrainBatchAsync`, the updated weights/patterns of CriticUnit and ImproverUnit
live in the machine's shared state. When `GetOrchestratorAsync` is later called, the orchestrator
receives units that read exactly this updated state.

If `GetOrchestratorAsync` is called **before** `TrainBatchAsync` (or before loading a checkpoint),
the units work with a zero/initial state — this is not an error, but it is Bootstrap mode,
not trained mode.

### Practical rule

> Never pass units "from outside" the machine object as independent references.
> The inference orchestrator must obtain units through `GetOrchestratorAsync` of the same
> machine in which `TrainBatchAsync` or `LoadCheckpointAsync` has already run.

---

## §4 SaveCheckpointAsync vs ExportAsync

This is the most common point of confusion. The two methods do different things.

### `SaveCheckpointAsync` — real persistence of state to disk

```
await machine.SaveCheckpointAsync(absoluteCheckpointPath, loopState, ct);
```

- Serializes the **full trained state** (CriticUnit and ImproverUnit weights,
  accumulated patterns, metrics) to a JSON file on disk.
- This is the only way to persist the model between runner sessions.
- Without this call, training exists only in memory until the process ends.
- Analog in TranslationDomain: `TranslationMachine.cs`, lines 384–388.

Comment from the analogous place in TranslationMachine.cs (line 252):

> «This method does NOT write to disk. Call SaveCheckpointAsync to persist.»

(This refers to an intermediate state update — persistence to disk happens
precisely in `SaveCheckpointAsync`.)

### `ExportAsync` — metadata for the manifest, not persistence

```
var export = await machine.ExportAsync(ct);
// export contains: CheckpointRef + ManifestRef
```

- Returns **only metadata**: a reference to the checkpoint and the manifest.
- Does **not** serialize the trained state.
- Does **not** write files to disk.
- Used after `SaveCheckpointAsync` — to get a reference to the already saved
  checkpoint and pass it to `IModelRepository.UploadAsync`.
- Analog: `TranslationBatch029Runner.cs`, line 224.

### `LoadCheckpointAsync` (static) — full restoration

```
var result = await DesignConversionMachine.LoadCheckpointAsync(absoluteCheckpointPath, ct);
```

- Reads the checkpoint JSON file from disk.
- Returns the fully restored machine state.
- This is the **continue-training** path: load → train → save → continue.
- Analog: `TranslationMachine.cs`, lines 444–447.

### `ImportAsync` — NOT a full model load

```
await machine.ImportAsync(importData, ct);
```

- Restores only **scalar metadata** (version, accuracy).
- Does not load the units' trained weights.
- Is not a path for continue-training.
- Used to synchronize metadata between nodes/services.

### Difference table

| Method | Writes to disk | Restores weights | Purpose |
|-------|--------------------|----------------------|------------|
| `SaveCheckpointAsync` | YES — JSON with weights | — | Real model persistence |
| `ExportAsync` | NO | — | Get metadata for the manifest |
| `LoadCheckpointAsync` | — | YES — full | Continue-training / fresh load |
| `ImportAsync` | NO | NO — scalars only | Metadata synchronization |

---

## §5 Runner protocol — call sequence

### Full sequence of one training cycle

```
1. Load the checkpoint (or create a new machine for cold-start)
   result = await DesignConversionMachine.LoadCheckpointAsync(path, ct)

2. For each batch (50 training pairs each):
   a. Prepare LearningBatch<DesignConversionDomain>
      - Examples: 50 training rows (HeldOut=false)
      - HeldOutExamples: 50 held-out rows (HeldOut=true) for evaluation

   b. Check source/license gates for each accepted DPO pair
      - Personal leg: the private source stays local, Global does not receive it without consent
      - Global leg: source license/consent permits training and derived signals
      - failed/not_run gate => the pair does not enter TrainBatchAsync

   c. Call TrainBatchAsync
      batchResult = await machine.TrainBatchAsync(batch, ct)

   d. Record batch metrics (see §6 and `dcm-measurement-GUIDE.md`)

   e. Check deduplication (by ExampleId+UnitId, NOT by BatchId)

3. Before overwriting an existing checkpoint — create a backup or record a waiver
   backupRef = checkpointBackup.Create(...)

4. After all batches — save state to disk
   await machine.SaveCheckpointAsync(absoluteCheckpointPath, loopState, ct)

5. Perform fresh-load verification
   reloaded = await DesignConversionMachine.LoadCheckpointAsync(absoluteCheckpointPath, ct)
   assert metrics delta <= allowed tolerance

6. Get metadata for the manifest
   var export = await machine.ExportAsync(ct)

7. Verify that ImportAsync is used only for metadata sync and does not replace LoadCheckpointAsync

8. If it passes the A-LM-03 gate (see §7) — promote to the repository
   await modelRepository.UploadAsync(export.CheckpointRef, export.ManifestRef, ct)
```

### Rule A-LM-04 — batch structure

Each batch contains exactly **50 training rows** and **50 held-out rows**.

- Training rows (`HeldOut=false`): used to update the weights.
- Held-out rows (`HeldOut=true`): used to evaluate `AccuracyAfter`
  without affecting the weights.

The held-out set must be **fixed** (unchanged from batch to batch),
so that the metric is comparable across batches.

### Deduplication rule

Pairs must not be re-trained based on the composite key `ExampleId+UnitId`.
BatchId is not a deduplication key — the same pair may appear
in different batches with a different BatchId, but must be skipped on re-training.

Analog: `TranslationMachine.cs` — `_structuredTrainingIdentities` tracks
the `ExampleId+UnitId` composite key.

### Important: `TrainBatchAsync` skips held-out rows

The method trains only rows with `HeldOut=false`. Rows with `HeldOut=true` are used
only for evaluation (computing `AccuracyAfter`), but do not update the weights.

---

## §6 Batch metrics

After each `TrainBatchAsync`, the runner must record the following fields
in the batch result (`LearningBatchResult`):

| Field | Type | Description |
|------|-----|----------|
| `BatchId` | string/GUID | Batch identifier |
| `RowsTrained` | int | Number of trained rows (HeldOut=false) |
| `AccuracyBefore` | double | Held-out accuracy BEFORE training this batch |
| `AccuracyAfter` | double | Held-out accuracy AFTER training this batch |
| `RewardMargin` | double | Mean margin between chosen and rejected in the batch |
| `CpuMs` | long | CPU time spent, in milliseconds |
| `PeakRamMb` | double | Peak RAM usage in megabytes |
| `ElapsedMs` | long | Real elapsed time in milliseconds |
| `PromotedUnitIds` | string[] | Ids of units that passed the promotion gate |

### How to interpret the metrics

**`AccuracyAfter` > `AccuracyBefore`** — training was beneficial. If the opposite —
check the quality of the DPO pairs (quarantine material is possible).

**`RewardMargin`** — the mean margin between `AccuracyChosen` and `AccuracyRejected` over
all pairs in the batch. If the margin is below 0.05 — the pairs are too similar; check
the minimum-margin rules in `dcm-dpo-construction-GUIDE.md`.

**`PromotedUnitIds`** — a non-empty list means that one or more units
passed the A-LM-03 gate and are ready for promotion via `IModelRepository.UploadAsync`.

### Runner report — mandatory fields

After the training cycle completes, the runner must save a report with all
the listed fields. A report with `UNKNOWN` in `CpuMs`, `PeakRamMb`, or `AccuracyAfter`
is not considered final — it is intermediate data.

---

## §7 Rule A-LM-03 — Bootstrap→Trained promotion

### Essence of the rule

Bootstrap units (`BootstrapIntentBuilderUnit`, `BootstrapProjectScannerUnit`, and others
from `42.ProjectWikiDomain`) are the **starting point** for DPO training.
A trained unit may replace the Bootstrap default only after the **runner
publishes validated weights**.

For DCM this means: CriticUnit and ImproverUnit start in Bootstrap mode
(deterministic rules) and move to trained mode only after successfully
passing the gates.

Promotion must not be replaced by a deterministic shortcut. If a unit answers via regex, dictionary, BM25, static validation,
flow-index, or another lookup without a trained checkpoint and QA/eval evidence, it is Bootstrap/scaffold, not a trained feature.

### Promotion gate for CriticUnit

CriticUnit is ready for promotion when:

- `AccuracyAfter` on the held-out set ≥ the tier threshold (see §8);
- the unit's verdict scores **reliably match the real set of defects**
  (correctness of critique), rather than just carrying a high accuracy number inside the verdict;
- passed the ablation test: the unit's result does not change under a shuffled
  candidate order (no positional leakage).
- the source/license gate is closed for all accepted DPO pairs;
- QA/eval evidence is attached before a semantic/product claim.

### Promotion gate for ImproverUnit

ImproverUnit is ready for promotion when:

- Render-QA on held-out designs shows accuracy ≥ the tier threshold;
- the accuracy of the chosen result ≥ the accuracy of the rejected result
  with a margin ≥ 0.05;
- the test at 4 resolutions (360px, 768px, 1280px, 1920px) passed.
- the source/license gate is closed for all accepted DPO pairs;
- QA/eval evidence is attached before a semantic/product claim.

### Cold-start — only a named path

Training from scratch (without loading a checkpoint) is allowed only as an explicitly marked
`cold-start` path. The runner must log it as `mode=cold_start`. A runner
that starts from scratch without an explicit marker is considered defective.

---

## §8 Full lifecycle

### State-loading priority

At startup the runner checks state sources in the following order:

```
1. User cache / the user's local checkpoint
   → if found and valid: load via LoadCheckpointAsync

2. Repository / shared checkpoint (Personal leg: user's,
   Global leg: from 25.ModelServer via IModelRepository)
   → if found and valid: load via LoadCheckpointAsync

3. Cold-start: create a new machine (explicitly log mode=cold_start)
```

Inverting the order (loading the shared one first, then the user's) is an error.
User preferences take priority over the shared repository.

### Full lifecycle of one leg (for example, Personal)

```
[Runner start]
   ↓
[Load] LoadCheckpointAsync(userCachePath)
   → if no file → LoadCheckpointAsync(repoPath)
   → if no file → cold-start (log it)
   ↓
[Train] TrainBatchAsync × N batches (50 rows each)
   → source/license gates checked before training
   → metrics recorded after each batch
   → deduplication by ExampleId+UnitId
   ↓
[Backup] backup of the existing checkpoint before overwrite
   → backupRef recorded or waiver justified
   ↓
[Save] SaveCheckpointAsync(userCachePath)
   → full state to disk
   ↓
[Fresh-load] LoadCheckpointAsync(userCachePath)
   → metrics reproduced within tolerance
   ↓
[Metadata] ExportAsync()
   → CheckpointRef + ManifestRef (NOT state persistence)
   ↓
[Promotion] if the A-LM-03 gate is passed:
   IModelRepository.UploadAsync(CheckpointRef, ManifestRef)
   ↓
[Inference] GetOrchestratorAsync()
   → units read the updated shared state
   ↓
[Continue-training] the next cycle starts with LoadCheckpointAsync
   from the just-saved checkpoint
```

### Export/Import/Share

- **Export**: `ExportAsync()` → metadata. The real checkpoint file is already on disk
  after `SaveCheckpointAsync`. The metadata is passed to `UploadAsync`.
- **Import**: `ImportAsync(importData)` → scalars only (version, accuracy).
  Does not replace `LoadCheckpointAsync` during continue-training.
- **Share**: sending CheckpointRef to another instance → it calls `LoadCheckpointAsync`
  on the received path.
- **Backup**: a copy of the checkpoint JSON file. The runner may periodically copy
  the checkpoint into a `backup/` directory before each new training cycle.

---

## §9 Tier threshold values (PROVISIONAL)

> **ATTENTION:** The values below are **PRELIMINARY** and require Luba's explicit decision.
> Until confirmed, these thresholds are not mandatory Gate values of the runner.

| Tier | Render-QA threshold | AI judge | Note |
|-------|----------------|----------|------------|
| Free | 0.75 | Render-QA only, no AI judge | PROVISIONAL |
| Paid | 0.85 | Up to 50 AI-judge calls per month | PROVISIONAL |
| Pro | 0.90 | No limits | PROVISIONAL |

These thresholds are consistent with the table in `dcm-dpo-construction-GUIDE.md` §5.

Until Luba's explicit decision, the runner may use a fixed threshold of **0.80** as
a working temporary value for all tiers during development.

---

## §10 Applicable architectural rules

| Rule | Essence for DCM |
|---------|-------------|
| **A-LM-02** | `Train*` only on `ILearningMachine`. CriticUnit and ImproverUnit are pure inference contracts, without `Train*`. |
| **A-LM-03** | A trained unit replaces Bootstrap only after validated weights are published through the runner. |
| **A-LM-04** | Batch = 50 training + 50 held-out rows. Both numbers are strict. |
| **A-LM-07** | The global leg lives on `25.ModelServer`, endpoint in `15.OrchestratorModel`. |
| **A-LM-08** | Personal → Global synchronization only through anonymized signals. |

### What this guide does NOT do

- Does not describe the DPO-pair structure — that is `dcm-dpo-construction-GUIDE.md`.
- Does not describe Render-QA or the AI judge — that is also `dcm-dpo-construction-GUIDE.md`.
- Does not set the final tier threshold values — PROVISIONAL, requiring Luba's decision.
- Does not describe the implementation of CriticUnit or ImproverUnit — that is the subject of a separate plan.

---

## Glossary

| Term | Definition |
|--------|-------------|
| DPO | Direct Preference Optimization — training on chosen/rejected pairs |
| DCM | DesignConversionDomain — the design-conversion domain |
| Render-QA | Rendering Quality Assurance — pixel diff at 4 resolutions |
| A-LM-xx | Locked decision for the ILearningMachine hierarchy (from `DECISIONS-LOCKED.md`) |
| Bootstrap | Starting deterministic unit implementations before training |
| Cold-start | Starting training from a zero state (without loading a checkpoint) |
| CheckpointRef | Reference to the checkpoint file (path or URI) |
| ManifestRef | Reference to the model manifest (version, metadata, hash) |
| Held-out | Batch rows not used to update weights — only for evaluation |

---

*Companion: `docs/skills/dcm-dpo-construction-GUIDE.md`*
*Architectural decisions: `docs/DECISIONS-LOCKED.md` §A-LM-02..A-LM-08*
