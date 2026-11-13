# Corpus Expansion Plan
# Whole-development corpus expansion after orchestration proof gates
# Version: 4.0 - 2026-05-04
# Supersedes: CORPUS-EXPANSION-PLAN-v3.md

---

## Superseded By v5

This version is superseded by `CORPUS-EXPANSION-PLAN-v5.md`.

Version 5 removes the stop-and-report gate model. Validation still happens, but
it is an internal execution check, not a user-facing stopping point. Corpus
completion is redefined around continuous full-path orchestration coverage and
source-backed learning usefulness, with production-scale quotas moved to a
maturity backlog instead of blocking the first build-ready corpus.

---

## What Changed From v3

This version corrects the sub-phase count and binds corpus expansion to the
whole-development-area experiment set.

The canonical lifecycle contains 31 sub-phases, not 27. The previous arithmetic
of 2,700 training documents and 540 held-out test documents is therefore not
complete for whole-development coverage.

Corrected corpus target:

- 31 sub-phases * 100 training documents = 3,100 training documents
- 31 sub-phases * 20 held-out test documents = 620 held-out test documents
- cross-phase transition set = 60 held-out transition documents

The full held-out evaluation target is therefore 680 documents:

- 620 sub-phase held-out documents
- 60 transition held-out documents

Training corpus target before transition-specific augmentation is 3,100
documents. Transition-gate and phase-context-compiler pairs are produced from
WDA and transition experiments, not from blind document quotas.

---

## Non-Negotiable Rule

Do not build the broad corpus before the orchestrator proves the shape it needs
to learn.

The WDA experiment set is the gate before corpus scale:

1. WDA-00 proves lifecycle topology representation.
2. WDA-01 proves document-to-sub-phase classification.
3. WDA-02 proves within-phase orchestration.
4. WDA-03 proves phase transition continuity.
5. WDA-04 proves learning from transition failure.
6. WDA-05 proves methodology variation.
7. WDA-06 proves full lifecycle orchestration.

The corpus expansion exists to train the units that fail or show thin evidence
inside those experiments. The corpus must not be used to mask missing
orchestration ability.

---

## Runtime State Assumption

The current runtime surface supports typed topology mutation:

- domains
- phases
- sub-phases
- transitions

The topology bootstrap proof, WDA-00, has passed at 1147/1147. That means the
system can represent the lifecycle topology through typed logical commands.

The following are still not proven by WDA-00:

- natural-language topology mutation
- document classification
- sub-phase routing
- phase transition verification
- learning from phase-transition failure
- full lifecycle orchestration

This plan treats those as experiment gates, not assumptions.

---

## Canonical Phase And Sub-Phase Quota Table

Each sub-phase receives 100 training documents and 20 held-out test documents
after its WDA classification and within-phase gates are defined.

Four strata are required:

- Stratum A: domain breadth
- Stratum B: team context
- Stratum C: depth level
- Stratum D: failure and edge case

Training split per sub-phase:

- 25 documents per stratum

Held-out split per sub-phase:

- 5 documents per stratum

| Phase | Sub-phase | Training | Held-out |
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
| **Total** | **31 sub-phases** | **3,100** | **620** |

---

## Source Authority Tiers

Every document entering the corpus must carry source authority annotation.

### Tier 1 - Local project documents

Preferred source. These include XIIGen plans, guides, architecture documents,
session plans, testing plans, QA plans, validation reports, task catalogs, and
business flow documents under `C:\Users\lubao\Documents\xiigen`.

Tier 1 documents are allowed into WDA-01 and WDA-02 immediately after document
readiness checks pass.

### Tier 2 - Public permissive project documents

Allowed when a local sub-phase lacks source coverage. License must be recorded
before the document enters the quota.

### Tier 3 - Synthetic reviewed documents

Allowed only when no Tier 1 or Tier 2 source exists. The synthetic document must
name its traceable authority and must include a reviewer note.

Synthetic documents cannot exceed 20 percent of any sub-phase training set.

---

## Document Readiness Checks

A document is ready only if all checks pass:

1. It has exactly one primary phase and one primary sub-phase.
2. It makes at least three verifiable claims.
3. It includes or implies at least one known failure mode.
4. It has source authority annotation.
5. Its expected top-three predicate profile is hand-written before training
   pair generation.
6. It has no missing license or provenance record.

Documents that fail readiness are not discarded. They are logged as knowledge
edges for the relevant sub-phase.

---

## Corpus Units Trained

The broad corpus trains these logical unit roles.

