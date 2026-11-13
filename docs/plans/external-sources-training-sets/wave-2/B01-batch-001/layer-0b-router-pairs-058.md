# Layer 0-B — Router Pairs (058)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-058-001 — CommentThread: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-058-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a comment thread component with nested replies and reaction buttons."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-058-002 — CommentThread: UI+IMPLEMENT (widget+input) routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-058-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a threaded comment widget with rich text input and mention support."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-058-003 — CommentThread: DOCUMENT only routes to 2.1

```
source_id: xiigen-b01-0b-058-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["DOCUMENT"]
   User request: "Document comment thread moderation use cases including flagging, hiding, and escalation workflows."
B: DecisionCount: 1 | SubPhaseId: "2.1" | KnowledgeEdge: false

---

## B01-0B-058-004 — CommentThread: five-predicate sweep routes to 1.1 + 4.1 + 3.1 + 2.1 + TEST.1

```
source_id: xiigen-b01-0b-058-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE", "DOCUMENT", "TEST"]
   User request: "Build comment thread component, implement moderation API, document use cases, define notification schema, write integration tests."
B: DecisionCount: 5 | SubPhaseIds: ["1.1", "4.1", "3.1", "2.1", "TEST.1"] | KnowledgeEdge: false (all)

v6-03 guard: DecisionCount (5) == len(CompiledPredicateIds) (5). Verified.
