# Held-Out and Corruption Pairs (019)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: held_out_corruption

---

## HC-019-A — Held-out: agent API boundary cases

A (gap): "We should expose the agent as an API." → Predicates: [] → GapHandler → gap-ai-agent-skill-001
A (IMPLEMENT): "Build the agent API endpoint." → Predicates: ["IMPLEMENT"]
A (API_CONTRACT): "Integrate the agent with Salesforce." → Predicates: ["API_CONTRACT"]
A (IMPLEMENT+API_CONTRACT): "Build the agent service and integrate with the CRM API." → Predicates: ["IMPLEMENT", "API_CONTRACT"]

---

## HC-019-B — Held-out: "orchestrate via API" fires API_CONTRACT not gap

A: "Orchestrate the agent calls via the orchestration API."
B: Predicates: ["API_CONTRACT"] | GapHandler suppressed; gap-ai-agent-adapt-001 silently skipped.
Reason: "via API" pattern → API_CONTRACT fires despite "orchestrate" AI verb.
Phrasing-B: API_CONTRACT — "via API" overrides "orchestrate" gap classification.

---

## HC-019-C — Corruption v6-03: IMPLEMENT+API_CONTRACT collapsed to API_CONTRACT

v6 corrupt: "Build the agent and integrate with the CRM API" → Predicates: ["API_CONTRACT"]
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT"]
Error: v6-03 (multi-predicate collapse). "build" → IMPLEMENT; "integrate" → API_CONTRACT. Both fire independently.

---

## HC-019-D — Held-out: "should expose agent as API" is not API_CONTRACT

A: "We should eventually expose the agent functionality as an API for partner consumption."
B: Predicates: [] | GapHandler → gap-ai-agent-skill-001
Reason: "should eventually expose" = strategic/planning language. No code action verb fires API_CONTRACT or IMPLEMENT. Gap.
Phrasing-B: Gap — "expose" and "should" are not code verbs. Only "integrate" fires API_CONTRACT.
