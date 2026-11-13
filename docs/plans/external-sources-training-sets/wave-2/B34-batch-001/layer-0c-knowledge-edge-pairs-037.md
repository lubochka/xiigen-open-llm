# Layer 0-C — Knowledge Edge Pairs (037)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-037-001 — "introduce [concept]" vs "introduce [capability]" edge

"Introduce the team to domain-driven design (DDD)" → gap (concept introduction → train-001)
"Introduce DDD patterns into the codebase" → IMPLEMENT (capability addition → IMPLEMENT)
"Introduce the event sourcing concept to the engineers and introduce the event store into the architecture" → gap warning + IMPLEMENT
Edge: sentence can contain both forms; each is evaluated independently.

---

## KE-037-002 — "brief [person] on [security]" vs "brief [document]" edge

"Brief the security team on the penetration test findings" → gap (brief verb + person target)
"Write the penetration test brief for the CISO (Chief Information Security Officer)" → DOCUMENT (brief noun + write action)
"Brief the team and then secure the identified vulnerabilities" → SECURITY fires; brief = gap warning
Boundary: brief as verb + person target = gap; brief as noun in write action = DOCUMENT.

---

## KE-037-003 — "demonstrate" scope: live demo vs teach how

"Demonstrate the deployment process to the new engineers" → gap (live showing = teaching act)
"Demonstrate [the feature] in the product review" → gap (demo = guidance act)
"Demonstrate [the feature] and implement the feedback" → IMPLEMENT fires; demonstrate = gap warning
"Give a demo of [X]" → gap (demo/demonstrate = guidance vocabulary)
All demonstrate contexts = gap when standalone. Only code action alongside fires predicate.

---

## KE-037-004 — "walk through integrating" vs "walk through how to integrate" edge

"Walk through integrating the CRM (Customer Relationship Management) with the API" → API_CONTRACT fires + gap warning (participial = embedded verb rule)
"Walk through how to integrate the CRM with the API" → gap (how to + infinitive = learning target; walk through = guidance)
Edge: "integrating" (present participle, describing action in progress) = embedded verb fires; "how to integrate" (infinitive) = learning target = gap.

---

## KE-037-005 — "facilitate integration" vs "facilitate [session about integration]" edge

"Facilitate the integration of the SSO (Single Sign-On) provider" → API_CONTRACT fires (integrate exception: facilitate + direct integration subject)
"Facilitate a session where the team discusses integration patterns" → gap (facilitation of discussion, not integration action)
"Facilitate a workshop on API (Application Programming Interface) integration" → gap (workshop on = training event, not direct integration)
Boundary: "facilitate integration of [X]" = API_CONTRACT; "facilitate [event] about [integration]" = gap.
