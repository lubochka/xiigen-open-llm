> **STATUS: SUPERSEDED — 2026-06**
> This v17 plan is superseded by ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md.
> Phase 37-40 were implemented per v20. This document is kept for historical reference only.
> For current architecture and gate commands, see docs/plans/ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md.

---

# ORCHESTRATION-MACHINE-TRAINING-PLAN-v17

Train `OrchestrationPlanMachine` with explicit three-mode architecture.

This v17 plan supersedes the pasted v16 plan in the current Codex thread. It keeps the v16 design, but applies the executor-blocking fixes found during architect review and raises the Mode 2 minimum accuracy target to 0.85.

## Main Goal

Implement a working `OrchestrationPlanMachine : ILearningMachine<OrchestrationPlanDomain>` that trains and serves `TrainedRouteComposerUnit` through three modes:

- Mode 1: `KnownFlowRetrieval`, deterministic `FlowIndex` source authority, not ML.
- Mode 2: `Mode2Generalization`, ML generation from KNN + pattern weights.
- Mode 3: `Mode3HumanExplanation`, ML human-readable explanation from grok explanation sessions.

Gate B is complete only when:

- `knownFlowRetrievalRouteAccuracy >= 0.90`
- `overallTrainingAccuracy == unseenStyleAccuracy`
- `unseenStyleAccuracy >= 0.85`
- `humanExplanationAccuracy >= 0.70` when Mode 3 pairs are present
- `forbiddenScopeViolationRate == 0.000`
- `phaseOnlyCompliance == 1.000`
- `approvalGateCompliance == 1.000`
- all Gate B build/test/CLI/resume checks pass

If `unseenStyleAccuracy < 0.85`, that is not a blocker. The executor must continue with corpus, scoring, routing, feature, and training-loop fixes until the target is met or a real OP-13 blocker is recorded.

## Source Materials