| Role | What it receives | What it emits |
|---|---|---|
| `sub-phase-router` | compiled predicate representation of a document | phase id, sub-phase id, confidence, evidence predicates |
| `domain-translator` | one domain packet | formal statement in domain vocabulary |
| `scheme-planner` | all formal statements for a run | ordered plan with dependencies and parallel groups |
| `domain-processor` | one plan step and context | success, failure, wait-state, and knowledge-edge outcomes |
| `applier` | one processed outcome | concrete applied result |
| `result-translator` | one formal result | human-readable fragment |
| `arbiter-selector` | result collection and flow context | arbiters to fire and reasons |
| `arbiter-unit` | one review responsibility and one result | pass, concern, or reprocess |
| `formatter` | approved fragments and audience | final human result |
| `phase-context-compiler` | completed phase predicate outputs | carry-forward predicate summary |
| `transition-gate` | upstream context and downstream source | PASS, PASS WITH GAPS, or BLOCK |
| `topology-intent-classifier` | user topology request | run-flow or mutate-topology intent |

---

## Pair Shape Rule

Every pair uses the logical unit boundary.

```text
A = exact input to ILogicalUnit<TInput,TOutput>
B = exact LogicalUnitResult<TOutput>
Reason = why this output is correct for this input
Phrasing B = same logical case with different surface wording
```

B must be the actual output, not a description of the output.

No pair can be written without an oracle section that defines the expected node
boundary behavior. If the oracle is missing, extend the oracle first.

---

## Held-Out Test Categories

Each sub-phase gets 20 held-out documents in five categories.

| Category | Count | Required behavior |
|---|---:|---|
| Correct flow selection | 4 | correct phase and sub-phase, confidence >= 0.70 |
| Knowledge edge detection | 4 | named gap, no invented solution |
| Cross-sub-phase constraint | 4 | boundary detected, arbiter fires when needed |
| Conflict detection | 4 | conflict blocked or reprocessed, zero formatter leakage |
| Edge cases | 4 | defined per sub-phase |

Conflict detection has zero tolerance. A conflict reaching the formatter fails
the sub-phase regardless of other scores.

Passing threshold per sub-phase:

```text
17/20 held-out documents pass
0/4 conflict documents leak conflict to formatter
```

---

## WDA-Gated Build Sequence

### Gate 0 - WDA-00 Topology Bootstrap

Status: passed.

Proof:

- 7 phases
- 31 sub-phases
- 6 transitions
- full suite 1147/1147

### Gate 1 - WDA-01 Document Classification

Purpose: prove each sub-phase has a classifiable source shape before broad
corpus intake.

Output:

- source document selection ledger
- expected classification trace
- confidence and wrong-domain guard checklist
- `sub-phase-router` gap map

Promotion:

- no broad sub-phase corpus until the sub-phase has a passing WDA-01
  classification probe or a named knowledge edge.

### Gate 2 - WDA-02 Within-Phase Orchestration

Purpose: prove each phase can orchestrate all of its sub-phases in one
realistic complex request.

Promotion:

- no phase corpus wave is promoted until its WDA-02 phase scenario passes.

### Gate 3 - WDA-03 Phase Transition Continuity

Purpose: prove adjacent phase boundaries preserve predicate-level constraints,
decisions, knowledge edges, and rejected items.

Promotion:

- no transition corpus is promoted until the matching WDA-03 transition passes.

### Gate 4 - WDA-04 Learning From Transition Failure

Purpose: prove transition failure produces learning data and improves future
detection without overgeneralization.

Promotion:

- no autonomous phase-level learning pair batch is promoted until this passes.

### Gate 5 - WDA-05 Methodology Variation

Purpose: prove the topology adapts to company-specific process differences
without code changes.

Promotion:

- no methodology-specific corpus enters the main corpus until topology mutation
  and transition gates handle the variant.

### Gate 6 - WDA-06 Full Lifecycle Orchestration

Purpose: prove the orchestrator can manage all seven phases and all six
transitions in one lifecycle request.

Promotion:

- full corpus expansion can proceed only after WDA-06 passes or after the
  failing phase/transition is explicitly scoped out.

---

## Corpus Waves

The waves are reorganized around WDA proof order, not document abundance.

### Wave 0 - Runtime adapters and topology surface

Status: partially complete.

Includes:

- logical contracts
- topology registry
- typed commands
- topology mutation units
- topology mutation flow
- assistant typed command route
- WDA-00 topology bootstrap proof

Remaining:

- `TopologyIntentClassifierUnit`
- natural-language topology mutation route

### Wave 1 - Classification seed for all 31 sub-phases

Documents:

- 1 source document or hand-authored source packet per sub-phase

Output:

- WDA-01 evidence
- initial `sub-phase-router` pairs only where needed

Promotion:

- all 31 probes classified or named as source knowledge edges

### Wave 2 - Mature center phase corpus

Sub-phases:

- 3.1 Architecture and component topology
- 3.3 API contract definition
- 3.4 Security and authorization model
- 4.1 Backend services
- 4.3 Integration and provider connectors

Reason:

