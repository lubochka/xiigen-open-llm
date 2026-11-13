# Layer 3 — Teaching Unit Pairs (018)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B38-TU-018-001 — Teaching: "handoff" is not "sign-off"

LESSON: "Handoff" and "sign-off" look similar but have completely different predicate meanings.

"sign-off" → QA (always — it is explicitly in the QA lexicon)
"handoff" → no predicate (it is an operational transfer noun)

"Collect sign-off from the ops team" → ["QA"]
"Prepare the production handoff for the ops team" → [] → knowledge_edge (if no other triggers)
"Document the production handoff" → ["DOCUMENT"] (document fires; handoff has no predicate)

MEMORABLE RULE: "sign-off" = approval action = QA. "handoff" = delivery transfer = no predicate.

---

## B38-TU-018-002 — Teaching: compliance fires QA in handoff context

LESSON: "Compliance" is in the QA lexicon and fires QA even in handoff sentences.

"Confirm compliance with the handoff checklist" → ["QA"]
- "compliance" → QA
- "handoff" → no predicate
- "checklist" → GAP (warning only)

The gap warning for "checklist" does not suppress the QA predicate from "compliance".
This demonstrates v6-01 anti-corruption in a handoff context:
QA fires from "compliance"; gap vocabulary co-present but doesn't suppress QA.

---

## B38-TU-018-003 — Teaching: handoff runbook is pure gap

LESSON: The operational vocabulary for production handoff execution is entirely gap.

"Execute the handoff runbook" → knowledge_edge
- "execute" → rule 8 (not IMPLEMENT)
- "handoff" → no predicate
- "runbook" → B38 gap vocabulary

Adding service vocabulary immediately changes the result:
"Execute the payment service handoff" → ["IMPLEMENT"] (service fires) + gap warning on execute

The service vocabulary rescues the sentence from knowledge_edge territory.
