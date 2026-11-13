# XIIGen Internal Source Registry
# Version: 7.0 — 2026-05-08
# Supersedes: XIIGEN-INTERNAL-SOURCE-REGISTRY-v6.md

---

## What changed from v6

**Canonicality framework added as a top-level section.** The assessment identified
that document type alone is insufficient — the same file can be a canonical source,
a mirror copy, an extracted snapshot, a stale version, or an archive container. Each
canonicality class has different rules for what can and cannot be used as training
material. The framework now precedes the type taxonomy.

**Type 23 — Gap remediation and fix documents.** U14 from the extended scan. Documents
that record a gap, the attempted wrong fix, and the correct fix. This is a three-part
correction structure unique in the corpus — no other type has the gap identification,
wrong-fix attempt, and correct-fix evidence in one document. Primary source for BUG
and ARCH repair pairs where the rejected is the wrong fix, not just a wrong domain.

**Type 24 — Generated code debug and correction histories.** U23 from the extended
scan. Debug traces and repair histories from design-to-code or code generation sessions.
Natural BUG and REFACTOR correction pairs where the original generated code is rejected
and the corrected version is chosen. Distinct from Type 3 correction events because
the defect is in generated code, not in a Claude response to a developer prompt.

**Zone mapping rules extended.** Assessment identified four additional bundle families
that need zone decomposition rather than new types:
- U15 visual UI quality evidence → Zone A (visual findings → Type 8), Zone B
  (role/screen maps → UI/ARCH B-side), Zone C (UML coverage → architecture evidence)
- U16 flow generator guidance library → Zone A (GUIDE-B procedure steps → PLAN B-side),
  Zone B (coverage reports → Type 8/13), Zone C (source maps → provenance only)
- U20 benchmark / stress / dry-run logs → Zone A (actual run metrics → Type 4),
  Zone B (expected traces → plan/test authority), Zone C (dry-run reports → evidence
  with status flag)
- U21 prompt handoff and session briefs → Zone A (session start gates → Type 15),
  Zone B (prompt templates → prompt-template authority), Zone C (handoff contracts →
  PLAN/agent ordering)

**Exclusion rules extended.** U17 (worktree mirrors), U18 (certificates and
environment material), U19 (code repository snapshots), and U22 (external training
sets) all added to the global exclusion section with explicit rules. U22 is
particularly important: external DPO pair batches must never silently mix into
internal teaching. They require the external registry and separate canonicality
tracking.

**Type 6 additions from v6 preserved.** ZIP archives and raw merged text dumps
remain in the exclusion section with their preprocessing rules.

**Quick-reference table extended to 24 rows.**
**Pre-submission checklist extended with Type 23–24 checks and canonicality gate.**

---

**Document type taxonomy expanded from 16 to 22 types.** U01–U13 from the uncovered
types scan were assessed and resolved:

**Type 17 — Translation and migration plans.** Cross-stack migration documents
with explicitly sequenced phases. The best available source of PLAN Type B pairs
because prerequisite language is already written in the document — the annotation
is extracted, not constructed. Companion STATE.json files record interrupted phases
which are correction events in machine-readable form.

**Type 18 — Flow portability and protocol documents.** The only source of
invariant-based rejected pairs. A portability violation can be derived mechanically:
take any correct flow, add one tenant-specific hardcode, and the rejected is
complete. The protocol document names the check that the violation fails.

**Type 19 — Agent tooling instructions.** Machine-facing behavior contracts
(AGENTS.md, CLAUDE.md). Separated from Type 14 (user behavioral rules) and
Type 15 (session gates) because the audience is different: these are executable
contracts for a tool, not behavioral preferences for a human. Source of PLAN
within-task ordering pairs and ArbiterUnit tool-compliance pairs.

**Type 20 — Skill library documents.** Skill definitions and manifests. Type C
rejected only — skill documents define domain boundaries but not the relationship
between adjacent sub-phases. Type A rejected requires Type 5 (topology) as authority,
not Type 20.

**Type 21 — Deep research and external domain reports.** Most restricted new type:
CODE_Q pairs only. External research is not ground truth for XIIGen decisions and
must never override Type 9 locked decisions. Requires a specific cited external
source in the annotation.

**Type 22 — Execution state JSON files.** Machine-readable workflow execution
progress records. Separated from Type 4 (DPO training metrics) because these record
which phases completed and which artifacts were produced — not model learning numbers.
Two subtypes: 22a (missing field, overlap < 0.20) and 22b (interruption correction
event, overlap 0.10–0.25).

**Type 23 — Business flow feature specifications (zone mapping rule, not a new type).**
Documents like 01-user-registration.md contain three distinct zones that require
different pair construction rules: Zone A (acceptance criteria → ArbiterUnit rules),
Zone B (architectural decisions → ARCH Type A rules), Zone C (task ordering →
PLAN Type B rules). Treating them as uniform Type 2 was producing wrong pair types.

**U10 and U13 added to Type 6 (excluded with preprocessing rules).** ZIP archives
must be unpacked and their contents reclassified by content type. Raw merged text
dumps must be segmented by delimiters before any extraction.

**Quick-reference table extended to 22 rows.**

**Pre-submission checklist extended with Type 17–22 checks.**

---

**Type 7 — A-side rule added.** v4 showed examples where the A side is a developer
prompt with intent verb "does / is / review", but did not state this as an explicit
rule. v5 adds: A side for Type 7 pairs is always a Type 1 prompt with those intent
verbs. Status document text is B-side evidence only. Ineligible patterns listed.

**Type 8 — Clean pass example added.** v4 had only concern-vs-pass and
reprocess-vs-concern examples. Without a pass example, the model develops a bias
toward flagging correct artifacts. v5 adds a third pair: clean artifact with explicit
dependency, chosen = pass with assertive finding, rejected = fabricated concern with
hedging language. Explains why the asymmetry causes overflagging.

**Type 10 — ArbiterUnit A-side corrected.** v4 used raw teaching ledger text as
the ArbiterUnit A side (`"teaching ledger showing 5 of 10..."`). This violates the
`ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>` contract. v5 replaces it with
a properly shaped `ArbiterReviewRequest` object containing `artifact_type`,
`artifact_content`, `review_criteria`, and `source_rule` fields. Distinction between
PLAN pairs (developer prompt as A) and ArbiterUnit pairs (contract object as A) is
now explicit.

**Type 11 — Split into three subtypes.** v4 mixed three structurally different
rejected patterns under one type with a single overlap ratio (0.10–0.20). This was
wrong: the wrong-formula subtype (11b) has overlap 0.40–0.55 and requires the
annotation to carry most of the reward margin signal, while the missing-layer
subtype (11a) has overlap < 0.15 and works without strong annotation. v5 defines:
— 11a: missing layer (overlap < 0.15, annotation useful but not critical)
— 11b: wrong formula (overlap 0.40–0.55, token overlap test mandatory, annotation
   must introduce ≥ 5 consequence-describing non-overlapping tokens)
— 11c: wrong field split (overlap 0.20–0.30, annotation helpful)
Quick-reference table and pre-submission checklist updated accordingly.

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

---

## Canonicality framework

Every XIIGen document must be assigned a canonicality class before any pair is
constructed from it. The type taxonomy governs what kind of pairs a document can
produce. The canonicality class governs whether this particular copy of the document
is a valid source at all.

```
canonical          The authoritative live version. Pair construction is allowed
                   subject to type rules.

mirror             An exact or near-exact copy of a canonical document in a
                   different location (worktree, extracted subdirectory, old folder).
                   Do not use as a training source — use the canonical copy.
                   If the canonical source is missing, promote the mirror to
                   canonical before use.

extracted_copy     A file extracted from a ZIP or repository snapshot, placed in
                   a working directory for inspection. Valid for pair construction
                   only if the extraction is dated and the canonical archive is
                   also recorded.

stale              A versioned document where a newer version exists (confirmed via
                   the document state register or "What changed" header). Use only
                   as rejected-vocabulary source. Never as chosen source.

external           Originates outside XIIGen — external research, open-source
                   documentation, external DPO pair batches. Governed by the
                   external source registry, not this registry. Must not silently
                   mix into internal teaching.

archive_container  A ZIP, repository snapshot, or merged text dump that bundles
                   other documents. Not a training source itself. Must be unpacked
                   and each extracted file classified separately.

secure_operational Certificates, private keys, environment credentials, and
                   deployment secrets. Excluded from all training regardless of
                   content type.
```

