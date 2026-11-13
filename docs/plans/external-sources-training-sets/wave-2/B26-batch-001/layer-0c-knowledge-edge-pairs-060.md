# Layer 0-C — Knowledge Edge Pairs (060) — Complete Knowledge Edge Reference
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E060-001 — Complete solo-noun SECURITY triggers (master list, 037–060 topics)

Additional solo nouns confirmed from topics 037–060:
  CSRF | clickjacking | XXE | SSRF | TOCTOU | HMAC | IOC | CVE | SBOM | SRI | mTLS | APT
  "lateral movement" | "credential harvesting" | "privilege escalation" | "backdoor"
  "gadget chain" | "insecure deserialization" | "malware" | "typosquatting"

---

## E060-002 — Additional gap vocabulary confirmed from topics 037–060

Always gap regardless of context:
  "configure [any middleware]" without audit verb
  "add [any security feature]" without audit verb
  "update [packages/software]" without vulnerability vocabulary
  "schedule [any event]" (always gap: planning action)
  "run [any exercise]" (always gap: execution without audit vocabulary)
  "read [any report]" (always gap: passive consumption)
  "monitor [traffic/logs]" without attack vocabulary
  "optimize [latency/performance]" without vulnerability vocabulary

---

## E060-003 — Dual-predicate trigger summary (all B26 topics)

Complete dual-predicate vocabulary (fires SECURITY + AUTHORIZATION):
  privilege escalation | IDOR | wildcard permission | RBAC bypass | role claim forgery
  least-privilege access | RBAC role injection | unauthorized resource access
  IAM role + vulnerability vocabulary in same request

---

## E060-004 — "security review" noun phrase disambiguation

"security review" as a past event noun → gap
"security review" as a requested action verb phrase → SECURITY
Test: "the security review that happened last week" → gap (past event reference)
Test: "review the security controls" → SECURITY (action verb applied to security object)

---

## E060-005 — Trust state master calibration (all 60 topics)

Zero predicates → knowledge_edge (gap vocabulary only)
1 weak trigger (no audit verb) → probationary_accepted
1 strong trigger (solo noun with audit verb) → deterministic_generic (moderate)
2+ triggers OR dual-predicate → deterministic_generic (high)
3+ independent triggers → deterministic_generic (highest confidence)
