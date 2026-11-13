# DPO Knowledge Map v1

Status: governance/source map.

## Purpose

This map prevents trainable/adaptive plans from mentioning DPO without loading
the correct guides, pair schema, rejected taxonomy, and validation gates.

## Required DPO Sources

| Domain | Required local skills | Required guides |
| --- | --- | --- |
| Generic DPO | `dpo-triple-format-SKILL.md`, `planning-dpo-authoring-SKILL.md` | `DPO-DEEP-DIVE-v1.md`, `DPO-DEEP-DIVE-v2.md` when available |
| Translation | `dpo-triple-format-SKILL.md`, `planning-dpo-authoring-SKILL.md` | `DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md`, `TRANSLATION-DPO-REGISTRY-GUIDE-v1.md`, `TRANSLATION-DPO-REJECTED-PROMPTS-v1.md`, `TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md` |
| OrchestrationReasoning | `dpo-triple-format-SKILL.md`, `planning-dpo-authoring-SKILL.md` | `ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md` |
| PromptToTask | `dpo-triple-format-SKILL.md`, `planning-dpo-authoring-SKILL.md` | `PROMPT-TO-TASK-DPO-NEGATIVE-STRATEGY-v1.md` |
| ArchitectBehavior | `conversation-intent-and-self-reflection-SKILL.md`, `architect-behavior-classifier-SKILL.md` | `ARCHITECT-BEHAVIOR-DPO-GUIDE-v1.md`, `ARCHITECT-AUTHORITY-REGRESSION-CASES.jsonl` |

## Gate

Every trainable/adaptive work order must list:

- loaded DPO skills;
- domain guide paths;
- chosen/rejected schema;
- rejected taxonomy;
- registry fields;
- train-ready validation;
- held-out/fresh-load/continue-training evidence.

If a plan says "DPO later" while claiming trainable readiness, the plan fails.
