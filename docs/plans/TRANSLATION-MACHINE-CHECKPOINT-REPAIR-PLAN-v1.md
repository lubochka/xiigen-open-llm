> **STATUS: RESOLVED — Phase 38 (2026-06)**
> The checkpoint repair goal of this plan was resolved by Phase 38's complete model asset lifecycle
> implementation. The full export/import/publish/load/continue-training lifecycle in 27.TranslationDomain.Tools
> supersedes the manual repair steps described here. See TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md §Phase 38.

---

# TRANSLATION-MACHINE-CHECKPOINT-REPAIR-PLAN-v1

Version: 1.0  
Date: 2026-05-29  
Mode: ARCHITECT / PLAN  
Scope: `27.TranslationDomain`, `27.TranslationDomain.Tools`, `Tests`, TranslationDomain training docs  
Status: PLAN ONLY - no code implementation in this document

---

## 0. Intent Clauses

Source request:

> "Пиши план как чинить"

Parsed intent clauses:

| # | Clause | Source phrase | Covered by |
|---|---|---|---|
| 1 | Fix real trained model persistence | "модель не сохраняется и не лоадится с диска" | Phases 1-5 |
| 2 | Remove replay/workaround as resume | "архитектурные дыры нельзя так решать" | Phases 6-7 |
| 3 | Return to 100+100 training only after real load works | prior active goal: 100 train + 100 test | Phase 8 |
| 4 | Make plan a full technical repair specification | "План это полное техническое описание" | Entire document |

Out of scope:

- No new corpus builder.
- No new language direction.
- No training run before real checkpoint persistence exists.
- No `--skip-cycles` replay as continuation.
- No claim that metadata export is a model checkpoint.

### 0.1 Governance mode for this plan

The parent session for this plan is ARCHITECT_INTERNAL. Architect owns review,
planning, governance/protocol updates, state/matrix reconciliation, and user
communication. Architect must not write production code or test code for this
plan and must not run `dotnet test`, `dotnet build`, `dotnet run`, training
runners, or `Start-Process`.

All implementation, build/test verification, runner execution, and training must
be done through bounded sub-agent work orders. If a sub-agent capability is not
available and the next safe action requires execution, the correct state is a
real blocker, not architect doing executor work.

Every checkpoint-repair implementation or verification sub-agent must use:

```text
model label: gpt-5.5 Extra High
tool model: gpt-5.5
reasoning_effort: xhigh
provider equivalent: sonnet latest with High reasoning when that is the configured available provider
```

No lower-tier sub-agent, silent fallback, inherited default, unspecified model,
or weaker reasoning effort may execute this plan. If neither
`gpt-5.5 Extra High` (`gpt-5.5`, `reasoning_effort=xhigh`) nor configured
`sonnet latest` with `High` reasoning is available, architect records a real
blocker and discusses it with Luba. Architect must not execute locally and must
not use a weaker sub-agent.

Architect must check current code/docs/evidence before answering technical
claims about checkpoint status. If sub-agent evidence conflicts, architect must
explain the conflict to Luba instead of silently choosing a result.

Checkpoint writer guard for all post-repair training:

```text
parent/main session role = ARCHITECT ONLY
execution/training = bounded sub-agents only
sub-agent model = gpt-5.5 with reasoning_effort=xhigh
local alias = 5.5 Extra High / sonnet latest High when configured equivalent
one work order = one code slice or one training chunk
one checkpoint path = one writer agent
no autonomous continuation past the bounded work order
no second writer on the same checkpoint
no runner when another run/build/training pipeline owns the checkpoint
```

Before any training writer is issued, architect must name the checkpoint path,
writer agent, source, runner command, heartbeat cadence, and max chunk boundary.
If source evidence has `MeetsPilotGate=false` or does not prove
`MeetsPilotGate=true`, the active checkpoint must not be written; the agent must
return source-gate evidence. This is governance now, and code enforcement is a
next repair task.

Contaminated checkpoints are not continuation sources. Recovery must be either
rollback to a clean checkpoint or forensic replay of approved reports with
explicit Luba approval.

---

## 1. Current Failure

### 1.1 What currently exists

File: `27.TranslationDomain/TranslationMachine.cs`

`ExportAsync()` currently creates `LearningMachineExport` with:

```text
MachineId
DomainTag
Version
CheckpointRef
ManifestRef
AccuracyAtExport
ExportedAtUtc
```

`ImportAsync()` currently validates `MachineId` and `DomainTag`, then restores only:

```text
_accuracyAfterLastBatch
_currentVersion
```

This is manifest metadata only. It is not a trained model checkpoint.

### 1.2 What is not persisted

The trained runtime state is held in private in-memory dictionaries:

```text
TranslationKnnStore._store
SemanticTokenizerUnit._patternCorrections
SemanticTokenizerUnit._signatureCorrections
ExecutorUnit._wordMap
ExecutorUnit._surfaceWordMap
ExecutorUnit._phraseMap
ExecutorUnit._exactSourceMap
QualityVerifierUnit._conceptRules
QualityVerifierUnit._requiredTermsBySignature
QualityVerifierUnit._requiredTermsBySourceMarker
TranslationMachine._trainingHistory
TatoebaPilotLoopRunner cursor / accepted training rows / cycle number
```

