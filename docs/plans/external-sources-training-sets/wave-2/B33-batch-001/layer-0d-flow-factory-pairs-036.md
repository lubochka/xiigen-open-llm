# Layer 0-D — Flow Factory Pairs (036)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-036-001 — GapHandler: BERT standalone

A: [] | "BERT is used for sentence classification tasks."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "BERT" = AI model architecture. No code verb.

---

## FF-036-002 — IMPLEMENT: build BERT inference client

A: ["IMPLEMENT"] | "Build a BERT inference client that sends requests to the model server."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "BERT inference client" = implementation artifact.

---

## FF-036-003 — GapHandler: Llama standalone

A: [] | "Llama 3 requires significant compute for inference at full precision."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "Llama" = AI model family. No code verb.

---

## FF-036-004 — IMPLEMENT: implement Llama API client

A: ["IMPLEMENT"] | "Implement a Llama API client with streaming response support."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "implement" fires IMPLEMENT. "Llama API client" = implementation artifact.

---

## FF-036-005 — ARCHITECTURE+IMPLEMENT: design + build Llama serving layer

A: ["ARCHITECTURE", "IMPLEMENT"] | "Design the Llama serving architecture and implement the request routing layer."
Factory: [ArchitectureFlow31, ImplementationFlow41] | TransitionType: feature_addition
Ordering: ArchitectureFlow31 → ImplementationFlow41 (ARCHITECTURE before IMPLEMENT).
