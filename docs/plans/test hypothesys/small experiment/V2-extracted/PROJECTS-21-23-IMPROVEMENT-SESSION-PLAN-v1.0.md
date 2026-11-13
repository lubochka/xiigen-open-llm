# SESSION PLAN — Projects 21-23 Improvements
## Paste this at the start of every session that works on these improvements

---

You are executing targeted improvements to projects 21, 22, and 23 in LlmFromScratch.sln.
Before doing anything else, complete the loading sequence below in order.
Do not skip steps. Do not proceed to the next step until the current one is done.

---

## Step 1 — Establish the baseline

Run the test suite and record the starting numbers. Every phase must leave these numbers
the same or higher. Zero failures is a hard gate — not a target.

```bash
cd /home/claude/llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj
# Record: total passed, total failed, total skipped
```

If any tests are already failing before you touch anything, stop and document what is
failing in a note at the top of your first response. Do not proceed until the baseline
is clean.

## Step 2 — Read the source files for every component you will touch

Read each file completely before touching it. Record one specific fact from the
final section of each file — that is the proof you finished reading it.

**Project 21 — AgentInfrastructure:**
```bash
cat 21.AgentInfrastructure/Loop/AgentLoop.cs
cat 21.AgentInfrastructure/Indexing/CodebaseIndexer.cs
cat 21.AgentInfrastructure/Editing/FileChangeApplier.cs
```

**Project 22 — AgentAlgorithms:**
```bash
cat 22.AgentAlgorithms/Context/ContextCompactor.cs
cat 22.AgentAlgorithms/Indexing/RepoMapBuilder.cs
cat 22.AgentAlgorithms/Pipeline/AgentPipeline.cs
cat 22.AgentAlgorithms/Tasks/ComplexTaskDecomposer.cs
```

**Project 23 — OssEquivalents:**
```bash
cat 23.OssEquivalents/CodeAnalysis/RoslynCodeAnalyzer.cs
cat 23.OssEquivalents/CodeAnalysis/CallGraphBuilder.cs
```

**Test files:**
```bash
cat Tests/AgentInfrastructureTests.cs
cat Tests/AgentAlgorithmsTests.cs
cat Tests/OssEquivalentsTests.cs
```

## Step 3 — State where you are

After completing Steps 1 and 2, write one paragraph in plain English stating:
- Which branch you are on
- What the baseline test count is (passed / failed)
- Which phase you are starting now

Then proceed with Phase 1.

---

## ROUND CONTRACT — runs before every response, every turn

Before writing any response, run these five checks internally. The output must appear
at the top of the response.

```
ROUND CONTRACT:
1. Baseline was:        [N tests passing, 0 failing — from Step 1]
2. Current state:       [run dotnet test and report the actual numbers right now]
3. This round produced: [exactly what was done — files changed, tests added, commands run]
4. Phase gate says done when: [the exact completion gate for the current phase]
5. Gap check:           [compare item 3 against item 4 — what is still missing]
```

If item 5 shows any gap, the phase is not complete.
Do not write "phase complete" until item 5 shows zero gaps.

---

## GATE — runs after every phase, before moving to the next

```bash
# All three must pass before the phase is marked done.
dotnet build LlmFromScratch.sln   # zero errors
dotnet test Tests/LlmFromScratch.Tests.csproj  # zero failures, count >= baseline
git diff --name-only              # list every file changed this phase
```

Record the results in the round contract before moving on.

---

## Phase 1 — Fix the two bugs

**What was found:**

Bug A: `AgentLoop.CompactHistory` hardcodes a dummy summary message
("Earlier conversation history was compacted") instead of calling the real
`ContextCompactor.CompactAsync`. The agent loses actual context at every
compaction. `ContextCompactor` in project 22 already takes a `summaryGenerator`
function — `AgentLoop` just never uses it.

Bug B: `CodebaseIndexer.ExtractSymbols` collects invocation calls from all
descendant nodes and assigns them as the class node's `References`. A class node
ends up with references like `["GetUser", "ValidateToken", "Save"]` — method calls
made by its own methods, not the class's own relationships. This inflates
out-degree and produces incorrect PageRank scores.

