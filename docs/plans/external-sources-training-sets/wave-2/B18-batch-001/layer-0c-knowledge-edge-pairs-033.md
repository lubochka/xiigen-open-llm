# Layer 0-C — Knowledge-Edge Pairs (033)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-033-001 — Edge interaction: SECURITY + ARCHITECTURE co-occurrence in DDL

**When both SECURITY and ARCHITECTURE fire in the same request:**

| Scenario | ARCHITECTURE trigger | SECURITY trigger | Combined predicates |
|---|---|---|---|
| "Design the PII-safe index structure, encrypt columns" | "structure" (ARCH) | PII + encrypted (SEC) | [IMPLEMENT, ARCHITECTURE, SECURITY] |
| "trigger architecture for GDPR audit" | "architecture" (ARCH) | GDPR (SEC) | [IMPLEMENT, ARCHITECTURE, SECURITY] |
| "JSONB column structure with encrypted PII" | "structure" (ARCH) | PII + encrypted (SEC) | [IMPLEMENT, ARCHITECTURE, SECURITY] |
| "null-safety pattern for compliance fields" | "pattern" (ARCH) | compliance (SEC) | [IMPLEMENT, ARCHITECTURE, SECURITY] |

**Both predicates apply simultaneously; they do not suppress each other.**

---

## B18-0C-033-002 — Edge interaction: TEST + ARCHITECTURE co-occurrence

**When both TEST and ARCHITECTURE fire:**

| Scenario | ARCHITECTURE trigger | TEST trigger | Combined predicates |
|---|---|---|---|
| "design index structure, write tests for query performance" | "structure" (ARCH) | "write tests" (TEST) | [IMPLEMENT, ARCHITECTURE, TEST] |
| "partition pattern, write integration tests" | "pattern" (ARCH) | "write integration tests" | [IMPLEMENT, ARCHITECTURE, TEST] |

**Execution order:** ArchitectureFlow31 → DataMigrationFlow44 → UnitTestingFlow51 (all sequential)

---

## B18-0C-033-003 — Edge interaction: DOCUMENT + SECURITY co-occurrence

**When both DOCUMENT and SECURITY fire:**

| Scenario | SECURITY trigger | DOCUMENT trigger | Combined predicates |
|---|---|---|---|
| "trigger for PII audit, document the GDPR rules" | PII + GDPR (SEC) | "document" (DOC) | [IMPLEMENT, SECURITY, DOCUMENT] |
| "JSONB with compliance fields, document access policy" | compliance (SEC) | "document" (DOC) | [IMPLEMENT, SECURITY, DOCUMENT] |

**Execution order:** DataMigrationFlow44 → SecurityReviewFlow ‖ DocumentationFlow21 (security and doc parallel after IMPLEMENT)

---

## B18-0C-033-004 — Full five-predicate edge firing in DDL context

**When all five predicates fire from a DDL migration request:**

**Example request:** "Design the encrypted index structure for the PII fields table, write the migration, write integration tests, and document the security policy."

| Token | Edge fired | Predicate |
|---|---|---|
| "design" | ARCHITECTURE (Rule 11) | ARCHITECTURE |
| "encrypted index structure" | ARCHITECTURE (structure, Rule 3) + SECURITY (encrypted, Rule 14) | ARCHITECTURE + SECURITY |
| "PII fields" | SECURITY (Rule 14) | SECURITY |
| "write the migration" | IMPLEMENT (Rule 2) | IMPLEMENT |
| "write integration tests" | TEST (Rule 12) | TEST |
| "document the security policy" | DOCUMENT (Rule 4) | DOCUMENT |

**Output predicates:** [IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT]

---

## B18-0C-033-005 — Edge counting: how many predicates maximum in B18?

**Maximum predicate count: 5**

| Predicate | Maximum fires (in one request) | Output count |
|---|---|---|
| IMPLEMENT | unlimited (multiple IMPLEMENT tokens) | 1 |
| ARCHITECTURE | unlimited (structure + architecture + pattern + design) | 1 |
| SECURITY | unlimited (PII + GDPR + encrypted + compliance + credentials) | 1 |
| TEST | 1 (verb form only: write/add/create tests) | 1 |
| DOCUMENT | 1 (document/annotate/describe/write runbook) | 1 |

**Rule:** Regardless of how many times a predicate fires, it produces exactly 1 predicate in the output.
