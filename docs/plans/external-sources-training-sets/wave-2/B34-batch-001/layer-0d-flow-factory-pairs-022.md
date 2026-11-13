# Layer 0-D — Flow Factory Pairs (022)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## FF-022-001 — "Explain" vocabulary: factory map

| Request | Predicates | Factory | TransitionType |
|---------|-----------|---------|----------------|
| "Explain the architecture" | [] | GapHandler | none |
| "Explain the security model" | [] | GapHandler | none |
| "Explain the test strategy" | [] | GapHandler | none |
| "Explain the integration" | [] | GapHandler | none |
| "Explain and document" | [DOCUMENT] | DocumentationFlow | documentation |
| "Explain and implement" | [IMPLEMENT] | ImplementationFlow41 | feature_addition |
| "Explain and write tests" | [TEST] | UnitTestingFlow51 | documentation |
| "Explain and integrate" | [API_CONTRACT] | APIContractFlow33 | documentation |

---

## FF-022-002 — "Explain" never selects a factory on its own

"Explain" alone → GapHandler always.
"Explain" + code verb → code verb's factory selected; "explain" becomes gap warning.
"Explain" never routes to: DocumentationFlow, SecurityAuditFlow, ArchitectureFlow31, APIContractFlow33 on its own.
"explain" ≠ DOCUMENT: DocumentationFlow requires explicit write/create/produce verbs.

---

## FF-022-003 — "Explain" gap warning ID assignment

"Explain" → always gap-skills-guidance-train-001 (explain = teaching vocabulary)
When "explain" accompanies a code predicate → gap warning: gap-skills-guidance-train-001
When "explain" is the only action verb → GapHandler: gap-skills-guidance-train-001 + SCHEDULE_CAPABILITY_LEARNING
