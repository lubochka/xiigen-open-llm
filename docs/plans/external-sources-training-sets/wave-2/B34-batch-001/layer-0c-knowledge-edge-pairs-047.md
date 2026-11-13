# Layer 0-C — Knowledge Edge Pairs (047)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-047-001 — "walk through [X] with" vs "walk through integrating [X]" final boundary

"Walk through the integration architecture with the team" → gap (walk-through of topic with person = guidance)
"Walk through integrating the SSO (Single Sign-On) provider" → API_CONTRACT + gap warning (participial integrate = embedded verb rule)
"Walk through how to integrate the SSO provider" → gap (how to + infinitive = learning target)
"Walk the team through the integration architecture" → gap (walk [person] through [topic] = guidance)
Rule: participial "integrating" = embedded verb fires; "how to integrate" = infinitive = gap.

---

## KE-047-002 — "present [document]" vs "write [document]" edge

"Present the technical specification to the clients" → gap (presenting document = guidance act)
"Write the technical specification" → DOCUMENT fires
"Present and then update the specification based on feedback" → DOCUMENT fires; present = gap warning
"Present the runbook to the ops team" → gap (presenting runbook = guidance)
Rule: presenting any document = gap; writing/creating/updating documents = DOCUMENT.

---

## KE-047-003 — "discuss [document]" vs "write [document]" edge

"Discuss the architecture decision record with the team" → gap (discuss = guidance; ADR = topic)
"Write the architecture decision record" → DOCUMENT fires
"Discuss and update the architecture decision record" → DOCUMENT fires; discuss = gap warning
"Review the ADR (Architecture Decision Record) with the team" → gap (review with = mentoring)
Rule: discussing/reviewing documents = gap; writing/updating documents = DOCUMENT.

---

## KE-047-004 — "consult on [API design]" vs "publish [API contract]" edge

"Consult with the API (Application Programming Interface) governance team on the versioning strategy" → gap
"Publish the API contract v2" → API_CONTRACT or DOCUMENT (publish = direct action)
"Integrate the v2 API endpoints into the gateway" → API_CONTRACT fires
"Consult on the versioning strategy and integrate the v2 endpoints" → API_CONTRACT fires; consult = gap warning
Rule: consulting about API design = gap; publishing/integrating API = code predicate.

---

## KE-047-005 — complete exception table for "how to [code verb]" after guidance vocabulary

| Guidance vocabulary | "how to [code verb]" present? | Treatment |
|--------------------|-------------------------------|-----------|
| show | yes | gap (infinitive exception) |
| demonstrate | yes | gap (infinitive exception) |
| explain | yes | gap (explain always gap) |
| teach | yes | gap (teach-to exception) |
| train | yes | gap (train-to exception) |
| consult/seek advice on | yes | gap (double exception) |
| walk through | yes | gap (how to = learning target) |
| guide | yes | gap (guide how to = learning frame) |
| help | "to [code verb]" without learn | code predicate fires |
| help | "learn/understand how to" | gap |