| Source | Path | Notes |
| --- | --- | --- |
| FLOW docs | `C:\Users\lubao\Documents\xiigen\claude-sessions\claude orchestrator\` | 42 FLOW + adapters/scenarios; two format variants |
| Grok explanations | `C:\Users\lubao\Documents\xiigen\claude-sessions\grok  orchestrator\` | two spaces in folder name; Mode 3 corpus source |
| Workset | `C:\Users\lubao\Documents\xiigen\claude-sessions\sonnet-user-prompts\_workset.csv` | Folder,UUID,Name,MessageCount,FilePath |
| Accepted prompts | `C:\Users\lubao\Documents\xiigen\claude-sessions\sonnet-user-prompts\sonnet - accepted user prompts.md` | primary PromptFlowIndex source |
| Approved orchestration | `C:\Users\lubao\Documents\xiigen\claude-sessions\sonnet-user-prompts\sonnet - approved orchestration.md` | secondary PromptFlowIndex source |
| Typo file | `C:\Users\lubao\Documents\xiigen\claude-sessions\user prompts\accepted orchestration pattertns.md` | tertiary PromptFlowIndex source; filename typo is real |

## v17 Critical Fixes

### CF-1: FlowIndex Retrieval Is Allowed Only For KnownFlowPlanning

`GeneratePlanAsync` must not retrieve a canonical FLOW doc for non-retrieval intents.

Required route gate:

```csharp
if (contract.Kind == IntentKind.KnownFlowPlanning)
{
    var flowRef = contract.AllowedFlowScope.FirstOrDefault();
    if (flowRef is not null &&
        !contract.ForbiddenFlowScope.Contains(flowRef) &&
        _flowIndex.TryGetValue(flowRef, out var chosen))
    {
        return Ok(new OrchestrationGenerationResult(
            SerializeChosen(chosen),
            GenerationRouteMode.KnownFlowRetrieval,
            boundarySource,
            RetrievalSource.FlowIndex,
            false,
            contract.Kind,
            warnings));
    }
}
```

Examples that must route to Mode 2, not Mode 1, even when they mention a FLOW:

- `translate flow 33 from python/react native to node/react`
- `phase 1-2.4 are done, proceed 2.5 phase only`
- `review architecture for FLOW-33`
- `create testing plan for FLOW-33`

### CF-2: PromptFlowIndex Merge Priority

`PromptFlowIndex` is a boundary contract source, not a plan-content source.

Merge priority:

- Classifier always runs first.
- PromptFlowIndex may win `AllowedFlowScope`, `ForbiddenFlowScope`, `PhaseScope`, `RequiresApprovalGate`, `RequiresSaveState`, and human constraint union.
- Classifier wins intent for explicit non-retrieval intents:
  `SkillLoadSessionInit`, `HumanOrchestrationExplanation`, `PhaseContinuation`, `DocumentationMigration`, `ArchitectureReview`, `TestingPlan`.
- PromptFlowIndex may upgrade only a fallback/generic `ImplementationExecutionRequest` to `KnownFlowPlanning` when an accepted prompt has a documented FLOW contract.
- PromptFlowIndex must not override explicit explanation keywords into Mode 1.
- PromptFlowIndex does not index grok sessions in this phase. Mode 3 fires only on explanation keywords.

### CF-3: FlowReferenceDetector Handles `1-st flow`

Ordinal digit regex must allow an optional hyphen or spaces between number and suffix:

```regex
(?:the\s+)?(\d{1,2})\s*-?\s*(?:st|nd|rd|th)\s+flow
```

The detector must continue to catch:

- `FLOW-33`
- `FLOW33`, `flow 33`, `flow-33`
- `33*`
- `31-*`, `31-functionality`
- `first flow`, `second flow`, up to `tenth flow`
- `1-st flow`, `1st flow`, `the 1-st flow`
- forbidden-context bare numbers such as `34 not now`

### CF-4: Mode 3 Held-Out Split Is By FlowRef Groups

Mode 3 train/eval split must group by inferred `FlowRef`, not by row index.

Required corpus gates:

- `Mode3TrainingPairs >= 30`
- `Mode3HeldOutEvalPairs >= 20`
- `NoMode3SourceBundleOverlap = true`
- `NoMode3FlowRefOverlap = true`
- `Mode3ExplanationPairs = Mode3TrainingPairs + Mode3HeldOutEvalPairs >= 50`

Generic prompt text overlap is allowed. Leakage is defined by attached file bundle and inferred FlowRef overlap.

### CF-5: Constructor Defaults For Fresh Machine Tests

`OrchestrationPlanMachine()` must compile and work without `privateCorpusDir`.

Required default initialization:

```csharp
private readonly Dictionary<string, OrchestrationPlanChosen> _flowIndex = new();
private readonly PromptFlowIndex _promptFlowIndex = PromptFlowIndex.Empty;
```

If `privateCorpusDir is not null`, reload both from `OrchestrationCorpusAudit.LoadFlowIndexAndPromptIndex(privateCorpusDir)`.

### CF-6: Mode 2 Eval Purity

Mode 2 held-out evaluation pairs must satisfy:

```text
boundarySource=Classifier AND retrievalSource=None AND mlGenerated=true
```

Any Mode 2 eval pair with `boundarySource=PromptFlowIndex` is corpus contamination.
Any Mode 2 eval pair with `retrievalSource=FlowIndex` is FLOW-ref leakage.

Mode 3 eval pairs must satisfy:

```text
retrievalSource=None AND mlGenerated=true AND routeMode=Mode3HumanExplanation
```

For this phase, Mode 3 should normally have `boundarySource=Classifier` because PromptFlowIndex does not index grok sessions.

### CF-7: Checkpoint Crash Window Is Closed By One Combined Batch

Each cycle must use exactly one combined `TrainOrchestrationBatchAsync` call:

1. Select Mode 2 train pairs not in `TrainedPairIds`.
2. Select Mode 3 train pairs not in `TrainedPairIds`.
3. Combine into one `OrchestrationTrainingBatch`.
4. Call `TrainOrchestrationBatchAsync` once.
5. Add all batch PairIds to `TrainedPairIds`.
6. Save checkpoint immediately with all ML state and `TrainedPairIds`.
7. Evaluate and decide promotion.
8. Save final checkpoint at end of run.

Separate per-mode training calls are forbidden.

## Public Contracts

The implementation should add the following domain contracts in `42.ProjectWikiDomain/OrchestrationPlanContracts.cs`.

```csharp
public sealed record FlowDetectionResult(
    IReadOnlyList<string> AllRefs,
    IReadOnlyList<string> ForbiddenCandidates);

