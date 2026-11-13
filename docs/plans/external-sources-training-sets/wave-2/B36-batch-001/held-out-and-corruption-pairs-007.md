# Held-Out and Corruption Pairs (007)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## B36-HO-007-001 — HELD-OUT: "implement epic features" → IMPLEMENT fires

**Input:** "Implement the features from the Q3 epic and mark the sprint tickets as done."

**Expected output:**
- predicates: ["IMPLEMENT"] ("implement" and "features" fire IMPLEMENT)
- KNOWLEDGE_EDGE: false
- DomainId: "software-development"
- CapabilityGapWarnings: epic-001, sprint-001, ticket-001
- TransitionType: feature_addition
- PhaseId: 4.1

**Why this is a held-out case:**
Tests IMPLEMENT firing when embedded in heavy epic/sprint/ticket vocabulary. Three gap terms present alongside two IMPLEMENT tokens.

---

## B36-HO-007-002 — Generalization test for B36-HO-007-001

The model generalizes correctly if it:
1. Fires IMPLEMENT on "implement" and "features"
2. Does NOT suppress IMPLEMENT because epic/sprint/ticket are planning vocabulary
3. Sets DomainId: "software-development"
4. Emits epic-001, sprint-001, ticket-001 as CapabilityGapWarnings

---

## B36-COR-007-001 — CORRUPTION: DomainId wrongly set to "project-management"

**Corrupt input/output (DO NOT REPRODUCE):**

```
Input: "Implement the features from the Q3 epic and mark the sprint tickets as done."
predicates: ["IMPLEMENT"]  ← correct predicate
DomainId: "project-management"  ← WRONG
Corrupt reasoning: "The sentence is mostly about epic/sprint/tickets (planning). DomainId should be project-management."
```

**Why this is wrong:** Any predicate fires → DomainId: "software-development". Period. The proportion of planning vocabulary does not override this rule.

**Correct output:**
- predicates: ["IMPLEMENT"]
- DomainId: "software-development" (IMPLEMENT fired)

---

## B36-COR-007-002 — DomainId corruption cascade

When DomainId is wrongly set to "project-management" with non-empty predicates:
- Downstream domain-specific formatting uses project-management template (wrong)
- Domain-specific response persona uses planning advisor context (wrong)
- Phase 4.1 output may be formatted as a project plan instead of implementation code
- Any downstream system that branches on DomainId gets incorrect data

This is why v6-03 is a significant corruption — it affects every downstream consumer of DomainId.
