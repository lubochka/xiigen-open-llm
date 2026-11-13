# Layer 0-C — Knowledge-Edge Pairs (048)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-048-001 — Complete knowledge-edge reference: absolute rule summary

**B18 absolute knowledge-edge rules (never overridden, all domains 001-060):**

1. "structure" (noun) → ARCHITECTURE edge, absolute (Rule 3)
2. "architecture" (noun) → ARCHITECTURE edge, absolute (Rule 7)
3. "pattern" (noun) → ARCHITECTURE edge, absolute (Rule 7)
4. "design" (verb) → ARCHITECTURE edge (Rule 11)
5. "PII" → SECURITY edge, absolute (Rule 14)
6. "GDPR" → SECURITY edge, absolute (Rule 14)
7. "encrypted" → SECURITY edge, absolute (Rule 14)
8. "sensitive data" → SECURITY edge, absolute (Rule 14)
9. "credentials" → SECURITY edge, absolute (Rule 14)
10. "anonymize" (verb) → IMPLEMENT + SECURITY edges
11. "mask" (verb, PII) → IMPLEMENT + SECURITY edges
12. "write tests"/"add tests"/"create tests" → TEST edge (Rule 12)
13. "document"/"annotate"/"spec out" → DOCUMENT edge (Rule 4)
14. Refactoring verbs (refactor, consolidate, restructure, rename) → TransitionType: refactoring edge

---

## B18-0C-048-002 — Complete knowledge-edge reference: non-firing edge disambiguation

**Tokens that do NOT fire their apparent predicate:**

| Token | Does NOT fire | Correct interpretation |
|---|---|---|
| "structured" (adjective) | ARCHITECTURE | Adjective; not "structure" noun |
| "verify" alone | TEST | Neutral migration action |
| "testing" (noun/gerund) | TEST | Noun form |
| "integration testing" (noun phrase) | TEST | Noun form |
| "the tests passed" | TEST | Past tense noun |
| "tests" alone | TEST | Standalone noun |
| "audit" alone | SECURITY | Data vocabulary; needs PII/GDPR |
| "archive" alone | SECURITY | Data operation; needs PII/GDPR |
| "trigger" (DB object) | SECURITY | DDL vocabulary only |
| "sequence" | SECURITY | DDL vocabulary only |
| "index" alone | SECURITY | DDL vocabulary only |
| "CQRS" alone | ARCHITECTURE | Acronym alone ≠ ARCHITECTURE |
| "multi-tenant" alone | SECURITY | Domain name alone |
| "the design" (noun) | ARCHITECTURE | "design" as noun ≠ design verb |

---

## B18-0C-048-003 — Complete knowledge-edge reference: predicate collapse rules

**Multiple triggers of the same predicate → one predicate in output:**

| Multiple triggers | Output |
|---|---|
| "PII" + "GDPR" + "encrypted" + "sensitive" | SECURITY (once) |
| "structure" + "architecture" + "pattern" + "design" | ARCHITECTURE (once) |
| "write migration" + "implement migration" + "add migration" | IMPLEMENT (once) |
| "write tests" + "add integration tests" | TEST (once) |
| "document" + "annotate" + "write runbook" | DOCUMENT (once) |

**Predicate output is always a SET (no duplicates), regardless of how many times a trigger fires.**

---

## B18-0C-048-004 — Complete knowledge-edge reference: GapHandler non-activation

**GapHandler non-activation rules for all B18 037-060 domains:**

B18 has NO GapHandler flow. The following vocabulary does NOT activate any gap-handling flow in B18:
- "gap in the migration data"
- "missing records"
- "data gap"
- "NULL gap"
- "schema gap"
- "version gap"

**GapHandler outputs:** GapHandler: none in all B18 N01 output.

---

## B18-0C-048-005 — Complete knowledge-edge reference: DomainId edge

**DomainId edge rule:**

- DomainId: "software-development" is the output for ALL B18 predicate combinations
- DomainId is set at Layer 0-A output and propagated unchanged through all layers
- No vocabulary token in B18 changes DomainId
- No domain (DataAnonymization, ShardingMigration, CQRSProjection, etc.) changes DomainId
- DomainId is always "software-development" in N09 output

**DomainId edge is absolute and invariant across all 60 numbered B18 files.**
