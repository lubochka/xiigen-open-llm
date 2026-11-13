# Layer 0-A — Compiler Pairs (020)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: compiler

---

## C020-001 — SECURITY+QA: network security review

A: "Audit the network perimeter configuration — check firewall rules, TLS termination, and open port exposure — and QA the ingress filtering controls."
  "audit" → SECURITY; "TLS termination" → SECURITY (tls trigger); "firewall rules" → SECURITY (security context); "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3

---

## C020-002 — SECURITY: MITM (Man-in-the-Middle) attack surface review

A: "Review the inter-service communication channels for MITM attack exposure — confirm mutual TLS and certificate validation."
  "review" (security context) → SECURITY; "MITM" → SECURITY; "mutual TLS" → SECURITY (tls trigger); "certificate validation" → SECURITY (certificate trigger)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C020-003 — SECURITY+DOCUMENT: network segmentation audit documented

A: "Audit the network segmentation between the DMZ (Demilitarized Zone) and internal services for security misconfigurations and document all exposure findings."
  "audit" → SECURITY; "security misconfigurations" → SECURITY; "document" → DOCUMENT
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhase: 6.3/6.2

---

## C020-004 — Gap: "network latency" is not SECURITY

A: "Review the network latency between services and optimize routing tables."
  "review" → gap (performance context, no security co-occurrence); "network latency" → gap; "optimize" → gap
  CompiledPredicateIds: [] → knowledge_edge
  Gap: Network performance reviews without security co-occurrence produce zero predicates.

---

## C020-005 — SECURITY+QA: replay attack protection review + QA

A: "Audit the WebhookService for replay attack vulnerabilities — verify nonce generation and timestamp validation — and QA the idempotency controls."
  "audit" → SECURITY; "replay attack vulnerabilities" → SECURITY; "nonce generation" → SECURITY; "QA" → QA
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhase: 6.3/7.3
