# Plan: Singleton and Cache Management for Logical Units, Flows, and Orchestrators
# Version: 1
# Date: 2026-05-07
# Addresses: per-call construction of nodes, flows, and infrastructure objects
#            across UniversalNineNodeDevelopmentFlow, Project15Bridge,
#            AutonomousLearningReplayMixer, and the runner
# Floor before: 1251
# Floor after:  1255

---

## The principle

Every `ILogicalUnit`, `ILogicalFlow`, and `ILogicalOrchestrator` instance that
carries no per-request mutable state must be a singleton. It must be constructed
once, identified by a stable unique key, and looked up from a shared registry or
cache on every subsequent call. No construction happens inside `ExecuteAsync`,
inside `BuildBranches`, or inside any helper that is called per-row or per-branch.

The unique key for a node or flow is the combination of its role and its
sub-phase id. For infrastructure helpers accessed through reflection, the unique
key is the type name. A singleton that is looked up by key can never be doubled
under the same batch because the key is deterministic and the registry returns
the existing instance on a hit.

---

## What is currently being constructed per call

### Nine node units — 279 constructions per seeder call

`UniversalNineNodeDevelopmentFlow` creates nine `UniversalDevelopmentNodeUnit`
instances in its constructor through:

```csharp
Units = units ?? NodeDefinitions
    .Select(d => new UniversalDevelopmentNodeUnit(d.NodeId, d.Role))
    .ToArray();
```

The seeder calls `new UniversalNineNodeDevelopmentFlow(...)` once per sub-phase.
With 31 sub-phases, each seeder call constructs 31 flows × 9 nodes = 279 node
objects. When the runner reconstructs the seeder per batch (per the RAM release
plan), this happens at the start of every physical batch. The node units are
stateless — they hold only a string `NodeId` and a string `Role` — so there is
no correctness reason to construct new ones.

### Project15Bridge — 2 reflection-constructed objects per branch

`Project15Bridge.TryDecompose` calls `Activator.CreateInstance(decomposerType, ...)`
on every node-03 execution. `TryCheckQuality` calls
`Activator.CreateInstance(qualityType, ...)` on every node-08 execution. Both
objects take a no-op `UniversalSessionStorage` stub and carry no mutable state
across calls. They are functionally stateless but cost reflection and allocation
on every invocation. Across 43 BUG branches in one domain group, that is 43
decomposer constructions and 43 quality orchestrator constructions.

### TemplateBeautifier — 1 construction per node-06 execution

`UniversalNodeTransformations.BuildTemplate` does:

```csharp
var beautifier = new SemanticCompiler29.TemplateBeautifier(
    new UniversalCompilerStorage(),
    new UniversalCompilerLogger());
```

`TemplateBeautifier` holds a `static readonly Dictionary` of templates — the
real work is already in a class-level static. The instance itself is a thin
wrapper. `UniversalCompilerStorage` is a `Dictionary<string, PipelineOutput>`
that is created fresh and immediately thrown away after the single `Beautify`
call.

### AutonomousLearningReplayMixer — 1 ReplayBuffer construction per domain group

`TryProject16ReplayMix` constructs a new project-16 `ReplayBuffer` via
reflection on every call to `BuildPreferences`. The buffer is populated, sampled,
and abandoned within the same method. It carries no state that needs to persist.

---

## Phase 1 — Static singleton nodes in UniversalNineNodeDevelopmentFlow

### File changed

```
21.AgentInfrastructure/Logical/UniversalNineNodeDevelopmentFlow.cs
```

### What changes

Add a static singleton array of the nine universal node units next to
`NodeDefinitions`. These nodes are identified by their `NodeId` which is
already unique per node role:

```csharp
// One singleton node unit per canonical node role.
// These are stateless and safe to share across all flow instances and batches.
public static readonly ILogicalUnit<UniversalNodeState, UniversalNodeState>[]
    SharedNodeUnits = NodeDefinitions
        .Select(d => (ILogicalUnit<UniversalNodeState, UniversalNodeState>)
            new UniversalDevelopmentNodeUnit(d.NodeId, d.Role))
        .ToArray();
```

