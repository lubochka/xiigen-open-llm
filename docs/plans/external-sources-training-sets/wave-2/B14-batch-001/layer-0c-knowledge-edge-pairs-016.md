# Layer 0-C — Knowledge Edge Pairs (016)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-016-001 — "edge" architecture fires ARCHITECTURE

A: "Design the edge architecture — CDN PoP (Point of Presence) placement, edge compute, and origin failover topology."
  "design" → ARCHITECTURE | "edge architecture" → ARCHITECTURE | "failover topology" → ARCHITECTURE ("topology")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "edge architecture" fires ARCHITECTURE via "architecture". "topology" reinforces it. "CDN", "PoP" → gap.

---

## B14-edge-016-002 — "cache" alone does not fire ARCHITECTURE

A: "Clear the edge cache for the static assets."
  "clear" → gap (operational action) | "edge cache" → gap (operational resource) | "static assets" → gap
  CompiledPredicateIds: []
  Gap signal: gap-devops-cloud-001
  Teaching: "cache" as a running operational resource is gap. "cache layer" or "caching architecture" fires ARCHITECTURE.

---

## B14-edge-016-003 — "TLS termination" fires SECURITY at CDN edge

A: "Configure TLS termination at the CDN edge layer — wildcard certificate, HSTS (HTTP Strict Transport Security) policy, and cipher suite selection."
  "configure" → IMPLEMENT (config context) | "TLS termination" → SECURITY ("tls") | "wildcard certificate" → SECURITY ("certificate") | "CDN edge layer" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY", "ARCHITECTURE"]
  Teaching: TLS, certificate, and "layer" fire their respective predicates independently.

---

## B14-edge-016-004 — "WAF" does not fire SECURITY alone

A: "Enable the WAF (Web Application Firewall) on the CloudFront distribution."
  "enable" → gap | "WAF" → gap (tool/service name) | "CloudFront" → gap
  CompiledPredicateIds: []
  Gap signal: gap-devops-cloud-001 — "WAF" is a tool name; "enable" is operational gap.
  Teaching: To fire SECURITY: "implement the WAF security policy and TLS configuration for the edge layer."
