# Plan — Full SemanticCompiler Wiring into the Orchestration and Training Path
# Version: 2.0 — 2026-05-06
# Supersedes: PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v1.md
# Session type: ARCHITECT
# Test floor entering this plan: 1224

---

## What this plan is

Version 1 connected project 12 to project 21 at the surface level — one call in the
compiler unit, one delegation in the domain classifier. That was the minimum change.

This version is the correct full architectural change. It addresses all three
dimensions of the problem identified in the audit sessions:

1. **User prompt path** — the prompt must pass through the full SemanticCompiler
   pipeline before reaching the router and domain classifier. Thirteen proven
   improvements (E01–E27) activate in one connection.

2. **Training path** — the capability trainer must compile training examples through
   the same pipeline that processes runtime prompts. This is the Teaching-Runtime
   Symmetry requirement proved by E51B. Training and inference must share the same
   60-slot feature vector representation.

3. **Result translation path** — the LoRA adapter produces output in the compressed
   predicate space. The result must be translated back through the reverse pipeline
   (beautifier, template lookup) into human-readable guidance. This is the path from
   machine-internal representation back to the developer.

The canonical pipeline to use is **project 29** (`SemanticCompilerPipeline`), not
project 12 (`SemanticCompilerFacade`). Project 29 is the newer, fabric-first version
with proper dependency injection, `ICompilerStorage` and `ICompilerEventLogger`
interfaces, the `RoleAssigner` (E08/E09), and the correct stage ordering enforced
by sequential assignment. Project 12 is a functional but less structured predecessor.

---

## The three flows this plan connects

```
FLOW A — Runtime prompt path

raw developer prompt
  → SemanticCompilerPipeline.Compile(prompt)
      Stage 1: AstTokenizer        (E01)  — AST-level token split
      Stage 2: StackDetector       (E20)  — C#/TS/PY detection BEFORE normalisation
      Stage 3: RouteClassifier     (E18)  — D-04 domain at 95% accuracy
      Stage 4: SemanticNormalizer  (E15)  — stem + stop-word removal
      Stage 5: FamilyCollapser     (E16)  — synonym collapse
      Stage 6: LogicExtractor      (E17)  — 22-predicate vocabulary
  → PipelineOutput (domain, predicates[], stack, compression_ratio)
  → Decomposer.Split(normalised segments) (E21)
  → For each segment:
      → MultiSubPhaseRouterUnit.Score(segment predicates)
      → Branch to domain adapter + sub-phase flow
  → Aggregate results
  → TemplateBeautifier (E23) on result predicates → human readable output


FLOW B — Training path (Teaching-Runtime Symmetry)

knowledge edge detected → capability trainer called
  → SemanticCompilerPipeline.Compile(userRequest) — SAME pipeline as Flow A
  → 60-slot feature vector (22 predicate slots, 5 domain slots, 32 root slots, 1 decomp)
  → this vector IS the chosen DPO example — not a raw string
  → rejected vector constructed from wrong-domain predicates
  → DpoTrainer.Step(chosen_vector, rejected_vector)
  → domain adapter weights updated
  → checkpoint saved


FLOW C — Result translation path

domain adapter produces predicate output vector
  → LogicExtractor.Decode (reverse map predicates → operation names)
  → TemplateBeautifier.FindBest(predicates, stackLabel) (E23)
      → if template found: return formatted code guidance
      → if not: CSharpBeautifier.Beautify(predicates, stack) fallback
  → human-readable guidance string
  → FinalHumanResult in LogicalFlowResult
```

---

## What exists and what must be built

### Already exists — no code to write

- `29.SemanticCompilerPipeline.Compile(PipelineInput)` — full 6-stage pipeline
- `29.SemanticCompilerPipeline` stages: AstTokenizer, StackDetector, RouteClassifier,
  SemanticNormalizer, FamilyCollapser, LogicExtractor
- `12.SemanticCompiler.Planning.Decomposer.Split(prompt)` — E21 boundary detection
- `12.SemanticCompiler.Beautification.TemplateBeautifier.FindBest(predicates, stack)`
- `12.SemanticCompiler.Beautification.CSharpBeautifier.Beautify(predicates, stack)`
- `29.SemanticCompiler.Compression.RoleAssigner.AssignRoles(tokens)` — E08/E09
- `22.AgentAlgorithms.Routing.RouteClassifier.ClassifyByKeywords` — E18 classifier
- `16.AutonomousLearning.Training.ReplayBuffer` — E43 70/30 replay
- `16.AutonomousLearning.TrustScorer.TrustScorer` — E40V2 trust promotion

### Must be built — new code required

