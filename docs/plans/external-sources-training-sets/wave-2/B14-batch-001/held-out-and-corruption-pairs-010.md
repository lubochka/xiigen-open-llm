# Held-Out and Corruption Pairs (010)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H010-001 — Held-out: ARCHITECTURE — strangler fig

A: "Design the strangler fig migration pattern for the monolith decomposition."
  Expected: ["ARCHITECTURE"]
  Held-out: "strangler fig migration pattern" fires ARCHITECTURE.

---

## H010-002 — Held-out: ARCHITECTURE+IMPLEMENT+TEST+DOCUMENT — full IaC

A: "Design the infrastructure topology, implement the Terraform modules, write integration tests to validate provisioning, and document the deployment runbook."
  Expected: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT"]
  Held-out: Four predicates, four flows, feature_addition TransitionType.

---

## H010-003 — Held-out: gap — deployment command

A: "Roll out the new version to the production cluster."
  Expected: []
  Held-out: "roll out" is operational gap. No design verb.

---

## C010-001 — Corruption v6-02: "topology" → ARCHITECTURE even without design verb

A: "The database topology is getting complex."
  Correct: []
  Corrupt: ["ARCHITECTURE"] ← v6-02 ("topology" triggers ARCHITECTURE even without design verb)
  Teaching: "topology" alone without a design verb → gap. "topology" fires ARCHITECTURE only with design/architect/define verbs.

---

## C010-002 — Corruption: TransitionType documentation → feature_addition (wrong)

A: ["ARCHITECTURE"] only | "design the topology"
  Correct: TransitionType: documentation
  Corrupt: TransitionType: feature_addition ← wrong (no IMPLEMENT predicate present)
  Teaching: TransitionType is feature_addition only when IMPLEMENT predicate is present. ARCHITECTURE alone → documentation.

