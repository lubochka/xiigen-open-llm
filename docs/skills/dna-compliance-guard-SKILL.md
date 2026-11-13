# dna-compliance-guard-SKILL.md — 10-Rule compliance check for every .cs file

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.
## v1.0: translated from xiigen DNA compliance guard (NestJS→C#)

## When to load
Load before committing any new or modified .cs file in a subproject (not in Tests/).
Load during EXECUTOR sessions when a new service class is created.
Load after any refactor that touches a public method signature.

---

## The 10 DNA Guard Patterns

| Rule | Check | Detection command |
|------|-------|------------------|
| Rule 1 | Every public method returns OperationResult<T> | see below |
| Rule 2 | Every external dependency injected through interface | see below |
| Rule 3 | Every production code line has a plain-English comment | see below |
| Rule 4 | dotnet test green before commit | `dotnet test ... 2>&1 \| tail -3` |
| Rule 5 | File read completely before edit | recorded in session as proof |
| Rule 6 | One commit per phase, correct message format | `git log --oneline -1` |
| Rule 7 | Metric captured before AND after change | in test + Round Contract |
| Rule 8 | Gate B is a bash assertion (not English prose) | read the plan phase |
| Rule 9 | Every abbreviation expanded on first use | grep for undefined acronyms |
| Rule 10 | No external ML frameworks | `grep -rn "using ML\.\|using TorchSharp\|using Microsoft.ML"` |
| BaseService | Every new service inherits BaseService(ISessionStorage) | see below |
| OperationResult | SC29 namespace only inside 29.SemanticCompiler | see below |

---

## Pre-commit gate — run on every modified file

```bash
FILE="<path-to-modified-file>"

# Rule 1: public methods must return OperationResult<T>
grep "public.*void " $FILE | grep -v "//\|override void Dispose"
# Expected: no output (void returns only allowed for Dispose)

grep "public.*Task " $FILE | grep -v "OperationResult\|//\|override"
# Expected: no output (all async methods must wrap in OperationResult)

# Rule 2: no concrete instantiation in constructors
grep "new [A-Z][a-z]" $FILE | grep -v "new OperationResult\|new List\|new Dictionary\|new string\|new [A-Z].*Config\|new [A-Z].*Options"
# Any service instantiation (new XyzService(), new ElasticSearchClient()) is a violation

# Rule 3: lines without comments
grep -n "^ \+[^/]" $FILE | grep -v "^\s*{$\|^\s*}$\|^\s*$"
# Review: every logic line should have a // comment explaining what it does

# Rule 10: no external ML frameworks
grep -rn "using ML\.\|using TorchSharp\|using Microsoft\.ML\|using PyTorch" $FILE
# Expected: no output

# BaseService: every service class must inherit BaseService
grep "class.*Service" $FILE | grep -v "BaseService\|:.*Base"
# Review: any hit that is not inheriting BaseService needs investigation

# OperationResult<T> namespace:
grep "SemanticCompiler29.OperationResult" $FILE
# Expected: only appears inside 29.SemanticCompiler/ directory
# If found outside 29.SemanticCompiler/ → violation — use AgentInfrastructure.OperationResult<T>
```

Expected for a clean file: all sections return no violations.

---

## Quick verdicts

| Observation | Rule | Action |
|-------------|------|--------|
| `public void ProcessExample(...)` | Rule 1 | Change to `public OperationResult<bool>` |
| `public Task<string> TrainAsync(...)` | Rule 1 | Change to `public Task<OperationResult<string>>` |
| `_reader = new FileSystemReader()` in constructor | Rule 2 | Inject `ICodeReader reader` via constructor |
| Line with no `//` comment | Rule 3 | Add a plain-English comment explaining the line |
| `using Microsoft.ML;` | Rule 10 | Remove — implement the algorithm in pure C# |
| `class ProjectIndexer` (no `: BaseService`) | BaseService | Add `: BaseService` and `(ISessionStorage storage) : base(storage)` |
| `SemanticCompiler29.OperationResult<T>` in a ContinuousLearning file | OperationResult | Use `AgentInfrastructure.OperationResult<T>`; add `using SC29 = SemanticCompiler29;` alias |

---

## When a violation is found

1. Do NOT commit the violating file
2. Fix the violation in the production code (not in the test)
3. Re-run `dotnet build` — must be 0 errors
4. Re-run `dotnet test` — must be 0 failures
5. Re-run the pre-commit gate — must show no violations
6. Then commit

---

## Rule 2 — Fabric First expanded

The most common Rule 2 violation is calling `new ConcreteService()` instead of accepting an interface:

