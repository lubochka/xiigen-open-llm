# Layer 0-C — Knowledge Edge Pairs (043)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-043-001 — AI+IMPLEMENT mixed: neural network wrapper patterns

Code verb patterns that produce IMPLEMENT from neural network requests:
- "Build a neural network wrapper" → IMPLEMENT (wrapper = software artifact)
- "Implement the neural network forward pass module" → IMPLEMENT (module = software artifact)
- "Create a neural network client" → IMPLEMENT (client = software artifact)
- "Refactor the neural network service" → IMPLEMENT (refactoring)

Pattern: code verb + neural network + software artifact noun → IMPLEMENT.

---

## KE-043-002 — Diagnostic statements never fire predicates

Rule: Diagnostic or observational statements about AI training behavior never fire predicates.
"The training loop experiences overfitting after 50 epochs" = diagnostic → []
"Neural network with overfitting issues and poor evaluation metrics" = description → []
"The training loss is not converging with gradient descent" = observation → []
All three → gap-ai-ml-training-001 (one event each).

---

## KE-043-003 — ARCHITECTURE requires structural noun with design/architect verb

Rule: ARCHITECTURE requires both (a) a design/architect verb AND (b) a structural noun.
Structural nouns: architecture, platform, framework, system design, infrastructure, structure, schema, blueprint.
"Design the training loop orchestration architecture" → ARCHITECTURE. ("architecture" = structural noun present)
"Design the training loop" (no structural noun) → does NOT fire ARCHITECTURE. → [] → gap.
"Design a service that manages training" → does NOT fire ARCHITECTURE (service ≠ structural noun here; need architecture-level context).

---

## KE-043-004 — IMPLEMENT+TEST: ordering when both appear

Rule: When both IMPLEMENT and TEST are in a request:
- IMPLEMENT fires before TEST per canonical ordering rule 3 (IMPLEMENT before TEST).
- Flow order: ImplementationFlow41 → TestFlow51.
- "Build the neural network module and write unit tests for each layer" → IMPLEMENT then TEST.

---

## KE-043-005 — Adjectives derived from gap vocabulary do not suppress code verbs

Rule: "self-healing", "few-shot-capable", "AI-powered", "neural-network-backed", "training-aware" as adjectives do not suppress code verbs.
"Build an AI-powered neural network service" → IMPLEMENT ("build" fires; "AI-powered" is adjective modifier).
"Implement a training-aware scheduler" → IMPLEMENT ("implement" fires; "training-aware" is adjective).
