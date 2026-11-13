# Held-Out and Corruption Pairs (012)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-012-001 — Four mixed held-out cases: Case 1 — DOCUMENT+IMPLEMENT with estimation intent

**Input:** "Write the acceptance criteria for each epic and estimate the story points."

**Expected output:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
  - "write" → IMPLEMENT
  - "criteria" → DOCUMENT
  - "story" → DOCUMENT
  - "estimate" → GAP (estimation-001)
  - "epic" → GAP (epic-001)
- CapabilityGapWarnings: estimation-001, epic-001, story-point-001
- DomainId: "software-development"
- Routing: Phase 4.1 + 2.1

---

## B36-HO-012-002 — Four mixed held-out cases: Case 2 — capacity + design (ARCHITECTURE fires)

**Input:** "Design the team capacity allocation model for the next sprint."

**Expected output:**
- predicates: ["ARCHITECTURE"] ("design" fires ARCHITECTURE)
- CapabilityGapWarnings: capacity-001, sprint-001
- DomainId: "software-development"
- Routing: Phase 3.1

**Key:** "design" fires ARCHITECTURE. The fact that the object is "team capacity allocation" (a planning concept) does not suppress ARCHITECTURE.

---

## B36-HO-012-003 — Four mixed held-out cases: Case 3 — "define" in sprint context (DOCUMENT fires)

**Input:** "Define the sprint exit criteria and the burndown completion threshold."

**Expected output:**
- predicates: ["DOCUMENT"] ("define" fires DOCUMENT; "criteria" fires DOCUMENT)
- CapabilityGapWarnings: sprint-001, burndown-001
- DomainId: "software-development"
- Routing: Phase 2.1

**Key:** "define" and "criteria" both fire DOCUMENT. Sprint and burndown are planning gap warnings.

---

## B36-HO-012-004 — Four mixed held-out cases: Case 4 — pure gap with five planning terms

**Input:** "Run the sprint retrospective, track the burndown, and groom the epic backlog."

**Expected output:**
- predicates: [] (run, track, groom are all neutral; retrospective, burndown, epic, backlog are all gap)
- KNOWLEDGE_EDGE: true
- DomainId: "project-management"
- CapabilityGapWarnings: sprint-001 (implicit in "sprint retrospective"), retrospective-001, burndown-001, epic-001, backlog-001
- SCHEDULE_CAPABILITY_LEARNING: ONE signal with all five gap IDs
- FinalHumanResult: null

**Key:** Five gap terms, zero predicates. One SCHEDULE_CAPABILITY_LEARNING with all IDs bundled. This is the maximum-complexity pure-gap case in B36 — tests that the model does not fire a predicate on "run", "track", or "groom."