**How to record canonicality in the pair file:**

Every pair file must include:

```json
{
  "source_type": "Type 17",
  "source_canonicality": "canonical",
  "source_path": "XIIGEN_NESTJS_REACT_TRANSLATION_PLAN_v2.md",
  "source_version": "v2",
  "canonical_confirmed_via": "DOCUMENT-STATE-REGISTER-2026-05-06-v8.md"
}
```

A pair file where `source_canonicality` is `mirror`, `stale`, or
`archive_container` is rejected at the submission gate regardless of pair quality.

---

## Document type taxonomy

Every XIIGen internal document belongs to one of twenty-four types. The type determines
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

**A-side rule for Type 7 pairs — mandatory:**

The A side is always a Type 1 developer prompt. It must use one of these intent
verbs: `"does / is / review / check / verify"`. The status document itself never
becomes the A side — it is authority evidence for the B side only.

Eligible A-side patterns:
```
"does X use Y?"                        — CODE_Q state-accuracy pair
"is E34V2 fusion implemented?"         — CODE_Q implementation check
"review the retrieval path"            — 6.1 architecture review pair
"check whether E12 SIMD is in place"   — 6.4 code review pair
```

Ineligible A-side patterns (status document text used directly):
```
"E34V2 +8% hit@1 rank fusion: alpha × BM25_score + (1-alpha) × KNN_score"
"EXPERIMENT-IMPLEMENTATION-STATUS: E34V2 = NOT IMPLEMENTED"
```
These are B-side evidence, not prompts. Any A side that contains a status field
name, an experiment ID, or a file path is Type 6 material — exclude it.

**Overlap ratio:** 0.10–0.25. Chosen uses status vocabulary (NOT IMPLEMENTED,
zero hits, no production class, gap). Rejected uses capability vocabulary (implements,
uses, combines). Near-zero overlap between these vocabularies.

**Pair types extractable:** CODE_Q (accurate state description), 6.1/6.4 review
findings, ArbiterUnit pairs (pass when status is correctly reported; concern when
status is wrong).

**Excluded use:** Never use status document text as A side. Never claim a
capability is implemented unless the status field is IMPLEMENTED and the evidence
path (the file/class name) is confirmed present in the actual codebase.

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

**Third pair type from this source — clean artifact, pass verdict:**

Without pass examples, the model sees only concern and reprocess pairs and develops
a bias toward flagging everything. A clean artifact must produce `verdict: pass`.
The rejected is a fabricated concern where no gap exists.

```
Contract: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>

A (arbiter review request):
  artifact: a PLAN where 3.4 Security is branch 1, 4.1 Backend is branch 3,
            and 4.1 explicitly states "depends on branch 1"
  review_criteria: "verify dependency ordering is semantically correct"

Chosen (ArbiterUnit — pass: relationships hold):
  verdict: pass
  finding: "3.4 Security is correctly ordered before 4.1 Backend. The dependency
            is explicit: branch 3 states 'depends on branch 1'. 4.1 cannot
            implement auth middleware before 3.4 defines the validation rules.
            The plan resolves this correctly."

Rejected (ArbiterUnit — concern: fabricated gap):
  verdict: concern
  finding: "3.4 and 4.1 may need to be reviewed for alignment. There could be
            assumptions in 4.1 that depend on 3.4 decisions not yet made."
[WRONG: "could be" and "may need" are not findings — they are absence-of-checking.
The artifact explicitly states the dependency. When the dependency is present and
correct, the verdict is pass. A concern verdict requires an identified gap, not
a hypothetical risk. Applying concern to a clean dependency chain trains the
model to flag correct work as problematic.]
```

**Why this pair is necessary:** The two existing examples (concern vs pass, reprocess
vs concern) train the model to distinguish severity levels when a problem exists.
This third example trains the model to recognise when no problem exists. Without it,
the model learns only "when there is ambiguity, choose concern or reprocess" without
learning the condition that produces pass. The result is a model that overflags.

**Overlap ratio:** 0.20–0.35 for pass vs fabricated-concern pairs. Both verdicts
contain relationship vocabulary (depends on, ordered before, requires). The
difference is in the certainty vocabulary: chosen uses assertive language (correctly
ordered, explicit, resolves), rejected uses hedging language (may need, could be,
not yet made).

**Overlap ratio:** 0.15–0.35 across all three pair types. Chosen uses verdict-type
vocabulary (reprocess, structural defect, currently wrong, must be corrected; or
pass, correctly ordered, explicit). Rejected uses softer or wrong severity vocabulary.

**Pair types extractable:** ArbiterUnit B sides for all three verdict types (pass,
concern, reprocess), Node 08 decisions, structural verification pairs.

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

Teaching plans contain explicit stopping rules. These are arbiter pass criteria.

**A-side rule for ArbiterUnit pairs:** The A side must be an `ArbiterReviewRequest`
object matching the `ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>` contract.
The teaching ledger data populates the artifact field inside the request — it is
not the A side itself.

```
Contract: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>

A (ArbiterReviewRequest — correct shape):
  artifact_type: "teaching_ledger_summary"
  artifact_content:
    total_rows: 10
    promoted: 5
    held: 3
    rejected: 2
    consecutive_rejected_tail: [7, 8, 9]
  review_criteria: "apply teaching plan stopping rule — 7 of 10 threshold"
  source_rule: "ORCHESTRATOR-TEACHING-PLAN-v3.md row-10 stop gate"

Chosen (ArbiterUnit B — reprocess: stopping rule triggered):
  verdict: reprocess
  finding: "5 of 10 decisions are promoted — below the 7-of-10 stopping
            threshold. Three consecutive rejected decisions at rows 7, 8, 9
            indicate a repeating pattern failure in the training data.
            Teaching must stop at row 10."
  requirement: "identify the systematic problem in the training data before
               proceeding to row 11"

Rejected (ArbiterUnit B — pass: ignores stopping rule):
  verdict: pass
  finding: "50% promotion rate is within acceptable range for early-stage
            training. Continue to row 11 to gather more data."
[WRONG: the stopping rule is explicit — fewer than 7 of 10 promoted triggers
a stop. 5 of 10 is below the threshold regardless of whether it "seems
acceptable". Continuing past the stopping rule without identifying the
systematic problem accumulates wrong behavior on top of wrong behavior.
The rule exists precisely to prevent this. The arbiter's role is to enforce
the rule, not to apply judgment about whether the number "seems" acceptable.]
```

**The distinction between PLAN pairs and ArbiterUnit pairs from this source:**
- PLAN pairs: A side is a developer intent prompt (`"plan the first teaching cycle"`).
  B side is a branch plan with dependency ordering.
- ArbiterUnit pairs: A side is an `ArbiterReviewRequest` object with the artifact
  and the review criteria. B side is an `ArbiterVerdict`. The A side is never raw
  ledger text.

**Overlap ratio:** 0.15–0.30 for PLAN dependency pairs. 0.10–0.20 for arbiter
stopping-rule pairs (stopping vocabulary vs continuation vocabulary).

**Pair types extractable:** PLAN ordering pairs (harness before teaching),
ArbiterUnit stopping-rule pairs with correct contract shape,
CorrectionLearningUnit boundary pairs.

**Excluded use:** Never use step descriptions as A sides for PLAN pairs.
Never use raw teaching ledger text as ArbiterUnit A side — wrap it in an
ArbiterReviewRequest object with the review_criteria and source_rule fields.

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

**Type 11 pairs have three structurally different subtypes. Each has a different
overlap ratio and annotation requirement. Treat them as 11a, 11b, 11c.**

---

**Subtype 11a — Missing layer (overlap < 0.15, annotation useful but not critical)**

