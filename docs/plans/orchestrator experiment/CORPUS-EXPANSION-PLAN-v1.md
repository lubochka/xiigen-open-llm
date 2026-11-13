# Corpus Expansion Plan — 100 Training Documents Per Sub-Phase
# With 20 Held-Out Test Documents Per Sub-Phase
# How to build and verify a real orchestrator corpus across the full development lifecycle
# Version: 1.0 — 2026-05-04

---

## Purpose and Scope

This plan defines how to build a corpus of 100 source documents per sub-phase
across the full seven-phase development lifecycle, plus 20 held-out test
documents per sub-phase for orchestrator evaluation. It covers twenty-seven
sub-phases, giving a total corpus of 2,700 training documents and 540 held-out
test documents.

The corpus serves two purposes. First, it provides the training material for
the node cells that handle each phase domain — the sub-phase router, the domain
translator, the scheme planner, the phase context compiler, and the transition
verification unit. Second, it provides the evaluation set that proves the
orchestrator handles real documents correctly across the full lifecycle, not
just the controlled scenarios in the E53 and T34 experiments.

The plan is organized into five parts. Part 1 defines the document quota per
sub-phase and the split rules. Part 2 defines what counts as a valid document
for each sub-phase. Part 3 defines the source authority rules — where documents
come from and what makes them usable. Part 4 defines the held-out test set
structure and what each test document must prove. Part 5 defines the evaluation
protocol — how to run the orchestrator against the test set and what passing
looks like.

---

## Part 1 — Document Quotas and Split Rules

### Quota per sub-phase

Each sub-phase receives exactly 100 training documents and 20 held-out test
documents. The split is 100/20, not a percentage of a larger pool, because the
orchestrator must prove it handles each sub-phase independently before the
cross-phase capability is tested.

The 100/20 numbers are not arbitrary. One hundred training documents is enough
to cover the predicate pattern variation within a sub-phase — the different
ways the same sub-phase appears across different companies, product types,
team sizes, and regulatory contexts — without introducing the corpus drift that
occurs when the training set becomes so large that infrequent patterns dominate
the learned distribution. Twenty test documents is enough to expose both common
and edge-case behavior within a manageable review budget.

### Sub-phase quota table

| Phase | Sub-phase | Training | Test |
|---|---|---:|---:|
| 1 Design | 1.1 Design system and tokens | 100 | 20 |
| 1 Design | 1.2 Screen and layout structure | 100 | 20 |
| 1 Design | 1.3 Interaction model and state transitions | 100 | 20 |
| 1 Design | 1.4 Accessibility and compliance | 100 | 20 |
| 1 Design | 1.5 Motion and animation | 100 | 20 |
| 2 Functional spec | 2.1 User stories and acceptance criteria | 100 | 20 |
| 2 Functional spec | 2.2 Business rules | 100 | 20 |
| 2 Functional spec | 2.3 Integration point specification | 100 | 20 |
| 2 Functional spec | 2.4 Data model boundaries | 100 | 20 |
| 3 Technical design | 3.1 Architecture and component topology | 100 | 20 |
| 3 Technical design | 3.2 Data schema and migration design | 100 | 20 |
| 3 Technical design | 3.3 API contract definition | 100 | 20 |
| 3 Technical design | 3.4 Security and authorization model | 100 | 20 |
| 3 Technical design | 3.5 Infrastructure and deployment topology | 100 | 20 |
| 4 Implementation | 4.1 Backend services | 100 | 20 |
| 4 Implementation | 4.2 Frontend and client rendering | 100 | 20 |
| 4 Implementation | 4.3 Integration and provider connectors | 100 | 20 |
| 4 Implementation | 4.4 Data migration execution | 100 | 20 |
| 4 Implementation | 4.5 Documentation | 100 | 20 |
| 5 Testing | 5.1 Unit and contract tests | 100 | 20 |
| 5 Testing | 5.2 Integration tests | 100 | 20 |
| 5 Testing | 5.3 End-to-end scenario tests | 100 | 20 |
| 5 Testing | 5.4 Performance and load tests | 100 | 20 |
| 6 QA | 6.1 Functional review | 100 | 20 |
| 6 QA | 6.2 Regression coverage verification | 100 | 20 |
| 6 QA | 6.3 Security and compliance review | 100 | 20 |
| 6 QA | 6.4 Accessibility audit | 100 | 20 |
| 7 QA automation | 7.1 Test scenario automation | 100 | 20 |
| 7 QA automation | 7.2 Regression suite maintenance | 100 | 20 |
| 7 QA automation | 7.3 CI/CD gate configuration | 100 | 20 |
| 7 QA automation | 7.4 Coverage and reporting | 100 | 20 |
| **Total** | | **2,700** | **540** |

