# Plan — E13B Expression Completion Unit: Exact Learned-Memory Design
# Version: 2.0 — 2026-05-07
# Supersedes: PLAN-E13B-EXPRESSION-COMPLETION-UNIT-v1.md
# Session type: ARCHITECT
# Test floor entering this plan: 1251

---

## What changed from v1 and why

v1 built on `KnnLogicalUnit<string, string>` and tried to use a similarity
threshold to prevent wrong guesses on unseen expressions. The held-out
validation proved that nearest-neighbour guessing is fundamentally wrong for
this use case: confidence 0.74 on a completely wrong answer, `Charge`
predicted three times for unrelated expressions.

The correct design is not a better threshold — it is no nearest-neighbour at
all. The unit should behave like a dictionary: if the key exists, return the
value; if it does not exist, return nothing. This is exact learned-memory
mode.

`KnnLogicalUnit` is not changed. Other units that need nearest-neighbour
behaviour keep it. `ExpressionCompletionUnit` does not use `KnnLogicalUnit`
at all — it uses a plain `Dictionary<string, string>` internally.

---

## Architecture

### New output type — project 21

```csharp
// ExpressionCompletionResult carries a clean matched/abstained signal.
// Matched = true:  Completion contains the known right-hand side.
// Matched = false: Completion is null. Caller treats this as no-effect.
public sealed record ExpressionCompletionResult(
    bool Matched,
    string? Completion,
    double Confidence,
    string Reason);
```

`Confidence` is always 1.0 when matched (exact dictionary hit) and 0.0 when
not matched. `Reason` is `"exact_match"` or `"expression_not_seen"`.

### New unit — project 21

```
ILogicalUnit<string, ExpressionCompletionResult>
```

Internal state: a `Dictionary<string, string>` keyed on the normalised
partial expression (trimmed, whitespace-collapsed, lowercased). Built once at
`Train` time. Immutable after construction. No KNN index, no BPE tokeniser,
no AdamW optimizer.

### New corpus builder — project 23

`ExpressionCorpusBuilder.ExtractPairs` splits complete expression strings
into `(partial, completion)` pairs. This is unchanged from v1 in purpose, but
the output is used differently: instead of building `KnnLogicalUnit` training
examples, the pairs are loaded directly into the dictionary.

### Project boundary

Project 23 only owns `ExpressionCorpusBuilder.ExtractPairs` (string
splitting, no project 21 types). The dictionary construction, normalisation,
and `ILogicalUnit` registration all live in project 21.

---

## Normalisation contract

The normalisation applied at both train time and query time must be
identical, otherwise a training key and a runtime query for the same
expression will not match.

Normalisation steps in order:
1. Trim leading and trailing whitespace.
2. Collapse all internal whitespace runs to a single space.
3. Lowercase the entire string.

Example: `"  Revenue - Cost =  "` → `"revenue - cost ="`.

The normalisation is a static helper method on `ExpressionCompletionUnit`,
not on `ExpressionCorpusBuilder`. Project 23 does not need to know about it.

---

## New success gate

v1 required held-out accuracy ≥ 60%. That gate was wrong for exact-memory
mode — the unit is not supposed to guess unseen expressions.

v2 success gate:

```
Training partials (20 rows):
  - Matched rate:    100%   (every training partial found in dictionary)
  - Completion correct: 100% (every matched completion is the right answer)

Held-out partials (10 rows):
  - Matched rate:    0%    (no unseen partial should produce a match)
  - Abstain rate:    100%  (every unseen partial returns Matched=false)

False positive rate: 0%
  (no held-out partial should return Matched=true with wrong completion)
```

This gate is binary and deterministic. There is no probabilistic threshold.
Either the dictionary behaves correctly or it does not.

---

## Phase 1 — ExpressionCompletionResult in project 21

**File:** `21.AgentInfrastructure/Logical/ExpressionCompletionResult.cs`

```csharp
namespace AgentInfrastructure.Logical;

// Result type for ExpressionCompletionUnit.
// Matched = true: the partial expression was found in the learned corpus.
// Matched = false: the partial was not seen during training — caller treats
//                  this as no-effect.
public sealed record ExpressionCompletionResult(
    bool Matched,
    string? Completion,
    double Confidence,
    string Reason);
```

No other files change in this phase.

### Gate — Phase 1
```bash
dotnet build 21.AgentInfrastructure/ 2>&1 | grep "error" | head -5
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
```
Expected: zero errors, floor unchanged (no tests added yet).

---

## Phase 2 — ExpressionCorpusBuilder in project 23

**File:** `23.SemanticCompilerAdapter/ExpressionCorpusBuilder.cs`

