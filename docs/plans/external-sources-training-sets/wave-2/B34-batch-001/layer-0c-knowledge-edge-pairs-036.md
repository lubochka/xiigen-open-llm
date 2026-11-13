# Layer 0-C — Knowledge Edge Pairs (036)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-036-001 — "explain [security topic]" vs "explain and audit" knowledge edge

"Explain the OWASP (Open Web Application Security Project) Top 10 to the team" → gap (explain always gap)
"Explain the security model and then audit the codebase" → SECURITY fires; explain = gap warning
"Explain how to audit" → gap (explain how to = learning target; audit in infinitive = learning target not requester action)
"Audit the codebase" → SECURITY fires (direct requester action)
Boundary: explain = always gap regardless of topic; direct security action in same request → SECURITY fires.

---

## KE-036-002 — "discuss [QA topic]" vs "QA abbreviation" knowledge edge

"Discuss quality assurance practices" → gap (discuss = guidance; quality assurance = topic)
"QA the release" → QA fires (abbreviation as direct action verb)
"Discuss and QA the release" → QA fires; discuss = gap warning
"Discuss and ensure quality" → gap (ensure quality ≠ QA abbreviation)
Boundary: guidance vocabulary + quality topic = gap; QA abbreviation as direct verb = QA fires.

---

## KE-036-003 — "walk through [documentation]" vs "document" knowledge edge

"Walk through the documentation with the team" → gap (walk-through = explanation vocabulary)
"Document the service API (Application Programming Interface)" → DOCUMENT fires
"Walk through and then document the service API" → DOCUMENT fires; walk-through = gap warning
"Walk through how to document the service" → gap (how to + infinitive = learning target)
Boundary: walk-through explanation of docs = gap; write/create/update documentation = DOCUMENT.

---

## KE-036-004 — "present [architecture]" vs "architect [system]" knowledge edge

"Present the architecture decision record to the team" → gap (present = guidance)
"Architect the system topology" → ARCHITECTURE fires
"Present and then architect the topology" → ARCHITECTURE fires; present = gap warning
"Present the architecture diagram" → gap (presenting diagram ≠ designing architecture)
Boundary: present (any topic) = always gap; architect (structural design action) = ARCHITECTURE.

---

## KE-036-005 — "second opinion on [integration]" vs "integrate" knowledge edge

"Get a second opinion on the integration approach" → gap (second opinion = advisory)
"Integrate the third-party service" → API_CONTRACT fires
"Get a second opinion and then integrate the service" → API_CONTRACT fires; second opinion = gap warning
"Get a second opinion on integrating the service" → gap (advisory frame; "integrating" here = discussion topic not participial code verb)
Boundary: advisory + integration topic = gap; requester's direct integrate action fires API_CONTRACT.
