# 29.SemanticCompiler — Session Plan v1.0
## E01-E27 pipeline implementation
## Claude Code execution plan

**Note on numbering:** `12.SearchEngine` already exists in the solution.
To avoid confusion this project uses `29.SemanticCompiler`.
All master plan references to `12.SemanticCompiler` map to this folder.

**Baseline test count:** 820 (after AGENT-ALGORITHMS-ENHANCEMENT commit 8a21d60)
**Target test count:** 845 (+25)
**Estimated time:** 4-5 sessions

---

## Pre-condition gate — verify before starting

Run these checks. If any fails: stop and report before touching anything.

```bash
# 1. BaseService exists
grep -rn "public abstract class BaseService" --include="*.cs" . | grep -v obj/
# Expected: line in 21.AgentInfrastructure/Core/Core.cs

# 2. AgentAlgorithms v5.0 is done (820 tests)
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -3
# Expected: 820 passed, 0 failed

# 3. No 29.SemanticCompiler folder exists yet
ls 29.SemanticCompiler 2>/dev/null && echo "EXISTS" || echo "CLEAN"
# Expected: CLEAN

# 4. Confirmed: 12.SearchEngine exists and is separate
ls 12.SearchEngine/
# Expected: SearchEngine.csproj present
```

State before starting:
- Branch: [run `git branch --show-current`]
- Test count: [run dotnet test, report actual number]
- Pre-conditions: [ALL PASS or stop]

---

## Loading sequence

```bash
cat CLAUDE.md            # 10 rules
cat AGENTS.md            # 6 OP rules + Round Contract
cat docs/DECISIONS-LOCKED.md  # canonical domain names + 10 locked decisions
cat docs/EXPERIMENT-RESULTS-MAP.md  # E01-E27 validated results
cat docs/skills/dev-safety-SKILL.md
cat docs/skills/code-execution-SKILL.md
```

---

## Round Contract — prepend to every response

```
ROUND CONTRACT:
1. Baseline:   820 tests passing, 0 failing
2. Current:    [dotnet test — actual now]
3. Produced:   [what changed this turn]
4. Phase gate: [what "done" means for this phase]
5. Gap:        [what remains between 3 and 4]
```

---

## Critical implementation rules — read before any code

### Rule A — Pipeline order (E20, CRITICAL)
```
CORRECT:   StackDetector → RouteClassifier → SemanticNormalizer → FamilyCollapser → LogicExtractor
WRONG:     SemanticNormalizer → StackDetector  ← TypeScript "Promise<" becomes "promise<", marker lost
```

### Rule B — SUCCESS=RETURN mapping (E23, CRITICAL)
```csharp
// Without this: template_match_rate = 10.9%
// With this:    template_match_rate = 100%
["VALIDATE_OP(X) → SUCCESS"] = "return Validate({X});",
//                    ↑ SUCCESS means RETURN — not a string "SUCCESS"
```

### Rule C — Dynamic weight formula (E44, CRITICAL)
```csharp
// NOT a constant ×10 — caused catastrophic forgetting at small dataset sizes
double maxWeight = totalExamples / ((double)numClasses * rareExamples);
// 1000 examples, 5 classes, 50 security examples → maxWeight = 4.0
```

### Rule D — Canonical domain names (DECISIONS-LOCKED D-04)
```csharp
// Short form in enum and code:
public enum Domain { CODE_Q, BUG, PLAN, ARCH, REFACTOR }
// Long form in comments only:
// CODE_Q = CodeQuestion, BUG = BugFix, PLAN = Planning,
// ARCH = Architecture, REFACTOR = Refactoring
```

---

---

## Phase 1 — Project scaffold + ISemanticCompiler interface
**Tests added:** +3 (total: 823)
**Commit:** `"phase 1: scaffold 29.SemanticCompiler — Core.cs, ISemanticCompiler, SemanticCompiler.csproj"`

### Files to create

**`29.SemanticCompiler/SemanticCompiler.csproj`**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\01.Tokenizer\Tokenizer.csproj" />
    <ProjectReference Include="..\21.AgentInfrastructure\AgentInfrastructure.csproj" />
  </ItemGroup>
