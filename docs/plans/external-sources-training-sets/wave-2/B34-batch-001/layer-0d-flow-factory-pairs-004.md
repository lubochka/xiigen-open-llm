# Layer 0-D — Flow Factory Pairs (004)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-004-001 — IMPLEMENT+DOCUMENT+QA factory

Predicates: ["IMPLEMENT", "DOCUMENT", "QA"]
Request: "Build the onboarding checklist app, document the process, and QA the sign-off workflow."
Factory: ImplementationFlow41 + DocumentationFlow + QAGatingFlow73
TransitionType: feature_addition | QA sub-phase: 7.3 | QA last: absolute

---

## FF-004-002 — IMPLEMENT+TEST+DOCUMENT factory

Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"]
Request: "Build the skill badge system, write tests, and document the badge criteria."
Factory: ImplementationFlow41 + UnitTestingFlow51 + DocumentationFlow
TransitionType: feature_addition

---

## FF-004-003 — ARCHITECTURE+IMPLEMENT factory

Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Request: "Design the learning platform architecture and build the course enrollment service."
Factory: ArchitectureFlow31 + ImplementationFlow41
TransitionType: feature_addition (IMPLEMENT present overrides documentation)

---

## FF-004-004 — Five-flow factory with guidance gap suppressed

Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT"]
Request: "Mentor the team, build the LMS, integrate with SSO, secure auth, write tests, and document the API."
Factory: ImplementationFlow41 + APIContractFlow33 + SecurityAuditFlow + TestingFlow + DocumentationFlow
TransitionType: feature_addition | Mentor gap: suppressed (five predicates fire)

---

## FF-004-005 — v6 corruption: TRAINING flow invented

v6 corrupt: "Train the backend team" → Predicates: ["TRAINING"] → TrainingFlow selected
Correct: Predicates: [] → GapHandler → gap-skills-guidance-train-001
Error type: v6-01 — TRAINING is not a predicate; TrainingFlow does not exist.
