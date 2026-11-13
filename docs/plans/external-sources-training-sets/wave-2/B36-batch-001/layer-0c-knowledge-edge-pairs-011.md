# Layer 0-C — Knowledge-Edge Pairs (011)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B36-0C-011-001 — Mixed-signal handling: predicates dominate

**B36 Rule 11 (full statement):**
When predicates fire alongside gap vocabulary → CapabilityGapWarnings only; routing proceeds normally based on fired predicates. Gap vocabulary does NOT alter routing.

**Three components of mixed-signal handling:**

1. **Predicates fire:** One or more predicates match lexicon tokens in the request
2. **Gap vocabulary present:** One or more B36 gap terms also appear in the request
3. **Outcome:** Routing follows predicates; gap terms produce CapabilityGapWarnings

---

## B36-0C-011-002 — Mixed-signal example: IMPLEMENT + sprint vocabulary

**Input:** "Implement the sprint authentication feature."

**Signal analysis:**
- IMPLEMENT fires ("implement", "feature")
- Sprint vocabulary present ("sprint")
- Mixed-signal state: IMPLEMENT predicate + sprint gap vocabulary

**Outcome:**
- predicates: ["IMPLEMENT"]
- Route: Phase 4.1 (IMPLEMENT drives routing — not the sprint vocabulary)
- CapabilityGapWarning: gap-project-planning-sprint-not-captured
- DomainId: "software-development"
- FinalHumanResult: from Phase 4.1 (not null)

---

## B36-0C-011-003 — Mixed-signal: gap vocabulary never becomes a routing input

**What gap vocabulary does NOT do in mixed-signal:**

| Action | Does gap vocab do this? |
|--------|------------------------|
| Select a phase | NO |
| Override predicate routing | NO |
| Set DomainId to "project-management" | NO |
| Trigger KNOWLEDGE_EDGE state | NO |
| Suppress a fired predicate | NO |
| Change TransitionType | NO |

**What gap vocabulary DOES do in mixed-signal:**

| Action | Does gap vocab do this? |
|--------|------------------------|
| Emit CapabilityGapWarning | YES |
| Notify AutonomousLearningPipeline | YES (via CapabilityGapWarnings channel) |
| Appear in RouterResult.capabilityGapWarnings | YES |

---

## B36-0C-011-004 — Mixed-signal vs KNOWLEDGE_EDGE: the dividing line

**The dividing line is binary:**

- predicates: [] → KNOWLEDGE_EDGE (gap vocabulary is the only signal)
- predicates: [anything] → Normal routing (gap vocabulary is supplementary)

There is no "partial knowledge edge" or "soft knowledge edge" state. As soon as one predicate fires, the system is in normal routing mode. The gap vocabulary becomes supplementary context, not a routing input.

This applies regardless of how many gap terms are present. Even ten gap terms plus one predicate: normal routing, not knowledge_edge.
