# Layer 0-A — Compiler Pairs (023)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-023-001 — gap: "model evolution" as capability aspiration

A: "The model should evolve the system over time as it processes more data."
B: [] — "evolve the system" in AI model context = AI adaptation vocabulary. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-023-002 — gap: "model evolution" via continued training

A: "Allow model evolution through continued pre-training on new domain corpora."
B: [] — "allow model evolution" = AI training strategy. No code verb fires. "continued pre-training" → AI vocabulary. Gap.
CompiledPredicateIds: []

---

## C0A-023-003 — IMPLEMENT fires: build model versioning system

A: "Build a model versioning system that tracks evolution across training checkpoints."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "model versioning system" is the software artifact. "evolution" is AI context only.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-023-004 — ARCHITECTURE+IMPLEMENT fires: design + build model lifecycle management

A: "Design the model lifecycle management architecture and implement the checkpoint promotion pipeline."
B: ["ARCHITECTURE", "IMPLEMENT"] — "design" + "architecture" → ARCHITECTURE; "implement" → IMPLEMENT. "model lifecycle" is AI context in object position.
CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]

---

## C0A-023-005 — gap: "evolve the system" through orchestrator adaptation

A: "Evolve the system by updating the orchestrator's skill registry as new capabilities emerge."
B: [] — "evolve the system" = AI system evolution vocabulary. "updating the orchestrator's skill registry" = AI capability management → gap. No code verb fires.
CompiledPredicateIds: []
