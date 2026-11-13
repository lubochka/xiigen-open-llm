# Layer 0-C — Knowledge Edge Pairs (055)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-055-001 — AI agent + neural network combined rules

| Input | Result |
|-------|--------|
| "AI agents use neural networks for decision-making" | [] → gap-ai-ml-model-001 |
| "Build an AI agent that uses a neural network backbone" | IMPLEMENT |

Neural network (round-2) + AI agent (round-3) both gap; combined → gap-ai-ml-model-001 (round-3 dominant).
With code verb: IMPLEMENT (agent runtime = software artifact).

---

## KE-055-002 — LLM + training vocabulary bridge rules

| Input | Result |
|-------|--------|
| "LLM pretrained with gradient descent, fine-tuned with RLHF" | [] → gap-ai-ml-model-001 |
| "Implement the LLM client and secure against prompt injection" | IMPLEMENT + SECURITY |

RLHF (reinforcement learning from human feedback) is a training method → gap-ai-ml-training-001.
LLM present → gap-ai-ml-model-001 (dominant).
With code verbs: each fires independently.

---

## KE-055-003 — IMPLEMENT+SECURITY ordering rule

When IMPLEMENT and SECURITY both fire:
Canonical ordering rule 4: IMPLEMENT before SECURITY (security audits the built artifact).
"Implement the LLM client library and secure it against prompt injection" → IMPLEMENT then SECURITY.
Flow: ImplementationFlow41 → SecurityAuditFlow.

---

## KE-055-004 — DOCUMENT+QA: two-predicate pattern

When DOCUMENT and QA both fire:
"Document the LLM integration design and QA the vector database connector" → [DOCUMENT, QA].
Ordering: DOCUMENT fires, then QA (QA always last).
QA validates the DOCUMENT output as well as all other preceding flows.

---

## KE-055-005 — Adjective "AI-powered" does not alter gap or code verb outcome

Rule (final restatement): "AI-powered" as adjective = neutral modifier.
"Need an AI-powered service" (no code verb) → gap (requirement statement).
"Build an AI-powered service" (code verb) → IMPLEMENT.
"Integrate the AI-powered API" → API_CONTRACT.
"AI-powered" never changes the predicate outcome. The code verb (or lack thereof) determines the outcome.