**New project: `23.SemanticCompilerAdapter`**

Project 21 is the execution agent. It should not contain pipeline logic. The wiring
between project 21 and project 29 belongs in a thin adapter project that:

- References project 29 (SemanticCompiler29) and project 12 (SemanticCompiler)
- Exposes exactly the interfaces project 21 needs as `ILogicalUnit` wrappers
- Contains no business logic — pure adaptation

This follows the same pattern as projects 21/22/23 adapters already in the codebase.

```
23.SemanticCompilerAdapter/
  SemanticCompilerAdapterUnit.cs    — ILogicalUnit<string, PipelineOutput>
  PromptSegmenterUnit.cs            — ILogicalUnit<string, PromptSegment[]>
  ResultTranslatorUnit.cs           — ILogicalUnit<int[], string>
  DomainClassifierUnit.cs           — ILogicalUnit<string, DomainLabel>
  TrainingVectorBuilder.cs          — not a logical unit, called by trainer
```

**Changes to project 21:**

- `PromptRoutingInputCompilerUnit` — inject `ILogicalUnit<string, PipelineOutput>` 
  (the SemanticCompilerAdapterUnit), call it before predicate extraction, use
  `PipelineOutput.Predicates` as the compiled predicate list, use
  `PipelineOutput.Domain` as the domain label.

- `DefaultLogicalCapabilityTrainer` — replace `BuildTrainingText` raw string with a
  call to `TrainingVectorBuilder.Build(userRequest)` which returns the 60-slot float
  vector. Pass this vector as the DPO chosen example.

- `UniversalNineNodeDevelopmentFlow` — wire `ResultTranslatorUnit` into node 06
  (result translator position) so the predicate output from nodes 04/05 is translated
  back through `TemplateBeautifier` before reaching the formatter.

- Add project reference to `23.SemanticCompilerAdapter` in
  `AgentInfrastructure.csproj`.

---

## Phase 0 — Baseline gate

```bash
cd C:\Projects\llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: 1224 passing, 0 failing. Stop and report if different.

---

## Phase 1 — Create project 23.SemanticCompilerAdapter

### What to create

New project file: `23.SemanticCompilerAdapter/SemanticCompilerAdapter.csproj`

References:
```xml
<ProjectReference Include="..\29.SemanticCompiler\SemanticCompiler29.csproj" />
<ProjectReference Include="..\12.SemanticCompiler\SemanticCompiler.csproj" />
<ProjectReference Include="..\21.AgentInfrastructure\AgentInfrastructure.csproj" />
```

### File 1: SemanticCompilerAdapterUnit.cs

This unit wraps `SemanticCompilerPipeline.Compile` as an `ILogicalUnit`.

```
Contract:
  SemanticCompilerAdapterUnit : ILogicalUnit<string, PipelineOutput>

Input:  raw prompt string
Output: PipelineOutput(domain, predicates[], stack, compression_ratio)

Algorithm:
  1. If input is null or empty, return failure with code EMPTY_PROMPT.
  2. Call SemanticCompilerPipeline.Compile(new PipelineInput(input)).
  3. If compile fails, return failure forwarding the pipeline error.
  4. Return the PipelineOutput wrapped in LogicalUnitResult.
```

### File 2: PromptSegmenterUnit.cs

This unit wraps `Decomposer.Split` as an `ILogicalUnit`.

```
Contract:
  PromptSegmenterUnit : ILogicalUnit<string, PromptSegment[]>

Input:  raw prompt string (before or after normalisation)
Output: PromptSegment[] — one entry when no boundary detected, multiple when split

Algorithm:
  1. Call Decomposer.Split(input).
  2. If result is a single segment identical to the input, return array of one.
  3. If multiple segments detected, return all segments.
  4. Never return empty array — fallback to single-segment wrapping the full prompt.
```

### File 3: ResultTranslatorUnit.cs

This unit translates predicate integer arrays back into human-readable guidance.

```
Contract:
  ResultTranslatorUnit : ILogicalUnit<ResultTranslatorRequest, string>

Input:  ResultTranslatorRequest(int[] predicates, StackLabel stack)
Output: human-readable guidance string

Algorithm:
  1. Call TemplateBeautifier.FindBest(predicates, stackName).
  2. If template found, return template.Code — this is the E23 path (+833%).
  3. If no template, call CSharpBeautifier.Beautify(predicates, stack) as fallback.
  4. Return the guidance string.
```

### File 4: DomainClassifierUnit.cs

This unit wraps `RouteClassifier.ClassifyByKeywords` as an `ILogicalUnit`.

```
Contract:
  DomainClassifierUnit : ILogicalUnit<string, DomainClassification>

