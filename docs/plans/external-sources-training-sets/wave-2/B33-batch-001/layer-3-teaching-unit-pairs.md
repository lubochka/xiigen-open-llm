# Layer 3 — Teaching Unit Pairs
# Batch: B33-batch-001
# Scenario: B33 — AI agent adaptation — building and adapting AI agents and skills
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B33B1-WDA-teach-001 — "adapt" does not fire IMPLEMENT

A: "Adapt the AI agent to handle new skills." Does IMPLEMENT fire?
B: No. "adapt" is not in the IMPLEMENT lexicon. IMPLEMENT fires on: build, create, write,
implement, code, add, develop, generate, scaffold, bootstrap, extend. "Adapt" describes
adjustment or modification in an AI context but carries no predicate. Result: zero predicates
→ knowledge_edge. DomainId: "software-development".

---

## Pair B33B1-WDA-teach-002 — "integrate" fires API_CONTRACT in any context including AI

A: "Integrate the AI agent with the external ticketing system." Which predicate fires?
B: API_CONTRACT fires on "integrate". This holds in any context: payment systems,
AI agents, CRM integrations, database connectors — "integrate" always maps to API_CONTRACT.
It never maps to IMPLEMENT. The AI agent framing produces a CapabilityGapWarning but
does not change the predicate outcome. Routing: sub-phase 3.3.

---

## Pair B33B1-WDA-teach-003 — development action verbs fire even when the object is AI vocabulary

A: "Build an LLM-powered agent orchestration module." What predicates fire?
B: IMPLEMENT fires on "build" and "module". "LLM", "agent", "orchestration" are all gap
vocabulary — they produce CapabilityGapWarnings but do not suppress IMPLEMENT. The
predicate fires on the action verb and structural noun, not on the domain descriptor.
CompiledPredicateIds: ["IMPLEMENT"]. Routing: 4.1.

---

## Pair B33B1-WDA-teach-004 — RAG and chain-of-thought are purely gap vocabulary

A: "Improve the RAG pipeline with chain-of-thought reasoning." What predicates fire?
B: Zero predicates. "improve" is not in any predicate lexicon. "RAG" is gap.
"chain-of-thought" is gap. "reasoning" is gap. "pipeline" alone does not fire ARCHITECTURE
without an architecture/design/structure action verb. Result: knowledge_edge.
Gap ID: gap-ai-agent-llm-001. DomainId: "software-development".

---

## Pair B33B1-WDA-teach-005 — action verb + AI vocabulary: predicate fires, AI framing is gap

A: "Design the architecture for the LLM agent." What fires?
B: ARCHITECTURE fires on "design" + "architecture". "LLM" is gap. "agent" is gap.
The AI vocabulary produces gap warnings but does not suppress the ARCHITECTURE predicate.
CompiledPredicateIds: ["ARCHITECTURE"]. Routing: 3.1. This is identical to any other
"design the architecture for X" request — the predicate depends on the action verbs and
structural nouns, not on what X is.
