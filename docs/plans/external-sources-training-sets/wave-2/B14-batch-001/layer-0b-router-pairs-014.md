# Layer 0-B — Router Pairs (014)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-014-001 — ARCHITECTURE → 3.5 (service mesh topology)

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Design the service mesh topology for inter-service communication across the microservices cluster."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      confidence: 0.92
      routing_note: "'design' + 'service mesh topology' → ARCHITECTURE → 3.5 (service mesh = infra)"
  TransitionType: feature_addition
  SubPhaseId: 3.5

---

## B14-router-014-002 — ARCHITECTURE+IMPLEMENT+SECURITY → 3.5 + 4.1 + 6.3 (Istio mTLS)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]
  compiled_text: "Architect the Istio sidecar injection pattern and implement the mTLS policy."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'architect' + 'sidecar injection pattern' → ARCHITECTURE → 3.5"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "'implement' + 'policy' → IMPLEMENT → 4.1"
    - predicate: SECURITY
      sub_phase: 6.3
      routing_note: "'mTLS', 'tls' → SECURITY → 6.3"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 4.1, 6.3]

---

## B14-router-014-003 — ARCHITECTURE+DOCUMENT → 3.5 + 6.2 (Linkerd mesh design)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Design the Linkerd service mesh architecture and document the observability integration points."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'service mesh architecture' → ARCHITECTURE → 3.5"
    - predicate: DOCUMENT
      sub_phase: 6.2
      routing_note: "'document' → DOCUMENT → 6.2"
  TransitionType: documentation
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.2]

---

## B14-router-014-004 — gap → knowledge_edge (enable Istio)

A:
  compiled_predicate_ids: []
  compiled_text: "Enable Istio on the Kubernetes cluster for all namespaces."

B:
  RoutingDecisions: []
  TransitionType: unknown
  SubPhaseId: null
  TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings:
    - "gap-devops-cloud-001: 'enable', 'Istio', 'Kubernetes cluster' are gap vocabulary"
