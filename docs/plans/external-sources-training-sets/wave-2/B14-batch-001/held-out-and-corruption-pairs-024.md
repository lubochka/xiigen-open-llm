# Held-Out and Corruption Pairs (024)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H024-001 — Held-out: complete vocabulary coverage — all 9 predicates in one request

A: "Design the infrastructure topology, implement the Terraform modules, configure TLS and secrets, validate the network connectivity, get QA sign-off, document the ADRs, design the UI dashboard, define the API contract, and authorize the service accounts."
  Expected: ["ARCHITECTURE", "IMPLEMENT", "SECURITY", "TEST", "QA", "DOCUMENT", "UI", "API_CONTRACT", "AUTHORIZATION"]
  Held-out: nine-predicate request fires all 9 predicates. Nine concurrent flows.

---

## H024-002 — Held-out: "infra vocabulary" vs "app vocabulary" disambiguation

A: "Design the service architecture for the OrderService domain."
  Expected: ["ARCHITECTURE"] → 3.1 (app/domain vocabulary: "service architecture", "domain")
  Held-out: same ARCHITECTURE predicate, but 3.1 routing (not 3.5) due to domain vocabulary.

---

## H024-003 — Held-out: ARCHITECTURE alone with design verb → feature_addition

A: "Design the database topology for the data layer."
  Expected: ["ARCHITECTURE"] → TransitionType: feature_addition
  Held-out: ARCHITECTURE with "design" (not "document") → feature_addition despite no IMPLEMENT.

---

## C024-001 — Corruption v6-03: documentation TransitionType for ARCHITECTURE alone

A: "Design the observability infrastructure topology."
  Correct: TransitionType: feature_addition
  Corrupt: TransitionType: documentation ← v6-03 (incorrectly treats all ARCHITECTURE as documentation)
  Teaching: ARCHITECTURE alone with design verb (design, architect) → feature_addition. Only ARCHITECTURE+DOCUMENT without IMPLEMENT → documentation.

---

## C024-002 — Corruption v6-02: predicate count capped at two

A: "Design the topology, implement the IaC, enforce TLS, test connectivity, document ADRs."
  Correct: ["ARCHITECTURE", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT"] — 5 predicates, 5 flows
  Corrupt: ["ARCHITECTURE", "IMPLEMENT"] ← v6-02 (caps predicate count at 2)
  Teaching: No cap on predicate count. Every valid vocabulary fires its predicate. Five predicates → five flows. v6-02 incorrectly limits multi-predicate requests.
