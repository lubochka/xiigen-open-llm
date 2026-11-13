# Layer 0-C — Knowledge-Edge Pairs (041)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-041-001 — Complete edge reference: SECURITY absolute tokens in 037-060

**All tokens that fire SECURITY absolutely in 037-060 domains:**

| Token | SECURITY fires | Notes |
|---|---|---|
| "PII" | YES (absolute) | Any domain, any context |
| "GDPR" | YES (absolute) | Any domain, any context |
| "encrypted" / "encrypt" | YES (absolute) | Any domain, any context |
| "sensitive data" / "sensitive" | YES (absolute) | Any domain, any context |
| "credentials" | YES (absolute) | Any domain, any context |
| "authentication" | YES (absolute) | Any domain, any context |
| "authorization" | YES (absolute) | Any domain, any context |
| "access control" | YES (absolute) | Any domain, any context |
| "row-level security" | YES | MultiTenantSchema |
| "TLS" | YES | Replication/encryption |
| "anonymize" (verb) | YES | DataAnonymization |
| "mask" (verb, PII context) | YES | PIIMasking |

**SECURITY does NOT fire on:** "audit" alone, "archive" alone, "shard" alone, "index" alone, "trigger" (DB object), "sequence", "replica" alone

---

## B18-0C-041-002 — Complete edge reference: NOT ARCHITECTURE tokens

**Tokens that appear related to architecture but do NOT fire ARCHITECTURE:**

| Token | ARCHITECTURE fires? | Reason |
|---|---|---|
| "structured" (adjective) | NO | Adjective, not noun |
| "data structure" compound noun | YES | "structure" is noun |
| "schema" alone | NO | "schema" is not "structure" |
| "CQRS" alone | NO | Acronym alone |
| "event sourcing" alone | NO | Domain name alone |
| "multi-tenant" alone | NO | Domain name alone |
| "partitioning" alone | NO | DDL vocabulary alone |
| "sharding" alone | NO | DDL vocabulary alone |
| "replication" alone | NO | DDL vocabulary alone |
| "the design looks good" | NO | "design" is noun/adjective |
| "software design" | NO | "design" is noun in "software design" |
| "design the schema" | YES | "design" is verb |

---

## B18-0C-041-003 — Complete edge reference: "design" verb vs noun disambiguation edge

**"design" as verb → ARCHITECTURE. "design" as noun → NOT ARCHITECTURE.**

| Phrase | Part of speech | ARCHITECTURE fires? |
|---|---|---|
| "design the schema" | verb | YES |
| "design a migration pattern" | verb | YES |
| "re-design the index structure" | verb | YES |
| "the database design" | noun | NO |
| "database design principles" | noun | NO |
| "good design practice" | noun/modifier | NO |
| "design review" | noun modifier | NO |
| "design document" | noun modifier | NO |

---

## B18-0C-041-004 — Complete edge reference: "pattern" scope in 037-060

**"pattern" fires ARCHITECTURE regardless of surrounding verb:**

| Phrase | ARCHITECTURE fires? |
|---|---|
| "design the soft-delete pattern" | YES — "design" verb + "pattern" |
| "use the existing rollback pattern" | YES — "pattern" absolute |
| "the migration follows the standard pattern" | YES — "pattern" absolute |
| "document the usage pattern" | YES — "pattern" absolute (DOCUMENT also fires) |
| "sharding pattern" | YES — "pattern" absolute |
| "index usage pattern" | YES — "pattern" absolute |
| "the pattern is well-tested" | YES — "pattern" is noun |

**Key rule:** "pattern" (noun) ALWAYS fires ARCHITECTURE. Even when surrounded by "document", "use", "follows", or "the", the noun "pattern" fires ARCHITECTURE Rule 7. Both ARCHITECTURE and the other predicate (DOCUMENT, etc.) are present in the output.

---

## B18-0C-041-005 — Complete edge reference: TEST verb form vs noun form

**TEST fires only on verb forms:**

| Phrase | TEST fires? | Reason |
|---|---|---|
| "write tests to verify migration" | YES | "write tests" = verb form |
| "add tests for rollback" | YES | "add tests" = verb form |
| "create test cases for the schema" | YES | "create tests" = verb form |
| "tests are written" | NO | Past tense, passive |
| "integration testing the migration" | NO | Noun/gerund form |
| "run the tests" | NO | "run" is not write/add/create |
| "the tests passed" | NO | Noun form |
| "test the migration" | NO | "test" as single verb without "tests" object |
| "verify the migration" | NO | "verify" ≠ TEST trigger |
| "test data migration" | NO | "test data" is 4.4 vocabulary |
