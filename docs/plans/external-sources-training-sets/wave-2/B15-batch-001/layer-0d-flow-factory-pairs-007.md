# Layer 0-D — Flow Factory Pairs (007)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: flow_factory

## FF-007-001 — ARCHITECTURE 3.5 (infra) + IMPLEMENT factory
Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Sub-phase: 3.5 (infrastructure design for order processing cluster)
Factory: ArchitectureFlow35 + ImplementationFlow41

## FF-007-002 — DOCUMENT+QA factory (no IMPLEMENT)
Predicates: ["DOCUMENT", "QA"]
Factory: DocumentationFlow + QAGatingFlow73 | TransitionType: documentation
Note: Rare in B15 but valid — document the API spec and QA the docs.

## FF-007-003 — TEST+DOCUMENT factory (no IMPLEMENT)
Predicates: ["TEST", "DOCUMENT"]
Factory: TestingFlow + DocumentationFlow | TransitionType: documentation
Note: Write tests and document the test coverage plan — IMPLEMENT absent.

## FF-007-004 — API_CONTRACT+DOCUMENT factory
Predicates: ["API_CONTRACT", "DOCUMENT"]
Factory: APIContractFlow33 + DocumentationFlow | TransitionType: documentation

## FF-007-005 — Refactoring TransitionType with any predicate set
If "refactor" verb present → TransitionType = refactoring regardless of other predicates.
Factory selection unchanged; only TransitionType changes.
Example: "Refactor and extend the order service" → ["IMPLEMENT"] | refactoring TransitionType.
