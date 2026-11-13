# Layer 0-C — Knowledge-Edge Pairs (007)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 007 teaches: "narrative" / "feature-brief" / "capability" / "epics" in DOCUMENT lexicon.

---

## B06-0C-007-001 — "narrative" fires DOCUMENT: Rule 13

```
source_id: xiigen-b06-0c-007-001
source_title: XIIGen B06 knowledge-edge — narrative fires DOCUMENT Rule 13
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-007-001-kedge"
  CompiledText: "Write the product narrative for the subscription management feature."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    evidence_document: ["narrative"]
    evidence_implement: ["write"]
    narrative_term_teaching:
      term: "narrative"
      fires_predicate: "DOCUMENT"
      b06_rule: "Rule 13 — narrative fires DOCUMENT"
      rationale: "A narrative is a written description of a feature or product
        capability. It is a documentation artifact — it describes what a feature
        does and why, not how to implement it. 'narrative' is in DOCUMENT lexicon."
      examples:
        - "product narrative" → DOCUMENT
        - "feature narrative" → DOCUMENT
        - "user journey narrative" → DOCUMENT

---

## B06-0C-007-002 — "feature-brief" fires DOCUMENT: Rule 14

```
source_id: xiigen-b06-0c-007-002
source_title: XIIGen B06 knowledge-edge — feature-brief fires DOCUMENT Rule 14
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-007-002-kedge"
  CompiledText: "Define the feature brief for the cart abandonment recovery capability."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    evidence_document: ["feature brief", "define", "capability"]
    feature_brief_term_teaching:
      term: "feature-brief"
      fires_predicate: "DOCUMENT"
      b06_rule: "Rule 14 — feature-brief fires DOCUMENT"
      rationale: "A feature brief is a structured requirements document for a
        specific feature. It is a documentation artifact, not a code artifact.
        The compound term 'feature-brief' (or 'feature brief') maps to the
        DOCUMENT lexicon. 'feature' alone maps to IMPLEMENT lexicon."
      distinction: "'feature brief' (compound) → DOCUMENT; 'feature' (alone) → IMPLEMENT"

---

## B06-0C-007-003 — "epics" fires DOCUMENT: Rule 15

```
source_id: xiigen-b06-0c-007-003
source_title: XIIGen B06 knowledge-edge — epics fires DOCUMENT Rule 15
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-007-003-kedge"
  CompiledText: "Define the epics for the checkout redesign project."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    evidence_document: ["epics", "define"]
    epics_term_teaching:
      term: "epics"
      fires_predicate: "DOCUMENT"
      b06_rule: "Rule 15 — epics fires DOCUMENT"
      rationale: "Epics are high-level requirements groupings in agile methodology.
        Defining epics is requirements authoring work — DOCUMENT territory.
        'epic' (singular) also fires DOCUMENT. Both match 'epics' in lexicon."
      distinction: "'epics' → DOCUMENT (requirements); 'implement the epic' → IMPLEMENT
        (because 'implement' fires IMPLEMENT, not because 'epic' does)"

---

## B06-0C-007-004 — "capability" fires DOCUMENT: Rule 16

```
source_id: xiigen-b06-0c-007-004
source_title: XIIGen B06 knowledge-edge — capability fires DOCUMENT Rule 16
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-007-004-kedge"
  CompiledText: "Document the capability requirements for the checkout service."
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

B:
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    evidence_document: ["document", "capability", "requirements"]
    capability_term_teaching:
      term: "capability"
      fires_predicate: "DOCUMENT"
      b06_rule: "Rule 16 — capability fires DOCUMENT"
      rationale: "'capability' signals requirements authoring — describing what the
        system can do. It is a documentation term. 'capability requirements' is a
        2.1 artifact. 'capability gaps' signals requirements completeness review."
      examples:
        - "capability requirements" → DOCUMENT (2.1)
        - "capability gap analysis" → DOCUMENT (2.1)
        - "system capabilities" → DOCUMENT (2.1)
      distinction: "'build the capability' → IMPLEMENT (build fires IMPLEMENT)"
