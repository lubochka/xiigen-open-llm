# Layer 0-C — Knowledge-Edge Pairs (009)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 009 covers TransitionType for B06: feature_addition (new requirements) vs
documentation (updating existing requirements).

---

## B06-0C-009-001 — TransitionType: feature_addition vs documentation

```
source_id: xiigen-b06-0c-009-001
source_title: XIIGen B06 knowledge-edge — TransitionType distinction
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-009-001-kedge"
  CompiledText: "TransitionType: feature_addition vs documentation for B06"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    transition_type_teaching:
      feature_addition:
        definition: "A new capability, artifact, or requirement is being created that
          does not yet exist. The primary signal is 'new' context — creating new use
          cases, writing new requirements, adding new acceptance criteria."
        signals:
          - IMPLEMENT predicate present → feature_addition always
          - UI predicate present → feature_addition (new UX work)
          - "add", "create", "new" verbs in request
          - Request references a feature not previously documented
        b06_examples:
          - "Write the user stories for the order management feature" → feature_addition
            (write fires IMPLEMENT)
          - "Define the requirements for a new checkout flow" → feature_addition
            if IMPLEMENT also fires; documentation if DOCUMENT only
      documentation:
        definition: "An existing capability is being described or documented. The output
          is a documentation artifact but no new code or feature is being created.
          The primary signal is DOCUMENT-only with no IMPLEMENT or UI predicates."
        signals:
          - Only DOCUMENT predicate (no IMPLEMENT, no UI)
          - "describe", "document", "specify", "define" verbs
          - Request references existing functionality
        b06_examples:
          - "Describe the checkout flow requirements" → documentation
          - "Define the acceptance criteria" → documentation (define is DOCUMENT-only)
          - "Specify the use cases" → documentation

---

## B06-0C-009-002 — TransitionType assignment rule

```
source_id: xiigen-b06-0c-009-002
source_title: XIIGen B06 knowledge-edge — TransitionType assignment rule
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-009-002-kedge"
  CompiledText: "TransitionType assignment rule"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    transition_type_rule:
      rule: "TransitionType is feature_addition if IMPLEMENT or UI is present in
        CompiledPredicateIds. TransitionType is documentation if only DOCUMENT
        and/or ARCHITECTURE are present."
      table:
        - predicates: ["DOCUMENT"]
          transition_type: "documentation"
        - predicates: ["DOCUMENT", "ARCHITECTURE"]
          transition_type: "documentation"
        - predicates: ["DOCUMENT", "IMPLEMENT"]
          transition_type: "feature_addition"
        - predicates: ["DOCUMENT", "UI"]
          transition_type: "feature_addition"
        - predicates: ["DOCUMENT", "IMPLEMENT", "UI"]
          transition_type: "feature_addition"
        - predicates: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "UI"]
          transition_type: "feature_addition"
      reason: "IMPLEMENT signals that code is being written — this is feature
        addition work. UI signals new UX work is being created. Both imply
        new functionality is being added, not just documented."

---

## B06-0C-009-003 — TransitionType: feature_addition for new requirements

```
source_id: xiigen-b06-0c-009-003
source_title: XIIGen B06 knowledge-edge — feature_addition for new requirements artifact
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-009-003-kedge"
  CompiledText: "Write the requirements for the new guest checkout feature."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    transition_type: "feature_addition"
    transition_classification_basis:
      IMPLEMENT_present: true
      signal_verbs: ["write"]
      primary_intent: "create"
      new_artifact_signal: "new guest checkout feature"
    teaching_note: "TransitionType is feature_addition because IMPLEMENT is present
      (from 'write'). Even though the primary output is a requirements document,
      the IMPLEMENT predicate changes the transition type to feature_addition.
      This is because 'write' signals code production work alongside documentation."
