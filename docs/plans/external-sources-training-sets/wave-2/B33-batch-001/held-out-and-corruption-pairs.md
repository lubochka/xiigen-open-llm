# Held-Out and Corruption Pairs
# Batch: B33-batch-001
# Scenario: B33 — AI agent adaptation gap
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05

---

## HO-001 — "implement" + AI agent framing: IMPLEMENT fires; AI vocabulary is gap

A: "Implement the agent skill router so it can dispatch tasks autonomously."
Expected: IMPLEMENT fires on "implement" and "router" (as a service/module noun).
"agent", "skill", "autonomously" → gap. CompiledPredicateIds: ["IMPLEMENT"]. Routing: 4.1.

---

## HO-002 — pure LLM tuning vocabulary: knowledge_edge

A: "Fine-tune the LLM to improve the agent's chain-of-thought responses."
Expected: zero predicates → knowledge_edge. "fine-tune" is not in any predicate lexicon.
"LLM", "agent", "chain-of-thought" are all gap. Gap ID: gap-ai-agent-llm-001.
DomainId: "software-development".

---

## HO-003 — "document" + AI agent: DOCUMENT fires; AI framing is gap

A: "Document the AI agent skill API contract and the tool-use interface."
Expected: DOCUMENT fires on "document". API_CONTRACT fires on "contract" and "interface".
"AI agent", "skill", "tool-use" → gap. CompiledPredicateIds: ["DOCUMENT", "API_CONTRACT"].
Two independent routing decisions: DOCUMENT → 6.x (documentation phase); API_CONTRACT → 3.3.

---

## COR-001 — v6-01: IMPLEMENT invented for "adapt the agent to use new skills"

CORRUPT: "Adapt the AI agent to use new skills." → CompiledPredicateIds: ["IMPLEMENT"].
Why corrupt: "adapt" is not an IMPLEMENT term. "use" alone is not an IMPLEMENT term.
"skills" is gap vocabulary. The corrupt model invented IMPLEMENT because the request
sounds like software development work. Correct: CompiledPredicateIds: [] → knowledge_edge.

---

## COR-002 — v6-01: ARCHITECTURE invented for "orchestrate agents"

CORRUPT: "Orchestrate the AI agents to handle the workflow." → CompiledPredicateIds: ["ARCHITECTURE"].
Why corrupt: "orchestrate" is not in the ARCHITECTURE predicate lexicon. ARCHITECTURE fires on:
design, architect, structure, layer, component, diagram, blueprint, plan (architectural),
pattern, scaffold (architectural), framework. "Orchestrate" as a verb is gap vocabulary.
The sentence contains no architecture/design action verb. Correct: CompiledPredicateIds: [] → knowledge_edge.

---

## COR-003 — v6-03: DomainId "ai-development" for AI agent gap

CORRUPT: Pure AI agent zero-predicate request → DomainId: "ai-development".
Why corrupt: There is no registered "ai-development" domain in the gap taxonomy.
The two valid gap DomainIds are "software-development" and "project-management".
AI agent adaptation vocabulary, when zero predicates fire, emits DomainId: "software-development"
because the activity is software development — the gap is in AI-specific vocabulary, not
in domain classification.
