# Plan — E13B Expression Completion Unit: Experiment to Production
# Version: 1.0 — 2026-05-07
# Session type: ARCHITECT
# Test floor entering this plan: 1251
# Experiment document: E13B-EXPRESSION-PREFIX-COMPLETION-EXPERIMENT.md
# Companion to: PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4.md (Phase 9)

---

## What this plan does

The E13B experiment passed its stated success criteria (100% corpus retrieval
accuracy, 93.3% lift over baseline, zero knowledge-edge rate). Before the unit
enters production two things remain:

1. A held-out split test proving the unit retrieves correctly on expressions
   not in the training corpus — the experiment used train == validation, which
   only proves corpus retrieval.

2. A production `ExpressionCompletionUnit` class registered in the unit registry
   following the same wrapper pattern as `PromptDomainClassifierUnit`.

This plan delivers both.

---

## Architecture pattern — how units enter production in this codebase

Project 23 (`SemanticCompilerAdapter`) contains plain classes. It does not
reference project 21 and does not know about `ILogicalUnit`.

Project 21 (`AgentInfrastructure`) references project 23. It wraps project 23
classes in `ILogicalUnit` adapters when registry registration is needed.

The established pattern, from `PromptDomainClassifier.cs`:

```
23.SemanticCompilerAdapter/DomainClassifierUnit.cs
    — plain class, no ILogicalUnit, no project 21 reference
    — owns the classification algorithm

21.AgentInfrastructure/Logical/PromptDomainClassifier.cs
    — PromptDomainClassifierUnit : ILogicalUnit<string, PromptDomainClassification>
    — wraps DomainClassifierUnit, delegates ExecuteAsync to it
    — registered in the unit registry by AdaptiveLogicalOrchestrator
```

`ExpressionCompletionUnit` follows the same two-file pattern.

---

## What `ExpressionCompletionUnit` does

Given a partial expression string such as `"Revenue - Cost ="`, it returns
the predicted completion string `"Profit"` by querying a trained
`KnnLogicalUnit<string, string>` index. The index is built from a corpus
of known complete expression rows extracted from the teaching ledger.

The unit is not a general algebraic reasoner. It is a corpus retrieval unit:
if a partial expression is close to a known expression in the training set,
it returns that expression's right-hand side. Unseen expressions not covered
by the corpus emit a knowledge-edge warning and return the nearest-found
completion with low confidence rather than failing.

---

## Baseline gate

```bash
cd C:\Projects\llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Record the count. Every gate must match or exceed 1251.

---

## Phase 1 — Held-out split validation

Before writing production code, run a harder version of the E13B experiment
that proves the unit works on genuinely unseen expressions.

**File to create:**
`35.ExperimentsV2/Experiments/E13BHeldOutValidation.cs`

```
Algorithm:

Training set (20 rows) — expressions the unit learns from:
  "Y + Z = X"
  "Width + Padding = OuterWidth"
  "Height + Margin = OuterHeight"
  "Subtotal + Tax = Total"
  "Price * Quantity = Total"
  "Revenue - Cost = Profit"
  "Start + Duration = End"
  "End - Start = Duration"
  "Current + Delta = Next"
  "Previous + Change = Current"
  "Base * Rate = Fee"
  "Principal * Interest = Charge"
  "Rows * Columns = Cells"
  "PageSize * PageIndex = Offset"
  "Offset + Limit = WindowEnd"
  "Input + Transform = Output"
  "Prompt + Policy = Decision"
  "Memory + Query = Recall"
  "Branch + Evidence = Verdict"
  "Payload + Metadata = Envelope"

Held-out set (10 rows) — expressions NOT in training:
  "Signal + Threshold = Alert"       → expected "Alert"
  "Vector + Predicate = Score"       → expected "Score"
  "Score + Margin = Rank"            → expected "Rank"
  "Plan + Constraint = Task"         → expected "Task"
  "Spec + Design = Blueprint"        → expected "Blueprint"
  "Frame + Bytes = Payload"          → expected "Payload"
  "Assets - Liabilities = Equity"    → expected "Equity"
  "Errors + Warnings = Findings"     → expected "Findings"
  "Request + Context = Prompt"       → expected "Prompt"
  "Success + Warning = Status"       → expected "Status"

