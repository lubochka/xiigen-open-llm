# Layer 0-C — Knowledge Edge Pairs (041)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-041-001 — "consult on how to" = double exception → gap

"Consult on how to architect the system" → gap (consult = advisory + how to = learning frame; double exception applies)
"Consult with expert on how to integrate" → gap (advisory + how to integrate = learning frame; integrate exception blocked)
"Consult on how to secure the service" → gap (advisory + how to = learning target)
Rule: "consult on/seek advice on/get feedback on + how to [code verb]" = double exception → gap always.

---

## KE-041-002 — "show how to integrate" vs "show [person] integrating" edge

"Show the team how to integrate the service" → gap (how to + infinitive = learning target)
"Show the team integrating the service in the demo" → gap (demonstration of others integrating = third-party action; show = guidance)
"Show the integration" (display) → neutral (show [output] = not guidance)
Rule: "show how to integrate" = gap (infinitive exception); participial "integrating" without requester as subject = third-party = gap.

---

## KE-041-003 — "train to integrate" vs "train on [integration topic]" edge

"Train the team to integrate third-party services" → gap (train to = "teach to X" pattern; integrate blocked)
"Train the team on integration patterns" → gap (train on = guidance; integration patterns = topic)
"Train [model] to classify" → outside B34 scope (ML training operation)
All human-targeted train = gap. "To integrate" and "on integration" = both blocked by training vocabulary.

---

## KE-041-004 — "explain the integration" vs "explain how to integrate" edge

"Explain the integration architecture to the team" → gap (explain always gap; integration topic = learning content)
"Explain how to integrate with the payment gateway" → gap (explain how to = learning frame; same as show how to)
"Explain and integrate the payment gateway" → API_CONTRACT fires; explain = gap warning
Rule: explain + any integration topic/framing = gap; "explain and [direct integrate action]" = API_CONTRACT + gap warning.

---

## KE-041-005 — "demonstrate the security model" vs "demonstrate how to secure" edge

"Demonstrate the security model to the team" → gap (demonstrate [noun] = teaching act)
"Demonstrate how to secure the API (Application Programming Interface)" → gap (demonstrate how to = learning frame)
"Demonstrate and harden the API" → SECURITY fires; demonstrate = gap warning
"Demonstrate security vulnerabilities in the code and then patch them" → SECURITY fires (patch = security action); demonstrate = gap warning
