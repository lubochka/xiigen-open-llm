# Layer 2 — Capability Gap Extended Pairs (029)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-029 — "consult" and "seek advice" vocabulary: extended execution

A: "Consult with the data engineering team on the optimal batch processing strategy."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED

A: "Seek advice on the distributed locking mechanism from the concurrency specialists."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED

A: "Consult on the caching approach and implement the Redis (Redis cache) cluster integration."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: SUPPRESSED | Verdict: API_CONTRACT_APPROVED + GAP_WARNING

A: "Seek advice on the security architecture and harden the API (Application Programming Interface) gateway."
B: Predicates: ["SECURITY"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: SUPPRESSED | SCHEDULE_CAPABILITY_LEARNING: NOT EMITTED | Verdict: SECURITY_APPROVED + GAP_WARNING
