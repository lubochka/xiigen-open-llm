# Layer 0-B — Router Pairs (048)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: router

## B18-0B-048-001 — Complete router reference: 10 must-know routing rules for 037-060

**The 10 must-know routing rules for B18 037-060 domains:**

1. Any database DDL artifact (index, table, column, constraint) → DataMigrationFlow44 (4.4), NOT BackendServiceFlow41 (4.1)
2. ARCHITECTURE fires before IMPLEMENT when both present (absolute ordering)
3. SECURITY fires after IMPLEMENT when both present (absolute ordering)
4. TEST fires after IMPLEMENT when both present (absolute ordering)
5. SECURITY and TEST are parallel-eligible after IMPLEMENT completes
6. DOCUMENT is always parallel-eligible (never blocking)
7. QA is always last (when QA predicate present)
8. "migrate from X to Y database" (X ≠ API) → DataMigrationFlow44 (not ApiContractFlow43)
9. "seed/fixture/populate" → DataMigrationFlow44 (not UnitTestingFlow51)
10. "rollback script" → DataMigrationFlow44 (migration artifact)

---

## B18-0B-048-002 — Complete router reference: flow activation conditions

**Flow activation conditions (B18 037-060):**

| Flow | Activates when | Never activates when |
|---|---|---|
| DataMigrationFlow44 | IMPLEMENT + 4.4 vocabulary | IMPLEMENT + 4.1 vocabulary only |
| ArchitectureFlow31 | ARCHITECTURE predicate present | ARCHITECTURE absent |
| SecurityReviewFlow | SECURITY predicate present | SECURITY absent |
| UnitTestingFlow51 | TEST predicate present | TEST absent |
| DocumentationFlow21 | DOCUMENT predicate present | DOCUMENT absent |
| QA flow | "QA" abbreviation present | QA absent |
| BackendServiceFlow41 | IMPLEMENT + 4.1 vocabulary | 4.4 vocabulary present |
| ApiContractFlow43 | IMPLEMENT + 4.3 vocabulary | 4.4 or 4.1 vocabulary present |

---

## B18-0B-048-003 — Router reference: three-flow execution patterns

**Common three-flow execution patterns in B18 037-060:**

| Pattern | Flow sequence | Trigger combination |
|---|---|---|
| Arch+Impl+Sec | ArchitectureFlow31 → DataMigrationFlow44 → SecurityReviewFlow | ARCH + IMPL + SECURITY |
| Arch+Impl+Test | ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51 | ARCH + IMPL + TEST |
| Impl+Sec+Test | DataMigrationFlow44 → SecurityReviewFlow ‖ UnitTestingFlow51 | IMPL + SECURITY + TEST |
| Impl+Test+Doc | DataMigrationFlow44 → UnitTestingFlow51; DocumentationFlow21 parallel | IMPL + TEST + DOCUMENT |
| Impl+Sec+Doc | DataMigrationFlow44 → SecurityReviewFlow; DocumentationFlow21 parallel | IMPL + SECURITY + DOCUMENT |

---

## B18-0B-048-004 — Router reference: refactoring verb impact on routing

**Impact of refactoring verbs on flow selection:**

| Verb | TransitionType | Flow selected | Notes |
|---|---|---|---|
| "restructure table" (4.4 target) | refactoring | DataMigrationFlow44 | 4.4 vocabulary overrides refactoring destination |
| "refactor migration" (4.4 target) | refactoring | DataMigrationFlow44 | Migration = 4.4 |
| "consolidate migrations" (4.4 target) | refactoring | DataMigrationFlow44 | Consolidate on DB artifacts = 4.4 |
| "refactor UserService" (4.1 target) | refactoring | BackendServiceFlow41 | Code vocabulary = 4.1 |
| "rename column" (4.4 target) | refactoring | DataMigrationFlow44 | Column rename = DDL = 4.4 |

**Key rule:** The sub-phase (4.4 vs 4.1 vs 4.3) is determined by the VOCABULARY (what is being operated on), not the TransitionType. Refactoring on database objects = 4.4.

---

## B18-0B-048-005 — Complete router reference: ARCHITECTURE-only (no IMPLEMENT) routing

**When ARCHITECTURE fires without IMPLEMENT:**

| Scenario | Flow | Output |
|---|---|---|
| "design the anonymization pattern" (no "write migration") | ArchitectureFlow31 only | Design spec output; no migration |
| "design the composite index structure" | ArchitectureFlow31 only | Index design spec |
| "design the multi-tenant schema structure" | ArchitectureFlow31 only | Schema design |
| "design and implement the event schema" | ArchitectureFlow31 → DataMigrationFlow44 | Both predicates; both flows |

**Key rule:** ARCHITECTURE-only requests (design without implement) → ArchitectureFlow31 only. No DataMigrationFlow44. No migration files generated.
