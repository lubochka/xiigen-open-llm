# Layer 0-C — Knowledge Edge Pairs (011)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-011-001 — "handoff" edge (AI agent handoff to human)

"agent handoff" (AI → human transfer) → gap without code verb
"handoff logic" (AI behavior) → gap without code verb
BUT: "implement the handoff routing logic" → IMPLEMENT
"build the escalation handoff service" → IMPLEMENT

---

## KE-011-002 — "safety" and "guardrails" edge

"content safety" / "AI safety" → gap without code verb
"guardrails" (AI output constraint) → gap without code verb
BUT: "implement content safety guardrails" → IMPLEMENT
"build the safety filter service" → IMPLEMENT
"validate AI output for safety" → SECURITY (if input validation for security context)

---

## KE-011-003 — "observation" and "monitoring" in AI context

"monitor agent behavior" → gap
"observe LLM outputs" → gap
"track agent decisions" → gap
"log agent reasoning" (without implement verb) → gap
BUT: "implement agent observability logging" → IMPLEMENT
"build agent monitoring service" → IMPLEMENT

---

## KE-011-004 — "configuration" in AI context is always gap

"configure the agent" → gap
"agent configuration" → gap
"LLM configuration" → gap
"model settings" → gap
"system configuration for agents" → gap

---

## KE-011-005 — "deployment" in AI context is gap

"deploy the AI agent" → gap (operational)
"agent deployment" → gap
"LLM deployment" → gap
"model serving" → gap
BUT: "implement the deployment service for the agent" → IMPLEMENT (building the deployment infrastructure)
"design the agent deployment architecture" → ARCHITECTURE
