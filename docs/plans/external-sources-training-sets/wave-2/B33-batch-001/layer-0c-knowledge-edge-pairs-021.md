# Layer 0-C — Knowledge Edge Pairs (021)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-021-001 — Edge: "adapt" (AI verb) vs "build" (code verb) — same object

"adapt the agent for medical queries" → gap ("adapt" = AI vocabulary)
"build the agent for medical queries" → IMPLEMENT ("build" = code verb)
"create an agent for medical queries" → IMPLEMENT ("create" = code verb)
Teaching: Same object ("agent for medical queries") produces different routing solely based on the verb. "adapt" → gap; "build"/"create" → IMPLEMENT.

---

## KE-021-002 — Edge: subordinate clause does not inherit predicate from main clause

"adapt the agent rather than build a new one" → gap (primary verb "adapt" → gap; "build" is in negative subordinate clause → does not fire)
"build a new agent rather than adapt the old one" → IMPLEMENT (primary verb "build" → IMPLEMENT; "adapt" is in negative subordinate clause → silently skipped)
Teaching: Predicate assignment is based on the primary action verb, not subordinate/rejected alternatives.

---

## KE-021-003 — Edge: "improve the agent" is gap vocabulary

"improve the agent's reasoning" → gap ("improve" in AI context = AI quality improvement vocabulary → gap)
"improve the agent service response time" → IMPLEMENT (code optimization context; "improve" applied to software performance metric → IMPLEMENT)
Teaching: "improve" is gap in AI behavior context but fires IMPLEMENT in software performance/code optimization context. Object type determines classification.

---

## KE-021-004 — Edge: "update the agent" disambiguation

"update the agent's personality" → gap (AI personality/behavior update = AI vocabulary → gap)
"update the agent service to v2" → IMPLEMENT (code versioning; "update" applied to software service → IMPLEMENT)
"update the agent configuration" → gap (AI configuration change → gap)
Teaching: "update" fires IMPLEMENT when applied to software versioning/code. "update" applied to AI configuration/behavior → gap.

---

## KE-021-005 — Edge: "teach the agent" and "give skills" both gap

"teach the agent to recognize sarcasm" → gap ("teach" = AI capability vocabulary → gap-ai-agent-skill-001)
"give the agent new coding skills" → gap ("give skills" = capability-grant vocabulary → gap-ai-agent-skill-001)
"create a sarcasm detection skill plugin for the agent" → IMPLEMENT ("create" = code verb; "plugin" = software artifact → IMPLEMENT)
