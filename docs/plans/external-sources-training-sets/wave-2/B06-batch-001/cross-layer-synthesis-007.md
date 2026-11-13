# Cross-Layer Synthesis (007)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## Synthesis note

File 007 traces the full layer-by-layer execution for the stakeholder dual-routing case:
"Document the checkout requirements and define the stakeholder acceptance criteria."
This request fires DOCUMENT with a "stakeholder" signal, producing routing to both
sub-phase 2.1 (business requirements) and sub-phase 2.4 (stakeholder acceptance criteria).

---

## Synthesis Trace — DOCUMENT → 2.1 + 2.4 stakeholder dual routing

```
source_id: xiigen-b06-synthesis-007
pair_kind: cross_layer_synthesis
scenario: "StakeholderDualRoute — DOCUMENT → 2.1 + 2.4"
```

### Input
UserRequest: "Document the checkout requirements and define the stakeholder acceptance criteria."
TransitionId: "transition-B06-synthesis-007"

---

### Layer 0-A — Compiler

Layer: 0-A
Unit: PredicateCompilerUnit
Input: the full request above

Scan results:
  "Document" → DOCUMENT (Rule 1: document fires DOCUMENT)
  "the" → not in any lexicon
  "checkout" → not in any lexicon (domain noun)
  "requirements" → DOCUMENT (Rule 3: requirements fires DOCUMENT)
  "and" → not in any lexicon
  "define" → DOCUMENT (Rule 6: define fires DOCUMENT)
  "stakeholder" → DOCUMENT (Rule 16: stakeholder fires DOCUMENT; also triggers 2.4 routing signal)
  "acceptance" → not in any lexicon (part of compound "acceptance criteria" — but "criteria" is not a DOCUMENT term)
  "criteria" → DOCUMENT (Rule D-17: criteria fires DOCUMENT)

DOCUMENT signals: ["Document", "requirements", "define", "stakeholder", "criteria"]
IMPLEMENT signals: [] (none)
Stakeholder flag: true ("stakeholder" detected in request)

Output:
  CompiledPredicateIds: ["DOCUMENT"]
  StakeholderSignal: true
  Confidence: 0.97
  KnowledgeEdge: false

Key teaching:
  DOCUMENT alone fires — no IMPLEMENT, no ARCHITECTURE, no UI.
  "stakeholder" is both a DOCUMENT lexicon term AND a routing signal for sub-phase 2.4.
  Five distinct DOCUMENT terms fire: document, requirements, define, stakeholder, criteria.
  The StakeholderSignal flag is passed through to Layer 0-B to trigger 2.4 routing.

v6-01 risk: Missing "stakeholder" or "criteria" from DOCUMENT terms.
v6-03 risk: StakeholderSignal not propagated → 2.4 routing not triggered.

Reference: layer-0a-compiler-pairs-020.md

---

### Layer 0-B — Router

Layer: 0-B
Unit: PredicateRouterUnit
Input: CompiledPredicateIds: ["DOCUMENT"], StakeholderSignal: true

Routing logic:
  DOCUMENT → 2.1 (primary requirements sub-phase)
  StakeholderSignal true → 2.4 (stakeholder acceptance criteria sub-phase)
  No IMPLEMENT → 4.1 NOT triggered
  No ARCHITECTURE → 3.1 NOT triggered
  No UI → 1.1 NOT triggered
  DecisionCount: 2

Output:
  RoutingDecisions:
    - SubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - SubPhaseId: "2.4"; EvidencePredicates: ["DOCUMENT", "stakeholder_signal"]

CanonicalOrdering: 2.1 before 2.4.
Corruption risk: Routing 2.4 before 2.1 is a v6-03 ordering violation.

Key teaching:
  Two routing decisions from a single predicate (DOCUMENT) because "stakeholder" fires 2.4.
  DecisionCount: 2 despite only one CompiledPredicateId.
  Both decisions carry DOCUMENT as evidence predicate; 2.4 additionally carries stakeholder_signal.

Reference: layer-0b-router-pairs-020.md

---

### Layer 0-D — Flow Factory

Layer: 0-D
Unit: ILogicalFlowFactory

Input:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "2.4"]

Output:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

  Flow_2_4:
    SelectedFlowId: "universal-nine-node-development-flow-2.4"
    FlowAlias: "StakeholderAcceptanceCriteriaFlow24"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

Key teaching:
  Two routing decisions → two flows, both DOCUMENT-rooted.
  ILogicalFlowFactory selects the correct flow for each sub-phase independently.
  Both flows are trusted_learned — no fallback required.

Reference: layer-0d-flow-factory-pairs-020.md

---

### Layer 1 — Transition Orchestrator

Layer: 1
Unit: LogicalTransitionOrchestratorUnit

TransitionType determination:
  DOCUMENT only (no IMPLEMENT, no UI) → documentation
  TransitionType: documentation applies to BOTH 2.1 and 2.4 transitions

