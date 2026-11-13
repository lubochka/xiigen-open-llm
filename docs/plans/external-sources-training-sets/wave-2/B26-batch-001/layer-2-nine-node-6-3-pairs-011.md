# Layer 2 — Nine-Node Flow Pairs (011)
# Batch: B26-batch-001 | Scenario: B26 — Security audit | SubPhase: 6.3
# Production date: 2026-05-05 | pair_kind: nine_node_flow

---

## N011 — SecurityAuditFlow63: CheckoutService CSRF Audit

Request: "Verify all state-changing endpoints in the CheckoutService include CSRF token validation."

### Node 1 — Classification
  CompiledPredicateIds: ["TEST", "SECURITY", "API_CONTRACT"] | SubPhase: 6.3

### Node 2 — Context Assembly
  Service: CheckoutService | Attack Vector: CSRF
  State-changing endpoints: POST /checkout, POST /apply-coupon, DELETE /cart-item

### Node 3 — Audit Plan
  3.1 Submit POST /checkout without CSRF token — expect 403
  3.2 Submit POST /apply-coupon with invalid CSRF token — expect 403
  3.3 Submit DELETE /cart-item from cross-origin — expect 403
  3.4 Verify SameSite=Strict on session cookie

### Node 4 — Tool Resolution
  HTTP test client with cross-origin simulation

### Node 5 — Audit Execution
  POST /checkout without token: 200 OK (CRITICAL — CSRF unprotected) | POST /apply-coupon: 403 (SAFE)
  DELETE /cart-item: 200 OK (HIGH) | SameSite cookie: None (HIGH)

### Node 6 — Verification
  1 CRITICAL (checkout endpoint unprotected) + 2 HIGH confirmed

### Node 7 — Quality Gate
  Status: FAIL | Critical: 1 | High: 2

### Node 8 — Output Assembly
  OutputType: SecurityAuditReport
  Remediation: Add CSRF middleware to checkout endpoint; set SameSite=Strict on all session cookies

### Node 9 — Result
  Result: FAIL — 1 CRITICAL, 2 HIGH CSRF findings
