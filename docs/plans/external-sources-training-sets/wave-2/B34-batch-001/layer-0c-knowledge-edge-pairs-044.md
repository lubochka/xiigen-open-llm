# Layer 0-C — Knowledge Edge Pairs (044)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-044-001 — "explain the UI (User Interface)" vs "build the UI" edge

"Explain the UI (User Interface) component library to the frontend team" → gap (explain always gap; UI topic = learning content)
"Build the UI component library" → IMPLEMENT fires
"Explain the UI and build the component library" → IMPLEMENT fires; explain = gap warning
"Walk the team through the UI design system" → gap (walk through = guidance; UI topic = learning content)
Rule: any guidance vocabulary + UI topic = gap; direct build/implement action + UI = IMPLEMENT.

---

## KE-044-002 — "present the architecture diagram" vs "architect the system" edge

"Present the C4 (Context, Containers, Components, Code) architecture diagram to the stakeholders" → gap (presenting diagram = guidance)
"Architect the target system using the C4 model" → ARCHITECTURE fires
"Draw the architecture diagram" → DOCUMENT or IMPLEMENT (depending on artifact; diagram = documentation artifact → DOCUMENT)
"Present and then architect" → ARCHITECTURE fires; present = gap warning
Rule: presenting an architecture artifact ≠ designing/architecting the system.

---

## KE-044-003 — "discuss the security model" vs "implement security controls" edge

"Discuss the zero-trust security model with the platform team" → gap (discuss = guidance; security topic = learning content)
"Implement the zero-trust security controls" → SECURITY or IMPLEMENT fires (harden = SECURITY; build = IMPLEMENT)
"Discuss and implement the zero-trust model" → SECURITY/IMPLEMENT fires; discuss = gap warning
"Present the threat model to leadership" → gap (present = guidance; threat model = topic)
Rule: discussing/presenting security topics = gap; implementing/hardening security = code predicate.

---

## KE-044-004 — "consult on QA strategy" vs "QA the build" edge

"Consult with the QA (Quality Assurance) team on the testing strategy" → gap (consult = advisory; testing strategy = topic)
"QA the build before release" → QA fires (abbreviation as direct verb)
"Consult and QA the build" → QA fires; consult = gap warning
"Get feedback on the QA process" → gap (get feedback = advisory; QA process = topic, not direct action)
Rule: consultation/feedback on QA = gap; QA abbreviation as action verb = QA fires.

---

## KE-044-005 — "introduce [person] to architecture" vs "architect for [person]" edge

"Introduce the new engineers to the hexagonal architecture pattern" → gap (introduce [person] to [concept] = training vocabulary)
"Architect the hexagonal model for the new service" → ARCHITECTURE fires
"Introduce [person] to architecture and architect the service" → ARCHITECTURE fires; introduce = gap warning
"Introduce [person] to [system architecture diagram]" → gap (person + presentation artifact = guidance)
Rule: introduce [person] = always gap; architect [action] = ARCHITECTURE.
