# Held-Out and Corruption Pairs
# Batch: B32-batch-001
# Scenario: B32 — Vibe coding gap
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## HO-001 — "creative mode" + "implement" → IMPLEMENT fires; creative mode is gap

A: "Implement the notification badge in creative mode — no constraints, just flow."
Expected: IMPLEMENT fires on "implement". "creative mode", "no constraints", "just flow" → gap.
CompiledPredicateIds: ["IMPLEMENT"]. Routing: 4.1.

---

## HO-002 — pure vibe no verb: knowledge_edge

A: "Just vibe something out for the onboarding — whatever feels natural."
Expected: zero predicates → knowledge_edge. No action verb present.
capability_gap_id: gap-vibe-coding-style-001. DomainId: "software-development".

---

## HO-003 — "hack together" + "write": IMPLEMENT fires

A: "Hack together a quick write-up of the API handler for the cart endpoint."
Expected: IMPLEMENT fires on "write" and "handler". "hack together" → gap.
CompiledPredicateIds: ["IMPLEMENT"]. Routing: 4.1.

---

## COR-001 — v6-01: IMPLEMENT invented for "vibe code a prototype" (no action verb)

CORRUPT: "Vibe code a prototype for the dashboard." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: No IMPLEMENT term present — "vibe code" is gap, "prototype" is gap, "dashboard"
is gap. The corrupt model invented IMPLEMENT because the request sounds like software development.
Correct: CompiledPredicateIds: [] → knowledge_edge.

---

## COR-002 — v6-01: UI invented for "dashboard"

CORRUPT: "Vibe code a dashboard." → CompiledPredicateIds: ["UI"].
Why corrupt: "dashboard" is not in UI lexicon. The corrupt model assumed dashboard = UI.
Correct: CompiledPredicateIds: [] → knowledge_edge.

---

## COR-003 — v6-03: DomainId "project-management" for vibe coding gap

CORRUPT: Pure vibe coding zero-predicate request → DomainId: "project-management".
Why corrupt: Vibe coding is software-development domain. "project-management" DomainId is
only for project planning/managing vocabulary (B36, B37). Vibe coding gaps emit
DomainId: "software-development".
