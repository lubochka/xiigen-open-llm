# Layer 0-B — Router Pairs (026)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-026-001 — HelmChart routing: IMPLEMENT → 4.1

IMPLEMENT always routes to 4.1 (BackendServiceFlow41) regardless of tool context:
  "implement the Helm chart" → 4.1
  "write the chart templates" → 4.1
  "scaffold the chart values" → 4.1
  FlowId: BackendServiceFlow41 | TrustState: deterministic_generic

---

## B14-router-026-002 — HelmChart routing: ARCHITECTURE → 3.5

ARCHITECTURE routes to 3.5 when Helm context uses infra vocabulary:
  "Helm chart architecture for multi-environment deployment" → 3.5 (deployment topology = infra)
  "Helm chart structure for the API service" → 3.1 (app-level service vocabulary → 3.1)
  Disambiguation: "multi-environment deployment topology" = infra → 3.5

---

## B14-router-026-003 — HelmChart routing: DOCUMENT → 6.2

DOCUMENT → 6.2 (DocumentationFlow62) always:
  "document the Helm chart parameters" → 6.2
  "write the Helm chart README" → 6.2 (DOCUMENT, not IMPLEMENT — README is documentary artifact)
  DOCUMENT + IMPLEMENT combination: both 6.2 and 4.1 fire; feature_addition TransitionType

---

## B14-router-026-004 — HelmChart routing: no TEST routing for operational validation

"Test the Helm chart installation" → IMPLEMENT (4.1) if creating test script; gap if operational
TEST at 5.1 routes only for test code production: unit tests, integration test suites
"Validate the chart values schema" → TEST (5.1) if test code; IMPLEMENT (4.1) if validation script
Disambiguation: "validate" in test-code context → 5.1; in operational context → 4.1 or gap
