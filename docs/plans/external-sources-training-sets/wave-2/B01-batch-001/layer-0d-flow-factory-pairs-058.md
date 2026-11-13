# Layer 0-D — Flow Factory Selection Pairs (058)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-058-001 — CommentThread: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-058-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a comment thread component with nested replies and reaction buttons."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-058-002 — CommentThread: DOCUMENT only factory

```
source_id: xiigen-b01-0d-058-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Document comment thread moderation use cases including flagging and escalation workflows."
CompiledPredicateIds: ["DOCUMENT"]

Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
FallbackUsed: false
TransitionType: documentation

---

## B01-0D-058-003 — CommentThread: five-predicate factory

```
source_id: xiigen-b01-0d-058-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build comment thread, implement moderation API, document use cases, define notification schema, write tests."
CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE", "DOCUMENT", "TEST"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[TEST.1]: null → "universal-nine-node-development-flow-test.1" | DETERMINISTIC_GENERIC_SELECTED

Rule: five predicates → five factory invocations.
