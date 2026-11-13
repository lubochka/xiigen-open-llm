# Layer 0-C — Knowledge-Edge Pairs (058)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-058-001 — Complete edge reference: 10 must-know edge rules for B18 037-060

**The 10 must-know knowledge edge rules for B18 037-060:**

1. "structure" (noun) fires ARCHITECTURE absolutely. No compound, no adjective form suppresses it.
2. "pattern" (noun) fires ARCHITECTURE absolutely. Context cannot suppress it.
3. "architecture" (noun) fires ARCHITECTURE absolutely.
4. "design" (verb) fires ARCHITECTURE. "design" (noun) does NOT.
5. "PII", "GDPR", "encrypted", "sensitive" fire SECURITY absolutely.
6. "anonymize" and "mask" (PII verb) fire BOTH IMPLEMENT and SECURITY.
7. "write/add/create tests" fires TEST. "tests" (noun), "testing", "verify" do NOT.
8. "document" (verb) fires DOCUMENT. "document" (noun/DB object) does NOT.
9. Refactoring verbs (refactor, consolidate, restructure, rename) set TransitionType: refactoring.
10. All database DDL vocabulary routes to 4.4 (DataMigrationFlow44).

---

## B18-0C-058-002 — Complete edge reference: predicate sets by domain

**Expected predicate sets for each 037-060 domain (common cases):**

| Domain | Common predicate set | Rare addition |
|---|---|---|
| DataAnonymization | IMPLEMENT + SECURITY | + ARCHITECTURE (with "pattern") |
| PIIMasking | IMPLEMENT + SECURITY | + TEST (with "write tests") |
| DataArchival | IMPLEMENT | + SECURITY (with PII/GDPR) |
| ShardingMigration | IMPLEMENT + ARCHITECTURE | + SECURITY (rare) |
| ReplicationSetup | IMPLEMENT | + ARCHITECTURE + SECURITY (with encrypted) |
| ReadReplicaPromotion | IMPLEMENT | + TEST |
| FullTextSearchIndex | IMPLEMENT | + ARCHITECTURE (with "structure") |
| GeoIndex | IMPLEMENT | + ARCHITECTURE (with "pattern") |
| CompositeIndex | IMPLEMENT | + ARCHITECTURE (with "pattern") |
| CoveringIndex | IMPLEMENT | + TEST |
| PartialIndex | IMPLEMENT | + ARCHITECTURE (with "structure") |
| CrossDatabaseMigration | IMPLEMENT | + SECURITY (with PII) + ARCHITECTURE |
| NoSQLToSQL | IMPLEMENT | + ARCHITECTURE + SECURITY |
| EventSourcingSchema | IMPLEMENT | + ARCHITECTURE + SECURITY (encrypted) |
| CQRSProjection | IMPLEMENT | + ARCHITECTURE |
| AuditTableSetup | IMPLEMENT | + SECURITY (PII/access control) |
| SoftDeleteMigration | IMPLEMENT | + ARCHITECTURE (soft-delete pattern) |
| MultiTenantSchema | IMPLEMENT | + ARCHITECTURE + SECURITY (row-level) |
| SchemaVersioning | IMPLEMENT | + ARCHITECTURE |
| SeedDataMigration | IMPLEMENT | + TEST (idempotency) |
| TestDataMigration | IMPLEMENT | + SECURITY (PII anonymization) |
| RollbackScript | IMPLEMENT | + ARCHITECTURE + TEST |

---

## B18-0C-058-003 — Complete edge reference: "v6-04" edge detection template

**v6-04 detection template:**

```
v6-04 Detection:
  IF any of ["PII", "GDPR", "encrypted", "sensitive", "credentials",
             "anonymize", "mask", "row-level security", "TLS",
             "authentication", "authorization", "access control"] FOUND in request
  AND SECURITY not present in output predicates
  THEN error: v6-04
  FIX: add SECURITY predicate; add SecurityReviewFlow to execution plan
```

---

## B18-0C-058-004 — Complete edge reference: edge stability proof

**Why edges are stable across domains (formal statement):**

The knowledge-edge layer is a token-matching layer that operates INDEPENDENTLY of domain context. Tokens fire edges based on their morphological form (noun/verb/adjective) and the rule they trigger. Domain context provides VOCABULARY for sub-phase routing but does NOT modify the predicate edge of any absolute token.

- "PII" fires SECURITY in DataAnonymization, in AuditTableSetup, in MultiTenantSchema, in any domain. The domain cannot suppress it.
- "structure" fires ARCHITECTURE in "index structure", "schema structure", "event sourcing schema structure", "anonymization structure". The compound does not suppress it.
- "write tests" fires TEST in any domain: migration tests, rollback tests, anonymization tests. The context does not suppress it.

---

## B18-0C-058-005 — Complete edge reference: NEW EDGE — "backfill" and "populate"

**"backfill" and "populate" edge rules (formalized for 037-060):**

| Token | IMPLEMENT fires? | TransitionType | Sub-phase |
|---|---|---|---|
| "backfill NULL values" | YES | feature_addition | 4.4 |
| "backfill tenant_id for all rows" | YES | feature_addition | 4.4 |
| "backfill deleted_at column" | YES | feature_addition | 4.4 |
| "populate the reference table" | YES | feature_addition | 4.4 |
| "populate default seed data" | YES | feature_addition | 4.4 |
| "populate with test fixtures" | YES | feature_addition | 4.4 (NOT 5.x) |

**"backfill" and "populate" are equivalent to "seed" in sub-phase routing. Both are 4.4 data vocabulary. Neither routes to 5.x or 4.1.**
