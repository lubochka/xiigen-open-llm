# Layer 0-A — Compiler Pairs (044)
# Batch: B37-batch-001 | Scenario: B37 — Project managing — progress, coordination, and status reporting
# Production date: 2026-05-06 | pair_kind: compiler-pairs

---

## B37-0A-044-001 — "escalation process" + predicate combinations

**Input A:** "Manage the escalation process." → Predicates: []
**Input B:** "Define the escalation process." → ["DOCUMENT"] — "define" fires
**Input C:** "Design the escalation architecture." → ["ARCHITECTURE"] — "architecture" fires
**Input D:** "Implement the escalation service." → ["IMPLEMENT"] — "service" fires
**Input E:** "Review the escalation process." → ["QA"] — "review" fires

All five confirm: "escalation process" = gap; predicate comes from co-occurring token.

---

## B37-0A-044-002 — "communication plan" — DOCUMENT fires when documenting

**Input A:** "Prepare the communication plan." → Predicates: []
**Input B:** "Write the communication plan." → ["DOCUMENT"] — "write" fires DOCUMENT
**Input C:** "Build the communication platform." → ["IMPLEMENT"] — "build" + "platform" fire

**Rule:** "write" fires DOCUMENT (same as "document", "spec", "describe").
"communication plan" alone → gap.

---

## B37-0A-044-003 — "change control board" compound vocabulary

**Input:** "Track change requests escalated to the change control board."

Token scan:
- "Track" → NOT TEST → gap
- "change requests" → gap
- "escalated" → gap
- "change control board" → gap

Predicates: []
DomainId: "project-management"
GapIds: gap-project-managing-manage-delivery-001, gap-project-managing-escalation-001, gap-project-managing-track-001
SCL: emitted

---

## B37-0A-044-004 — "scope creep" in long PM sentence — zero predicates

**Input:** "Monitor scope creep, track milestones, coordinate the team, and escalate blockers."

Token scan:
- "Monitor" → gap
- "scope creep" → gap
- "track" → NOT TEST → gap
- "milestones" → gap
- "coordinate" → gap
- "team" → gap
- "escalate" → gap
- "blockers" → gap

Predicates: []
DomainId: "project-management"
GapIds: manage-delivery-001, track-001, coordinate-001, blockers-001, escalation-001
SCL: emitted

---

## B37-0A-044-005 — "risk mitigation" vocabulary combinations

**Input A:** "Run risk mitigation activities." → Predicates: []
**Input B:** "Document the risk mitigation plan." → ["DOCUMENT"]
**Input C:** "Design the risk mitigation framework." → ["ARCHITECTURE"] — "design" fires
**Input D:** "Review risk mitigation outcomes." → ["QA"] — "review" fires
**Input E:** "Implement the risk mitigation service." → ["IMPLEMENT"]

"risk mitigation" = zero-predicate gap vocabulary. All predicates from co-occurring tokens.