Input:  raw prompt string
Output: DomainClassification(label, confidence, evidence[])

Algorithm:
  1. Call RouteClassifier.ClassifyByKeywords(input).
  2. Compute confidence from keyword hit ratio.
  3. Return DomainClassification with label, confidence, evidence tokens.
```

### File 5: TrainingVectorBuilder.cs

Not an ILogicalUnit. A static helper called by the capability trainer.

```
Contract:
  TrainingVectorBuilder.Build(string userRequest) → float[60]

Algorithm:
  1. Call SemanticCompilerPipeline.Compile(new PipelineInput(userRequest)).
  2. Extract predicates from PipelineOutput.
  3. Build 60-slot vector:
     - slots 0-21: predicate counts (22 predicate vocabulary)
     - slots 22-26: domain one-hot (5 D-04 domains)
     - slots 27-58: normalised root word presence (32 root slots)
     - slot 59: segment count from Decomposer (1 if single task, 2+ if multi-task)
  4. Return vector.

This is the E51B FeatureVectorContract:
  "60 slots: 22 predicate counts, 5 domain slots, up to 32 normalized roots,
   and 1 decomposition-count slot."
```

### Gate B — Phase 1

```bash
dotnet build 23.SemanticCompilerAdapter/ 2>&1 | grep -E "error" | head -10
```

Expected: zero errors. Also run:

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: still 1224 passing (no behaviour changed yet — new project only).

---

## Phase 2 — Wire SemanticCompilerAdapterUnit into PromptRoutingInputCompilerUnit

### Changes to project 21

**File:** `21.AgentInfrastructure/AgentInfrastructure.csproj`

Add:
```xml
<ProjectReference Include="..\23.SemanticCompilerAdapter\SemanticCompilerAdapter.csproj" />
```

**File:** `21.AgentInfrastructure/Logical/PromptRoutingInputCompilerUnit.cs`

Current: calls `ExtractPredicates(rawPrompt)` directly on the raw string.

Target:

```
Algorithm — new ExecuteAsync:

1. Call SemanticCompilerAdapterUnit.ExecuteAsync(prompt).
   — If success: use PipelineOutput.Predicates as predicate list.
     Use PipelineOutput.Domain.ToString() as domain label.
     The normalised text from the pipeline enriches predicate accuracy.
   — If failure: fall back to existing ExtractPredicates(rawPrompt).
     Log warning that semantic compilation fell back to raw path.

2. Call PromptSegmenterUnit.ExecuteAsync(prompt).
   — If single segment: proceed as today (one document, one routing pass).
   — If multiple segments: return one SubPhaseRouterSourceDocument per segment,
     each compiled independently with its own predicate set and domain label.
     The orchestrator receives an array of source documents, one per task.

3. Construct SubPhaseRouterSourceDocument with domain label field populated.
```

**Concrete rule for multi-segment:** when `PromptSegmenterUnit` returns N segments,
`ExecuteAsync` returns N `SubPhaseRouterSourceDocument` records. The orchestrator's
`RouteAsync` iterates over all of them and scores branches per segment. This means
a three-task prompt produces three independent routing passes. Each segment routes
to the sub-phases most relevant to its own text, not to the combined signal.

### Gate B — Phase 2

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "PromptRoutingInputCompilerUnit" 2>&1 | tail -10
```

Expected: all existing tests pass. Normalised text may change predicate sets for some
prompts — verify each change is correct before committing. If a test now fails because
normalisation produces a different but more accurate predicate set, update the test
with a comment explaining the correction.

New tests to add:

- Multi-task prompt splits into two segments and produces two source documents.
- Single-task prompt produces one source document as before.
- Semantic pipeline failure falls back to raw path without throwing.
- Domain label from pipeline appears on all produced source documents.

---

## Phase 3 — Wire TrainingVectorBuilder into DefaultLogicalCapabilityTrainer

### Changes to project 21

**File:** `21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs`

Current: `BuildTrainingText` returns a raw concatenated string. This string is
tokenised by `BpeTokenizer.Train` and used as the DPO chosen text.

Target: replace the chosen/rejected text pair with compiled feature vectors.

```
Algorithm — new training example construction:

For each branch being trained:

1. Call TrainingVectorBuilder.Build(userRequest).
   → Returns float[60] — the chosen vector in predicate space.
   → This represents "what the prompt is asking for" in the model's vocabulary.

2. Construct the rejected vector as the bitwise complement in domain space:
   → Set the correct domain slot to 0.
   → Set a randomly selected wrong domain slot to 1.
   → Keep predicate slots from the chosen vector (same predicates, wrong domain).
   → This teaches the model to prefer correct domain over wrong domain.

3. The DPO batch is now:
   chosen:   float[60] feature vector for this prompt (correct domain + predicates)
   rejected: float[60] feature vector with wrong domain (same predicates, wrong bucket)

4. Verify symmetry before training:
   → TrainingVectorBuilder.Build applied to a held-out prompt must produce a
     vector whose cosine similarity to the chosen vector is higher than to the
     rejected vector.
   → If symmetry check fails, log warning but continue — the training will still
     improve over time.
```

