# Held-Out and Corruption Pairs (022)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-022-A — Held-out: code predicates + AI context — full independence

A: "Design the multi-agent architecture, build the orchestration service, and integrate with the registry API."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT"]
Orchestration: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41
Teaching: Three code verbs in AI context fire three predicates. AI vocabulary ("multi-agent", "orchestration") is object context only.

---

## HC-022-B — Held-out: IMPLEMENT+TEST+DOCUMENT in AI context

A: "Implement the agent skill router, write integration tests for routing, and document the routing algorithm."
B: Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"]
Orchestration: ImplementationFlow41 → E2ETestingFlow53 ‖ DocumentationFlow62
Teaching: TEST and DOCUMENT may run in parallel after IMPLEMENT. QA would be last if present.

---

## HC-022-C — Corruption v6-03: seven predicates collapsed to IMPLEMENT in AI context

v6 corrupt: "Architect the agent platform, integrate API, implement scheduler, secure injection, write tests, document, QA" → Predicates: ["IMPLEMENT"]
Correct: Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
Error: v6-03 (extreme collapse). Seven verbs, seven predicates. No collapsing permitted.

---

## HC-022-D — Held-out: SECURITY is the only prompt-related predicate

A: "Improve prompt engineering and prevent prompt injection in the agent inputs."
B: Predicates: ["SECURITY"] | GapHandler suppressed; gap-ai-agent-llm-001 silently skipped.
Reason: "prompt injection" → SECURITY fires. "prompt engineering" = gap (skipped). "improve" = AI quality vocabulary → gap (skipped). Result: ["SECURITY"] only.
Phrasing-B: SECURITY — "prompt injection" is the only prompt-related predicate trigger.
