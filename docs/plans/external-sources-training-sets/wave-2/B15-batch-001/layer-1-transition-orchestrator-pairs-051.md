# Layer 1 — Transition Orchestrator Pairs (051)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-051-001 — Complete TransitionType rules for 028–060 range

TransitionType: feature_addition — all new service builds (default).
TransitionType: refactoring — when "refactor" verb is present (overrides feature_addition regardless of other verbs).
No other TransitionType exists in B15 scenario.

---

## B15-t1-051-002 — "extend" verb TransitionType confirmed

A: "Extend the CacheService to add a cold-start warming strategy."
B: TransitionType: feature_addition | "extend" → IMPLEMENT | feature_addition (not refactoring).
"extend" adds new capability to an existing service → feature_addition, not refactoring.

---

## B15-t1-051-003 — "refactor" + "extend" mixed verb TransitionType

A: "Refactor CacheService and extend it with a warming strategy."
B: TransitionType: refactoring | "refactor" wins over "extend".
TransitionType: refactoring overrides feature_addition — "refactor" always wins when present.

---

## B15-t1-051-004 — SelectedLogicalTransition is the Node 01 A field input (Layer 2 connection)

The SelectedLogicalTransition produced by the transition orchestrator (Layer 1) becomes the Node 01 A field in the nine-node flow (Layer 2).
SubPhaseIds and TransitionType are carried into the nine-node flow as part of the transition record.

---

## B15-t1-051-005 — No GapHandler in B15 (confirmed for 028–060)

All requests in 028–060 fire at least IMPLEMENT. GapHandler is never triggered.
SubPhaseIds always contains at least ["4.1"]. Empty SubPhaseIds is never valid in B15.
