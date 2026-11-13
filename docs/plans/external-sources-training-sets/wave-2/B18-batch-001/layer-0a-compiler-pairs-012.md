# Layer 0-A — Compiler Pairs (012)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-012-001 — Complete B18 compiler predicate rule table

**B18 Predicate Rule Table — All trigger words and outcomes:**

### IMPLEMENT triggers (always present in B18)

| Trigger word/phrase | TransitionType | Sub-phase signal | Rule |
|---|---|---|---|
| refactor | refactoring | 4.1 | Rule 1 |
| extract (code) | refactoring | 4.1 | Rule 1 |
| rename | refactoring | 4.1 | Rule 19 |
| consolidate | refactoring | 4.1 | Rule 20 |
| restructure | refactoring | 4.1 | Rule 1 |
| reorganize | refactoring | 4.1 | Rule 1 |
| move (code) | refactoring | 4.1 | Rule 1 |
| clean up | refactoring | 4.1 | Rule 1 |
| migrate (code) | refactoring or feature_addition | 4.1 | Rule 1/context |
| write migration | feature_addition | 4.4 | Rule 2 |
| create migration | feature_addition | 4.4 | Rule 2 |
| build migration | feature_addition | 4.4 | Rule 2 |
| seed | feature_addition | 4.4 | Rule 4 |
| fixture | feature_addition | 4.4 | Rule 4 |
| migrate (data) | feature_addition | 4.4 | Rule 5 |
| transform (data) | feature_addition | 4.4 | Rule 5 |
| ETL | feature_addition | 4.4 | Rule 5 |
| load (data) | feature_addition | 4.4 | Rule 5 |

---

## B18-0A-012-002 — ARCHITECTURE triggers and sub-phase routing

**ARCHITECTURE triggers (fires additional predicate):**

| Trigger word/phrase | Sub-phase | Rule |
|---|---|---|
| structure | 3.1 (default) or 3.5 | Rule 3 (always fires) |
| pattern | 3.1 | Rule 7 |
| architecture | 3.1 | Rule 7 |
| design (structural noun) | 3.1 | Rule 7 |
| redesign | 3.1 | Rule 7 |
| schema (paired with structural noun) | 3.1 | Rule 16 (borderline) |

**Sub-phase 3.1 vs 3.5:**
- 3.1 (Application/Domain Architecture): application structure, domain design, service architecture, database schema design
- 3.5 (Infrastructure Architecture): infrastructure pattern, deployment architecture, system architecture at infra level

**Default in B18:** 3.1 (most B18 architecture is application/domain level)

---

## B18-0A-012-003 — TEST, DOCUMENT, SECURITY triggers

**TEST triggers:**
| Trigger | Rule |
|---|---|
| write tests | Rule 12 |
| create tests | Rule 12 |
| add tests | Rule 12 |
| integration tests | Rule 12 |
| unit tests | Rule 12 |
| verify (test context) | Reinforcement |

**DOCUMENT triggers:**
| Trigger | Rule |
|---|---|
| document | Rule 11 |
| spec out | Rule 11 |
| write spec | Rule 11 |
| describe | Rule 11 |
| annotate | Rule 11 |

**SECURITY triggers (applicable in B18):**
| Trigger | Notes |
|---|---|
| GDPR (General Data Protection Regulation) | Data protection regulation |
| encrypted / encryption | Encryption vocabulary |
| PII (Personally Identifiable Information) | Personal data vocabulary |
| compliance | Security compliance vocabulary |
| security (in data context) | Security review trigger |

---

## B18-0A-012-004 — TransitionType determination rules

**TransitionType hierarchy (Rule 8):** refactoring > feature_addition > documentation > no contribution > none

**Determination algorithm:**
1. Check for refactoring verbs: refactor, restructure, rename, consolidate, extract, reorganize, move, clean up → TransitionType: refactoring (wins)
2. If no refactoring verb, check for feature_addition signals: write migration, create migration, seed, fixture, migrate (data), ETL, transform, load (data) → TransitionType: feature_addition
3. If neither: TransitionType based on dominant predicate

**Key conflict resolution:**
- "Write migration to consolidate" → feature_addition (Rule 2 wins; new migration artifact)
- "Consolidate classes" → refactoring (no migration artifact)
- "Refactor and write migration" → refactoring (refactoring > feature_addition)
- SchemaConsolidation (both present) → refactoring wins

---

## B18-0A-012-005 — Sub-phase co-occurrence vocabulary table

**Rule 6 vocabulary signals:**

| Vocabulary | Sub-phase |
|---|---|
| service, logic, class, function, module, repository, domain, rule, business, handler | 4.1 |
| connector, integration, webhook, API, external service | 4.3 |
| migrate, migration, seed, fixture, transform, ETL, data, schema change, database, batch, table, records | 4.4 |

**Conflict resolution when multiple sub-phases signal:**
- If both 4.1 and 4.4 vocabulary present → both sub-phases fire (IMPLEMENT routes to both)
- TransitionType still determined by verb (refactoring if refactoring verb present)

---

## B18-0A-012-006 — B18 absolute rules (cannot be overridden by context)

**The following rules are absolute in B18 — no contextual override permitted:**

1. **Rule 3:** "structure" ALWAYS fires ARCHITECTURE. No exceptions. Compound nouns (table structure, data structure, file structure, normalized structure) all fire. Context cannot suppress this.

2. **Rule 9:** "refactor" verb → TransitionType: refactoring. Always. Even if new files are created. Even if "write migration" also fires. Refactoring > feature_addition.

3. **Rule 6 (4.4 routing):** Data migration vocabulary → sub-phase 4.4. Data migration is never routed to 4.1.

4. **Rule 10 (No GapHandler):** B18 has no GapHandler. Gap vocabulary in B18 does not trigger a gap-handling flow.

5. **Rule 14 (DomainId):** DomainId is always "software-development" in B18. No exceptions.

**v6 corruptions violate:** Rule 3 (v6-01), Rule 9 (v6-02), Rule 6 4.4 routing (v6-03).
