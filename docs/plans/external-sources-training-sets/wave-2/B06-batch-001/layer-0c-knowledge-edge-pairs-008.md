# Layer 0-C — Knowledge-Edge Pairs (008)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 008 teaches the three v6 corruption types in B06 from the knowledge-edge perspective.

---

## B06-0C-008-001 — v6-01: DOCUMENT missed on requirements request

```
source_id: xiigen-b06-0c-008-001
source_title: XIIGen B06 knowledge-edge — v6-01 DOCUMENT missed
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-008-001-kedge"
  CompiledText: "Define the use cases for the payment processing feature."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "v6-01-document-missed"

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "v6-01-document-missed"
    matched_predicate_count: "0"
    capability_gap_id: "gap-b06-v6-01-001"
    capability_gap_description: "Request 'Define the use cases for the payment processing
      feature' should have fired DOCUMENT (define→DOCUMENT, use cases→use-case→DOCUMENT)
      but the compiler failed to detect either term. This is v6-01 corruption: DOCUMENT
      missed on a clear requirements-authoring request."
    correct_behavior:
      CompiledPredicateIds: ["DOCUMENT"]
      evidence: ["define", "use cases"]
    actual_behavior:
      CompiledPredicateIds: []
      error: "Both 'define' and 'use cases' (→ use-case) are in DOCUMENT lexicon.
        Failure to match them is a lexicon gap."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  Warnings:
    - "v6-01 corruption detected: DOCUMENT predicate missed"
    - "capability_gap_id: gap-b06-v6-01-001"

---

## B06-0C-008-002 — v6-02: IMPLEMENT suppressed when "write" present

```
source_id: xiigen-b06-0c-008-002
source_title: XIIGen B06 knowledge-edge — v6-02 IMPLEMENT suppressed
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-008-002-kedge"
  CompiledText: "Write the user stories for the order management feature with acceptance criteria."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "knowledge_edge: IMPLEMENT suppressed"
  KnownFailureMode: "v6-02-implement-suppressed"

B:
  FlowId: "universal-nine-node-development-flow-2.1"
  OverallVerdict: "FLOW_EXECUTING_WITH_WARNING"
  Metrics:
    knowledge_edge_reason: "v6-02-implement-suppressed"
    correct_behavior:
      CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
    actual_behavior:
      CompiledPredicateIds: ["DOCUMENT"]
      suppressed: "IMPLEMENT"
      suppression_reason_given: "intent is documentation"
    capability_gap_id: "gap-b06-v6-02-001"
    capability_gap_description: "IMPLEMENT predicate was suppressed because 'write'
      appeared in a documentation context. The compiler must not suppress predicates
      based on intent. 'write' is in IMPLEMENT lexicon and must always fire IMPLEMENT."
    recommended_action: "FLAG_CORRUPTION_V6_02"
  Warnings:
    - "v6-02 corruption: IMPLEMENT suppressed"

---

## B06-0C-008-003 — v6-03: routing collapsed to single sub-phase

```
source_id: xiigen-b06-0c-008-003
source_title: XIIGen B06 knowledge-edge — v6-03 routing collapsed
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-008-003-kedge"
  CompiledText: "Define the use cases and stakeholder requirements for the checkout flow."
  CompiledPredicateIds: ["DOCUMENT"]
  RoutingDecisions_actual: 1
  RoutingDecisions_correct: 2
  SourceStatus: "knowledge_edge: v6-03 routing collapse"
  KnownFailureMode: "v6-03-routing-collapsed"

B:
  FlowId: "universal-nine-node-development-flow-2.1"
  OverallVerdict: "FLOW_EXECUTING_WITH_WARNING"
  Metrics:
    knowledge_edge_reason: "v6-03-routing-collapsed"
    correct_behavior:
      routing_decisions: [{"sub_phase": "2.1"}, {"sub_phase": "2.4"}]
      decision_count: 2
    actual_behavior:
      routing_decisions: [{"sub_phase": "2.1"}]
      decision_count: 1
      missing: "2.4 routing decision"
    capability_gap_id: "gap-b06-v6-03-001"
    capability_gap_description: "Router produced only 2.1 routing decision despite
      'stakeholder' signal present. 2.4 (Stakeholder and External Requirements) was
      not selected. Routing was collapsed."
    recommended_action: "FLAG_CORRUPTION_V6_03"
  Warnings:
    - "v6-03 corruption: routing collapsed from 2 to 1 decision"
    - "missing 2.4 routing decision for stakeholder signal"
