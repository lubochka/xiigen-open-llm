# Layer 2 — Capability Gap Extended Pairs (028)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B34-gap-028 — "brief" and "familiarize" vocabulary: extended execution

A: "Brief the engineering chapter on the new observability stack rollout."
B: Predicates: [] | GapId: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED

A: "Familiarize the new backend developers with the gRPC (gRPC Remote Procedure Call) service framework."
B: Predicates: [] | GapId: gap-skills-guidance-onboard-001 | DomainId: software-development
GapHandler: INVOKED | SCHEDULE_CAPABILITY_LEARNING: EMITTED | Verdict: GAP_LOGGED

A: "Brief the security team on the new threat landscape and implement the recommended firewall rules."
B: Predicates: ["IMPLEMENT"] | GapWarning: gap-skills-guidance-mentor-001 | DomainId: software-development
GapHandler: SUPPRESSED | SCHEDULE_CAPABILITY_LEARNING: NOT EMITTED | Verdict: IMPLEMENT_APPROVED + GAP_WARNING

A: "Familiarize the integration team with the OAuth2 (OAuth 2.0) provider and integrate the identity service."
B: Predicates: ["API_CONTRACT"] | GapWarning: gap-skills-guidance-onboard-001 | DomainId: software-development
GapHandler: SUPPRESSED | SCHEDULE_CAPABILITY_LEARNING: NOT EMITTED | Verdict: API_CONTRACT_APPROVED + GAP_WARNING