### Split rules

The 100/20 split uses stratified assignment, not random assignment. This is
required by D-08. Stratified assignment means: before splitting, the 120
documents for each sub-phase are grouped into at least four strata by document
variation type. Then each stratum contributes proportionally to both the
training set and the test set.

The four variation strata for every sub-phase are:

**Stratum A — domain breadth.** Documents from different business verticals
(e-commerce, healthcare, fintech, developer tools, consumer social). Each
stratum A document must be assignable to a different business vertical.

**Stratum B — team context.** Documents that differ by team size and
organizational context (startup with two engineers, mid-size product team,
regulated enterprise, open-source project). Each stratum B document must
reflect a different team context.

**Stratum C — depth level.** Documents at different levels of completeness
and detail — a one-page sketch, a detailed specification, a comprehensive
reference document. Each stratum C document must differ in document depth
from the others.

**Stratum D — failure and edge case.** Documents that contain incomplete
information, ambiguous requirements, conflicting decisions, or boundary
conditions. These are the documents that probe the knowledge edge and
rejected-item detection behavior.

The 100 training documents contain at minimum: 25 from stratum A, 25 from
stratum B, 25 from stratum C, 25 from stratum D. The 20 test documents
contain at minimum: 5 from each stratum. This guarantees the test set is not
drawn entirely from easy, clean, well-formed documents.

---

## Part 2 — What Makes a Valid Document for Each Sub-Phase

### The three validity conditions

A document is valid for a sub-phase's quota when it satisfies all three
conditions from the experiment readiness check in the development phases guide.

**Condition 1 — Primary phase assignment is unambiguous.** The document
belongs to exactly one sub-phase by the two-question classification method.
If the classification is ambiguous after applying the four rules, the document
is rejected from the quota.

**Condition 2 — At least three verifiable claims.** The document makes
specific statements that can be checked. Not vague intentions — specific
claims about what is required, enforced, or prohibited.

**Condition 3 — At least one known failure mode.** You can describe what
it looks like for a claim to appear satisfied while actually failing. A
document with no failure modes is too simple to be useful.

### Sub-phase-specific validity signals

Each sub-phase has additional validity requirements beyond the three general
conditions. These are not extra burden — they prevent quota-filling with
documents that are technically valid but teach nothing useful.

**Sub-phase 1.1 — Design system and tokens.** The document must name at
least three specific token values (not categories — values). A document that
says "we have a color system" without naming tokens is not valid. A document
that names `color-primary: #1A73E8`, `spacing-base: 8px`, and
`font-size-body: 16px` is valid.

**Sub-phase 1.2 — Screen and layout structure.** The document must name at
least four screens by purpose and describe the navigation path between at
least two pairs of screens. A screen list without navigation relationships
is stratum C depth-zero and does not qualify.

**Sub-phase 1.3 — Interaction model and state transitions.** The document
must define at least one state machine — a named component with named states
and named triggers between them. A prose description of interactions without
state names is not valid.

**Sub-phase 1.4 — Accessibility and compliance.** The document must name
a specific standard level (WCAG 2.1 AA, Section 508, EN 301 549) and at
least two success criteria that apply to the product's specific interaction
patterns. Generic accessibility statements without criteria references are
stratum C depth-zero.

**Sub-phase 1.5 — Motion and animation.** The document must specify at
least two named animation categories with duration values. A document that
says "animations should feel smooth" is not valid.

**Sub-phase 2.1 — User stories and acceptance criteria.** Each story must
name the user type, the goal, and at least one testable acceptance criterion.
Stories without acceptance criteria are not valid.

**Sub-phase 2.2 — Business rules.** Each rule must name its authority source
— the regulatory body, business policy document, or contractual term that
requires it. Rules without authority sources are stratum D failure cases, not
stratum A standard cases.

**Sub-phase 2.3 — Integration point specification.** The document must name
at least one external system by its business purpose (not its vendor name)
and state what the product needs from it without specifying implementation.

