# XIIGen Internal Source Registry
# Version: 4.0 — 2026-05-08
# Supersedes: XIIGEN-INTERNAL-SOURCE-REGISTRY-v3.md

---

## What changed from v3

**Document type taxonomy expanded from 11 to 16 types.** v3 covered experiment
documents, artifact examination, decisions, teaching plans, and measurement
guidance. Five additional types were missing:

**Type 12 — Project history documents.** These contain documented mistakes and
their consequences. Each Part records a real decision and often records the earlier
wrong version. The superseded reasoning is a pre-made rejected for ARCH trade-off
pairs (A46 vs A50). The grounded historical claim is the chosen for CODE_Q pairs.

**Type 13 — Document state register.** Contains explicit thread blocking
relationships with Gate B commands per thread. The thread order is PLAN content.
Running blocked threads in parallel is the rejected. It also provides CODE_Q
document-authority pairs: the register names which version is current, making
a superseded version the ready-made rejected.

**Type 14 — Behavioral rules documents (What-To-Do-What-Not-To-Do).** Each
"What not to do" entry names an exact forbidden output pattern — that pattern
is the rejected. Plans without Gate B commands, Node 01 A-sides with raw strings,
pairs missing domain labels — all are pre-documented rejections. Version check
required: rules superseded in later versions must not be used as chosen.

**Type 15 — Session start prompt documents.** Contains pre-work gates (live test
count before any work) and post-work gates (manifest + verification before round
close). Gate-skipped behavior is the rejected. Correct gate behavior is the chosen.
Overlap 0.10–0.20: live-verification vocabulary vs memory-recall vocabulary.

**Type 16 — Versioned document evolution.** The sequence of versions of any
document produces superseded-vocabulary pairs. The old vocabulary (UserPrompt as
Node 01 input, 1-layer model, A46 DO NOT IMPLEMENT, Layer 1 planned) is the
rejected. The current vocabulary is the chosen. Lowest overlap ratio in the corpus
(0.05–0.15) because old and new contract names are entirely non-overlapping tokens.

**Three-check rule added (Type 16).** Before using any versioned document: (1) is
this the most recent version, (2) does the claim appear in a "changes from" section
of a newer version, (3) does the document have a "do not use this file" header.

**Quick-reference table extended to 16 rows** with key distinctions note: Types
12–16 all produce rejected from within their own corpus — no rejected needs to
be constructed externally. The wrong answer is already documented.

**Document type taxonomy expanded from 6 to 11 types.** v2 grouped all formal
XIIGen documents into a single "B-side authority" category. This masked important
differences: a decision document, a teaching plan, an experiment status file, an
artifact examination document, and a measurement guidance document each produce
different DPO pair content and require different rejected construction strategies.
v3 separates them.

**Experiment status and code review documents added (Type 7).** These documents
record which experiments are IMPLEMENTED / PARTIAL / NOT IMPLEMENTED in the codebase.
They are the canonical source for DPO pairs that teach the model to distinguish
"this capability exists" from "this capability was proven but not wired". This
distinction does not exist in any other document type.

**Artifact examination documents added (Type 8).** Round examination documents
record structural, contract, and judgment verification of produced artifacts. They
are the canonical source for ArbiterUnit pairs — they show what a real pass, concern,
and blocking finding looks like, with specific evidence rather than abstract criteria.

**Decision documents separated from B-side authority (Type 9).** The A-series
locked decisions (A1–A66 in DECISIONS-LOCKED.md) each contain a decision, a
rationale, and an alternative that was rejected. The rejected alternative is a
pre-made DPO rejected response for trade-off pairs. v2 listed DECISIONS-LOCKED.md
under Type 2 without extracting this structure.

**Teaching plan documents separated from B-side authority (Type 10).** Teaching
plans (ORCHESTRATOR-TEACHING-PLAN-v3.md) contain ordered sub-cycle steps with
explicit stopping rules and acceptance criteria. These are the ground truth for
PLAN dependency-ordering pairs. Their stopping rules are the ground truth for
arbiter pass/fail boundaries.

**Measurement guidance documents added (Type 11).** Documents like
TEACHING-MEASUREMENT-GUIDANCE-v4.md define what correct measurement output looks
like: which fields are required, what the acceptable ranges are, which formulas
to use. This is the canonical source for ModelTrainingUnit and HeldOutEvaluationUnit
B-side content, and for rejected responses that contain wrong metric fields or
wrong formulas.

**DPO pair construction rules added per type.** Each new type section includes:
the exact rejected pattern that produces positive margin for pairs from that type,
the overlap ratio range, and at least one worked example.

---

## Document type taxonomy

Every XIIGen internal document belongs to one of eleven types. The type determines
how it can be used in DPO pair construction. A document that does not fit any type
must be assessed against the exclusion rules before use.

---

### Type 1 — A-side authority: real developer prompts

**What it is:** Raw human messages from Claude export conversations or local session
files. Unedited. May contain typos, Hebrew, Russian, code fragments, partial sentences.

**Why it is A-side authority:** The model must learn to receive real developer input,
not idealised prompts. Only this type provides authentic input-language distribution.

**Examples in the XIIGen corpus:**
```
"fix the rxjs pipe in the Angular interceptor — I keep getting
 'object unsubscribed' after the first request"

"עזור לי עם המשך לחלק הבא — JOB שמחשב התאמות לזוגות"

"I don't want to use legacy peer I want to fix the problem
 npm install keeps failing with peer dependency conflict"

"Please generate flow 10 files that will include only these
 specific to flow 10 sections: P1Family 27 — Tena..."
```

**Eligibility test for PLAN and ARCH pairs** — at least one of:
- Named artifact: component name, phase number, flow ID, file name, technology name
- Dependency language: "before", "after", "depends on", "once X is complete", "first"
- Technical specificity: predicate vocabulary, sub-phase reference, stack name

**Messages that must never enter training as A sides:**
```
"Option A is clearly the right path"
"this is grounded in existing documents"
"let's continue with the plan"
"Continue."
"Read this."
"Please load the session."
"Based on the review and the response you've shared..."
"The plan is structured in five phases, each with a clear gate..."
"This is the right plan. It's grounded in the docs..."
```

These are meta-commentary. `CompiledPredicateIds` will be near-empty for them.
TrainingText built from these carries no routing signal. The calibration run
confirmed this: batches 30, 37 used meta-commentary rows from session 1235–1753
and produced mean accuracy 0.306–0.415.

**Overlap ratio when used as A side:** not applicable — A side is prompt input,
not paired content. The DPO signal comes from chosen/rejected token distributions
relative to this A side.

---

### Type 2 — B-side authority: formal XIIGen documents

**What it is:** Architecture decisions, session state documents, implementation
documentation, orchestrator teaching plans, dynamic flow design patterns,
and the canonical design pattern. Written in formal architecture vocabulary.

**Why it is B-side authority:** These documents define what the correct output looks
like. The sub-phase Purpose fields, dependency ordering rules, and arbiter criteria
in these documents are the ground truth for chosen responses.

**Examples in the XIIGen corpus:**
```
DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v3-5.md
SESSION-STATE-2026-05-07-v18.md
IMPLEMENTATION-DOCUMENTATION-v8.md
ORCHESTRATOR-TEACHING-PLAN-v3.md
DevelopmentLifecycleTopologySeeder.cs (sub-phase Purpose fields)
DECISIONS-LOCKED.md (A-series decisions)
```

**How to use for chosen response construction:**
- Sub-phase Purpose field → chosen content for 3.1, 3.2, 3.3 ARCH pairs
- Dependency ordering from teaching plan → chosen for PLAN Type B pairs
- Decision entries (A43–A66) → chosen for architectural trade-off pairs
- Locked decisions with reasoning → chosen for arbiter pass/fail criteria

**How NOT to use:** Never use as A side. Architecture document language produces
register mismatch: the model learns to expect formal vocabulary as input, which no
real developer ever provides.

**Overlap ratio when used as B side (chosen):** 0.10–0.20 against a Type 1 A side,
because the formal vocabulary of the document is largely non-overlapping with the
developer's surface words.

---

### Type 3 — Correction source: Claude export conversations

**What it is:** H0/A0/H1/A1 chains in export conversations where Claude produced a
wrong answer (A0) that the user corrected (H1) or where Claude revised its output
(A1). The correction pair is pre-made: A0 becomes rejected, A1 (or H1-derived) becomes
chosen, H0 (the original prompt) is the A side.

**Why this is the highest-value pair type:** The wrong answer (A0) came from a real
reasoning path, not from a constructed counter-example. The correct answer (A1) is
ground truth, not an approximation. The token distributions are naturally non-overlapping
because they came from different reasoning steps. Expected overlap ratio: below 0.25.

**The extraction pattern:**
```
Export conversation contains:
  H0  "design the data model for multi-tenant user preferences"
  A0  "Create a UserPreferencesRepository with GetByUserId(string userId)..."
  H1  "this is wrong — I need the entity shape, not the repository"
  A1  "Entity: UserPreference { tenantId (partition key), userId, key, value,
       updatedAt }. Partition key is tenantId to ensure tenant isolation at
       the storage layer."

Extracted pair:
  A:        H0 — "design the data model for multi-tenant user preferences"
  Chosen:   A1 — "Entity: UserPreference { tenantId (partition key)..."
  Rejected: A0 — "Create a UserPreferencesRepository with GetByUserId..."
  [WRONG: "design the data model" asks for entity shape and partitioning rules,
  not the repository class. The repository is 4.1 Backend Services work —
  it is built after the model is defined, not instead of it.]

  conversation_uuid: {uuid}
  human_message_index: 0
  quality_rating: High
```

