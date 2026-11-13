# Layer 0-B — Router Pairs (020)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: router

---

## R020-001 — SECURITY+QA → [6.3, 7.3]: network perimeter audit + ingress filtering QA
Request: "Audit the network perimeter configuration — TLS termination and open port exposure — and QA ingress filtering."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]

## R020-002 — SECURITY → 6.3: MITM attack surface review
Request: "Review the inter-service communication channels for MITM attack exposure — confirm mutual TLS and certificate validation."
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
  Note: "MITM", "mutual TLS", "certificate validation" all fire SECURITY.

## R020-003 — SECURITY+DOCUMENT → [6.3, 6.2]: network segmentation audit documented
Request: "Audit network segmentation between the DMZ and internal services for security misconfigurations and document all exposure findings."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"] | SubPhases: [6.3, 6.2]

## R020-004 — Zero predicates → GapHandler: "network latency review"
Request: "Review the network latency between services and optimize routing tables."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Gap: Network performance reviews without security co-occurrence are zero predicates.

## R020-005 — SECURITY+QA → [6.3, 7.3]: replay attack vulnerability audit + idempotency QA
Request: "Audit the WebhookService for replay attack vulnerabilities — verify nonce generation and timestamp validation — and QA idempotency controls."
  CompiledPredicateIds: ["SECURITY", "QA"] | SubPhases: [6.3, 7.3]
