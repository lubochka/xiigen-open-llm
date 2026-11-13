# Held-Out and Corruption Pairs (008)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-008-001 — HELD-OUT: "run standup" → knowledge_edge

**Input:** "Run the daily standup and capture the blockers from the team."

**Expected output:**
- predicates: [] ("run" not in any lexicon — Rule 18; "capture" not in DOCUMENT lexicon — Rule 14)
- KNOWLEDGE_EDGE: true
- DomainId: "project-management"
- CapabilityGapWarnings: standup-001
- SCHEDULE_CAPABILITY_LEARNING: emitted with standup-001
- FinalHumanResult: null

**Why this is a held-out case:**
Tests whether the model correctly applies Rule 18 ("run" not in any predicate lexicon) and Rule 14 ("capture" not in DOCUMENT lexicon) simultaneously in a standup context.

---

## B36-HO-008-002 — Generalization test for B36-HO-008-001

The model generalizes correctly if it:
1. Does NOT fire IMPLEMENT on "run" (Rule 18)
2. Does NOT fire DOCUMENT on "capture" (Rule 14)
3. Emits standup-001 gap warning
4. Enters KNOWLEDGE_EDGE
5. Emits SCHEDULE_CAPABILITY_LEARNING

---

## B36-COR-008-001 — CORRUPTION: "run" fires IMPLEMENT

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Run the daily standup."
Corrupt output: predicates: ["IMPLEMENT"]
Corrupt reasoning: "run" is similar to "execute" or "start" — treating it as triggering implementation.
```

**Why this is wrong:** "run" is not in the IMPLEMENT lexicon. IMPLEMENT tokens are: implement, build, create, write (code), feature. "run" is not one of them. In ceremony context, "run the standup" means facilitation — a planning activity, not software implementation.

**Correct output:**
- predicates: []
- KNOWLEDGE_EDGE: true
- standup-001

---

## B36-COR-008-002 — CORRUPTION: "capture" fires DOCUMENT

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Capture the sprint retrospective notes."
Corrupt output: predicates: ["DOCUMENT"]
Corrupt reasoning: "capture" is similar to "document" — both involve recording information.
```

**Why this is wrong:** "capture" is specifically excluded from the DOCUMENT lexicon (B36 Rule 14). While "capture" semantically resembles "document," they are different tokens with different lexicon registrations. "document" fires DOCUMENT. "capture" does not.

**Correct output:**
- predicates: []
- KNOWLEDGE_EDGE: true
- CapabilityGapWarnings: sprint-001, retrospective-001
