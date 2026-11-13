# Layer 0-C — Knowledge Edge Pairs (039)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-039-001 — Hyperparameter gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| hyperparameter (standalone) | No code verb | gap-ai-ml-training-001 |
| hyperparameter tuning service (with "build") | Code verb present | IMPLEMENT fires |
| hyperparameter endpoint (with "secure") | "secure" present | SECURITY fires |
| hyperparameter tuner (with "integrate") | "integrate" present | API_CONTRACT fires |

---

## KE-039-002 — Overfitting gap trigger rules

| Term | Condition | Result |
|------|-----------|--------|
| overfitting (standalone) | No code verb | gap-ai-ml-training-001 |
| overfitting detector (with "build") | Code verb present | IMPLEMENT fires |
| overfitting + hyperparameter (both standalone) | No code verb | gap-ai-ml-training-001 (one event) |

---

## KE-039-003 — Hyperparameter vs. overfitting: both gap-ai-ml-training-001

Rule: Both "hyperparameter" and "overfitting" belong to gap-ai-ml-training-001.
When both appear without code verb → one SCHEDULE_CAPABILITY_LEARNING, one gap ID.
Example: "hyperparameter selection controls overfitting" → [] → gap-ai-ml-training-001 (one event).

---

## KE-039-004 — SECURITY fires on "secure" + any endpoint regardless of AI context

Rule: "secure the [any] endpoint" → SECURITY.
"hyperparameter configuration endpoint" is an endpoint → SECURITY fires.
AI vocabulary in the endpoint name does not change this rule.
Contrast: "secure the training loop" (no endpoint noun) → depends on context, but "secure" + tangible artifact → SECURITY.

---

## KE-039-005 — Gap suppression is silent: no warning emitted for training terms

Rule: When a code verb fires and training vocabulary is in the object position, the gap is silently skipped.
No GapWarning is emitted for gap-ai-ml-training-001 when suppressed (unlike gap-ai-agent-* which may emit GapWarning).
Example: "integrate hyperparameter tuner with API" → API_CONTRACT only. No gap event.
