# Layer 0-B — Router Pairs (057)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-057-001 — Advanced: flow count determination

**Flow count rules for B18 037-060:**

| Predicate count | Max flows | Notes |
|---|---|---|
| 1 predicate | 1 flow | Single flow only |
| 2 predicates (IMPL + one) | 2 flows | Depends on combination |
| 3 predicates | 3 flows | All three activated |
| 4 predicates | 4 flows | All four activated |
| 5 predicates | 5 flows | Full five-flow stack |
| [IMPL, DOCUMENT] | 2 flows (parallel) | Parallel = 2 flows, not 1 |
| [SEC, TEST] after IMPL | 2 parallel flows | SecurityReviewFlow + UnitTestingFlow51 |

---

## B18-0B-057-002 — Advanced: execution schedule metadata output

**N01 execution metadata template for multi-flow scenarios:**

```
N01ExecutionMetadata {
  SelectedLogicalTransition: "<predicate combination>",
  FlowCount: <integer>,
  ExecutionOrder: ["FlowA", "FlowB → FlowC ‖ FlowD"],
  ParallelPairs: [["FlowX", "FlowY"]],   // present when parallel flows exist
  GateBCount: <integer>,                 // one Gate B per flow
  TransitionType: "feature_addition" | "refactoring",
  DomainId: "software-development"
}
```

---

## B18-0B-057-003 — Advanced: GateBCount = FlowCount

**Gate B count rule:**
- Every flow has exactly one Gate B evaluation
- FlowCount == GateBCount in all B18 scenarios
- Five flows = five Gate B evaluations
- Parallel flows each have their own independent Gate B condition
- All Gate B conditions must PASS for N08 to return PASS

**Examples:**
- DataMigrationFlow44 only → 1 Gate B
- ArchitectureFlow31 + DataMigrationFlow44 → 2 Gate B evaluations
- ArchitectureFlow31 + DataMigrationFlow44 + SecurityReviewFlow ‖ UnitTestingFlow51 + DocumentationFlow21 → 5 Gate B evaluations

---

## B18-0B-057-004 — Advanced: Gate B FAIL in multi-flow scenarios

**Gate B FAIL behavior in multi-flow scenarios:**

| Failing flow | Impact |
|---|---|
| ArchitectureFlow31 fails | Execution stops; DataMigrationFlow44 does NOT start |
| DataMigrationFlow44 fails | Rollback executed; SecurityReviewFlow and TEST do NOT start |
| SecurityReviewFlow fails | Migration data reverted (if possible); factory_mode = probationary_rejected |
| UnitTestingFlow51 fails | factory_mode = probationary_rejected |
| DocumentationFlow21 fails | Overall N08 FAIL; docs incomplete; other flows not rolled back |

**Key rule:** In sequential flows, a FAIL stops subsequent flows. In parallel flows, both run to completion; both results evaluated in N08.

---

## B18-0B-057-005 — Advanced: router output for SECURITY-mandatory scenarios

**SECURITY-mandatory routing output format:**

When SECURITY predicate fires alongside IMPLEMENT, the N01 SelectedLogicalTransition must include SecurityReviewFlow in the execution plan. This is mandatory. Omitting SecurityReviewFlow when SECURITY predicate is present is a routing error (equivalent to v6-01 suppression but for SECURITY).

**Correct output template (IMPLEMENT + SECURITY):**
```
RouterOutput {
  Flows: ["DataMigrationFlow44", "SecurityReviewFlow"],
  ExecutionOrder: "DataMigrationFlow44 → SecurityReviewFlow",
  SelectedLogicalTransition: "IMPLEMENT+SECURITY"
}
```
