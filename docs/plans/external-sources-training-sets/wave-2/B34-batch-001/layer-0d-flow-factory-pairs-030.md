# Layer 0-D — Flow Factory Pairs (030)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-030-001 — "sounding board" + ARCHITECTURE factory

A: "Use the engineering guild as a sounding board for the service decomposition and architect the final domain boundaries."
Factory: ArchitectureFlow31 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "sounding board for decomposition" = gap warning; "architect final domain boundaries" = ARCHITECTURE → ArchitectureFlow31.

---

## FF-030-002 — "discuss" + IMPLEMENT + TEST multi-factory

A: "Discuss the feature scope, implement the search indexing service, and write E2E (End-to-End) tests."
Factory: ImplementationFlow41 ‖ E2ETestingFlow53 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: discuss = gap warning; implement → IMPLEMENT (ImplementationFlow41); write E2E tests → TEST (E2ETestingFlow53).

---

## FF-030-003 — "present" + ARCHITECTURE + DOCUMENT multi-factory

A: "Present the new service topology, architect the final component boundaries, and document the design decisions."
Factory: ArchitectureFlow31 ‖ DocumentationFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: present = gap warning; architect → ARCHITECTURE (ArchitectureFlow31); document → DOCUMENT (DocumentationFlow).

---

## FF-030-004 — multiple advisory verbs + single SECURITY factory

A: "Consult with the security guild, get a second opinion on the approach, and then harden the session management layer."
Factory: SecurityAuditFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: consult + second opinion = gap warnings (both advisory); "harden session management" = SECURITY → SecurityAuditFlow.

---

## FF-030-005 — "explain and build" factory = ImplementationFlow41

A: "Explain the reactive programming patterns to the team and build the reactive notification pipeline."
Factory: ImplementationFlow41 | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-train-001
Evidence: explain = gap warning; "build the reactive notification pipeline" = IMPLEMENT → ImplementationFlow41.
