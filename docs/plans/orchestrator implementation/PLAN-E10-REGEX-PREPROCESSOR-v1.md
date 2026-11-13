# Plan — E10 RegexPreprocessor: Experiment to Production
# Version: 1.0 — 2026-05-06
# Session type: ARCHITECT
# Test floor entering this plan: 1224

---

## What E10 is

`RegexPreprocessor.Normalize` strips noise from C# source text before any
other processing touches it. It runs five regex passes in order:

```
1. XmlDocs        ///[^\n]*             → remove XML documentation comments
2. BlockComments  /\*.*?\*/             → remove block comments
3. LineComments   //[^\n]*              → remove line comments
4. StringLiterals @"..." and "..."      → replace with "" (empty pair)
5. ExtraBlankLines \n(\s*\n){2,}        → collapse to single blank line
```

Tier 1 result: **+29.5% Roslyn parse time improvement**, 21% average char
reduction, zero syntax errors introduced across all test files.

---

## Why it does not apply to natural language prompts

`SemanticNormalizer` (E15) handles natural language. It splits on non-alpha
characters, removes stopwords, and stems. It never calls Roslyn.

`RegexPreprocessor` targets C# source — files that will be passed to
`CSharpSyntaxTree.ParseText`. It must NOT run on natural language prompts
because those contain no XML docs, no block comments, and no string literals;
the regex passes would be harmless but wasteful, and the `///` and `/*`
stripping could corrupt edge-case prompt text.

The guard is already in place: `StackDetector.Detect` (E20) classifies the
source as `CSharp`, `TypeScript`, `Python`, or `Generic` before `NormalizeSource`
is called. E10 runs only when the stack is `CSharp`.

---

## Exact insertion point

**File:** `12.SemanticCompiler/SemanticCompilerFacade.cs`

Current `Compile` method, lines 77–120:

```
Compile(source)
  → guard empty
  → StackDetector.Detect(source)          ← E20 CRITICAL — must stay first
  → NormalizeSource(source)               ← E15 SemanticNormalizer
  → CollapseToOps(normalizedWords)        ← FamilyCollapser
  → LogicExtractor.Extract(source)        ← predicate extraction
  → Decomposer.Split(source)              ← E21 segment detection
  → RouteClassifier.ClassifyByKeywords    ← domain label
  → return CompilationResult
```

After E10 is wired:

```
Compile(source)
  → guard empty
  → StackDetector.Detect(source)          ← E20 CRITICAL — unchanged
  → if stack == CSharp:                   ← NEW: E10 guard
      source = RegexPreprocessor.Normalize(source)   ← E10 +29.5%
  → NormalizeSource(source)               ← E15 receives cleaned source
  → CollapseToOps(normalizedWords)
  → LogicExtractor.Extract(source)        ← uses cleaned source
  → Decomposer.Split(source)             ← uses cleaned source
  → RouteClassifier.ClassifyByKeywords
  → return CompilationResult
```

The `source` variable is reassigned in place. All downstream calls in
`Compile` already use `source` — they automatically receive the cleaned
version with no further changes needed.

---

## Baseline gate

```bash
cd C:\Projects\llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Record the count. Every gate must match or exceed it.

---

## Phase 1 — Move RegexPreprocessor to production

### File to create: `12.SemanticCompiler/Compression/RegexPreprocessor.cs`

Copy verbatim from `35.ExperimentsV2/Infrastructure/RegexPreprocessor.cs`,
change only the namespace:

```csharp
using System.Text.RegularExpressions;

namespace SemanticCompiler;

// RegexPreprocessor reduces C# source text before Roslyn parsing.
// E10: +29.5% parse time improvement, ~21% char reduction, zero syntax errors.
// Apply ONLY to C# source files — not to natural language prompts.
// Call before NormalizeSource inside SemanticCompilerFacade.Compile.
public static class RegexPreprocessor
{
    // Application order matters: XML docs first, then block, then strings.
    private static readonly Regex XmlDocs = new(
        @"///[^\n]*", RegexOptions.Compiled);

    private static readonly Regex BlockComments = new(
        @"/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex LineComments = new(
        @"//[^\n]*", RegexOptions.Compiled);

    private static readonly Regex StringLiterals = new(
        @"@""(?:[^""]|"""")*""|""(?:[^""\\]|\\.)*""",
        RegexOptions.Compiled);

    private static readonly Regex ExtraBlankLines = new(
        @"\n(\s*\n){2,}", RegexOptions.Compiled);

    // Normalize C# source: strip comments, empty string literals, blank lines.
    // Input: raw C# source. Output: syntactically valid, reduced-size source.
    public static string Normalize(string source)
    {
        if (string.IsNullOrWhiteSpace(source)) return source;
        string s = XmlDocs.Replace(source, "");
        s = BlockComments.Replace(s, " ");
        s = LineComments.Replace(s, "");
        s = StringLiterals.Replace(s, "\"\"");
        s = ExtraBlankLines.Replace(s, "\n\n");
        return s;
    }
}
```

### Gate B — Phase 1

```bash
dotnet build 12.SemanticCompiler/ 2>&1 | grep "error" | head -10
```

Expected: zero errors. No behaviour change yet — class exists but is not called.

---

## Phase 2 — Wire into SemanticCompilerFacade.Compile

**File:** `12.SemanticCompiler/SemanticCompilerFacade.cs`

Add one field and one injection point. The full diff:

```csharp
// Add field alongside existing injected components:
private readonly RegexPreprocessor? _regexPreprocessor;

