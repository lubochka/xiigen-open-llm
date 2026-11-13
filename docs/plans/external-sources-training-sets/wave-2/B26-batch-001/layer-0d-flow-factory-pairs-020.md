# Layer 0-D — Flow Factory Pairs (020)
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## F020-001 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (network perimeter TLS + ingress)
Input: "Audit the network perimeter — TLS termination and open port exposure — and QA ingress filtering controls."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]

## F020-002 — SECURITY → SecurityComplianceReviewFlow63 (MITM + mutual TLS + certificate)
Input: "Review inter-service communication channels for MITM attack exposure — confirm mutual TLS and certificate validation."
  CompiledPredicateIds: ["SECURITY"]
  Factory: FlowFactory.Create("SECURITY") → SecurityComplianceReviewFlow63

## F020-003 — SECURITY+DOCUMENT → [SecurityComplianceReviewFlow63, DocumentationFlow62] (network segmentation)
Input: "Audit network segmentation between DMZ and internal services for security misconfigurations and document exposures."
  CompiledPredicateIds: ["SECURITY", "DOCUMENT"]
  Factory: FlowFactory.CreateMulti(["SECURITY","DOCUMENT"]) → [SecurityComplianceReviewFlow63, DocumentationFlow62]

## F020-004 — Zero predicates → GapHandler (network latency review)
Input: "Review network latency between services and optimize routing tables."
  CompiledPredicateIds: [] | GapHandler.Emit(SCHEDULE_CAPABILITY_LEARNING)

## F020-005 — SECURITY+QA → [SecurityComplianceReviewFlow63, QATestingFlow73] (replay attack + nonce)
Input: "Audit WebhookService for replay attack vulnerabilities — verify nonce generation — and QA idempotency controls."
  CompiledPredicateIds: ["SECURITY", "QA"]
  Factory: FlowFactory.CreateMulti(["SECURITY","QA"]) → [SecurityComplianceReviewFlow63, QATestingFlow73]
