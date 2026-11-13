# LlmFromScratch — Open Gap Document
# Version: 1.0 — 2026-05-07
# Covers: A51, A52, A53, A54
# Purpose: for each gap, explain what is wrong, why it matters,
#          how to test that the problem exists, and how to fix it

---

## Overview

Four gaps remain open after the v7 implementation documentation. Each gap
is independent — fixing one does not require fixing the others first. They
are ordered here by impact: A54 is the most operationally damaging, A52 is
the most fragile, A51 is the most hygiene-critical, A53 is the most
validation-critical.

---

## A54 — ReplayBuffer has no capacity cap

### What is wrong

`DefaultLogicalCapabilityTrainer.FindReplayManifests` scans the entire
checkpoint root directory on every training call:

```csharp
private GeneratedCapabilityCheckpointManifest[] LoadGeneratedCapabilityManifests()
{
    foreach (var manifestPath in Directory.GetFiles(
        _checkpointRoot,
        GeneratedCapabilityManifestFileName,
        SearchOption.AllDirectories))
    { ... }
}
```

This recursively walks every subdirectory under the checkpoint root and
loads every manifest file it finds. The number of manifests grows by
`branches_per_row × rows_trained` across the lifetime of a teaching
session. After 20 rows with an average of 3.9 branches each, the root
contains roughly 77 manifest files. After 100 rows it contains roughly
390 files. After 1,080 rows (the full XIIGen corpus) it would contain
over 4,200 files — and every single training call would scan and
deserialise all of them.

The actual replay sample is capped at 3 manifests (`Take(3)` in
`FindReplayManifests`), so the work done beyond the first 3 results is
wasted. The AdamW optimizer inside the policy model also accumulates
first and second moment estimates (`_m` and `_v` tensors, one per
trainable parameter) across every step in the session. These are never
reset between rows. After 12 BUG-domain steps, the moment estimates
have been updated 12 times and the effective learning rate has decayed
relative to what it would be at step 1. This is why the teaching run
data shows DPO cost growing from 1.13s/branch at step 1 to 34.81s/branch
at step 12 — the optimizer is doing more work per step because it has
accumulated 12 steps of moment history.

### Why it matters

The accumulation means teaching sessions become progressively slower as
they run longer. The first 20 rows in a domain cost roughly what the
first 20 rows should cost. By step 12 in the same domain, each step
costs 30× more. For a 1,080-row full corpus run, the later rows would
be unusable without either restarting the session or capping the
accumulation.

This also means the batch size comparison experiment (see
PLAN-BATCH-SIZE-COMPARISON-v2.md) will see catastrophic accumulation at
large batch sizes even when CPU and RAM are well within their thresholds,
because the cost is in optimizer state rather than hardware load.

### How to test that the problem exists

**Test 1 — Manifest scan cost grows with checkpoint count.**

Create a temp directory. Write N manifest JSON files into subdirectories
under it (simulate a large run). Call `LoadGeneratedCapabilityManifests`
(or replicate its `Directory.GetFiles` call). Measure elapsed time.
Repeat with N = 10, 100, 500, 1000. Expect time to grow linearly with N
even though only 3 results will ever be used.

```csharp
[Fact]
public void ManifestScan_TimeGrowsLinearly_WithCheckpointCount()
{
    var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    // Write 10, 100, 500 manifest files in subdirectories
    // Measure elapsed time for each count
    // Assert: time at N=500 is significantly more than at N=10
}
```

**Test 2 — DPO cost per branch grows with step count.**

Train the same domain 12 times in sequence using the trainer with
`BuildTeachingStack` continuing from prior checkpoint. Record the time
for each DPO step. Assert that step 12 does not exceed 3× the cost of
step 1.

```csharp
[Fact]
public async Task DpoStepCost_DoesNotExceed3xStep1_After12Steps()
{
    // Train domain 12 times, record DpoMsTotal per step
    // Assert: step12_cost / step1_cost < 3.0
}
```

Currently this test will fail because the ratio at step 12 is
approximately 30× in production data.

