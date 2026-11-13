# Held-Out and Corruption Pairs (009)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-009-001 — HELD-OUT: "update the backlog" → knowledge_edge (no code-level context)

**Input:** "Update the backlog with the new tickets from the epic."

**Expected output:**
- predicates: [] ("update" has no code-level co-occurrence — Rule 12)
- KNOWLEDGE_EDGE: true
- DomainId: "project-management"
- CapabilityGapWarnings: backlog-001, ticket-001, epic-001
- SCHEDULE_CAPABILITY_LEARNING: emitted
- FinalHumanResult: null

**Why this is a held-out case:**
Tests Rule 12: "update" in planning context does NOT fire IMPLEMENT. The sentence has no code-level entities (function, class, module, logic, code, service).

---

## B36-HO-009-002 — Generalization test for B36-HO-009-001

The model generalizes correctly if it:
1. Does NOT fire IMPLEMENT on "update" alone (no code-level entity present)
2. Enters KNOWLEDGE_EDGE
3. Emits backlog-001, ticket-001, epic-001 warnings
4. Emits SCHEDULE_CAPABILITY_LEARNING

**Contrast case (should fire IMPLEMENT):**
Input: "Update the backlog service logic."
Expected: predicates: ["IMPLEMENT"] — "service" and "logic" are code-level entities, IMPLEMENT fires via update + code-level co-occurrence. Actually: wait — does update + service/logic fire IMPLEMENT? Per Rule 12, "update" needs code-level co-occurrence. "service" and "logic" are code-level → yes, IMPLEMENT fires.

---

## B36-COR-009-001 — CORRUPTION: "update" fires IMPLEMENT in planning context

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Update the backlog with the new tickets."
Corrupt output: predicates: ["IMPLEMENT"]
Corrupt reasoning: "update" triggers IMPLEMENT because updating is a software action.
```

**Why this is wrong:** Rule 12 — "update" in project/planning context does NOT fire IMPLEMENT. It needs code-level co-occurrence (function/class/logic/module/code). "backlog" and "tickets" are planning artifacts, not code-level entities.

**Correct output:**
- predicates: []
- KNOWLEDGE_EDGE: true

---

## B36-COR-009-002 — The "update" boundary is a frequent confusion point

Models commonly confuse "update" with IMPLEMENT because:
1. "update the function" → IMPLEMENT (correct)
2. "update the backlog" → NOT IMPLEMENT (correct — planning context)

The test is: what is being updated? Code-level entity → IMPLEMENT fires. Planning artifact → no predicate fires.

Training on these pairs helps the model learn the update boundary without over-generalizing.