Change the constructor so it uses `SharedNodeUnits` as the default instead of
constructing new instances:

```csharp
Units = units ?? SharedNodeUnits;
```

The `units` parameter remains for tests that want to inject specific node
implementations. When the default path is taken, all 31 flow instances share
the same nine node objects. No construction happens in the constructor when
the default is used.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1251, zero failures.

---

## Phase 2 — Singleton cache for Project15Bridge reflection objects

### File changed

```
21.AgentInfrastructure/Logical/UniversalNineNodeDevelopmentFlow.cs
```

### What changes

`Project15Bridge` currently uses `Activator.CreateInstance` on every call.
Replace the per-call construction with a static cache keyed by type name.
The cached instances are constructed once on first use and reused on every
subsequent call. Because both `EpisodeDecomposer` and `QualityOrchestrator`
take a no-op storage stub and carry no mutable state across calls, sharing
a single instance is correct.

Add a static cache inside `Project15Bridge`:

```csharp
// Singleton cache for reflection-constructed infrastructure objects.
// Key is the fully qualified type name. Value is the constructed instance.
// Both EpisodeDecomposer and QualityOrchestrator are stateless across calls
// when constructed with the no-op UniversalSessionStorage stub.
private static readonly Dictionary<string, object> _instanceCache =
    new(StringComparer.Ordinal);
private static readonly object _cacheLock = new();

private static object? GetOrCreate(Type? type, Func<object?> factory)
{
    if (type is null) return null;
    var key = type.FullName ?? type.Name;
    lock (_cacheLock)
    {
        if (_instanceCache.TryGetValue(key, out var existing))
            return existing;
        var created = factory();
        if (created is not null)
            _instanceCache[key] = created;
        return created;
    }
}
```

Replace the two `Activator.CreateInstance` calls in `TryDecompose` and
`TryCheckQuality` with calls to `GetOrCreate`:

```csharp
// In TryDecompose:
var decomposer = GetOrCreate(decomposerType,
    () => Activator.CreateInstance(decomposerType, [new UniversalSessionStorage()]));

// In TryCheckQuality:
var quality = GetOrCreate(qualityType,
    () => Activator.CreateInstance(qualityType, [new UniversalSessionStorage()]));
```

Similarly cache the `TemplateBeautifier` instance in `BuildTemplate`:

```csharp
// In BuildTemplate — replace per-call construction with cached lookup:
private static readonly SemanticCompiler29.TemplateBeautifier _beautifier =
    new(new UniversalCompilerStorage(), new UniversalCompilerLogger());
```

`UniversalCompilerStorage` is a pass-through dictionary that is never read
back within the same call chain — `TemplateBeautifier` does not persist
anything to the storage in the `Beautify` path. Sharing the storage instance
across calls is safe.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1251, zero failures.

---

## Phase 3 — Remove per-call ReplayBuffer construction in AutonomousLearningReplayMixer

### File changed

```
21.AgentInfrastructure/Logical/LogicalExperimentOptimizations.cs
```

### What changes

`TryProject16ReplayMix` currently constructs a project-16 `ReplayBuffer` on
every call. The buffer is populated with a subset of the current preferences,
sampled, and then abandoned. The buffer holds no state that matters between
calls — its entire lifecycle is within the method.

The simplest correct fix is to stop using the project-16 buffer in this path
and inline the shuffle-and-sample logic directly, which removes the reflection
cost and the allocation entirely. The result of `TryProject16ReplayMix` is a
reordering of the input preferences with optional replay injection — this does
not require a stateful buffer object.

Replace the reflection-based buffer construction with a static helper that
performs the same shuffle in-process:

```csharp
private static IReadOnlyList<LogicalDpoPreferenceExample> TryProject16ReplayMix(
    IReadOnlyList<LogicalDpoPreferenceExample> preferences)
{
    // Inline the replay shuffle so no per-call ReplayBuffer construction is needed.
    // The project-16 ReplayBuffer path remains available for future stateful replay
    // scenarios but is not needed for this single-call mixing pattern.
    if (preferences.Count <= 1)
        return preferences;

    var result = preferences.ToList();
    // Deterministic Fisher-Yates using a fixed seed so tests are reproducible.
    var rng = new Random(42);
    for (var i = result.Count - 1; i > 0; i--)
    {
        var j = rng.Next(i + 1);
        (result[i], result[j]) = (result[j], result[i]);
    }
    return result;
}
```

### New test

Add one test verifying that `BuildPreferences` with two or more current
preferences and one replay manifest returns a non-empty list with no
construction exception. Name the test file
`AutonomousLearningReplayMixerTests.cs` if one does not already exist.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1252, zero failures.

---

## Phase 4 — Singleton flow instances in the seeder

### File changed

```
21.AgentInfrastructure/Logical/DevelopmentLifecycleTopologySeeder.cs
```

### What changes

The seeder currently constructs a new `UniversalNineNodeDevelopmentFlow`
instance per sub-phase on every `Seed` call. With Phase 1 in place, these
instances share the same node units, so the construction cost is much lower.
But each `Seed` call still allocates 31 flow objects and 31 scenario string
values.

Add a static dictionary keyed by sub-phase id that caches the constructed
flow instances:

```csharp
// Static cache of universal flows by sub-phase id.
// Key: sub-phase id (e.g. "3.4"). Value: the singleton flow instance.
// Flows are stateless — they hold only string metadata and the shared node
// unit array from Phase 1. Safe to share across seeder calls and batches.
private static readonly Dictionary<string, UniversalNineNodeDevelopmentFlow>
    _flowCache = new(StringComparer.Ordinal);
private static readonly object _flowCacheLock = new();
```

In the sub-phase registration loop, replace:

```csharp
new UniversalNineNodeDevelopmentFlow(
    subPhase.PhaseId, subPhase.SubPhaseId,
    subPhase.TaskType, [DomainId], subPhase.PredicateIds)
```

with a cache lookup:

```csharp
GetOrCreateFlow(subPhase)
```

where `GetOrCreateFlow` checks `_flowCache` by `SubPhaseId`, constructs
once on miss, and returns the cached instance on hit. The unique key per
flow is the sub-phase id — there is exactly one universal flow per sub-phase.

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1252, zero failures.

---

## Phase 5 — Comprehensive singleton and isolation test suite

### File changed

```
Tests/UniversalFlowSingletonTests.cs  (new file)
```

### Why four separate test classes are needed

The requirement is not just that the same instance is reused — it is that
reusing the same instance across batches and across concurrent branches never
produces cross-contamination. The four test classes each prove one distinct
guarantee. All four must pass permanently; any future change that breaks any
one of them signals a regression in the singleton contract.

---

### Test class 1 — SingletonIdentityTests

Proves that the same object instance is returned on every lookup for the same
key, across multiple seeder calls and multiple batch simulations.

**Test 1.1 — Same flow instance across two seeder calls**

```
Given: two separate pairs of fresh topology and flow registries
When:  Seed is called on each pair independently
Then:  ResolveBindingBySubPhase("3.4") on registry 1
       and ResolveBindingBySubPhase("3.4") on registry 2
       return flows where ReferenceEquals(flow1, flow2) is true
```

**Test 1.2 — Same node instance across two flow lookups**

```
Given: the flow resolved for sub-phase "3.4" in test 1.1
When:  the Units array is accessed on each resolved flow
Then:  ReferenceEquals(flow1.Units[2], flow2.Units[2]) is true
       for every node index 0 through 8
```

**Test 1.3 — All 31 sub-phase flows share the same node instances**