**Test 3 — Replay sample is always exactly Take(3) regardless of manifest count.**

Assert that `FindReplayManifests` returns at most 3 manifests when the
checkpoint root has 500 manifests. The fix must not change this behaviour.

```csharp
[Fact]
public void FindReplayManifests_ReturnsAtMost3_WhenManyManifestsExist()
{
    // Write 500 manifests for the same domain tag
    // Assert result.Length <= 3
}
```

### How to fix it

Two independent fixes are needed. Either one alone reduces the problem;
both together eliminate it.

**Fix 1 — Cap the manifest scan to the most recent N files.**

Change `LoadGeneratedCapabilityManifests` to limit the recursive scan
to the most recently modified files. Because the `FindReplayManifests`
call always takes at most 3 results ordered by `CreatedAtUtc` descending,
a pre-filter of 20 most recent manifests per domain would produce the
same replay selection while scanning at most 20 files instead of
potentially thousands:

```csharp
private GeneratedCapabilityCheckpointManifest[] FindReplayManifests(
    string domainAdapterTag)
{
    // Enumerate paths, take the 20 most recently written files,
    // deserialise only those, filter by domain, take 3.
    return Directory
        .GetFiles(_checkpointRoot,
            GeneratedCapabilityManifestFileName,
            SearchOption.AllDirectories)
        .OrderByDescending(File.GetLastWriteTimeUtc)
        .Take(20)                              // cap scan work
        .Select(path => /* deserialise */)
        .Where(m => m is not null &&
            string.Equals(m.DomainTag, domainAdapterTag,
                StringComparison.Ordinal))
        .OrderByDescending(m => m.CreatedAtUtc)
        .Take(3)
        .ToArray();
}
```

**Fix 2 — Reset AdamW moments between domain sessions.**

Add a `ResetMoments()` method to the optimizer or recreate the optimizer
at the start of each new row. The moment estimates from row N-1 should
not carry over to row N because they were built on different training
data. Resetting moments at the start of each `TrainAsync` call restores
step-1 cost regardless of how many prior rows have been trained.

The simplest implementation: clear the moment cache on the AdamW instance
before the DPO step, or create a new `AdamW` instance per training call
(the policy model weights are preserved — only the optimizer state is
discarded).

**New test floor:** +1 test (the DpoStepCost ratio test).

---

## A52 — ContextPacketReader uses reflection

### What is wrong

`ContextPacketReader.Read(object)` reads properties off a
`LogicalContextPacket` using `System.Reflection.GetProperty`:

```csharp
var fragmentsProperty = contextPacket.GetType()
    .GetProperty("Fragments",
        BindingFlags.Public | BindingFlags.Instance);
if (fragmentsProperty?.GetValue(contextPacket)
    is not System.Collections.IEnumerable fragments)
    return ContextSignals.Empty;
```

It then reads five properties off each fragment by name string:

```csharp
var verdict = ReadString(fragment, "OverallVerdict");
var metrics = ReadStringDictionary(fragment, "Metrics");
var evidenceIds = ReadStringArray(fragment, "EvidenceIds");
var subPhaseId = ReadString(fragment, "SubPhaseId");
```

The `LogicalContextPacket` and `LogicalContextFragment` types are both
`sealed record` types defined in project 21. The reader lives in project
23, which cannot reference project 21 (that would create a circular
dependency: 21→23→21). Reflection was used as a workaround so project 23
could read a type it cannot name directly.

### Why it matters

If any property of `LogicalContextFragment` is renamed — for example if
`OverallVerdict` becomes `Verdict`, or `SubPhaseId` becomes `RoutedSubPhaseId`
— the reader silently returns empty strings or empty arrays. No compile
error occurs. No test catches it unless the test specifically exercises
context carry-forward through a full orchestrator call and asserts on the
extracted values.

The properties being read are: `Fragments` on `LogicalContextPacket`,
and `OverallVerdict`, `Metrics`, `EvidenceIds`, `SubPhaseId` on
`LogicalContextFragment`. All five are public positional parameters of
sealed record types and are unlikely to be renamed, but the brittleness
is real.

