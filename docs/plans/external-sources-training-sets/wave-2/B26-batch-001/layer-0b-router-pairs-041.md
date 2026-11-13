# Layer 0-B — Router Pairs (041) — SSRFAudit Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R041-001 — SSRF audit → 6.3

R041-001 test: "Audit all URL fetch endpoints for SSRF vulnerability."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R041-002 — SSRF + IMPLEMENT allowlist → 4.1 + 6.3

R041-002 test: "Implement an outbound request allowlist and audit existing URL fetch paths for SSRF exposure."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | AllSubPhaseIds: [4.1, 6.3]
  TransitionType: feature_addition

---

## R041-003 — "call external payment API" → GapHandler

R041-003 test: "Call the external payment gateway API from the checkout service."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R041-004 — Cloud metadata SSRF → 6.3

R041-004 test: "Review webhook URL parameters for SSRF paths reaching the AWS metadata endpoint."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R041-005 — SSRF + test-writing → 5.2

R041-005 test: "Write security tests asserting the webhook service blocks SSRF to 169.254.169.254."
  SECURITY → 5.2 (write-tests context: "write tests" + "asserting")
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 5.2
