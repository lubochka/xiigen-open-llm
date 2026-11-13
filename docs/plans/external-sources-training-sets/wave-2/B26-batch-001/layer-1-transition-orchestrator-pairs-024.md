# Layer 1 — Transition Orchestrator Pairs (024) — TransitionType Reference for Sub-Phase 6.3
# Batch: B26-batch-001 | Scenario: B26 — Security and GDPR compliance review
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T024-001 — TransitionType: feature_addition — criteria for 6.3

feature_addition applies when:
  - SECURITY fires alone (audit produces new artifact = feature_addition)
  - SECURITY+QA (both produce new review artifacts)
  - SECURITY+IMPLEMENT (implementation of new security feature)
  - SECURITY+AUTHORIZATION+QA (multi-predicate audit set)

Examples:
  "Audit the JWT configuration." → SECURITY → feature_addition
  "Audit and QA the access controls." → SECURITY+QA → feature_addition

---

## T024-002 — TransitionType: documentation — criteria for 6.3

documentation applies when:
  - SECURITY+DOCUMENT (without IMPLEMENT)
  - SECURITY+AUTHORIZATION+DOCUMENT (without IMPLEMENT)
  - DOCUMENT is present AND IMPLEMENT is absent

Examples:
  "Audit and document the encryption findings." → SECURITY+DOCUMENT → documentation
  "Review privilege escalation and document attack vectors." → SECURITY+AUTHORIZATION+DOCUMENT → documentation

---

## T024-003 — TransitionType: QA alone → no contribution to TransitionType in 6.3

Per predicate rules: QA alone does not contribute TransitionType in sub-phase 6.3 context.
QA contributes TransitionType in its own sub-phase 7.3 context.
In 6.3: SECURITY drives TransitionType; QA is additive but not the determiner.

Example:
  "QA the security audit findings." → SECURITY drives feature_addition; QA adds 7.3 routing but not TransitionType.

---

## T024-004 — TransitionType: unknown / N/A — gap path

TransitionType is unknown / N/A when:
  - Zero predicates (all-gap vocabulary)
  - GapHandler path activated
  - No sub-phase routable from predicate set

Examples:
  "Ensure GDPR compliance." → [] → N/A
  "Schedule the pen test." → [] → N/A

---

## T024-005 — Predicate interaction summary for TransitionType determination

  IMPLEMENT present → feature_addition (overrides documentation)
  DOCUMENT present, IMPLEMENT absent → documentation
  SECURITY or QA alone, no DOCUMENT, no IMPLEMENT → feature_addition
  Zero predicates → N/A (GapHandler)

Priority: IMPLEMENT > DOCUMENT > (SECURITY/QA alone → feature_addition) > (zero → N/A)
