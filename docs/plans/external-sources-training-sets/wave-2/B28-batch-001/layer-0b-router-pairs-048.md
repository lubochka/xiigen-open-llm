# Layer 0-B — Router Pairs (048) — QA+IMPLEMENT combo router reference
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-048-001 — IMPLEMENT+QA: canonical two-flow combo

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT+QA → always feature_addition. QA gate-only, does not override TransitionType.

## B28-router-048-002 — QA alone: one flow, no TransitionType

A: ["QA"] | [ReleaseGateFlow71] | no TransitionType
  Teaching: QA alone → gate only. Always true regardless of Round context.

## B28-router-048-003 — Sentence-order independence: QA first in sentence, IMPLEMENT second

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: Predicate output order is canonical regardless of sentence order.

## B28-router-048-004 — Multiple IMPLEMENT triggers + QA: same two flows

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: All IMPLEMENT triggers deduplicate. Output is still IMPLEMENT+QA → two flows.

## B28-router-048-005 — IMPLEMENT+QA complete router reference

A: Any number of IMPLEMENT triggers (build, implement, create service, develop service) + any QA trigger (QA verb/noun/modifier)
  Output: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: Complete IMPLEMENT+QA router reference for all sub-phase 7.1 contexts.