**What makes an annotation strong (names three things):**
- The verb in H0 that signals the scope: "design" signals shape decisions, not code
- What the correct sub-phase actually owns: entity shape, partitioning rules
- Why rejected is in the wrong sub-phase: "repository is 4.1 work — built after, not instead"

**What makes an annotation weak (fails the margin test):**
```
[WRONG: this is the wrong sub-phase.]
[WRONG: rejected provides implementation instead of design.]
```

These add tokens but not distinguishing ones. The annotation must name the confusion
specifically enough that the tokens in the annotation do not also appear in rejected.

**Quality rating:**
```
High:   Chosen and rejected have clearly different vocabulary (overlap < 0.25)
        Include unconditionally.

Medium: Chosen and rejected address different responsibilities but share some
        vocabulary (overlap 0.25–0.50)
        Include with strong annotation.

Low:    Correction is only phrasing, not substance
        (chosen and rejected say the same thing differently, overlap > 0.50)
        Exclude.
```

---

### Type 4 — Measurement evidence: calibration run data

**What it is:** `dpo_step_*.json` files, `batch-summary.jsonl`, `progress.txt`,
checkpoint manifests from calibration runs.

**How to use:** Populate B sides for ModelTrainingUnit, HeldOutEvaluationUnit pairs.
The exact numbers (elapsed_ms, cpu_percent, working_set_delta_mb, checkpoint_bytes,
accuracy, loss) are the B-side content for teaching measurement pairs.

**How NOT to use:** Never derive A sides from run logs. Run log language is diagnostic
output, not developer input.

---

### Type 5 — Topology authority: seeder and registry code

**What it is:** `DevelopmentLifecycleTopologySeeder.cs`, `LogicalOrchestratorAssistant.cs`,
the 31 sub-phase registration blocks, and predicate lexicon definitions.

**How to use:** The `Name`, `Purpose`, and `PredicateIds` fields of each `LogicalSubPhase`
record are the canonical vocabulary for chosen responses in domain routing pairs and
DomainTranslatorUnit pairs. When writing a Type A rejected for sub-phase 3.1, the chosen
must use vocabulary from the 3.1 Purpose field. When writing a rejected, it must use
vocabulary from a different sub-phase's Purpose field.

**Sub-phase Purpose fields as vocabulary source for rejected construction:**
```
3.1 "component boundaries, dependency direction, lifecycle topology, responsibilities"
    → chosen vocabulary for ARCH boundary pairs

4.1 "service implementation, controller logic, dependency injection, framework calls"
    → rejected vocabulary when confused with 3.1

3.4 "authentication, authorisation, tenant isolation, JWT, RBAC, token validation"
    → rejected vocabulary when 3.1 boundary work is confused with security work

3.5 "metrics, traces, logs, alert thresholds, observability contracts"
    → rejected vocabulary when confused with 3.1 or 4.1
```

The rule: chosen uses Purpose vocabulary from the correct sub-phase. Rejected uses
Purpose vocabulary from the most plausible wrong sub-phase. Overlap ratio between
these two vocabulary sets is typically 0.10–0.25 for adjacent sub-phases.

---

### Type 6 — Excluded: document-loading and session-management messages

**What it is:** All prompt messages that load files, name documents, or manage
the session state without expressing a development intent.

**Examples:**
```
"Please load the session state."
"Read this document and confirm you understood it."
"Continue from where we stopped."
"What are the open threads?"
"Please update the document register."
```

**Why excluded:** These produce near-zero `CompiledPredicateIds`. The compiled text
carries no development signal. Any pair built from these A sides will have random
accuracy regardless of how well the rejected is constructed.

---

### Type 7 — Experiment status and code review documents

**What it is:** Documents that record which experiments are IMPLEMENTED, PARTIAL,
or NOT IMPLEMENTED in the actual codebase. Examples from the corpus:

```
EXPERIMENT-IMPLEMENTATION-STATUS-v1.md
EXPERIMENT-RESULTS-MAP.md
experiment_knn_state.json
```

These documents contain three things that no other type provides:

1. The experiment ID and proven result (e.g., E34V2 +8% hit@1)
2. The production class that was supposed to receive it (e.g., `AstRelevanceRanker.cs`)
3. The implementation gap — what exists vs what the experiment required

**How to use for DPO pairs:**

These documents are the canonical source for pairs that teach the difference between
"this capability works" and "this capability was proven but the production code does
not have it yet". This distinction is critical for the code review and architecture
review sub-phases (6.1, 6.4).

For CODE_Q pairs (how does X work):

```
Prompt:   "does the AstRelevanceRanker use BM25 and KNN fusion?"

Chosen (CODE_Q — accurate state):
  "AstRelevanceRanker has BM25 (E04) and KNN (E09) implemented separately.
   The rank fusion step — alpha × BM25_score + (1-alpha) × KNN_score with
   auto-tuned alpha — is not in the file. The +8% hit@1 gain from E34V2
   requires the fusion, but it is NOT IMPLEMENTED. The two signals exist
   but are never combined."

Rejected (CODE_Q — overstates implementation):
  "Yes — AstRelevanceRanker implements both BM25 and KNN retrieval and
   combines them using a weighted fusion formula. The alpha parameter
   is tuned automatically based on query type."
[WRONG: the experiment result exists but the fusion implementation does not.
AstRelevanceRanker.cs has no 'fusion', 'alpha', 'blend', or 'fuse' logic.
Reporting an unimplemented experiment as implemented is an accuracy failure
that would cause the orchestrator to believe a capability exists when it does not.]
```

For 6.1 architecture review pairs:

```
Prompt:   "review the retrieval path — is the BM25+KNN fusion from E34V2 in place?"

Chosen (6.1 — accurate review finding):
  "E34V2 rank fusion: NOT IMPLEMENTED. AstRelevanceRanker.cs contains BM25
   (via E04 implementation) and KNN (via E09). Zero hits for 'fusion', 'alpha',
   'blend', 'fuse'. The +8% hit@1 from E34V2 is not in production. This is
   the highest-impact retrieval gap in the current codebase."

Rejected (6.1 — review without checking code):
  "The retrieval path uses BM25 and KNN. Both experiments are implemented in
   AstRelevanceRanker. The path should be reviewed for performance under load
   and for correctness of the alpha parameter."
[WRONG: a review that claims implemented capabilities are present without
verifying them against the code is not a review — it is paraphrase of the
experiment list. The review must check EXPERIMENT-IMPLEMENTATION-STATUS against
the actual file. Reporting an unimplemented capability as present blocks the
team from knowing where the gap is.]
```

**Overlap ratio:** 0.10–0.25. Chosen uses status vocabulary (NOT IMPLEMENTED,
zero hits, no production class, gap). Rejected uses capability vocabulary (implements,
uses, combines). Near-zero overlap between these vocabularies.

**Pair types extractable:** CODE_Q (accurate state description), 6.1/6.4 review
findings, ArbiterUnit pairs (pass when status is correctly reported; concern when
status is wrong).

**Excluded use:** Never use as A side. Never use a status document to claim a
capability exists unless the status field is IMPLEMENTED and the evidence path
(the file/class) is confirmed present.

---

### Type 8 — Artifact examination documents

**What it is:** Documents that record the structured examination of produced
artifacts — session round artifacts, plan documents, flow files, training batches.
Examples from the corpus:

```
artifact_reviewing.md        — examination methodology for round artifacts
EXAMINATION-METHOD-ORCHESTRATION-v7.md / v8.md — pair examination standards
```

These documents describe three examination layers: structural verification
(complete?), contract verification (gate checks correct?), and judgment verification
(relationships correct, not just words?). Each layer maps directly to a different
arbiter verdict type.

**How to use for ArbiterUnit pairs:**

The examination layers define what distinguishes a pass from a concern from a
blocking finding. This is the exact content of ArbiterUnit B sides.

The key insight from `artifact_reviewing.md`: "Mechanical checking verifies that
the right words appear. Careful examination verifies that the right relationships
hold." This distinction is the difference between a pass and a concern verdict.

```
Contract: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>

A (arbiter review request for a PLAN artifact):
  artifact: a dependency plan where b08 appears in parallel group AND as
            dependency of b09
  review_criteria: "verify dependency ordering is semantically correct"

Chosen (ArbiterUnit B — concern verdict with specific finding):
  verdict: concern
  finding: "b08 appears in both the first parallel group and as a
            prerequisite of b09. This requires b08 to have two states:
            an early-start state (can run in parallel with group 1) and
            a resolved state (b09 depends on). The plan does not
            distinguish these states. b09 may start before b08 produces
            its result if the executor interprets 'parallel' as meaning
            'no ordering constraint'."
  recommendation: "split b08 into b08-start (parallel) and b08-resolved
                  (prerequisite of b09), or add an explicit wait-for-b08
                  gate between the parallel group and b09."

Rejected (ArbiterUnit B — pass verdict without examining relationships):
  verdict: pass
  finding: "b08 is present in the plan and is listed as a dependency of b09."
[WRONG: the arbiter found the right word ('b08') but did not check the
relationship. A plan where b08 is both in the parallel group and a
dependency of b09 has a structural ambiguity that the word presence
check cannot detect. The correct verdict is concern, not pass — the
relationship between b08's parallel execution and b09's dependency on
b08's result is not resolved by listing both facts.]
```

**Second pair type from this source — wrong verdict type:**

The examination document distinguishes `concern` (noted gap, round can be accepted
with flag) from `reprocess` (structural defect, must be corrected before acceptance).
These are the hardest pairs to construct correctly because both verdicts indicate
a problem — the rejected is a plausible but wrong severity classification.

