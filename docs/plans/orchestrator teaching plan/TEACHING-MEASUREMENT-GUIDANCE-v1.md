# Teaching Measurement Guidance
# Version: 1.0 — 2026-05-05
# Authority: ORCHESTRATOR-TEACHING-PLAN-v2.md
# Grounded in: AdaptiveLogicalOrchestrator.cs, MultiSubPhaseRouterUnit.cs,
#              PromptRoutingInputCompilerUnit.cs, KnnLogicalUnit.cs

---

## What this document is

This document tells you how to read the numbers that come out of each teaching
row, what they mean for the health of the machine, and what to do when they are
wrong. It is not about the teaching mechanics — those are in the teaching plan.
It is about interpreting the evidence after the fact.

Every teaching row produces three layers of numbers. You must read all three
before deciding whether the row helped, hurt, or revealed a structural problem.

---

## The three measurement layers

```text
Layer 1 — Resource numbers
  How long training took. How much RAM and CPU were consumed. How big the
  checkpoint is. Whether the machine can sustain this cost at scale.

Layer 2 — Routing numbers
  Which sub-phases the router selected. What the confidence was. Whether the
  routing matched the human meaning of the prompt.

Layer 3 — Verdict numbers
  What the orchestrator concluded after routing and executing flows. Whether
  it passed, hit a knowledge edge, or found concerns. Whether teaching moved
  the verdict in the right direction.
```

A row is only successful when all three layers are acceptable. A row can show
good resources and a wrong verdict. A row can show a correct verdict and a
routing that was too narrow. Read all three.

---

## Layer 1 — Resource numbers

### What the numbers mean

From the first real teaching cycle (bundle `legacy-docs-skills-from-old-code`,
row 1, 2026-05-05):

```text
Baseline inference (before training):
  Elapsed:                  42.039 ms
  CPU:                      18.58%
  Working-set delta:        0.328 MB
  Managed allocation delta: 117,824 bytes
  Disk delta:               4,096 bytes

Training + retry (one DPO step):
  Elapsed:                  2,561.71 ms
  CPU:                      12.35%
  Working-set delta:        19.398 MB
  Managed allocation delta: 173,456,536 bytes
  Disk delta:               3,870,720 bytes
```

### How to read them

**Inference cost (42 ms, 0.33 MB, 18.6% CPU)** is the routing cost per prompt.
This is what a user pays on every query after training is done. At 42ms for a
full 31-sub-phase routing pass this is excellent — well within real-time
interactive response time. This number should stay below 200ms as the model
grows. If it exceeds 200ms, the HNSW index or the BPE tokenizer has been given
too many training examples to search efficiently.

**Training cost (2,562 ms, 19.4 MB, 12.4% CPU)** is the cost of one DPO step.
This is paid once per teaching row, not per user query. At 2.5 seconds per row
and 74 rows in the first bundle, the full bundle trains in about 3 minutes.
That is acceptable on any ordinary PC. This number should stay below 10,000 ms
per row. If it exceeds this, the model configuration is too large for the
hardware — reduce `LoRA Rank` from 4 to 2.

**Managed allocation delta (173 MB)** looks alarming but is not a retained heap.
It is the total bytes allocated on this thread during the DPO step — including
all temporary tensors created and released during the forward and backward passes.
The .NET GC reclaims these. The number you care about is working-set delta
(19.4 MB), which is the actual change in process memory. 19 MB per row is fine.
If working-set delta grows beyond 100 MB, the model is retaining tensors it
should be releasing.

**Disk delta (3.87 MB)** is the checkpoint written after one DPO step.
This is `BaseCheckpointBytes` — the full model including frozen base weights
and updated adapter weights. The LoRA adapter contribution alone is much smaller:
with Rank=4, q_proj and v_proj, and a small hidden dimension, the adapter tensors
are typically under 100 KB. The 3.87 MB is dominated by the frozen base model.
This is fine as long as it does not grow between rows — the checkpoint should be
approximately constant in size across rows for the same model.

### The numbers that signal a problem

