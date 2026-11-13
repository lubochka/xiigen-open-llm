# Layer 0-B — Router Pairs (016)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-016-001 — ARCHITECTURE → 3.5 (CDN topology)

A:
  compiled_predicate_ids: ["ARCHITECTURE"]
  compiled_text: "Design the CDN edge topology — origin shield, PoP placement, and cache hierarchy."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'design' + 'edge topology' + 'cache hierarchy' → ARCHITECTURE → 3.5 (CDN = infra layer)"
  TransitionType: feature_addition
  SubPhaseId: 3.5

---

## B14-router-016-002 — ARCHITECTURE+DOCUMENT → 3.5 + 6.2 (cache invalidation strategy)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "DOCUMENT"]
  compiled_text: "Architect the cache invalidation strategy for the edge layer and document the TTL policy decisions."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'architect' + 'strategy' + 'edge layer' → ARCHITECTURE → 3.5"
    - predicate: DOCUMENT
      sub_phase: 6.2
      routing_note: "'document' → DOCUMENT → 6.2"
  TransitionType: documentation
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.2]

---

## B14-router-016-003 — ARCHITECTURE+SECURITY → 3.5 + 6.3 (edge security boundary)

A:
  compiled_predicate_ids: ["ARCHITECTURE", "SECURITY"]
  compiled_text: "Structure the edge security boundary — WAF integration, DDoS mitigation layer, and TLS termination at edge."

B:
  RoutingDecisions:
    - predicate: ARCHITECTURE
      sub_phase: 3.5
      routing_note: "'structure' + 'security boundary' + 'mitigation layer' → ARCHITECTURE → 3.5"
    - predicate: SECURITY
      sub_phase: 6.3
      routing_note: "'TLS termination' → SECURITY → 6.3"
  TransitionType: feature_addition
  PrimarySubPhaseId: 3.5
  AllSubPhaseIds: [3.5, 6.3]

---

## B14-router-016-004 — gap → knowledge_edge (purge CDN cache)

A:
  compiled_predicate_ids: []
  compiled_text: "Purge the CDN cache for the product image assets."

B:
  RoutingDecisions: []
  TransitionType: unknown
  TrustState: knowledge_edge_synthetic
  CapabilityGapWarnings:
    - "gap-devops-cloud-001: 'purge', 'CDN cache', 'product image assets' are operational gap vocabulary"
