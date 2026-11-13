# Layer 0-B — Router Pairs (036) — Complete SECURITY Routing Reference
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R036-001 — SECURITY solo → 6.3 in audit context

R036-001 test: "Audit the bcrypt password hashing configuration."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R036-002 — SECURITY 5.2 exception: write-test context

R036-002 test: "Write security unit tests for the AuthService password hashing layer."
  SECURITY → 5.2 (write-test context — action verb "write tests" → SecurityTestingFlow52)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 5.2
  Teaching: "write security tests" → 5.2. "audit security controls" → 6.3.

---

## R036-003 — "ensure quality of security" → SECURITY → 6.3, NOT QA

R036-003 test: "Ensure the quality of security controls across the authentication layer."
  SECURITY → 6.3 ("security controls" + "authentication") | QA does NOT fire (no literal "QA")
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## R036-004 — Zero predicates (all gap vocabulary) → GapHandler

R036-004 test: "Make the system more secure overall."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  Teaching: "more secure" is always gap — vague adjective, no technical action.

---

## R036-005 — Multi-group stacking → deterministic_generic confidence

R036-005 test: "Audit bcrypt hashing, TLS certificate pinning, and JWT token signing."
  SECURITY → 6.3 | Three independent SECURITY triggers → TrustState: deterministic_generic
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3 | TrustState: deterministic_generic
