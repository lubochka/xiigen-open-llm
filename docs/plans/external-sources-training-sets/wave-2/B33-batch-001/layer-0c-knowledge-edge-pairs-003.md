# Layer 0-C — Knowledge Edge Pairs (003)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-003-001 — "orchestrate" edge disambiguation

"orchestrate agents" / "orchestrate the pipeline" (AI orchestration) → gap
"architect the orchestration layer" → ARCHITECTURE (architecture verb)
"implement the orchestration service" → IMPLEMENT (code verb)
"design the orchestration topology" → ARCHITECTURE (design verb)

Rule: "orchestrate" as verb → gap. Code/architecture verbs applied to orchestration concepts → predicates fire.

---

## KE-003-002 — "prompt" edge (always gap in prompt engineering sense)

"improve the prompt" → gap
"write a better prompt" → gap (writing prompt ≠ writing code)
"rewrite the system prompt" → gap
"prompt engineering" → gap
"few-shot prompt" → gap
"engineer a prompt" → gap

Exception: "write the API prompt template code" → IMPLEMENT (code context)

---

## KE-003-003 — "RAG" and "retrieval" edge

"RAG" keyword alone → gap (AI vocabulary)
"improve RAG accuracy" → gap
"build a RAG pipeline" → IMPLEMENT (build is code verb)
"design the RAG architecture" → ARCHITECTURE (design verb)
"implement retrieval-augmented generation" → IMPLEMENT (implement verb)

Rule: RAG/retrieval in object → determines nothing; verb determines predicate.

---

## KE-003-004 — "integrate" always fires API_CONTRACT

"integrate the AI agent with X" → API_CONTRACT (always, regardless of AI context)
"integrate LLM into the service" → API_CONTRACT
"integrate the tool-calling API" → API_CONTRACT

Rule: "integrate" fires API_CONTRACT in ALL contexts, including AI/LLM/agent contexts. This is a hard rule from B33 key teaching point 3.

---

## KE-003-005 — "SCHEDULE_CAPABILITY_LEARNING" emitted for all B33 gaps

All gap requests in B33 → SCHEDULE_CAPABILITY_LEARNING event emitted to subproject 16 (AutonomousLearningPipeline).
DomainId: "software-development" (no "ai-development" domain exists).
Gap IDs: gap-ai-agent-adapt-001, gap-ai-agent-skill-001, gap-ai-agent-llm-001
