# Layer 0-B — Router Pairs (048)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-048-001 — UI+SECURITY: UI+IMPLEMENT+SECURITY routes to 1.1 + 4.1 + SEC.1

```
source_id: xiigen-b01-0b-048-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "SECURITY"]
   User request: "Build a login form component with input validation, XSS protection, and CSRF token integration."
B: DecisionCount: 3 | SubPhaseIds: ["1.1", "4.1", "SEC.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (3) == len(CompiledPredicateIds) (3). Verified.

---

## B01-0B-048-002 — UI+SECURITY: UI+IMPLEMENT+SECURITY routes to 1.1 + 4.1 + SEC.1

```
source_id: xiigen-b01-0b-048-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "SECURITY"]
   User request: "Create a file upload widget with client-side type validation, file size limits, and malware scan integration."
B: DecisionCount: 3 | SubPhaseIds: ["1.1", "4.1", "SEC.1"] | KnowledgeEdge: false (all)

---

## B01-0B-048-003 — UI+SECURITY: UI+DOCUMENT+SECURITY routes to 1.1 + 2.1 + SEC.1

```
source_id: xiigen-b01-0b-048-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "DOCUMENT", "SECURITY"]
   User request: "Document security requirements for all form components including input sanitization and authentication token handling."
B: DecisionCount: 3 | SubPhaseIds: ["1.1", "2.1", "SEC.1"] | KnowledgeEdge: false (all)

---

## B01-0B-048-004 — UI+SECURITY: six-predicate sweep

```
source_id: xiigen-b01-0b-048-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE", "DOCUMENT", "SECURITY", "TEST"]
   User request: "Build secure component library, define auth API schema, document security requirements, write penetration tests."
B: DecisionCount: 6 | SubPhaseIds: ["1.1", "4.1", "3.1", "2.1", "SEC.1", "TEST.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (6) == len(CompiledPredicateIds) (6). Verified.
