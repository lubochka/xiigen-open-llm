# Layer 0-B — Router Pairs (060) — Complete Router Reference (Round 3)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-060-001 — Round 3 flow mapping summary

Round 3 predicate → flow mapping:
  TEST (verification, chaos engineering, capacity, SLA, migration gate, observability check) → TestingFlow
  IMPLEMENT (service rule in all Round 3 contexts) → ImplementationFlow41
  DOCUMENT (changelog, release notes, executive dashboard, stakeholder report, criteria) → DocumentationFlow62
  SECURITY (secure verb in TLS, signing, access control contexts) → SecurityFlow6x
  QA (abbreviation in all positions) → ReleaseGateFlow71 (always last)
  ARCHITECTURE (design + architecture noun) → ArchitectureFlow31

## B28-router-060-002 — TransitionType resolution for Round 3

IMPLEMENT present → feature_addition (regardless of other predicates)
  No IMPLEMENT + (TEST or DOCUMENT or SECURITY or ARCHITECTURE) → documentation
  QA alone → no TransitionType
  Gap alone → knowledge_edge (no TransitionType)

## B28-router-060-003 — QA + any non-IMPLEMENT predicate: documentation TransitionType

A: ["TEST", "QA"] | documentation | ["SECURITY", "QA"] | documentation | ["DOCUMENT", "QA"] | documentation
  Teaching: IMPLEMENT is the only predicate that produces feature_addition. QA alone or with non-IMPLEMENT predicates → documentation or no TransitionType.

## B28-router-060-004 — Maximum Round 3 predicate set

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "SECURITY", "DOCUMENT", "QA"] | [ArchitectureFlow31, ImplementationFlow41, TestingFlow, SecurityFlow6x, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Six flows maximum. All predicates fire independently. No cap on flow count.

## B28-router-060-005 — Gap vocabulary produces knowledge_edge in Round 3

A: Any Round 3 gap term (promote, cutover, flip, toggle, rollout, DNS record, TTL, weighted routing, round-robin, telemetry, dashboards, uptime, error budget, tag, semver, commit log, alert, alerting, steady state hypothesis, fault injection alone)
  Output: [] | GapFlow | knowledge_edge
  Teaching: Pure gap → knowledge_edge. Not GapHandler. Not SCHEDULE_CAPABILITY_LEARNING.
