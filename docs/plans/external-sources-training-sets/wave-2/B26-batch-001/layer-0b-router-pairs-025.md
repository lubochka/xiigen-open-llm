# Layer 0-B — Router Pairs (025) — SOC2Audit Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R025-001 — SOC2 audit verb → SECURITY → 6.3

R025-001 test: "Audit authentication controls against SOC2 CC6 requirements."
  "audit" + "authentication controls" → SECURITY → SubPhase 6.3
  "SOC2 CC6" → gap context label; does not alter routing
  CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3

---

## R025-002 — SOC2 + DOCUMENT → dual routing

R025-002 test: "Audit the SOC2 access control evidence and document findings for the auditor package."
  SECURITY → 6.3 | DOCUMENT → 6.2
  PrimarySubPhase: 6.3 | TransitionType: documentation (DOCUMENT present, IMPLEMENT absent)

---

## R025-003 — SOC2 gap path → GapHandler

R025-003 test: "Ensure the product is SOC2 compliant before year-end."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING
  No sub-phase assignment; no flow instantiated.

---

## R025-004 — SOC2 + IMPLEMENT + SECURITY → feature_addition

R025-004 test: "Implement audit logging for SOC2 CC7 event monitoring and audit the current log coverage for gaps."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | TransitionType: feature_addition (IMPLEMENT present)
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]

---

## R025-005 — SOC2 + AUTHORIZATION routing

R025-005 test: "Audit user privilege assignments and RBAC boundaries per SOC2 CC6.3."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 (RBAC fires both)
  Two routing decisions both at 6.3 — not collapsed.
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
