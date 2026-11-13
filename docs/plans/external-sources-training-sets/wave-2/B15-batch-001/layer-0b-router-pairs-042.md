# Layer 0-B — Router Pairs (042)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-042-001 — DataRetentionService: IMPLEMENT routing

A: "Create a DataRetentionService to enforce data lifecycle policies."
B: Flow: ImplementationFlow41 | DecisionCount: 1

---

## B15-router-042-002 — DataRetentionService: IMPLEMENT+SECURITY routing

A: "Create DataRetentionService and encrypt the archived data store."
B: Flow: SecurityAuditFlow → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-042-003 — DataRetentionService: IMPLEMENT+ARCHITECTURE routing

A: "Create DataRetentionService with a data lifecycle domain model."
B: Flow: ArchitectureFlow31 → ImplementationFlow41 | DecisionCount: 2

---

## B15-router-042-004 — DataRetentionService: IMPLEMENT+TEST+DOCUMENT routing

A: "Create DataRetentionService, write retention policy tests, and document the purge rules."
B: Flow: ImplementationFlow41 → TestingFlow51 ‖ DocumentationFlow21 | DecisionCount: 3

---

## B15-router-042-005 — DecisionCount matches predicate count after deduplication

DecisionCount always equals the number of unique predicates after deduplication.
Three predicates → DecisionCount: 3. Two predicates → DecisionCount: 2.
Anti-pattern: DecisionCount: 1 when multiple predicates are present.
