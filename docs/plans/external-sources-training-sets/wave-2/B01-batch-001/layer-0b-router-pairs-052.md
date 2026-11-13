# Layer 0-B — Router Pairs (052)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-052-001 — FileUploadWidget: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-052-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a file upload widget with drag-and-drop zone, progress indicator, and multi-file support."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-052-002 — FileUploadWidget: UI+IMPLEMENT (via "form") routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-052-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a file upload form with client-side type validation and error feedback."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-052-003 — FileUploadWidget: ARCHITECTURE only routes to 3.1

```
source_id: xiigen-b01-0b-052-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["ARCHITECTURE"]
   User request: "Define the file upload service API schema including endpoint, file size limits, and supported MIME types."
B: DecisionCount: 1 | SubPhaseId: "3.1" | KnowledgeEdge: false

---

## B01-0B-052-004 — FileUploadWidget: five-predicate sweep routes to 1.1 + 4.1 + SEC.1 + 2.1 + 3.1

```
source_id: xiigen-b01-0b-052-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "SECURITY", "DOCUMENT", "ARCHITECTURE"]
   User request: "Build file upload widget, implement virus scanning, document upload requirements, define storage API."
B: DecisionCount: 5 | SubPhaseIds: ["1.1", "4.1", "SEC.1", "2.1", "3.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (5) == len(CompiledPredicateIds) (5). Verified.
