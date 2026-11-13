# Held-Out and Corruption Pairs (005)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-005-001 — HELD-OUT: "architect the sprint structure" → ARCHITECTURE

**Input:** "Architect the sprint workflow structure and define the phase transitions."

**Expected output:**
- predicates: ["ARCHITECTURE", "DOCUMENT"] ("architect" fires ARCHITECTURE; "define" fires DOCUMENT)
- KNOWLEDGE_EDGE: false
- DomainId: "software-development"
- CapabilityGapWarnings: sprint-001
- TransitionType: documentation
- Phases: 3.1 + 2.1

**Why this is a held-out case:**
Tests whether "architect" fires ARCHITECTURE even in a sprint workflow sentence. "define" also fires DOCUMENT. Both must fire. The sprint context must not suppress either.

---

## B36-HO-005-002 — Generalization test for B36-HO-005-001

The model generalizes correctly if it:
1. Fires ARCHITECTURE on "architect" (Rule 4)
2. Fires DOCUMENT on "define" (Rule 5)
3. Does NOT suppress either because "sprint" is planning vocabulary
4. Sets DomainId: "software-development" (predicates fired)
5. Routes to Phase 3.1 + 2.1

---

## B36-COR-005-001 — CORRUPTION: "sprint" suppresses ARCHITECTURE

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Architect the sprint workflow structure."
Corrupt output: predicates: []
Corrupt reasoning: "This is about sprint workflow — a project planning task. ARCHITECTURE is for software systems, not sprint workflows."
```

**Why this is wrong:** "architect" is in the ARCHITECTURE lexicon. It fires regardless of the object being "sprint workflow" or "software system." The compiler does not evaluate whether the architectural subject is a software system or a project management artifact.

**Correct output:**
- predicates: ["ARCHITECTURE"]
- DomainId: "software-development"
- CapabilityGapWarnings: sprint-001

---

## B36-COR-005-002 — ARCHITECTURE fires on "structure" too

**Additional held-out variant:**

Input: "Structure the sprint milestones into phases."
- "structure" fires ARCHITECTURE (Rule 4)
- sprint: GAP, milestone: GAP
- Expected: predicates: ["ARCHITECTURE"]

This tests whether "structure" as a verb fires ARCHITECTURE even in sprint milestone context. It must.
