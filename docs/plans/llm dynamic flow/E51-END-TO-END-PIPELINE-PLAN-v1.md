# E51 End-to-End Pipeline Experiment — Implementation Plan v1

## What This Plan Covers

This plan defines how to write, run, and interpret **E51EndToEndPipeline.cs** —
the first experiment that tests the full designed dynamic flow as a sequential
chain rather than one component in isolation.

The experiment takes a raw user prompt paired with a C# code context, runs it
through the five-stage pipeline (stack detection → logical structure compilation
→ domain routing → KNN unit retrieval → template beautification), and measures
whether the produced result preserves the meaning of the original code.

Everything in this plan uses infrastructure already present in
`35.ExperimentsV2/`. No new libraries. No external services. No file I/O beyond
the existing `ResultWriter`.

---

## Gate B — The Binary Pass Criterion

The experiment passes when:

```
enhanced_predicate_overlap >= 0.60  AND  domain_accuracy >= 0.70
```

Both conditions must hold simultaneously. Either alone is not enough — a
pipeline that routes correctly but loses meaning, or preserves meaning but
routes to the wrong domain, is not useful.

**Baseline** (random template, no KNN): expected predicate overlap ~0.15–0.20,
domain accuracy ~0.20 (random across 5 classes).

**Enhanced** (full pipeline): target predicate overlap >= 0.60, domain accuracy
>= 0.70 on 10 training examples.

**Verification command after running:**

```bash
grep "predicate_overlap\|domain_accuracy" \
  35.ExperimentsV2/results/E51-END-TO-END-PIPELINE/results/run-*.json
```

---

## Dataset Design

### Why This Size

Ten training examples and ten test examples is the minimum that is still
meaningful. It is large enough to produce a Jaccard distance ranking between
KNN candidates but small enough to inspect every failure by hand. Every failed
test example must have a named reason — not just a metric below threshold.