Output:
  Transition_2_1:
    TransitionType: "documentation"
    DomainId: "software-development"
    SubPhaseId: "2.1"
    SelectedFlowId: "universal-nine-node-development-flow-2.1"

  Transition_2_4:
    TransitionType: "documentation"
    DomainId: "software-development"
    SubPhaseId: "2.4"
    SelectedFlowId: "universal-nine-node-development-flow-2.4"

Key teaching:
  DOCUMENT alone → documentation TransitionType on BOTH transitions.
  This is the only case where TransitionType: documentation co-exists with DecisionCount: 2.
  DomainId: software-development on both transitions (Rule 17).
  Contrast with DOCUMENT+IMPLEMENT: that also has DecisionCount 2 but uses feature_addition.

Reference: layer-1-transition-orchestrator-pairs-020.md

---

### Layer 2 — Two Concurrent Nine-Node Flows (DOCUMENT only, Stakeholder split)

Flow_2_1 (RequirementsAuthoringFlow21) — Checkout Business Requirements:
  N01 input: SelectedLogicalTransition (TransitionType: documentation, SubPhaseId: 2.1)
  N02: Decompose into checkout business requirements scope
       ("Document the checkout requirements" portion)
  N03: Plan: functional requirements, business rules, acceptance criteria for checkout
  N04: Domain processor — checkout business domain
  N05: Apply requirements authoring rules
  N06: Translate to RequirementsDoc format
  N07: Select arbiter for requirements quality
  N08: Arbiter validates requirement completeness
  N09 Verdict: PASS
  Artifacts: CheckoutRequirementsDoc (functional requirements, business rules)

Flow_2_4 (StakeholderAcceptanceCriteriaFlow24) — Stakeholder Acceptance Criteria:
  N01 input: SelectedLogicalTransition (TransitionType: documentation, SubPhaseId: 2.4)
  N02: Decompose into stakeholder acceptance criteria scope
       ("define the stakeholder acceptance criteria" portion)
  N03: Plan: stakeholder-facing acceptance criteria, sign-off conditions, UAT (User Acceptance Testing) criteria
  N04: Domain processor — stakeholder requirements
  N05: Apply acceptance criteria authoring rules
  N06: Translate to StakeholderAcceptanceCriteriaDoc format
  N07: Select arbiter for stakeholder criteria quality
  N08: Arbiter validates stakeholder criteria completeness and testability
  N09 Verdict: PASS
  Artifacts: StakeholderAcceptanceCriteriaDoc (UAT criteria, sign-off conditions)

Both flows run concurrently. Each flow handles its own portion of the original request.
RequirementsAuthoringFlow21 and StakeholderAcceptanceCriteriaFlow24 are both DOCUMENT flows.

Reference: layer-2-nine-node-2.1-pairs-020.md

---

### Synthesis Summary

Predicates: DOCUMENT (one predicate, with StakeholderSignal: true)
Routing decisions: 2 (→ 2.1 + 2.4)
TransitionType: documentation (both transitions — IMPLEMENT absent)
DomainId: software-development on both transitions (Rule 17)
Flows: 2 concurrent flows (RequirementsAuthoringFlow21 + StakeholderAcceptanceCriteriaFlow24)
N09 verdicts: PASS × 2
Canonical ordering: 2.1 before 2.4

Key teachings:
  One predicate (DOCUMENT) can produce two routing decisions when StakeholderSignal present
  "stakeholder" is both a DOCUMENT lexicon term AND a 2.4 routing signal
  DOCUMENT alone → documentation TransitionType even with DecisionCount: 2
  This is the only scenario where TransitionType: documentation and DecisionCount: 2 co-exist
  DomainId: software-development always (Rule 17)
  ILogicalFlowFactory selects distinct flows for 2.1 and 2.4 independently

Corruption checklist:
  v6-03: Do not suppress 2.4 routing when "stakeholder" appears in request
  v6-03: Do not route 2.4 before 2.1 (canonical ordering violation)
  v6-02: Do not add IMPLEMENT to predicate set (no IMPLEMENT term in request)
  v6-03: Do not use feature_addition TransitionType (IMPLEMENT absent)

---

### Comparison: Stakeholder vs Non-Stakeholder DOCUMENT scenarios

Scenario                              | Predicates | StakeholderSignal | Routing Decisions | TransitionType   | Flows
--------------------------------------|------------|-------------------|-------------------|------------------|------
DOCUMENT alone (no stakeholder)       | DOCUMENT   | false             | 1 (→ 2.1)         | documentation    | 1
DOCUMENT + stakeholder (007)          | DOCUMENT   | true              | 2 (→ 2.1, 2.4)    | documentation    | 2
DOCUMENT + IMPLEMENT (no stakeholder) | DOCUMENT, IMPLEMENT | false    | 2 (→ 2.1, 4.1)    | feature_addition | 2

Key distinction: Stakeholder signal adds a routing decision without adding a predicate.
DecisionCount can exceed predicate count when stakeholder signal is present.
