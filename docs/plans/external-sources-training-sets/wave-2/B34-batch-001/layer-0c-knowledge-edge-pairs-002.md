# Layer 0-C — Knowledge Edge Pairs (002)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-002-001 — Complete guidance vocabulary gap list

Gap vocabulary (always produces zero predicates alone):
- Mentoring: mentor, coach, advise, counsel, guide (action verb), facilitate, sponsor
- Teaching: teach, train (human), instruct, explain, demonstrate, show how, clarify, introduce
- Onboarding: onboard, onboarding, buddy system, orientation, shadow, shadow day
- Skills: upskill, reskill, skill-building, skill development, cross-train, knowledge transfer
- Events: workshop, bootcamp, lunch-and-learn, session, retrospective (facilitation), demo (presentation)
- Observation: observe, watch, attend, participate (in training)

None of the above trigger any predicate. All route to GapHandler.

---

## KE-002-002 — The "teach to [verb]" distinction

"Teach to implement" → gap (requester is teaching; IMPLEMENT is what others will learn)
"Implement" → IMPLEMENT (requester is implementing)
"Guide through writing tests" → TEST fires (writing tests = embedded TEST action)
"Teach to write tests" → gap (the writing is what others will learn; requester is teaching)

Rule: The relevant verb is the requester's action, not the action they are teaching others to perform.

---

## KE-002-003 — "guide through X" embedded predicate rule

When "guide through [action]" contains a predicate-firing action, the predicate fires:
- "Guide through writing the API docs" → DOCUMENT (writing docs = embedded DOCUMENT)
- "Guide through architecting the system" → ARCHITECTURE (architecting = embedded ARCHITECTURE)
- "Guide through implementing the feature" → IMPLEMENT (implementing = embedded IMPLEMENT)

Exception: "Guide through [understanding/learning/discussing]" → gap (no predicate action embedded).