```csharp
namespace SemanticCompilerAdapter;

// Extracts (partial, completion) pairs from complete expression strings.
// A complete expression is in the form "LHS operator = RHS".
// Splitting on the rightmost = gives partial = "LHS operator =" and
// completion = "RHS". Rows with no = are skipped.
public static class ExpressionCorpusBuilder
{
    public static IReadOnlyList<(string Partial, string Completion)>
        ExtractPairs(IReadOnlyList<string> completeExpressions)
    {
        var pairs = new List<(string, string)>();
        foreach (var expr in completeExpressions)
        {
            var idx = expr.LastIndexOf('=');
            if (idx < 0) continue;
            var partial    = expr[..(idx + 1)].Trim();
            var completion = expr[(idx + 1)..].Trim();
            if (string.IsNullOrWhiteSpace(partial) ||
                string.IsNullOrWhiteSpace(completion)) continue;
            pairs.Add((partial, completion));
        }
        return pairs;
    }
}
```

### Gate — Phase 2

New tests in `Tests/ExpressionCompletionTests.cs`:
- `ExtractPairs` splits `"Revenue - Cost = Profit"` into
  `("Revenue - Cost =", "Profit")`
- `ExtractPairs` skips rows with no `=`
- `ExtractPairs` splits on the rightmost `=` when multiple `=` exist
- `ExtractPairs` skips rows where either side is empty after trim

```bash
dotnet build 23.SemanticCompilerAdapter/ 2>&1 | grep "error" | head -5
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
```
Expected: 4 new tests passing, floor 1251 → 1255.

---

## Phase 3 — ExpressionCompletionUnit in project 21

**File:** `21.AgentInfrastructure/Logical/ExpressionCompletionUnit.cs`

```csharp
namespace AgentInfrastructure.Logical;

using SemanticCompilerAdapter;

// ExpressionCompletionUnit is an exact learned-memory unit.
// It stores a dictionary of normalised partial expressions → completions
// built from a training corpus. At query time it does an exact dictionary
// lookup after applying the same normalisation. If the normalised partial
// is in the dictionary it returns the stored completion with Matched=true
// and Confidence=1.0. If it is not in the dictionary it returns Matched=false
// with no completion — the caller receives a clean no-effect signal.
//
// This unit does NOT use nearest-neighbour search. It will never guess.
// It is safe to use in any context where a wrong guess is worse than no
// answer.
public sealed class ExpressionCompletionUnit
    : ILogicalUnit<string, ExpressionCompletionResult>
{
    public const string Id        = "unit-expression-completion-v2";
    public const string Role      = "expression_completion";
    public const string DomainTag = "expression_corpus";

    private readonly IReadOnlyDictionary<string, string> _lookup;

    // Private — use Train() factory.
    private ExpressionCompletionUnit(
        IReadOnlyDictionary<string, string> lookup)
    {
        _lookup = lookup;
    }

    // Factory: build a trained unit from a list of complete expressions.
    // Each string must be in the form "LHS operator = RHS".
    // Duplicate normalised partials: last one wins.
    public static OperationResult<ExpressionCompletionUnit> Train(
        IReadOnlyList<string> completeExpressions)
    {
        var pairs = ExpressionCorpusBuilder.ExtractPairs(completeExpressions);
        if (pairs.Count == 0)
            return OperationResult<ExpressionCompletionUnit>.Fail(
                "No valid expression pairs found in corpus.",
                "NO_EXPRESSION_PAIRS");

        var lookup = new Dictionary<string, string>(
            StringComparer.Ordinal);
        foreach (var (partial, completion) in pairs)
            lookup[Normalise(partial)] = completion;

        return OperationResult<ExpressionCompletionUnit>.Ok(
            new ExpressionCompletionUnit(lookup));
    }

    // Normalise: trim → collapse internal whitespace → lowercase.
    // Applied identically at train time and query time.
    public static string Normalise(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var trimmed = s.Trim();
        // Collapse internal whitespace runs to single space.
        var collapsed = System.Text.RegularExpressions.Regex.Replace(
            trimmed, @"\s+", " ");
        return collapsed.ToLowerInvariant();
    }

    public Task<OperationResult<LogicalUnitResult<ExpressionCompletionResult>>>
        ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(
                OperationResult<LogicalUnitResult<ExpressionCompletionResult>>
                    .Fail("Cancelled.", "CANCELLED"));

        if (string.IsNullOrWhiteSpace(request.Input))
            return Task.FromResult(
                OperationResult<LogicalUnitResult<ExpressionCompletionResult>>
                    .Fail("Input cannot be empty.", "EMPTY_INPUT"));

        var key = Normalise(request.Input);

        ExpressionCompletionResult output;
        if (_lookup.TryGetValue(key, out var completion))
        {
            output = new ExpressionCompletionResult(
                Matched:    true,
                Completion: completion,
                Confidence: 1.0,
                Reason:     "exact_match");
        }
        else
        {
            output = new ExpressionCompletionResult(
                Matched:    false,
                Completion: null,
                Confidence: 0.0,
                Reason:     "expression_not_seen");
        }

        var result = new LogicalUnitResult<ExpressionCompletionResult>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            output,
            output.Confidence,
            [],          // no predicate ids
            [DomainTag],
            [],          // no evidence ids
            []);         // no warnings — Matched=false is not a warning,
                         // it is a clean contract signal

        return Task.FromResult(
            OperationResult<LogicalUnitResult<ExpressionCompletionResult>>
                .Ok(result));
    }
}
```

