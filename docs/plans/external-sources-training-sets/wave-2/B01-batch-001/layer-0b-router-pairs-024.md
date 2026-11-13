# Layer 0-B — Router Pairs (024)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## Complete B01 Routing Table

This file is the authoritative routing reference for B01-batch-001.

### Predicate-to-SubPhase mapping

| Predicate | SubPhaseId | Flow | Phase |
|---|---|---|---|
| UI | 1.1 | ScreenInventoryFlow11 | 1 |
| DOCUMENT | 2.1 | BusinessRequirementsFlow21 | 2 |
| ARCHITECTURE | 3.1 | SystemArchitectureFlow31 | 3 |
| IMPLEMENT | 4.1 | ImplementationFlow41 | 4 |

### DecisionCount table

| CompiledPredicateIds | DecisionCount |
|---|---|
| [] | 1 (knowledge-edge entry) |
| [UI] | 1 |
| [DOCUMENT] | 1 |
| [ARCHITECTURE] | 1 |
| [IMPLEMENT] | 1 |
| [UI, DOCUMENT] | 2 |
| [UI, IMPLEMENT] | 2 |
| [UI, ARCHITECTURE] | 2 |
| [DOCUMENT, IMPLEMENT] | 2 |
| [UI, DOCUMENT, IMPLEMENT] | 3 |
| [UI, ARCHITECTURE, DOCUMENT] | 3 |
| [UI, DOCUMENT, IMPLEMENT, ARCHITECTURE] | 4 |

### Knowledge-edge routing rules

1. CompiledPredicateIds: [] → single entry with KnowledgeEdge: true, SubPhaseId: null
2. No predicate firing → no sub-phase assigned
3. Gap vocabulary (navigation, menu, redesign, etc.) → CompiledPredicateIds stays []
4. Model must NOT fabricate a predicate from context

### EM-9 guard — forbidden RoutingDecision fields

These four fields must NEVER appear in a RoutingDecision output:
  dependency_order | parallel_group | domain_label | is_primary

### B01 routing summary pairs

---

## B01-0B-024-001 — UI-only routing summary

```
source_id: xiigen-b01-0b-024-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

---

## B01-0B-024-002 — DOCUMENT-only routing summary

```
source_id: xiigen-b01-0b-024-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["DOCUMENT"]
B: DecisionCount: 1 | SubPhaseId: "2.1" | KnowledgeEdge: false

---

## B01-0B-024-003 — UI+DOCUMENT routing summary

```
source_id: xiigen-b01-0b-024-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "DOCUMENT"]
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "2.1"] | KnowledgeEdge: false (both)

---

## B01-0B-024-004 — knowledge-edge routing summary

```
source_id: xiigen-b01-0b-024-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
