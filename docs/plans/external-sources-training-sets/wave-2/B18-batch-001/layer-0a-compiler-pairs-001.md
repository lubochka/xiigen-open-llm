# Layer 0-A — Compiler Pairs (001)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: compiler

## B18-0A-001-001 — "refactor" fires IMPLEMENT with TransitionType: refactoring

**Request:** "Refactor the CustomerRepository to extract the query-building logic into a separate QueryBuilder class."

**Compiler trace:**
- Token: "refactor" → IMPLEMENT (refactoring vocabulary)
- Token: "extract" → IMPLEMENT (refactoring vocabulary)
- Token: "class" → sub-phase signal 4.1 (service/class vocabulary)
- TransitionType: refactoring (refactor verb dominates)
- No ARCHITECTURE trigger (no "pattern", "structure", "architecture", "design" noun)

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1

---

## B18-0A-001-002 — "migrate" fires IMPLEMENT with TransitionType: feature_addition

**Request:** "Migrate all customer records from the legacy flat schema to the new normalized customers/addresses/contacts tables."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT (data migration vocabulary)
- Token: "schema" → borderline; paired with no structural noun → ARCHITECTURE does NOT fire
- TransitionType: feature_addition (new artifact created — migration script)
- Sub-phase: 4.4 (migrate, data, schema change vocabulary)

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4

---

## B18-0A-001-003 — "structure" always fires ARCHITECTURE

**Request:** "Migrate all product records to the new normalized table structure."

**Compiler trace:**
- Token: "migrate" → IMPLEMENT
- Token: "table" → 4.4 sub-phase signal
- Token: "structure" → ARCHITECTURE (literal compiler rule — always fires on "structure")
- TransitionType: feature_addition (migration creates new artifact; no "refactor" verb)

**Result:** predicates: ["IMPLEMENT", "ARCHITECTURE"], TransitionType: feature_addition, sub-phases: 4.4 + 3.1

---

## B18-0A-001-004 — DomainId is always "software-development" in B18

**Request:** "Build an ETL pipeline to transform legacy order CSV exports into the new analytics warehouse schema."

**Compiler trace:**
- Token: "build" → IMPLEMENT
- Token: "ETL" (Extract/Transform/Load) → IMPLEMENT; 4.4 routing signal
- Token: "transform" → IMPLEMENT; 4.4 routing signal
- DomainId: "software-development" (always in B18)
- TransitionType: feature_addition

**Result:** predicates: ["IMPLEMENT"], TransitionType: feature_addition, sub-phase: 4.4, DomainId: "software-development"

---

## B18-0A-001-005 — No GapHandler in B18

**Request:** "Consolidate the authentication logic scattered across three service classes into a single AuthService."

**Compiler trace:**
- Token: "consolidate" → IMPLEMENT (refactoring vocabulary)
- Token: "service" → 4.1 sub-phase signal
- Token: "class" → 4.1 sub-phase signal
- GapHandler: NOT present (B18 has no gap vocabulary)
- TransitionType: refactoring

**Result:** predicates: ["IMPLEMENT"], TransitionType: refactoring, sub-phase: 4.1, no GapHandler

---

## B18-0A-001-006 — B18 compiler predicate rule summary (overview)

**Core predicate triggers for B18:**

| Trigger word/phrase | Predicate | TransitionType | Sub-phase |
|---|---|---|---|
| refactor, extract, rename, consolidate, restructure, reorganize, move, clean up | IMPLEMENT | refactoring | 4.1 |
| write migration, create migration, build migration | IMPLEMENT | feature_addition | 4.4 |
| seed, fixture | IMPLEMENT | feature_addition | 4.4 |
| migrate (data context) | IMPLEMENT | feature_addition | 4.4 |
| ETL, transform, load (data) | IMPLEMENT | feature_addition | 4.4 |
| document, spec out, annotate | DOCUMENT | — | — |
| write tests, add tests | TEST | — | — |
| structure, pattern, architecture, design (structural) | ARCHITECTURE | — | 3.1 or 3.5 |

**DomainId:** "software-development" always in B18.
**GapHandler:** not present in B18.