Metrics to record:
  held_out_top1_accuracy     (how often top-1 hit equals expected completion)
  held_out_avg_cosine_gap    (gap between top-1 and top-2 on held-out set)
  held_out_knowledge_edge_rate (fraction below confidence floor 0.65)
  training_top1_accuracy     (same metric on the 20 training rows — should stay 100%)
```

Success gate for Phase 1:
- `held_out_top1_accuracy >= 0.60` — at least 6 of 10 unseen expressions correct
- `held_out_knowledge_edge_rate < 0.40` — fewer than 4 of 10 fall below floor
- `training_top1_accuracy == 1.0` — corpus retrieval unchanged

If `held_out_top1_accuracy < 0.60`: the unit only memorises, not matches.
Do not proceed to Phase 2. Document the finding and stop.

### Gate B — Phase 1

```bash
dotnet build 35.ExperimentsV2/ 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: zero build errors, floor unchanged (Phase 1 adds no test suite tests —
the validation runner is an experiment, not an xunit test).

---

## Phase 2 — ExpressionCorpusBuilder in project 23

The production unit needs a way to build its training corpus from teaching
ledger rows. This is a separate class from the unit itself.

**File:** `23.SemanticCompilerAdapter/ExpressionCorpusBuilder.cs`

```
public static class ExpressionCorpusBuilder
{
    // Extract (partial, completion) pairs from a list of complete expression strings.
    // Each complete expression must be in the form "LHS operator = RHS".
    // Returns one pair per complete expression.
    public static IReadOnlyList<(string Partial, string Completion)>
        ExtractPairs(IReadOnlyList<string> completeExpressions)

    // Build training examples for KnnLogicalUnit from expression pairs.
    // ExampleId is derived from the partial expression text.
    public static IReadOnlyList<LogicalKnnTrainingExample<string, string>>
        BuildTrainingExamples(IReadOnlyList<(string Partial, string Completion)> pairs)
}
```

`ExtractPairs` splits on the rightmost `=` and trims both sides. If no `=`
is found, the row is skipped. The partial is `"LHS ="` (left side plus equals
sign), the completion is the trimmed right side.

`BuildTrainingExamples` converts each pair to:
```csharp
new LogicalKnnTrainingExample<string, string>(
    ExampleId:    Sanitise(pair.Partial),  // slug from partial text
    Input:        pair.Partial,
    Output:       pair.Completion,
    PredicateIds: [],
    DomainTags:   ["expression_completion"],
    EvidenceIds:  [$"corpus-row:{pair.Partial}"])
```

Note: `LogicalKnnTrainingExample` is in project 21. Project 23 does not
reference project 21. `BuildTrainingExamples` must therefore return a type
that project 23 can express without importing project 21.

**Resolution:** `BuildTrainingExamples` is NOT in project 23. Only `ExtractPairs`
lives in project 23. `BuildTrainingExamples` lives in the project 21 wrapper
(Phase 3), where `LogicalKnnTrainingExample` is accessible.

### Gate B — Phase 2

```bash
dotnet build 23.SemanticCompilerAdapter/ 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests to add (in `Tests/SemanticCompilerTests.cs` or a new
`Tests/ExpressionCompletionTests.cs`):
- `ExtractPairs` splits `"Revenue - Cost = Profit"` into partial `"Revenue - Cost ="` and completion `"Profit"`
- `ExtractPairs` skips rows with no `=`
- `ExtractPairs` handles leading and trailing whitespace
- `ExtractPairs` splits on the last `=` when multiple `=` signs exist

---

## Phase 3 — ExpressionCompletionUnit wrapper in project 21

Following the `PromptDomainClassifierUnit` pattern exactly.

**File:** `21.AgentInfrastructure/Logical/ExpressionCompletionUnit.cs`

```csharp
// ExpressionCompletionUnit retrieves the right-hand side of a partial expression
// from a trained KNN index of known complete expressions.
// E13B: 100% corpus retrieval accuracy, 93.3% lift over majority baseline.
// Use for: autocomplete of known expressions, flow branch planning,
// predicate completion from partial specifications.
public sealed class ExpressionCompletionUnit : ILogicalUnit<string, string>
{
    public const string Id      = "unit-expression-completion-v1";
    public const string Role    = "expression_completion";
    public const string DomainTag = "expression_corpus";

    private readonly KnnLogicalUnit<string, string> _knn;

