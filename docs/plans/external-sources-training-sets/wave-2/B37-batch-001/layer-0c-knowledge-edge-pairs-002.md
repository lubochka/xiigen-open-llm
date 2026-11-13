# Layer 0-C — Knowledge-Edge Pairs (002)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-05 | pair_kind: knowledge-edge-pairs

---

## B37-0C-002-001 — "track" ≠ TEST: complete boundary explanation

**Knowledge edge: The "track" boundary**

"track" has NO entry in the TEST lexicon. This is the most critical boundary in B37.

**TEST lexicon (complete list):**
test, verify, assert, validate (actually QA not TEST), coverage, spec, case, suite, scenario, acceptance, unit, regression, e2e

"track" is NOT on this list. "track" fires NOTHING.

**Examples where "track" produces zero predicates:**
- "track progress" → []
- "track status" → []
- "track delivery" → []
- "track blockers" → []
- "track milestones" → []
- "track workstream" → []

**Examples where TEST fires:**
- "test the service" → [TEST]
- "unit test the module" → [TEST]
- "validate the output" → [QA]
- "verify the result" → [TEST]
- "acceptance criteria" → [TEST] on "acceptance"

**Why this matters:** A model trained on B36 or general vocabulary might associate "track" with testing/verification. In B37, "track" is exclusively project managing vocabulary with no predicate.

---

## B37-0C-002-002 — "track" vs "test": side-by-side comparison

| Token | Predicate | Gap ID if no predicate |
|-------|----------|----------------------|
| track | NONE | gap-project-managing-track-001 |
| test | TEST | — |
| verify | TEST | — |
| validate | QA | — |
| assert | TEST | — |
| coverage | TEST | — |
| review | QA | — |

**Critical: "track test coverage" → TEST fires on "test" and "coverage"; "track" is incidental and does not add or subtract from the predicate.**

---

## B37-0C-002-003 — "track" compound gap IDs

**All "track" gap IDs in B37:**

| Compound phrase | Gap ID |
|----------------|--------|
| track (standalone) | gap-project-managing-track-001 |
| track progress | gap-project-managing-track-progress-001 |
| track status / on-track / off-track | gap-project-managing-track-status-001 |

**Usage rule:** Use the most specific gap ID that matches the context. "track progress" → gap-project-managing-track-progress-001. "track delivery status" → gap-project-managing-track-status-001.

---

## B37-0C-002-004 — "track" with adjacent TEST-firing tokens

**Input:** "Track test coverage across all modules and report to the delivery manager."

**Analysis:**
- "Track" → gap (no predicate)
- "test" → TEST fires
- "coverage" → TEST fires
- "modules" → IMPLEMENT? No — "module" alone fires IMPLEMENT but "modules" in this context is general; however by lexical rule "module" fires IMPLEMENT
- "report" → DOCUMENT does NOT fire on "report"
- "delivery" → gap

Result: ["TEST", "IMPLEMENT"]

**Knowledge edge:** "track" is incidental here. TEST fires on "test" and "coverage". IMPLEMENT fires on "module". "track" contributes nothing to predicates.

---

## B37-0C-002-005 — Confirming "track" zero-predicate status: five test cases

1. "Track all project blockers" → [] (track + blockers = all gap)
2. "Track the RAID log updates" → [] (track + RAID = all gap)
3. "Track and coordinate workstreams" → [] (track + coordinate + workstreams = all gap)
4. "Track delivery dependencies" → [] (track + dependencies no API co-occurrence = all gap)
5. "Track the progress of each workstream" → [] (track + progress + workstream = all gap)

All five: DomainId "project-management", SCHEDULE_CAPABILITY_LEARNING emitted, knowledge_edge route.
