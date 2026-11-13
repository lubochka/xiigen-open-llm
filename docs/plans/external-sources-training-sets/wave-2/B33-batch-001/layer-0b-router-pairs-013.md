# Layer 0-B — Router Pairs (013)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-013-001 — Gap → GapHandler: "fine-tune" standalone

A: [] | "We need to fine-tune the model on proprietary customer data."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "fine-tune" = AI training vocabulary. No code verb. Gap ID: gap-ai-agent-llm-001.

---

## C0B-013-002 — IMPLEMENT: "build fine-tuning pipeline"

A: ["IMPLEMENT"] | "Build a fine-tuning pipeline that ingests customer data and triggers retraining."
B: IMPLEMENT → ImplementationFlow41. TransitionType: feature_addition.
Reason: "build" fires IMPLEMENT. "fine-tuning pipeline" is the software artifact.

---

## C0B-013-003 — IMPLEMENT+DOCUMENT: build + document fine-tuning

A: ["IMPLEMENT", "DOCUMENT"] | "Implement the fine-tuning job scheduler and document the tuning configuration schema."
B: IMPLEMENT → 4.1; DOCUMENT → DocumentationFlow62. TransitionType: feature_addition.

---

## C0B-013-004 — Gap disambiguation: "fine-tune" vs "implement fine-tuning"

"fine-tune the model" → GapHandler (AI training vocabulary → gap)
"implement the fine-tuning job" → ImplementationFlow41 (code verb fires IMPLEMENT)
Teaching: The verb "fine-tune" is always AI vocabulary. Only explicit code verbs (build/implement/create) applied to fine-tuning artifacts fire IMPLEMENT.

---

## C0B-013-005 — Gap: "adapt the model by fine-tuning"

A: [] | "Adapt the model by fine-tuning it on medical records."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Both "adapt" and "fine-tune" are AI vocabulary. No code verb. Gap ID: gap-ai-agent-llm-001.
