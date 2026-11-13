# Layer 0-A — Compiler Pairs (057)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-057-001 — DDoSProtection: ARCHITECTURE fires on DDoS design vocabulary

"architect the DDoS (Distributed Denial of Service) protection topology for the platform" → ARCHITECTURE ("architect", "topology")
"design the multi-layer DDoS mitigation architecture" → ARCHITECTURE ("design", "architecture", "layer")
"structure the CDN (Content Delivery Network) and WAF shield model" → ARCHITECTURE ("structure", "model")
"check DDoS protection status" → gap ("check" is operational gap)

---

## B14-compiler-057-002 — DDoSProtection: IMPLEMENT fires on DDoS IaC artifacts

"implement the DDoS protection Terraform module with Shield Advanced" → IMPLEMENT ("implement" + "module")
"write the rate-limiting and shield configuration manifest" → IMPLEMENT ("write" + "manifest" + "configuration")
"scaffold the Cloudflare DDoS rule set configuration" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-057-003 — DDoSProtection: SECURITY fires on DDoS security vocabulary

"enforce volumetric attack mitigation via DDoS protection layer" → SECURITY ("enforce" in security context)
"harden the ingress with SYN flood protection and rate limiting" → SECURITY ("harden")
"configure TLS termination with DDoS-aware routing" → SECURITY ("TLS termination" = SECURITY compound phrase)

---

## B14-compiler-057-004 — DDoSProtection: DOCUMENT compilation

"document the DDoS protection strategy ADR and incident response playbook" → DOCUMENT ("document" + "ADR") + DOCUMENT ("playbook" in documentation context)
"write the DDoS runbook for on-call teams" → DOCUMENT ("write" + "runbook")
CompiledPredicateIds: ["DOCUMENT"]

---

## B14-compiler-057-005 — DDoSProtection: gap vocabulary — active attack response

Confirmed gap verbs for DDoS response operations (fire zero predicates):
  block (IP block), ban, rate-limit (operational), absorb, scrub, filter (traffic filter), redirect (traffic), blackhole, anycast (routing), quarantine
  "block the attacking IP range" → gap | "redirect DDoS traffic to scrubbing center" → gap
