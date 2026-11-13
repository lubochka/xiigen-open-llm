# Held-Out and Corruption Pairs (003)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H003-001 — Held-out: "blueprint" fires ARCHITECTURE

A: "Produce the infrastructure blueprint for the new data center expansion."
  Expected: ["ARCHITECTURE"]
  Held-out: "blueprint" fires ARCHITECTURE independently.

---

## H003-002 — Held-out: gap — "move to cloud"

A: "Move our on-premises services to the cloud."
  Expected: []
  Held-out: "move" is operational gap; "cloud" is platform vocabulary gap.

---

## H003-003 — Held-out: ARCHITECTURE+SECURITY — zero-trust design

A: "Design the zero-trust network architecture — enforce mTLS on all service communication."
  Expected: ["ARCHITECTURE", "SECURITY"]
  Held-out for evaluation. mTLS fires SECURITY; design fires ARCHITECTURE.

---

## C003-001 — Corruption v6-02: gap becomes ARCHITECTURE

A: "Set up the Kubernetes cluster for production."
  Correct: []
  Corrupt: ["ARCHITECTURE"] ← v6-02 (gap request incorrectly defaults to ARCHITECTURE)
  Teaching: v6-02 suppresses gap by defaulting to a plausible predicate. "set up" is gap. Must remain gap.

---

## C003-002 — Corruption v6-01: CLOUD invented as predicate

A: "Design the cloud infrastructure topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "CLOUD"] ← v6-01 (CLOUD is not a valid predicate)
  Teaching: v6-01 invents CLOUD, DEVOPS, INFRA as predicates. None exist. Only 9 valid predicates.

