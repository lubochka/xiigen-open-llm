# Layer 0-C — Knowledge-Edge Pairs (017)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: knowledge-edge-pairs

---

## B37-0C-017-001 — "lessons-learned" knowledge edge: retrospective vocabulary

**Knowledge edge: lessons-learned is project retrospective gap vocabulary**

A lessons-learned session (also called a retrospective or post-implementation review) captures what went well, what didn't, and improvement actions. "Lessons-learned", "lessons learned", and related retrospective vocabulary are B37 gap vocabulary with no registered predicate.

**Gap ID:** gap-project-managing-manage-delivery-001

**Zero-predicate lessons-learned scenarios:**
- "Run a lessons-learned session." → []
- "Facilitate the lessons-learned workshop." → []
- "Collect lessons learned from the team." → []
- "Analyse lessons learned from the programme." → []
- "Share lessons learned across workstreams." → []

---

## B37-0C-017-002 — "lessons-learned" boundary: "run" vs "review" vs "validate"

**Knowledge edge: "run" and "facilitate" produce zero predicates**

| Token | Predicate | Reason |
|-------|----------|--------|
| run | None | "run" not in any predicate lexicon |
| facilitate | None | "facilitate" not in any predicate lexicon |
| review | QA | "review" fires QA always (Rule 19) |
| validate | QA | "validate" fires QA |
| test | TEST | "test" fires TEST (Rule 21) |
| document | DOCUMENT | "document" fires DOCUMENT (Rule 8) |

"Run the lessons-learned session" → []
"Review the lessons learned" → [QA]

---

## B37-0C-017-003 — "lessons-learned" action items compound

**Input:** "Track the lessons-learned action items and follow up with owners."

Knowledge edge analysis:
- "lessons-learned" → gap-project-managing-manage-delivery-001
- "track" → gap-project-managing-track-001 (Rule 2: track ≠ TEST)
- "action items" → gap-project-managing-action-items-001
- "follow up" → gap-project-managing-follow-up-001
- "owners" → gap-project-managing-accountability-001

Result: [] — four gap IDs co-fire, zero predicates.

DomainId "project-management". SCL emitted.

---

## B37-0C-017-004 — "lessons-learned" with "document" and "stakeholder"

**Input:** "Document the lessons learned and share with all programme stakeholders."

Knowledge edge:
- DOCUMENT fires on "document"
- DOCUMENT fires on "stakeholders" (Rule 6)
- One DOCUMENT predicate produced
- DomainId "software-development"
- Gap warnings: gap-project-managing-manage-delivery-001, gap-project-managing-stakeholder-update-001

Routing: DocumentationFlow (2.1). Lessons-learned intent not addressed by DocumentationFlow.

---

## B37-0C-017-005 — Five test cases for lessons-learned

1. "Facilitate a lessons-learned session with the delivery team." → []
2. "Track lessons-learned action items." → []
3. "Review lessons learned from the last sprint." → [QA]
4. "Document lessons learned and circulate to stakeholders." → [DOCUMENT]
5. "Define the lessons-learned structure for future projects." → [DOCUMENT, ARCHITECTURE]

Cases 1-2: knowledge_edge, SCL emitted.
Cases 3-5: routed to flows, no SCL, gap warnings forwarded.
