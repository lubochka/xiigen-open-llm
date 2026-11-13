# XIIGen Internal Source Registry
# Version: 11.0 â€” 2026-05-08
# Supersedes: XIIGEN-INTERNAL-SOURCE-REGISTRY-v10.md

---
## 2026-05-11 Internal Run-Experience Fix

Operational runbook:
`C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`

This guide must be read with the current run evidence in:
`C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\RUN-STATUS.json`

Current protected baseline: RUN-416 / V246, 672 rows, weighted accuracy 0.7068,
weighted reward margin 0.016892, domains orchestration 257 / plans 341 / skills 74.
Future additions must protect orchestration, plans, and skills first.

Hard fixes from the live run:

1. Do not use deterministic filename/path code to decide semantic authority.
   Scripts may inventory, validate, split, hash, and train. They may not decide
   that a user request, `basic_prompt.txt`, session-start prompt, genesis prompt,
   or flow initiation document is low-authority without model-led registry
   adjudication.
2. Root prompt and user-intent documents are potential prompt authority. They
   belong in model-led authority adjudication, not generic review-required, when
   the script cannot type them.
3. Train-ready pairs must satisfy: `chosen != rejected`, rejected contains a
   specific `[WRONG: ...]`, no mojibake in prompt/a_side/chosen/rejected, source
   span recorded, canonicality/version state explicit.
4. Ledger, status, audit, inventory, and progress rows are evidence surfaces.
   They may justify pairs but must not be treated as DPO rows themselves.
5. New or repaired sets must run as 16-64 row standalone ablations before combo
   promotion. Promote only if reward margin is positive and weighted accuracy
   does not damage the protected baseline.
6. Routing source entries that mention `PromptRoutingInputCompilerUnit` or
   `MultiSubPhaseRouterUnit` are station-level evidence only. Accepted routing
   learning requires `OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>`
   or equivalent `AdaptiveLogicalOrchestrator` behavior with compiled document,
   routing decisions, knowledge-edge state, flow/factory resolution, and final
   routing verdict. Unit-only rows must be marked
   `PAIRSET_READY_ROUTING_FLOW_PENDING`, not accepted.
6. External code-documentation rows must not be mixed directly into internal
   orchestration/plans/skills training; they require their own code-doc gates.
7. If a document is a new type vs a new version, decide by structure, consumer
   action, A-side question, and lifecycle/canonicality. Do not decide by name
   alone.

Script references updated for this rule:

- `prepare_internal_learning_sets.py` now writes `model-led-authority-candidates.jsonl`
  for root prompt/user-intent candidates and `training-pairs-repair-required.jsonl`
  for rows blocked by hard DPO gates.
- `augment_internal_learning_sets_tier3.py` now routes B162/B165 prompt-authority
  candidates to model-led authority adjudication instead of treating them as
  ordinary low-confidence review rows.

---

## What changed from v10

**Additional companion registry linked.** The companion file
`XIIGEN-INTERNAL-SOURCE-REGISTRY-ADDITIONAL-TYPES-v1.md` now covers additional
families `A01-A51`, `B13-B43`, `C01-C12`, `Z41-Z56`, `D01-D19`, `E01-E16`,
`F01-F17`, `G01-G13`, `H01-H08`, `I01-I14`, `J01-J09`, `K01-K13`, `L01-L08`,
`M01-M21`, `N01-N09`, `O01-O19`, `P01-P12`, `Q01-Q05`, `R01-R13`,
`S01-S14`, `T01-T10`, `U01-U20`, `V01-V08`, `W01-W16`, `X01-X16`,
`Y01-Y10`, `AA01-AA17`, `AB01-AB07`, `AC01-AC20`, `AD01-AD08`,
`AE01-AE10`, `AF01-AF13`, and `AG01-AG06`. v11 remains the numbered-type authority;
the companion supplies explicit handling and DTO-quality gates for newly found
session, archive, runtime, external-structural, and named artifact families.

**Direct corpus scan produced twelve new families (B01â€“B12).** The prior rounds
covered families identified by path/name taxonomy scans. This round inspects the
actual content families present in xiigen_history that still have no explicit
registry type or limited-authority rule.

**Types 29â€“34 added.** Six new types from high-signal corpus families:

- **Type 29** â€” Phase implementation plans and phase state documents. Distinct from
  Type 17 (migration plans): these are phased development plans with sub-steps,
  delivery checkpoints, and per-phase state files. Source of PLAN Type B pairs from
  within-phase dependency ordering.

- **Type 30** â€” Skills factory registry documents. Distinct from Type 20 (individual
  skill definitions): these are catalogue-level documents listing all skills, their
  IDs, trigger conditions, and coverage. Source for knowledge-edge pairs (missing
  skill coverage) and routing pairs.

- **Type 31** â€” Naming and mapping translation tables. Documents like ROSETTA_STONE.md
  and FAMILY_RENUMBERING_MAP.md define authoritative mappings between old and new
  system names. Source for Type C rejected (wrong name system) and Type 16 extension
  (superseded vocabulary).

- **Type 32** â€” Validation reports and certificates. Documents that record a formal
  gate verdict on a delivery â€” passed, failed, conditional. Source for ArbiterUnit
  pairs that teach what a real gate verdict looks like vs a fabricated one.

- **Type 33** â€” Conflict and comparison analysis documents. Documents that compare
  two or more approaches and record why one was chosen. Source for ARCH trade-off
  pairs â€” the unchosen approach is the pre-made rejected.

- **Type 34** â€” Definition of done and prerequisites documents. Documents that define
  the exact conditions that must be true before a phase or feature is complete. Source
  for ArbiterUnit pairs about whether completion criteria are met.

**Six limited-authority families added.** B02, B08, B10, B11, and B12 have explicit
boundary rules.

**Pre-submission checklist and quick-reference table extended.**

---

**Second addendum scan A13â€“A24 processed.** Twelve additional artifact families from
the extended folder scan were not covered by Types 1â€“26 or the prior A01â€“A12 families.

**Type 27 â€” Diagram and topology source files (A13).** Machine-readable diagram files
(`.mmd`, `.drawio`, `.puml`) contain explicit node-edge relationships that prose cannot
represent precisely. These are stronger than architecture documents for pairs about
which components depend on which, and for PLAN ordering pairs where the dependency
graph is the authority. Parse nodes and edges â€” never flatten to prose before
extracting pair content.

**Type 28 â€” Test suite source files (A15).** Executable test files (`*.test.ts`,
`*Tests.cs`) encode expected behavior more precisely than plan documents. The test
assertion is the chosen for a CODE_Q or BUG pair about expected behavior. A response
that contradicts a passing test assertion is the rejected. Mocking scaffolding is
excluded â€” only test intent (assertions, expected values) is pair material.

**Five limited-authority families added.** A14, A16, A19, A20, A21, A22, and A24
have explicit boundary rules.

**A21 â€” Positive/negative example catalogs.** High-value DPO source: chosen and
rejected are already authored. Requires provenance check before use â€” synthetic
examples need a label before they can be included.

**Three exclusion rules added.** A17 (office/PDF exported docs), A18 (vendored
third-party codebases), and a note that A23 (localized user-facing guides) is covered
by the existing A11 rule â€” no separate handling needed.

**Pre-submission checklist extended** with A13â€“A24 gates.
**Quick-reference table extended** to 28 types plus new limited-authority rows.

---

**Addendum scan A01â€“A12 processed.** Twelve additional artifact families from the
extended folder scan were not covered by Types 1â€“24 or the U01â€“U24 families.

**Type 25 â€” Configuration schema contracts (A01).** JSON schema files that define
machine-readable contracts for product behavior and config validation. These are
stronger than prose for exact field-level validation pairs because they are executable
â€” a value that fails schema validation is objectively wrong. Source for ArbiterUnit
pairs where the verdict is determined by schema conformance.

**Type 26 â€” Source code contract files (A05).** Live interface, DTO, enum, and
runtime contract definitions from source code outside the topology seeder. These are
the ground truth for exact field names, method signatures, and contract shapes.
Stronger than Type 2 (prose architecture documents) for pairs where the correct answer
depends on an exact type name or interface definition.

**Five limited-authority families added.** A02, A03, A04, A08, A11, and A12 each
have explicit boundary rules â€” they contribute evidence in narrow scopes but must
not become general architecture authority.

**Four exclusion rules added.** A06 (model checkpoints), A07 (training dataset rows),
A09 (rendered HTML), and A10 (visual binary assets) added to the global exclusion
section. A07 has a specific self-training loop warning.

**Pre-submission checklist extended** with A01â€“A12 coverage gates.

---

**Four remaining document families from the extended scan now have explicit rules.**
v7 added Types 17â€“24 and zone mapping rules for U15/U16/U20/U21. Four families
from the extended scan still had no explicit guidance:

**U04 â€” Plugin and marketplace extension docs** now has an explicit limited-authority
section: A-side prompts from plugin integration sessions are valid Type 1 material;
B-side authority is limited to platform-specific integration pairs only and must never
be used for core XIIGen orchestration pairs.

**U08 â€” Design conversion documents** now has an explicit limited-authority section:
A-side UI prompts from Figma-to-code sessions are valid Type 1 material with UI
predicate; B-side authority is limited to sub-phases 1.1 and 4.2 only. Assessment
noted that design conversion has geometry/layout-specific mistake patterns that
normal Type 2 does not model â€” these are now captured in the Type 24 extension note.

**U12 â€” Capability catalogs and matrices** now has an explicit zone mapping rule:
capability matrices â†’ Type 7 extension (knowledge-edge detection pairs); task type
catalogs â†’ Type 5 extension (topology authority); RAG indexes and source indexes â†’
vocabulary calibration only, not direct pair sources.

**U24 â€” Marketplace legal and store assets** now has an explicit exclusion rule:
LICENSE, NOTICE, privacy.html, sitemap.xml, and marketplace.json are operational
distribution metadata. No DPO pair construction is permitted unless the task being
taught is specifically about marketplace listing, license compliance, or plugin
distribution. Added to global exclusion section with one named exception.

**Type 24 extended** with a note connecting it to U08 design conversion: Figma-to-code
and design pipeline debug histories are the primary source of U08-type correction pairs.

**Pre-submission checklist extended** with U04/U08/U12 authority boundary checks.

---

**Canonicality framework added as a top-level section.** The assessment identified
that document type alone is insufficient â€” the same file can be a canonical source,
a mirror copy, an extracted snapshot, a stale version, or an archive container. Each
canonicality class has different rules for what can and cannot be used as training
material. The framework now precedes the type taxonomy.

**Type 23 â€” Gap remediation and fix documents.** U14 from the extended scan. Documents
that record a gap, the attempted wrong fix, and the correct fix. This is a three-part
correction structure unique in the corpus â€” no other type has the gap identification,
wrong-fix attempt, and correct-fix evidence in one document. Primary source for BUG
and ARCH repair pairs where the rejected is the wrong fix, not just a wrong domain.

**Type 24 â€” Generated code debug and correction histories.** U23 from the extended
scan. Debug traces and repair histories from design-to-code or code generation sessions.
Natural BUG and REFACTOR correction pairs where the original generated code is rejected
and the corrected version is chosen. Distinct from Type 3 correction events because
the defect is in generated code, not in a Claude response to a developer prompt.

**Zone mapping rules extended.** Assessment identified four additional bundle families
that need zone decomposition rather than new types:
- U15 visual UI quality evidence â†’ Zone A (visual findings â†’ Type 8), Zone B
  (role/screen maps â†’ UI/ARCH B-side), Zone C (UML coverage â†’ architecture evidence)
- U16 flow generator guidance library â†’ Zone A (GUIDE-B procedure steps â†’ PLAN B-side),
  Zone B (coverage reports â†’ Type 8/13), Zone C (source maps â†’ provenance only)
- U20 benchmark / stress / dry-run logs â†’ Zone A (actual run metrics â†’ Type 4),
  Zone B (expected traces â†’ plan/test authority), Zone C (dry-run reports â†’ evidence
  with status flag)
- U21 prompt handoff and session briefs â†’ Zone A (session start gates â†’ Type 15),
  Zone B (prompt templates â†’ prompt-template authority), Zone C (handoff contracts â†’
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
**Pre-submission checklist extended with Type 23â€“24 checks and canonicality gate.**

---

**Document type taxonomy expanded from 16 to 22 types.** U01â€“U13 from the uncovered
types scan were assessed and resolved:

**Type 17 â€” Translation and migration plans.** Cross-stack migration documents
with explicitly sequenced phases. The best available source of PLAN Type B pairs
because prerequisite language is already written in the document â€” the annotation
is extracted, not constructed. Companion STATE.json files record interrupted phases
which are correction events in machine-readable form.

**Type 18 â€” Flow portability and protocol documents.** The only source of
invariant-based rejected pairs. A portability violation can be derived mechanically:
take any correct flow, add one tenant-specific hardcode, and the rejected is
complete. The protocol document names the check that the violation fails.

**Type 19 â€” Agent tooling instructions.** Machine-facing behavior contracts
(AGENTS.md, CLAUDE.md). Separated from Type 14 (user behavioral rules) and
Type 15 (session gates) because the audience is different: these are executable
contracts for a tool, not behavioral preferences for a human. Source of PLAN
within-task ordering pairs and ArbiterUnit tool-compliance pairs.

**Type 20 â€” Skill library documents.** Skill definitions and manifests. Type C
rejected only â€” skill documents define domain boundaries but not the relationship
between adjacent sub-phases. Type A rejected requires Type 5 (topology) as authority,
not Type 20.

**Type 21 â€” Deep research and external domain reports.** Most restricted new type:
CODE_Q pairs only. External research is not ground truth for XIIGen decisions and
must never override Type 9 locked decisions. Requires a specific cited external
source in the annotation.

**Type 22 â€” Execution state JSON files.** Machine-readable workflow execution
progress records. Separated from Type 4 (DPO training metrics) because these record
which phases completed and which artifacts were produced â€” not model learning numbers.
Two subtypes: 22a (missing field, overlap < 0.20) and 22b (interruption correction
event, overlap 0.10â€“0.25).

**Type 23 â€” Business flow feature specifications (zone mapping rule, not a new type).**
Documents like 01-user-registration.md contain three distinct zones that require
different pair construction rules: Zone A (acceptance criteria â†’ ArbiterUnit rules),
Zone B (architectural decisions â†’ ARCH Type A rules), Zone C (task ordering â†’
PLAN Type B rules). Treating them as uniform Type 2 was producing wrong pair types.

**U10 and U13 added to Type 6 (excluded with preprocessing rules).** ZIP archives
must be unpacked and their contents reclassified by content type. Raw merged text
dumps must be segmented by delimiters before any extraction.

**Quick-reference table extended to 22 rows.**

**Pre-submission checklist extended with Type 17â€“22 checks.**

---

**Type 7 â€” A-side rule added.** v4 showed examples where the A side is a developer
prompt with intent verb "does / is / review", but did not state this as an explicit
rule. v5 adds: A side for Type 7 pairs is always a Type 1 prompt with those intent
verbs. Status document text is B-side evidence only. Ineligible patterns listed.

**Type 8 â€” Clean pass example added.** v4 had only concern-vs-pass and
reprocess-vs-concern examples. Without a pass example, the model develops a bias
toward flagging correct artifacts. v5 adds a third pair: clean artifact with explicit
dependency, chosen = pass with assertive finding, rejected = fabricated concern with
hedging language. Explains why the asymmetry causes overflagging.

**Type 10 â€” ArbiterUnit A-side corrected.** v4 used raw teaching ledger text as
the ArbiterUnit A side (`"teaching ledger showing 5 of 10..."`). This violates the
`ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>` contract. v5 replaces it with
a properly shaped `ArbiterReviewRequest` object containing `artifact_type`,
`artifact_content`, `review_criteria`, and `source_rule` fields. Distinction between
PLAN pairs (developer prompt as A) and ArbiterUnit pairs (contract object as A) is
now explicit.

**Type 11 â€” Split into three subtypes.** v4 mixed three structurally different
rejected patterns under one type with a single overlap ratio (0.10â€“0.20). This was
wrong: the wrong-formula subtype (11b) has overlap 0.40â€“0.55 and requires the
annotation to carry most of the reward margin signal, while the missing-layer
subtype (11a) has overlap < 0.15 and works without strong annotation. v5 defines:
â€” 11a: missing layer (overlap < 0.15, annotation useful but not critical)
â€” 11b: wrong formula (overlap 0.40â€“0.55, token overlap test mandatory, annotation
   must introduce â‰¥ 5 consequence-describing non-overlapping tokens)
â€” 11c: wrong field split (overlap 0.20â€“0.30, annotation helpful)
Quick-reference table and pre-submission checklist updated accordingly.

**Document type taxonomy expanded from 11 to 16 types.** v3 covered experiment
documents, artifact examination, decisions, teaching plans, and measurement
guidance. Five additional types were missing:

**Type 12 â€” Project history documents.** These contain documented mistakes and
their consequences. Each Part records a real decision and often records the earlier
wrong version. The superseded reasoning is a pre-made rejected for ARCH trade-off
pairs (A46 vs A50). The grounded historical claim is the chosen for CODE_Q pairs.

**Type 13 â€” Document state register.** Contains explicit thread blocking
relationships with Gate B commands per thread. The thread order is PLAN content.
Running blocked threads in parallel is the rejected. It also provides CODE_Q
document-authority pairs: the register names which version is current, making
a superseded version the ready-made rejected.

**Type 14 â€” Behavioral rules documents (What-To-Do-What-Not-To-Do).** Each
"What not to do" entry names an exact forbidden output pattern â€” that pattern
is the rejected. Plans without Gate B commands, Node 01 A-sides with raw strings,
pairs missing domain labels â€” all are pre-documented rejections. Version check
required: rules superseded in later versions must not be used as chosen.

**Type 15 â€” Session start prompt documents.** Contains pre-work gates (live test
count before any work) and post-work gates (manifest + verification before round
close). Gate-skipped behavior is the rejected. Correct gate behavior is the chosen.
Overlap 0.10â€“0.20: live-verification vocabulary vs memory-recall vocabulary.

**Type 16 â€” Versioned document evolution.** The sequence of versions of any
document produces superseded-vocabulary pairs. The old vocabulary (UserPrompt as
Node 01 input, 1-layer model, A46 DO NOT IMPLEMENT, Layer 1 planned) is the
rejected. The current vocabulary is the chosen. Lowest overlap ratio in the corpus
(0.05â€“0.15) because old and new contract names are entirely non-overlapping tokens.

**Three-check rule added (Type 16).** Before using any versioned document: (1) is
this the most recent version, (2) does the claim appear in a "changes from" section
of a newer version, (3) does the document have a "do not use this file" header.

**Quick-reference table extended to 16 rows** with key distinctions note: Types
12â€“16 all produce rejected from within their own corpus â€” no rejected needs to
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
are the canonical source for ArbiterUnit pairs â€” they show what a real pass, concern,
and blocking finding looks like, with specific evidence rather than abstract criteria.

**Decision documents separated from B-side authority (Type 9).** The A-series
locked decisions (A1â€“A66 in DECISIONS-LOCKED.md) each contain a decision, a
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
                   Do not use as a training source â€” use the canonical copy.
                   If the canonical source is missing, promote the mirror to
                   canonical before use.

extracted_copy     A file extracted from a ZIP or repository snapshot, placed in
                   a working directory for inspection. Valid for pair construction
                   only if the extraction is dated and the canonical archive is
                   also recorded.

stale              A versioned document where a newer version exists (confirmed via
                   the document state register or "What changed" header). Use only
                   as rejected-vocabulary source. Never as chosen source.

external           Originates outside XIIGen â€” external research, open-source
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

Every XIIGen internal document belongs to one of thirty-four types. The type determines
how it can be used in DPO pair construction. A document that does not fit any type
must be assessed against the exclusion rules before use.

---

### Type 1 â€” A-side authority: real developer prompts

**What it is:** Raw human messages from Claude export conversations or local session
files. Unedited. May contain typos, Hebrew, Russian, code fragments, partial sentences.

**Why it is A-side authority:** The model must learn to receive real developer input,
not idealised prompts. Only this type provides authentic input-language distribution.

**Examples in the XIIGen corpus:**
```
"fix the rxjs pipe in the Angular interceptor â€” I keep getting
 'object unsubscribed' after the first request"

"×¢×–×•×¨ ×œ×™ ×¢× ×”×ž×©×š ×œ×—×œ×§ ×”×‘× â€” JOB ×©×ž×—×©×‘ ×”×ª××ž×•×ª ×œ×–×•×’×•×ª"

"I don't want to use legacy peer I want to fix the problem
 npm install keeps failing with peer dependency conflict"

"Please generate flow 10 files that will include only these
 specific to flow 10 sections: P1Family 27 â€” Tena..."
```

**Eligibility test for PLAN and ARCH pairs** â€” at least one of:
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
confirmed this: batches 30, 37 used meta-commentary rows from session 1235â€“1753
and produced mean accuracy 0.306â€“0.415.

**Overlap ratio when used as A side:** not applicable â€” A side is prompt input,
not paired content. The DPO signal comes from chosen/rejected token distributions
relative to this A side.

---

### Type 2 â€” B-side authority: formal XIIGen documents

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
- Sub-phase Purpose field â†’ chosen content for 3.1, 3.2, 3.3 ARCH pairs
- Dependency ordering from teaching plan â†’ chosen for PLAN Type B pairs
- Decision entries (A43â€“A66) â†’ chosen for architectural trade-off pairs
- Locked decisions with reasoning â†’ chosen for arbiter pass/fail criteria

**How NOT to use:** Never use as A side. Architecture document language produces
register mismatch: the model learns to expect formal vocabulary as input, which no
real developer ever provides.

**Overlap ratio when used as B side (chosen):** 0.10â€“0.20 against a Type 1 A side,
because the formal vocabulary of the document is largely non-overlapping with the
developer's surface words.

---

### Type 3 â€” Correction source: Claude export conversations

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
  H1  "this is wrong â€” I need the entity shape, not the repository"
  A1  "Entity: UserPreference { tenantId (partition key), userId, key, value,
       updatedAt }. Partition key is tenantId to ensure tenant isolation at
       the storage layer."

Extracted pair:
  A:        H0 â€” "design the data model for multi-tenant user preferences"
  Chosen:   A1 â€” "Entity: UserPreference { tenantId (partition key)..."
  Rejected: A0 â€” "Create a UserPreferencesRepository with GetByUserId..."
  [WRONG: "design the data model" asks for entity shape and partitioning rules,
  not the repository class. The repository is 4.1 Backend Services work â€”
  it is built after the model is defined, not instead of it.]

  conversation_uuid: {uuid}
  human_message_index: 0
  quality_rating: High
```

**What makes an annotation strong (names three things):**
- The verb in H0 that signals the scope: "design" signals shape decisions, not code
- What the correct sub-phase actually owns: entity shape, partitioning rules
- Why rejected is in the wrong sub-phase: "repository is 4.1 work â€” built after, not instead"

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
        vocabulary (overlap 0.25â€“0.50)
        Include with strong annotation.

Low:    Correction is only phrasing, not substance
        (chosen and rejected say the same thing differently, overlap > 0.50)
        Exclude.
```

---

### Type 4 â€” Measurement evidence: calibration run data

**What it is:** `dpo_step_*.json` files, `batch-summary.jsonl`, `progress.txt`,
checkpoint manifests from calibration runs.

**How to use:** Populate B sides for ModelTrainingUnit, HeldOutEvaluationUnit pairs.
The exact numbers (elapsed_ms, cpu_percent, working_set_delta_mb, checkpoint_bytes,
accuracy, loss) are the B-side content for teaching measurement pairs.

**How NOT to use:** Never derive A sides from run logs. Run log language is diagnostic
output, not developer input.

---

### Type 5 â€” Topology authority: seeder and registry code

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
    â†’ chosen vocabulary for ARCH boundary pairs

4.1 "service implementation, controller logic, dependency injection, framework calls"
    â†’ rejected vocabulary when confused with 3.1