After a process kill, all of this is lost.

### 1.3 The invalid workaround

File: `27.TranslationDomain/TatoebaPilotLoopRunner.cs`

Pre-repair runner had:

```text
SkipCompletedCycles
LegacyPromotionThroughCycle
```

and replays old cycles into a fresh in-memory `TranslationMachine`.

That is not resume. It is retraining from source rows.

### 1.4 User-visible consequence

The user sees "continuing" or "checkpoint" language, but there is no loaded trained model from disk.

Correct status:

```text
Export/import API exists: yes.
Manifest metadata export exists: yes.
Trained model persistence: no.
Usable checkpoint load after restart: no.
Runner resume without replay: no.
```

---

## 2. Root Cause Record

### Level 1 - Immediate Fix

Failed behavior:

```text
Train -> process exits -> new machine cannot load learned KNN/word/phrase/tokenizer/verifier state.
```

Immediate code cause:

```text
TranslationMachine.ExportAsync/ImportAsync move only metadata, not unit state.
TranslationRuntime does not expose concrete unit references needed for checkpoint export.
Units have no ExportCheckpoint/ImportCheckpoint methods.
Runner has no checkpoint file path and no persisted cursor/history.
```

Immediate fix:

```text
Add checkpoint DTOs.
Add export/import methods to each trainable unit.
Make TranslationMachine save/load a full checkpoint JSON.
Make TatoebaPilotLoopRunner resume from checkpoint file, not replay.
```

Tests:

```text
TranslationMachine_CheckpointRoundTrip_PreservesExactSourceMap
TranslationMachine_CheckpointRoundTrip_PreservesWordMap
TranslationMachine_CheckpointRoundTrip_PreservesPhraseMap
TranslationMachine_CheckpointRoundTrip_PreservesTokenizerCorrections
TranslationMachine_CheckpointRoundTrip_PreservesVerifierRules
TatoebaPilotLoopRunner_ResumeUsesCheckpointWithoutReplay
```

### Level 2 - Structural Fix

Missing practice:

```text
The word "checkpoint" was accepted based on metadata fields, without a behavior-preserving round trip.
```

Structural rule:

```text
No checkpoint claim is valid without:
train -> save to disk -> create fresh machine -> load from disk -> same learned output -> no replay.
```

Files to update:

```text
docs/skills/translation-domain-reality-check-SKILL.md
docs/plans/TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md
```

### Level 3 - Architectural Decision

Missing architectural decision:

```text
TranslationMachine checkpoint is not a manifest reference. It is serialized trained unit state plus loop cursor/history.
```

Document action:

```text
This plan defines the checkpoint contract.
Training guide must be updated so resume means load checkpoint, never replay.
```

---

## 3. Phase 0 - Plan Self-Validation

### Check 1 - Goal Delivery Completeness

| Goal element | Phase | Gate B |
|---|---|---|
| Save trained model to disk | Phase 1-4 | checkpoint file contains all unit state |
| Load trained model from disk | Phase 4 | fresh machine reproduces learned output |
| Preserve every trained unit | Phase 2 | unit-specific round-trip tests pass |
| Remove replay as resume | Phase 6 | runner resume test proves no previous-cycle training calls |
| Return to 100+100 training only after persistence | Phase 8 | docs and runner require checkpoint path |

Result: PASS.

### Check 2 - Phantom Skill Detection

Referenced skills exist:

```text
plan-review-SKILL.md
no-product-decisions-SKILL.md
planning-session-startup-SKILL.md
intent-to-plan-SKILL.md
plan-self-validation-SKILL.md
architect-behavior-classifier-SKILL.md
root-cause-ladder-SKILL.md
translation-domain-reality-check-SKILL.md
```

Result: PASS.

### Check 3 - Skill Presence Matrix

No new skills are created by this plan.

Result: PASS.

### Check 4 - Execution Order Consistency

Order:

```text
Phase 1 DTOs
Phase 2 unit export/import
Phase 3 machine runtime references
Phase 4 machine save/load
Phase 5 tests for machine checkpoint
Phase 6 runner checkpoint resume
Phase 7 docs/protocol cleanup
Phase 8 return-to-training gate
```

No phase depends on a later artifact.

Result: PASS.

---

## 4. Target Architecture

### 4.1 Definition of checkpoint

A TranslationMachine checkpoint is a UTF-8 JSON file that contains:

```text
machine identity
schema version
model version
accuracy
structured-training flag
all trained unit state
training history
optional runner cursor/history
```

It is not:

```text
only CheckpointRef string
only ManifestRef string
only AccuracyAtExport
runner report
replay command
```

### 4.2 Checkpoint file location

Checkpoint files are runtime artifacts, not source code.

Default private location:

```text
C:\Users\lubao\Documents\xiigen\private-corpora\translation-checkpoints\
```