**Sub-phase 2.4 — Data model boundaries.** The document must name at least
three entities with their business definitions and at least two relationships
between them in business vocabulary.

**Sub-phase 3.1 — Architecture and component topology.** The document must
contain at least one rule stated as must or must not (not should or would
prefer). Suggestions without rules are stratum C depth-zero.

**Sub-phase 3.2 — Data schema and migration design.** The document must
specify at least one constraint beyond column types — a unique constraint,
a foreign key, a check constraint, or a migration safety rule.

**Sub-phase 3.3 — API contract definition.** The document must specify
at least one error code or error condition with its meaning. An API document
without error handling is incomplete and qualifies only as stratum D.

**Sub-phase 3.4 — Security and authorization model.** The document must
name at least two roles with different permission sets. A document with one
role is not modeling authorization — it is documenting a single access level.

**Sub-phase 3.5 — Infrastructure and deployment topology.** The document
must name at least one gate condition — a condition that must be true before
deployment proceeds. Infrastructure documents without gates are aspirational
and qualify only as stratum D.

**Sub-phases 4.1 through 4.5 — Implementation.** Each document must
reference a specific Phase 3 decision as a constraint it is implementing,
not discovering. Implementation documents that treat technical design
decisions as open questions belong to stratum D.

**Sub-phases 5.1 through 5.4 — Testing.** Each document must specify pass
conditions in measurable terms. Test documents that describe what to test
without specifying what passing looks like are stratum D.

**Sub-phases 6.1 through 6.4 — QA.** Each document must produce a verdict
for at least one acceptance criterion or requirement. QA documents without
verdicts are in-progress field notes, not QA outputs.

**Sub-phases 7.1 through 7.4 — QA automation.** Each document must name
at least one gate condition that blocks promotion when violated. Automation
documents without gates are descriptions of what automation does, not
enforcement definitions.

---

## Part 3 — Source Authority Rules

### Where documents come from

The corpus is built from three source tiers. Each tier has different trust
handling and different collection procedures.

**Tier 1 — Real project documents (highest value).** Documents produced in
the course of actual software development projects, anonymized where necessary.
These are the most valuable because they contain the variation, incompleteness,
and ambiguity that real orchestration requests produce. They are also the most
restricted because they may contain confidential information that cannot be
used in training.

Collection procedure: read the document, identify all company names, product
names, personal names, and proprietary technical details, replace them with
generic placeholders, verify that the replaced document still makes the same
verifiable claims, add the document to the corpus only if it passes all three
validity conditions after anonymization.

Trust: training OK after anonymization and validity check.

**Tier 2 — Public open-source project documents (current primary source).**
Documents from public repositories, specifications, and technical standards
that are published under licenses permitting training use. The current external
corpus batch-001 through batch-003 uses this tier.

License classes and their handling are already defined in the corpus index.
Apache 2.0 and MIT are training OK. CC-BY 4.0 requires attribution. PSF 2.0
is training OK.

Collection procedure: identify the source, confirm the license, extract the
relevant section, apply the experiment readiness check, produce the node pairs.

Trust: training OK per license class.

**Tier 3 — Synthetic documents (gap filler only).** Documents authored
specifically to fill gaps in the corpus that tier 1 and tier 2 cannot cover.
Used when a sub-phase has a stratum that cannot be reached from real or
open-source sources — for example, the regulatory compliance stratum of
sub-phase 2.2 for healthcare-specific business rules that do not appear in
public repositories.

Synthetic documents must be authored against a real regulatory or technical
reference. A synthetic document without a traceable source authority is not
valid. The author must cite the specification, standard, or domain reference
that the synthetic content is derived from.

Trust: training OK only after review by a domain expert who confirms the
synthetic document correctly represents the cited authority.

### License annotation requirements

Every document in the corpus must carry four annotations before it enters
the quota count:

Source identifier — a unique ID in the format `[BATCH]-[SUB-PHASE]-[SEQ]`,
for example `B001-1.1-042`.

License class — the license under which the source material is used.

Attribution required — yes or no, and if yes, the required attribution text.

Anonymization applied — yes or no, and if yes, the anonymization log
confirming what was replaced and with what.

A document without all four annotations does not count toward the quota.

---

## Part 4 — The Held-Out Test Set Structure

### What each test document must prove