public enum PairMode
{
    Mode1KnownFlowRetrieval,
    Mode2Generalization,
    Mode3HumanExplanation
}

public sealed record OrchestrationGenerationResult(
    string PlanText,
    GenerationRouteMode RouteMode,
    BoundarySource BoundarySource,
    RetrievalSource RetrievalSource,
    bool MlGenerated,
    IntentKind IntentKind,
    IReadOnlyList<string> Warnings);

public enum GenerationRouteMode
{
    KnownFlowRetrieval,
    Mode2Generalization,
    Mode3HumanExplanation,
    Passthrough
}

public enum BoundarySource
{
    Classifier,
    PromptFlowIndex
}

public enum RetrievalSource
{
    FlowIndex,
    None
}

public sealed record OrchestrationLoopCheckpoint(
    string PrivatePairsPath,
    int LastCompletedCycle,
    int Cursor,
    IReadOnlyList<string> TrainedPairIds,
    IReadOnlyList<string> Mode2PromotedPairIds,
    IReadOnlyList<string> Mode3PromotedPairIds);
```

`TrainBatchAsync(LearningBatch<OrchestrationPlanDomain>)` is only an adapter. The runner calls `TrainOrchestrationBatchAsync(OrchestrationTrainingBatch, ct)`.

Adapter mapping to real core contract:

- `LearningTrainingItem.ItemId = pair.PairId`
- `LearningTrainingItem.Prompt = pair.Prompt`
- `LearningTrainingItem.ChosenResponse = JsonSerializer.Serialize(pair)`
- `LearningTrainingItem.RejectedResponse = pair.RejectedText`

## GeneratePlanAsync Required Route Order

```csharp
var contract = HumanIntentBoundaryClassifier.Classify(prompt);
var warnings = new List<string>();
var boundarySource = BoundarySource.Classifier;

if (_promptFlowIndex.TryGetContract(prompt, out var knownContract) && knownContract is not null)
{
    contract = MergeContracts(contract, knownContract);
    boundarySource = BoundarySource.PromptFlowIndex;
}

if (contract.Kind == IntentKind.SkillLoadSessionInit)
{
    return Ok(new OrchestrationGenerationResult(
        "",
        GenerationRouteMode.Passthrough,
        boundarySource,
        RetrievalSource.None,
        false,
        contract.Kind,
        []));
}

if (contract.RequiresApprovalGate)
    warnings.Add("PLAN_ONLY_PENDING_APPROVAL");

if (contract.Kind == IntentKind.HumanOrchestrationExplanation)
{
    var nearest = _humanExplanationKnn.FindNearest(prompt);
    var text = RenderHumanExplanation(nearest, _humanExplanationPatternMap, contract, prompt);
    return Ok(new OrchestrationGenerationResult(
        text,
        GenerationRouteMode.Mode3HumanExplanation,
        boundarySource,
        RetrievalSource.None,
        true,
        contract.Kind,
        warnings));
}

if (contract.Kind == IntentKind.KnownFlowPlanning)
{
    var flowRef = contract.AllowedFlowScope.FirstOrDefault();
    if (flowRef is not null &&
        !contract.ForbiddenFlowScope.Contains(flowRef) &&
        _flowIndex.TryGetValue(flowRef, out var chosen))
    {
        return Ok(new OrchestrationGenerationResult(
            SerializeChosen(chosen),
            GenerationRouteMode.KnownFlowRetrieval,
            boundarySource,
            RetrievalSource.FlowIndex,
            false,
            contract.Kind,
            warnings));
    }
}

