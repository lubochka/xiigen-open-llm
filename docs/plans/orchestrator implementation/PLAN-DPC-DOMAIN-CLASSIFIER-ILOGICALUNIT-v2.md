# PLAN-DPC-DOMAIN-CLASSIFIER-ILOGICALUNIT-v2

Date: 2026-05-07
Status: planning document
Supersedes: `PLAN-DPC-DOMAIN-CLASSIFIER-ILOGICALUNIT-v1.md`
Addresses: A61, raw developer-prompt classification, non-extensible deterministic services, experiment/source-of-truth drift

## Locked Decisions

1. Project 23 is the semantic source of truth.
   `23.SemanticCompilerAdapter` owns compiled prompt representation, domain-classification core logic, attributed segment core logic, and training vector construction.

2. Project 21 owns `ILogicalUnit` wrappers.
   Project 21 references project 23. Project 23 must not reference project 21. Therefore project-23 core classes do not implement `ILogicalUnit`; project-21 wrappers do.

3. No raw developer prompt may enter domain classification, teaching text, tokenizer training, generated capability lookup, transition classification, internal gateway routing, or attributed segment routing when a compiled prompt is available.

4. Role strings remain registry contract keys only.
   Runtime code must not depend on a deterministic class directly. It resolves an `ILogicalUnit` through the registry by role/domain contract, with a deterministic wrapper only as fallback. Learned units can replace or extend the deterministic unit through the same contract.

5. E13B expression completion is not a developer-prompt path.
   It uses expression-specific normalization (`A+B=?` -> `a + b =`) and should not run through `DevelopmentPromptCompiler`. It still must be exposed as an `ILogicalUnit` wrapper if promoted.

## Current State Before Execution

`main` is pushed at the deeper-model/back-translation commit.

There is a partial uncommitted A61 patch in the working tree:

- `AdaptiveLogicalOrchestrator` partially compiles before classifying.
- `PromptDomainClassifier` partially reads compiled metadata.
- `DomainClassifierUnit` has `EncodePredicateProfile`.
- `PromptRoutingInputCompilerUnit` no longer includes raw prompt text in `CompiledText`.
- Two tests were added.

Targeted tests passed: 17/17.

Full suite failed: 3 failures, 1318 passing, 1321 total.
Known failure class: transition low-confidence behavior changed because compiled-predicate evidence inflated confidence and caused follow-up branches that should have been skipped.

Implementation must continue from the current working tree. Do not revert unless explicitly instructed.

## Phase 0 - Stabilize The Partial Patch

Goal: restore the suite to green before adding larger structural changes.

Files:

- `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`
- `21.AgentInfrastructure/Logical/PromptDomainClassifier.cs`
- `23.SemanticCompilerAdapter/DomainClassifierUnit.cs`
- `Tests/AdaptiveLogicalOrchestratorTests.cs`
- `Tests/PromptRoutingInputCompilerUnitTests.cs`

Work:

1. Keep compile-before-classify in `RunAsync`.
2. Keep transition prompts compiled before transition classification.
3. Fix confidence inflation:
   - `compiled_predicate_profile` is evidence, but it must not automatically make ambiguous text high confidence.
   - If compiled predicate slots are empty, confidence must stay at the fallback floor.
   - If predicate slots exist only because of generic default domain text, confidence must not cross the transition follow-up floor by itself.
4. Re-run the three failing transition tests and full suite.

Gate:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "TransitionSignal"
dotnet test Tests\LlmFromScratch.Tests.csproj
```

Expected: full suite green. New live floor is whatever the green suite reports.

## Phase 1 - Introduce Compiled Domain Classification Contract

Goal: make compiled classification an explicit contract rather than metadata tricks.

Project 23 files:

- `23.SemanticCompilerAdapter/DomainClassifierUnit.cs`

Project 21 files:

- `21.AgentInfrastructure/Logical/CompiledDomainClassifierUnit.cs` (new)
- `21.AgentInfrastructure/Logical/PromptDomainClassifier.cs`
- `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`

Changes:

1. Extend project-23 request shape:

```csharp
public sealed record DomainClassifierRequest(
    string Prompt,
    int[]? Predicates = null,
    string[]? DevelopmentPredicateIds = null,
    string[]? PipelinePredicates = null,
    string? NormalizedText = null,
    SemanticCompiler29.Domain? CompilerDomain = null,
    object? PriorContext = null,
    IReadOnlyDictionary<string, object>? Metadata = null);
