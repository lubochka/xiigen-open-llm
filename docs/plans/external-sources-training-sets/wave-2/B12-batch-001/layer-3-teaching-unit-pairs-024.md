# Layer 3 — Teaching Unit Pairs (024)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## B12-TU-024 — Complete B12 teaching summary (files 013-024)

```
source_id: xiigen-b12-tu-024
pair_kind: teaching_unit_reference
```

### 12 teaching rules exercised in files 013-024

| File | Rule | Teaching point |
|------|------|----------------|
| 013 | B12-RULE-WEBHOOK-CONTRACT | "webhook contract" double trigger → one API_CONTRACT instance |
| 014 | B12-RULE-SCHEMA-CO-OCCURRENCE | "database schema" → no API_CONTRACT; "REST schema" → API_CONTRACT |
| 015 | B12-RULE-IMPLEMENT-VERBS | IMPLEMENT verb list vs documentation verb list |
| 016 | B12-RULE-SPEC-DOCUMENT | "spec" fires DOCUMENT not API_CONTRACT |
| 017 | B12-RULE-DESIGN-DUAL-FIRE | "design" + API term → ARCHITECTURE + API_CONTRACT both fire |
| 018 | B12-RULE-IDEMPOTENT | "idempotent" fires API_CONTRACT (API contract term) |
| 019 | B12-RULE-INTEGRATION-CO-OCCURRENCE | "integration" alone → no API_CONTRACT; needs API co-occurrence |
| 020 | B12-RULE-ROUTE | "route" fires API_CONTRACT unconditionally |
| 021 | B12-RULE-PUBLISH-NOT-IMPLEMENT | "publish" is release verb, not code-creation → no IMPLEMENT |
| 022 | B12-RULE-UPDATE-VS-IMPLEMENT | "update" → documentation; "implement/write" → feature_addition |
| 023 | B12-RULE-IMPLEMENT-API-CONTRACT | IMPLEMENT + API_CONTRACT → feature_addition + 4.3 in routes |
| 024 | Summary | Complete rule index for files 013-024 |

### Three most important rules for B12 (by corruption frequency)

1. "contract" fires API_CONTRACT regardless of modifier (from original file 002)
   Most common v6-01 corruption: suppressing API_CONTRACT on "data contract", "service contract", etc.

2. "schema" requires API co-occurrence (this file set, rule 014)
   "database schema" → no API_CONTRACT is frequently confused

3. "authentication" fires SECURITY ALWAYS (Core Rule 6)
   v6-02 corruption: suppressing SECURITY because the context is API documentation

### Three boundary pairs most tested in files 013-024

1. webhook contract (API_CONTRACT) vs notification domain word (no predicate)
2. database schema (no API_CONTRACT) vs API/REST schema (API_CONTRACT)
3. "update" (documentation) vs "implement/write/build" (feature_addition)
