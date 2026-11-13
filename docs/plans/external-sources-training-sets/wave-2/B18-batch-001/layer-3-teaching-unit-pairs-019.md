# Layer 3 — Teaching Unit Pairs (019)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-019-001 — Teaching: GDPR fires SECURITY absolutely

**Rule:** "GDPR" (General Data Protection Regulation) is an absolute SECURITY trigger (Rule 14).

**Test questions:**
1. "Migrate audit logs applying GDPR retention rules" → SECURITY fires? **YES.** GDPR is present.
2. "Migrate records per data retention policy" → SECURITY fires? **No.** "retention policy" alone ≠ GDPR.
3. "GDPR-compliant migration" → SECURITY fires? **YES.** GDPR is present.
4. "Regulatory retention rules" → SECURITY fires? **No.** Not GDPR/PII/encrypted vocabulary.

---

## B18-TU-019-002 — Teaching: PII fires SECURITY regardless of context

**Rule:** "PII" (Personally Identifiable Information) is an absolute SECURITY trigger. Context does not matter.

**PII fires SECURITY in:**
- Migration context: "migrate records containing PII" → SECURITY ✓
- Seeding context: "seed preferences ensuring PII encrypted" → SECURITY ✓
- Refactoring context: "refactor handling of PII fields" → SECURITY ✓
- Documentation context: "document PII handling procedures" → SECURITY ✓ (SECURITY fires; DOCUMENT also fires)

**SECURITY is additive.** It fires alongside any other predicate. It is never suppressed.

---

## B18-TU-019-003 — Teaching: "structured" (adjective) vs "structure" (noun)

**Critical rule:** "structured" is an adjective. It does NOT fire ARCHITECTURE. Only the noun "structure" fires ARCHITECTURE.

**Test:**
- "structured audit log" → ARCHITECTURE fires? **No.** "structured" is adjective form.
- "audit log structure" → ARCHITECTURE fires? **YES.** "structure" is noun form.
- "structured approach to audit logging" → ARCHITECTURE fires? **No.** "structured" adjective.

**Mnemonic:** If you can replace "structured" with "organized" or "well-organized" and it still makes sense as an adjective, it does NOT fire ARCHITECTURE.

---

## B18-TU-019-004 — Teaching: "rename" on database columns → refactoring, 4.4

**Rule:** "rename" fires refactoring TransitionType. When applied to database columns, sub-phase is 4.4 (database vocabulary).

**Full trace for "Rename audit_trail columns to audit_log naming convention":**
- "rename" → IMPLEMENT; refactoring TransitionType
- "audit_trail columns" → 4.4 sub-phase (database vocabulary)
- "naming convention" → not ARCHITECTURE trigger
- Result: ["IMPLEMENT"], refactoring, 4.4, DataMigrationFlow44

---

## B18-TU-019-005 — Teaching: SECURITY + TEST can run in parallel (after IMPLEMENT)

**Orchestration rule:** SecurityReviewFlow and UnitTestingFlow51 are both eligible to run in parallel after DataMigrationFlow44 completes.

**Scenario:** AuditLogMigration + GDPR + write integration tests
```
1. DataMigrationFlow44 — migrate and anonymize
2. SecurityReviewFlow ‖ UnitTestingFlow51 — parallel after step 1
3. N08: both Gate B conditions
4. N09 output
```

**Teaching point:** Two post-IMPLEMENT flows can run concurrently. Only DOCUMENT is described as "parallel-eligible" in documentation, but SECURITY + TEST can also parallelize after IMPLEMENT.
