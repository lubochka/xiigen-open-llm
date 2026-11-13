# Layer 0-D — Flow Factory Pairs (036)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-036-001 — factory for API_CONTRACT only + guidance

A: "Sounding board the integration approach and integrate the telemetry service with the monitoring platform."
Factory: APIContractFlow33 | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "sounding board approach" = gap warning; "integrate telemetry service" = API_CONTRACT → APIContractFlow33. TransitionType: documentation (API_CONTRACT only, no IMPLEMENT).

---

## FF-036-002 — factory for SECURITY only + guidance

A: "Second opinion on the zero-trust model, then harden the network edge."
Factory: SecurityAuditFlow | TransitionType: documentation | CapabilityGapWarning: gap-skills-guidance-mentor-001
Evidence: "second opinion" = gap warning; "harden network edge" = SECURITY → SecurityAuditFlow. TransitionType: documentation (SECURITY only, no IMPLEMENT).

---

## FF-036-003 — complete B34 TransitionType matrix (final reference)

| Primary predicate(s) present | TransitionType |
|-----------------------------|----------------|
| [] (gap only) | none |
| [DOCUMENT] only | documentation |
| [TEST] only | documentation |
| [ARCHITECTURE] only | documentation |
| [API_CONTRACT] only | documentation |
| [SECURITY] only | documentation |
| [IMPLEMENT] (any combination) | feature_addition |
| [IMPLEMENT] + refactor intent | refactoring |
| [QA] last in chain | no contribution |

Guidance vocabulary framing NEVER changes TransitionType. Gap warning presence NEVER changes TransitionType.

---

## FF-036-004 — factory suppression rule: GapHandler suppressed by any predicate

If predicate set = [IMPLEMENT]: GapHandler suppressed. Factory = ImplementationFlow41. Gap = warning only.
If predicate set = [TEST]: GapHandler suppressed. Factory = UnitTestingFlow51. Gap = warning only.
If predicate set = [SECURITY]: GapHandler suppressed. Factory = SecurityAuditFlow. Gap = warning only.
If predicate set = []: GapHandler invoked. No code factory. SCHEDULE_CAPABILITY_LEARNING emitted.
Invariant: GapHandler and code factory never coexist.

---

## FF-036-005 — factory ordering rule for B34 multi-predicate requests

When multiple predicates fire, factory ordering follows standard priority:
1. ArchitectureFlow31 (or ArchitectureFlow35 if applicable)
2. APIContractFlow33
3. ImplementationFlow41
4. SecurityAuditFlow (post-validates IMPLEMENT)
5. AuthorizationFlow (after IMPLEMENT)
6. UnitTestingFlow51 or E2ETestingFlow53
7. DocumentationFlow (parallel with TEST after IMPLEMENT)
8. QAGatingFlow73 (absolute last)
Guidance vocabulary framing never affects ordering.
