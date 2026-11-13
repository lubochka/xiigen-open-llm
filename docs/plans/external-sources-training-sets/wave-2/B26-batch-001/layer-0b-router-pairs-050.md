# Layer 0-B — Router Pairs (050) — RedTeamExercise Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R050-001 — Red team findings review → 6.3

R050-001 test: "Review the red team findings for authentication bypass and credential harvesting vectors."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R050-002 — "run a red team exercise" → GapHandler

R050-002 test: "Run a red team exercise to test the incident response team's detection capabilities."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R050-003 — Red team attack path + AUTHORIZATION → 6.3 dual

R050-003 test: "Audit the attack paths from the red team: privilege escalation via JWT claim forgery and IDOR in billing."
  SECURITY → 6.3 | AUTHORIZATION → 6.3 | TrustState: deterministic_generic

---

## R050-004 — Red team + DOCUMENT → 6.3 + 6.2

R050-004 test: "Audit red team findings for persistence mechanisms and document a risk remediation roadmap."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation

---

## R050-005 — Lateral movement scan → 6.3

R050-005 test: "Scan network segmentation for lateral movement paths identified in the red team exercise."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | SubPhase: 6.3
