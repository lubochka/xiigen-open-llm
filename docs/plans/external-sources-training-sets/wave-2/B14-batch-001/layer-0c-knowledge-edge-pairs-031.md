# Layer 0-C — Knowledge Edge Pairs (031)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-031-001 — GrafanaDashboard: "import" is gap

Gap: "import the Grafana dashboard" (operational import action), "set the default time range" (operational UI action)
IMPLEMENT fires: "implement the dashboard panels" (code artifact creation), "write the JSON model" (code artifact)
Teaching: Importing or configuring a dashboard in the UI is operational gap. Writing the dashboard code artifact fires IMPLEMENT.

---

## B14-edge-031-002 — GrafanaDashboard: "diagram" fires ARCHITECTURE

ARCHITECTURE fires: "diagram the panel hierarchy" — "diagram" is a structural verb that fires ARCHITECTURE
ARCHITECTURE fires: "diagram the metrics collection topology" — "diagram" + "topology" fires ARCHITECTURE
Teaching: "diagram" is one of the design verbs for ARCHITECTURE (blueprint, diagram, design, architect, structure). It fires even in documentation contexts.

---

## B14-edge-031-003 — GrafanaDashboard: DOCUMENT + ARCHITECTURE without IMPLEMENT

DOCUMENT + ARCHITECTURE with no IMPLEMENT → documentation TransitionType:
  "document the dashboard schema and diagram the panel hierarchy" → DOCUMENT + ARCHITECTURE, no IMPLEMENT
  TransitionType: documentation | Both 6.2 and 3.5 route as concurrent documentation flows
Teaching: "diagram" fires ARCHITECTURE; "document" fires DOCUMENT; no IMPLEMENT → documentation TransitionType.

---

## B14-edge-031-004 — GrafanaDashboard: API_CONTRACT boundary

API_CONTRACT fires: "define the Grafana API contract for dashboard provisioning" — "API contract" + "define" in endpoint context
Gap: "call the Grafana API to list dashboards" (runtime API call), "query the dashboard API" (operational)
Teaching: Defining/specifying the contract fires API_CONTRACT. Using/calling the API at runtime is gap.