```

2. Keep `DomainClassifierUnit` as project-23 core class.
   It exposes:
   - `Classify(DomainClassifierRequest request)`
   - `Train(IReadOnlyList<(int[] Predicates, Domain Domain)> examples)`
   - `EncodePredicateProfile(IEnumerable<string> predicateIds)`
   - `BuildRequestFromCompilation(DevelopmentPromptCompilation compiled, ...)`

3. Add project-21 wrapper:

```csharp
public sealed class CompiledDomainClassifierUnit
    : ILogicalUnit<DomainClassifierRequest, DomainClassification>
```

The wrapper contains no domain logic. It delegates to project-23 `DomainClassifierUnit`.

4. Keep `PromptDomainClassifierUnit` only as a compatibility wrapper for old callers that still pass `string`.
   It must internally create a `DomainClassifierRequest`.

5. Update `AdaptiveLogicalOrchestrator.ClassifyDomainAsync` to resolve:

```csharp
ResolveActiveUnit<DomainClassifierRequest, DomainClassification>(
    flags,
    CompiledDomainClassifierUnit.Role,
    CompiledDomainClassifierUnit.DomainTag,
    new CompiledDomainClassifierUnit())
```

It must pass the compiled request object, not `string`.

Gate:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "DomainClassifierUnit|CompiledDomainClassifier"
dotnet test Tests\LlmFromScratch.Tests.csproj
```

## Phase 2 - Compile Once And Reuse Everywhere

Goal: remove duplicate raw-prompt semantic scans from runtime paths.

Files:

- `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`
- `21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs`
- `21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs`
- `21.AgentInfrastructure/Logical/GeneratedCapabilityFlowFactory.cs`
- `21.AgentInfrastructure/Logical/GeneratedCapabilityFlow.cs`
- `experiments-v2/XIIGEN-DOCS-BATCH-COMPARISON/runner/XiigenDocsBatchComparisonRunner/Program.cs`

Runtime order:

```text
raw prompt
  -> PromptRoutingInputCompilerUnit
  -> DevelopmentPromptCompilation / SubPhaseRouterSourceDocument
  -> CompiledDomainClassifierUnit
  -> MultiSubPhaseRouterUnit
  -> flows / generated capability lookup
```

Required checks:

1. `RunAsync` compiles before domain classification.
2. `FollowTransitionSignalsAsync` compiles transition prompts before classification.
3. `RunWithCapabilityGenerationAsync` reads training domain from metadata/result first. It must not call raw `PromptDomainClassifier.Classify(request.UserRequest)` when compiled/domain metadata is present.
4. `DefaultLogicalCapabilityTrainer.BuildTrainingText` uses compiled prompt text and vector.
5. `BuildTokenizer` seeds from compiled canonical predicate seed.
6. `GeneratedCapabilityFlow.ExecuteAsync` compiles the runtime input before learned-unit lookup.
7. Batch runner builds `OriginalRequest.UserRequest` from actual row prompts, compiles it, and passes `CompiledPrompt`.

Gate:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "DevelopmentPromptCompilerTeachingIntegration|GeneratedCapabilityFlow|DefaultLogicalCapabilityTrainer"
dotnet test Tests\LlmFromScratch.Tests.csproj
```

## Phase 3 - Make Attributed Segment Routing Extensible

Goal: E24 path uses project-23 compiled segments and is replaceable by learned units.

Project 23:

- `23.SemanticCompilerAdapter/AttributedSegmentRouter.cs`

Project 21:

- `21.AgentInfrastructure/Logical/AttributedSegmentRouterUnit.cs` (new)
- `21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs`

Changes:

1. Project-23 `AttributedSegmentRouter` remains core logic.
2. It routes `DevelopmentPromptCompilation.Segments`, not raw full prompt.
3. It passes each segment's compiled predicate ids into `DomainClassifierUnit`.
4. Project-21 `AttributedSegmentRouterUnit` implements:

```csharp
ILogicalUnit<DevelopmentPromptCompilation, AttributedSegmentDocument[]>
```

5. `PromptRoutingInputCompilerUnit` resolves `AttributedSegmentRouterUnit` through the unit registry when available; deterministic wrapper fallback is allowed.

Gate:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "AttributedSegmentRouter"
dotnet test Tests\LlmFromScratch.Tests.csproj
```

## Phase 4 - Teach The Domain Classifier Through The Same Learning Path

Goal: E18 KNN path is actually trained in production, not only in the experiment.

Files:

- `21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs`
- `21.AgentInfrastructure/Logical/CompiledDomainClassifierUnit.cs`
- `23.SemanticCompilerAdapter/DomainClassifierUnit.cs`

Changes:

1. During `TrainAndRegisterAsync`, collect `(compiled predicate profile, domain)` from each trained branch/domain group.
2. Store these examples in the same checkpoint/adapter training path used by other learned units.
3. Resolve the active compiled domain classifier through the registry and call `Train`.
4. Re-register the trained classifier as probationary/trusted according to the existing trust promotion path.
5. Never train a fresh isolated classifier object that the orchestrator cannot resolve.

Test requirements:

- After a teaching run, the registry-resolved classifier reports that the trained path was used.
- A trained predicate profile can override raw keyword fallback.
- Training examples survive cache clearing and batch-boundary reconstruction through persisted checkpoint/manifest state.

Gate:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "DomainClassifierTraining|DefaultLogicalCapabilityTrainer"
dotnet test Tests\LlmFromScratch.Tests.csproj
```

## Phase 5 - InternalLlmGateway Uses Compiled Logical Unit

Goal: internal model routing does not keep a private frozen classifier.

Files:

- `21.AgentInfrastructure/Loop/InternalLlmGateway.cs`

Changes:

1. Remove direct primary keyword scoring over `SemanticCompiler29.RouteClassifier.MatchedKeywords`.
2. Compile the prompt with `SemanticTeachingPromptCompiler`.
3. Build a `DomainClassifierRequest` from the compilation.
4. Resolve `CompiledDomainClassifierUnit` from registry when available.
5. Fall back to deterministic wrapper only when no registry is supplied.
6. Keep `InternalModelDomain` as adapter-key enum only.

Gate:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "InternalLlmGateway"
dotnet test Tests\LlmFromScratch.Tests.csproj
```

## Phase 6 - Experiment Source-Of-Truth Cleanup

Goal: successful experiments that still execute in the suite must not own production prompt logic copies.

Files to inspect and update:

- `35.ExperimentsV2/Experiments/E18*`
- `35.ExperimentsV2/Experiments/E24*`
- `35.ExperimentsV2/Experiments/E51A*`
- Any experiment-local `TextToPredicates`, `DomainClassifier`, `AttributedRouter`, or prompt keyword scan used by a passing experiment.

Changes:

1. Replace local raw prompt predicate conversion with `SemanticTeachingPromptCompiler`.
2. Replace local domain classification with project-23 `DomainClassifierUnit` core or project-21 logical wrapper when the test is an orchestration test.
3. Replace E24 full-prompt attribution with compiled segments.
4. Keep historical result documents unchanged, but source code tests must use current production source of truth.

Gate:

```powershell
Select-String -Path 35.ExperimentsV2\Experiments\*.cs -Pattern "TextToPredicates|ClassifyByKeywords|new DomainClassifier|AttributedRouter|FullPrompt" 
dotnet test Tests\LlmFromScratch.Tests.csproj --filter "E18|E24|E51A|ExperimentsV2"
dotnet test Tests\LlmFromScratch.Tests.csproj
```

Expected: no active successful experiment owns a duplicate developer-prompt semantic pipeline.

## Phase 7 - Regression Tests

Add or update tests:

1. `CompiledDomainClassifierUnit_ResolvesFromRegistry`
2. `AdaptiveLogicalOrchestrator_CompilesBeforeDomainClassification`
3. `TransitionSignal_LowConfidenceStillSkipsAfterCompiledClassification`
4. `DomainClassifierUnit_TrainedPredicatePathOverridesRawKeywordFallback`
5. `PromptDomainClassifier_CompatibilityWrapperUsesCompiledMetadataWhenPresent`
6. `AttributedSegmentRouterUnit_UsesCompiledSegmentsOnly`
7. `InternalLlmGateway_UsesCompiledDomainClassifierFromRegistry`
8. `Experiments_DoNotUseLocalDeveloperPromptPredicateScans`

Gate:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj
```

Expected floor: current green floor + new tests.

## Commit And Push Gate

After the full suite is green:

```powershell
git status --short
git diff --stat
git add 21.AgentInfrastructure 23.SemanticCompilerAdapter 35.ExperimentsV2 Tests
git commit -m "Wire compiled prompt domain classification through logical units"
git push origin main
```

Do not stage run artifacts from `experiments-v2/*/results` or `XIIGEN-DOCS-BATCH-COMPARISON/run-*` unless explicitly requested.

## Final Validation

Run:

```powershell
dotnet test Tests\LlmFromScratch.Tests.csproj 2>&1 | Select-Object -Last 8
git log --oneline -5
git status --short
```

Report:

- final test count
- commit hash
- push confirmation
- remaining untracked experiment artifacts, if any

