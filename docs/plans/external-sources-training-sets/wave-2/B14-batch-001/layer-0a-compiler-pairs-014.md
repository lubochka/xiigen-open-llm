# Layer 0-A — Compiler Pairs (014)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-014-001 — ARCHITECTURE: service mesh topology

A: "Design the service mesh topology for inter-service communication across the microservices cluster."
  "design" → ARCHITECTURE | "service mesh topology" → ARCHITECTURE ("topology", "service mesh") | "inter-service communication" → ARCHITECTURE ("service", "layer")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "microservices cluster" → platform vocabulary, gap

---

## B14-compiler-014-002 — ARCHITECTURE+IMPLEMENT: Istio sidecar

A: "Architect the Istio sidecar injection pattern and implement the mTLS (mutual TLS) policy for all service-to-service communication."
  "architect" → ARCHITECTURE | "sidecar injection pattern" → ARCHITECTURE ("pattern") | "implement" → IMPLEMENT | "mTLS policy" → SECURITY ("mTLS", "tls")
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]
  Gap: "Istio", "sidecar injection" → tool/platform vocabulary, gap

---

## B14-compiler-014-003 — ARCHITECTURE: traffic management layer

A: "Structure the traffic management layer — weighted routing, circuit breaking, and retry policies across services."
  "structure" → ARCHITECTURE | "traffic management layer" → ARCHITECTURE ("layer") | "circuit breaking" → ARCHITECTURE ("circuit", "pattern") | "policies" → ARCHITECTURE ("pattern")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "weighted routing", "retry policies" → operational configuration vocabulary, gap

---

## B14-compiler-014-004 — ARCHITECTURE+DOCUMENT: Linkerd mesh design

A: "Design the Linkerd service mesh architecture and document the observability integration points."
  "design" → ARCHITECTURE | "service mesh architecture" → ARCHITECTURE | "document" → DOCUMENT | "observability integration points" → ARCHITECTURE ("component")
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Gap: "Linkerd" → tool name, gap

---

## B14-compiler-014-005 — gap: "enable Istio"

A: "Enable Istio on the Kubernetes cluster for all namespaces."
  "enable" → gap (operational toggle, not in any predicate lexicon) | "Istio" → gap | "Kubernetes cluster" → gap | "namespaces" → gap
  CompiledPredicateIds: []
  Teaching: "enable" is operational gap. To fire ARCHITECTURE: "design the Istio mesh architecture for the cluster."
