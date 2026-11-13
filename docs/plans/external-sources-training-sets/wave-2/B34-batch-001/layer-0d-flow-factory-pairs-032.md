# Layer 0-D — Flow Factory Pairs (032)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-032-001 — "loop in" + SECURITY factory (new Round 3 vocabulary)

A: "Loop in the security team on the encryption approach and harden the data-at-rest storage."
Factory: SecurityAuditFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "loop in security team" = gap warning (loop in = advisory vocabulary); "harden data-at-rest storage" = SECURITY → SecurityAuditFlow.

---

## FF-032-002 — "knowledge transfer" + DOCUMENT factory

A: "Do a knowledge transfer on the service architecture and document the architecture decision records."
Factory: DocumentationFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "knowledge transfer on architecture" = gap warning; "document ADRs (Architecture Decision Records)" = DOCUMENT → DocumentationFlow.

---

## FF-032-003 — "have [person] review" + TEST factory

A: "Have the tech lead review the test approach and write the unit tests for the billing calculator."
Factory: UnitTestingFlow51 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "have tech lead review" = gap warning; "write unit tests for billing calculator" = TEST → UnitTestingFlow51.

---

## FF-032-004 — "involve [team]" + ARCHITECTURE factory

A: "Involve the platform architects in reviewing the topology and architect the final service mesh layout."
Factory: ArchitectureFlow31 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "involve architects in reviewing" = gap warning; "architect final service mesh layout" = ARCHITECTURE → ArchitectureFlow31.

---

## FF-032-005 — purpose-clause guidance + IMPLEMENT + TEST + DOCUMENT three-factory

A: "As part of knowledge transfer, implement the reference service template, write integration tests, and document the pattern library."
Factory: ImplementationFlow41 ‖ UnitTestingFlow51 ‖ DocumentationFlow | TransitionType: feature_addition | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "knowledge transfer" = purpose clause → gap warning; implement → IMPLEMENT; write tests → TEST; document → DOCUMENT.
