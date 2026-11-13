# Plan — E13B Expression Completion Unit: Binary KNN Participant
# Version: 3.0 — 2026-05-07
# Supersedes: v1.0, v2.0, v2.1
# Test floor entering this plan: 1251

---

## Design summary

`ExpressionCompletionUnit` is a standard `ILogicalUnit<string, string>` that
participates in the KNN scoring matrix identically to every other learned
unit. It produces `LogicalUnitResult<string>` — the same output type as other
KNN units that return string completions.

The only thing that distinguishes it is how it produces its confidence score:

```
Seen partial   → confidence = 1.0, output = "Profit"
Unseen partial → confidence = 0.0, output = null
```

The matrix sees a score and a label, exactly as it does for every other unit.
It cannot tell this unit used a dictionary rather than cosine similarity.
DPO updates the unit's weight through the same path as all other vectors.
Checkpoints save and load through the same path.

No new result type. No new interfaces. No new matrix integration.
`ExpressionCompletionResult` from v2.x is dropped entirely.

---

## Why binary and not cosine

Starting with binary (1.0 / 0.0) keeps the weight learning signal clean.
The weight converges on evidence about this exact expression — right or wrong.
A cosine near-miss would mix the learning signal with partial matches, making
it harder for DPO to determine whether the weight should go up or down.

Binary can be relaxed later once the weight learning is stable and there is
evidence that near-miss scoring would help rather than hurt.

---

## Project split

Same constraint as v2.1 — project 23 cannot reference project 21:

```
Project 23:  ExpressionCompletionCore   — dictionary, normalisation, lookup
             ExpressionCorpusBuilder    — pair extraction from corpus strings

Project 21:  ExpressionCompletionUnit  — thin ILogicalUnit<string, string>
                                          wrapper, delegates to Core
```

`ExpressionCompletionCore.Lookup` returns `(bool Matched, string? Completion)`.
The wrapper converts this to `LogicalUnitResult<string>` with confidence 1.0
or 0.0. No intermediate result record.

---

## Normalisation contract

The normalisation applied at train time and query time must be identical.
It handles the realistic runtime cases:

1. Trim leading and trailing whitespace
2. If input contains `=`, keep only the left side including `=`
   — strips placeholders like `?`, `[unknown]`, trailing text
3. Pad operators `+`, `-`, `*`, `/` with exactly one space each side
   — handles `revenue-cost=` matching `revenue - cost =`
4. Collapse all internal whitespace runs to single space
5. Lowercase

```
"  Revenue  -  Cost =  "   → "revenue - cost ="   ✓ match
"Revenue-Cost=?"            → "revenue - cost ="   ✓ match
"REVENUE - COST = ?"        → "revenue - cost ="   ✓ match
"revenue - cost ="          → "revenue - cost ="   ✓ match
"A + B = ?"                 → "a + b ="            ✗ no match (not trained)
```

The normaliser is a `public static` method on `ExpressionCompletionCore`
so tests can verify it directly without going through `Lookup`.

---

## Phase 1 — ExpressionCompletionCore in project 23

**File:** `23.SemanticCompilerAdapter/ExpressionCompletionCore.cs`

```csharp
namespace SemanticCompilerAdapter;

// Extracts (partial, completion) pairs from complete expression strings.
// A complete expression has the form "LHS operator = RHS".
// Splitting on the rightmost = gives partial = "LHS operator ="
// and completion = "RHS". Rows with no = are skipped.
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

// Dictionary-backed exact-match lookup for expression completions.
// No dependency on ILogicalUnit or any project 21 type.
// ExpressionCompletionUnit in project 21 delegates here entirely.
public sealed class ExpressionCompletionCore
{
    private static readonly System.Text.RegularExpressions.Regex
        _whitespaceRun = new(@"\s+",
            System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex
        _operatorPad = new(@"\s*([+\-*/])\s*",
            System.Text.RegularExpressions.RegexOptions.Compiled);

    private readonly IReadOnlyDictionary<string, string> _lookup;

    private ExpressionCompletionCore(
        IReadOnlyDictionary<string, string> lookup)
    {
        _lookup = lookup;
    }

    // Build a trained core from a list of complete expressions.
    // Returns null when no valid pairs are found.
    public static ExpressionCompletionCore? Train(
        IReadOnlyList<string> completeExpressions)
    {
        var pairs = ExpressionCorpusBuilder.ExtractPairs(completeExpressions);
        if (pairs.Count == 0) return null;

        var lookup = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (partial, completion) in pairs)
            lookup[Normalise(partial)] = completion;  // last wins on duplicate

        return new ExpressionCompletionCore(lookup);
    }

    // Normalise a partial expression for consistent matching.
    // Steps: trim → strip placeholder after = → pad operators → 
    //        collapse whitespace → lowercase.
    // Applied identically at train time and query time.
    public static string Normalise(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;

        // Step 1: trim
        var result = s.Trim();

        // Step 2: if input contains =, keep only the left side including =
        // This strips placeholders: "Revenue - Cost = ?" → "Revenue - Cost ="
        var eqIdx = result.LastIndexOf('=');
        if (eqIdx >= 0)
            result = result[..(eqIdx + 1)].TrimEnd();

        // Step 3: pad operators with single space each side
        result = _operatorPad.Replace(result, " $1 ");

        // Step 4: collapse whitespace runs
        result = _whitespaceRun.Replace(result, " ").Trim();

        // Step 5: lowercase
        return result.ToLowerInvariant();
    }

    // Exact dictionary lookup after normalisation.
    // Matched=true:  confidence will be 1.0 in the KNN matrix.
    // Matched=false: confidence will be 0.0 — unit does not affect scoring.
    public (bool Matched, string? Completion) Lookup(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (false, null);

        var key = Normalise(input);
        return _lookup.TryGetValue(key, out var completion)
            ? (true, completion)
            : (false, null);
    }

    // How many expressions were learned.
    public int Count => _lookup.Count;
}
```

