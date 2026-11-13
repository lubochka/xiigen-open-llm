# Layer 0-B — Router Pairs (056) — APTHunting Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R056-001 — APT hunting audit → 6.3

R056-001 test: "Audit authentication and network logs for APT lateral movement and credential harvesting indicators."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R056-002 — "monitor network traffic" → GapHandler

R056-002 test: "Monitor all network traffic to baseline normal behavior patterns."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R056-003 — APT privilege escalation → 6.3 dual

R056-003 test: "Audit for APT privilege escalation via compromised service account credentials and RBAC role injection."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | TrustState: deterministic_generic

---

## R056-004 — Persistence mechanism scan → 6.3

R056-004 test: "Scan for APT persistence mechanisms including scheduled task injection and registry key modifications."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R056-005 — APT + DOCUMENT → 6.3 + 6.2

R056-005 test: "Review APT hunting findings for exfiltration paths and document affected data stores and credential scope."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation
