# Layer 0-B — Router Pairs (038) — ClickjackingAudit Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R038-001 — Clickjacking audit → 6.3

R038-001 test: "Audit whether X-Frame-Options is set on all pages."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R038-002 — Clickjacking + IMPLEMENT → 4.1 + 6.3

R038-002 test: "Implement X-Frame-Options DENY on all pages and audit existing CSP frame-ancestors settings."
  IMPLEMENT → 4.1 | SECURITY → 6.3
  PrimarySubPhaseId: 6.3 | AllSubPhaseIds: [4.1, 6.3]

---

## R038-003 — Iframe embedding (gap) → GapHandler

R038-003 test: "Allow the dashboard to be embedded in the partner portal iframe."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R038-004 — Clickjacking + DOCUMENT → 6.3 + 6.2

R038-004 test: "Audit clickjacking exposure and document the X-Frame-Options findings."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation

---

## R038-005 — CSP frame-ancestors review → 6.3

R038-005 test: "Review CSP frame-ancestors directive for clickjacking exposure."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic
