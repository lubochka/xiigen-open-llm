# Layer 0-C — Knowledge-Edge Pairs (026)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-026-001 — "subscription" domain noun: no predicate fires

"subscription", "recurring billing", "plan" are domain vocabulary.
"create a SubscriptionService" → "create" fires IMPLEMENT. "subscription" does not fire anything.

---

## B15-kedge-026-002 — "subscription domain model" fires ARCHITECTURE

"Define the subscription domain model" → ARCHITECTURE fires.
"subscription domain model" = domain noun (subscription) + model → ARCHITECTURE trigger.
Teaching: any "[domain noun] model" fires ARCHITECTURE.

---

## B15-kedge-026-003 — "write unit tests" vs "write integration tests"

"write unit tests" → TEST → routes to TestingFlow51 (sub-phase 5.1).
"write integration tests" → TEST → routes to TestingFlow51 (sub-phase 5.1).
"write end-to-end tests" → TEST → routes to E2ETestingFlow53 (sub-phase 5.3).
Both unit and integration → sub-phase 5.1. Only E2E → sub-phase 5.3.

---

## B15-kedge-026-004 — "billing provider" in "integrate with billing provider"

"integrate with the billing provider" → API_CONTRACT fires on "integrate".
"billing provider" names the external system — external system presence is not required for API_CONTRACT to fire.

---

## B15-kedge-026-005 — "tiered billing" does not fire ARCHITECTURE

"Refactor SubscriptionService to support tiered billing" → IMPLEMENT (refactoring) only.
"tiered billing" is a feature description, not a domain model or architecture term.
Teaching: ARCHITECTURE requires explicit architecture vocabulary (domain model / bounded context / architect verb / architecture noun).