```
A: artifact with duplicate route registration causing wrong handler execution

Chosen (ArbiterUnit — reprocess: structural defect):
  verdict: reprocess
  finding: "the wildcard route is registered before the API route. Every
            request is intercepted by the wildcard handler before reaching
            the API endpoint. Current output is wrong."
  requirement: "fix the registration order before this result can be accepted"

Rejected (ArbiterUnit — concern: wrong severity, should be reprocess):
  verdict: concern
  finding: "route registration order may affect which handler receives
            requests in some edge cases. Worth reviewing."
[WRONG: when the output is currently wrong — the wildcard intercepts every
request — the correct verdict is reprocess, not concern. A 'concern' verdict
allows the round to be accepted with a note. 'reprocess' requires correction.
Downgrading a defect that produces wrong output to a concern leaves the wrong
behavior in the next round's starting state.]
```

**Overlap ratio:** 0.15–0.30. Chosen uses verdict-type vocabulary (reprocess,
structural defect, currently wrong, must be corrected). Rejected uses softer
vocabulary (concern, edge case, worth reviewing). Moderate vocabulary difference.

**Pair types extractable:** ArbiterUnit B sides for all verdict types, Node 08
pass/concern/reprocess decisions, structural verification pairs.

**Excluded use:** Never use examination methodology text as A side. It is authority
evidence for what the correct B looks like.

---

### Type 9 — Decision documents

**What it is:** The DECISIONS-LOCKED.md file and any document containing locked
A-series or D-series decisions. Each entry has: the decision made, the rationale,
and implicitly or explicitly — the alternative that was rejected.

```
DECISIONS-LOCKED.md (A1–A66, D01–D15)
```

**Why this is a distinct type:** Every locked decision is a pre-made trade-off pair.
The decision (A-series) is the chosen. The alternative that was not taken is the
rejected. The rationale explains why the alternative fails — this becomes the
annotation.

**How to extract DPO pairs from decision entries:**

```
Decision entry:
  A61: DomainClassifierUnit in PromptDomainClassifier must be pre-seeded at
       static construction. A new() instance has _trained=false permanently,
       making the proven E18 +15% KNN path unreachable.

Extracted pair:
  A (ARCH trade-off prompt):
    "should DomainClassifierUnit be seeded at static init or lazily
     when the first prompt arrives?"

  Chosen (ARCH — the locked decision):
    "Pre-seed at static construction via BuildSeededClassifier(). Seed with
     five canonical domain predicate profiles — one per D-04 domain. This
     ensures _trained=true on the first call. The E18 +15% KNN path requires
     _trained=true; it is permanently unreachable if the object is
     constructed with new() and Train() is never called."

  Rejected (ARCH — the alternative not taken):
    "Lazy seeding: construct with new() and seed on first prompt arrival.
     This avoids startup cost and allows the seed data to come from real
     runtime prompts rather than hardcoded predicate profiles."
  [WRONG: lazy seeding means _trained=false for all prompts until the first
  runtime prompt triggers the seed. Every prompt before that point falls to
  ClassifyByKeywords — the static keyword scan. Any startup sequence that
  includes domain classification before the first real prompt (e.g., health
  checks, warm-up calls) would produce wrong domain labels that get cached.
  The E18 gain requires the KNN path to be available on the first call,
  not eventually.]
```

**Second pair type — D-series decisions for implementation rules:**

D-series decisions define rules like "no external ML frameworks" (D-11), "replay
buffer 70/30" (D-06). These are IMPLEMENT/TEST pairs: chosen follows the rule,
rejected violates it.

```
A: "implement the capability trainer's replay mechanism"

Chosen (IMPLEMENT — follows D-06):
  "Use the 70/30 split from project 16's ReplayBuffer: 70% current training
   examples, 30% replay from prior manifests. Cap at DefaultMaxCapacity=4096
   entries. Call ClearCache() at batch boundary to release memory."

Rejected (IMPLEMENT — violates D-06):
  "Add ML.NET's memory-efficient replay implementation — it handles the 70/30
   split automatically and supports dynamic capacity adjustment based on
   available RAM."
[WRONG: D-11 prohibits external ML frameworks. ML.NET is an external
ML framework. The replay mechanism must be implemented using project 16's
existing ReplayBuffer.cs which is already proven in production. Using ML.NET
would require adding a dependency that the project explicitly prohibits.]
```

**Overlap ratio:** 0.10–0.20 for architecture trade-offs (chosen uses acceptance
vocabulary, rejected uses alternative vocabulary). 0.05–0.15 for rule violations
(chosen uses rule-compliant vocabulary, rejected uses the prohibited framework/pattern
name).

**Pair types extractable:** ARCH trade-off pairs, IMPLEMENT rule-following pairs,
ArbiterUnit pairs (D-rule violation = reject verdict).

**Excluded use:** Never invent a decision not in the document. The alternative
must be derivable from the decision rationale or from prior session history showing
what was attempted before the decision was made.

---

### Type 10 — Teaching plan documents

**What it is:** Step-by-step plans for teaching the orchestrator, with explicit
ordering constraints, stopping rules, and acceptance criteria. Examples:

```
ORCHESTRATOR-TEACHING-PLAN-v3.md
XIIGEN-DRIVEN-SELF-TEACHING-GUIDANCE-v2.md
```

These documents differ from architecture documents (Type 2) in one critical way:
they contain **stopping rules** — specific conditions that must be met before
proceeding to the next step. Stopping rules are the ground truth for ArbiterUnit
pass/fail/reprocess pairs and for PLAN dependency-ordering pairs.

**How to use for PLAN dependency-ordering pairs:**

Teaching plans make dependency ordering explicit. The sub-cycle from
`ORCHESTRATOR-TEACHING-PLAN-v3.md` states:

```
Step 1 — Harness wiring (learned-unit selector per role) — must complete before Step 2
Step 2 — Tracing wrappers for verification run — must complete before Step 3
Step 3 — LoRA adapter inclusion in forward path — must complete before Step 4
Step 4 — Checkpoint field split measurement — must complete before first row
Step 5 — Teach one row, measure, decide — repeat
```

This ordering is PLAN content. The rejected for a PLAN pair from this source
puts implementation before harness:

```
A: "plan the first teaching cycle for the domain classifier unit"

Chosen (PLAN — teaching plan order):
  Branch 1 — harness wiring (dependency order 1):
    Write a learned-unit selector for the DomainClassifier role following
    the SecurityVerification34FlowFactory pattern. Register it in the flow
    registry for each sub-phase being taught. This must be complete before
    any teaching row runs — without it, a Probationary unit is registered
    but never called by the verification run.

  Branch 2 — tracing wrappers (dependency order 2):
    Wrap the orchestrator flows with TracingFlowDecorator before the
    verification run. Collect StationTrace objects via the emitTrace callback.
    Dependency: the selector from branch 1 must exist — tracing wraps calls
    that the selector enables.

  Branch 3 — teach row 1 (dependency order 3):
    Run one DPO step on the first prepared pair. Measure all three layers
    (resources, routing, verdict). Accept only if LoraAdapterBytes < 2MB.
    Dependency: branches 1 and 2 must be complete — the harness must exist
    before any row is taught.

Rejected (PLAN — wrong order, teaching before harness):
  Branch 1 — teach row 1
  Branch 2 — write the learned-unit selector
  Branch 3 — add tracing wrappers
[WRONG: teaching row 1 without the learned-unit selector means the
Probationary unit is registered but never called. The verification run
uses the deterministic unit, not the taught unit. The measurement shows
the deterministic unit's behavior, not the trained unit's. The selector
must exist before teaching begins — it cannot be added retroactively to
an already-taught row's measurement evidence.]
```

**How to use for ArbiterUnit stopping-rule pairs:**

Teaching plans contain explicit stopping rules. These are arbiter pass criteria:

```
Teaching plan stopping rule:
  "After row 10: pause, review the ledger. If fewer than 7 of 10 decisions
   are 'promoted', the training data for that component has a systematic
   problem. Stop. Do not proceed to row 11."

ArbiterUnit pair:
  A: teaching ledger showing 5 of 10 decisions promoted, 3 held, 2 rejected

  Chosen (ArbiterUnit — reprocess: stopping rule triggered):
    verdict: reprocess
    finding: "5 of 10 decisions are promoted — below the stopping threshold
              of 7 of 10. The training data for this component has a
              systematic problem. Three consecutive rejected decisions
              (rows 7, 8, 9) indicate the same pattern failure repeating.
              Teaching must stop at row 10."
    requirement: "identify the systematic problem in the training data before
                 proceeding to row 11"

  Rejected (ArbiterUnit — pass: ignores stopping rule):
    verdict: pass
    finding: "50% promotion rate is within acceptable range for early-stage
              training. Continue to row 11 to gather more data."
  [WRONG: the stopping rule is explicit — fewer than 7 of 10 promoted triggers
  a stop. 5 of 10 is below the threshold regardless of whether it "seems
  acceptable". Continuing past the stopping rule without identifying the
  systematic problem accumulates wrong behavior on top of wrong behavior.
  The rule exists precisely to prevent this.]
```

**Overlap ratio:** 0.15–0.30 for PLAN dependency pairs (chosen uses prerequisite
vocabulary, rejected uses same steps in wrong order). 0.10–0.20 for arbiter
stopping-rule pairs (chosen uses stopping vocabulary, rejected uses continuation
vocabulary).

**Pair types extractable:** PLAN ordering pairs (harness before teaching),
ArbiterUnit stopping-rule pairs, CorrectionLearningUnit boundary pairs.