if (contract.AllowedFlowScope.Any())
    warnings.Add("retrieval_miss_fallback_to_mode2");

var nearestPlan = _knn.FindNearest(prompt);
var planText = ApplyPatternMapOverlay(nearestPlan, _patternMap, prompt, contract.PhaseScope);
return Ok(new OrchestrationGenerationResult(
    planText,
    GenerationRouteMode.Mode2Generalization,
    boundarySource,
    RetrievalSource.None,
    true,
    contract.Kind,
    warnings));
```

## Files In Scope

Allowed code files:

- `42.ProjectWikiDomain/OrchestrationPlanDomain.cs`
- `42.ProjectWikiDomain/OrchestrationPlanContracts.cs`
- `42.ProjectWikiDomain/FlowReferenceDetector.cs`
- `42.ProjectWikiDomain/HumanIntentBoundaryClassifier.cs`
- `42.ProjectWikiDomain/OrchestrationKnnStore.cs`
- `42.ProjectWikiDomain/TrainedRouteComposerUnit.cs`
- `42.ProjectWikiDomain/OrchestrationPlanOrchestrator.cs`
- `42.ProjectWikiDomain/OrchestrationPlanMachine.cs`
- `42.ProjectWikiDomain/OrchestrationCorpusAudit.cs`
- `42.ProjectWikiDomain/OrchestrationCorpusBuilder.cs`
- `42.ProjectWikiDomain/OrchestrationPilotLoopRunner.cs`
- `27.TranslationDomain.Tools/Program.cs`
- `Tests/OrchestrationPlanTests.cs`

Allowed support/governance files:

- `docs/plans/ORCHESTRATION-MACHINE-TRAINING-PLAN-v17.md`
- `docs/plans/ORCHESTRATION-MACHINE-TRAINING-STATE.json`
- generated private reports/checkpoints under ignored artifact paths only

Do not modify Bootstrap units, orchestrator registry in `15.OrchestratorModel`, unrelated tests, or unrelated user changes.

## Tests

Add 36 tests in `Tests/OrchestrationPlanTests.cs`, preserving the v16 test list and adding assertions for v17 fixes inside the existing relevant tests:

- `DocumentationMigration` with `flow 33` routes Mode 2, not `KnownFlowRetrieval`.
- `PhaseContinuation` with a flow mention routes Mode 2, not `KnownFlowRetrieval`.
- `1-st flow` is detected.
- Mode 2 eval purity requires `BoundarySource.Classifier`.
- Resume report uses `trainedPairIdsUsedForExclusionOnly`, not `acceptedPairIdsUsedForSelectionOnly`.
- Mode 3 split is by FlowRef group.
- Fresh `new OrchestrationPlanMachine()` works without corpus dir.

Keep total test count at 36 by extending existing tests rather than adding a new count unless the plan and Gate B are updated consistently.

## Gate B

The parent architect session must not run these commands. The sub-agent executor runs them literally.

```powershell
# B.0 audit
dotnet run --project 27.TranslationDomain.Tools -- audit-orchestration-corpus `
  --source-dir "C:\Users\lubao\Documents\xiigen\claude-sessions" `
  --output "C:\...\orchestration-corpus-audit.json"

# B.1 corpus build
dotnet run --project 27.TranslationDomain.Tools -- build-orchestration-corpus `
  --source-dir "C:\Users\lubao\Documents\xiigen\claude-sessions" `
  --audit "C:\...\orchestration-corpus-audit.json" `
  --output "C:\...\orchestration-pilot-private.jsonl"

# B.2 baseline
dotnet test Tests/LlmFromScratch.Tests.csproj `
  --filter "TranslationDomain|ProjectWikiDomain|OrchestrationPlan" --nologo

# B.3 new tests
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "OrchestrationPlan" --nologo

# B.4 builds
dotnet build 42.ProjectWikiDomain/42.ProjectWikiDomain.csproj --nologo
dotnet build 27.TranslationDomain.Tools/27.TranslationDomain.Tools.csproj --nologo

