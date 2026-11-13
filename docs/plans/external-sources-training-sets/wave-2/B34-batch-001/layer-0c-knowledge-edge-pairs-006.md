# Layer 0-C — Knowledge Edge Pairs (006)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-006-001 — Embedded verb fires own predicate in mixed guidance+code request

Pattern: [guidance verb] + [code verb + artifact] → code verb predicate fires, guidance = gap warning
- "Mentor the team and build the skill tracker" → ["IMPLEMENT"]
- "Coach the engineers and document the API" → ["DOCUMENT"]
- "Onboard the engineers and write integration tests" → ["TEST"]
- "Train the team and integrate with the HR API" → ["API_CONTRACT"]

In all cases: guidance verb → gap (suppressed by firing predicate); code verb → predicate fires.

---

## KE-006-002 — "workshop" + embedded code verb

"Run a workshop where we build the authentication service" → IMPLEMENT (build fires)
"Run a workshop on authentication" → gap (no code verb; "run a workshop" = facilitation → gap)

Rule: Workshop/session/bootcamp framing does not suppress embedded code verbs. If the requester is building something within the workshop, IMPLEMENT fires.

---

## KE-006-003 — "walkthrough" + embedded DOCUMENT

"Write a walkthrough guide for the deployment process" → DOCUMENT (write + guide = DOCUMENT)
"Run a walkthrough of the deployment process" → gap ("run a walkthrough" = facilitation → gap)

Rule: "walkthrough" as noun in documentation artifact → DOCUMENT (via write/create). "Walkthrough" as facilitation activity → gap.

---

## KE-006-004 — Gap vocabulary complete list for B34

Gap verbs: mentor, coach, teach, train (human), guide (standalone), explain, demonstrate, show, clarify, instruct, advise, facilitate, sponsor, upskill, reskill, cross-train, onboard, shadow, observe, introduce (methodology), present (presentation), demo, help understand, hand off (knowledge), share (knowledge), review (mentoring context), assess (skills), evaluate (skills), certify (HR process)

Gap nouns (alone, no code verb): training program, curriculum, learning path, skill-building, workshop, bootcamp, tutorial (without write/create), walkthrough (without write/create), mentoring session, onboarding, buddy system, knowledge transfer, lunch-and-learn, retrospective (facilitation), team discussion
