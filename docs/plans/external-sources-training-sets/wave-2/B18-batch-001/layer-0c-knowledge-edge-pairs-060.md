# Layer 0-C — Knowledge-Edge Pairs (060)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-060-001 — Complete knowledge-edge reference: final disambiguation checklist

**37-question disambiguation checklist for B18 037-060 (definitive answers):**

1. "anonymize" verb → SECURITY fires? **Yes**
2. "archive records" alone → SECURITY fires? **No**
3. "GDPR-compliant archive" → SECURITY fires? **Yes**
4. "sharding" alone → ARCHITECTURE fires? **No**
5. "sharded architecture" → ARCHITECTURE fires? **Yes**
6. "replication setup" alone → ARCHITECTURE fires? **No**
7. "replication architecture" → ARCHITECTURE fires? **Yes**
8. "GiST index" → sub-phase? **4.4**
9. "composite index pattern" → ARCHITECTURE fires? **Yes**
10. "covering index" alone → ARCHITECTURE fires? **No**
11. "migrate from MongoDB" → 4.3 or 4.4? **4.4**
12. "CQRS" alone → ARCHITECTURE fires? **No**
13. "CQRS projection pattern" → ARCHITECTURE fires? **Yes**
14. "create event_store" → sub-phase? **4.4**
15. "event sourcing architecture" → ARCHITECTURE fires? **Yes**
16. "audit table" alone → SECURITY fires? **No**
17. "audit table with PII" → SECURITY fires? **Yes**
18. "soft-delete column" alone → SECURITY fires? **No**
19. "soft-delete pattern" → ARCHITECTURE fires? **Yes**
20. "row-level security" → SECURITY fires? **Yes**
21. "multi-tenant" alone → SECURITY fires? **No**
22. "schema_version table" → sub-phase? **4.4**
23. "schema versioning architecture" → ARCHITECTURE fires? **Yes**
24. "seed data migration" → sub-phase? **4.4**
25. "test fixtures" → TEST fires? **No** — 4.4 seed vocabulary
26. "test data migration" → sub-phase? **4.4**
27. "write tests for rollback" → TEST fires? **Yes**
28. "rollback" alone → SECURITY fires? **No**
29. "rollback pattern" → ARCHITECTURE fires? **Yes**
30. "structured" adjective → ARCHITECTURE fires? **No**
31. "data structure" → ARCHITECTURE fires? **Yes**
32. "the design" noun → ARCHITECTURE fires? **No**
33. "design the schema" verb → ARCHITECTURE fires? **Yes**
34. "integration testing" noun → TEST fires? **No**
35. "write integration tests" → TEST fires? **Yes**
36. "document the pattern" → DOCUMENT and ARCHITECTURE both fire? **Yes**
37. "TLS" → SECURITY fires? **Yes**

---

## B18-0C-060-002 — Complete knowledge-edge reference: edge invariant summary

**The B18 knowledge-edge invariants (never change across 001-060):**

1. Predicate edges are token-based, not domain-based
2. Absolute edges (Rules 3, 7, 11, 14) cannot be suppressed by context, domain, or negation
3. Predicate output is always a SET (no duplicates, regardless of trigger count)
4. TransitionType is determined by the most dominant action verb (refactoring verbs win)
5. Sub-phase is determined by vocabulary (database object vs code vs API)
6. Gate B type is determined by operation type (DDL/data migration/seed)
7. GapHandler is always "none" in B18
8. DomainId is always "software-development" in B18

---

## B18-0C-060-003 — Complete knowledge-edge reference: new edge additions (037-060)

**New edges added in 037-060 (not present in 001-036):**

| New edge | Token | Rule |
|---|---|---|
| "anonymize" → IMPLEMENT + SECURITY | "anonymize" | Rules 2 + 14 |
| "mask (PII)" → IMPLEMENT + SECURITY | "mask" | Rules 2 + 14 |
| "row-level security" → SECURITY | "security" in compound | Rule 14 |
| "TLS" → SECURITY | "TLS" = encryption protocol | Rule 14 |
| "promote replica" → IMPLEMENT | "promote" | Rule 2 |
| "backfill" → IMPLEMENT; 4.4 | "backfill" | Rule 5 |
| "configure (DB)" → IMPLEMENT; 4.4 | "configure" | Rule 2 |
| "rollback" (verb) → IMPLEMENT; 4.4 | "rollback" | Rule 2 |
| "GiST/GIN/covering/partial index" → 4.4 | index type names | DDL vocabulary |
| "DBMS name" → 4.4 (not 4.3) | "MySQL", "Oracle", "MongoDB" | Context rule |

---

## B18-0C-060-004 — Complete knowledge-edge reference: domain-specific edge extensions

**Domain-specific edge extensions for 037-060 (edges present only in certain domains):**

| Edge | Domain | Notes |
|---|---|---|
| "shard key structure" → ARCHITECTURE | ShardingMigration | "structure" absolute; domain-specific compound |
| "event payload encryption" → SECURITY | EventSourcingSchema | "encryption" fires SECURITY |
| "projection table pattern" → ARCHITECTURE | CQRSProjection | "pattern" absolute |
| "audit trigger activation" → IMPLEMENT | AuditTableSetup | "trigger" = DDL vocabulary |
| "tenant isolation" → 4.4 | MultiTenantSchema | Data vocabulary |
| "down migration" → IMPLEMENT | RollbackScript | "down migration" = 4.4 rollback |
| "cold storage archival" → IMPLEMENT | DataArchival | Extended archival vocabulary |

---

## B18-0C-060-005 — Complete knowledge-edge reference: B18 edge system summary

**B18 knowledge-edge system summary (all 60 files):**

The knowledge-edge system for B18-batch-001 covers:
- **37 new domains** added in files 037-060 (on top of 24 from 001-036)
- **61 total unique tokens** mapped to predicate/TransitionType/sub-phase edges
- **4 v6 corruption types** (v6-01 through v6-04) with detection edges
- **25 absolute rules** that fire regardless of domain context
- **8 disambiguation principles** for ambiguous token forms (noun vs verb, compound nouns, negation, passive voice)

The edge system is complete for all B18 DataMigrationFlow44 scenarios from files 001 through 060.
