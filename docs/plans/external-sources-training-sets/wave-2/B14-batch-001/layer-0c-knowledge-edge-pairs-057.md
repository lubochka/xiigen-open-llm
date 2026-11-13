# Layer 0-C — Knowledge Edge Pairs (057)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-057-001 — DDoSProtection: gap vocabulary — attack response operations

Confirmed gap vocabulary for DDoS (Distributed Denial of Service) active response operations (zero predicates):
  Action verbs: block (IP), ban, rate-limit (operational), absorb, scrub (traffic scrubbing), filter (traffic), redirect (traffic), blackhole, anycast (route), quarantine, throttle, drop (packet)
  Attack response nouns: IP blocklist, scrubbing center, anycast route, blackhole route, packet drop counter

---

## B14-edge-057-002 — DDoSProtection: SECURITY boundary — DDoS mitigation compound phrases

SECURITY fires for:
  "enforce volumetric attack mitigation" → SECURITY ("enforce" + security context)
  "harden ingress with SYN flood protection" → SECURITY ("harden")
  "configure TLS termination with DDoS-aware routing" → SECURITY ("TLS termination" = SECURITY compound phrase)
  "block the attacking IP range" → gap ("block" = operational response gap)

---

## B14-edge-057-003 — DDoSProtection: IMPLEMENT boundary — DDoS IaC artifacts

IMPLEMENT fires for: Shield Advanced Terraform module, rate-limiting configuration, shield configuration manifest, Cloudflare DDoS rule set configuration
  "implement Shield Advanced Terraform module" → IMPLEMENT
  "redirect DDoS traffic to scrubbing center" → gap ("redirect" is operational)

---

## B14-edge-057-004 — DDoSProtection: disambiguation — "protect" as architectural vs security verb

"protect" verb: context-dependent
  "design a DDoS protection topology" → ARCHITECTURE (design verb + topology)
  "protect the API with DDoS mitigation" → SECURITY (security protection context with "mitigation")
  "protect" alone without following security/design vocabulary → ambiguous → requires following noun
  Rule: "DDoS protection" as a compound noun fires SECURITY only when paired with SECURITY verb context; "design the protection topology" → ARCHITECTURE from "design" + "topology".