    // Constructor takes a pre-trained KnnLogicalUnit.
    // Use Train(corpus) factory to build from a list of complete expressions.
    public ExpressionCompletionUnit(KnnLogicalUnit<string, string> knn)
    {
        _knn = knn;
    }

    // Factory: build a trained unit from a list of complete expression strings.
    // Each string must be in the form "LHS operator = RHS".
    public static OperationResult<ExpressionCompletionUnit> Train(
        IReadOnlyList<string> completeExpressions,
        int vectorDimensions = 64,
        int k = 3,
        double confidenceFloor = 0.65)
    {
        var pairs = SemanticCompilerAdapter.ExpressionCorpusBuilder.ExtractPairs(completeExpressions);
        if (pairs.Count == 0)
            return OperationResult<ExpressionCompletionUnit>.Fail(
                "No valid expression pairs found.", "NO_EXPRESSION_PAIRS");

        var examples = pairs.Select((pair, index) => new LogicalKnnTrainingExample<string, string>(
            ExampleId:    $"expr-{index + 1:D3}",
            Input:        pair.Partial,
            Output:       pair.Completion,
            PredicateIds: [],
            DomainTags:   [DomainTag],
            EvidenceIds:  [$"corpus:{pair.Partial}"])).ToList();

        var trained = KnnLogicalUnit<string, string>.Train(
            Id, Role, DomainTag, examples,
            textSelector: s => s,
            vectorDimensions: vectorDimensions,
            k: k,
            confidenceFloor: confidenceFloor);

        return trained.IsSuccess
            ? OperationResult<ExpressionCompletionUnit>.Ok(new ExpressionCompletionUnit(trained.Value!))
            : OperationResult<ExpressionCompletionUnit>.Fail(trained.ErrorMessage, trained.ErrorCode);
    }

    // Predict the completion of a partial expression.
    // Returns knowledge-edge warning when confidence is below floor.
    public Task<OperationResult<LogicalUnitResult<string>>> ExecuteAsync(
        LogicalUnitRequest<string> request,
        CancellationToken cancellationToken = default)
        => _knn.ExecuteAsync(request, cancellationToken);
}
```

**Registration in `AdaptiveLogicalOrchestrator`:**

Do not register `ExpressionCompletionUnit` during orchestrator startup. The unit
requires a trained index and the training corpus is not available at startup.
Registration happens at the call site where the corpus is known — the orchestrator
registers it lazily when a node requests expression completion.

The registration API:
```csharp
units.Register<string, string>(
    ExpressionCompletionUnit.Id,
    ExpressionCompletionUnit.Role,
    ExpressionCompletionUnit.DomainTag,
    version: "1.0.0",
    trustState: LogicalUnitTrustState.Trusted,
    isLearned: true,
    unit: expressionCompletionUnit);
```

### Gate B — Phase 3

```bash
dotnet build 21.AgentInfrastructure/ 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests to add (in `Tests/AgentInfrastructureTests.cs` or
`Tests/ExpressionCompletionTests.cs`):
- `ExpressionCompletionUnit.Train` with a 3-expression corpus succeeds
- Trained unit retrieves exact match for a known partial expression
- Trained unit emits knowledge-edge warning for an unrecognised partial
- Unit returns the nearest completion (not failure) when confidence is below floor
- Training with zero valid expressions returns a failure result
- `ExpressionCompletionUnit` implements `ILogicalUnit<string, string>`

---

## Phase 4 — Add project 23 reference to test project

The test project currently does not reference project 23. `ExpressionCorpusBuilder`
tests require it.

**File:** `Tests/LlmFromScratch.Tests.csproj`

Add:
```xml
<ProjectReference Include="..\23.SemanticCompilerAdapter\SemanticCompilerAdapter.csproj" />
```

Confirm the test project builds after this addition:
```bash
dotnet build Tests/LlmFromScratch.Tests.csproj 2>&1 | grep "error" | head -10
```

Expected: zero errors.

---

## Phase 5 — Full suite verification and commit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New test count:
- Phase 2 (ExpressionCorpusBuilder): 4 tests
- Phase 3 (ExpressionCompletionUnit): 6 tests
- Total new: 10 tests
- New floor: 1251 + 10 = 1261