3.4 "authentication, authorisation, tenant isolation, JWT, RBAC, token validation"
    â†’ rejected vocabulary when 3.1 boundary work is confused with security work

3.5 "metrics, traces, logs, alert thresholds, observability contracts"
    â†’ rejected vocabulary when confused with 3.1 or 4.1
```

The rule: chosen uses Purpose vocabulary from the correct sub-phase. Rejected uses
Purpose vocabulary from the most plausible wrong sub-phase. Overlap ratio between
these two vocabulary sets is typically 0.10â€“0.25 for adjacent sub-phases.

---

### Type 6 â€” Excluded: document-loading and session-management messages

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

### Type 7 â€” Experiment status and code review documents

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
3. The implementation gap â€” what exists vs what the experiment required

**How to use for DPO pairs:**

These documents are the canonical source for pairs that teach the difference between
"this capability works" and "this capability was proven but the production code does
not have it yet". This distinction is critical for the code review and architecture
review sub-phases (6.1, 6.4).

For CODE_Q pairs (how does X work):

```
Prompt:   "does the AstRelevanceRanker use BM25 and KNN fusion?"

Chosen (CODE_Q â€” accurate state):
  "AstRelevanceRanker has BM25 (E04) and KNN (E09) implemented separately.
   The rank fusion step â€” alpha x BM25_score + (1-alpha) x KNN_score with
   auto-tuned alpha â€” is not in the file. The +8% hit@1 gain from E34V2
   requires the fusion, but it is NOT IMPLEMENTED. The two signals exist
   but are never combined."

Rejected (CODE_Q â€” overstates implementation):
  "Yes â€” AstRelevanceRanker implements both BM25 and KNN retrieval and
   combines them using a weighted fusion formula. The alpha parameter
   is tuned automatically based on query type."
[WRONG: the experiment result exists but the fusion implementation does not.
AstRelevanceRanker.cs has no 'fusion', 'alpha', 'blend', or 'fuse' logic.
Reporting an unimplemented experiment as implemented is an accuracy failure
that would cause the orchestrator to believe a capability exists when it does not.]
```

For 6.1 architecture review pairs:

```
Prompt:   "review the retrieval path â€” is the BM25+KNN fusion from E34V2 in place?"

Chosen (6.1 â€” accurate review finding):
  "E34V2 rank fusion: NOT IMPLEMENTED. AstRelevanceRanker.cs contains BM25
   (via E04 implementation) and KNN (via E09). Zero hits for 'fusion', 'alpha',
   'blend', 'fuse'. The +8% hit@1 from E34V2 is not in production. This is
   the highest-impact retrieval gap in the current codebase."

Rejected (6.1 â€” review without checking code):
  "The retrieval path uses BM25 and KNN. Both experiments are implemented in
   AstRelevanceRanker. The path should be reviewed for performance under load
   and for correctness of the alpha parameter."
[WRONG: a review that claims implemented capabilities are present without
verifying them against the code is not a review â€” it is paraphrase of the
experiment list. The review must check EXPERIMENT-IMPLEMENTATION-STATUS against
the actual file. Reporting an unimplemented capability as present blocks the
team from knowing where the gap is.]
```

**A-side rule for Type 7 pairs â€” mandatory:**

The A side is always a Type 1 developer prompt. It must use one of these intent
verbs: `"does / is / review / check / verify"`. The status document itself never
becomes the A side â€” it is authority evidence for the B side only.

Eligible A-side patterns:
```
"does X use Y?"                        â€” CODE_Q state-accuracy pair
"is E34V2 fusion implemented?"         â€” CODE_Q implementation check
"review the retrieval path"            â€” 6.1 architecture review pair
"check whether E12 SIMD is in place"   â€” 6.4 code review pair
```

Ineligible A-side patterns (status document text used directly):
```
"E34V2 +8% hit@1 rank fusion: alpha x BM25_score + (1-alpha) x KNN_score"
"EXPERIMENT-IMPLEMENTATION-STATUS: E34V2 = NOT IMPLEMENTED"
```
These are B-side evidence, not prompts. Any A side that contains a status field
name, an experiment ID, or a file path is Type 6 material â€” exclude it.

**Overlap ratio:** 0.10â€“0.25. Chosen uses status vocabulary (NOT IMPLEMENTED,
zero hits, no production class, gap). Rejected uses capability vocabulary (implements,
uses, combines). Near-zero overlap between these vocabularies.

**Pair types extractable:** CODE_Q (accurate state description), 6.1/6.4 review
findings, ArbiterUnit pairs (pass when status is correctly reported; concern when
status is wrong).

**Excluded use:** Never use status document text as A side. Never claim a
capability is implemented unless the status field is IMPLEMENTED and the evidence
path (the file/class name) is confirmed present in the actual codebase.

---

### Type 8 â€” Artifact examination documents

**What it is:** Documents that record the structured examination of produced
artifacts â€” session round artifacts, plan documents, flow files, training batches.
Examples from the corpus:

```
artifact_reviewing.md        â€” examination methodology for round artifacts
EXAMINATION-METHOD-ORCHESTRATION-v7.md / v8.md â€” pair examination standards
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

Chosen (ArbiterUnit B â€” concern verdict with specific finding):
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

Rejected (ArbiterUnit B â€” pass verdict without examining relationships):
  verdict: pass
  finding: "b08 is present in the plan and is listed as a dependency of b09."
