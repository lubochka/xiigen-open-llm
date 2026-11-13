# Layer 0-C — Knowledge-Edge Pairs (010)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 010 covers the DOCUMENT vs IMPLEMENT boundary: when is a request
requirements authoring vs code authoring?

---

## B06-0C-010-001 — DOCUMENT vs IMPLEMENT boundary: action verb determines predicate

```
source_id: xiigen-b06-0c-010-001
source_title: XIIGen B06 knowledge-edge — DOCUMENT vs IMPLEMENT boundary
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-010-001-kedge"
  CompiledText: "DOCUMENT vs IMPLEMENT boundary analysis"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    boundary_teaching:
      rule: "The primary determinant of IMPLEMENT vs DOCUMENT is the action verb.
        IMPLEMENT fires on code-action verbs (build, create, implement, write, develop).
        DOCUMENT fires on documentation-action verbs (describe, define, document,
        specify) and on requirements vocabulary nouns (requirements, use-case, story,
        criteria, specification, behavior, narrative, feature-brief, capability, epics,
        business, stakeholder, functional)."
      key_principle: "When IMPLEMENT verb is present, IMPLEMENT fires regardless of
        whether the object is code ('write the service') or documentation ('write the spec').
        The object does NOT determine the predicate — the verb does."
      examples:
        - verb: "write"
          predicate_fired: "IMPLEMENT"
          object_1: "the service"
          object_2: "the specification"
          note: "IMPLEMENT fires for both objects"
        - verb: "build"
          predicate_fired: "IMPLEMENT"
          object_1: "the checkout module"
          object_2: "the requirements doc"
          note: "IMPLEMENT fires for both objects"
        - verb: "describe"
          predicate_fired: "DOCUMENT"
          object_1: "the system"
          object_2: "the requirements"
          note: "DOCUMENT fires only; IMPLEMENT does not fire"
        - verb: "define"
          predicate_fired: "DOCUMENT"
          note: "DOCUMENT fires only; IMPLEMENT does not fire"

---

## B06-0C-010-002 — Requirements authoring vs code authoring: dual-predicate cases

```
source_id: xiigen-b06-0c-010-002
source_title: XIIGen B06 knowledge-edge — requirements authoring vs code authoring
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-010-002-kedge"
  CompiledText: "Requirements authoring vs code authoring: when both apply"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    dual_predicate_cases:
      - request: "write the user stories"
        type: "requirements authoring (writing stories) but 'write' fires IMPLEMENT"
        predicates: ["DOCUMENT", "IMPLEMENT"]
        routing: "2.1 + 4.1"
        note: "Dual predicate even though the work is requirements authoring"
      - request: "build the checkout service"
        type: "code authoring only"
        predicates: ["IMPLEMENT"]
        routing: "4.1"
        note: "Single IMPLEMENT predicate"
      - request: "describe the checkout service"
        type: "requirements documentation only"
        predicates: ["DOCUMENT"]
        routing: "2.1"
        note: "Single DOCUMENT predicate"
      - request: "write and implement the checkout service"
        type: "both code authoring and documentation"
        predicates: ["IMPLEMENT"]
        routing: "4.1"
        note: "No DOCUMENT terms present despite 'write' appearing"
      important_note: "The dual-predicate case in B06 is specifically when a
        DOCUMENT vocabulary noun appears alongside a IMPLEMENT verb. Without a
        DOCUMENT noun (story, spec, requirements, etc.), 'write' alone only fires
        IMPLEMENT."

---

## B06-0C-010-003 — DOCUMENT noun + IMPLEMENT verb = dual predicate table

```
source_id: xiigen-b06-0c-010-003
source_title: XIIGen B06 knowledge-edge — DOCUMENT noun + IMPLEMENT verb table
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-010-003-kedge"
  CompiledText: "DOCUMENT noun + IMPLEMENT verb combination table"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    doc_noun_impl_verb_table:
      columns: ["IMPLEMENT verb", "DOCUMENT noun", "Predicates fired"]
      rows:
        - ["write", "user stories", "DOCUMENT + IMPLEMENT"]
        - ["write", "specification", "DOCUMENT + IMPLEMENT"]
        - ["write", "requirements", "DOCUMENT + IMPLEMENT"]
        - ["write", "feature brief", "DOCUMENT + IMPLEMENT"]
        - ["write", "use cases", "DOCUMENT + IMPLEMENT"]
        - ["create", "specification", "DOCUMENT + IMPLEMENT"]
        - ["create", "feature brief", "DOCUMENT + IMPLEMENT"]
        - ["build", "requirements document", "DOCUMENT + IMPLEMENT"]
        - ["describe", "requirements", "DOCUMENT only"]
        - ["define", "requirements", "DOCUMENT only"]
        - ["document", "requirements", "DOCUMENT only"]
        - ["specify", "requirements", "DOCUMENT only"]
      summary: "When IMPLEMENT verb appears alongside DOCUMENT noun, both predicates
        fire. When only DOCUMENT verb appears, only DOCUMENT fires."
