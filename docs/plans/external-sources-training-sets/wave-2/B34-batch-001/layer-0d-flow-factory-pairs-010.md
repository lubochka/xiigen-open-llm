# Layer 0-D — Flow Factory Pairs (010)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-010-001 — IMPLEMENT: build quiz system

Predicates: ["IMPLEMENT"]
Request: "Create the quiz engine for the skills assessment platform."
Factory: ImplementationFlow41 | TransitionType: feature_addition

---

## FF-010-002 — DOCUMENT: spec the certification process

Predicates: ["DOCUMENT"]
Request: "Spec out the software engineer certification process."
Factory: DocumentationFlow | TransitionType: documentation

---

## FF-010-003 — IMPLEMENT+DOCUMENT parallel-eligible factory

Predicates: ["IMPLEMENT", "DOCUMENT"]
Orchestration: ImplementationFlow41 ‖ DocumentationFlow (parallel; no dependency)
TransitionType: feature_addition
Request: "Build the learning path tracker and document the progression milestones."

---

## FF-010-004 — Gap + DOCUMENT: one predicate suppresses GapHandler

Predicates: ["DOCUMENT"] (mentor→gap suppressed; document fires)
Request: "Mentor the team on best practices and document the coding standards."
Factory: DocumentationFlow
TransitionType: documentation
Gap warning: gap-skills-guidance-mentor-001 logged as CapabilityGapWarning (not full SCHEDULE_CAPABILITY_LEARNING)

---

## FF-010-005 — Six-flow factory: full cycle in B34 context

Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "SECURITY", "DOCUMENT", "QA"]
Request: "Architect the learning platform, build the course service, integrate with the payment API, secure user data, document the API, and QA the release."
Factory: ArchitectureFlow31 + APIContractFlow33 + ImplementationFlow41 + SecurityAuditFlow + DocumentationFlow + QAGatingFlow73
TransitionType: feature_addition | QA last | Gap for any "train/mentor" context: suppressed.
