# Layer 0-C — Knowledge Edge Pairs (035)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-035-001 — "consult on security" vs "audit security" knowledge edge

"Consult on the security posture" → gap (advisory → gap)
"Consult and harden the security posture" → SECURITY fires (harden = code action)
"Audit the security posture" → SECURITY fires (audit = direct security action)
"Seek advice on securing the service" → gap (seek advice = advisory)
"Secure the service" → SECURITY fires (secure = direct code action)
Boundary: advisory + security topic = gap; direct action on security = SECURITY.

---

## KE-035-002 — "get feedback on tests" vs "write tests" knowledge edge

"Get feedback on the test suite from the QA (Quality Assurance) lead" → gap
"Get feedback on the tests and then add the missing assertions" → TEST fires; get feedback = gap warning
"Write the test suite" → TEST fires
"Review the test coverage with the lead" → gap (review with = mentoring)
"Review and extend the test coverage" → TEST fires; review = gap warning
Boundary: advisory/mentoring vocabulary + test topic = gap; writing/creating tests = TEST.

---

## KE-035-003 — "sanity check the API" vs "publish the API" knowledge edge

"Sanity check the API (Application Programming Interface) design" → gap
"Publish the API contract" → DOCUMENT or API_CONTRACT depending on verb
"Sanity check and then publish the API contract" → DOCUMENT fires; sanity check = gap warning
"Sanity check and integrate the third-party API" → API_CONTRACT fires; sanity check = gap warning
Boundary: advisory vocabulary + API topic = gap; direct publish/integrate action = code predicate fires.

---

## KE-035-004 — "rubber duck [implementation design]" vs "implement" knowledge edge

"Rubber duck the service implementation approach" → gap (thinking-aloud = advisory)
"Rubber duck and implement the service" → IMPLEMENT fires; rubber duck = gap warning
"Think through [design]" → similar to rubber duck → gap
"Think through and build [component]" → IMPLEMENT fires; think through = gap warning
Boundary: reflective/advisory vocabulary alone = gap; requester's build/implement action fires IMPLEMENT.

---

## KE-035-005 — "sounding board for [architecture]" vs "architect [system]" knowledge edge

"Use the team as a sounding board for the microservices architecture" → gap
"Use the team as a sounding board and architect the microservices" → ARCHITECTURE fires; sounding board = gap warning
"Consult the architecture guild on the design" → gap
"Consult and then architect the design" → ARCHITECTURE fires; consult = gap warning
Boundary: sounding board/consultation alone = gap; requester's direct architect action fires ARCHITECTURE.
