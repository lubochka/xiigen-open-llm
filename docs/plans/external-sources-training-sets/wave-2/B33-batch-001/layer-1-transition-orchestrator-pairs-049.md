# Layer 1 — Transition Orchestrator Pairs (049)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-049-001 — Orchestration: gap suppression rule for B33 extended vocabulary

Rule: When any code predicate fires in a B33 mixed gap+code request, all gap vocabulary from all rounds is silently skipped.
Example: "embed the documents and integrate with the vector store API" → ["API_CONTRACT"]
"embed" (gap-ai-ml-model-001) silently skipped. "integrate" fires API_CONTRACT.

---

## TO-049-002 — Orchestration: implement multi-agent + write tests + QA

Predicates: ["IMPLEMENT", "TEST", "QA"]
Request: "Implement the multi-agent coordinator, write unit tests for routing, QA the system."
Flow order: ImplementationFlow41 → TestFlow51 → QAGatingFlow73
TransitionType: feature_addition | QA last.

---

## TO-049-003 — Orchestration: ARCHITECTURE + IMPLEMENT for AI platform

Predicates: ["ARCHITECTURE", "IMPLEMENT"]
Request: "Architect the AI agent system and implement the tool dispatcher."
Flow order: ArchitectureFlow31 → ImplementationFlow41
TransitionType: feature_addition

---

## TO-049-004 — Orchestration: documentation-only TransitionType

Predicates: ["DOCUMENT"]
Request: "Document the multi-agent system design."
Flow: DocumentationFlow62 | TransitionType: documentation
Teaching: Documentation-only request → TransitionType: documentation.

---

## TO-049-005 — Orchestration: complete B33 gap ID priority rules

Selection order when gap vocabulary from multiple rounds:
1. gap-ai-ml-model-001 when round-1 or round-3 model terms are present
2. gap-ai-ml-training-001 only when exclusively round-2 training terms present
3. Earlier B33 rounds (gap-ai-agent-*) follow their own selection rules
One SCHEDULE_CAPABILITY_LEARNING per request regardless of how many gap IDs are detected.