The code must accept any explicit path from CLI/tests.

Do not commit checkpoint JSON with trained private data to git.

### 4.3 Serialization rules

Use `System.Text.Json`.

Required options:

```csharp
new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    Converters = { new JsonStringEnumConverter() }
}
```

Reason:

- enum values remain readable;
- Russian/Hebrew text is not escaped into unreadable sequences;
- snake_case matches existing runner reports.

---

## 5. New DTO Contract

Create file:

```text
27.TranslationDomain/TranslationCheckpointContracts.cs
```

### 5.1 Top-level machine checkpoint

```csharp
public sealed record TranslationMachineCheckpoint(
    int SchemaVersion,
    string MachineId,
    string DomainTag,
    string Version,
    double AccuracyAfterLastBatch,
    bool HasStructuredTraining,
    DateTimeOffset ExportedAtUtc,
    TranslationKnnStoreCheckpoint KnnStore,
    SemanticTokenizerUnitCheckpoint SemanticTokenizer,
    IReadOnlyList<ExecutorUnitCheckpoint> Executors,
    QualityVerifierUnitCheckpoint QualityVerifier,
    IReadOnlyList<TranslationTrainingHistoryEntryCheckpoint> TrainingHistory,
    TatoebaPilotLoopCheckpoint? TatoebaLoop);
```

Validation:

```text
SchemaVersion == 1
MachineId == TranslationMachine.MachineId
DomainTag == TranslationMachine.DomainTag
Executors contain one entry per trained direction
```

### 5.2 KNN store checkpoint

```csharp
public sealed record TranslationKnnStoreCheckpoint(
    IReadOnlyList<TranslationKnnStoreEntryCheckpoint> Entries);

public sealed record TranslationKnnStoreEntryCheckpoint(
    string Signature,
    string Source,
    string Target);
```

Export order:

```text
Order by Signature, then Source, then Target for deterministic files.
```

Import rule:

```text
Clear current store, then rebuild all buckets exactly from Entries.
```

### 5.3 Semantic tokenizer checkpoint

```csharp
public sealed record SemanticTokenizerUnitCheckpoint(
    IReadOnlyList<SemanticTokenizerPatternCorrectionCheckpoint> PatternCorrections,
    IReadOnlyList<SemanticTokenizerSignatureCorrectionCheckpoint> SignatureCorrections);

public sealed record SemanticTokenizerPatternCorrectionCheckpoint(
    string Signature,
    SentencePattern Pattern);

public sealed record SemanticTokenizerSignatureCorrectionCheckpoint(
    string NormalizedRawText,
    TokenizerOutput Output);
```

Import rule:

```text
Clear _patternCorrections and _signatureCorrections.
Load exact keys as serialized.
Do not rerun tokenizer during import.
```

### 5.4 Executor checkpoint

```csharp
public sealed record ExecutorUnitCheckpoint(
    TranslationDirection Direction,
    IReadOnlyList<ExecutorWordMapEntryCheckpoint> WordMap,
    IReadOnlyList<ExecutorStringMapEntryCheckpoint> SurfaceWordMap,
    IReadOnlyList<ExecutorStringMapEntryCheckpoint> PhraseMap,
    IReadOnlyList<ExecutorStringMapEntryCheckpoint> ExactSourceMap);

public sealed record ExecutorWordMapEntryCheckpoint(
    string Lemma,
    SemanticConcept Concept,
    string Target);

public sealed record ExecutorStringMapEntryCheckpoint(
    string Key,
    string Value);
```

Export source:

```text
_wordMap -> WordMap
_surfaceWordMap -> SurfaceWordMap
_phraseMap -> PhraseMap
_exactSourceMap -> ExactSourceMap
```

Import rule:

```text
Clear all four dictionaries.
Load direction-specific state only into matching ExecutorUnit.
Reject checkpoint if Direction mismatches the unit.
```

Phase 41 load-time sanitizer:

```text
EN_RU checkpoints from phases before the script-aware lexicon fixes may contain
polluted WordMap and SurfaceWordMap entries such as able=>не, boston=>том, and
accept=>Пожалуйста. ImportCheckpoint prunes only EN_RU WordMap and
SurfaceWordMap entries whose target is low-information Russian filler, Latin or
mixed-script output, or a target with no Cyrillic signal unless it is a safe
numeric/punctuation target.

Explicitly allowed low-information EN_RU pairs are narrow:
  not/no/cannot -> не
  tom -> Том
  door -> дверь

PhraseMap and ExactSourceMap remain authoritative checkpoint evidence and are
not sanitized by this word-map cleanup. RU_EN imports keep valid English targets.
```

Phase 42 EN_RU function-word and phrase-first sanitizer:

```text
Standalone EN_RU WordMap and SurfaceWordMap entries are rejected when the source
key is an English function word, determiner, auxiliary, pronoun, or similarly
low-information modifier. Examples: the, a, an, to, be, is, was, were, do,
does, did, have, has, had, whole, and only. The only standalone function-word
exceptions stay narrow and exact: not/no/cannot/can't -> Russian negation
(`не`) only.

The same policy is applied both while importing checkpoints and while ingesting
new ExecutorOutput.WordMappings, so function words can still participate inside
PhraseMap entries such as "put out", "the fire", "need to know", and "don't
like" without becoming standalone unigram outputs.

Executor composition runs PhraseMap before unigram word maps. It now requires at
least one phrase span for phrase-map composition and permits repeated phrase
chunks to compose even when an unmapped connector word is skipped; unknown source
words are not copied into the EN_RU candidate.
```

Phase 43 guarded EN_RU ordinary-row KNN fallback:

```text
After exact-source, PhraseMap, and standalone word composition all fail, EN_RU
ordinary rows with no semantic concepts may try source-aware KNN before
Bootstrap. This is intentionally EN_RU-only and does not weaken RU_EN guards.

The ordinary-row KNN path accepts only trained targets that are compatible with
Russian output: target contains Cyrillic, contains no Latin letters, and does
not look like a source copy. Candidate source overlap is checked again in the
executor with a stricter guard: at least two non-low-information source tokens
must overlap and overlap/query_content_tokens must be at least 0.60.

Exact-source and PhraseMap remain higher authority than KNN. This keeps phrase
repairs authoritative while allowing ordinary Tatoeba EN_RU rows such as
hospital questions to reuse a close Russian neighbor instead of bootstrapping.
```

Phase 44 adaptive promotable candidate selection:

```text
The promotion predicate remains strict AND:
Rows.Count == requested
AND NewPatternRate > (1 - bestMaxAccuracy)
AND SourceTokenSeenRate < bestMaxAccuracy.

The runner no longer treats the next sequential full candidate as the only
possible visible cycle. It scans ahead through full unique candidates that
preserve exact-overlap, checkpoint-memory, and similarity filters. Full
candidates that fail the strict AND novelty gate are recorded as skipped rows,
the scan cursor advances past them, and the selector continues until it finds
the next full candidate that satisfies the current thresholds.

If a promotable candidate is found, the visible ACCURACY_SET reports that later
candidate while candidate_test_rows_scanned includes the extra scan distance.
Reports also expose noveltyScanSkippedRows and candidateSelectionMode so the
scan behavior is auditable without changing existing report fields.

If the current source contains no promotable full candidate, the runner reports
the last scanned failing/shortfall candidate without training rows that fail the
AND gate. Source rollover remains explicit through
--start-new-source-from-checkpoint; no replay, no reset, and no OR fallback are
allowed.
```

### 5.5 Quality verifier checkpoint

```csharp
public sealed record QualityVerifierUnitCheckpoint(
    IReadOnlyList<QualityVerifierConceptRuleCheckpoint> ConceptRules,
    IReadOnlyList<QualityVerifierStringListCheckpoint> RequiredTermsBySignature,
    IReadOnlyList<QualityVerifierStringListCheckpoint> RequiredTermsBySourceMarker);

public sealed record QualityVerifierConceptRuleCheckpoint(
    SemanticConcept SourceConcept,
    SemanticConcept ContextConcept,
    string Required);

public sealed record QualityVerifierStringListCheckpoint(
    string Key,
    IReadOnlyList<string> Values);
```

Import rule:

```text
Clear all verifier dictionaries.
Load lists exactly.
Deduplicate values with StringComparer.OrdinalIgnoreCase only if duplicates exist.
```

### 5.6 Training history checkpoint

```csharp
public sealed record TranslationTrainingHistoryEntryCheckpoint(
    string BatchId,
    int RowsTrained,
    double AccuracyBefore,
    double AccuracyAfter,
    double RewardMargin,
    long CpuMs,
    long PeakRamMb,
    long ElapsedMs,
    IReadOnlyList<string> PromotedUnitIds);
```

Import rule:

```text
Clear _trainingHistory.
Rebuild LearningBatchResult objects from checkpoint entries.
```

### 5.7 Tatoeba loop checkpoint

```csharp
public sealed record TatoebaPilotLoopCheckpoint(
    string PrivatePairsPath,
    int TrainSetSize,
    int TestSetSize,
    int LastCompletedCycle,
    int Cursor,
    int TrainRows,
    IReadOnlyList<TatoebaPilotPairRefCheckpoint> AcceptedTrainingRows,
    IReadOnlyList<TatoebaPilotPairRefCheckpoint> SkippedRows);

public sealed record TatoebaPilotPairRefCheckpoint(
    string Direction,
    int SourceSentenceId,
    int TargetSentenceId,
    string SourcePattern);
```

This is runner state, not model weights.

---

## 6. Unit Export/Import Methods

### 6.1 TranslationKnnStore

File:

```text
27.TranslationDomain/TranslationUnitContracts.cs
```

Add:

```csharp
public TranslationKnnStoreCheckpoint ExportCheckpoint();
public void ImportCheckpoint(TranslationKnnStoreCheckpoint checkpoint);
```

Implementation details:

