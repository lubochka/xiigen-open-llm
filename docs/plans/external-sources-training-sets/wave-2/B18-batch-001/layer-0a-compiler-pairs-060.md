# Layer 0-A — Compiler Pairs (060)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-060-001 — Complete B18 compiler reference: 25 must-know rules

**The 25 must-know compiler rules for B18 batch 001-060:**

1. "write migration" → IMPLEMENT + feature_addition + 4.4 (Rule 2, absolute)
2. "migrate data/records/rows" → IMPLEMENT + feature_addition + 4.4 (Rule 5)
3. "seed/populate/fixture" → IMPLEMENT + feature_addition + 4.4 (Rule 6)
4. "structure" (noun) → ARCHITECTURE (Rule 3, absolute; never suppressed)
5. "architecture" (noun) → ARCHITECTURE (Rule 7, absolute)
6. "pattern" (noun) → ARCHITECTURE (Rule 7, absolute)
7. "design" (verb) → ARCHITECTURE (Rule 11)
8. "refactor/consolidate/restructure/rename" → TransitionType: refactoring (Rule 9, absolute)
9. "PII" → SECURITY (Rule 14, absolute)
10. "GDPR" → SECURITY (Rule 14, absolute)
11. "encrypted/encrypt" → SECURITY (Rule 14, absolute)
12. "sensitive data" → SECURITY (Rule 14, absolute)
13. "credentials" → SECURITY (Rule 14, absolute)
14. "authentication/authorization/access control" → SECURITY (Rule 14, absolute)
15. "anonymize/mask" → SECURITY (Rule 14: PII handling vocabulary)
16. "row-level security/TLS" → SECURITY (Rule 14: security tokens)
17. "write tests/add tests/create tests" → TEST (Rule 12: verb form only)
18. "tests" (noun), "testing" (noun), "verify" alone → NOT TEST
19. "document/annotate/spec out/write runbook" → DOCUMENT (Rule 4)
20. "QA" abbreviation → QA predicate (always last in execution order)
21. All predicates collapse to set: no duplicates in output predicate list
22. ARCHITECTURE always runs before IMPLEMENT (execution order, absolute)
23. SECURITY and TEST are parallel-eligible after IMPLEMENT completes
24. DOCUMENT is always parallel-eligible
25. DomainId is always "software-development" in all B18 scenarios

---

## B18-0A-060-002 — Complete B18 compiler reference: 10 most common error cases

**The 10 most common compiler error cases in B18 001-060:**

| Error | Trigger | Correct output |
|---|---|---|
| 1. Misrouting "migrate data" to 4.1 | "migrate" present | sub-phase: 4.4 (DataMigrationFlow44) |
| 2. Missing ARCHITECTURE on "structure" | "structure" present | ARCHITECTURE added |
| 3. feature_addition despite refactoring verb | "restructure/consolidate/rename" present | TransitionType: refactoring |
| 4. Missing SECURITY on "PII" | "PII" present | SECURITY added |
| 5. TEST fires from "verify" alone | "verify" without "write tests" | No TEST predicate |
| 6. TEST fires from "testing" noun | "integration testing" noun | No TEST predicate |
| 7. "structured" fires ARCHITECTURE | "structured" adjective | No ARCHITECTURE predicate |
| 8. Missing SECURITY on "anonymize" | "anonymize" verb | SECURITY added |
| 9. "archive" fires SECURITY without PII | "archive" without PII/GDPR/encrypted | No SECURITY predicate |
| 10. DOCUMENT fires on "document table" | "document" as DB object noun | No DOCUMENT predicate |

---

## B18-0A-060-003 — Complete B18 compiler reference: predicate priority rules

**When conflicting signals appear in the same request:**

| Signal A | Signal B | Resolution |
|---|---|---|
| "migrate data" (feature_addition signal) | "refactor" (refactoring signal) | TransitionType: refactoring wins (Rule 9, absolute) |
| "create index" (feature_addition) | "consolidate indexes" (refactoring) | TransitionType: refactoring wins |
| multiple ARCHITECTURE triggers | single occurrence | One ARCHITECTURE predicate |
| multiple SECURITY triggers | single occurrence | One SECURITY predicate |
| "design" (ARCHITECTURE verb) | "structure" (ARCHITECTURE noun) | One ARCHITECTURE predicate |
| ARCHITECTURE fires | IMPLEMENT fires | Both predicates present; ARCHITECTURE precedes IMPLEMENT in execution |
| QA present | other predicates | QA is last in execution plan; always |

---

## B18-0A-060-004 — Complete B18 compiler reference: DomainId and GapHandler rules

**DomainId rules:**
- Always "software-development" in all B18 scenarios
- Set at L0-A; never changed through L2 nine-node
- N09 always outputs DomainId: "software-development"

**GapHandler rules:**
- B18 has NO GapHandler flow
- GapHandler: none in all B18 output
- No vocabulary triggers a gap-handling flow in B18
- Gap-handling flows exist in other scenarios (not B18)

---

## B18-0A-060-005 — Complete B18 compiler reference: 037-060 new domain summary

**All new domains added in files 037-060:**

| Files | Domains added | Primary predicates |
|---|---|---|
| 037-042 | DataAnonymization, PIIMasking, DataArchival, ShardingMigration, ReplicationSetup, ReadReplicaPromotion, FullTextSearchIndex, GeoIndex, CompositeIndex, CoveringIndex, PartialIndex | IMPLEMENT + SECURITY (PII domains); IMPLEMENT alone (DDL domains) |
| 043-048 | CrossDatabaseMigration, NoSQLToSQL, EventSourcingSchema, CQRSProjection, AuditTableSetup, SoftDeleteMigration | IMPLEMENT alone or with ARCHITECTURE/SECURITY |
| 049-054 | MultiTenantSchema, SchemaVersioning, SeedDataMigration, TestDataMigration, RollbackScript (continued) | Various predicate combinations |
| 055-060 | Complete reference files: trigger maps, Gate B types, v6 corruption reference, rule summaries | Reference material |

**Total B18 4.4 domains: 36 (001-036 from earlier phases + 37-048 new + 049-060 new)**
