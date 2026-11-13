# Layer 0-C — Knowledge Edge Pairs (042)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-042-001 — "brief" edge: briefing security topic vs performing security action

"Brief the security team on the threat model" → gap (brief [person] on = guidance vocabulary)
"Brief the engineering team on the penetration test results" → gap (briefing on results = guidance)
"Brief and then patch the identified vulnerabilities" → SECURITY fires; brief = gap warning
"Brief and conduct the penetration test" → SECURITY fires; brief = gap warning
Key: briefing ABOUT security ≠ performing security action.

---

## KE-042-002 — "discuss" edge: discussion vs decision + code action

"Discuss whether to use GraphQL (Graph Query Language) or REST (Representational State Transfer)" → gap (discuss = guidance; decision deliberation = gap)
"Discuss and choose GraphQL" → gap (choose = not a code action; decision only)
"Discuss and implement GraphQL" → IMPLEMENT fires; discuss = gap warning
"Discuss and document the architectural decision" → DOCUMENT fires; discuss = gap warning
Key: discussion + decision = still gap; discussion + code action = code fires.

---

## KE-042-003 — "run through [X] with" edge: requester explains vs someone else explains

"Run through the deployment process with the new team" (requester explains) → gap
"Have [senior engineer] run through the process with the team" (third-party explains) → gap (third-party's guidance action = not a code predicate; still classified as guidance gap for requester to arrange)
"Run through while integrating the service" → API_CONTRACT fires; run through = gap warning (participial integrate = embedded verb rule)
Key: run through = explanation vocabulary → gap in all forms.

---

## KE-042-004 — "present [feature]" vs "build [feature]" edge

"Present the new feature capabilities to the product team" → gap (present = guidance)
"Build the new feature" → IMPLEMENT fires
"Present and then build the feature" → IMPLEMENT fires; present = gap warning
"Demonstrate the feature and build it out" → IMPLEMENT fires; demonstrate = gap warning
"Show the feature in a demo" → gap (show/demo = guidance vocabulary)
Key: presenting/showing/demonstrating features = guidance → gap; building features = IMPLEMENT.

---

## KE-042-005 — "advisory vocabulary + imperative + code verb" composite edge

"Get a second opinion — then harden the auth layer." → SECURITY fires; second opinion = gap warning
"Consult with the team, then architect the service mesh." → ARCHITECTURE fires; consult = gap warning
"Sanity check the plan — then implement it." → IMPLEMENT fires; sanity check = gap warning
"Rubber duck the design, then write the tests." → TEST fires; rubber duck = gap warning
Rule: dash/semicolon separation, "then", comma = all equivalent. Adjacent code action after advisory = predicate fires.