### How to test that the problem exists

**Test 1 — Rename a property and observe silent failure.**

Temporarily rename `SubPhaseId` to `SubPhaseIdentifier` in
`LogicalContextFragment`. Run the orchestrator with context carry-forward
enabled. Assert that the downstream branch receives `SubPhaseId` in its
context signals. The assertion will pass (the build succeeds) but the
value will be `null` because the reflection lookup failed silently.

This test cannot currently be written as a standing automated test
because it requires a code mutation. But it demonstrates the risk
concretely.

**Test 2 — Assert round-trip fidelity of context extraction.**

Create a `LogicalContextPacket` with known field values. Pass it as the
`context.packet` metadata value. Call `ContextPacketReader.Read(metadata)`.
Assert that every field in the returned `ContextSignals` matches the
known values exactly.

```csharp
[Fact]
public void ContextPacketReader_ReadsAllFieldsCorrectly_FromKnownPacket()
{
    var fragment = new LogicalContextFragment(
        FlowId: "flow-1",
        ScenarioId: "3.4",
        OverallVerdict: "PASS",
        FinalHumanResult: "ok",
        PhaseId: "3",
        SubPhaseId: "3.4",
        Metrics: new Dictionary<string, string>
        {
            ["domain.label"] = "BUG",
            ["domain.confidence"] = "0.92"
        },
        EvidenceIds: ["ev-1"],
        Warnings: []);

    var packet = new LogicalContextPacket(
        RunId: "r1",
        SourceFlowIds: ["flow-1"],
        SourceScenarioIds: ["3.4"],
        Fragments: [fragment],
        CreatedAtUtc: DateTimeOffset.UtcNow);

    var metadata = new Dictionary<string, object>
    {
        ["context.packet"] = packet
    };

    var signals = ContextPacketReader.Read(metadata);

    Assert.Contains(Domain.BUG, signals.PriorDomains);
    Assert.Contains("3.4", signals.CompletedSubPhases);
    Assert.Equal(0.92, signals.PriorConfidenceMap[Domain.BUG], precision: 2);
}
```

This test will pass today. Its value is as a regression guard — if the
reflection is replaced with a typed approach and a property name later
changes, this test catches the break.

**Test 3 — Assert graceful empty return on null packet.**

```csharp
[Fact]
public void ContextPacketReader_ReturnsEmpty_WhenPacketIsNull()
{
    var signals = ContextPacketReader.Read((object?)null);
    Assert.True(signals.IsEmpty);
}
```

### How to fix it

The circular dependency is the root constraint. Project 23 cannot
reference project 21. The two clean solutions are:

**Option A — Extract shared context types into a new project 20.**

Create `20.AgentContracts` containing `LogicalContextPacket` and
`LogicalContextFragment`. Both project 21 and project 23 reference
project 20. No circular dependency. `ContextPacketReader` casts directly
to `LogicalContextPacket` instead of using reflection.

This is the cleanest solution architecturally but requires moving two
types and updating all their usages in project 21.

**Option B — Pass a typed delegate from project 21 to project 23.**

Instead of `ContextPacketReader.Read(object)`, introduce a typed reader
interface in project 23 that project 21 provides an implementation for
at construction time. Project 23 calls the delegate; project 21 supplies
the typed cast. No reflection; no circular dependency.

This is more complex but avoids moving types between projects.

For now, the minimum safe action without a structural change is to add
the round-trip fidelity test (Test 2 above) to the test suite so that
any future property rename on `LogicalContextFragment` produces a test
failure rather than silent data loss.

**New test floor:** +1 test (round-trip fidelity test).

---

## A51 — Eight Infrastructure copies not deleted

### What is wrong

Eight files in `35.ExperimentsV2/Infrastructure/` duplicate types that
now have canonical homes elsewhere:

| File | Canonical home | Canonical namespace |
|---|---|---|
| `FamilyCollapser.cs` | `12.SemanticCompiler/Compression/` | `SemanticCompiler` |
| `LogicExtractor.cs` | `12.SemanticCompiler/Compression/` | `SemanticCompiler` |
| `SemanticNormalizer.cs` | `12.SemanticCompiler/Compression/` | `SemanticCompiler` |
| `StopSignalDetector.cs` | `12.SemanticCompiler/Planning/` | `SemanticCompiler` |
| `DomainClassifier.cs` | `23.SemanticCompilerAdapter/DomainClassifierUnit.cs` | `SemanticCompilerAdapter` |
| `SentencePatternClassifier.cs` | No canonical home yet | — |
| `PromptSplitter.cs` | No canonical home yet | — |
| `AttributedRouter.cs` | Superseded by `AttributedSegmentRouter` in project 23 | `SemanticCompilerAdapter` |

The four files with no canonical home (`SentencePatternClassifier`,
`PromptSplitter`, `DomainClassifier`, `AttributedRouter`) are only used
by experiments within `35.ExperimentsV2/Experiments/` in the `ExperimentsV2`
namespace. The four files with canonical homes in project 12 are also
used by experiments in the same namespace.

The risk is divergence: if a bug is fixed in `SemanticCompiler.FamilyCollapser`,
the fix does not automatically apply to `ExperimentsV2.FamilyCollapser`.
Experiments E36, E44, E45, E22, E24 are currently testing the infrastructure
copy rather than the canonical implementation.

### How to test that the problem exists

**Test 1 — Verify experiments reference canonical types, not copies.**

Build the solution with the Infrastructure copies removed (temporarily
delete or rename them). If experiments fail to compile, they are still
importing from the copy rather than the canonical project.

```bash
# Move all Infrastructure copies to a backup location
mv 35.ExperimentsV2/Infrastructure/FamilyCollapser.cs /tmp/
# ... repeat for all 8 files
dotnet build 35.ExperimentsV2/
# Expect: compile errors listing which experiments reference the copies
```

Currently this will produce compile errors in at least E22, E24, E25,
E36, E44, E45, E31, and T35.

**Test 2 — Verify canonical and copy produce identical output on the same input.**

For the four files that have canonical counterparts, write a parity test:

```csharp
[Fact]
public void FamilyCollapser_InfrastructureCopy_MatchesCanonical()
{
    var input = "authentication token validation";

    var canonical = SemanticCompiler.FamilyCollapser.CollapseString(input);
    var copy = ExperimentsV2.FamilyCollapser.CollapseString(input);

    Assert.Equal(canonical, copy);
}
```

If this test passes today, deleting the copy and updating the experiment
`using` statement is safe. If it fails, the copies have diverged and
the divergence must be resolved before deletion.

### How to fix it

**Step 1 — Run parity tests** for `FamilyCollapser`, `LogicExtractor`,
`SemanticNormalizer`, and `StopSignalDetector`. Confirm each produces
identical output to its canonical counterpart.

**Step 2 — Update `using` statements** in experiments that reference the
copies. Change `using ExperimentsV2;` references to `using SemanticCompiler;`
where the class exists in project 12. The `35.ExperimentsV2` project
already references project 12's `SemanticCompiler` assembly.

**Step 3 — Handle the four files with no canonical home.** For
`SentencePatternClassifier` (used by E25 only) and `PromptSplitter` (used
by experiments only), two options:

- Leave them in the Infrastructure folder but rename the namespace to
  `ExperimentsV2.Infrastructure` to make clear they are experiment-only
  and not production copies.
- Move them to a new project if they need to be wired into production.
  Currently neither is needed in the runtime path.

**Step 4 — Handle `AttributedRouter`** (Infrastructure copy) vs
`AttributedSegmentRouter` (canonical in project 23). E24 uses the
`AttributedRouter` copy. Update E24 to use `AttributedSegmentRouter`
from project 23 and confirm the experiment still produces its stated
result.

**Step 5 — Handle `DomainClassifier`** (Infrastructure copy). The copy
uses integer constants (`CODE_QUESTION=0`, `BUG_FIX=1`) instead of the
canonical `Domain` enum. Any experiment still using the copy is comparing
against the wrong type. Update to use `DomainClassifierUnit` from project
23 or `RouteClassifier` from project 29.

