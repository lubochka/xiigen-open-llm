# Self-Verification

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the equivalent in the operator's language), challenges an answer, or says a failure came
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

## Overview

Six rules for verifying your own work before claiming it is correct.
Applied after writing code, before claiming completion.

---

## Rule 1 — Fresh Fixtures

Tests must use real contracts, not hand-crafted fakes that happen to pass.

```csharp
// ✅ Real fixture — uses actual BruteForceIndex with real float[] data
var index = new BruteForceIndex(dimensions: 3);
index.Add(0, new float[] { 1.0f, 0.0f, 0.0f });
index.Add(1, new float[] { 0.0f, 1.0f, 0.0f });
var result = index.Search(new float[] { 0.9f, 0.1f, 0.0f }, topK: 1);
Assert.Equal(0, result.Value.First().Index);

// ❌ Hand-crafted fake — returns whatever you tell it to, proves nothing
var mock = new Mock<IVectorIndex>();
mock.Setup(x => x.Search(It.IsAny<float[]>(), 1)).Returns(OperationResult<...>.Ok(fakeList));
```

When in doubt: use the real implementation. Mocks are for testing callers,
not for verifying that the thing being called works.

---

## Rule 2 — Categorize the Change

Before writing a verification plan, classify what kind of change was made:

| Category | Definition | Verification Required |
|----------|-----------|----------------------|
| `MACHINE_CHANGE` | Changed a fixed interface or rule | All callers + integration tests |
| `FREEDOM_CHANGE` | Changed a swappable implementation | That implementation's tests only |
| `BACKWARD_COMPAT` | Changed without breaking contract | Contract test + regression suite |
| `NEW_ARTIFACT` | Added a new class/interface | Unit + integration + doc sync |

Categorize first, then choose the right verification scope. Over-verifying wastes time.
Under-verifying ships bugs.

---

## Rule 3 — Right Level

Match the test level to the blast radius of the change.

| Change Blast Radius | Test Level |
|--------------------|-----------|
| Single method, no callers changed | Unit test (`[Fact]` + direct construction) |
| Class with DI dependencies | Integration test (`ServiceCollection` + real DI) |
| Multiple subprojects affected | Full suite (`dotnet test LlmFromScratch.sln`) |
| Public interface changed | All callers tested + full suite |

Do not run the full suite to verify a private method change.
Do not run only a unit test to verify an interface change.

---

## Rule 4 — Cascade

Trace the full effect chain before declaring a fix complete.

```
Changed: BruteForceIndex.Search()
  → Used by: HybridRetriever.SearchAsync()
    → Used by: AgentOrchestrator (session search)
      → Used by: 21.AgentInfrastructure (E2E flow)
```

Verification must cover every node in the chain that the change can reach.

**How to find callers:**
```bash
grep -rn "BruteForceIndex\|IBruteForceIndex\|IVectorIndex" Src/ --include="*.cs"
```

Verify each caller's tests still pass after the change.

---

## Rule 5 — Non-Renderable

Generated code (SemanticCompilerPipeline output, TemplateBeautifier output)
cannot be verified by inspection alone.

**Inspection is not verification:**
```
❌ "The generated predicate looks correct to me"
✅ Run the generated code through a Roslyn parse check or xUnit assertion
```

For generated C# code:
```csharp
// Verify generated code parses without error
var tree = CSharpSyntaxTree.ParseText(generatedCode);
var diagnostics = tree.GetDiagnostics();
Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
```

For generated templates:
```csharp
// Verify template renders expected structure
var output = beautifier.Render(template, data);
Assert.Contains("expected_marker", output);
Assert.DoesNotContain("{{", output); // No unresolved placeholders
```

---

## Rule 6 — Predict

Before making a change, state a prediction. After making the change, verify it.

**Pre-change prediction checklist:**
```
Before touching Src/09.VectorSearch/BruteForceIndex.cs:

1. Current test count: [run dotnet test --filter "VectorSearch" → record N]
2. Predicted outcome: the zero-vector test will go from FAIL to PASS
3. Predicted side effects: none — this method has no callers outside VectorSearch
4. Blast radius check: grep confirms IVectorIndex callers = {HybridRetriever only}
5. Risk: dividing magnitude change may affect non-zero vectors too
   → Mitigation: run full VectorSearch suite, not just the new test
```

After the change:
```
1. Actual test count: [run dotnet test → N+1 or same]
2. Prediction matched? YES / NO — if NO, explain delta
3. Side effects observed: none / [describe]
```

If prediction does not match outcome: investigate before claiming complete.

---

## Verification Quick Checklist

```
□ Categorized the change (MACHINE/FREEDOM/BACKWARD_COMPAT/NEW_ARTIFACT)?
□ Identified blast radius — all callers in the chain?
□ Test level matches blast radius?
□ Fresh fixtures used (not mocks of the thing being tested)?
□ Generated code verified with parse check, not just inspection?
□ Pre-change prediction recorded?
□ Post-change outcome matches prediction?
□ dotnet test --no-build shows Failed: 0?
```
