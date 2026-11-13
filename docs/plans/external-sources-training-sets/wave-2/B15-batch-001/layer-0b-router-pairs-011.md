# Layer 0-B — Router Pairs (011)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: router

---

## B15-router-011-001 — IMPLEMENT alone (bootstrap)

A: "Bootstrap the inventory service with dependency injection and logging."
Router: ["IMPLEMENT"] → ImplementationFlow41 (4.1) | TransitionType: feature_addition

---

## B15-router-011-002 — IMPLEMENT+API_CONTRACT+SECURITY three flows

A: "Build the payment processor, integrate with Stripe, and secure the webhook verification."
Router: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow

---

## B15-router-011-003 — IMPLEMENT+TEST: write E2E for service

A: "Implement the order submission API and write E2E tests for the submission flow."
Router: ["IMPLEMENT", "TEST"] → ImplementationFlow41 → E2ETestingFlow53 (5.3)

---

## B15-router-011-004 — IMPLEMENT+DOCUMENT+QA three flows

A: "Build the order analytics service, document the metrics API, and QA the reporting."
Router: ["IMPLEMENT", "DOCUMENT", "QA"] → ImplementationFlow41 ‖ DocumentationFlow → QAGatingFlow73

---

## B15-router-011-005 — v6 corruption: QA before IMPLEMENT

v6 corrupt: "Build order service and QA" → QAGatingFlow73 → ImplementationFlow41 (QA first)
Correct: ImplementationFlow41 → QAGatingFlow73 (QA always last)
Error: absolute ordering constraint violated.
