# Layer 0-D — Flow Factory Pairs (011)
# Batch: B32-batch-001 | Scenario: B32 — Vibe coding — exploratory, prototype, "just make it work"
# Production date: 2026-05-05 | pair_kind: layer-0d-flow-factory-pairs

---

## B32-0D-011-001 — IMPLEMENT+DOCUMENT factory in vibe context

**Predicates:** `["IMPLEMENT", "DOCUMENT"]`

**Source request:** "Vibe code the notification service and document the flow — nothing fancy."

**Gap warnings:** gap-vibe-coding-style-001, gap-vibe-coding-nothing-fancy-001

**Factory selection:**

```
Factories selected: [BackendServiceFlow41, DocumentFlow]
Reason for BackendServiceFlow41: "code" root fires IMPLEMENT
Reason for DocumentFlow: "document" fires DOCUMENT
GapMetadata: {vibe-style: true, nothing-fancy: true}
ParallelEligible: conditional (DocumentFlow may depend on BackendServiceFlow41 output)
TransitionType: feature_addition
DomainId: "software-development"
```

---

## B32-0D-011-002 — IMPLEMENT+DOCUMENT execution order

**Ordering consideration:**

If documentation describes the implementation (i.e., documents what is built), then:
- Phase 1: BackendServiceFlow41 (build the service)
- Phase 2: DocumentFlow (document the built service)

If documentation is specification-style (documents what will be built), then:
- Phase 1 (parallel): BackendServiceFlow41 + DocumentFlow
- Both proceed from the request context

In B32 vibe context, documentation is typically post-implementation, so sequential ordering applies.

---

## B32-0D-011-003 — IMPLEMENT+TEST factory

**Predicates:** `["IMPLEMENT", "TEST"]`

**Source request:** "Quick and dirty — implement the cache service and write basic unit tests."

**Gap warnings:** gap-vibe-coding-quick-dirty-001

**Factory selection:**

```
Factories selected: [BackendServiceFlow41, TestFlow]
Ordering: BackendServiceFlow41 first, TestFlow after
GapMetadata: {quick-dirty: true}
TransitionType: feature_addition
```

**Note:** TestFlow depends on BackendServiceFlow41 output (tests test the implementation). Sequential ordering applies.

---

## B32-0D-011-004 — Dependency matrix for factory execution

| Factory pair | Parallel eligible? | Reason |
|---|---|---|
| UIFlow + BackendServiceFlow41 | Yes | UI and backend are independent |
| ArchitectureFlow31 + APIContractFlow | Yes | Architecture and contracts are independent |
| BackendServiceFlow41 + TestFlow | No | Tests depend on implementation |
| BackendServiceFlow41 + DocumentFlow | Conditional | Depends on doc type |
| BackendServiceFlow41 + QAFlow | No | QA depends on implementation |
| ArchitectureFlow31 + BackendServiceFlow41 | Conditional | Impl may depend on architecture |

This dependency matrix applies regardless of vibe framing. Gap metadata does not change execution dependencies.

---