| Number | Threshold | Problem | Action |
|---|---|---|---|
| Inference elapsed | > 200 ms | HNSW index too large | Reduce training examples or increase `efSearch` |
| Training elapsed | > 10,000 ms | Model too large for hardware | Reduce LoRA Rank from 4 to 2 |
| Working-set delta (training) | > 100 MB | Tensor leak | Check LoraApplicator.Apply is called on same model instance as DpoTrainer |
| Working-set delta (inference) | > 10 MB | KNN index grown too large | Cap training examples per role at 500 |
| Checkpoint size grows between rows | > 10% per row | Wrong checkpoint path or model duplication | Verify SaveCheckpoint path is the same file, not a new one per row |
| `LoraAdapterBytes` | > 2 MB | LoRA Rank too high (decision D-05) | Reduce Rank from 4 to 2 |

---

## Layer 2 — Routing numbers

### What the numbers mean

The router assigns a confidence score to each sub-phase it selects. The scoring
formula is:

```text
score = (predicate_overlap × 0.55)
      + (exact_sub_phase_mention × 0.25)
      + (token_overlap × 0.20)
```

The confidence is then mapped from the raw score:

```text
confidence = 0.70 + (score × 0.27)   (capped at 0.97)
```

This means a sub-phase with no predicate match and no token match scores 0.0
and is excluded. A perfect predicate match with the sub-phase ID mentioned
explicitly in the prompt scores up to 0.97.

A sub-phase is suppressed from the routing decision when its confidence is below
0.70 — it appears as a knowledge edge instead.

From the first teaching cycle:

```text
Prompt: "I need a documentation on this project and skills for claude code"
Compiler output: DOCUMENT predicate only, confidence 0.92
Router output:
  2.1 Business Capability Definition — confidence 0.86
  2.4 Edge Cases and Boundary Conditions — confidence 0.85
```

### How to read them

**Compiler confidence (0.92)** means at least one predicate matched. 0.92 is
the standard confidence for any prompt where predicates are found. 0.70 is the
fallback when no predicates match. A confidence of 0.70 means the compiler
could not classify the prompt and the routing is unreliable.

**Router confidence (0.86, 0.85)** means both selected sub-phases had substantial
predicate and token evidence. These are reasonable scores for a clean single-predicate
prompt. High router confidence is not a sign of success by itself — the question is
whether the right sub-phases were selected at high confidence.

**Branch count** is the most important routing signal. For the first row, branch
count was 2 (sub-phases 2.1 and 2.4). The correct answer for "I need documentation
and skills for this project" is more like 5 or 6 branches: business capability
definition, architecture extraction, backend implementation review, test coverage,
and a learning/skills output sub-phase.

A single-predicate route to Phase 2 sub-phases tells you the compiler saw
"documentation" and matched only the DOCUMENT predicate. The prompt contains
implicit signals for ARCHITECTURE (understand project structure), IMPLEMENT
(inspect code), TEST (existing test coverage), and LEARNING (skills library output)
— but those words are absent so the predicates did not fire.

### What correct routing looks like

For the prompt "I need a documentation on this project and skills for claude code"
the correct routing target is:

```text
2.1 Business Capability Definition     (what does this project do?)
3.1 Architecture and Component Topology (what is its structure?)
4.1 Backend Services                   (what code exists to understand?)
5.1 Unit Test Design                   (what test coverage exists?)
7.5 Learning Set and Model Feedback    (skills library output)
```

Five branches, spanning three phases. The actual result was two branches in
Phase 2 only. This is a routing gap, not a teaching failure.

### The routing numbers that signal a problem

| Signal | Meaning | Action |
|---|---|---|
| Branch count = 1 for a multi-domain prompt | Compiler collapsed the prompt | Check which predicates fired; expand the lexicon |
| All branches in the same phase | Predicate lexicon missing terms for other phases | Add missing predicate terms (see predicate gap section below) |
| Confidence < 0.70 on a branch that should be primary | Token overlap too low | Add domain vocabulary to the sub-phase Purpose field |
| Router selects 10+ sub-phases for a single-domain prompt | Relevance floor too permissive | Check `bestScore × 0.80` floor is working; verify the prompt is not too generic |
| Sub-phase 7.5 never selected despite learning-related prompts | LEARNING predicate absent from compiler lexicon | Add LEARNING predicate (see below) |

### The predicate gap confirmed by the first cycle

