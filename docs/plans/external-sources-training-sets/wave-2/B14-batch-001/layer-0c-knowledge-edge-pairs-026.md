# Layer 0-C — Knowledge Edge Pairs (026)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-026-001 — HelmChart: "install" vs "implement" boundary

Gap: "install the nginx Helm chart" → "install" is operational gap regardless of artifact noun
IMPLEMENT: "implement the Helm chart for nginx" → "implement" + code artifact noun fires IMPLEMENT
Teaching: The verb determines predicate firing, not the artifact noun alone. "install" is always gap.

---

## B14-edge-026-002 — HelmChart: "upgrade" is always gap

Gap: "upgrade the Helm chart to version 3.2", "helm upgrade the ingress controller"
No predicate fires from "upgrade" regardless of what follows
IMPLEMENT fires from: "implement", "write", "scaffold", "create" (code context) + chart artifact noun
Teaching: "upgrade" is an operational action (like restart, rollback) → always gap in B14

---

## B14-edge-026-003 — HelmChart: DOCUMENT boundary for "README" vs "values file"

DOCUMENT: "write the Helm chart README" → "README" is documentary artifact → DOCUMENT fires
IMPLEMENT: "write the Helm chart values file" → "values file" is code artifact → IMPLEMENT fires
Teaching: "write" fires IMPLEMENT for code artifacts and DOCUMENT for documentary artifacts — the noun following "write" is the determining factor.

---

## B14-edge-026-004 — HelmChart: ARCHITECTURE only with design verb — not operational configuration

ARCHITECTURE fires: "design the Helm chart dependency topology" (design verb + topology noun)
Gap: "configure the Helm values" (operational configuration action — no code artifact specification)
Gap: "set the replica count in the values file" (operational parameter setting)
Teaching: ARCHITECTURE requires design/architect/structure — not configure/set/update.