**Excluded use:** Never use step descriptions as A sides. The prompt must be a
developer intent request, not a plan step narration.

---

### Type 11 — Measurement guidance documents

**What it is:** Documents that define required measurement fields, acceptable
ranges, and correct formulas. Examples:

```
TEACHING-MEASUREMENT-GUIDANCE-v4.md
```

These documents specify exactly which fields a teaching row record must contain,
what the acceptable ranges are, and what happens when ranges are violated. This
is the ground truth for ModelTrainingUnit, HeldOutEvaluationUnit, and
CorrectionLearningUnit B-side content — and for rejected responses that use
wrong fields or wrong formulas.

**The three measurement layers from TEACHING-MEASUREMENT-GUIDANCE-v4.md:**

```
Layer 1 — Resources (must appear in every B side):
  inference_time_ms, teaching_time_ms, cpu_percent, working_set_delta_mb,
  managed_allocation_mb, disk_checkpoint_delta_bytes, branch_count,
  registered_flow_count, registered_unit_count

Layer 2 — Routing (must appear in every B side):
  compiled_predicate_ids, routed_sub_phases, routing_confidence,
  branch_count, branch_justification

Layer 3 — Verdict (must appear in every B side):
  pre_teaching_verdict, teaching_result, post_teaching_verdict,
  knowledge_edge_count_before, knowledge_edge_count_after,
  cumulative_registry_improved
```

**How to use for ModelTrainingUnit pairs:**

```
A (ModelTrainingUnit input):
  feature_vector_dataset: 93 rows, domain BUG, batch 3/63
  training_settings: { max_steps: 1, lora_rank: 2, lora_alpha: 4 }

Chosen (ModelTrainingUnit B — all three layers present):
  model_artifact_path: "...batch-0003-bug/bug/3-2/policy.bin"
  training_row_count: 93
  training_accuracy: 0.2751
  held_out_accuracy: null  (not applicable for single-step)
  teaching_time_ms: 66485.9
  cpu_percent: 13.9
  working_set_delta_mb: 12.8
  managed_allocation_delta_mb: 71633.2
  checkpoint_bytes_delta: 1531426
  lora_adapter_bytes: 4096    (rank=2, computed from adapter tensors)
  dpo_loss: 0.6945
  chosen_reward: 0.01569
  rejected_reward: 0.01838
  reward_margin: -0.00269     (negative — model assigned higher prob to rejected)
  warnings: ["negative_reward_margin", "accuracy_below_threshold"]

Rejected (ModelTrainingUnit B — missing Layer 1 resource fields):
  model_artifact_path: "...batch-0003-bug/bug/3-2/policy.bin"
  training_row_count: 93
  training_accuracy: 0.2751
  dpo_loss: 0.6945
[WRONG: a training result that omits resource measurements (cpu_percent,
working_set_delta_mb, managed_allocation_delta_mb, checkpoint_bytes_delta)
cannot be used to assess whether the training step is within operational
bounds. TEACHING-MEASUREMENT-GUIDANCE-v4.md requires all three layers.
A result with only accuracy and loss is insufficient — it provides no
signal about memory pressure, disk growth, or convergence health.]
```

**How to use for wrong-formula rejected:**

```
A: "compute cpu_percent for the training step"

Chosen (correct formula from guidance):
  "cpu_percent = (total_cpu_time_ms / elapsed_ms / Environment.ProcessorCount) × 100
   For a 4-core machine with elapsed=96214ms and cpu_time=53344ms:
   cpu_percent = (53344 / 96214 / 4) × 100 = 13.9%"

Rejected (wrong formula — missing processor count division):
  "cpu_percent = (total_cpu_time_ms / elapsed_ms) × 100
   = (53344 / 96214) × 100 = 55.4%"
[WRONG: the formula must divide by Environment.ProcessorCount to stay within
0-100% on multi-core machines. Without the division, a 4-core machine reports
cpu_percent > 100% on any batch that uses more than one core. This was fixed
in TEACHING-MEASUREMENT-GUIDANCE-v4.md (P2-7). The wrong formula produces a
number that appears to show a resource problem where none exists.]
```

**How to use for wrong-field rejected:**

```
A: "record the model artifact size fields for a teaching step"

Chosen (correct fields from guidance):
  base_checkpoint_bytes: new FileInfo(checkpointPath).Length
  lora_adapter_bytes: sum of (Rank × InDim + OutDim × Rank) × 4 per LoraLinear
  dpo_metric_bytes: sum of dpo_step_*.json file lengths in checkpoint directory

Rejected (wrong: single combined field instead of split fields):
  model_artifact_bytes: new FileInfo(checkpointPath).Length
[WRONG: ModelArtifactBytes was split into three fields in v4 (P2-6).
Using the combined field loses the ability to check the LoRA adapter size
against the 2MB stopping rule (D-05). LoraAdapterBytes is what the stopping
rule tests. A combined field that mixes base weights with adapter weights
cannot be compared against the adapter-only threshold.]
```

**Overlap ratio:** 0.10–0.20. Chosen uses required-field vocabulary and correct
formula terms. Rejected either omits fields entirely or uses wrong formula terms.
Strong distributional difference.

**Pair types extractable:** ModelTrainingUnit B sides, HeldOutEvaluationUnit B sides,
CorrectionLearningUnit B sides, ArbiterUnit stopping-rule pairs for resource bounds.

**Excluded use:** Never use guidance text as A side. Never use guidance field
definitions as developer-language prompts.

---

### Type 12 — Project history documents

**What it is:** Narrative records of what was built, why decisions were made, and
what failed at each stage. Examples from the corpus:

```
PROJECT-HISTORY-v1.md through PROJECT-HISTORY-v6.md
```

These documents contain something no other type has: **documented mistakes and
their consequences**. Each Part records a real decision, often records the earlier
wrong version, and explains what had to be revised. This is direct ground truth
for pairs that teach the distinction between a decision that held and one that
was superseded.

**Structural feature that matters for DPO:** Project History documents use Part
numbering (Part 1, Part 2, ... Part 16). Each Part is a temporal event with a
date. Multiple Parts may address the same topic as understanding evolved. The
later Parts contain revisions of earlier claims.

**How to use for CODE_Q pairs — accurate historical claim:**

```
Prompt:   "why did the project choose to build TensorCore from scratch
           instead of using TorchSharp?"

Chosen (CODE_Q — accurate with source grounding):
  "TorchSharp wraps a closed binary — you can call its API but cannot read
   or change the internal code. The project's goal is to understand every
   algorithm from the inside, which requires reading and modifying the
   implementation. This was recorded in Part 2 of the project history as
   the origin of the 'no external ML frameworks' constraint, later locked
   as D-11."

Rejected (CODE_Q — plausible but invented motivation):
  "TorchSharp was too slow for the performance targets. The team wanted
   full control over SIMD optimizations and memory layout to hit the
   latency requirements for real-time orchestration."
[WRONG: the reason is not performance — it is inspectability. TorchSharp
wraps a closed binary; you cannot read the implementation. The performance
motivation was never stated in any project document. A CODE_Q answer that
invents a motivation not present in the source teaches the model to
hallucinate project history. The correct answer is grounded in Part 2.]
```

**How to use for ARCH pairs — superseded decision as rejected:**

Project History documents record decisions that were made and then changed.
A46 (E24 DO NOT IMPLEMENT) was superseded by A50 (E24 rehabilitated). The
A46 reasoning is the rejected; the A50 reasoning is the chosen.

```
Prompt:   "should AttributedSegmentRouter (E24) be implemented given
           current E21 boundary precision?"

Chosen (ARCH — A50, current decision):
  "E24 is safe to implement with the per-segment-per-domain contract.
   The original A46 failure was caused by a data structure issue in the
   old design — structurally impossible with the current implementation
   where each segment is routed to its own domain separately. E24 can
   be added after E21 is verified in production."

Rejected (ARCH — A46, superseded):
  "E24 must NOT be implemented until E21 boundary precision is verified.
   E24 assumes E21 produces correct segment boundaries — if E21 is wrong,
   E24 amplifies the error. Wait until E21 is confirmed stable."
[WRONG: A46 was superseded by A50 in Part 16 Addendum. The original
-37% noise increase came from a data structure problem in the old design,
not from the algorithm itself. The current per-segment-per-domain design
makes the original failure mode structurally impossible. Applying A46
reasoning to the current design is applying a constraint built for a
system that no longer exists.]
```

**Overlap ratio:** 0.15–0.30 for CODE_Q historical accuracy (chosen cites source,
rejected invents). 0.10–0.20 for superseded-decision pairs (chosen uses current
reasoning vocabulary, rejected uses superseded reasoning vocabulary).

**Pair types extractable:** CODE_Q historical accuracy, ARCH decision-revision
pairs, ArbiterUnit pairs for claims about project history.

**Excluded use:** Never use Part narrative text as A side. Never cite Part numbers
in A sides — real developer prompts do not reference project history parts.

---

### Type 13 — Document state register

**What it is:** The authoritative index of which documents are current, which are
superseded, and what each open thread requires. Examples:

```
DOCUMENT-STATE-REGISTER-2026-05-04-v2.md through
DOCUMENT-STATE-REGISTER-2026-05-06-v8.md
```

These documents contain **explicit thread blocking relationships**: Thread 1 is
highest priority; Thread 2 is blocked until Thread 1 is complete; Thread 3 is
blocked until Thread 2 is complete. Each thread has Gate B bash commands.

**How to use for PLAN dependency-ordering pairs:**