```csharp
// VIOLATION: concrete instantiation
public ProjectIndexer(ISessionStorage storage)
{
    _compiler = new SemanticCompilerPipeline();  // ← VIOLATION
    _reader   = new FileSystemReader();           // ← VIOLATION
}

// CORRECT: inject through interfaces
public ProjectIndexer(ISessionStorage storage, SC29.ISemanticCompiler29 compiler, ICodeReader reader)
    : base(storage)
{
    _compiler = compiler;  // injected
    _reader   = reader;    // injected
}
```

Interfaces to use for common dependencies:

| Concrete type (never use directly) | Interface to inject |
|-----------------------------------|--------------------|
| SemanticCompilerPipeline | SC29.ISemanticCompiler29 |
| FileSystemReader | ICodeReader |
| FileSystemWriter | ICodeWriter |
| SessionStateStore | ISessionStorage |
| LoraApplicator | reference via IncrementalTrainer delegation (not direct inject) |
| DpoTrainer | reference via IncrementalTrainer delegation (not direct inject) |

---

## Rule 1 — OperationResult<T> return pattern

```csharp
// VIOLATION: raw return types
public string GetDomain(string input) { ... }
public bool IsValid(TrainingExample ex) { ... }
public List<ModelAdapter> ListAll() { ... }

// CORRECT: always wrapped
public OperationResult<string>                    GetDomain(string input) { ... }
public OperationResult<bool>                      IsValid(TrainingExample ex) { ... }
public OperationResult<IReadOnlyList<ModelAdapter>> ListAll() { ... }

// CORRECT error path:
return OperationResult<string>.Fail("Domain not found", "DOMAIN_NOT_FOUND");

// VIOLATION: throwing for business logic
throw new InvalidOperationException("Domain not found");  // ← VIOLATION Rule 1
```

---

## BaseService pattern

```csharp
// CORRECT: single-parameter constructor, storage first
public sealed class ProjectIndexer : BaseService, IProjectIndexer
{
    public ProjectIndexer(ISessionStorage storage, SC29.ISemanticCompiler29 compiler, ICodeReader reader)
        : base(storage)
    {
        _compiler = compiler;
        _reader   = reader;
    }
}

// VIOLATION: missing BaseService
public sealed class ProjectIndexer : IProjectIndexer { ... }

// VIOLATION: missing interface
public sealed class ProjectIndexer : BaseService { ... }

// VIOLATION: storage not first parameter
public sealed class ProjectIndexer : BaseService, IProjectIndexer
{
    public ProjectIndexer(SC29.ISemanticCompiler29 compiler, ISessionStorage storage)  // ← VIOLATION
        : base(storage)
```

---

## Generated output audit

SemanticCompilerPipeline already produces generated output today. Any generated artifact
must pass this audit before commit, regardless of which subproject produces it.

### Current generators

| Generator | Output type | Location |
|-----------|------------|----------|
| SemanticCompilerPipeline | `PipelineOutput.Predicates[]` | 29.SemanticCompiler/ |
| TemplateBeautifier | Formatted template strings | 29.SemanticCompiler/Beautification/ |
| 17.DynamicDomains (future) | C# type definition strings | 17.DynamicDomains/ (planned) |

### Audit steps for any generated file

```bash
GENERATED_FILE="<path-to-generated-output>"

# 1. Does the generated output parse without errors?
#    For C# output: attempt a Roslyn parse (or dotnet build the consuming project)
dotnet build <consuming-project>.csproj 2>&1 | grep -E "error|Error"
# Expected: 0 errors

# 2. Does the generator return OperationResult<T> on all paths?
grep "public.*void.*Generate\|public.*Task.*Generate" <generator-file> | grep -v "OperationResult"
# Expected: no output

# 3. Does the generated output contain any hardcoded absolute paths?
grep -n "C:\\\\\|/home/\|/Users/" "$GENERATED_FILE"
# Expected: no output

# 4. Does the generated output reference any non-existent type?
#    Verify by building the project that consumes the generated output
dotnet build LlmFromScratch.sln 2>&1 | grep "CS0246\|CS0234\|error"
# Expected: 0 errors
```

### Quick verdicts

| Observation | Action |
|-------------|--------|
| Generator method returns `void` or `string` directly | Change return type to `OperationResult<string>` (Rule 1) |
| Generated C# file causes CS0246 (type not found) | Add the missing `using` directive in the generator's template |
| Generated file contains absolute path | Replace with a relative path or a configurable root parameter |
| Generated file builds clean but logic assertion fails | Add a [Fact] that verifies the generator output for a known input |

### Gate B format for generation phases

```
Gate B: dotnet build LlmFromScratch.sln exits 0 after generation;
        <GeneratorClass>.Generate(<sample input>) returns IsSuccess=true;
        generated output contains expected token/predicate count (was 0)
```
```