</Project>
```

**`29.SemanticCompiler/Core/Core.cs`**
```csharp
namespace SemanticCompiler;

// ─── Domain enum ─────────────────────────────────────────────────────────────
// Short forms in code, long forms in comments.
// CODE_Q=CodeQuestion, BUG=BugFix, PLAN=Planning, ARCH=Architecture, REFACTOR=Refactoring
// Source: DECISIONS-LOCKED D-04
public enum Domain { CODE_Q, BUG, PLAN, ARCH, REFACTOR, UNKNOWN }

// ─── Pipeline input ───────────────────────────────────────────────────────────
// Raw text entering the semantic compiler pipeline.
public sealed record PipelineInput(
    string Text,          // raw prompt or code fragment
    string? FilePath);    // optional: source file path for StackDetector

// ─── Pipeline output ──────────────────────────────────────────────────────────
// Result after all five compression stages (E01→E17).
public sealed record PipelineOutput(
    Domain   Domain,          // E18 RouteClassifier result
    string[] Predicates,      // E17 LogicExtractor: logical predicate tokens
    string   StackLanguage,   // E20 StackDetector: "csharp" | "typescript" | "python" | "unknown"
    double   CompressionRatio); // original_tokens / predicate_count
```

**`29.SemanticCompiler/Fabric/Interfaces.cs`**
```csharp
namespace SemanticCompiler;

// ISemanticCompiler — main fabric interface for the five-stage pipeline.
// 16.AutonomousLearning.DataPipeline must inject this interface, not copy the implementation.
// Rule: grep -n "ISemanticCompiler" 16.AutonomousLearning/DataPipeline.cs must return a line.
public interface ISemanticCompiler
{
    // Run the full pipeline: StackDetector → RouteClassifier → E15 → E16 → E17.
    // Returns predicates, domain, and compression ratio.
    OperationResult<PipelineOutput> Compile(PipelineInput input);
}

// IStackDetector — detects programming language from raw text.
// Must run BEFORE any normalization (E20 Critical Rule A).
public interface IStackDetector
{
    string Detect(string rawText); // "csharp" | "typescript" | "python" | "unknown"
}

// IRouteClassifier — routes input to one of five domains (E18, 95% accuracy).
public interface IRouteClassifier
{
    Domain Classify(string[] tokens);
}

// ISemanticNormalizer — stemming + stop-word removal (E15, +9.7%).
public interface ISemanticNormalizer
{
    string[] Normalize(string text);
}

// IFamilyCollapser — collapse word families to root form (E16, +8.4%).
public interface IFamilyCollapser
{
    string[] Collapse(string[] tokens);
}

// ILogicExtractor — extract logical predicates, 5% of original size (E17, +97%).
public interface ILogicExtractor
{
    string[] Extract(string[] tokens);
}
```

### Add to Tests.csproj

```xml
<ProjectReference Include="..\29.SemanticCompiler\SemanticCompiler.csproj" />
```

### Tests (3)

```csharp
// File: Tests/SemanticCompilerTests.cs

// SemanticCompiler_DomainEnum_HasFiveValues
// Assert.Equal(5, Enum.GetValues<Domain>().Where(d => d != Domain.UNKNOWN).Count());

// SemanticCompiler_PipelineInput_StoresText
// var input = new PipelineInput("add null check", null);
// Assert.Equal("add null check", input.Text);

// SemanticCompiler_PipelineOutput_CompressionRatioPositive
// var output = new PipelineOutput(Domain.CODE_Q, ["VALIDATE_OP"], "csharp", 12.5);
// Assert.True(output.CompressionRatio > 0);
```

### Gate B
```bash
dotnet build 29.SemanticCompiler/SemanticCompiler.csproj   # zero errors
dotnet test --filter "SemanticCompiler_Domain" --no-build   # 3 pass
grep "Domain.UNKNOWN" 29.SemanticCompiler/Core/Core.cs      # present
```

---

## Phase 2 — Compression pipeline (E01, E08, E15, E16, E17)
**Tests added:** +7 (total: 830)
**Commit:** `"phase 2: Compression/ — AstTokenizer, RoleAssigner, SemanticNormalizer, FamilyCollapser, LogicExtractor"`

### Files to create

**`29.SemanticCompiler/Compression/AstTokenizer.cs`** (E01, +93%)
```csharp
namespace SemanticCompiler;