### Phase 1, Step A — Fix AgentLoop.CompactHistory

**File to change:** `21.AgentInfrastructure/Loop/AgentLoop.cs`

Change `AgentLoop` to accept an optional `ContextCompactor` and a summary generator.
If neither is provided, keep the current fallback (the dummy message) so existing tests
still compile. When a compactor is provided, call `CompactAsync` instead.

```csharp
// Add optional compactor and summary generator to the constructor:
public AgentLoop(
    ILlmClient   llm,
    ToolRegistry tools,
    int          maxIterations           = 50,
    int          compactionThreshold     = 30_000,
    ContextCompactor? compactor          = null,
    Func<List<ConversationMessage>, Task<string>>? summaryGenerator = null)

// In RunAsync, replace the CompactHistory call with:
if (compactor is not null && summaryGenerator is not null)
{
    var compactResult = await compactor.CompactAsync(
        state.MessageHistory, summaryGenerator);
    if (compactResult.IsSuccess)
        state = state with { MessageHistory = compactResult.Value! };
}
else
{
    state = CompactHistory(state); // existing fallback
}
```

**Tests to add in `AgentInfrastructureTests.cs`:**

```csharp
[Fact]
public async Task AgentLoop_WithCompactor_SummarizesOldHistory()
{
    // Arrange: LLM that triggers compaction (returns many assistant turns)
    // then TASK_COMPLETE
    var manyMessages = Enumerable.Range(0, 15)
        .Select(i => new LlmResponse($"thinking step {i}", null, null))
        .Concat([new LlmResponse("TASK_COMPLETE:done!", null, null)]);
    var llm = new ScriptedLlmClient(manyMessages);

    var compactor   = new ContextCompactor(recentTurnsToPreserve: 5);
    string capturedSummary = "";
    Task<string> Generator(List<ConversationMessage> msgs)
    {
        capturedSummary = $"summarized {msgs.Count} messages";
        return Task.FromResult(capturedSummary);
    }

    var gate     = new PermissionGate(@"C:\projects");
    var registry = new ToolRegistry(gate);
    var loop     = new AgentLoop(llm, registry,
        maxIterations: 50,
        compactionThreshold: 10,   // low threshold to force compaction early
        compactor: compactor,
        summaryGenerator: Generator);

    var state  = new AgentState("Summarize this task");
    var result = await loop.RunAsync(state);

    // The summary generator was called — context was not silently dropped
    Assert.NotEmpty(capturedSummary);
}

[Fact]
public async Task AgentLoop_WithoutCompactor_StillWorksWithFallback()
{
    // Existing behaviour must not break when no compactor is provided
    var llm = new ScriptedLlmClient([
        new LlmResponse("TASK_COMPLETE:done!", null, null)
    ]);
    var gate     = new PermissionGate(@"C:\projects");
    var registry = new ToolRegistry(gate);
    var loop     = new AgentLoop(llm, registry); // no compactor
    var result   = await loop.RunAsync(new AgentState("simple task"));
    Assert.True(result.IsSuccess);
    Assert.True(result.Value!.IsComplete);
}
```

**Gate A:** both new tests pass, all existing AgentLoop tests still pass.

---

### Phase 1, Step B — Fix CodebaseIndexer class References

**File to change:** `21.AgentInfrastructure/Indexing/CodebaseIndexer.cs`

A class node's `References` should contain the names of the methods it
**declares** — not the names of methods those methods happen to call.
Change the class extraction to collect declared method names instead of
descendant invocations.

```csharp
// Replace this:
var methods = cls.DescendantNodes()
    .OfType<InvocationExpressionSyntax>()
    .Select(inv => inv.Expression.ToString().Split('.').Last())
    .Distinct()
    .ToList();

// With this:
var methods = cls.Members
    .OfType<MethodDeclarationSyntax>()
    .Select(m => m.Identifier.Text)
    .ToList();
```

**Tests to add in `AgentInfrastructureTests.cs`:**

