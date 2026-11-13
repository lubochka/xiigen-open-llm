# Layer 0-D — Flow Factory Pairs (024)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-024-001 — Complete flow ID registry for sub-phase 3.5

Primary flow for ARCHITECTURE / 3.5:
  FlowId: "InfrastructureArchitectureFlow35"
  FlowLabel: "Infrastructure and DevOps Architecture Flow — sub-phase 3.5"
  TrustState: deterministic_generic (all ARCHITECTURE cases at 3.5)
  NodeCount: 9 (DomainSplitter → DomainTranslator → SchemePlanner → DomainProcessor → Applier → ResultTranslator → ArbiterSelector → Arbiter → Formatter)

Concurrent flows when other predicates present:
  IMPLEMENT → "BackendServiceFlow41" (sub-phase 4.1)
  SECURITY → "SecurityAuditFlow63" (sub-phase 6.3)
  DOCUMENT → "DocumentationFlow62" (sub-phase 6.2)
  TEST → "UnitTestingFlow51" (sub-phase 5.1)
  QA → "QAGatingFlow73" (sub-phase 7.3)

---

## B14-factory-024-002 — Complete trust state rules for B14

deterministic_generic: any request with at least one valid predicate
knowledge_edge_synthetic: zero predicates (all vocabulary is gap)
Trust state is NOT degraded by gap vocabulary. Gap warnings are emitted but do not downgrade from deterministic_generic.
Trust state IS degraded only when zero predicates fire — this is the only path to knowledge_edge_synthetic.

---

## B14-factory-024-003 — Complete flow count rules

Number of flow instances = number of distinct predicates in CompiledPredicateIds
  1 predicate → 1 flow instance
  2 predicates → 2 concurrent flow instances
  3 predicates → 3 concurrent flow instances
  Up to 9 predicates (ARCHITECTURE, SECURITY, AUTHORIZATION, API_CONTRACT, IMPLEMENT, TEST, QA, UI, DOCUMENT) → up to 9 concurrent flow instances
  QA gating flow always executes last in sequential ordering

---

## B14-factory-024-004 — TransitionType selection in FlowFactory

FlowFactory.SelectTransitionType(predicates):
  if IMPLEMENT in predicates → feature_addition
  elif DOCUMENT in predicates and no IMPLEMENT → documentation
  elif ARCHITECTURE only → feature_addition (designing new artifact = feature_addition)
  elif ARCHITECTURE+DOCUMENT only → documentation
  elif TEST only → feature_addition (test addition)
  elif zero predicates → unknown
