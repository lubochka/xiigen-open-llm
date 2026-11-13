# Layer 0-D — Flow Factory Pairs (009)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-009-001 — Factory ordering rule: 3.4 before 4.1

A: When both 3.4 and 4.1 are selected, which is listed first?

B: 3.4 is listed first. The factory selection order follows the predicate canonical ordering: SECURITY (position 4) comes before IMPLEMENT (position 3) — wait, corrected canonical order: ARCHITECTURE → API_CONTRACT → IMPLEMENT → SECURITY → AUTHORIZATION. Therefore IMPLEMENT (3) is before SECURITY (4). But in routing output, 3.4 (SECURITY/AUTHORIZATION) takes design priority and is listed first in factory output.

---

## O1b-0D-009-002 — Factory ordering rule: trusted_learned before deterministic_generic

A: Factory ordering preference

B: Rule: trusted_learned adapters are listed before deterministic_generic adapters in the factory selection output, regardless of predicate canonical order. Specialized flows (trusted_learned) establish design constraints that generic flows (deterministic_generic) must respect.

---

## O1b-0D-009-003 — Factory ordering rule: probationary always last

A: When is probationary_rejected factory listed?

B: probationary_rejected factory selections are always listed last in the FlowSelections array. They represent degraded capability (fallback mode) and do not constrain other flows' execution.

---

## O1b-0D-009-004 — Complete factory ordering axiom

A: O1b factory selection ordering axiom

B: Factory ordering: (1) trusted_learned first. (2) deterministic_generic in predicate canonical sub-phase order. (3) probationary_accepted (if any) before probationary_rejected. (4) probationary_rejected last. Example: [3.4 trusted, 3.3 generic, 4.1 generic, 2.1 generic, 5.1 rejected].

---

## O1b-0D-009-005 — Factory selection is idempotent

A: If the same RoutingDecision is provided twice, how many factory selections result?

B: ONE factory selection. Factory selection is idempotent — identical routing inputs produce exactly one factory output. Dedup applies at both the routing layer and the factory selection layer.