The rejected omits an entire measurement layer. Chosen contains fields that rejected
does not have at all — very low overlap. The annotation explains why the missing
layer matters, but the token distribution difference is already strong.

```
A (ModelTrainingUnit input):
  feature_vector_dataset: 93 rows, domain BUG, batch 3/63
  training_settings: { max_steps: 1, lora_rank: 2, lora_alpha: 4 }

Chosen (11a — all three layers present):
  model_artifact_path: "...batch-0003-bug/bug/3-2/policy.bin"
  training_row_count: 93
  training_accuracy: 0.2751
  teaching_time_ms: 66485.9
  cpu_percent: 13.9
  working_set_delta_mb: 12.8
  managed_allocation_delta_mb: 71633.2
  checkpoint_bytes_delta: 1531426
  lora_adapter_bytes: 4096
  dpo_loss: 0.6945
  chosen_reward: 0.01569
  rejected_reward: 0.01838
  reward_margin: -0.00269
  warnings: ["negative_reward_margin", "accuracy_below_threshold"]

Rejected (11a — Layer 1 resource fields absent):
  model_artifact_path: "...batch-0003-bug/bug/3-2/policy.bin"
  training_row_count: 93
  training_accuracy: 0.2751
  dpo_loss: 0.6945
[WRONG: a training result that omits resource measurements (cpu_percent,
working_set_delta_mb, managed_allocation_delta_mb, checkpoint_bytes_delta)
cannot be used to assess whether the training step is within operational
bounds. TEACHING-MEASUREMENT-GUIDANCE-v4.md requires all three layers.
A result with only accuracy and loss provides no signal about memory
pressure, disk growth, or convergence health.]
```

**Overlap ratio: < 0.15.** Chosen has cpu_percent, working_set_delta_mb,
managed_allocation_delta_mb, lora_adapter_bytes, reward_margin fields. Rejected
has none of them. The distributional difference is large without a strong annotation.

---

**Subtype 11b — Wrong formula (overlap 0.40–0.55, annotation carries most of the signal)**

Both chosen and rejected contain the same numbers. The formula is different but
the vocabulary is largely shared. This is the highest-overlap subtype in the
document — it behaves like a Type B pair (wrong order within correct structure).

**Apply the token overlap test before submitting any 11b pair.** If
`overlap_ratio > 0.50`, the annotation must be strong enough to carry the margin
by itself — the content difference alone will not produce positive margin.

```
A: "compute cpu_percent for the training step on a 4-core machine,
    elapsed_ms=96214, total_cpu_time_ms=53344"

Chosen (11b — correct formula):
  cpu_percent = (total_cpu_time_ms / elapsed_ms / Environment.ProcessorCount) × 100
  = (53344 / 96214 / 4) × 100 = 13.9%
  [Formula divides by processor count so the result stays within 0–100%
   on any multi-core machine.]

Rejected (11b — missing processor count division):
  cpu_percent = (total_cpu_time_ms / elapsed_ms) × 100
  = (53344 / 96214) × 100 = 55.4%
[WRONG: the formula must divide by Environment.ProcessorCount. Without the
division, a 4-core machine reports cpu_percent > 100% when more than one
core is used. This was fixed in TEACHING-MEASUREMENT-GUIDANCE-v4.md (P2-7).
The wrong formula produces 55.4% — a number that appears to show high CPU
pressure when the actual per-core load is 13.9%. An operational alert
calibrated against the correct formula would fire incorrectly on every
multi-core training step if the wrong formula is used.]
```

**Overlap ratio: 0.40–0.55.** Both sides contain `total_cpu_time_ms`, `elapsed_ms`,
`96214`, `53344`, and arithmetic operators. The annotation is what carries the signal:
it introduces `processor count`, `multi-core`, `100%`, `per-core load`, `operational
alert` — vocabulary present in chosen but absent in rejected. Without the annotation
this pair will likely produce negative margin.

**Strong annotation requirement for 11b:** The annotation must introduce new tokens
that appear in the context of the correct formula but not the wrong formula. Naming
the consequence (reports > 100%, fires incorrectly, wrong calibration) is more
effective than naming the rule (divide by ProcessorCount) because it introduces
more non-overlapping tokens.

---

**Subtype 11c — Wrong field split (overlap 0.20–0.30, annotation helpful)**

Chosen uses split fields (three separate measurements). Rejected uses a combined
field that merges them. Moderate overlap because both sides reference the same
underlying artifact path — but the field names are different.

```
A: "record the model artifact size fields for a teaching step"

Chosen (11c — split fields):
  base_checkpoint_bytes: new FileInfo(checkpointPath).Length
  lora_adapter_bytes: sum of (Rank × InDim + OutDim × Rank) × 4 per LoraLinear
  dpo_metric_bytes: sum of dpo_step_*.json file lengths in checkpoint directory

Rejected (11c — combined field):
  model_artifact_bytes: new FileInfo(checkpointPath).Length
[WRONG: ModelArtifactBytes was split into three fields in v4 (P2-6).
The combined field mixes base weights with adapter weights and metric files.
LoraAdapterBytes is what the D-05 stopping rule tests (2MB adapter limit).
A combined field cannot be compared against an adapter-only threshold —
it would require knowing what fraction of the combined size is adapters,
which is not recorded. The stopping rule becomes unenforceable.]
```

**Overlap ratio: 0.20–0.30.** Both sides reference the same checkpoint path.
The difference is field names (split vs combined) and the formula for lora_adapter_bytes.

---

**Overlap ratio summary for Type 11:**

| Subtype | What differs | Overlap | Annotation weight |
|---------|-------------|---------|-------------------|
| 11a — missing layer | entire fields absent | < 0.15 | useful but not critical |
| 11b — wrong formula | formula with same numbers | 0.40–0.55 | carries most of the signal |
| 11c — wrong field split | field names and count | 0.20–0.30 | helpful |

**For 11b pairs specifically:** run the token overlap test before submitting.
If `overlap_ratio > 0.50`, revise the annotation to introduce at least 5
additional non-overlapping tokens before submitting.

**Pair types extractable:** ModelTrainingUnit B sides (11a, 11c), computation
verification pairs (11b), HeldOutEvaluationUnit B sides, CorrectionLearningUnit
B sides, ArbiterUnit stopping-rule pairs for resource bounds.

**Excluded use:** Never use guidance text as A side. Never use guidance field
definitions as developer-language prompts. For 11b pairs, never submit without
running the token overlap test.

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

### Type 17 — Translation and migration plans

**What it is:** Cross-stack migration documents and their companion state files.
Plans that describe how to move an existing system from one technology or architecture
to another, in explicitly sequenced phases with dependency constraints between them.

**Examples in the XIIGen corpus:**
```
XIIGEN_NESTJS_REACT_TRANSLATION_PLAN.md
XIIGEN_NESTJS_REACT_TRANSLATION_PLAN_v2.md
DETAILED_TRANSLATION_PLAN.md
TRANSLATION_PHASE_PLAN.md
PHASE_STATE.json  (companion state to translation phases)
TRANSLATION_STATE.json
```

**Why this is a distinct type:** Migration plans are the most explicit source of
PLAN Type B dependency ordering in the entire corpus. Every phase is preceded by a
mandatory prerequisite already written in the document — the annotation is extracted,
not constructed.

STATE.json companion files record which phases completed and which were interrupted.
An interrupted-then-restarted phase is a machine-readable correction event: the
interrupted attempt is rejected, the completed restart is chosen.

**How to use for PLAN Type B pairs:**

