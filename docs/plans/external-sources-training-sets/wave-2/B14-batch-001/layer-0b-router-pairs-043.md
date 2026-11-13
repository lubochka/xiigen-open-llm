# Layer 0-B — Router Pairs (043)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-043-001 — SnapshotSchedule: routing for ARCHITECTURE → 3.5

"Architect the snapshot retention hierarchy across regions."
  ARCHITECTURE → 3.5 | TransitionType: feature_addition | TrustState: deterministic_generic

---

## B14-router-043-002 — SnapshotSchedule: routing for IMPLEMENT + SECURITY

"Implement the snapshot schedule CronJob manifest and enforce KMS encryption."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | Sequential: 6.3 → 4.1
  TransitionType: feature_addition | AllSubPhaseIds: [4.1, 6.3]

---

## B14-router-043-003 — SnapshotSchedule: routing disambiguation — "schedule" verb vs noun

"Schedule the database snapshot at 02:00 UTC." → zero predicates (schedule as operational verb = gap)
"Implement the snapshot schedule policy." → IMPLEMENT → 4.1 (schedule is a noun here, implement is the trigger)
"Design the snapshot schedule policy." → ARCHITECTURE → 3.5 (design is trigger; schedule is noun)

---

## B14-router-043-004 — SnapshotSchedule: routing for DOCUMENT alone

"Document the snapshot schedule ADR."
  DOCUMENT → 6.2 | TransitionType: documentation | TrustState: deterministic_generic
