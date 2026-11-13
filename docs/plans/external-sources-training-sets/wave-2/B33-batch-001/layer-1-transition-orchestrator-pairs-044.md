# Layer 1 — Transition Orchestrator Pairs (044)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-044-001 — Orchestration: adjective does not fire predicate

Gap request: "We need an AI-powered multi-agent coordination system."
Predicates: [] | GapHandler → gap-ai-ml-model-001
Teaching: "AI-powered" = adjective; "need" = requirement word; no code verb. → gap.

---

## TO-044-002 — Orchestration: "AI-powered" + build verb

Predicates: ["IMPLEMENT"]
Request: "Build an AI-powered multi-agent coordination system."
Flow: ImplementationFlow41 | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "AI-powered" = adjective; does not suppress.

---

## TO-044-003 — v6 corruption: all predicates for AI request collapsed to single (v6-03)

v6 corrupt: "architect the AI platform + integrate LLM API + implement client + secure + test + document + QA" → ["IMPLEMENT"] only
Correct: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
Error: v6-03 extreme collapse. Seven code verbs fire seven predicates independently.

---

## TO-044-004 — Orchestration: primary verb rule

Rule: In compound sentences, the primary action verb determines routing.
"Build the LLM client rather than integrating directly" → ["IMPLEMENT"] only.
"rather than integrating" = subordinate/negative clause; "integrate" in subordinate position does not fire API_CONTRACT.

---

## TO-044-005 — Orchestration: negative clause does not fire predicates

Rule: "instead of building", "rather than implementing", "without integrating" = negative/subordinate clauses.
These do not fire predicates.
"Instead of building a wrapper, we need to configure the LLM API" → no code verb in primary clause. → gap.