```
A (Type 1 prompt):
  "plan the migration of the xiigen engine from NestJS to Python —
   start with the core infrastructure layer"

Chosen (PLAN — correct migration order):
  Branch 1 — 3.2 Data Model (order 1):
    Define the Python equivalents of the core entity classes. No dependencies.
    This is the foundation everything else references.

  Branch 2 — 3.3 API Contract (order 2):
    Define the Python service interfaces and method signatures.
    Dependency: 3.2 must be complete — interface signatures reference entity types.

  Branch 3 — 4.1 Backend Services (order 3):
    Implement the Python classes.
    Dependency: both 3.2 and 3.3 must be complete — implementation targets
    the defined entities and interfaces.

Rejected (PLAN — wrong order: implementation before interfaces):
  Branch 1 — 4.1 Backend Services (immediate)
  Branch 2 — 3.3 API Contract (after implementation)
  Branch 3 — 3.2 Data Model (derived from what was implemented)
[WRONG: implementing before the interface is defined means the implementation
targets its own assumptions. When 3.3 produces different method signatures from
what 4.1 assumed, 4.1 must be rewritten. The TRANSLATION_PHASE_PLAN.md explicitly
orders: entity schema → interface contract → implementation. Reversing this order
always produces rework at the interface boundary.]
```

**How to use STATE.json interruption events:**

```
Interrupted state in JSON:
  { "phase": "3.3_api_contract", "status": "interrupted",
    "reason": "wrong contract shape — missing pagination fields",
    "attempted_artifact": "api-contract-v1.md" }

Completed restart in JSON:
  { "phase": "3.3_api_contract", "status": "completed",
    "artifact": "api-contract-v2.md",
    "correction": "added cursor-based pagination contract per 3.3 spec" }

Extraction:
  A:        phase prompt — "define the API contract for the reporting service"
  Rejected: content of api-contract-v1.md
            [WRONG: the contract shape was missing cursor-based pagination.
            The 3.3 spec requires pagination contract before implementation begins.]
  Chosen:   content of api-contract-v2.md
```

Rate before including: interruption from wrong approach = High. Interruption from
missing file = Low (exclude — not a reasoning correction).

**Expected overlap ratio:** 0.30–0.45 for Type B ordering pairs.

**Pair types extractable:** PLAN Type B (primary), correction events from STATE.json
interruptions, SchemePlannerUnit B-sides.

**Excluded use:**
```
Do not use migration plan phase descriptions as A sides.
Do not use STATE.json field values as chosen content without wrapping in
the correct measurement schema.
```

---

### Type 18 — Flow portability and protocol documents

**What it is:** Documents that define invariant requirements every flow must satisfy
regardless of tenant, environment, or deployment target. Fork execution directives,
portability test protocols, tenant isolation contracts.

**Examples in the XIIGen corpus:**
```
FLOW-PORTABILITY-TEST-PROTOCOL-*.md
FORK-EXECUTION-DIRECTIVE-*.md
flow-prerequisites docs
portability / authorization protocol docs
```

**Why this is a distinct type:** Portability protocol documents are the only source
of invariant-based rejected pairs. A portability violation is derived mechanically:
take any correct flow, add one tenant-specific hardcode, and the rejected writes itself.
The protocol document names the check that the violation fails.

This is different from Type 9 decisions: a decision is a choice between alternatives.
A portability protocol is a contract with no alternative — it must be satisfied or the
flow is rejected.

**How to use for ArbiterUnit pairs:**

```
A (ArbiterReviewRequest):
  artifact_type: "flow_service_file"
  artifact_content: "flow service with hardcoded tenantId = 'tenant-abc' in constructor"
  review_criteria: "verify portability: no hardcoded tenant identifiers"
  source_rule: "FLOW-PORTABILITY-TEST-PROTOCOL check P-3"

Chosen (ArbiterUnit — reprocess: portability violation):
  verdict: reprocess
  finding: "hardcoded tenantId in constructor violates the portability contract.
            The tenantId must come from IExecutionContext.TenantId, not from a
            constant. This flow will fail for any tenant other than 'tenant-abc'.
            FLOW-PORTABILITY-TEST-PROTOCOL check P-3 requires zero hardcoded
            tenant identifiers."
  requirement: "replace constant with IExecutionContext.TenantId injection
               before this flow can be promoted"

Rejected (ArbiterUnit — concern: wrong severity):
  verdict: concern
  finding: "tenantId is hardcoded. Consider making it configurable."
[WRONG: a hardcoded tenant identifier is a portability violation that causes wrong
output for every other tenant — not a concern to be noted. The correct verdict is
reprocess, not concern. A concern verdict allows promotion; reprocess blocks it.
Downgrading a violation to a concern means this flow reaches production with a defect
that silently fails for all tenants except one.]
```

**How to use for PLAN Type B pairs (fork semantics from FORK-EXECUTION-DIRECTIVE):**

```
Chosen: fork after 3.4 completes; branches 4.1 and 4.2 run in parallel;
        join requires both to succeed before 5.2 starts

Rejected: fork immediately; 3.4, 4.1, 4.2 all parallel; 5.2 starts when first
          branch completes
[WRONG: FORK-EXECUTION-DIRECTIVE requires 3.4 (security model) to complete before
any implementation branches fork. 4.1 and 4.2 implement against the security
contract — they cannot run before the contract exists. Early fork creates
implementation that targets an unfinished spec.]
```

**Expected overlap ratio:** 0.10–0.20 for portability violation pairs. 0.30–0.45
for fork semantics pairs.

**Pair types extractable:** ArbiterUnit portability reprocess pairs, SchemePlannerUnit
fork/join ordering pairs, Stage 9 corruption cases.

**Excluded use:**
```
Never derive a portability violation from inference — only from a named check
in the protocol document.
Do not confuse with Type 9 decisions — protocols are contracts with no valid
alternative; decisions are choices between alternatives.
```

---

### Type 19 — Agent tooling instructions

**What it is:** Machine-facing behavior contracts for Claude Code or other agent tools
operating in the XIIGen codebase. These specify what tools to call, in what order,
when to stop, and what output format to produce.

**Examples in the XIIGen corpus:**
```
AGENTS.md
CLAUDE.md
copilot-instructions.md
ROUTING_GUIDE.md
```

**Why this is a distinct type:** Type 14 (behavioral rules) defines what the human
user must not do. Type 15 (session start prompt) defines how the human sets up a
session. Type 19 defines what the agent tool must do when given a task — it is an
executable contract, not a behavioral preference. The audience is the agent, not the
human.

**How to use for PLAN dependency-ordering pairs:**

AGENTS.md often contains explicit step sequences with dependencies:
"first read project knowledge, then check existing tests, then write code,
then run tests, then commit."

```
A (Type 1 prompt):
  "add the missing portability check to the flow service"

Chosen (PLAN — correct agent execution order per AGENTS.md):
  Branch 1 — reconnaissance (order 1):
    Read the flow service file. Read the portability protocol.
    Dependency: must read before writing — cannot know what to change
    without knowing what exists and what the protocol requires.

  Branch 2 — implementation (order 2):
    Write the portability fix.
    Dependency: reconnaissance must be complete.

  Branch 3 — verification (order 3):
    Run the portability check test.
    Dependency: implementation must be complete. Do not commit before green.

Rejected (PLAN — wrong order: write before read):
  Branch 1 — implementation (immediate): write the fix
  Branch 2 — run tests: fix failures as they appear
  Branch 3 — read the protocol: check what was required
[WRONG: AGENTS.md requires reconnaissance before any write. Writing without
reading the existing code and the protocol produces a fix that may solve the
symptom while violating a different portability rule. The protocol read must
happen before the implementation, not as a retrospective check.]
```

**Expected overlap ratio:** 0.15–0.30 for PLAN ordering pairs. 0.10–0.20 for
ArbiterUnit tool-compliance pairs.

**Pair types extractable:** PLAN Type B ordering pairs (reconnaissance before write),
ArbiterUnit tool-compliance pairs, SchemePlannerUnit branch ordering.

**Excluded use:**
```
Do not use AGENTS.md instruction text as A side.
A side must be a Type 1 developer task prompt.
Do not conflate with Type 15 (session gates) — Type 19 is within-task
execution order, not session setup.
```

---

### Type 20 — Skill library documents

**What it is:** Skill definitions, skill manifests, and prompt templates that define
reusable execution procedures for the agent. Each skill has a defined scope, a trigger
condition, and an output contract.

