# Cross-Layer Synthesis (005)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## Synthesis note

File 005 traces the full layer-by-layer execution for:
"Write the subscription billing requirements and implement the billing service."
This is the DOCUMENT + IMPLEMENT dual-predicate scenario for the subscription billing domain.

---

## Synthesis Trace — Two predicates: DOCUMENT + IMPLEMENT

```
source_id: xiigen-b06-synthesis-005
pair_kind: cross_layer_synthesis
scenario: "SubscriptionBillingDualPredicate — DOCUMENT + IMPLEMENT"
```

### Input
UserRequest: "Write the subscription billing requirements and implement the billing service."
TransitionId: "transition-B06-synthesis-005"
Domain subject: subscription billing

---

### Layer 0-A — Compiler

Layer: 0-A
Unit: PredicateCompilerUnit
Input: the full request above

Scan results:
  "Write" → IMPLEMENT (Rule I-01: write fires IMPLEMENT)
  "subscription" → not in any predicate lexicon (domain noun, not a predicate term)
  "billing" → not in any predicate lexicon (domain noun, not a predicate term)
  "requirements" → DOCUMENT (Rule 3: requirements fires DOCUMENT)
  "and" → not in any predicate lexicon
  "implement" → IMPLEMENT (Rule I-04: implement fires IMPLEMENT)
  "the" → not in any predicate lexicon
  "service" → not in any predicate lexicon

DOCUMENT signals: ["requirements"]
IMPLEMENT signals: ["Write", "implement"]

Output:
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Confidence: 0.96
  KnowledgeEdge: false

Key teaching:
  "Write" fires IMPLEMENT even though the immediate object is "requirements" (a DOCUMENT term).
  Both DOCUMENT and IMPLEMENT fire from their respective lexicon triggers.
  Domain nouns (subscription, billing, service) do not fire any predicate.

v6-01 risk: DOCUMENT missed — "requirements" not detected.
v6-02 risk: IMPLEMENT suppressed when both "write requirements" appear together.
Anti-pattern: Treating "write requirements" as DOCUMENT only (suppressing IMPLEMENT from "write").

Reference: layer-0a-compiler-pairs-016.md

---

### Layer 0-B — Router

Layer: 0-B
Unit: PredicateRouterUnit
Input: CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]

Routing logic:
  DOCUMENT → 2.1
  IMPLEMENT → 4.1
  No "stakeholder" → 2.4 NOT triggered
  No "architecture" → 3.1 NOT triggered
  No "UI" → 1.1 NOT triggered
  DecisionCount: 2

Output:
  RoutingDecisions:
    - SubPhaseId: "2.1"; EvidencePredicates: ["DOCUMENT"]
    - SubPhaseId: "4.1"; EvidencePredicates: ["IMPLEMENT"]

CanonicalOrdering: 2.1 before 4.1 (requirements before implementation — mandatory).
Corruption risk: Routing 4.1 before 2.1 is a v6-03 ordering violation.

Reference: layer-0b-router-pairs-016.md

---

### Layer 0-D — Flow Factory

Layer: 0-D
Unit: ILogicalFlowFactory

Input:
  RoutingDecisions: [SubPhaseId: "2.1", SubPhaseId: "4.1"]

Output:
  Flow_2_1:
    SelectedFlowId: "universal-nine-node-development-flow-2.1"
    FlowAlias: "RequirementsAuthoringFlow21"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

  Flow_4_1:
    SelectedFlowId: "universal-nine-node-development-flow-4.1"
    FlowAlias: "ImplementationFlow41"
    TrustState: "trusted_learned"
    FallbackUsed: false
    Warnings: []

Key teaching: ILogicalFlowFactory never returns null. Two routing decisions produce two flows.
Both flows are trusted_learned — no fallback required.

Reference: layer-0d-flow-factory-pairs-016.md

---

### Layer 1 — Transition Orchestrator

Layer: 1
Unit: LogicalTransitionOrchestratorUnit

TransitionType determination:
  IMPLEMENT present → feature_addition (unconditional override)
  DOCUMENT present → would be documentation alone, but IMPLEMENT overrides
  Result: feature_addition

