# Layer 0-A — Compiler Pairs (056)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-056-001 — WAFRules: ARCHITECTURE fires on WAF design vocabulary

"architect the WAF (Web Application Firewall) rule topology for public-facing endpoints" → ARCHITECTURE ("architect", "topology")
"design the WAF layer model with rate limiting and geo-blocking" → ARCHITECTURE ("design", "layer", "model")
"structure the WAF rule hierarchy and managed rule group selection" → ARCHITECTURE ("structure", "hierarchy")
"enable the WAF" → gap ("enable" is operational gap)

---

## B14-compiler-056-002 — WAFRules: IMPLEMENT fires on WAF IaC artifacts

"implement the WAF rule group Terraform module" → IMPLEMENT ("implement" + "module")
"write the WAF managed rule set configuration" → IMPLEMENT ("write" + "configuration")
"scaffold the AWS WAFv2 web ACL (Access Control List) manifest" → IMPLEMENT ("scaffold" + "manifest")

---

## B14-compiler-056-003 — WAFRules: SECURITY fires on WAF security vocabulary

"enforce WAF rules to block OWASP (Open Web Application Security Project) Top 10 attacks" → SECURITY ("enforce" in security context)
"harden API gateway with WAF TLS inspection" → SECURITY ("harden", "TLS")
"configure WAF rate limit rules for DDoS (Distributed Denial of Service) mitigation" → SECURITY (security configuration)

---

## B14-compiler-056-004 — WAFRules: AUTHORIZATION fires on WAF access control

"restrict WAF rule management to security-admin IAM role" → AUTHORIZATION ("restrict" in access context, "IAM role")
"grant WAF read access to the audit-service service account" → AUTHORIZATION ("grant", "service account")

---

## B14-compiler-056-005 — WAFRules: API_CONTRACT fires on WAF endpoint exposure

"define the WAF-protected API contract for external clients" → API_CONTRACT ("define" + "API contract")
"expose the WAF-guarded endpoint with published rate-limit policy" → API_CONTRACT ("expose" + endpoint)
CompiledPredicateIds: ["API_CONTRACT"] for endpoint exposure vocabulary