Commit message:
```
implement E13B ExpressionCompletionUnit — corpus prefix retrieval via KNN

Phase 1 held-out validation result:
  Training set: 20 expressions — 100% retrieval accuracy (unchanged)
  Held-out set: 10 unseen expressions — [record actual accuracy here]
  Knowledge-edge rate on held-out: [record actual rate here]

New classes:
  23.SemanticCompilerAdapter/ExpressionCorpusBuilder.cs
    ExtractPairs: splits "LHS operator = RHS" strings into (partial, completion) pairs
    Skips rows with no =; splits on rightmost = when multiple present

  21.AgentInfrastructure/Logical/ExpressionCompletionUnit.cs
    ILogicalUnit<string, string> wrapper over KnnLogicalUnit<string, string>
    Train factory: builds trained unit from a list of complete expression strings
    Role: "expression_completion", DomainTag: "expression_corpus"
    Confidence floor: 0.65 (lowered from default 0.70 — partial expressions
    share most but not all tokens with their completions)

Tests added: 10
  4 ExpressionCorpusBuilder unit tests
  6 ExpressionCompletionUnit unit tests

Floor raises from 1251 to 1261.
Tests project now references 23.SemanticCompilerAdapter.

Note: registration is caller-side (requires trained corpus).
ExpressionCompletionUnit is not registered at orchestrator startup.
```

---

## Execution order and stop conditions

Phases in strict order: baseline gate → 1 → 2 → 3 → 4 → 5.

**Stop at Phase 1 if held-out accuracy < 0.60.** This is the only conditional
stop. If fewer than 6 of 10 unseen expressions retrieve correctly, the unit does
not generalise beyond memorisation. Document the result, add it to the experiment
history, and do not proceed to Phase 2. The production unit is not viable until
a better vectorisation or a larger corpus improves held-out accuracy.

Stop immediately if:
- Any Gate B build step produces errors
- Any phase causes a previously passing test to fail
- A test is modified to suppress a genuine regression

---

## What this plan does NOT change

| Component | Unchanged |
|---|---|
| `KnnLogicalUnit<TInput, TOutput>` | Unchanged — `ExpressionCompletionUnit` delegates to it |
| `AdaptiveLogicalOrchestrator` startup | Unchanged — no registration at startup |
| `DomainClassifierUnit` | Unchanged |
| Nine-node flow | Unchanged — unit is called by nodes that need it, not wired globally |
| Teaching path | Unchanged — expression completion does not interact with DPO |
| `LogicalKnnTrainingExample` | Unchanged — used as-is |

---

## Relationship to other plans

**PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4.md Phase 9** named
`ExpressionCompletionUnit` as the production target for E13B. This plan
is Phase 9. It can execute before or after v4 Phases 1-8 because it touches
different files.

**E13B-EXPRESSION-PREFIX-COMPLETION-EXPERIMENT.md** is the experiment contract
this plan promotes to production. The held-out validation in Phase 1 of this
plan is the missing half of that experiment.

---

## Where held-out accuracy is expected to land

The three lowest-gap rows from the original 30-row run were:
- `Assets - Liabilities = Equity` — gap 0.1817
- `Errors + Warnings = Findings` — gap 0.1817
- `Score + Margin = Rank` — gap 0.2285

All three are in the held-out set. These are where incorrect retrievals are
most likely — the second-nearest neighbour is very close to the correct one.
The minimum cosine gap of 0.1817 means another expression in the training set
shares almost as many tokens with the query as the correct completion does.

The six held-out expressions with higher expected gaps (Signal, Vector, Plan,
Spec, Frame, Request) should retrieve correctly. The four lowest-gap held-out
expressions (Assets, Errors, Score, Success) may not. An accuracy of 6/10 to
8/10 is the realistic range.

If the result is below 6/10, the fix is expanding the training corpus with
domain-specific expressions relevant to the application (orchestration, logistics,
security, UI) rather than the generic mathematical expressions used here. A
corpus of 200 real application expressions covering the actual vocabulary the
unit will encounter in production is likely to achieve 80–90% held-out accuracy.

---

# Plan version: 1.0
# Author: ARCHITECT session 2026-05-07
# Phases: 5 (Phase 1 is a conditional gate)
# New classes: ExpressionCorpusBuilder (project 23), ExpressionCompletionUnit (project 21)
# New tests: 10
# New floor: 1261
# Confidence floor: 0.65 (lower than default 0.70 — partial expressions justified)
# Phase 1 stop condition: held-out accuracy < 0.60
