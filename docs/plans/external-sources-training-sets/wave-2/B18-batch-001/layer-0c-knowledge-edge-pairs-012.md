# Layer 0-C — Knowledge-Edge Pairs (012)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: knowledge-edge

## B18-0C-012-001 — B18 knowledge edge summary: all rules in one reference table

**Complete B18 rule reference:**

| Rule # | Rule name | Rule statement | Impact |
|---|---|---|---|
| Rule 1 | Refactoring vocabulary | refactor/extract/rename/consolidate/restructure/reorganize/move/clean up → IMPLEMENT, refactoring | TransitionType: refactoring |
| Rule 2 | Migration creation | write/create/build migration → IMPLEMENT, feature_addition | TransitionType: feature_addition; 4.4 |
| Rule 3 | Structure absolute | "structure" always fires ARCHITECTURE | Adds ARCHITECTURE; sub-phase 3.1 |
| Rule 4 | Seed/fixture | seed/fixture → IMPLEMENT, feature_addition, 4.4 | Routes to DataMigrationFlow44 |
| Rule 5 | ETL/data vocabulary | ETL/transform (data)/load (data)/migrate (data) → IMPLEMENT, feature_addition, 4.4 | Routes to DataMigrationFlow44 |
| Rule 6 | Sub-phase vocabulary | Co-occurrence vocabulary determines 4.1 vs 4.3 vs 4.4 | Sub-phase routing |
| Rule 7 | Architecture triggers | architecture/pattern/design (structural) → ARCHITECTURE | Adds ARCHITECTURE; 3.1 |
| Rule 8 | TransitionType hierarchy | refactoring > feature_addition > documentation | Conflict resolution |
| Rule 9 | Refactor override | "refactor" → refactoring overrides feature_addition | TransitionType: refactoring wins |
| Rule 10 | No GapHandler | B18 has no GapHandler | GapHandler never fires in B18 |
| Rule 11 | Document | document/spec out/annotate/describe → DOCUMENT | Adds DOCUMENT; 2.1 |
| Rule 12 | Test | write/create/add tests → TEST | Adds TEST; 5.x |
| Rule 13 | IMPLEMENT+ARCHITECTURE | When both fire, route to both sub-phases | Dual routing |
| Rule 14 | DomainId | DomainId: "software-development" always | Context setting |
| Rule 15 | v6 corruptions | v6-01 (structure suppressed), v6-02 (refactor→feature_addition), v6-03 (4.4→4.1) | Anti-corruption patterns |
| Rule 16 | Schema borderline | "schema" alone → NOT ARCHITECTURE; schema + structural noun → ARCHITECTURE | Conditional trigger |
| Rule 17 | Data migration TransitionType | Data migration → feature_addition (new artifact created) | TransitionType: feature_addition |
| Rule 18 | Code refactoring TransitionType | Code refactoring → refactoring (existing code restructured) | TransitionType: refactoring |
| Rule 19 | Rename | rename → IMPLEMENT, refactoring | TransitionType: refactoring |
| Rule 20 | Consolidate | consolidate → IMPLEMENT, refactoring | TransitionType: refactoring |

---

## B18-0C-012-002 — Most common B18 edge cases and resolutions

**Edge case 1:** "migrate" in ambiguous context
- Resolution: check for data vocabulary (records, table, database) → 4.4. Check for code vocabulary (codebase, framework) → 4.1. Default: 4.4 in B18.

**Edge case 2:** "structure" in compound noun
- Resolution: always fires ARCHITECTURE (Rule 3, absolute). No exception.

**Edge case 3:** "extract" ambiguity
- Resolution: code target (class, method) → refactoring, 4.1. Data target (records, file) → feature_addition, 4.4.

**Edge case 4:** "consolidate" + "write migration"
- Resolution: if migration is the primary artifact, feature_addition (Rule 2). If consolidation is the primary action, refactoring (Rule 9).

**Edge case 5:** "rename" + "database column"
- Resolution: rename fires refactoring TransitionType always. Sub-phase: 4.4 if database vocabulary present.

---

## B18-0C-012-003 — B18 predicate count distribution (expected)

**Expected predicate count in B18 requests:**

| Predicate count | Frequency | Example |
|---|---|---|
| 1 predicate: [IMPLEMENT] | Most common (60%) | Simple migration or refactoring |
| 2 predicates: [IMPLEMENT, ARCHITECTURE] | Common (20%) | Migration + structure/pattern |
| 2 predicates: [IMPLEMENT, TEST] | Moderate (8%) | Migration + tests |
| 2 predicates: [IMPLEMENT, DOCUMENT] | Moderate (7%) | Migration + documentation |
| 3 predicates | Less common (4%) | Migration + design + tests |
| 4-5 predicates | Rare (1%) | Full GDPR migration scenario |

---

## B18-0C-012-004 — DomainId and scenario context edges

**B18 is always:**
- DomainId: "software-development"
- Scenario: "B18 — Development — refactoring and data migration"
- Sub-phases in scope: 4.4, 4.1, 3.1, 5.x, 2.1, security
- Sub-phases NOT in scope: 4.3 (unless explicit external API vocabulary), 3.5 (unless explicit infrastructure vocabulary)

---

## B18-0C-012-005 — B18 knowledge edge confidence levels

**High confidence edges (apply literally, no ambiguity):**
- "structure" → ARCHITECTURE (Rule 3)
- "refactor" → refactoring TransitionType (Rule 9)
- "seed" → IMPLEMENT, 4.4 (Rule 4)
- "ETL" → IMPLEMENT, 4.4 (Rule 5)
- "write migration" → IMPLEMENT, feature_addition, 4.4 (Rule 2)
- "pattern" → ARCHITECTURE (Rule 7)
- "architecture" → ARCHITECTURE (Rule 7)

**Medium confidence edges (require context):**
- "migrate" → 4.4 vs 4.1 (depends on data vs code context)
- "extract" → refactoring vs feature_addition (depends on target type)
- "schema" → ARCHITECTURE or not (depends on paired nouns)
- "transform" → 4.4 vs other (depends on data vs code context)

**Low confidence edges (borderline):**
- "load" → may or may not be 4.4 (requires strong data context)
- "design" → ARCHITECTURE or not (depends on noun context)

---

## B18-0C-012-006 — B18 knowledge edge: final summary

**Seven key knowledge edges to memorize for B18:**

1. **"structure" → ARCHITECTURE** (absolute; no context suppression)
2. **"refactor" → refactoring TransitionType** (overrides feature_addition; Rule 9)
3. **"seed"/"fixture" → IMPLEMENT, feature_addition, 4.4** (not 5.x)
4. **"ETL"/"transform"/"load" (data) → IMPLEMENT, feature_addition, 4.4** (not 4.3)
5. **"write migration" → IMPLEMENT, feature_addition, 4.4** (new artifact; Rule 2)
6. **Data migration → DataMigrationFlow44 (4.4)**, not BackendServiceFlow41 (4.1)
7. **DomainId → "software-development"** (always in B18)
