# Held-Out and Corruption Pairs (001)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: held-out-corruption

## B18-HO-001-001 — Held-out overview: what held-out pairs test

**Held-out pairs test the trained classifier on novel inputs not seen in training.**

Each held-out pair has:
1. **HO (Held-Out) case:** Novel request → correct classification
2. **COR (Corruption) case:** Same or similar request → incorrect classification (what a corrupted classifier produces)

**B18 held-out pairs cover:**
- Files 002-012: testing edge cases, boundary conditions, and v6 corruption detection
- Total: 66 HO+COR pairs across 11 files

---

## B18-HO-001-002 — B18 held-out taxonomy

**Types of held-out cases:**

1. **Vocabulary edge cases:** request uses unusual phrasing for standard B18 actions
2. **Compound vocabulary:** request combines vocabulary from multiple sub-phases
3. **v6 corruption:** request that a v6 classifier would misclassify
4. **Disambiguation required:** request where context determines routing (migrate code vs data)
5. **Multiple predicates:** request fires 3-5 predicates correctly

---

## B18-HO-001-003 — B18 corruption taxonomy

**Types of corruptions in B18:**

1. **v6-01 type:** ARCHITECTURE predicate missing (suppressed on "structure" trigger)
2. **v6-02 type:** TransitionType: feature_addition when refactoring verb present
3. **v6-03 type:** 4.1 routing when 4.4 vocabulary present (data migration → BackendServiceFlow41)
4. **Predicate conflation:** treating refactoring vocabulary as feature_addition creation
5. **Sub-phase misassignment:** routing seed/fixture to 5.x instead of 4.4
6. **ARCHITECTURE suppression:** suppressing ARCHITECTURE in "pattern" or "design" contexts

---

## B18-HO-001-004 — Scoring criteria for held-out evaluation

**Each held-out case is evaluated on three dimensions:**

1. **Predicate accuracy:** Are the correct predicates present? No false positives, no false negatives.
2. **TransitionType accuracy:** Is the TransitionType correct (refactoring vs feature_addition)?
3. **Routing accuracy:** Are the correct sub-phases and flows selected?

**Pass conditions:**
- Predicate accuracy: 100% required (all correct predicates present; no extras)
- TransitionType accuracy: 100% required
- Routing accuracy: 100% required

**One failure in any dimension = failed held-out case.**

---

## B18-HO-001-005 — Corruption severity levels

**B18 corruption severity:**

| Severity | Type | Example |
|---|---|---|
| CRITICAL | v6-03 (wrong flow — data migration never runs) | Seeding/migration routed to BackendServiceFlow41 |
| HIGH | v6-02 (wrong Gate B type — false PASS) | Refactoring labeled as feature_addition |
| MEDIUM | v6-01 (schema design skipped) | ARCHITECTURE suppressed on "structure" |
| LOW | Sub-phase misassignment (seed → 5.x instead of 4.4) | Wrong flow selected but similar behavior |

---

## B18-HO-001-006 — B18 held-out files 002-012 roadmap

| File | HO topic | COR topic |
|---|---|---|
| 002 | "extract" class → refactoring, 4.1 | "extract" classified as feature_addition |
| 003 | "seed staging data" → 4.4 | seed routed to 4.1 |
| 004 | ETL pipeline → 4.4 | ETL → 4.3 API_CONTRACT (wrong) |
| 005 | "data structure" → ARCHITECTURE fires | "structure" in "data structure" suppressed |
| 006 | "consolidate repositories" → refactoring, 4.1 | consolidate → feature_addition |
| 007 | migration + ARCHITECTURE + TEST → three predicates | ARCHITECTURE suppressed |
| 008 | "rename + restructure" → IMPLEMENT, refactoring | restructure → ARCHITECTURE (wrong) |
| 009 | "write migration" vs "migrate data": both → 4.4 | migrate → refactoring (wrong) |
| 010 | GDPR migration with SECURITY → IMPLEMENT+SECURITY | SECURITY suppressed |
| 011 | Three v6 corruptions with examples | All three v6 corruption types |
| 012 | Four mixed held-out cases | Mixed corruption scenarios |