```
Prompt:   "plan the work for the current open threads — XIIGen export
           teaching, experiment wiring, and orchestration scenarios"

Chosen (PLAN — register order, blocking preserved):
  Branch 1 — XIIGen export teaching (order 1, unblocked):
    Produce pair rows from the 1,080 usable rows in the XIIGen export.
    No dependencies — can start immediately.

  Branch 2 — experiment wiring (order 2):
    Wire all 27 positive experiments into project 21.
    Dependency: Branch 1 must be complete. Wiring without teaching data
    produces a wired model with no training signal.

  Branch 3 — orchestration scenarios O1-O5 (order 3):
    Run the five scenario proofs end to end.
    Dependency: Branches 1 and 2 must both be complete. Scenarios require
    both teaching data and wired experiments.

Rejected (PLAN — parallel, ignores blocking):
  All three threads can run in parallel — start everything simultaneously.
[WRONG: DOCUMENT-STATE-REGISTER-v8 marks experiment wiring as explicitly
blocked until teaching is complete, and scenarios as blocked until both
teaching and wiring are complete. The blocking relationships reflect real
pipeline dependencies — running in parallel means wiring proceeds without
training signal and scenarios run against an unwired model.]
```

**How to use for CODE_Q document authority pairs:**

```
Prompt:   "which session state document is current?"

Chosen:   "SESSION-STATE-2026-05-06-v15.md — recorded in the register
           as superseding v13, covering workstreams 6/7, decisions A47-A51."

Rejected: "SESSION-STATE-2026-05-06-v13.md"
[WRONG: v13 is superseded. Decisions A47-A51 (including A50 rehabilitating
E24) do not exist in v13. Using v13 makes those decisions invisible.]
```

**Overlap ratio:** 0.10–0.25 for thread ordering; 0.05–0.15 for document version
pairs (superseded vs current version names are entirely non-overlapping tokens).

**Pair types extractable:** PLAN thread-ordering pairs, CODE_Q document-authority
pairs, ArbiterUnit pairs for document version claims.

**Excluded use:** Never invent blocking relationships not in the register. Never
use Gate B bash commands as A sides.

---

### Type 14 — Behavioral rules documents

**What it is:** The operational rules governing valid plans, forbidden output
patterns, and required behaviors. Examples:

```
WHAT-TO-DO-WHAT-NOT-TO-DO-v2.md through WHAT-TO-DO-WHAT-NOT-TO-DO-v12.md
```

These documents are a direct source of two DPO pair types no other type provides:

1. **Forbidden output as rejected** — each "What not to do" rule names an exact
   output pattern that must not appear. That pattern is the rejected. The
   compliant output is the chosen.

2. **Rule violations that look correct** — the hardest pairs come from rules that
   are non-obvious. A plan without Gate B commands looks complete. A Node 01 A-side
   with a raw string looks correct to a reader unfamiliar with the v2 contract.

**How to use — forbidden output as rejected:**

```
Prompt:   "write the A-side for a Node 01 pair about implementing JWT auth"

Chosen (v2 contract — current):
  A:
    transition_type: feature_addition
    selected_flow_id: "security-verification-34-flow"
    human_context: "Add JWT authentication to the API with tenant isolation"
    confidence: 0.87

Rejected (v1 contract — forbidden in v2+):
  A: "Add JWT authentication to the API with tenant isolation"
[WRONG: WHAT-TO-DO requires Node 01 A-side to be ILogicalUnit<SelectedLogicalTransition,
DomainPacket[]>. A raw string is the v1 contract, explicitly superseded in v2.
The canonical design pattern header says "Do not use this v1 file for pair writing."]
```

**How to use — plan missing Gate B:**

```
Prompt:   "write a plan for adding CompiledDomainClassifierUnit"

Chosen (plan with Gate B):
  Phase 1 — create wrapper:
    ...implementation description...
    Gate B:
      dotnet build LlmFromScratch.sln 2>&1 | tail -3
      # Expected: Build succeeded, 0 errors
      dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
      # Expected: floor or higher, 0 failed

Rejected (plan without executable Gate B):
  Phase 1 — create wrapper:
    ...implementation description...
    Verify: build succeeds and tests pass.
[WRONG: WHAT-TO-DO requires Gate B to contain exact bash commands, not a
prose instruction. A plan without executable Gate B cannot be dispatched to
an executor — the executor has no command to run. "Verify tests pass" is
indistinguishable from a plan that was never verified.]
```

**Version check before using a rule:** Check that the rule appears in the most
recent version. A46 (E24 DO NOT IMPLEMENT) from v10 was superseded in v11 by A50.
Pairs built from superseded rules train the model on invalidated constraints.

**Overlap ratio:** 0.05–0.15 for contract pairs (old type name vs new type name —
entirely different tokens). 0.15–0.25 for Gate B presence pairs (compliant vs
incomplete structure).

**Pair types extractable:** PLAN Gate-B-present pairs, contract-compliance pairs,
ArbiterUnit rule-violation pairs.

**Excluded use:** Never use a rule from a version where it has been superseded.
Never use rule text as A side.

---

### Type 15 — Session start prompt documents

**What it is:** Operational checklists governing how sessions open and how rounds
close. Examples:

```
SESSION-START-PROMPT-v2.1.md through SESSION-START-PROMPT-v9.md
SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v1.md through v6.md
```

These contain **pre-work gates** (must pass before work begins) and **post-work
gates** (must complete before round closes). Both are DPO pair sources: correct
session behavior is the chosen, skipped gate is the rejected.

**How to use — live test count gate:**

```
Prompt:   "start the session — workstream: extend PLAN pair coverage"

Chosen (correct — live count):
  Session type: ARCHITECT
  Baseline: [run dotnet test live] — 1224 passing, 0 failing — confirmed
  Open threads: Thread 1 (XIIGen export teaching) highest priority
  Intended round boundary: extend PLAN pairs from rows 521-727

Rejected (skipped gate — count from memory):
  Session type: ARCHITECT
  Test count: 1224 (from last session)
  Starting PLAN pair work
[WRONG: SESSION-START-PROMPT requires live test count — never from memory.
"From last session" is the exact failure pattern the rule was written to
prevent. A regression introduced between sessions is invisible without a
live run.]
```

**How to use — round close gate:**

```
Prompt:   "close the round — 12 PLAN pairs extracted"

Chosen (correct close — manifest + verification):
  Files changed: plan-pairs-round-03.jsonl (new)
  Verification: overlap_ratio < 0.35 for all 12 pairs ✓
  Manifest written: manifest-round-03.md (what changed / why / how verified)
  Zip: round-03.zip containing pair file + manifest + session start prompt

Rejected (incomplete close — no manifest):
  12 pairs written. Zip: round-03.zip
[WRONG: SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING requires a manifest
naming what changed, why, and how verified. A zip without a manifest cannot
be evaluated by a subsequent session. Verification before close is also
required — an unverified batch may contain negative-margin pairs.]
```

**Overlap ratio:** 0.10–0.20 for gate-present vs gate-skipped pairs (live
verification vocabulary vs memory-recall vocabulary).

**Pair types extractable:** Session management pairs, round closure pairs,
ArbiterUnit pairs (missing mandatory gate = reprocess).

**Excluded use:** Never use checklist items as A sides. A sides must be human
instructions ("start the session"), not checklist lines.

---

### Type 16 — Versioned document evolution

**What it is:** The relationship between successive versions of the same document.
This applies to documents of any type. Examples:

```
DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v1 → v2 → v3 → v3-4
SESSION-STATE-v2 → v3 → ... → v15
WHAT-TO-DO-WHAT-NOT-TO-DO-v2 → v3 → ... → v12
IMPLEMENTATION-DOCUMENTATION-v1 → v2 → ... → v8
```

**Why this is a distinct type:** Version evolution produces the **superseded-
vocabulary pair** — a pair class no single document version provides. The old
vocabulary is the rejected. The new vocabulary is the chosen. The revision
documents why the change was made.

**Extraction rule:** Every versioned document has a "changes from vN" section.
That section names superseded claims. Each superseded claim → one rejected.

**Key superseded-vocabulary patterns in the corpus:**

```
v1 → v2 of canonical design pattern:
  Superseded: "Node 01 A-side = UserPrompt (raw string)"
  Current:    "Node 01 A-side = SelectedLogicalTransition"
  Pair: prompt about Node 01 input type → chosen=SelectedLogicalTransition,
        rejected=UserPrompt

v1 → v8 of implementation documentation:
  Superseded: "1-layer model, hidden dim 16" (pre-A58)
  Current:    "4-layer model, hidden dim 32" (A58)
  Pair: prompt about model config → chosen=4L/32 dim, rejected=1L/16 dim

  Superseded: "inference uses raw prompt for KNN" (pre-A59)
  Current:    "inference compiles prompt before KNN" (A59)
  Pair: prompt about inference path → chosen=compiled, rejected=raw

v10 → v11 of behavioral rules:
  Superseded: "E24 DO NOT IMPLEMENT (A46)"
  Current:    "E24 rehabilitated (A50)"
  Pair: prompt about E24 → chosen=safe with per-segment contract,
        rejected=wait until E21 verified

v13 → v15 of session state:
  Superseded: "Layer 1 transition orchestrator planned"
  Current:    "Layer 1 locked out (A47)"
  Pair: prompt about Layer 1 → chosen=locked out, rejected=planned
```

**The three-check rule before using any versioned document:**

1. Is this the most recent version? Check document state register.
2. Does the claim appear in a "changes from" section of a newer version?
   If yes, it is superseded — use the newer version's vocabulary for chosen.
3. Does the document have a "do not use this file" header? (v1 of the canonical
   design pattern has this.) If yes, it may only be used as rejected source.

