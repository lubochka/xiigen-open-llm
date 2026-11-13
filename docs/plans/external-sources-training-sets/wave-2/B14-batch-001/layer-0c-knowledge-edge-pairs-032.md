# Layer 0-C — Knowledge Edge Pairs (032)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-032-001 — AlertManager: "silence" is always gap

Gap: "silence the firing alerts", "delete the suppression rules", "mute the notification channel"
No predicates fire from runtime alert management operational actions
Teaching: Alert management operations (silence, mute, delete alert rules at runtime) are gap. Designing routing topology (ARCHITECTURE) or writing configuration files (IMPLEMENT) fire predicates.

---

## B14-edge-032-002 — AlertManager: YAML configuration file is a code artifact

IMPLEMENT fires: "write the routing tree YAML" — YAML configuration file = code artifact → IMPLEMENT
IMPLEMENT fires: "implement the AlertManager configuration file" — configuration file = code artifact
Gap: "update the alert thresholds" (operational parameter change — not creating an artifact)
Teaching: Creating/writing a YAML/config file fires IMPLEMENT. Updating a runtime threshold value is operational gap.

---

## B14-edge-032-003 — AlertManager: "routing topology" fires ARCHITECTURE

ARCHITECTURE fires: "design the AlertManager routing topology with receiver hierarchy"
"routing topology" = infra vocabulary (topology fires ARCHITECTURE); "hierarchy" = structural noun
Gap: "check the routing configuration" (operational check — not design), "view routing tree" (operational view)
Teaching: Viewing/checking the routing tree is operational. Designing the routing topology fires ARCHITECTURE.

---

## B14-edge-032-004 — AlertManager: API key fires SECURITY

SECURITY fires: "API key authentication for AlertManager" — "API key" = authentication context → SECURITY
SECURITY fires: "TLS for webhook endpoints" — "TLS" fires SECURITY directly
Gap: "webhook URL" alone (operational endpoint reference), "receiver name" alone (configuration reference)
Teaching: API key and TLS are direct SECURITY triggers. Webhook URLs and receiver names alone are gap.
