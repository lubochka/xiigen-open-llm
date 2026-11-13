# Layer 0-C — Knowledge Edge Pairs (005)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E005-001 — "QA" in role titles fires QA

A: "The QA engineer will validate the release." → ["QA", "TEST"]
  "QA engineer" → QA | "validate" → TEST
  Teaching: Role titles containing QA (QA engineer, QA lead, QA manager, QA analyst) fire QA.

---

## E005-002 — "SLO" and "SLA" are gap

A: "Ensure the SLO (Service Level Objective) for the PaymentService is within bounds." → []
  Teaching: SLO, SLA, error budget, latency percentile → gap (observability/monitoring vocabulary).

---

## E005-003 — "gate" in QA context fires QA

A: "The release gate must pass QA before promotion." → ["QA"]
  "QA" → QA | "release gate" → QA (in QA compound context)
  Teaching: "gate" fires QA only when paired with "QA" compound vocabulary.

---

## E005-004 — "rollout" is gap

A: "Rollout the new feature to 100% of users." → []
  Teaching: "rollout", "gradual rollout", "canary rollout", "feature flag rollout" → gap.

---

## E005-005 — "QA" in "QA automation" fires QA

A: "Build the QA automation suite for the release pipeline." → ["QA", "IMPLEMENT"]
  "QA automation" → QA | "build" → IMPLEMENT
  Teaching: "QA automation" fires QA. "build" fires IMPLEMENT. Two predicates.

