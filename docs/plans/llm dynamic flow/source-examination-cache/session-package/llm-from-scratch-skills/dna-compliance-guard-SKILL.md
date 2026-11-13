# DNA Compliance Guard — LlmFromScratch
## For: Every C# file before commit

---

## When to Invoke

- Before committing any new or modified `.cs` file
- After any algorithm implementation or refactor
- When `bug-to-tests` identifies the root cause in a DNA violation
- During phase-end gate (mandatory pre-commit check)

---

## The 10 DNA Rules (from CLAUDE.md)

| Rule | Principle | What to check |
|------|-----------|--------------|
| Rule 1 | `OperationResult<T>` on every public method | No public method returns `void`, raw `T`, `bool`, or `null` |
| Rule 2 | Fabric First — interfaces only in production code | No `new ConcreteStorage()` in non-test code |
| Rule 3 | Every line has a plain-language comment | No uncommented production lines |
| Rule 4 | Zero failures is a hard gate | Checked by dev-safety gate, not this skill |
| Rule 5 | Read before touching | Process rule — not detectable in code |
| Rule 6 | One commit per phase | Process rule — not detectable in code |
| Rule 7 | Test-before-after per placement | Plan rule — each placement has a metric |
| Rule 8 | Gate B per placement | Plan rule — pass condition is a comparison or count |
| Rule 9 | No abbreviations without expansion | First use of any abbreviation includes full form |
| Rule 10 | No external ML frameworks | No `TensorFlow`, `Microsoft.ML`, etc. |

---

## Pre-Commit Gate (run on every modified file)

```bash
FILE="path/to/modified/file.cs"

# Rule 1: Every public method returns OperationResult<T>
grep -n "public.*\(.*\)" $FILE | grep -v "OperationResult\|void Main\|static void\|class \|interface \|// "
# Any hit where return type is not OperationResult<T> = violation

# Rule 2: No direct instantiation of storage/logger/provider in non-test code
grep -n "new.*Storage\|new.*Logger\|new.*Provider\|new.*Client" $FILE | grep -v "//\|Null\|InMemory\|Test\|Fake"
# Any hit in production code = violation (ok in tests with Null* stubs)

# Rule 3: Uncommented lines
grep -n "^\s*[a-zA-Z]" $FILE | grep -v "^\s*//"
# Review each hit — every production line needs a comment above or inline

# Rule 10: No ML framework imports
grep -n "using TensorFlow\|using Microsoft\.ML\|using Torch" $FILE
# Any hit = violation
```

Expected: all sections empty for a clean file.

---

## Quick Verdicts

| Observation | DNA Rule | Verdict |
|-------------|----------|---------|
| `public void Process(string input)` | Rule 1 | VIOLATION — return `OperationResult<bool>` |
| `public string Tokenize(string input)` | Rule 1 | VIOLATION — return `OperationResult<string[]>` |
| `public bool IsValid(string x)` | Rule 1 | VIOLATION — return `OperationResult<bool>` with reason |
| `_storage = new SqliteStorage(path)` in service constructor | Rule 2 | VIOLATION — inject `ISessionStorage` |
| `var reader = new FileSystemCodeReader()` in algorithm | Rule 2 | VIOLATION — inject `ICodeReader` |
| Lines without comments in production code | Rule 3 | VIOLATION — add plain-language comment |
| `using TensorFlow;` | Rule 10 | VIOLATION — implement from scratch |
| `public OperationResult<T> Process(...)` — clean | — | PASS |
| `public BaseService(ISessionStorage storage)` — injection | — | PASS |

---

## Rule 1 in Depth: OperationResult Pattern

Every public method returns the result envelope:

```csharp
// ✅ CORRECT — explicit success/failure, caller must check
public OperationResult<CompilationResult> Compile(PipelineInput input)
{
    if (string.IsNullOrEmpty(input.Text))
        return OperationResult<CompilationResult>.Fail("Input text is empty", "EMPTY_INPUT");

    var result = RunPipeline(input);
    return OperationResult<CompilationResult>.Ok(result);
}

// ❌ VIOLATION — can return null, caller has no error information
public CompilationResult? Compile(PipelineInput input) { ... }

// ❌ VIOLATION — throws for business logic, not for programmer errors
public CompilationResult Compile(PipelineInput input)
{
    if (string.IsNullOrEmpty(input.Text))
        throw new ArgumentException("empty");
    ...
}

// ❌ VIOLATION — caller cannot distinguish success from failure
public bool TryCompile(PipelineInput input, out CompilationResult result) { ... }
```

The only exceptions:
- `static void Main()` entry points
- Test helper methods (not public in production code)
- Override of framework methods (`ToString()`, `Equals()`)

---

## Rule 2 in Depth: Fabric-First

Dependencies are injected through interfaces, never instantiated directly:

```csharp
// ✅ CORRECT — constructor injection through interface
public class SemanticNlpAnnotator : BaseService
{
    // ISemanticStorage injected — concrete type chosen at construction site
    public SemanticNlpAnnotator(ISemanticStorage storage, ISemanticEventLogger logger)
        : base(storage, logger) { }
}

// ✅ CORRECT in tests — Null* objects are explicit stubs
var annotator = new SemanticNlpAnnotator(new NullSemanticStorage(), new NullSemanticEventLogger());

// ❌ VIOLATION in production code — concrete type hardwired
public class SemanticNlpAnnotator
{
    private readonly SqliteStorage _storage = new SqliteStorage("data.db"); // VIOLATION
}
```

LlmFromScratch fabric interfaces (from actual codebase):
- `ISessionStorage` — session state (21.AgentInfrastructure)
- `ICodeReader` / `ICodeWriter` — file access
- `ILlmClient` — LLM API calls
- `ISemanticStorage` / `ISemanticEventLogger` — semantic NLP (28.SemanticNlp)
- `ICompressionStorage` — compression (24.Compression)
- Per-subproject: `ICompilerStorage`, `ICompilerEventLogger`, etc.

---

## When a Violation Is Found

1. **Do NOT commit the violating file**
2. Classify the violation: which DNA rule, which file, which line
3. Fix the production code (not the test)
4. Write 3 failing tests before fixing if the violation caused a bug
   (see bug-to-tests-SKILL.md)
5. Re-run pre-commit gate → must be clean before committing

---

## Integration

```
dna-compliance-guard
  → pre-commit gate → blocks violating commits
  → works with bug-to-tests: DNA violation = CLASS C bug
  → feeds retroactive-development: violation in existing code
    requires checking all sibling classes in same namespace
```
