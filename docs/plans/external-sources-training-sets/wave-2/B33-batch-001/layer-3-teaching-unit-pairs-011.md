# Layer 3 — Teaching Unit Pairs (011)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-011 — AI framework names are gaps on their own

**Teaching goal**: AI framework names (LangChain, AutoGPT, Semantic Kernel, etc.) are gap vocabulary — they do not fire predicates.

**Rule**: Framework names in subject or object position → gap. Code-action verb + framework name → code verb overrides.

**Examples**:

| Request | Predicate | Reason |
|---------|-----------|--------|
| "Use LangChain for the agent" | [] gap | "use" + framework name → gap |
| "Migrate to Semantic Kernel" | [] gap | "migrate to" (framework migration) → gap |
| "Evaluate AutoGPT vs CrewAI" | [] gap | evaluation vocabulary → gap |
| "Build a LangChain-style pipeline" | IMPLEMENT | "build" = code verb overrides "LangChain-style" |
| "Implement agent chains using the pipeline pattern" | IMPLEMENT | "implement" overrides; LangChain not even named |
| "Integrate LangChain with our backend" | API_CONTRACT | "integrate" always fires API_CONTRACT |

**AI framework gap list**:
LangChain, AutoGPT, CrewAI, LlamaIndex, Semantic Kernel, Haystack, DSPy, LangGraph, Autogen, OpenAgents

**Note**: Framework names as adjectives ("LangChain-style") do not add predicates — only the action verb matters.
