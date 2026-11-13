# Layer 0-C — Knowledge-Edge Pairs (005)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 005 teaches the Phase 2 sub-phase map: 2.1/2.2/2.3/2.4 and their signals.

---

## B06-0C-005-001 — Phase 2 sub-phase map: 2.1/2.2/2.3/2.4

```
source_id: xiigen-b06-0c-005-001
source_title: XIIGen B06 knowledge-edge — phase 2 sub-phase map
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-005-001-kedge"
  CompiledText: "Phase 2 sub-phase map query"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"
  KnownFailureMode: "sub_phase_routing_gap"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    knowledge_edge_reason: "sub_phase_routing_gap"
    phase_2_sub_phase_map:
      "2.1":
        name: "Business Requirements and Use Cases"
        primary_signals: ["requirements", "use-case", "use case", "acceptance criteria",
          "business rules", "functional", "behavior", "story", "user story",
          "feature brief", "narrative", "epics", "capability"]
        flow: "RequirementsAuthoringFlow21"
        owner_predicate: "DOCUMENT"
      "2.2":
        name: "Technical Requirements"
        primary_signals: ["technical requirements", "data model", "API contract",
          "schema", "interface spec", "data structure", "entity", "service contract"]
        flow: "TechnicalRequirementsFlow22"
        owner_predicate: "DOCUMENT"
      "2.3":
        name: "Non-Functional Requirements"
        primary_signals: ["performance", "scalability", "security requirements",
          "non-functional", "SLA", "load", "availability", "reliability",
          "throughput", "latency", "capacity"]
        flow: "NonFunctionalRequirementsFlow23"
        owner_predicate: "DOCUMENT"
      "2.4":
        name: "Stakeholder and External Requirements"
        primary_signals: ["stakeholder", "external", "partner", "compliance",
          "regulatory", "legal", "vendor", "audit", "third-party",
          "external requirements"]
        flow: "StakeholderRequirementsFlow24"
        owner_predicate: "DOCUMENT"

---

## B06-0C-005-002 — B06 is sub-phase 2.1 focused

```
source_id: xiigen-b06-0c-005-002
source_title: XIIGen B06 knowledge-edge — B06 is 2.1 focused
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-005-002-kedge"
  CompiledText: "Checkout flow business requirements scenario — scope"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    teaching_note: "B06 is the training batch for sub-phase 2.1 (Business Requirements
      and Use Cases) with the checkout flow as the domain scenario. B06 also touches
      2.4 (held-out HO-003 and router-pairs-005) but does NOT deeply cover 2.2 or 2.3.
      Requests with technical requirements signals (data model, API contract) would
      route to 2.2. Requests with non-functional signals (performance, availability)
      would route to 2.3. These are outside B06 scope but the model must still produce
      correct routing decisions for them if the signals are present in the compiled text."

---

## B06-0C-005-003 — All four sub-phases share DOCUMENT predicate

```
source_id: xiigen-b06-0c-005-003
source_title: XIIGen B06 knowledge-edge — all phase 2 sub-phases share DOCUMENT
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-005-003-kedge"
  CompiledText: "All phase 2 sub-phases share DOCUMENT predicate — disambiguation"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    teaching_note: "All four Phase 2 sub-phases (2.1, 2.2, 2.3, 2.4) are owned
      by the DOCUMENT predicate. DOCUMENT fires when any Phase 2 requirements
      vocabulary is present. Sub-phase disambiguation happens in the router by
      checking sub-phase signals within the compiled text.

      The compiler does NOT select the sub-phase. The router does.
      The compiler produces: CompiledPredicateIds: ['DOCUMENT']
      The router produces: PredictedSubPhaseId based on signal analysis.

      If the compiled text has mixed signals (e.g., 'functional requirements' for 2.1
      AND 'performance requirements' for 2.3), the router produces routing decisions
      for both 2.1 and 2.3 — both are valid DOCUMENT-predicate sub-phase targets."