**Examples in the XIIGen corpus:**
```
SKILL.md files in skills directories
VXX_SKILLS_FACTORY.md
SKILLS_FACTORY_RAG.md
AF_SKILL_LIBRARY.md
planning-skills-v3-final/ directory contents
```

**Why this is a distinct type:** Skill documents define domain boundaries in
executable form. The rejected for Type 20 pairs is always Type C (wrong domain —
wrong skill applied). Type A rejected (adjacent sub-phase) is not appropriate here
because skill documents define what belongs to the skill, not the relationship
between adjacent sub-phases.

**Core rule: Type C rejected only.** Type A requires adjacent sub-phase vocabulary
from Type 5 (topology). Type 20 provides skill boundary language, which is different.

**How to use for Type C domain-boundary pairs:**

```
A (Type 1 prompt):
  "review the authentication implementation for the tenant service"

Chosen (QA — 6.3 Security and Compliance Review):
  The review checks whether the implementation satisfies the auth model (3.4).
  Findings: JWT validation present (pass), tenant claim isolation confirmed (pass),
  bypass-path registry not checked (concern — the security review skill requires
  bypass-path check in every security review).

Rejected (ARCH — wrong domain: design instead of review):
  The auth model for the tenant service should define the JWT structure: sub claim
  = userId, tenantId claim = tenant partition key, roles claim = RBAC array.
  Consider adding a refresh token family to prevent reuse.
[WRONG: "review the authentication implementation" asks whether the existing
implementation satisfies the model — 6.3 review scope. Describing what the model
should contain is 3.4 design scope. The skill library distinguishes these: the
security review skill checks compliance with a model that already exists; the
security model skill defines the model. Using a design skill for a review prompt
misses the compliance check entirely.]
```

