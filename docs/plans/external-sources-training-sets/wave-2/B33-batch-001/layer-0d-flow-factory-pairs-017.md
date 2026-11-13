# Layer 0-D — Flow Factory Pairs (017)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-017-001 — GapHandler: "self-healing" agent behavior

A: [] | "The agent should be self-healing and correct reasoning errors automatically."
Factory: GapHandler | Gap ID: gap-ai-agent-adapt-001
Reason: "self-healing" = AI agent behavior vocabulary. No code verb.

---

## FF-017-002 — IMPLEMENT: "implement self-healing retry logic"

A: ["IMPLEMENT"] | "Implement self-healing retry logic that re-prompts the agent on validation failure."
Factory: [ImplementationFlow41] | TransitionType: feature_addition

---

## FF-017-003 — ARCHITECTURE+IMPLEMENT: design + build self-healing system

A: ["ARCHITECTURE", "IMPLEMENT"] | "Design the self-healing architecture and implement the fallback routing service."
Factory: [ArchitectureFlow31, ImplementationFlow41] | TransitionType: feature_addition
Ordering: ArchitectureFlow31 before ImplementationFlow41.

---

## FF-017-004 — SECURITY: "self-healing" + prompt injection

A: ["SECURITY"] | "Harden the self-healing agent's input layer against prompt injection."
Factory: [SecurityAuditFlow]
Teaching: "prompt injection" fires SECURITY; "self-healing" is AI adjective in object position. Gap silently skipped.

---

## FF-017-005 — Factory selection summary: self-healing vocabulary

| Request form | Factory selected | Reason |
|-------------|-----------------|--------|
| self-healing agent | GapHandler | AI behavior vocabulary |
| implement self-healing retry logic | ImplementationFlow41 | Code verb |
| design self-healing architecture | ArchitectureFlow31 | Design verb + structural noun |
| harden self-healing agent against injection | SecurityAuditFlow | Injection = security |
| how do self-healing agents decide to retry? | GapHandler | AI strategy question |