### Gate — Phase 3

New tests in `Tests/ExpressionCompletionTests.cs`:

- `Train` with valid corpus succeeds
- `Train` with empty corpus returns failure
- Known partial (exact case) returns `Matched=true`, correct completion,
  `Confidence=1.0`, `Reason="exact_match"`
- Known partial with different whitespace returns `Matched=true`
  (normalisation works)
- Known partial in different case returns `Matched=true`
  (case-insensitive normalisation)
- Unseen partial returns `Matched=false`, `Completion=null`,
  `Confidence=0.0`, `Reason="expression_not_seen"`
- Empty input returns operation failure, not `Matched=false`
- `Normalise` collapses multiple spaces to one
- `Normalise` lowercases correctly
- `ExpressionCompletionUnit` implements
  `ILogicalUnit<string, ExpressionCompletionResult>`

```bash
dotnet build 21.AgentInfrastructure/ 2>&1 | grep "error" | head -5
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
```
Expected: 10 new tests passing, floor 1255 → 1265.

---

## Phase 4 — Add project 23 reference to test project

**File:** `Tests/LlmFromScratch.Tests.csproj`

Add:
```xml
<ProjectReference
  Include="..\23.SemanticCompilerAdapter\SemanticCompilerAdapter.csproj" />
```

```bash
dotnet build Tests/ 2>&1 | grep "error" | head -5
```
Expected: zero errors.

---

## Phase 5 — Held-out validation experiment

Run the same 20/10 split from v1 but against the new unit. The expected
result is now deterministic:

Training set (20 partials): all should return `Matched=true` with the
correct completion.

Held-out set (10 partials): all should return `Matched=false`.

```
Expected output:
  Training matched:  20/20 (100%)
  Training correct:  20/20 (100%)
  Held-out matched:   0/10 (0%)   ← correct: unit abstains on all
  Held-out abstained: 10/10 (100%)
  False positives:    0/10 (0%)
```

If any held-out partial returns `Matched=true`, that means the normalised
form of the unseen partial accidentally matches a training key — this would
be a normalisation collision and must be investigated before shipping.

If any training partial returns `Matched=false`, the normalisation at train
time and query time diverged — this is a bug.

### Gate — Phase 5

Both training and held-out results must match expectations exactly. No
probabilistic tolerance. The unit is either deterministic or it is broken.

---

## Phase 6 — Full suite and commit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
```

Expected: 1265/1265, 0 failures.

Commit message:
```
implement E13B ExpressionCompletionUnit v2 — exact learned-memory design

Supersedes v1 (KNN-based approach). No nearest-neighbour guessing.

Design:
  ExpressionCompletionResult(Matched, Completion, Confidence, Reason)
  ILogicalUnit<string, ExpressionCompletionResult>
  Internal state: Dictionary<string, string> keyed on normalised partial
  Normalisation: trim → collapse whitespace → lowercase

Behaviour:
  Seen partial  → Matched=true,  Completion=<stored>,  Confidence=1.0
  Unseen partial → Matched=false, Completion=null,      Confidence=0.0

Motivation: held-out validation showed KNN returned confident wrong
answers (0.74 confidence for incorrect predictions). Exact-memory mode
eliminates false positives entirely. The unit is silent when it does not
know — silence is always preferable to a confident wrong answer.

New files:
  21.AgentInfrastructure/Logical/ExpressionCompletionResult.cs
  21.AgentInfrastructure/Logical/ExpressionCompletionUnit.cs
  23.SemanticCompilerAdapter/ExpressionCorpusBuilder.cs

Tests added: 14 (4 corpus builder + 10 unit)
Floor: 1251 → 1265
```

---

## What this plan does NOT contain

`KnnLogicalUnit` is unchanged. No similarity threshold added to it.
Other units using `KnnLogicalUnit` are unaffected.

No startup registration. Same reasoning as v1: the corpus is caller-owned.

No connection to the teaching pipeline yet. The unit fires only for
expressions explicitly in its training corpus. Teaching data integration
is a separate future step once the unit is proven stable in production.

---

## Execution order and stop conditions

Phases in strict order: baseline gate → 1 → 2 → 3 → 4 → 5 → 6.

Stop immediately if:
- Any gate produces build errors
- Any phase causes a previously passing test to fail
- Phase 5 shows any false positive (held-out partial returns `Matched=true`)
- Phase 5 shows any training miss (known partial returns `Matched=false`)

---

# Plan version: 2.0
# Supersedes: v1.0
# Phases: 6
# New types: ExpressionCompletionResult, ExpressionCorpusBuilder,
#            ExpressionCompletionUnit
# New tests: 14
# New floor: 1265
# Key property: deterministic — no probability, no threshold, no guessing
