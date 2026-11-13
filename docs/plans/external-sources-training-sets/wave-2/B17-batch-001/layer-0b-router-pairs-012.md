# Layer 0-B — Router Pairs (012)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: router | focus: complete routing decision table

---

## B17-0B-012-001 — Complete B17 predicate routing table

| Predicate | Sub-phase | Sub-phase name | FlowId |
|---|---|---|---|
| IMPLEMENT (+ integration vocabulary) | 4.3 | Integration and External Services | IntegrationConnectorFlow43 |
| IMPLEMENT (no integration vocabulary) | 4.1 | Backend Services and Logic | BackendServiceFlow41 |
| API_CONTRACT | 3.3 | API Contract Definition | APIContractFlow33 |
| SECURITY | 3.4 | Security and Compliance | SecurityAuditFlow34 |
| AUTHORIZATION | 3.4 | Security and Compliance | AuthorizationFlow34 |
| TEST | 5.1 | Unit and Integration Testing | TestingFlow51 |
| DOCUMENT | 2.1 | Documentation and Specification | DocumentationFlow21 |
| QA | 7.x | QA Gating | QAGatingFlow73 |

---

## B17-0B-012-002 — Complete B17 predicate combination routing table

| CompiledPredicateIds | Sub-phases activated | DecisionCount | TransitionType |
|---|---|---|---|
| [API_CONTRACT] | 3.3 | 1 | documentation |
| [IMPLEMENT, API_CONTRACT] | 4.3, 3.3 | 2 | feature_addition |
| [IMPLEMENT, API_CONTRACT] + refactor | 4.3, 3.3 | 2 | refactoring |
| [IMPLEMENT, API_CONTRACT, SECURITY] | 4.3, 3.3, 3.4 | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, AUTHORIZATION] | 4.3, 3.3, 3.4 | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION] | 4.3, 3.3, 3.4 | 4 | feature_addition |
| [IMPLEMENT, API_CONTRACT, TEST] | 4.3, 3.3, 5.1 | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT] | 4.3, 3.3, 2.1 | 3 | feature_addition |
| [IMPLEMENT, API_CONTRACT, DOCUMENT, QA] | 4.3, 3.3, 2.1, 7.x | 4 | feature_addition |
| [IMPLEMENT, API_CONTRACT, SECURITY, AUTHORIZATION, TEST] | 4.3, 3.3, 3.4, 5.1 | 5 | feature_addition |
| [QA] | 7.x | 1 | no contribution |
| [TEST] | 5.1 | 1 | no contribution |
| [DOCUMENT] | 2.1 | 1 | documentation |

---

## B17-0B-012-003 — Integration vocabulary that shifts IMPLEMENT from 4.1 to 4.3

Complete list of vocabulary items that trigger 4.3 routing for IMPLEMENT:
- connector (any compound: PayPalConnector, StripeConnector, etc.)
- integration (in context: email integration, payment integration)
- webhook (any compound: webhook listener, webhook handler)
- API (external API reference)
- external service
- third-party
- gateway (payment gateway, API gateway)
- SDK (third-party SDK)

**One token from this list is sufficient** to route IMPLEMENT to 4.3 instead of 4.1.

---

## B17-0B-012-004 — DecisionCount formula for B17

**Formula:** DecisionCount = |CompiledPredicateIds|

Where |x| denotes cardinality (count of elements).

**Examples:**
- ["IMPLEMENT", "API_CONTRACT"] → 2
- ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION"] → 4
- ["API_CONTRACT"] → 1
- ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST"] → 5

**Unique sub-phases ≤ DecisionCount always.** When SECURITY and AUTHORIZATION both fire, DecisionCount includes both but unique sub-phases counts 3.4 once.
