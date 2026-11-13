# Layer 0-C — Knowledge-Edge Pairs (018)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-018-001 — "accountability" knowledge edge: ownership and RACI vocabulary

**Knowledge edge: accountability is B37 gap vocabulary**

Accountability in project management refers to clear ownership of deliverables, decisions, and outcomes. RACI (Responsible, Accountable, Consulted, Informed) matrices define accountability. Neither "accountability" nor "RACI" nor "ownership" are in any predicate lexicon.

**Gap ID:** gap-project-managing-accountability-001

**Zero-predicate accountability scenarios:**
- "Establish accountability for all delivery outcomes." → []
- "Clarify ownership across workstream leads." → []
- "Assign RACI for each project activity." → []
- "Confirm delivery accountability with the team." → []
- "Coordinate accountability across programme workstreams." → []

---

## B37-0C-018-002 — "accountability structure" boundary: ARCHITECTURE fires on "structure"

**Knowledge edge: "accountability structure" fires ARCHITECTURE on the "structure" token**

This is the B37 "structure" false positive — the most common false positive in the extended vocabulary (013-024 range).

| Input | Predicates | Reason |
|-------|-----------|--------|
| "Define the accountability structure" | [DOCUMENT, ARCHITECTURE] | "define" fires DOCUMENT; "structure" fires ARCHITECTURE |
| "Map the accountability structure" | [ARCHITECTURE] | "structure" fires ARCHITECTURE |
| "Establish accountability" | [] | no predicate trigger tokens |
| "Clarify the accountability framework" | [] | "framework" ≠ "structure" |
| "Design the accountability model" | [ARCHITECTURE] | "design" fires ARCHITECTURE |

**Teaching point:** The word "structure" (or "design") in any accountability-related sentence fires ARCHITECTURE. "Framework", "model", "approach" do not fire ARCHITECTURE.

---

## B37-0C-018-003 — "accountability" compound gap IDs

**All accountability-related gap IDs:**

| Vocabulary | Gap ID |
|-----------|--------|
| accountability | gap-project-managing-accountability-001 |
| ownership | gap-project-managing-accountability-001 |
| RACI | gap-project-managing-accountability-001 |
| escalation + accountability | gap-project-managing-escalation-accountability-001 |

**Compound gap ID:** When "escalation" and "accountability" both appear in the same sentence, the compound gap ID gap-project-managing-escalation-accountability-001 fires alongside the individual gap IDs.

---

## B37-0C-018-004 — RACI matrix and IMPLEMENT false positive

**Input:** "Build the RACI matrix for the project."

Knowledge edge:
- "build" fires IMPLEMENT (Rule 9)
- "RACI" → gap, no predicate
- Result: ["IMPLEMENT"], DomainId "software-development"
- Gap warning: gap-project-managing-accountability-001

This is the IMPLEMENT false positive for accountability vocabulary. "Build" in the managing context refers to creating a management artefact, but the compiler fires IMPLEMENT on "build" lexically.

**Contrast:** "Prepare the RACI matrix" → [] (zero predicates; "prepare" not in any lexicon)

---

## B37-0C-018-005 — Five test cases for accountability

1. "Establish accountability for all workstream leads." → []
2. "Coordinate RACI and accountability across teams." → []
3. "Define the accountability structure." → [DOCUMENT, ARCHITECTURE]
4. "Build the RACI matrix." → [IMPLEMENT]
5. "Review the accountability framework." → [QA]

Cases 1-2: knowledge_edge, SCL emitted.
Cases 3-5: routed to flows, no SCL, gap-project-managing-accountability-001 forwarded.
