# Layer 1 — Transition Orchestrator Pairs (060) — Complete TransitionType Reference Summary
# Batch: B26-batch-001 | Scenario: B26 — Security audit and compliance
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T060-001 — Complete TransitionType priority rules (master reference)

Rule 1: IMPLEMENT present → feature_addition (highest priority, overrides everything)
Rule 2: DOCUMENT present, IMPLEMENT absent → documentation
Rule 3: SECURITY alone (audit context, no DOCUMENT/IMPLEMENT) → feature_addition
Rule 4: AUTHORIZATION alone → feature_addition
Rule 5: SECURITY+AUTHORIZATION (no DOCUMENT/IMPLEMENT) → feature_addition
Rule 6: QA in 6.3 context → additive, does not determine TransitionType (SECURITY drives)
Rule 7: Zero predicates → N/A (GapHandler)

---

## T060-002 — Six common patterns and their TransitionType

Pattern A: audit only → feature_addition
Pattern B: audit + document → documentation
Pattern C: implement + audit → feature_addition
Pattern D: audit + QA → feature_addition (QA additive)
Pattern E: implement + audit + document → feature_addition (IMPLEMENT overrides)
Pattern F: gap vocabulary only → N/A

---

## T060-003 — "ensure quality of security" full trace

T060-003 test: "Ensure the quality of security controls across the authentication layer."
  SECURITY → 6.3 (not QA — "quality of security" ≠ "QA") | TransitionType: feature_addition
  QA does NOT fire. TransitionType = feature_addition.

---

## T060-004 — Dual predicate with all four TransitionType tests

Privilege escalation + audit → feature_addition
Privilege escalation + document → documentation
Privilege escalation + implement → feature_addition (IMPLEMENT overrides)
Privilege escalation + no predicates (e.g. "manage privilege escalation") → N/A

---

## T060-005 — Sub-phase 5.2 TransitionType behavior

SECURITY → 5.2 (write-tests context): TransitionType = feature_addition
Rationale: Writing tests produces a new test artifact = feature_addition in 5.2 context.
TransitionType is consistent across sub-phases for SECURITY: always feature_addition unless DOCUMENT present without IMPLEMENT.