```csharp
[Fact]
public async Task CodebaseIndexer_ClassNode_ReferencesContainsDeclaredMethods()
{
    var source = """
        namespace Demo
        {
            public class UserService
            {
                public void GetUser() { }
                public void SaveUser() { }
            }
        }
        """;
    var reader  = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/UserService.cs"] = source
    });
    var indexer = new CodebaseIndexer(reader);
    var result  = await indexer.IndexDirectoryAsync("src");
    Assert.True(result.IsSuccess);

    var classNode = result.Value!.AllNodes
        .First(n => n.Kind == "Class" && n.Name == "UserService");

    // Class references should be the methods it declares, not what those methods call
    Assert.Contains("GetUser",  classNode.References);
    Assert.Contains("SaveUser", classNode.References);
}

[Fact]
public async Task CodebaseIndexer_ClassNode_DoesNotContainNestedCallTargets()
{
    // A class that calls external methods — those should NOT appear as class References
    var source = """
        namespace Demo
        {
            public class OrderService
            {
                public void PlaceOrder()
                {
                    _repo.Save();
                    _validator.Validate();
                }
            }
        }
        """;
    var reader  = new InMemoryCodeReader(new Dictionary<string, string>
    {
        ["src/OrderService.cs"] = source
    });
    var indexer = new CodebaseIndexer(reader);
    var result  = await indexer.IndexDirectoryAsync("src");

    var classNode = result.Value!.AllNodes
        .FirstOrDefault(n => n.Kind == "Class" && n.Name == "OrderService");
    if (classNode is null) return; // empty class is valid

    // "Save" and "Validate" are calls inside PlaceOrder — not class-level references
    Assert.DoesNotContain("Save",     classNode.References);
    Assert.DoesNotContain("Validate", classNode.References);
}
```

**Gate B:** both new tests pass, all existing CodebaseIndexer tests still pass.

---

### Phase 1 completion gate

```bash
dotnet build LlmFromScratch.sln          # zero errors
dotnet test Tests/LlmFromScratch.Tests.csproj   # zero failures, count >= baseline + 4
git diff --name-only
# Must show exactly:
#   21.AgentInfrastructure/Loop/AgentLoop.cs
#   21.AgentInfrastructure/Indexing/CodebaseIndexer.cs
#   Tests/AgentInfrastructureTests.cs
git commit -m "phase 1: fix AgentLoop compaction and CodebaseIndexer class references"
git push
```

---

## Phase 2 — Rename and document (no behaviour changes)

**What was found:**

Issue A: `RepoMapBuilder.SymbolSummary.PageRankScore` is not PageRank.
It is `callers.Count / max_callers` — normalized in-degree.
`CodebaseIndexer` in project 21 implements actual power-iteration PageRank.
Calling the field `PageRankScore` in project 22 is wrong and will mislead
whoever connects the two.

Issue B: `AgentPipeline.RunAsync` comments say "four stages" but Stage 4 is
an acknowledged stub (`ChangedFiles = []` always). The comment says
"the caller applies file changes using FileChangeApplier from sub-project 21"
but nothing in the codebase actually connects these two. This is not a runtime
bug — but it is an architectural gap that will surprise the next developer.

### Phase 2, Step A — Rename PageRankScore to InDegreeScore in RepoMapBuilder

**File to change:** `22.AgentAlgorithms/Indexing/RepoMapBuilder.cs`

```csharp
// Change the record field name:
public sealed record SymbolSummary(
    string   Name,
    string   Kind,
    string   Signature,
    string   FilePath,
    double   InDegreeScore,    // was PageRankScore — this is normalized in-degree, not PageRank
    IReadOnlyList<string> CalledBy);

// Add an explanatory comment above the scoring computation:
// Compute normalized in-degree: callers / max_callers.
// This is not iterative PageRank. For full PageRank use SymbolGraph.ComputePageRank()
// from CodebaseIndexer in project 21.
double score = max == 0 ? 0.0
    : (callEdges.TryGetValue(s.Name, out var callers)
        ? (double)callers.Count / max : 0.0);
```

Update the two tests in `AgentAlgorithmsTests.cs` that reference `PageRankScore`
to use `InDegreeScore` — the assertion logic stays the same.

### Phase 2, Step B — Document AgentPipeline Stage 4 explicitly

