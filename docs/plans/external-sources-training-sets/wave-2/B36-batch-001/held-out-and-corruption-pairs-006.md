# Held-Out and Corruption Pairs (006)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-006-001 — HELD-OUT: "document sprint goals" → DOCUMENT fires

**Input:** "Document the sprint goals and specify the definition of done."

**Expected output:**
- predicates: ["DOCUMENT"] ("document" fires; "specify" fires)
- KNOWLEDGE_EDGE: false
- DomainId: "software-development"
- CapabilityGapWarnings: sprint-001
- TransitionType: documentation
- PhaseId: 2.1

**Why this is a held-out case:**
Tests whether DOCUMENT fires on "document" in a sprint goals sentence. The sprint context must not suppress DOCUMENT.

---

## B36-HO-006-002 — Generalization test for B36-HO-006-001

The model generalizes correctly if it:
1. Fires DOCUMENT on "document" and "specify" (Rule 5)
2. Does NOT suppress DOCUMENT because subject is sprint goals
3. Sets DomainId: "software-development"
4. Emits sprint-001 as CapabilityGapWarning

---

## B36-COR-006-001 — CORRUPTION: DOCUMENT suppressed because context is planning

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Document the sprint goals."
Corrupt output: predicates: []
Corrupt reasoning: "Sprint goals are a project planning artifact. DOCUMENT routing would be inappropriate here — this should be KNOWLEDGE_EDGE."
```

**Why this is wrong:** "document" is in the DOCUMENT lexicon. It fires unconditionally. The nature of what is being documented (sprint goals vs software requirements) is irrelevant to the predicate firing.

**Correct output:**
- predicates: ["DOCUMENT"]
- DomainId: "software-development"
- CapabilityGapWarnings: sprint-001
- Routing: Phase 2.1

---

## B36-COR-006-002 — Downstream effect of suppression

When DOCUMENT is suppressed for "document sprint goals":
- KNOWLEDGE_EDGE state entered (wrong)
- DomainId: "project-management" (wrong)
- SCHEDULE_CAPABILITY_LEARNING emitted with sprint-001 (wrong — documenting sprint goals is handled by DOCUMENT)
- FinalHumanResult: null (wrong — DocumentationFlow would have produced output)

The user gets nothing when they asked for documentation of sprint goals. This is a routing failure caused by semantic suppression.
