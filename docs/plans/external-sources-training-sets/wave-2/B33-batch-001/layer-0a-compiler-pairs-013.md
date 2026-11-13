# Layer 0-A — Compiler Pairs (013)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-013-001 — gap: "fine-tune" as AI capability request

A: "We need to fine-tune the model on our internal support tickets."
B: [] — "fine-tune" = AI training vocabulary. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-013-002 — IMPLEMENT fires: "build fine-tuning pipeline"

A: "Build a fine-tuning pipeline that ingests support tickets and triggers retraining jobs."
B: ["IMPLEMENT"] — "build" = code verb. "fine-tuning pipeline" is the software artifact. IMPLEMENT fires despite AI context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-013-003 — gap: "fine-tune" as observation

A: "The model outputs would improve if we fine-tuned on domain-specific data."
B: [] — conditional AI quality observation. No predicate fires. "fine-tune" = gap vocabulary.
CompiledPredicateIds: []

---

## C0A-013-004 — IMPLEMENT+DOCUMENT fires: build + document fine-tuning

A: "Implement the fine-tuning job scheduler and document the tuning configuration schema."
B: ["IMPLEMENT", "DOCUMENT"] — "implement" fires IMPLEMENT; "document" fires DOCUMENT. "fine-tuning" is AI object, not predicate trigger.
CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]

---

## C0A-013-005 — gap: "adapt the model" via fine-tuning

A: "Adapt the model by fine-tuning it on medical records to improve diagnostic vocabulary."
B: [] — "adapt" = AI vocabulary; "fine-tuning" = AI method. No code verb. Both → gap.
CompiledPredicateIds: []
