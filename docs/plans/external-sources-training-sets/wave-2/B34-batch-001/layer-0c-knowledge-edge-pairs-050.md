# Layer 0-C — Knowledge Edge Pairs (050)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-050-001 — "consult and architect" vs "consult on how to architect" edge

"Consult with the principal engineer and architect the system" → ARCHITECTURE fires; consult = gap warning
"Consult with the principal engineer on how to architect the system" → gap (consult + how to + infinitive = double exception → gap)
"Consult on architecture and then design the component boundaries" → ARCHITECTURE fires; consult = gap warning
"Consult on how to design the component boundaries" → gap (consult on how to = advisory + learning target)
Rule: "consult + [direct code action]" = code predicate fires; "consult on how to + [code verb]" = gap.

---

## KE-050-002 — "seek advice and implement" vs "seek advice on how to implement" edge

"Seek advice from the team and implement the recommended approach" → IMPLEMENT fires; seek advice = gap warning
"Seek advice on how to implement the retry mechanism" → gap (seek advice + how to = double exception)
"Get feedback and implement the suggestions" → IMPLEMENT fires; get feedback = gap warning
"Get feedback on how to implement the suggestions" → gap (get feedback + how to = double exception)
Rule: "advisory vocabulary + [direct code verb]" = code fires; "advisory vocabulary + how to [code verb]" = gap.

---

## KE-050-003 — "second opinion on how to test" vs "second opinion and test" edge

"Get a second opinion on how to write the unit tests" → gap (second opinion + how to = double exception)
"Get a second opinion and write the unit tests" → TEST fires; second opinion = gap warning
"Sanity check how to approach the security audit" → gap (sanity check + how to = double exception)
"Sanity check and perform the security audit" → SECURITY fires; sanity check = gap warning
Rule: advisory + "how to [code verb]" = gap; advisory + direct code action = code predicate fires.

---

## KE-050-004 — "rubber duck and integrate" vs "rubber duck the integration design" edge

"Rubber duck the integration design and integrate the service" → API_CONTRACT fires; rubber duck = gap warning
"Rubber duck the integration design approach" → gap (rubber duck + integration design as topic = gap; no direct action)
"Rubber duck and walk through integrating [X]" → API_CONTRACT fires (walk through integrating = embedded verb rule); rubber duck = gap warning
Rule: rubber duck alone with integration topic = gap; "rubber duck + [direct integrate]" or participial form = API_CONTRACT fires.

---

## KE-050-005 — "sounding board for [test approach] and write tests" edge

"Sounding board for the test approach and write the integration tests" → TEST fires; sounding board = gap warning
"Sounding board for the test approach" → gap (sounding board + test topic = gap; no test-writing action)
"Sounding board for security review and harden the API" → SECURITY fires; sounding board = gap warning
"Sounding board for the API design and integrate it" → API_CONTRACT fires; sounding board = gap warning
Rule: sounding board + direct code action = code predicate fires; sounding board alone = gap.