```text
ExportCheckpoint:
  flatten _store into Entries
  preserve every duplicate pair
  deterministic sort

ImportCheckpoint:
  _store.Clear()
  foreach entry: AddPair(entry.Signature, entry.Source, entry.Target)
```

### 6.2 SemanticTokenizerUnit

File:

```text
27.TranslationDomain/TranslationTrainedUnits.cs
```

Add:

```csharp
public SemanticTokenizerUnitCheckpoint ExportCheckpoint();
public void ImportCheckpoint(SemanticTokenizerUnitCheckpoint checkpoint);
```

Implementation details:

```text
Export:
  serialize _patternCorrections
  serialize _signatureCorrections

Import:
  clear both dictionaries
  restore exact keys and values
```

### 6.3 ExecutorUnit

File:

```text
27.TranslationDomain/TranslationTrainedUnits.cs
```

Add:

```csharp
public ExecutorUnitCheckpoint ExportCheckpoint();
public void ImportCheckpoint(ExecutorUnitCheckpoint checkpoint);
```

Implementation details:

```text
Export:
  include _direction
  serialize _wordMap as lemma/concept/target
  serialize _surfaceWordMap
  serialize _phraseMap
  serialize _exactSourceMap

Import:
  if checkpoint.Direction != _direction -> throw or fail through caller validation
  clear all dictionaries
  restore all maps
```

### 6.4 QualityVerifierUnit

File:

```text
27.TranslationDomain/TranslationTrainedUnits.cs
```

Add:

```csharp
public QualityVerifierUnitCheckpoint ExportCheckpoint();
public void ImportCheckpoint(QualityVerifierUnitCheckpoint checkpoint);
```

Implementation details:

```text
Export:
  serialize _conceptRules
  serialize _requiredTermsBySignature
  serialize _requiredTermsBySourceMarker

Import:
  clear all dictionaries
  restore exact keys and values
```

---

## 7. TranslationMachine Runtime Repair

File:

```text
27.TranslationDomain/TranslationMachine.cs
```

### 7.1 Current runtime problem

Current private record:

```csharp
private sealed record TranslationRuntime(
    TranslationRoutingFlow Flow,
    IReadOnlyDictionary<string, ITranslationUnitTrainer> UnitTrainers);
```

This hides concrete units from `TranslationMachine`.

### 7.2 Required runtime record

Replace with:

```csharp
private sealed record TranslationRuntime(
    TranslationRoutingFlow Flow,
    IReadOnlyDictionary<string, ITranslationUnitTrainer> UnitTrainers,
    TranslationKnnStore KnnStore,
    SemanticTokenizerUnit SemanticTokenizer,
    IReadOnlyDictionary<TranslationDirection, ExecutorUnit> Executors,
    QualityVerifierUnit Verifier);
```

Add field:

```csharp
private readonly TranslationRuntime? _runtime;
```

Constructor rules:

```text
TranslationMachine() -> CreateRuntime() -> _runtime non-null
private TranslationMachine(TranslationRuntime runtime) -> stores _runtime
TranslationMachine(TranslationRoutingFlow flow) -> _runtime null, checkpoint export unsupported
```

If `_runtime` is null and checkpoint export is requested:

```text
OperationResult.Fail("Runtime state is not available for this machine.", "RUNTIME_STATE_UNAVAILABLE")
```

### 7.3 Machine checkpoint methods

Add:

```csharp
public OperationResult<TranslationMachineCheckpoint> ExportCheckpoint();

public OperationResult<bool> ImportCheckpoint(TranslationMachineCheckpoint checkpoint);

public async Task<OperationResult<LearningMachineExport>> SaveCheckpointAsync(
    string checkpointPath,
    CancellationToken ct = default);

public static async Task<OperationResult<TranslationMachine>> LoadCheckpointAsync(
    string checkpointPath,
    CancellationToken ct = default);
```

### 7.4 ExportCheckpoint implementation

Required behavior:

```text
Validate _runtime != null.
Build TranslationMachineCheckpoint from:
  _currentVersion
  _accuracyAfterLastBatch
  _hasStructuredTraining
  _runtime.KnnStore.ExportCheckpoint()
  _runtime.SemanticTokenizer.ExportCheckpoint()
  _runtime.Executors.Values.Select(ExportCheckpoint)
  _runtime.Verifier.ExportCheckpoint()
  _trainingHistory mapped to checkpoint entries
  optional runner checkpoint only when supplied by runner
```

Do not write disk in `ExportCheckpoint()`.

### 7.5 ImportCheckpoint implementation

Required behavior:

```text
Validate checkpoint not null.
Validate schema version.
Validate MachineId and DomainTag.
Validate _runtime != null.
Set _currentVersion.
Set _accuracyAfterLastBatch.
Set _hasStructuredTraining.
_runtime.KnnStore.ImportCheckpoint(checkpoint.KnnStore)
_runtime.SemanticTokenizer.ImportCheckpoint(checkpoint.SemanticTokenizer)
For each executor checkpoint:
  find matching _runtime.Executors[Direction]
  import
_runtime.Verifier.ImportCheckpoint(checkpoint.QualityVerifier)
Clear and rebuild _trainingHistory.
```

