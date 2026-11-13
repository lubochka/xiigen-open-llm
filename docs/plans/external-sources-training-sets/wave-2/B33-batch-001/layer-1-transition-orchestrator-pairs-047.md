# Layer 1 — Transition Orchestrator Pairs (047)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-047-001 — Orchestration: IMPLEMENT fires from multiple synonymous code verbs

Rule: "build", "implement", "create", "develop" all fire IMPLEMENT. Multiple instances in one request → still one IMPLEMENT predicate.
"Implement the LLM client and create the embedding service" → ["IMPLEMENT"] (one predicate, two artifacts).

---

## TO-047-002 — Orchestration: IMPLEMENT + API_CONTRACT for LLM + integration

Predicates: ["IMPLEMENT", "API_CONTRACT"]
Request: "Implement the LLM client SDK and integrate it with the model gateway API."
Flow order: APIContractFlow33 → ImplementationFlow41 | TransitionType: feature_addition

---

## TO-047-003 — Orchestration: "write" fires IMPLEMENT for code, DOCUMENT for docs, TEST for tests

Rule disambiguation:
- "write the backpropagation routine" → IMPLEMENT (code artifact)
- "write docs for the LLM API" → DOCUMENT
- "write tests for the embedding service" → TEST
The object type determines which predicate "write" fires.

---

## TO-047-004 — Orchestration: SECURITY fires on "harden" as synonym for "secure"

Predicates: ["SECURITY"]
Request: "Harden the AI agent API against prompt injection and data leakage."
Flow: SecurityAuditFlow | TransitionType: N/A
Teaching: "harden" = synonym for "secure". Fires SECURITY.

---

## TO-047-005 — Orchestration: gap ID selection when all three rounds combine

Rule: Round 1 (model ops) + Round 2 (training) + Round 3 (model names) in one gap request → gap-ai-ml-model-001.
Model-level vocabulary (round-1 or round-3) always dominates over training vocabulary (round-2) when combined.
