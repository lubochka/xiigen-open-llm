# Layer 0-C — Knowledge-Edge Pairs (002)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 002 covers the DOCUMENT lexicon complete list as a knowledge-edge teaching item.
These pairs teach the model what the complete DOCUMENT lexicon is so it can correctly
identify when a request should or should not fire DOCUMENT.

---

## B06-0C-002-001 — DOCUMENT lexicon: all 17 terms with examples

```
source_id: xiigen-b06-0c-002-001
source_title: XIIGen B06 knowledge-edge — DOCUMENT lexicon complete list
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-002-001-kedge"
  CompiledText: "What terms in the user request fire the DOCUMENT predicate?"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"
  KnownFailureMode: "lexicon_gap"

B:
  FlowId: "knowledge-edge-lexicon-teaching"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    knowledge_edge_reason: "lexicon_gap"
    predicate_id: "DOCUMENT"
    lexicon_terms:
      - term: "document"
        example: "document the requirements"
        fires: true
      - term: "spec"
        example: "write the spec"
        fires: true
      - term: "specification"
        example: "create the specification"
        fires: true
      - term: "requirement"
        example: "define the requirements"
        fires: true
      - term: "story"
        example: "write the user stories"
        fires: true
      - term: "criteria"
        example: "define the acceptance criteria"
        fires: true
      - term: "describe"
        example: "describe the use cases"
        fires: true
      - term: "define"
        example: "define the business rules"
        fires: true
      - term: "functional"
        example: "functional requirements"
        fires: true
      - term: "behavior"
        example: "specify the behavior"
        fires: true
      - term: "use-case"
        example: "use cases for checkout"
        fires: true
      - term: "narrative"
        example: "product narrative"
        fires: true
      - term: "feature-brief"
        example: "feature brief for cart"
        fires: true
      - term: "capability"
        example: "capability requirements"
        fires: true
      - term: "epics"
        example: "define the epics"
        fires: true
      - term: "business"
        example: "business requirements"
        fires: true
      - term: "stakeholder"
        example: "stakeholder expectations"
        fires: true
    total_terms: 17
    capability_gap_id: "null"

---

## B06-0C-002-002 — DOCUMENT non-terms: what does NOT fire DOCUMENT

```
source_id: xiigen-b06-0c-002-002
source_title: XIIGen B06 knowledge-edge — terms that do NOT fire DOCUMENT
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-002-002-kedge"
  CompiledText: "Terms that might seem like DOCUMENT but are not in the lexicon"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"
  KnownFailureMode: "false_positive_risk"

B:
  FlowId: "knowledge-edge-nonterm-teaching"
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    knowledge_edge_reason: "false_positive_risk"
    non_terms_that_look_like_document:
      - term: "process"
        example: "checkout process needs improvement"
        fires_document: false
        note: "process is not in DOCUMENT lexicon"
      - term: "flow"
        example: "the checkout flow"
        fires_document: false
        note: "flow alone is not in DOCUMENT lexicon"
      - term: "scenario"
        example: "payment scenario"
        fires_document: false
        note: "scenario is not in DOCUMENT lexicon"
      - term: "capture"
        example: "capture the business rules"
        fires_document: false
        fires_implement: false
        note: "capture is not in any predicate lexicon"
      - term: "improve"
        example: "improve the checkout"
        fires_document: false
        note: "improve is not in any predicate lexicon → knowledge-edge"
      - term: "better"
        example: "make it better"
        fires_document: false
        note: "better is not in any predicate lexicon → knowledge-edge"
      - term: "user"
        example: "user experience"
        fires_document: false
        note: "user alone does not fire; 'ux' fires UI"
      - term: "feature"
        example: "new feature"
        fires_document: false
        fires_implement: true
        note: "feature fires IMPLEMENT, not DOCUMENT"