**File to change:** `22.AgentAlgorithms/Pipeline/AgentPipeline.cs`

Replace the current comment in Stage 4 with a clear architectural note:

```csharp
// Stage 4 — change application.
//
// This pipeline returns the language model's plan as the result.
// The caller is responsible for parsing that plan and calling
// FileChangeApplier (21.AgentInfrastructure/Editing/FileChangeApplier.cs)
// to apply each search-replace block to the relevant files.
//
// This separation is intentional: the pipeline provides the reasoning;
// the infrastructure layer owns file writes and atomic rollback.
// ChangedFiles is empty here because no files are written at this layer.
return OperationResult<PipelineResult>.Ok(
    new PipelineResult(true, llmResult.Value!, []));
```

**No new tests needed for Phase 2** — these are rename and documentation changes.
Existing tests must still pass after the rename.

### Phase 2 completion gate

```bash
dotnet build LlmFromScratch.sln          # zero errors
dotnet test Tests/LlmFromScratch.Tests.csproj   # zero failures, count unchanged from Phase 1
# Verify the rename:
grep -rn "PageRankScore" 22.AgentAlgorithms/ Tests/ && echo "FAIL — old name still present" || echo "rename complete"
grep -rn "InDegreeScore" 22.AgentAlgorithms/Indexing/RepoMapBuilder.cs || echo "FAIL — new name missing"
git commit -m "phase 2: rename PageRankScore to InDegreeScore, document AgentPipeline Stage 4"
git push
```

---

## Phase 3 — Add missing tests for RoslynCodeAnalyzer and AgentPipeline

**What was found:**

`RoslynCodeAnalyzer` in project 23 has zero tests despite being the core
symbol extraction component that other code depends on.
`AgentPipeline` in project 22 has zero tests for its four-stage run path.

### Phase 3, Step A — RoslynCodeAnalyzer tests

Add to `OssEquivalentsTests.cs`:

```csharp
// ── RoslynCodeAnalyzer ────────────────────────────────────────────────────────

[Fact]
public async Task RoslynCodeAnalyzer_WithClassAndMethod_ExtractsBoth()
{
    var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    Directory.CreateDirectory(tempDir);
    try
    {
        await File.WriteAllTextAsync(Path.Combine(tempDir, "Sample.cs"), """
            namespace Demo
            {
                /// <summary>A sample service.</summary>
                public class SampleService
                {
                    /// <summary>Gets a value.</summary>
                    public string GetValue() => "hello";
                }
            }
            """);

        var analyzer = new RoslynCodeAnalyzer();
        var result   = await analyzer.AnalyzeDirectoryAsync(tempDir);

        Assert.True(result.IsSuccess);
        Assert.Contains(result.Value!, s => (string)s["name"] == "SampleService"
                                         && (string)s["kind"] == "Class");
        Assert.Contains(result.Value!, s => (string)s["name"] == "GetValue"
                                         && (string)s["kind"] == "Method");
    }
    finally { Directory.Delete(tempDir, recursive: true); }
}

[Fact]
public async Task RoslynCodeAnalyzer_XmlDocComment_ExtractedForClass()
{
    var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    Directory.CreateDirectory(tempDir);
    try
    {
        await File.WriteAllTextAsync(Path.Combine(tempDir, "Doc.cs"), """
            namespace Demo
            {
                /// <summary>Documented class.</summary>
                public class DocClass { }
            }
            """);

        var analyzer = new RoslynCodeAnalyzer();
        var result   = await analyzer.AnalyzeDirectoryAsync(tempDir);
        Assert.True(result.IsSuccess);

        var classEntry = result.Value!
            .First(s => (string)s["name"] == "DocClass");
        var doc = (string)classEntry["documentation"];
        Assert.Contains("Documented class", doc);
    }
    finally { Directory.Delete(tempDir, recursive: true); }
}

[Fact]
public async Task RoslynCodeAnalyzer_MethodCallsExtracted_PopulatesCallsList()
{
    var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    Directory.CreateDirectory(tempDir);
    try
    {
        await File.WriteAllTextAsync(Path.Combine(tempDir, "Caller.cs"), """
            namespace Demo
            {
                public class CallerService
                {
                    public void Run() { Helper(); }
                    private void Helper() { }
                }
            }
            """);

        var analyzer = new RoslynCodeAnalyzer();
        var result   = await analyzer.AnalyzeDirectoryAsync(tempDir);
        Assert.True(result.IsSuccess);

        var runMethod = result.Value!
            .First(s => (string)s["name"] == "Run" && (string)s["kind"] == "Method");
        var calls = (System.Text.Json.JsonElement)runMethod["calls"];
        Assert.Contains("Helper", calls.EnumerateArray().Select(e => e.GetString()));
    }
    finally { Directory.Delete(tempDir, recursive: true); }
}

[Fact]
public async Task RoslynCodeAnalyzer_NonexistentDirectory_ReturnsFailure()
{
    var analyzer = new RoslynCodeAnalyzer();
    var result   = await analyzer.AnalyzeDirectoryAsync("/path/that/does/not/exist");
    Assert.False(result.IsSuccess);
    Assert.Equal("DIR_NOT_FOUND", result.ErrorCode);
}

[Fact]
public async Task RoslynCodeAnalyzer_EmptyDirectory_ReturnsEmptyList()
{
    var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
    Directory.CreateDirectory(tempDir);
    try
    {
        var analyzer = new RoslynCodeAnalyzer();
        var result   = await analyzer.AnalyzeDirectoryAsync(tempDir);
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
    finally { Directory.Delete(tempDir, recursive: true); }
}
```