### Gate — Phase 1

New tests in `Tests/ExpressionCompletionTests.cs`:

**ExpressionCorpusBuilder:**
- Splits `"Revenue - Cost = Profit"` into `("Revenue - Cost =", "Profit")`
- Skips rows with no `=`
- Splits on rightmost `=` when multiple `=` exist
- Skips rows where either side is empty after trim

**ExpressionCompletionCore — Train:**
- Valid corpus returns non-null core with correct `Count`
- Empty list returns null

**ExpressionCompletionCore — Normalise:**
- Strips placeholder: `"Revenue - Cost = ?"` → `"revenue - cost ="`
- Strips placeholder no spaces: `"Revenue-Cost=?"` → `"revenue - cost ="`
- Pads operators: `"revenue-cost="` → `"revenue - cost ="`
- Collapses whitespace: `"Revenue  -  Cost ="` → `"revenue - cost ="`
- Lowercases: `"REVENUE - COST ="` → `"revenue - cost ="`
- Trims: `"  Revenue - Cost =  "` → `"revenue - cost ="`
- Empty string returns empty string

**ExpressionCompletionCore — Lookup:**
- Known partial (exact) → `Matched=true`, correct completion
- Known partial with placeholder → `Matched=true` (normalisation strips it)
- Known partial no spaces around operators → `Matched=true`
- Known partial different case → `Matched=true`
- Unseen partial → `Matched=false`, `Completion=null`
- Empty string → `Matched=false`

```bash
dotnet build 23.SemanticCompilerAdapter/ 2>&1 | grep "error" | head -5
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
```

Expected: 17 new tests passing, floor 1251 → 1268.

---

## Phase 2 — ExpressionCompletionUnit wrapper in project 21

**File:** `21.AgentInfrastructure/Logical/ExpressionCompletionUnit.cs`

Returns `LogicalUnitResult<string>` — the same type as other KNN string
units. Confidence 1.0 on match, 0.0 on no-match. The matrix treats this
identically to any other unit that produces those scores.

```csharp
namespace AgentInfrastructure.Logical;

using SemanticCompilerAdapter;

// ILogicalUnit<string, string> wrapper for ExpressionCompletionCore.
//
// Participates in the KNN scoring matrix as a standard learned unit.
// Returns confidence=1.0 on exact match, confidence=0.0 on no-match.
// The matrix applies the unit's learned weight to the confidence score
// through the same DPO path used by all other KNN vectors.
//
// Pattern: same as PromptDomainClassifierUnit wrapping DomainClassifierUnit.
// All expression logic lives in project 23 (ExpressionCompletionCore).
// This class exists only to satisfy the ILogicalUnit interface constraint.
public sealed class ExpressionCompletionUnit
    : ILogicalUnit<string, string>
{
    public const string Id        = "unit-expression-completion-v2";
    public const string Role      = "expression_completion";
    public const string DomainTag = "expression_corpus";

    private readonly ExpressionCompletionCore _core;

    private ExpressionCompletionUnit(ExpressionCompletionCore core)
    {
        _core = core;
    }

    // Build a trained unit from a list of complete expressions.
    public static OperationResult<ExpressionCompletionUnit> Train(
        IReadOnlyList<string> completeExpressions)
    {
        var core = ExpressionCompletionCore.Train(completeExpressions);
        return core is null
            ? OperationResult<ExpressionCompletionUnit>.Fail(
                "No valid expression pairs found in corpus.",
                "NO_EXPRESSION_PAIRS")
            : OperationResult<ExpressionCompletionUnit>.Ok(
                new ExpressionCompletionUnit(core));
    }

    // Delegate lookup to ExpressionCompletionCore.
    // Confidence=1.0 on match → unit participates in matrix with learned weight.
    // Confidence=0.0 on no-match → unit contributes nothing to matrix score.
    public Task<OperationResult<LogicalUnitResult<string>>>
        ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(
                OperationResult<LogicalUnitResult<string>>
                    .Fail("Cancelled.", "CANCELLED"));

        var (matched, completion) = _core.Lookup(request.Input ?? string.Empty);

        var confidence = matched ? 1.0 : 0.0;

        var result = new LogicalUnitResult<string>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            completion!,   // null when confidence=0.0; matrix ignores it
            confidence,
            [],
            [DomainTag],
            [],
            []);

        return Task.FromResult(
            OperationResult<LogicalUnitResult<string>>.Ok(result));
    }
}
```