Reject unknown executor direction with:

```text
EXECUTOR_DIRECTION_MISSING
```

### 7.6 SaveCheckpointAsync implementation

Required behavior:

```text
Call ExportCheckpoint().
Create parent directory.
Write JSON to temporary file.
Atomic replace/move into checkpointPath.
Return LearningMachineExport with:
  CheckpointRef = absolute checkpointPath
  ManifestRef = checkpointPath + ".manifest.json" or same directory manifest
  AccuracyAtExport = _accuracyAfterLastBatch
```

Temporary file rule:

```text
checkpointPath + ".tmp"
write tmp
File.Move(tmp, checkpointPath, overwrite: true)
```

### 7.7 LoadCheckpointAsync implementation

Required behavior:

```text
Read checkpoint JSON.
Create new TranslationMachine().
Call ImportCheckpoint(checkpoint).
Return loaded machine.
```

This is the only valid post-restart load path.

### 7.8 Existing ExportAsync/ImportAsync

Do not keep misleading behavior.

Required change:

```text
ExportAsync remains metadata-only and must not be called a checkpoint.
ImportAsync remains metadata-only and must not claim learned behavior restore.
```

Add comments and/or error message:

```text
For trained-state persistence use SaveCheckpointAsync/LoadCheckpointAsync.
```

Existing `TranslationMachine_ExportImportCycle_Succeeds` test must be renamed or narrowed:

```text
TranslationMachine_MetadataExportImportCycle_DoesNotClaimCheckpoint
```

---

## 8. Runner Resume Repair

Files:

```text
27.TranslationDomain/TatoebaPilotLoopRunner.cs
27.TranslationDomain.Tools/Program.cs
```

### 8.1 Current invalid behavior

Current runner:

```text
creates new TranslationMachine
trains initial rows
uses SkipCompletedCycles to replay old cycles without printing them
then starts visible cycles
```

This is forbidden as resume.

### 8.2 New runner options

Replace resume-related options:

```csharp
string? CheckpointPath = null
bool RequireCheckpointResume = false
```

Remove or deprecate:

```text
SkipCompletedCycles
LegacyPromotionThroughCycle
```

If legacy options remain temporarily:

```text
They must be named legacy_replay, not resume.
They must print warning: "REPLAY_NOT_CHECKPOINT".
They must not be used in normal training path.
```

### 8.3 New startup behavior

Runner startup:

```text
if CheckpointPath exists:
  Load TranslationMachine from checkpoint.
  Read TatoebaLoop checkpoint section.
  Set adaptive maxAccuracy = checkpoint.TatoebaLoop.BestRuntimeProductAccuracy
      or fallback 0.78 when loading an older checkpoint.
  If --start-new-source-from-checkpoint is absent:
    Require PrivatePairsPath to match checkpoint.TatoebaLoop.PrivatePairsPath.
    Set cursor = checkpoint.TatoebaLoop.Cursor.
    Set trainingRows from AcceptedTrainingRows.
    Do NOT train previous cycles.
  If --start-new-source-from-checkpoint is present:
    Require PrivatePairsPath to differ from checkpoint.TatoebaLoop.PrivatePairsPath.
    Keep loaded TranslationMachine weights.
    Start the new source loop at cursor 0.
    Train the first block of the new source once.
    Set resumeMode=source-rollover and replayedCycles=0.
    Save checkpoint with the new PrivatePairsPath and preserved maxAccuracy.

else:
  Create new TranslationMachine.
  Take initial 100 train rows.
  Train them once.
  Save checkpoint with cursor after initial train.
```

### 8.4 Per-cycle save behavior

After every cycle:

```text
Always update cursor past candidate rows scanned.
If promotionDecision starts with PROMOTE_NEW_PATTERN_GT_:
  train selected rows
  append AcceptedTrainingRows
  save machine checkpoint
If promotionDecision does not start with PROMOTE_NEW_PATTERN_GT_:
  do not train selected rows
  append SkippedRows
  save loop cursor checkpoint
```

This prevents re-scanning the same skipped sets after restart.

### 8.5 Console output

Every visible 100-set must still print:

```text
ACCURACY_SET
cycle
train_rows
test_rows
newPatternRate
sourceTokenSeenRate
runtimeProductAccuracy
runtimeExactAccuracy
runtimeBootstrapRate
promotedToTraining
promotionMaxAccuracy
minimumPromotionNewPatternRate
maximumPromotionSeenTokenRate
promotionDecision
bestRuntimeProductAccuracy
checkpointPath
checkpointSaved=true|false
checkpointLoaded=true|false
resumeMode=checkpoint|source-rollover|fresh
sourceRolloverFrom=<old path or null>
sourceRolloverTo=<new path or null>
sourceRolloverStartCursor=0|null
```

Forbidden output:

```text
resumeMode=replay
continuing from checkpoint
```

unless a real checkpoint was loaded.

### 8.6 CLI changes

File:

```text
27.TranslationDomain.Tools/Program.cs
```

