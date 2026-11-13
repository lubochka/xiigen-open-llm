# Layer 0-A — Compiler Pairs (056) — APTHunting
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: compiler

---

## C056-001 — APT (Advanced Persistent Threat) hunting → SECURITY

A: "Audit authentication and network logs for APT (Advanced Persistent Threat) lateral movement and credential harvesting indicators."
  "audit" → SECURITY | "credential harvesting" → SECURITY | "lateral movement" → SECURITY (attack vector)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C056-002 — Threat hunting for persistence → SECURITY

A: "Scan the environment for APT persistence mechanisms including scheduled task injection and registry key modifications."
  "scan" → SECURITY | "persistence mechanisms" → SECURITY | "injection" → SECURITY
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C056-003 — "monitor network traffic" → gap (no security-action vocabulary)

A: "Monitor all network traffic to baseline normal behavior patterns."
  "monitor" → gap | "baseline normal behavior" → gap (observability task, no audit/vulnerability vocabulary)
  CompiledPredicateIds: [] → knowledge_edge

---

## C056-004 — APT hunting + AUTHORIZATION compromise → SECURITY + AUTHORIZATION

A: "Audit for APT privilege escalation via compromised service account credentials and RBAC role injection."
  "audit" → SECURITY | "privilege escalation" → SECURITY+AUTHORIZATION | "RBAC role injection" → SECURITY+AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3 | TrustState: deterministic_generic

---

## C056-005 — APT + DOCUMENT → SECURITY+DOCUMENT

A: "Review APT hunting findings for exfiltration paths and document the affected data stores and credential scope."
  "review" → SECURITY | "exfiltration" → SECURITY (attack vector) | "credential scope" → SECURITY | "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | TransitionType: documentation
