# Layer 0-C — Knowledge-Edge Pairs (003)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 003 teaches why "write user stories" is dual-predicate: "write" fires IMPLEMENT
regardless of whether the object is code or documentation.

---

## B06-0C-003-001 — "write" fires IMPLEMENT in documentation context

```
source_id: xiigen-b06-0c-003-001
source_title: XIIGen B06 knowledge-edge — write fires IMPLEMENT in doc context
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-003-001-kedge"
  CompiledText: "Write the user stories for the order management feature."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "universal-nine-node-development-flow-2.1"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "2"
    capability_gap_id: "null"
    dual_predicate_explanation:
      predicate_1: "DOCUMENT"
      predicate_1_evidence: ["story", "from 'user stories'"]
      predicate_1_route: "2.1"
      predicate_2: "IMPLEMENT"
      predicate_2_evidence: ["write"]
      predicate_2_route: "4.1"
      intent_suppression: false
      lexicon_based: true
    teaching_note: "The compiler fires IMPLEMENT on 'write' regardless of intent.
      'write the user stories' is documentation work — but the IMPLEMENT predicate
      fires because 'write' is in the IMPLEMENT lexicon. Intent-based suppression
      of IMPLEMENT would be v6-02 corruption. Both predicates fire."
  EvidenceIds: ["B06-0c-003-001-kedge"]
  Warnings: []

---

## B06-0C-003-002 — Contrast: "describe" does NOT fire IMPLEMENT

```
source_id: xiigen-b06-0c-003-002
source_title: XIIGen B06 knowledge-edge — describe vs write IMPLEMENT boundary
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-003-002-kedge"
  CompiledText: "Describe the user stories for the order management feature."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "universal-nine-node-development-flow-2.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "1"
    dual_predicate_explanation:
      predicate_1: "DOCUMENT"
      predicate_1_evidence: ["describe", "story"]
      predicate_1_route: "2.1"
      predicate_2: null
      predicate_2_note: "'describe' is in DOCUMENT lexicon only; IMPLEMENT does not fire"
    teaching_note: "Replacing 'write' with 'describe' removes the IMPLEMENT predicate.
      'describe' is in DOCUMENT lexicon but not IMPLEMENT lexicon. Result: single
      predicate DOCUMENT, single routing decision 2.1."

---

## B06-0C-003-003 — Intent-based suppression is the v6-02 corruption

```
source_id: xiigen-b06-0c-003-003
source_title: XIIGen B06 knowledge-edge — intent suppression is v6-02 corruption
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-003-003-kedge"
  CompiledText: "Write the requirements specification for the checkout service."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "2"
    correct_behavior:
      description: "Both DOCUMENT and IMPLEMENT fire. Router produces 2.1 and 4.1 decisions."
      CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
    incorrect_behavior_v6_02:
      description: "Model observes 'write the requirements specification' and reasons
        the intent is documentation, suppressing IMPLEMENT. Only DOCUMENT fires."
      CompiledPredicateIds_wrong: ["DOCUMENT"]
      corruption_type: "v6-02"
    teaching_note: "The compiler does not evaluate intent. It fires predicates on term
      presence. 'write' is in IMPLEMENT lexicon. 'requirements' and 'specification'
      are in DOCUMENT lexicon. Both fire. The router produces decisions for both 2.1
      and 4.1. Suppressing IMPLEMENT based on documentation context is always wrong."