The first real teaching cycle confirmed a structural gap in the compiler lexicon.
The nine predicates currently defined are:

```text
SECURITY, AUTHORIZATION, API_CONTRACT, ARCHITECTURE,
IMPLEMENT, TEST, QA, UI, DOCUMENT
```

Two predicates that exist in the topology seeder (`LEARNING`, `AUTOMATION`) have
no corresponding compiler lexicon entries. Sub-phases tagged with these predicates
will never be selected by the router because the compiler can never emit them.

Sub-phase 7.5 (Learning Set and Model Feedback) is tagged `LEARNING`. It will
never appear in a routing decision until the compiler lexicon is extended.

The fix is one addition to `PromptRoutingInputCompilerUnit.cs`:

```csharp
("LEARNING", ["skill", "skills", "learning set", "retraining", "teach the model",
              "model feedback", "capability gap", "learning cycle", "dpo",
              "training data", "pair", "held-out"]),
("AUTOMATION", ["automation", "ci", "pipeline", "monitor", "monitoring",
                "feedback loop", "harness", "coverage report"]),
```

This is a code change, not a teaching row. Teaching rows cannot fix a missing
predicate. The lexicon must be extended first, then the teaching rows that
exercise the new predicates become meaningful.

---

## Layer 3 — Verdict numbers

### The verdict scale

The orchestrator assigns verdicts in this priority order (highest severity wins
when multiple branches are aggregated):

```text
REJECT          — arbiter rejected the result; do not show to user
REPROCESS       — arbiter found a significant concern; retry required
KNOWLEDGE_EDGE  — no executable flow for the routed sub-phase
PASS_WITH_GAPS  — flow ran but found missing coverage
PASS_WITH_CONCERNS — flow ran with minor concerns
CONCERN         — same rank as PASS_WITH_CONCERNS
PASS            — clean result with no concerns
```

When a request fans out across multiple sub-phases, the aggregate verdict is
the highest-severity verdict across all branches. If 4 branches return PASS and
1 returns KNOWLEDGE_EDGE, the overall result is KNOWLEDGE_EDGE.

### What a successful teaching row changes in the verdict

Before teaching, a prompt that routes to an unregistered sub-phase always
produces KNOWLEDGE_EDGE. After teaching, `RunWithCapabilityGenerationAsync`
creates and registers a probationary flow for that sub-phase and retries. If the
retry succeeds, the overall verdict changes from KNOWLEDGE_EDGE to PASS.

From the first cycle:

```text
Before: KNOWLEDGE_EDGE  (branches: 2, knowledge edges: 2)
After:  PASS            (branches: 2, knowledge edges: 0)
```

This is the correct pattern for a successful teaching row. The verdict moved
from the worst non-error state to the cleanest state in one teaching step.

### What the verdict does NOT tell you

A PASS verdict does not mean the routing was correct. In the first cycle, the
prompt was routed to only 2 sub-phases when the correct answer was 5. Both
branches passed — the generated capability flows are extremely simple (they
produce a generic `UniversalNineNodeDevelopmentFlow` result, not a trained
specialist result). The machine is honest about what it knows and does not know,
but the verdict being PASS does not imply the human received a rich or complete
answer.

This is why Layer 2 (routing) must be read alongside Layer 3 (verdict). A PASS
verdict with wrong routing is a false success. The machine convinced itself it
handled the task while actually handling only a fraction of it.

### Reading verdicts across rows

Track the verdict trend across the rows of a bundle. Three patterns matter:

**Pattern 1 — KNOWLEDGE_EDGE to PASS:** The standard successful teaching row.
The machine detected a gap, learned a capability, and resolved it. Expected for
every new sub-phase on its first teaching row.

**Pattern 2 — PASS_WITH_GAPS persists across rows:** The flow is executing but
the arbiter is finding missing coverage. This means the training pairs for nodes
7 and 8 (arbiter selector and arbiter unit) are inadequate. The machine knows
how to route but does not know how to verify its own work. Add arbiter pairs.

