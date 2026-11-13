# Layer 0-C — Knowledge Edge Pairs (056)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-056-001 — WAFRules: gap vocabulary — WAF runtime operations

Confirmed gap vocabulary for WAF (Web Application Firewall) runtime operations (zero predicates):
  Action verbs: enable (WAF), disable, block (request — runtime), allow (request — runtime), monitor (WAF logs), sample, count, review (WAF logs), tune (rule sensitivity)
  Runtime nouns: WAF log, request match, rule match, IP reputation, bot signature, rate counter

---

## B14-edge-056-002 — WAFRules: SECURITY boundary — WAF enforcement compound phrases

SECURITY fires for:
  "enforce OWASP (Open Web Application Security Project) Top 10 WAF rules" → SECURITY ("enforce" + security context)
  "harden API gateway with WAF TLS inspection" → SECURITY ("harden", "TLS")
  "configure WAF rate limiting rules for DDoS mitigation" → SECURITY (security configuration context)
  "enable the WAF" → gap ("enable" is operational gap)
  "block the attacker IP" → gap ("block" runtime = gap)

---

## B14-edge-056-003 — WAFRules: API_CONTRACT boundary — WAF-protected endpoint exposure

API_CONTRACT fires when WAF is framed as an API protection contract:
  "define the WAF-protected API contract" → API_CONTRACT ("define" + "API contract")
  "expose the WAF-guarded endpoint with rate-limit policy" → API_CONTRACT ("expose" + endpoint)
  "block requests that violate the API contract" → API_CONTRACT (API contract violation context)
  "monitor WAF logs for API traffic" → gap ("monitor" is operational)

---

## B14-edge-056-004 — WAFRules: IMPLEMENT boundary — WAF IaC artifacts

IMPLEMENT fires for: WAF rule group Terraform module, WAF managed rule set configuration, AWS WAFv2 web ACL manifest, rate-limit rule configuration
  "implement the WAF rule group module" → IMPLEMENT
  "enable the WAF on the load balancer" → gap ("enable" is operational)