**Step 6 — Delete the files** once all experiments compile and pass
against the canonical implementations.

**New test floor:** +4 tests (parity tests for FamilyCollapser,
LogicExtractor, SemanticNormalizer, StopSignalDetector).

---

## A53 — ExpressionCompletionUnit requires held-out validation

### What is wrong

`ExpressionCompletionUnit` is built, tested with a 3-expression micro-corpus,
and integrated into the KNN scoring matrix. The existing 28 tests cover:

- `ExpressionCorpusBuilder.ExtractPairs` — pair extraction
- `ExpressionCompletionCore.Normalise` — all normalisation cases including
  placeholder stripping and operator padding
- `ExpressionCompletionCore.Lookup` — match and no-match paths
- `ExpressionCompletionUnit.ExecuteAsync` — wrapper contract

What is not tested is behaviour on a realistic corpus size. The v1
experiment used 20 training expressions and achieved 100% corpus
retrieval — but 0/10 accuracy on 10 held-out expressions. The 0/10
failure was caused by the old KNN design (vocabulary starvation in the
BPE tokeniser). The new exact-memory design eliminates that failure mode
entirely: an unseen expression cannot produce a false positive because
the lookup is a dictionary lookup, not a nearest-neighbour search.

However, the following question remains unanswered: does normalisation
produce any false positives — cases where a held-out expression's
normalised form accidentally matches a training expression's normalised
key?

For example, consider training on `"A + B = C"`. The normalised key is
`"a + b ="`. If a held-out expression normalises to `"a + b ="` by
coincidence — perhaps `"A + B = [unknown]"` or `"(A + B = ?)"` or
`"**A+B=**"` — it would return `Matched=true` with completion `"C"`. This
is a false positive and would corrupt the KNN matrix weight learning
because the unit would vote confidently on an expression it was never
trained on.

### Why it matters

The unit participates in the KNN scoring matrix at confidence 1.0 on
every match. Weight learning will reward the unit whenever a match
coincides with a correct branch decision, and penalise it whenever it
does not. If the unit produces false positives — matching expressions it
was not trained on — it corrupts the weight signal for those rows. The
DPO update will adjust the weight based on whether those false-positive
predictions were coincidentally correct, not based on whether the unit
genuinely recognised the expression.

False positives are deterministic — the same query always produces the
same normalised key — so they would consistently corrupt the weight
signal for specific expression patterns rather than introducing random
noise.

### How to test that the problem exists

**Test 1 — Held-out abstain rate.**

Train the unit on the original 20 expressions from the v1 experiment.
Run the 10 held-out expressions from the v1 results. Assert that all 10
return `Matched=false`. This is the primary gate.

```csharp
[Fact]
public void ExpressionCompletionUnit_HeldOut_AllAbstain()
{
    var training = new[]
    {
        "Revenue - Cost = Profit",
        "Principal * Interest = Charge",
        "Assets - Liabilities = Equity",
        "Errors + Warnings = Findings",
        "Request + Context = Prompt",
        "Success + Warning = Status",
        "Frame + Bytes = Payload",
        "Spec + Design = Blueprint",
        "Plan + Constraint = Task",
        "Signal + Threshold = Alert",
        // ... remaining 10 training expressions
    };

    var heldOut = new[]
    {
        "Vector + Predicate =",
        "Score + Margin =",
        "Assets - Liabilities =",   // ← this one IS in training — should match
        // ... only truly unseen expressions
    };

    var core = ExpressionCompletionCore.Train(training)!;

    foreach (var partial in heldOut)
    {
        var result = core.Lookup(partial);
        Assert.False(result.Matched,
            $"False positive on held-out expression: '{partial}' → '{result.Completion}'");
    }
}
```

**Test 2 — Training recall rate.**

Assert that all 20 training expressions return `Matched=true` with the
correct completion. This confirms normalisation is applied consistently
at train and query time.