**Overlap ratio:** 0.05–0.15. Old and new contract vocabulary are often entirely
non-overlapping (UserPrompt vs SelectedLogicalTransition; 1-layer vs 4-layer).

**Pair types extractable:** CODE_Q contract accuracy, ARCH decision-revision,
ArbiterUnit outdated-contract pairs.

**Excluded use:** Never use superseded vocabulary as chosen. Check the three-check
rule before every pair. A pair where chosen = superseded and rejected = current
is an inverted pair that teaches the wrong answer.

---

## Rejected construction rules by domain

The calibration run (2026-05-06) confirmed that 75% of batches had negative reward
margin because rejected was either a negation template or another correct sub-phase
response for the same prompt. Neither produces positive margin.

This section defines how to construct rejected for each domain using the document type
taxonomy above.

---

### BUG domain — rejected construction

BUG domain mean accuracy in the calibration run: 0.417. Root cause: multiple sub-phases
received the same prompt as chosen. The model saw "fix the rxjs pipe" routed to both
2.1 and 4.3 within the same training set and could not learn the distinction.

**Rule:** For any BUG A side, rejected must address a different responsibility class,
not a different sub-phase that the same prompt also legitimately owns.

**Type A rejected for BUG — wrong responsibility class:**

A BUG prompt that asks to fix a runtime error has a correct sub-phase (e.g., 4.1 Backend
Services). The rejected must address a completely different responsibility:

```
Prompt:   "wildcard fallback makes it skip azure ai search — please fix"
Domain:   BUG
Sub-phase chosen: 4.1 Backend Services
  "The wildcard route handler intercepts requests before the Azure AI Search
   route is matched. Fix: register the Azure AI Search route before the
   wildcard. In app.module.ts, move AzureAiSearchController registration
   above the fallback route. The router matches in registration order."

Sub-phase rejected: 3.3 API Contract Definition
  "Define the Azure AI Search endpoint contract: POST /api/search with
   request body { query: string, filters: object }. Document the response
   shape including result array, total count, and pagination tokens."
[WRONG: defining the API contract describes what the endpoint should accept
and return — it does not fix the routing registration order that causes
requests to be intercepted by the wrong handler. Contract definition is
3.3 work; fixing the wrong handler is 4.1 work.]
```

**Why this produces positive margin:** The chosen uses implementation vocabulary
(route handler, registration order, module, controller). The rejected uses contract
vocabulary (endpoint, request body, response shape, pagination). Near-zero overlap.

**Type A rejected for BUG — wrong diagnostic scope:**

```
Prompt:   "I don't want to use legacy peer — fix the npm install conflict"
Chosen:   "Add --legacy-peer-deps to .npmrc or switch the conflicting
           dependency to a compatible version. Check npm ls for the full
           dependency chain: the conflict is between..."
Rejected: "Design the dependency management policy: define which package
           versions the project accepts, document the rationale for pinning
           versus ranges, and establish a review gate for major version
           upgrades."
[WRONG: npm install is failing now — the immediate fix is a command or
config change, not a policy document. Policy design (3.1 / 2.2) follows
working dependencies; it does not unblock the broken install.]
```

**Hard negative for BUG — intra-domain confusion between BUG and REFACTOR:**

These are the hardest cases. The signal words overlap ("fix", "clean", "wrong"):

```
Prompt:   "the auth middleware runs twice on protected routes — fix it"
Chosen (BUG — 4.1):
  "The middleware is registered globally in app.module.ts AND individually
   on the route decorator. Remove the route-level @UseGuards() — the global
   registration handles all routes already."

Rejected (REFACTOR — 4.4 — plausible but wrong domain):
  "Extract the auth middleware into a dedicated AuthInterceptor class.
   Move the token validation logic out of the route handler and into
   the interceptor. Rename the existing middleware to reflect its actual
   scope."
[WRONG: the symptom is double execution from duplicate registration — not
disorganised code. Refactoring the interceptor class does not fix the
duplicate run. The fix is removing one registration. Code organisation
is a separate concern that can be addressed after the bug is resolved.]
```

The annotation here is critical: it names why REFACTOR vocabulary (extract, move,
rename, reorganise) does not fix the BUG (duplicate registration). Without the
annotation, overlap ratio is ~0.35 — borderline. With the annotation, the additional
tokens in chosen (duplicate, registration, removes, global) push the distributional
separation to produce reliable positive margin.

---

### CODE_Q domain — rejected construction

CODE_Q calibration result: 0% positive margin batches. All four batches had
overlap_ratio ~0.70 from negation template. The rejected must be a wrong domain
classification, not a non-answer.

**The core rule for CODE_Q:** A CODE_Q prompt asks "what does X do" or "how does Y
work". The rejected must be an action response — either a BUG fix or a PLAN step —
not another explanation.

**Type C rejected for CODE_Q — wrong domain (explanation vs action):**

```
Prompt:   "how does the replay buffer work in the capability trainer"
Chosen (CODE_Q — explain):
  "The replay buffer stores manifests from previous training steps for this
   domain adapter. On each training call, the trainer draws up to 3 manifests
   from the buffer (FindReplayManifests) and includes them alongside the new
   example. This prevents catastrophic forgetting: the model sees recent past
   examples on every step, not only the current one. Buffer growth is bounded
   at 4096 entries per DefaultMaxCapacity."

Rejected (BUG — wrong domain):
  "The replay buffer is growing without bound because MaxCapacity is not
   being enforced. Add a cap: after each Append(), check buffer.Count >
   _maxCapacity and call RemoveRange(0, buffer.Count - _maxCapacity).
   This prevents RAM growth across long calibration runs."
[WRONG: the prompt asks how the replay buffer works — what its mechanism
is and what problem it solves. The fix for unbounded growth is A54-level
work, not an answer to "how does this work". Answering a how-does-it-work
question with a bug fix addresses a different intent entirely.]
```

**Why this works:** Chosen uses explanation vocabulary (stores, draws, prevents,
catastrophic forgetting, bounded). Rejected uses fix vocabulary (growing without bound,
add a cap, prevents RAM growth). Overlap ratio: typically 0.15–0.25.

**Intra-domain confusion for CODE_Q — explanation vs demonstration:**

```
Prompt:   "show me how the DomainClassifierUnit classifies predicates"
Chosen (CODE_Q — explain mechanism):
  "DomainClassifierUnit.ClassifyBaseDomain checks two paths. If _trained
   is true and Predicates is non-null, it calls the KNN classifier on the
   predicate integer array. If _trained is false, it falls to
   ClassifyByKeywords — a static keyword scan of the raw prompt string.
   In the current code, _trained is always false because Train() is never
   called from PromptDomainClassifier — this is the A61 gap."

Rejected (CODE_Q — demonstrate usage, not mechanism):
  "Here is how to call DomainClassifierUnit:
   var unit = new DomainClassifierUnit();
   var request = new DomainClassifierRequest(prompt, Predicates: [0, 1, 2]);
   var result = unit.ClassifyBaseDomain(request);
   // result.Domain contains the predicted D-04 domain"
[WRONG: "how does it classify" asks for the decision logic — what conditions
trigger which path. A usage example shows the API surface but not the
internal decision. The prompt asks about the mechanism, not the call syntax.]
```

---

### PLAN domain — rejected construction

PLAN calibration result: 4 of 6 batches negative margin. Root cause: batches 30, 26,
37, 12 used meta-commentary prompts. Batches 16, 17 (positive) used named-artifact prompts.

**Type B rejected for PLAN — wrong dependency order (primary pair type):**

Use the dependency confusion table from `DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v2.md`.
Every annotation must explain WHY the wrong order fails, not just that it is wrong.

```
Prompt:   "plan the implementation of JWT authentication for the multi-tenant API"
Chosen (PLAN — correct order):
  Branch 1 — 3.4 Security and Authorization Model (dependency order 1):
    Define the JWT validation rules: signature algorithm (RS256), required
    claims (sub, tenantId, roles), expiry window (15min access, 7d refresh).
    This branch owns the contract that all other branches must satisfy.

  Branch 2 — 3.3 API Contract Definition (dependency order 2):
    Define the protected endpoints and their required roles.
    Dependency: 3.4 must be complete — the contract cannot specify role
    requirements until the role model is decided.

  Branch 3 — 4.1 Backend Services (dependency order 3):
    Implement the JwtValidationMiddleware, TokenService, and
    AuthorizationPolicyProvider.
    Dependency: 3.4 and 3.3 must be complete — implementation follows spec.

Rejected (PLAN — wrong order, all parallel):
  Branch 1 — 3.4 Security and Authorization Model
  Branch 2 — 3.3 API Contract Definition
  Branch 3 — 4.1 Backend Services
  All branches can run in parallel — start everything at once.
[WRONG: branch 3 implements the middleware against a JWT spec. If 3.4 is
not complete, the implementation uses assumptions that may conflict with the
final validation rules. Branch 2 cannot define protected endpoint contracts
until the role model from 3.4 is decided — the contract references roles
that do not yet exist. Parallel execution of these branches always produces
rework when 3.4 decisions change what 3.3 and 4.1 already built.]
```

**Type C rejected for PLAN — abstract phase labels instead of sub-phases:**

