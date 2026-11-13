# Layer 0-D — Flow Factory Pairs (012)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## FF-012-001 — Full five-flow: IMPLEMENT+TEST+DOCUMENT+SECURITY+QA

A: ["IMPLEMENT", "TEST", "DOCUMENT", "SECURITY", "QA"] | "Build agent API, write tests, document, prevent injection, QA sign-off"
Factory: [ImplementationFlow41, E2ETestingFlow53, DocumentationFlow62, SecurityAuditFlow, QAGatingFlow73]
TransitionType: feature_addition | QA last

---

## FF-012-002 — GapHandler: "AI model selection"

A: [] | "Decide which LLM to use for the production agent"
Factory: GapHandler | Gap ID: gap-ai-agent-llm-001
Reason: Model selection/evaluation is analytical gap. No code verb.

---

## FF-012-003 — IMPLEMENT: "build model proxy"

A: ["IMPLEMENT"] | "Build an LLM provider proxy service with fallback to backup models"
Factory: [ImplementationFlow41] | feature_addition
Teaching: "build" fires IMPLEMENT. LLM provider proxy is a software service (code artifact).

---

## FF-012-004 — GapHandler: "agent self-improvement"

A: [] | "Enable the agent to self-improve from user feedback"
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001

---

## FF-012-005 — GapHandler vs IMPLEMENT: "add" disambiguation

"Add few-shot examples to the prompt" → GapHandler (adding to prompt = AI configuration, not code)
"Add a retry mechanism to the agent service" → IMPLEMENT (adding software feature)
"Add prompt injection validation to the endpoint" → IMPLEMENT+SECURITY (adding security code feature)
Teaching: "add" fires IMPLEMENT only in code-artifact context. "add to prompt" is AI configuration → gap.