Output:
  Transition_2_1:
    TransitionType: "feature_addition"
    DomainId: "software-development"
    SubPhaseId: "2.1"
    SelectedFlowId: "universal-nine-node-development-flow-2.1"

  Transition_4_1:
    TransitionType: "feature_addition"
    DomainId: "software-development"
    SubPhaseId: "4.1"
    SelectedFlowId: "universal-nine-node-development-flow-4.1"

Key teaching: IMPLEMENT presence forces feature_addition on ALL transitions in the set,
including the 2.1 DOCUMENT transition. Neither transition uses "documentation" here.
DomainId: "software-development" on both transitions (Rule 17: always software-development).

Reference: layer-1-transition-orchestrator-pairs-016.md

---

### Layer 2 — Two Concurrent Nine-Node Flows

Flow_2_1 (RequirementsAuthoringFlow21) — Subscription Billing Requirements:
  N01 input: SelectedLogicalTransition (TransitionType: feature_addition, SubPhaseId: 2.1)
  N02: Decompose into subscription billing requirements scope
  N03: Plan: billing cycle requirements, proration logic, upgrade/downgrade rules, failed payment handling
  N04: Domain processor — subscription billing domain (software-development)
  N05: Apply requirements authoring rules
  N06: Translate output to RequirementsDoc format
  N07: Select arbiter for requirements quality validation
  N08: Arbiter validates completeness and format compliance
  N09 Verdict: PASS
  Artifacts: SubscriptionBillingRequirementsDoc with 4 billing scenarios, 8 acceptance criteria

Flow_4_1 (ImplementationFlow41) — Subscription Billing Service:
  N01 input: SelectedLogicalTransition (TransitionType: feature_addition, SubPhaseId: 4.1)
  N02: Decompose into billing service components
  N03: Plan: BillingEngine, SubscriptionStateManager, PaymentGatewayAdapter, NotificationService
  N04: Domain processor — billing service implementation
  N05: Apply implementation rules
  N06: Translate output to CodeArtifact format
  N07: Select arbiter for implementation quality validation
  N08: Arbiter validates code structure and test coverage
  N09 Verdict: PASS
  Artifacts: BillingService implementation with 4 components, unit tests

Both flows run concurrently. Each flow's nine nodes execute independently.
RequirementsAuthoringFlow21 is domain-agnostic: subscription billing is treated identically
to checkout, fraud detection, or any other DOCUMENT domain.

Reference: layer-2-nine-node-2.1-pairs-016.md

---

### Synthesis Summary

Predicates: DOCUMENT + IMPLEMENT
Routing decisions: 2 (→ 2.1 + 4.1)
TransitionType: feature_addition (IMPLEMENT present — overrides documentation)
DomainId: software-development on both transitions (Rule 17)
Flows: 2 concurrent flows (RequirementsAuthoringFlow21 + ImplementationFlow41)
N09 verdicts: PASS × 2
Canonical ordering: 2.1 before 4.1

Key teachings:
  "Write" fires IMPLEMENT even when the object is a DOCUMENT noun ("requirements")
  DOCUMENT and IMPLEMENT fire independently from their own lexicon signals
  IMPLEMENT overrides TransitionType to feature_addition on ALL transitions
  RequirementsAuthoringFlow21 is domain-agnostic: works for any DOCUMENT domain
  Domain nouns (subscription, billing) add zero predicates and zero routing decisions
  Two predicates → two routing decisions → two concurrent flows

Corruption checklist:
  v6-01: Do not miss DOCUMENT even when "write requirements" appears together
  v6-02: Do not suppress IMPLEMENT from "write" when DOCUMENT also fires
  v6-03: Do not route 4.1 before 2.1 (ordering violation)
  v6-03: Do not use "documentation" TransitionType when IMPLEMENT is present

---

### Comparison with Synthesis 001 (DOCUMENT alone)

Scenario                   | Predicates          | Routing | TransitionType   | Flows
---------------------------|---------------------|---------|------------------|------
DOCUMENT alone (HO-001)    | DOCUMENT            | 1 (2.1) | documentation    | 1
DOCUMENT + IMPLEMENT (005) | DOCUMENT, IMPLEMENT | 2 (2.1, 4.1) | feature_addition | 2

Key difference: "Write ... requirements" adds IMPLEMENT to DOCUMENT alone.
This doubles the routing decisions, adds a second concurrent flow, and changes TransitionType.