```
Prompt:   "plan the migration from REST to GraphQL for the reporting service"
Chosen (PLAN — specific sub-phases):
  Branch 1 — 3.3 API Contract Definition (dependency order 1):
    Define the GraphQL schema: query types, field names, pagination
    pattern (cursor-based), and error type. The schema is the contract
    that all downstream work must match.

  Branch 2 — 4.3 Infrastructure and Deployment Code (dependency order 2):
    Add the GraphQL server package, configure the schema registration,
    and add the playground endpoint behind a feature flag.

  Branch 3 — 5.2 Integration Tests (dependency order 3):
    Write integration tests against the new GraphQL endpoint using
    real query strings. Dependency: schema must be finalised.

Rejected (PLAN — abstract labels):
  Phase 1: design the GraphQL migration.
  Phase 2: implement the migration.
  Phase 3: test the migration.
  Phase 4: deploy the migration.
[WRONG: a plan must name the specific sub-phases, their dependency order,
and what decision or artifact each branch produces. "Design, implement,
test, deploy" are labels, not a plan. They carry no routing signal to
the orchestrator — it cannot dispatch work to a sub-phase called "design".]
```

---

### ARCH domain — rejected construction

ARCH calibration result: zero batches. ARCH vocabulary appeared in BUG/PLAN sessions
(rows 1–520) but was never classified as ARCH. The core pair type is Type A: boundary
decision vs implementation.

**Type A rejected for ARCH — implementation confused with boundary decision:**

```
Prompt:   "define the component boundaries for the notification service"
Chosen (ARCH — 3.1 boundary decision):
  NotificationDispatcher — owns sending. Stateless. No persistence.
  TemplateStore — owns template registry. Read-only. Versioned.
  DeliveryTracker — owns delivery state and retry scheduling. Owns its own storage.
  Dependency direction: Dispatcher depends on TemplateStore and DeliveryTracker.
  TemplateStore and DeliveryTracker must not depend on Dispatcher.
  Forbidden: Dispatcher must not own state between requests.

Rejected (4.1 Backend Services — wrong scope):
  public class NotificationDispatcher : INotificationDispatcher
  {
      private readonly ITemplateStore _templates;
      private readonly IDeliveryTracker _tracker;

      public NotificationDispatcher(ITemplateStore templates,
          IDeliveryTracker tracker) { ... }

      public async Task SendAsync(Notification n)
      {
          var template = await _templates.GetAsync(n.TemplateId);
          await _tracker.RecordAsync(n.Id);
          await _emailClient.SendAsync(template.Render(n));
      }
  }
[WRONG: "define component boundaries" asks who owns what and where
dependencies are forbidden — it does not ask for the class body.
The class body is 4.1 implementation work that follows after the
boundary decision. Writing the implementation before the boundaries
are decided embeds coupling assumptions that later cause rework.]
```

**Overlap ratio:** chosen uses ownership vocabulary (owns, stateless, depends on,
forbidden, must not). Rejected uses class vocabulary (public class, private, async,
inject). Near-zero overlap. Reliably positive margin.

**Type A rejected for ARCH — operational configuration confused with signal design:**

```
Prompt:   "design the observability contract for the payment processing service"
Chosen (ARCH — 3.5 Observability Design):
  Metrics to emit:
    payment.processed — counter with dimensions: status (success/fail/timeout),
      provider (stripe/paypal), amount_tier (low/medium/high)
    payment.latency_ms — histogram, p50/p95/p99 breakpoints
  Alert threshold: payment.processed{status=fail} > 5% of total in 5min window
  Trace requirement: every payment request carries a trace ID from entry through
    all provider calls. Minimum span depth: 3 (gateway → service → provider)

Rejected (7.3 Monitoring and Feedback Loop — wrong scope):
  Set up a Grafana dashboard for payment processing:
  - Panel 1: payment success rate (line chart, 5min intervals)
  - Panel 2: latency p95 (gauge, alert at 800ms)
  - Panel 3: provider breakdown (pie chart)
  Configure PagerDuty integration: escalate when success rate < 95%.
[WRONG: "design the observability contract" defines what signals must be
emitted and what thresholds are meaningful — it does not configure the
tools that consume those signals. Grafana and PagerDuty are consumers of
the contract. They cannot be configured until the contract says what to
measure. Tool configuration is 7.3 Monitoring work; signal contract design
is 3.5 work.]
```

---

## Hard negative construction rules

A hard negative is a rejected response that a casual reader might accept as correct.
These are the highest-value pairs: they produce the strongest positive margin when
constructed correctly, and cause the worst negative margin when constructed carelessly.

---

### Hard negative class 1 — Same domain, wrong sub-phase

Two sub-phases in the same domain share vocabulary. The model must learn the boundary
between them. The rejected must use the vocabulary of the plausible wrong sub-phase
without crossing into clearly wrong territory.

**Rule:** The rejected must be defensible as an answer to a subtly different phrasing
of the same prompt. If a reasonable person would mark the rejected as clearly wrong,
it is not a hard negative — it is a Type A pair. Use Type A for the easy cases; use
hard negatives only for genuinely ambiguous boundaries.

**Example — BUG vs REFACTOR (most common intra-domain confusion):**

The distinction: BUG = something is broken and wrong output is produced. REFACTOR =
something works but is structured poorly. A prompt with "fix" can belong to either.
The signal is whether the output is currently wrong or merely messy.

```
BUG signal words:  error, exception, null, wrong, fails, breaks, unexpected,
                   returns incorrect, doesn't work, crash, TypeError

REFACTOR signal words: messy, hard to read, duplicate, too long, extract,
                       rename, clean up, simplify, restructure, dead code
```

A "fix" prompt that describes wrong output → BUG. Same vocabulary but describing poor
structure with correct output → REFACTOR. The hard negative for a BUG pair uses
REFACTOR vocabulary on the same code, and vice versa.

**Example — PLAN vs ARCH (adjacent domains):**

PLAN = sequencing existing decisions. ARCH = making a new structural decision.
A prompt asking "what should I do about X" can route to either.

```
PLAN signal: "plan the", "in what order", "sequence the", "which comes first",
             "how should I phase", "steps for implementing"

ARCH signal: "design the", "what pattern", "should I use X or Y",
             "define the boundary", "is this the right structure",
             "what are the components"
```

A "plan" prompt that implicitly requires an ARCH decision before sequencing is a
hard case. The hard negative uses sequencing vocabulary for work that cannot yet be
sequenced because the architectural decision is missing.

---

### Hard negative class 2 — Wrong domain classification from overlapping signal words

Some prompts contain signal words from multiple domains. The model must learn which
signal dominates.

**Priority rule from calibration run evidence (EXAMINATION-METHOD-ORCHESTRATION-v8.md):**
```
BUG before REFACTOR before PLAN before ARCH before CODE_Q
```

When a prompt contains both BUG and CODE_Q signals, BUG dominates. A hard negative
for this case classifies the prompt as CODE_Q (explanation) when it should be BUG (fix):

```
Prompt:   "the tenant isolation check in the JWT middleware returns true for
           every request — explain why this happens and how to fix it"

Chosen (BUG — BUG dominates because output is wrong):
  "The check `if (token.TenantId == request.TenantId)` uses string equality on
   a field that DevelopmentPromptCompiler normalises to lowercase. The token
   carries the original case from the auth provider. Fix: normalise both sides
   before comparison — token.TenantId.ToLower() == request.TenantId.ToLower()."

Rejected (CODE_Q — wrong because wrong output exists, not explanation requested):
  "The tenant isolation check compares token.TenantId against the request's
   tenantId claim. When these do not match, the middleware rejects the request.
   The field comes from the JWT payload — the auth provider sets it during
   token issuance."
[WRONG: the prompt says the check returns true for EVERY request — meaning
the output is currently wrong. A prompt that describes wrong output is BUG,
not CODE_Q, regardless of the "explain why" phrasing. The explanation should
name the defect; the chosen response names the defect AND provides the fix.
A pure explanation without a fix leaves the broken behaviour in place.]
```

---

### Hard negative class 3 — Structurally correct but wrong scope

The rejected has the right format and technical content but covers a different scope
than the prompt requested. This class is most common in PLAN pairs where a valid partial
plan is submitted as the full plan.

```
Prompt:   "plan the full implementation of the tenant management feature —
           includes data model, API, auth, and UI"

Chosen (PLAN — full scope, four branches):
  Branch 1 — 3.2 Data Model and Persistence Design (order 1)
  Branch 2 — 3.4 Security and Authorization Model (order 2)
  Branch 3 — 3.3 API Contract Definition (order 3)
  Branch 4 — 4.2 Frontend and Client Integration (order 4, depends on 3.3)

Rejected (PLAN — partial scope, only one branch):
  Branch 1 — 4.1 Backend Services:
    Implement the TenantController with CRUD endpoints, TenantRepository
    with entity framework, and TenantService with validation logic.
[WRONG: the prompt asks for the full plan across data model, API, auth,
and UI. A single implementation branch does not address 3.2 entity design,
3.4 auth model, 3.3 API contract, or 4.2 frontend. Implementing before
the data model and auth model are defined creates dependencies on assumptions
that the omitted branches are supposed to resolve.]
```

---

## Session row classification rules

These rules resolve ambiguous rows from the internal session export.

**Rule 1 — Prompt with "fix" and wrong output described → BUG, not REFACTOR**

If the session row describes a symptom (wrong value, crash, unexpected result), classify
as BUG even if "clean up" or "extract" language appears later in the same prompt.
BUG dominates REFACTOR.

**Rule 2 — Prompt with phase/flow/step reference → PLAN**

If the session row names a flow ID, phase number, or sub-phase, classify as PLAN
even if the prompt also asks how something works. PLAN dominates CODE_Q when
sequencing language is present.

**Rule 3 — Prompt asking "should I use X or Y" → ARCH**

Trade-off prompts belong to ARCH regardless of whether the answer is a short explanation.
ARCH signals: "best approach", "what pattern", "should I use", "is this the right structure".

**Rule 4 — Prompt with "explain", "what does", "how does" and no defect → CODE_Q**

