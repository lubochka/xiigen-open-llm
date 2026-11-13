# Layer 0-B — Router Pairs (005)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: router

---

## B14-router-005-001 — ARCHITECTURE+IMPLEMENT → TransitionType: feature_addition

A: ["ARCHITECTURE", "IMPLEMENT"] + verb context: "design" + "implement"
B: SubPhase 3.5 + SubPhase 4.1 | TransitionType: feature_addition
  Teaching: "implement" drives feature_addition TransitionType. "design" alone → TransitionType: documentation. Combined: feature_addition wins.

---

## B14-router-005-002 — ARCHITECTURE+DOCUMENT → TransitionType: documentation

A: ["ARCHITECTURE", "DOCUMENT"] + verb context: "design" + "document/diagram"
B: SubPhase 3.5 + SubPhase 6.2 | TransitionType: documentation
  Teaching: "document", "diagram" verbs without "implement" → TransitionType: documentation.

---

## B14-router-005-003 — ARCHITECTURE alone + "design" verb → TransitionType: documentation

A: ["ARCHITECTURE"] + verb context: "design/architect" only
B: SubPhase 3.5 | TransitionType: documentation
  Teaching: A pure architecture design request (no IMPLEMENT predicate) → TransitionType: documentation. Design artifacts are documentation outputs.

---

## B14-router-005-004 — ARCHITECTURE with "refactor" → TransitionType: refactoring

A: ["ARCHITECTURE"] + verb context: "refactor the architecture" 
B: SubPhase 3.5 | TransitionType: refactoring
  Teaching: "refactor" in architecture context → TransitionType: refactoring. This overrides documentation even though no code is produced.

---

## B14-router-005-005 — v6-02 corruption: gap suppression

A: [] (gap path)
B: CORRUPT: model falls back to ARCHITECTURE without evidence → v6-02 (fallback suppression)
  Correct: [] → GapHandler; SCHEDULE_CAPABILITY_LEARNING emitted
  Teaching: v6-02 occurs when the model invents ARCHITECTURE as a default fallback for infra-sounding requests. Gap requests must remain gap.