// AstTokenizer: converts raw code text into AST-level tokens.
// E01: +93.1% improvement over BPE tokenization for code.
// Key insight: one AST node ≈ 5.2× more informative than one BPE token (E11).
public sealed class AstTokenizer : BaseService
{
    // Split code into syntactic units: identifiers, operators, keywords, literals.
    // Does NOT lowercase — StackDetector needs original case (E20 Rule A).
    public OperationResult<string[]> Tokenize(string code);
}
```

**`29.SemanticCompiler/Compression/RoleAssigner.cs`** (E08+E09, +56%/+45%)
```csharp
namespace SemanticCompiler;

// RoleAssigner: assigns syntactic roles to tokens — code, SQL, JSON, NL.
// E08: +56.3% accuracy with non-code syntax roles.
// E09: +45.3% with structural K-NN for role boundary detection.
public sealed class RoleAssigner : BaseService
{
    // Assign role tags: CODE / SQL / JSON / NL to each token.
    public OperationResult<(string Token, string Role)[]> AssignRoles(string[] tokens);
}
```

**`29.SemanticCompiler/Compression/SemanticNormalizer.cs`** (E15, +9.7%)
```csharp
namespace SemanticCompiler;

// SemanticNormalizer: stemming and stop-word removal.
// E15: +9.7% — reduces vocabulary noise before predicate extraction.
// Runs AFTER StackDetector (E20 Rule A): markers already captured.
public sealed class SemanticNormalizer : BaseService, ISemanticNormalizer
{
    public string[] Normalize(string text);
}
```

**`29.SemanticCompiler/Compression/FamilyCollapser.cs`** (E16, +8.4%)
```csharp
namespace SemanticCompiler;

// FamilyCollapser: collapse word families to root form.
// E16: +8.4% — "validation", "validates", "validated" → "validate"
// Runs after SemanticNormalizer: operates on normalized tokens.
public sealed class FamilyCollapser : BaseService, IFamilyCollapser
{
    public string[] Collapse(string[] tokens);
}
```

**`29.SemanticCompiler/Compression/LogicExtractor.cs`** (E17, +97%)
```csharp
namespace SemanticCompiler;

// LogicExtractor: extract logical predicates from token stream.
// E17: +97.1% — reduces input to 5% of original size while preserving meaning.
// Output: predicate strings like "VALIDATE_OP(X)", "NULL_CHECK(Y)", "ASSIGN_OP(Z)"
// vocabSize = 22 (verified: LogicExtractor returns predicates 0-21)
public sealed class LogicExtractor : BaseService, ILogicExtractor
{
    public const int VocabSize = 22; // do not change — all experiments calibrated to 22
    public string[] Extract(string[] tokens);
}
```

### Tests (7)

```csharp
// AstTokenizer_SplitsOnOperators
// AstTokenizer_PreservesCase_ForStackDetector    ← E20: must NOT lowercase
// RoleAssigner_SqlFragment_GetsSqlRole
// SemanticNormalizer_StopWords_AreRemoved
// FamilyCollapser_Validates_CollapsesToValidate
// LogicExtractor_VocabSize_IsExactly22           ← critical constant
// LogicExtractor_ExtractsNullCheck_FromNullGuard
```

### Gate B
```bash
dotnet test --filter "AstTokenizer|RoleAssigner|SemanticNormalizer|FamilyCollapser|LogicExtractor" --no-build
# 7 pass
grep "VocabSize = 22" 29.SemanticCompiler/Compression/LogicExtractor.cs
# must match — experiments break if this changes
```

---

## Phase 3 — Routing (E20, E18)
**Tests added:** +5 (total: 835)
**Commit:** `"phase 3: Routing/ — StackDetector (before normalisation), RouteClassifier (Domain enum)"`

### Files to create

**`29.SemanticCompiler/Routing/StackDetector.cs`** (E20)
```csharp
namespace SemanticCompiler;