These have the strongest overlap with existing project 21-23 and E53 work.

Quota:

- 500 training
- 100 held-out

Promotion:

- WDA-02 Phase 3 passes
- WDA-03 transition 3 -> 4 passes
- held-out score >= 17/20 for each sub-phase

### Wave 3 - Functional-to-implementation bridge

Sub-phases:

- 2.1 User stories and acceptance criteria
- 2.2 Business rules
- 2.3 Integration point specification
- 2.4 Data model boundaries
- 3.2 Data schema and migration design
- 3.5 Infrastructure and deployment topology
- 4.2 Frontend and client rendering
- 4.4 Data migration execution
- 4.5 Documentation

Quota:

- 900 training
- 180 held-out

Promotion:

- WDA-02 Phase 2 and Phase 4 pass
- WDA-03 transitions 2 -> 3 and 4 -> 5 pass

### Wave 4 - Testing and QA

Sub-phases:

- 5.1 Unit and contract tests
- 5.2 Integration tests
- 5.3 End-to-end scenario tests
- 5.4 Performance and load tests
- 6.1 Functional review
- 6.2 Regression coverage verification
- 6.3 Security and compliance review
- 6.4 Accessibility audit

Quota:

- 800 training
- 160 held-out

Promotion:

- WDA-02 Phase 5 and Phase 6 pass
- WDA-03 transition 5 -> 6 passes

### Wave 5 - Design and QA automation

Sub-phases:

- 1.1 Design system and tokens
- 1.2 Screen and layout structure
- 1.3 Interaction model and state transitions
- 1.4 Accessibility and compliance
- 1.5 Motion and animation
- 7.1 Test scenario automation
- 7.2 Regression suite maintenance
- 7.3 CI/CD gate configuration
- 7.4 Coverage and reporting

Quota:

- 900 training
- 180 held-out

Promotion:

- WDA-02 Phase 1 and Phase 7 pass
- WDA-03 transitions 1 -> 2 and 6 -> 7 pass

### Wave 6 - Cross-phase and methodology variation

Documents:

- 60 transition held-out documents
- methodology variation examples
- phase-context-compiler pairs
- transition-gate pairs
- topology-intent-classifier pairs

Promotion:

- WDA-04 passes
- WDA-05 passes
- WDA-06 passes

---

## Source Ledger Fields

Every corpus document ledger entry must contain:

```text
document_id
source_path_or_url
source_tier
license_or_authority
primary_phase_id
primary_sub_phase_id
stratum
document_type
three_verifiable_claims
known_failure_mode
expected_top_predicates
expected_knowledge_edges
expected_conflicts
train_or_held_out
promotion_status
```

No document counts toward quota until all fields are populated.

---

## Batch Artifact Contract

Every corpus batch produces:

1. source ledger delta
2. training pair files organized by logical unit role
3. held-out test manifest entries
4. conformance suite entries
5. coverage ledger update
6. round manifest
7. round zip
8. next action item

The index update happens after every action: correction, gap closure, coverage
extension. A pair not listed in the index does not exist for training.

---

## Evaluation Protocol

Evaluation calls the logical assistant or the relevant logical flow surface
against held-out documents.

Inspect:

- selected flow id
- selected phase id
- selected sub-phase id
- confidence
- warnings
- evidence ids
- overall verdict
- final human result
- applied flags

Required defaults:

- `UseCompiledPredicateRepresentation = true`
- `UseDomainScopedRetrieval = true`
- `UseConfidenceFloor = true`
- `UseArbiterAuthority = true`
- `UseKnowledgeEdgeDetection = true`
- `UseInternalFirstExecution = true`
- `RequireExternalAiApproval = true`

---

## Final Corpus Declaration

The corpus expansion is complete only when all of these are true:

1. WDA-00 through WDA-06 pass.
2. All 31 sub-phases pass at least 17/20 held-out documents.
3. All 31 sub-phases pass all conflict documents with zero formatter leakage.
4. All six transitions pass the 60-document transition evaluation.
5. All required logical unit cells are trusted.
6. All active sub-phase flows are registered.
7. A probationary regression suspends dependent flows until retraining.
8. The topology-intent-classifier handles methodology mutation requests or
   returns a clean knowledge edge.
9. The phase-context-compiler and transition-gate preserve predicate-level
   carry-forward.
10. Every corpus batch has source ledger entries, pair files, conformance
    entries, manifest, zip, and next action item.

Until those conditions hold, corpus expansion is in progress, not complete.

---

## Immediate Next Action

Run WDA-01 Round A.

Required artifacts:

1. source document selection ledger for 31 sub-phases
2. expected classification trace
3. confidence and wrong-domain guard checklist
4. executor contract for deterministic classification probes

WDA-01 is the gate that decides which real local documents enter Wave 1 and
which sub-phases must be marked as source knowledge edges.