**Pattern 3 — KNOWLEDGE_EDGE persists despite teaching:** The capability trainer
registered a probationary flow, but the retry still produces KNOWLEDGE_EDGE.
This means the retry ran, the new flow executed, but the flow itself returned
PASS_WITH_GAPS or CONCERN which the branching logic then scored as a knowledge
edge. Check whether `IsKnowledgeEdgeBranch` is matching on warnings inside the
generated flow rather than on the verdict itself.

### The verdict numbers that signal a problem

| Signal | Meaning | Action |
|---|---|---|
| PASS verdict but branch count lower than expected | Routing too narrow | Fix predicate lexicon or add sub-phase vocabulary |
| KNOWLEDGE_EDGE after retry | Generated capability flow returned gap verdict | Inspect warnings in the retry branch result |
| REPROCESS on a branch that should be clean | Arbiter found a real issue | Read the arbiter verdict text; this may be a genuine gap in the pair |
| Confidence improves but verdict stays KNOWLEDGE_EDGE | Confidence floor not crossed (< 0.70) | This row improved the score but not enough; add one more row for the same sub-phase |
| Overall verdict degrades from PASS to PASS_WITH_GAPS across rows | New training data introduced a new arbiter concern | Check which branch changed verdict; read its warnings |

---

## The summary decision table

After reading all three layers, use this table:

| Layer 1 resources | Layer 2 routing | Layer 3 verdict | Decision |
|---|---|---|---|
| Within limits | Correct sub-phases, expected branch count | KNOWLEDGE_EDGE → PASS | **Row succeeded. Continue to next row.** |
| Within limits | Too narrow, wrong phases | PASS | **Routing gap. Fix predicate lexicon before next row. Mark row as partial success.** |
| Within limits | Correct sub-phases | KNOWLEDGE_EDGE persists | **Retry gap. Inspect generated flow warnings. May need arbiter pairs.** |
| Within limits | Correct sub-phases | PASS_WITH_GAPS or REPROCESS | **Arbiter gap. Add arbiter pair rows before next teaching row.** |
| Training > 10,000 ms | Any | Any | **Hardware limit. Reduce LoRA Rank to 2. Restart from last promoted row.** |
| Working-set > 100 MB | Any | Any | **Tensor leak. Fix LoraApplicator call site. Do not continue teaching.** |
| LoraAdapterBytes > 2 MB | Any | Any | **D-05 violation. Reduce Rank to 2. Restart from last promoted row.** |
| Checkpoint grows per row | Any | Any | **Path error. Fix SaveCheckpoint to write same file, not new file per row.** |

---

## What the first real cycle tells us

The first teaching cycle (2026-05-05, `legacy-docs-skills-from-old-code`) was
a qualified success with one structural finding:

**What worked:**
- Resource costs are sustainable. 42ms inference, 2.5s training, 19MB RAM.
  These numbers can scale across 74 rows without issues on an ordinary PC.
- The knowledge-edge-to-pass transition worked correctly. The machine detected
  the gap, trained a probationary capability, retried, and passed.
- The routing logic is functioning. The compiler correctly identified DOCUMENT,
  the router correctly scored sub-phases 2.1 and 2.4, and the confidence values
  are within expected range.

**What needs fixing before the next batch:**
- The predicate lexicon is missing LEARNING and AUTOMATION. Until these are
  added, any prompt about skills libraries, retraining, or automation will route
  only to DOCUMENT sub-phases regardless of how many rows are taught.
- The branch count is too narrow for multi-domain prompts. "Documentation and
  skills from existing code" should produce 5 branches across 3 phases. It
  produced 2 branches in 1 phase. This is a lexicon gap, not a training gap.
- Sub-phase 7.5 (Learning Set and Model Feedback) cannot be reached by any
  prompt until LEARNING is added to the compiler.

**The next row target:**
Prompt: "I have some issue with my figma code it uses figmaBase64 of just
generated frame"

Expected predicates: IMPLEMENT (code issue), UI (figma, frame), DOCUMENT (implicit
skill output), and after the lexicon fix, LEARNING (skills improvement).

If after the lexicon fix this prompt still routes to only 1-2 branches in
Phase 2, the token-overlap term scores for the UI and IMPLEMENT sub-phases need
inspection — the sub-phase Purpose fields may not contain enough Figma-adjacent
vocabulary to score above the relevance floor.

---

# Last updated: 2026-05-05