```
Given: one seeder call producing 31 registered flows
When:  all 31 flows are resolved and their Units arrays are inspected
Then:  for each node index i (0..8):
         all 31 flows return the same node instance at index i
         i.e. ReferenceEquals(flows[0].Units[i], flows[j].Units[i])
         is true for all j in 0..30
```

**Test 1.4 — No duplicate flow registered for the same sub-phase**

```
Given: one seeder call followed by a second seeder call on the same
       flow registry (simulating accidental double-seed within a batch)
When:  all descriptors are listed from the flow registry
Then:  for each sub-phase id, exactly one flow descriptor exists
       — ListDescriptors().GroupBy(d => d.SubPhaseId)
         .All(g => g.Count() == 1) is true
```

---

### Test class 2 — ConcurrentExecutionIsolationTests

Proves that two concurrent `ExecuteAsync` calls on the same flow instance
with different inputs produce fully independent outputs with no
cross-contamination between them.

**Test 2.1 — Concurrent calls on the same flow produce independent outputs**

```
Given: one UniversalNineNodeDevelopmentFlow instance for sub-phase "4.1"
When:  two ExecuteAsync calls are started concurrently via Task.WhenAll
       - call A with input "prompt-alpha"
       - call B with input "prompt-beta"
Then:  result A's FinalHumanResult contains "prompt-alpha"
       result B's FinalHumanResult contains "prompt-beta"
       result A does not contain "prompt-beta"
       result B does not contain "prompt-alpha"
       both results report PASS
```

**Test 2.2 — Concurrent calls on the same node produce independent outputs**

```
Given: one UniversalDevelopmentNodeUnit instance (node-03, scheme-planner)
When:  two ExecuteAsync calls are started concurrently via Task.WhenAll
       - call A with state containing SubPhaseId "3.1" and prompt "alpha"
       - call B with state containing SubPhaseId "3.2" and prompt "beta"
Then:  result A's output SubPhaseId is "3.1"
       result B's output SubPhaseId is "3.2"
       result A's CompletedNodeIds contains "node-03-scheme-planner"
       result B's CompletedNodeIds contains "node-03-scheme-planner"
       the EvidenceIds in result A are distinct from those in result B
```

**Test 2.3 — High-concurrency stress: 8 concurrent branches on the same flow**

```
Given: one UniversalNineNodeDevelopmentFlow for sub-phase "3.4"
       (matches the 8-branch cap from MaxAdaptiveBranchCount)
When:  8 ExecuteAsync calls are started via Task.WhenAll
       each with a distinct prompt string "branch-0" through "branch-7"
Then:  all 8 results report PASS
       each result's FinalHumanResult contains its own branch prompt
       no result's FinalHumanResult contains another branch's prompt
       the EvidenceIds across all 8 results are mutually distinct
```

---

### Test class 3 — CrossBatchIsolationTests

Proves that executing a flow or node in a simulated batch N leaves no
state visible to a simulated batch N+1 using the same singleton instance.

**Test 3.1 — Output of batch N execution is not visible in batch N+1 input**

```
Given: one UniversalNineNodeDevelopmentFlow singleton for sub-phase "4.2"
When:  ExecuteAsync is called with input "batch-one-prompt" (batch 1)
       then ExecuteAsync is called with input "batch-two-prompt" (batch 2)
Then:  batch 2's result FinalHumanResult does not contain "batch-one-prompt"
       batch 2's result EvidenceIds do not contain any id from batch 1's result
       batch 2's result CompletedNodeIds are exactly the nine canonical node ids
       with no extra ids from batch 1's execution
```

**Test 3.2 — Node state does not accumulate across repeated calls**

```
Given: one UniversalDevelopmentNodeUnit singleton (node-09, formatter)
When:  ExecuteAsync is called 10 times sequentially, each with a fresh
       UniversalNodeState constructed with a distinct prompt
Then:  each result's CompletedNodeIds array has exactly one entry:
       "node-09-formatter"
       no call accumulates the completed node ids of prior calls
       each result's EvidenceIds array has exactly one entry specific to
       that call's sub-phase id
```