### Phase 3, Step B — AgentPipeline tests

Add to `AgentAlgorithmsTests.cs`:

```csharp
// ── AgentPipeline ─────────────────────────────────────────────────────────────

[Fact]
public async Task AgentPipeline_SuccessfulRun_ReturnsPlan()
{
    var symbols = new StubSymbolGraphProvider();
    var search  = new StubSemanticSearchProvider();
    var llm     = new ScriptedLlmGateway(["Add a null check before line 42."]);
    var pipeline = new AgentPipeline(symbols, search, llm);

    var result = await pipeline.RunAsync("Add null check to GetUser method");

    Assert.True(result.IsSuccess);
    Assert.True(result.Value!.Succeeded);
    Assert.Contains("null check", result.Value!.Summary);
    // Stage 4 is intentionally a stub — changed files is always empty at this layer
    Assert.Empty(result.Value!.ChangedFiles);
}

[Fact]
public async Task AgentPipeline_LlmFailure_ReturnsStage3Failed()
{
    var symbols  = new StubSymbolGraphProvider();
    var search   = new StubSemanticSearchProvider();
    // Simulate LLM returning no responses (failure)
    var llm      = new ScriptedLlmGateway([]);
    var pipeline = new AgentPipeline(symbols, search, llm);

    var result = await pipeline.RunAsync("some task");

    Assert.False(result.IsSuccess);
    Assert.Equal("STAGE3_FAILED", result.ErrorCode);
}
```