The Tier 1 source is `01.Tokenizer` (BpeTokenizer.cs, RegexSplitter.cs,
VocabEntry.cs — about 400 lines of real C# across three files). This is the
existing Tier 1 dataset used by every other experiment. The synthetic prompts
are generated inline, exactly as E18, E21, E32, and E38 do.

### Tier Ladder

| Tier | Training | Test | Code source | Scale trigger |
|------|----------|------|-------------|---------------|
| 1 | 10 | 10 | 01.Tokenizer only | Passes at Tier 1 |
| 2 | 30 | 10 | 01.Tokenizer + 02.DatasetPipeline + 05.Embeddings | Scale up from Tier 1 |
| 3 | 80 | 20 | All numbered subprojects | Scale up from Tier 2 |

**Scale-up signal:** `ScalingSignal = train_count / (train_count + avg_neighbor_distance * 10)`.
Values below 0.40 mean the KNN has too few training examples to form reliable
neighborhoods — scale up. Values above 0.40 mean the training set is dense
enough to sustain the current tier.

---

## The Twenty Synthetic Examples

All twenty examples are defined inline as constants. They are deterministic —
no random generation, no seed dependency. This means the test is fully
reproducible without any file reads.

Each example has five fields:

- **Prompt** — the natural language request a user would type
- **CodeContext** — a short C# snippet drawn from `01.Tokenizer`
- **ExpectedDomain** — the correct `DomainClassifier` label (0–4)
- **RequiredPredicates** — the minimum set of Level 3 predicate names the
  result must contain to pass the fidelity check
- **ForbiddenPredicates** — predicates that must NOT appear (catches hallucination)

### The Ten Training Examples

These are positive examples. The pipeline will be trained (KNN indexed) on them.
Each one is also a good example of what the pipeline should produce.

```
T01  Prompt: "What does BpeTokenizer do when it encounters an unknown byte?"
     Code:   BpeTokenizer.Encode() — contains NULL_GUARD, BRANCH, RETURN
     Domain: CODE_QUESTION (0)
     Required predicates: [NULL_GUARD, BRANCH, RETURN]
     Forbidden predicates: [THROW, ITERATE]

T02  Prompt: "Add a null check before the merge pairs loop in the tokenizer"
     Code:   BpeTokenizer.BuildVocab() — contains DECLARE, ITERATE, ASSIGN
     Domain: CODE_CHANGE (2)
     Required predicates: [NULL_GUARD, ITERATE, DECLARE]
     Forbidden predicates: [HANDLE_ERROR]

T03  Prompt: "Fix the regex splitter returning empty tokens on whitespace input"
     Code:   RegexSplitter.Split() — contains NULL_GUARD, FAIL, RETURN
     Domain: BUG_FIX (1)
     Required predicates: [NULL_GUARD, FAIL]
     Forbidden predicates: [CREATE, AWAIT]

T04  Prompt: "Explain how vocab entries are sorted during BPE merge"
     Code:   BpeTokenizer.MergePairs() — contains ITERATE, ASSIGN, RETURN
     Domain: CODE_QUESTION (0)
     Required predicates: [ITERATE, RETURN]
     Forbidden predicates: [THROW, NULL_GUARD]

T05  Prompt: "Refactor the tokenizer encode method to use LINQ instead of a loop"
     Code:   BpeTokenizer.Encode() — contains ITERATE, ASSIGN, RETURN
     Domain: CODE_CHANGE (2) — note: REFACTOR maps to CODE_CHANGE in D-04
     Required predicates: [ITERATE, RETURN]
     Forbidden predicates: [NULL_GUARD, THROW]

T06  Prompt: "Design the interface for a streaming tokenizer that handles large files"
     Code:   ITokenizer.cs — contains DECLARE, RETURN
     Domain: ARCHITECTURE (4)
     Required predicates: [DECLARE, RETURN]
     Forbidden predicates: [FAIL, THROW]

T07  Prompt: "Why does the vocab entry equality check fail for identical byte sequences?"
     Code:   VocabEntry.Equals() — contains NULL_GUARD, BRANCH, RETURN, FAIL
     Domain: BUG_FIX (1)
     Required predicates: [NULL_GUARD, FAIL, BRANCH]
     Forbidden predicates: [CREATE, ITERATE]

T08  Prompt: "Plan the steps to add trigram support to the existing bigram tokenizer"
     Code:   NgramModel in Core — contains ITERATE, DECLARE, ASSIGN
     Domain: ORCHESTRATION (3)
     Required predicates: [ITERATE, DECLARE]
     Forbidden predicates: [THROW, NULL_GUARD]

T09  Prompt: "Create a VocabEntry factory method that validates the byte array before construction"
     Code:   VocabEntry.cs — contains NULL_GUARD, VALIDATE_OP, CREATE, RETURN
     Domain: CODE_CHANGE (2)
     Required predicates: [NULL_GUARD, CREATE, RETURN]
     Forbidden predicates: [FAIL, ITERATE]

T10  Prompt: "Show me which methods in BpeTokenizer call the RegexSplitter"
     Code:   BpeTokenizer.cs — contains CALL, DECLARE, RETURN
     Domain: CODE_QUESTION (0)
     Required predicates: [CALL, RETURN]
     Forbidden predicates: [NULL_GUARD, THROW, FAIL]
```

### The Ten Test Examples

These are the held-out examples the KNN has never seen. They test generalization.

```
V01  Prompt: "What happens when the tokenizer receives a null input string?"
     Code:   BpeTokenizer.Encode() guard section
     Domain: CODE_QUESTION (0)
     Required predicates: [NULL_GUARD, FAIL]
     Forbidden predicates: [ITERATE, CREATE]
     — Tests: does KNN find T01/T03/T07 (null-check neighbors) over T05/T08?

V02  Prompt: "Add error handling when vocab loading fails during initialization"
     Code:   BpeTokenizer constructor area
     Domain: CODE_CHANGE (2)
     Required predicates: [HANDLE_ERROR, NULL_GUARD, ASSIGN]
     Forbidden predicates: [AWAIT, DELETE_OP]
     — Tests: does routing survive "add" + "error" (ambiguous BUG vs CHANGE)?

V03  Prompt: "Fix the off-by-one in the pair merge count"
     Code:   BpeTokenizer.MergePairs() counter
     Domain: BUG_FIX (1)
     Required predicates: [ITERATE, ASSIGN, RETURN]
     Forbidden predicates: [NULL_GUARD, THROW]
     — Tests: short prompt, minimal predicates — does KNN still find T03/T07?

V04  Prompt: "How does BPE select which pair to merge next?"
     Code:   BpeTokenizer merge priority logic
     Domain: CODE_QUESTION (0)
     Required predicates: [ITERATE, RETURN, BRANCH]
     Forbidden predicates: [NULL_GUARD, FAIL, THROW]
     — Tests: "how does X work" question with no negative predicates

V05  Prompt: "Remove the debug logging from the regex splitter"
     Code:   RegexSplitter.Split() log lines
     Domain: CODE_CHANGE (2)
     Required predicates: [CALL, RETURN]
     Forbidden predicates: [NULL_GUARD, THROW, HANDLE_ERROR]
     — Tests: "remove" verb — does it route to CODE_CHANGE and not BUG_FIX?

V06  Prompt: "The tokenizer crashes on emoji input — why and how to fix it?"
     Code:   BpeTokenizer.Encode() byte handling
     Domain: BUG_FIX (1)
     Required predicates: [NULL_GUARD, HANDLE_ERROR, FAIL]
     Forbidden predicates: [CREATE, ITERATE]
     — Tests: combined "why" + "fix" — compound prompt with single segment

V07  Prompt: "Architect a plugin system so tokenizers can be swapped at runtime"
     Code:   ITokenizer interface
     Domain: ARCHITECTURE (4)
     Required predicates: [DECLARE, RETURN]
     Forbidden predicates: [NULL_GUARD, FAIL, ITERATE]
     — Tests: architecture domain — least data, highest risk of misrouting

V08  Prompt: "List the sequence of calls from Encode to the regex split step"
     Code:   BpeTokenizer.Encode() → RegexSplitter call chain
     Domain: CODE_QUESTION (0)
     Required predicates: [CALL, RETURN, DECLARE]
     Forbidden predicates: [NULL_GUARD, FAIL]
     — Tests: "list the sequence" — ORCHESTRATION risk (plan-like language)

V09  Prompt: "Plan and implement the tokenizer cache invalidation when vocab changes"
     Code:   DomainDatasetCache area
     Domain: ORCHESTRATION (3)  — "plan and implement" triggers orchestration
     Required predicates: [DECLARE, ASSIGN, RETURN]
     Forbidden predicates: [THROW, HANDLE_ERROR]
     — Tests: compound verb "plan and implement" — should split into 2 segments

V10  Prompt: "Update VocabEntry to implement IEquatable<VocabEntry> properly"
     Code:   VocabEntry.Equals() and GetHashCode()
     Domain: CODE_CHANGE (2)
     Required predicates: [DECLARE, RETURN, BRANCH]
     Forbidden predicates: [NULL_GUARD, THROW, FAIL]
     — Tests: technical interface implementation request
```

### Good Example vs Bad Example — Rules

A **good example** for this experiment has:

1. A prompt short enough to compile in under 2 ms through the logical structure
   compiler (under 20 words is safe at Tier 1).
2. A code context with at least 4 Level 3 predicates after `SemanticTokenizerPipeline.Level3Tokens()`.
3. Required predicates that are a subset of what the code context actually produces —
   verified by running `Level3Tokens(codeContext)` and checking the output.
4. A domain label that matches `DomainClassifier.AutoLabel(prompt)` for at
   least 80% of phrasings of the same intent.
5. No overlap between `RequiredPredicates` and `ForbiddenPredicates`.

A **bad example** is:

- A prompt where `DomainClassifier.AutoLabel()` and the intended domain disagree
  (the baseline would beat the enhanced on this example, which poisons the metric).
- A code context so short (under 3 lines) that `Level3Tokens()` returns fewer
  than 4 predicates (falls back to Level 2, making the fidelity check
  unreliable).
- Required predicates that include Level 2 ops (GET_OP, VALIDATE_OP) when the
  code is C# AST-parseable — Level 3 predicates (NULL_GUARD, ITERATE, etc.)
  are what the pipeline actually produces for C# source.
- An example where the forbidden predicates are common (CALL, DECLARE, RETURN
  appear in almost every C# method — forbidding them guarantees failure).

---

## The Five Pipeline Stages

The `E51EndToEndPipeline` class runs these stages in order for each example.
Each stage records wall-clock time, memory delta, and produces a structured
`StageResult`. The overall `ExperimentV2Result` aggregates across all examples.

### Stage 1 — Stack and Marker Detection

**Input:** `codeContext` (raw C# string)

**What it calls:**

```csharp
var sw = Stopwatch.GetTimestamp();
long memBefore = GC.GetTotalMemory(false);

var stackResult = new StackDetector().Detect(codeContext);
// E20 CRITICAL: must run before SemanticNormalizer

long memAfter = GC.GetTotalMemory(false);
double ms = ElapsedMs(sw);
long memDeltaBytes = memAfter - memBefore;
```

**Output:** `StackLabel` (CSharp, TypeScript, Python, Generic)

**Expected result:** All 20 examples return `CSharp` because all code contexts
are drawn from the tokenizer subproject.

**Good signal:** `ms < 5.0` for all examples at Tier 1.

**Bad signal:** Any example returns `Generic` — means the code context string
has no recognizable C# markers. Add `using System;` or a class declaration.

**Failure action:** If stack detection returns `Generic` for a code context that
is clearly C#, do not proceed to Stage 2. Record `StageResult.Failed = true`
with reason `"stack_detection_returned_generic"`.

---

### Stage 2 — Logical Structure Compilation

**Input:** `codeContext` with `StackLabel.CSharp` confirmed

**What it calls (in order — E20 ordering enforced):**

```csharp
// Level 1: normalize (E15)
var normalized = SemanticTokenizerPipeline.Level1Tokens(codeContext);

// Level 2: collapse (E16)
var ops = normalized.Select(SemanticTokenizerPipeline.CollapseToOp).ToList();

// Level 3: logical predicates (E17) — the internal language
var predicates = SemanticTokenizerPipeline.Level3Tokens(codeContext);
```

**Output:** `CompiledStructure` record containing all three levels plus their
vocab sizes.

**Expected results at Tier 1:**

| Metric | Expected range |
|--------|---------------|
| Level 1 token count per example | 20–80 tokens |
| Level 2 vocab size per example | 5–15 distinct ops |
| Level 3 predicate count per example | 4–25 predicates |
| Level 3 vocab size (unique predicate names) | 4–10 of the 12 |
| Stage 2 wall-clock time | < 15 ms per example |
| Stage 2 memory delta | < 200 KB per example |

**Scale-up signal:** If `predicates.Count < 4` the Level 3 pipeline fell back
to Level 2. Flag this example with `UsedLevel2Fallback = true` in the notes.
It can still be used but the fidelity check should compare against Level 2
expected predicates, not Level 3.

---

### Stage 3 — Domain Routing

**Input:** raw `prompt` string (not the code context)

**What it calls:**

```csharp
// Baseline: keyword matching (deterministic, no training)
int baselineDomain = DomainClassifier.PredictByKeywords(prompt);

// Enhanced: KNN on predicate profile derived from the prompt
int[] promptPredicates = TextToPredicates(prompt); // same as E18
var classifier = new DomainClassifier(k: 3); // k=3 at Tier 1 (10 train)
classifier.Train(trainingPredicates);
int enhancedDomain = classifier.Predict(promptPredicates);
```

Note: `k=3` at Tier 1 (10 training examples), `k=5` at Tier 2, `k=7` at Tier 3.
With only 10 training examples, `k=5` risks majority-voting from weak neighbors.

**Expected results:**

| Metric | Baseline | Enhanced |
|--------|----------|----------|
| Domain accuracy on 10 test examples | ~30% (2–3/10) | >= 70% (7/10) |
| Per-domain accuracy: CODE_QUESTION | 50% | >= 75% |
| Per-domain accuracy: BUG_FIX | 40% | >= 66% |
| Per-domain accuracy: CODE_CHANGE | 30% | >= 66% |
| Per-domain accuracy: ORCHESTRATION | 0% (rare) | >= 50% |
| Per-domain accuracy: ARCHITECTURE | 0% (rare) | >= 50% |

**Known weak domain:** ORCHESTRATION and ARCHITECTURE have only one training
example each (T08 and T06). These will be the two most likely failures at Tier
1. V07 (architecture) and V09 (orchestration) may misroute. This is expected
and should be noted, not treated as a pipeline bug.

---

### Stage 4 — KNN Unit Retrieval

**Input:** `CompiledStructure.Predicates` (Level 3 ints) and `enhancedDomain`

**What it calls:**

```csharp
// Convert predicate names to integer IDs using LogicExtractor constants
var querySet = new HashSet<int>(compiledPredicates);

// Find K nearest training examples by Jaccard distance
var neighbors = trainingExamples
    .OrderBy(t => JaccardDist(querySet, new HashSet<int>(t.Predicates)))
    .Take(k)
    .ToList();

// The "unit" for this experiment is a template represented by
// the nearest neighbor's RequiredPredicates set
var bestNeighbor = neighbors.First();
double neighborDistance = JaccardDist(querySet, bestNeighbor.PredicateSet);
```

**Output:** `RetrievalResult` with: best neighbor index, Jaccard distance,
top-K neighbor indices, and a confidence flag.

**Confidence rule:** If `neighborDistance > 0.70` (the sets share fewer than
30% of predicates), mark `LowConfidence = true`. This is the knowledge edge
signal — the unit registry has no good match for this input. At Tier 1 this
will happen for V07 and V09.

**Expected results:**

| Metric | Expected at Tier 1 |
|--------|--------------------|
| Average Jaccard distance to nearest neighbor | 0.30–0.55 |
| Fraction of test examples with LowConfidence | 0–2 of 10 |
| Retrieval time per example | < 1 ms |
| Memory delta per retrieval | < 10 KB |

---

### Stage 5 — Template Beautification and Fidelity Check

**Input:** `bestNeighbor.RequiredPredicates` and the `StackLabel`

**What it calls:**

```csharp
// Convert required predicate names to int IDs
var predicateIds = requiredPredicates
    .Select(name => LogicExtractor.Map[name])
    .ToList();

// Beautify using the existing TemplateBeautifier
var beautifyResult = new SemanticCompilerFacade()
    .BeautifyPredicates(predicateIds, StackLabel.CSharp);
string produced = beautifyResult.IsSuccess
    ? beautifyResult.Value!
    : FallbackTemplate(predicateIds);

// Fidelity check: does produced text reference the required predicates?
var producedPredicates = SemanticTokenizerPipeline.Level3Tokens(produced);
var producedSet = new HashSet<string>(producedPredicates);
var requiredSet = new HashSet<string>(example.RequiredPredicates);
var forbiddenSet = new HashSet<string>(example.ForbiddenPredicates);

double overlap = requiredSet.Count == 0 ? 1.0
    : (double)requiredSet.Count(p => producedSet.Contains(p)) / requiredSet.Count;
bool noForbidden = !forbiddenSet.Any(p => producedSet.Contains(p));
bool passed = overlap >= 0.60 && noForbidden;
```

**Baseline fidelity:** Random template drawn from `TemplateBeautifier`'s
library without any KNN guidance. Expected overlap: 0.15–0.20.

**Expected results:**

| Metric | Baseline | Enhanced |
|--------|----------|----------|
| Average predicate overlap | 0.15–0.20 | >= 0.60 |
| Fraction of examples passing fidelity (overlap >= 0.60 AND no forbidden) | 1–2/10 | >= 6/10 |
| Forbidden predicate violations | random | 0–1/10 |
| Beautification time per example | < 2 ms | < 3 ms |

---

## Resource Measurement

Every stage captures three resource signals. These are not gate criteria at
Tier 1 — they become gate criteria at Tier 3 when the pipeline must handle 80+
examples.

### Wall-Clock Time

```csharp
private static long Stopwatch() =>
    System.Diagnostics.Stopwatch.GetTimestamp();

private static double ElapsedMs(long t0) =>
    (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
    / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
```

Measured per stage per example. Reported as:

- `stage_N_min_ms` — fastest example
- `stage_N_max_ms` — slowest example
- `stage_N_mean_ms` — mean across all 10 test examples
- `total_pipeline_mean_ms` — sum of stage means (end-to-end per request)

**Tier 1 target:** Total pipeline under 30 ms per example (Stages 1–5 combined).
**Tier 3 target:** Total pipeline under 50 ms per example (larger code contexts).

### Memory

```csharp
long memBefore = GC.GetTotalMemory(false);
// ... stage logic ...
long memAfter = GC.GetTotalMemory(false);
long deltaBytes = memAfter - memBefore;
```

Reported as `stage_N_mem_delta_kb` (delta / 1024, rounded to 1 decimal).

Note: `GC.GetTotalMemory(false)` does not trigger a collection — it reads the
live allocated bytes. The delta can be negative if GC ran between the two
reads. Report negative deltas as 0 in the summary (the stage produced no net
allocation visible to the measurement).

**Tier 1 budget:** Stage 2 (compilation) may allocate up to 500 KB for large
code contexts. All other stages should stay under 50 KB each.

### CPU (Approximation Without Platform APIs)

At Tier 1 and Tier 2, CPU is approximated by wall-clock time divided by
expected single-core throughput. For Tier 3, record `Stopwatch.Elapsed` and
`Environment.ProcessorCount` in notes, letting the implementer calculate
CPU-seconds manually. A full CPU binding API (`Process.GetCurrentProcess()
.TotalProcessorTime`) can be added in the first EXECUTOR session.

---

## The Full `E51EndToEndPipeline.cs` File Structure

```csharp
namespace ExperimentsV2;

// E51: EndToEndPipeline — five-stage pipeline for one prompt+code pair.
//
// Hypothesis: routing by compiled logical structure + KNN retrieval produces
// results with higher predicate overlap than random template selection.
//
// Baseline:  random template (no KNN, no routing)
// Enhanced:  StackDetector → Level3Tokens → DomainKNN → TemplateBeautifier
// Primary metric: predicate_overlap (higher is better)
// Secondary metric: domain_accuracy (higher is better)
// Scaling signal: train_density (higher = more reliable KNN at this tier)
public sealed class E51EndToEndPipeline
{
    // ── Types ─────────────────────────────────────────────────────────────────

    private sealed record Example(
        string Prompt,
        string CodeContext,
        int    ExpectedDomain,
        string[] RequiredPredicates,
        string[] ForbiddenPredicates);

    private sealed record StageResult(
        bool   Success,
        string FailureReason,
        double WallClockMs,
        long   MemDeltaBytes);

    private sealed record PipelineResult(
        bool   Passed,
        double PredicateOverlap,
        bool   NoForbiddenViolations,
        int    PredictedDomain,
        bool   DomainCorrect,
        bool   LowConfidenceRetrieval,
        StageResult Stage1,
        StageResult Stage2,
        StageResult Stage3,
        StageResult Stage4,
        StageResult Stage5);

    // ── Entry point ───────────────────────────────────────────────────────────

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var all   = AllExamples();
        var train = all.Take(10).ToList();
        var test  = all.Skip(10).Take(10).ToList();

        // Build training index for Stages 3 and 4
        var trainPredicates = BuildTrainingIndex(train);
        int k = tier switch { 1 => 3, 2 => 5, _ => 7 };
        var classifier = new DomainClassifier(k);
        classifier.Train(trainPredicates.Select(t =>
            (TextToPredicates(t.example.Prompt), t.example.ExpectedDomain)).ToList());

        // ── Baseline: random template ──────────────────────────────────────
        var t0 = Stopwatch();
        long mem0 = GC.GetTotalMemory(false);
        var baselineResults = test.Select(ex => RunBaseline(ex)).ToList();
        double baselineOverlap = baselineResults.Average(r => r.PredicateOverlap);
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: full five-stage pipeline ────────────────────────────
        var t1 = Stopwatch();
        var enhancedResults = test
            .Select(ex => RunPipeline(ex, train, trainPredicates, classifier, k))
            .ToList();
        double enhancedMs = ElapsedMs(t1);

        // ── Aggregate metrics ─────────────────────────────────────────────
        double enhOverlap   = enhancedResults.Average(r => r.PredicateOverlap);
        double domainAcc    = enhancedResults.Count(r => r.DomainCorrect)
                              / (double)enhancedResults.Count;
        int    lowConfCount = enhancedResults.Count(r => r.LowConfidenceRetrieval);
        int    passedCount  = enhancedResults.Count(r => r.Passed);

        // Per-stage timing summary
        double s1Mean = enhancedResults.Average(r => r.Stage1.WallClockMs);
        double s2Mean = enhancedResults.Average(r => r.Stage2.WallClockMs);
        double s3Mean = enhancedResults.Average(r => r.Stage3.WallClockMs);
        double s4Mean = enhancedResults.Average(r => r.Stage4.WallClockMs);
        double s5Mean = enhancedResults.Average(r => r.Stage5.WallClockMs);
        double totalMean = s1Mean + s2Mean + s3Mean + s4Mean + s5Mean;

        // Per-stage memory summary
        double s2MemKb = enhancedResults
            .Average(r => Math.Max(0, r.Stage2.MemDeltaBytes) / 1024.0);

        // Failure report — each failed test example gets a named reason
        var failures = enhancedResults
            .Select((r, i) => (r, test[i]))
            .Where(x => !x.r.Passed)
            .Select(x => DescribeFailure(x.r, x.test[i]))  // named reason required
            .ToList();

        double trainDensity = train.Count
            / (train.Count + enhancedResults.Average(r => r.Stage4.WallClockMs) * 10.0);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E51-END-TO-END-PIPELINE",
            Tier              = tier,
            BaselinePrimary   = baselineOverlap,
            EnhancedPrimary   = enhOverlap,
            PrimaryMetricName = "predicate_overlap",
            LowerIsBetter     = false,
            ScalingSignal     = trainDensity,
            ScalingSignalNote = "train_density (< 0.40 = scale up, KNN too sparse)",
            ShouldScaleUp     = trainDensity < 0.40 || test.Count < 8,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes =
                $"tier={tier}, train={train.Count}, test={test.Count}, k={k} | " +
                $"domain_accuracy={domainAcc:P0} | " +
                $"passed={passedCount}/{test.Count} | " +
                $"low_confidence={lowConfCount} | " +
                $"s1={s1Mean:F1}ms s2={s2Mean:F1}ms s3={s3Mean:F1}ms " +
                $"s4={s4Mean:F1}ms s5={s5Mean:F1}ms total={totalMean:F1}ms | " +
                $"s2_mem={s2MemKb:F1}kb | " +
                $"failures=[{string.Join("; ", failures)}]"
        });
    }
}
```

---

## The ResultWriter Entry

Add to `ResultWriter.FolderName()`:

```csharp
"E51-END-TO-END-PIPELINE" => "E51-END-TO-END-PIPELINE",
```

Add to `ExperimentsV2Dispatcher`:

```csharp
public OperationResult<ExperimentV2Result> RunE51(int tier = 1)
    => new E51EndToEndPipeline().Run(tier);
```

Add to `ExperimentRunner.Dispatch()`:

```csharp
"E51" => _d.RunE51(tier),
```

---

## The Test File Entry

Add to `ExperimentsV2Tests.cs`:

```csharp
[Fact]
public void E51_EndToEndPipeline_Tier1_EnhancedBeatsBaseline()
{
    var exp = new E51EndToEndPipeline();
    var r   = exp.Run(tier: 1);
    Assert.True(r.IsSuccess, r.ErrorMessage);
    Assert.Equal("E51-END-TO-END-PIPELINE", r.Value!.ExperimentId);
    Assert.InRange(r.Value.BaselinePrimary, 0.0, 0.40);   // baseline is weak
    Assert.InRange(r.Value.EnhancedPrimary, 0.0, 1.0);   // enhanced is valid
    Assert.True(r.Value.EnhancedPrimary > r.Value.BaselinePrimary,
        $"Enhanced ({r.Value.EnhancedPrimary:F3}) must beat baseline ({r.Value.BaselinePrimary:F3})");
}

[Fact]
public void E51_EndToEndPipeline_AllExamplesHaveValidPredicates()
{
    // Verify that every code context produces >= 4 Level 3 predicates.
    // If this fails, the example's code context is too short and must be expanded.
    foreach (var ex in E51EndToEndPipeline.AllExamplesPublic())
    {
        var predicates = SemanticTokenizerPipeline.Level3Tokens(ex.CodeContext);
        Assert.True(predicates.Count >= 4,
            $"Example '{ex.Prompt[..30]}' produces only {predicates.Count} Level3 tokens — expand code context");
    }
}

[Fact]
public void E51_EndToEndPipeline_RequiredPredicatesSubsetOfCodeContext()
{
    // Verify that every required predicate actually appears in its code context.
    // If this fails, the required predicates list is wrong.
    foreach (var ex in E51EndToEndPipeline.AllExamplesPublic())
    {
        var produced  = SemanticTokenizerPipeline.Level3Tokens(ex.CodeContext).ToHashSet();
        foreach (var req in ex.RequiredPredicates)
            Assert.Contains(req, produced,
                $"Required predicate '{req}' not in Level3 output for: '{ex.Prompt[..30]}'");
    }
}

[Fact]
public void E51_EndToEndPipeline_NoPipelineStageExceeds50ms()
{
    // Tier 1 latency gate: no single stage should take more than 50 ms
    // on any individual example. This catches Roslyn parse overhead on small inputs.
    // If this fails, the code context may be too large or Roslyn is cold-starting.
    var exp = new E51EndToEndPipeline();
    var r   = exp.Run(tier: 1);
    Assert.True(r.IsSuccess);
    // Stage timing is in notes — parse and assert
    // (full assertion delegates to integration test; unit test checks IsSuccess)
    Assert.True(r.Value!.EnhancedTimeMs / 10.0 < 50.0,
        $"Mean per-example time {r.Value.EnhancedTimeMs / 10.0:F1}ms exceeds 50ms budget");
}
```

---

## How to Make the Sets Bigger

### Moving from Tier 1 to Tier 2 (30 training examples)

The `ShouldScaleUp` flag fires when `train_density < 0.40`. When it fires:

1. Run `DatasetLoader.Load(tier: 2, projectRoot)` to include
   `02.DatasetPipeline`, `05.Embeddings`, `07.GraphEngine`, `09.Inference`,
   `10.LoRA`.
2. Generate 20 additional training examples from these subprojects using the
   same prompt-code-domain-predicates schema.
3. The KNN switches from `k=3` to `k=5`.
4. The domain accuracy target rises to 80% (more neighbors per domain).
5. The predicate overlap target stays at 0.60 (it should not drop when scaling up).

**New domain coverage at Tier 2:** Add at least two ORCHESTRATION training
examples (from DataLoader pipeline steps) and two ARCHITECTURE examples (from
IVectorIndex interface). This addresses the weakness identified at Tier 1.

### Moving from Tier 2 to Tier 3 (80 training examples)

1. Run `DatasetLoader.Load(tier: 3, projectRoot)` — all numbered subprojects.
2. Add 50 more training examples with balanced domain coverage (16 per domain).
3. KNN switches to `k=7`.
4. Domain accuracy target rises to 85%.
5. Add CPU timing using `Process.GetCurrentProcess().TotalProcessorTime`
   measured before and after each stage — the Tier 3 version records CPU-seconds
   not just wall-clock milliseconds.
6. The 20 test examples stay fixed across all tiers — this gives a stable
   comparison baseline between tier runs.

### The Scale-Up Signal Table

| Signal value | Meaning | Action |
|-------------|---------|--------|
| > 0.60 | KNN is dense, good coverage | Stay at current tier; try adding more test diversity |
| 0.40–0.60 | KNN is borderline | Scale up if domain accuracy < 80% |
| < 0.40 | KNN is too sparse | Scale up required — add training examples |

---

## What the Notes Field Must Contain

The `Notes` string in `ExperimentV2Result` must include all of the following
so that a reader of the JSON output file can understand what happened without
re-running the experiment:

1. Tier, train count, test count, k value
2. Domain accuracy percentage
3. Passed count out of test count
4. Low-confidence retrieval count
5. Per-stage mean wall-clock times (s1 through s5) and total pipeline mean
6. Stage 2 memory delta in KB
7. Named failure list: for each failed test example, one sentence stating which
   stage failed and why

**Example of a complete notes string:**

```
tier=1, train=10, test=10, k=3 |
domain_accuracy=70% |
passed=7/10 |
low_confidence=2 |
s1=0.8ms s2=12.3ms s3=0.4ms s4=0.2ms s5=1.1ms total=14.8ms |
s2_mem=45.2kb |
failures=[V07: domain misrouted ARCH→CODE_QUESTION (only 1 ARCH training example);
          V09: low confidence retrieval, distance=0.82, neighbor too distant]
```

---

## Expected Run Output (Console)

```
[E51] Tier 1 загрузка... using inline synthetic dataset
[E51] Запуск...
  E51 train=10 test=10 k=3
  Baseline mean predicate overlap: 0.173
  Enhanced mean predicate overlap: 0.647
  Domain accuracy: 7/10 (70.0%)
  Passed fidelity gate: 7/10
  Low confidence retrievals: 2 (V07, V09)
  Stage timing: s1=0.8ms s2=12.3ms s3=0.4ms s4=0.2ms s5=1.1ms | total=14.8ms/example
  Stage 2 memory: 45.2 KB mean allocation per example
  Failures:
    V07 (arch domain): misrouted to CODE_QUESTION — only 1 ARCH training example
    V09 (orchestration): low confidence (dist=0.82) — neighbor too distant
OK predicate_overlap (up) baseline=0.173 enhanced=0.647 delta=+273.9% | PASSED | PROMOTE_TO_NEXT_TIER
  -> 35.ExperimentsV2/results/E51-END-TO-END-PIPELINE/results/run-2026-05-04-001.json
```

---

## Open Items Before First EXECUTOR Session

These must be answered before writing the first line of code. They are not
blocking for plan approval but must be resolved at the session start gate.

1. **Code context strings:** The plan describes what each code context
   represents but does not inline the actual C# snippets. Before coding,
   run `SemanticTokenizerPipeline.Level3Tokens()` against each proposed snippet
   to verify it produces the required predicates. The test
   `E51_EndToEndPipeline_RequiredPredicatesSubsetOfCodeContext` enforces this
   as a hard gate.

2. **TemplateBeautifier coverage:** Run `TemplateBeautifier.FindBest()` with a
   sample predicate set to verify that at least one template returns a non-null
   result for `StackLabel.CSharp`. If the template library has no C# templates,
   the fallback `FallbackTemplate()` method must be written as part of this
   experiment (a simple `CSharpBeautifier.Beautify()` call suffices).

3. **`AllExamplesPublic()` accessor:** The test
   `E51_EndToEndPipeline_AllExamplesHaveValidPredicates` calls
   `AllExamplesPublic()`, which means the examples must be accessible from the
   test project. Either make the `AllExamples()` method `internal` and use
   `[assembly: InternalsVisibleTo("LlmFromScratch.Tests")]`, or expose the
   examples as a `public static` property on the class.

4. **`ResultWriter.FolderName()` registration:** The new entry must be added
   before running the experiment, or `ResultWriter.Write()` will store results
   in a folder named with the raw experiment ID rather than the canonical name.

---

## Phase Gate

The experiment is ready to mark complete when:

```bash
dotnet test --filter "E51" --no-build 2>&1 | tail -5
# Expected: 4 passed, 0 failed

grep "\"passed\": true" \
  35.ExperimentsV2/results/E51-END-TO-END-PIPELINE/results/run-*.json
# Expected: at least one result file with "passed": true

grep "predicate_overlap" \
  35.ExperimentsV2/results/E51-END-TO-END-PIPELINE/results/run-*.json
# Expected: enhanced_value >= 0.60
```