# B.5 smoke training
dotnet run --project 27.TranslationDomain.Tools -- run-orchestration-loop `
  --private-pairs "C:\...\orchestration-pilot-private.jsonl" `
  --corpus-dir "C:\Users\lubao\Documents\xiigen\claude-sessions" `
  --train-size 50 --test-size 50 --mode3-train-size 20 --max-cycles 29 `
  --retrieval-target 0.90 --style-target 0.85 --explanation-target 0.70 `
  --checkpoint "C:\...\orchestration-machine-checkpoint.json" `
  --report "C:\...\orchestration-loop-report-cycles-001-029.json"

# B.6 resume gate
dotnet run --project 27.TranslationDomain.Tools -- run-orchestration-loop `
  --private-pairs "C:\...\orchestration-pilot-private.jsonl" `
  --corpus-dir "C:\Users\lubao\Documents\xiigen\claude-sessions" `
  --train-size 50 --test-size 50 --mode3-train-size 20 --max-cycles 3 `
  --retrieval-target 0.90 --style-target 0.85 --explanation-target 0.70 `
  --checkpoint "C:\...\orchestration-machine-checkpoint.json" `
  --require-checkpoint-resume `
  --report "C:\...\orchestration-loop-report-resume.json"
```

### Gate B.2 Equivalent Aggregate Exception

Architect repair for phase 37: the literal combined B.2 filter may be satisfied by
three serial scoped sub-runs when the combined command leaves orphaned process
trees or repeatedly times out without a test summary.

Replacement workers must re-inspect the exact combined-filter process roots before
continuing the aggregate exception, stop only those exact roots and children if
still alive, and record the PID action in state before running scoped sub-runs.

Equivalent B.2 coverage must preserve the exact discovered selected coverage:

- `OrchestrationPlan = 36`
- `ProjectWikiDomain = 81`
- `TranslationDomain = 88`
- total `205`

Allowed equivalent B.2 commands:

```powershell
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "OrchestrationPlan" --nologo
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "ProjectWikiDomain" --nologo
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "TranslationDomain" --nologo
```

This is an equivalent aggregate exception only for B.2. It is not a stop gate and
does not relax B.3, B.4, B.5, B.6, or the `unseenStyleAccuracy >= 0.85` target.
If a scoped sub-run times out, isolate within the same filter to find the hanging
method, fix any phase-introduced issue, or record existing long-running tests as
equivalent coverage only when all selected test names are accounted for and no
failures occur.

If a scoped `ProjectWikiDomain` sub-run fails only because an existing local
project wiki under another repository is stale or incomplete, or because a
pre-existing ProjectWiki expectation mismatch is present in clean files not
modified by phase 37, record exact evidence in state and exclude only those
unrelated tests from phase 37 B.2 equivalence. The remaining `ProjectWikiDomain`
selected tests must still be partitioned, run, and accounted for before the
aggregate exception can cover the unaffected ProjectWiki scope.

B.5 assertions:

- `checkpoint_written = true`
- `replayed_cycles = 0`
- `flowIndexLoaded = true`
- `overallTrainingAccuracy = unseenStyleAccuracy`
- rolling average of last 3 `unseenStyleAccuracy` values is `>= 0.85`
- every Mode 2 eval pair has `boundarySource=Classifier AND retrievalSource=None AND mlGenerated=true`
- every Mode 3 eval pair has `retrievalSource=None AND mlGenerated=true AND routeMode=Mode3HumanExplanation`
- `knownFlowRetrievalRouteAccuracy >= 0.90`
- `forbiddenScopeViolationRate = 0.000`
- `phaseOnlyCompliance = 1.000`
- `approvalGateCompliance = 1.000`
- fixture `flow 33` retrieves `FLOW-33`
- fixture `31-* documents attached` retrieves `FLOW-31`
- fixture `start with the 1-st flow` retrieves `FLOW-01`
- fixture `translate flow 33 from python/react native to node/react` routes Mode 2, not Mode 1
- fixture `phase 1-2.4 are done, proceed 2.5 phase only` routes Mode 2 with phase scope only
- fixture `33-system initiation.md объясни` routes Mode 3
- fixture `33* files, 34 not now` excludes `FLOW-34`
- fixture `load planning skills` passthrough

