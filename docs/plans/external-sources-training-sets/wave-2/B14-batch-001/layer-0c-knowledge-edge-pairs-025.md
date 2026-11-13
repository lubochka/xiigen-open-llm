# Layer 0-C — Knowledge Edge Pairs (025)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-025-001 — ContainerRegistry: boundary between gap and IMPLEMENT

IMPLEMENT fires: "write the Dockerfile", "implement the image scanning pipeline", "scaffold the registry configuration"
Gap (no IMPLEMENT): "push the image", "pull the image", "tag the image", "delete the image tag"
Determining factor: creating a code artifact (Dockerfile, pipeline config) → IMPLEMENT; runtime operational image operations → gap

---

## B14-edge-025-002 — ContainerRegistry: SECURITY boundary

SECURITY fires: "image signing" (certificate/credential context), "TLS on registry endpoint", "credential rotation for registry"
Gap (no SECURITY): "scan the image" (operational scan action alone), "check vulnerabilities" (operational), "enable vulnerability scanning" (operational enable verb)
Teaching: Enabling scanning is operational gap. Enforcing TLS or configuring signing policy fires SECURITY.

---

## B14-edge-025-003 — ContainerRegistry: ARCHITECTURE boundary

ARCHITECTURE fires: "design the registry topology", "architect the image layer architecture", "define the registry network boundary"
Gap: "set up the container registry", "configure ECR", "create the repository" (operational create-resource action)
Teaching: "create" in operational resource-creation context is gap. "design" + topology/boundary noun fires ARCHITECTURE.

---

## B14-edge-025-004 — ContainerRegistry: API_CONTRACT boundary

API_CONTRACT fires: "define the registry API contract", "specify the image push endpoint schema", "expose the registry service endpoint"
Gap: "call the registry API" (runtime call), "query the image tags" (runtime query), "list images" (operational)
Teaching: Defining/exposing the contract fires API_CONTRACT; calling/querying at runtime is gap.