**Type 20 annotation vs Type 5 annotation:** Type 5 cites the sub-phase Purpose
field ("3.4 owns..."). Type 20 cites the skill boundary ("the security review skill
checks compliance with an existing model; the security model skill defines it"). More
specific trigger condition language, not just ownership.

**Expected overlap ratio:** 0.15–0.30.

**Pair types extractable:** Type C domain-boundary pairs (primary), ArbiterUnit
skill-compliance verdicts.

**Excluded use:**
```
Do not use skill definitions as A sides.
Do not use skill manifests as chosen content for ARCH pairs.
Type C only — never Type A.
```

---

### Type 21 — Deep research and external domain reports

**What it is:** Research documents drawn from external sources — market analysis,
open source ecosystem surveys, competitive landscape, technology evaluation reports.
These documents describe the external world, not XIIGen's internal decisions.

**Examples in the XIIGen corpus:**
```
MARKETPLACE-ORGANIC-TRAFFIC-RESEARCH.md
MARKETPLACE-NEW-PLATFORM-DISCOVERIES.md
deep-research-report chat gpt.md
01-user-registration deep search.md
28-multi tenant deep-research-report 1.md
XIIGEN-AI-WORKFLOW-GENERATOR-ANALYSIS.md
```

**Critical restriction: CODE_Q pairs only.** External research is not ground truth
for XIIGen decisions. Using it as B-side authority for ARCH or PLAN pairs risks
training the model on external best practices that may conflict with Type 9 locked
decisions. If a research document contradicts a Type 9 decision, the decision wins
and the research is not used.

**How to use for CODE_Q pairs:**

```
A (Type 1 prompt):
  "how does multi-tenant isolation work in GraphQL federation"

Chosen (CODE_Q — accurate external mechanism):
  "In GraphQL federation each subgraph handles its own authorization. The gateway
   forwards the original request headers including auth tokens. Each subgraph must
   validate the tenant claim independently — the gateway does not enforce tenant
   isolation. If a subgraph omits tenant validation, any authenticated user from
   any tenant can query that subgraph's data. The Apollo federation spec (§4.2)
   states authorization is a subgraph responsibility, not a gateway guarantee."

Rejected (CODE_Q — oversimplified, incorrect isolation model):
  "GraphQL federation handles multi-tenant isolation at the gateway level. The
   gateway validates the tenant token once and all subgraphs inherit the validated
   tenant context automatically."
[WRONG: federation does not enforce per-subgraph isolation from the gateway. Each
subgraph must validate independently. The research document cites the Apollo
federation spec §4.2: authorization is a subgraph responsibility. The rejected
model would allow any authenticated tenant to query any subgraph that skips
its own validation.]
```

**The annotation must cite a specific external source section.** Without a citation,
the pair has no authority and becomes an opinion pair — exclude it.

**Quality gate for Type 21 pairs:**
```
□ Research document cites a specific external standard or spec section
□ Chosen content is verifiable against that external source
□ Domain is CODE_Q only
□ No locked Type 9 decision contradicts the chosen content
□ Research document is dated — check whether the cited spec has been superseded
```

**Expected overlap ratio:** 0.20–0.40. Strong annotation critical.

**Pair types extractable:** CODE_Q external mechanism pairs only.

**Excluded use:**
```
Never use as B-side authority for ARCH or PLAN pairs.
Never override a Type 9 locked decision with external research.
Never use undated research without a specific cited source.
"Consider X approach" without a spec citation is opinion — exclude.
```

---

### Type 22 — Execution state JSON files

**What it is:** Machine-readable workflow execution state files that record which
phases completed, which artifacts were produced, what the current checkpoint is,
and what failed or was interrupted.

**Examples in the XIIGen corpus:**
```
FLOW21_STATE.json
FLOW35_STATE_P2.json
MASTER_STATE.json
PHASE_STATE.json
TRANSLATION_STATE.json
STATE-consolidation.json
```

**Why this is distinct from Type 4:** Type 4 (calibration run data) records DPO
training metrics — loss, accuracy, reward margin, checkpoint bytes. These are numbers
that measure model learning. Type 22 records workflow execution progress — which phases
are `completed`, `in_progress`, `blocked`, or `interrupted`. Different data, same
missing-field pair logic.

**Two subtypes:**

**Subtype 22a — Missing field (overlap < 0.20)**

A state file that records phase completion without required artifact references is
incomplete. Chosen has all required fields; rejected omits them.

```
A: "record the completion state for phase 3.3 API Contract"

Chosen (complete state record):
  { "phase": "3.3_api_contract", "status": "completed",
    "artifact": "flow21/api-contract-v2.md",
    "completed_at": "2026-05-05T14:23:00",
    "correction": null }

Rejected (missing artifact path):
  { "phase": "3.3_api_contract", "status": "completed" }
[WRONG: a completed phase record without an artifact path cannot be used to verify
what was produced. The state file is the audit trail — without the artifact path,
there is no way to confirm the correct version was produced or to replay the phase
from this checkpoint.]
```

**Subtype 22b — Interruption correction event (overlap 0.10–0.25)**

An interrupted-then-restarted phase maps directly to a correction pair:

```
Interrupted state: { "status": "interrupted", "reason": "wrong contract shape",
                     "attempted_artifact": "api-contract-v1.md" }
Restarted state:   { "status": "completed", "artifact": "api-contract-v2.md",
                     "correction": "added pagination contract per 3.3 spec" }

A:        phase intent prompt
Rejected: attempted artifact content + [WRONG: from correction reason field]
Chosen:   completed artifact content
```

Use `correction_reason` field as annotation source. Do not use state files where
`status = "in_progress"` — only `completed` or `interrupted` states are stable.

**Expected overlap ratio:** 22a < 0.20; 22b 0.10–0.25.

**Pair types extractable:** ModelTrainingUnit B-sides (execution state evidence),
CorrectionLearningUnit pairs (22b), ArbiterUnit pairs (complete vs incomplete record).

**Excluded use:**
```
Do not use raw JSON field values as A sides.
Do not confuse with Type 4 (DPO training metrics).
Do not use in_progress states as chosen content.
```

---

### Business flow spec zone mapping (Type 2 extension)

**This is a handling rule within Type 2, not a separate type.**

Business flow feature specifications (01-user-registration.md, 04-post-publishing.md,
FLOW01_ENGINE_EXTENSION.md, BFA docs, TASK_TYPES_CATALOG files) contain three distinct
zones that require different pair construction rules. Treating them as uniform Type 2
produces wrong pair types.

**The three zones inside every business flow spec:**

**Zone A — Acceptance criteria and BFA requirements.** Markers: "must", "gate",
"pass/fail", "the flow is complete when". Apply ArbiterUnit pair construction rules.

**Zone B — Architectural decisions for this domain.** Markers: "owns", "must not",
"boundary", "depends on", "forbidden". Apply ARCH Type A pair construction rules,
same as Type 2.

**Zone C — Task type and sub-phase assignments.** Markers: "sub-phase", "task type",
"order", "depends_on", "parallel", "after". Apply PLAN Type B pair construction rules,
same as Type 10/17.

**Rule:** Before building any pair from a business flow spec, identify the zone of the
source paragraph. Zone A → ArbiterUnit rules. Zone B → ARCH Type A rules.
Zone C → PLAN Type B rules.

**Expected overlap ratio by zone:** Zone A: 0.15–0.30. Zone B: 0.10–0.20.
Zone C: 0.30–0.45.

---

### Type 6 additions — Excluded families from scan

**Source archives (*.zip files):** A ZIP file is a delivery artifact, not a document.
The ZIP itself has no DPO value. Unpack, classify each extracted file by content type
(Types 1–22), apply corresponding rules, delete temp extraction. Never treat a ZIP
filename as a document title for an A-side prompt.

**Raw merged text dumps (extracted_merged.txt, planning session.txt):** Concatenations
of multiple conversations or documents without segment boundaries. Using them directly
produces prompts that contain fragments from multiple unrelated contexts. Required
preprocessing: segment by known delimiters ("---", "Human:", "Assistant:", timestamps,
document headings), classify each segment independently, apply Type 1 eligibility test
per segment, discard ineligible segments. After preprocessing the segments are
reclassified as their content types — they are no longer Type 6.

---

### Type 23 — Gap remediation and fix documents

**What it is:** Documents that record a confirmed gap, the wrong fix that was
attempted or rejected, and the correct fix with evidence. These documents have a
three-part structure unique in the corpus.

**Examples in the XIIGen corpus:**
```
GAP-IMPLEMENTATION-PLAN-*.md
FIX-PLAN-*.md
REMEDIATION-PLAN-*.md
PER-FLOW-FIX-MAP-*.md
Missing gaps documents
GATE-B-FINDINGS.md (gap findings section)
```

**Why this is a distinct type:** No other type has gap identification + wrong-fix
attempt + correct-fix evidence in one document. Type 7 (experiment status) records
what is implemented vs not — it does not record why a fix attempt failed. Type 8
(artifact examination) records verdicts on artifacts — it does not record the repair
process. Type 23 records the full repair arc: gap → wrong approach → correct approach.

This is the primary source for BUG and ARCH repair pairs where the rejected is
not just a wrong domain but a specifically wrong repair strategy.

**How to use for BUG repair pairs:**

```
A (Type 1 prompt):
  "the AstRelevanceRanker BM25+KNN fusion is returning wrong scores —
   fix the ranking defect"

Chosen (BUG — correct fix from remediation plan):
  "The fusion alpha weight is computed per-query but applied to the global
   ranker state. Fix: move alpha computation inside the per-query scoring
   loop. Each query must compute its own alpha from the query predicate
   profile, not read the last-written global alpha.
   Evidence: REMEDIATION-PLAN confirms zero alpha drift after this fix."

Rejected (BUG — wrong fix from gap document's rejected attempt):
  "Reset alpha to 0.5 (the default) before each scoring call. This
   prevents the previous query's alpha from bleeding into the next query."
[WRONG: resetting to default alpha disables the query-adaptive tuning
entirely. The per-query alpha from E34V2 is what produces +8% hit@1 —
a global reset loses the gain. The correct fix moves computation inside
the loop while preserving per-query values. The remediation plan
explicitly records the reset attempt as a failed fix that introduced
worse accuracy than no fusion at all.]
```

**How to use for ARCH repair pairs (false-pass correction):**

Gap remediation documents often record a false pass — a change that appeared to
fix a gap but left a structural defect. The false pass is the rejected.

```
A (Type 1 prompt):
  "the portability check for flow 21 passes but the tenantId is still
   hardcoded in the constructor — plan the correct fix"

Chosen (ARCH — correct fix from fix map):
  "The hardcode is in the constructor parameter default, not in the body.
   The check scanned the body only. Fix: replace the default parameter
   with IExecutionContext injection and remove the default value entirely.
   The portability check must be updated to scan constructor parameter
   defaults, not only the body."

Rejected (ARCH — false-pass fix from gap doc):
  "Wrap the hardcoded value in a configuration lookup:
   tenantId = config.GetValue('default_tenant') ?? 'tenant-abc'.
   This removes the literal string from the constructor."
[WRONG: replacing a hardcoded literal with a config lookup still produces
a fixed tenant at construction time. The portability check sees no literal
string and passes — but the behavior is the same: one tenant baked in at
construction. The PER-FLOW-FIX-MAP records this as a false-pass pattern.
The correct fix removes the default entirely and injects from context.]
```

**Three-part structure extraction rule:**

Before building any pair from a gap remediation document:
1. Identify the gap description (what was wrong)
2. Identify the wrong-fix attempt (if present) — this becomes rejected
3. Identify the correct fix with evidence — this becomes chosen

If the document only has gap + correct fix (no wrong-fix attempt), construct
rejected using the most plausible wrong repair from the ARCH or BUG confusion
tables, not from the gap document itself.

**Expected overlap ratio:** 0.15–0.30 for gap repair pairs. Wrong-fix vocabulary
(reset, wrap, default, configure) vs correct-fix vocabulary (inject, per-query,
move inside loop, remove default) is moderately non-overlapping. Annotation from
the gap document's "why this failed" section is the primary signal.

**Pair types extractable:** BUG repair pairs (wrong fix vs correct fix), ARCH
false-pass correction pairs, ArbiterUnit pairs (wrong-fix attempt produces
reprocess verdict).

**Excluded use:**
```
Do not use gap description text as A side.
A side is always a Type 1 developer prompt.
Do not invent a wrong-fix attempt if the document does not record one —
use ARCH/BUG confusion tables instead.
Verify that the correct fix in the document has evidence attached (test
result, run output, or verification statement) — a fix without evidence
is a claim, not ground truth.
```

---

### Type 24 — Generated code debug and correction histories

**What it is:** Debug traces and repair histories from design-to-code or code
generation sessions. These record the original generated (wrong) code, the debug
process, and the corrected output.

**Examples in the XIIGen corpus:**
```
css-debug-history.json
generated code.txt (with correction annotations)
CORRECTED-DEFECT-CATALOG-*.md
GEOMETRIC-LEARNING-SYSTEM debug sessions
Figma-to-code conversion repair logs
```

**Why this is a distinct type:** Type 3 (correction events) covers corrections to
Claude's responses to developer prompts. Type 24 covers corrections to code that
Claude generated as output — the defect is in the generated artifact, not in the
response itself. The A side is the generation request, the rejected is the generated
code before debugging, the chosen is the code after correction.

This distinction matters for overlap ratio: Type 3 corrections have overlap < 0.25
because the wrong and correct answers come from different reasoning paths. Type 24
corrections often have higher overlap because the wrong and correct code share most
of their structure — only specific lines differ. The annotation must carry more of
the signal.

**How to use for BUG repair pairs:**

```
A (Type 1 prompt):
  "generate the CSS layout for the tenant dashboard — two-column grid
   with the sidebar collapsing below 768px"

Chosen (generated code — after debug correction):
  .tenant-dashboard {
    display: grid;
    grid-template-columns: 240px 1fr;
    gap: 16px;
  }
  @media (max-width: 768px) {
    .tenant-dashboard {
      grid-template-columns: 1fr;
    }
    .sidebar { order: 2; }
  }

Rejected (generated code — before debug, from css-debug-history.json):
  .tenant-dashboard {
    display: flex;
    flex-direction: row;
  }
  .sidebar { width: 240px; flex-shrink: 0; }
  @media (max-width: 768px) {
    .tenant-dashboard { flex-direction: column; }
  }
[WRONG: the flex implementation collapses the sidebar to full width at
768px but does not reorder it — the sidebar appears above content on
mobile. The grid implementation uses 'order: 2' to push sidebar below
the main content. The debug history records the flex version as the
defect that caused visual regression on mobile devices.]
```

**Quality gate for Type 24 pairs:**

Type 24 pairs have higher overlap than Type 3 because generated code shares
structure. Apply the overlap test strictly.

```
□ Overlap ratio computed before including
□ If overlap_ratio > 0.45: annotation must introduce ≥ 5 non-overlapping
  tokens describing why the wrong code fails functionally (not just which
  lines differ)
□ Debug history confirms the wrong code was actually generated (not
  constructed) — the rejected must come from a real generation event
□ The correct code has a verification signal in the debug history
  (build passed, test passed, visual confirmed)
```

**Expected overlap ratio:** 0.25–0.50. Generated code corrections share more
structure than conversation corrections. Annotation weight is higher than Type 3.

**Pair types extractable:** BUG generated-code repair pairs, REFACTOR generated-code
improvement pairs, UI/layout correction pairs for sub-phase 4.2 and 1.1.

**Excluded use:**
```
Do not use debug trace text (stack traces, browser console output) as A sides.
A side is always the original generation request prompt.
Do not include pairs where the "wrong" version is a deliberate placeholder —
the rejected must be genuinely wrong, not intentionally incomplete.
```

---

### Extended zone mapping rules (U15, U16, U20, U21)

These document families do not need new types. Each has multiple distinct zones
that map to existing pair construction rules. Identify the zone before building
any pair.

---

**Visual and UI quality evidence (U15)**

Documents: VIZ-QA files, UI-STATE-MAP, ROLE-SCREEN-MATRIX, GALLERY-DESIGN-SYSTEM,
UML_COVERAGE_ANALYSIS.

```
Zone A — Visual findings and defect reports
  Markers: "defect", "visual regression", "wrong color", "layout break",
           "accessibility violation", "fails visual check"
  Apply: Type 8 ArbiterUnit rules — findings are verdicts on artifacts.
  Chosen: concern or reprocess verdict with specific visual finding.
  Rejected: pass verdict without checking the visual relationship.

Zone B — Role/screen maps and design system references
  Markers: "role", "screen", "layout", "design token", "component spec"
  Apply: Type 2 (ARCH/UI) B-side rules for sub-phases 1.1 and 4.2.
  Use as chosen content for UI ARCH boundary pairs.

Zone C — UML and coverage analysis
  Markers: "UML", "coverage", "class diagram", "sequence diagram",
           "which screens are covered"
  Apply: Type 7 extension rules — coverage as implementation status.
  Use for knowledge-edge detection pairs (what is covered vs not).
```

---

**Flow generator guidance library (U16)**

Documents: GUIDE-B01 through GUIDE-B50 files, LIST-A-COVERAGE-REPORT,
LIBRARY-MASTER-INDEX.

```
Zone A — GUIDE-B procedure steps
  Markers: "Step N", "required before", "gate", "prerequisite"
  Apply: Type 10 PLAN ordering rules.
  Chosen: correct step order with dependency chain.
  Rejected: steps in wrong order or gate skipped.

Zone B — Coverage reports
  Markers: "covered", "missing", "gap", "LIST-A"
  Apply: Type 8 arbiter rules (coverage = verdict on completeness)
         or Type 13 document register rules (which guide is current).

Zone C — Source maps and library index
  Markers: "maps to", "source", "LIBRARY-MASTER-INDEX"
  Apply: Provenance only. Not a training pair source.
  Use for vocabulary calibration and source tracing.
```

---

**Benchmark, stress test, and dry-run logs (U20)**

Documents: V62_BFA_STRESS_TEST, REFERENCE-RAG-BENCHMARK-PLAN, DRY-RUN files,
EXECUTION-LOG, EXPECTED-TRACE.

```
Zone A — Actual run metrics (measured output)
  Markers: "elapsed_ms", "cpu_percent", "accuracy", "loss", recorded numbers
  Apply: Type 4 measurement evidence rules.
  Use as B-side content for ModelTrainingUnit pairs.

Zone B — Expected traces and reference plans
  Markers: "expected", "should produce", "reference output", "plan"
  Apply: Type 10/14 authority rules.
  These are the ground truth for what a correct run looks like —
  use as chosen content, with a run that diverges from expected as rejected.

Zone C — Dry-run and stress reports (planned but not yet measured)
  Markers: "dry run", "stress test plan", "hypothetical", "expected to"
  Apply: B-side authority with explicit status flag in the pair file:
         "evidence_type": "expected_not_measured"
  Do not use as Type 4 measurement evidence — these are plans, not results.
  Use only as test plan authority (Type 10 rules).
```

---

**Prompt handoff and session briefs (U21)**

Documents: PROMPTS.md, basic_prompt.txt, CLAUDE-CODE-SESSION-PROMPT,
HANDOFF-CONTRACT, SESSION-BRIEF.

```
Zone A — Session start gates (pre-work and post-work conditions)
  Markers: "before any work", "live test count", "manifest required",
           "gate", "must verify"
  Apply: Type 15 rules exactly.

Zone B — Prompt templates (reusable instruction patterns)
  Markers: "template", "insert here", "{{placeholder}}", "use this format"
  Apply: Type 20 (skill library) rules for domain boundary pairs.
  Prompt templates define what a task should look like — rejected is
  a prompt that violates the template's required structure.

Zone C — Handoff contracts and session briefs
  Markers: "handoff to", "next session", "carry forward", "context for"
  Apply: Type 19 (agent tooling instructions) rules for ordering pairs.
  Handoff contracts define what the receiving agent must do and in what
  order — rejected skips a required handoff step.
```

---

### Global exclusion rules (U17, U18, U19, U22)

**Worktree mirrors and duplicate copies (U17)**

Locations: `.claude/worktrees/`, `business-flows/` when duplicating
`business flows/`, `auth_extracted/`, `old copies/` subdirectories.

Rule: Mirrors are excluded as training sources. If a mirror and a canonical
source both exist, use the canonical source only. If the canonical source is
missing and only the mirror exists, promote the mirror to `extracted_copy`
canonicality and record the source gap before using it.

Never include both the canonical and a mirror of the same document in the same
batch — this creates contradictory training signal with near-identical token
distributions.

**Certificates and environment material (U18)**

Locations: `certificates/*.crt`, `certificates/*.csr`, `environment.zip`,
`env*.zip`, `.env` files.

Rule: Excluded from all training without exception. These are
`secure_operational` canonicality — no pair construction is permitted
regardless of content. Do not log these paths in pair manifests.

**Code repository snapshots with embedded docs (U19)**

Examples: `xiigen_v18_complete.zip`, `llm_mvp_core-main.zip`,
`orchestrator-mvp-main.zip`.

Rule: Repository snapshots are `archive_container` canonicality. The snapshot
itself is not a training source. Extract, then apply:

```
Source code files (.cs, .ts, .py)  → potential Type 5 (topology) or vocabulary
                                      calibration only; not direct pair source
Markdown docs embedded in repo    → classify by content type (Types 1–24)
Test files                         → Type 7/8 extension if they record
                                      pass/fail findings
Package manifests                  → provenance metadata only
```

Deduplicate against canonical copies before classifying extracted files.

**External training sets and DPO pairs (U22)**

Locations: `external-sources/`, `external-sources-training-sets/`,
`external-sources-training-sets 2/`, `dpo-pairs-*/`.

Rule: External DPO pair batches are `external` canonicality. They must not
silently mix into internal teaching. All external sources are governed by the
external source registry (`EXTERNAL-SOURCE-REGISTRY-v8.md`), not this registry.

The critical failure mode: an external pair batch that contains XIIGen session
prompts as A sides looks like internal material but was constructed with external
B-side authority. If mixed silently into internal teaching, the model receives
contradictory signal where the same A-side prompt produces different chosen
responses in different batches.

Required fields when recording an external pair in any batch:
```json
{
  "source_canonicality": "external",
  "source_registry": "EXTERNAL-SOURCE-REGISTRY-v8.md",
  "external_source_id": "wave-3-arch-adr",
  "mixed_with_internal": false
}
```

`mixed_with_internal: true` is a gate failure. External and internal pairs must
be in separate batch files.

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
| 6 | Excluded (session-loading, archives, dumps, mirrors, certs) | ✗ | ✗ | ✗ | — |
| 7 | Experiment status / code review | ✗ | ✓ accurate status | Overstated status | 0.10–0.25 |
| 8 | Artifact examination documents | ✗ | ✓ arbiter verdicts | Wrong severity level | 0.15–0.35 |
| 9 | Decision documents | ✗ | ✓ chosen decision | Rejected alternative from rationale | 0.05–0.20 |
| 10 | Teaching plan documents | ✗ | ✓ step ordering | Wrong step order | 0.15–0.30 |
| 11 | Measurement guidance documents | ✗ | ✓ measurement fields | 11a missing layer / 11b wrong formula / 11c wrong field split | see subtypes |
| 12 | Project history documents | ✗ | ✓ grounded historical claim | Invented motivation | 0.10–0.30 |
| 13 | Document state register | ✗ | ✓ current version / blocking order | Superseded version / parallel execution | 0.05–0.25 |
| 14 | Behavioral rules documents | ✗ | ✓ rule-compliant output | Forbidden pattern or missing Gate B | 0.05–0.25 |
| 15 | Session start prompt documents | ✗ | ✓ gate-complete session | Gate-skipped session | 0.10–0.20 |
| 16 | Versioned document evolution | ✗ | ✓ current-version vocabulary | Superseded-version vocabulary | 0.05–0.15 |
| 17 | Translation / migration plans | ✗ | ✓ correct phase order | Wrong phase order from same doc | 0.30–0.45 |
| 18 | Flow portability protocols | ✗ | ✓ portability-compliant flow | Hardcoded tenant violation | 0.10–0.20 |
| 19 | Agent tooling instructions | ✗ | ✓ correct tool execution order | Step skipped or wrong order | 0.15–0.30 |
| 20 | Skill library documents | ✗ | ✓ correct skill domain | Wrong skill — Type C only | 0.15–0.30 |
| 21 | Deep research / external reports | ✗ | ✓ accurate external mechanism | Simplified / outdated — CODE_Q only | 0.20–0.40 |
| 22 | Execution state JSON files | ✗ | ✓ complete state record | 22a missing field / 22b interrupted attempt | 0.10–0.25 |
| 23 | Gap remediation and fix documents | ✗ | ✓ correct fix with evidence | Wrong fix or false-pass attempt | 0.15–0.30 |
| 24 | Generated code debug histories | ✗ | ✓ corrected generated code | Pre-debug generated code | 0.25–0.50 |

**Key distinctions:**
- Types 12–16 all produce rejected from within their own corpus. Types 9, 12, 13, 16 frequently produce the same pair from different angles — use all four angles.
- Type 6 is the only type with no valid DPO use.
- Types 17 and 19 both produce PLAN Type B pairs. Type 17 rejected = wrong migration phase order. Type 19 rejected = skipped tool execution step. Both require functional annotations.
- Type 18 is the only type where rejected is mechanically derived — add a hardcode to a correct flow.
- Type 20 is Type C only. Never Type A. Adjacent sub-phase vocabulary comes from Type 5.
- Type 21 is CODE_Q only. ARCH or PLAN from Type 21 is a gate failure.
- Type 22 subtypes mirror Type 11a (missing field) and Type 3 (correction event) logic respectively.
- Type 23 differs from Type 7 and Type 8: it records the repair arc (gap → wrong fix → correct fix), not just status or verdict. If no wrong-fix attempt exists in the document, use ARCH/BUG confusion tables for rejected.
- Type 24 differs from Type 3: the defect is in generated code, not in a response. Overlap is higher (0.25–0.50). Annotation weight is higher.
- Zone mapping rules (Types 23 zone, U15, U16, U20, U21) are not types — they are extraction guides within existing types.
- External pairs (U22) must never mix into internal batches. `mixed_with_internal: true` is a hard gate failure.

---

## Pre-submission checklist

Before any pair from any internal source enters a batch:

```
□ Source type identified: Type 1–24 (zone rules apply within types)
□ Source canonicality confirmed: canonical or extracted_copy only
  — mirror, stale, archive_container, secure_operational → gate failure
  — external → must use external registry, must not mix with internal batch
□ source_canonicality field recorded in pair file with canonical_confirmed_via
□ Type 6 prompt detected and excluded (includes ZIP archives, raw dumps,
  worktree mirrors, certificates, environment material)
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
□ For Type 11 pairs: subtype identified (11a/11b/11c) before writing
  — 11a (missing layer): overlap < 0.15, annotation useful but not critical
  — 11b (wrong formula): overlap 0.40–0.55, token overlap test mandatory,
    annotation must introduce ≥ 5 non-overlapping tokens describing the consequence
  — 11c (wrong field split): overlap 0.20–0.30, annotation helpful
  For 11b: if overlap_ratio > 0.50 after annotation, revise before submitting
□ For Type 12 pairs: event cited (part number, date) — not invented from general knowledge
□ For Type 13 pairs: state is from a specific named version — not an average across versions
□ For Type 14 pairs: rule cited by number and version — not paraphrased from memory
□ For Type 15 pairs: forbidden output is the rejected — not a description of the rule
□ For Type 16 pairs: rejected uses earlier-version vocabulary that the newer version corrected
□ For Type 17 pairs: chosen phase order matches the migration plan's explicit prerequisite
  chain — verified against the plan document, not from memory. STATE.json interruption
  events rated High/Medium/Low before use (same as Type 3 correction events).
□ For Type 18 pairs: portability violation in rejected is derived from a named check
  in the protocol document — never invented. Chosen passes all named checks explicitly.
□ For Type 19 pairs: step sequence in chosen matches AGENTS.md / CLAUDE.md explicitly.
  Do not confuse with Type 15 (session gates) — Type 19 is within-task tool execution
  order. A side is always a Type 1 developer task prompt, never an instruction line.
□ For Type 20 pairs: rejected is Type C only (wrong domain / wrong skill). Type A is not
  permitted for this type. Annotation cites the skill boundary from the skill document.
□ For Type 21 pairs: domain is CODE_Q only — ARCH or PLAN pair from this type is a gate
  failure. Specific external source cited in annotation. No locked Type 9 decision
  contradicts the chosen content.
□ For Type 22 pairs: subtype identified (22a missing field / 22b interruption). STATUS
  field in source JSON is "completed" or "interrupted" — never "in_progress". For 22b:
  correction_reason field present and used in annotation.
□ For Type 23 pairs: three-part structure identified (gap / wrong fix / correct fix).
  If no wrong-fix attempt in document: use ARCH/BUG confusion tables for rejected —
  do not invent a wrong fix. Correct fix must have evidence (test result, run output,
  or verification statement).
□ For Type 24 pairs: debug history confirms wrong code came from a real generation
  event. overlap_ratio computed: if > 0.45, annotation must introduce ≥ 5 non-overlapping
  tokens describing why wrong code fails functionally. Correct code must have
  verification signal (build, test, or visual confirmation) in the debug history.
□ For zone-mapped pairs (U15/U16/U20/U21): zone identified (A/B/C) before pair
  construction. See extended zone mapping section for rules per zone.
□ For business flow spec zones (Type 2 zone mapping): Zone A → ArbiterUnit rules.
  Zone B → ARCH Type A rules. Zone C → PLAN Type B rules.
□ External source check: source_canonicality = external → separate batch file required.
  mixed_with_internal: false. external_source_registry field required in pair file.
□ reward_margin_mean >= 0 confirmed after batch runs before committing
```

---

## Last Updated

2026-05-08. Version 7.0.
