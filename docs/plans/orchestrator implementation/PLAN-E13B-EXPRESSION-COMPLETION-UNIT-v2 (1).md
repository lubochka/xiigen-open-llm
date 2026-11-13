# Plan — E13B Expression Completion Unit: Exact Learned-Memory Design
# Version: 2.1 — 2026-05-07
# Supersedes: PLAN-E13B-EXPRESSION-COMPLETION-UNIT-v1.md
# Test floor entering this plan: 1251

---

## Why v1 was wrong

v1 built on `KnnLogicalUnit<string, string>` with a similarity threshold.
The held-out validation proved that nearest-neighbour guessing is wrong for
this use case: confidence 0.74 on a completely wrong answer, `Charge`
predicted three times for unrelated expressions. A threshold does not fix
this — the correct design is no nearest-neighbour at all.

---

## Design: exact learned-memory mode

```
seen partial    → return stored completion, Matched=true,  Confidence=1.0
unseen partial  → abstain,                  Matched=false, Confidence=0.0
```

Internal state is a `Dictionary<string, string>` keyed on the normalised
partial expression. No KNN index. No BPE tokeniser. No AdamW optimizer.

---

## Project split

Project 23 already references project 12 and project 29.
Project 21 already references project 23.
Adding project 21 → project 23 would create a cycle (21→23→... and 23 does
not reference 21, but 21 already references 23 so 23 referencing 21 would be
circular).

Therefore `ILogicalUnit` cannot live in project 23. The same constraint
exists for `DomainClassifierUnit`, and the codebase already has the right
pattern for it:

```
Project 23:  DomainClassifierUnit        — plain class, no ILogicalUnit
Project 21:  PromptDomainClassifierUnit  — thin ILogicalUnit wrapper
```

`ExpressionCompletionUnit` follows this exact pattern:

```
Project 23:  ExpressionCompletionCore    — all expression-domain logic:
                                           ExpressionCompletionResult,
                                           ExpressionCorpusBuilder,
                                           dictionary, normalisation, lookup

Project 21:  ExpressionCompletionUnit   — thin ILogicalUnit<string,
                                           ExpressionCompletionResult> wrapper,
                                           delegates ExecuteAsync to Core
```

Everything that is expression knowledge — the result type, the corpus
builder, the normalisation rules, the dictionary — lives in project 23.
The only reason anything lives in project 21 is to satisfy the `ILogicalUnit`
interface constraint.

---

## Normalisation contract

Applied identically at train time and query time:
1. Trim leading and trailing whitespace.
2. Collapse all internal whitespace runs to a single space.
3. Lowercase the entire string.

`"  Revenue - Cost =  "` → `"revenue - cost ="`

The normaliser is a `static` method on `ExpressionCompletionCore`, not on
the wrapper. The wrapper never touches the string directly.

---

## New success gate

The held-out success gate changes completely from v1. This unit does not
guess — it either knows or it does not. The gate is deterministic:

```
Training partials (20 rows):
  Matched rate:      100%  — every training partial found in dictionary
  Completion correct: 100% — every matched completion is the right answer

Held-out partials (10 rows):
  Matched rate:        0%  — no unseen partial produces a match
  Abstain rate:       100% — every unseen partial returns Matched=false

False positive rate:   0%  — no held-out partial returns Matched=true
```

Any false positive means a normalisation collision: a held-out partial's
normalised form accidentally matches a training key. This must be
investigated and fixed — it is not acceptable at any tolerance.

---

## Phase 1 — ExpressionCompletionCore in project 23

**File:** `23.SemanticCompilerAdapter/ExpressionCompletionCore.cs`

Three types in one file:

```csharp
namespace SemanticCompilerAdapter;

// Output contract for ExpressionCompletionCore.Lookup.
// Matched = true:  Completion is the stored right-hand side. Confidence = 1.0.
// Matched = false: Completion is null. Confidence = 0.0.
//                  Caller treats this as no-effect — never as an error.
public sealed record ExpressionCompletionResult(
    bool Matched,
    string? Completion,
    double Confidence,
    string Reason);

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

// All expression-domain logic for exact learned-memory lookup.
// This class has no dependency on ILogicalUnit or any project 21 type.
// The project 21 wrapper ExpressionCompletionUnit delegates here entirely.
public sealed class ExpressionCompletionCore
{
    private readonly IReadOnlyDictionary<string, string> _lookup;

    // Private — use Train() factory.
    private ExpressionCompletionCore(IReadOnlyDictionary<string, string> lookup)
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
            lookup[Normalise(partial)] = completion;   // last one wins on duplicate

        return new ExpressionCompletionCore(lookup);
    }

    // Normalise: trim → collapse internal whitespace → lowercase.
    // Applied identically at train time and query time.
    public static string Normalise(string s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        var trimmed   = s.Trim();
        var collapsed = System.Text.RegularExpressions.Regex
                              .Replace(trimmed, @"\s+", " ");
        return collapsed.ToLowerInvariant();
    }

    // Exact dictionary lookup after normalisation.
    public ExpressionCompletionResult Lookup(string partial)
    {
        if (string.IsNullOrWhiteSpace(partial))
            return new ExpressionCompletionResult(
                false, null, 0.0, "empty_input");

        var key = Normalise(partial);
        return _lookup.TryGetValue(key, out var completion)
            ? new ExpressionCompletionResult(true, completion, 1.0, "exact_match")
            : new ExpressionCompletionResult(false, null, 0.0, "expression_not_seen");
    }

    // How many expressions were learned.
    public int Count => _lookup.Count;
}
```

### Gate — Phase 1

```bash
dotnet build 23.SemanticCompilerAdapter/ 2>&1 | grep "error" | head -5
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
```

New tests in `Tests/ExpressionCompletionTests.cs`:

- `ExtractPairs` splits `"Revenue - Cost = Profit"` into
  `("Revenue - Cost =", "Profit")`
- `ExtractPairs` skips rows with no `=`
- `ExtractPairs` splits on rightmost `=` when multiple `=` exist
- `ExtractPairs` skips rows where either side is empty after trim
- `Train` with valid corpus returns non-null core with correct Count
- `Train` with empty list returns null
- `Lookup` on a known partial returns `Matched=true`, correct completion,
  `Confidence=1.0`, `Reason="exact_match"`
- `Lookup` on a known partial with different whitespace returns `Matched=true`
- `Lookup` on a known partial in different case returns `Matched=true`
- `Lookup` on unseen partial returns `Matched=false`, `Completion=null`,
  `Confidence=0.0`, `Reason="expression_not_seen"`
- `Lookup` on empty string returns `Matched=false`, `Reason="empty_input"`
- `Normalise` collapses multiple spaces to one
- `Normalise` lowercases correctly
- `Normalise` trims leading and trailing whitespace

Expected: 13 new tests passing, floor 1251 → 1264.

---

## Phase 2 — ExpressionCompletionUnit wrapper in project 21

**File:** `21.AgentInfrastructure/Logical/ExpressionCompletionUnit.cs`

Intentionally thin. All logic is in `ExpressionCompletionCore`.
This file exists only to satisfy the `ILogicalUnit` interface constraint,
for the same reason `PromptDomainClassifierUnit` exists as a wrapper for
`DomainClassifierUnit`.

```csharp
namespace AgentInfrastructure.Logical;

using SemanticCompilerAdapter;

// Thin ILogicalUnit wrapper for ExpressionCompletionCore.
// All expression-domain logic lives in project 23 (ExpressionCompletionCore).
// This class exists only because ILogicalUnit is defined in project 21, which
// project 23 cannot reference without creating a circular dependency.
//
// Pattern: same as PromptDomainClassifierUnit wrapping DomainClassifierUnit.
public sealed class ExpressionCompletionUnit
    : ILogicalUnit<string, ExpressionCompletionResult>
{
    public const string Id        = "unit-expression-completion-v2";
    public const string Role      = "expression_completion";
    public const string DomainTag = "expression_corpus";

    private readonly ExpressionCompletionCore _core;

    // Private — use Train() factory.
    private ExpressionCompletionUnit(ExpressionCompletionCore core)
    {
        _core = core;
    }

    // Factory: build a trained unit from a list of complete expressions.
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

    // Delegate entirely to ExpressionCompletionCore.Lookup.
    // No logic here — this is a pass-through wrapper.
    public Task<OperationResult<LogicalUnitResult<ExpressionCompletionResult>>>
        ExecuteAsync(
            LogicalUnitRequest<string> request,
            CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(
                OperationResult<LogicalUnitResult<ExpressionCompletionResult>>
                    .Fail("Cancelled.", "CANCELLED"));

        var output = _core.Lookup(request.Input ?? string.Empty);

        var result = new LogicalUnitResult<ExpressionCompletionResult>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            output,
            output.Confidence,
            [],
            [DomainTag],
            [],
            []);         // no warnings — Matched=false is a clean signal,
                         // not a degraded path

        return Task.FromResult(
            OperationResult<LogicalUnitResult<ExpressionCompletionResult>>
                .Ok(result));
    }
}
```

### Gate — Phase 2

New tests in `Tests/ExpressionCompletionTests.cs`:

- `Train` via wrapper with valid corpus succeeds
- `Train` via wrapper with empty corpus returns failure
- `ExecuteAsync` on known partial returns `Matched=true`, correct completion
- `ExecuteAsync` on unseen partial returns `Matched=false`
- `ExpressionCompletionUnit` implements
  `ILogicalUnit<string, ExpressionCompletionResult>`

```bash
dotnet build 21.AgentInfrastructure/ 2>&1 | grep "error" | head -5
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
```

Expected: 5 new tests passing, floor 1264 → 1269.

---

## Phase 3 — Add project 23 reference to test project

**File:** `Tests/LlmFromScratch.Tests.csproj`

Add:
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

Run the same 20/10 split from v1 against the new unit.

Training set (20 partials known to the unit):
```
Revenue - Cost =         → Profit
Principal * Interest =   → Charge
Assets - Liabilities =   → Equity
...
```

Held-out set (10 partials the unit has never seen):
```
Signal + Threshold =     → Alert
Vector + Predicate =     → Score
Score + Margin =         → Rank
...
```

Expected output:
```
Training matched:   20/20 (100%)
Training correct:   20/20 (100%)
Held-out matched:    0/10 (0%)   ← correct: unit abstains on all
Held-out abstained: 10/10 (100%)
False positives:     0/10 (0%)
```

### Gate — Phase 4

Binary. No tolerance.

If any held-out partial returns `Matched=true`: normalisation collision.
Stop, investigate, fix before proceeding.

If any training partial returns `Matched=false`: normalisation divergence
between train time and query time. Stop, investigate, fix.

---

## Phase 5 — Full suite and commit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -3
```

Expected: 1269/1269, 0 failures.

Commit message:
```
implement E13B ExpressionCompletionUnit v2 — exact learned-memory design

Supersedes v1 (KNN-based). No nearest-neighbour guessing.

Project split follows the DomainClassifierUnit / PromptDomainClassifierUnit
pattern. All expression-domain logic lives in project 23:

  ExpressionCompletionResult  — output record (Matched, Completion,
                                 Confidence, Reason)
  ExpressionCorpusBuilder     — splits "LHS = RHS" into pair list
  ExpressionCompletionCore    — dictionary, normalisation, Lookup

Project 21 contains only the thin ILogicalUnit wrapper:

  ExpressionCompletionUnit    — ILogicalUnit<string,
                                 ExpressionCompletionResult>
                               — delegates ExecuteAsync to Core entirely

Normalisation: trim → collapse whitespace → lowercase.
Applied identically at train time and query time.

Behaviour:
  Seen partial   → Matched=true,  Completion=<stored>,  Confidence=1.0
  Unseen partial → Matched=false, Completion=null,       Confidence=0.0

Held-out validation result:
  Training:  20/20 matched correctly
  Held-out:  0/10 matched (100% abstained, 0 false positives)

Motivation: held-out test in v1 showed KNN returned 0.74 confidence for
wrong answers. Exact-memory eliminates false positives entirely.
Silence is preferable to a confident wrong answer.

New files:
  23.SemanticCompilerAdapter/ExpressionCompletionCore.cs
  21.AgentInfrastructure/Logical/ExpressionCompletionUnit.cs

Tests added: 18
Floor: 1251 → 1269
```

---

## What this plan does not contain

`KnnLogicalUnit` is unchanged.
No startup registration — corpus is caller-owned.
No teaching pipeline integration — separate future step.

---

## Execution order and stop conditions

Phases in strict order: 1 → 2 → 3 → 4 → 5.

Stop immediately if:
- Any gate produces build errors
- Any phase causes a previously passing test to fail
- Phase 4 shows any false positive
- Phase 4 shows any training miss

---

# Plan version: 2.1
# Supersedes: v1.0, v2.0
# Phases: 5
# New files: ExpressionCompletionCore.cs (project 23),
#            ExpressionCompletionUnit.cs (project 21)
# New types in project 23: ExpressionCompletionResult,
#                          ExpressionCorpusBuilder,
#                          ExpressionCompletionCore
# New types in project 21: ExpressionCompletionUnit (wrapper only)
# New tests: 18
# New floor: 1269
# Key property: deterministic — no probability, no threshold, no guessing
