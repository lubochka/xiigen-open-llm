# Layer 0-C — Knowledge Edge Pairs (010)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-010-001 — Gap category determination: B34 three IDs

gap-skills-guidance-mentor-001: mentor, coach, guide (verb), advise, facilitate, sponsor, pair program, buddy, shadow, observe, review (mentoring), give feedback, knowledge sharing
gap-skills-guidance-onboard-001: onboard, orientation, walkthrough (facilitation), bootcamp, ramp-up, new-hire program, buddy system, first-week
gap-skills-guidance-train-001: teach, train (human), instruct, explain, demonstrate, show, clarify, upskill, reskill, cross-train, learning path, curriculum, workshop, skill-building, bootcamp (learning), TDD (methodology), introduce (methodology)

Priority when multiple categories: mentor-001 > onboard-001 > train-001

---

## KE-010-002 — "write" disambiguation: DOCUMENT vs TEST

"Write a guide" → DOCUMENT (guide = documentation artifact)
"Write tests" → TEST (tests = test artifact)
"Write a tutorial" → DOCUMENT (tutorial = documentation artifact)
"Write unit tests" → TEST + routes to 5.1
"Write documentation" → DOCUMENT
"Write assertions" → TEST (assertions = test artifact)
"Write runbooks" → DOCUMENT (runbook = operational documentation)

Rule: "write" routes to DOCUMENT or TEST based on what follows. Documentation artifacts → DOCUMENT. Test artifacts → TEST.

---

## KE-010-003 — "create" disambiguation: DOCUMENT vs IMPLEMENT

"Create a README" → DOCUMENT
"Create a module" → IMPLEMENT
"Create a tutorial" → DOCUMENT
"Create a service" → IMPLEMENT
"Create a test suite" → TEST (test artifact)
"Create a specification" → DOCUMENT
"Create a system" → IMPLEMENT

Rule: Documentation artifact nouns → DOCUMENT. Software component nouns → IMPLEMENT. Test nouns → TEST.

---

## KE-010-004 — Guidance frame with SECURITY embedded

"Walk the team through securing the API" → SECURITY fires (securing = embedded SECURITY action)
"Explain secure coding practices" → gap (explaining ≠ implementing)
"Coach the team on security and harden the API" → ["SECURITY"] (harden fires; coach → gap suppressed)
