# Layer 0-C — Knowledge Edge Pairs (029)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-029-001 — IstioMesh: "enable sidecar injection" is gap

Gap: "enable Istio sidecar injection in the production namespace"
"enable" = operational verb; "sidecar injection" = operational feature enablement; "namespace" = platform term
No ARCHITECTURE fires: enabling a feature is operational, not designing a topology
Teaching: "Enable X" is always operational gap. "Design the X topology" fires ARCHITECTURE.

---

## B14-edge-029-002 — IstioMesh: "peer authentication" fires SECURITY

SECURITY fires: "configure peer authentication" — "peer authentication" = mTLS context → SECURITY
"enforce STRICT mTLS" → SECURITY ("mTLS", "STRICT" in security enforcement context)
Teaching: Even with operational verb "configure", mTLS/peer authentication vocabulary fires SECURITY.

---

## B14-edge-029-003 — IstioMesh: VirtualService manifests are IMPLEMENT

IMPLEMENT fires: "implement VirtualService routing rules" (code artifact — Kubernetes manifest)
IMPLEMENT fires: "write the DestinationRule configuration file" (code artifact)
Gap: "apply the VirtualService manifest" (operational kubectl apply → gap)
Teaching: Writing/implementing the manifest = IMPLEMENT; applying/deploying it = gap.

---

## B14-edge-029-004 — IstioMesh: "zero-trust" fires SECURITY independently

SECURITY fires: "enforce zero-trust" alone in any infra sentence
Gap vocabulary around "zero-trust" does not suppress SECURITY: "Enable zero-trust in the service mesh" → SECURITY fires from "zero-trust" even though "enable" is operational
Teaching: SECURITY vocabulary terms fire independently regardless of surrounding gap verbs. Only zero-predicate requests produce knowledge_edge_synthetic.