### Phase 3 completion gate

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj
# Count must be >= baseline + 4 (Phase 1) + 7 (Phase 3) = baseline + 11
grep -c "\[Fact\]" Tests/OssEquivalentsTests.cs   # must be 36 (was 31 + 5 new)
grep -c "\[Fact\]" Tests/AgentAlgorithmsTests.cs  # must be 30 (was 28 + 2 new)
git commit -m "phase 3: add RoslynCodeAnalyzer and AgentPipeline tests"
git push
```

---

## Phase 4 — FileChangeApplier and ComplexTaskDecomposer (from CODE-EDITING-TEST-PLAN-v1.0)

This phase implements the work already designed in CODE-EDITING-TEST-PLAN-v1.0.
Read that plan completely before starting this phase.

**Phase 4 has three steps:**

### Phase 4, Step A — Add fuzzy matching to FileChangeApplier

Implement `ApplyPatchFuzzyAsync` in `21.AgentInfrastructure/Editing/FileChangeApplier.cs`
following the exact specification in CODE-EDITING-TEST-PLAN-v1.0 Part 1.

Add the 5 fuzzy tests to `AgentInfrastructureTests.cs`.

Gate: all 5 fuzzy tests pass.

### Phase 4, Step B — Add multi-patch support to FileChangeApplier

Add `MultiPatchResult` record and `ApplyManyPatchesAsync` method to
`FileChangeApplier.cs` following the exact specification in Part 2 of the plan.

Add the 4 multi-patch tests to `AgentInfrastructureTests.cs`.

Gate: all 4 multi-patch tests pass, atomicity test verifies file is not written
when second patch fails.

### Phase 4, Step C — Fix false-CONJ in ComplexTaskDecomposer

Add `IsIndependentOperation` to `22.AgentAlgorithms/Tasks/ComplexTaskDecomposer.cs`
following the specification in Part 3 of the plan.

Add the 4 false-CONJ tests to `AgentAlgorithmsTests.cs`.

Gate: "Find null and log them" stays as one task, "Add null check and write tests"
splits into two tasks.

### Phase 4 completion gate

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj
# Final count: baseline + 4 (Phase 1) + 7 (Phase 3) + 13 (Phase 4) = baseline + 24
# baseline was 89 (30 + 28 + 31), so final must be >= 113
grep -c "\[Fact\]" Tests/AgentInfrastructureTests.cs  # 30 + 9 fuzzy+multi = 39
grep -c "\[Fact\]" Tests/AgentAlgorithmsTests.cs      # 28 + 2 pipeline + 4 false-CONJ = 34
git commit -m "phase 4: fuzzy patch, multi-patch, false-CONJ guard"
git push
```

---

## Final verification

After all four phases, run the complete check:

```bash
dotnet build LlmFromScratch.sln
dotnet test Tests/LlmFromScratch.Tests.csproj

# Verify all four bugs are fixed:
# Bug A: CompactHistory uses ContextCompactor
grep -n "compactor" 21.AgentInfrastructure/Loop/AgentLoop.cs | head -5

# Bug B: CodebaseIndexer class References from Members not DescendantNodes
grep -n "\.Members" 21.AgentInfrastructure/Indexing/CodebaseIndexer.cs

# Issue A: no PageRankScore remaining
grep -rn "PageRankScore" 22.AgentAlgorithms/ Tests/ && echo "FAIL" || echo "rename clean"

# Issue B: AgentPipeline Stage 4 documented
grep -n "caller is responsible" 22.AgentAlgorithms/Pipeline/AgentPipeline.cs

# New tests present
grep -c "\[Fact\]" Tests/AgentInfrastructureTests.cs   # >= 39
grep -c "\[Fact\]" Tests/AgentAlgorithmsTests.cs       # >= 34
grep -c "\[Fact\]" Tests/OssEquivalentsTests.cs        # >= 36
```

---

## Rules that apply throughout this session

**Zero failures is not a target — it is a hard gate.** If any test fails after
a change, fix it before moving to the next step. Do not report a phase complete
if any test fails.

**Read before touching.** Every file is read in full before it is changed.
There is no exception for "small" changes.

**One commit per phase.** Each phase ends with a commit and push. The commit
message names the phase and lists the files changed.

**No behaviour changes in Phase 2.** Phase 2 is rename and documentation only.
If you find yourself changing logic in Phase 2, stop and move it to Phase 1 or
raise it as a new finding.

**The false-CONJ fix in Phase 4 must not break existing decomposer tests.**
The four existing `ComplexTaskDecomposer` tests must still pass after the fix.
Run them individually if needed to confirm.

---

## Session target

Projects: 21.AgentInfrastructure, 22.AgentAlgorithms, 23.OssEquivalents
Branch: main (current branch at session start — do not switch branches)
Starting test count: 89 (30 + 28 + 31)
Target test count: 113 or higher
Bugs to fix: 2 (AgentLoop compaction, CodebaseIndexer class References)
Issues to resolve: 2 (RepoMapBuilder naming, AgentPipeline Stage 4 comment)
New tests to add: 24 (5 fuzzy + 4 multi-patch + 4 false-CONJ + 5 RoslynCodeAnalyzer + 2 AgentPipeline + 4 bugs Phase 1)

---

*If this plan and any source file disagree about what a function does, the source file governs. Read the source file again before concluding there is a contradiction.*
