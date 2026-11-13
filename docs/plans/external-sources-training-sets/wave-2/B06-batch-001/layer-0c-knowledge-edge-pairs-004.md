# Layer 0-C — Knowledge-Edge Pairs (004)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 004 teaches: one predicate can route to multiple sub-phases.
DOCUMENT → 2.1 and 2.4 when both signals are present.

---

## B06-0C-004-001 — One predicate routing to multiple sub-phases

```
source_id: xiigen-b06-0c-004-001
source_title: XIIGen B06 knowledge-edge — one predicate multiple sub-phase routing
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-004-001-kedge"
  CompiledText: "Define the checkout requirements and document the stakeholder expectations for external partners."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "universal-nine-node-development-flow-2.1"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "1"
    routing_decisions_count: "2"
    routing_explanation:
      predicate: "DOCUMENT"
      sub_phase_signals:
        - signal: "requirements"
          maps_to: "2.1"
        - signal: "stakeholder"
          maps_to: "2.4"
        - signal: "external partners"
          maps_to: "2.4"
      decisions_produced:
        - sub_phase: "2.1"
          evidence: "requirements, define"
        - sub_phase: "2.4"
          evidence: "stakeholder, external partners"
    teaching_note: "One DOCUMENT predicate produces two routing decisions because
      the compiled text contains signals for both 2.1 (requirements) and 2.4
      (stakeholder, external partners). This is B06 Core Rule 11: one predicate
      can route to multiple sub-phases."

---

## B06-0C-004-002 — 2.1 vs 2.4 signal distinction

```
source_id: xiigen-b06-0c-004-002
source_title: XIIGen B06 knowledge-edge — 2.1 vs 2.4 signal distinction
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-004-002-kedge"
  CompiledText: "Describe the stakeholder requirements for the checkout partner integration."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "1"
    routing_decisions_count: "2"
    sub_phase_signal_analysis:
      "stakeholder" : "2.4 primary signal"
      "requirements": "both 2.1 and 2.4 signal (lower discriminating power)"
      "partner"     : "2.4 signal (integration = external partner = 2.4)"
      "describe"    : "DOCUMENT verb only, no sub-phase preference"
    routing_outcome:
      decision_1: "2.1 (requirements) — confidence 0.85"
      decision_2: "2.4 (stakeholder, partner) — confidence 0.92"
    teaching_note: "When 'stakeholder' and 'partner' co-occur, 2.4 is the primary
      signal. 'requirements' also weakly maps to 2.1. Both are produced.
      The 2.4 decision may have higher confidence because it has two
      strong signals (stakeholder + partner) vs one for 2.1."

---

## B06-0C-004-003 — DOCUMENT alone → 2.1 only (no 2.4 signal)

```
source_id: xiigen-b06-0c-004-003
source_title: XIIGen B06 knowledge-edge — DOCUMENT alone single routing
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-004-003-kedge"
  CompiledText: "Define the acceptance criteria for the checkout flow."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "1"
    routing_decisions_count: "1"
    sub_phase_signal_analysis:
      "define"             : "DOCUMENT verb only, no sub-phase preference"
      "acceptance criteria": "2.1 primary signal"
      "checkout flow"      : "domain context, no sub-phase signal"
    routing_outcome:
      decision_1: "2.1 (acceptance criteria) — confidence 0.92"
      no_2.4_signal: "no 'stakeholder' or 'external' terms present"
    teaching_note: "No stakeholder or external partner signals → single routing
      decision 2.1. DecisionCount: 1. This is the default case when DOCUMENT
      fires without 2.4-specific signals."