### Gate — Phase 2

New tests in `Tests/ExpressionCompletionTests.cs`:

- `Train` via wrapper with valid corpus succeeds
- `Train` via wrapper with empty corpus returns failure
- `ExecuteAsync` on known partial returns `Confidence=1.0`, correct output
- `ExecuteAsync` on known partial with placeholder returns `Confidence=1.0`
- `ExecuteAsync` on unseen partial returns `Confidence=0.0`, null output
- `ExpressionCompletionUnit` implements `ILogicalUnit<string, string>`

```bash
dotnet build 21.AgentInfrastructure/ 2>&1 | grep "error" | head -5
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
```

Expected: 6 new tests passing, floor 1268 → 1274.

---

## Phase 3 — Add project 23 reference to test project

**File:** `Tests/LlmFromScratch.Tests.csproj`

```xml
<ProjectReference
  Include="..\23.SemanticCompilerAdapter\SemanticCompilerAdapter.csproj" />
```

```bash
dotnet build Tests/ 2>&1 | grep "error" | head -5
```

Expected: zero errors, no test count change.

---

## Phase 4 — Held-out validation

Training set (20 partials): all should return `Confidence=1.0` with correct
completion.

Held-out set (10 partials): all should return `Confidence=0.0`.

Additionally validate the normalisation handles runtime query formats:

```
"Revenue - Cost = ?"    → Confidence=1.0  (placeholder stripped)
"Revenue-Cost=?"        → Confidence=1.0  (operators padded + placeholder stripped)
"REVENUE - COST = ?"    → Confidence=1.0  (case + placeholder)
"A + B = ?"             → Confidence=0.0  (not in corpus — correct)
```

### Gate — Phase 4

```
Training matched:   20/20 (100%)
Training correct:   20/20 (100%)
Held-out matched:    0/10 (0%)
False positives:     0/10 (0%)
Normalisation variants: all 4 pass
```

Binary. No tolerance.

---

## Phase 5 — Full suite and commit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
```

Expected: 1274/1274, 0 failures.

Commit message:
```
implement E13B ExpressionCompletionUnit v3 — binary KNN participant

Supersedes v1 (KNN cosine) and v2.x (standalone dictionary).

The unit participates in the KNN scoring matrix as a standard
ILogicalUnit<string, string>. The matrix cannot distinguish it from
any other unit that produces confidence=1.0 or confidence=0.0.
DPO updates its weight through the same path as all other vectors.

Design:
  Seen partial   → confidence=1.0, output=completion
  Unseen partial → confidence=0.0, output=null
  Weight         → owned by KNN matrix, updated by DPO

Normalisation pipeline (train time = query time):
  trim → strip placeholder after = → pad operators → 
  collapse whitespace → lowercase

Handles: "Revenue-Cost=?", "REVENUE - COST = ?", extra spaces,
         case differences. Does not handle near-misses (by design).

Project split:
  Project 23: ExpressionCorpusBuilder, ExpressionCompletionCore
              (all expression-domain logic, no ILogicalUnit dependency)
  Project 21: ExpressionCompletionUnit
              (thin wrapper, satisfies ILogicalUnit constraint only)

New files:
  23.SemanticCompilerAdapter/ExpressionCompletionCore.cs
  21.AgentInfrastructure/Logical/ExpressionCompletionUnit.cs

Tests added: 23
Floor: 1251 → 1274
```

---

## What this plan does not contain

`KnnLogicalUnit` is unchanged.
`ExpressionCompletionResult` from v2.x does not exist in this plan.
No startup registration — corpus is caller-owned.
No near-miss / partial cosine scoring — binary by design, revisit later.
No teaching pipeline integration — separate future step.

---

## Execution order and stop conditions

Phases: 1 → 2 → 3 → 4 → 5. Strict order.

Stop immediately if:
- Any gate produces build errors
- Any phase causes a previously passing test to fail
- Phase 4 shows any false positive
- Phase 4 shows any training miss
- Phase 4 normalisation variants fail

---

# Plan version: 3.0
# Supersedes: v1.0, v2.0, v2.1
# Phases: 5
# New files: ExpressionCompletionCore.cs (project 23),
#            ExpressionCompletionUnit.cs (project 21)
# Output type: LogicalUnitResult<string> — same as other KNN string units
# New tests: 23
# New floor: 1274
# Key property: binary confidence, standard matrix participation, DPO weight
