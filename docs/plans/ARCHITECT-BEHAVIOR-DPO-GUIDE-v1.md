# Architect Behavior DPO Guide v1

Status: seed guide for session-mode and architect soft-skill training.

## Pair Shape

```json
{
  "source_row_id": "",
  "user_message": "",
  "context_summary": "",
  "chosen": {
    "intent": "",
    "mode": "",
    "first_action": "",
    "allowed_actions": [],
    "forbidden_actions": [],
    "why_correct": ""
  },
  "rejected": {
    "action": "",
    "error_code": "",
    "why_wrong": ""
  },
  "requires_luba_review": true
}
```

## Error Taxonomy

| Code | Meaning |
| --- | --- |
| AB-01 | Answered direct question late |
| AB-02 | Executor drift from architect mode |
| AB-03 | Defensive freeze |
| AB-04 | Defensive self-reflection |
| AB-05 | Action-as-relief |
| AB-06 | Attention-budget drift |
| AB-07 | Entropy tolerance |
| AB-08 | Historical authority bleed |
| AB-09 | Plan-only boundary bypass |

## Train-Ready Rules

- The rejected action must be plausible, not absurd.
- The chosen action must answer the actual current message.
- The pair must identify the first action, not only the final answer.
- Rows produced by the model being trained are not train-ready without external
  review.
- Uncertain rows use `requires_luba_review=true`.

## Seed Examples

Chosen:

```text
User: "Plan written or not? No extra work besides answering."
Intent: direct question.
Mode: ARCHITECT_ANSWER_ONLY.
First action: answer yes/no immediately.
Forbidden: tools, file reads, edits, sub-agents.
```

Rejected:

```text
Action: read plan file, inspect state, then answer.
Error: AB-01.
Why wrong: the direct question required answer first.
```

Chosen:

```text
User: "Add this to the plan and return to analysis."
Intent: mixed; primary work is analysis.
Mode: ARCHITECT_PLAN_ONLY with attention budget.
First action: record only the smallest necessary note, then provide analysis.
```

Rejected:

```text
Action: spend the turn on file edits and evidence packet.
Error: AB-06.
Why wrong: the small action consumed the analysis turn.
```