// StackDetector: detect programming language from raw text.
// CRITICAL (E20 Rule A): must run on RAW text, before any normalisation.
// TypeScript marker "Promise<" becomes "promise<" after normalisation — undetectable.
// C# markers (using/async/await) survive normalisation — order matters for TS/PY only.
//
// C# structural markers (added after genietasks 90%→100% fix):
//   "{ get; set; }", "public class ", "= string.Empty"
//   — lean EF model classes have no using/namespace/async but do have these
public sealed class StackDetector : BaseService, IStackDetector
{
    // Minimum marker count to declare a language (avoids false positives).
    private const int MinMarkers = 2;
    public string Detect(string rawText); // "csharp" | "typescript" | "python" | "unknown"
}
```

**`29.SemanticCompiler/Routing/RouteClassifier.cs`** (E18, 95%)
```csharp
namespace SemanticCompiler;

// RouteClassifier: classify input into one of five domains.
// E18: +15.2% — 95% domain accuracy on C# corpus.
// Uses predicate profile (E17 output) as feature vector.
// Canonical domain names: CODE_Q / BUG / PLAN / ARCH / REFACTOR (DECISIONS-LOCKED D-04)
public sealed class RouteClassifier : BaseService, IRouteClassifier
{
    public Domain Classify(string[] predicateTokens);
}
```

### Tests (5)

```csharp
// StackDetector_CSharp_DetectedFromUsing
// StackDetector_TypeScript_DetectedBeforeNormalisation  ← E20 critical test
// StackDetector_LeanEfModel_DetectedFromGetSet          ← genietasks fix
// RouteClassifier_NullCheck_IsCodeQ
// RouteClassifier_FixException_IsBug
```

### Gate B
```bash
dotnet test --filter "StackDetector|RouteClassifier" --no-build
# 5 pass — especially TypeScript test

# Verify pipeline order in SemanticCompilerPipeline (created in Phase 5):
# StackDetector must appear BEFORE SemanticNormalizer in the method body
grep -n "StackDetector\|SemanticNormalizer" 29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs
# StackDetector line number < SemanticNormalizer line number
```

---

## Phase 4 — Planning (E21, E33→SOLVED, E35)
**Tests added:** +5 (total: 840)
**Commit:** `"phase 4: Planning/ — Decomposer, StopSignalDetector, DependencyGraph"`

**Note on E33:** E33-STOP-SIGNAL-DETECTOR showed +0.0% in final run — both baseline and
enhanced hit 100% binary accuracy after StratifiedSplit fixed the data distribution.
This means the stop-signal problem is already solved at the data level.
StopSignalDetector is still worth implementing (it encodes the 4 Depth-Calibration rules)
but its test just verifies logic, not metric improvement.

### Files to create

**`29.SemanticCompiler/Planning/Decomposer.cs`** (E21, +143%)
```csharp
namespace SemanticCompiler;

// Decomposer: detect task boundaries within a prompt.
// E21: +142.9% — correctly identifies when "AND" connects independent tasks vs
//      when it connects parts of a single task ("Find null AND log them" = one task).
// Rule: change in predicate profile across conjunction = true boundary.
// false-CONJ guard already implemented in 22.AgentAlgorithms.ComplexTaskDecomposer (db80dc5).
public sealed class Decomposer : BaseService
{
    public OperationResult<string[]> Decompose(string prompt);
}
```

**`29.SemanticCompiler/Planning/StopSignalDetector.cs`** (E33, SOLVED)
```csharp
namespace SemanticCompiler;

