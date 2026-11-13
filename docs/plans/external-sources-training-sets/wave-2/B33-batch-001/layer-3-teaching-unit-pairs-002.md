# Layer 3 — Teaching Unit Pairs (002)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-002 — The code-verb override rule

**Teaching goal**: Code-action verbs override AI vocabulary objects. The verb determines the predicate, not the object.

**Rule**: If the action verb is a code-action verb (build, implement, create, write [code], add [code artifact], refactor), the predicate fires regardless of how "AI" the object is.

**Examples**:

| Request | Verb | Object | Predicate |
|---------|------|--------|-----------|
| "Build a RAG pipeline" | build | RAG pipeline | IMPLEMENT |
| "Implement the LLM response cache" | implement | LLM response cache | IMPLEMENT |
| "Create the agent skill registry" | create | agent skill registry | IMPLEMENT |
| "Adapt the agent for finance" | adapt | finance domain | [] (gap) |
| "Fine-tune the model" | fine-tune | model | [] (gap) |
| "Orchestrate the agent workflows" | orchestrate | agent workflows | [] (gap) |

**Mnemonic**: Ask "Is the verb a software engineering action?" If yes → code-verb override → predicate fires. If no → check AI vocabulary list → gap.