A held-out test document is not just a document the orchestrator has not
seen. It is a document chosen specifically to probe one behavior the
orchestrator must exhibit. Each of the 20 test documents for a sub-phase
tests one of five behavior categories, with four documents per category.

**Category 1 — Correct domain assignment (4 documents per sub-phase).**
Each document is clearly assignable to the target sub-phase. The orchestrator
must route it correctly. These are the baseline competence tests. Pass
condition: node-01 produces the correct sub-phase domain label with confidence
above 0.70.

**Category 2 — Knowledge edge detection (4 documents per sub-phase).**
Each document contains at least one claim that depends on information the
corpus does not have — a regulatory requirement for a jurisdiction not in the
training set, a proprietary API whose specification is not public, a business
rule whose authority source is not named. The orchestrator must detect the
knowledge edge and produce a learning gap packet rather than inventing an
answer. Pass condition: knowledge edge packet produced, no invented answer
in node-09 output.

**Category 3 — Cross-sub-phase constraint detection (4 documents per
sub-phase).** Each document contains a constraint that depends on a decision
from a different sub-phase. For example, a sub-phase 3.3 API contract document
that assumes an authorization model from sub-phase 3.4 without naming it. The
orchestrator must detect the cross-sub-phase dependency and name it as a
boundary rather than treating the assumption as resolved. Pass condition:
boundary predicate produced for the cross-sub-phase assumption.

**Category 4 — Conflict detection (4 documents per sub-phase).** Each
document contains at least one internal conflict — two claims that cannot
both be true. For example, a sub-phase 3.2 schema document that defines a
nullable column and then requires it in a constraint. The orchestrator must
detect the conflict and produce a rejected item with the specific predicates
named. Pass condition: rejected item produced naming the two conflicting
claims.

**Category 5 — Stratum D edge cases (4 documents per sub-phase).** Each
document represents a genuine edge case — a document at an unusual level of
incompleteness, a document with ambiguous sub-phase assignment, a document
that makes claims that appear valid but fail on examination. Pass condition
varies by case and is specified per document in the test set manifest.

### Test set manifest format

Every test document has a manifest entry with five fields:

Document ID — the unique identifier in the corpus.

Test category — one of the five categories above.

Expected orchestrator behavior — the specific behavior the document is
designed to elicit, stated precisely enough to be checked mechanically.

Pass condition — the exact metric value or output content that constitutes
a pass. Not "the orchestrator should handle this correctly" — the specific
measurable condition.

Failure classification — what the failure means if the test does not pass.
One of: wrong domain assignment, knowledge edge missed, constraint not
detected, conflict not detected, edge case not handled.

A test document without a complete manifest entry cannot be used as an
evaluation document. The manifest entry is part of the test document, not
a separate artifact.

---

## Part 5 — The Evaluation Protocol

### Running the orchestrator against the test set

The evaluation protocol runs once per sub-phase after the training set for
that sub-phase is complete and the relevant node cells have been trained.
It does not run during corpus construction. It does not run before training
is complete.

**Step 1 — Baseline measurement.** Before running any test documents,
record the full test suite count. This is the floor. The evaluation protocol
does not introduce new passing tests — it produces an evaluation report.
The floor must hold throughout. If the full suite count drops during
evaluation, stop and investigate before continuing.

**Step 2 — Run category 1 documents.** Feed each of the four category 1
documents through the orchestrator. Record: the domain label produced by
node-01, the confidence score, whether the label matches the expected sub-
phase, and the complete node-09 output. A category 1 document passes if
node-01 produces the correct label at confidence above 0.70. Record pass
or fail per document.

**Step 3 — Run category 2 documents.** Feed each of the four category 2
documents through the orchestrator. Record: whether a knowledge edge packet
was produced, the specific knowledge edge named, whether the node-09 output
contains invented content for the knowledge edge domain. A category 2
document passes if a knowledge edge packet is produced and no invented answer
appears in node-09. Record pass or fail per document.

**Step 4 — Run category 3 documents.** Feed each of the four category 3
documents through the orchestrator. Record: whether a boundary predicate
was produced for the cross-sub-phase assumption, what the boundary predicate
names. A category 3 document passes if a boundary predicate is produced that
correctly identifies the cross-sub-phase dependency. Record pass or fail
per document.

**Step 5 — Run category 4 documents.** Feed each of the four category 4
documents through the orchestrator. Record: whether a rejected item was
produced, the specific predicates named in the rejected item, whether the
reject reason matches the actual conflict in the document. A category 4
document passes if a rejected item is produced naming both conflicting claims.
Record pass or fail per document.

