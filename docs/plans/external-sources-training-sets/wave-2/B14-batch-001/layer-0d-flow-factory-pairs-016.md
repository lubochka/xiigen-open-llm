# Layer 0-D — Flow Factory Pairs (016)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B14-factory-016-001 — ARCHITECTURE / 3.5 → InfrastructureArchitectureFlow35 (CDN topology)

A:
  sub_phase: 3.5
  predicate: ARCHITECTURE
  compiled_text: "Design the CDN edge topology — origin shield, PoP placement, and cache hierarchy."

B:
  TrustState: deterministic_generic
  FlowId: "InfrastructureArchitectureFlow35"
  SelectionReason: "ARCHITECTURE at 3.5 — CDN edge topology = infra/network design"
  NodeCount: 9

---

## B14-factory-016-002 — ARCHITECTURE+DOCUMENT / 3.5+6.2 → two flows (cache invalidation ADR)

A:
  sub_phase: [3.5, 6.2]
  predicate: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Architect the cache invalidation strategy for the edge layer and document the TTL policy decisions."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "DocumentationFlow62"
      predicate: DOCUMENT
      sub_phase: 6.2
  SelectionReason: "ARCHITECTURE (strategy, layer) + DOCUMENT → documentation TransitionType"

---

## B14-factory-016-003 — ARCHITECTURE+SECURITY / 3.5+6.3 → two flows (edge security)

A:
  sub_phase: [3.5, 6.3]
  predicate: ["ARCHITECTURE", "SECURITY"]
  compiled_text: "Structure the edge security boundary — WAF integration, DDoS mitigation layer, and TLS termination at edge."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "InfrastructureArchitectureFlow35"
      predicate: ARCHITECTURE
      sub_phase: 3.5
    - FlowId: "SecurityAuditFlow63"
      predicate: SECURITY
      sub_phase: 6.3
  SelectionReason: "ARCHITECTURE (boundary, layer) + SECURITY (TLS termination) → two concurrent flows"

---

## B14-factory-016-004 — zero predicates → knowledge_edge_synthetic (CDN cache purge)

A:
  sub_phase: null
  predicate: null
  compiled_text: "Purge the CDN cache for the product image assets."

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  SelectionReason: "Zero predicates → knowledge_edge_synthetic; 'purge', 'CDN cache', 'product image assets' are all gap"
  NodeCount: 0
