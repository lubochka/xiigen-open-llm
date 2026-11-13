# Layer 0-C — Knowledge Edge Pairs (011)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-011-001 — "document" noun vs "document" verb

"Document the process" → DOCUMENT (document = verb; process = object → DOCUMENT fires)
"The document covers onboarding" → gap (document = noun; no action verb fires)
"Onboarding document needs updating" → gap (observation; no code/document action verb)
"Update the onboarding document" → DOCUMENT (update + documentation artifact → DOCUMENT fires)

Rule: "document" as a verb fires DOCUMENT. "document" as a noun does not fire anything on its own.

---

## KE-011-002 — "coach" mixed with multiple code verbs

"Coach the team, build the tracker, document the criteria, and QA the workflow" → ["IMPLEMENT", "DOCUMENT", "QA"]
Breakdown: coach→gap; build→IMPLEMENT; document→DOCUMENT; QA→QA (abbreviation)
GapHandler not invoked (three predicates fire). TransitionType: feature_addition. QA last.

---

## KE-011-003 — "introduce" vs "architect": structural noun distinction

"Introduce microservices architecture to the team" → gap (introduce = teaching verb)
"Architect the microservices system" → ARCHITECTURE (architect + system = structural noun)
"Introduce the new microservices architecture plan" → gap (introduce = presentation/teaching)
"Design the microservices architecture" → ARCHITECTURE (design + architecture = structural noun)

Rule: Verb, not object, determines the predicate. "introduce" is always gap; "architect/design" fires ARCHITECTURE.

---

## KE-011-004 — "session" noun → gap

"Run a training session" → gap
"Schedule a mentoring session" → gap
"Book a learning session" → gap
"Deliver a coding session" → gap

"session" as noun never fires a predicate. It's an event vocabulary word → gap.
