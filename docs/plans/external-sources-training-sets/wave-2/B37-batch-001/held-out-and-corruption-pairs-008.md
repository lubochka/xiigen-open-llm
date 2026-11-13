# Held-Out and Corruption Pairs (008)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption-pairs

---

## B37-HO-008-001 — HO: "track test coverage" → TEST fires (on "test"/"coverage"), track is incidental

**Held-out request:** "Track test coverage across all modules and ensure adequate regression coverage."

**Expected output:**
- "Track" → gap (Rule 2: track ≠ TEST)
- "test" → TEST fires (Rule 21)
- "coverage" → TEST fires (Rule 2: coverage is in TEST lexicon)
- "modules" → IMPLEMENT fires on "module" (Rule 9) — "modules" plural qualifies
- "regression" → TEST fires (Rule 2: regression is in TEST lexicon)
- Predicates: ["TEST", "IMPLEMENT"]
- DomainId: "software-development"
- Routes: TestFlow5x + BackendServiceFlow41

**Teaching:** "Track" is incidental. TEST fires on "test", "coverage", "regression". IMPLEMENT fires on "modules". "track" adds nothing.

---

## B37-HO-008-002 — HO: "track" in TEST sentence is gap only

**Comparison:**
"Test coverage across modules." → ["TEST", "IMPLEMENT"] (test + coverage + modules)
"Track test coverage across modules." → ["TEST", "IMPLEMENT"] (same — "track" adds nothing)
"Track coverage across modules." → ["TEST", "IMPLEMENT"] (same — coverage + modules fire; track is incidental)

In all three: "track" never adds or subtracts a predicate.

---

## B37-HO-008-003 — COR: "track" fires TEST alongside "coverage" (wrong)

**Corrupted output:** "Track test coverage" → ["TEST", "TEST"] — double TEST for "track" and "test"/"coverage"

**Why this is wrong:** "track" does not fire TEST. TEST fires once for "test" and "coverage". No double-firing from "track".

**Correct output:** ["TEST"] — one TEST predicate, fired by "test" and/or "coverage" tokens.

---

## B37-HO-008-004 — HO: "track" in defect sentence

**Request:** "Track all open defects and test the resolution for each fixed item."

**Expected output:**
- "Track" → gap (Rule 2)
- "defects" → not in predicate lexicon
- "test" → TEST fires (Rule 21)
- "resolution" → not in predicate lexicon
- "fixed" → not in predicate lexicon
- Predicates: ["TEST"]
- DomainId: "software-development"
- Gap IDs: gap-project-managing-track-001 (track defects = managing activity)

---

## B37-HO-008-005 — HO: "track" only, no TEST tokens → gap

**Request:** "Track all defect statuses and report to the delivery manager."

**Expected output:**
- "Track" → gap (Rule 2)
- "defect statuses" → not in predicate lexicon
- "report" → DOCUMENT does NOT fire (Rule 3)
- "delivery" → gap
- "manager" → not in predicate lexicon
- Predicates: []
- DomainId: "project-management"
- Route: knowledge_edge
- SCL: emitted
- Gap IDs: gap-project-managing-track-status-001, gap-project-managing-status-report-001, gap-project-managing-manage-delivery-001

**Key difference from 008-004:** No "test" token → no TEST predicate → zero predicates → knowledge_edge. Adding "test" in the sentence fires TEST.
