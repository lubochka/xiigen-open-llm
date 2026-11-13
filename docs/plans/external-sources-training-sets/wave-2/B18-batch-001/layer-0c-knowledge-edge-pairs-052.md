# Layer 0-C — Knowledge-Edge Pairs (052)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-052-001 — Complete knowledge-edge reference: v6 error edge detection

**v6-01 detection edges (ARCHITECTURE suppressed):**

Token present but ARCHITECTURE absent in output:
- "anonymization pattern" → v6-01 error
- "sharded architecture" → v6-01 error
- "full-text search index structure" → v6-01 error
- "CQRS projection pattern" → v6-01 error
- "multi-tenant schema structure" → v6-01 error
- "soft-delete pattern" → v6-01 error
- "rollback architecture" → v6-01 error
- "event sourcing architecture" → v6-01 error

**v6-02 detection edges (wrong TransitionType):**

Refactoring verb present but TransitionType: feature_addition in output:
- "restructure table for sharding" → v6-02 error
- "consolidate anonymization migrations" → v6-02 error
- "rename deleted_at to removed_at" → v6-02 error
- "refactor rollback scripts" → v6-02 error

**v6-03 detection edges (wrong sub-phase):**

Data vocabulary present but 4.1 or 4.3 selected:
- "create composite index" → v6-03 if BackendServiceFlow41 selected
- "migrate from MongoDB" → v6-03 if ApiContractFlow43 selected
- "write rollback script" → v6-03 if BackendServiceFlow41 selected

**v6-04 detection edges (SECURITY suppressed):**

PII/GDPR/encrypted/anonymize/mask present but SECURITY absent:
- "anonymize PII fields" without SECURITY → v6-04 error
- "GDPR-compliant migration" without SECURITY → v6-04 error
- "encrypted at rest" without SECURITY → v6-04 error

---

## B18-0C-052-002 — Complete knowledge-edge reference: edge priority rules

**When two edges conflict, priority rules apply:**

| Conflict | Winner | Rule |
|---|---|---|
| feature_addition signal + refactoring verb | refactoring | Rule 9 absolute |
| ARCHITECTURE token + "no design needed" context | ARCHITECTURE | Rule 3/7 absolute; context cannot suppress |
| SECURITY token + "not a security concern" context | SECURITY | Rule 14 absolute; context cannot suppress |
| Multiple ARCHITECTURE triggers | Single ARCHITECTURE predicate | Collapse to set |
| Multiple SECURITY triggers | Single SECURITY predicate | Collapse to set |
| "document" verb + "pattern" noun | Both DOCUMENT and ARCHITECTURE | Neither suppresses the other |

---

## B18-0C-052-003 — Complete knowledge-edge reference: five-predicate collapse pattern

**When all five predicates fire in a single request:**

Token requirements for five-predicate output:
1. At least one IMPLEMENT trigger (write migration, migrate, create, add, anonymize, etc.)
2. At least one ARCHITECTURE trigger (structure, architecture, pattern, design verb)
3. At least one SECURITY trigger (PII, GDPR, encrypted, sensitive, credentials, etc.)
4. At least one TEST trigger (write tests, add tests, create tests)
5. At least one DOCUMENT trigger (document, annotate, spec out, write runbook)

Each trigger fires its predicate independently. All five predicates collapse to the set: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]

---

## B18-0C-052-004 — Complete knowledge-edge reference: edge stability across domains

**Edge stability principle:** The same token fires the same edge regardless of which domain it appears in.

| Token | Edge (constant across all domains) |
|---|---|
| "structure" (noun) | ARCHITECTURE — in ANY of 001-060 domains |
| "PII" | SECURITY — in ANY of 001-060 domains |
| "write migration" | IMPLEMENT — in ANY of 001-060 domains |
| "write tests" | TEST — in ANY of 001-060 domains |
| "document" (verb) | DOCUMENT — in ANY of 001-060 domains |
| "refactor" | TransitionType: refactoring — in ANY domain |

**This is the core knowledge-edge principle: edges are token-based, not domain-based. Domain context does not suppress or modify absolute edges.**

---

## B18-0C-052-005 — Complete knowledge-edge reference: B18 edge catalogue summary

**Complete edge catalogue for B18 001-060 (abbreviated):**

| Token category | Predicate edge | TransitionType edge | Sub-phase edge |
|---|---|---|---|
| "write migration" | IMPLEMENT | feature_addition | 4.4 |
| "migrate (data)" | IMPLEMENT | feature_addition | 4.4 |
| "seed/populate/fixture" | IMPLEMENT | feature_addition | 4.4 |
| "create (DB object)" | IMPLEMENT | feature_addition | 4.4 |
| "add (column/index)" | IMPLEMENT | feature_addition | 4.4 |
| "archive (data)" | IMPLEMENT | feature_addition | 4.4 |
| "anonymize" | IMPLEMENT + SECURITY | feature_addition | 4.4 |
| "mask (PII)" | IMPLEMENT + SECURITY | feature_addition | 4.4 |
| "refactor/consolidate/restructure/rename" | IMPLEMENT | refactoring | depends on target |
| "structure" | ARCHITECTURE | — | 3.1 |
| "architecture" | ARCHITECTURE | — | 3.1 |
| "pattern" | ARCHITECTURE | — | 3.1 |
| "design" (verb) | ARCHITECTURE | — | 3.1 |
| "PII/GDPR/encrypted/sensitive/credentials" | SECURITY | — | security |
| "write/add/create tests" | TEST | — | 5.x |
| "document/annotate/spec out" | DOCUMENT | — | 2.1 |
