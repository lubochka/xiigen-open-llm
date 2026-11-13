# Layer 0-C — Knowledge-Edge Pairs (012)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 012 is the B06 knowledge edge summary: all teaching points consolidated.

---

## B06-0C-012-001 — B06 knowledge edge teaching summary

```
source_id: xiigen-b06-0c-012-001
source_title: XIIGen B06 knowledge-edge — complete teaching summary
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-012-001-kedge"
  CompiledText: "B06 knowledge edge complete teaching summary"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    b06_knowledge_edge_summary:
      "KE-01: DOCUMENT lexicon (17 terms)":
        terms: "document, spec, specification, requirement, story, criteria,
          describe, define, functional, behavior, use-case, narrative,
          feature-brief, capability, epics, business, stakeholder"
        covered_in_files: ["layer-0c-002", "layer-0c-007"]

      "KE-02: write fires IMPLEMENT in doc context":
        rule: "write user stories → DOCUMENT + IMPLEMENT (both fire)"
        covered_in_files: ["layer-0c-003", "layer-0a-003"]

      "KE-03: one predicate → multiple sub-phases":
        rule: "DOCUMENT → 2.1 AND 2.4 when stakeholder signals present"
        covered_in_files: ["layer-0c-004", "layer-0b-005"]

      "KE-04: phase 2 sub-phase map":
        sub_phases: "2.1 Business Requirements, 2.2 Technical, 2.3 Non-Functional, 2.4 Stakeholder"
        covered_in_files: ["layer-0c-005"]

      "KE-05: business and stakeholder in DOCUMENT lexicon":
        rules: "Rule 7 (business), Rule 8 (stakeholder)"
        covered_in_files: ["layer-0c-006"]

      "KE-06: narrative/feature-brief/capability/epics in DOCUMENT lexicon":
        rules: "Rule 13 (narrative), Rule 14 (feature-brief), Rule 15 (epics), Rule 16 (capability)"
        covered_in_files: ["layer-0c-007"]

      "KE-07: v6 corruption types":
        types: "v6-01 (DOCUMENT missed), v6-02 (IMPLEMENT suppressed), v6-03 (routing collapsed)"
        covered_in_files: ["layer-0c-008", "layer-0a-011", "layer-0b-011"]

      "KE-08: TransitionType rule":
        rule: "feature_addition if IMPLEMENT or UI present; documentation if DOCUMENT/ARCHITECTURE only"
        covered_in_files: ["layer-0c-009"]

      "KE-09: DOCUMENT vs IMPLEMENT boundary":
        rule: "Verb determines predicate; IMPLEMENT verb fires IMPLEMENT on any object"
        covered_in_files: ["layer-0c-010"]

      "KE-10: DomainId always software-development":
        rule: "Rule 17 — DomainId: software-development always"
        note: "B06 is a software-development scenario; all pairs carry DomainId: software-development"

---

## B06-0C-012-002 — B06 knowledge gap cases: what triggers knowledge-edge

```
source_id: xiigen-b06-0c-012-002
source_title: XIIGen B06 knowledge-edge — B06 knowledge gap trigger cases
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-012-002-kedge"
  CompiledText: "B06 knowledge gap trigger cases"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    b06_knowledge_gap_triggers:
      - request: "Our checkout needs to be better"
        predicates: []
        result: "knowledge-edge (no predicate terms)"
        capability_gap_id: "gap-checkout-improvement-vague-001"
      - request: "The checkout process should handle different payment methods gracefully"
        predicates: []
        result: "knowledge-edge (no predicate terms; process, gracefully not in any lexicon)"
        capability_gap_id: "gap-checkout-process-graceful-001"
      - request: "We need to fix the cart"
        predicates: []
        result: "knowledge-edge (fix is not in any predicate lexicon in this context)"
        note: "refactor IS in IMPLEMENT lexicon; fix is not"
      - request: "Improve payment performance"
        predicates: []
        result: "knowledge-edge (improve and performance alone do not fire predicates)"
        note: "performance fires DOCUMENT only in non-functional requirements context
          — but only if it appears with a requirements noun"
    teaching_note: "Knowledge-edge occurs when the user request contains no predicate
      vocabulary. Vague improvement requests, process descriptions without requirements
      vocabulary, and performance observations without SLA (Service-Level Agreement) or
      non-functional requirements framing all produce empty predicate sets."
