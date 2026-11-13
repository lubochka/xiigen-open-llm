# Layer 3 — Teaching Unit Pairs (024)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: teaching-unit

## B18-TU-024-001 — Complete teaching summary: the 7 must-know rules for B18 013-024

**The 7 rules that cover all new migration domains added in files 013-024:**

1. **"structure" (noun) ALWAYS fires ARCHITECTURE.** Compound nouns preserve it. Adjective "structured" does NOT fire it.

2. **"refactor" ALWAYS sets TransitionType: refactoring (Rule 9).** Even when applied to database tables, ETL pipelines, or combined with migration vocabulary.

3. **"seed"/"fixture"/"populate"/"rebuild" (data) → feature_addition + 4.4.** Never routes to 5.x or 4.1.

4. **ETL vocabulary ("extract" data, "transform", "load", "ETL") → feature_addition + 4.4.** Never routes to 4.3.

5. **Security triggers (PII, GDPR, encrypted, credentials, authentication, compliance) are absolute.** No context suppresses them. Always fires SecurityReviewFlow.

6. **"write/add/create tests" (verb form) fires TEST.** Noun forms ("integration testing", "the tests") do NOT.

7. **Multiple sub-phase vocabulary → multiple flows.** 4.4 + 4.3 together → DataMigrationFlow44 + ApiContractFlow43. Neither suppresses the other.

---

## B18-TU-024-002 — Complete teaching summary: TransitionType determination for new domains

**TransitionType chart for 013-024 domain scenarios:**

| Domain | Typical verb | TransitionType |
|---|---|---|
| AddressBook migration | migrate | feature_addition |
| Analytics ETL | build ETL, load | feature_addition |
| SearchIndex rebuild | rebuild, seed | feature_addition |
| ProductVariants seeding | seed, populate | feature_addition |
| InvoiceHistory migration | migrate, write migration | feature_addition |
| TaxRecord migration | migrate, ETL | feature_addition |
| AuditLog migration | migrate | feature_addition |
| SessionData migration | migrate, move | feature_addition |
| UserPreferences seeding | seed, fixture | feature_addition |
| ApiKeyRotation migration | migrate, rotate | feature_addition |
| SubscriptionBilling migration | migrate, write migration | feature_addition |
| Any domain + "refactor" | refactor (Rule 9) | refactoring |
| Any domain + "consolidate" | consolidate (Rule 20) | refactoring |
| Any domain + "restructure" | restructure (Rule 1) | refactoring |

---

## B18-TU-024-003 — Complete teaching summary: SECURITY triggers for 013-024 domains

**When does SECURITY fire in the new migration domains?**

| Domain | SECURITY trigger |
|---|---|
| AddressBook | PII present |
| Analytics/EventLog | encrypted, user_id encryption |
| SearchIndex | (uncommon; no typical PII) |
| ProductVariants | (uncommon) |
| InvoiceHistory | (uncommon; amounts not PII) |
| TaxRecord | compliance (regulatory compliance) |
| AuditLog | PII, GDPR, anonymization |
| SessionData | encrypted, authentication, tokens |
| UserPreferences | PII in preference fields |
| ApiKeyRotation | credentials, hashed, authentication |
| SubscriptionBilling | PII (user billing data if mentioned) |

---

## B18-TU-024-004 — Complete teaching summary: Gate B types for new domains

| Domain | Operation type | Gate B type |
|---|---|---|
| AddressBook | migration | row_count_assertion |
| Analytics/EventLog | ETL | row_count_assertion |
| SearchIndex | seeding | idempotency_check |
| ProductVariants | seeding | idempotency_check |
| InvoiceHistory | migration | row_count_assertion |
| TaxRecord (one-to-many) | migration (expanded) | row_count_assertion (estimated) |
| AuditLog | migration | row_count_assertion |
| SessionData | migration | row_count_assertion |
| UserPreferences | seeding | idempotency_check |
| ApiKeyRotation | migration | row_count_assertion |
| SubscriptionBilling | migration | row_count_assertion |

---

## B18-TU-024-005 — Complete teaching summary: three v6 corruption types applied to new domains

**v6 corruption checklist for 013-024 domains:**

| v6 type | Detection signal | Correct fix |
|---|---|---|
| v6-01 | "structure" present but ARCHITECTURE absent | Add ARCHITECTURE predicate; add ArchitectureFlow31 |
| v6-02 | "refactor"/"consolidate"/"restructure" present but TransitionType: feature_addition | Set TransitionType: refactoring |
| v6-03 | "migrate"/"seed"/"ETL"/"fixture" present but BackendServiceFlow41 selected (4.1) | Route to DataMigrationFlow44 (4.4) |

**All three corruptions are violations of absolute rules. No context overrides them.**
