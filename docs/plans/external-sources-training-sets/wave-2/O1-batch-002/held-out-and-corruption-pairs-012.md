# Held-Out and Corruption Pairs (012)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## O1b-HO-012-001 — Held-out summary: novel prompt coverage

A: Held-out summary for O1-batch-002

B: Total held-out pairs: 33 (3 per file × 11 files). Novel prompt categories: (1) service account + credentials, (2) client certificate auth, (3) API gateway + tenant isolation, (4) rate limiting + scopes, (5) OpenAPI/gRPC contracts, (6) refactoring scenarios, (7) key/secret rotation, (8) tenant quota management. All held-out prompts were NOT in the base training files (002-006).

---

## O1b-HO-012-002 — Corruption summary: v6-01 cases

A: v6-01 corruption summary for O1-batch-002

B: Total v6-01 pairs: 12. Invented predicates demonstrated: OAUTH_FLOW, CLIENT_AUTH, CLIENT_CREDS, OAUTH_GRANT, TENANT_SCOPE, RATE_LIMIT, SERVICE_ACCOUNT_VALIDATOR. All corrected in CORRECT B section. Pattern: model invents scenario-specific predicate names instead of using canonical SECURITY or AUTHORIZATION.

---

## O1b-HO-012-003 — Corruption summary: v6-02 cases

A: v6-02 corruption summary for O1-batch-002

B: Total v6-02 pairs: 10. Gap suppression patterns: (1) "client portal" → false SECURITY (files 003, 011). (2) "scope" not detected as AUTHORIZATION (files 006, 009). (3) "tenant" not detected as AUTHORIZATION (file 009). (4) AUTHORIZATION fully suppressed in multi-predicate prompts (file 006). All corrected in CORRECT B section.

---

## O1b-HO-012-004 — Corruption summary: v6-03 cases

A: v6-03 corruption summary for O1-batch-002

B: Total v6-03 pairs: 10. Wrong routing patterns: (1) "service account" → 4.1 instead of 3.4 (files 004, 007). (2) SECURITY+AUTH → two 3.4 decisions (file 007). (3) "scope" → wrong sub-phase (files 004, 007). (4) AUTHORIZATION → 2.1 instead of 3.4 (file 007). (5) "refactor" → feature_addition instead of refactoring (file 010).

---

## O1b-HO-012-005 — Complete held-out and corruption Gate B

A: Gate B for held-out and corruption module

B: Gate B: "Module passes when: (1) ≥30 held-out pairs covering ≥8 novel prompt categories. (2) ≥10 v6-01 corruption pairs with distinct invented predicates. (3) ≥8 v6-02 gap suppression pairs. (4) ≥8 v6-03 wrong routing pairs. (5) All corrupted pairs include CORRECT B contrast." Result: PASSED for O1-batch-002.