[WRONG: the arbiter found the right word ('b08') but did not check the
relationship. A plan where b08 is both in the parallel group and a
dependency of b09 has a structural ambiguity that the word presence
check cannot detect. The correct verdict is concern, not pass â€” the
relationship between b08's parallel execution and b09's dependency on
b08's result is not resolved by listing both facts.]
```

**Second pair type from this source â€” wrong verdict type:**

The examination document distinguishes `concern` (noted gap, round can be accepted
with flag) from `reprocess` (structural defect, must be corrected before acceptance).
These are the hardest pairs to construct correctly because both verdicts indicate
a problem â€” the rejected is a plausible but wrong severity classification.

```
A: artifact with duplicate route registration causing wrong handler execution

Chosen (ArbiterUnit â€” reprocess: structural defect):
  verdict: reprocess
  finding: "the wildcard route is registered before the API route. Every
            request is intercepted by the wildcard handler before reaching
            the API endpoint. Current output is wrong."
  requirement: "fix the registration order before this result can be accepted"

Rejected (ArbiterUnit â€” concern: wrong severity, should be reprocess):
  verdict: concern
  finding: "route registration order may affect which handler receives
            requests in some edge cases. Worth reviewing."
[WRONG: when the output is currently wrong â€” the wildcard intercepts every
request â€” the correct verdict is reprocess, not concern. A 'concern' verdict
allows the round to be accepted with a note. 'reprocess' requires correction.
Downgrading a defect that produces wrong output to a concern leaves the wrong
behavior in the next round's starting state.]
```

**Third pair type from this source â€” clean artifact, pass verdict:**

Without pass examples, the model sees only concern and reprocess pairs and develops
a bias toward flagging everything. A clean artifact must produce `verdict: pass`.
The rejected is a fabricated concern where no gap exists.

```
Contract: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>

A (arbiter review request):
  artifact: a PLAN where 3.4 Security is branch 1, 4.1 Backend is branch 3,
            and 4.1 explicitly states "depends on branch 1"
  review_criteria: "verify dependency ordering is semantically correct"

Chosen (ArbiterUnit â€” pass: relationships hold):
  verdict: pass
  finding: "3.4 Security is correctly ordered before 4.1 Backend. The dependency
            is explicit: branch 3 states 'depends on branch 1'. 4.1 cannot
            implement auth middleware before 3.4 defines the validation rules.
            The plan resolves this correctly."

Rejected (ArbiterUnit â€” concern: fabricated gap):
  verdict: concern
  finding: "3.4 and 4.1 may need to be reviewed for alignment. There could be
            assumptions in 4.1 that depend on 3.4 decisions not yet made."
[WRONG: "could be" and "may need" are not findings â€” they are absence-of-checking.
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

**Overlap ratio:** 0.20â€“0.35 for pass vs fabricated-concern pairs. Both verdicts
contain relationship vocabulary (depends on, ordered before, requires). The
difference is in the certainty vocabulary: chosen uses assertive language (correctly
ordered, explicit, resolves), rejected uses hedging language (may need, could be,
not yet made).

**Overlap ratio:** 0.15â€“0.35 across all three pair types. Chosen uses verdict-type
vocabulary (reprocess, structural defect, currently wrong, must be corrected; or
pass, correctly ordered, explicit). Rejected uses softer or wrong severity vocabulary.

**Pair types extractable:** ArbiterUnit B sides for all three verdict types (pass,
concern, reprocess), Node 08 decisions, structural verification pairs.

**Excluded use:** Never use examination methodology text as A side. It is authority
evidence for what the correct B looks like.

---

### Type 9 â€” Decision documents

**What it is:** The DECISIONS-LOCKED.md file and any document containing locked
A-series or D-series decisions. Each entry has: the decision made, the rationale,
and implicitly or explicitly â€” the alternative that was rejected.

```
DECISIONS-LOCKED.md (A1â€“A66, D01â€“D15)
```

**Why this is a distinct type:** Every locked decision is a pre-made trade-off pair.
The decision (A-series) is the chosen. The alternative that was not taken is the
rejected. The rationale explains why the alternative fails â€” this becomes the
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

  Chosen (ARCH â€” the locked decision):
    "Pre-seed at static construction via BuildSeededClassifier(). Seed with
     five canonical domain predicate profiles â€” one per D-04 domain. This
     ensures _trained=true on the first call. The E18 +15% KNN path requires
     _trained=true; it is permanently unreachable if the object is
     constructed with new() and Train() is never called."

  Rejected (ARCH â€” the alternative not taken):
    "Lazy seeding: construct with new() and seed on first prompt arrival.
     This avoids startup cost and allows the seed data to come from real
     runtime prompts rather than hardcoded predicate profiles."
  [WRONG: lazy seeding means _trained=false for all prompts until the first
  runtime prompt triggers the seed. Every prompt before that point falls to
  ClassifyByKeywords â€” the static keyword scan. Any startup sequence that
  includes domain classification before the first real prompt (e.g., health
  checks, warm-up calls) would produce wrong domain labels that get cached.
  The E18 gain requires the KNN path to be available on the first call,
  not eventually.]
```

**Second pair type â€” D-series decisions for implementation rules:**

D-series decisions define rules like "no external ML frameworks" (D-11), "replay
buffer 70/30" (D-06). These are IMPLEMENT/TEST pairs: chosen follows the rule,
rejected violates it.

```
A: "implement the capability trainer's replay mechanism"

Chosen (IMPLEMENT â€” follows D-06):
  "Use the 70/30 split from project 16's ReplayBuffer: 70% current training
   examples, 30% replay from prior manifests. Cap at DefaultMaxCapacity=4096
   entries. Call ClearCache() at batch boundary to release memory."

Rejected (IMPLEMENT â€” violates D-06):
  "Add ML.NET's memory-efficient replay implementation â€” it handles the 70/30
   split automatically and supports dynamic capacity adjustment based on
   available RAM."
[WRONG: D-11 prohibits external ML frameworks. ML.NET is an external
ML framework. The replay mechanism must be implemented using project 16's
existing ReplayBuffer.cs which is already proven in production. Using ML.NET
would require adding a dependency that the project explicitly prohibits.]
```

**Overlap ratio:** 0.10â€“0.20 for architecture trade-offs (chosen uses acceptance
vocabulary, rejected uses alternative vocabulary). 0.05â€“0.15 for rule violations
(chosen uses rule-compliant vocabulary, rejected uses the prohibited framework/pattern
name).

**Pair types extractable:** ARCH trade-off pairs, IMPLEMENT rule-following pairs,
ArbiterUnit pairs (D-rule violation = reject verdict).

**Excluded use:** Never invent a decision not in the document. The alternative
must be derivable from the decision rationale or from prior session history showing
what was attempted before the decision was made.

---

### Type 10 â€” Teaching plan documents

**What it is:** Step-by-step plans for teaching the orchestrator, with explicit
ordering constraints, stopping rules, and acceptance criteria. Examples:

```
ORCHESTRATOR-TEACHING-PLAN-v3.md
XIIGEN-DRIVEN-SELF-TEACHING-GUIDANCE-v2.md
```

These documents differ from architecture documents (Type 2) in one critical way:
they contain **stopping rules** â€” specific conditions that must be met before
proceeding to the next step. Stopping rules are the ground truth for ArbiterUnit
pass/fail/reprocess pairs and for PLAN dependency-ordering pairs.

**How to use for PLAN dependency-ordering pairs:**

Teaching plans make dependency ordering explicit. The sub-cycle from
`ORCHESTRATOR-TEACHING-PLAN-v3.md` states:

```
Step 1 â€” Harness wiring (learned-unit selector per role) â€” must complete before Step 2
Step 2 â€” Tracing wrappers for verification run â€” must complete before Step 3
Step 3 â€” LoRA adapter inclusion in forward path â€” must complete before Step 4
Step 4 â€” Checkpoint field split measurement â€” must complete before first row
Step 5 â€” Teach one row, measure, decide â€” repeat
```

This ordering is PLAN content. The rejected for a PLAN pair from this source
puts implementation before harness:

```
A: "plan the first teaching cycle for the domain classifier unit"

Chosen (PLAN â€” teaching plan order):
  Branch 1 â€” harness wiring (dependency order 1):
    Write a learned-unit selector for the DomainClassifier role following
    the SecurityVerification34FlowFactory pattern. Register it in the flow
    registry for each sub-phase being taught. This must be complete before
    any teaching row runs â€” without it, a Probationary unit is registered
    but never called by the verification run.

  Branch 2 â€” tracing wrappers (dependency order 2):
    Wrap the orchestrator flows with TracingFlowDecorator before the
    verification run. Collect StationTrace objects via the emitTrace callback.
    Dependency: the selector from branch 1 must exist â€” tracing wraps calls
    that the selector enables.

  Branch 3 â€” teach row 1 (dependency order 3):
    Run one DPO step on the first prepared pair. Measure all three layers
    (resources, routing, verdict). Accept only if LoraAdapterBytes < 2MB.
    Dependency: branches 1 and 2 must be complete â€” the harness must exist
    before any row is taught.

Rejected (PLAN â€” wrong order, teaching before harness):
  Branch 1 â€” teach row 1
  Branch 2 â€” write the learned-unit selector
  Branch 3 â€” add tracing wrappers
[WRONG: teaching row 1 without the learned-unit selector means the
Probationary unit is registered but never called. The verification run
uses the deterministic unit, not the taught unit. The measurement shows
the deterministic unit's behavior, not the trained unit's. The selector
must exist before teaching begins â€” it cannot be added retroactively to
an already-taught row's measurement evidence.]
```

**How to use for ArbiterUnit stopping-rule pairs:**

Teaching plans contain explicit stopping rules. These are arbiter pass criteria.

**A-side rule for ArbiterUnit pairs:** The A side must be an `ArbiterReviewRequest`
object matching the `ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>` contract.
The teaching ledger data populates the artifact field inside the request â€” it is
not the A side itself.

```
Contract: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>

A (ArbiterReviewRequest â€” correct shape):
  artifact_type: "teaching_ledger_summary"
  artifact_content:
    total_rows: 10
    promoted: 5
    held: 3
    rejected: 2
    consecutive_rejected_tail: [7, 8, 9]
  review_criteria: "apply teaching plan stopping rule â€” 7 of 10 threshold"
  source_rule: "ORCHESTRATOR-TEACHING-PLAN-v3.md row-10 stop gate"

Chosen (ArbiterUnit B â€” reprocess: stopping rule triggered):
  verdict: reprocess
  finding: "5 of 10 decisions are promoted â€” below the 7-of-10 stopping
            threshold. Three consecutive rejected decisions at rows 7, 8, 9
            indicate a repeating pattern failure in the training data.
            Teaching must stop at row 10."
  requirement: "identify the systematic problem in the training data before
               proceeding to row 11"

Rejected (ArbiterUnit B â€” pass: ignores stopping rule):
  verdict: pass
  finding: "50% promotion rate is within acceptable range for early-stage
            training. Continue to row 11 to gather more data."
[WRONG: the stopping rule is explicit â€” fewer than 7 of 10 promoted triggers
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

**Overlap ratio:** 0.15â€“0.30 for PLAN dependency pairs. 0.10â€“0.20 for arbiter
stopping-rule pairs (stopping vocabulary vs continuation vocabulary).

**Pair types extractable:** PLAN ordering pairs (harness before teaching),
ArbiterUnit stopping-rule pairs with correct contract shape,
CorrectionLearningUnit boundary pairs.

**Excluded use:** Never use step descriptions as A sides for PLAN pairs.
Never use raw teaching ledger text as ArbiterUnit A side â€” wrap it in an
ArbiterReviewRequest object with the review_criteria and source_rule fields.

---

### Type 11 â€” Measurement guidance documents

**What it is:** Documents that define required measurement fields, acceptable
ranges, and correct formulas. Examples:

```
TEACHING-MEASUREMENT-GUIDANCE-v4.md
```

These documents specify exactly which fields a teaching row record must contain,
what the acceptable ranges are, and what happens when ranges are violated. This
is the ground truth for ModelTrainingUnit, HeldOutEvaluationUnit, and
CorrectionLearningUnit B-side content â€” and for rejected responses that use
wrong fields or wrong formulas.

**The three measurement layers from TEACHING-MEASUREMENT-GUIDANCE-v4.md:**

```
Layer 1 â€” Resources (must appear in every B side):
  inference_time_ms, teaching_time_ms, cpu_percent, working_set_delta_mb,
  managed_allocation_mb, disk_checkpoint_delta_bytes, branch_count,
  registered_flow_count, registered_unit_count

Layer 2 â€” Routing (must appear in every B side):
  compiled_predicate_ids, routed_sub_phases, routing_confidence,
  branch_count, branch_justification

Layer 3 â€” Verdict (must appear in every B side):
  pre_teaching_verdict, teaching_result, post_teaching_verdict,
  knowledge_edge_count_before, knowledge_edge_count_after,
  cumulative_registry_improved
```

**Type 11 pairs have three structurally different subtypes. Each has a different
overlap ratio and annotation requirement. Treat them as 11a, 11b, 11c.**

---

**Subtype 11a â€” Missing layer (overlap < 0.15, annotation useful but not critical)**

The rejected omits an entire measurement layer. Chosen contains fields that rejected
does not have at all â€” very low overlap. The annotation explains why the missing
layer matters, but the token distribution difference is already strong.

```
A (ModelTrainingUnit input):
  feature_vector_dataset: 93 rows, domain BUG, batch 3/63
  training_settings: { max_steps: 1, lora_rank: 2, lora_alpha: 4 }

Chosen (11a â€” all three layers present):
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

Rejected (11a â€” Layer 1 resource fields absent):
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

**Subtype 11b â€” Wrong formula (overlap 0.40â€“0.55, annotation carries most of the signal)**

Both chosen and rejected contain the same numbers. The formula is different but
the vocabulary is largely shared. This is the highest-overlap subtype in the
document â€” it behaves like a Type B pair (wrong order within correct structure).

**Apply the token overlap test before submitting any 11b pair.** If
`overlap_ratio > 0.50`, the annotation must be strong enough to carry the margin
by itself â€” the content difference alone will not produce positive margin.

```
A: "compute cpu_percent for the training step on a 4-core machine,
    elapsed_ms=96214, total_cpu_time_ms=53344"

Chosen (11b â€” correct formula):
  cpu_percent = (total_cpu_time_ms / elapsed_ms / Environment.ProcessorCount) x 100
  = (53344 / 96214 / 4) x 100 = 13.9%
  [Formula divides by processor count so the result stays within 0â€“100%
   on any multi-core machine.]

Rejected (11b â€” missing processor count division):
  cpu_percent = (total_cpu_time_ms / elapsed_ms) x 100
  = (53344 / 96214) x 100 = 55.4%
[WRONG: the formula must divide by Environment.ProcessorCount. Without the
division, a 4-core machine reports cpu_percent > 100% when more than one
core is used. This was fixed in TEACHING-MEASUREMENT-GUIDANCE-v4.md (P2-7).
The wrong formula produces 55.4% â€” a number that appears to show high CPU
pressure when the actual per-core load is 13.9%. An operational alert
calibrated against the correct formula would fire incorrectly on every
multi-core training step if the wrong formula is used.]
```

**Overlap ratio: 0.40â€“0.55.** Both sides contain `total_cpu_time_ms`, `elapsed_ms`,
`96214`, `53344`, and arithmetic operators. The annotation is what carries the signal:
it introduces `processor count`, `multi-core`, `100%`, `per-core load`, `operational
alert` â€” vocabulary present in chosen but absent in rejected. Without the annotation
this pair will likely produce negative margin.

**Strong annotation requirement for 11b:** The annotation must introduce new tokens
that appear in the context of the correct formula but not the wrong formula. Naming
the consequence (reports > 100%, fires incorrectly, wrong calibration) is more
effective than naming the rule (divide by ProcessorCount) because it introduces
more non-overlapping tokens.

---

**Subtype 11c â€” Wrong field split (overlap 0.20â€“0.30, annotation helpful)**

Chosen uses split fields (three separate measurements). Rejected uses a combined
field that merges them. Moderate overlap because both sides reference the same
underlying artifact path â€” but the field names are different.

```
A: "record the model artifact size fields for a teaching step"

Chosen (11c â€” split fields):
  base_checkpoint_bytes: new FileInfo(checkpointPath).Length
  lora_adapter_bytes: sum of (Rank x InDim + OutDim x Rank) x 4 per LoraLinear
  dpo_metric_bytes: sum of dpo_step_*.json file lengths in checkpoint directory

Rejected (11c â€” combined field):
  model_artifact_bytes: new FileInfo(checkpointPath).Length
[WRONG: ModelArtifactBytes was split into three fields in v4 (P2-6).
The combined field mixes base weights with adapter weights and metric files.
LoraAdapterBytes is what the D-05 stopping rule tests (2MB adapter limit).
A combined field cannot be compared against an adapter-only threshold â€”
it would require knowing what fraction of the combined size is adapters,
which is not recorded. The stopping rule becomes unenforceable.]
```

**Overlap ratio: 0.20â€“0.30.** Both sides reference the same checkpoint path.
The difference is field names (split vs combined) and the formula for lora_adapter_bytes.

---

**Overlap ratio summary for Type 11:**

| Subtype | What differs | Overlap | Annotation weight |
|---------|-------------|---------|-------------------|
| 11a â€” missing layer | entire fields absent | < 0.15 | useful but not critical |
| 11b â€” wrong formula | formula with same numbers | 0.40â€“0.55 | carries most of the signal |
| 11c â€” wrong field split | field names and count | 0.20â€“0.30 | helpful |

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

### Type 12 â€” Project history documents

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

**How to use for CODE_Q pairs â€” accurate historical claim:**

```
Prompt:   "why did the project choose to build TensorCore from scratch
           instead of using TorchSharp?"

Chosen (CODE_Q â€” accurate with source grounding):
  "TorchSharp wraps a closed binary â€” you can call its API but cannot read
   or change the internal code. The project's goal is to understand every
   algorithm from the inside, which requires reading and modifying the
   implementation. This was recorded in Part 2 of the project history as
   the origin of the 'no external ML frameworks' constraint, later locked
   as D-11."

Rejected (CODE_Q â€” plausible but invented motivation):
  "TorchSharp was too slow for the performance targets. The team wanted
   full control over SIMD optimizations and memory layout to hit the
   latency requirements for real-time orchestration."
[WRONG: the reason is not performance â€” it is inspectability. TorchSharp
wraps a closed binary; you cannot read the implementation. The performance
motivation was never stated in any project document. A CODE_Q answer that
invents a motivation not present in the source teaches the model to
hallucinate project history. The correct answer is grounded in Part 2.]
```

**How to use for ARCH pairs â€” superseded decision as rejected:**

Project History documents record decisions that were made and then changed.
A46 (E24 DO NOT IMPLEMENT) was superseded by A50 (E24 rehabilitated). The
A46 reasoning is the rejected; the A50 reasoning is the chosen.

```
Prompt:   "should AttributedSegmentRouter (E24) be implemented given
           current E21 boundary precision?"

Chosen (ARCH â€” A50, current decision):
  "E24 is safe to implement with the per-segment-per-domain contract.
   The original A46 failure was caused by a data structure issue in the
   old design â€” structurally impossible with the current implementation
   where each segment is routed to its own domain separately. E24 can
   be added after E21 is verified in production."

Rejected (ARCH â€” A46, superseded):
  "E24 must NOT be implemented until E21 boundary precision is verified.
   E24 assumes E21 produces correct segment boundaries â€” if E21 is wrong,
   E24 amplifies the error. Wait until E21 is confirmed stable."
[WRONG: A46 was superseded by A50 in Part 16 Addendum. The original
-37% noise increase came from a data structure problem in the old design,
not from the algorithm itself. The current per-segment-per-domain design
makes the original failure mode structurally impossible. Applying A46
reasoning to the current design is applying a constraint built for a
system that no longer exists.]
```

**Overlap ratio:** 0.15â€“0.30 for CODE_Q historical accuracy (chosen cites source,
rejected invents). 0.10â€“0.20 for superseded-decision pairs (chosen uses current
reasoning vocabulary, rejected uses superseded reasoning vocabulary).

**Pair types extractable:** CODE_Q historical accuracy, ARCH decision-revision
pairs, ArbiterUnit pairs for claims about project history.

**Excluded use:** Never use Part narrative text as A side. Never cite Part numbers
in A sides â€” real developer prompts do not reference project history parts.

---

### Type 13 â€” Document state register

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
Prompt:   "plan the work for the current open threads â€” XIIGen export
           teaching, experiment wiring, and orchestration scenarios"

Chosen (PLAN â€” register order, blocking preserved):
  Branch 1 â€” XIIGen export teaching (order 1, unblocked):
    Produce pair rows from the 1,080 usable rows in the XIIGen export.
    No dependencies â€” can start immediately.

  Branch 2 â€” experiment wiring (order 2):
    Wire all 27 positive experiments into project 21.
    Dependency: Branch 1 must be complete. Wiring without teaching data
    produces a wired model with no training signal.

  Branch 3 â€” orchestration scenarios O1-O5 (order 3):
    Run the five scenario proofs end to end.
    Dependency: Branches 1 and 2 must both be complete. Scenarios require
    both teaching data and wired experiments.

Rejected (PLAN â€” parallel, ignores blocking):
  All three threads can run in parallel â€” start everything simultaneously.
[WRONG: DOCUMENT-STATE-REGISTER-v8 marks experiment wiring as explicitly
blocked until teaching is complete, and scenarios as blocked until both
teaching and wiring are complete. The blocking relationships reflect real
pipeline dependencies â€” running in parallel means wiring proceeds without
training signal and scenarios run against an unwired model.]
```

**How to use for CODE_Q document authority pairs:**

```
Prompt:   "which session state document is current?"

Chosen:   "SESSION-STATE-2026-05-06-v15.md â€” recorded in the register
           as superseding v13, covering workstreams 6/7, decisions A47-A51."

Rejected: "SESSION-STATE-2026-05-06-v13.md"
[WRONG: v13 is superseded. Decisions A47-A51 (including A50 rehabilitating
E24) do not exist in v13. Using v13 makes those decisions invisible.]
```

**Overlap ratio:** 0.10â€“0.25 for thread ordering; 0.05â€“0.15 for document version
pairs (superseded vs current version names are entirely non-overlapping tokens).

**Pair types extractable:** PLAN thread-ordering pairs, CODE_Q document-authority
pairs, ArbiterUnit pairs for document version claims.

**Excluded use:** Never invent blocking relationships not in the register. Never
use Gate B bash commands as A sides.

---

### Type 14 â€” Behavioral rules documents

**What it is:** The operational rules governing valid plans, forbidden output
patterns, and required behaviors. Examples:

```
WHAT-TO-DO-WHAT-NOT-TO-DO-v2.md through WHAT-TO-DO-WHAT-NOT-TO-DO-v12.md
```

These documents are a direct source of two DPO pair types no other type provides:

1. **Forbidden output as rejected** â€” each "What not to do" rule names an exact
   output pattern that must not appear. That pattern is the rejected. The
   compliant output is the chosen.

2. **Rule violations that look correct** â€” the hardest pairs come from rules that
   are non-obvious. A plan without Gate B commands looks complete. A Node 01 A-side
   with a raw string looks correct to a reader unfamiliar with the v2 contract.

**How to use â€” forbidden output as rejected:**

```
Prompt:   "write the A-side for a Node 01 pair about implementing JWT auth"

Chosen (v2 contract â€” current):
  A:
    transition_type: feature_addition
    selected_flow_id: "security-verification-34-flow"
    human_context: "Add JWT authentication to the API with tenant isolation"
    confidence: 0.87

Rejected (v1 contract â€” forbidden in v2+):
  A: "Add JWT authentication to the API with tenant isolation"
[WRONG: WHAT-TO-DO requires Node 01 A-side to be ILogicalUnit<SelectedLogicalTransition,
DomainPacket[]>. A raw string is the v1 contract, explicitly superseded in v2.
The canonical design pattern header says "Do not use this v1 file for pair writing."]
```

**How to use â€” plan missing Gate B:**

```
Prompt:   "write a plan for adding CompiledDomainClassifierUnit"

Chosen (plan with Gate B):
  Phase 1 â€” create wrapper:
    ...implementation description...
    Gate B:
      dotnet build LlmFromScratch.sln 2>&1 | tail -3
      # Expected: Build succeeded, 0 errors
      dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
      # Expected: floor or higher, 0 failed

Rejected (plan without executable Gate B):
  Phase 1 â€” create wrapper:
    ...implementation description...
    Verify: build succeeds and tests pass.
[WRONG: WHAT-TO-DO requires Gate B to contain exact bash commands, not a
prose instruction. A plan without executable Gate B cannot be dispatched to
an executor â€” the executor has no command to run. "Verify tests pass" is
indistinguishable from a plan that was never verified.]
```

**Version check before using a rule:** Check that the rule appears in the most
recent version. A46 (E24 DO NOT IMPLEMENT) from v10 was superseded in v11 by A50.
Pairs built from superseded rules train the model on invalidated constraints.

**Overlap ratio:** 0.05â€“0.15 for contract pairs (old type name vs new type name â€”
entirely different tokens). 0.15â€“0.25 for Gate B presence pairs (compliant vs
incomplete structure).

**Pair types extractable:** PLAN Gate-B-present pairs, contract-compliance pairs,
ArbiterUnit rule-violation pairs.

**Excluded use:** Never use a rule from a version where it has been superseded.
Never use rule text as A side.

---

### Type 15 â€” Session start prompt documents

**What it is:** Operational checklists governing how sessions open and how rounds
close. Examples:

```
SESSION-START-PROMPT-v2.1.md through SESSION-START-PROMPT-v9.md
SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v1.md through v6.md
```

These contain **pre-work gates** (must pass before work begins) and **post-work
gates** (must complete before round closes). Both are DPO pair sources: correct
session behavior is the chosen, skipped gate is the rejected.

**How to use â€” live test count gate:**

```
Prompt:   "start the session â€” workstream: extend PLAN pair coverage"

Chosen (correct â€” live count):
  Session type: ARCHITECT
  Baseline: [run dotnet test live] â€” 1224 passing, 0 failing â€” confirmed
  Open threads: Thread 1 (XIIGen export teaching) highest priority
  Intended round boundary: extend PLAN pairs from rows 521-727

Rejected (skipped gate â€” count from memory):
  Session type: ARCHITECT
  Test count: 1224 (from last session)
  Starting PLAN pair work
[WRONG: SESSION-START-PROMPT requires live test count â€” never from memory.
"From last session" is the exact failure pattern the rule was written to
prevent. A regression introduced between sessions is invisible without a
live run.]
```

**How to use â€” round close gate:**

```
Prompt:   "close the round â€” 12 PLAN pairs extracted"

Chosen (correct close â€” manifest + verification):
  Files changed: plan-pairs-round-03.jsonl (new)
  Verification: overlap_ratio < 0.35 for all 12 pairs âœ“
  Manifest written: manifest-round-03.md (what changed / why / how verified)
  Zip: round-03.zip containing pair file + manifest + session start prompt

Rejected (incomplete close â€” no manifest):
  12 pairs written. Zip: round-03.zip
[WRONG: SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING requires a manifest
naming what changed, why, and how verified. A zip without a manifest cannot
be evaluated by a subsequent session. Verification before close is also
required â€” an unverified batch may contain negative-margin pairs.]
```

**Overlap ratio:** 0.10â€“0.20 for gate-present vs gate-skipped pairs (live
verification vocabulary vs memory-recall vocabulary).

**Pair types extractable:** Session management pairs, round closure pairs,
ArbiterUnit pairs (missing mandatory gate = reprocess).

**Excluded use:** Never use checklist items as A sides. A sides must be human
instructions ("start the session"), not checklist lines.

---

### Type 16 â€” Versioned document evolution

**What it is:** The relationship between successive versions of the same document.
This applies to documents of any type. Examples:

```
DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v1 â†’ v2 â†’ v3 â†’ v3-4
SESSION-STATE-v2 â†’ v3 â†’ ... â†’ v15
WHAT-TO-DO-WHAT-NOT-TO-DO-v2 â†’ v3 â†’ ... â†’ v12
IMPLEMENTATION-DOCUMENTATION-v1 â†’ v2 â†’ ... â†’ v8
```

**Why this is a distinct type:** Version evolution produces the **superseded-
vocabulary pair** â€” a pair class no single document version provides. The old
vocabulary is the rejected. The new vocabulary is the chosen. The revision
documents why the change was made.

**Extraction rule:** Every versioned document has a "changes from vN" section.
That section names superseded claims. Each superseded claim â†’ one rejected.

**Key superseded-vocabulary patterns in the corpus:**

```
v1 â†’ v2 of canonical design pattern:
  Superseded: "Node 01 A-side = UserPrompt (raw string)"
  Current:    "Node 01 A-side = SelectedLogicalTransition"
  Pair: prompt about Node 01 input type â†’ chosen=SelectedLogicalTransition,
        rejected=UserPrompt

v1 â†’ v8 of implementation documentation:
  Superseded: "1-layer model, hidden dim 16" (pre-A58)
  Current:    "4-layer model, hidden dim 32" (A58)
  Pair: prompt about model config â†’ chosen=4L/32 dim, rejected=1L/16 dim

  Superseded: "inference uses raw prompt for KNN" (pre-A59)
  Current:    "inference compiles prompt before KNN" (A59)
  Pair: prompt about inference path â†’ chosen=compiled, rejected=raw

v10 â†’ v11 of behavioral rules:
  Superseded: "E24 DO NOT IMPLEMENT (A46)"
  Current:    "E24 rehabilitated (A50)"
  Pair: prompt about E24 â†’ chosen=safe with per-segment contract,
        rejected=wait until E21 verified

v13 â†’ v15 of session state:
  Superseded: "Layer 1 transition orchestrator planned"
  Current:    "Layer 1 locked out (A47)"
  Pair: prompt about Layer 1 â†’ chosen=locked out, rejected=planned
```

**The three-check rule before using any versioned document:**

1. Is this the most recent version? Check document state register.
2. Does the claim appear in a "changes from" section of a newer version?
   If yes, it is superseded â€” use the newer version's vocabulary for chosen.
3. Does the document have a "do not use this file" header? (v1 of the canonical
   design pattern has this.) If yes, it may only be used as rejected source.

**Overlap ratio:** 0.05â€“0.15. Old and new contract vocabulary are often entirely
non-overlapping (UserPrompt vs SelectedLogicalTransition; 1-layer vs 4-layer).

**Pair types extractable:** CODE_Q contract accuracy, ARCH decision-revision,
ArbiterUnit outdated-contract pairs.

**Excluded use:** Never use superseded vocabulary as chosen. Check the three-check
rule before every pair. A pair where chosen = superseded and rejected = current
is an inverted pair that teaches the wrong answer.

---

### Type 17 â€” Translation and migration plans

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
mandatory prerequisite already written in the document â€” the annotation is extracted,
not constructed.

STATE.json companion files record which phases completed and which were interrupted.
An interrupted-then-restarted phase is a machine-readable correction event: the
interrupted attempt is rejected, the completed restart is chosen.

**How to use for PLAN Type B pairs:**

```
A (Type 1 prompt):
  "plan the migration of the xiigen engine from NestJS to Python â€”
   start with the core infrastructure layer"

Chosen (PLAN â€” correct migration order):
  Branch 1 â€” 3.2 Data Model (order 1):
    Define the Python equivalents of the core entity classes. No dependencies.
    This is the foundation everything else references.

  Branch 2 â€” 3.3 API Contract (order 2):
    Define the Python service interfaces and method signatures.
    Dependency: 3.2 must be complete â€” interface signatures reference entity types.

  Branch 3 â€” 4.1 Backend Services (order 3):
    Implement the Python classes.
    Dependency: both 3.2 and 3.3 must be complete â€” implementation targets
    the defined entities and interfaces.

Rejected (PLAN â€” wrong order: implementation before interfaces):
  Branch 1 â€” 4.1 Backend Services (immediate)
  Branch 2 â€” 3.3 API Contract (after implementation)
  Branch 3 â€” 3.2 Data Model (derived from what was implemented)
[WRONG: implementing before the interface is defined means the implementation
targets its own assumptions. When 3.3 produces different method signatures from
what 4.1 assumed, 4.1 must be rewritten. The TRANSLATION_PHASE_PLAN.md explicitly
orders: entity schema â†’ interface contract â†’ implementation. Reversing this order
always produces rework at the interface boundary.]
```

**How to use STATE.json interruption events:**

```
Interrupted state in JSON:
  { "phase": "3.3_api_contract", "status": "interrupted",
    "reason": "wrong contract shape â€” missing pagination fields",
    "attempted_artifact": "api-contract-v1.md" }

Completed restart in JSON:
  { "phase": "3.3_api_contract", "status": "completed",
    "artifact": "api-contract-v2.md",
    "correction": "added cursor-based pagination contract per 3.3 spec" }

Extraction:
  A:        phase prompt â€” "define the API contract for the reporting service"
  Rejected: content of api-contract-v1.md
            [WRONG: the contract shape was missing cursor-based pagination.
            The 3.3 spec requires pagination contract before implementation begins.]
  Chosen:   content of api-contract-v2.md
```

Rate before including: interruption from wrong approach = High. Interruption from
missing file = Low (exclude â€” not a reasoning correction).

**Expected overlap ratio:** 0.30â€“0.45 for Type B ordering pairs.

**Pair types extractable:** PLAN Type B (primary), correction events from STATE.json
interruptions, SchemePlannerUnit B-sides.

**Excluded use:**
```
Do not use migration plan phase descriptions as A sides.
Do not use STATE.json field values as chosen content without wrapping in
the correct measurement schema.
```

---

### Type 18 â€” Flow portability and protocol documents

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
A portability protocol is a contract with no alternative â€” it must be satisfied or the
flow is rejected.

**How to use for ArbiterUnit pairs:**

```
A (ArbiterReviewRequest):
  artifact_type: "flow_service_file"
  artifact_content: "flow service with hardcoded tenantId = 'tenant-abc' in constructor"
  review_criteria: "verify portability: no hardcoded tenant identifiers"
  source_rule: "FLOW-PORTABILITY-TEST-PROTOCOL check P-3"

Chosen (ArbiterUnit â€” reprocess: portability violation):
  verdict: reprocess
  finding: "hardcoded tenantId in constructor violates the portability contract.
            The tenantId must come from IExecutionContext.TenantId, not from a
            constant. This flow will fail for any tenant other than 'tenant-abc'.
            FLOW-PORTABILITY-TEST-PROTOCOL check P-3 requires zero hardcoded
            tenant identifiers."
  requirement: "replace constant with IExecutionContext.TenantId injection
               before this flow can be promoted"

Rejected (ArbiterUnit â€” concern: wrong severity):
  verdict: concern
  finding: "tenantId is hardcoded. Consider making it configurable."
[WRONG: a hardcoded tenant identifier is a portability violation that causes wrong
output for every other tenant â€” not a concern to be noted. The correct verdict is
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
contract â€” they cannot run before the contract exists. Early fork creates
implementation that targets an unfinished spec.]
```

**Expected overlap ratio:** 0.10â€“0.20 for portability violation pairs. 0.30â€“0.45
for fork semantics pairs.

**Pair types extractable:** ArbiterUnit portability reprocess pairs, SchemePlannerUnit
fork/join ordering pairs, Stage 9 corruption cases.

**Excluded use:**
```
Never derive a portability violation from inference â€” only from a named check
in the protocol document.
Do not confuse with Type 9 decisions â€” protocols are contracts with no valid
alternative; decisions are choices between alternatives.
```

---

### Type 19 â€” Agent tooling instructions

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
session. Type 19 defines what the agent tool must do when given a task â€” it is an
executable contract, not a behavioral preference. The audience is the agent, not the
human.

**How to use for PLAN dependency-ordering pairs:**

AGENTS.md often contains explicit step sequences with dependencies:
"first read project knowledge, then check existing tests, then write code,
then run tests, then commit."

```
A (Type 1 prompt):
  "add the missing portability check to the flow service"

Chosen (PLAN â€” correct agent execution order per AGENTS.md):
  Branch 1 â€” reconnaissance (order 1):
    Read the flow service file. Read the portability protocol.
    Dependency: must read before writing â€” cannot know what to change
    without knowing what exists and what the protocol requires.

  Branch 2 â€” implementation (order 2):
    Write the portability fix.
    Dependency: reconnaissance must be complete.

  Branch 3 â€” verification (order 3):
    Run the portability check test.
    Dependency: implementation must be complete. Do not commit before green.

Rejected (PLAN â€” wrong order: write before read):
  Branch 1 â€” implementation (immediate): write the fix
  Branch 2 â€” run tests: fix failures as they appear
  Branch 3 â€” read the protocol: check what was required
[WRONG: AGENTS.md requires reconnaissance before any write. Writing without
reading the existing code and the protocol produces a fix that may solve the
symptom while violating a different portability rule. The protocol read must
happen before the implementation, not as a retrospective check.]
```

**Expected overlap ratio:** 0.15â€“0.30 for PLAN ordering pairs. 0.10â€“0.20 for
ArbiterUnit tool-compliance pairs.

**Pair types extractable:** PLAN Type B ordering pairs (reconnaissance before write),
ArbiterUnit tool-compliance pairs, SchemePlannerUnit branch ordering.

**Excluded use:**
```
Do not use AGENTS.md instruction text as A side.
A side must be a Type 1 developer task prompt.
Do not conflate with Type 15 (session gates) â€” Type 19 is within-task
execution order, not session setup.
```

---

### Type 20 â€” Skill library documents

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
executable form. The rejected for Type 20 pairs is always Type C (wrong domain â€”
wrong skill applied). Type A rejected (adjacent sub-phase) is not appropriate here
because skill documents define what belongs to the skill, not the relationship
between adjacent sub-phases.

**Core rule: Type C rejected only.** Type A requires adjacent sub-phase vocabulary
from Type 5 (topology). Type 20 provides skill boundary language, which is different.

**How to use for Type C domain-boundary pairs:**

```
A (Type 1 prompt):
  "review the authentication implementation for the tenant service"

Chosen (QA â€” 6.3 Security and Compliance Review):
  The review checks whether the implementation satisfies the auth model (3.4).
  Findings: JWT validation present (pass), tenant claim isolation confirmed (pass),
  bypass-path registry not checked (concern â€” the security review skill requires
  bypass-path check in every security review).

Rejected (ARCH â€” wrong domain: design instead of review):
  The auth model for the tenant service should define the JWT structure: sub claim
  = userId, tenantId claim = tenant partition key, roles claim = RBAC array.
  Consider adding a refresh token family to prevent reuse.
[WRONG: "review the authentication implementation" asks whether the existing
implementation satisfies the model â€” 6.3 review scope. Describing what the model
should contain is 3.4 design scope. The skill library distinguishes these: the
security review skill checks compliance with a model that already exists; the
security model skill defines the model. Using a design skill for a review prompt
misses the compliance check entirely.]
```

**Type 20 annotation vs Type 5 annotation:** Type 5 cites the sub-phase Purpose
field ("3.4 owns..."). Type 20 cites the skill boundary ("the security review skill
checks compliance with an existing model; the security model skill defines it"). More
specific trigger condition language, not just ownership.

**Expected overlap ratio:** 0.15â€“0.30.

**Pair types extractable:** Type C domain-boundary pairs (primary), ArbiterUnit
skill-compliance verdicts.

**Excluded use:**
```
Do not use skill definitions as A sides.
Do not use skill manifests as chosen content for ARCH pairs.
Type C only â€” never Type A.
```

---

### Type 21 â€” Deep research and external domain reports

**What it is:** Research documents drawn from external sources â€” market analysis,
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

Chosen (CODE_Q â€” accurate external mechanism):
  "In GraphQL federation each subgraph handles its own authorization. The gateway
   forwards the original request headers including auth tokens. Each subgraph must
   validate the tenant claim independently â€” the gateway does not enforce tenant
   isolation. If a subgraph omits tenant validation, any authenticated user from
   any tenant can query that subgraph's data. The Apollo federation spec (section4.2)
   states authorization is a subgraph responsibility, not a gateway guarantee."

Rejected (CODE_Q â€” oversimplified, incorrect isolation model):
  "GraphQL federation handles multi-tenant isolation at the gateway level. The
   gateway validates the tenant token once and all subgraphs inherit the validated
   tenant context automatically."
[WRONG: federation does not enforce per-subgraph isolation from the gateway. Each
subgraph must validate independently. The research document cites the Apollo
federation spec section4.2: authorization is a subgraph responsibility. The rejected
model would allow any authenticated tenant to query any subgraph that skips
its own validation.]
```

**The annotation must cite a specific external source section.** Without a citation,
the pair has no authority and becomes an opinion pair â€” exclude it.

**Quality gate for Type 21 pairs:**
```
â–¡ Research document cites a specific external standard or spec section
â–¡ Chosen content is verifiable against that external source
â–¡ Domain is CODE_Q only
â–¡ No locked Type 9 decision contradicts the chosen content
â–¡ Research document is dated â€” check whether the cited spec has been superseded
```

**Expected overlap ratio:** 0.20â€“0.40. Strong annotation critical.

**Pair types extractable:** CODE_Q external mechanism pairs only.

**Excluded use:**
```
Never use as B-side authority for ARCH or PLAN pairs.
Never override a Type 9 locked decision with external research.
Never use undated research without a specific cited source.
"Consider X approach" without a spec citation is opinion â€” exclude.
```

---

### Type 22 â€” Execution state JSON files

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
training metrics â€” loss, accuracy, reward margin, checkpoint bytes. These are numbers
that measure model learning. Type 22 records workflow execution progress â€” which phases
are `completed`, `in_progress`, `blocked`, or `interrupted`. Different data, same
missing-field pair logic.

**Two subtypes:**

**Subtype 22a â€” Missing field (overlap < 0.20)**

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
what was produced. The state file is the audit trail â€” without the artifact path,
there is no way to confirm the correct version was produced or to replay the phase
from this checkpoint.]
```

**Subtype 22b â€” Interruption correction event (overlap 0.10â€“0.25)**

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
`status = "in_progress"` â€” only `completed` or `interrupted` states are stable.

**Expected overlap ratio:** 22a < 0.20; 22b 0.10â€“0.25.

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

**Zone A â€” Acceptance criteria and BFA requirements.** Markers: "must", "gate",
"pass/fail", "the flow is complete when". Apply ArbiterUnit pair construction rules.

**Zone B â€” Architectural decisions for this domain.** Markers: "owns", "must not",
"boundary", "depends on", "forbidden". Apply ARCH Type A pair construction rules,
same as Type 2.

**Zone C â€” Task type and sub-phase assignments.** Markers: "sub-phase", "task type",
"order", "depends_on", "parallel", "after". Apply PLAN Type B pair construction rules,
same as Type 10/17.

**Rule:** Before building any pair from a business flow spec, identify the zone of the
source paragraph. Zone A â†’ ArbiterUnit rules. Zone B â†’ ARCH Type A rules.
Zone C â†’ PLAN Type B rules.

**Expected overlap ratio by zone:** Zone A: 0.15â€“0.30. Zone B: 0.10â€“0.20.
Zone C: 0.30â€“0.45.

---

### Type 6 additions â€” Excluded families from scan

**Source archives (*.zip files):** A ZIP file is a delivery artifact, not a document.
The ZIP itself has no DPO value. Unpack, classify each extracted file by content type
(Types 1â€“22), apply corresponding rules, delete temp extraction. Never treat a ZIP
filename as a document title for an A-side prompt.

**Raw merged text dumps (extracted_merged.txt, planning session.txt):** Concatenations
of multiple conversations or documents without segment boundaries. Using them directly
produces prompts that contain fragments from multiple unrelated contexts. Required
preprocessing: segment by known delimiters ("---", "Human:", "Assistant:", timestamps,
document headings), classify each segment independently, apply Type 1 eligibility test
per segment, discard ineligible segments. After preprocessing the segments are
reclassified as their content types â€” they are no longer Type 6.

---

### Type 23 â€” Gap remediation and fix documents

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
what is implemented vs not â€” it does not record why a fix attempt failed. Type 8
(artifact examination) records verdicts on artifacts â€” it does not record the repair
process. Type 23 records the full repair arc: gap â†’ wrong approach â†’ correct approach.

This is the primary source for BUG and ARCH repair pairs where the rejected is
not just a wrong domain but a specifically wrong repair strategy.

**How to use for BUG repair pairs:**

```
A (Type 1 prompt):
  "the AstRelevanceRanker BM25+KNN fusion is returning wrong scores â€”
   fix the ranking defect"

Chosen (BUG â€” correct fix from remediation plan):
  "The fusion alpha weight is computed per-query but applied to the global
   ranker state. Fix: move alpha computation inside the per-query scoring
   loop. Each query must compute its own alpha from the query predicate
   profile, not read the last-written global alpha.
   Evidence: REMEDIATION-PLAN confirms zero alpha drift after this fix."

Rejected (BUG â€” wrong fix from gap document's rejected attempt):
  "Reset alpha to 0.5 (the default) before each scoring call. This
   prevents the previous query's alpha from bleeding into the next query."
[WRONG: resetting to default alpha disables the query-adaptive tuning
entirely. The per-query alpha from E34V2 is what produces +8% hit@1 â€”
a global reset loses the gain. The correct fix moves computation inside
the loop while preserving per-query values. The remediation plan
explicitly records the reset attempt as a failed fix that introduced
worse accuracy than no fusion at all.]
```

**How to use for ARCH repair pairs (false-pass correction):**

Gap remediation documents often record a false pass â€” a change that appeared to
fix a gap but left a structural defect. The false pass is the rejected.

```
A (Type 1 prompt):
  "the portability check for flow 21 passes but the tenantId is still
   hardcoded in the constructor â€” plan the correct fix"

Chosen (ARCH â€” correct fix from fix map):
  "The hardcode is in the constructor parameter default, not in the body.
   The check scanned the body only. Fix: replace the default parameter
   with IExecutionContext injection and remove the default value entirely.
   The portability check must be updated to scan constructor parameter
   defaults, not only the body."

Rejected (ARCH â€” false-pass fix from gap doc):
  "Wrap the hardcoded value in a configuration lookup:
   tenantId = config.GetValue('default_tenant') ?? 'tenant-abc'.
   This removes the literal string from the constructor."
[WRONG: replacing a hardcoded literal with a config lookup still produces
a fixed tenant at construction time. The portability check sees no literal
string and passes â€” but the behavior is the same: one tenant baked in at
construction. The PER-FLOW-FIX-MAP records this as a false-pass pattern.
The correct fix removes the default entirely and injects from context.]
```

**Three-part structure extraction rule:**

Before building any pair from a gap remediation document:
1. Identify the gap description (what was wrong)
2. Identify the wrong-fix attempt (if present) â€” this becomes rejected
3. Identify the correct fix with evidence â€” this becomes chosen

If the document only has gap + correct fix (no wrong-fix attempt), construct
rejected using the most plausible wrong repair from the ARCH or BUG confusion
tables, not from the gap document itself.

**Expected overlap ratio:** 0.15â€“0.30 for gap repair pairs. Wrong-fix vocabulary
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
Do not invent a wrong-fix attempt if the document does not record one â€”
use ARCH/BUG confusion tables instead.
Verify that the correct fix in the document has evidence attached (test
result, run output, or verification statement) â€” a fix without evidence
is a claim, not ground truth.
```

---

### Type 24 â€” Generated code debug and correction histories

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
Claude generated as output â€” the defect is in the generated artifact, not in the
response itself. The A side is the generation request, the rejected is the generated
code before debugging, the chosen is the code after correction.

This distinction matters for overlap ratio: Type 3 corrections have overlap < 0.25
because the wrong and correct answers come from different reasoning paths. Type 24
corrections often have higher overlap because the wrong and correct code share most
of their structure â€” only specific lines differ. The annotation must carry more of
the signal.

**How to use for BUG repair pairs:**

```
A (Type 1 prompt):
  "generate the CSS layout for the tenant dashboard â€” two-column grid
   with the sidebar collapsing below 768px"

Chosen (generated code â€” after debug correction):
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

Rejected (generated code â€” before debug, from css-debug-history.json):
  .tenant-dashboard {
    display: flex;
    flex-direction: row;
  }
  .sidebar { width: 240px; flex-shrink: 0; }
  @media (max-width: 768px) {
    .tenant-dashboard { flex-direction: column; }
  }
[WRONG: the flex implementation collapses the sidebar to full width at
768px but does not reorder it â€” the sidebar appears above content on
mobile. The grid implementation uses 'order: 2' to push sidebar below
the main content. The debug history records the flex version as the
defect that caused visual regression on mobile devices.]
```

**Quality gate for Type 24 pairs:**

Type 24 pairs have higher overlap than Type 3 because generated code shares
structure. Apply the overlap test strictly.

```
â–¡ Overlap ratio computed before including
â–¡ If overlap_ratio > 0.45: annotation must introduce â‰¥ 5 non-overlapping
  tokens describing why the wrong code fails functionally (not just which
  lines differ)
â–¡ Debug history confirms the wrong code was actually generated (not
  constructed) â€” the rejected must come from a real generation event
â–¡ The correct code has a verification signal in the debug history
  (build passed, test passed, visual confirmed)
```

**Expected overlap ratio:** 0.25â€“0.50. Generated code corrections share more
structure than conversation corrections. Annotation weight is higher than Type 3.

**Pair types extractable:** BUG generated-code repair pairs, REFACTOR generated-code
improvement pairs, UI/layout correction pairs for sub-phase 4.2 and 1.1.

**Excluded use:**
```
Do not use debug trace text (stack traces, browser console output) as A sides.
A side is always the original generation request prompt.
Do not include pairs where the "wrong" version is a deliberate placeholder â€”
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
Zone A â€” Visual findings and defect reports
  Markers: "defect", "visual regression", "wrong color", "layout break",
           "accessibility violation", "fails visual check"
  Apply: Type 8 ArbiterUnit rules â€” findings are verdicts on artifacts.
  Chosen: concern or reprocess verdict with specific visual finding.
  Rejected: pass verdict without checking the visual relationship.

Zone B â€” Role/screen maps and design system references
  Markers: "role", "screen", "layout", "design token", "component spec"
  Apply: Type 2 (ARCH/UI) B-side rules for sub-phases 1.1 and 4.2.
  Use as chosen content for UI ARCH boundary pairs.

Zone C â€” UML and coverage analysis
  Markers: "UML", "coverage", "class diagram", "sequence diagram",
           "which screens are covered"
  Apply: Type 7 extension rules â€” coverage as implementation status.
  Use for knowledge-edge detection pairs (what is covered vs not).
```

---

**Flow generator guidance library (U16)**

Documents: GUIDE-B01 through GUIDE-B50 files, LIST-A-COVERAGE-REPORT,
LIBRARY-MASTER-INDEX.

```
Zone A â€” GUIDE-B procedure steps
  Markers: "Step N", "required before", "gate", "prerequisite"
  Apply: Type 10 PLAN ordering rules.
  Chosen: correct step order with dependency chain.
  Rejected: steps in wrong order or gate skipped.

Zone B â€” Coverage reports
  Markers: "covered", "missing", "gap", "LIST-A"
  Apply: Type 8 arbiter rules (coverage = verdict on completeness)
         or Type 13 document register rules (which guide is current).

Zone C â€” Source maps and library index
  Markers: "maps to", "source", "LIBRARY-MASTER-INDEX"
  Apply: Provenance only. Not a training pair source.
  Use for vocabulary calibration and source tracing.
```

---

**Benchmark, stress test, and dry-run logs (U20)**

Documents: V62_BFA_STRESS_TEST, REFERENCE-RAG-BENCHMARK-PLAN, DRY-RUN files,
EXECUTION-LOG, EXPECTED-TRACE.

```
Zone A â€” Actual run metrics (measured output)
  Markers: "elapsed_ms", "cpu_percent", "accuracy", "loss", recorded numbers
  Apply: Type 4 measurement evidence rules.
  Use as B-side content for ModelTrainingUnit pairs.

Zone B â€” Expected traces and reference plans
  Markers: "expected", "should produce", "reference output", "plan"
  Apply: Type 10/14 authority rules.
  These are the ground truth for what a correct run looks like â€”
  use as chosen content, with a run that diverges from expected as rejected.

Zone C â€” Dry-run and stress reports (planned but not yet measured)
  Markers: "dry run", "stress test plan", "hypothetical", "expected to"
  Apply: B-side authority with explicit status flag in the pair file:
         "evidence_type": "expected_not_measured"
  Do not use as Type 4 measurement evidence â€” these are plans, not results.
  Use only as test plan authority (Type 10 rules).
```

---

**Prompt handoff and session briefs (U21)**

Documents: PROMPTS.md, basic_prompt.txt, CLAUDE-CODE-SESSION-PROMPT,
HANDOFF-CONTRACT, SESSION-BRIEF.

```
Zone A â€” Session start gates (pre-work and post-work conditions)
  Markers: "before any work", "live test count", "manifest required",
           "gate", "must verify"
  Apply: Type 15 rules exactly.

Zone B â€” Prompt templates (reusable instruction patterns)
  Markers: "template", "insert here", "{{placeholder}}", "use this format"
  Apply: Type 20 (skill library) rules for domain boundary pairs.
  Prompt templates define what a task should look like â€” rejected is
  a prompt that violates the template's required structure.

Zone C â€” Handoff contracts and session briefs
  Markers: "handoff to", "next session", "carry forward", "context for"
  Apply: Type 19 (agent tooling instructions) rules for ordering pairs.
  Handoff contracts define what the receiving agent must do and in what
  order â€” rejected skips a required handoff step.
```

---

### Limited-authority source families (U04, U08, U12, U24)

These families are not excluded, but they have boundaries on what they can
contribute. Crossing the boundary produces wrong authority: the model learns
a platform-specific pattern as if it were a general XIIGen orchestration rule.

---

**Plugin and marketplace extension docs (U04)**

Documents: extensions research, Wordpress integration plans, Adobe adaptation plans,
plugin packages, platform adaptation documents.

Boundary rule: **A-side prompts are valid. B-side authority is limited to
platform-specific integration sub-phases only.**

```
Valid use â€” A-side prompts (Type 1):
  Real developer prompts from plugin integration sessions are eligible
  Type 1 material when they pass the eligibility test (named artifact,
  dependency language, or technical specificity).
  Example eligible: "wire the Wordpress webhook to XIIGen flow 21 â€”
  the payload comes in as JSON, needs to route to the correct tenant"
  Example ineligible: "let's think about the Wordpress integration strategy"

Valid use â€” B-side for platform integration pairs:
  Sub-phases 4.5 (Integration Connectors) and 3.3 (API Contract) for the
  specific platform being integrated. Chosen content describes the correct
  adapter or contract shape for that platform.

Forbidden use â€” B-side for core XIIGen pairs:
  Do not use plugin plans as B-side authority for ARCH (3.1, 3.2) or
  PLAN orchestration pairs. Plugin integration decisions are platform-specific
  and do not generalise to the core XIIGen architecture.
  Do not use marketplace strategy as evidence for any technical pair.
```

Overlap ratio for valid platform integration pairs: 0.15â€“0.30.

---

**Design conversion documents (U08)**

Documents: Figma-to-code sessions, Adobe Express conversion maps,
FIGMA_TO_EXPRESS_API_MAP.md, visual pipeline specs, design-to-code plans.

Boundary rule: **A-side UI prompts are valid. B-side authority is limited to
sub-phases 1.1 (Design System and Tokens) and 4.2 (Frontend and Client Integration).**

```
Valid use â€” A-side prompts (Type 1 with UI predicate):
  Prompts from Figma-to-code or design conversion sessions where the
  developer is asking about a specific visual component, layout rule,
  or design token. These carry UI predicate vocabulary.
  Example eligible: "the dashboard grid breaks at 768px â€” the sidebar
  overlaps the content area on tablet"
  Example ineligible: "let's figure out the Figma export strategy"

Valid use â€” B-side for 1.1 and 4.2 pairs:
  Design system token specs â†’ chosen for 1.1 Design System pairs.
  UI conversion maps â†’ B-side vocabulary for 4.2 Frontend pairs.
  These documents model geometry/layout-specific mistakes that normal
  Type 2 architecture documents do not cover. Use them where layout
  and visual relationships are the subject of the pair.

Forbidden use:
  Do not use design conversion maps as B-side authority for any sub-phase
  outside 1.1 and 4.2.
  Do not use Figma export plans as PLAN or ARCH pair authority.
```

Note: debug histories from Figma-to-code sessions are Type 24 material
(generated code correction histories) when they record a wrong generated
layout and its corrected version.

Overlap ratio for valid 1.1/4.2 pairs: 0.20â€“0.35.

---

**Capability catalogs and matrices (U12)**

Documents: CAPABILITY_MATRIX.md, TASK_TYPES_CATALOG, UNIFIED_SOURCE_INDEX,
SKILLS_FACTORY_RAG.

These documents contain several structurally different things. The zone mapping
determines what each part can contribute.

```
Zone A â€” Capability matrices (what is implemented, what is not)
  Markers: "implemented", "not implemented", "partial", "gap", "coverage"
  Apply: Type 7 extension rules.
  Use for knowledge-edge detection pairs: a sub-phase not in the capability
  matrix = KNOWLEDGE_EDGE result. A sub-phase incorrectly marked as implemented
  = wrong-status rejected (same logic as Type 7).
  Overlap ratio: 0.10â€“0.25.

Zone B â€” Task type catalogs (sub-phase contracts and task type schemas)
  Markers: "TaskType", "SubPhaseId", "contract", "schema", "predicate"
  Apply: Type 5 extension rules (topology authority).
  Task type vocabulary feeds DomainTranslatorUnit pairs and router pairs.
  Use as B-side vocabulary source for sub-phase routing pairs alongside
  the seeder Purpose fields.
  Overlap ratio: 0.10â€“0.25.

Zone C â€” Source indexes and RAG indexes
  Markers: "source index", "RAG", "document list", "provenance", "index"
  Apply: Vocabulary calibration only.
  These documents describe which documents exist and what they contain.
  They are not pair sources. Do not use as A-side or B-side content.
  Use only for: predicate lexicon expansion, vocabulary calibration,
  and source tracing during audit.
```

Assessment note: capability matrices and task catalogs are useful as
coverage-edge and routing evidence. Only Zone C (source indexes and RAG
indexes) is vocabulary/provenance-only. Zones A and B produce real pairs.

---

**Marketplace legal and store assets (U24)**

Documents: STORE_LISTING.md, LICENSE, NOTICE.md, privacy.html,
sitemap.xml, marketplace.json, plugin package manifests.

Default rule: **Excluded from all training.**

These are operational distribution metadata for plugin packages. They describe
licensing terms, privacy policies, and marketplace listing requirements â€” not
XIIGen development decisions or orchestration behavior.

Named exception: if the task being taught is specifically about marketplace
listing, license compliance, or plugin distribution workflow, these documents
can be used as B-side authority for that narrow scope. This applies only to
pairs where the domain is explicitly marketplace integration (sub-phases in
the plugin extension area).

```
Excluded (default):
  LICENSE, NOTICE.md â†’ operational compliance artifacts
  privacy.html, sitemap.xml â†’ web distribution metadata
  marketplace.json â†’ store listing fields

Exception â€” narrow scope only:
  STORE_LISTING.md â†’ B-side authority for pairs about writing correct
  marketplace listing descriptions; A-side is a developer prompt about
  what to write in the listing. Not a general orchestration source.
```

Canonicality for these files: treat as `secure_operational` unless explicitly
needed for the named exception. Do not log these paths in pair manifests
unless using the named exception.

---

### Addendum limited-authority families (A02, A03, A04, A08, A11, A12)

---

**Dependency and build manifests (A02)**

Files: `package.json`, `*.csproj`, `*.sln`, `*.props`, `*.targets`,
`pnpm-lock.yaml`.

These files define the build graph â€” which projects reference which, which
package versions are pinned, which target frameworks are in use.

Boundary rule: **Build and dependency evidence only. Never general architecture truth.**

Valid DPO use â€” Type B rejected for IMPLEMENT pairs:

A build manifest is authoritative for what dependency is actually installed.
A pair that teaches "use the version in package.json, not the version you assume"
is valid. A pair that teaches architecture from the presence of a package is not.

```
A: "add the GraphQL federation gateway dependency"

Chosen: "@apollo/gateway@2.7.0 â€” the version pinned in package.json.
         Do not use a different version; the workspace lock file pins this
         version for compatibility with @apollo/subgraph@2.7.0."

Rejected: "@apollo/gateway@latest â€” install the newest version to get
           the latest features."
[WRONG: package.json pins 2.7.0 for subgraph compatibility. Installing latest
breaks the pinned workspace dependency. Build manifest pins are the authority
for which version to use, not an estimate of what is current.]
```

Do not derive sub-phase ownership, component boundaries, or orchestration
decisions from build manifests. Dependency presence is not architecture truth.

---

**CI, editor, and repository configs (A03)**

Files: `.github/workflows/*.yml`, `.vscode/settings.json`, `.gitignore`,
`.gitattributes`, `.cursorrules`, `.clinerules`.

These are different from Type 19 (agent tooling instructions). Type 19 covers
machine-facing executable contracts for Claude Code. A03 covers repository
automation policies, editor workspace behavior, and ignore/include rules.

Valid DPO use â€” limited to two scopes:

**Scope 1: CI workflow ordering pairs.** A GitHub Actions workflow with explicit
`needs:` dependencies is a PLAN Type B source for CI execution order. The rejected
reverses or removes the dependency.

```
A: "plan the CI steps for deploying flow 21"

Chosen: "build â†’ test (needs: build) â†’ deploy-staging (needs: test) â†’
         integration-test (needs: deploy-staging) â†’ deploy-prod
         (needs: integration-test, manual approval).
         Each step depends on the prior passing."

Rejected: "build, test, and deploy-staging run in parallel; integration
           tests run after all three."
[WRONG: .github/workflows/deploy.yml shows test needs build and
deploy-staging needs test. Parallel execution skips the dependency.
The workflow file is the authority for CI ordering.]
```

**Scope 2: Tool compliance pairs.** `.cursorrules` and `.clinerules` define
what the tool must and must not do in this repository â€” same logic as Type 19.
When these files exist alongside AGENTS.md, they extend the tool contract.

Excluded: `.gitignore` and `.gitattributes` have no DPO pair value. Editor
settings (`.vscode/settings.json`) are developer preferences, not pair sources.

---

**Executable utility scripts (A04)**

Files: `translate_all_docs.py`, `push_to_github.py`,
`generate_phase_a_fixtures.py`, `build-*.ps1`, `run-tests.sh`.

These scripts encode actual process order through their code paths. The sequence
of function calls is process-order evidence that documents sometimes miss or
describe imprecisely.

Valid DPO use â€” **process order evidence only, from code paths not comments.**

Extract the call sequence from the script's main execution path. This is Type 17
or Type 10 extension material: the script's call order is a more authoritative
source of dependency ordering than the prose description in a plan document when
they conflict.

```
Script extract: translate_all_docs.py calls in order:
  1. load_source_index()
  2. validate_schema(docs)
  3. translate_batch(docs)
  4. write_output(results)
  5. update_ledger(results)

If a plan document says "translate first, validate second" â€” the script is right.
Use script call order as the chosen for a PLAN Type B pair.
```

Quality gate: extract from code paths only. Comment text in scripts is not reliable
evidence â€” use what the code actually does, not what a comment says it does. Do not
use script file text as A side.

---

**Spreadsheets and decision matrices (A08)**

Files: `×ª×›× ×•×Ÿ ×ž×¢×¨×›×ª.xlsx`, `Decision-Matrix-v8-ALL-SCENES.xlsx`,
`Figma-to-CSS-Mapping-Matrix-v*.xlsx`, `GraphRAG_Alternatives_Comparison.xlsx`.

These are bundles of sheets. Each sheet must be classified separately before any
pair is constructed.

```
Sheet zone classification:

Decision matrix sheet (rows = options, columns = criteria, cells = scores)
  Apply: Type 9 extension rules â€” trade-off pairs.
  The option with the highest score is chosen. The rejected is a lower-scored
  option presented as if it were the correct choice.
  Overlap ratio: 0.10â€“0.25 (decision vocabulary vs rejected-option vocabulary).

Mapping matrix sheet (rows = source items, columns = target items, cells = mappings)
  Apply: Type 5 or Type 26 extension â€” mapping authority.
  Use as B-side vocabulary for translation/migration pairs where the mapping
  is the authoritative ground truth (e.g., Figma-to-CSS property map).

Metric or comparison sheet (rows = alternatives, columns = metric values)
  Apply: Type 4 or Type 21 extension â€” measurement or research evidence.
  Use as B-side content for comparison pairs; rejected is wrong metric claim.

Planning/gantt sheet (rows = tasks, sequence implied by row order or dates)
  Apply: Type 17 or Type 10 extension â€” ordering pairs.
  Row order is dependency evidence when the sheet has a clear sequencing intent.

Excluded sheets:
  Raw data dumps with no structure, lookup tables, formatting-only sheets.
```

Preprocessing required: extract sheet contents to structured text before pair
construction. Do not process `.xlsx` files as raw binary.

---

**Localization and language variants (A11)**

Files: `README-HE.md`, Hebrew planning spreadsheets, translated copies of
guide documents.

Rule: **Attach canonical-source metadata. Never count as independent truth.**

A translated document is a mirror of the canonical source in a different language.
It does not constitute independent authority. If the translated version says
something different from the canonical source, the canonical source wins.

Valid DPO use â€” **A-side prompts in the original language only.**

Real developer prompts written in Hebrew or Russian are valid Type 1 A sides â€”
the model must learn to receive input in the developer's actual language. The
B side must still be in the canonical response language (English).

```
Valid A side: "×¢×–×•×¨ ×œ×™ ×¢× ×”×ž×©×š ×œ×—×œ×§ ×”×‘× â€” JOB ×©×ž×—×©×‘ ×”×ª××ž×•×ª ×œ×–×•×’×•×ª"
  â†’ A side is the real prompt. B side is the correct English orchestration response.
  â†’ This is Type 1 eligibility: real developer prompt, whatever language.

Invalid B side: a translated version of a plan document used as chosen content
  for an ARCH pair, where the translation differs from the English canonical source.
  â†’ Use the canonical English document as chosen, not the translation.
```

Required metadata for any pair using a localized document:
```json
{ "source_canonicality": "mirror",
  "canonical_source": "README.md",
  "language": "he",
  "use": "a_side_prompt_only" }
```

---

**Runtime environment and deployment templates (A12)**

Files: `docker-compose.yml`, deployment manifests, `.env.sample`, `skill.yaml`.

These define runtime wiring â€” which services connect to which, what ports are
exposed, what environment variables are required. This is operational contract
evidence for the deployment layer.

Valid DPO use â€” **limited to deployment-layer pairs after secret/sensitive-field
screening.**

```
Valid use:
  docker-compose.yml service dependency order â†’ PLAN Type B source for
  deployment ordering. chosen: correct service startup order.
  rejected: services started before their dependencies.

  skill.yaml â†’ same as Type 20 (skill library) zone mapping. Skill YAML
  files that define skill contracts are Type 20 material.

  .env.sample â†’ shows required environment variables. Source for ArbiterUnit
  pairs: chosen = all required env vars present, rejected = missing required var.
```

Security gate â€” **mandatory before any pair construction from A12:**

```
â–¡ File does not contain real secrets, API keys, passwords, or private keys.
  .env.sample files must use placeholder values only (DATABASE_URL=<your-url>).
  If actual values are present: treat file as secure_operational, exclude.
â–¡ docker-compose.yml does not contain embedded credentials.
â–¡ deployment manifests do not reference private registry credentials inline.
```

---

### Addendum exclusion rules (A06, A07, A09, A10)

---

**Model checkpoint and training artifacts (A06)**

Files: `policy.bin`, checkpoint manifest files, tokenizer vocabulary artifacts,
run checkpoint directories.

Rule: **Binary artifacts excluded from all DPO text training.**

`policy.bin` and other binary checkpoint files contain model weights â€” they
are not text documents and cannot produce DPO pairs. Do not attempt to read
or include them as text.

**Checkpoint manifest files are Type 4 extension material** â€” they record
training lineage (parent checkpoint, training step count, loss at checkpoint).
Use manifests as B-side measurement evidence following Type 4 rules. The
manifest content describes training state; the binary is not accessible.

```json
Manifest fields usable as Type 4 B-side content:
  parent_checkpoint_id, training_step_count, dpo_loss_at_checkpoint,
  lora_adapter_bytes, base_model_version, created_at
```

---

**Training dataset row files (A07)**

Files: `rows.jsonl`, `node-*-set.jsonl`, `COVERAGE-LEDGER.jsonl`, `*.csv`
training datasets.

Rule: **Classify as dataset artifacts. Audit for provenance before any reuse.
Self-training loop warning applies.**

These files contain already-generated learning rows. Reusing them as DPO pair
sources without provenance audit creates a self-training loop: the model trained
on these rows produced some of them, and training on them again reinforces
the model's existing biases rather than providing new signal.

Before reusing any A07 file as a training source:

```
â–¡ Confirm provenance: where did each row originate?
  If rows came from a prior training run of this model â†’ exclude.
  If rows came from human-authored sources with no model involvement â†’ eligible.
  If rows are mixed provenance â†’ audit and exclude model-generated portions.
â–¡ Confirm no prompt appears as both chosen and rejected across different rows.
â–¡ Confirm overlap_ratio < 0.60 for all rows (may have degraded since creation).
```

Coverage ledger files (`COVERAGE-LEDGER.jsonl`) record which sub-phases have
been trained. Use as provenance metadata only â€” not as training content.

---

**Rendered HTML and browser captures (A09)**

Files: `Attached text prompt - Claude.html`, generated UI HTML files,
`index.html`, static site pages.

Rule: **Do not treat raw HTML as text source. Strip or render first, then
classify the resulting text.**

Raw HTML contains markup, scripts, and style blocks that produce garbage tokens
when fed to the compiler as text. The actual content (developer prompt, UI
layout, legal text) is buried inside tags.

Preprocessing required:

```
Strip tags â†’ extract visible text content
Classify by content type:
  Developer prompt captured in HTML â†’ Type 1 (if passes eligibility)
  UI component layout â†’ Type 24 (if it is a generated layout with corrections)
  Legal/privacy text â†’ U24 marketplace legal rules (excluded by default)
  Session capture â†’ Type 3 (if it is a conversation correction event)
```

`privacy.html` is always excluded (U24 rule). `index.html` requires content
inspection after stripping â€” it may be a landing page (excluded) or may contain
a session capture or developer prompt (classify after stripping).

---

**Visual binary assets and theme packages (A10)**

Files: `*.png`, `*.svg`, `*.gif`, `*.ttf`, `Themes/*.zip`.

Rule: **Keep out of text DPO. Use only for visual QA or asset provenance.**

Binary image and font files cannot be tokenized as text. SVG files that contain
meaningful structure (UI component specs, diagrams) can be processed as XML text
after inspection â€” but the resulting pairs belong to Type 24 (generated code
correction) or U15 zone A (visual QA evidence), not to a separate type.

Theme packages (ZIP archives containing visual assets) follow the U10 archive
rule: unpack, classify each file individually. Font files and image files within
the archive are excluded from DPO.

---

---

### Second addendum limited-authority families (A14, A16, A19, A20, A21, A22, A24)

---

**Runtime debug and tool logs (A14)**

Files: `debug.log`, `firebase-debug.log`, tool execution logs, build/test console logs.

These logs record what actually happened at runtime â€” build failures, test output,
server errors, package manager messages. They are measurement and failure evidence,
not architecture authority.

Valid DPO use â€” **failure evidence for BUG pairs only, after normalization.**

A log entry that shows a specific error is the A-side evidence for a BUG pair.
The error message describes what is wrong; the fix is the chosen. The wrong fix
(that addresses different symptoms) is the rejected.

Required normalization before any pair construction:
```
â–¡ Strip timestamps (they vary per run and add noise tokens)
â–¡ Strip absolute paths (replace with relative or placeholder)
â–¡ Strip environment-specific values (machine names, user directories)
â–¡ Keep: error message, stack trace excerpt, command that was run
```

Do not use log file text as A side directly. The A side must be a developer
prompt derived from the error: "firebase-debug.log shows PERMISSION_DENIED on
collection 'flows' â€” fix the security rules."

---

**Plugin extension manifest files (A16)**

Files: `manifest.json`, extension manifests, browser plugin descriptors, Figma plugin
manifests.

These define runtime capability boundaries for plugin extensions â€” which permissions
are requested, which entry points are registered, which UI surfaces are available.
Different from A02 (build manifests) because they define what the plugin can do at
runtime, not how it is built.

Valid DPO use â€” **limited to plugin capability boundary pairs.**

```
A: "does the Figma plugin have permission to access the network?"

Chosen (CODE_Q â€” from manifest):
  "Yes â€” manifest.json declares networkAccess: { allowedDomains: ['api.xiigen.com'] }.
   The plugin can only call api.xiigen.com, not arbitrary URLs. Any call to a
   domain outside this list will be blocked by Figma's runtime."

Rejected (CODE_Q â€” inferred without manifest):
  "Figma plugins can access the network freely â€” they run in a sandboxed
   JavaScript environment and can fetch any URL the user's browser can reach."
[WRONG: Figma plugins declare network access in the manifest. The manifest
controls what is allowed, not the browser's general capability. Without the
allowedDomains declaration, network access is blocked entirely.]
```

Do not use plugin manifests as general architecture authority for XIIGen orchestration.

---

**Generated UI code outputs (A19)**

Files: `*_generated.html`, generated React/TSX files, generated CSS,
Figma-to-code outputs.

These are artifacts produced by conversion experiments â€” they are not specifications.
Using generated code as chosen content in a pair teaches the model that generation
output is authoritative, which creates a circular training signal.

Valid DPO use â€” **Type 24 extension only (generated code correction).**

When a generated UI output was reviewed and corrected, the correction is a
Type 24 pair: generated code (wrong) â†’ corrected code (right). The generated
code is the rejected, not the chosen.

```
Chosen: the corrected code (human-reviewed and accepted)
Rejected: the generated code before correction
A side: the generation request prompt
```

Do not use uncorrected generated code as B-side chosen content. The model
learns from what is correct, not from what was generated.

---

**Before/after visual comparison sets (A20)**

Files: `dashboard.before.webp`, `dashboard.after.webp`, `form.before.webp`,
`form.after.webp`.

These are binary visual files â€” they cannot be tokenized as text and cannot
directly produce DPO pairs. Their value is as visual QA evidence for the
before/after direction of a UI improvement.

Valid DPO use â€” **metadata only, as evidence for Type 8 ArbiterUnit pairs.**

The before/after direction preserved in the filename metadata supports ArbiterUnit
pairs about whether a UI change was an improvement. The pair content comes from
the associated text description (Type 24 generated code history, U15 visual QA
findings), not from the image files themselves.

```
Do not tokenize binary image files.
Use only: filename metadata (before/after label), associated text description
if present (visual finding document, Type 24 correction history).
```

---

**Positive/negative example catalogs (A21)**

Files: `POSITIVE-NEGATIVE-EXAMPLES.md`, `CORRECTED-DEFECT-CATALOG`,
example catalogs, failure/success example lists.

**High-value DPO source.** Chosen and rejected are already authored in these
documents â€” no construction is needed. This is the fastest path to new pairs
outside of Type 3 correction events.

Required checks before use:

```
Provenance check â€” mandatory:
  â–¡ Are examples human-authored or synthetically generated?
    Human-authored â†’ include after quality check
    Synthetically generated by a model â†’ treat as A07 (self-training loop risk)
    Apply A07 provenance audit rules to model-generated examples

Label check:
  â–¡ Is the positive/negative labelling confirmed by the document header or
    a human reviewer, not inferred from position or formatting?
    Confirmed label â†’ eligible
    Ambiguous label â†’ exclude

Quality check (same as Type 3):
  â–¡ Chosen and rejected have clearly different vocabulary (High â€” include)
  â–¡ Different responsibilities, some shared vocabulary (Medium â€” include with annotation)
  â–¡ Same content, different phrasing (Low â€” exclude)
```

When examples pass all checks, apply the same pair construction rules as
the content type they represent: ARCH examples â†’ ARCH pair rules, PLAN examples
â†’ PLAN pair rules, etc.

**Expected overlap ratio:** Inherits from content type. Pre-authored examples
typically have overlap 0.10â€“0.30 (human authors naturally write distinct examples).
Synthetic examples may have higher overlap.

---

**Algorithm reference and math notes (A22)**

Files: `ml_algorithms_implementations_en.md`, `ML_SIMULATIONS.md`,
algorithm notebooks, math reference notes.

This material describes algorithms and mathematical concepts that may or may not
be used by XIIGen. The distinction matters: a paper about KNN retrieval is not
XIIGen architecture. A locked decision (Type 9) that says "use E34V2 KNN fusion"
is architecture â€” and it cites the algorithm as evidence.

Valid DPO use â€” **CODE_Q only, unless explicitly adopted by a Type 9 decision.**

```
Valid:
  CODE_Q pair about how KNN retrieval works (algorithm reference as B-side authority).
  The rejected is an inaccurate description of the algorithm.

Valid (with decision adoption):
  If a Type 9 decision cites algorithm X and says "adopt this for component Y",
  then algorithm X's specification becomes B-side authority for ARCH/PLAN pairs
  about component Y. Cite the decision, not just the algorithm reference.

Forbidden:
  Using algorithm reference material as ARCH authority without a corresponding
  Type 9 adoption decision. "This paper shows KNN works well" is not the same
  as "A47 adopts KNN fusion for AstRelevanceRanker."
```

---

**Template and boilerplate libraries (A24)**

Files: `templates/infra/*`, skill templates, component templates, boilerplate
project folders.

Templates define the shape of what should be generated â€” the placeholder structure
of a correct output. They are generation contract evidence: what a correctly
generated skill, flow, or component must look like.

Valid DPO use â€” **generation contract pairs only.**

```
A: "generate a new skill file for the tenant management domain"

Chosen (generation â€” follows template contract):
  skill:
    id: "tenant-management-skill"
    domain: "ARCH"
    trigger: "design the tenant management"
    sub_phases: ["3.1", "3.2"]
    output_contract: "TenantBoundaryDecision"
    [all required template fields populated correctly]

Rejected (generation â€” missing required template fields):
  skill:
    id: "tenant-management"
    domain: "ARCH"
    [trigger, sub_phases, output_contract absent]
[WRONG: the skill template requires trigger, sub_phases, and output_contract.
A skill file missing these fields cannot be registered â€” the skill registry
validator will reject it. The template is the contract, not a suggestion.]
```

**Critical distinction from Type 20 (skill library):** Type 20 uses skill
documents as B-side authority for domain-boundary pairs. Type 24 (template
library) uses template structure as B-side authority for generation-correctness
pairs â€” the content of the generated artifact must match the template contract.

---

### Second addendum exclusion rules (A17, A18, A23)

---

**Office, PDF, and exported document files (A17)**

Files: `*.docx`, `*.pdf`, `*.xlsx` (planning exports), exported specification packets.

Rule: **Render and extract first. Deduplicate against markdown. Classify by content.**

Binary office files may duplicate or supersede markdown sources. Processing them
as text without extraction produces garbage tokens from binary formatting.

Required preprocessing:
```
1. Render/extract text content (docx â†’ text, pdf â†’ text, xlsx â†’ see A08 rules)
2. Deduplicate: check whether a markdown version of the same document exists.
   If yes: use the markdown version as canonical source. The office export is
   an archive_container mirror.
3. If no markdown version exists: classify extracted content by type (Types 1â€“26)
   and apply corresponding pair construction rules.
4. Version check: office exports are often stale. Apply three-check rule.
```

Do not process binary office files without extraction. Do not assume that an
exported PDF or DOCX is more current than a markdown source of the same content.

---

**Vendored third-party codebases (A18)**

Directories: `claude-useful-skills-master/`, `vibecoder-master/`,
`get-shit-done-main/`, `uiux-lib/*`.

Rule: **External boundary required. Never mix as internal XIIGen authority
without an explicit adoption record.**

These are extracted third-party repositories embedded in the workspace. Their
content reflects the third party's decisions and design, not XIIGen's decisions.
Using them as B-side authority for XIIGen ARCH or PLAN pairs is the same error
as U22 (external training sets mixing into internal learning) â€” it introduces
external authority as if it were internal ground truth.

Valid use â€” **only after explicit adoption:**

If a Type 9 locked decision adopts a pattern from a vendored codebase
("use the vibecoder skill template structure as the standard for XIIGen skill files"),
then that specific adopted pattern becomes B-side authority â€” citing the decision,
not the vendored source directly.

Without an adoption decision:
```
Vendored codebase content â†’ external canonicality
Cannot be B-side authority for any ARCH, PLAN, or IMPLEMENT pair
Can be: CODE_Q reference about what the third-party code does (Type 21 rules apply)
```

---

**Localized user-facing guides (A23)**

This family is covered by the existing A11 rule (localization and language
variants). Apply A11 rules: attach locale and canonical-source pointer; A-side
prompts in original language are valid Type 1; B-side must use canonical source.

No additional handling required. A23 and A11 are the same family.

---

---

### Third round limited-authority families (B02-style: B08, B10, B11, B12)

---

**Serialized implementation delivery parts (B02-family)**

Files: `BFA_PY_PART1..7.md`, `ENG_PY_PART1..7.md`, `MEP_PY_PART1..7.md`,
`SKF_PY_PART1..7.md`, `SS_PY_PART1..7.md`, `TTC_PY_PART1..7.md`,
`USI_PY_PART1..7.md`.

These are large implementation deliveries split across 7 parts due to size.
Each part is a segment of a complete implementation specification or code
delivery â€” not an independent document.

Rule: **Treat as delivery artifact segments. Apply the same rules as the
corresponding implementation documentation type.**

Before using any part:
```
â–¡ Identify which implementation the parts belong to (BFA = Business Flow
  Architecture, ENG = Engine, MEP = Master Execution Plan, etc.)
â–¡ Check whether the complete version exists in a merged or combined file
  (e.g., ENGINE_ARCHITECTURE_MERGED.md supersedes ENG_PY_PART1..7.md)
â–¡ If merged version exists: use merged version as canonical, parts are mirrors
â–¡ If no merged version: treat parts as archive_container â€” extract content,
  classify by zone (implementation â†’ Type 2/26, plan â†’ Type 29, state â†’ Type 22)
```

Never use a part file as an A side. Never assume Part 3 is independent of Part 2.

---

**Prompt pattern libraries and benchmark guides (B08)**

Files: `PROMPT_PATTERNS_COMPLEX_TASKS.md`, `RAG-BENCHMARK-TO-XIIGEN-FLOWS-PLANNING-SKILL.md`,
`basic_prompt.txt`.

These documents catalogue reusable prompt patterns or describe how to benchmark
against specific flows.

Valid DPO use â€” **Type 20 extension (skill domain boundaries) and TYPE 21 extension
(CODE_Q reference) only.**

Prompt pattern documents define what a well-formed prompt for a given domain looks
like. This is Type 20 material: the pattern defines the skill trigger condition.
Rejected = a prompt that violates the pattern structure.

Benchmark guides (RAG-BENCHMARK-TO-XIIGEN-FLOWS) define expected behavior for
specific scenarios. This is Type 28 extension: the benchmark expectation is the
assertion, same logic as a test assertion.

Do not use as general architecture authority.

---

**DNA pattern and graph extension documents (B10)**

Files: `DNA_PATTERNS_TYPESCRIPT.md`, `GRAPHRAG_NODES_ATTRIBUTION.md`,
`FEEDBACK_GRAPHRAG_EXTENSION.md`.

DNA patterns define the structural conventions for how XIIGen components should
be composed â€” the design grammar of the system.

Valid DPO use â€” **Type 18 extension (invariant protocol) for DNA compliance pairs.**

DNA patterns define rules that every component must follow â€” same logic as
portability protocols but at the design-grammar level rather than the
tenant-isolation level. A component that violates a DNA pattern is a Type 18
style rejected: the violation can be derived mechanically from the pattern rule.

Graph extension documents (GraphRAG nodes, attribution) are B-side authority
for pairs about graph topology and node attribution â€” Type 27 extension.

---

**Onboarding, summary, and README documents (B11)**

Files: `README.md`, `executive_summary.md`, `gemini.md`, `multi-tenant-support.md`,
`deep-research-report chat gpt.md`.

Rule: **A-side prompts only. Never B-side authority.**

READMEs and summaries describe the system at a high level. They are often
imprecise, outdated, or incomplete compared to the detailed specification documents.
Using them as B-side chosen content for ARCH or PLAN pairs trains the model on
high-level approximations.

The only valid DPO use: a developer reading a README and asking a clarifying question
provides a real Type 1 A-side prompt. The B-side must use Type 2/5/26 as authority,
not the README itself.

`gemini.md` and similar comparison/evaluation docs: apply Type 33 rules if they
compare approaches, or Type 21 rules if they cite external sources.

---

**Miscellaneous state JSON files (B12)**

Files: `STATE 03.04.2026.json`, `STATE-marketplace-research.json`,
`diet-coaching-flow.json`, `skills-factory.json`.

These are state files not covered by Type 22 (execution state) or Type 4
(calibration metrics) because they record domain-specific application state
rather than workflow execution or model training progress.

Apply classification by content:
```
Contains workflow execution phases â†’ Type 22 rules
Contains DPO training metrics â†’ Type 4 rules
Contains marketplace/business research state â†’ Type 21 rules
  (external limited-authority with date/source required)
Contains flow definitions â†’ apply flow spec zone mapping (Type 2 extension)
```

`diet-coaching-flow.json` and similar non-XIIGen domain flow files: these are
example flows from other domains. Useful as A-side prompts ("how would this
flow route to sub-phases") â€” not as XIIGen architecture authority.

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
batch â€” this creates contradictory training signal with near-identical token
distributions.

**Certificates and environment material (U18)**

Locations: `certificates/*.crt`, `certificates/*.csr`, `environment.zip`,
`env*.zip`, `.env` files.

Rule: Excluded from all training without exception. These are
`secure_operational` canonicality â€” no pair construction is permitted
regardless of content. Do not log these paths in pair manifests.

**Code repository snapshots with embedded docs (U19)**

Examples: `xiigen_v18_complete.zip`, `llm_mvp_core-main.zip`,
`orchestrator-mvp-main.zip`.

Rule: Repository snapshots are `archive_container` canonicality. The snapshot
itself is not a training source. Extract, then apply:

```
Source code files (.cs, .ts, .py)  â†’ potential Type 5 (topology) or vocabulary
                                      calibration only; not direct pair source
Markdown docs embedded in repo    â†’ classify by content type (Types 1â€“24)
Test files                         â†’ Type 7/8 extension if they record
                                      pass/fail findings
Package manifests                  â†’ provenance metadata only
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

### Type 25 â€” Configuration schema contracts

**What it is:** Machine-readable JSON schema files that define validation rules,
field constraints, and allowed values for product configuration and runtime behavior.

**Examples in the XIIGen corpus:**
```
genie-dna/config-schemas/*.schema.json
availability_rules.schema.json
cart_rules.schema.json
```

**Why this is a distinct type:** Configuration schema files are executable contracts.
A value that fails schema validation is objectively wrong â€” this is not a matter of
architectural interpretation. This makes them the strongest available source for
ArbiterUnit pairs where the verdict criterion is field-level conformance, and for
CODE_Q pairs where the question is about what values a field accepts.

This is different from Type 5 (topology seeder code): Type 5 defines sub-phase
registration and predicate vocabulary. Type 25 defines the valid shape of a
runtime configuration object â€” a different layer of the contract stack.

**How to use for ArbiterUnit pairs:**

```
Contract: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>

A (ArbiterReviewRequest):
  artifact_type: "config_object"
  artifact_content:
    { "availabilityRule": "always",
      "maxCartItems": -1,
      "checkoutBehavior": "redirect_external" }
  review_criteria: "validate against availability_rules.schema.json"
  source_rule: "availability_rules.schema.json"

Chosen (ArbiterUnit â€” reprocess: schema violation):
  verdict: reprocess
  finding: "maxCartItems must be >= 0 per schema definition â€” negative values
            are not in the enum. checkoutBehavior 'redirect_external' is not
            in the allowed enum: ['inline', 'modal', 'redirect_internal'].
            Two schema violations; object is not valid."
  requirement: "correct both fields before this config can be accepted"

Rejected (ArbiterUnit â€” concern: wrong severity):
  verdict: concern
  finding: "maxCartItems is -1 which seems low. checkoutBehavior value
            may not match expected options."
[WRONG: schema validation is binary â€” a value that violates the schema enum
is invalid, not a concern. "Seems low" is opinion, not a schema finding.
When a field value is not in the schema's allowed enum, the correct verdict
is reprocess, not concern. A concern verdict allows the config to be used
as-is; reprocess blocks it until the violation is corrected.]
```

**How to use for CODE_Q pairs:**

```
A: "what values does checkoutBehavior accept in the availability rules config?"

Chosen (CODE_Q â€” schema-grounded):
  "checkoutBehavior accepts exactly three values per availability_rules.schema.json:
   'inline' (renders checkout inside the current page), 'modal' (shows a dialog),
   and 'redirect_internal' (navigates within the app). External redirect is not
   supported. Any other value fails schema validation."

Rejected (CODE_Q â€” inferred without schema):
  "checkoutBehavior accepts 'inline', 'popup', 'modal', or 'redirect' based on
   how the checkout needs to behave in the UI."
[WRONG: 'popup' and 'redirect' are not in the schema enum. Inferring allowed
values from behavior vocabulary without checking the schema produces wrong
answers that will cause config validation failures at runtime.]
```

**Expected overlap ratio:** 0.10â€“0.20 for schema-violation ArbiterUnit pairs
(schema field vocabulary vs implementation vocabulary). 0.15â€“0.30 for CODE_Q
enum-value pairs (exact enum values vs inferred vocabulary).

**Pair types extractable:** ArbiterUnit schema-conformance pairs, CODE_Q
field-value accuracy pairs, rejected construction for wrong enum values.

**Excluded use:**
```
Do not use schema file text as A side.
Do not derive architecture decisions from config schemas â€” schemas define
valid values, not why those values were chosen. Type 9 (decisions) is the
authority for the rationale.
Do not invent schema enum values not present in the actual schema file.
```

---

### Type 26 â€” Source code contract files

**What it is:** Live interface definitions, DTO classes, enum declarations, and
runtime contract types from source code outside the topology seeder. These are
the ground truth for exact field names, method signatures, and type contracts.

**Examples in the XIIGen corpus:**
```
context/interfaces.cs
00.Core/*.cs (interface and DTO definitions)
AdaptiveLogicalOrchestrator.cs (ILogicalOrchestrator contract)
SemanticCompiler*.cs (compilation contract types)
```

**Why this is a distinct type:** Type 5 (topology seeder) defines sub-phase
registration. Type 26 defines the runtime contracts that flowing data must
satisfy â€” the interfaces and DTOs that every unit works against.

Type 2 (prose architecture documents) often describes these contracts in natural
language, which may be incomplete or imprecise. When prose says "the compiler
returns a `SubPhaseRouterSourceDocument`", Type 26 tells you exactly what fields
that type contains, what their types are, and what the interface contract requires.
For pairs where the correct answer depends on an exact type name, field type, or
interface method signature, Type 26 is the authoritative source.

**How to use for CODE_Q pairs â€” exact interface/DTO fields:**

```
A: "what fields does SubPhaseRouterSourceDocument contain?"

Chosen (CODE_Q â€” from source contract):
  "SubPhaseRouterSourceDocument contains: CompiledText (string, the normalized
   prompt with predicate tokens appended), CompiledPredicateIds (int[], the matched
   predicate IDs from the nine-predicate lexicon), Confidence (double, 0.70 when
   no predicates matched, 0.92 when at least one matched), EvidenceTerms (string[],
   terms that activated each predicate), and KnowledgeEdges (string[], vocabulary
   present in the prompt that is not in any predicate's lexicon)."

Rejected (CODE_Q â€” inferred from prose):
  "SubPhaseRouterSourceDocument contains the compiled prompt, the matched predicates,
   and a confidence score. It may also include evidence terms and knowledge gaps."
[WRONG: "may also include" describes uncertainty about a field that exists and is
always populated. EvidenceTerms and KnowledgeEdges are always present â€” they are
never optional. Using "may" for required fields trains the model to hedge on facts
that are defined in the type contract.]
```

**How to use for ARCH pairs â€” interface boundary pairs:**

When a developer asks about component ownership, the source code interface
definition is the authoritative boundary. A class that implements
`ILogicalUnit<TInput, TOutput>` owns exactly the logic between TInput and TOutput.

```
A: "who owns the routing decision â€” the compiler or the router?"

Chosen (ARCH â€” from interface contracts):
  "The compiler (PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest,
   SubPhaseRouterSourceDocument>) owns predicate extraction and prompt normalization.
   The router (MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument,
   SubPhaseRoutingDecision[]>) owns which sub-phases to activate based on the compiled
   predicates. The compiler produces the input; the router decides what to do with it.
   Ownership is defined by the interface boundary: each unit owns its output type."

Rejected (ARCH â€” boundary confused):
  "The compiler handles routing â€” it compiles the prompt and decides which
   sub-phases are relevant. The router just formats the output."
[WRONG: the interface contracts show the split. The compiler's output is
SubPhaseRouterSourceDocument â€” it produces structured input for the router.
The router's output is SubPhaseRoutingDecision[] â€” it makes the routing decisions.
Calling the compiler's work "routing" misidentifies which contract owns the decision.]
```

**Expected overlap ratio:** 0.10â€“0.20 for interface boundary pairs. 0.15â€“0.25 for
DTO field accuracy pairs. Source code vocabulary (interface names, type names, method
signatures) is largely non-overlapping with inferred prose vocabulary.

**Pair types extractable:** CODE_Q field-accuracy pairs, ARCH interface-boundary
pairs, ArbiterUnit contract-conformance pairs.

**Excluded use:**
```
Do not use source code implementation bodies as pair content â€” only interface
definitions, DTO declarations, and enum types. Implementation bodies belong
to Type 3/24 correction events when they contain defects.
Do not use .cs file text verbatim as A side.
Three-check rule applies: verify this is the current version of the contract
via the document state register before using as chosen content.
```

---

---

### Type 27 â€” Diagram and topology source files

**What it is:** Machine-readable diagram files that encode component relationships,
dependency graphs, and system topology as structured graph data rather than prose.

**Examples in the XIIGen corpus:**
```
*.mmd / *.mermaid  (Mermaid diagrams)
*.drawio           (Draw.io topology graphs)
*.puml             (PlantUML sequence and class diagrams)
DEPENDENCY-GRAPH.mmd
```

**Why this is a distinct type:** Prose architecture documents describe relationships
in natural language, which can be ambiguous. A Mermaid diagram that says
`A --> B` is an unambiguous directed edge â€” A depends on B, and not the reverse.
Diagram files provide machine-verifiable topology evidence that prose cannot replicate
without losing precision.

For pairs about component dependency direction, the diagram node-edge structure is
the chosen content. A response that reverses the edge direction or omits an edge
is the rejected.

**Critical rule: parse nodes and edges. Do not flatten to prose first.**

Converting a diagram to prose before extracting pair content loses the edge
directionality and introduces ambiguity. The pair must be constructed from the
parsed graph structure, not from a human summary of the diagram.

**How to use for ARCH dependency-direction pairs:**

```
Source: DEPENDENCY-GRAPH.mmd contains edge:
  PromptRoutingInputCompilerUnit --> MultiSubPhaseRouterUnit
  MultiSubPhaseRouterUnit --> AdaptiveLogicalOrchestrator
  AdaptiveLogicalOrchestrator --> DomainSplitterUnit

A: "which direction does the dependency flow between the compiler and the router?"

Chosen (ARCH â€” from parsed edge):
  "The compiler (PromptRoutingInputCompilerUnit) produces output that the router
   (MultiSubPhaseRouterUnit) consumes. The dependency flows from compiler to router:
   the router depends on the compiler's output, not the reverse. The compiler has
   no knowledge of the router."

Rejected (ARCH â€” reversed edge):
  "The router depends on the compiler and the compiler depends on the router â€”
   they have a bidirectional relationship because the compiler needs to know
   what sub-phases the router can handle."
[WRONG: the dependency graph shows a directed edge compiler --> router. There is
no return edge. The compiler produces SubPhaseRouterSourceDocument without any
reference to the router type. A bidirectional dependency would create a cycle â€”
the diagram has none.]
```

**How to use for PLAN ordering pairs:**

A directed graph with an explicit dependency chain is a PLAN Type B source.
The topological sort of the graph is the correct execution order. The rejected
reverses or removes an edge.

```
Chosen: A before B before C (topological sort of Aâ†’Bâ†’C chain)
Rejected: B before A (reversed edge) or A and C in parallel (missing edge)
```

**Expected overlap ratio:** 0.10â€“0.20. Graph vocabulary (edge, node, depends-on,
direction, cycle) is largely non-overlapping with wrong-direction vocabulary.

**Pair types extractable:** ARCH dependency-direction pairs, PLAN topological
ordering pairs, ArbiterUnit cycle-detection pairs.

**Excluded use:**
```
Do not use raw diagram file text (Mermaid syntax, XML) as A side.
Do not flatten diagram to prose before extracting pair content â€” parse edges directly.
Three-check rule applies: verify diagram is current version before using as authority.
```

---

### Type 28 â€” Test suite source files

**What it is:** Executable test files that encode expected behavior as assertions.
The test body is the specification â€” it defines exactly what output a unit must
produce for a given input.

**Examples in the XIIGen corpus:**
```
*.test.ts
*.spec.ts
*Tests.cs
mock-figma.ts (mock contracts)
test-settings.ts
```

**Why this is a distinct type:** Plan documents describe intended behavior in
natural language. Test files assert expected behavior in executable form. When
a plan document says "the compiler should return confidence 0.92 when predicates
are matched", the test file says `Assert.Equal(0.92, result.Confidence)`. The
test assertion is more precise and more reliable as chosen content.

A test file that is currently passing is the ground truth for what the system
actually does. A response that contradicts a passing assertion is objectively
wrong for the current codebase.

**Separation rule: test intent vs mocking scaffolding.**

Test files contain two structurally different things:
- Test intent: assertions, expected values, test case descriptions
- Mocking scaffolding: mock setups, test helpers, fixture data

Only test intent is pair material. Mocking scaffolding is implementation detail
and must not become pair content.

**How to use for CODE_Q pairs â€” expected behavior:**

```
A: "what confidence value does PromptRoutingInputCompilerUnit return
    when no predicates are matched?"

Chosen (CODE_Q â€” from test assertion):
  "0.70 â€” the unit returns confidence 0.70 when CompiledPredicateIds is empty.
   This is confirmed in PromptRoutingInputCompilerUnitTests.cs:
   Assert.Equal(0.70, result.Confidence) in the no-predicate test case.
   The unit also sets a KnowledgeEdge warning in this case."

Rejected (CODE_Q â€” inferred without test):
  "Around 0.5 â€” when no predicates match, the compiler has low confidence
   and returns approximately 0.5 to indicate uncertainty."
[WRONG: the test asserts 0.70, not 0.5. Inferring the value from general
reasoning about uncertainty produces a wrong number. The test is the
specification for this exact behavior.]
```

**How to use for BUG pairs â€” test failure as A side:**

A failing test is a BUG A side. The test name and assertion describe exactly
what is broken. The chosen is the fix that makes the test pass.

```
A: "PromptRoutingInputCompilerUnitTests.ReturnsKnowledgeEdgeWhenNoPredicate
    is failing â€” fix the unit"

Chosen (BUG â€” fix that satisfies the assertion):
  "The unit is not populating KnowledgeEdges when CompiledPredicateIds is empty.
   Fix: add the no-predicate-warning population before returning.
   The test asserts KnowledgeEdges.Count > 0 in this case."

Rejected (BUG â€” fix that doesn't satisfy the assertion):
  "Set confidence to 0.5 when no predicates match â€” this signals uncertainty
   without needing to populate KnowledgeEdges."
[WRONG: the failing assertion is KnowledgeEdges.Count > 0, not Confidence value.
Changing confidence does not make the test pass. The fix must populate
KnowledgeEdges in the no-predicate case.]
```

**Expected overlap ratio:** 0.15â€“0.30 for CODE_Q pairs. 0.10â€“0.20 for BUG fix
pairs (fix vocabulary vs wrong-fix vocabulary). Test assertion vocabulary is
precise and largely non-overlapping with inferred prose vocabulary.

**Pair types extractable:** CODE_Q expected-behavior pairs, BUG test-failure pairs,
ArbiterUnit test-compliance verdicts.

**Excluded use:**
```
Do not use mock setup code or test helper methods as pair content.
Do not use test file text verbatim as A side.
Do not use test files from an earlier version where the behavior has changed â€”
apply the three-check rule to confirm the test reflects current expected behavior.
Only use passing tests as chosen authority. Skipped or ignored tests are not
acceptable behavior specifications.
```

---

---

### Type 29 â€” Phase implementation plans and phase state documents

**What it is:** Plans for specific development phases with sub-step ordering,
delivery checkpoints, and per-step state files. Distinct from Type 17 (migration
plans) because the dependency ordering is within a development phase, not between
technology stacks.

**Examples in the XIIGen corpus:**
```
PHASE_1_PLAN.md through PHASE_13_PLAN.md
PHASE5_PLAN.md, PHASE6_PLAN.md, PHASE6_EXECUTION_PLAN.md
PHASE7_PLAN_REACT_NATIVE.md
PHASE8_PLAN_ANCHOR_DOCS.md
PHASE1_STATE.json through PHASE_STATE_FINAL.json
PHASE8A_DELIVERY_DOC.md (delivery artifact for a specific phase step)
```

**How to use for PLAN Type B pairs:**

Phase plans contain explicit step sequences with prerequisites. Every step that
references another step as a prerequisite is PLAN Type B material.

```
A: "plan the implementation of phase 5 â€” factories and registry"

Chosen (PLAN â€” correct phase order from PHASE5_PLAN.md):
  Step 1 â€” Factory core (PHASE5A): base factory contracts and interfaces.
    No dependencies within phase â€” starts immediately.
  Step 2 â€” Factory registry (PHASE5B): registry that maps factory IDs to instances.
    Dependency: PHASE5A must be complete â€” registry references the contracts.
  Step 3 â€” Factory catalog (PHASE5C): catalog of all registered factories.
    Dependency: PHASE5B must be complete â€” catalog reads from registry.
  Step 4 â€” Task types (PHASE5D): task type definitions for each factory.
    Dependency: PHASE5C must be complete â€” task types reference catalog entries.
  Step 5 â€” Backward compatibility (PHASE5E): adapter layer for prior API.
    Dependency: PHASE5D must be complete â€” adapters wrap current task types.

Rejected (PLAN â€” wrong order, all parallel):
  All five steps run in parallel â€” each team picks a step and starts immediately.
[WRONG: PHASE5B references the contracts from PHASE5A; running them in parallel
means PHASE5B has no contracts to reference. Each step in the phase plan has an
explicit dependency on the prior step's output artifacts.]
```

**How to use STATE.json files from this family:**

Phase state files record which sub-steps completed and which artifacts were produced.
Apply Type 22 rules: completed phases â†’ chosen state record, interrupted phases â†’
22b correction events.

**Expected overlap ratio:** 0.30â€“0.45 for within-phase ordering pairs.

**Pair types extractable:** PLAN Type B (step ordering within phase), Type 22
correction events (interrupted phase steps), ArbiterUnit phase-completion pairs.

---

### Type 30 â€” Skills factory registry documents

**What it is:** Catalogue-level documents that list all skills, their IDs, trigger
conditions, sub-phase coverage, and factory mappings. These are the registry of
what skills exist and what each skill handles.

**Examples in the XIIGen corpus:**
```
SKILLS_FACTORY.md
SKILLS_FACTORY_RAG.md
SKILLS_FACTORY_RAG_CORE_SK1_SK16.md
SKILLS_FACTORY_RAG_AI_RAG_SK81_SK160.md
SKILLS_FACTORY_RAG_SK161_SK401_COMPLETE.md
AF_SKILL_LIBRARY.md
```

**Why this is distinct from Type 20 (individual skill definitions):** Type 20 defines
what one skill does. Type 30 is the complete registry of all skills and their
relationships to each other, to sub-phases, and to the factory system. Type 30 is
the authority for what is registered vs what is missing.

**How to use for knowledge-edge detection pairs:**

The skills factory registry is the canonical source for which skills are registered
and which sub-phases they cover. A sub-phase with no registered skill = KNOWLEDGE_EDGE.

```
A: "does the skills factory have a skill for sub-phase 5.3 End-to-End Testing?"

Chosen (CODE_Q â€” registry-grounded):
  "SKILLS_FACTORY_RAG shows no skill registered for sub-phase 5.3.
   The registry covers 5.1 (unit test design, SK-47) and 5.2 (integration
   tests, SK-48) but 5.3 has no entry. Any routing to 5.3 will produce
   a KNOWLEDGE_EDGE result until a skill is registered."

Rejected (CODE_Q â€” assumes coverage):
  "Yes â€” the factory includes skills for all testing sub-phases including
   end-to-end testing. Check the test skills section."
[WRONG: the registry shows no 5.3 entry. Assuming coverage without checking
the registry produces a false PASS result for sub-phases that have no flow.
The registry is the ground truth for what is registered.]
```

**How to use for routing pairs:**

Skill factory maps show which factory handles which sub-phase. A mismatch
between the factory assignment and the sub-phase is a Type A rejected.

**Expected overlap ratio:** 0.10â€“0.25.

**Pair types extractable:** CODE_Q coverage-accuracy pairs, knowledge-edge
detection pairs (sub-phase not in registry), routing factory-assignment pairs.

---

### Type 31 â€” Naming and mapping translation tables

**What it is:** Documents that define the authoritative mapping between old and
new naming systems, family numbering, or vocabulary. These are renaming events
recorded as lookup tables.

**Examples in the XIIGen corpus:**
```
ROSETTA_STONE.md      (mapping between old and new system names)
FAMILY_RENUMBERING_MAP.md  (old family numbers â†’ new family numbers)
TRANSLATION_PHASE_PLAN.md  (phase name translations across system versions)
```

**Why this is a distinct type:** Type 16 (versioned document evolution) handles
one document's vocabulary changing across versions. Type 31 handles a system-wide
renaming where many documents must all be updated simultaneously. The mapping table
is the authority for the correct current name â€” the old name is the pre-made rejected.

**How to use for vocabulary translation pairs:**

```
A: "what is the current name for what used to be called Family 23?"

Chosen (CODE_Q â€” from FAMILY_RENUMBERING_MAP.md):
  "Family 23 was renumbered to Family 27 in the renumbering event documented
   in FAMILY_RENUMBERING_MAP.md. All references to Family 23 in pre-renaming
   documents should be read as Family 27 in current documents."

Rejected (CODE_Q â€” old name used as current):
  "Family 23 handles tenant isolation flows â€” it contains the tenant
   separation logic and multi-tenant routing."
[WRONG: Family 23 no longer exists under that number. Using the old number
produces references that do not resolve in the current registry. The map
records the authoritative current number.]
```

**How to use for Type C rejected (wrong naming system):**

When a prompt uses old vocabulary that has been replaced, the correct response
uses the current name and cites the mapping table. A response that uses the old
name is Type C rejected.

**Expected overlap ratio:** 0.05â€“0.15. Old names and new names are typically
entirely non-overlapping tokens. Lowest overlap after Type 16.

**Pair types extractable:** CODE_Q vocabulary translation pairs, Type 16
extension (old name as rejected), ArbiterUnit name-compliance pairs.

---

### Type 32 â€” Validation reports and certificates

**What it is:** Documents that record a formal gate verdict on a delivery phase,
flow, or architecture milestone â€” passed, failed, conditional, or certified.

**Examples in the XIIGen corpus:**
```
VALIDATION_REPORT.md
PHASE_19_CERTIFICATE.md
MIGRATION_CERTIFICATE.md
ENGINE_ARCHITECTURE_DR101_DR266_CERT.md
ENGINE_ARCHITECTURE_F1_F500_DR1_DR100.md (requirement-level certification)
FLOW30_VALIDATION_REPORT.md, FLOW31_VALIDATION_REPORT.md
```

**Why this is a distinct type:** Type 8 (artifact examination) records the
examination process â€” how to look at an artifact and what verdict to produce.
Type 32 records the verdict that was actually produced on a real delivery.
Type 8 teaches how to arrive at a verdict; Type 32 is evidence that a real
verdict was correct.

**How to use for ArbiterUnit pairs:**

A real validation report is the ground truth for what a correct verdict looks
like for a real artifact. The rejected is a verdict that would have let the
artifact pass when it should not, or blocked it when it should pass.

```
A (ArbiterReviewRequest):
  artifact_type: "flow_delivery"
  artifact_content: "FLOW30 delivery package â€” Phase 3 complete"
  review_criteria: "validate against FLOW30 gate requirements"
  source_rule: "FLOW30_VALIDATION_REPORT.md"

Chosen (ArbiterUnit â€” from real validation report):
  verdict: pass
  finding: "All 12 gate requirements satisfied. Phase 3 delivery confirmed:
            factory contracts present (F301â€“F320), BFA stress test passed
            (all 28 scenarios), session state updated, manifest complete."

Rejected (ArbiterUnit â€” incomplete check, fabricated concern):
  verdict: concern
  finding: "Phase 3 appears complete but BFA stress test results were
            not explicitly confirmed. Suggest re-running before accepting."
[WRONG: the validation report confirms BFA stress test passed (all 28 scenarios).
A concern about an item that the report explicitly confirms is fabricating a gap.
When the report says passed, the arbiter verdict is pass â€” not concern based
on a hypothetical that the report already addressed.]
```

**Expected overlap ratio:** 0.15â€“0.25. Validation vocabulary (passed, confirmed,
satisfied, certified) vs concern vocabulary (appears, suggest, not explicitly).

**Pair types extractable:** ArbiterUnit verdict pairs (report as ground truth),
CODE_Q delivery-status pairs, Type 7 extension (certification status).

---

### Type 33 â€” Conflict and comparison analysis documents

**What it is:** Documents that compare two or more approaches, plans, or
implementations and record the analysis of which is better and why.

**Examples in the XIIGen corpus:**
```
CONFLICT_ANALYSIS.md
DRIFT_ANALYSIS.md
APPROACH_COMPARISON_IMPROVED.md
PLAN_COMPARISON.md
COMPARISON_V4_vs_V5.md
COMPARISON_V6_VS_MINE.md
```

**Why this is a distinct type:** Type 9 (decisions) records which alternative was
chosen. Type 33 records the comparison analysis that informed the choice â€” the
detailed evaluation of why one approach is better. Type 33 is the evidence behind
the decision; Type 9 is the decision itself.

The unchosen alternative in a comparison document is a pre-made rejected. The
analysis of why it was not chosen is a pre-made annotation.

**How to use for ARCH trade-off pairs:**

```
A: "should the orchestrator use a registry-based factory or constructor injection
    for resolving logical units?"

Chosen (ARCH â€” winning approach from APPROACH_COMPARISON_IMPROVED.md):
  "Registry-based factory: the orchestrator calls InMemoryLogicalUnitRegistry
   .ResolveLatest(role, domain) at runtime. This supports hot-swap of learned
   units without restart, trust-state filtering (Probationary/Trusted/Rejected),
   and per-domain unit versioning. The registry is the single authority for
   what unit handles what role."

Rejected (ARCH â€” unchosen approach):
  "Constructor injection: inject ILogicalUnit<TInput, TOutput> directly in the
   orchestrator constructor. This is simpler and avoids registry lookup overhead.
   The framework resolves the correct unit at startup."
[WRONG: constructor injection binds the unit at startup â€” learned units cannot
be promoted to Trusted or demoted to Rejected at runtime. APPROACH_COMPARISON
records this as the reason injection was rejected: runtime trust-state changes
are impossible with static injection. The registry pattern exists specifically
to support this dynamic behavior.]
```

**How to use for DRIFT analysis pairs:**

DRIFT_ANALYSIS.md records where the current implementation has diverged from
the intended design. Each drift item is a gap between intended (chosen) and
actual (rejected):

```
A: "does the current routing implementation use compiled predicates or raw text?"

Chosen (CODE_Q â€” intended design from drift analysis):
  "The intended design uses a 60-slot feature vector compiled from normalized
   predicates. PromptRoutingInputCompilerUnit should output CompiledPredicateIds
   that feed the router."

Rejected (CODE_Q â€” current drifted state):
  "The current implementation uses raw keyword scanning on the original prompt
   string. PromptRoutingInputCompilerUnit bypasses the semantic compiler."
[WRONG as a claim about how it should work: the drift analysis documents this
as a known gap (A59 gap â€” raw text path not updated). The answer about intended
design and current state are both true but answer different questions. The
prompt asks about current state â€” the rejection is the incorrect claim that
raw scanning is the design intent.]
```

**Expected overlap ratio:** 0.10â€“0.25. Chosen-approach vocabulary vs unchosen
alternative vocabulary are moderately non-overlapping.

**Pair types extractable:** ARCH trade-off pairs, CODE_Q intended-vs-actual pairs,
Type 7 extension (drift = gap between intended and implemented).

---

### Type 34 â€” Definition of done and prerequisites documents

**What it is:** Documents that define the exact conditions that must be true
before a phase, feature, or delivery is considered complete. These include
formal DoD criteria, tier prerequisites, and completion gate definitions.

**Examples in the XIIGen corpus:**
```
XIIGEN_DEFINITION_OF_DONE.md
TIER1-PREREQUISITES-PLAN.md
```

**Why this is a distinct type:** Type 10 (teaching plans) contains stopping rules
for teaching cycles. Type 34 contains completion criteria for product delivery â€”
what the system must do, not what the teaching process must do. Type 18 (portability
protocols) contains invariants that always apply. Type 34 contains criteria that
apply at a specific completion gate.

**How to use for ArbiterUnit completion pairs:**

```
A (ArbiterReviewRequest):
  artifact_type: "phase_completion_claim"
  artifact_content: "Phase 3 is complete â€” all factories implemented"
  review_criteria: "verify against XIIGEN_DEFINITION_OF_DONE.md Phase 3 criteria"

Chosen (ArbiterUnit â€” reprocess: incomplete):
  verdict: reprocess
  finding: "DoD Phase 3 requires: (1) factory contracts, (2) unit tests for
            all factories, (3) integration test for factory registry, (4) BFA
            stress test. Items 2 and 3 are not present in the delivery.
            'All factories implemented' does not satisfy the DoD â€” the DoD
            requires tests, not only implementation."
  requirement: "add unit tests and integration test before Phase 3 can close"

Rejected (ArbiterUnit â€” pass: ignores DoD):
  verdict: pass
  finding: "Factories are implemented as stated. Phase 3 is complete."
[WRONG: the DoD requires tests, not only implementation. A completion claim
that does not satisfy all DoD criteria cannot produce a pass verdict. The
arbiter's role is to check the DoD, not to accept self-reported completion.]
```

**Expected overlap ratio:** 0.10â€“0.20. DoD vocabulary (requires, criteria,
must be present, cannot close) vs implementation-only vocabulary (implemented,
as stated, complete).

**Pair types extractable:** ArbiterUnit completion-gate pairs, PLAN
prerequisite-ordering pairs (DoD items as dependencies).

---

## Rejected construction rules by domainThe calibration run (2026-05-06) confirmed that 75% of batches had negative reward
margin because rejected was either a negation template or another correct sub-phase
response for the same prompt. Neither produces positive margin.

This section defines how to construct rejected for each domain using the document type
taxonomy above.

---

### BUG domain â€” rejected construction

BUG domain mean accuracy in the calibration run: 0.417. Root cause: multiple sub-phases
received the same prompt as chosen. The model saw "fix the rxjs pipe" routed to both
2.1 and 4.3 within the same training set and could not learn the distinction.

**Rule:** For any BUG A side, rejected must address a different responsibility class,
not a different sub-phase that the same prompt also legitimately owns.

**Type A rejected for BUG â€” wrong responsibility class:**

A BUG prompt that asks to fix a runtime error has a correct sub-phase (e.g., 4.1 Backend
Services). The rejected must address a completely different responsibility:

```
Prompt:   "wildcard fallback makes it skip azure ai search â€” please fix"
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
and return â€” it does not fix the routing registration order that causes
requests to be intercepted by the wrong handler. Contract definition is
3.3 work; fixing the wrong handler is 4.1 work.]
```

**Why this produces positive margin:** The chosen uses implementation vocabulary
(route handler, registration order, module, controller). The rejected uses contract
vocabulary (endpoint, request body, response shape, pagination). Near-zero overlap.

**Type A rejected for BUG â€” wrong diagnostic scope:**

```
Prompt:   "I don't want to use legacy peer â€” fix the npm install conflict"
Chosen:   "Add --legacy-peer-deps to .npmrc or switch the conflicting
           dependency to a compatible version. Check npm ls for the full
           dependency chain: the conflict is between..."
Rejected: "Design the dependency management policy: define which package
           versions the project accepts, document the rationale for pinning
           versus ranges, and establish a review gate for major version
           upgrades."
[WRONG: npm install is failing now â€” the immediate fix is a command or
config change, not a policy document. Policy design (3.1 / 2.2) follows
working dependencies; it does not unblock the broken install.]
```

**Hard negative for BUG â€” intra-domain confusion between BUG and REFACTOR:**

These are the hardest cases. The signal words overlap ("fix", "clean", "wrong"):

```
Prompt:   "the auth middleware runs twice on protected routes â€” fix it"
Chosen (BUG â€” 4.1):
  "The middleware is registered globally in app.module.ts AND individually
   on the route decorator. Remove the route-level @UseGuards() â€” the global
   registration handles all routes already."

Rejected (REFACTOR â€” 4.4 â€” plausible but wrong domain):
  "Extract the auth middleware into a dedicated AuthInterceptor class.
   Move the token validation logic out of the route handler and into
   the interceptor. Rename the existing middleware to reflect its actual
   scope."
[WRONG: the symptom is double execution from duplicate registration â€” not
disorganised code. Refactoring the interceptor class does not fix the
duplicate run. The fix is removing one registration. Code organisation
is a separate concern that can be addressed after the bug is resolved.]
```

The annotation here is critical: it names why REFACTOR vocabulary (extract, move,
rename, reorganise) does not fix the BUG (duplicate registration). Without the
annotation, overlap ratio is ~0.35 â€” borderline. With the annotation, the additional
tokens in chosen (duplicate, registration, removes, global) push the distributional
separation to produce reliable positive margin.

---

### CODE_Q domain â€” rejected construction

CODE_Q calibration result: 0% positive margin batches. All four batches had
overlap_ratio ~0.70 from negation template. The rejected must be a wrong domain
classification, not a non-answer.

**The core rule for CODE_Q:** A CODE_Q prompt asks "what does X do" or "how does Y
work". The rejected must be an action response â€” either a BUG fix or a PLAN step â€”
not another explanation.

**Type C rejected for CODE_Q â€” wrong domain (explanation vs action):**

```
Prompt:   "how does the replay buffer work in the capability trainer"
Chosen (CODE_Q â€” explain):
  "The replay buffer stores manifests from previous training steps for this
   domain adapter. On each training call, the trainer draws up to 3 manifests
   from the buffer (FindReplayManifests) and includes them alongside the new
   example. This prevents catastrophic forgetting: the model sees recent past
   examples on every step, not only the current one. Buffer growth is bounded
   at 4096 entries per DefaultMaxCapacity."

Rejected (BUG â€” wrong domain):
  "The replay buffer is growing without bound because MaxCapacity is not
   being enforced. Add a cap: after each Append(), check buffer.Count >
   _maxCapacity and call RemoveRange(0, buffer.Count - _maxCapacity).
   This prevents RAM growth across long calibration runs."
[WRONG: the prompt asks how the replay buffer works â€” what its mechanism
is and what problem it solves. The fix for unbounded growth is A54-level
work, not an answer to "how does this work". Answering a how-does-it-work
question with a bug fix addresses a different intent entirely.]
```

**Why this works:** Chosen uses explanation vocabulary (stores, draws, prevents,
catastrophic forgetting, bounded). Rejected uses fix vocabulary (growing without bound,
add a cap, prevents RAM growth). Overlap ratio: typically 0.15â€“0.25.

**Intra-domain confusion for CODE_Q â€” explanation vs demonstration:**

```
Prompt:   "show me how the DomainClassifierUnit classifies predicates"
Chosen (CODE_Q â€” explain mechanism):
  "DomainClassifierUnit.ClassifyBaseDomain checks two paths. If _trained
   is true and Predicates is non-null, it calls the KNN classifier on the
   predicate integer array. If _trained is false, it falls to
   ClassifyByKeywords â€” a static keyword scan of the raw prompt string.
   In the current code, _trained is always false because Train() is never
   called from PromptDomainClassifier â€” this is the A61 gap."

Rejected (CODE_Q â€” demonstrate usage, not mechanism):
  "Here is how to call DomainClassifierUnit:
   var unit = new DomainClassifierUnit();
   var request = new DomainClassifierRequest(prompt, Predicates: [0, 1, 2]);
   var result = unit.ClassifyBaseDomain(request);
   // result.Domain contains the predicted D-04 domain"
[WRONG: "how does it classify" asks for the decision logic â€” what conditions
trigger which path. A usage example shows the API surface but not the
internal decision. The prompt asks about the mechanism, not the call syntax.]
```

---

### PLAN domain â€” rejected construction

PLAN calibration result: 4 of 6 batches negative margin. Root cause: batches 30, 26,
37, 12 used meta-commentary prompts. Batches 16, 17 (positive) used named-artifact prompts.

**Type B rejected for PLAN â€” wrong dependency order (primary pair type):**

Use the dependency confusion table from `DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v2.md`.
Every annotation must explain WHY the wrong order fails, not just that it is wrong.

```
Prompt:   "plan the implementation of JWT authentication for the multi-tenant API"
Chosen (PLAN â€” correct order):
  Branch 1 â€” 3.4 Security and Authorization Model (dependency order 1):
    Define the JWT validation rules: signature algorithm (RS256), required
    claims (sub, tenantId, roles), expiry window (15min access, 7d refresh).
    This branch owns the contract that all other branches must satisfy.

  Branch 2 â€” 3.3 API Contract Definition (dependency order 2):
    Define the protected endpoints and their required roles.
    Dependency: 3.4 must be complete â€” the contract cannot specify role
    requirements until the role model is decided.

  Branch 3 â€” 4.1 Backend Services (dependency order 3):
    Implement the JwtValidationMiddleware, TokenService, and
    AuthorizationPolicyProvider.
    Dependency: 3.4 and 3.3 must be complete â€” implementation follows spec.

Rejected (PLAN â€” wrong order, all parallel):
  Branch 1 â€” 3.4 Security and Authorization Model
  Branch 2 â€” 3.3 API Contract Definition
  Branch 3 â€” 4.1 Backend Services
  All branches can run in parallel â€” start everything at once.
[WRONG: branch 3 implements the middleware against a JWT spec. If 3.4 is
not complete, the implementation uses assumptions that may conflict with the
final validation rules. Branch 2 cannot define protected endpoint contracts
until the role model from 3.4 is decided â€” the contract references roles
that do not yet exist. Parallel execution of these branches always produces
rework when 3.4 decisions change what 3.3 and 4.1 already built.]
```

**Type C rejected for PLAN â€” abstract phase labels instead of sub-phases:**

```
Prompt:   "plan the migration from REST to GraphQL for the reporting service"
Chosen (PLAN â€” specific sub-phases):
  Branch 1 â€” 3.3 API Contract Definition (dependency order 1):
    Define the GraphQL schema: query types, field names, pagination
    pattern (cursor-based), and error type. The schema is the contract
    that all downstream work must match.

  Branch 2 â€” 4.3 Infrastructure and Deployment Code (dependency order 2):
    Add the GraphQL server package, configure the schema registration,
    and add the playground endpoint behind a feature flag.

  Branch 3 â€” 5.2 Integration Tests (dependency order 3):
    Write integration tests against the new GraphQL endpoint using
    real query strings. Dependency: schema must be finalised.

Rejected (PLAN â€” abstract labels):
  Phase 1: design the GraphQL migration.
  Phase 2: implement the migration.
  Phase 3: test the migration.
  Phase 4: deploy the migration.
[WRONG: a plan must name the specific sub-phases, their dependency order,
and what decision or artifact each branch produces. "Design, implement,
test, deploy" are labels, not a plan. They carry no routing signal to
the orchestrator â€” it cannot dispatch work to a sub-phase called "design".]
```

---

### ARCH domain â€” rejected construction

ARCH calibration result: zero batches. ARCH vocabulary appeared in BUG/PLAN sessions
(rows 1â€“520) but was never classified as ARCH. The core pair type is Type A: boundary
decision vs implementation.

**Type A rejected for ARCH â€” implementation confused with boundary decision:**

```
Prompt:   "define the component boundaries for the notification service"
Chosen (ARCH â€” 3.1 boundary decision):
  NotificationDispatcher â€” owns sending. Stateless. No persistence.
  TemplateStore â€” owns template registry. Read-only. Versioned.
  DeliveryTracker â€” owns delivery state and retry scheduling. Owns its own storage.
  Dependency direction: Dispatcher depends on TemplateStore and DeliveryTracker.
  TemplateStore and DeliveryTracker must not depend on Dispatcher.
  Forbidden: Dispatcher must not own state between requests.

Rejected (4.1 Backend Services â€” wrong scope):
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
dependencies are forbidden â€” it does not ask for the class body.
The class body is 4.1 implementation work that follows after the
boundary decision. Writing the implementation before the boundaries
are decided embeds coupling assumptions that later cause rework.]
```

**Overlap ratio:** chosen uses ownership vocabulary (owns, stateless, depends on,
forbidden, must not). Rejected uses class vocabulary (public class, private, async,
inject). Near-zero overlap. Reliably positive margin.

**Type A rejected for ARCH â€” operational configuration confused with signal design:**

```
Prompt:   "design the observability contract for the payment processing service"
Chosen (ARCH â€” 3.5 Observability Design):
  Metrics to emit:
    payment.processed â€” counter with dimensions: status (success/fail/timeout),
      provider (stripe/paypal), amount_tier (low/medium/high)
    payment.latency_ms â€” histogram, p50/p95/p99 breakpoints
  Alert threshold: payment.processed{status=fail} > 5% of total in 5min window
  Trace requirement: every payment request carries a trace ID from entry through
    all provider calls. Minimum span depth: 3 (gateway â†’ service â†’ provider)

Rejected (7.3 Monitoring and Feedback Loop â€” wrong scope):
  Set up a Grafana dashboard for payment processing:
  - Panel 1: payment success rate (line chart, 5min intervals)
  - Panel 2: latency p95 (gauge, alert at 800ms)
  - Panel 3: provider breakdown (pie chart)
  Configure PagerDuty integration: escalate when success rate < 95%.
[WRONG: "design the observability contract" defines what signals must be
emitted and what thresholds are meaningful â€” it does not configure the
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

### Hard negative class 1 â€” Same domain, wrong sub-phase

Two sub-phases in the same domain share vocabulary. The model must learn the boundary
between them. The rejected must use the vocabulary of the plausible wrong sub-phase
without crossing into clearly wrong territory.

**Rule:** The rejected must be defensible as an answer to a subtly different phrasing
of the same prompt. If a reasonable person would mark the rejected as clearly wrong,
it is not a hard negative â€” it is a Type A pair. Use Type A for the easy cases; use
hard negatives only for genuinely ambiguous boundaries.

**Example â€” BUG vs REFACTOR (most common intra-domain confusion):**

The distinction: BUG = something is broken and wrong output is produced. REFACTOR =
something works but is structured poorly. A prompt with "fix" can belong to either.
The signal is whether the output is currently wrong or merely messy.

```
BUG signal words:  error, exception, null, wrong, fails, breaks, unexpected,
                   returns incorrect, doesn't work, crash, TypeError

REFACTOR signal words: messy, hard to read, duplicate, too long, extract,
                       rename, clean up, simplify, restructure, dead code
```

A "fix" prompt that describes wrong output â†’ BUG. Same vocabulary but describing poor
structure with correct output â†’ REFACTOR. The hard negative for a BUG pair uses
REFACTOR vocabulary on the same code, and vice versa.

**Example â€” PLAN vs ARCH (adjacent domains):**

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

### Hard negative class 2 â€” Wrong domain classification from overlapping signal words

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
           every request â€” explain why this happens and how to fix it"

Chosen (BUG â€” BUG dominates because output is wrong):
  "The check `if (token.TenantId == request.TenantId)` uses string equality on
   a field that DevelopmentPromptCompiler normalises to lowercase. The token
   carries the original case from the auth provider. Fix: normalise both sides
   before comparison â€” token.TenantId.ToLower() == request.TenantId.ToLower()."

Rejected (CODE_Q â€” wrong because wrong output exists, not explanation requested):
  "The tenant isolation check compares token.TenantId against the request's
   tenantId claim. When these do not match, the middleware rejects the request.
   The field comes from the JWT payload â€” the auth provider sets it during
   token issuance."
[WRONG: the prompt says the check returns true for EVERY request â€” meaning
the output is currently wrong. A prompt that describes wrong output is BUG,
not CODE_Q, regardless of the "explain why" phrasing. The explanation should
name the defect; the chosen response names the defect AND provides the fix.
A pure explanation without a fix leaves the broken behaviour in place.]
```

---

### Hard negative class 3 â€” Structurally correct but wrong scope

The rejected has the right format and technical content but covers a different scope
than the prompt requested. This class is most common in PLAN pairs where a valid partial
plan is submitted as the full plan.

```
Prompt:   "plan the full implementation of the tenant management feature â€”
           includes data model, API, auth, and UI"

Chosen (PLAN â€” full scope, four branches):
  Branch 1 â€” 3.2 Data Model and Persistence Design (order 1)
  Branch 2 â€” 3.4 Security and Authorization Model (order 2)
  Branch 3 â€” 3.3 API Contract Definition (order 3)
  Branch 4 â€” 4.2 Frontend and Client Integration (order 4, depends on 3.3)

Rejected (PLAN â€” partial scope, only one branch):
  Branch 1 â€” 4.1 Backend Services:
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

**Rule 1 â€” Prompt with "fix" and wrong output described â†’ BUG, not REFACTOR**

If the session row describes a symptom (wrong value, crash, unexpected result), classify
as BUG even if "clean up" or "extract" language appears later in the same prompt.
BUG dominates REFACTOR.

**Rule 2 â€” Prompt with phase/flow/step reference â†’ PLAN**

If the session row names a flow ID, phase number, or sub-phase, classify as PLAN
even if the prompt also asks how something works. PLAN dominates CODE_Q when
sequencing language is present.

**Rule 3 â€” Prompt asking "should I use X or Y" â†’ ARCH**

Trade-off prompts belong to ARCH regardless of whether the answer is a short explanation.
ARCH signals: "best approach", "what pattern", "should I use", "is this the right structure".

**Rule 4 â€” Prompt with "explain", "what does", "how does" and no defect â†’ CODE_Q**

If there is no described wrong output and no sequencing request and no trade-off
question, classify as CODE_Q even if the content is technically complex.

**Rule 5 â€” Prompt containing ARCHITECTURE predicate vocabulary BUT described as BUG
in session context â†’ reclassify as ARCH for pair construction**

Rows 1â€“520 of XIIGEN-SESSION-1000 were labelled BUG/PLAN in the calibration run, but
many contain "architecture", "component", "boundary", "topology" vocabulary. These rows
contain ARCH content that was never trained. Extract as ARCH pairs by reclassifying the
domain label and constructing Type A rejected (implementation vs boundary decision).

**Rule 6 â€” Row where same prompt appears as chosen for multiple sub-phases â†’ exclude from
single-sub-phase pairs; use only for multi-route PLAN pairs**

If the same prompt legitimately routes to five sub-phases, it cannot be the A side for
a pair that teaches sub-phase 3.2 specifically â€” because rejected would have to be one
of the other four correct sub-phases. Use these prompts only for PLAN pairs where the
full multi-branch output is the chosen.

This rule explains 20/27 negative-margin BUG batches in the calibration run. The runner
assigned the same prompt to each sub-phase as its chosen, creating intra-batch conflict.

---

## Sources

### Source 1 â€” XIIGEN-SESSION-1000

Document type: Type 1 (A-side authority) for rows 521â€“727; mixed for rows 1â€“520;
excluded as A side for rows 1235â€“1753.

**Pair extraction priority:**
- Rows 521â€“727: PLAN Type B. Proven positive margin. Extract more pairs from this range.
- Rows 1â€“520: reclassify ARCHITECTURE-vocabulary rows as ARCH Type A.
- Rows 1235â€“1753: correction events only. Do not use raw prompts as A sides.

**Row classification test before extraction:**
- Check `CompiledPredicateIds` for the row. If near-empty: exclude as A side.
- Check for meta-commentary phrases (see Type 6). If present: exclude.
- Check for named artifacts or dependency language. If absent and PLAN: exclude.

---

### Source 2 â€” Claude Export Batch-0000

Document type: Type 3 (correction source) â€” primary.

**~393 correction events not yet extracted.** These are the highest-priority pairs.
Extraction order:

```
1. legacy-docs-skills-from-old-code (35 corrections) â€” ARCH, CODE_Q, DOCUMENT
2. value-proposition-code-gaps (104 corrections) â€” ARCH, BUG, PLAN
3. flow-01-station-testing (79 corrections) â€” BUG, IMPLEMENT, TEST
4. figma-to-code-skills-to-xiigen (69 corrections) â€” PLAN, ARCH, IMPLEMENT
5. session-prompt-and-learning-protocol (71 corrections) â€” PLAN, CODE_Q
```

For each correction event:
- H0 â†’ A side (only if Type 1 eligibility passes)
- A0 â†’ rejected + [WRONG: annotation derived from H1 correction text]
- A1 â†’ chosen

Quality gate before including:
- Run token overlap test: overlap < 0.50 required, < 0.25 preferred
- Rate High/Medium/Low (see Type 3 quality rating)
- Low = exclude regardless of how clean the export looks

---

### Source 3 â€” Local session files (xiigen_history)

Document type: Type 2 (B-side authority) for architecture documents;
Type 1 (A-side authority) for local session H turns.

Documents in `xiigen_history/old/` are Type 2 â€” formal architecture vocabulary.
Use for chosen B sides in ARCH and PLAN pairs. Do NOT use as A sides.

---

### Source 4 â€” Orchestrator Teaching Plan Documents

Document type: Type 2 (B-side authority). Never A side.

Use `ORCHESTRATOR-TEACHING-PLAN-v3.md` sub-cycle steps for PLAN Type B chosen content.
Use stopping rules and MUST conditions for ArbiterUnit chosen/rejected criteria.

---

### Source 5 â€” Session Teaching Ledger

Document type: Type 4 (measurement evidence).

Use for ModelTrainingUnit B sides. Do not derive A sides from teaching ledger rows.

---

## Gap map by domain

| Domain | Calibration result | Root cause | Primary gap |
|--------|-------------------|------------|-------------|
| ARCH | 0 batches | Never extracted â€” vocabulary present in rows 1â€“520 | Need Type A pairs (boundary vs implementation) |
| CODE_Q | 0% positive margin | Negation template â€” overlap ~0.70 | Need Type C rejected (action response, not negation) |
| BUG | 74% negative margin | Same prompt as chosen for 8+ sub-phases | Need Rule 6: exclude multi-route prompts from single sub-phase pairs |
| PLAN | 66% negative margin | Meta-commentary A sides | Need Rule: exclude rows 1235â€“1753 as A sides |

---

## Document type quick-reference

| Type | Name | A-side | B-side chosen | Rejected source | Overlap range |
|------|------|--------|---------------|-----------------|---------------|
| 1 | Real developer prompts | âœ“ | â€” | â€” | not applicable |
| 2 | Formal architecture documents | âœ— | âœ“ sub-phase vocab | Different sub-phase Purpose vocab | 0.10â€“0.20 |
| 3 | Correction events (export) | âœ“ H0 | âœ“ A1 | âœ“ A0, pre-made | < 0.25 |
| 4 | Calibration run data | âœ— | âœ“ metrics B side | Missing-field variant | 0.10â€“0.20 |
| 5 | Topology / seeder code | âœ— | âœ“ Purpose fields | Adjacent sub-phase Purpose | 0.10â€“0.25 |
| 6 | Excluded (session-loading, archives, dumps, mirrors, certs) | âœ— | âœ— | âœ— | â€” |
| 7 | Experiment status / code review | âœ— | âœ“ accurate status | Overstated status | 0.10â€“0.25 |
| 8 | Artifact examination documents | âœ— | âœ“ arbiter verdicts | Wrong severity level | 0.15â€“0.35 |
| 9 | Decision documents | âœ— | âœ“ chosen decision | Rejected alternative from rationale | 0.05â€“0.20 |
| 10 | Teaching plan documents | âœ— | âœ“ step ordering | Wrong step order | 0.15â€“0.30 |
| 11 | Measurement guidance documents | âœ— | âœ“ measurement fields | 11a missing layer / 11b wrong formula / 11c wrong field split | see subtypes |
| 12 | Project history documents | âœ— | âœ“ grounded historical claim | Invented motivation | 0.10â€“0.30 |
| 13 | Document state register | âœ— | âœ“ current version / blocking order | Superseded version / parallel execution | 0.05â€“0.25 |
| 14 | Behavioral rules documents | âœ— | âœ“ rule-compliant output | Forbidden pattern or missing Gate B | 0.05â€“0.25 |
| 15 | Session start prompt documents | âœ— | âœ“ gate-complete session | Gate-skipped session | 0.10â€“0.20 |
| 16 | Versioned document evolution | âœ— | âœ“ current-version vocabulary | Superseded-version vocabulary | 0.05â€“0.15 |
| 17 | Translation / migration plans | âœ— | âœ“ correct phase order | Wrong phase order from same doc | 0.30â€“0.45 |
| 18 | Flow portability protocols | âœ— | âœ“ portability-compliant flow | Hardcoded tenant violation | 0.10â€“0.20 |
| 19 | Agent tooling instructions | âœ— | âœ“ correct tool execution order | Step skipped or wrong order | 0.15â€“0.30 |
| 20 | Skill library documents | âœ— | âœ“ correct skill domain | Wrong skill â€” Type C only | 0.15â€“0.30 |
| 21 | Deep research / external reports | âœ— | âœ“ accurate external mechanism | Simplified / outdated â€” CODE_Q only | 0.20â€“0.40 |
| 22 | Execution state JSON files | âœ— | âœ“ complete state record | 22a missing field / 22b interrupted attempt | 0.10â€“0.25 |
| 23 | Gap remediation and fix documents | âœ— | âœ“ correct fix with evidence | Wrong fix or false-pass attempt | 0.15â€“0.30 |
| 24 | Generated code debug histories | âœ— | âœ“ corrected generated code | Pre-debug generated code | 0.25â€“0.50 |
| 25 | Configuration schema contracts | âœ— | âœ“ schema-valid config / enum values | Schema-violation variant | 0.10â€“0.30 |
| 26 | Source code contract files | âœ— | âœ“ exact interface / DTO fields | Inferred or imprecise field variant | 0.10â€“0.25 |
| 27 | Diagram and topology source files | âœ— | âœ“ parsed edge / dependency direction | Reversed or missing edge | 0.10â€“0.20 |
| 28 | Test suite source files | âœ— | âœ“ passing test assertion / expected value | Contradicts assertion or wrong fix | 0.15â€“0.30 |
| 29 | Phase implementation plans + state | âœ— | âœ“ correct step order within phase | Wrong step order or missing prerequisite | 0.30â€“0.45 |
| 30 | Skills factory registry documents | âœ— | âœ“ registered skill coverage | Missing coverage / overstated registration | 0.10â€“0.25 |
| 31 | Naming / mapping translation tables | âœ— | âœ“ current name from map | Old name / wrong naming system | 0.05â€“0.15 |
| 32 | Validation reports and certificates | âœ— | âœ“ real gate verdict from report | Fabricated concern / false pass | 0.15â€“0.25 |
| 33 | Conflict and comparison analysis | âœ— | âœ“ winning approach + rationale | Unchosen alternative (pre-made) | 0.10â€“0.25 |
| 34 | Definition of done and prerequisites | âœ— | âœ“ DoD criteria + gate verdict | Incomplete check / ignored criterion | 0.10â€“0.20 |

**Limited-authority families (not types â€” boundary rules apply):**

| Family | A-side | B-side | Scope limit |
|---|---|---|---|
| U04 Plugin / marketplace extension docs | âœ“ Type 1 (integration prompts) | âœ“ 4.5 and 3.3 only | No core XIIGen ARCH or PLAN authority |
| U08 Design conversion docs | âœ“ Type 1 (UI prompts, UI predicate) | âœ“ 1.1 and 4.2 only | Debug histories â†’ Type 24 |
| U12 Zone A capability matrices | âœ— | âœ“ Type 7 extension | Knowledge-edge detection pairs |
| U12 Zone B task type catalogs | âœ— | âœ“ Type 5 extension | Topology / router vocabulary |
| U12 Zone C source / RAG indexes | âœ— | âœ— | Vocabulary calibration only |
| U24 Marketplace legal / store assets | âœ— | âœ“ narrow exception only | Store listing pairs only; excluded by default |
| A02 Dependency / build manifests | âœ— | âœ“ version-pinning pairs | Wrong dependency version as rejected |
| A03 CI / editor / repo configs | âœ— | âœ“ CI ordering + tool compliance | `.github/workflows` needs â†’ PLAN; `.cursorrules` â†’ Type 19 extension |
| A04 Executable utility scripts | âœ— | âœ“ process order from code paths | Code paths only; not comments |
| A08 Spreadsheets / decision matrices | âœ— | âœ“ per-sheet zone | Decision â†’ Type 9; mapping â†’ Type 5/26; metric â†’ Type 4/21; planning â†’ Type 17/10 |
| A11 Localization / language variants | âœ“ A-side prompts in original language only | âœ— | Never independent truth; canonical source wins |
| A12 Runtime / deployment templates | âœ— | âœ“ deployment ordering + env var completeness | Secret screening mandatory before any use |
| A14 Runtime debug / tool logs | âœ— | âœ“ failure evidence (normalized) | BUG pairs only; timestamps/paths stripped |
| A16 Plugin extension manifests | âœ— | âœ“ runtime capability boundary | Plugin permission pairs only |
| A19 Generated UI code outputs | âœ— | âœ“ corrected code (Type 24 extension) | Generated code is rejected, not chosen |
| A20 Before/after visual sets | âœ— | âœ— metadata only | Visual QA evidence for Type 8; no tokenized images |
| A21 Positive/negative example catalogs | âœ“ if human-authored | âœ“ if human-authored | Provenance check mandatory; synthetics â†’ A07 rules |
| A22 Algorithm reference / math notes | âœ— | âœ“ CODE_Q only | ARCH/PLAN only with Type 9 adoption decision |
| A24 Template / boilerplate libraries | âœ— | âœ“ generation contract (required fields) | Missing required template field = rejected |
| B02 Serialized implementation parts | âœ— | âœ“ if no merged version exists | Check for merged file first; parts are mirrors if merged exists |
| B08 Prompt pattern libraries | âœ— | âœ“ Type 20/28 extension (pattern trigger) | Pattern violation or benchmark deviation |
| B10 DNA pattern documents | âœ— | âœ“ Type 18 extension (design grammar) | DNA pattern violation = mechanically derived rejected |
| B11 README / summary / onboarding docs | âœ“ A-side prompts only | âœ— never B-side authority | High-level approximations; B-side must use Type 2/5/26 |
| B12 Miscellaneous state JSON | âœ— | âœ“ classify by content zone | workflow â†’ Type 22; metrics â†’ Type 4; business state â†’ Type 21 |

**Key distinctions:**
- Types 12â€“16 all produce rejected from within their own corpus. Types 9, 12, 13, 16 frequently produce the same pair from different angles â€” use all four.
- Type 6 is the only type with no valid DPO use.
- Types 17 and 19 both produce PLAN Type B pairs. Type 17 = wrong migration phase order. Type 19 = skipped tool execution step.
- Type 18 is the only type where rejected is mechanically derived â€” add a hardcode to a correct flow.
- Type 20 is Type C only. Never Type A.
- Type 21 is CODE_Q only. ARCH or PLAN from Type 21 is a gate failure.
- Type 22 subtypes mirror Type 11a (missing field) and Type 3 (correction event).
- Type 23 records the full repair arc (gap â†’ wrong fix â†’ correct fix). If no wrong-fix attempt is in the document, use ARCH/BUG confusion tables.
- Type 24 differs from Type 3: defect is in generated code. Overlap higher (0.25â€“0.50). Design conversion debug histories (U08) are the primary source.
- Type 25 is the strongest source for field-level validation pairs â€” schema conformance is binary.
- Type 26 is stronger than Type 2 prose for exact interface/DTO field pairs. Three-check rule applies.
- U04 and U08 contribute A-side prompts (Type 1). B-side scope: U04 â†’ 4.5/3.3 only; U08 â†’ 1.1/4.2 only.
- U12 Zone A and B produce real pairs. Zone C is vocabulary calibration only.
- A02 build manifests: version-pinning rejected. Never architecture authority.
- A03 CI configs: `needs:` dependencies â†’ PLAN Type B. `.cursorrules` â†’ Type 19 extension.
- A04 scripts: code path order only. Not comment text.
- A07 training row files: self-training loop risk. Audit provenance before any reuse.
- A11 localized docs: A-side prompts valid in any language. B-side must use canonical source.
- A12 deployment templates: secret screening mandatory before any pair.
- Type 27 diagram files: parse nodes and edges â€” never flatten to prose first.
- Type 28 test files: only passing test assertions are chosen authority. Skipped/ignored tests are not.
- A14 debug logs: normalize (strip timestamps, paths) before any BUG pair. Never A side directly.
- A16 plugin manifests: plugin capability boundary pairs only. Not general architecture authority.
- A19 generated UI outputs: generated code is rejected, not chosen. Always use corrected version as chosen.
- A21 example catalogs: highest-value source after Type 3. Provenance check mandatory â€” model-generated examples â†’ A07 rules.
- A22 algorithm references: CODE_Q only unless adopted by Type 9 decision.
- A24 templates: generation contract pairs. Missing required template field = rejected.
- A17 office/PDF exports: extract first, dedupe against markdown, classify extracted content.
- A18 vendored codebases: external boundary required. Never internal authority without Type 9 adoption.
- A23 localized guides: covered by A11 rules â€” no separate handling.
- Type 29 phase plans: within-phase ordering. State files â†’ Type 22 rules.
- Type 30 skills factory: registry authority for knowledge-edge detection. What is registered vs missing.
- Type 31 naming maps: lowest overlap after Type 16 (0.05â€“0.15). Old name is the pre-made rejected.
- Type 32 validation reports: real verdict is the chosen. Fabricated concern about a confirmed item is the rejected.
- Type 33 comparison analysis: unchosen alternative is pre-made rejected. Drift analysis â†’ intended vs actual pairs.
- Type 34 DoD: completion claim without satisfying all criteria = reprocess. Arbiter checks DoD, not self-reported completion.
- B02 implementation parts: check for merged version first. Parts are mirrors if merged exists.
- B10 DNA patterns: Type 18 extension. Violation derived mechanically from pattern rule.
- B11 README/summaries: A-side prompts only. Never B-side authority.
- External pairs (U22) must never mix into internal batches.

---

## Pre-submission checklist

Before any pair from any internal source enters a batch:

```
â–¡ Source type identified: Type 1â€“34 or companion family A01-A51 / B13-B43 /
  C01-C12 / Z41-Z56 / D01-D19 / E01-E16 / F01-F17 / G01-G13 / H01-H08
  / I01-I14 / J01-J09 / K01-K13 / L01-L08 / M01-M21 / N01-N09 / O01-O19
  / P01-P12 / Q01-Q05 / R01-R13 / S01-S14 / T01-T10 / U01-U20 / V01-V08 / W01-W16
  / X01-X16 / Y01-Y10 / AA01-AA17 / AB01-AB07 / AC01-AC20 / AD01-AD08
  / AE01-AE10 / AF01-AF13 / AG01-AG06
  (zone rules and limited-authority families apply within types)
â–¡ If source is from the companion addendum: source_family, source_document_family,
  source_locator, target_registry_type, and family-specific DTO fields are present
â–¡ Companion addendum families L01-AG06 require the exact family DTO from
  XIIGEN-INTERNAL-SOURCE-REGISTRY-ADDITIONAL-TYPES-v1.md; broad labels such as
  "skill file", "experiment plan", "state JSON", "ZIP", or "reference doc" are
  not sufficient
â–¡ Machine-readable companion sources (JSON/YAML/JSONL) are parsed into their
  required DTO fields; filename/prose summaries are not evidence
â–¡ llm_mvp_core sources record project_id and adoption status before any XIIGen use
â–¡ xiigen-COMPLETE.zip sources record canonical_nestjs=true when used as the
  NestJS-era chosen source; conflicting Python/FLOW33 snapshots are stale
â–¡ Source canonicality confirmed: canonical or extracted_copy only
  â€” mirror, stale, archive_container, secure_operational â†’ gate failure
  â€” external â†’ must use external registry, must not mix with internal batch
â–¡ source_canonicality field recorded in pair file with canonical_confirmed_via
â–¡ Type 6 prompt detected and excluded (includes ZIP archives, raw dumps,
  worktree mirrors, certificates, environment material)
â–¡ A side passes Type 1 eligibility: named artifact OR dependency language OR
  technical specificity (for PLAN/ARCH); not meta-commentary
â–¡ CompiledPredicateIds not near-empty for the A side
â–¡ Row classification rule applied (rules 1â€“6 above)
â–¡ Token overlap test: overlap_ratio < 0.60 (target: < 0.35)
â–¡ Rejected is a complete, coherent response from the wrong sub-phase vocabulary
â–¡ Rejected is NOT a negation, NOT a non-answer, NOT another correct response
  for a different sub-phase of the same prompt
â–¡ [WRONG: ...] annotation names:
    (1) the sub-phase the rejected incorrectly addresses
    (2) why that sub-phase does not own this responsibility
    (3) what the prompt actually asked for that rejected missed
â–¡ Annotation is specific: "this is wrong" fails; named confusion required
â–¡ Hard negative class identified (1, 2, or 3) if applicable
â–¡ For correction events: quality rated High or Medium; Low excluded
â–¡ For correction events: conversation_uuid recorded
â–¡ TrainingText will use compiledText from SemanticTeachingPromptCompiler
  â€” not raw prompt string (hard gate)
â–¡ The same prompt does not appear as chosen for more than one sub-phase
  in this batch (Rule 6 violation check)
â–¡ For Type 7 pairs: implementation status verified against actual code file,
  not only against experiment results map â€” IMPLEMENTED claim requires file path evidence
â–¡ For Type 8 pairs: verdict type (pass/concern/reprocess) matches severity of finding â€”
  wrong output = reprocess, noted gap = concern, no issue = pass
â–¡ For Type 9 pairs: rejected alternative is derivable from decision rationale or
  prior session history â€” not invented
â–¡ For Type 10 pairs: dependency order in chosen matches the teaching plan's explicit
  prerequisite chain â€” harness before teaching, tracing before verification
â–¡ For Type 11 pairs: subtype identified (11a/11b/11c) before writing
  â€” 11a (missing layer): overlap < 0.15, annotation useful but not critical
  â€” 11b (wrong formula): overlap 0.40â€“0.55, token overlap test mandatory,
    annotation must introduce â‰¥ 5 non-overlapping tokens describing the consequence
  â€” 11c (wrong field split): overlap 0.20â€“0.30, annotation helpful
  For 11b: if overlap_ratio > 0.50 after annotation, revise before submitting
â–¡ For Type 12 pairs: event cited (part number, date) â€” not invented from general knowledge
â–¡ For Type 13 pairs: state is from a specific named version â€” not an average across versions
â–¡ For Type 14 pairs: rule cited by number and version â€” not paraphrased from memory
â–¡ For Type 15 pairs: forbidden output is the rejected â€” not a description of the rule
â–¡ For Type 16 pairs: rejected uses earlier-version vocabulary that the newer version corrected
â–¡ For Type 17 pairs: chosen phase order matches the migration plan's explicit prerequisite
  chain â€” verified against the plan document, not from memory. STATE.json interruption
  events rated High/Medium/Low before use (same as Type 3 correction events).
â–¡ For Type 18 pairs: portability violation in rejected is derived from a named check
  in the protocol document â€” never invented. Chosen passes all named checks explicitly.
â–¡ For Type 19 pairs: step sequence in chosen matches AGENTS.md / CLAUDE.md explicitly.
  Do not confuse with Type 15 (session gates) â€” Type 19 is within-task tool execution
  order. A side is always a Type 1 developer task prompt, never an instruction line.
â–¡ For Type 20 pairs: rejected is Type C only (wrong domain / wrong skill). Type A is not
  permitted for this type. Annotation cites the skill boundary from the skill document.
â–¡ For Type 21 pairs: domain is CODE_Q only â€” ARCH or PLAN pair from this type is a gate
  failure. Specific external source cited in annotation. No locked Type 9 decision
  contradicts the chosen content.
â–¡ For Type 22 pairs: subtype identified (22a missing field / 22b interruption). STATUS
  field in source JSON is "completed" or "interrupted" â€” never "in_progress". For 22b:
  correction_reason field present and used in annotation.
â–¡ For Type 23 pairs: three-part structure identified (gap / wrong fix / correct fix).
  If no wrong-fix attempt in document: use ARCH/BUG confusion tables for rejected â€”
  do not invent a wrong fix. Correct fix must have evidence (test result, run output,
  or verification statement).
â–¡ For Type 24 pairs: debug history confirms wrong code came from a real generation
  event. overlap_ratio computed: if > 0.45, annotation must introduce â‰¥ 5 non-overlapping
  tokens describing why wrong code fails functionally. Correct code must have
  verification signal (build, test, or visual confirmation) in the debug history.
â–¡ For zone-mapped pairs (U15/U16/U20/U21): zone identified (A/B/C) before pair
  construction. See extended zone mapping section for rules per zone.
â–¡ For business flow spec zones (Type 2 zone mapping): Zone A â†’ ArbiterUnit rules.
  Zone B â†’ ARCH Type A rules. Zone C â†’ PLAN Type B rules.
â–¡ For U04 plugin/marketplace docs: A-side prompt passes Type 1 eligibility.
  B-side scope confirmed as 4.5 or 3.3 only â€” any core XIIGen ARCH or PLAN
  authority from this source is a gate failure.
â–¡ For U08 design conversion docs: A-side prompt has UI predicate confirmed.
  B-side scope confirmed as 1.1 or 4.2 only. Debug histories from these sessions
  are Type 24 â€” apply Type 24 rules.
â–¡ For U12 capability catalog pairs: zone identified (A/B/C). Zone A â†’ Type 7
  extension (knowledge-edge). Zone B â†’ Type 5 extension (topology vocabulary).
  Zone C â†’ excluded (vocabulary calibration only, no pair output).
â–¡ For U24 marketplace legal assets: default is excluded. If using named exception
  (store listing description pairs): scope confirmed as marketplace listing only,
  source_canonicality recorded as canonical, not secure_operational.
â–¡ External source check: source_canonicality = external â†’ separate batch file required.
  mixed_with_internal: false. external_source_registry field required in pair file.
â–¡ For Type 25 pairs: enum values in chosen are present verbatim in the schema file.
  Schema file confirmed as current version (three-check rule). No enum values invented.
â–¡ For Type 26 pairs: interface/DTO fields in chosen match the source file exactly.
  Three-check rule applied â€” confirmed current contract version via document state register.
â–¡ For A02 build manifest pairs: version numbers in chosen match package.json / .csproj.
  B-side scope confirmed as version-pinning pair only â€” no architecture authority claimed.
â–¡ For A03 CI config pairs: workflow `needs:` chain verified against actual .yml file.
  `.cursorrules` / `.clinerules` pairs follow Type 19 rules.
â–¡ For A04 script pairs: call sequence extracted from code paths, not comments.
  Script file confirmed as executable evidence, not documentation proxy.
â–¡ For A07 dataset reuse: provenance audit complete. Model-generated rows excluded.
  No prompt appears as chosen in one row and rejected in another.
â–¡ For A08 spreadsheet pairs: sheet zone identified before pair construction.
  Preprocessing confirmed (sheet extracted to structured text, not raw binary).
â–¡ For A11 localized document prompts: source_canonicality = mirror recorded.
  A-side is the developer's real language. B-side uses canonical English source only.
â–¡ For A12 deployment template pairs: secret screening passed â€” no real credentials
  in source file. Scope confirmed as deployment ordering or env var completeness.
â–¡ Source type updated: Type 1â€“28 (zone rules and limited-authority families apply).
â–¡ For Type 27 diagram pairs: diagram file parsed as graph (nodes + edges). Chosen
  content derived from parsed edges, not from prose summary of diagram. Three-check
  rule applied â€” diagram confirmed current version.
â–¡ For Type 28 test pairs: test is currently passing (not skipped or ignored).
  Test intent (assertion, expected value) extracted â€” mocking scaffolding excluded.
  Three-check rule applied â€” test reflects current expected behavior.
â–¡ For A14 debug log pairs: log normalized (timestamps stripped, paths replaced).
  A side is a developer prompt derived from the error, not raw log text.
â–¡ For A16 plugin manifest pairs: scope confirmed as plugin capability boundary.
  Not used as general XIIGen architecture authority.
â–¡ For A19 generated UI pairs: generated code confirmed as rejected, corrected code
  as chosen. Uncorrected generated code never appears as chosen.
â–¡ For A21 example catalog pairs: provenance confirmed â€” human-authored, not synthetic.
  If synthetic: A07 provenance audit applied first. Label confirmed (not inferred).
â–¡ For A22 algorithm reference pairs: domain is CODE_Q only, OR Type 9 adoption
  decision cited. ARCH/PLAN without adoption decision is a gate failure.
â–¡ For A24 template pairs: all required template fields present in chosen.
  Missing required field confirmed as the rejected pattern.
â–¡ For A17 office/PDF pairs: text extracted from binary, deduplication against
  markdown source confirmed, version check applied.
â–¡ For A18 vendored codebase pairs: Type 9 adoption decision cited. Without
  adoption: external canonicality, Type 21 CODE_Q rules apply at most.
â–¡ For Type 29 pairs: step ordering confirmed against the phase plan document.
  Phase state files apply Type 22 rules.
â–¡ For Type 30 pairs: sub-phase ID checked against SKILLS_FACTORY registry.
  Coverage claim verified â€” not assumed from domain vocabulary.
â–¡ For Type 31 pairs: mapping table confirmed as current version. Old name
  confirmed as superseded in the map document before using as rejected.
â–¡ For Type 32 pairs: validation report confirmed as ground truth for the
  specific delivery. Rejected does not fabricate a gap that the report confirms.
â–¡ For Type 33 pairs: unchosen alternative is from the document's comparison,
  not invented. Annotation uses the document's own rejection rationale.
â–¡ For Type 34 pairs: all DoD criteria listed â€” not just the satisfied ones.
  Reprocess verdict when any criterion is missing from the delivery.
â–¡ For B02 parts: merged version checked first. If merged exists, use it.
  Parts files record source_canonicality as mirror if merged exists.
â–¡ For B10 DNA pattern pairs: pattern violation derived from the named rule
  in the document â€” not invented. Chosen satisfies all named rules.
â–¡ For B11 summary/README prompts: B-side content uses Type 2/5/26 source,
  not the README text. README provides A-side prompt only.
â–¡ For B12 state JSON: content zone identified. Source_type set to Type 22/4/21
  based on zone, not to a generic state type.
â–¡ For Z50-Z56 external structural families: source_canonicality = external,
  structural_pattern_only = true, and no XIIGen-specific B-side authority without
  Type 9 adoption.
â–¡ For F01 missing skill files: recovered SKILL.md content present; activation rule
  and skill boundary quoted before Type 20 pair construction.
â–¡ For G01-G05 runtime/validation artifacts: flow_id/phase_id and runtime_source_id
  or matrix_row_id recorded; DPO metrics not confused with product runtime logs.
â–¡ For G06-G11 archive bundles: manifest verified, archive extracted, and each
  member classified by content before pair construction.
â–¡ For G12-G13 source contracts: DTO fields and seed records extracted from source
  code, not inferred from prose or filename.
â–¡ For H01-H08 named session documents: named_artifact_kind, scope/version,
  target_type, and evidence_locator recorded; broad Type 29/32/34 labels are not enough.
â–¡ reward_margin_mean >= 0 confirmed after batch runs before committing
```

---

## Last Updated

2026-05-08. Version 11.0.