**Important:** This changes the DPO training input from a raw string to a float
vector. The `LogicalDpoTrainingUnit` currently tokenises the training text and passes
token IDs to the transformer. The transformer then needs to receive the feature
vector, not token IDs. This requires a new path in `LogicalDpoTrainingUnit`:

```
New DPO training path for feature vectors:

LogicalDpoTrainingRequest gains a new field:
  float[]? ChosenVector    — 60-slot feature vector (nullable, new path)
  float[]? RejectedVector  — 60-slot rejected vector (nullable, new path)

When these fields are populated:
  - Skip BpeTokenizer tokenisation.
  - Pass vectors directly to DpoTrainer.StepVector (new method — see Phase 4).

When these fields are null:
  - Use existing raw string tokenisation path (backward compatible).
```

### Gate B — Phase 3

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "DefaultLogicalCapabilityTrainer" 2>&1 | tail -10
```

Expected: all existing trainer tests pass. New tests to add:

- TrainingVectorBuilder produces a 60-slot vector for a known prompt.
- Chosen and rejected vectors have cosine similarity below 0.5 (they must differ).
- Teaching symmetry: Build(promptA) is closer to Build(promptB_same_domain) than
  to Build(promptC_different_domain).

---

## Phase 4 — Add DpoTrainer.StepVector for feature-vector training

**File:** `11.DPO/DpoTrainer.cs`

Add a new method alongside the existing `Step(DpoBatch batch, int step)`:

```
Algorithm — StepVector:

Input: float[] chosenVector, float[] rejectedVector, int step

1. Project both vectors from 60-dimensional predicate space into the model's
   hidden dimension space via a learned linear projection layer.
   (This projection layer is initialised as a random matrix and is part of the
   LoRA adapter — it is trainable.)

2. Run the projected vectors through the policy model's final layer to get
   log-probabilities for chosen and rejected.

3. Run the same projections through the frozen reference model.

4. Compute margin = chosenReward - rejectedReward (same formula as existing Step).

5. Compute loss = -log(sigmoid(margin)).

6. Backward through projection layer and LoRA adapter parameters only.

7. AdamW step on projection + LoRA parameters.

8. Return DpoMetrics (same structure as existing Step).
```

The projection layer bridges the gap between the 60-slot semantic feature space and
the model's hidden dimension. For the tiny model (hidden=16), the projection is a
60×16 matrix — 960 parameters. For ModelConfig.Small (hidden=512), it is 60×512 —
30,720 parameters, still tiny.

### Gate B — Phase 4

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --filter "DpoTrainer" 2>&1 | tail -10
```

Expected: all existing DPO tests pass. New tests:

- StepVector accepts 60-slot vectors and returns DpoMetrics.
- Loss decreases when chosen and rejected vectors are clearly separated.
- Projection layer weights are updated by StepVector.
- Base model weights remain frozen after StepVector.

---

## Phase 5 — Wire ResultTranslatorUnit into UniversalNineNodeDevelopmentFlow

**File:** `21.AgentInfrastructure/Logical/UniversalNineNodeDevelopmentFlow.cs`

Current: all nine nodes are scaffold nodes that append their own names to a chain.
Node 06 is the result translator position but does nothing meaningful.

Target for node 06 only (all other nodes unchanged):

```
Algorithm — node 06 result translator:

Input: LogicalFlowRequest<string> with the accumulated result from nodes 04 and 05.

1. Extract predicate integer array from the flow metadata (written by nodes 04/05).
   If no predicate metadata exists, pass through unchanged (safe fallback).

2. Call ResultTranslatorUnit.ExecuteAsync(predicates, stackLabel).
   → Calls TemplateBeautifier.FindBest first (E23 path).
   → Falls back to CSharpBeautifier if no template matches.

3. Replace the FinalHumanResult with the beautifier output if non-empty.
   If beautifier returns empty, keep the existing scaffold output.

4. This is additive — node 06 enriches the result, it does not replace nodes 07-09.
```

The intent: nodes 04 and 05 accumulate predicate evidence about what the sub-phase
requires. Node 06 translates that into human-readable guidance. Nodes 07-09 then
apply arbiter verification and formatting on top of the translated result.

