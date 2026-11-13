# Layer 0-C — Knowledge-Edge Pairs (024)
# Batch: B18-batch-001 | Scenario: B18 — Customer data migration and seeding scripts
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-024-001 — Complete knowledge edge reference: absolute vs contextual edges

**Absolute knowledge edges (cannot be suppressed by any context):**

| Token | Edge target | Rule |
|---|---|---|
| "structure" (noun) | ARCHITECTURE predicate | Rule 3 |
| "architecture" (noun) | ARCHITECTURE predicate | Rule 7 |
| "pattern" | ARCHITECTURE predicate | Rule 7 |
| "refactor" (verb) | IMPLEMENT + refactoring TransitionType | Rule 9 |
| "PII" | SECURITY predicate | Rule 14 |
| "encrypted" / "encryption" | SECURITY predicate | Rule 14 |
| "GDPR" | SECURITY predicate | Rule 14 |
| "credentials" | SECURITY predicate | Rule 14 |
| "authentication" | SECURITY predicate | — |

**Contextual knowledge edges (require co-occurrence to fire):**

| Token | Context required | Edge target |
|---|---|---|
| "schema" | + structural noun (structure/architecture) | ARCHITECTURE |
| "design" | structural noun context | ARCHITECTURE |
| "extract" | code target | refactoring + 4.1 |
| "extract" | data target | feature_addition + 4.4 |
| "move" | code target | refactoring + 4.1 |
| "move" | data target | feature_addition + 4.4 |
| "normalize" | any | NOT an ARCHITECTURE edge |

---

## B18-0C-024-002 — Complete knowledge edge reference: TransitionType edges

**TransitionType determination edges (Rule 8 hierarchy):**

| Token/phrase | TransitionType edge | Priority |
|---|---|---|
| refactor, extract (code), rename, consolidate, restructure, reorganize, move (code), clean up | refactoring | Highest (1) |
| write migration, create migration, build migration, seed, fixture, migrate (data), ETL, transform (data), load (data) | feature_addition | Second (2) |
| document, spec out, annotate | documentation | Third (3) |

**Conflict resolution rule:** higher priority edge wins. "refactor" + "migrate" → refactoring wins.

**Exception:** "write migration to [action]" → feature_addition wins over consolidate/restructure when "write migration" is the primary artifact creator (Rule 2).

---

## B18-0C-024-003 — Complete knowledge edge reference: sub-phase routing edges

**Sub-phase routing edge table:**

| Vocabulary node | Sub-phase edge | Flow |
|---|---|---|
| service, class, method, module, repository, domain, business logic, handler | 4.1 | BackendServiceFlow41 |
| connector, webhook, API (integration), external service | 4.3 | ApiContractFlow43 |
| migrate, migration, seed, fixture, ETL, transform (data), load (data), records, table, database, batch, CSV, archive | 4.4 | DataMigrationFlow44 |
| structure, pattern, architecture, design (structural) | 3.1 | ArchitectureFlow31 |
| write/add/create tests | 5.x | UnitTestingFlow51 |
| document, runbook, spec | 2.1 | DocumentationFlow21 |
| PII, GDPR, encrypted, credentials, authentication | security | SecurityReviewFlow |

---

## B18-0C-024-004 — Complete knowledge edge reference: compound noun edge preservation

**Compound nouns with absolute trigger tokens preserve the trigger edge:**

| Compound noun | Absolute token | ARCHITECTURE fires? |
|---|---|---|
| "data structure" | "structure" | YES |
| "table structure" | "structure" | YES |
| "address structure" | "structure" | YES |
| "index structure" | "structure" | YES |
| "billing schema structure" | "structure" | YES |
| "preference schema structure" | "structure" | YES |
| "audit log structure" | "structure" | YES |
| "normalized structure" | "structure" | YES |
| "schema" (alone) | none | NO |
| "structured data" | "structured" (adjective) | NO |

---

## B18-0C-024-005 — Complete knowledge edge reference: edge suppression rules

**Edges that CANNOT be suppressed:**
- Rule 3: "structure" → ARCHITECTURE (no suppression by any context)
- Rule 9: "refactor" → refactoring TransitionType (no suppression even if new files created)
- Rule 14: PII/encrypted/GDPR → SECURITY (no suppression in any scenario)
- Rule 14: DomainId → "software-development" (always; B18 absolute)

**Edges that CAN be modified by context:**
- "schema" → ARCHITECTURE only with structural noun co-present
- "extract" → refactoring (code) or ETL (data) depending on target vocabulary
- "move" → refactoring (code) or feature_addition (data) depending on target
- TransitionType: refactoring vs feature_addition → verb determines (Rule 8/9 hierarchy)
