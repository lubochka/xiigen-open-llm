# integration-testing-SKILL.md — In-process integration testing for .NET fabric

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
## v1.0: translated from xiigen docker-local-testing-SKILL (Docker/Compose → in-process .NET)

## When to load
Load before writing any test that exercises more than one class together.
Load when a unit test uses stubs for BM25Scorer, HnswIndex, or HybridRetriever.
Load when phase gate requires fabric coverage.
Load when Docker was used in an equivalent xiigen test.

---

## The problem this skill solves

Xiigen used Docker Compose to spin up Elasticsearch and Redis containers for integration tests.
This project has no container layer. Every implementation — BM25Scorer, HnswIndex,
ISessionStorage, IAgentMessageBus, PermissionGate — is pure in-process .NET.

This means integration tests are simpler: create real instances directly in test code.
No containers, no sidecar services, no network. Each test creates its own fresh instance
with no shared state.

---

## What "integration test" means in this project

A test that wires two or more real implementations together and exercises a cross-class code path.
No stubs on the interfaces directly involved in the behaviour being tested.

The main product integration host is `Tests/LlmFromScratch.Tests.csproj`. An experiment may have its own assigned Checks project; it does not silently inherit the product's full acceptance program. Use the real boundary, project and commands in the card and [QA46](../plans/replanning-possible-diffs/experimets/46-qa-execution-test-repair-and-fresh-results-2026-11-03.md).

---

## Fabric coverage matrix

Every fabric implementation must have at least one integration test that exercises it
with real inputs and verifies real output. This replaces the Docker Compose "container up"
coverage check from xiigen.

### Search fabric (12.SearchEngine / 06.VectorSearch / 08.GraphRag)

| Implementation | Must test | Assertion |
|----------------|-----------|-----------|
| BM25Scorer | Score two documents against a query; top document must have higher score | `Assert.True(score1 > score2)` |
| InvertedIndex | Index two documents, search a term; only the correct document returned | `Assert.Contains(expected, results)` |
| BruteForceIndex | Add two vectors; nearest-neighbour query returns the closer one | `Assert.Equal(expectedId, result[0].Id)` |
| HnswIndex | Add known vectors and query independently expected neighbours | Check expected IDs/order where contractual and count; count alone does not prove correctness |
| HybridRetriever | Feed BM25 + vector results; combined output respects hybrid weight | `Assert.True(hybridScore > 0)` |
| SearchEngineFacade | Full pipeline: index text, search a query, get ranked results | `Assert.True(results[0].Score > 0)` |

### Agent infrastructure fabric (21.AgentInfrastructure)

| Implementation | Must test | Assertion |
|----------------|-----------|-----------|
| SessionStateStore | Save + Load round-trip for AgentState | `Assert.Equal(savedTask, loaded.CurrentTask)` |
| PermissionGate | Path inside root → Allowed; path outside root → Denied | two separate [Fact] methods |
| ToolRegistry | Register + invoke a tool; result flows through OperationResult | `Assert.True(result.IsSuccess)` |
| FileChangeApplier | Apply a search-replace patch; new content matches expectation | `Assert.Equal(expected, writer.Get(path))` |

### Agent algorithms fabric (22.AgentAlgorithms)

| Implementation | Must test | Assertion |
|----------------|-----------|-----------|
| AgentOrchestrator | Register two agents; route a task; both agents called via IAgentMessageBus | `Assert.Equal(2, bus.Calls.Count)` |
| ComplexTaskDecomposer | "do X then do Y" → two sub-tasks with correct roles | `Assert.Equal(2, subTasks.Count)` |
| ContextCompactor | 12-message history → 1 summary + 3 recent | `Assert.Equal(4, compacted.Count)` |

---

## Test structure pattern

```csharp
// Integration test — no stubs on the components being tested
[Fact]
public async Task BM25Scorer_TwoDocuments_HigherScoreForMatchingDoc()
{
    // Arrange: real InvertedIndex + real BM25Scorer wired together
    var index  = new InvertedIndex();
    var scorer = new BM25Scorer(index);

    index.Index("doc1", "the quick brown fox");
    index.Index("doc2", "a slow green tortoise");

    // Act: score both documents against a query
    var result1 = scorer.Score("doc1", "quick fox");
    var result2 = scorer.Score("doc2", "quick fox");

    // Assert: matching document scores higher (fabric produces real scores)
    Assert.True(result1.Value > result2.Value,
        $"Expected doc1 ({result1.Value:F3}) > doc2 ({result2.Value:F3}) for query 'quick fox'");
}
```

---

## What replaces Docker Compose setup/teardown

| Docker concept | This project equivalent |
|----------------|------------------------|
| `docker compose up -d` before test run | Each `[Fact]` creates its own instance in the constructor or test body |
| Container isolation (each test sees clean state) | `new BruteForceIndex()` / `new InvertedIndex()` per test — no shared state |
| `docker compose down` after test run | .NET GC — no explicit teardown needed |
| `docker logs` for debugging | `result.ErrorMessage` + `result.ErrorCode` from OperationResult |
| Health check before tests | Build step: `dotnet build` must succeed before any test runs |

---

## Stubs that are allowed in integration tests

Stubs are allowed for components that are NOT part of the behaviour being tested.

| Interface | Allowed stub | When |
|-----------|-------------|------|
| ICodeReader | InMemoryCodeReader | When testing ProjectIndexer without filesystem I/O |
| ICodeWriter | InMemoryCodeWriter | When testing FileChangeApplier without filesystem I/O |
| ILlmClient / ILlmGateway | MockLlmClient / MockLlmGateway | When testing agent loop orchestration, not LLM accuracy |
| IAgentMessageBus | MockAgentMessageBus | When testing orchestrator routing, not actual agent execution |

Stubs are NOT allowed for:
- BM25Scorer, InvertedIndex (search fabric under test)
- HnswIndex, BruteForceIndex (vector fabric under test)
- HybridRetriever (hybrid fabric under test)
- PermissionGate (security fabric under test)
- ISessionStorage / SessionStateStore (persistence under test)

---

## Gate B format for integration tests

Every plan phase that includes integration tests must have a Gate B in the form:

```
Gate B: dotnet test --filter "FullyQualifiedName~<ClassName>" exits 0;
        <ClassName>.cs contains [Fact] covering <specific behaviour>;
        test count >= <N> (was <M>)
```

---

## Anti-patterns

### Anti-pattern 1: Stub replacing the thing under test
```
✗ Testing HybridRetriever with a stub IVectorIndex that always returns empty
  The retriever never exercises its merge logic — test is vacuous

✓ Provide a real BruteForceIndex or HnswIndex as the vector component
  Only stub the ICodeReader or ILlmClient if those are not under test
```

### Anti-pattern 2: Shared state between tests
```
✗ static InvertedIndex _sharedIndex = new();  // polluted by earlier tests
  Test results depend on run order — flaky

✓ Create a new InvertedIndex() per test
  No static fields in test classes
```

### Anti-pattern 3: Testing only the happy path
```
✗ Only test BM25Scorer.Score succeeds — never test Score on an empty index

✓ At least one test per implementation must exercise the error/empty path
  e.g., BruteForceIndex with 0 items, InvertedIndex.Search for a term not indexed
```