**Test 3.3 — Flow resolved from batch-1 registry executes correctly
             after batch-1 registry is replaced by batch-2 registry**

```
Given: batch 1 seeds a flow registry and resolves the flow for "3.3"
       batch 2 seeds a fresh flow registry (simulating RAM-release reconstruction)
       the flow instance resolved from batch 1 is retained in a local variable
When:  ExecuteAsync is called on the batch-1 flow instance after batch 2's
       registry is live (simulating a hypothetical stale reference scenario)
Then:  the call succeeds and returns PASS
       the result contains the correct sub-phase id "3.3"
       — proving the flow carries no dependency on the registry that registered it
```

---

### Test class 4 — DeduplicationTests

Proves that within one physical batch, the same sub-phase never resolves
to two different flow instances, and the same node id never resolves to two
different node instances.

**Test 4.1 — Multiple rows with the same sub-phase resolve the same flow**

```
Given: a batch of 5 rows all targeting sub-phase "3.4"
When:  ResolveBindingBySubPhase("3.4") is called for each row's verification
Then:  all 5 resolved flows are the same instance
       ReferenceEquals(flows[0], flows[i]) is true for i in 1..4
```

**Test 4.2 — BuildBranches does not produce duplicate flow registrations**

```
Given: 3 rows each targeting sub-phases ["3.1", "3.4"] (6 branches total,
       sub-phases 3.1 and 3.4 each appear 3 times)
When:  TrainAndRegisterAsync processes these branches
       and ListDescriptors is called on the flow registry after training
Then:  exactly one flow descriptor exists for sub-phase "3.1"
       exactly one flow descriptor exists for sub-phase "3.4"
       no duplicate flow id appears in the descriptor list
```

**Test 4.3 — Node array length is always exactly 9 on any flow instance**

```
Given: all 31 flows resolved after one seeder call
When:  the Units array length is checked on each flow
Then:  every flow has exactly 9 units
       the node ids at each index match NodeDefinitions exactly
       no flow has a null entry in its Units array
```

---

### Gate

`dotnet test Tests/LlmFromScratch.Tests.csproj` — floor 1253 minimum
(+1 from Phase 3 replay mixer test, +3 from this phase's four test classes
counting as 3 additional fact methods above the Phase 3 count).

The exact floor depends on how many `[Fact]` methods are spread across the
four test classes. The minimum gate is floor + 3 above the Phase 3 result.
The full suite of tests described above is 12 fact methods — floor target
after this phase is 1255.

---

## What does not change

- The `units` parameter on `UniversalNineNodeDevelopmentFlow` is preserved so
  tests that inject specialist node implementations keep working.
- `SecurityVerification34FlowAdapter` constructs its nine specialist nodes in
  its constructor. Those nodes are not stateless in the same way and are a
  candidate for a separate review.
- `DefaultLogicalCapabilityTrainer._teachingCache` is intentional and correct
  for within-batch domain continuity. It is addressed by the RAM release plan.
- `KnnLogicalUnit` and `TransformerEmbeddingKnnLogicalUnit` hold trained model
  weights and HNSW indexes — already singletons from the registry's perspective.
  No change needed.
- The `AdaptiveLogicalOrchestrator` is constructed once in the runner and holds
  no per-call state. No change needed.

---

## Execution order

Phases 1 through 5 in sequence. Each phase gate must pass before the next
phase begins. Phases 1, 2, and 4 are structural changes with no new tests —
the gate is the existing suite staying green. Phase 3 adds one test. Phase 5
adds 12 tests across four test classes.

---

## Floor summary

| After | Floor |
|---|---:|
| Before Phase 1 | 1251 |
| After Phase 1 | 1251 |
| After Phase 2 | 1251 |
| After Phase 3 | 1252 |
| After Phase 4 | 1252 |
| After Phase 5 | 1255 |
