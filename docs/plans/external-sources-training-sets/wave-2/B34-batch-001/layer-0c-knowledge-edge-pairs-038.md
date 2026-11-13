# Layer 0-C — Knowledge Edge Pairs (038)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-038-001 — "consult on [implementation]" vs "implement [X]" edge

"Consult the platform team on the best implementation approach" → gap (consult = advisory)
"Implement the selected approach" → IMPLEMENT fires
"Consult and then implement" → IMPLEMENT fires; consult = gap warning
"Consult on how to implement [X]" → gap (advisory + how to = learning target)
Boundary: advisory about implementation ≠ implementing; "consult on how to" = gap (double exception).

---

## KE-038-002 — "seek advice on [testing]" vs "write tests" edge

"Seek advice on the test strategy" → gap
"Seek advice on testing and then write the tests" → TEST fires; seek advice = gap warning
"Seek advice on how to test [X]" → gap (advisory + how to infinitive)
"Ask for testing guidance" → gap (ask for = seeking advisory)
Boundary: seeking advice about testing = gap; directly writing tests = TEST.

---

## KE-038-003 — "get feedback on [document]" vs "write document" edge

"Get feedback on the architecture decision record (ADR)" → gap
"Get feedback on the ADR (Architecture Decision Record) and update it" → DOCUMENT fires; get feedback = gap warning
"Request documentation feedback" → gap (request/seek = advisory)
"Write the ADR" → DOCUMENT fires
Boundary: requesting feedback on a document ≠ writing/updating the document.

---

## KE-038-004 — "sanity check [architecture]" vs "design architecture" edge

"Sanity check the proposed architecture with the principal engineer" → gap
"Sanity check and then finalize the architecture design" → ARCHITECTURE fires; sanity check = gap warning
"Sanity check the architecture" = same as "review the architecture with someone" → gap
"Design the target architecture" → ARCHITECTURE fires
Boundary: sanity checking = advisory review = gap; designing/architecting = code action.

---

## KE-038-005 — "second opinion on [security]" vs "implement security" edge

"Get a second opinion on the security approach from the guild" → gap
"Get a second opinion and then implement the recommended security controls" → IMPLEMENT + SECURITY fire; second opinion = gap warning
"Ask for a security review" → gap (ask for review = advisory)
"Perform a security review" → SECURITY fires (perform = direct action)
Boundary: requesting a second opinion/review = gap; performing the security action = SECURITY.