### Gate B — Phase 5

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: all tests pass. The scaffold warning `generic_node_requires_domain_learning`
should now be absent for node 06 on prompts with known predicate patterns.

---

## Phase 6 — Full suite verification and commit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: 1224 + new tests passing, 0 failing.

Count of new tests:
- Phase 1: 5 unit tests for the adapter project
- Phase 2: 4 tests for PromptRoutingInputCompilerUnit
- Phase 3: 3 tests for TrainingVectorBuilder
- Phase 4: 4 tests for DpoTrainer.StepVector
- Phase 5: 2 tests for node 06 result translation
- Total new: 18 tests
- New floor: 1242

Commit message:
```
wire SemanticCompiler pipeline into prompt path, training path, and result translation

- New project 23.SemanticCompilerAdapter: ILogicalUnit wrappers for pipeline stages
- SemanticCompilerAdapterUnit: full 6-stage pipeline (E01,E15-E17,E18,E20) per prompt
- PromptSegmenterUnit: E21 boundary detection, multi-task prompts split independently
- ResultTranslatorUnit: E23 template beautifier + beautifier fallback on node-06 output
- DomainClassifierUnit: E18 RouteClassifier as ILogicalUnit
- PromptRoutingInputCompilerUnit: calls pipeline before predicate scan, domain label
  propagated; multi-segment prompts produce one source document per segment
- DefaultLogicalCapabilityTrainer: 60-slot feature vectors replace raw training strings
  (E51B teaching-runtime symmetry); TrainingVectorBuilder.Build as shared utility
- DpoTrainer.StepVector: new method accepts feature vectors, projects into hidden space
  via trainable projection layer in LoRA adapter
- UniversalNineNodeDevelopmentFlow node-06: ResultTranslatorUnit wired in
- Floor raises from 1224 to 1242
```

---

## Execution order and stop conditions

Execute phases in strict order: 0, 1, 2, 3, 4, 5, 6.

Do not start a phase until the Gate B of the prior phase passes.

Stop immediately and report if:
- Phase 0 gate shows a count below 1224
- Any Gate B build step produces errors
- Any phase causes a previously passing test to fail
- A test is modified to silence a genuine regression rather than to correct it

---

## What changes and what does not

| Component | Change |
|---|---|
| `23.SemanticCompilerAdapter` | New project — five adapter classes |
| `PromptRoutingInputCompilerUnit` | Calls pipeline before predicate scan; multi-segment aware |
| `DefaultLogicalCapabilityTrainer` | Uses 60-slot feature vectors for DPO training |
| `DpoTrainer` | New StepVector method for feature-vector training |
| `UniversalNineNodeDevelopmentFlow` node 06 | Wired to ResultTranslatorUnit |
| `MultiSubPhaseRouterUnit` | No change |
| `AdaptiveLogicalOrchestrator` | No change |
| `InMemoryLogicalUnitRegistry` | No change |
| `LogicalContracts.cs` | No change to interfaces |
| `29.SemanticCompilerPipeline` | No change — used as-is |
| `12.SemanticCompilerFacade` | No change — not used by new path |

---

## The experiments this activates

When all six phases are complete, the following proven improvements become active
in the execution path for the first time:

| Experiment | Improvement | Activated by |
|---|---|---|
| E01 AstTokenizer | +93% perplexity | Phase 2 — pipeline stage 1 |
| E15 Semantic tokenizer | +10% perplexity | Phase 2 — pipeline stage 4 |
| E16 Synonym collapse | +8% perplexity | Phase 2 — pipeline stage 5 |
| E17 Logic predicates | +97% perplexity | Phase 2 — pipeline stage 6 |
| E18 Domain classifier | +15% accuracy, 95% domain | Phase 2 — pipeline stage 3 |
| E20 StackDetector | validated (synthetic) | Phase 2 — pipeline stage 2 |
| E21 Prompt splitter | +143% boundary accuracy | Phase 2 — segmenter unit |
| E23 TemplateBeautifier | +833% template match | Phase 5 — node 06 |
| E25 PatternClassifier | +155% translation | Phase 2 — stage 3 feeds classifier |
| E26 ContextualDictionary | +150% disambiguation | Phase 2 — normalisation |
| E27 PatternReorderer | +250% exact match | Phase 2 — stage 4/5 normalise first |
| E51B Symmetry | teaching = runtime repr. | Phase 3 — feature vector training |

---

# Plan version: 2.0
# Author: ARCHITECT session 2026-05-06
# Phases: 6 + Gate 0
# New tests: 18
# New floor: 1242
# New project: 23.SemanticCompilerAdapter
