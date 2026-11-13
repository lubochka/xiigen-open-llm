# Layer 0-B — Router Pairs (052) — ThreatIntelReport Routing
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: router

---

## R052-001 — Threat intel review → 6.3

R052-001 test: "Review the threat intelligence report for IOCs matching our authentication service logs."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R052-002 — IOC scan → 6.3

R052-002 test: "Scan application logs for indicators of compromise from the latest CISA advisory."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R052-003 — "read the threat intel briefing" → GapHandler

R052-003 test: "Read through the weekly threat intelligence briefing for the security team."
  CompiledPredicateIds: [] | GapHandler: SCHEDULE_CAPABILITY_LEARNING

---

## R052-004 — Threat intel + credential exposure → 6.3

R052-004 test: "Audit exposed credentials in the threat intelligence feed against our active service accounts."
  SECURITY → 6.3 | CompiledPredicateIds: ["SECURITY"] | TrustState: deterministic_generic

---

## R052-005 — Threat intel + DOCUMENT → 6.3 + 6.2

R052-005 test: "Review threat intelligence IOCs against perimeter logs and document findings in the security incident register."
  SECURITY → 6.3 | DOCUMENT → 6.2 | TransitionType: documentation