// Add to constructor (optional injection — defaults to null for non-C# use):
public SemanticCompilerFacade(
    StackDetector?      stackDetector      = null,
    RegexPreprocessor?  regexPreprocessor  = null,   // NEW
    ...)
{
    _stackDetector     = stackDetector     ?? new StackDetector();
    _regexPreprocessor = regexPreprocessor ?? new RegexPreprocessor(); // NEW
    ...
}
```

Wait — `RegexPreprocessor` is a static class with no instance. No field needed.
Call it directly. The wiring is one block added to `Compile`:

```csharp
public OperationResult<CompilationResult> Compile(string source)
{
    if (string.IsNullOrWhiteSpace(source))
        return OperationResult<CompilationResult>.Fail(
            "Source cannot be empty", "EMPTY_SOURCE");

    // E20 CRITICAL: StackDetector BEFORE SemanticNormalizer — do not reorder.
    var stackResult = _stackDetector.Detect(source);
    if (!stackResult.IsSuccess)
        return OperationResult<CompilationResult>.Fail(
            stackResult.ErrorMessage, stackResult.ErrorCode);

    // E10: strip XML docs, block/line comments, string literals, extra blanks
    // before any further processing. C# source only — guard on stack label.
    if (stackResult.Value == StackLabel.CSharp)
        source = RegexPreprocessor.Normalize(source);

    var normalizedResult = NormalizeSource(source);
    ...
```

That is the complete change. Three lines added inside the existing `Compile` method
(comment + guard + assignment). All downstream calls use `source` already.

### Gate B — Phase 2

```bash
dotnet build 12.SemanticCompiler/ 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: zero build errors, floor unchanged.
The existing `SemanticCompiler_Facade_Compile_CSharpSource_Succeeds` test must
still pass — it uses a C# snippet, which now passes through E10 first.

---

## Phase 3 — Update experiment Infrastructure copy

The experiment's `35.ExperimentsV2/Infrastructure/RegexPreprocessor.cs` is now a
duplicate of the production class. Per A51 consolidation policy, experiment
Infrastructure copies must be removed and callers redirected to production.

**Step 3A:** Check what calls `ExperimentsV2.RegexPreprocessor` in the experiments:

```bash
grep -rn "RegexPreprocessor" 35.ExperimentsV2/ --include="*.cs" | grep -v obj
```

Expected callers: `E10RegexPrep.cs` only.

**Step 3B:** Update `E10RegexPrep.cs` to import from `SemanticCompiler`:

```csharp
// Remove: using ExperimentsV2 (implicit namespace);
// The experiment now calls the production class directly.
using SemanticCompiler;
```

Replace the `RegexPreprocessor.Normalize(text)` call — it already matches the
production signature, so only the namespace resolution changes.

**Step 3C:** Delete `35.ExperimentsV2/Infrastructure/RegexPreprocessor.cs`.

**Step 3D:** Add `SemanticCompiler` project reference to `35.ExperimentsV2.csproj`
if not already present:

```bash
grep "SemanticCompiler" 35.ExperimentsV2/ExperimentsV2.csproj
```

If absent, add:
```xml
<ProjectReference Include="..\12.SemanticCompiler\SemanticCompiler.csproj" />
```

### Gate B — Phase 3

```bash
dotnet build 35.ExperimentsV2/ 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: zero errors, floor unchanged.

---

## Phase 4 — Add tests

**File:** `Tests/SemanticCompilerTests.cs`

Add to the existing Facade section (after line 328):

```csharp
// ── E10 RegexPreprocessor ────────────────────────────────────────────────

[Fact]
public void RegexPreprocessor_Normalize_RemovesXmlDocs()
{
    // Triple-slash XML doc comments must be stripped.
    const string input = "/// <summary>Does a thing</summary>\npublic void Foo() {}";
    var result = RegexPreprocessor.Normalize(input);
    Assert.DoesNotContain("///", result);
    Assert.Contains("public void Foo()", result);
}

[Fact]
public void RegexPreprocessor_Normalize_RemovesBlockComments()
{
    // Block comments must be replaced with a single space.
    const string input = "int x /* inline comment */ = 1;";
    var result = RegexPreprocessor.Normalize(input);
    Assert.DoesNotContain("/*", result);
    Assert.Contains("int x", result);
    Assert.Contains("= 1;", result);
}

[Fact]
public void RegexPreprocessor_Normalize_EmptiesStringLiterals()
{
    // String literals must be replaced with empty pair — preserves syntax.
    const string input = "var s = \"hello world\";";
    var result = RegexPreprocessor.Normalize(input);
    Assert.Contains("var s = \"\";", result);
}

[Fact]
public void RegexPreprocessor_Normalize_EmptyInput_ReturnsInput()
{
    // Guard: null/whitespace must pass through unchanged.
    Assert.Equal("", RegexPreprocessor.Normalize(""));
    Assert.Equal("   ", RegexPreprocessor.Normalize("   "));
}

[Fact]
public void SemanticCompiler_Facade_Compile_CSharpWithComments_StripsBefore_Normalise()
{
    // E10 must run before NormalizeSource on C# source.
    // A source file with XML docs must compile successfully after stripping.
    const string source = """
        /// <summary>User service</summary>
        public class UserService
        {
            // get the user by id
            public User GetById(int id) => null;
        }
        """;
    var compiler = new SemanticCompilerFacade();
    var result = compiler.Compile(source);
    Assert.True(result.IsSuccess, result.ErrorMessage);
}

[Fact]
public void SemanticCompiler_Facade_Compile_NaturalLanguage_SkipsRegexPreprocessor()
{
    // Natural language prompts (Generic stack) must not be altered by E10.
    // The prompt contains "///" which would be stripped if E10 ran on it.
    const string prompt = "explain how /// works in C# documentation";
    var compiler = new SemanticCompilerFacade();
    var result = compiler.Compile(prompt);
    // Must succeed — natural language routes as Generic, skips E10.
    Assert.True(result.IsSuccess, result.ErrorMessage);
}
```

### Gate B — Phase 4

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: floor + 6 new tests passing.

---

## Phase 5 — Full suite verification and commit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New test count:
- Phase 4: 6 tests
- Total new: 6 tests
- New floor: 1224 + 6 = 1230

Commit message:
```
wire E10 RegexPreprocessor into SemanticCompilerFacade.Compile

E10 result: +29.5% Roslyn parse time, ~21% char reduction, zero syntax errors

Changes:
  12.SemanticCompiler/Compression/RegexPreprocessor.cs — new production class
    moved from 35.ExperimentsV2/Infrastructure/RegexPreprocessor.cs
    namespace SemanticCompiler (was ExperimentsV2)

  12.SemanticCompiler/SemanticCompilerFacade.cs — Compile method
    added E10 guard: if (stack == CSharp) source = RegexPreprocessor.Normalize(source)
    position: after StackDetector.Detect (E20 guard stays first), before NormalizeSource
    natural language prompts unaffected — guard checks StackLabel.CSharp only

  35.ExperimentsV2/Infrastructure/RegexPreprocessor.cs — deleted
    E10RegexPrep.cs now imports SemanticCompiler.RegexPreprocessor directly

Tests added: 6
  RegexPreprocessor_Normalize_RemovesXmlDocs
  RegexPreprocessor_Normalize_RemovesBlockComments
  RegexPreprocessor_Normalize_EmptiesStringLiterals
  RegexPreprocessor_Normalize_EmptyInput_ReturnsInput
  SemanticCompiler_Facade_Compile_CSharpWithComments_StripsBefore_Normalise
  SemanticCompiler_Facade_Compile_NaturalLanguage_SkipsRegexPreprocessor

Floor raises from 1224 to 1230
```

---

## Execution order and stop conditions

Phases in strict order: baseline gate → 1 → 2 → 3 → 4 → 5.

Stop immediately if:
- Any Gate B step shows a test count below the current floor
- Any Gate B build step produces errors
- Phase 2 Gate B shows the CSharpSource_Succeeds test failing
  (means E10 is corrupting valid source — check regex order)
- A test is modified to suppress a genuine regression

---

## Relationship to other plans

**This plan is self-contained.** It touches only `12.SemanticCompiler` and the
experiment Infrastructure folder. It does not depend on v4 phases 1-8, the domain
classifier plan, or the context routing plan.

**Safe execution order:** This plan can run before or after v4 Phase 0. However,
because Phase 0 makes project 12 delegate to project 29 for pipeline classes, it
is cleaner to run E10 before Phase 0 so the production class lands in project 12
directly, then Phase 0 picks it up as part of the canonical 12→29 delegation pass.

**After this plan completes:** `RegexPreprocessor` is removed from the Infrastructure
folder, reducing the Infrastructure copy count. The `PLAN-DOMAIN-CLASSIFIER-SINGLE-SOURCE-v1.md`
Step 2D no longer needs to handle `RegexPreprocessor.cs` — it is already gone.

---

## What this plan does NOT change

| Component | Unchanged |
|---|---|
| E20 StackDetector position | Stays first in Compile — non-negotiable |
| SemanticNormalizer (E15) | Unchanged — receives cleaned source, produces same output |
| LogicExtractor | Unchanged — benefits from cleaner source automatically |
| Decomposer (E21) | Unchanged — benefits from cleaner source automatically |
| RouteClassifier | Unchanged |
| Natural language prompt path | Skips E10 — StackLabel.Generic guard |
| TypeScript / Python sources | Skip E10 — guard is CSharp-only |
| Any project other than 12 | Unchanged |

---

# Plan version: 1.0
# Author: ARCHITECT session 2026-05-06
# Phases: 5
# New production class: 12.SemanticCompiler/Compression/RegexPreprocessor.cs
# Deleted: 35.ExperimentsV2/Infrastructure/RegexPreprocessor.cs
# Call site: SemanticCompilerFacade.Compile — 3 lines after StackDetector.Detect
# New tests: 6
# New floor: 1230
# Experiment improvement: +29.5% Roslyn parse time (E10)