// StopSignalDetector: detect when decomposition should stop.
// E33: baseline and enhanced both hit 100% after StratifiedSplit — problem solved at data level.
// Implements the four Depth-Calibration stop signals:
//   1. Next level = sequential execution steps (not sub-tasks)
//   2. Next level requires technical context to understand
//   3. Single document, no iteration
//   4. No correction signal (no version history, no user loop)
public sealed class StopSignalDetector : BaseService
{
    public bool ShouldStop(string[] subTaskPredicates, int currentDepth);
}
```

**`29.SemanticCompiler/Planning/DependencyGraph.cs`** (E35, +30%)
```csharp
namespace SemanticCompiler;

// DependencyGraph: detect ordering and parallelism between tasks.
// E35: +30.0% — correctly identifies A→B (sequential) vs A||B (parallel).
// Default edges (from E35 + E49 findings):
//   PLAN → ARCH → CODE_Q    (plan before architecture before implementation)
//   PLAN → ARCH → REFACTOR  (plan before architecture before refactoring)
//   CODE_Q → BUG            (implementation before debugging)
// This is the same DependencyGraph used in 22.AgentAlgorithms.AgentOrchestrator.
// SemanticCompiler version operates on Domain[] not SubTask[].
public sealed class DependencyGraph : BaseService
{
    public OperationResult<Domain[][]> Sort(Domain[] domains); // returns levels
}
```

### Tests (5)

```csharp
// Decomposer_TwoDistinctTasks_SplitsCorrectly
// Decomposer_FindNullAndLog_StaysAsSingleTask     ← false-CONJ guard
// StopSignalDetector_SequentialSteps_Stops
// DependencyGraph_ArchBeforeCodeQ
// DependencyGraph_CodeQAndRefactor_SameLevel      ← parallel
```

### Gate B
```bash
dotnet test --filter "Decomposer|StopSignalDetector|DependencyGraph" --no-build
# 5 pass
```

---

## Phase 5 — Translation + Beautification + Pipeline integration
**Tests added:** +5 (total: 845)
**Commit:** `"phase 5: Translation/, Beautification/, SemanticCompilerPipeline — full E01-E27 pipeline"`

### Files to create

**`29.SemanticCompiler/Translation/PatternClassifier.cs`** (E25, +154%)
```csharp
namespace SemanticCompiler;

// PatternClassifier: classify sentence structure pattern (SVO / SOV / VSO).
// E25: +154.6% — identifies word order before translation to English SVO.
public sealed class PatternClassifier : BaseService
{
    public string Classify(string[] tokens); // "SVO" | "SOV" | "VSO" | "UNKNOWN"
}
```

**`29.SemanticCompiler/Translation/ContextualDictionary.cs`** (E26, +150%)
```csharp
namespace SemanticCompiler;

// ContextualDictionary: word → meaning given surrounding context.
// E26: +150.0% — "null" in C# vs "null" in SQL have different meanings.
public sealed class ContextualDictionary : BaseService
{
    public string Lookup(string word, string[] context);
}
```

**`29.SemanticCompiler/Translation/PatternReorderer.cs`** (E27, +250%)
```csharp
namespace SemanticCompiler;

// PatternReorderer: reorder tokens from detected pattern into English SVO.
// E27: +250.0% — converts SOV/VSO structures to Subject-Verb-Object English order.
public sealed class PatternReorderer : BaseService
{
    public string[] Reorder(string[] tokens, string fromPattern); // fromPattern: "SOV" etc.
}
```

**`29.SemanticCompiler/Beautification/TemplateBeautifier.cs`** (E23, +820%)
```csharp
namespace SemanticCompiler;

// TemplateBeautifier: map predicate sequences to code templates.
// E23: +833.3% — SUCCESS=RETURN is the critical mapping (Rule B).
// WARNING: "VALIDATE_OP(X) → SUCCESS" MUST map to "return Validate({X});"
//          NOT to a string containing "SUCCESS". This is the E23 critical fix.
//          Without it: template_match_rate = 10.9%. With it: 100%.
public sealed class TemplateBeautifier : BaseService
{
    // Canonical mappings — do not change without re-running E23:
    private static readonly Dictionary<string, string> Templates = new()
    {
        ["VALIDATE_OP(X) → SUCCESS"]    = "return Validate({X});",  // SUCCESS = return
        ["NULL_CHECK(X) → ASSIGN_OP"]   = "if ({X} == null) return OperationResult<T>.Fail(...);",
        ["LOOP_OP(X) → FILTER_OP(Y)"]   = "foreach (var item in {X}.Where({Y}))",
        ["ASSIGN_OP(X) → RETURN_OP"]    = "{X} = ...; return OperationResult<T>.Ok({X});",
    };