B.6 assertions:

- `trainedStateLoaded = true`
- `replayed_cycles = 0`
- `cursor > 0`
- `initialTrainingSkippedDueCheckpoint = true`
- `resumeTrainBatchCalls = 0`
- `trainedPairIdsUsedForExclusionOnly = true`
- `trainedPairIdsRestoredCount > 0`
- `mode2TrainedPairsRestored > 0`
- `mode3TrainedPairsRestored > 0` if Mode 3 training ran in B.5
- `forbiddenScopeViolationRate = 0.000`
- `unseenStyleAccuracy >= 0.85`

## Governance Block

State file:

`docs/plans/ORCHESTRATION-MACHINE-TRAINING-STATE.json`

The executor must create/update it:

- before first implementation edit
- after every numbered step
- after every review
- after each Gate B command
- after commit/push attempts
- before any final response

Minimum state schema:

```json
{
  "phase": "37",
  "goal": "OrchestrationPlanMachine three-mode training foundation with unseenStyleAccuracy >= 0.85",
  "goal_reached": false,
  "stop_allowed": false,
  "current_role": "EXECUTOR_INTERNAL",
  "next_action": "implement v17 contracts",
  "last_completed_step": null,
  "gate_status": "IN_PROGRESS_NEXT_STEP_KNOWN",
  "execution_score": {
    "current_score": 0,
    "progress_step_count": 0,
    "premature_stop_count": 0,
    "severe_violation_count": 0
  },
  "hole_register": [],
  "completion_matrix": []
}
```

Completion matrix rows:

- CM-1 code compiles for `42.ProjectWikiDomain`
- CM-2 tools project compiles
- CM-3 36 OrchestrationPlan tests pass
- CM-4 audit corpus gates pass
- CM-5 corpus build gates pass
- CM-6 smoke loop reaches `unseenStyleAccuracy >= 0.85`
- CM-7 Mode 2 eval purity holds
- CM-8 hard compliance gates hold every cycle
- CM-9 resume gate proves no retrain replay and restored weights
- CM-10 one phase commit exists with correct message
- CM-11 current branch push succeeds, unless remote/auth is a real OP-13 blocker

Anti-loopholes:

- Do not stop after writing a report when `unseenStyleAccuracy < 0.85`.
- Do not call sub-0.85 accuracy "done", "ready", or "accepted".
- Do not use PromptFlowIndex as RetrievalSource.
- Do not use FlowIndex for non-`KnownFlowPlanning` intents.
- Do not train restored `TrainedPairIds` on resume.
- Do not commit private corpus, private JSONL, checkpoints, or reports containing private corpus text.
- Do not switch branches.
- Do not run ad-hoc `dotnet run`; only the three Gate B CLI commands are allowed.

Commit message after Gate B:

```text
phase 37: OrchestrationMachine three-mode training foundation — orchestration plan machine, corpus, loop, CLI, tests
```

Push the current branch after the phase commit if the existing remote/auth path permits it. If push is unavailable, record the exact git error in state and final response.

## OP-41 Executor Protocol

Parent session is architect-only. Execution is sub-agent only.

Required sub-agent:

- model `gpt-5.5`
- reasoning `xhigh`
- label/role equivalent to `gpt-5.5 Extra High`

Allowed commands:

- `dotnet build` for scoped projects in Gate B
- `dotnet test` with Gate B filters only
- the three exact CLI command families in Gate B
- `git status`, `git diff`, `git add`, `git commit`, `git push`

Forbidden:

- ad-hoc `dotnet run`
- `--skip-cycles`
- accessing private corpus except through the Gate B CLI entrypoints
- branch switching
- destructive git operations