If there is no described wrong output and no sequencing request and no trade-off
question, classify as CODE_Q even if the content is technically complex.

**Rule 5 — Prompt containing ARCHITECTURE predicate vocabulary BUT described as BUG
in session context → reclassify as ARCH for pair construction**

Rows 1–520 of XIIGEN-SESSION-1000 were labelled BUG/PLAN in the calibration run, but
many contain "architecture", "component", "boundary", "topology" vocabulary. These rows
contain ARCH content that was never trained. Extract as ARCH pairs by reclassifying the
domain label and constructing Type A rejected (implementation vs boundary decision).

**Rule 6 — Row where same prompt appears as chosen for multiple sub-phases → exclude from
single-sub-phase pairs; use only for multi-route PLAN pairs**

If the same prompt legitimately routes to five sub-phases, it cannot be the A side for
a pair that teaches sub-phase 3.2 specifically — because rejected would have to be one
of the other four correct sub-phases. Use these prompts only for PLAN pairs where the
full multi-branch output is the chosen.

This rule explains 20/27 negative-margin BUG batches in the calibration run. The runner
assigned the same prompt to each sub-phase as its chosen, creating intra-batch conflict.

---

## Sources

### Source 1 — XIIGEN-SESSION-1000

Document type: Type 1 (A-side authority) for rows 521–727; mixed for rows 1–520;
excluded as A side for rows 1235–1753.

**Pair extraction priority:**
- Rows 521–727: PLAN Type B. Proven positive margin. Extract more pairs from this range.
- Rows 1–520: reclassify ARCHITECTURE-vocabulary rows as ARCH Type A.
- Rows 1235–1753: correction events only. Do not use raw prompts as A sides.

**Row classification test before extraction:**
- Check `CompiledPredicateIds` for the row. If near-empty: exclude as A side.
- Check for meta-commentary phrases (see Type 6). If present: exclude.
- Check for named artifacts or dependency language. If absent and PLAN: exclude.

---

### Source 2 — Claude Export Batch-0000

Document type: Type 3 (correction source) — primary.

**~393 correction events not yet extracted.** These are the highest-priority pairs.
Extraction order:

```
1. legacy-docs-skills-from-old-code (35 corrections) — ARCH, CODE_Q, DOCUMENT
2. value-proposition-code-gaps (104 corrections) — ARCH, BUG, PLAN
3. flow-01-station-testing (79 corrections) — BUG, IMPLEMENT, TEST
4. figma-to-code-skills-to-xiigen (69 corrections) — PLAN, ARCH, IMPLEMENT
5. session-prompt-and-learning-protocol (71 corrections) — PLAN, CODE_Q
```

For each correction event:
- H0 → A side (only if Type 1 eligibility passes)
- A0 → rejected + [WRONG: annotation derived from H1 correction text]
- A1 → chosen

Quality gate before including:
- Run token overlap test: overlap < 0.50 required, < 0.25 preferred
- Rate High/Medium/Low (see Type 3 quality rating)
- Low = exclude regardless of how clean the export looks

---

### Source 3 — Local session files (xiigen_history)

Document type: Type 2 (B-side authority) for architecture documents;
Type 1 (A-side authority) for local session H turns.

Documents in `xiigen_history/old/` are Type 2 — formal architecture vocabulary.
Use for chosen B sides in ARCH and PLAN pairs. Do NOT use as A sides.

---

### Source 4 — Orchestrator Teaching Plan Documents

Document type: Type 2 (B-side authority). Never A side.

Use `ORCHESTRATOR-TEACHING-PLAN-v3.md` sub-cycle steps for PLAN Type B chosen content.
Use stopping rules and MUST conditions for ArbiterUnit chosen/rejected criteria.

---

### Source 5 — Session Teaching Ledger

Document type: Type 4 (measurement evidence).

Use for ModelTrainingUnit B sides. Do not derive A sides from teaching ledger rows.

---

## Gap map by domain

| Domain | Calibration result | Root cause | Primary gap |
|--------|-------------------|------------|-------------|
| ARCH | 0 batches | Never extracted — vocabulary present in rows 1–520 | Need Type A pairs (boundary vs implementation) |
| CODE_Q | 0% positive margin | Negation template — overlap ~0.70 | Need Type C rejected (action response, not negation) |
| BUG | 74% negative margin | Same prompt as chosen for 8+ sub-phases | Need Rule 6: exclude multi-route prompts from single sub-phase pairs |
| PLAN | 66% negative margin | Meta-commentary A sides | Need Rule: exclude rows 1235–1753 as A sides |

---

## Document type quick-reference

| Type | Name | A-side | B-side chosen | Rejected source | Overlap range |
|------|------|--------|---------------|-----------------|---------------|
| 1 | Real developer prompts | ✓ | — | — | not applicable |
| 2 | Formal architecture documents | ✗ | ✓ sub-phase vocab | Different sub-phase Purpose vocab | 0.10–0.20 |
| 3 | Correction events (export) | ✓ H0 | ✓ A1 | ✓ A0, pre-made | < 0.25 |
| 4 | Calibration run data | ✗ | ✓ metrics B side | Missing-field variant | 0.10–0.20 |
| 5 | Topology / seeder code | ✗ | ✓ Purpose fields | Adjacent sub-phase Purpose | 0.10–0.25 |
| 6 | Session-loading messages | ✗ | ✗ | ✗ excluded entirely | — |
| 7 | Experiment status / code review | ✗ | ✓ accurate status | Overstated status | 0.10–0.25 |
| 8 | Artifact examination documents | ✗ | ✓ arbiter verdicts | Wrong severity level | 0.15–0.30 |
| 9 | Decision documents | ✗ | ✓ chosen decision | Rejected alternative from rationale | 0.05–0.20 |
| 10 | Teaching plan documents | ✗ | ✓ step ordering | Wrong step order | 0.15–0.30 |
| 11 | Measurement guidance documents | ✗ | ✓ measurement fields | Wrong fields or formula | 0.10–0.20 |
| 12 | Project history documents | ✗ | ✓ grounded historical claim | Invented motivation | 0.10–0.30 |
| 13 | Document state register | ✗ | ✓ current version / blocking order | Superseded version / parallel execution | 0.05–0.25 |
| 14 | Behavioral rules documents | ✗ | ✓ rule-compliant output | Forbidden pattern or missing Gate B | 0.05–0.25 |
| 15 | Session start prompt documents | ✗ | ✓ gate-complete session | Gate-skipped session | 0.10–0.20 |
| 16 | Versioned document evolution | ✗ | ✓ current-version vocabulary | Superseded-version vocabulary | 0.05–0.15 |

**Key distinctions:**
- Types 12–16 all produce rejected from within their own corpus — no external rejected needs to be constructed. The wrong answer is already documented.
- Types 9, 12, 13, 16 frequently produce the same pair from different angles. A decision revision (Type 9: A46→A50) appears in project history (Type 12: Part 16 Addendum), in the register (Type 13: v8 supersession note), and as a version evolution (Type 16: v10→v11 vocabulary change). Use all four angles when building a batch that targets this decision.
- Type 6 is the only type with no valid DPO use. All other types contribute at least rejected vocabulary if not chosen content.

---

## Pre-submission checklist

Before any pair from any internal source enters a batch:

```
□ Source type identified: Type 1–16
□ Type 6 prompt detected and excluded
□ A side passes Type 1 eligibility: named artifact OR dependency language OR
  technical specificity (for PLAN/ARCH); not meta-commentary
□ CompiledPredicateIds not near-empty for the A side
□ Row classification rule applied (rules 1–6 above)
□ Token overlap test: overlap_ratio < 0.60 (target: < 0.35)
□ Rejected is a complete, coherent response from the wrong sub-phase vocabulary
□ Rejected is NOT a negation, NOT a non-answer, NOT another correct response
  for a different sub-phase of the same prompt
□ [WRONG: ...] annotation names:
    (1) the sub-phase the rejected incorrectly addresses
    (2) why that sub-phase does not own this responsibility
    (3) what the prompt actually asked for that rejected missed
□ Annotation is specific: "this is wrong" fails; named confusion required
□ Hard negative class identified (1, 2, or 3) if applicable
□ For correction events: quality rated High or Medium; Low excluded
□ For correction events: conversation_uuid recorded
□ TrainingText will use compiledText from SemanticTeachingPromptCompiler
  — not raw prompt string (hard gate)
□ The same prompt does not appear as chosen for more than one sub-phase
  in this batch (Rule 6 violation check)
□ For Type 7 pairs: implementation status verified against actual code file,
  not only against experiment results map — IMPLEMENTED claim requires file path evidence
□ For Type 8 pairs: verdict type (pass/concern/reprocess) matches severity of finding —
  wrong output = reprocess, noted gap = concern, no issue = pass
□ For Type 9 pairs: rejected alternative is derivable from decision rationale or
  prior session history — not invented
□ For Type 10 pairs: dependency order in chosen matches the teaching plan's explicit
  prerequisite chain — harness before teaching, tracing before verification
□ For Type 11 pairs: all three measurement layers present in chosen B side —
  resources + routing + verdict; missing any layer is the rejected pattern
□ For Type 12 pairs: event cited (part number, date) — not invented from general knowledge
□ For Type 13 pairs: state is from a specific named version — not an average across versions
□ For Type 14 pairs: rule cited by number and version — not paraphrased from memory
□ For Type 15 pairs: forbidden output is the rejected — not a description of the rule
□ For Type 16 pairs: rejected uses earlier-version vocabulary that the newer version corrected
□ reward_margin_mean >= 0 confirmed after batch runs before committing
```

---

## Last Updated

2026-05-08.
