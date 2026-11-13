# Layer 0-D — Flow Factory Pairs (023)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-023-001 — Mixed guidance + multiple code verbs: factory chaining examples

| Request | Predicates | Factories | TransitionType |
|---------|-----------|-----------|----------------|
| "Coach through building, walk through integrating, guide through writing tests" | [IMPLEMENT, API_CONTRACT, TEST] | ImplementationFlow41 → APIContractFlow33 → UnitTestingFlow51 (or E2E) | feature_addition |
| "Advise, design the architecture, build the service" | [ARCHITECTURE, IMPLEMENT] | ArchitectureFlow31 → ImplementationFlow41 | feature_addition |
| "Mentor, implement, document" | [IMPLEMENT, DOCUMENT] | ImplementationFlow41 + DocumentationFlow | feature_addition |
| "Explain, write tests, QA" | [TEST, QA] | UnitTestingFlow51 → QAGatingFlow73 | documentation (QA last) |
| "Multiple guidance verbs only" | [] | GapHandler | none |

---

## FF-023-002 — Factory ordering rules for mixed guidance+code requests

Ordering follows standard B34 priority:
1. ArchitectureFlow31 first (if ARCHITECTURE)
2. APIContractFlow33 (if API_CONTRACT, before IMPLEMENT)
3. ImplementationFlow41 (if IMPLEMENT)
4. SecurityAuditFlow (if SECURITY, post-IMPLEMENT)
5. UnitTestingFlow51 / E2ETestingFlow53 (if TEST)
6. DocumentationFlow (parallel with TEST, post-IMPLEMENT)
7. QAGatingFlow73 / ReleaseGateFlow71 (QA absolute last)

Guidance vocabulary as gap warnings does not affect factory ordering.

---

## FF-023-003 — Gap warning consolidation for multi-guidance requests

Multiple guidance verbs in one request → single consolidated CapabilityGapWarning
"Coach, mentor, and advise on the approach, then implement the feature"
CapabilityGapWarning: "coach, mentor, advise = guidance vocabulary; gap-skills-guidance-mentor-001"
One warning per gap ID regardless of how many guidance verbs appear.
