# Layer 0-C — Knowledge Edge Pairs (045)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-045-001 — "explain and build" compound: all forms

"Explain [X] to [person] and build [Y]" → IMPLEMENT fires; explain = gap warning
"Explain [X] to [person] and implement [Y]" → IMPLEMENT fires; explain = gap warning
"Explain [X] to [person] and create [Y] (software component)" → IMPLEMENT fires; explain = gap warning
"Explain [X] to [person] and develop [Y]" → IMPLEMENT fires; explain = gap warning
Rule: explain + build/implement/create/develop = IMPLEMENT fires + explain becomes gap warning. No exceptions.

---

## KE-045-002 — "demonstrate the onboarding process" vs "onboard [person]" edge

"Demonstrate the onboarding process to the HR (Human Resources) team" → gap (demonstrate = guidance; process demo = training)
"Onboard the new engineers" → gap (onboard = guidance vocabulary; direct onboarding = gap)
"Demonstrate and build the onboarding tool" → IMPLEMENT fires; demonstrate = gap warning
"Create the onboarding portal" → IMPLEMENT fires (portal = software component noun)
Rule: demonstrate onboarding + build software = IMPLEMENT fires; demonstrate/onboard alone = gap.

---

## KE-045-003 — "brief on the test results" vs "write tests" edge

"Brief the product team on the test results from the last sprint" → gap (brief on results = guidance)
"Write the regression tests for the new feature" → TEST fires
"Brief on the test results and write the missing tests" → TEST fires; brief = gap warning
"Review the test results with the team" → gap (review with = mentoring)
Rule: briefing/reviewing test outcomes = gap; creating/writing tests = TEST.

---

## KE-045-004 — "familiarize with the API" vs "integrate the API" edge

"Familiarize the team with the partner API (Application Programming Interface) documentation" → gap (familiarize = onboard vocabulary; API docs = learning content)
"Integrate the partner API into the platform" → API_CONTRACT fires
"Familiarize the team and then integrate the partner API" → API_CONTRACT fires; familiarize = gap warning
"Familiarize with how to integrate" → gap (familiarize + how to + infinitive = learning target)
Rule: familiarization with API ≠ integrating the API.

---

## KE-045-005 — "run through the security checklist" vs "complete the security checklist" edge

"Run through the security checklist with the team" → gap (run through with person = explanation vocabulary)
"Complete the security checklist" → SECURITY fires (complete a security task = security action)
"Run through and then patch the identified items" → SECURITY fires; run through = gap warning
"Audit using the security checklist" → SECURITY fires (audit = direct security action)
Rule: explaining/reviewing a security checklist = gap; completing/acting on security tasks = SECURITY.
