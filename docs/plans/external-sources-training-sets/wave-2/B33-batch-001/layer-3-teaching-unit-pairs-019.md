# Layer 3 — Teaching Unit Pairs (019)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-019 — Agent API integration teaching unit

**Teaching goal**: Clarify when agent API requests fire predicates vs remain gap, with focus on "integrate" always firing API_CONTRACT.

**Rule 1**: "integrate the agent with Salesforce" → API_CONTRACT. "integrate" always fires API_CONTRACT regardless of AI context, object, or surrounding vocabulary.

**Rule 2**: "expose the agent as an API" → gap. "expose" = strategic planning language. No code predicate fires. Gap-ai-agent-skill-001.

**Rule 3**: "build the agent API endpoint" → IMPLEMENT. "build" = code verb. "endpoint" = software artifact.

**Rule 4**: "build the agent and integrate with the CRM API" → [IMPLEMENT, API_CONTRACT]. Both verbs fire their predicates independently. No collapsing permitted.

**Rule 5**: "build the agent API and prevent prompt injection" → [IMPLEMENT, SECURITY]. "build" → IMPLEMENT; "prompt injection" → SECURITY. Both fire.

**Rule 6**: "orchestrate agent calls via the API" → API_CONTRACT. "via API" pattern triggers API_CONTRACT even when "orchestrate" (AI gap verb) is present in the request.

**Rule 7**: "we should build an agent API eventually" → gap. "should" + future framing = strategic discussion → gap. No code verb fires without explicit code action.

**Summary**: "integrate" always fires API_CONTRACT. "expose", "should build", "consider" are strategic/planning language → gap. Explicit "build"/"implement" fires IMPLEMENT.
