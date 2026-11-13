# Layer 0-D — Flow Factory Pairs (008)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-008-001 — IMPLEMENT+SECURITY+TEST+DOCUMENT+QA five-flow factory

Predicates: ["IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
Request: "Build the mentorship platform, secure user data, write E2E tests, document the privacy policy, and QA before launch."
Factory: ImplementationFlow41 + SecurityAuditFlow + E2ETestingFlow53 + DocumentationFlow + QAGatingFlow73
TransitionType: feature_addition | QA last: absolute
Gap for "mentorship" context: suppressed by five predicates.

---

## FF-008-002 — DOCUMENT+ARCHITECTURE factory

Predicates: ["ARCHITECTURE", "DOCUMENT"]
Request: "Design the training system architecture and document the design decisions."
Factory: ArchitectureFlow31 + DocumentationFlow
TransitionType: documentation (IMPLEMENT absent)
Note: ARCHITECTURE alone → documentation; DOCUMENT adds → still documentation. Both flows execute; TransitionType = documentation.

---

## FF-008-003 — API_CONTRACT+IMPLEMENT+QA factory

Predicates: ["API_CONTRACT", "IMPLEMENT", "QA"]
Request: "Integrate the LMS with the Zoom API, build the session recorder, and QA the integration."
Factory: APIContractFlow33 + ImplementationFlow41 + QAGatingFlow73
TransitionType: feature_addition | QA last

---

## FF-008-004 — Pure gap factory: three-gap vocabulary

Request: "Train, coach, and mentor the new engineering cohort."
Predicates: [] | Factory: GapHandler
Gap ID: gap-skills-guidance-mentor-001 (highest priority of three categories present)
Event: SCHEDULE_CAPABILITY_LEARNING

---

## FF-008-005 — IMPLEMENT with refactor: TransitionType = refactoring

Predicates: ["IMPLEMENT"]
Request: "Teach the team refactoring patterns by refactoring the legacy training module."
TransitionType: refactoring ("refactor" verb present → overrides feature_addition)
Note: "teach" → gap; "refactoring the legacy module" → IMPLEMENT; TransitionType: refactoring.