**Step 6 — Run category 5 documents.** Feed each of the four category 5
documents through the orchestrator. Record against the per-document pass
condition from the test set manifest. These require manual review — the
evaluator reads the node-09 output and the manifest pass condition and
records pass or fail.

**Step 7 — Produce the sub-phase evaluation report.** The report contains:
the sub-phase identifier, the training document count, the test document count
by category, the pass count by category, the fail count by category, the
overall pass rate, the failure classification for every failing document, and
the recommended next action.

### Passing thresholds

A sub-phase passes its evaluation when:

Category 1 pass rate is 4/4. Domain assignment is not a soft target — the
orchestrator must correctly identify every clearly-assigned document.

Category 2 pass rate is at least 3/4. One missed knowledge edge is acceptable
if the failure classification is "weak signal" — the knowledge edge was not
clearly marked in the document. Zero tolerance for invented answers even when
the knowledge edge is missed.

Category 3 pass rate is at least 3/4. One missed cross-sub-phase constraint
is acceptable if the dependency was implicit rather than signaled.

Category 4 pass rate is at least 3/4. One missed conflict is acceptable if
the conflict was between two distant sections of the document.

Category 5 pass rate is at least 3/4. Edge cases by definition have no
universal rule — the one acceptable failure must be classified and understood,
not simply noted.

**Overall minimum: 17/20 documents passing.** A sub-phase that passes
below 17/20 is not ready for cross-phase testing. The failing documents must
be analyzed, the failure pattern must be identified, and the relevant node
cells must be retrained before the sub-phase re-evaluates.

### Cross-phase evaluation

After all twenty-seven sub-phases pass their individual evaluations, the
cross-phase evaluation runs. This is a separate evaluation using documents
that deliberately span phase boundaries — a document that is correctly
assignable to Phase 3 but whose content requires acknowledgment of a Phase 2
decision, or a document that is correctly assignable to Phase 4 but whose
content creates a constraint for Phase 5.

The cross-phase evaluation uses ten documents per phase transition — six
phase transitions in the canonical lifecycle, sixty cross-phase test documents
in total. Each document has a manifest entry specifying: which two phases
are involved, what the cross-phase content is, whether the expected behavior
is a PASS, PASS WITH GAPS, or BLOCK verdict from the transition gate, and
the specific gate condition that makes the verdict correct.

The cross-phase evaluation passes when all ten documents per transition
produce the correct verdict. A single incorrect verdict on a cross-phase
document is a protocol-level failure — it means either the phase context
compiler is dropping a constraint or the transition verification unit is
misclassifying an artifact type. Both require investigation before the
overall corpus evaluation can be declared complete.

---

## Part 6 — Build Sequence

### Why the build sequence is not alphabetical by phase

The 2,700 training documents and 540 test documents cannot be built in
phase order — Phase 1 first, Phase 7 last — because the node cells that
handle later phases depend on predicate patterns learned from earlier phases,
and the cross-phase evaluation depends on the transition verification unit
which is trained on examples from both sides of each transition.

The correct build sequence groups sub-phases by two criteria: how much
predicate pattern overlap they share with the existing corpus, and whether
their test documents feed into the training material for the transition cells.

**Wave 1 — Highest overlap with existing corpus (build first).**

Sub-phases 3.1, 3.3, 3.4 (architecture, API contract, security model) overlap
most directly with the infrastructure orchestration pairs already in clean-
batch-001 and clean-batch-002. The existing predicate patterns for component
contracts, API shapes, and security enforcement are already represented in
the corpus from the OTel, EF Core, and Dapper scenarios. Building these
sub-phases first extends the existing coverage rather than introducing new
domain vocabulary from scratch.

Sub-phases 4.1, 4.3 (backend services, integration connectors) follow
immediately because they reference the Phase 3 decisions already in the corpus.

**Wave 2 — Medium overlap (build second).**

Sub-phases 2.1, 2.2, 2.3, 2.4 (the full functional specification phase)
and sub-phases 3.2, 3.5 (data schema, infrastructure). These introduce
business-rule vocabulary and schema vocabulary that is adjacent to but not
identical to the current corpus.

Sub-phases 4.2, 4.4, 4.5 (frontend, data migration, documentation) follow
the same wave because they reference Phase 2 and Phase 3 decisions.

