# XIIGen DPO Registry — External Sources Guide
## How to Identify, Define, and Measure DPO Pairs from External Licensed Sources

**Version:** 2.0 | **Extends:** XIIGEN-DPO-REGISTRY-GUIDE-v1.md
**Reference batch:** ES-2026-05-08 | **Calibration baseline accuracy:** 0.422

> This guide covers the **external sources workflow** only.
> For first-party XIIGen corpus types (B89–B165), see Guide v1.
> The two guides are complementary — run both pipelines in parallel.

---

## Table of Contents

1. [What Makes External Sources Different](#part-1-what-makes-external-sources-different)
2. [License Gate — What Is Allowed](#part-2-license-gate-what-is-allowed)
3. [Source Family Classification](#part-3-source-family-classification)
4. [How to Identify a New External Source Family](#part-4-how-to-identify-a-new-external-source-family)
5. [How to Define DPO Pairs from External Sources](#part-5-how-to-define-dpo-pairs-from-external-sources)
6. [Coverage Labels — Required Metadata per Pair](#part-6-coverage-labels-required-metadata-per-pair)
7. [Rejected-Type Taxonomy](#part-7-rejected-type-taxonomy)
8. [Corruption Cases — Three Required Kinds](#part-8-corruption-cases-three-required-kinds)
9. [Held-Out and Rephrasing Pairs](#part-9-held-out-and-rephrasing-pairs)
10. [How to Measure Translation Success](#part-10-how-to-measure-translation-success)
11. [Sub-Agent Prompts by Document Type](#part-11-sub-agent-prompts-by-document-type)
12. [Learning Subsets by Priority Tier](#part-12-learning-subsets-by-priority-tier)

---

## Part 1: What Makes External Sources Different

### The core difference from first-party sources

First-party sources (Guide v1) contain **XIIGen-specific knowledge** — artifact IDs, renumber maps, DNA patterns, flow state. The model trains to recall or apply that specific knowledge.

External sources contain **general software engineering authority** — RFC specifications, architecture templates, testing frameworks, security standards. The model trains to **route correctly and apply the right standard** when facing a real development request.

The two pipelines are complementary:

| Dimension | First-Party (Guide v1) | External Sources (Guide v2) |
|---|---|---|
| What the model learns | XIIGen-specific facts and patterns | General SE reasoning and routing |
| Source of authority | XIIGen canonical documents | RFC, CWE, NIST, MIT/Apache repos |
| A-side origin | Internal user questions | Runtime user requests |
| License concern | None (internal) | Must pass license gate before use |
| Pair shape | ID / A / B / Reason / Phrasing-B | Same, but with coverage labels |

### Why the calibration gaps require external sources

The calibration run of 2026-05-06 showed specific empty or weak sub-phases:

```
PLAN domain        — only 2 of 6 batches positive
ARCH domain        — zero training batches
5.2 Integration    — empty sub-phase
6.3 Security       — empty sub-phase
Layer 0 routing    — no pairs existed
```

First-party documents cannot fill these gaps because they are about XIIGen's
own architecture, not about general software engineering decisions. A pair
teaching "dependency order matters in PLAN" must be grounded in an external
authority (OpenAPI spec, Temporal saga docs, arc42 template) — not in XIIGen
flow state files.

---

## Part 2: License Gate — What Is Allowed

Every external source must pass the license gate before any pair from it enters
the corpus. Admitting an unlicensed source contaminates the entire batch.

### Accepted licenses

| License | Attribution required | Action |
|---|---|---|
| MIT | No (retain copyright notice) | Accepted |
| Apache 2.0 | No (retain NOTICE file where present) | Accepted |
| CC BY 4.0 | Yes — specific text required | Accepted with attribution |
| RFC / IETF publications | No (public domain note in registry) | Accepted |
| MITRE CWE entries | No (public domain note in registry) | Accepted |
| NIST SP publications | No (public domain note in registry) | Accepted |

### Rejected licenses — never use

| License | Reason |
|---|---|
| CC BY-SA (share-alike) | Incompatible with closed corpus |
| AGPL | Incompatible with closed corpus |
| GPL | Incompatible |
| Unlicensed / unknown | Cannot admit without clearing |

### The RFC / MITRE / NIST treatment

These are admitted as `external_license_checked` per Family 7 and Family 8
of the external source registry. The parenthetical `(public domain)` in the
registry is an attribution note, not a separate license category. Every pair
citing these sources carries:

```
source_license_state: external_license_checked
attribution_required: false
```

### Attribution when required (CC BY 4.0)

When `attribution_required: true`, the pair must carry:

```
attribution_text: "© [Author], CC BY 4.0"
```

Example: Google AIP-180 requires `attribution_text: "© Google LLC, CC BY 4.0"`.

The attribution text appears in the coverage labels of the pair, not inside
the B-side content. It is metadata, not training content.

---

### ✅ Good Example — License Check Passed

**Source:** `temporalio/documentation` (saga and compensation patterns)
**License:** MIT
**Check:**
- Repository has MIT license file ✓
- No NOTICE file required ✓
- Copyright notice retained in source registry entry ✓

**Decision:** Admitted. `attribution_required: false`.

---

### ❌ Bad Example — License Check Failed

**Candidate source:** A StackOverflow answer explaining JWT validation
**License:** CC BY-SA 4.0 (all StackOverflow content)
**Check:**
- Share-alike clause requires derivative works to use CC BY-SA ✓ (this is the problem)
- Closed training corpus cannot comply with share-alike ✗

**Decision:** Rejected. Do not use. Find an RFC or MIT-licensed documentation
source covering the same JWT validation content instead.

---

## Part 3: Source Family Classification

External sources are grouped into families in the registry. Each family maps to
a set of sub-phases and domain labels. Know which family your source belongs to
before writing pairs — it determines which node contracts the pairs feed.

| Family | Source Type | Sub-Phases Covered | Example Sources |
|---|---|---|---|
| **Family 4** | Architecture Decision Records | 3.1 boundary decisions | arc42, ADR-template, adr-tools |
| **Family 5** | Orchestration Pattern Docs | 3.5 signal design, 4.1 impl | Temporal, Dapr, Apache Airflow |
| **Family 6** | API Contract Specifications | 3.3 API design, 3.4 auth | OpenAPI 3.1, AsyncAPI, Google AIP |
| **Family 7** | Testing, RFC, CWE | 3.4 auth, 5.2 integration, 6.3 security | TestContainers, RFC 6749/7519/8725, MITRE CWE |
| **Family 8** | NIST Publications | 6.3 security review | NIST SP 800-53 AC-3, FIPS |

---

## Part 4: How to Identify a New External Source Family

A new external source family is warranted when the source covers a
**sub-phase or domain combination that existing families do not cover**.
The test is functional, not topical.

### Step 4.1 — Check the sub-phase coverage gap first

Before adding a source, check whether the sub-phase it would fill already has
coverage. If it does, check whether the new source provides genuinely different
authority (not just a different way to say the same thing).

**Decision tree:**

```
Is there a confirmed calibration gap for this sub-phase?
  → No  → Do not add a new source. Existing coverage is sufficient.
  → Yes → Does an existing family source cover this sub-phase?
           → Yes → Does the new source provide different authority?
                    → No  → Do not add. Use the existing source.
                    → Yes → Register as a new source within the existing family.
           → No  → Consider registering as a new family.
```

### Step 4.2 — Minimum registration entry for a new source

```
source_id:           short-lowercase-hyphenated identifier
source_title:        Human-readable title
license:             MIT | Apache 2.0 | CC BY 4.0 | external_license_checked
attribution_required: true | false
attribution_text:    (present only when true)
family:              4 | 5 | 6 | 7 | 8 | new number
sub_phases_covered:  list of sub-phase IDs
scenarios_used_in:   list of scenario IDs this source grounds
authority_claim:     one sentence — what makes this source authoritative for its sub-phases
```

### ✅ Good Example — New Source Correctly Identified

**Candidate:** `google/re2` documentation (regex syntax)
**Proposed sub-phase:** 4.1 — fix a regex validation defect
**Check:**
- Family 7 already covers BUG sub-phase 4.1 via CWE and RFC
- re2 documentation provides different authority: it defines regex syntax and
  backtracking guarantees — no existing family source covers this
- Calibration gap exists for BUG adapter (confirmed)

**Decision:** Register as Family 7 addition. New `source_id: google-re2-docs`,
MIT license, `attribution_required: false`.

---

### ❌ Bad Example — Redundant Source Mistaken for New Family

**Candidate:** `auth0/docs` (JWT documentation)
**Proposed family:** Family 9 — "vendor JWT documentation"
**Check:**
- RFC 7519 and RFC 8725 already cover JWT authority in Family 7
- auth0/docs describes the same JWT claims and algorithms
- No new sub-phase is covered
- auth0 docs are not a normative standard — RFC is more authoritative

**Decision:** Rejected. RFC 7519 + RFC 8725 are the correct authority sources.
Using vendor docs when the normative RFC exists introduces weaker grounding.

---

## Part 5: How to Define DPO Pairs from External Sources

### Step 5.1 — Write the target behavior sentence first (required)

Same rule as Guide v1: before collecting any pairs, complete this sentence:

> "After training on these pairs, the model should **[X]** instead of **[Y]** when asked **[Z]**."

**Good examples from the ES-2026-05-08 batch:**

| Domain | Target Behavior Sentence |
|---|---|
| ARCH | Model should produce a boundary decision document (sub-phase 3.1) instead of a class skeleton (4.1) when the user asks "decide what service X owns" |
| PLAN | Model should order branch-1 → branch-2 with dependency direction correct instead of reversing the order when the user asks "design X then implement X" |
| BUG | Model should route to 4.1 fix-implementation instead of 3.4 redesign-the-model when the reported defect is in the code path, not the design |
| Node 08 | Model should emit `verdict: "pass"` with no extra fields when a fragment satisfies all RFC constraints, instead of fabricating `violated_constraint` fields |

---

### Step 5.2 — The universal pair shape

Every external source pair follows the same shape:

```
Pair[ID]   — ES-[BATCH]-[SUB-PHASE]-[NODE]-[SEQ]
A          — the exact runtime input (user request or contract object)
B          — the correct output (chosen)
Reason     — [WRONG: ...] annotation explaining what the rejected gets wrong
Phrasing-B — the incorrect output (rejected) — one of Type A, B, C, or corruption_case
```

The `[WRONG: ...]` annotation is mandatory. It names the specific RFC section,
CWE number, or architectural principle that the rejected violates. Without it,
a reviewer cannot confirm that the rejected is actually wrong — they would have
to look it up themselves.

---

### Step 5.3 — The three rejected types

The rejected type determines what confusion the pair teaches the model to avoid.

#### Type A — Adjacent sub-phase confusion

The rejected produces output for a sub-phase that is next to the correct one
in the pipeline. The content is plausible but targets the wrong stage.

**Signature:** same domain, adjacent sub-phase number.

**✅ Good Type A pair (ARCH domain, Node 08):**

```
A:  ArbiterReviewRequest showing a JWT fragment that satisfies RFC 7519
    MUST requirements: iss, aud, exp, alg=none rejected per RFC 8725

B (chosen):
    ArbiterVerdict: { verdict: "pass" }
    (silent fields — no violated_constraint, no reprocess_instruction)

Phrasing-B (Type A — fabricated fields on a clean pass):
    ArbiterVerdict:
      verdict:               "pass"
      reason:                "model is acceptable"
      violated_constraint:   "none, but suggest adding kid claim"
      reprocess_instruction: "consider adding kid claim for key rotation"

Reason: [WRONG: fragment satisfies every RFC 7519 MUST. Clean pass — silent
verdict is correct per v8 §Node 08. The rejected fabricates a
violated_constraint and reprocess_instruction when no constraint is violated.
Inventing fields on a clean pass corrupts the verdict signal: downstream Node
09 would format reprocess hints when no reprocess is needed.]
```

**❌ Bad Type A pair — wrong sub-phase delta:**

```
A:  "Define the ownership boundary between order and billing services"
B:  3.1 ADR boundary document
Phrasing-B: 3.1 ADR boundary document with one field renamed

Reason: [WRONG: too vague]
```

Why bad: chosen and rejected are almost identical — one field renamed is not
a sub-phase confusion. The overlap_ratio would be > 0.60. This is not a
teaching pair; it teaches field naming, not sub-phase routing.

---

#### Type B — Dependency direction reversal

The rejected contains the correct sub-phases but reverses the dependency
direction — the output that should come second is listed first.

**Signature:** same sub-phase IDs, `depends_on` values reversed.

**✅ Good Type B pair (PLAN domain):**

```
A:  "Define JWT auth model, then implement AuthGuard middleware"

B (chosen):
    branch-1: sub_phase=3.4 (define auth model)   depends_on: []
    branch-2: sub_phase=4.1 (implement AuthGuard)  depends_on: [branch-1]

Phrasing-B (Type B — dependency direction reversed):
    branch-1: sub_phase=4.1 (implement AuthGuard first)  depends_on: []
    branch-2: sub_phase=3.4 (model retrofitted after)    depends_on: [branch-1]

Reason: [WRONG: AuthGuard implemented first hardcodes assumptions that the
auth model (3.4) may invalidate. If the model later requires additional claims
(e.g., tenant, role), the middleware must be reworked. RFC 7519 §4.1 defines
claims; the design phase must establish which claims are required before any
validator is written.]

overlap_ratio: 0.45  (same sub-phases, depends_on values disjoint)
```

**❌ Bad Type B pair — overlap_ratio too high:**

```
A:  "Define API contract then implement controller"
B:  3.3 first → 4.1 depends_on=[3.3]
Phrasing-B: 3.3 first → 4.1 depends_on=[3.3]   (only: one field name differs)
overlap_ratio: 0.82
```

Why bad: overlap_ratio of 0.82 exceeds the 0.60 gate. The pair cannot be
submitted. The rejected is not meaningfully different from the chosen. Revise
the rejected to reverse the dependency direction, not just rename a field.

---

#### Type C — Wrong domain for intent verb

The rejected assigns the prompt to a different domain based on a surface
feature of the prompt (a noun that sounds like another domain) rather than
the intent verb.

**Intent-verb → correct domain mapping:**

```
"explain / how does / what is"         → CODE_Q
"throws / fails / error / bug"         → BUG
"implement / build / add / create"     → PLAN or sub-phase 4.x
"define boundaries / design / decide"  → ARCH (sub-phase 3.1/3.2)
"review / audit / verify compliance"   → ARCH 6.x or QA
```

**✅ Good Type C pair (Stage 9 corruption supplement):**

```
A:  "How does the JWT iss claim get validated against the OpenID Connect
     issuer URL?"

B (chosen):
    expected_transition_type: "explain"
    expected_domain:          "CODE_Q"
    expected_routing:
      - PredictedSubPhaseId: "3.4"
        EvidencePredicates:  ["SECURITY"]
    expected_response_class: "explanation grounded in RFC 7519 §4.1.1
                              and OpenID Connect Discovery 1.0"

Phrasing-B (Type C — "implement" mis-assignment):
    expected_transition_type: "feature_addition"
    expected_domain:          "PLAN"
    expected_routing:
      - PredictedSubPhaseId: "4.1"
        EvidencePredicates:  ["IMPLEMENT"]

Reason: [WRONG: "How does" is the canonical CODE_Q intent verb. The
prompt asks for an explanation, not an implementation. JWT is a noun
that also appears in IMPLEMENT contexts, but "How does X work" always
maps to CODE_Q. The rejected promotes the noun to the primary signal
and ignores the intent verb.]
```

**❌ Bad Type C pair — intent verb is genuinely ambiguous:**

```
A:  "JWT token handling for the new auth service"

B (chosen):  CODE_Q (explain)
Phrasing-B:  PLAN (implement)

Reason: [WRONG: JWT is in CODE_Q domain]
```

Why bad: the prompt "JWT token handling for the new auth service" does not
contain a clear intent verb. "handling" could mean explain, implement, or
review. The A-side is ambiguous — a model cannot correctly learn from this pair
because the ground truth is not clear. Fix the A-side to include an explicit
intent verb before using this as a pair.

---

### Step 5.4 — Overlap ratio gate

Every rejected must satisfy `overlap_ratio < 0.60`.

Overlap ratio is measured as token overlap between chosen B-side and
rejected B-side. A ratio above 0.60 means the pair cannot teach a meaningful
distinction — both sides are too similar.

**What to do when overlap_ratio ≥ 0.60:**

- For Type A: choose a rejected from a more distant sub-phase (not just adjacent)
- For Type B: make sure the `depends_on` arrays are fully reversed, not partially
- For Type C: choose a rejected domain that looks more plausible given the prompt

**Record the ratio in the coverage labels for every pair.** It is not optional metadata.

---

## Part 6: Coverage Labels — Required Metadata per Pair

Every pair must carry a complete coverage label set. Missing labels fail the
batch validation check at v8 line 806.

### File-constant labels (declared once per file)

These apply to every pair in the file and are declared in a "Common coverage
labels" header at the top. They do not need to repeat in each pair's block.

```
source_license_state   — external_license_checked  (every file in external batch)
contract_signature     — the unit contract for that file
node_role              — layer_0_a | layer_0_b | node_03 | ... | stage_9
pair_kind              — adaptive_routing_compiler | flow_node | corruption_case | teaching_unit
```

### Per-pair labels (required in every individual pair block)

```
source_id              — short ID from source registry (e.g., rfc-7519)
source_title           — human-readable title (e.g., "RFC 7519 — JSON Web Token")
attribution_required   — true | false
attribution_text       — present only when attribution_required = true
transition_type        — feature_addition | repair | explain | topology_mutation | ...
phase                  — the routed phase number (3, 4, 5, 6)
sub_phase              — the routed sub-phase (3.1, 3.4, 4.1, 5.2, 6.3, ...)
task_type              — the topology task_type from registration
coverage_status        — covered | source_gap | corruption_case
rejected_type          — A | B | C | corruption_case
overlap_ratio          — token-overlap test result (must be < 0.60)
```

### ✅ Good Example — Complete coverage block

```
source_id:            rfc-7519, rfc-8725
source_title:         RFC 7519 — JSON Web Token + RFC 8725 — JWT BCP
source_license_state: external_license_checked
attribution_required: false
contract_signature:   ArbiterUnit
transition_type:      feature_addition
phase:                3
sub_phase:            3.4
task_type:            jwt-auth-contract
node_role:            node_08
coverage_status:      covered
pair_kind:            flow_node
rejected_type:        A
overlap_ratio:        0.38
```

### ❌ Bad Example — Missing required fields

```
source_id:   rfc-7519
phase:       3
rejected_type: A
```

Why bad: missing `source_title`, `source_license_state`, `attribution_required`,
`contract_signature`, `transition_type`, `sub_phase`, `task_type`, `node_role`,
`coverage_status`, `pair_kind`, `overlap_ratio`. This pair fails batch
validation at v8 line 806 and cannot be submitted.

---

## Part 7: Rejected-Type Taxonomy

Quick reference — use this to choose the right rejected type before writing
the pair.

| Type | Pattern | Typical domain | overlap_ratio target |
|---|---|---|---|
| A | Adjacent sub-phase confusion | Any | 0.30–0.50 |
| B | Dependency direction reversal | PLAN | 0.40–0.55 |
| C | Wrong domain for intent verb | Routing | 0.25–0.45 |
| corruption_case | Systematic failure mode (Stage 9) | Layer 0 / Node 08 | N/A — teach detection |

**Batch composition minimums (from v7 §"Batch composition minimums"):**

```
Type A:          ≥ 40% of pairs in any single domain file
Type B:          dominant type for PLAN domain
Type C:          ≥ 30% across the full batch (supplemented by Supplement-Domain-TypeC)
corruption_case: exactly 3 per batch (one per kind — see Part 8)
```

---

## Part 8: Corruption Cases — Three Required Kinds

Every batch must include exactly one of each of the following corruption kinds.
These pairs teach the model to detect and never produce specific failure modes.

They are collected in a separate `Stage-9-Corruption.md` file. They use
two distinct authority types that must not be conflated:

- **External authority** — the RFC, CWE, NIST text that grounds the prompt
  content. Recorded in `source_id` / `source_license_state`.
- **Internal compiler contract** — the nine-predicate lexicon, contract shapes,
  field names. These are NOT external sources. Recorded separately in
  `internal_contract_reference`.

### Kind 1: Wrong-predicate-assignment

A prompt where one lexicon term appears as a noun modifier but another is the
primary intent verb. The corrupted version promotes the modifier to primary.

**Construction rule:** find a prompt where a high-frequency noun (e.g.,
"authentication") appears as context but the verb is from a different predicate
("test coverage"). The corrupted version assigns the noun's predicate instead
of the verb's predicate.

**✅ Good corruption pair:**

```
A:  "Test coverage shows 42% on the authentication module — we need to bring
     this up to 80% before the release."

B (correct):
    CompiledPredicateIds: ["TEST"]
    Confidence: 0.92

Phrasing-B (wrong-predicate-assignment corruption):
    CompiledPredicateIds: ["SECURITY"]
    Confidence: 0.92

Reason: [WRONG: "test coverage" and "80%" are the primary signal — they map
to TEST. "authentication" appears as a noun modifier (the thing being tested),
not as the intent. RFC 7519 defines "authentication" as a domain noun. The
TEST predicate requires "test", "testing", "unit test", or "coverage" in the
primary position — all of which appear here. SECURITY requires "jwt", "auth",
"oauth" etc. as the primary signal. Promoting the modifier to primary is the
wrong-predicate-assignment failure mode.]

Internal contract reference: nine-predicate lexicon (PromptRoutingInputCompilerUnit)
External authority: RFC 7519 (grounds "authentication" as domain noun), microsoft/testfx (grounds "test coverage" as TEST signal)
```

---

### Kind 2: Ambiguous-predicate-case

A prompt where two predicates are equally supported and the correct routing
requires choosing one and documenting why the other was not chosen.

**Construction rule:** find a prompt where the intent verb maps to one predicate
but a strong domain noun maps to another. The corrupted version produces only
one predicate when two are valid — the wrong one.

---

### Kind 3: Knowledge-edge-suppressed

A prompt where the KnowledgeEdge flag should be `true` but the corrupted
version returns `PASS` (suppresses the edge detection).

**Construction rule:** find a prompt where an external source (RFC, ADR
template, saga pattern) is named or strongly implied. The corrupted version
ignores this signal and routes as if no knowledge edge were needed.

---

## Part 9: Held-Out and Rephrasing Pairs

Every batch must include a held-out evaluation set. These pairs are NOT used
in DPO training — they evaluate whether the trained model generalises beyond
the in-batch chosen prompts.

Minimum required per batch (from v7 line 371):

```
3 held-out prompts             — disjoint from batch chosen prompts
3 rephrasing pairs             — same intent, different surface form
1 wrong-transition prompt      — intent verb mis-mapped to transition type
1 dropped-constraint prompt    — required field missing from B-side
1 wrong-sub-phase prompt       — routing target wrong
1 arbiter-contradiction prompt — Node 08 verdict contradicts Node 06
```

Held-out pairs carry:

```
pair_kind:        held_out | rephrasing
coverage_status:  held_out
```

They feed into `HeldOutEvaluationUnit` at Layer 3. They are excluded from
the DPO training step.

### ✅ Good held-out prompt

```
Pair[ID]: ES-2026-05-08-HO-01

A (held-out — not in any batch chosen pair):
    user_request: "We're moving the customer profile data out of the billing
                   service into a new identity service. Define what each service
                   owns now and which calls cross the boundary."

B (expected):
    expected_routing:
      - PredictedSubPhaseId: "3.1"
        EvidencePredicates:  ["ARCHITECTURE"]
        KnowledgeEdge:       true
    expected_transition_type: "topology_mutation"
    expected_authority: "arc42 §5 Building Block View"
```

### ❌ Bad held-out prompt — overlaps with a batch chosen pair

```
Pair held-out:
    user_request: "Define what the order service owns vs the inventory service"
```

Why bad: this is nearly identical to batch pair ARCH-0001's A-side
(`"Define what the order service owns vs what the inventory service owns"`).
The held-out set tests generalisation. A prompt that is a near-duplicate of
a training pair does not test generalisation — it only tests memorisation.

---

## Part 10: How to Measure Translation Success

### Step 10.1 — Confirm the calibration gaps being targeted

Check the README for the batch. It lists which sub-phases were empty or weak
before the batch. These are the specific accuracy metrics to measure after
training.

From ES-2026-05-08:

```
PLAN domain        — target: ≥ 4 of 6 batches positive (was: 2 of 6)
ARCH domain        — target: ≥ 0.65 accuracy (was: zero training)
5.2 Integration    — target: ≥ 0.70 (was: empty)
6.3 Security       — target: ≥ 0.70 (was: empty)
Layer 0 routing    — target: ≥ 0.80 predicate accuracy (was: no pairs)
```

### Step 10.2 — Three measurement levels (same as Guide v1)

#### Level 1: Direct routing accuracy

Present the model with A-side requests from the held-out set and check whether
the predicted sub-phase and predicate IDs match the expected values in B.

```
Metric:    routing accuracy = fraction of held-out prompts where
           PredictedSubPhaseId matches expected_routing[0].PredictedSubPhaseId
           AND EvidencePredicates are a superset of expected EvidencePredicates
Threshold: ≥ 0.80 for Layer 0 routing
           ≥ 0.70 for domain-level sub-phase assignment
```

#### Level 2: Dependency order preservation (PLAN domain)

Give the model a multi-task request. Check whether the branch dependency
direction is correct without prompting the model to explain its reasoning.

```
Metric:    dependency correctness = fraction of multi-branch plans where
           design branch has depends_on: [] and implement branch has
           depends_on: [design branch]
Threshold: ≥ 0.75 for PLAN domain
```

#### Level 3: Verdict field discipline (Node 08)

Give the model an ArbiterReviewRequest where the fragment satisfies all
normative constraints. Check whether the model produces a clean
`verdict: "pass"` with no extra fields.

```
Metric:    silent-verdict rate = fraction of clean-pass cases where
           the model does not fabricate violated_constraint or
           reprocess_instruction fields
Threshold: ≥ 0.80 for Node 08 clean-pass cases
```

---

### Step 10.3 — Layer 3 metrics (ModelTrainingUnit output)

The Layer 3 training unit records these fields in `TrainingResult`. Use them
to verify the batch was absorbed correctly:

```
training_time_seconds    — wall-clock training time
held_out_accuracy        — accuracy on the held-out evaluation set
training_count           — number of pairs processed in this batch
held_out_count           — number of held-out pairs evaluated
model_artifact_size_mb   — size of the updated adapter
cpu_time_seconds
ram_peak_gb
disk_read_gb, disk_write_gb
```

**Minimum acceptable held-out accuracy after any batch: 0.65.**
If `held_out_accuracy < 0.65`, the batch failed to generalise. Review
the pairs for overlap_ratio violations and fix before the next batch.

---

### Step 10.4 — Regression check for existing sub-phases

After training on a new batch, verify that sub-phases that already had positive
calibration scores did not regress.

```
Regression gate: no previously positive sub-phase may drop more than 0.05
                 in held-out accuracy after the new batch is absorbed.
```

If a regression occurs, inspect the `CorrectionLearningUnit` pairs for the
affected sub-phase. The batch may have introduced conflicting signal on a
shared predicate.

---

### Step 10.5 — Delta target

Same as Guide v1:

> A working external batch should push accuracy on its targeted sub-phases
> **≥ +0.10 above the 0.422 calibration baseline**.

For ARCH (zero batches before ES-2026-05-08): any positive accuracy is progress.
For PLAN (2 of 6 positive): target 4 of 6 positive after this batch.

---

## Part 11: Sub-Agent Prompts by Document Type

---

### Layer-0-A — PromptRoutingInputCompiler pairs

```
You are a routing input compilation pair extraction agent.
Read [ATTACHED Layer-0-A-PromptCompiler.md].

This file contains pairs for the PromptRoutingInputCompilerUnit contract:
  ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>

For each pair, extract:

{
  "pair_id":        "ES-BATCH-PHASE-NODE-SEQ",
  "a_side": {
    "run_id":       "...",
    "user_request": "...",
    "flags":        { "UseLearnedUnitWhenAvailable": bool, "UseKnowledgeEdgeDetection": bool }
  },
  "chosen": {
    "CompiledPredicateIds": [...],
    "Confidence":           0.92 | 0.70,
    "SourceStatus":         "ready"
  },
  "rejected": {
    "CompiledPredicateIds": [...  wrong predicates ...],
    "Confidence":           0.92 | 0.70
  },
  "reason":          "[WRONG: ...] annotation verbatim",
  "rejected_type":   "A" | "B" | "C" | "corruption_case",
  "overlap_ratio":   0.00–0.59,
  "source_ids":      [...],
  "coverage_labels": { complete label set }
}

Output: JSON array.
Validation: overlap_ratio must be < 0.60. Reject any pair that exceeds it
and flag it as needs_revision.
```

---

### Node-08 — Arbiter Pairs

```
You are an arbiter verdict pair extraction agent.
Read [ATTACHED Node-08-Arbiter.md].

Contract: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>

Verdict shape:
  verdict: pass | concern | reprocess | reject
  reason, violated_constraint, reprocess_instruction, learning_gap_packet
  (fields are silent when verdict = "pass" and no constraint is violated)

For each pair, extract:

{
  "pair_id":   "ES-BATCH-PHASE-N08-SEQ",
  "a_side":    { ArbiterReviewRequest object — include request_id, arbiter_id, target_fragments },
  "chosen":    { ArbiterVerdict — include only the non-silent fields },
  "rejected":  { ArbiterVerdict — the wrong version, e.g., fabricated fields on clean pass },
  "reason":    "[WRONG: ...] annotation — must name the RFC section or CWE number",
  "rejected_type": "A" | "B" | "C",
  "overlap_ratio": 0.00–0.59,
  "source_ids":    [...],
  "coverage_labels": { complete label set }
}

Special rule for clean-pass pairs: if chosen verdict is exactly { verdict: "pass" }
with no other fields, the rejected must fabricate at least one field (violated_constraint,
reprocess_instruction, or reason) to be a meaningful Type A pair.

Output: JSON array.
```

---

### ARCH-Rows — Compact Row Extraction

```
You are a compact ARCH row extraction agent.
Read [ATTACHED ARCH-Rows.md].

Each row follows the format:
  [ID | type/OR/trans/sub] sources | A | B | P | R

Fields:
  ID:    pair identifier
  type:  rejected_type (A, B, or C)
  OR:    overlap_ratio
  trans: transition_type
  sub:   sub_phase (e.g., 3.1)
  A:     user prompt (A-side)
  B:     chosen action (correct sub-phase + response type)
  P:     rejected action (wrong sub-phase or output type)
  R:     [WRONG: ...] annotation

For each row, produce:

{
  "pair_id":       "ARCH-NNNN",
  "a_side":        "...",
  "chosen":        "sub_phase: X.X — [description from B field]",
  "rejected":      "sub_phase: X.X — [description from P field]",
  "reason":        "[WRONG: ...] verbatim from R field",
  "rejected_type": "A" | "B" | "C",
  "overlap_ratio": 0.00–0.59,
  "source_ids":    [list from sources field],
  "domain":        "ARCH",
  "coverage_labels": { ... }
}

Output: JSON array.
Note: rows marked [ARCH-NNNN..ARCH-MMMM — N more rows] are placeholders.
Flag these as placeholder_range: true and extract only the fully-written rows.
```

---

### PLAN-Rows — Dependency-Order Row Extraction

```
You are a PLAN dependency-order pair extraction agent.
Read [ATTACHED PLAN-Rows.md].

Each row follows the same compact format as ARCH-Rows.
Dominant rejected type is B (dependency direction reversal).

For each row, extract:

{
  "pair_id":      "PLAN-NNNN",
  "a_side":       "...",
  "chosen": {
    "branch_1":   { "sub_phase": "3.x", "depends_on": [] },
    "branch_2":   { "sub_phase": "4.x", "depends_on": ["branch_1"] }
  },
  "rejected": {
    "branch_1":   { "sub_phase": "4.x first", "depends_on": [] },
    "branch_2":   { "sub_phase": "3.x after", "depends_on": ["branch_1"] }
  },
  "reason":       "[WRONG: ...] verbatim — must explain why reversed order fails",
  "rejected_type": "B",
  "overlap_ratio": 0.40–0.55,
  "source_ids":    [...],
  "domain":        "PLAN",
  "coverage_labels": { ... }
}

Output: JSON array.
Validation: rejected.branch_1.depends_on must be [] and rejected.branch_2
must depend on branch_1 to qualify as a valid Type B pair. If the dependency
structure is not reversed, flag as invalid_type_b.
```

---

### BUG-Rows — Repair vs Redesign Row Extraction

```
You are a BUG domain row extraction agent.
Read [ATTACHED BUG-Rows.md].

Dominant correct routing: 4.1 fix-implementation.
Dominant wrong routing: 3.4 redesign-the-model.

For each row, extract:

{
  "pair_id":      "BUG-NNNN",
  "a_side":       "...",
  "chosen":       "sub_phase: 4.1 — [fix description from B field]",
  "rejected":     "sub_phase: 3.4 — [redesign description from P field]",
  "reason":       "[WRONG: ...] verbatim — must cite the RFC or CWE that confirms it is an implementation defect",
  "rejected_type": "A" | "C",
  "overlap_ratio": 0.25–0.40,
  "source_ids":    [...],
  "domain":        "BUG",
  "coverage_labels": { ... }
}

Output: JSON array.
```

---

### Stage-9-Corruption — Corruption Case Extraction

```
You are a corruption case extraction agent.
Read [ATTACHED Stage-9-Corruption.md].

Three corruption kinds: wrong-predicate-assignment | ambiguous-predicate-case
| knowledge-edge-suppressed

IMPORTANT — authority separation: every corruption pair has two distinct
authority types. Extract them separately:

{
  "pair_id":   "ES-BATCH-COR-NN",
  "corruption_kind": "wrong-predicate-assignment | ambiguous-predicate-case | knowledge-edge-suppressed",
  "a_side":    { user_request and flags },
  "chosen":    { correct compiler output — CompiledPredicateIds, Confidence },
  "rejected":  { corrupted compiler output — wrong predicates },
  "reason":    "[WRONG: ...] annotation verbatim",
  "external_authority": {
    "source_ids":     [...],
    "source_titles":  [...],
    "license_states": [...]
  },
  "internal_contract_reference": "...",
  "coverage_labels": { ... }
}

Output: JSON array with exactly 3 entries — one per corruption kind.
If any corruption kind is missing from the file, flag as missing_kind: [...].
```

---

### HeldOut-Rephrasing — Evaluation Pair Extraction

```
You are a held-out evaluation pair extraction agent.
Read [ATTACHED HeldOut-Rephrasing.md].

These pairs are NOT for training. They are for evaluation only.

For each pair, extract:

{
  "pair_id":        "ES-BATCH-HO-NN | ES-BATCH-RP-NN",
  "pair_kind":      "held_out | rephrasing | wrong_transition | dropped_constraint | wrong_sub_phase | arbiter_contradiction",
  "a_side":         { run_id, user_request },
  "expected_b": {
    "expected_routing":         [...],
    "expected_transition_type": "...",
    "expected_authority":       "...",
    "expected_node_03_branches": [...] if present
  },
  "eval_only": true,
  "source_ids":     [...],
  "coverage_labels": { ... }
}

Output: JSON array.
Minimum required: 3 held_out + 3 rephrasing + 1 wrong_transition +
1 dropped_constraint + 1 wrong_sub_phase + 1 arbiter_contradiction = 10 pairs.
Flag as missing_kinds: [...] if any of the 6 kinds is absent.
```

---

### Layer-3-Teaching — Training and Evaluation Unit Pairs

```
You are a Layer 3 training unit pair extraction agent.
Read [ATTACHED Layer-3-Teaching.md].

Three contracts:
  ModelTrainingUnit:      ILogicalUnit<FeatureVectorDataset, TrainingResult>
  HeldOutEvaluationUnit:  ILogicalUnit<ModelEvaluationInput, HeldOutEvaluationResult>
  CorrectionLearningUnit: ILogicalUnit<CorrectionTrainingInput, TrainingResult>

For each pair, extract:

{
  "pair_id":         "ES-BATCH-DOMAIN-L3-NN",
  "contract":        "ModelTrainingUnit | HeldOutEvaluationUnit | CorrectionLearningUnit",
  "a_side":          { full input object — FeatureVectorDataset or ModelEvaluationInput or CorrectionTrainingInput },
  "chosen":          { full output object — TrainingResult or HeldOutEvaluationResult },
  "rejected":        { wrong output — e.g., wrong held_out_accuracy, wrong chunking decision },
  "reason":          "[WRONG: ...] annotation",
  "domain":          "ARCH | PLAN | BUG | CODE_Q | multi",
  "coverage_labels": { ... }
}

Output: JSON array.
Minimum required: 3 ModelTrainingUnit + 1 HeldOutEvaluationUnit +
2 CorrectionLearningUnit = 7 pairs.
```

---

### Coverage-Labels-Sweep — Completeness Verification

```
You are a coverage label completeness verification agent.
Read [ATTACHED Coverage-Labels-Sweep.md].

This file records the complete v8 §"Required Coverage Labels" set plus
the overlap_ratio for every pair in Node-03 through Node-09.

For each pair entry, verify:
  1. All required fields present (source_id, source_title, source_license_state,
     contract_signature, transition_type, phase, sub_phase, task_type,
     node_role, coverage_status, pair_kind, rejected_type)
  2. overlap_ratio < 0.60
  3. source_license_state = external_license_checked

Output: JSON with:
{
  "total_pairs_checked": N,
  "passed": M,
  "failed": [{
    "pair_id": "...",
    "failures": ["missing field X", "overlap_ratio 0.72 exceeds gate", ...]
  }],
  "gate_verdict": "PASS" | "FAIL"
}
```

---

## Part 12: Learning Subsets by Priority Tier

---

### Tier 1 — Routing Foundation (Train First)

These fill the Layer 0 routing gap that had zero pairs before this batch.
Without correct routing, nothing else in the pipeline works.

| File | Pairs | Target metric |
|---|---|---|
| Layer-0-A-PromptCompiler | 8 pairs (one per scenario) | Predicate accuracy ≥ 0.80 |
| Layer-0-B-MultiSubPhaseRouter | multi-sub-phase routing | Dependency order correct ≥ 0.75 |
| Layer-0-C-KnowledgeEdge | edge detection | KnowledgeEdge flag accuracy ≥ 0.80 |
| Layer-0-D-FlowFactory | factory routing | Sub-phase assignment ≥ 0.75 |

**Tier 1 success gate:** Layer 0 routing accuracy ≥ 0.80 before advancing.

---

### Tier 2 — Domain Adapters with Zero Coverage (Train Second)

These fill the ARCH gap (zero batches) and strengthen the PLAN gap
(2 of 6 batches positive).

| File | Pairs | Target metric |
|---|---|---|
| ARCH-Rows | 70+ rows | ARCH routing accuracy ≥ 0.65 |
| PLAN-Rows (§1 and §2) | 50+ rows (§1) + 30+ rows (§2) | PLAN dependency order ≥ 0.75 |
| Node-01 through Node-04 | Core pipeline nodes | Sub-phase assignment ≥ 0.70 |

---

### Tier 3 — Security and Testing Sub-Phases (Train Third)

These fill the empty 5.2 Integration Tests and 6.3 Security Review sub-phases.

| File | Pairs | Target metric |
|---|---|---|
| BUG-Rows | 1000+ rows | BUG routing accuracy ≥ 0.75 |
| CODE_Q-Rows | explanation routing | CODE_Q accuracy ≥ 0.75 |
| Node-05 through Node-09 | Processing nodes | Node accuracy ≥ 0.70 |
| REFACTOR-Rows | refactor domain | REFACTOR accuracy ≥ 0.70 |
| Supplement-PLAN-TypeB | additional Type B PLAN | PLAN dependency ≥ 0.80 |
| Supplement-Domain-TypeC | Type C fraction top-up | Type C fraction ≥ 30% |

---

### Tier 4 — Corruption Detection and Held-Out Validation (Run Last)

These are not training pairs — they validate that Tier 1–3 training generalised.

| File | Type | Purpose |
|---|---|---|
| Stage-9-Corruption | 3 corruption pairs | Verify model never produces failure modes |
| HeldOut-Rephrasing | 10 evaluation pairs | Verify generalisation beyond training prompts |
| Layer-3-Teaching | 7 teaching unit pairs | Verify training infrastructure handles the batch |
| Coverage-Labels-Sweep | Verification doc | Confirm all labels are complete and overlap < 0.60 |

---

### Full Priority Summary

```
TRAIN FIRST (Tier 1 — routing):
  Layer-0-A, Layer-0-B, Layer-0-C, Layer-0-D

TRAIN SECOND (Tier 2 — zero-coverage domains):
  ARCH-Rows, PLAN-Rows §1, Layer-1-TransitionOrchestrator,
  Node-01, Node-02, Node-03, Node-04

TRAIN THIRD (Tier 3 — empty sub-phases):
  BUG-Rows, CODE_Q-Rows, REFACTOR-Rows,
  Node-05, Node-06, Node-07, Node-08, Node-09,
  Supplement-PLAN-TypeB, Supplement-Domain-TypeC

EVALUATE (not training — Tier 4):
  Stage-9-Corruption, HeldOut-Rephrasing,
  Layer-3-Teaching, Coverage-Labels-Sweep
```

---

## Quick Reference

### License gate (one-line)

MIT ✅ · Apache 2.0 ✅ · CC BY 4.0 ✅ (with attribution) · RFC/MITRE/NIST ✅ · CC BY-SA ❌ · AGPL ❌

### Pair shape (one-line)

`Pair[ID] / A / B / Reason [WRONG: ...] / Phrasing-B (Type A|B|C|corruption_case)`

### Three rejected types (one-line each)

- **Type A:** same domain, wrong adjacent sub-phase (overlap 0.30–0.50)
- **Type B:** same sub-phases, `depends_on` reversed (overlap 0.40–0.55)
- **Type C:** wrong domain for intent verb (overlap 0.25–0.45)

### Overlap gate

> `overlap_ratio < 0.60` is required for every pair. Exceeding it fails
> batch validation. Revise the rejected before submitting.

### Delta target

> Targeted sub-phases should reach **≥ +0.10 above the 0.422 baseline**.
> Regression gate: no previously positive sub-phase drops more than 0.05
> after a new batch is absorbed.