    public OperationResult<string> Beautify(string[] predicates);
}
```

**`29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs`** — full pipeline integration
```csharp
namespace SemanticCompiler;

// SemanticCompilerPipeline: the complete E01-E27 five-stage pipeline.
// Stage order is MANDATORY (E20 Rule A — StackDetector before normalisation):
//   1. AstTokenizer      (E01)
//   2. StackDetector     (E20) ← MUST be stage 2, before any normalisation
//   3. RouteClassifier   (E18)
//   4. SemanticNormalizer (E15)
//   5. FamilyCollapser   (E16)
//   6. LogicExtractor    (E17)
//   7. Decomposer        (E21) — optional: only when prompt has multiple tasks
//   8. TemplateBeautifier (E23) — optional: only when generating code output
public sealed class SemanticCompilerPipeline : BaseService, ISemanticCompiler
{
    public OperationResult<PipelineOutput> Compile(PipelineInput input);
}
```

### Tests (5)

```csharp
// TemplateBeautifier_ValidateOpSuccess_MapsToReturn    ← E23 critical: SUCCESS=RETURN
// TemplateBeautifier_NullCheck_MapsToGuardClause
// PatternReorderer_SovToSvo_CorrectOrder
// Pipeline_TypeScript_DetectedBeforeNormalisation      ← E20 integration test
// Pipeline_FullRun_ProducesPredicatesAndDomain         ← end-to-end
```

### Gate B
```bash
dotnet test --filter "SemanticCompiler" --no-build
# 845 total, 0 failing

# E23 critical check:
grep -A1 "VALIDATE_OP" 29.SemanticCompiler/Beautification/TemplateBeautifier.cs
# Must show: "return Validate({X});" NOT a string containing "SUCCESS"

# E20 order check:
grep -n "StackDetector\|SemanticNormalizer" 29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs
# StackDetector line number MUST be less than SemanticNormalizer line number

# ISemanticCompiler interface used in pipeline:
grep -n "ISemanticCompiler" 29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs
# Must implement the interface
```

---

## Final verification

```bash
# All tests green:
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -5
# Expected: 845 passed, 0 failed

# Three critical rules verified:
grep "VocabSize = 22" 29.SemanticCompiler/Compression/LogicExtractor.cs
grep "return Validate" 29.SemanticCompiler/Beautification/TemplateBeautifier.cs
grep -n "StackDetector" 29.SemanticCompiler/Pipeline/SemanticCompilerPipeline.cs | head -1
# StackDetector appears before SemanticNormalizer

# ISemanticCompiler interface exists for 16.AutonomousLearning to inject:
grep "ISemanticCompiler" 29.SemanticCompiler/Fabric/Interfaces.cs
```

---

## Phase summary

| Phase | What | Files | Tests | Gate |
|---|---|---|---|---|
| 1 | Scaffold + interfaces | Core.cs, Interfaces.cs, .csproj | +3 → 823 | build clean |
| 2 | Compression (E01/E08/E15/E16/E17) | 5 .cs files | +7 → 830 | VocabSize=22 |
| 3 | Routing (E20/E18) | 2 .cs files | +5 → 835 | TS detected before norm |
| 4 | Planning (E21/E33/E35) | 3 .cs files | +5 → 840 | ARCH before CODE_Q |
| 5 | Translation+Beautification+Pipeline | 5 .cs files | +5 → 845 | SUCCESS=RETURN |

**After 29.SemanticCompiler is done:** update `Tests.csproj` with the project reference,
then proceed to `13.VisualStudioExtension` (Trек A) or `14.ContinuousLearning` (Трек Б)
per `LLM-MVP-CORE-MASTER-IMPLEMENTATION-PLAN-v1.1`.
