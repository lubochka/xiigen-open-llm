# Held-Out and Corruption Pairs (007)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H007-001 — Held-out: ARCHITECTURE — canary deployment

A: "Design the canary deployment architecture — traffic splitting rules and rollback triggers."
  Expected: ["ARCHITECTURE"]
  Held-out: "canary deployment architecture" fires ARCHITECTURE.

---

## H007-002 — Held-out: ARCHITECTURE — data mesh

A: "Architect the data mesh topology — define domain-oriented ownership boundaries."
  Expected: ["ARCHITECTURE"]
  Held-out: "data mesh topology" fires ARCHITECTURE.

---

## H007-003 — Held-out: gap — scale verb

A: "Scale the OrderService to handle Black Friday traffic."
  Expected: []
  Held-out: "scale" (operational verb) → gap.

---

## C007-001 — Corruption v6-01: MIGRATION invented

A: "Design the migration architecture from monolith to microservices."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "MIGRATION"] ← v6-01 (MIGRATION is not a valid predicate)
  Teaching: MIGRATION is not a predicate. Migration architectural design → ARCHITECTURE only.

---

## C007-002 — Corruption v6-03: ARCHITECTURE+DOCUMENT collapsed to DOCUMENT

A: ["ARCHITECTURE", "DOCUMENT"]
  Correct: [InfrastructureArchitectureFlow35, DocumentationFlow62]
  Corrupt: [DocumentationFlow62] only ← v6-03 (ARCHITECTURE suppressed by DOCUMENT)
  Teaching: DOCUMENT never suppresses ARCHITECTURE. Both flows always created.