```csharp
[Fact]
public void ExpressionCompletionUnit_AllTraining_Match()
{
    var training = new[] { /* 20 complete expressions */ };
    var core = ExpressionCompletionCore.Train(training)!;

    foreach (var expr in training)
    {
        // Extract partial (left side including =)
        var idx = expr.LastIndexOf('=');
        var partial = expr[..(idx + 1)];
        var expected = expr[(idx + 1)..].Trim();

        var result = core.Lookup(partial);
        Assert.True(result.Matched, $"Training miss: '{partial}'");
        Assert.Equal(expected, result.Completion);
    }
}
```

**Test 3 — Normalisation collision detection.**

For every training expression, assert that no held-out expression
normalises to the same key. This test is the direct proof that no false
positives can occur for this corpus.

```csharp
[Fact]
public void ExpressionCompletionCore_NoNormalisationCollision_TrainingVsHeldOut()
{
    var trainingKeys = training.Select(e =>
        ExpressionCompletionCore.Normalise(e[..(e.LastIndexOf('=')+1)])).ToHashSet();

    foreach (var heldOut in heldOutPartials)
    {
        var key = ExpressionCompletionCore.Normalise(heldOut);
        Assert.DoesNotContain(key, trainingKeys,
            $"Collision: held-out '{heldOut}' normalises to training key '{key}'");
    }
}
```

**Test 4 — Realistic query formats on held-out expressions.**

The normaliser strips brackets, stars, and placeholders. Confirm that
these transformations applied to held-out expressions do not accidentally
produce training keys.

```csharp
[Fact]
public void ExpressionCompletionCore_BracketAndStarVariants_StillAbstain()
{
    var core = ExpressionCompletionCore.Train(training)!;

    var heldOutVariants = new[]
    {
        "[*Vector+Predicate=?]",
        "**Score + Margin = **",
        "(Score + Margin = ?)",
    };

    foreach (var variant in heldOutVariants)
    {
        var result = core.Lookup(variant);
        Assert.False(result.Matched,
            $"Unexpected match on held-out variant: '{variant}'");
    }
}
```

### How to fix it

There is nothing to code-fix in A53. The tests described above either
pass or identify a normalisation collision that needs to be resolved by
adjusting the normalisation rules. If all four tests pass, A53 is closed
and the unit is ready for production teaching runs.

The action items are:

1. Reconstruct the original 20-expression training corpus and 10-expression
   held-out set from the v1 experiment results.
2. Write tests 1–4 above.
3. Run the full suite. If all pass, A53 is closed.
4. If test 3 or 4 fails (normalisation collision found), adjust the
   normaliser to resolve the collision before proceeding.

**New test floor:** +4 tests (held-out abstain, training recall,
collision detection, realistic query variants).

---

## Summary table

| Gap | Root cause | Symptom | Tests to add | Fix complexity |
|---|---|---|---|---|
| A54 — ReplayBuffer no cap | Manifest scan is unbounded + AdamW moments never reset | DPO cost grows 30× by step 12; later rows in a session are 30× slower than the first | 3 tests | Medium — add scan cap + optimizer reset |
| A52 — ContextPacketReader reflection | Project 23 cannot reference project 21 types directly | Renaming any `LogicalContextFragment` property silently returns empty context signals in downstream branches | 1 test (round-trip fidelity) | High — requires shared contract project or delegate injection |
| A51 — Infrastructure copies not deleted | Step 2D of domain classifier plan incomplete | Bug fixes to canonical classes do not propagate to experiments; experiment tests exercise stale code | 4 tests (parity tests) | Low — update `using` statements, delete files |
| A53 — ExpressionCompletionUnit held-out not validated | No validation run on the new exact-memory design | Unknown whether normalisation produces false positives on realistic held-out expressions; if it does, KNN weight learning is corrupted | 4 tests (held-out abstain, recall, collision, variants) | None if tests pass; normaliser adjustment if collision found |

**Combined new test floor after all gaps closed:** +12 tests, floor
1251 → 1263.

---

# Gap document version: 1.0
# Date: 2026-05-07
# Companion to: IMPLEMENTATION-DOCUMENTATION-v7.md
# All gap references: A51, A52, A53, A54
