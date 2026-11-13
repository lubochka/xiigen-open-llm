# Layer 0-C — Knowledge-Edge Pairs (045)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-045-001 — Complete edge reference: 037-060 complete token-to-predicate map

**Master token-to-predicate map for B18 037-060:**

| Token | → Predicate(s) | Rule |
|---|---|---|
| "write migration" | IMPLEMENT | Rule 2 |
| "create [DB object]" | IMPLEMENT | Rule 2 |
| "add [column/index/constraint]" | IMPLEMENT | Rule 2 |
| "migrate [data/records]" | IMPLEMENT | Rule 5 |
| "seed/populate/fixture" | IMPLEMENT | Rule 6 |
| "archive [records]" | IMPLEMENT | Rule 5 |
| "anonymize" (verb) | IMPLEMENT + SECURITY | Rules 2 + 14 |
| "mask" (PII verb) | IMPLEMENT + SECURITY | Rules 2 + 14 |
| "promote replica" | IMPLEMENT | Rule 2 |
| "structure" (noun) | ARCHITECTURE | Rule 3 |
| "architecture" (noun) | ARCHITECTURE | Rule 7 |
| "pattern" (noun) | ARCHITECTURE | Rule 7 |
| "design" (verb) | ARCHITECTURE | Rule 11 |
| "PII" | SECURITY | Rule 14 |
| "GDPR" | SECURITY | Rule 14 |
| "encrypted" | SECURITY | Rule 14 |
| "sensitive data" | SECURITY | Rule 14 |
| "credentials" | SECURITY | Rule 14 |
| "row-level security" | SECURITY | Rule 14 |
| "TLS" | SECURITY | Rule 14 |
| "write tests" | TEST | Rule 12 |
| "add tests" | TEST | Rule 12 |
| "create tests" | TEST | Rule 12 |
| "document" (verb) | DOCUMENT | Rule 4 |
| "annotate" | DOCUMENT | Rule 4 |
| "spec out" | DOCUMENT | Rule 4 |
| "QA" | QA | Position-locked last |

---

## B18-0C-045-002 — Complete edge reference: TransitionType determination

**TransitionType token-to-edge map:**

| Token/verb | → TransitionType | Rule |
|---|---|---|
| "refactor" | refactoring | Rule 9 (absolute) |
| "consolidate" | refactoring | Rule 20 (absolute) |
| "restructure" | refactoring | Rule 1 (absolute) |
| "rename" | refactoring | Rule 9 |
| "reorganize" | refactoring | Rule 1 |
| "clean up" | refactoring | Rule 1 |
| "move" (code target) | refactoring | Rule 1 |
| "write migration" | feature_addition | Rule 2 |
| "migrate" (data) | feature_addition | Rule 5 |
| "create" (DB object) | feature_addition | Rule 2 |
| "add" (column/index) | feature_addition | Rule 2 |
| "seed/populate" | feature_addition | Rule 6 |
| "archive" (data) | feature_addition | Rule 5 |
| "anonymize/mask" | feature_addition | Rule 2 |
| "promote" (replica) | feature_addition | Rule 5 |

---

## B18-0C-045-003 — Complete edge reference: Gate B type selection

**Gate B type edge map for all B18 037-060 domains:**

| Operation type | Gate B type | Canonical condition format |
|---|---|---|
| Standard data migration | row_count_assertion | count(target) == count(source) |
| One-to-many migration | row_count_assertion (estimated) | count(target) == estimated_count |
| Seed/fixture/populate | idempotency_check | seeded_count == expected AND idempotent == true |
| DDL schema change (no data) | schema_change_verification | [object]_exists AND [constraint/property] == expected |
| PII anonymization | row_count_assertion | count(pii_col IS NOT NULL) == 0 |
| Masking | row_count_assertion | count(masked_rows) == total_rows |
| Archival | row_count_assertion | count(archive_table) == count(archived_source_rows) |
| Index creation | schema_change_verification | index_exists AND query_plan_uses_index == true |
| Security review | security_scan_pass | no_plaintext_pii AND all_fields_encrypted == true |
| Architecture | schema_review_complete | schema_spec_approved == true |
| Test suite | test_pass_assertion | all_tests_pass == true |
| Documentation | doc_complete | document_published == true |

---

## B18-0C-045-004 — Complete edge reference: v6 corruption type summary

**All v6 corruption types with detection edges:**

| v6 type | Detection token | Correct edge |
|---|---|---|
| v6-01 | "structure"/"architecture"/"pattern"/"design" present but ARCHITECTURE absent | Add ARCHITECTURE |
| v6-02 | refactoring verb present but TransitionType = feature_addition | Set TransitionType = refactoring |
| v6-03 | data/DDL vocabulary present but BackendServiceFlow41 (4.1) or ApiContractFlow43 (4.3) selected | Route to DataMigrationFlow44 (4.4) |
| v6-04 | "PII"/"GDPR"/"encrypted"/"sensitive"/"anonymize"/"mask" present but SECURITY absent | Add SECURITY predicate |

---

## B18-0C-045-005 — Complete edge reference: 10 disambiguation test questions for 037-060

**10 disambiguation test questions for 037-060 domains:**

1. "archive customer records" → SECURITY fires? **No** — "archive" alone not a security trigger
2. "archive customer PII records" → SECURITY fires? **Yes** — "PII" absolute trigger
3. "GiST index" → ARCHITECTURE fires? **No** — GiST alone is DDL vocabulary
4. "GiST index structure" → ARCHITECTURE fires? **Yes** — "structure" absolute trigger
5. "create event_store" → sub-phase? **4.4** — table creation = DDL migration
6. "CQRS alone" → ARCHITECTURE fires? **No** — acronym alone, no absolute token
7. "multi-tenant isolation pattern" → ARCHITECTURE fires? **Yes** — "pattern" absolute trigger
8. "test fixtures for staging" → TEST fires? **No** — "fixtures" = 4.4 seed vocabulary
9. "write integration tests" → TEST fires? **Yes** — "write tests" verb form
10. "row-level security migration" → SECURITY fires? **Yes** — "security" absolute trigger
