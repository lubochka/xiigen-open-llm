# Layer 0-B — Router Pairs (008)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: router

---

## R008-001 — TEST+IMPLEMENT → [5.1, 4.1] (generate test class)

compiled_predicate_ids: ["TEST", "IMPLEMENT"]
compiled_text: "Generate the test class for the ExchangeRateService and implement test cases for currency rounding and precision."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test class' + 'test cases' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'generate' + 'implement' → IMPLEMENT → 4.1" }
TransitionType: feature_addition

---

## R008-002 — TEST+IMPLEMENT → [5.1, 4.1] (stubs + API_CONTRACT)

compiled_predicate_ids: ["TEST", "IMPLEMENT", "API_CONTRACT"]
compiled_text: "Create stubs for the external WeatherService API and write tests for the DeliveryEstimationService weather-adjusted routing."
RoutingDecisions:
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'write tests' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'create' → IMPLEMENT → 4.1" }
  - { predicate: API_CONTRACT, sub_phase: 3.3, routing_note: "'WeatherService API' → API_CONTRACT → 3.3" }
TransitionType: feature_addition

---

## R008-003 — TEST+IMPLEMENT+DOCUMENT+ARCHITECTURE → [5.1, 4.1, 6.2, 3.1]

compiled_predicate_ids: ["TEST", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]
compiled_text: "Design the test suite architecture for the PaymentModule, write tests for all payment strategies, and document the test coverage map."
RoutingDecisions:
  - { predicate: ARCHITECTURE, sub_phase: 3.1, routing_note: "'design' + 'test suite architecture' → ARCHITECTURE → 3.1" }
  - { predicate: TEST, sub_phase: 5.1, routing_note: "'test suite' + 'tests' → TEST → 5.1" }
  - { predicate: IMPLEMENT, sub_phase: 4.1, routing_note: "'write' → IMPLEMENT → 4.1" }
  - { predicate: DOCUMENT, sub_phase: 6.2, routing_note: "'document' → DOCUMENT → 6.2" }
TransitionType: feature_addition
Teaching: Four predicates, four routing decisions spanning 4 different sub-phases: 3.1, 4.1, 5.1, 6.2.

---

## R008-004 — TEST → 5.1 (event assertion)

compiled_predicate_ids: ["TEST"]
compiled_text: "Assert the DomainEventDispatcher raises the OrderCreated event exactly once when a new order is placed."
RoutingDecisions: [{ predicate: TEST, sub_phase: 5.1, routing_note: "'assert' → TEST → 5.1" }]
TransitionType: feature_addition
