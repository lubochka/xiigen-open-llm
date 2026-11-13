# Layer 0-B — Router Pairs (009)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-009-001 — ARCHITECTURE: "blueprint" fires ARCHITECTURE → 3.5

A: ["ARCHITECTURE"] | trigger: "blueprint" in infra context
B: → 3.5
  Teaching: "blueprint" alone fires ARCHITECTURE without a separate design verb. "Infrastructure blueprint", "deployment blueprint" → ARCHITECTURE → 3.5.

---

## B14-router-009-002 — ARCHITECTURE: "diagram" fires ARCHITECTURE → 3.5 (or 3.1)

A: ["ARCHITECTURE"] | trigger: "diagram" in infra context
B: → 3.5
  Teaching: "diagram" fires ARCHITECTURE independently. "diagram the infrastructure components" → ARCHITECTURE → 3.5.

---

## B14-router-009-003 — ARCHITECTURE: "topology" fires ARCHITECTURE → 3.5

A: ["ARCHITECTURE"] | trigger: "topology" in network/infra context
B: → 3.5
  Teaching: "topology" in infrastructure context (network topology, replication topology, cluster topology) fires ARCHITECTURE → 3.5 without a separate design verb.

---

## B14-router-009-004 — ARCHITECTURE: "infrastructure" as standalone trigger

A: ["ARCHITECTURE"] | trigger: "infrastructure" as structural noun with design framing
B: → 3.5
  Teaching: "infrastructure" as a structural noun (not purely as an adjective) fires ARCHITECTURE → 3.5 when paired with design-framing verbs (design, architect, define, structure).

---

## B14-router-009-005 — v6-01 corruption: invented predicate

A: ["ARCHITECTURE", "DEVOPS"] ← v6-01 corruption (DEVOPS is not a valid predicate)
  Correct: ["ARCHITECTURE"] — "cloud infrastructure design" fires only ARCHITECTURE; DEVOPS is not in the predicate lexicon
  Teaching: v6-01 invents predicates not in the lexicon. Valid predicates: SECURITY, AUTHORIZATION, API_CONTRACT, ARCHITECTURE, IMPLEMENT, TEST, QA, UI, DOCUMENT. DEVOPS is not valid.

