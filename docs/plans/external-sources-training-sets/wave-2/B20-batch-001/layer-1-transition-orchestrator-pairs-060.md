# Layer 1 — Transition Orchestrator Pairs (060) — Complete TransitionType reference: all B20 rules
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T060-001 — Complete TransitionType decision table (all B20 predicate combinations)

| CompiledPredicateIds | TransitionType | Notes |
|---------------------|----------------|-------|
| ["TEST"] | documentation | TEST alone; no construction |
| ["IMPLEMENT"] | feature_addition | IMPLEMENT alone (e.g., "generate") |
| ["DOCUMENT"] | documentation | DOCUMENT alone; no construction |
| ["TEST", "IMPLEMENT"] | feature_addition | Construction verb present |
| ["TEST", "SECURITY"] | feature_addition | Multi-predicate = feature_addition |
| ["TEST", "DOCUMENT"] | documentation | Two documentation-class predicates |
| ["TEST", "IMPLEMENT", "SECURITY"] | feature_addition | IMPLEMENT governs |
| ["IMPLEMENT", "TEST", "SECURITY", "DOCUMENT"] | feature_addition | IMPLEMENT governs |
| [] | unknown | No predicates |

---

## T060-002 — IMPLEMENT governs rule: complete statement

Rule: TransitionType is governed by the highest-priority predicate present.
Priority order: IMPLEMENT > SECURITY (when with IMPLEMENT) > TEST = DOCUMENT.
When IMPLEMENT is present: always feature_addition, regardless of how many other predicates fire.
When IMPLEMENT is absent and TEST+SECURITY present: feature_addition (multi-predicate default).
When only TEST or only DOCUMENT (or both): documentation.
When no predicates: unknown.

---

## T060-003 — Single TransitionType per request: invariant confirmed across all B20 phases

One TransitionType applies to the whole request.
Individual predicates do not carry separate TransitionTypes.
Multiple predicates → one orchestrator decision → one TransitionType.
This is a fundamental invariant of the B20 transition orchestration model.
