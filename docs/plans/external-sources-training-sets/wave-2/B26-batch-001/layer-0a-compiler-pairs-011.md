# Layer 0-A — Compiler Pairs (011)
# Batch: B26-batch-001 | Scenario: B26 — Security audit, SECURITY+QA, sub-phase 6.3
# Production date: 2026-05-05 | pair_kind: compiler

---

## C011-001 — SECURITY via "directory traversal"

A: "Scan the FileStorageService for directory traversal attacks in the file-path resolution logic."
  "scan" → SECURITY; "directory traversal attacks" → SECURITY; "file-path resolution" → gap (domain)
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## C011-002 — SECURITY + AUTHORIZATION via wildcard permission audit

A: "Audit the AdminService permission matrix for wildcard grants — flag any role with * (all) permissions."
  "audit" → SECURITY; "permission matrix" → AUTHORIZATION; "wildcard grants" → AUTHORIZATION+SECURITY; "role with * permissions" → AUTHORIZATION
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] | SubPhase: 6.3

---

## C011-003 — SECURITY + QA + TEST via "security acceptance test"

A: "Write security acceptance tests for the CheckoutService, QA the payment flow for data leakage."
  "write" → IMPLEMENT; "security acceptance tests" → TEST+SECURITY; "QA" → QA; "data leakage" → SECURITY
  CompiledPredicateIds: ["SECURITY", "IMPLEMENT", "TEST", "QA"] | SubPhase: 5.2/4.1/6.3

---

## C011-004 — SECURITY + ARCHITECTURE via "zero-trust architecture review"

A: "Review the IdentityService network architecture for zero-trust compliance — confirm no implicit trust between services."
  "review" (security context) → SECURITY; "network architecture" → ARCHITECTURE; "zero-trust" → SECURITY; "implicit trust" → SECURITY
  CompiledPredicateIds: ["SECURITY", "ARCHITECTURE"] | SubPhase: 6.3/3.5

---

## C011-005 — Gap: "phishing awareness" training is gap

A: "Run a phishing awareness training session for the development team."
  "run" → gap; "phishing awareness training" → gap (organizational training, not code/security-action); "development team" → gap
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING, domainId: "software-development"
  Teaching: Security awareness/training is gap. SECURITY fires on vulnerability assessment and enforcement actions in code/system context.
