# Layer 1 — Transition Orchestrator Pairs (008)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-008-001 — Predicate deduplication in orchestration

Request: "Build the agent, implement the skill router, and implement the memory module."
Predicates: ["IMPLEMENT"] (three IMPLEMENT triggers → one IMPLEMENT predicate; set not multiset)
Orchestration: ImplementationFlow41 (single flow — deduplication prevents duplicate flow execution)
TransitionType: feature_addition

---

## TO-008-002 — DOCUMENT+TEST (no IMPLEMENT) orchestration

Request: "Write tests for the agent's error handler and document the error codes."
Predicates: ["TEST", "DOCUMENT"]
Orchestration: TestingFlow51 ‖ DocumentationFlow (parallel; neither depends on the other)
TransitionType: documentation (no IMPLEMENT; TEST+DOCUMENT → documentation)
Note: "write tests" → TEST; "document" → DOCUMENT; no code-build verb → IMPLEMENT absent.

---

## TO-008-003 — Single-gap item in five-verb request

Request: "Build the intent classifier, integrate with the analytics API, secure input, document the schema, and adapt the tone."
Analysis: build→IMPLEMENT; integrate→API_CONTRACT; secure→SECURITY; document→DOCUMENT; adapt→gap
Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "DOCUMENT"]
Orchestration: Four-flow pipeline; GapHandler NOT invoked (four predicates fired)
TransitionType: feature_addition
Note: Gap item ("adapt the tone") is silently skipped — at least one predicate fired.

---

## TO-008-004 — ARCHITECTURE+IMPLEMENT+TEST+QA full cycle

Request: "Design the agent reasoning architecture, build it, write tests, and QA."
Predicates: ["ARCHITECTURE", "IMPLEMENT", "TEST", "QA"]
Orchestration: ArchitectureFlow31 → ImplementationFlow41 → TestingFlow51 → QAGatingFlow73
TransitionType: feature_addition (IMPLEMENT present)
Flow count: 4 sequential stages; QA validates all three preceding flows.

---

## TO-008-005 — "orchestrate" verb disambiguation

Request: "Orchestrate the agent workflows using the pipeline controller."
Analysis: "orchestrate" is AI vocabulary → gap (does NOT fire ARCHITECTURE or IMPLEMENT)
Predicates: [] (gap)
Orchestration: GapHandler → SCHEDULE_CAPABILITY_LEARNING → gap-ai-agent-adapt-001
DomainId: software-development
Critical: "orchestrate" ≠ ARCHITECTURE trigger. ARCHITECTURE requires design verbs + structural nouns.
