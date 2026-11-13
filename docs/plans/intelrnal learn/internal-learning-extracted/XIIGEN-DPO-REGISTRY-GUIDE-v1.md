# XIIGen DPO Registry — Complete Guide
## How to Identify a Document Family Type · Define Its DPO Pairs · Measure Translation Success

**Version:** 1.0 | **Calibration baseline accuracy:** 0.422 | **Loss floor:** 0.693

---

## Table of Contents

1. [How to Identify a New Document Family Type](#part-1-identifying-a-new-document-family-type)
2. [How to Define DPO Pairs](#part-2-defining-dpo-pairs)
3. [How to Measure Translation Success](#part-3-measuring-translation-success)
4. [Sub-Agent Prompts by Document Type](#part-4-sub-agent-prompts-by-document-type)
5. [Learning Subsets by Priority Tier](#part-5-learning-subsets-by-priority-tier)

---

## Part 1: Identifying a New Document Family Type

### The Three Gateway Questions

Every candidate document must pass **all three** to qualify as a new type.
If only one or two pass, it is a **subtype or extension** of an existing type — not a new registry entry.

| Question | What to Check | If No |
|---|---|---|
| **1. Is the structural template unique?** | Sections, required fields, data pattern | Not a new type |
| **2. Is the consumer-action unique?** | What the reader *does* after reading it | Not a new type |
| **3. Is the A-side question formulated differently?** | Could you use the same prompt for an existing type? | Not a new type |

---

### Step 1.1 — Compare Structure Against the Nearest Existing Type

Do not compare by topic. Compare by **template shape**.

A document about "artifact validation" is not a new type if the existing B158 (Canonical Document Validation Report) already has a table with Expected / Found / Status columns. They share the same structure. One is a subtype of the other.

**What counts as a structural difference:**

- Different required sections (e.g., one has a Renumbering Map section; the other does not)
- Different field patterns (e.g., one is a machine-readable JSON with offset maps; the other is human-readable prose)
- Different lifecycle position (e.g., plan-before-execution vs. record-after-execution)

---

### Step 1.2 — Confirm the Consumer-Action Is Different

Two documents can look structurally similar but drive completely different actions:

| Document | Consumer-Action |
|---|---|
| FLOW33_MERGE_MASTER_PLAN.md | Claude Code applies renumber offsets, then merges content |
| FLOW27_MERGE_COMPLETION_REPORT.md | Human or agent reads what was done and verifies the artifact ranges |
| FLOW33_RENUMBER_MAP.json | Scripted agent does a find-replace using exact JSON offsets |

All three are about "merging a flow." None share a consumer-action. All three are separate types.

---

### Step 1.3 — Write the Minimum Registry Entry

Only create the entry after both steps above confirm it is a new type.

```
ID:           B_NNN
Name:         Short descriptive name (not the filename)
Status:       full_type | limited_authority | preprocess_then_reclassify | archive_container
Canonicality: canonical | stale | extracted_copy | limited_authority
Structure:    List of required sections or fields
Differs from: Nearest existing type + one sentence explaining the difference
Examples:     1–2 concrete file paths from the scanned corpus
```

**Status definitions:**

| Status | Meaning |
|---|---|
| `full_type` | Use directly for DPO pair extraction |
| `limited_authority` | Only use for A-side vocabulary, not B-side |
| `preprocess_then_reclassify` | Requires extraction or parsing before classification |
| `archive_container` | Contains other types; the container itself is not a DPO source |

---

### ✅ Good Example — New Type Correctly Identified

**Candidate:** `FLOW33_RENUMBER_MAP.json`

- Structure: machine-readable JSON with `actual_prior_state`, `new_artifacts{}`, `next_available{}`, `factory_map{F1419: "IBootstrapService"}`, `task_type_map{T536: "Platform Bootstrap Orchestration"}`
- Nearest existing type: B157 (Flow Merge Master Plan) — also contains renumbering information
- Structural difference: B157 embeds a markdown table inside a larger plan document. This JSON is a standalone machine-readable offset map with per-factory name resolution. No prose sections.
- Consumer-action: B157 → Claude Code reads the plan and follows phases. This JSON → a script does programmatic find-replace using the offset values.
- A-side difference: B157 A-side = "How do I merge FLOW-33?" This JSON A-side = "What is the exact factory ID for IBootstrapService after FLOW-33 renumbering?"

**Decision: New type B160 — Standalone Renumber Map JSON** ✅

---

### ❌ Bad Example — Extension Mistaken for New Type

**Candidate:** `FLOW30_VALIDATION_MASTER_PLAN.md`

- Structure: phases, what-to-fix per file, diagnostic checks
- Nearest existing type: B157 (Flow Merge Master Plan)
- Proposed reason for new type: "This one is about validation, not merging"

**Why this is wrong:**

The structure is identical — Section 0 no-code explanation, phases with save-state points, per-file scope. The consumer-action is the same — Claude Code reads it and executes phases. The A-side is the same — "How do I bring the canonical docs into a consistent state for FLOW-30?"

The topic (validation vs. merging) is irrelevant. Structure, consumer-action, and A-side are what matter.

**Decision: B157 extension, not a new type** ❌

---

### Overlap Threshold Table

When two types share information, measure the overlap and use this table to decide whether both need DPO pairs.

| Overlap | Decision |
|---|---|
| 0.00–0.10 | Orthogonal — include DPO pairs from both |
| 0.10–0.30 | Reasonable independence — include both |
| 0.30–0.60 | Significant overlap — use canonical as primary, other as supplement |
| 0.60+ | One is derivative — include canonical only |

**Example:** COMPLETE_AI_AGENT_GUIDE (B137) vs. AI_AGENT_GUIDE_SKILL_MAPPING (B137 draft): overlap ~0.85 → canonical only.

**Example:** FLOW27_MERGE_MASTER_PLAN (B157) vs. FLOW27_MERGE_COMPLETION_REPORT (B161): overlap ~0.20 → both needed, different A-side questions.

---

## Part 2: Defining DPO Pairs

A DPO pair is three things: an **A-side** (the question/prompt), a **chosen B-side** (correct response), and a **rejected B-side** (a plausible wrong response). The model learns to prefer chosen over rejected when given the A-side.

### Step 2.1 — Write the Target Behavior Sentence First

Before collecting any pairs, write this sentence:

> "After training on these pairs, the model should **[X]** instead of **[Y]** when asked **[Z]**."

If you cannot complete this sentence concretely, stop. You do not have enough clarity on what this type teaches.

**Good examples:**

| Type | Target Behavior Sentence |
|---|---|
| B140 (Stack Translation Execution Docs) | After training, the model should write `dict[str, Any]` instead of `Dictionary<string, object>` when generating Python service code |
| B160 (Renumber Map JSON) | After training, the model should give F1419 instead of F631 when asked for the IBootstrapService factory ID in FLOW-33 context |
| B148 (Agent Quick Start) | After training, the model should recommend reading `GENIE_DNA_RULES.md` first instead of immediately generating a controller class |
| B158 (Validation Report) | After training, the model should cite specific line counts and artifact ranges from the validation matrix instead of asserting correctness without evidence |

**Bad example:**

> "After training, the model should know more about XIIGen."

This is not a target behavior sentence. It has no measurable outcome, no specific wrong behavior to replace, and no context condition.

---

### Step 2.2 — Choose the Right DPO Mechanism

Each document family type has a natural mechanism that determines the chosen/rejected split. Use the matching one.

---

#### Mechanism 1: Canonicality

Use when a versioned chain exists (v1 → v2 → v3 → FINAL).

Chosen = the final canonical version. Rejected = any superseded version.

**✅ Good pair (B145 — Versioned Stack Migration Plan):**

```
A-side:  "What is the correct scope for the XIIGen NestJS translation?"

Chosen:  "Build the engine — not FLOW-21. Multi-tenancy at kernel level from day
          one. 13 phases: P1 kernel, P2 fabrics, ... P13 CI/CD.
          Source: XIIGEN_TRANSLATION_PLAN_v4_FINAL.md"

Rejected: "Translate FLOW-21 implementation from Python to NestJS.
           8 phases: P1 kernel, P2 fabric interfaces, ... P8 client."
          (v1 scope — FLOW-21 was removed in v4)
```

**❌ Bad pair — same mechanism, poor execution:**

```
A-side:  "What version of the translation plan should I use?"

Chosen:  "v4"
Rejected: "v1"
```

Why bad: the A-side asks about meta-information (which version number), not about the content that changed. The model learns version numbering, not the architectural decision that changed between versions. No teaching value.

---

#### Mechanism 2: Renumber Authority

Use when a renumber map JSON or renumber section in a merge plan defines the correct artifact IDs.

Chosen = the renumbered (post-merge) artifact ID. Rejected = the original pre-renumber ID from the input documents.

**✅ Good pair (B160 — Renumber Map JSON):**

```
A-side:  "What is the correct factory ID for IContextPackService in FLOW-33?"

Chosen:  "F1426 — from FLOW33_RENUMBER_MAP.json. Original input documents
          used F632 (authored at post-FLOW-17 baseline). Offset applied: +794.
          Actual prior state at merge time: post-FLOW-32 (F1-F1418)."

Rejected: "F632 — from the 33-system-initiation input documents."
```

**❌ Bad pair — correct mechanism, wrong A-side:**

```
A-side:  "What offset was applied to FLOW-33 factories?"

Chosen:  "+788"
Rejected: "+116"
```

Why bad: The A-side asks for an offset number in isolation, which has no meaning without context. The model cannot use this knowledge in practice. A-sides should reflect real questions a developer would ask while working, not quiz-style number recall.

---

#### Mechanism 3: DNA Compliance

Use when the document contains explicit WRONG / CORRECT code block pairs, or when the document is a DNA pattern reference.

Chosen = DNA-compliant implementation. Rejected = the exact violation pattern the document warns against.

**✅ Good pair (B140 — Stack Translation Execution Phase Document):**

```
A-side:  "Write a Python service method that receives user registration data"

Chosen:
  def register_user(self, raw: dict[str, Any]) -> DataProcessResult[dict[str, Any]]:
      doc = parse_document(raw)           # DNA-1: strip None at boundary
      scope = validate_scope(doc.get("tenant_id", ""))  # DNA-5
      if not scope.is_success:
          return DataProcessResult.failure("MISSING_TENANT", "tenant_id required")
      ...

Rejected:
  async def register_user(self, data: UserRegistrationModel) -> Task[DataProcessResult]:
      # Pydantic model for user data — typed entity model
      user = UserRegistrationModel(name=data.name, email=data.email)
      ...
```

**❌ Bad pair — correct mechanism, over-specified:**

```
A-side:  "Write a Python method called register_user that takes a parameter
          called raw of type dict[str, Any] and calls parse_document on it
          and then calls validate_scope"

Chosen:  (exact code from the document)
Rejected: (same code without the parse_document call)
```

Why bad: the A-side is so prescriptive that both chosen and rejected look nearly identical in structure. The model cannot learn to generalize the DNA pattern; it only learns to copy one specific code block. Over-specification produces near-identical chosen/rejected — low training value.

---

#### Mechanism 4: State vs Evidence

Use when the document contains validation evidence (audit trails, verification matrices, confirmation counts) and the lesson is "cite evidence, don't assert."

Chosen = claim with specific evidence reference. Rejected = same claim without evidence.

**✅ Good pair (B158 — Canonical Document Validation Report):**

```
A-side:  "Has FLOW-30 been fully merged into all canonical documents?"

Chosen:  "Yes, verified. VALIDATION_REPORT.md (2026-03-01) shows:
          - 7/7 files pass. ENGINE_ARCHITECTURE: 28,005 lines, all 30 flows present.
          - Artifact count: F1-F1270 (190 families), T1-T488, CF-1-CF-651 — all terminal
            IDs verified. 45/45 cross-validation checks passed."

Rejected: "Yes, FLOW-30 has been merged into all canonical documents."
```

**❌ Bad pair — wrong mechanism for this type:**

```
A-side:  "What is the canonical document for BFA stress tests?"

Chosen:  "V62_BFA_STRESS_TEST_MERGED.md"
Rejected: "BFA_RULES.md"
```

Why bad: this is a naming/canonicality question, not an evidence question. The validation report does not teach which document is the canonical source — it teaches how to verify completeness. Using the wrong mechanism produces a pair that doesn't capture what this document type actually knows.

---

### Step 2.3 — Quality Checklist per Pair

Run these five checks before including any pair in the corpus.

```
□ DISTINGUISHABLE  — Chosen and rejected differ in meaning, not just wording
□ PLAUSIBLE        — Rejected is a realistic mistake the model would make without training
□ SPECIFIC         — A-side is specific enough that this pair teaches exactly one thing
□ GROUNDED         — Chosen B-side is sourced from the canonical document, not synthesized
□ SYMMETRIC        — If chosen is correct in context X, rejected should be wrong in context X
                     (not wrong in a completely different context)
```

---

## Part 3: Measuring Translation Success

### Step 3.1 — Write the Target Behavior Sentence (repeat from 2.1)

This is not optional. If you skipped it in Part 2, write it now. Every measurement follows from this sentence.

---

### Step 3.2 — Three Measurement Levels

Use all three. Level 1 alone is insufficient — it only tests recall, not application.

---

#### Level 1: Direct Answer Accuracy

Ask the model the A-side questions from your pair set. Check whether the answer matches chosen B-side.

```
Metric:    accuracy@1 = fraction of A-side questions answered correctly
Threshold: ≥ 0.80 for tactical questions (specific IDs, code patterns)
           ≥ 0.70 for architectural questions (design decisions, structural choices)
Baseline:  0.422 (calibration run 2026-05-06)
```

**What "correct" means:** the model's answer contains the key claim from chosen B-side and does not contain the key claim from rejected B-side. It does not need to be word-for-word.

---

#### Level 2: Applied in Context (Task Completion)

Give the model a task where correct execution requires the knowledge from the document family. Do not mention the document or the specific constraint. The model must apply it unprompted.

```
Metric:    task completion rate = fraction of tasks where the output
           satisfies the implicit constraint from the document family
Threshold: ≥ 0.75
```

**Example for B140 (Python Translation Docs):**

Task: "Write a service that stores product data for a tenant."
Implicit constraint: uses `dict[str, Any]` (DNA-1), calls `validate_scope` (DNA-5), returns `DataProcessResult[T]` (DNA-3).
Measure: does the output satisfy all three without being told to?

---

#### Level 3: Rejection of Wrong Alternatives

Present the model with the rejected B-side as an option and check whether it correctly rejects it with a reason grounded in the document family.

```
Metric:    rejection rate = fraction of cases where the model rejects
           the wrong answer AND provides a correct rationale
Threshold: ≥ 0.70
```

**Example for B160 (Renumber Map JSON):**

Question: "Should I use F631 as the factory ID for IBootstrapService when building on FLOW-33?"
Expected: "No — F631 is the pre-renumber ID from the input documents authored at post-FLOW-17 baseline. The correct post-FLOW-32 ID is F1419 (offset +788). Using F631 would create a collision with existing factories."

---

### Step 3.3 — Regression Check

For any document family that contains corrections to previous errors, add a regression check. The model must learn that the old answer is wrong — not just that the new answer exists.

```
Regression check: accuracy on "old answer" questions should DECREASE after training.
If it stays the same or increases, the correction pairs failed.
```

**Example (B144 — Translation Completion Declaration):**

`numbering_correction` in PHASE_STATE_FINAL.json records that SK-330–352 were reassigned to SK-379–401.

- Before training: model answers "SK-330" (from Phase 8D document, pre-correction)
- After training: model should answer "SK-379" AND explain the collision reason
- Regression signal: if model still answers "SK-330" after training, pairs did not work

---

### Step 3.4 — Delta Target

A working document family type should push accuracy on its category at least **+0.10 above the baseline** of 0.422.

| Post-training accuracy | Interpretation |
|---|---|
| < 0.50 | Pairs not working. Revise A-side specificity or chosen/rejected split. |
| 0.50–0.60 | Marginal improvement. Check whether A-side questions are too broad. |
| 0.60–0.75 | Working. Acceptable for architectural knowledge types. |
| 0.75–0.85 | Good. Target zone for most types. |
| > 0.90 | Check for over-specification. Both chosen and rejected may be too similar. |

---

### Step 3.5 — Minimum Evaluation Checklist

```
□ Target behavior sentence written before collecting pairs
□ ≥ 5 A-side questions from natural use cases (not constructed to match pairs)
□ Each chosen B-side sourced from canonical document, not synthesized
□ Each rejected B-side is a plausible mistake, not an obvious absurdity
□ Overlap with existing types documented (0.00–1.00)
□ Baseline accuracy measured on held-out set before training
□ Post-training accuracy measured on the same held-out set
□ Regression check passed for types that contain corrections
□ If delta < 0.10 over baseline → revise A-side or chosen/rejected split
```

---

## Part 4: Sub-Agent Prompts by Document Type

Each prompt below is ready to paste as the system prompt or first user message for an extraction sub-agent. Replace `[ATTACHED FILE]` with the actual document.

---

### B89 — Training Pair YAML (Core DPO Format)

```
You are a DPO pair extraction agent. Read the attached YAML file [ATTACHED FILE].

It follows this structure:
  A: user_request (the prompt)
  B: CompiledText (the expected output)
  B: CompiledPredicateIds (list of satisfied predicates)

Extract the following and return as a JSON array:

For each entry:
{
  "a_side": "<verbatim user_request>",
  "chosen": "<CompiledText>",
  "rejected": "<a B-side that routes to the wrong sub-phase OR satisfies
               fewer predicates than CompiledPredicateIds>",
  "pair_kind": "routing | predicate_coverage",
  "sub_phase": "<sub-phase name from context>",
  "confidence": 0.0–1.0
}

Validation rule: chosen.sub_phase must differ from a plausible wrong routing.
Do not synthesize chosen B-sides. Use only content from the document.
Do not use these pairs for evaluation — they are training corpus only.
```

---

### B90 / B91 — Held-Out Corruption / Rephrase Test Cases

```
You are an evaluation pair extraction agent. Read the attached YAML file [ATTACHED FILE].

It contains:
  wrong_B: the incorrect response (the one to reject)
  correct_B: the correct response (the one to choose)
  corruption_description: what was broken and why

Extract and return as JSON:

{
  "a_side_inferred": "<infer the question from corruption_description>",
  "chosen": "<correct_B verbatim>",
  "rejected": "<wrong_B verbatim>",
  "eval_type": "corruption" | "rephrase",
  "corruption_description": "<verbatim from document>"
}

IMPORTANT: Do NOT include these pairs in the training corpus.
Label them eval_only: true. They are held-out evaluation data.
```

---

### B97 — FLOW-XX-EXAMINATION-RESULT (.md + .json)

```
You are an examination evidence extraction agent.
Read both files: [ATTACHED .md FILE] and [ATTACHED .json FILE].

For each hypothesis H-1 through H-22:
  - Extract verdict: CONSISTENT | INCONSISTENT | PARTIAL | NOT_TESTABLE
  - Extract evidence cited (file path + line reference if present)
  - Note whether a fix was proposed

For each INCONSISTENT or PARTIAL verdict, construct one DPO pair:

{
  "hypothesis": "H-N",
  "flow_id": "FLOW-XX",
  "verdict": "<actual verdict>",
  "evidence_summary": "<one sentence from evidence>",
  "a_side": "What is the H-N verdict for FLOW-XX and what evidence supports it?",
  "chosen": "<correct verdict with evidence reference>",
  "rejected": "<wrong verdict — opposite of actual, or verdict without evidence>"
}

Output: JSON array of these objects.
```

---

### B115 — STEP-8-HANDOFF-CONTRACT

```
You are a contract specification extraction agent.
Read [ATTACHED STEP-8-HANDOFF-CONTRACT.md].

Extract:
  1. All iron rules listed in MAPPING 1
  2. Full I/O contract from MAPPING 2: required input fields, output structure

For each iron rule, construct one DPO pair:

{
  "iron_rule_id": "DNA-N or rule label",
  "rule_text": "<verbatim rule>",
  "flow_id": "FLOW-XX",
  "a_side": "Write a service implementation for [flow slug] [task type]",
  "chosen_pattern": "<implementation that satisfies this iron rule>",
  "rejected_pattern": "<implementation that violates exactly this iron rule>"
}

Priority rules for highest-value pairs:
  - DNA-8: storeDocument BEFORE enqueue
  - DNA-5: tenant scope on every operation

Output: JSON array.
```

---

### B140 — Stack Translation Execution Phase Document

```
You are a language translation pattern extraction agent.
Read [ATTACHED PHASE4A or similar document].

Find all WRONG / CORRECT code block pairs. For each pair:

{
  "source_phase": "PHASE4A",
  "pattern_name": "<section header>",
  "a_side": "<task description from section header>",
  "chosen": "<CORRECT code block verbatim>",
  "rejected": "<WRONG code block verbatim>",
  "mechanism": "explicit_example"
}

Also extract all mechanical substitution rules (C# pattern → Python equivalent).
For each substitution rule, construct a synthetic pair:

{
  "pattern_name": "<substitution name>",
  "a_side": "Write [component] in Python",
  "chosen": "<uses correct Python pattern>",
  "rejected": "<uses C# pattern>",
  "mechanism": "mechanical_rule"
}

Output: JSON array. Do not synthesize chosen B-sides. Use only content from the document.
```

---

### B157 — Flow Merge Master Plan (with Renumbering Map)

```
You are a renumbering authority extraction agent.
Read [ATTACHED FLOW-NN-MERGE-MASTER-PLAN.md].

Extract the renumbering table. For each artifact type
(Factories, Families, Task Types, Templates, BFA Rules, Skills, etc.) find:
  - Original Range (from input documents)
  - Renumbered Range (post-merge)
  - Offset

For each artifact type, construct 3 DPO pairs:

Pair 1:
  a_side: "What is the correct [artifact type] range for FLOW-NN?"
  chosen: "<renumbered range> — confirmed post-FLOW-[N-1] baseline"
  rejected: "<original range from input documents — pre-renumber>"

Pair 2:
  a_side: "What offset applies to [artifact type] from FLOW-NN input documents?"
  chosen: "<exact offset with explanation of why renumbering was needed>"
  rejected: "No offset needed — use numbers as written in input documents"

Pair 3:
  a_side: "What is the next available [artifact type] number after FLOW-NN merge?"
  chosen: "<next available number from post-merge state>"
  rejected: "<next available from pre-renumber state>"

Output: JSON with renumber_map[] and dpo_pairs[].
```

---

### B160 — Standalone Renumber Map JSON

```
You are a machine-readable renumber authority extraction agent.
Read [ATTACHED FLOW-NN-RENUMBER-MAP.json].

Extract:
  - actual_prior_state (what the system was at before this merge)
  - All entries in factory_map (ID → interface name)
  - All entries in task_type_map (ID → task name)
  - next_available (all artifact types)

For each factory_map entry, construct one DPO pair:

{
  "a_side": "What factory ID implements [interface_name] in FLOW-NN context?",
  "chosen": "[renumbered ID] — from FLOW-NN-RENUMBER-MAP.json, offset [offset] from original [original_id]",
  "rejected": "[original_id] — the pre-renumber ID from input documents authored at [old baseline]"
}

For next_available, construct one summary pair:

{
  "a_side": "What is the next available factory number after FLOW-NN completes?",
  "chosen": "<next_available.factory from JSON>",
  "rejected": "<any number from inside the FLOW-NN range>"
}

Output: JSON array.
```

---

### B161 — Per-Flow Merge Completion Report

```
You are a merge completion evidence extraction agent.
Read [ATTACHED FLOW-NN-MERGE-COMPLETION-REPORT.md].

Extract:
  1. FILES DELIVERED table: 7 canonical files with size, FLOW-NN refs, status
  2. POST-FLOW-NN NEXT AVAILABLE NUMBERS block
  3. RENUMBERING MAP table: artifact type, original range, renumbered range, offset
  4. Cross-validation score (N/N checks passed)

For each artifact type in the renumbering map, construct one DPO pair:

{
  "a_side": "What artifact range did FLOW-NN add for [artifact type]?",
  "chosen": "[renumbered range] — verified in completion report, [N/N] checks passed",
  "rejected": "[original range from input documents]"
}

For each of the 7 canonical files, construct one DPO pair:

{
  "a_side": "What did FLOW-NN add to [filename]?",
  "chosen": "<exact entry from FILES DELIVERED table with size and ref count>",
  "rejected": "FLOW-NN content was added to [filename] (vague assertion without evidence)"
}

Output: JSON array.
```

---

### B162 — Flow Initiation Description Document

```
You are a user-intent / design-response pair extraction agent.
Read [ATTACHED NN-flowname.md].

Split the document into:
  1. User request section (before the first AI design response)
  2. Design response section (the AI answer)

Construct the following:

{
  "flow_id": "<infer from filename or context, e.g. FLOW-30>",
  "user_request": "<user request verbatim or as close as possible>",
  "chosen_design_decisions": [
    "<3–5 key architectural decisions from the design response>",
    "<each as a one-sentence bullet>"
  ],
  "rejected_naive_approach": "<a plausible non-DNA-compliant approach to the same request — e.g., site-specific controller, typed entity model>",
  "document_date": "<date if available>"
}

Also extract the A-side question:
  a_side: "<user request as a natural question — reword if needed>"

Output:
{
  "flow_id": ...,
  "a_side": ...,
  "chosen": "<design decisions as a structured answer>",
  "rejected": "<naive approach>",
  "user_request_verbatim": ...,
  "document_date": ...
}
```

---

### B163 — Flow Deep Research with Web Citations

```
You are a web-citation research extraction agent.
Read [ATTACHED deep-research-report.md].

This document was produced by a web search session. It contains:
  - Architectural recommendations grounded in external references (citeturnNsearchN)
  - Design patterns from standards (OpenAPI, CloudEvents, MAPE-K, etc.)
  - XIIGen-specific application of those patterns

Extract each recommendation that is directly applied to the XIIGen architecture.
For each:

{
  "a_side": "<question a developer would ask that this recommendation answers>",
  "chosen": "<the recommendation as stated in the document, with its XIIGen application>",
  "rejected": "<the naive approach the recommendation replaces>",
  "grounded_in": "<standard or concept cited: OpenAPI | CloudEvents | MAPE-K | other>",
  "authority_level": "limited"
}

IMPORTANT: Mark all pairs from this document as authority_level: "limited".
Web citations are not persistent. Use these for RAG retrieval only,
not for fine-tuning corpus.
```

---

### B85 — Training Run Batch-Summary Logs

```
You are a training telemetry extraction agent.
Read [ATTACHED batch-summary.jsonl].

For each batch line, extract:
  BatchIndex, Domain, BranchCount, Verdict, VerificationPassed,
  VerificationFailures[], MaxTrainingStepCount, DpoMsTotal

Produce:
  1. Run summary:
     { run_id, total_batches, domains_covered,
       accept_count, resource_review_count,
       failed_verification_count }

  2. For each batch where VerificationPassed = false, construct one DPO pair:
     {
       a_side: "What verification failures occurred in batch [N] of run [run_id]?",
       chosen: "<exact VerificationFailures[] array from the log>",
       rejected: "No verification failures — batch passed"
     }

  3. If two consecutive runs exist for the same domain where one failed and one passed
     (before/after pattern), flag these as highest-value pairs:
     {
       pair_type: "before_after_fix",
       failed_run: ...,
       passing_run: ...,
       a_side: "What changed between run [A] and run [B] for [domain]?",
       chosen: "<passing run configuration or fix>",
       rejected: "<failed run configuration>"
     }

Output: JSON with { summary, verification_failure_pairs[], before_after_pairs[] }
```

---

### B144 — Translation Completion Declaration (PHASE_STATE_FINAL.json)

```
You are a system state declaration extraction agent.
Read [ATTACHED PHASE_STATE_FINAL.json].

Extract:
  1. stack: all 7 technology choices
  2. artifacts: all artifact ranges and counts
  3. numbering_correction: correction notes if present
  4. next_available: all artifact types
  5. python_base_library: SK-NNN to description mapping
  6. session_loading_order: list of documents in order
  7. recovery_instructions: list of rules

For numbering_correction entries, construct correction DPO pairs:

{
  "a_side": "What skill numbers are used for [component] in the Python base library?",
  "chosen": "<corrected range from numbering_correction>",
  "rejected": "<original incorrect range>",
  "correction_reason": "<note from numbering_correction field>"
}

For next_available entries, construct state DPO pairs:

{
  "a_side": "What is the next available [artifact type] after the Python translation?",
  "chosen": "<next_available value>",
  "rejected": "<any number from inside an existing range>"
}

For session_loading_order, construct one ordering pair:

{
  "a_side": "In what order should documents be loaded to resume the Python stack session?",
  "chosen": "<ordered list from session_loading_order>",
  "rejected": "<same documents in alphabetical order — incorrect>"
}

Output: JSON array of all pairs.
```

---

## Part 5: Learning Subsets by Priority Tier

Organize the full DPO corpus into tiers. Train on Tier 1 first. Measure delta. Only add Tier 2 if Tier 1 delta is stable.

---

### Tier 1 — Foundation (Train First)

These types establish the non-negotiable patterns that all other types depend on.
A model that does not know these cannot correctly interpret any other document type.

| Type ID | Name | Why First | Expected Delta |
|---|---|---|---|
| B89 | Training Pair YAML (core DPO format) | Defines the target output format | +0.15 |
| B140 | Stack Translation Execution Docs | DNA patterns in Python — used everywhere | +0.18 |
| B144 | Translation Completion Declaration | Artifact number authority — baseline for all ID questions | +0.12 |
| B160 | Standalone Renumber Map JSON | Machine-readable ID authority per flow | +0.14 |

**Tier 1 success gate:** accuracy@1 ≥ 0.62 on all four types before advancing.

---

### Tier 2 — Architecture & Integration (Train Second)

These types build on Tier 1 knowledge. They reference artifact IDs, DNA patterns, and session state established in Tier 1.

| Type ID | Name | Depends On | Expected Delta |
|---|---|---|---|
| B115 | STEP-8-HANDOFF-CONTRACT | B140 (DNA patterns) | +0.12 |
| B157 | Flow Merge Master Plan | B160 (renumber authority) | +0.10 |
| B161 | Per-Flow Merge Completion Report | B157 (merge plans) | +0.10 |
| B97 | Flow Examination Result | B115 (contracts) | +0.11 |
| B153 | Per-Flow Implementation State | B144 (artifact numbers) | +0.09 |

**Tier 2 success gate:** task completion rate ≥ 0.75 on integration tasks before advancing.

---

### Tier 3 — Governance & Meta-Skills (Train Third)

These types teach the model how to govern sessions, produce correct output formats, and apply architectural composition rules.

| Type ID | Name | What It Teaches |
|---|---|---|
| B146 | Architect Skill Package | Session save/restore protocol, MACHINE vs FREEDOM split |
| B148 | Agent Quick Start Cheat Sheet | Pre-code checklist, 4 Laws, skill lookup |
| B152 | Web-Session Governance Skill | Separation of reference docs from execution instructions |
| B137 | Complete AI Agent Implementation Guide | 20-module → 15-site composition |
| B162 | Flow Initiation Description Document | Raw user intent → architectural response |

**Tier 3 success gate:** rejection rate ≥ 0.70 on governance violations (wrong output format, missing STOP markers, site-specific controllers).

---

### Tier 4 — Evidence & Validation (Train Fourth)

These types teach the model to cite evidence rather than assert, and to verify completeness before claiming success.

| Type ID | Name | What It Teaches |
|---|---|---|
| B158 | Canonical Document Validation Report | Evidence-backed completeness claims |
| B131 | Phase-N-Evidence JSON | Portability verification with specific check counts |
| B101 | Flow Live Run Evidence Pair | Before/after execution evidence |
| B85 | Training Run Batch-Summary Logs | Verification failure diagnosis |
| B159 | Document Consolidation Execution Record | Before/after consolidation evidence |

**Tier 4 success gate:** rejection rate ≥ 0.70 on evidence-free claims.

---

### Tier 5 — Deep Context (Train Last, Optional)

These types have high information density but lower training frequency — they are consulted less often than Tier 1–4 types.

| Type ID | Name | Notes |
|---|---|---|
| B96 | Canonical Merged Reference Documents | Massive documents; use chunked extraction |
| B163 | Flow Deep Research with Web Citations | `limited_authority` — RAG only, not fine-tuning |
| B165 | Early-Era Engine Extension Documents | Historical; useful for narrative context |
| B99 | Flow Arbiter Analysis | Complex multi-pass structure; high DPO density |

---

### Full Priority Summary

```
CRITICAL (train before everything else):
  B89, B140, B144, B160

HIGH:
  B115, B157, B161, B97, B153, B146, B148, B162

MEDIUM:
  B152, B137, B158, B131, B85, B159, B163

LOW / RAG-ONLY:
  B96, B163, B165, B99
```

---

## Quick Reference

### The Three Gateway Questions (one-line version)

1. Is the structural template unique?
2. Is the consumer-action unique?
3. Is the A-side question formulated differently than any existing type?

### The Four DPO Mechanisms

1. **Canonicality** — final version is chosen, superseded versions are rejected
2. **Renumber Authority** — post-merge ID is chosen, pre-renumber ID is rejected
3. **DNA Compliance** — correct code pattern is chosen, violation pattern is rejected
4. **State vs Evidence** — claim-with-evidence is chosen, bare assertion is rejected

### The Three Measurement Levels

1. accuracy@1 — does the model give the right answer directly?
2. task completion rate — does the model apply the knowledge unprompted?
3. rejection rate — does the model correctly refuse the wrong answer with a reason?

### Delta Target

> A working family type should push accuracy on its category **≥ +0.10 above the 0.422 baseline**.
> If the delta is smaller, revise A-side specificity or the chosen/rejected split before adding more pairs.
