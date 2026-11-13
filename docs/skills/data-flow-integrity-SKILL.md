# Data Flow Integrity

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

## What This Skill Prevents

The "all classes pass individually but the pipeline fails" failure mode.
Each class tests with its own fixtures. But in the live chain, if class A drops
a field from its output, every downstream class receives null where it expects a value —
and all of them produce wrong results while their individual unit tests still pass.

---

## Step 1 — Build the Data Flow Map

For each processing chain in the project, document the data contract:

```
Chain: SemanticCompilerPipeline → TemplateBeautifier → GeneratedPredicates

SemanticCompilerPipeline OUTPUT (PipelineOutput):
  {
    Predicates: Predicate[],   ← consumed by TemplateBeautifier
    TokenCount: int,           ← consumed by metrics logging
    ParseErrors: string[]      ← consumed by error reporting
  }

TemplateBeautifier EXPECTS:
  Predicates: ✅ present in PipelineOutput
  FieldNames: ⚠️ not in PipelineOutput schema — needs Predicate.FieldName property

Chain: HybridRetriever → SessionStorage.SaveAsync()

HybridRetriever OUTPUT (SearchResults):
  {
    Results: IReadOnlyList<SearchResult>,  ← consumed by SessionStorage
    QueryVector: float[],                   ← consumed by cache
    TotalMatchCount: int                    ← consumed by pagination
  }
```

---

## Step 2 — Extract Actual Schemas from Live Code

Do not trust fixtures. Read the actual record or class definitions:

```bash
# Find the actual output type of a class
grep -n "public.*class PipelineOutput\|public.*record PipelineOutput" Src/ -r --include="*.cs"
# Then read the file to see all fields

# Find what a consuming class actually uses from its input
grep -n "\.Predicates\|\.TokenCount\|\.FieldName" Src/ -r --include="*.cs" | grep -v "Tests"
```

---

## Step 3 — Run the Schema Diff Per Chain Step

For each (producer, consumer) pair in the chain:

```bash
# Find fields the consumer uses
grep -n "input\.\|result\.\|output\.\|message\." Src/NN.ConsumerClass/ConsumerClass.cs | head -20
# Note every .Property accessed from the input

# Find fields the producer actually provides
grep -n "public.*{.*get;" Src/NN.ProducerClass/ProducerOutput.cs | head -20
# Cross-reference: is every accessed property present in the producer's output?
```

Any property accessed by the consumer but not present in the producer's output = DATA INTEGRITY GAP.

---

## Step 4 — Classify Each Gap

| Gap Type | Description | Fix |
|----------|-------------|-----|
| MISSING_FIELD | Producer omits a field the consumer requires | Add to producer's output record |
| RENAMED_FIELD | Producer uses `Count`, consumer expects `TotalCount` | Align naming in one record |
| TYPE_MISMATCH | Producer emits `string`, consumer expects `float` | Fix in producer |
| NULLABLE_GAP | Field present sometimes (nullable) but consumer expects non-null | Add null guard in consumer + document contract |
| NESTED_MISMATCH | Producer flattens, consumer expects nested | Align structure |

---

## Step 5 — Produce the Integrity Report

```markdown
## Data Flow Integrity Report — [date]

Chain: [ClassA] → [ClassB] → [ClassC]

| Link | Producer Field | Consumer Expects | Status |
|------|---------------|-----------------|--------|
| PipelineOutput→Beautifier | Predicates | Predicates | ✅ MATCH |
| PipelineOutput→Beautifier | FieldName (on Predicate) | Predicate.FieldName | ❌ MISSING |
| SearchResults→SessionStorage | Results | Results | ✅ MATCH |

Gaps Found: 1
Fix Required: Add FieldName property to Predicate record in PipelineOutput

Verified By:
  grep -n "FieldName" Src/29.SemanticCompiler/Predicate.cs → line not found (gap confirmed)
  grep -n "\.FieldName" Src/29.SemanticCompiler/TemplateBeautifier.cs → line 45 (usage confirmed)
```

---

## Integration Test for Data Flow

After fixing any integrity gap, write an integration test that verifies the chain end-to-end:

```csharp
[Fact]
public async Task Pipeline_EndToEnd_AllFieldsPropagateCorrectly()
{
    // Arrange: real DI with real implementations (no mocks of the chain itself)
    var services = new ServiceCollection();
    services.AddSingleton<SemanticCompilerPipeline>();
    services.AddSingleton<TemplateBeautifier>();
    var provider = services.BuildServiceProvider();

    var pipeline = provider.GetRequiredService<SemanticCompilerPipeline>();
    var beautifier = provider.GetRequiredService<TemplateBeautifier>();

    // Act: run the full chain with a known input
    var pipelineOutput = await pipeline.ProcessAsync("fieldName = 'value'");
    var beautifiedOutput = await beautifier.RenderAsync(pipelineOutput.Value.Predicates);

    // Assert: the output has no unresolved placeholders (data made it through)
    Assert.True(pipelineOutput.IsOk);
    Assert.True(beautifiedOutput.IsOk);
    Assert.DoesNotContain("{{", beautifiedOutput.Value); // No unfilled template markers
    Assert.DoesNotContain("null", beautifiedOutput.Value); // No null propagation
}
```

---

## Anti-Patterns

1. **Trusting unit tests as proof of chain integrity** — mocks hide the gaps
2. **Running integrity check only on the first class in the chain** — the gap is always downstream
3. **Treating nullable fields as "low priority"** — null values in scoring functions produce 0 or NaN
4. **Running after the pipeline is in use** — run before any class that depends on the chain is written
