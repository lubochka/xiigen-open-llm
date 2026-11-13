# Layer 1 — Transition Orchestrator Pairs (011)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-011-001 — Six-predicate full orchestration in AI context

Request: "Architect the agent platform, build the skill engine, integrate with the CRM, secure the agent gateway, write tests, and QA sign-off."
Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "QA"]
Orchestration: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → TestingFlow51 → QAGatingFlow73
TransitionType: feature_addition (IMPLEMENT present)
QA last: absolute ordering constraint regardless of predicate count.

---

## TO-011-002 — Gap registry ID assignment rules

On gap, GapHandler assigns gap registry ID based on gap category:
- AI adaptation / persona / domain adaptation → gap-ai-agent-adapt-001
- AI skill / capability / domain skill → gap-ai-agent-skill-001
- AI model / LLM / reasoning quality / fine-tuning → gap-ai-agent-llm-001
Multiple AI gap verbs in one request → single gap ID (closest match); single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## TO-011-003 — IMPLEMENT TransitionType precedence over refactoring

Request: "Refactor and extend the agent skill router with new capabilities."
Predicates: ["IMPLEMENT"] (both refactor and extend fire IMPLEMENT)
TransitionType: refactoring ("refactor" verb takes highest priority even when IMPLEMENT fires for other verbs)
Orchestration: ImplementationFlow41
Note: TransitionType = refactoring when "refactor" is one of the action verbs, regardless of other verbs present.

---

## TO-011-004 — DOCUMENT alone in AI architecture context

Request: "Document the agent's architecture decision records."
Predicates: ["DOCUMENT"]
Orchestration: DocumentationFlow
TransitionType: documentation
Note: "architecture decision records" is documentation noun — does not fire ARCHITECTURE predicate. ARCHITECTURE requires design/architect verbs.

---

## TO-011-005 — TEST routing 5.1 vs 5.3 in AI context

Request: "Write unit tests for the agent's token counter utility."
Predicates: ["TEST"]
Orchestration: UnitTestingFlow51 (sub-phase 5.1 — "unit tests" + pure utility function)
TransitionType: documentation (no IMPLEMENT)
Note: "token counter utility" is AI context but "write unit tests" + utility function → 5.1, not 5.3.
