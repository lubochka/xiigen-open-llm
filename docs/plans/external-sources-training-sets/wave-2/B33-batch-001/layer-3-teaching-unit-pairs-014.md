# Layer 3 — Teaching Unit Pairs (014)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-014 — "few-shot learning" vocabulary teaching unit

**Teaching goal**: Explain the boundary between "few-shot" as AI vocabulary and code artifacts built around few-shot patterns.

**Rule 1**: "use few-shot learning for the agent" → gap. "use" in AI context does not fire a code predicate. "few-shot learning" = AI training strategy.

**Rule 2**: "add few-shot examples to the system prompt" → gap. "add to prompt" = AI prompt configuration. Prompts are not code artifacts; they are AI configuration objects.

**Rule 3**: "build a few-shot example loader" → IMPLEMENT. "build" = code verb. "example loader" = software service. The few-shot strategy is the domain; the loader is the artifact.

**Rule 4**: "add a few-shot example database table" → IMPLEMENT. "add" fires IMPLEMENT when the object is a code artifact (database table). "add to prompt" is gap; "add to database" is IMPLEMENT.

**Rule 5**: "evaluate few-shot vs zero-shot performance" → gap. "evaluate" = AI quality assessment vocabulary → gap. "implement a few-shot vs zero-shot evaluation harness" → IMPLEMENT (code verb + artifact).

**Summary**: "few-shot" is gap vocabulary. The object type (prompt = gap; software artifact = IMPLEMENT) determines the boundary when "add" appears.