**Wave 3 — Testing and QA phases (build third).**

Sub-phases 5.1 through 5.4 and 6.1 through 6.4. These reference decisions
and implementations from waves 1 and 2 as the things being tested and
reviewed. Building them before the implementation sub-phases would produce
test documents with no ground truth for the claims they verify.

**Wave 4 — Design phase (build fourth).**

Sub-phases 1.1 through 1.5. This is counterintuitive — the design phase is
first in the lifecycle but fourth in the build sequence. The reason is that
design-phase vocabulary (tokens, component primitives, interaction states,
WCAG criteria) is the furthest from the current corpus and introduces the
most new predicate patterns. Building it after waves 1 through 3 means the
sub-phase router and domain translator cells have already learned to recognize
several predicate pattern families, and adding the design vocabulary extends
a working system rather than starting from an empty one.

**Wave 5 — QA automation phase and transition documents (build last).**

Sub-phases 7.1 through 7.4 reference all prior phases. The transition
documents for the cross-phase evaluation require completed sub-phase test
sets from both sides of each transition. These are produced last because
they depend on the output of every prior wave.

### Build wave summary

| Wave | Sub-phases | Training docs | Test docs |
|---|---|---:|---:|
| 1 | 3.1, 3.3, 3.4, 4.1, 4.3 | 500 | 100 |
| 2 | 2.1-2.4, 3.2, 3.5, 4.2, 4.4, 4.5 | 900 | 180 |
| 3 | 5.1-5.4, 6.1-6.4 | 800 | 160 |
| 4 | 1.1-1.5 | 500 | 100 |
| 5 | 7.1-7.4, cross-phase docs | 400 + 60 | 80 + 60 |
| **Total** | | **3,160** | **680** |

Note: the total including cross-phase documents is 3,160 training documents
and 680 test documents. The sub-phase-only counts are 2,700 and 540.

---

## Part 7 — What Each Batch Produces

Every wave is divided into batches of approximately fifty source documents.
Each batch follows the same production sequence used in the existing corpus
work: examine the source, classify to sub-phase, apply the experiment
readiness check, produce node pairs, run the five pre-promotion checks, add
to the batch registry.

Each batch produces:

A source manifest listing every document with its source identifier, license
class, anonymization status, stratum assignment, and validity check result.

Node pair files for nodes 01 through 09 covering the sub-phases in the batch.
Array nodes (02, 04, 05, 06) produce one pair per scenario element, not one
pair per scenario.

A simulation check for each sub-phase answering whether the pairs would teach
the relevant node cells correctly.

An updated batch registry entry in the corpus index.

A coverage ledger update recording the new source identifiers and their
promotion status.

No batch is promoted until all five pre-promotion checks pass: license
confirmed, validity conditions met, node pairs correctly shaped, simulation
check passes, phrasing B companion present for every original pair.

---

## Part 8 — Evaluation Cadence

The evaluation protocol runs once per completed wave, not once at the end of
all five waves. This allows the transition cells to be trained on wave 1 and
wave 2 documents before wave 3 and wave 4 documents introduce new vocabulary
that might shift the predicate distribution.

Wave 1 evaluation: after wave 1 corpus is complete and cells are trained.
Pass threshold: 17/20 per sub-phase across all five wave 1 sub-phases.

Wave 2 evaluation: after wave 2 corpus is complete and cells are retrained
on wave 1 plus wave 2 documents. Pass threshold: 17/20 per sub-phase. Any
wave 1 sub-phase that drops below 17/20 after wave 2 retraining is a
regression — the wave 2 vocabulary has interfered with wave 1 behavior and
must be investigated before wave 3 begins.

Wave 3 evaluation: same pattern. Additional regression check against wave 1
and wave 2 sub-phases.

Wave 4 evaluation: same pattern. Design-phase vocabulary is the highest risk
for interference with the existing corpus — extra attention to wave 1
category 1 results during the wave 4 regression check.

Wave 5 evaluation: sub-phase evaluation plus cross-phase evaluation. The
cross-phase evaluation runs for the first time here because all sub-phase
corpora are now complete.

Final corpus declaration: the corpus is complete when all twenty-seven
sub-phases pass at 17/20 or above, no wave regression is present, and the
cross-phase evaluation passes all sixty transition documents at the correct
verdicts.