Add:

```text
--checkpoint <checkpoint.json>
--require-checkpoint-resume true|false
--start-new-source-from-checkpoint
```

`--start-new-source-from-checkpoint` is a flag, not an implicit fallback. It is
required for approved source rollover and forbidden as a silent way to bypass a
checkpoint `PrivatePairsPath` mismatch.

Remove from normal usage text:

```text
--skip-cycles
--legacy-promotion-through-cycle
```

If kept temporarily:

```text
put under "LEGACY/DIAGNOSTIC ONLY - not resume"
```

---

## 9. Test Plan

File:

```text
Tests/TranslationDomainTests.cs
```

### 9.1 Machine checkpoint tests

#### Test 1

```text
TranslationMachine_CheckpointRoundTrip_PreservesExactSourceMap
```

Steps:

```text
1. Train exact source pair through structured executor example.
2. Translate source before save -> chosen output, no Bootstrap.
3. Save checkpoint to temp path.
4. Load fresh machine from checkpoint path.
5. Translate same source after load.
6. Assert output equals before-save output.
7. Assert no Bootstrap.
```

This proves `_exactSourceMap` survives disk.

#### Test 2

```text
TranslationMachine_CheckpointRoundTrip_PreservesWordMap
```

Steps:

```text
1. Train source with word mappings under non-runtime signature.
2. Translate related source via word map before save.
3. Save/load fresh machine.
4. Translate same related source after load.
5. Assert learned words appear.
6. Assert no copied Cyrillic and no Bootstrap.
```

This proves `_wordMap` and `_surfaceWordMap` survive disk.

#### Test 3

```text
TranslationMachine_CheckpointRoundTrip_PreservesPhraseMap
```

Steps:

```text
1. Train pair with [PHRASE: source phrase => target phrase].
2. Translate source requiring phrase map.
3. Save/load.
4. Assert phrase-map translation survives.
```

This proves `_phraseMap` survives disk.

#### Test 4

```text
TranslationMachine_CheckpointRoundTrip_PreservesTokenizerCorrections
```

Steps:

```text
1. Train tokenizer example with chosen pattern/signature.
2. Save/load.
3. Train or evaluate a held-out tokenizer example requiring correction.
4. Assert corrected pattern/signature is used.
```

This proves `_patternCorrections` and `_signatureCorrections` survive disk.

#### Test 5

```text
TranslationMachine_CheckpointRoundTrip_PreservesVerifierRules
```

Steps:

```text
1. Train verifier failure requiring term X.
2. Verify candidate without X before save -> fail.
3. Save/load.
4. Verify same candidate after load -> fail with same issue.
```

This proves verifier rules survive disk.

#### Test 6

```text
TranslationMachine_LoadCheckpoint_RejectsWrongMachineId
```

Steps:

```text
1. Save checkpoint JSON.
2. Modify machine_id to wrong value.
3. Load.
4. Assert failure code MACHINE_ID_MISMATCH.
```

#### Test 7

```text
TranslationMachine_LoadCheckpoint_RejectsWrongSchemaVersion
```

Steps:

```text
1. Save checkpoint JSON.
2. Modify schema_version to unsupported value.
3. Load.
4. Assert failure code CHECKPOINT_SCHEMA_UNSUPPORTED.
```

### 9.2 Runner checkpoint tests

#### Test 8

```text
TatoebaPilotLoopRunner_WritesCheckpointAfterInitialTrain
```

Steps:

```text
1. Run runner with temp private pairs and checkpoint path.
2. Assert checkpoint file exists.
3. Assert checkpoint contains machine state and TatoebaLoop cursor.
4. Assert no replay warning.
```

#### Test 9

```text
TatoebaPilotLoopRunner_ResumeUsesCheckpointWithoutReplay
```

Steps:

```text
1. Run first cycle with checkpoint path.
2. Record checkpoint cursor/train rows.
3. Run again with same checkpoint path.
4. Assert runner loads checkpoint.
5. Assert first visible cycle is after cursor.
6. Assert previous cycles were not trained again.
7. Assert output includes checkpointLoaded=true.
```

Implementation detail for "not trained again":

```text
Use report fields:
  checkpoint_loaded = true
  resume_mode = checkpoint
  replayed_cycles = 0
  initial_training_skipped_due_checkpoint = true
```

If these fields do not exist, add them to report DTO.

For source rollover tests, assert `resume_mode=source-rollover`,
`replayed_cycles=0`, `sourceRolloverFrom`, `sourceRolloverTo`, preserved
`bestRuntimeProductAccuracy`, and that old checkpoint knowledge is still usable
while the new source loop begins from cursor 0.

#### Test 10

```text
TatoebaPilotLoopRunner_SkipCycles_IsNotResume
```

Only if legacy flag remains.

Steps:

```text
1. Invoke legacy replay option.
2. Assert report contains warning REPLAY_NOT_CHECKPOINT.
3. Assert report does not set checkpointLoaded=true.
```

---

## 10. Documentation Changes

### 10.1 Training guide

File:

```text
docs/plans/TRANSLATION-DOMAIN-TRAINING-GUIDE-v1.md
```

Replace:

```text
Resume rule: use --skip-cycles N to rehydrate...
```

with:

```text
Resume rule: use --checkpoint <checkpoint.json>.
Resume is valid only when TranslationMachine.LoadCheckpointAsync loads trained unit state from disk.
--skip-cycles is replay, not resume, and is forbidden for normal training.
```

### 10.2 Reality-check skill

File:

```text
docs/skills/translation-domain-reality-check-SKILL.md
```

Update FAQ-15 after implementation:

```text
Before fix: no real checkpoint.
After fix: checkpoint is valid only if round-trip test passes.
```

### 10.3 Sourcing plan

File:

```text
docs/plans/TRANSLATION-CORPUS-SOURCING-PLAN-v1.md
```

Clarify:

```text
checkpoint = serialized trained unit state
manifest = locator/metadata
manifest without checkpoint file is not trained model persistence
```

---

## 11. Gate B

Execution gate after code implementation:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "TranslationMachine_CheckpointRoundTrip|TranslationMachine_LoadCheckpoint" --nologo /p:UseSharedCompilation=false
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "TatoebaPilotLoopRunner_WritesCheckpoint|TatoebaPilotLoopRunner_ResumeUsesCheckpoint" --nologo /p:UseSharedCompilation=false
dotnet build 27.TranslationDomain\27.TranslationDomain.csproj --nologo /p:UseSharedCompilation=false
dotnet build 27.TranslationDomain.Tools\27.TranslationDomain.Tools.csproj --nologo /p:UseSharedCompilation=false
git diff --check
```

Expected:

```text
0 failed
0 build errors
checkpoint file exists in temp test path
fresh machine after LoadCheckpointAsync returns learned output without Bootstrap
runner resume reports checkpointLoaded=true
runner resume reports replayed_cycles=0
normal usage no longer presents --skip-cycles as resume
```

---

## 12. Bounded Sub-Agent Work Order

Implementation may start only after this plan is approved and only through a
bounded sub-agent. The parent architect session must not become executor.

Bounded work order:

```text
Sub-agent model: gpt-5.5 Extra High
Tool model: gpt-5.5
Reasoning effort: xhigh
Provider equivalent: sonnet latest with High reasoning when configured available
Implement exactly the phase/slice named by architect in the work order.
Do not treat "phases 1-7" as blanket continuation authority.
One work order = one code slice or one training chunk, then return packet.
Do not start a training run.
Do not change corpus selection.
Do not continue 100+100 loop.
Only prove disk save/load and runner checkpoint resume.
Allowed commands are the Gate B commands in section 11 and `git diff --check`.
Forbidden in architect parent session: dotnet tests, builds, dotnet run,
training runners, Start-Process, production code edits, test code edits.
Checkpoint writer rules from section 0.1 and
`docs/plans/TRANSLATION-CHECKPOINT-WRITER-GUARD-HOWTO.md` apply before any
post-repair training writer is issued.
Stage only files touched by this repair.
Commit once after Gate B passes.
Push current branch.
Return evidence packet to architect:
  files changed
  tests run
  checkpoint test results
  git commit hash
  push result
```

---

## 13. Return To Training After Repair

After Gate B passes and commit/push are complete:

```text
1. Architect prepares a bounded 100-train/100-test packet for a
   `gpt-5.5 Extra High` (`gpt-5.5`, `reasoning_effort=xhigh`) training
   sub-agent, or `sonnet latest` with `High` reasoning when that is the
   configured available provider.
2. Training sub-agent starts or resumes Tatoeba loop only with --checkpoint.
3. No --skip-cycles as resume.
4. Strict adaptive novelty gate:
   newPattern > (1 - maxAccuracy)
   AND
   seenTokens < maxAccuracy
   Initial fallback maxAccuracy=78% means newPattern > 22% AND seenTokens < 78%;
   checkpoint resume must use the persisted best runtime product accuracy when present.
5. Print ACCURACY_SET after every 100-row test set.
6. Return deltas to architect:
   accuracy %, exact %, bootstrap %, new tokens %, new structures %,
   newPattern %, seenTokens %, promotion max/thresholds, train_rows delta, promotionDecision.
7. Active target remains current user target: 85% until Luba changes it.
8. Architect must assign one checkpoint path and one writer sub-agent for each
   training chunk. A second writer must not use that checkpoint until the first
   writer returns or is closed/stopped after read-only inspection.
9. The training chunk must heartbeat every 10-15 minutes or after each 29-cycle
   chunk and must return after the bounded chunk instead of chaining another
   source rollover or runner continuation.
10. Every 100-set `ACCURACY_SET` must include product, exact, bootstrap,
    train_rows, newPattern, seenTokens, promoted, and promotionDecision.
11. If `MeetsPilotGate=false` or unknown for the source, the active checkpoint
    is read-only and the runner must return evidence instead of saving.
```

Training cannot honestly continue across process restarts until this checkpoint repair is implemented.
