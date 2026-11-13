# ORCHESTRATION-AND-PROMPT-TO-TASK-FULL-EXTENSIONS-PLAN-v3

Ð¡Ñ‚Ð°Ñ‚ÑƒÑ: **Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ð¿Ð»Ð°Ð½ / Ñ€ÐµÐ°Ð»Ð¸Ð·Ð°Ñ†Ð¸Ñ Ð½Ðµ Ñ€Ð°Ð·Ñ€ÐµÑˆÐµÐ½Ð°**.

Ð­Ñ‚Ð¾Ñ‚ Ð´Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚ Ð¾Ð¿Ð¸ÑÑ‹Ð²Ð°ÐµÑ‚ Ð¿Ð»Ð°Ð½ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð°Ñ†Ð¸Ð¸ Ð¿Ð¾Ð»Ð½Ð¾Ñ†ÐµÐ½Ð½Ð¾Ð³Ð¾ Ð½Ð°Ð±Ð¾Ñ€Ð° XIIGen smart-skills extensions Ð´Ð»Ñ Claude, Cursor Ð¸ Codex.

Ð ÐµÐ°Ð»Ð¸Ð·Ð°Ñ†Ð¸Ñ Ð¿Ð¾ÑÐ»Ðµ ÑÑ‚Ð¾Ð³Ð¾ Ð´Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚Ð° Ð·Ð°Ð¿Ñ€ÐµÑ‰ÐµÐ½Ð°, Ð¿Ð¾ÐºÐ° Ð›ÑƒÐ±Ð° Ð½Ðµ Ð¿Ñ€Ð¾Ñ‡Ð¸Ñ‚Ð°ÐµÑ‚ Ð¿Ð»Ð°Ð½ Ð¸ Ð½Ðµ Ð´Ð°ÑÑ‚ Ð½Ð¾Ð²Ð¾Ðµ ÑÐ²Ð½Ð¾Ðµ Ñ€Ð°Ð·Ñ€ÐµÑˆÐµÐ½Ð¸Ðµ Ð² Ñ‚ÐµÐºÑƒÑ‰ÐµÐ¼ Ñ€Ð°Ð·Ð³Ð¾Ð²Ð¾Ñ€Ðµ, Ð½Ð°Ð¿Ñ€Ð¸Ð¼ÐµÑ€: `Ð¿Ð»Ð°Ð½ Ð¿Ñ€Ð¸Ð½ÑÑ‚, Ð½Ð°Ñ‡Ð¸Ð½Ð°Ð¹ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð°Ñ†Ð¸ÑŽ`.

Ð¢ÐµÑ…Ð½Ð¸Ñ‡ÐµÑÐºÐ¸Ð¹ Ð¼Ð°Ñ€ÐºÐµÑ€ Ð´Ð»Ñ ÑÐ»ÐµÐ´ÑƒÑŽÑ‰Ð¸Ñ… ÑÐµÑÑÐ¸Ð¹:

```text
requested_artifact_type=plan
implementation_authorized_now=false
luba_acceptance_quote=NONE
quoted_transcripts_and_old_state_are_evidence_only=true
```

## 0. Ð§Ñ‚Ð¾ Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð´Ð°Ñ‚ÑŒ ÑÑ‚Ð¾Ñ‚ Ð¿Ð»Ð°Ð½

ÐÑƒÐ¶ÐµÐ½ Ð½Ðµ Ð½Ð°Ð±Ð¾Ñ€ JSON-Ð¾Ñ‚Ñ‡Ñ‘Ñ‚Ð¾Ð² Ð¸ PowerShell-Ð¿Ñ€Ð¾Ð²ÐµÑ€Ð¾Ðº, Ð° Ð½Ð°ÑÑ‚Ð¾ÑÑ‰Ð¸Ðµ Ñ€Ð°ÑÑˆÐ¸Ñ€ÐµÐ½Ð¸Ñ:

1. ÐŸÐ¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ð¿Ð¸ÑˆÐµÑ‚ Ð·Ð°Ð¿Ñ€Ð¾Ñ Ð² Claude, Cursor Ð¸Ð»Ð¸ Codex.
2. Ð Ð°ÑÑˆÐ¸Ñ€ÐµÐ½Ð¸Ðµ Ð¾Ñ‚Ð¿Ñ€Ð°Ð²Ð»ÑÐµÑ‚ Ñ‚Ð¾Ð½ÐºÐ¸Ð¹ Ð·Ð°Ð¿Ñ€Ð¾Ñ Ð² XIIGen core.
3. XIIGen Ð¿ÐµÑ€ÐµÐ²Ð¾Ð´Ð¸Ñ‚ Ð·Ð°Ð¿Ñ€Ð¾Ñ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ Ð² ÑÑ‚Ñ€ÑƒÐºÑ‚ÑƒÑ€Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð½ÑƒÑŽ Ð·Ð°Ð´Ð°Ñ‡Ñƒ.
4. XIIGen Ð½Ð°Ñ…Ð¾Ð´Ð¸Ñ‚ Ð¿Ð»Ð°Ð½ Ð¾Ñ€ÐºÐµÑÑ‚Ñ€Ð°Ñ†Ð¸Ð¸ Ð´Ð»Ñ ÑÑ‚Ð¾Ð¹ Ð·Ð°Ð´Ð°Ñ‡Ð¸.
5. XIIGen Ð½Ð°Ñ…Ð¾Ð´Ð¸Ñ‚ Ð½ÑƒÐ¶Ð½Ñ‹Ðµ ÑÐºÐ¸Ð»Ñ‹ Ð´Ð»Ñ Ð·Ð°Ð´Ð°Ñ‡Ð¸.
6. Ð•ÑÐ»Ð¸ Ð½ÑƒÐ¶Ð½Ñ‹Ð¹ Ð¿Ð°Ñ‚Ñ‚ÐµÑ€Ð½ Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½, Ñ‚Ð¾ Ð¿Ð¾ÑÐ»Ðµ Ð¾Ñ‚Ð²ÐµÑ‚Ð° Ð²Ð½ÐµÑˆÐ½ÐµÐ³Ð¾ AI ÑÐ¸ÑÑ‚ÐµÐ¼Ð° ÑÑ‚Ñ€Ð¾Ð¸Ñ‚ DPO-ÑÐ¸Ð³Ð½Ð°Ð» Ð¸ Ð¾Ð±ÑƒÑ‡Ð°ÐµÑ‚ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÑƒÑŽ adaptive-Ð¼Ð¾Ð´ÐµÐ»ÑŒ.
7. Ð•ÑÐ»Ð¸ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ð¼ÐµÐ½ÑÐµÑ‚ Ð¸Ð»Ð¸ Ð¸ÑÐ¿Ñ€Ð°Ð²Ð»ÑÐµÑ‚ Ñ€ÐµÐ·ÑƒÐ»ÑŒÑ‚Ð°Ñ‚, ÑÐ¸ÑÑ‚ÐµÐ¼Ð° ÑÑ‚Ñ€Ð¾Ð¸Ñ‚ correction DPO-ÑÐ¸Ð³Ð½Ð°Ð» Ð¸ ÑƒÐ»ÑƒÑ‡ÑˆÐ°ÐµÑ‚ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÑƒÑŽ adaptive-Ð¼Ð¾Ð´ÐµÐ»ÑŒ.
8. ÐžÐ±Ñ‰Ð°Ñ Ð»Ð¾Ð³Ð¸ÐºÐ°, Ð¾Ð±Ñ‰Ð¸Ðµ Ð´Ð°Ñ‚Ð°ÑÐµÑ‚Ñ‹, ÑÐ¾Ð·Ð´Ð°Ð½Ð¸Ðµ common-Ð¼Ð¾Ð´ÐµÐ»ÐµÐ¹ Ð¸ common-Ð¼Ð¾Ð´ÐµÐ»ÑŒÐ½Ñ‹Ðµ Ð°Ñ€Ñ‚ÐµÑ„Ð°ÐºÑ‚Ñ‹ Ð½Ð°Ñ…Ð¾Ð´ÑÑ‚ÑÑ Ð² `C:\Projects\llm_mvp_core`.
9. ÐŸÐ¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ¸Ðµ adaptive-Ð´Ð°Ñ‚Ð°ÑÐµÑ‚Ñ‹ Ð¸ adaptive-Ð¼Ð¾Ð´ÐµÐ»Ð¸ Ð½Ð°Ñ…Ð¾Ð´ÑÑ‚ÑÑ Ñƒ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ Ð¸ Ð½Ðµ Ð¿ÐµÑ€ÐµÑ‚Ð¸Ñ€Ð°ÑŽÑ‚ÑÑ common-Ð°Ð¿Ð³Ñ€ÐµÐ¹Ð´Ð¾Ð¼.
10. Ð›ÑŽÐ±Ð°Ñ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ ÑÑ„ÐµÑ€Ð° Ð½Ð° Ð±Ð°Ð·Ðµ `ILearningMachine` Ð¸Ð¼ÐµÐµÑ‚ Ð´Ð²Ðµ Ð½Ð¾Ð³Ð¸: common/global leg Ñƒ Ð½Ð°Ñ Ð² `llm_mvp_core` Ð¸ personal/adaptive leg Ñƒ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ. ÐŸÐ¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ°Ñ Ð½Ð¾Ð³Ð° Ð¶Ð¸Ð²Ñ‘Ñ‚ Ð½Ð° ÐµÐ³Ð¾ ÐºÐ¾Ð¼Ð¿ÑŒÑŽÑ‚ÐµÑ€Ðµ, Ð°Ð´Ð°Ð¿Ñ‚Ð¸Ñ€ÑƒÐµÑ‚ Ð¿Ð¾Ð²ÐµÐ´ÐµÐ½Ð¸Ðµ Ð¿Ð¾Ð´ Ð½ÐµÐ³Ð¾, ÐµÑ‘ Ð¼Ð¾Ð¶Ð½Ð¾ Ð·Ð°Ð±ÑÐºÐ°Ð¿Ð¸Ñ‚ÑŒ, ÑÐºÑÐ¿Ð¾Ñ€Ñ‚Ð¸Ñ€Ð¾Ð²Ð°Ñ‚ÑŒ, Ð¸Ð¼Ð¿Ð¾Ñ€Ñ‚Ð¸Ñ€Ð¾Ð²Ð°Ñ‚ÑŒ, Ð¿ÐµÑ€ÐµÐ½ÐµÑÑ‚Ð¸ Ð½Ð° Ð´Ñ€ÑƒÐ³Ð¾Ðµ ÑƒÑÑ‚Ñ€Ð¾Ð¹ÑÑ‚Ð²Ð¾ Ð¸ Ñ€Ð°ÑÑˆÐ°Ñ€Ð¸Ñ‚ÑŒ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ñ‡ÐµÑ€ÐµÐ· Ð±ÐµÐ·Ð¾Ð¿Ð°ÑÐ½Ñ‹Ð¹ user-approved/anonymised package.
11. Ð’ `claude-xiigen-smart-skills`, `cursor-xiigen-smart-skills`, `codex-xiigen-smart-skills` Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ Ñ€ÐµÐ°Ð»ÑŒÐ½Ñ‹Ð¹ host entrypoint code, manifest/package wiring Ð¸ Ñ‚Ð¾Ð½ÐºÐ¸Ð¹ adapter Ðº XIIGen exe Ð¸Ð»Ð¸ ÑƒÑ‚Ð²ÐµÑ€Ð¶Ð´Ñ‘Ð½Ð½Ð¾Ð¼Ñƒ API Ð¸Ð· `llm_mvp_core`.

## 0.1 Ð˜ÑÑ‚Ð¾Ñ‡Ð½Ð¸ÐºÐ¸, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ðµ Ð¿Ð¾ÐºÑ€Ñ‹Ð²Ð°ÐµÑ‚ ÑÑ‚Ð¾Ñ‚ Ð¿Ð»Ð°Ð½

| Ð˜ÑÑ‚Ð¾Ñ‡Ð½Ð¸Ðº | ÐšÐ°Ðº Ð¸ÑÐ¿Ð¾Ð»ÑŒÐ·ÑƒÐµÑ‚ÑÑ |
|---|---|
| `C:\Users\lubao\Documents\xiigen\extensions  - orchestration based on our data\ORCHESTRATION-REASONING-MACHINE-PLAN-v1.md` | Ð¢Ñ€ÐµÐ±Ð¾Ð²Ð°Ð½Ð¸Ñ Ðº `OrchestrationReasoningMachine` |
| `C:\Users\lubao\Documents\xiigen\extensions  - orchestration based on our data\PROMPT-TO-TASK-MACHINE-PLAN-v1.md` | Ð¢Ñ€ÐµÐ±Ð¾Ð²Ð°Ð½Ð¸Ñ Ðº `PromptToTaskMachine` |
| `C:\Users\lubao\Documents\xiigen\extensions  - orchestration based on our data\ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md` | Ð¢Ð°ÐºÑÐ¾Ð½Ð¾Ð¼Ð¸Ñ negative DPO Ð´Ð»Ñ Ð¾Ñ€ÐºÐµÑÑ‚Ñ€Ð°Ñ†Ð¸Ð¸ |
| `C:\Users\lubao\Documents\xiigen\extensions  - orchestration based on our data\PROMPT-TO-TASK-DPO-NEGATIVE-STRATEGY-v1.md` | Ð¢Ð°ÐºÑÐ¾Ð½Ð¾Ð¼Ð¸Ñ negative DPO Ð´Ð»Ñ prompt-to-task |
| `C:\Projects\llm_mvp_core\docs\plans\DPO-KNOWLEDGE-MAP-v1.md` | ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÑŒÐ½Ð°Ñ ÐºÐ°Ñ€Ñ‚Ð° DPO skills/guides/schema/taxonomy/validation Ð´Ð»Ñ ÐºÐ°Ð¶Ð´Ð¾Ð³Ð¾ trainable/adaptive work order |
| `C:\Users\lubao\Documents\xiigen\extensions  - orchestration based on our data\ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-PLAN-v2.md` | Ð¡Ñ‚Ð°Ñ€Ñ‹Ð¹ Ð¿Ð»Ð°Ð½ ÐºÐ°Ðº evidence only; v3 Ð·Ð°Ð¼ÐµÐ½ÑÐµÑ‚ ÐµÐ³Ð¾ ÐºÐ°Ðº Ñ‚ÐµÐºÑƒÑ‰Ð¸Ð¹ approval artifact |
| `C:\Projects\llm_mvp_core\docs\plans\ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md` | Ð¢Ð¾Ð»ÑŒÐºÐ¾ ÑÑ‚Ð¸Ð»ÑŒ Ð¸ ÑƒÑ€Ð¾ÐºÐ¸ model lifecycle; Ð½Ðµ primary coverage target |

ÐšÐ¾Ñ€Ð½Ð¸ Ð´Ð»Ñ Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð¸Ñ Ð¸ reference examples:

| ÐšÐ¾Ñ€ÐµÐ½ÑŒ | Ð Ð¾Ð»ÑŒ |
|---|---|
| `C:\Users\lubao\Documents\xiigen\sonnet local - analyzes` | Ð ÐµÑ„ÐµÑ€ÐµÐ½ÑÑ‹ Ñ…Ð¾Ñ€Ð¾ÑˆÐµÐ³Ð¾ Ð°Ð½Ð°Ð»Ð¸Ð·Ð° Ð´Ð»Ñ `OrchestrationReasoningMachine` |
| `C:\Users\lubao\Documents\xiigen\claude-sessions\user prompts` | Ð¡Ñ‹Ñ€Ñ‹Ðµ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ¸Ðµ prompts Ð´Ð»Ñ `PromptToTaskMachine` |
| `C:\Users\lubao\Documents\xiigen\claude-sessions\sonnet-user-prompts` | ÐŸÑ€Ð¸Ð¼ÐµÑ€Ñ‹ prompt -> Ð¿Ð»Ð°Ð½, accepted/rejected orchestration |

Raw private prompts and raw private analyses must not be copied into committed artifacts. ÐšÐ¾Ð¼Ð¼Ð¸Ñ‚ÑÑ‚ÑÑ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ ÑÑ…ÐµÐ¼Ñ‹, Ñ…ÑÑˆÐ¸, source refs, redacted features Ð¸ approved DPO rows.

## 0.2 Ð¡ÐºÐ²Ð¾Ð·Ð½Ñ‹Ðµ gates, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ðµ Ð¾Ð±ÑÐ·Ð°Ð½Ñ‹ Ð±Ñ‹Ñ‚ÑŒ Ð² ÐºÐ°Ð¶Ð´Ð¾Ð¼ work order

ÐÐ¸ Ð¾Ð´Ð¸Ð½ Ð±ÑƒÐ´ÑƒÑ‰Ð¸Ð¹ work order Ð¸Ð· ÑÑ‚Ð¾Ð³Ð¾ Ð¿Ð»Ð°Ð½Ð° Ð½Ðµ ÑÑ‡Ð¸Ñ‚Ð°ÐµÑ‚ÑÑ Ð³Ð¾Ñ‚Ð¾Ð²Ñ‹Ð¼ Ðº Ð²Ñ‹Ð´Ð°Ñ‡Ðµ executor-ÑÑƒÐ±Ð°Ð³ÐµÐ½Ñ‚Ñƒ, Ð¿Ð¾ÐºÐ° parent `REVIEW_INTERNAL` Ð½Ðµ Ð¿Ñ€Ð¾Ð²ÐµÑ€Ð¸Ñ‚ Ñ‡ÐµÑ‚Ñ‹Ñ€Ðµ admission-ÑÐ»Ð¾Ñ Ð½Ð¸Ð¶Ðµ. Ð­Ñ‚Ð¾ Ð½Ðµ stop gate Ð´Ð»Ñ Ð°Ñ€Ñ…Ð¸Ñ‚ÐµÐºÑ‚Ð¾Ñ€Ð°: ÐµÑÐ»Ð¸ ÑÐ»Ð¾Ð¹ Ð½Ðµ Ð¿Ð¾ÐºÑ€Ñ‹Ñ‚, Ð°Ñ€Ñ…Ð¸Ñ‚ÐµÐºÑ‚Ð¾Ñ€ Ð¾Ð±ÑÐ·Ð°Ð½ Ð¸ÑÐ¿Ñ€Ð°Ð²Ð¸Ñ‚ÑŒ work order Ð¸Ð»Ð¸ Ð¿Ð»Ð°Ð½ Ð¸ Ð¿Ñ€Ð¾Ð´Ð¾Ð»Ð¶Ð¸Ñ‚ÑŒ Ð¿Ð¾Ð´Ð³Ð¾Ñ‚Ð¾Ð²ÐºÑƒ.

| Ð¡ÐºÐ²Ð¾Ð·Ð½Ð¾Ð¹ gate | Ð“Ð´Ðµ Ð¾Ð±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ | Ð§Ñ‚Ð¾ Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð´Ð¾ÐºÐ°Ð·Ð°Ñ‚ÑŒ artifact |
|---|---|---|
| `DPO-KNOWLEDGE-MAP` | ÐšÐ°Ð¶Ð´Ñ‹Ð¹ trainable, adaptive Ð¸Ð»Ð¸ DPO work order | Ð—Ð°Ð³Ñ€ÑƒÐ¶ÐµÐ½Ñ‹ DPO skills, Ð½Ð°Ð·Ð²Ð°Ð½Ñ‹ guide paths, Ð¾Ð¿Ñ€ÐµÐ´ÐµÐ»ÐµÐ½Ñ‹ chosen/rejected schema, taxonomy, registry fields, train-ready validation, held-out evidence, fresh-load evidence Ð¸ continue-training evidence. |
| `DPO-UNIVERSALIZATION-GATE` | ÐšÐ°Ð¶Ð´Ñ‹Ð¹ common Ð¸Ð»Ð¸ adaptive DPO work order Ð´Ð»Ñ OrchestrationReasoning, PromptToTask, SkillSelection Ð¸ ResultGap | DPO rows Ñ€Ð°Ð·Ð´ÐµÐ»ÐµÐ½Ñ‹ Ð½Ð° universal reasoning / universal skill semantics, project adapter refs Ð¸ excluded technical execution refs. ÐŸÐ¾ÐºÐ° `TECH-EXEC-LEARNING-GATE` Ð½Ðµ Ð¿Ñ€Ð¸Ð½ÑÑ‚, common Ð¸ adaptive rows Ð½Ðµ ÑÐ¾Ð´ÐµÑ€Ð¶Ð°Ñ‚ code-bound execution instructions. |
| `TEST-STRATEGY-ADMISSION` | ÐšÐ°Ð¶Ð´Ñ‹Ð¹ work order, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ð¼ÐµÐ½ÑÐµÑ‚ code, model, data, host, API Ð¸Ð»Ð¸ asset | Ð”Ð»Ñ ÑÑ‚Ð¾Ð¹ Ð²ÐµÑ‚ÐºÐ¸ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ñ‹ Ð½ÑƒÐ¶Ð½Ñ‹Ðµ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÐºÐ¸: unit, integration, e2e/host, QA automation, negative tests, untrained honesty, trained acceptance, regression. Ð•ÑÐ»Ð¸ ÐºÐ°ÐºÐ°Ñ-Ñ‚Ð¾ Ð²ÐµÑ‚ÐºÐ° Ð½ÐµÐ¿Ñ€Ð¸Ð¼ÐµÐ½Ð¸Ð¼Ð°, artifact Ð¾Ð±ÑŠÑÑÐ½ÑÐµÑ‚ Ð¿Ð¾Ñ‡ÐµÐ¼Ñƒ. |
| `UML-EXECUTABLE-CASE-MAP` | ÐšÐ°Ð¶Ð´Ñ‹Ð¹ work order, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ñ€ÐµÐ°Ð»Ð¸Ð·ÑƒÐµÑ‚ UML edge, Ð°Ð»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼Ð¸Ñ‡ÐµÑÐºÑƒÑŽ Ð²ÐµÑ‚ÐºÑƒ Ð¸Ð»Ð¸ host path | ÐšÐ°Ð¶Ð´Ð°Ñ UML-ÑÑ‚Ñ€ÐµÐ»ÐºÐ° Ð¸ ÐºÐ°Ð¶Ð´Ð°Ñ algorithm branch Ð¼Ð°Ð¿Ð¸Ñ‚ÑÑ Ð½Ð° input fixture, expected fields, expected asset touched/not touched, expected visible chat/debug output. |
| `AUTHORITY-SELF-REFLECTION-GATE` | ÐšÐ°Ð¶Ð´Ñ‹Ð¹ work order Ð±ÐµÐ· Ð¸ÑÐºÐ»ÑŽÑ‡ÐµÐ½Ð¸Ñ | Ð—Ð°Ð³Ñ€ÑƒÐ¶ÐµÐ½Ñ‹ conversation-intent/self-reflection, architect-behavior-classifier, plan-review, skill-injection, implementation-integrity Ð¸ phase-completion skills as applicable. Gate ÑÐ²Ð½Ð¾ Ð¿Ñ€ÐµÐ´Ð¾Ñ‚Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ executor drift, defensive freeze, action-as-relief, focus loss Ð¸ entropy tolerance. |
| `CHECKPOINT-WRITER-GUARD` | ÐšÐ°Ð¶Ð´Ñ‹Ð¹ training/adaptive/model asset work order | ÐžÐ´Ð¸Ð½ checkpoint Ð¿Ð¸ÑˆÐµÑ‚ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ð¾Ð´Ð¸Ð½ writer; checkpoint Ð½Ðµ Ð¿Ð¸ÑˆÐµÑ‚ÑÑ Ð¿Ñ€Ð¸ unknown/failed gate; resume ÑÑ‚Ñ€Ð¾Ð¸Ñ‚ÑÑ Ð¸Ð· accepted pair ids / accepted source refs, Ð° Ð½Ðµ Ð¸Ð· cursor-prefix Ð¸Ð»Ð¸ replay; corrupted checkpoint Ð½Ðµ Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÐµÑ‚ÑÑ ÐºÐ°Ðº continuation. |
| `METRIC-HONESTY-GATE` | ÐšÐ°Ð¶Ð´Ñ‹Ð¹ work order, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ð·Ð°ÑÐ²Ð»ÑÐµÑ‚ accuracy, readiness, trained quality Ð¸Ð»Ð¸ improvement | Retrieval/routing Ð¸ trained generalization ÑÑ‡Ð¸Ñ‚Ð°ÑŽÑ‚ÑÑ Ð¾Ñ‚Ð´ÐµÐ»ÑŒÐ½Ð¾; headline quality = only truly-novel held-out generalization; metrics Ð²Ð¾ÑÐ¿Ñ€Ð¾Ð¸Ð·Ð²Ð¾Ð´ÑÑ‚ÑÑ Ð¸Ð· runner/generator, Ð½Ðµ Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÑŽÑ‚ÑÑ ÐºÐ°Ðº hand-written JSON. |
| `HUMAN-AUTHORITY-BOUNDARY-GATE` | PromptToTask, OrchestrationReasoning, plan-review and host-facing work orders | Ð¢ÐµÐºÑƒÑ‰Ð°Ñ ÑÐ²Ð½Ð°Ñ ÐºÐ¾Ð¼Ð°Ð½Ð´Ð° Ð›ÑƒÐ±Ñ‹ Ð¿Ð¾Ð±ÐµÐ¶Ð´Ð°ÐµÑ‚ Ð²Ð»Ð¾Ð¶ÐµÐ½Ð½Ñ‹Ð¹ Ñ„Ð°Ð¹Ð», ÑÑ‚Ð°Ñ€Ñ‹Ð¹ Ð¿Ð»Ð°Ð½, state, inferred task and machine artifact name. Attachment alone never overrides current human instruction. |
| `SUBAGENT-PACKET-QUALITY-GATE` | ÐšÐ°Ð¶Ð´Ñ‹Ð¹ bounded sub-agent packet and parent review | Packet names required reasoning tier/capability, forbids self-approval, treats review as stale after mutation, and exposes ALLOW/CONTINUE/REPAIR/OPERATOR-INPUT verdicts. |
| `TEST-EXECUTION-HYGIENE-GATE` | ÐšÐ°Ð¶Ð´Ñ‹Ð¹ work order with test/runtime command | Plan separates pre-existing red from phase-induced regression, checks/remediates exact orphan process roots before rerun, and requires heartbeat/replacement-worker handoff for long-running sub-agent work. |

ÐœÐ¸Ð½Ð¸Ð¼Ð°Ð»ÑŒÐ½Ñ‹Ðµ Ð¸Ð¼ÐµÐ½Ð° artifacts:

```text
artifacts/dpo-knowledge-map/<work_order>.json
artifacts/dpo-universalization/<work_order>.json
artifacts/test-strategy-admission/<work_order>.json
artifacts/uml-executable-case-map/<work_order>.json
artifacts/authority-self-reflection/<work_order>.json
artifacts/checkpoint-writer-guard/<work_order>.json
artifacts/metric-honesty/<work_order>.json
artifacts/human-authority-boundary/<work_order>.json
artifacts/subagent-packet-quality/<work_order>.json
artifacts/test-execution-hygiene/<work_order>.json
```

CM-24B future invariant for `AUTHORITY-SELF-REFLECTION-GATE`:

- Every future bounded sub-agent work order must name the required skills/guides and must return or reference `artifacts/authority-self-reflection/<work_order>.json` before parent acceptance.
- Minimum artifact fields are: `work_order_id`, `authority_source`, `loaded_skills_guides`, `direct_question_priority_check`, `architect_parent_not_executor_check`, `action_as_relief_check`, `defensive_freeze_or_defensive_self_reflection_check`, `focus_soft_notes_review`, `entropy_ordering_check`, `test_only_not_implementation_check`, `parent_review_required`, `status`, `gaps`, and `next_action`.
- Parent `REVIEW_INTERNAL` rejects any bounded sub-agent packet that lacks this artifact or an explicit not-applicable reason. The next action is the authority/self-reflection artifact repair for that same work order, not phase completion and not final acceptance.
- Historical work orders before CM-24B remain recorded as partial when no standalone per-work-order artifact exists; this invariant closes only the future admission hole and does not fabricate historical evidence.

Ð•ÑÐ»Ð¸ work order Ð½Ðµ ÑÐ¾Ð·Ð´Ð°Ñ‘Ñ‚ ÑÑ‚Ð¸ artifacts Ð¸Ð»Ð¸ Ð½Ðµ Ð¾Ð±ÑŠÑÑÐ½ÑÐµÑ‚ Ð½ÐµÐ¿Ñ€Ð¸Ð¼ÐµÐ½Ð¸Ð¼Ð¾ÑÑ‚ÑŒ ÑÐ»Ð¾Ñ, parent review Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ ÐµÐ³Ð¾ Ð½Ð° repair. Executor Ð½Ðµ Ð¸Ð¼ÐµÐµÑ‚ Ð¿Ñ€Ð°Ð²Ð° Ð·Ð°Ð¼ÐµÐ½Ð¸Ñ‚ÑŒ ÑÑ‚Ð¸ gates ÑƒÑÑ‚Ð½Ñ‹Ð¼ Ð¾Ð±ÐµÑ‰Ð°Ð½Ð¸ÐµÐ¼.

---

# Part A - Ð¿Ð¾Ð½ÑÑ‚Ð½Ð°Ñ Ð°Ñ€Ñ…Ð¸Ñ‚ÐµÐºÑ‚ÑƒÑ€Ð° Ð´Ð»Ñ Ð›ÑƒÐ±Ñ‹

Part A ÑÐ²Ð»ÑÐµÑ‚ÑÑ Ð³Ð»Ð°Ð²Ð½Ñ‹Ð¼ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ¸Ð¼ Ð¾Ð¿Ð¸ÑÐ°Ð½Ð¸ÐµÐ¼. Part B Ð½Ðµ Ð¸Ð¼ÐµÐµÑ‚ Ð¿Ñ€Ð°Ð²Ð° Ñ‚Ð°Ð¹Ð½Ð¾ Ð¼ÐµÐ½ÑÑ‚ÑŒ Ð°Ñ€Ñ…Ð¸Ñ‚ÐµÐºÑ‚ÑƒÑ€Ñƒ, Ð°Ð»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼Ñ‹, ownership Ð´Ð°Ð½Ð½Ñ‹Ñ… Ð¸Ð»Ð¸ product behavior.

Governance parity rule: Ñ‚ÐµÐºÑƒÑ‰Ð°Ñ Ð¼ÐµÑ‚ÐºÐ° ÑÑ‚Ð¾Ð³Ð¾ Ð¿Ñ€Ð°Ð²Ð¸Ð»Ð° â€” **OP-49 / FC-19** (Ð½Ðµ OP-48). Parent `REVIEW_INTERNAL` Ð¾Ð±ÑÐ·Ð°Ð½ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÑ‚ÑŒ `Part A == Part B` Ð¿Ð¾ core-Ð¾Ð±Ð»Ð°ÑÑ‚ÑÐ¼: architecture, product decisions Ð¸ algorithms. Ð•ÑÐ»Ð¸ Part B Ð¼ÐµÐ½ÑÐµÑ‚ Ð°Ñ€Ñ…Ð¸Ñ‚ÐµÐºÑ‚ÑƒÑ€Ñƒ, Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÐµÑ‚ product decision Ð¸Ð»Ð¸ Ð´Ð¾Ð±Ð°Ð²Ð»ÑÐµÑ‚/Ð¿Ð¾Ð´Ð¼ÐµÐ½ÑÐµÑ‚ Ð°Ð»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼, ÐºÐ¾Ñ‚Ð¾Ñ€Ð¾Ð³Ð¾ Ð½ÐµÑ‚ Ð² Part A, Ð¿Ð»Ð°Ð½ Ð½Ðµ Ð¿Ñ€Ð¸Ð½ÑÑ‚ Ð¸ Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ Ð¾Ñ‚Ñ€ÐµÐ¼Ð¾Ð½Ñ‚Ð¸Ñ€Ð¾Ð²Ð°Ð½ Ð´Ð¾ Ð¸ÑÐ¿Ð¾Ð»Ð½ÐµÐ½Ð¸Ñ.

## A0. ÐŸÑ€Ð¾ÑÑ‚Ð°Ñ UML-Ð´Ð¸Ð°Ð³Ñ€Ð°Ð¼Ð¼Ð° Ð´Ð»Ñ draw.io

Ð˜Ð¼Ð¿Ð¾Ñ€Ñ‚ Ð² draw.io: **Arrange -> Insert -> Advanced -> PlantUML**.

```plantuml
@startuml
title XIIGen smart-skills extensions - Ð»Ð¾Ð³Ð¸ÐºÐ° Ð¸ Ð¿Ð¾Ñ‚Ð¾Ðº Ð´Ð°Ð½Ð½Ñ‹Ñ…

skinparam classAttributeIconSize 0

class "Claude extension\nclaude-xiigen-smart-skills" as ClaudeHost {
  user prompt
  debug setting
  host entrypoint
}

class "Cursor extension\ncursor-xiigen-smart-skills" as CursorHost {
  user prompt
  debug setting
  VS Code entrypoint
}

class "Codex extension\ncodex-xiigen-smart-skills" as CodexHost {
  user prompt
  debug setting
  Codex plugin/app entrypoint
}

class "Ð—Ð°Ð¿Ñ€Ð¾Ñ Ð² XIIGen\nXiigenExtensionRequest" as Request {
  HostName
  Method
  UserPromptOrTaskText
  DebugEnabled
  AvailableSkills
  CheckSetInputs
  Provenance
}

class "XIIGen core API\nXiigenExtensionApiService" as CoreApi {
  validate request
  hash/redact prompt
  call core machines
  build response/debug trace
}

class "PromptToTaskMachine\nÐ·Ð°Ð¿Ñ€Ð¾Ñ -> Ð·Ð°Ð´Ð°Ñ‡Ð°" as PromptToTask {
  ÑÐµÐ¹Ñ‡Ð°Ñ: skeleton
  Ñ†ÐµÐ»ÑŒ: Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ñ‹Ð¹ Ð¿ÐµÑ€ÐµÐ²Ð¾Ð´ prompt Ð² task
}

class "OrchestrationReasoningMachine\nÐ·Ð°Ð´Ð°Ñ‡Ð° -> Ð¿Ð»Ð°Ð½" as Reasoning {
  ÑÐµÐ¹Ñ‡Ð°Ñ: skeleton
  Ñ†ÐµÐ»ÑŒ: Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ðµ Ð¿Ð»Ð°Ð½Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð¸Ðµ Ð¿Ð¾ Ð³Ð°Ð¹Ð´Ð°Ð¼
}

class "SkillSelectionMachine\nÐ·Ð°Ð´Ð°Ñ‡Ð° -> ÑÐºÐ¸Ð»Ñ‹" as SkillSelection {
  ÑÐµÐ¹Ñ‡Ð°Ñ: partial trained state
  Ñ†ÐµÐ»ÑŒ: semantic skill selection
}

class "ResultGapMachine\nÐ¿Ð¾ÑÐ»Ðµ Ð¾Ñ‚Ð²ÐµÑ‚Ð° AI -> missing pattern" as ResultGap {
  ÑÐµÐ¹Ñ‡Ð°Ñ: bootstrap fixture
  Ñ†ÐµÐ»ÑŒ: Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ñ‹Ð¹ result-gap unit
}

class "SkillSelectionFeedbackWriter\ncorrection -> DPO signal" as FeedbackWriter {
  writes project-local signal
  does not mutate common library
}

class "Common assets\nllm_mvp_core" as CommonAssets {
  common training sets
  common checkpoints
  manifests
  locators
  upgrades
}

class "User adaptive assets\nuser side .xiigen" as UserAssets {
  adaptive checkpoints
  user DPO signals
  preferences
  feedback
  backup packages
  restore/import packages
  cross-device transfer
}

class "Two-leg ILearningMachinePair\ncommon + personal" as TwoLegPair {
  common/global leg
  personal/adaptive leg
  backup/export user leg
  restore/import user leg
  user-approved anonymised share
}

class "ÐžÑ‚Ð²ÐµÑ‚ XIIGen\nXiigenExtensionResponse" as Response {
  selected skills
  structured task refs
  scenario refs
  warnings/errors
  optional debug trace
}

ClaudeHost --> Request : Ð¾Ñ‚Ð¿Ñ€Ð°Ð²Ð»ÑÐµÑ‚ prompt
CursorHost --> Request : Ð¾Ñ‚Ð¿Ñ€Ð°Ð²Ð»ÑÐµÑ‚ prompt
CodexHost --> Request : Ð¾Ñ‚Ð¿Ñ€Ð°Ð²Ð»ÑÐµÑ‚ prompt

Request --> CoreApi : thin API/check-set request
CoreApi --> PromptToTask : prompt hash + redacted context
PromptToTask --> Reasoning : StructuredTaskContract
Reasoning --> SkillSelection : scenario/task context
SkillSelection --> CoreApi : SkillSelectionDecision
CoreApi --> Response : chat text + refs + debug trace
Response --> ClaudeHost : display in chat
Response --> CursorHost : display in UI/chat
Response --> CodexHost : display in plugin/app

PromptToTask --> TwoLegPair : must have common + personal legs
Reasoning --> TwoLegPair : must have common + personal legs
SkillSelection --> TwoLegPair : must have common + personal legs
ResultGap --> TwoLegPair : must have common + personal legs
TwoLegPair --> CommonAssets : common checkpoint/model/set
TwoLegPair --> UserAssets : personal checkpoint/model/DPO

Response --> ResultGap : Ð¿Ð¾ÑÐ»Ðµ AI output Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÐµÐ¼ missing pattern
ResultGap --> UserAssets : adaptive DPO row
FeedbackWriter --> UserAssets : correction DPO row
CommonAssets --> CoreApi : common model load/upgrade
UserAssets --> CoreApi : adaptive model load
CoreApi --> UserAssets : preserve adaptive state

note right of ClaudeHost
Host repo must contain real Claude entrypoint code.
JSON/PowerShell evidence is not a plugin substitute.
end note

note right of CursorHost
Host repo must contain real Cursor/VS Code command,
chat participant, panel, or equivalent entrypoint.
end note

note right of CodexHost
Host repo must contain real Codex plugin/app entrypoint.
end note

note bottom of CoreApi
Core logic lives in llm_mvp_core.
Hosts remain thin clients.
No external AI/tool use without explicit Luba approval.
end note

@enduml
```

### A0.1 Ð¡Ð¸Ð¼ÑƒÐ»ÑÑ†Ð¸Ð¸ Ð¸ Ð¾Ð¶Ð¸Ð´Ð°ÐµÐ¼Ñ‹Ðµ Ñ€ÐµÐ·ÑƒÐ»ÑŒÑ‚Ð°Ñ‚Ñ‹ Ð¿Ð¾ ÐºÐ°Ð¶Ð´Ð¾Ð¼Ñƒ ÑÐ¾ÐµÐ´Ð¸Ð½ÐµÐ½Ð¸ÑŽ UML

Ð­Ñ‚Ð¾ Ñ‡Ð°ÑÑ‚ÑŒ UML, Ð° Ð½Ðµ Ñ‚ÐµÑ…Ð½Ð¸Ñ‡ÐµÑÐºÐ°Ñ Ð¼ÐµÐ»Ð¾Ñ‡ÑŒ. ÐšÐ°Ð¶Ð´Ð°Ñ ÑÑ‚Ñ€ÐµÐ»ÐºÐ° Ð½Ð° Ð´Ð¸Ð°Ð³Ñ€Ð°Ð¼Ð¼Ðµ Ð´Ð¾Ð»Ð¶Ð½Ð° Ð¸Ð¼ÐµÑ‚ÑŒ Ð¿Ð¾Ð½ÑÑ‚Ð½Ñ‹Ð¹ ÑÑ†ÐµÐ½Ð°Ñ€Ð¸Ð¹: Ñ‡Ñ‚Ð¾ Ð¾Ñ‚Ð¿Ñ€Ð°Ð²Ð¸Ð»Ð¸, Ñ‡Ñ‚Ð¾ Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ð¿Ñ€Ð¾Ð¸Ð·Ð¾Ð¹Ñ‚Ð¸, ÐºÐ°ÐºÐ¾Ð¹ Ñ€ÐµÐ·ÑƒÐ»ÑŒÑ‚Ð°Ñ‚ ÑÑ‡Ð¸Ñ‚Ð°ÐµÑ‚ÑÑ Ð¿Ñ€Ð°Ð²Ð¸Ð»ÑŒÐ½Ñ‹Ð¼. Ð•ÑÐ»Ð¸ Ð² Part B Ð¿Ð¾ÑÐ²Ð»ÑÐµÑ‚ÑÑ Ð½Ð¾Ð²Ð°Ñ ÑÑ‚Ñ€ÐµÐ»ÐºÐ° Ð±ÐµÐ· ÑÑ‚Ñ€Ð¾ÐºÐ¸ Ð² ÑÑ‚Ð¾Ð¹ Ñ‚Ð°Ð±Ð»Ð¸Ñ†Ðµ, Ð¿Ð»Ð°Ð½ ÑÐ½Ð¾Ð²Ð° Ð½Ð°Ñ€ÑƒÑˆÐ°ÐµÑ‚ `A == B`.

| UML connection | Ð¡Ð¸Ð¼ÑƒÐ»ÑÑ†Ð¸Ñ | ÐžÐ¶Ð¸Ð´Ð°ÐµÐ¼Ñ‹Ð¹ Ñ€ÐµÐ·ÑƒÐ»ÑŒÑ‚Ð°Ñ‚ |
|---|---|---|
| `ClaudeHost --> Request` | ÐŸÐ¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ð² Claude Ð¿Ð¸ÑˆÐµÑ‚: "ÐŸÑ€Ð¾Ð²ÐµÑ€ÑŒ Ð¿Ð»Ð°Ð½ extensions, Ð½Ð°Ð¹Ð´Ð¸ Ð´Ñ‹Ñ€Ñ‹ Ð¸ Ð½Ðµ Ð½Ð°Ñ‡Ð¸Ð½Ð°Ð¹ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð°Ñ†Ð¸ÑŽ". Debug Ð²ÐºÐ»ÑŽÑ‡ÐµÐ½. | Claude extension Ð½Ðµ Ñ€ÐµÑˆÐ°ÐµÑ‚ Ð·Ð°Ð´Ð°Ñ‡Ñƒ ÑÐ°Ð¼Ð°. ÐžÐ½Ð° ÑÐ¾Ð±Ð¸Ñ€Ð°ÐµÑ‚ `XiigenExtensionRequest` Ñ `HostName=Claude`, `Method=plan_review_orchestration`, Ñ‚ÐµÐºÑÑ‚Ð¾Ð¼ Ð·Ð°Ð¿Ñ€Ð¾ÑÐ°, debug flag, provenance Ð¸ ÑÐ¿Ð¸ÑÐºÐ¾Ð¼ Ð´Ð¾ÑÑ‚ÑƒÐ¿Ð½Ñ‹Ñ… host facts. |
| `CursorHost --> Request` | ÐŸÐ¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ð² Cursor Ð¿Ñ€Ð¾ÑÐ¸Ñ‚: "ÐÐ°Ð¹Ð´Ð¸ ÑÐºÐ¸Ð»Ñ‹ Ð´Ð»Ñ Ñ€ÐµÐ¼Ð¾Ð½Ñ‚Ð° VS Code extension Ð¸ Ð¿Ð¾ÐºÐ°Ð¶Ð¸ trace". | Cursor extension ÑÐ¾Ð·Ð´Ð°Ñ‘Ñ‚ Ñ‚Ð°ÐºÐ¾Ð¹ Ð¶Ðµ Ñ‚Ð¾Ð½ÐºÐ¸Ð¹ request, Ð½Ð¾ Ñ `HostName=Cursor` Ð¸ VS Code/Cursor provenance. Ð’ host repo Ð½ÐµÑ‚ semantic ranking Ð¸Ð»Ð¸ DPO logic. |
| `CodexHost --> Request` | ÐŸÐ¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ð² Codex Ð¿Ñ€Ð¾ÑÐ¸Ñ‚: "ÐŸÐµÑ€ÐµÐ²ÐµÐ´Ð¸ Ð¼Ð¾Ð¹ prompt Ð² Ð·Ð°Ð´Ð°Ñ‡Ñƒ Ð¸ Ð¿Ñ€ÐµÐ´Ð»Ð¾Ð¶Ð¸ orchestration plan". | Codex plugin/app entrypoint ÑÐ¾Ð·Ð´Ð°Ñ‘Ñ‚ request Ñ `HostName=Codex`, Ð½Ð¾ Ð½Ðµ Ð²Ñ‹Ð·Ñ‹Ð²Ð°ÐµÑ‚ Ð²Ð½ÐµÑˆÐ½ÑŽÑŽ AI model Ð¸ Ð½Ðµ Ð²Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ Ð¿Ð»Ð°Ð½ Ð²Ð½ÑƒÑ‚Ñ€Ð¸ host. |
| `Request --> CoreApi` | Host Ð¾Ñ‚Ð¿Ñ€Ð°Ð²Ð»ÑÐµÑ‚ request Ñ raw prompt, debug flag, available skills and check-set inputs. | `XiigenExtensionApiService` Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÐµÑ‚ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ thin API/check-set request, Ð²Ð°Ð»Ð¸Ð´Ð¸Ñ€ÑƒÐµÑ‚ Ð¾Ð±ÑÐ·Ð°Ñ‚ÐµÐ»ÑŒÐ½Ñ‹Ðµ Ð¿Ð¾Ð»Ñ, redacts/hashes private prompt before training artifacts, rejects unsafe external-tool request unless Luba approved it. |
| `CoreApi --> PromptToTask` | Core Ð¿Ð¾Ð»ÑƒÑ‡Ð¸Ð» prompt: "Ð¡Ð´ÐµÐ»Ð°Ð¹ plan-only repair, Ð½Ðµ Ð²Ñ‹Ð¿Ð¾Ð»Ð½ÑÐ¹ ÐºÐ¾Ð´". | Ð’ `PromptToTaskMachine` ÑƒÑ…Ð¾Ð´Ð¸Ñ‚ redacted prompt/context, Ð° Ð½Ðµ full private transcript. ÐžÐ¶Ð¸Ð´Ð°ÐµÐ¼Ñ‹Ð¹ Ð²Ñ‹Ñ…Ð¾Ð´: `StructuredTaskContract` Ñ Ñ‚Ð¸Ð¿Ð¾Ð¼ `plan_repair`, constraints `no_implementation`, `subagent_review`, `A_equals_B_required`. |
| `PromptToTask --> Reasoning` | Structured task Ð³Ð¾Ð²Ð¾Ñ€Ð¸Ñ‚: "Ð´Ð¾Ð±Ð°Ð²Ð¸Ñ‚ÑŒ UML simulations per connection". | `OrchestrationReasoningMachine` Ð¿Ð¾Ð»ÑƒÑ‡Ð°ÐµÑ‚ Ð¸Ð¼ÐµÐ½Ð½Ð¾ Ð·Ð°Ð´Ð°Ñ‡Ñƒ, Ð° Ð½Ðµ raw chat. ÐžÐ¶Ð¸Ð´Ð°ÐµÐ¼Ñ‹Ð¹ Ð²Ñ‹Ñ…Ð¾Ð´: scenario refs Ð´Ð»Ñ readable plan repair, affected Part A sections, gates: static coverage + sub-agent review, forbidden actions: code/runtime. |
| `Reasoning --> SkillSelection` | Reasoning Ð²Ñ‹Ð±Ñ€Ð°Ð» ÑÑ†ÐµÐ½Ð°Ñ€Ð¸Ð¹ "plan-readable architecture repair". | `SkillSelectionMachine` Ð¿Ð¾Ð»ÑƒÑ‡Ð°ÐµÑ‚ task/scenario context Ð¸ Ð²Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ skills like planning, plan-review, architecture-review, not executor/build/test skills. Ð’Ñ‹Ð±Ð¾Ñ€ Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ backed by trained semantic state, not candidate-position weights. |
| `SkillSelection --> CoreApi` | Skill selection Ð²ÐµÑ€Ð½ÑƒÐ» candidate skills + confidence + evidence refs. | CoreApi Ð¿Ð¾Ð»ÑƒÑ‡Ð°ÐµÑ‚ `SkillSelectionDecision`; ÐµÑÐ»Ð¸ model gate refs missing, response must say semantic quality is not proven. No host-side reranking. |
| `CoreApi --> Response` | Core ÑÐ¾Ð±Ñ€Ð°Ð» task refs, scenario refs, selected skills, warnings and debug trace. | `XiigenExtensionResponse` ÑÐ¾Ð´ÐµÑ€Ð¶Ð¸Ñ‚ readable result for chat: selected skills, structured task refs, scenario refs, warnings/errors, optional safe debug trace. It does not expose private corpus rows or model internals. |
| `Response --> ClaudeHost` | Claude Ð¿Ð¾Ð»ÑƒÑ‡Ð°ÐµÑ‚ response for plan repair. | Claude Ð¿Ð¾ÐºÐ°Ð·Ñ‹Ð²Ð°ÐµÑ‚ Ð² Ñ‡Ð°Ñ‚Ðµ Ð¿Ð¾Ð½ÑÑ‚Ð½Ñ‹Ð¹ Ð¾Ñ‚Ð²ÐµÑ‚ Ð¸, ÐµÑÐ»Ð¸ debug Ð²ÐºÐ»ÑŽÑ‡ÐµÐ½, safe trace: `xiigen <method> :`, `input:`, `output:`. Raw private prompt/secrets are redacted. |
| `Response --> CursorHost` | Cursor Ð¿Ð¾Ð»ÑƒÑ‡Ð°ÐµÑ‚ response for extension work. | Cursor Ð¾Ñ‚Ð¾Ð±Ñ€Ð°Ð¶Ð°ÐµÑ‚ Ð¾Ñ‚Ð²ÐµÑ‚ Ð² UI/chat/panel. Debug trace visible only if setting enabled and redacted. |
| `Response --> CodexHost` | Codex Ð¿Ð¾Ð»ÑƒÑ‡Ð°ÐµÑ‚ response for smart-skills orchestration. | Codex plugin/app Ð¿Ð¾ÐºÐ°Ð·Ñ‹Ð²Ð°ÐµÑ‚ structured answer. It does not claim plugin completion from JSON-only artifacts. |
| `PromptToTask --> TwoLegPair` | PromptToTask learns common prompt-to-task behavior from approved prompt/task pairs and also adapts to Luba's personal phrasing. | PromptToTask must have common/global checkpoint in `llm_mvp_core` and personal/adaptive checkpoint under user `.xiigen`. Missing either leg fails the plan. |
| `Reasoning --> TwoLegPair` | Reasoning learns common orchestration patterns from guides and personal preferences from Luba corrections. | OrchestrationReasoning must load common guide-state model plus user adaptive state. It cannot use filename/BM25/static route table as final reasoning. |
| `SkillSelection --> TwoLegPair` | SkillSelection learns general skill mapping and adapts when user corrects selected skills. | All semantic units must be represented in model assets; adaptive user corrections must not mutate common checkpoint. |
| `ResultGap --> TwoLegPair` | After AI output, system sees "the answer missed UML edge simulations". | ResultGap has common missing-pattern model and user adaptive leg. It produces an adaptive signal instead of only a chat note. |
| `TwoLegPair --> CommonAssets` | Common model v1 is upgraded by project. | Common checkpoint/manifest/locator/training refs live in `llm_mvp_core`; upgrade can change common hash but must not overwrite user adaptive hash. |
| `TwoLegPair --> UserAssets` | User has personal corrections and preferred plan style. | Personal checkpoint/DPO/preferences live in user `.xiigen`, can be backed up/exported/restored/shared only through safe user-approved package. |
| `Response --> ResultGap` | AI/host output comes back and user says: "this missed a whole branch". | ResultGap checks the output against expected task/scenario/skills. If pattern missing, it returns a missing-pattern finding and creates/requests adaptive DPO path. |
| `ResultGap --> UserAssets` | Missing branch is confirmed after response. | A user-local DPO row is written with chosen/rejected refs, redaction report and provenance. Common model is unchanged. |
| `FeedbackWriter --> UserAssets` | User changes selected skills or rewrites the plan answer. | Correction DPO signal is written to `.xiigen`; chosen reflects user's accepted correction, rejected reflects previous weaker output. |
| `CommonAssets --> CoreApi` | Core starts and needs latest common smart-skills models. | CoreApi loads common manifests/checkpoints/locators from `llm_mvp_core`; if missing, response must downgrade semantic claim and name missing asset. |
| `UserAssets --> CoreApi` | User has adaptive checkpoint from previous sessions. | CoreApi loads adaptive user model alongside common model. If user package is absent, common still works but personalization claim is disabled. |
| `CoreApi --> UserAssets` | Common model upgraded or request processed. | CoreApi preserves adaptive state. It may append approved adaptive signals but cannot overwrite/delete user checkpoint during common upgrade. |

ÐœÐ¸Ð½Ð¸Ð¼Ð°Ð»ÑŒÐ½Ñ‹Ð¹ simulation gate Ð´Ð»Ñ ÐºÐ°Ð¶Ð´Ð¾Ð¹ ÑÑ‚Ñ€Ð¾ÐºÐ¸: `input fixture -> expected request/response fields -> expected asset touched/not touched -> expected visible chat/debug output`. Ð•ÑÐ»Ð¸ executor Ð½Ðµ Ð¼Ð¾Ð¶ÐµÑ‚ ÑƒÐºÐ°Ð·Ð°Ñ‚ÑŒ ÑÑ‚Ð¸ Ñ‡ÐµÑ‚Ñ‹Ñ€Ðµ ÑÐ»ÐµÐ¼ÐµÐ½Ñ‚Ð° Ð´Ð»Ñ ÑÑ‚Ñ€ÐµÐ»ÐºÐ¸, ÑÑ‚Ð° ÑÐ²ÑÐ·ÑŒ ÐµÑ‰Ñ‘ Ð½Ðµ Ð³Ð¾Ñ‚Ð¾Ð²Ð° Ðº Ñ€ÐµÐ°Ð»Ð¸Ð·Ð°Ñ†Ð¸Ð¸.

### A0.1.1 Executable model/host test-case artifact Ð¸Ð· UML

Ð¢Ð°Ð±Ð»Ð¸Ñ†Ð° Ð²Ñ‹ÑˆÐµ Ð½Ðµ ÑÐ²Ð»ÑÐµÑ‚ÑÑ Ñ„Ð¸Ð½Ð°Ð»ÑŒÐ½Ñ‹Ð¼ Ð´Ð¾ÐºÐ°Ð·Ð°Ñ‚ÐµÐ»ÑŒÑÑ‚Ð²Ð¾Ð¼. ÐŸÐµÑ€ÐµÐ´ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð°Ñ†Ð¸ÐµÐ¹ Ð»ÑŽÐ±Ð¾Ð³Ð¾ UML edge Ð¸Ð»Ð¸ algorithm branch executor Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð¿Ð¾Ð»ÑƒÑ‡Ð¸Ñ‚ÑŒ Ð³Ð¾Ñ‚Ð¾Ð²Ñ‹Ð¹ executable artifact `artifacts/uml-executable-case-map/<work_order>.json`.

ÐšÐ°Ð¶Ð´Ð°Ñ ÑÑ‚Ñ€Ð¾ÐºÐ° artifact Ð´Ð¾Ð»Ð¶Ð½Ð° ÑÐ¾Ð´ÐµÑ€Ð¶Ð°Ñ‚ÑŒ:

| ÐŸÐ¾Ð»Ðµ | Ð¡Ð¼Ñ‹ÑÐ» |
|---|---|
| `uml_edge_or_algorithm_branch` | Ð¢Ð¾Ñ‡Ð½Ð°Ñ ÑÑ‚Ñ€ÐµÐ»ÐºÐ° UML Ð¸Ð»Ð¸ Ñ‚Ð¾Ñ‡Ð½Ð°Ñ Ð²ÐµÑ‚ÐºÐ° Ð°Ð»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼Ð° Ð¸Ð· Part A. |
| `input_fixture` | Redacted Ð²Ñ…Ð¾Ð´, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ð¼Ð¾Ð¶Ð½Ð¾ Ð¿Ð¾Ð´Ð°Ñ‚ÑŒ Ð² core machine, API Ð¸Ð»Ð¸ host entrypoint. |
| `expected_request_fields` | ÐšÐ°ÐºÐ¸Ðµ Ð¿Ð¾Ð»Ñ Ð´Ð¾Ð»Ð¶Ð½Ñ‹ Ð¿Ð¾ÑÐ²Ð¸Ñ‚ÑŒÑÑ Ð² request/DTO. |
| `expected_response_fields` | ÐšÐ°ÐºÐ¸Ðµ Ð¿Ð¾Ð»Ñ Ð´Ð¾Ð»Ð¶Ð½Ñ‹ Ð¿Ð¾ÑÐ²Ð¸Ñ‚ÑŒÑÑ Ð² response/report. |
| `expected_asset_touched` | ÐšÐ°ÐºÐ¸Ðµ model/data/adaptive artifacts Ð¼Ð¾Ð¶Ð½Ð¾ Ð¼ÐµÐ½ÑÑ‚ÑŒ. |
| `expected_asset_not_touched` | ÐšÐ°ÐºÐ¸Ðµ common/adaptive/host/private artifacts Ð¼ÐµÐ½ÑÑ‚ÑŒ Ð½ÐµÐ»ÑŒÐ·Ñ. |
| `expected_visible_chat_or_debug_output` | Ð§Ñ‚Ð¾ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ð¸Ð»Ð¸ Ð¿Ñ€Ð¾Ð³Ñ€Ð°Ð¼Ð¼Ð¸ÑÑ‚ ÑƒÐ²Ð¸Ð´Ð¸Ñ‚ Ð² Claude/Cursor/Codex, Ð²ÐºÐ»ÑŽÑ‡Ð°Ñ `xiigen <method> :`, `input:`, `output:` Ð¿Ñ€Ð¸ debug. |
| `negative_case` | Ð§Ñ‚Ð¾ Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ð¿Ñ€Ð¾Ð²Ð°Ð»Ð¸Ñ‚ÑŒÑÑ: wrong host-side logic, missing redaction, false trained claim, forbidden external tool, hidden deterministic shortcut. |

Parent `REVIEW_INTERNAL` Ð¾Ð±ÑÐ·Ð°Ð½ ÑÑ€Ð°Ð²Ð½Ð¸Ñ‚ÑŒ ÑÑ‚Ð¾Ñ‚ artifact Ñ UML Ð¸ Ñ€ÑƒÑÑÐºÐ¸Ð¼ Ð°Ð»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼Ð¾Ð¼ Ð¸Ð· Part A. Ð•ÑÐ»Ð¸ Ñ…Ð¾Ñ‚Ñ Ð±Ñ‹ Ð¾Ð´Ð½Ð° UML-ÑÑ‚Ñ€ÐµÐ»ÐºÐ° Ð¸Ð»Ð¸ algorithm branch Ð½Ðµ Ð¸Ð¼ÐµÐµÑ‚ executable case, work order Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ÑÑ Ð½Ð° repair Ð´Ð¾ Ð¸ÑÐ¿Ð¾Ð»Ð½ÐµÐ½Ð¸Ñ.

### A0.2 ÐšÐ°Ðº Ñ‡Ð¸Ñ‚Ð°Ñ‚ÑŒ ÑÑ‚Ñƒ Ð´Ð¸Ð°Ð³Ñ€Ð°Ð¼Ð¼Ñƒ Ñ‡ÐµÐ»Ð¾Ð²ÐµÑ‡ÐµÑÐºÐ¸Ð¼ ÑÐ·Ñ‹ÐºÐ¾Ð¼

Ð¡Ñ…ÐµÐ¼Ð° Ñ‡Ð¸Ñ‚Ð°ÐµÑ‚ÑÑ ÑÐ»ÐµÐ²Ð° Ð½Ð°Ð¿Ñ€Ð°Ð²Ð¾: Claude, Cursor Ð¸ Codex - ÑÑ‚Ð¾ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ð²Ñ…Ð¾Ð´Ð½Ñ‹Ðµ Ð´Ð²ÐµÑ€Ð¸. ÐžÐ½Ð¸ Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÑŽÑ‚ Ñ‚ÐµÐºÑÑ‚ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ, Ð½Ð°ÑÑ‚Ñ€Ð¾Ð¹ÐºÑƒ debug, ÑÐ¿Ð¸ÑÐ¾Ðº Ð´Ð¾ÑÑ‚ÑƒÐ¿Ð½Ñ‹Ñ… Ð½Ð°Ð²Ñ‹ÐºÐ¾Ð² Ð¸ ÑÐ»ÑƒÐ¶ÐµÐ±Ð½Ñ‹Ð¹ ÐºÐ¾Ð½Ñ‚ÐµÐºÑÑ‚, Ð¿Ð¾ÑÐ»Ðµ Ñ‡ÐµÐ³Ð¾ Ð¾Ñ‚Ð¿Ñ€Ð°Ð²Ð»ÑÑŽÑ‚ Ñ‚Ð¾Ð½ÐºÐ¸Ð¹ `XiigenExtensionRequest` Ð² XIIGen core. Ð’ host extension Ð½Ðµ Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ð±Ñ‹Ñ‚ÑŒ Ð»Ð¾Ð³Ð¸ÐºÐ¸ Ð²Ñ‹Ð±Ð¾Ñ€Ð° Ð¿Ð»Ð°Ð½Ð°, Ð²Ñ‹Ð±Ð¾Ñ€Ð° ÑÐºÐ¸Ð»Ð¾Ð², Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð¸Ñ, DPO Ð¸Ð»Ð¸ Ñ€Ð°Ð½Ð¶Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð¸Ñ.

`XiigenExtensionRequest` - ÑÑ‚Ð¾ ÐºÐ¾Ð½Ð²ÐµÑ€Ñ‚. Ð’ Ð½Ñ‘Ð¼ Ð»ÐµÐ¶Ð¸Ñ‚ Ñ‚Ð¾, Ñ‡Ñ‚Ð¾ Ð½Ð°Ð¿Ð¸ÑÐ°Ð» Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ, Ð¾Ñ‚ÐºÑƒÐ´Ð° Ð¿Ñ€Ð¸ÑˆÑ‘Ð» Ð·Ð°Ð¿Ñ€Ð¾Ñ, ÐºÐ°ÐºÐ¸Ðµ Ð½Ð°Ð²Ñ‹ÐºÐ¸ Ð²Ð¸Ð´Ð¸Ñ‚ host, Ð²ÐºÐ»ÑŽÑ‡Ñ‘Ð½ Ð»Ð¸ debug Ð¸ ÐºÐ°ÐºÐ¸Ðµ check-set inputs Ð½Ð°Ð´Ð¾ Ð¿ÐµÑ€ÐµÐ´Ð°Ñ‚ÑŒ. Ð”Ð¾ Ð¿Ð¾Ð¿Ð°Ð´Ð°Ð½Ð¸Ñ Ð² Ð¾Ð±ÑƒÑ‡Ð°ÑŽÑ‰Ð¸Ðµ Ð°Ñ€Ñ‚ÐµÑ„Ð°ÐºÑ‚Ñ‹ raw prompt Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ Ð·Ð°Ñ…ÑÑˆÐ¸Ñ€Ð¾Ð²Ð°Ð½ Ð¸Ð»Ð¸ Ð¾Ñ‚Ñ€ÐµÐ´Ð°ÐºÑ‚Ð¸Ñ€Ð¾Ð²Ð°Ð½: private Ñ‚ÐµÐºÑÑ‚ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ Ð½Ðµ Ð´Ð¾Ð»Ð¶ÐµÐ½ ÑÐ»ÑƒÑ‡Ð°Ð¹Ð½Ð¾ ÑÑ‚Ð°Ñ‚ÑŒ common data.

`XiigenExtensionApiService` - ÑÑ‚Ð¾ Ñ†ÐµÐ½Ñ‚Ñ€Ð°Ð»ÑŒÐ½Ñ‹Ð¹ Ð²Ñ…Ð¾Ð´ Ð² `llm_mvp_core`. ÐžÐ½ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÐµÑ‚ Ð·Ð°Ð¿Ñ€Ð¾Ñ, ÑƒÐ±Ð¸Ñ€Ð°ÐµÑ‚ Ð»Ð¸ÑˆÐ½ÐµÐµ/private, Ð²Ñ‹Ð·Ñ‹Ð²Ð°ÐµÑ‚ core-Ð¼Ð°ÑˆÐ¸Ð½Ñ‹ Ð¸ ÑÐ¾Ð±Ð¸Ñ€Ð°ÐµÑ‚ Ð¾Ñ‚Ð²ÐµÑ‚. Ð•ÑÐ»Ð¸ debug Ð²ÐºÐ»ÑŽÑ‡Ñ‘Ð½, Ð¾Ð½ Ñ‚Ð°ÐºÐ¶Ðµ ÑÐ¾Ð±Ð¸Ñ€Ð°ÐµÑ‚ Ð±ÐµÐ·Ð¾Ð¿Ð°ÑÐ½Ñ‹Ð¹ trace Ð²Ð¸Ð´Ð° `xiigen <method> :`, `input:`, `output:`, Ñ‡Ñ‚Ð¾Ð±Ñ‹ Ð¿Ñ€Ð¾Ð³Ñ€Ð°Ð¼Ð¼Ð¸ÑÑ‚ Ð²Ð¸Ð´ÐµÐ», Ñ‡Ñ‚Ð¾ Ð¸Ð¼ÐµÐ½Ð½Ð¾ ÑƒÑˆÐ»Ð¾ Ð² XIIGen Ð¸ Ñ‡Ñ‚Ð¾ Ð²ÐµÑ€Ð½ÑƒÐ»Ð¾ÑÑŒ Ð¾Ð±Ñ€Ð°Ñ‚Ð½Ð¾.

Ð”Ð°Ð»ÑŒÑˆÐµ Ð¸Ð´ÑƒÑ‚ Ñ‚Ñ€Ð¸ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ñ‹Ñ… ÑˆÐ°Ð³Ð°. `PromptToTaskMachine` Ð¿ÐµÑ€ÐµÐ²Ð¾Ð´Ð¸Ñ‚ Ñ‡ÐµÐ»Ð¾Ð²ÐµÑ‡ÐµÑÐºÐ¸Ð¹ prompt Ð² ÑÑ‚Ñ€ÑƒÐºÑ‚ÑƒÑ€Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð½ÑƒÑŽ Ð·Ð°Ð´Ð°Ñ‡Ñƒ. `OrchestrationReasoningMachine` Ð±ÐµÑ€Ñ‘Ñ‚ ÑÑ‚Ñƒ Ð·Ð°Ð´Ð°Ñ‡Ñƒ Ð¸ Ð½Ð°Ñ…Ð¾Ð´Ð¸Ñ‚ Ð¿Ð¾Ð´Ñ…Ð¾Ð´ÑÑ‰Ð¸Ð¹ ÑÑ†ÐµÐ½Ð°Ñ€Ð¸Ð¹ Ð¾Ñ€ÐºÐµÑÑ‚Ñ€Ð°Ñ†Ð¸Ð¸ Ð¿Ð¾ Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð½Ñ‹Ð¼ guide patterns. `SkillSelectionMachine` Ð±ÐµÑ€Ñ‘Ñ‚ Ð·Ð°Ð´Ð°Ñ‡Ñƒ Ð¸ ÑÑ†ÐµÐ½Ð°Ñ€Ð¸Ð¹ Ð¸ Ð²Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ Ð½ÑƒÐ¶Ð½Ñ‹Ðµ Ð½Ð°Ð²Ñ‹ÐºÐ¸. Ð­Ñ‚Ð¸ Ñ€ÐµÑˆÐµÐ½Ð¸Ñ Ð½Ðµ Ð´Ð¾Ð»Ð¶Ð½Ñ‹ Ð±Ñ‹Ñ‚ÑŒ regex, exact lookup, BM25 Ð¸Ð»Ð¸ Ñ‚Ð°Ð±Ð»Ð¸Ñ†ÐµÐ¹ `if/else`: Ð¾Ð½Ð¸ Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÑŽÑ‚ÑÑ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ trained state Ñ checkpoint, manifest, export/import, fresh-load, continue-training Ð¸ held-out evidence.

`XiigenExtensionResponse` Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ÑÑ Ð¾Ð±Ñ€Ð°Ñ‚Ð½Ð¾ Ð² host Ð¸ Ð¾Ñ‚Ð¾Ð±Ñ€Ð°Ð¶Ð°ÐµÑ‚ÑÑ Ð² Ñ‡Ð°Ñ‚Ðµ Ð¸Ð»Ð¸ UI. Ð’ Ð½Ñ‘Ð¼ Ð´Ð¾Ð»Ð¶Ð½Ñ‹ Ð±Ñ‹Ñ‚ÑŒ Ð½Ðµ Ð²Ð½ÑƒÑ‚Ñ€ÐµÐ½Ð½Ð¾ÑÑ‚Ð¸ Ð¼Ð¾Ð´ÐµÐ»Ð¸, Ð° Ð¿Ð¾Ð½ÑÑ‚Ð½Ñ‹Ðµ Ñ€ÐµÐ·ÑƒÐ»ÑŒÑ‚Ð°Ñ‚Ñ‹: Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ðµ Ð½Ð°Ð²Ñ‹ÐºÐ¸, ÑÑÑ‹Ð»ÐºÐ¸ Ð½Ð° ÑÑ‚Ñ€ÑƒÐºÑ‚ÑƒÑ€Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð½ÑƒÑŽ Ð·Ð°Ð´Ð°Ñ‡Ñƒ, ÑÑÑ‹Ð»ÐºÐ¸ Ð½Ð° ÑÑ†ÐµÐ½Ð°Ñ€Ð¸Ð¹, Ð¿Ñ€ÐµÐ´ÑƒÐ¿Ñ€ÐµÐ¶Ð´ÐµÐ½Ð¸Ñ/Ð¾ÑˆÐ¸Ð±ÐºÐ¸ Ð¸, ÐµÑÐ»Ð¸ Ð²ÐºÐ»ÑŽÑ‡Ñ‘Ð½ debug, Ð±ÐµÐ·Ð¾Ð¿Ð°ÑÐ½Ñ‹Ð¹ trace.

ÐŸÐ¾ÑÐ»Ðµ Ð¾Ñ‚Ð²ÐµÑ‚Ð° Ð²Ð½ÐµÑˆÐ½ÐµÐ³Ð¾ AI Ð²ÐºÐ»ÑŽÑ‡Ð°ÐµÑ‚ÑÑ learning loop. `ResultGapMachine` Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÐµÑ‚, Ð½Ðµ Ð¿Ñ€Ð¾Ð¿ÑƒÑÑ‚Ð¸Ð»Ð° Ð»Ð¸ ÑÐ¸ÑÑ‚ÐµÐ¼Ð° Ð²Ð°Ð¶Ð½Ñ‹Ð¹ Ð¿Ð°Ñ‚Ñ‚ÐµÑ€Ð½. Ð•ÑÐ»Ð¸ Ð¿Ð°Ñ‚Ñ‚ÐµÑ€Ð½ Ð±Ñ‹Ð» Ð¿Ñ€Ð¾Ð¿ÑƒÑ‰ÐµÐ½, Ð¾Ð½ Ð¿Ð¸ÑˆÐµÑ‚ adaptive DPO row Ð² user side `.xiigen`. Ð•ÑÐ»Ð¸ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ð¸ÑÐ¿Ñ€Ð°Ð²Ð¸Ð» Ñ€ÐµÐ·ÑƒÐ»ÑŒÑ‚Ð°Ñ‚ Ð²Ñ€ÑƒÑ‡Ð½ÑƒÑŽ, `SkillSelectionFeedbackWriter` Ñ‚Ð¾Ð¶Ðµ Ð¿Ð¸ÑˆÐµÑ‚ correction DPO row Ð² user side `.xiigen`. Ð­Ñ‚Ð¾ ÑƒÐ»ÑƒÑ‡ÑˆÐ°ÐµÑ‚ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÑƒÑŽ Ð¼Ð¾Ð´ÐµÐ»ÑŒ, Ð½Ð¾ Ð½Ðµ Ð¼ÑƒÑ‚Ð¸Ñ€ÑƒÐµÑ‚ Ð½Ð°ÑˆÑƒ common library.

Ð£ ÐºÐ°Ð¶Ð´Ð¾Ð¹ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ð¹ ÑÑ„ÐµÑ€Ñ‹ ÐµÑÑ‚ÑŒ Ð´Ð²Ðµ Ð½Ð¾Ð³Ð¸. Common/global Ð½Ð¾Ð³Ð° Ð¶Ð¸Ð²Ñ‘Ñ‚ Ñƒ Ð½Ð°Ñ Ð² `llm_mvp_core`: common training sets, common checkpoints, manifests, locators Ð¸ upgrades. Personal/adaptive Ð½Ð¾Ð³Ð° Ð¶Ð¸Ð²Ñ‘Ñ‚ Ñƒ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ: adaptive checkpoints, user DPO signals, preferences, feedback, backup packages Ð¸ restore/import packages. Ð­Ñ‚Ð¸ Ð´Ð²Ðµ Ð½Ð¾Ð³Ð¸ ÑÐ²ÑÐ·Ñ‹Ð²Ð°ÐµÑ‚ `ILearningMachinePair`: common upgrade Ð¼Ð¾Ð¶ÐµÑ‚ Ð¾Ð±Ð½Ð¾Ð²Ð¸Ñ‚ÑŒ Ð½Ð°ÑˆÑƒ Ð¾Ð±Ñ‰ÑƒÑŽ Ð¼Ð¾Ð´ÐµÐ»ÑŒ, Ð½Ð¾ Ð½Ðµ Ð¸Ð¼ÐµÐµÑ‚ Ð¿Ñ€Ð°Ð²Ð° Ð¿ÐµÑ€ÐµÑ‚ÐµÑ€ÐµÑ‚ÑŒ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÑƒÑŽ adaptive-Ð¼Ð¾Ð´ÐµÐ»ÑŒ.

Backup/share/restore - ÑÑ‚Ð¾ Ð¾Ñ‚Ð´ÐµÐ»ÑŒÐ½Ð¾Ðµ Ñ‚Ñ€ÐµÐ±Ð¾Ð²Ð°Ð½Ð¸Ðµ, Ð° Ð½Ðµ ÑƒÐºÑ€Ð°ÑˆÐµÐ½Ð¸Ðµ. ÐŸÐ¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ°Ñ Ð¼Ð¾Ð´ÐµÐ»ÑŒ Ð´Ð¾Ð»Ð¶Ð½Ð° ÑƒÐ¼ÐµÑ‚ÑŒ ÑÐºÑÐ¿Ð¾Ñ€Ñ‚Ð¸Ñ€Ð¾Ð²Ð°Ñ‚ÑŒÑÑ Ð² Ð¿ÐµÑ€ÐµÐ½Ð¾ÑÐ¸Ð¼Ñ‹Ð¹ Ð¿Ð°ÐºÐµÑ‚, Ð²Ð¾ÑÑÑ‚Ð°Ð½Ð°Ð²Ð»Ð¸Ð²Ð°Ñ‚ÑŒÑÑ Ð² fresh profile/device path Ð¸ Ð±ÐµÐ·Ð¾Ð¿Ð°ÑÐ½Ð¾ ÐºÐ¾Ð¿Ð¸Ñ€Ð¾Ð²Ð°Ñ‚ÑŒÑÑ Ð¸Ð»Ð¸ ÑˆÐ°Ñ€Ð¸Ñ‚ÑŒÑÑ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ ÐºÐ°Ðº user-approved/anonymised package. ÐŸÑ€Ð¾ÑÑ‚Ð°Ñ Ð¿Ð°Ð¿ÐºÐ° `.xiigen` Ð±ÐµÐ· manifest, checkpoint hash, redaction report Ð¸ restore/import probe Ð½Ðµ ÑÑ‡Ð¸Ñ‚Ð°ÐµÑ‚ÑÑ Ð²Ñ‹Ð¿Ð¾Ð»Ð½ÐµÐ½Ð¸ÐµÐ¼ Ñ‚Ñ€ÐµÐ±Ð¾Ð²Ð°Ð½Ð¸Ñ.

## A1. ÐÐ»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼Ñ‹ Ð¿Ñ€Ð¾ÑÑ‚Ñ‹Ð¼ Ñ€ÑƒÑÑÐºÐ¸Ð¼ ÑÐ·Ñ‹ÐºÐ¾Ð¼

ÐšÐ°Ð¶Ð´Ñ‹Ð¹ Ð°Ð»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼ Ð½Ð¸Ð¶Ðµ Ð¾Ð¿Ð¸ÑÐ°Ð½ ÐºÐ°Ðº Ð¿Ð°ÑÐ¿Ð¾Ñ€Ñ‚ Ñ‚ÐµÐºÑƒÑ‰ÐµÐ¹ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð°Ñ†Ð¸Ð¸: ÐºÐ°ÐºÐ°Ñ Ñ„ÑƒÐ½ÐºÑ†Ð¸Ñ Ð²Ñ‹Ð·Ñ‹Ð²Ð°ÐµÑ‚ÑÑ, ÐºÐ°ÐºÐ¾Ð¹ Ð²Ñ…Ð¾Ð´ Ð¾Ð½Ð° Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÐµÑ‚, ÐºÐ°ÐºÐ¾Ð¹ Ð²Ñ‹Ñ…Ð¾Ð´ Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚, Ñ‡Ñ‚Ð¾ Ð¾Ð½Ð° Ð´ÐµÐ»Ð°ÐµÑ‚ ÑÐµÐ¹Ñ‡Ð°Ñ Ð¿Ð¾ ÑˆÐ°Ð³Ð°Ð¼, Ð¸ Ð³Ð´Ðµ Ñ‚ÐµÐºÑƒÑ‰Ð°Ñ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð°Ñ†Ð¸Ñ ÐµÑ‰Ñ‘ ÑÐ²Ð»ÑÐµÑ‚ÑÑ skeleton/bootstrap/partial Ð²Ð¼ÐµÑÑ‚Ð¾ Ð¿Ð¾Ð»Ð½Ð¾Ñ†ÐµÐ½Ð½Ð¾Ð¹ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ð¹ Ð¼Ð¾Ð´ÐµÐ»Ð¸.

### A1.1 ÐŸÐ¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ¸Ð¹ Ð·Ð°Ð¿Ñ€Ð¾Ñ -> ÑÑ‚Ñ€ÑƒÐºÑ‚ÑƒÑ€Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð½Ð°Ñ Ð·Ð°Ð´Ð°Ñ‡Ð°

ÐšÐ»Ð°ÑÑ: `SmartSkillsDomain.PromptToTaskMachine`.

Ð¤ÑƒÐ½ÐºÑ†Ð¸Ñ: `PromptToTaskMachine.Translate(PromptToTaskRequest request)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\PromptToTaskMachine.cs:36`, quote: `public OperationResult<PromptToTaskResponse> Translate(PromptToTaskRequest request)`).

Ð’Ñ…Ð¾Ð´: `PromptToTaskRequest` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\PromptToTaskContracts.cs:9`) ÑÐ¾Ð´ÐµÑ€Ð¶Ð¸Ñ‚ `PromptId`, `RawPrompt`, `RawPromptSha256`, `PromptProvenance`, `UserLanguage`, `ExplicitConstraints`, `ContextFields`. API ÑÐµÐ¹Ñ‡Ð°Ñ ÑÑ‚Ñ€Ð¾Ð¸Ñ‚ ÐµÐ³Ð¾ Ð² `BuildPromptToTaskRequest(...)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs:241`) Ð¸ Ð¿ÐµÑ€ÐµÐ´Ð°Ñ‘Ñ‚ redacted prompt: raw text Ð½Ðµ Ð¸Ð´Ñ‘Ñ‚ Ð´Ð°Ð»ÑŒÑˆÐµ, Ð¾ÑÑ‚Ð°Ñ‘Ñ‚ÑÑ hash Ð¸ context fields.

Ð’Ñ‹Ñ…Ð¾Ð´: `PromptToTaskResponse` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\PromptToTaskContracts.cs:107`) Ñ `StructuredTaskContract`, evidence Ð¸ status.

ÐÐ»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼ ÑÐµÐ¹Ñ‡Ð°Ñ:
1. ÐŸÑ€Ð¾Ð±ÑƒÐµÑ‚ Ð·Ð°Ð³Ñ€ÑƒÐ·Ð¸Ñ‚ÑŒ learned-state descriptor Ñ‡ÐµÑ€ÐµÐ· `TryLoadLearnedState()`.
2. Ð¡Ð¾Ð·Ð´Ð°Ñ‘Ñ‚ `StructuredTaskContract`, Ð½Ð¾ ÑÑ‚Ð°Ð²Ð¸Ñ‚ `TaskType = not_ready_no_semantic_task`.
3. ÐŸÐµÑ€ÐµÐ½Ð¾ÑÐ¸Ñ‚ Ð±ÐµÐ·Ð¾Ð¿Ð°ÑÐ½Ñ‹Ðµ Ð¿Ð¾Ð»Ñ: prompt id, raw prompt hash, language, explicit constraints, provenance.
4. Ð’ `EvidenceNeeds` Ð¿Ð¸ÑˆÐµÑ‚, Ñ‡Ñ‚Ð¾ semantic inference ÐµÑ‰Ñ‘ Ð½Ðµ ÑÐ´ÐµÐ»Ð°Ð½.
5. Ð’Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ `PromptToTaskResponse` ÑÐ¾ status `not_trained_no_loaded_state` Ð¸Ð»Ð¸ `loaded_state_ready_no_inference_implemented`.

ÐšÐ°Ðº Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ñ€Ð°Ð±Ð¾Ñ‚Ð°Ñ‚ÑŒ: Ð¼Ð°ÑˆÐ¸Ð½Ð° Ð¿Ð¾Ð»ÑƒÑ‡Ð°ÐµÑ‚ ÑÑ‹Ñ€Ð¾Ð¹ prompt Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ Ð¸ Ð¿ÐµÑ€ÐµÐ²Ð¾Ð´Ð¸Ñ‚ ÐµÐ³Ð¾ Ð² Ð·Ð°Ð´Ð°Ñ‡Ñƒ: Ñ‡Ñ‚Ð¾ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ñ…Ð¾Ñ‡ÐµÑ‚ ÑÐ´ÐµÐ»Ð°Ñ‚ÑŒ, ÐºÐ°ÐºÐ¸Ðµ ÐµÑÑ‚ÑŒ constraints, deliverables, forbidden actions, target hosts Ð¸ evidence needs. Ð­Ñ‚Ð¾ Ð´Ð¾Ð»Ð¶Ð½Ð° Ð±Ñ‹Ñ‚ÑŒ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ Ð¼Ð°ÑˆÐ¸Ð½Ð°, Ð° Ð½Ðµ regex-classifier Ð¸ Ð½Ðµ Ñ‚Ð°Ð±Ð»Ð¸Ñ†Ð° prompt keywords.

ÐšÐ°Ðº Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½Ð¾ ÑÐµÐ¹Ñ‡Ð°Ñ: ÑÑ‚Ð¾ skeleton. ÐšÐ¾Ð´ ÑÐ¾Ñ…Ñ€Ð°Ð½ÑÐµÑ‚ hash prompt Ð¸ constraints, Ð½Ð¾ Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ placeholder `not_ready_no_semantic_task`. Semantic inference ÐµÑ‰Ñ‘ Ð½Ðµ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½.

Ð§Ñ‚Ð¾ Ð½ÐµÐ»ÑŒÐ·Ñ Ð´ÐµÐ»Ð°Ñ‚ÑŒ: Ð½ÐµÐ»ÑŒÐ·Ñ Ñ€ÐµÑˆÐ°Ñ‚ÑŒ task type Ñ‡ÐµÑ€ÐµÐ· `if prompt contains ...`, Ð½ÐµÐ»ÑŒÐ·Ñ ÑÐºÐ»Ð°Ð´Ñ‹Ð²Ð°Ñ‚ÑŒ private raw prompts Ð² common repo artifacts.

### A1.2 Ð¡Ñ‚Ñ€ÑƒÐºÑ‚ÑƒÑ€Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð½Ð°Ñ Ð·Ð°Ð´Ð°Ñ‡Ð° -> Ð¿Ð»Ð°Ð½ Ð¾Ñ€ÐºÐµÑÑ‚Ñ€Ð°Ñ†Ð¸Ð¸

ÐšÐ»Ð°ÑÑ: `SmartSkillsDomain.OrchestrationReasoningMachine`.

Ð¤ÑƒÐ½ÐºÑ†Ð¸Ñ: `OrchestrationReasoningMachine.Plan(OrchestrationReasoningRequest request)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\OrchestrationReasoningMachine.cs:37`, quote: `public OperationResult<OrchestrationReasoningResponse> Plan(OrchestrationReasoningRequest request)`).

Ð’Ñ…Ð¾Ð´: `OrchestrationReasoningRequest` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\OrchestrationReasoningContracts.cs:9`) ÑÐ¾Ð´ÐµÑ€Ð¶Ð¸Ñ‚ `RequestId`, `Task`, `GuideDerivedStateRefs`, `CorpusBundleIds`, `AdaptiveContextRefs`, `RuntimePolicyRefs`. API ÑÑ‚Ñ€Ð¾Ð¸Ñ‚ ÐµÐ³Ð¾ Ð² `BuildOrchestrationRequest(...)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs:258`).

Ð’Ñ‹Ñ…Ð¾Ð´: `OrchestrationReasoningResponse` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\OrchestrationReasoningContracts.cs:103`) Ñ `OrchestrationScenarioPlan`, evidence Ð¸ status.

ÐÐ»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼ ÑÐµÐ¹Ñ‡Ð°Ñ:
1. ÐŸÑ€Ð¾Ð±ÑƒÐµÑ‚ Ð·Ð°Ð³Ñ€ÑƒÐ·Ð¸Ñ‚ÑŒ learned guide-state descriptor Ñ‡ÐµÑ€ÐµÐ· `TryLoadLearnedGuideState()`.
2. Ð¡Ð¾Ð·Ð´Ð°Ñ‘Ñ‚ `OrchestrationScenarioPlan` Ñ `ScenarioId = not-ready-no-orchestration-inference`.
3. ÐÐµ Ð²Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ learned patterns, phase families, gate families, assets Ð¸Ð»Ð¸ host check-sets.
4. Ð—Ð°Ð¿Ð¾Ð»Ð½ÑÐµÑ‚ risk flags Ð¸ next actions, Ñ‡Ñ‚Ð¾Ð±Ñ‹ Ñ‡ÐµÑÑ‚Ð½Ð¾ Ð¿Ð¾ÐºÐ°Ð·Ð°Ñ‚ÑŒ, Ñ‡Ñ‚Ð¾ inference ÐµÑ‰Ñ‘ Ð½Ðµ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½.
5. Ð’Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ status `not_trained_no_learned_guide_state` Ð¸Ð»Ð¸ `loaded_guide_state_ready_no_inference_implemented`.

ÐšÐ°Ðº Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ñ€Ð°Ð±Ð¾Ñ‚Ð°Ñ‚ÑŒ: Ð¼Ð°ÑˆÐ¸Ð½Ð° Ð¿Ð¾Ð»ÑƒÑ‡Ð°ÐµÑ‚ `StructuredTaskContract`, ÑÐ¾Ð¿Ð¾ÑÑ‚Ð°Ð²Ð»ÑÐµÑ‚ ÐµÐ³Ð¾ Ñ Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð½Ñ‹Ð¼Ð¸ guide-derived patterns Ð¸ Ð²Ñ‹Ð´Ð°Ñ‘Ñ‚ scenario plan: ÐºÐ°ÐºÐ¸Ðµ phase families, gates, assets, host check-sets, risks Ð¸ granular work orders Ð½ÑƒÐ¶Ð½Ñ‹.

ÐšÐ°Ðº Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½Ð¾ ÑÐµÐ¹Ñ‡Ð°Ñ: ÑÑ‚Ð¾ skeleton. ÐšÐ¾Ð´ Ð¼Ð¾Ð¶ÐµÑ‚ Ð·Ð°Ð³Ñ€ÑƒÐ·Ð¸Ñ‚ÑŒ descriptor learned guide state, Ð½Ð¾ semantic orchestration plan ÐµÑ‰Ñ‘ Ð½Ðµ Ð³ÐµÐ½ÐµÑ€Ð¸Ñ€ÑƒÐµÑ‚ÑÑ. Ð’Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ÑÑ scenario id `not-ready-no-orchestration-inference`.

Ð§Ñ‚Ð¾ Ð½ÐµÐ»ÑŒÐ·Ñ Ð´ÐµÐ»Ð°Ñ‚ÑŒ: Ð½ÐµÐ»ÑŒÐ·Ñ Ð²Ñ‹Ð±Ð¸Ñ€Ð°Ñ‚ÑŒ Ð¿Ð»Ð°Ð½ Ð¿Ð¾ Ð¸Ð¼ÐµÐ½Ð¸ Ñ„Ð°Ð¹Ð»Ð°, BM25-like search, Ð·Ð°Ð³Ð¾Ð»Ð¾Ð²ÐºÑƒ Ð³Ð°Ð¹Ð´Ð° Ð¸Ð»Ð¸ static route table. Ð¢Ð°ÐºÐ¸Ðµ Ð²ÐµÑ‰Ð¸ Ð¼Ð¾Ð³ÑƒÑ‚ Ð±Ñ‹Ñ‚ÑŒ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ corpus/search infrastructure. Runtime planning decision Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð¸Ð´Ñ‚Ð¸ Ð¸Ð· trained state Ñ checkpoint, manifest, export/import, fresh-load, continue-training Ð¸ ablation evidence.

### A1.3 Ð—Ð°Ð´Ð°Ñ‡Ð° -> Ð½ÑƒÐ¶Ð½Ñ‹Ðµ ÑÐºÐ¸Ð»Ñ‹

ÐšÐ»Ð°ÑÑ: `SmartSkillsDomain.SkillSelectionMachine`.

Ð¤ÑƒÐ½ÐºÑ†Ð¸Ñ Ð²Ñ‹Ð±Ð¾Ñ€Ð°: `SkillSelectionMachine.DecideFromTrainedState(SkillSelectionRequest request)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:57`, quote: `public OperationResult<SkillSelectionDecision> DecideFromTrainedState(SkillSelectionRequest request)`).

Ð¤ÑƒÐ½ÐºÑ†Ð¸Ñ Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð¸Ñ Ñ‚ÐµÐºÑƒÑ‰ÐµÐ³Ð¾ partial state: `TrainMachineBatchAsync(IReadOnlyList<SkillSelectionMachinePair> pairs, CancellationToken ct = default)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:41`). Ð’Ð½ÑƒÑ‚Ñ€Ð¸ Ð¾Ð½Ð° Ð²Ñ‹Ð·Ñ‹Ð²Ð°ÐµÑ‚ `LearnPair(SkillSelectionMachinePair pair)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:286`).

Ð’Ñ…Ð¾Ð´ Ð²Ñ‹Ð±Ð¾Ñ€Ð°: `SkillSelectionRequest` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:6`) ÑÐ¾Ð´ÐµÑ€Ð¶Ð¸Ñ‚ prompt/hash, project root, `AvailableSkillIds`, host facts Ð¸ source ledger id. API ÑÑ‚Ñ€Ð¾Ð¸Ñ‚ request Ð² `DecideSkills(...)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs:203`).

Ð’Ñ‹Ñ…Ð¾Ð´ Ð²Ñ‹Ð±Ð¾Ñ€Ð°: `SkillSelectionDecision` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:13`) Ñ selected skills, rejected skills, evidence features, confidence, second-pass flag Ð¸ Ñ‚ÐµÐºÑÑ‚Ð¾Ð¼ Ñ€ÐµÑˆÐµÐ½Ð¸Ñ.

ÐÐ»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼ ÑÐµÐ¹Ñ‡Ð°Ñ:
1. Ð•ÑÐ»Ð¸ `_candidatePositionWeights` Ð¿ÑƒÑÑ‚Ð¾Ð¹ Ð¸Ð»Ð¸ host Ð½Ðµ Ð´Ð°Ð» `AvailableSkillIds`, Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ no trained state Ð¸ Ð¿Ñ€Ð¾ÑÐ¸Ñ‚ second pass.
2. Ð”Ð»Ñ ÐºÐ°Ð¶Ð´Ð¾Ð³Ð¾ Ð´Ð¾ÑÑ‚ÑƒÐ¿Ð½Ð¾Ð³Ð¾ skill Ð±ÐµÑ€Ñ‘Ñ‚ ÐµÐ³Ð¾ Ð¿Ð¾Ð·Ð¸Ñ†Ð¸ÑŽ Ð² ÑÐ¿Ð¸ÑÐºÐµ host-Ð°.
3. Ð¡Ð¼Ð¾Ñ‚Ñ€Ð¸Ñ‚ Ð²ÐµÑ ÑÑ‚Ð¾Ð¹ Ð¿Ð¾Ð·Ð¸Ñ†Ð¸Ð¸ Ð² `_candidatePositionWeights`.
4. Ð’Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ skill Ñ Ð¼Ð°ÐºÑÐ¸Ð¼Ð°Ð»ÑŒÐ½Ñ‹Ð¼ Ð²ÐµÑÐ¾Ð¼, Ð¿Ñ€Ð¸ Ñ€Ð°Ð²ÐµÐ½ÑÑ‚Ð²Ðµ - Ð±Ð¾Ð»ÐµÐµ Ñ€Ð°Ð½Ð½ÑŽÑŽ Ð¿Ð¾Ð·Ð¸Ñ†Ð¸ÑŽ.
5. Evidence Ð¿Ð¸ÑˆÐµÑ‚ `TRAINED_CANDIDATE_POSITION_WEIGHT`, `candidate_index`, `candidate_weight`.
6. ÐžÐ±ÑƒÑ‡ÐµÐ½Ð¸Ðµ `LearnPair` ÑƒÐ²ÐµÐ»Ð¸Ñ‡Ð¸Ð²Ð°ÐµÑ‚ `_candidatePositionWeights[index]` Ð´Ð»Ñ Ð¿Ð¾Ð·Ð¸Ñ†Ð¸Ð¸ chosen skill-Ð° Ð² DPO row. Ð­Ñ‚Ð¾ Ð¿Ð¾Ð·Ð¸Ñ†Ð¸Ð¾Ð½Ð½Ñ‹Ð¹ Ñ‡Ð°ÑÑ‚Ð¾Ñ‚Ð½Ñ‹Ð¹ ÑÑ‡Ñ‘Ñ‚Ñ‡Ð¸Ðº, Ð½Ðµ semantic selector.

ÐšÐ°Ðº Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ñ€Ð°Ð±Ð¾Ñ‚Ð°Ñ‚ÑŒ: Ð¼Ð°ÑˆÐ¸Ð½Ð° Ð²Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ ÑÐºÐ¸Ð»Ñ‹ Ñ‡ÐµÑ€ÐµÐ· trained state. Ð’ Ñ„Ð¸Ð½Ð°Ð»ÑŒÐ½Ð¾Ð¹ Ð²ÐµÑ€ÑÐ¸Ð¸ Ð¾Ð½Ð° Ð´Ð¾Ð»Ð¶Ð½Ð° Ð¸ÑÐ¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÑŒ 11 trainable semantic units Ð¸Ð· `SmartSkillsSemanticUnitCatalog`: prompt understanding, task understanding, task type, routing, skill candidates, skill selection, prompt context, result gap, output validation, task completion, intermediate checks.

ÐšÐ°Ðº Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½Ð¾ ÑÐµÐ¹Ñ‡Ð°Ñ: ÐµÑÑ‚ÑŒ partial trained behavior. ÐœÐ°ÑˆÐ¸Ð½Ð° ÑƒÑ‡Ð¸Ñ‚ candidate-position weights Ð¸Ð· DPO rows Ð¸ ÑƒÐ¼ÐµÐµÑ‚ save/load checkpoint. **Ð’Ð°Ð¶Ð½Ð¾ (Ñ‡ÐµÑÑ‚Ð½Ð¾): Ñ‚ÐµÐºÑƒÑ‰ÐµÐµ Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð¸Ðµ â€” ÑÑ‚Ð¾ Ð¿Ð¾Ð·Ð¸Ñ†Ð¸Ð¾Ð½Ð½Ñ‹Ð¹ Ñ‡Ð°ÑÑ‚Ð¾Ñ‚Ð½Ñ‹Ð¹ ÑÑ‡Ñ‘Ñ‚Ñ‡Ð¸Ðº (`_candidatePositionWeights[index]++` Ð·Ð° Ð¿Ð¾Ð·Ð¸Ñ†Ð¸ÑŽ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ð¾Ð³Ð¾ Ð½Ð°Ð²Ñ‹ÐºÐ°), Ð° ÐÐ• ÑÐµÐ¼Ð°Ð½Ñ‚Ð¸Ñ‡ÐµÑÐºÐ¾Ðµ Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð¸Ðµ. Ð­Ñ‚Ð¾ ÑÐ¾Ð³Ð»Ð°ÑÑƒÐµÑ‚ÑÑ Ñ Ñ‚ÐµÐ¼, Ñ‡Ñ‚Ð¾ `AccuracyAtExport = 0.0`, Ð° `EvaluateAsync` Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ `PassesGate=false`.** candidate position ÑÐ»Ð¸ÑˆÐºÐ¾Ð¼ ÑÐ»Ð°Ð±Ñ‹Ð¹ ÑÐ¸Ð³Ð½Ð°Ð» Ð´Ð»Ñ Ð¿Ð¾Ð»Ð½Ð¾Ð¹ semantic skill selection.

Ð§Ñ‚Ð¾ Ð½ÐµÐ»ÑŒÐ·Ñ Ð´ÐµÐ»Ð°Ñ‚ÑŒ: Ð½ÐµÐ»ÑŒÐ·Ñ Ð¸ÑÐ¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÑŒ BM25, regex, hardcoded skill ids, host-side ranking Ð¸Ð»Ð¸ position weight ÐºÐ°Ðº Ñ„Ð¸Ð½Ð°Ð»ÑŒÐ½ÑƒÑŽ semantic authority. Candidate-position weight Ð¼Ð¾Ð¶Ð½Ð¾ Ð¾ÑÑ‚Ð°Ð²Ð¸Ñ‚ÑŒ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ ÐºÐ°Ðº Ð²Ñ€ÐµÐ¼ÐµÐ½Ð½Ñ‹Ð¹ scaffold Ñ Ñ‡ÐµÑÑ‚Ð½Ð¾Ð¹ Ð¿Ð¾Ð¼ÐµÑ‚ÐºÐ¾Ð¹ partial.

### A1.4 ÐŸÐ¾ÑÐ»Ðµ Ð¾Ñ‚Ð²ÐµÑ‚Ð° AI Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½ Ð¿Ð°Ñ‚Ñ‚ÐµÑ€Ð½ -> DPO + Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð¸Ðµ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ¾Ð¹ Ð¼Ð¾Ð´ÐµÐ»Ð¸

ÐšÐ»Ð°ÑÑÑ‹: `SmartSkillsDomain.ResultGapMachine`, Ð±ÑƒÐ´ÑƒÑ‰Ð¸Ð¹ trainable `ResultGapMachine`, adaptive training adapter.

Ð¤ÑƒÐ½ÐºÑ†Ð¸Ñ: `ResultGapMachine.Evaluate(ResultGapRequest request)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:173`, quote: `public ResultGapResult Evaluate(ResultGapRequest request)`).

Ð’Ñ…Ð¾Ð´: `ResultGapRequest` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:127`) ÑÐ¾Ð´ÐµÑ€Ð¶Ð¸Ñ‚ prompt, selected skills, AI output, expected/observed missing skill id Ð¸ source prompt ledger id.

Ð’Ñ‹Ñ…Ð¾Ð´: `ResultGapResult` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:151`) Ñ Ñ„Ð»Ð°Ð³Ð¾Ð¼ gap, missing skill id, second-pass update, DPO row, evidence features Ð¸ evidence mode.

ÐÐ»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼ ÑÐµÐ¹Ñ‡Ð°Ñ:
1. Ð‘ÐµÑ€Ñ‘Ñ‚ `ObservedMissingSkillId`, Ð° ÐµÑÐ»Ð¸ Ð¾Ð½ Ð¿ÑƒÑÑ‚Ð¾Ð¹ - `ExpectedMissingSkillId`.
2. Ð¡Ñ‡Ð¸Ñ‚Ð°ÐµÑ‚ gap Ð½Ð°Ð¹Ð´ÐµÐ½Ð½Ñ‹Ð¼, ÐµÑÐ»Ð¸ missing skill Ð½Ðµ Ð¿ÑƒÑÑ‚Ð¾Ð¹ Ð¸ ÐµÐ³Ð¾ Ð½ÐµÑ‚ Ð² `SelectedSkillIds`.
3. Ð•ÑÐ»Ð¸ gap Ð½Ðµ Ð½Ð°Ð¹Ð´ÐµÐ½, Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ `NO_SKILL_GAP_DETECTED`.
4. Ð•ÑÐ»Ð¸ gap Ð½Ð°Ð¹Ð´ÐµÐ½, Ð´Ð¾Ð±Ð°Ð²Ð»ÑÐµÑ‚ missing skill Ðº required skills.
5. Ð¡Ð¾Ð·Ð´Ð°Ñ‘Ñ‚ `SecondPassContextUpdate`, Ñ‡Ñ‚Ð¾Ð±Ñ‹ ÑÐ»ÐµÐ´ÑƒÑŽÑ‰Ð¸Ð¹ Ð¿Ñ€Ð¾Ñ…Ð¾Ð´ Ð½Ðµ Ð¸Ð³Ð½Ð¾Ñ€Ð¸Ñ€Ð¾Ð²Ð°Ð» missing skill.
6. Ð¡Ð¾Ð·Ð´Ð°Ñ‘Ñ‚ `ResultGapDpoRow`: chosen = selected + missing skill, rejected = ÑÑ‚Ð°Ñ€Ñ‹Ðµ selected skills.
7. Evidence mode Ñ‡ÐµÑÑ‚Ð½Ð¾ Ð¿Ð¸ÑˆÐµÑ‚ `bootstrap_test_harness_not_semantic_authority`.

ÐšÐ°Ðº Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ñ€Ð°Ð±Ð¾Ñ‚Ð°Ñ‚ÑŒ: Ð¿Ð¾ÑÐ»Ðµ Ð¾Ñ‚Ð²ÐµÑ‚Ð° Claude/Cursor/Codex XIIGen Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÐµÑ‚, Ñ…Ð²Ð°Ñ‚Ð¸Ð»Ð¾ Ð»Ð¸ Ð²Ñ‹Ð±Ñ€Ð°Ð½Ð½Ñ‹Ñ… skills Ð¸ orchestration. Ð•ÑÐ»Ð¸ Ð½ÑƒÐ¶Ð½Ñ‹Ð¹ skill/pattern Ð¾Ñ‚ÑÑƒÑ‚ÑÑ‚Ð²Ð¾Ð²Ð°Ð», ÑÐ¸ÑÑ‚ÐµÐ¼Ð° ÑÐ¾Ð·Ð´Ð°Ñ‘Ñ‚ DPO: chosen = corrected skill/task/plan set, rejected = original insufficient set. Ð¡Ð¸Ð³Ð½Ð°Ð» ÑÐ½Ð°Ñ‡Ð°Ð»Ð° Ð¸Ð´Ñ‘Ñ‚ Ð² user adaptive model. Ð•ÑÐ»Ð¸ Ð¿Ð°Ñ‚Ñ‚ÐµÑ€Ð½ Ð¾Ð±Ñ‰Ð¸Ð¹ Ð¸ approved, Ð¾Ð½ Ð¼Ð¾Ð¶ÐµÑ‚ Ð¿Ð¾Ð·Ð¶Ðµ ÑÑ‚Ð°Ñ‚ÑŒ common training candidate.

ÐšÐ°Ðº Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½Ð¾ ÑÐµÐ¹Ñ‡Ð°Ñ: ÐµÑÑ‚ÑŒ bootstrap fixture logic, ÐºÐ¾Ñ‚Ð¾Ñ€Ð°Ñ ÑÐ¾Ð·Ð´Ð°Ñ‘Ñ‚ `ResultGapDpoRow`, ÐºÐ¾Ð³Ð´Ð° expected skill Ð¾Ñ‚ÑÑƒÑ‚ÑÑ‚Ð²ÑƒÐµÑ‚. ÐšÐ¾Ð´ Ð¿Ñ€ÑÐ¼Ð¾ Ð¼Ð°Ñ€ÐºÐ¸Ñ€ÑƒÐµÑ‚ ÑÑ‚Ð¾ ÐºÐ°Ðº not semantic authority. Training adaptive model from this signal ÐµÑ‰Ñ‘ Ð½Ðµ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½.

Ð§Ñ‚Ð¾ Ð½ÐµÐ»ÑŒÐ·Ñ Ð´ÐµÐ»Ð°Ñ‚ÑŒ: Ð½ÐµÐ»ÑŒÐ·Ñ Ð¼Ð¾Ð»Ñ‡Ð° Ð´Ð¾Ð±Ð°Ð²Ð»ÑÑ‚ÑŒ skill Ñ‡ÐµÑ€ÐµÐ· if/else Ð¸ Ð½Ð°Ð·Ñ‹Ð²Ð°Ñ‚ÑŒ ÑÑ‚Ð¾ Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð¸ÐµÐ¼. ÐÐµÐ»ÑŒÐ·Ñ Ð¿ÐµÑ€ÐµÐºÐ»Ð°Ð´Ñ‹Ð²Ð°Ñ‚ÑŒ Ð½Ð° Ð›ÑƒÐ±Ñƒ Ñ€ÑƒÑ‡Ð½Ð¾Ðµ Ð·Ð°Ð¿Ð¾Ð»Ð½ÐµÐ½Ð¸Ðµ DPO bindings, ÐµÑÐ»Ð¸ local approved strategies and source refs can safely seed rows automatically.

### A1.5 ÐŸÐ¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ð¸ÑÐ¿Ñ€Ð°Ð²Ð¸Ð» Ð¸Ð»Ð¸ Ð¸Ð·Ð¼ÐµÐ½Ð¸Ð» Ñ€ÐµÐ·ÑƒÐ»ÑŒÑ‚Ð°Ñ‚ -> ÑƒÐ»ÑƒÑ‡ÑˆÐ¸Ñ‚ÑŒ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÑƒÑŽ Ð¼Ð¾Ð´ÐµÐ»ÑŒ

ÐšÐ»Ð°ÑÑ: `SmartSkillsDomain.SkillSelectionFeedbackWriter`.

Ð¤ÑƒÐ½ÐºÑ†Ð¸Ñ: `SkillSelectionFeedbackWriter.WriteProjectLocalSignal(ProjectLocalSkillState currentState, SkillSelectionFeedbackEvent feedback)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:282`).

Ð’Ñ…Ð¾Ð´: `ProjectLocalSkillState` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:69`) Ð¸ `SkillSelectionFeedbackEvent` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:76`). Ð’ feedback Ð»ÐµÐ¶Ð¸Ñ‚ prompt, originally selected skills, corrected skills, feedback text Ð¸ source ledger id.

Ð’Ñ‹Ñ…Ð¾Ð´: `SkillSelectionFeedbackWriteResult` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:98`) Ñ Ð¾Ð±Ð½Ð¾Ð²Ð»Ñ‘Ð½Ð½Ñ‹Ð¼ local state, DPO-like signal Ð¸ Ñ„Ð»Ð°Ð³Ð¾Ð¼, Ð¼ÐµÐ½ÑÐ»Ð°ÑÑŒ Ð»Ð¸ universal library.

ÐÐ»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼ ÑÐµÐ¹Ñ‡Ð°Ñ:
1. Ð”ÐµÐ»Ð°ÐµÑ‚ chosen Ð¸Ð· `CorrectedSkillIds`.
2. Ð”ÐµÐ»Ð°ÐµÑ‚ rejected Ð¸Ð· originally selected skills, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ñ… Ð½ÐµÑ‚ Ð² chosen.
3. Ð¡Ð¾Ð±Ð¸Ñ€Ð°ÐµÑ‚ source id Ð¸Ð· ledger id Ð¸ feedback id.
4. Ð¡Ð¾Ð·Ð´Ð°Ñ‘Ñ‚ `ProjectLocalSkillFeedbackSignal` Ð² scope `project-local:.xiigen/smart-skills/dpo-signals.jsonl`.
5. Ð¡Ñ‚Ð°Ð²Ð¸Ñ‚ `SemanticQualityClaimed = false`.
6. ÐŸÑ€Ð¾Ð²ÐµÑ€ÑÐµÑ‚, Ð½ÐµÑ‚ Ð»Ð¸ private absolute path Ð² prompt/reason/source id.
7. Ð”Ð¾Ð±Ð°Ð²Ð»ÑÐµÑ‚ feedback text Ð² local state Ð¸ Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ `UniversalLibraryMutated = false`.

ÐšÐ°Ðº Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ñ€Ð°Ð±Ð¾Ñ‚Ð°Ñ‚ÑŒ: correction Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ Ð¿Ñ€ÐµÐ²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ÑÑ Ð² DPO signal. Corrected behavior ÑÑ‚Ð°Ð½Ð¾Ð²Ð¸Ñ‚ÑÑ chosen, previous behavior ÑÑ‚Ð°Ð½Ð¾Ð²Ð¸Ñ‚ÑÑ rejected. Ð¡Ð¸Ð³Ð½Ð°Ð» Ð¾ÑÑ‚Ð°Ñ‘Ñ‚ÑÑ Ð² user adaptive storage Ð¸ Ð¼Ð¾Ð¶ÐµÑ‚ Ð¾Ð±ÑƒÑ‡Ð¸Ñ‚ÑŒ user model. Common repo assets Ð½Ðµ Ð¼ÑƒÑ‚Ð¸Ñ€ÑƒÑŽÑ‚ÑÑ private feedback Ð±ÐµÐ· Ð¾Ñ‚Ð´ÐµÐ»ÑŒÐ½Ð¾Ð³Ð¾ approved promotion path.

ÐšÐ°Ðº Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½Ð¾ ÑÐµÐ¹Ñ‡Ð°Ñ: `SkillSelectionFeedbackWriter` Ð¿Ð¸ÑˆÐµÑ‚ project-local DPO signal Ð¸ Ð´Ð¾Ð±Ð°Ð²Ð»ÑÐµÑ‚ feedback Ð² local state. Training loop ÐµÑ‰Ñ‘ Ð½Ðµ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½.

Ð§Ñ‚Ð¾ Ð½ÐµÐ»ÑŒÐ·Ñ Ð´ÐµÐ»Ð°Ñ‚ÑŒ: Ð½ÐµÐ»ÑŒÐ·Ñ Ð¿ÐµÑ€ÐµÐ·Ð°Ð¿Ð¸ÑÑ‹Ð²Ð°Ñ‚ÑŒ universal skill library Ð¸Ð»Ð¸ common checkpoint private feedback-Ð¾Ð¼.

### A1.6 Common/adaptive upgrade

ÐšÐ»Ð°ÑÑÑ‹: `SmartSkillsDomain.SkillLibraryUpgradePlanner`, `ProjectLocalSkillStateRefresh`, `SkillSelectionMachine`.

Ð¤ÑƒÐ½ÐºÑ†Ð¸Ñ: `SkillLibraryUpgradePlanner.PlanUpgrade(SkillLibraryUpgradeRequest request)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:328`, quote: `public SkillLibraryUpgradeResult PlanUpgrade(SkillLibraryUpgradeRequest request)`).

Ð’ÑÐ¿Ð¾Ð¼Ð¾Ð³Ð°Ñ‚ÐµÐ»ÑŒÐ½Ð°Ñ Ñ„ÑƒÐ½ÐºÑ†Ð¸Ñ refresh: `ProjectLocalSkillStateRefresh.ApplyServerLibraryRefresh(ProjectLocalSkillState localState, IReadOnlyList<ServerSkillLibraryEntry> refreshedServerEntries)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:422`) Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÐµÑ‚ local state + refreshed server entries Ð¸ Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ Ð½Ð¾Ð²Ñ‹Ð¹ `ProjectLocalSkillState` Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ñ Ð¾Ð±Ð½Ð¾Ð²Ð»Ñ‘Ð½Ð½Ñ‹Ð¼ `ServerLibraryEntries`.

Ð’Ñ…Ð¾Ð´: `SkillLibraryUpgradeRequest` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:103`) ÑÐ¾Ð´ÐµÑ€Ð¶Ð¸Ñ‚ old universal library, new universal library Ð¸ user/project local state.

Ð’Ñ‹Ñ…Ð¾Ð´: `SkillLibraryUpgradeResult` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:117`) Ñ upgraded local state, applied universal library, patch proposals, preservation flags Ð¸ evidence mode.

ÐÐ»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼ ÑÐµÐ¹Ñ‡Ð°Ñ:
1. Ð¡Ð¾Ñ€Ñ‚Ð¸Ñ€ÑƒÐµÑ‚ Ð½Ð¾Ð²ÑƒÑŽ universal library Ð¿Ð¾ `SkillId` Ð¸ `Version`.
2. ÐžÐ±Ð½Ð¾Ð²Ð»ÑÐµÑ‚ Ð² local state Ñ‚Ð¾Ð»ÑŒÐºÐ¾ `ServerLibraryEntries`.
3. Ð¡Ñ‚Ñ€Ð¾Ð¸Ñ‚ patch proposals Ð´Ð»Ñ local keys, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ðµ Ð¼Ð¾Ð³Ð»Ð¸ Ð¿ÐµÑ€ÐµÐµÑ…Ð°Ñ‚ÑŒ Ð² Ð½Ð¾Ð²Ñ‹Ðµ universal skill ids.
4. ÐŸÑ€Ð¾Ð²ÐµÑ€ÑÐµÑ‚, Ñ‡Ñ‚Ð¾ `WikiNotes`, `Preferences`, `Feedback`, `CustomSkillWeights` Ð½Ðµ Ð¸Ð·Ð¼ÐµÐ½Ð¸Ð»Ð¸ÑÑŒ.
5. Ð’Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ `ProjectLocalStatePreserved`, `UniversalLibraryMutatedProjectLocalState = false` Ð¸ `EvidenceMode = schema_upgrade_boundary_not_semantic_authority`.

ÐšÐ°Ðº Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ñ€Ð°Ð±Ð¾Ñ‚Ð°Ñ‚ÑŒ: common assets Ð¸Ð· `llm_mvp_core` Ð¼Ð¾Ð¶Ð½Ð¾ Ð¾Ð±Ð½Ð¾Ð²Ð»ÑÑ‚ÑŒ. User adaptive state Ð·Ð°Ð³Ñ€ÑƒÐ¶Ð°ÐµÑ‚ÑÑ Ñ€ÑÐ´Ð¾Ð¼ Ð¸ ÑÐ¾Ñ…Ñ€Ð°Ð½ÑÐµÑ‚ÑÑ. Ð•ÑÐ»Ð¸ common skill identity Ð¸Ð·Ð¼ÐµÐ½Ð¸Ð»ÑÑ, ÑÐ¸ÑÑ‚ÐµÐ¼Ð° ÑÐ¾Ð·Ð´Ð°Ñ‘Ñ‚ patch proposal Ð²Ð¼ÐµÑÑ‚Ð¾ silent overwrite.

ÐšÐ°Ðº Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½Ð¾ ÑÐµÐ¹Ñ‡Ð°Ñ: schema-boundary upgrade logic ÑƒÐ¶Ðµ ÐµÑÑ‚ÑŒ Ð¸ ÑÐ¾Ñ…Ñ€Ð°Ð½ÑÐµÑ‚ local state. `SkillSelectionMachine` ÑƒÐ¼ÐµÐµÑ‚ save/load checkpoint and manifest. Asset lifecycle Ð´Ð»Ñ `PromptToTaskMachine` Ð¸ `OrchestrationReasoningMachine` ÐµÑ‰Ñ‘ Ð½Ðµ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½.

Ð§Ñ‚Ð¾ Ð½ÐµÐ»ÑŒÐ·Ñ Ð´ÐµÐ»Ð°Ñ‚ÑŒ: Ð½ÐµÐ»ÑŒÐ·Ñ Ð¿Ð°ÐºÐ¾Ð²Ð°Ñ‚ÑŒ `.xiigen` user data Ð² common model release Ð¸ Ð½ÐµÐ»ÑŒÐ·Ñ Ð¿ÐµÑ€ÐµÑ‚Ð¸Ñ€Ð°Ñ‚ÑŒ user state Ð¿Ñ€Ð¸ common upgrade.

### A1.7 Ð”Ð²Ðµ Ð½Ð¾Ð³Ð¸ Ñƒ ÐºÐ°Ð¶Ð´Ð¾Ð¹ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ð¹ ÑÑ„ÐµÑ€Ñ‹

ÐšÐ»Ð°ÑÑÑ‹: `AgentInfrastructure.Logical.ILearningMachinePair<TPersonal,TGlobal>`, `AgentInfrastructure.Logical.ILearningMachine<TDomain>`, Ð±ÑƒÐ´ÑƒÑ‰Ð¸Ðµ pair-adapters Ð´Ð»Ñ `PromptToTaskMachine`, `OrchestrationReasoningMachine`, `SkillSelectionMachine`, `ResultGapMachine`.

ÐšÐ¾Ð½Ñ‚Ñ€Ð°ÐºÑ‚: `ILearningMachinePair<TPersonal,TGlobal>` (`C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs:185`, quote: `public interface ILearningMachinePair<TPersonal, TGlobal>`).

Ð’Ð°Ð¶Ð½Ð¾: A1.7 - ÑÑ‚Ð¾ Ð½Ðµ Ð¾Ñ‚Ð´ÐµÐ»ÑŒÐ½Ñ‹Ð¹ runtime algorithm, Ð° lifecycle-Ð¸Ð½Ð²Ð°Ñ€Ð¸Ð°Ð½Ñ‚ Ð´Ð»Ñ Ð²ÑÐµÑ… trainable algorithms Ð²Ñ‹ÑˆÐµ. Ð•ÑÐ»Ð¸ implementation Ð¿Ñ‹Ñ‚Ð°ÐµÑ‚ÑÑ ÑÐ´ÐµÐ»Ð°Ñ‚ÑŒ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼ÑƒÑŽ ÑÑ„ÐµÑ€Ñƒ Ð±ÐµÐ· Ð´Ð²ÑƒÑ… Ð½Ð¾Ð³, Ð¿Ð»Ð°Ð½ Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð¿Ð°Ð´Ð°Ñ‚ÑŒ.

Ð¢ÐµÐºÑƒÑ‰Ð¸Ðµ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½Ð½Ñ‹Ðµ Ñ„ÑƒÐ½ÐºÑ†Ð¸Ð¸, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ðµ Ð¿Ð¾ÐºÑ€Ñ‹Ð²Ð°ÑŽÑ‚ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ñ‡Ð°ÑÑ‚Ð¸ ÑÑ‚Ð¾Ð³Ð¾ Ð¸Ð½Ð²Ð°Ñ€Ð¸Ð°Ð½Ñ‚Ð°:

1. Personal signal leg: `SkillSelectionFeedbackWriter.WriteProjectLocalSignal(ProjectLocalSkillState currentState, SkillSelectionFeedbackEvent feedback)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:282`) Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÐµÑ‚ current user local state + correction feedback, Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ `SkillSelectionFeedbackWriteResult`, Ð¿Ð¸ÑˆÐµÑ‚ project-local signal Ð² `.xiigen` scope Ð¸ Ð½Ðµ Ð¼ÑƒÑ‚Ð¸Ñ€ÑƒÐµÑ‚ universal library.
2. Common upgrade preservation leg: `SkillLibraryUpgradePlanner.PlanUpgrade(SkillLibraryUpgradeRequest request)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:328`) Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÐµÑ‚ old/new universal library + project local state, Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ `SkillLibraryUpgradeResult`, Ð¾Ð±Ð½Ð¾Ð²Ð»ÑÐµÑ‚ server entries Ð¸ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÐµÑ‚, Ñ‡Ñ‚Ð¾ local notes/preferences/feedback/custom weights ÑÐ¾Ñ…Ñ€Ð°Ð½ÐµÐ½Ñ‹.
3. Common checkpoint transfer leg: `SkillSelectionMachine.ExportAsync(CancellationToken ct = default)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:158`) Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ `OperationResult<LearningMachineExport>`; `SkillSelectionMachine.ImportAsync(LearningMachineExport export, CancellationToken ct = default)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:173`) Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÐµÑ‚ `LearningMachineExport` Ð¸ Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ `OperationResult<bool>`.
4. Pair contract methods: `Personal`, `Global`, `SyncPersonalToGlobalAsync(CancellationToken ct = default)`, `PullGlobalToPersonalAsync(CancellationToken ct = default)` ÐµÑÑ‚ÑŒ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ ÐºÐ°Ðº interface surface (`C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs:188`, `:191`, `:197`, `:203`), Ð±ÐµÐ· smart-skills pair implementation.

ÐÐ»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼ ÑÐµÐ¹Ñ‡Ð°Ñ, Ñ‡ÐµÑÑ‚Ð½Ð¾:
1. User correction Ð¼Ð¾Ð¶ÐµÑ‚ ÑÑ‚Ð°Ñ‚ÑŒ project-local signal Ñ‡ÐµÑ€ÐµÐ· `WriteProjectLocalSignal`.
2. Common library refresh Ð¼Ð¾Ð¶ÐµÑ‚ ÑÐ¾Ñ…Ñ€Ð°Ð½Ð¸Ñ‚ÑŒ local state Ñ‡ÐµÑ€ÐµÐ· `PlanUpgrade`.
3. SkillSelection common checkpoint Ð¼Ð¾Ð¶Ð½Ð¾ export/import Ñ‡ÐµÑ€ÐµÐ· `ExportAsync`/`ImportAsync`.
4. Typed pair-Ð¸Ð½Ñ‚ÐµÑ€Ñ„ÐµÐ¹Ñ ÑÑƒÑ‰ÐµÑÑ‚Ð²ÑƒÐµÑ‚, Ð½Ð¾ `PromptToTask/OrchestrationReasoning/SkillSelection/ResultGap` pair-adapters ÐµÑ‰Ñ‘ Ð¾Ñ‚ÑÑƒÑ‚ÑÑ‚Ð²ÑƒÑŽÑ‚.
5. Backup/export/restore/cross-device user package ÐµÑ‰Ñ‘ Ð¾Ñ‚ÑÑƒÑ‚ÑÑ‚Ð²ÑƒÐµÑ‚, Ð¿Ð¾ÑÑ‚Ð¾Ð¼Ñƒ AD-00 Ð¸ AD-06..AD-09 Ð¾ÑÑ‚Ð°ÑŽÑ‚ÑÑ Ð¾Ð±ÑÐ·Ð°Ñ‚ÐµÐ»ÑŒÐ½Ñ‹Ð¼Ð¸ work orders.

ÐšÐ°Ðº Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ñ€Ð°Ð±Ð¾Ñ‚Ð°Ñ‚ÑŒ: ÐºÐ°Ð¶Ð´Ð°Ñ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ ÑÑ„ÐµÑ€Ð° Ð¶Ð¸Ð²Ñ‘Ñ‚ ÐºÐ°Ðº Ð¿Ð°Ñ€Ð°. Common/global Ð½Ð¾Ð³Ð° Ñ…Ñ€Ð°Ð½Ð¸Ñ‚ Ð½Ð°ÑˆÑƒ Ð¾Ð±Ñ‰ÑƒÑŽ Ð¼Ð¾Ð´ÐµÐ»ÑŒ, Ð¾Ð±Ñ‰Ð¸Ðµ ÑÐµÑ‚Ñ‹, manifest, locator Ð¸ upgrade path Ð² `llm_mvp_core`. Personal/adaptive Ð½Ð¾Ð³Ð° Ñ…Ñ€Ð°Ð½Ð¸Ñ‚ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÑƒÑŽ Ð¼Ð¾Ð´ÐµÐ»ÑŒ, Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ¸Ðµ DPO-ÑÐ¸Ð³Ð½Ð°Ð»Ñ‹ Ð¸ Ð¿Ñ€ÐµÐ´Ð¿Ð¾Ñ‡Ñ‚ÐµÐ½Ð¸Ñ Ð½Ð° ÐºÐ¾Ð¼Ð¿ÑŒÑŽÑ‚ÐµÑ€Ðµ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ Ð² `.xiigen`. ÐšÐ¾Ð³Ð´Ð° Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ð¸ÑÐ¿Ñ€Ð°Ð²Ð»ÑÐµÑ‚ Ñ€ÐµÐ·ÑƒÐ»ÑŒÑ‚Ð°Ñ‚ Ð¸Ð»Ð¸ ÐºÐ¾Ð³Ð´Ð° Ð¿Ð¾ÑÐ»Ðµ AI-Ð¾Ñ‚Ð²ÐµÑ‚Ð° Ð½Ð°Ð¹Ð´ÐµÐ½ Ð¿Ñ€Ð¾Ð¿ÑƒÑ‰ÐµÐ½Ð½Ñ‹Ð¹ Ð¿Ð°Ñ‚Ñ‚ÐµÑ€Ð½, Ð¾Ð±ÑƒÑ‡Ð°ÐµÑ‚ÑÑ personal/adaptive Ð½Ð¾Ð³Ð°. ÐšÐ¾Ð³Ð´Ð° Ð¼Ñ‹ Ð²Ñ‹Ð¿ÑƒÑÐºÐ°ÐµÐ¼ common upgrade, Ð¾Ð½ Ð¿Ð¾Ð´Ñ‚ÑÐ³Ð¸Ð²Ð°ÐµÑ‚ÑÑ Ñ€ÑÐ´Ð¾Ð¼ Ñ personal/adaptive Ð½Ð¾Ð³Ð¾Ð¹, Ð½Ð¾ Ð½Ðµ Ð¿ÐµÑ€ÐµÑ‚Ð¸Ñ€Ð°ÐµÑ‚ ÐµÑ‘.

ÐžÑ‚Ð´ÐµÐ»ÑŒÐ½Ð¾Ðµ Ñ‚Ñ€ÐµÐ±Ð¾Ð²Ð°Ð½Ð¸Ðµ: personal/adaptive Ð½Ð¾Ð³Ñƒ Ð¼Ð¾Ð¶Ð½Ð¾ Ð·Ð°Ð±ÑÐºÐ°Ð¿Ð¸Ñ‚ÑŒ Ð¸ Ð¿ÐµÑ€ÐµÐ½ÐµÑÑ‚Ð¸. Ð”Ð»Ñ ÑÑ‚Ð¾Ð³Ð¾ Ð¿Ð»Ð°Ð½ Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð´Ð°Ñ‚ÑŒ user-model package: manifest, checkpoint hash, schema version, source/provenance refs, redaction report, restore/import probe Ð¸ cross-device fresh profile probe. â€œÐœÐ¾Ð´ÐµÐ»ÑŒ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ ÑÐ¸Ð´Ð¸Ñ‚ Ñƒ Ð½ÐµÐ³Ð¾ Ð½Ð° ÐºÐ¾Ð¼Ð¿Ðµâ€ Ð½Ðµ ÑÑ‡Ð¸Ñ‚Ð°ÐµÑ‚ÑÑ Ð²Ñ‹Ð¿Ð¾Ð»Ð½ÐµÐ½Ð½Ñ‹Ð¼ Ñ‚Ñ€ÐµÐ±Ð¾Ð²Ð°Ð½Ð¸ÐµÐ¼, Ð¿Ð¾ÐºÐ° ÐµÑ‘ Ð½ÐµÐ»ÑŒÐ·Ñ ÑÐºÑÐ¿Ð¾Ñ€Ñ‚Ð¸Ñ€Ð¾Ð²Ð°Ñ‚ÑŒ, Ð²Ð¾ÑÑÑ‚Ð°Ð½Ð¾Ð²Ð¸Ñ‚ÑŒ Ð¸ Ð·Ð°Ð³Ñ€ÑƒÐ·Ð¸Ñ‚ÑŒ Ð½Ð° Ð´Ñ€ÑƒÐ³Ð¾Ð¼ user profile/device path.

ÐšÐ°Ðº Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½Ð¾ ÑÐµÐ¹Ñ‡Ð°Ñ: Ð¾Ð±Ñ‰Ð¸Ð¹ ÐºÐ¾Ð½Ñ‚Ñ€Ð°ÐºÑ‚ `ILearningMachinePair<TPersonal,TGlobal>` ÑƒÐ¶Ðµ ÐµÑÑ‚ÑŒ Ð² `00.Core` ÐºÐ°Ðº personal + global Ð¼Ð°ÑˆÐ¸Ð½Ñ‹ Ñ anonymised sync/pull methods. Ð’ smart-skills ÑÐµÐ¹Ñ‡Ð°Ñ ÐµÑÑ‚ÑŒ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ñ‡Ð°ÑÑ‚Ð¸Ñ‡Ð½Ð°Ñ Ñ„Ð°Ð¹Ð»Ð¾Ð²Ð°Ñ Ð³Ñ€Ð°Ð½Ð¸Ñ†Ð°: `.xiigen` local state, feedback writer Ð¸ upgrade-preservation logic. Pair-adapter Ð¸ Ð¿ÐµÑ€ÐµÐ½Ð¾ÑÐ¸Ð¼Ñ‹Ð¹ user-model package Ð´Ð»Ñ PromptToTask/OrchestrationReasoning/SkillSelection/ResultGap ÐµÑ‰Ñ‘ Ð½Ðµ Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½Ñ‹.

Ð§Ñ‚Ð¾ Ð½ÐµÐ»ÑŒÐ·Ñ Ð´ÐµÐ»Ð°Ñ‚ÑŒ: Ð½ÐµÐ»ÑŒÐ·Ñ ÑÑ‡Ð¸Ñ‚Ð°Ñ‚ÑŒ `.xiigen` folder convention Ð¿Ð¾Ð»Ð½Ð¾Ñ†ÐµÐ½Ð½Ð¾Ð¹ Ð²Ñ‚Ð¾Ñ€Ð¾Ð¹ Ð½Ð¾Ð³Ð¾Ð¹ Ð±ÐµÐ· typed pair contract, backup/export, restore/import Ð¸ cross-device probe. ÐÐµÐ»ÑŒÐ·Ñ ÑˆÐ°Ñ€Ð¸Ñ‚ÑŒ raw private prompts, raw private analyses, secrets Ð¸Ð»Ð¸ private corpus rows. ÐÐµÐ»ÑŒÐ·Ñ Ð¼ÑƒÑ‚Ð¸Ñ€Ð¾Ð²Ð°Ñ‚ÑŒ common model private feedback-Ð¾Ð¼ Ð±ÐµÐ· approved promotion path.

### A1.8 Debug trace Ð² extension chat

ÐšÐ»Ð°ÑÑ: `SmartSkillsDomain.XiigenExtensionApiContract`.

Ð“Ð»Ð°Ð²Ð½Ð°Ñ Ñ„ÑƒÐ½ÐºÑ†Ð¸Ñ API: `XiigenExtensionApiService.ProcessHostExtensionRequest(XiigenExtensionRequest request)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs:38`).

Ð¤ÑƒÐ½ÐºÑ†Ð¸Ñ Ñ„Ð¾Ñ€Ð¼Ð°Ñ‚Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð¸Ñ trace: `XiigenExtensionApiContract.FormatDebugTraceForChat(bool debugEnabled, XiigenDebugTrace? debugTrace)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs:72`). Ð¤ÑƒÐ½ÐºÑ†Ð¸Ñ ÑÐºÐ»ÐµÐ¹ÐºÐ¸ Ñ Ð¾Ð±Ñ‹Ñ‡Ð½Ñ‹Ð¼ Ñ‚ÐµÐºÑÑ‚Ð¾Ð¼: `BuildChatDisplayText(string chatDisplayText, bool debugEnabled, XiigenDebugTrace? debugTrace)` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs:93`).

Ð’Ñ…Ð¾Ð´: `XiigenExtensionRequest` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs:4`) Ñ host name/type, method, prompt text, host facts, debug flag, requested/available skills, check-set inputs, provenance Ð¸ correlation id.

Ð’Ñ‹Ñ…Ð¾Ð´: `XiigenExtensionResponse` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs:18`) Ñ chat text, selected skills, check-set results, structured task refs, scenario refs, warnings/errors Ð¸ optional `XiigenDebugTrace`.

ÐÐ»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼ ÑÐµÐ¹Ñ‡Ð°Ñ:
1. Ð’Ð°Ð»Ð¸Ð´Ð¸Ñ€ÑƒÐµÑ‚ request Ñ‡ÐµÑ€ÐµÐ· `ValidateRequest`.
2. Hash-Ð¸Ñ‚ prompt Ð¸ Ð½Ðµ ÐºÐ»Ð°Ð´Ñ‘Ñ‚ raw prompt Ð² debug payload.
3. Ð’Ñ‹Ð·Ñ‹Ð²Ð°ÐµÑ‚ `PromptToTaskMachine.Translate`, `OrchestrationReasoningMachine.Plan`, `SkillSelectionMachine.DecideFromTrainedState`.
4. Ð¡Ñ‚Ñ€Ð¾Ð¸Ñ‚ response ÑÐ¾ status `core_boundary_partial_no_semantic_quality_claim`.
5. `BuildDebugTrace` (`C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs:433`) ÑÐµÑ€Ð¸Ð°Ð»Ð¸Ð·ÑƒÐµÑ‚ input/output Ñ‚Ð°Ðº, Ñ‡Ñ‚Ð¾ `UserPromptOrTaskText = "[redacted]"`.
6. `FormatDebugTraceForChat` Ð¿Ð¾ÐºÐ°Ð·Ñ‹Ð²Ð°ÐµÑ‚ trace Ñ‚Ð¾Ð»ÑŒÐºÐ¾ ÐµÑÐ»Ð¸ debug Ð²ÐºÐ»ÑŽÑ‡Ñ‘Ð½ Ð¸ trace Ð½Ðµ ÑÐ¾Ð´ÐµÑ€Ð¶Ð¸Ñ‚ raw private content/secrets.

ÐšÐ°Ðº Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ñ€Ð°Ð±Ð¾Ñ‚Ð°Ñ‚ÑŒ: Ð² ÐºÐ°Ð¶Ð´Ð¾Ð¹ extension ÐµÑÑ‚ÑŒ Ð½Ð°ÑÑ‚Ñ€Ð¾Ð¹ÐºÐ° debug for programmer. ÐšÐ¾Ð³Ð´Ð° Ð¾Ð½Ð° Ð²ÐºÐ»ÑŽÑ‡ÐµÐ½Ð° Ð¸ trace Ð±ÐµÐ·Ð¾Ð¿Ð°ÑÐµÐ½, Ñ‡Ð°Ñ‚ Ð¿Ð¾ÐºÐ°Ð·Ñ‹Ð²Ð°ÐµÑ‚ Ñ€Ð¾Ð²Ð½Ð¾:

```text
xiigen <method> :
input: <what was sent to xiigen exe>
output: <what was received from xiigen exe>
```

ÐšÐ°Ðº Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½Ð¾ ÑÐµÐ¹Ñ‡Ð°Ñ: core formatter and redaction guard ÑƒÐ¶Ðµ ÐµÑÑ‚ÑŒ. Host extensions ÐµÑ‰Ñ‘ Ð´Ð¾Ð»Ð¶Ð½Ñ‹ Ð¿Ð¾ÐºÐ°Ð·Ð°Ñ‚ÑŒ ÑÑ‚Ð¾ Ñ‡ÐµÑ€ÐµÐ· Ñ€ÐµÐ°Ð»ÑŒÐ½Ñ‹Ðµ entrypoints Claude, Cursor, Codex.

Ð§Ñ‚Ð¾ Ð½ÐµÐ»ÑŒÐ·Ñ Ð´ÐµÐ»Ð°Ñ‚ÑŒ: Ð½ÐµÐ»ÑŒÐ·Ñ Ð¿Ð¾ÐºÐ°Ð·Ñ‹Ð²Ð°Ñ‚ÑŒ raw private prompts, secrets, full private corpus rows Ð¸Ð»Ð¸ internal training kitchen, ÐµÑÐ»Ð¸ trace flags Ð³Ð¾Ð²Ð¾Ñ€ÑÑ‚, Ñ‡Ñ‚Ð¾ Ñ‚Ð°Ð¼ private content or secrets.

### A1.9 Ð’ÑÐµ Ð±Ð¾Ð»ÑŒÑˆÐ¸Ðµ Ð²ÐµÑ‚ÐºÐ¸ Part B Ñ‡ÐµÐ»Ð¾Ð²ÐµÑ‡ÐµÑÐºÐ¸Ð¼ ÑÐ·Ñ‹ÐºÐ¾Ð¼

Ð­Ñ‚Ð¾ ÐºÐ¾Ð½Ñ‚Ñ€Ð¾Ð»ÑŒ `A == B`. Part B Ð¼Ð¾Ð¶ÐµÑ‚ Ð´Ð¾Ð±Ð°Ð²Ð»ÑÑ‚ÑŒ file scopes, Ð¸Ð¼ÐµÐ½Ð° work orders, Ð»Ð¸Ð¼Ð¸Ñ‚Ñ‹ Ð²Ñ€ÐµÐ¼ÐµÐ½Ð¸, Ñ‚ÐµÑÑ‚Ñ‹, validator names Ð¸ commit boundaries. Part B Ð½Ðµ Ð¸Ð¼ÐµÐµÑ‚ Ð¿Ñ€Ð°Ð²Ð° Ð´Ð¾Ð±Ð°Ð²Ð»ÑÑ‚ÑŒ Ð½Ð¾Ð²ÑƒÑŽ Ð¿Ñ€Ð¾Ð´ÑƒÐºÑ‚Ð¾Ð²ÑƒÑŽ Ð²ÐµÑ‚ÐºÑƒ, Ð°Ð»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼, Ð¼Ð¾Ð´ÐµÐ»ÑŒÐ½Ñ‹Ð¹ lifecycle, ownership Ð´Ð°Ð½Ð½Ñ‹Ñ… Ð¸Ð»Ð¸ host behavior, ÐµÑÐ»Ð¸ ÑÑ‚Ð¾Ð³Ð¾ Ð½ÐµÑ‚ Ð² Part A.

| Ð’ÐµÑ‚ÐºÐ° Ð² Part B | Ð§Ñ‚Ð¾ ÑÑ‚Ð¾ Ð·Ð½Ð°Ñ‡Ð¸Ñ‚ Ð´Ð»Ñ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ | ÐšÐ°ÐºÐ¸Ðµ Ð´Ð°Ð½Ð½Ñ‹Ðµ Ð¸Ð´ÑƒÑ‚ | Ð§Ñ‚Ð¾ Ð½Ðµ Ð´Ð¾Ð»Ð¶Ð½Ð¾ Ð±Ñ‹Ñ‚ÑŒ ÑÐ¿Ñ€ÑÑ‚Ð°Ð½Ð¾ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ð² B |
|---|---|---|---|
| `B6 Foundation work orders` | Ð¡Ð½Ð°Ñ‡Ð°Ð»Ð° Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÐµÐ¼ ÑÐ°Ð¼ Ð¿Ð»Ð°Ð½ Ð¸ Ð¿Ñ€Ð°Ð²Ð¸Ð»Ð°, Ð° Ð½Ðµ Ð¿Ð¸ÑˆÐµÐ¼ ÐºÐ¾Ð´. Ð•ÑÐ»Ð¸ review Ð½Ð°ÑˆÑ‘Ð» Ð´Ñ‹Ñ€Ñƒ Ð² ÑÐºÐ¸Ð»Ð°Ñ…/AGENTS/CLAUDE Ð¸Ð»Ð¸ Ð² readable-doc format, Ñ‡Ð¸Ð½Ð¸Ñ‚ÑÑ governance/documentation. | Ð¢Ð¾Ð»ÑŒÐºÐ¾ plan/review notes, governance text, docs with exact code refs. | ÐÐµÐ»ÑŒÐ·Ñ Ð½Ð°Ñ‡Ð¸Ð½Ð°Ñ‚ÑŒ implementation Ñ‡ÐµÑ€ÐµÐ· foundation-Ð²ÐµÑ‚ÐºÑƒ. ÐÐµÐ»ÑŒÐ·Ñ ÑÑ‡Ð¸Ñ‚Ð°Ñ‚ÑŒ review notes Ð·Ð°Ð¼ÐµÐ½Ð¾Ð¹ Ð³Ð¾Ñ‚Ð¾Ð²Ð¾Ð¼Ñƒ Ð¿Ð»Ð°Ð½Ñƒ. |
| `B7 PromptToTaskMachine` | Ð­Ñ‚Ð¾ Ð¾Ñ‚Ð´ÐµÐ»ÑŒÐ½Ð°Ñ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ Ð²ÐµÑ‚ÐºÐ° "prompt Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ -> ÑÑ‚Ñ€ÑƒÐºÑ‚ÑƒÑ€Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð½Ð°Ñ Ð·Ð°Ð´Ð°Ñ‡Ð°". ÐžÐ½Ð° Ð´Ð¾Ð»Ð¶Ð½Ð° Ð¿Ð¾Ð»ÑƒÑ‡Ð¸Ñ‚ÑŒ ÑÐ²Ð¾Ð¸ training pairs, redacted corpus, validator, checkpoint, manifest, export/import, fresh-load Ð¸ continue-training. | Prompt hash, source refs, chosen/rejected `StructuredTaskContract`, constraints, deliverables, forbidden actions, evidence needs. | ÐÐµÐ»ÑŒÐ·Ñ Ð´ÐµÑ€Ð¶Ð°Ñ‚ÑŒ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ skeleton `not_ready_no_semantic_task`. ÐÐµÐ»ÑŒÐ·Ñ Ð·Ð°Ð¼ÐµÐ½Ð¸Ñ‚ÑŒ Ð¿ÐµÑ€ÐµÐ²Ð¾Ð´ prompt->task regex-Ð¾Ð¼, exact prompt lookup Ð¸Ð»Ð¸ raw prompt copy Ð² common repo. |
| `B8 OrchestrationReasoningMachine` | Ð­Ñ‚Ð¾ Ð¾Ñ‚Ð´ÐµÐ»ÑŒÐ½Ð°Ñ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ Ð²ÐµÑ‚ÐºÐ° "ÑÑ‚Ñ€ÑƒÐºÑ‚ÑƒÑ€Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð½Ð°Ñ Ð·Ð°Ð´Ð°Ñ‡Ð° -> Ð¿Ð»Ð°Ð½ Ð¾Ñ€ÐºÐµÑÑ‚Ñ€Ð°Ñ†Ð¸Ð¸". ÐžÐ½Ð° ÑƒÑ‡Ð¸Ñ‚ÑÑ Ð½Ð° guide-derived patterns and approved analyses, Ð° Ð½Ðµ Ð¸Ñ‰ÐµÑ‚ Ñ„Ð°Ð¹Ð» Ð¿Ð¾ Ð¸Ð¼ÐµÐ½Ð¸. | Task contract, guide-state refs, corpus bundle refs, chosen/rejected scenario plans, phase/gate/asset/host-check families. | ÐÐµÐ»ÑŒÐ·Ñ ÑÐ¿Ñ€ÑÑ‚Ð°Ñ‚ÑŒ BM25, filename lookup, guide title matching Ð¸Ð»Ð¸ static route table ÐºÐ°Ðº Ð±ÑƒÐ´Ñ‚Ð¾ ÑÑ‚Ð¾ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ðµ reasoning. |
| `B9 SkillSelectionMachine and semantic units` | Ð¢ÐµÐºÑƒÑ‰Ð¸Ð¹ position-weight scaffold Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ Ð·Ð°Ð¼ÐµÐ½Ñ‘Ð½ ÑÐµÐ¼Ð°Ð½Ñ‚Ð¸Ñ‡ÐµÑÐºÐ¾Ð¹ Ð¼Ð°ÑˆÐ¸Ð½Ð¾Ð¹ Ð²Ñ‹Ð±Ð¾Ñ€Ð° skills. ÐžÐ½Ð° ÑÑ‚Ñ€Ð¾Ð¸Ñ‚ÑÑ Ð¸Ð· 11 Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ñ‹Ñ… unit-Ð¾Ð², ÐºÐ°Ð¶Ð´Ñ‹Ð¹ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÐµÑ‚ÑÑ Ð¾Ñ‚Ð´ÐµÐ»ÑŒÐ½Ð¾, Ð¿Ð¾Ñ‚Ð¾Ð¼ Ð²ÑÑ‘ ÑÐºÑÐ¿Ð¾Ñ€Ñ‚Ð¸Ñ€ÑƒÐµÑ‚ÑÑ/Ð¸Ð¼Ð¿Ð¾Ñ€Ñ‚Ð¸Ñ€ÑƒÐµÑ‚ÑÑ ÐºÐ°Ðº Ð¼Ð¾Ð´ÐµÐ»ÑŒÐ½Ñ‹Ð¹ asset. | Task/scenario context, available skills, semantic unit checkpoints, held-out reports, ablation evidence. | ÐÐµÐ»ÑŒÐ·Ñ Ð¾ÑÑ‚Ð°Ð²Ð¸Ñ‚ÑŒ `_candidatePositionWeights[index]++` Ñ„Ð¸Ð½Ð°Ð»ÑŒÐ½Ñ‹Ð¼ selector-Ð¾Ð¼. ÐÐµÐ»ÑŒÐ·Ñ ÑÐºÐ°Ð·Ð°Ñ‚ÑŒ "ÑÐºÐ¸Ð»Ñ‹ Ð²Ñ‹Ð±Ð¸Ñ€Ð°ÑŽÑ‚ÑÑ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾", Ð¿Ð¾ÐºÐ° 11 unit-Ð¾Ð² Ð½Ðµ Ð¿Ñ€ÐµÐ´ÑÑ‚Ð°Ð²Ð»ÐµÐ½Ñ‹ Ð² checkpoint and reports. |
| `B10 Adaptive user learning` | Ð­Ñ‚Ð¾ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ°Ñ Ð½Ð¾Ð³Ð° Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð¸Ñ. Ð•ÑÐ»Ð¸ ÑÐ¸ÑÑ‚ÐµÐ¼Ð° Ð½Ðµ Ð½Ð°ÑˆÐ»Ð° Ð¿Ð°Ñ‚Ñ‚ÐµÑ€Ð½ Ð¸Ð»Ð¸ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ð¸ÑÐ¿Ñ€Ð°Ð²Ð¸Ð» Ñ€ÐµÐ·ÑƒÐ»ÑŒÑ‚Ð°Ñ‚, ÑÑ‚Ñ€Ð¾Ð¸Ñ‚ÑÑ user-local DPO, Ð¾Ð±ÑƒÑ‡Ð°ÐµÑ‚ÑÑ adaptive model, common state Ð½Ðµ Ð¿ÐµÑ€ÐµÑ‚Ð¸Ñ€Ð°ÐµÑ‚ÑÑ. User model Ð´Ð¾Ð»Ð¶Ð½Ð° backup/export/restore/share safely. | `.xiigen` adaptive checkpoint, user DPO rows, correction signals, redaction report, user-model package manifest/hash. | ÐÐµÐ»ÑŒÐ·Ñ Ñ…Ñ€Ð°Ð½Ð¸Ñ‚ÑŒ feedback Ñ‚Ð¾Ð»ÑŒÐºÐ¾ ÐºÐ°Ðº Ñ‡Ð°Ñ‚. ÐÐµÐ»ÑŒÐ·Ñ Ð¼ÑƒÑ‚Ð¸Ñ€Ð¾Ð²Ð°Ñ‚ÑŒ common model private feedback-Ð¾Ð¼. ÐÐµÐ»ÑŒÐ·Ñ ÑÑ‡Ð¸Ñ‚Ð°Ñ‚ÑŒ `.xiigen` Ð¿Ð°Ð¿ÐºÑƒ Ð¿Ð¾Ð»Ð½Ð¾Ñ†ÐµÐ½Ð½Ð¾Ð¹ Ð²Ñ‚Ð¾Ñ€Ð¾Ð¹ Ð½Ð¾Ð³Ð¾Ð¹ Ð±ÐµÐ· pair contract and portability probes. |
| `B11 Core API boundary` | XIIGen core Ð´Ð¾Ð»Ð¶ÐµÐ½ ÑÑ‚Ð°Ñ‚ÑŒ Ñ€ÐµÐ°Ð»ÑŒÐ½Ñ‹Ð¼ API Ð´Ð»Ñ host extensions: host Ð¾Ñ‚Ð¿Ñ€Ð°Ð²Ð»ÑÐµÑ‚ request, core Ð²Ñ‹Ð·Ñ‹Ð²Ð°ÐµÑ‚ trained assets, Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ task refs, scenario refs, selected skills, warnings and debug trace. | `XiigenExtensionRequest`, trained-model refs, `XiigenExtensionResponse`, safe debug trace. | ÐÐµÐ»ÑŒÐ·Ñ Ð¿ÐµÑ€ÐµÐ½ÐµÑÑ‚Ð¸ semantic ranking/training/corpus logic Ð² Claude/Cursor/Codex repos. ÐÐµÐ»ÑŒÐ·Ñ Ð·Ð°ÑÐ²Ð»ÑÑ‚ÑŒ semantic quality, Ð¿Ð¾ÐºÐ° response Ð½Ðµ ÑÑÑ‹Ð»Ð°ÐµÑ‚ÑÑ Ð½Ð° trained-model gate evidence. |
| `B12 Claude extension repo` | Ð’ `claude-xiigen-smart-skills` Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ Ð½Ð°ÑÑ‚Ð¾ÑÑ‰Ð¸Ð¹ Claude entrypoint code, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ð²Ñ‹Ð·Ñ‹Ð²Ð°ÐµÑ‚ XIIGen exe/API Ð¸ Ð¿Ð¾ÐºÐ°Ð·Ñ‹Ð²Ð°ÐµÑ‚ Ñ€ÐµÐ·ÑƒÐ»ÑŒÑ‚Ð°Ñ‚ Ð² Ñ‡Ð°Ñ‚Ðµ. | Host prompt, settings, thin request, response/debug trace. | JSON/PowerShell reports Ð½Ðµ ÑÑ‡Ð¸Ñ‚Ð°ÑŽÑ‚ÑÑ Claude plugin. Host repo Ð½Ðµ Ð²Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ skills and Ð½Ðµ Ð¾Ð±ÑƒÑ‡Ð°ÐµÑ‚ model. |
| `B13 Cursor extension repo` | Ð’ `cursor-xiigen-smart-skills` Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ Ð½Ð°ÑÑ‚Ð¾ÑÑ‰Ð¸Ð¹ Cursor/VS Code command, chat participant, panel or equivalent entrypoint. | VS Code/Cursor request, available skills, XIIGen response, safe debug output. | ÐÐµÐ»ÑŒÐ·Ñ ÑÑ‡Ð¸Ñ‚Ð°Ñ‚ÑŒ package manifest Ð±ÐµÐ· Extension Host/UI/CLI smoke Ð¿Ð¾Ð»Ð½Ð¾Ñ†ÐµÐ½Ð½Ð¾Ð¹ extension. |
| `B14 Codex extension repo` | Ð’ `codex-xiigen-smart-skills` Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ Ð½Ð°ÑÑ‚Ð¾ÑÑ‰Ð¸Ð¹ Codex plugin/app entrypoint. | Codex plugin/app request, XIIGen response, safe debug trace. | ÐÐµÐ»ÑŒÐ·Ñ Ð·Ð°Ð¼ÐµÐ½Ð¸Ñ‚ÑŒ Codex plugin Ð»Ð¾ÐºÐ°Ð»ÑŒÐ½Ñ‹Ð¼ JSON report Ð¸Ð»Ð¸ internal helper. |
| `B15 Cross-host gates` | Claude, Cursor and Codex Ð´Ð¾Ð»Ð¶Ð½Ñ‹ Ð²ÑÐµ Ð¿Ñ€Ð¾Ð¹Ñ‚Ð¸ Ð¾Ð´Ð¸Ð½Ð°ÐºÐ¾Ð²ÑƒÑŽ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÐºÑƒ: real entrypoint, thin host, prompt->task->plan->skills request, debug trace, no external AI, correct commits/pushes. | Host runtime traces, source refs, redaction evidence, git evidence. | ÐÐµÐ»ÑŒÐ·Ñ Ð¿Ñ€Ð¸Ð½ÑÑ‚ÑŒ Ð¾Ð´Ð¸Ð½ host Ð¸ ÑÑ‡Ð¸Ñ‚Ð°Ñ‚ÑŒ Ð¾ÑÑ‚Ð°Ð»ÑŒÐ½Ñ‹Ðµ Ð¿Ð¾ÐºÑ€Ñ‹Ñ‚Ñ‹Ð¼Ð¸. ÐÐµÐ»ÑŒÐ·Ñ Ð¿Ñ€Ð¸Ð½ÑÑ‚ÑŒ static contract Ð²Ð¼ÐµÑÑ‚Ð¾ host-facing evidence. |
| `B16 No-hidden-determinism review` | ÐžÑ‚Ð´ÐµÐ»ÑŒÐ½Ð°Ñ review-Ð²ÐµÑ‚ÐºÐ° Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÐµÑ‚, Ñ‡Ñ‚Ð¾ deterministic infrastructure Ð½Ðµ Ð¿Ð¾Ð´Ð¼ÐµÐ½ÑÐµÑ‚ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ñ‹Ð¹ unit. Parse/hash/redact/save/load Ñ€Ð°Ð·Ñ€ÐµÑˆÐµÐ½Ñ‹; Ñ€ÐµÑˆÐµÐ½Ð¸Ñ task/plan/skills/result-gap Ð´Ð¾Ð»Ð¶Ð½Ñ‹ Ð¸Ð´Ñ‚Ð¸ Ð¸Ð· trained state. | Static sweep, request traces, ablation reports, checkpoint evidence. | ÐÐµÐ»ÑŒÐ·Ñ ÑÐ¿Ñ€ÑÑ‚Ð°Ñ‚ÑŒ deterministic shortcut Ð² runtime decision Ð¸ Ð½Ð°Ð·Ð²Ð°Ñ‚ÑŒ ÐµÐ³Ð¾ model. |
| `B17-B21 Final coverage and acceptance` | Ð¤Ð¸Ð½Ð°Ð» Ð½Ðµ Ð½Ð°ÑÑ‚ÑƒÐ¿Ð°ÐµÑ‚ Ð¾Ñ‚ ÐºÑ€Ð°ÑÐ¸Ð²Ð¾Ð³Ð¾ report-Ð°. ÐÑƒÐ¶Ð½Ð° Ð¼Ð°Ñ‚Ñ€Ð¸Ñ†Ð° ÑÑ†ÐµÐ½Ð°Ñ€Ð¸ÐµÐ², external-tool audit, anti-loophole sweep, completion matrix and explicit final architect verdict. | Scenario matrix, tool evidence, anti-loophole findings, completion matrix rows. | ÐÐµÐ»ÑŒÐ·Ñ claim "full extension complete" Ð¾Ñ‚ plan text, JSON artifacts, static contracts or partial host readiness. |

Ð¡ÐµÐ¼Ð°Ð½Ñ‚Ð¸Ñ‡ÐµÑÐºÐ¸Ðµ units Ð¸Ð· `B9` Ñ‚Ð¾Ð¶Ðµ ÑÐ²Ð»ÑÑŽÑ‚ÑÑ Ð½Ðµ Ñ‚ÐµÑ…Ð½Ð¸Ñ‡ÐµÑÐºÐ¾Ð¹ Ð¼ÐµÐ»Ð¾Ñ‡ÑŒÑŽ, Ð° Ñ‡Ð°ÑÑ‚ÑŒÑŽ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ¾Ð¹ Ð»Ð¾Ð³Ð¸ÐºÐ¸:

| Unit | Ð§ÐµÐ»Ð¾Ð²ÐµÑ‡ÐµÑÐºÐ¸Ð¹ ÑÐ¼Ñ‹ÑÐ» |
|---|---|
| `prompt_understanding` | ÐŸÐ¾Ð½ÑÑ‚ÑŒ, Ñ‡Ñ‚Ð¾ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒ Ñ€ÐµÐ°Ð»ÑŒÐ½Ð¾ Ð½Ð°Ð¿Ð¸ÑÐ°Ð», Ð±ÐµÐ· keyword shortcut. |
| `task_understanding` | ÐŸÑ€ÐµÐ²Ñ€Ð°Ñ‚Ð¸Ñ‚ÑŒ ÑÐ¼Ñ‹ÑÐ» Ð·Ð°Ð¿Ñ€Ð¾ÑÐ° Ð² Ñ€Ð°Ð±Ð¾Ñ‡ÑƒÑŽ Ð·Ð°Ð´Ð°Ñ‡Ñƒ Ñ constraints and deliverables. |
| `task_type` | ÐžÐ¿Ñ€ÐµÐ´ÐµÐ»Ð¸Ñ‚ÑŒ Ñ‚Ð¸Ð¿ Ñ€Ð°Ð±Ð¾Ñ‚Ñ‹: planning, review, implementation, debugging, extension integration, adaptive learning Ð¸ Ñ‚.Ð´. |
| `routing` | Ð ÐµÑˆÐ¸Ñ‚ÑŒ, ÐºÐ°ÐºÐ°Ñ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ Ð²ÐµÑ‚ÐºÐ° Ð½ÑƒÐ¶Ð½Ð° Ð´Ð°Ð»ÑŒÑˆÐµ: prompt-to-task, reasoning, skill selection, result-gap, feedback. |
| `skill_candidates` | ÐÐ°Ð¹Ñ‚Ð¸ Ð²Ð¾Ð·Ð¼Ð¾Ð¶Ð½Ñ‹Ðµ skills-ÐºÐ°Ð½Ð´Ð¸Ð´Ð°Ñ‚Ñ‹ Ð¸Ð· Ð±Ð¸Ð±Ð»Ð¸Ð¾Ñ‚ÐµÐºÐ¸. |
| `skill_selection` | Ð’Ñ‹Ð±Ñ€Ð°Ñ‚ÑŒ final skills Ð¸Ð· candidates Ñ‡ÐµÑ€ÐµÐ· trained semantic evidence. |
| `prompt_context` | Ð£Ñ‡ÐµÑÑ‚ÑŒ host facts, project context, provenance and safe redacted fields. |
| `result_gap` | ÐŸÐ¾ÑÐ»Ðµ Ð¾Ñ‚Ð²ÐµÑ‚Ð° AI Ð¿Ð¾Ð½ÑÑ‚ÑŒ, Ñ‡ÐµÐ³Ð¾ Ð½Ðµ Ñ…Ð²Ð°Ñ‚Ð¸Ð»Ð¾ Ð¸ Ð½ÑƒÐ¶ÐµÐ½ Ð»Ð¸ second pass/adaptive DPO. |
| `output_validation` | ÐŸÑ€Ð¾Ð²ÐµÑ€Ð¸Ñ‚ÑŒ, ÑÐ¾Ð¾Ñ‚Ð²ÐµÑ‚ÑÑ‚Ð²ÑƒÐµÑ‚ Ð»Ð¸ Ð¾Ñ‚Ð²ÐµÑ‚ Ð·Ð°Ð´Ð°Ñ‡Ðµ and constraints. |
| `task_completion` | ÐÐµ Ð´Ð°Ñ‚ÑŒ ÑÐ¸ÑÑ‚ÐµÐ¼Ðµ ÑÐºÐ°Ð·Ð°Ñ‚ÑŒ "Ð³Ð¾Ñ‚Ð¾Ð²Ð¾", ÐµÑÐ»Ð¸ gap ÐµÑ‰Ñ‘ Ð¾ÑÑ‚Ð°Ð»ÑÑ. |
| `intermediate_checks` | ÐŸÐ¾Ð´Ð´ÐµÑ€Ð¶Ð¸Ð²Ð°Ñ‚ÑŒ ÐºÐ¾Ñ€Ð¾Ñ‚ÐºÐ¸Ðµ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÐºÐ¸ Ð¼ÐµÐ¶Ð´Ñƒ ÑˆÐ°Ð³Ð°Ð¼Ð¸, Ñ‡Ñ‚Ð¾Ð±Ñ‹ Ð½Ðµ ÑƒÑ…Ð¾Ð´Ð¸Ñ‚ÑŒ Ð² Ð½ÐµÐ²ÐµÑ€Ð½Ð¾Ðµ Ð½Ð°Ð¿Ñ€Ð°Ð²Ð»ÐµÐ½Ð¸Ðµ. |

## A2. Ð“Ð´Ðµ ÑÑ‚Ð¾ ÑÐµÐ¹Ñ‡Ð°Ñ ÑÐ¸Ð´Ð¸Ñ‚ Ð² ÐºÐ¾Ð´Ðµ: Ñ‡ÐµÑÑ‚Ð½Ñ‹Ð¹ ÑÑ‚Ð°Ñ‚ÑƒÑ

Ð’ÑÐµ ÑÑÑ‹Ð»ÐºÐ¸ Ð½Ð¸Ð¶Ðµ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÐµÐ½Ñ‹ Ð¿Ð¾ Ñ‚ÐµÐºÑƒÑ‰ÐµÐ¼Ñƒ Ð»Ð¾ÐºÐ°Ð»ÑŒÐ½Ð¾Ð¼Ñƒ ÐºÐ¾Ð´Ñƒ. ÐŸÑ€Ð°Ð²Ð¸Ð»Ð¾ ÑÑÑ‹Ð»Ð¾Ðº Ð² ÑÑ‚Ð¾Ð¹ Ñ‚Ð°Ð±Ð»Ð¸Ñ†Ðµ: ÐºÐ°Ð¶Ð´Ð°Ñ Ñ†Ð¸Ñ‚Ð°Ñ‚Ð° Ð´Ð¾Ð»Ð¶Ð½Ð° Ð±Ñ‹Ñ‚ÑŒ Ð¿Ð¾Ð»Ð½Ñ‹Ð¼ `file:line` Ð¸Ð»Ð¸ `file:line-line`; ÐºÐ¾Ñ€Ð¾Ñ‚ÐºÐ¸Ðµ ÑÑÑ‹Ð»ÐºÐ¸ Ð²Ð¸Ð´Ð° "colon + Ð½Ð¾Ð¼ÐµÑ€ ÑÑ‚Ñ€Ð¾ÐºÐ¸ Ð±ÐµÐ· Ñ„Ð°Ð¹Ð»Ð°" Ð·Ð°Ð¿Ñ€ÐµÑ‰ÐµÐ½Ñ‹, Ð¿Ð¾Ñ‚Ð¾Ð¼Ñƒ Ñ‡Ñ‚Ð¾ review Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ Ð²Ð¾Ð·Ð¼Ð¾Ð¶ÐµÐ½ Ð±ÐµÐ· Ð´Ð¾Ð³Ð°Ð´Ð¾Ðº.

| ÐÐ»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼ | Ð¡Ñ‚Ð°Ñ‚ÑƒÑ | Ð¢Ð¾Ñ‡Ð½Ð°Ñ ÑÑÑ‹Ð»ÐºÐ° Ð¸ ÐºÐ¾Ñ€Ð¾Ñ‚ÐºÐ°Ñ Ñ†Ð¸Ñ‚Ð°Ñ‚Ð° |
|---|---|---|
| Prompt -> task | **Ð¡ÐšÐ•Ð›Ð•Ð¢ / semantic model ÐÐ• Ð Ð•ÐÐ›Ð˜Ð—ÐžÐ’ÐÐ** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\PromptToTaskMachine.cs:21` - `not_trained_no_loaded_state`; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\PromptToTaskMachine.cs:23` - `loaded_state_ready_no_inference_implemented`; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\PromptToTaskMachine.cs:44` - `TaskType: NotReadyTaskType`. |
| Task -> orchestration plan | **Ð¡ÐšÐ•Ð›Ð•Ð¢ / semantic model ÐÐ• Ð Ð•ÐÐ›Ð˜Ð—ÐžÐ’ÐÐ** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\OrchestrationReasoningMachine.cs:22` - `not_trained_no_learned_guide_state`; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\OrchestrationReasoningMachine.cs:24` - `loaded_guide_state_ready_no_inference_implemented`; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\OrchestrationReasoningMachine.cs:42` - `ScenarioId: NoInferenceScenarioId`. |
| Ð£Ð¶Ðµ ÑÑƒÑ‰ÐµÑÑ‚Ð²ÑƒÑŽÑ‰Ð¸Ð¹ learned orchestration flow grounding | **Ð§ÐÐ¡Ð¢Ð˜Ð§ÐÐž Ð Ð•ÐÐ›Ð¬ÐÐ«Ð™ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ñ‹Ð¹ Ð¿ÑƒÑ‚ÑŒ, Ð½Ð¾ ÑÑ‚Ð¾ ProjectWikiDomain, Ð½Ðµ Ð½Ð¾Ð²Ð°Ñ ReasoningMachine** | `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\OrchestrationPlanMachine.cs:86` - `_promptFlowGroundingModel.TryGround`; `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\OrchestrationPlanMachine.cs:94` - `RenderLearnedFlowPlan`; `C:\Projects\llm_mvp_core\42.ProjectWikiDomain\OrchestrationPlanMachine.cs:178-181` - `_promptFlowGroundingModel.Train(pair)`. |
| Task -> skill selection | **Ð§ÐÐ¡Ð¢Ð˜Ð§ÐÐž / Ð¿Ð¾Ð·Ð¸Ñ†Ð¸Ð¾Ð½Ð½Ñ‹Ð¹ Ñ‡Ð°ÑÑ‚Ð¾Ñ‚Ð½Ñ‹Ð¹ ÑÑ‡Ñ‘Ñ‚Ñ‡Ð¸Ðº (`_candidatePositionWeights[index]++`), ÐÐ• ÑÐµÐ¼Ð°Ð½Ñ‚Ð¸Ñ‡ÐµÑÐºÐ¸Ð¹ selector** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:41-52` trains candidate-position weights; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:57-88` decides from `_candidatePositionWeights`; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:84` reports `TRAINED_CANDIDATE_POSITION_WEIGHT`. |
| Skill-selection asset lifecycle | **Ð§ÐÐ¡Ð¢Ð˜Ð§ÐÐž Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½ Ð´Ð»Ñ SkillSelectionMachine** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:158-170` exports through `SaveCheckpointAsync`; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:211-256` writes checkpoint and manifest; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionMachine.cs:267-280` loads checkpoint. |
| Result gap after AI response | **BOOTSTRAP / Ð½Ðµ semantic authority** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:171-224` creates second-pass context and `RESULT_GAP_DPO_ROW`; evidence mode is `bootstrap_test_harness_not_semantic_authority`. |
| Completion decision | **BOOTSTRAP / Ð½Ðµ Ð¿Ð¾Ð»Ð½Ñ‹Ð¹ trainable completion unit** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:228-250` blocks false done when skill gap remains. |
| User correction -> DPO signal | **SIGNAL WRITER only / training loop ÐµÑ‰Ñ‘ Ð½ÐµÑ‚** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:280-316` writes `project-local:.xiigen/smart-skills/dpo-signals.jsonl` and `SemanticQualityClaimed: false`. |
| Common/adaptive upgrade preservation | **SCHEMA BOUNDARY / semantic upgrade ÐµÑ‰Ñ‘ Ð½Ðµ Ð¾Ð±ÑƒÑ‡ÐµÐ½** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:326-348` preserves local state and returns `schema_upgrade_boundary_not_semantic_authority`; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SkillSelectionContracts.cs:420-430` refreshes server entries. |
| Two-leg `ILearningMachinePair` contract | **ÐšÐžÐÐ¢Ð ÐÐšÐ¢ Ð•Ð¡Ð¢Ð¬ / smart-skills pair-adapters Ð¸ Ð¿ÐµÑ€ÐµÐ½Ð¾ÑÐ¸Ð¼Ñ‹Ð¹ user package ÐµÑ‰Ñ‘ ÐÐ• Ð Ð•ÐÐ›Ð˜Ð—ÐžÐ’ÐÐÐ«** | `C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs:185` - `public interface ILearningMachinePair<TPersonal, TGlobal>`; `C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs:188` - `Personal`; `C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs:191` - `Global`; `C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs:197` - `SyncPersonalToGlobalAsync`; `C:\Projects\llm_mvp_core\00.Core\LearningMachineContracts.cs:203` - `PullGlobalToPersonalAsync`. |
| Core API chain | **CORE BOUNDARY Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½, semantic quality Ð½Ðµ Ð·Ð°ÑÐ²Ð»ÑÐµÑ‚ÑÑ** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs:56-58` builds PromptToTask request and calls `Translate`; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs:70-71` calls `Plan`; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs:83` calls skill decision; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs:99` says `Semantic quality is not claimed`. |
| Thin request/response contract | **CONTRACT Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs:4-15` request fields; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs:18-28` response fields; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs:31-36` debug trace fields. |
| Debug trace chat shape | **CORE FORMAT Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½, host display missing** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiContracts.cs:72-90` returns `xiigen {debugTrace.Method} :`, `input:`, `output:`. |
| Core redacted debug payload | **CORE FORMAT Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\XiigenExtensionApiService.cs:433-474` redacts prompt text and returns `ContainsRawPrivateContent: false`, `ContainsSecrets: false`. |
| Phase 40 evidence contracts | **CONTRACT Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\Phase40ExtensionContracts.cs:28-116` defines host kind, prompt suite, trace, status, tool evidence, asset refs, anti-masquerade evidence. |
| 11 semantic units catalog | **CATALOG Ñ€ÐµÐ°Ð»Ð¸Ð·Ð¾Ð²Ð°Ð½; Ð±Ð¾Ð»ÑŒÑˆÐ¸Ð½ÑÑ‚Ð²Ð¾ unit implementations ÐµÑ‰Ñ‘ Ð½ÐµÑ‚** | `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SmartSkillsSemanticUnitCatalog.cs:16-28` lists expected domains; `C:\Projects\llm_mvp_core\43.SmartSkillsDomain\SmartSkillsSemanticUnitCatalog.cs:126-128` sets `Trainable: true`, `DeterministicFinalAuthority: false`, `RuntimeSemanticQualityClaimed: false`. |
| Claude real extension code | **ÐÐ• Ð Ð•ÐÐ›Ð˜Ð—ÐžÐ’ÐÐÐž Ð² ÑÑ‚Ð¾Ð¼ evidence set** | Must be created in private repo `claude-xiigen-smart-skills`; JSON/PowerShell substitute Ð·Ð°Ð¿Ñ€ÐµÑ‰Ñ‘Ð½. |
| Cursor real extension code | **ÐÐ• Ð Ð•ÐÐ›Ð˜Ð—ÐžÐ’ÐÐÐž Ð² ÑÑ‚Ð¾Ð¼ evidence set** | Must be created in private repo `cursor-xiigen-smart-skills`; JSON/PowerShell substitute Ð·Ð°Ð¿Ñ€ÐµÑ‰Ñ‘Ð½. |
| Codex real extension code | **ÐÐ• Ð Ð•ÐÐ›Ð˜Ð—ÐžÐ’ÐÐÐž Ð² ÑÑ‚Ð¾Ð¼ evidence set** | Must be created in private repo `codex-xiigen-smart-skills`; JSON/PowerShell substitute Ð·Ð°Ð¿Ñ€ÐµÑ‰Ñ‘Ð½. |

## A3. Ð“Ð´Ðµ Ð¶Ð¸Ð²ÑƒÑ‚ Ð¼Ð¾Ð´ÐµÐ»Ð¸, ÑÐµÑ‚Ñ‹ Ð¸ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ¸Ðµ Ð´Ð°Ð½Ð½Ñ‹Ðµ

| Ð§Ñ‚Ð¾ | Ð“Ð´Ðµ Ð¶Ð¸Ð²Ñ‘Ñ‚ | ÐŸÑ€Ð°Ð²Ð¸Ð»Ð¾ |
|---|---|---|
| Common PromptToTask schemas, seed DPO strategy, approved common DPO rows | `C:\Projects\llm_mvp_core\training-data\prompt-to-task\...` | Repo-owned, export/import/upgrade required |
| Common OrchestrationReasoning schemas, guide-state datasets, approved common DPO rows | `C:\Projects\llm_mvp_core\training-data\orchestration-reasoning\...` | Repo-owned, no raw private guide text |
| Common skill-selection training data/checkpoints | `C:\Projects\llm_mvp_core\training-data\skill-selection\...`, `C:\Projects\llm_mvp_core\models\skill-selection\...` | Repo-owned, manifest/locator/fresh-load/continue required |
| User adaptive prompt-to-task data/model | user side `.xiigen\prompt-to-task\...` | User-owned, common upgrade must not overwrite |
| User adaptive orchestration-reasoning data/model | user side `.xiigen\orchestration-reasoning\...` | User-owned, updated from feedback/result gaps |
| User adaptive smart-skills data/model | user side `.xiigen\smart-skills\...` | User-owned; current writer already points here |
| User adaptive backup/share package | user side `.xiigen\model-backups\...` or user-selected export path | User-owned portable package; must include manifest, checkpoint hash, schema version, provenance refs, redaction report, and restore/import probe |
| Cross-device restored adaptive model | user-selected fresh profile/device path | Must load without common checkpoint overwrite and must preserve user-model behavior evidence |
| Host extension package files | `claude-xiigen-smart-skills`, `cursor-xiigen-smart-skills`, `codex-xiigen-smart-skills` | Host-owned thin clients only |

## A4. ÐŸÐ¾Ñ‡ÐµÐ¼Ñƒ v2 Ð¸ ÑÑ‚Ð°Ñ€Ñ‹Ð¹ v20-style Ð¿Ð»Ð°Ð½ Ð±Ñ‹Ð»Ð¸ Ð½ÐµÐ´Ð¾ÑÑ‚Ð°Ñ‚Ð¾Ñ‡Ð½Ñ‹

`ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md` Ð¿Ð¾Ð»ÐµÐ·ÐµÐ½ Ð´Ð»Ñ learned orchestration, TranslationMachine asset lifecycle, smart-skills readiness and Phase 40 lessons. ÐÐ¾ Ð¾Ð½ Ð½Ðµ Ð¿Ñ€Ð¾Ñ…Ð¾Ð´Ð¸Ð» Ð½Ð¾Ð²Ñ‹Ð¹ readable-plan admission gate: Ð² Ð½Ð°Ñ‡Ð°Ð»Ðµ Ð½Ðµ Ð±Ñ‹Ð»Ð¾ Ð³Ð¾Ñ‚Ð¾Ð²Ð¾Ð¹ Ð´Ð»Ñ Ð›ÑƒÐ±Ñ‹ Part A Ñ PlantUML/draw.io, Ñ€ÑƒÑÑÐºÐ¸Ð¼Ð¸ Ð°Ð»Ð³Ð¾Ñ€Ð¸Ñ‚Ð¼Ð°Ð¼Ð¸ Ð¸ exact current-code refs.

`ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-PLAN-v2.md` Ð±Ð»Ð¸Ð¶Ðµ Ðº Ñ‚ÐµÐ¼Ðµ extensions, Ð½Ð¾ Ð¾Ð½ Ñ‚Ð¾Ð¶Ðµ Ð½ÐµÐ´Ð¾ÑÑ‚Ð°Ñ‚Ð¾Ñ‡ÐµÐ½ ÐºÐ°Ðº Ñ‚ÐµÐºÑƒÑ‰Ð¸Ð¹ approval artifact:

- ÐµÐ³Ð¾ Ð¼Ð¾Ð¶Ð½Ð¾ Ð±Ñ‹Ð»Ð¾ Ð¿Ñ€Ð¾Ñ‡Ð¸Ñ‚Ð°Ñ‚ÑŒ ÐºÐ°Ðº readiness/evidence tooling, Ð° Ð½Ðµ Ð¿Ð»Ð°Ð½ Ð½Ð°ÑÑ‚Ð¾ÑÑ‰Ð¸Ñ… host extensions;
- Ð¾Ð½ Ð½Ðµ ÑÑ‚Ð°Ð²Ð¸Ð» "JSON/PowerShell-only substitute forbidden" Ð¿ÐµÑ€Ð²Ñ‹Ð¼ gate Ð´Ð»Ñ ÐºÐ°Ð¶Ð´Ð¾Ð³Ð¾ host;
- Ð¾Ð½ ÑÐ¼ÐµÑˆÐ¸Ð²Ð°Ð» ÑÑ‚Ð°Ñ€Ñ‹Ð¹ state/evidence Ñ Ñ‚ÐµÐºÑƒÑ‰Ð¸Ð¼ authority;
- Ð¾Ð½ Ð½Ðµ Ð·Ð°ÐºÑ€Ñ‹Ð²Ð°Ð» plan-only lock Ð´Ð¾ÑÑ‚Ð°Ñ‚Ð¾Ñ‡Ð½Ð¾ ÑÐ²Ð½Ð¾;
- Ð¾Ð½ Ð½Ðµ Ð¾Ñ‚Ð´Ð°Ð²Ð°Ð» deterministic-shortcut sign-off Ñ€Ð¾Ð´Ð¸Ñ‚ÐµÐ»ÑŒÑÐºÐ¾Ð¼Ñƒ `REVIEW_INTERNAL` Ð´Ð»Ñ Ð²ÑÐµÐ¹ Ñ†ÐµÐ¿Ð¾Ñ‡ÐºÐ¸ prompt -> task -> plan -> skills -> feedback.

Ð­Ñ‚Ð¾Ñ‚ v3 Ð·Ð°Ð¼ÐµÐ½ÑÐµÑ‚ v2 ÐºÐ°Ðº Ð½Ð¾Ð²Ñ‹Ð¹ readable approval target.

## A5. Ð˜ÐµÑ€Ð°Ñ€Ñ…Ð¸Ñ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ð¹ ÑÐ¸ÑÑ‚ÐµÐ¼Ñ‹: Ð¾Ñ‚ Ð¼Ð°ÑˆÐ¸Ð½Ñ‹ Ð´Ð¾ Ð´Ð²ÑƒÑ… Ð½Ð¾Ð³

Ð­Ñ‚Ð¾Ñ‚ Ñ€Ð°Ð·Ð´ÐµÐ» Ð½ÑƒÐ¶ÐµÐ½, Ñ‡Ñ‚Ð¾Ð±Ñ‹ Part A Ð½Ðµ Ð±Ñ‹Ð»Ð° Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ð³Ð¾Ñ€Ð¸Ð·Ð¾Ð½Ñ‚Ð°Ð»ÑŒÐ½Ð¾Ð¹ ÑÑ…ÐµÐ¼Ð¾Ð¹ "prompt -> task -> plan -> skills". Ð—Ð´ÐµÑÑŒ Ð¿Ð¾ÐºÐ°Ð·Ð°Ð½Ð° Ð²ÐµÑ€Ñ‚Ð¸ÐºÐ°Ð»ÑŒ, ÐºÐ¾Ñ‚Ð¾Ñ€ÑƒÑŽ Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð²Ð¸Ð´ÐµÑ‚ÑŒ Ð¸ Ñ‡ÐµÐ»Ð¾Ð²ÐµÐº, Ð¸ Ð¸ÑÐ¿Ð¾Ð»Ð½Ð¸Ñ‚ÐµÐ»ÑŒ: Ñ‡Ñ‚Ð¾ ÑÐ²Ð»ÑÐµÑ‚ÑÑ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ð¹ Ð¼Ð°ÑˆÐ¸Ð½Ð¾Ð¹, ÐºÑ‚Ð¾ Ð¾Ñ€ÐºÐµÑÑ‚Ñ€Ð¸Ñ€ÑƒÐµÑ‚ Ð¿Ð¾Ñ‚Ð¾Ðº, ÐºÐ°ÐºÐ¸Ðµ units Ð²Ð½ÑƒÑ‚Ñ€Ð¸ Ñ€Ð°Ð±Ð¾Ñ‚Ð°ÑŽÑ‚, Ð¸ Ð³Ð´Ðµ Ñƒ ÐºÐ°Ð¶Ð´Ð¾Ð¹ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ð¹ ÑÑ„ÐµÑ€Ñ‹ Ð´Ð²Ðµ Ð½Ð¾Ð³Ð¸.

### A5.1 ÐŸÑ€Ð¾ÑÑ‚Ð°Ñ Ð²ÐµÑ€Ñ‚Ð¸ÐºÐ°Ð»ÑŒÐ½Ð°Ñ UML-Ð´Ð¸Ð°Ð³Ñ€Ð°Ð¼Ð¼Ð°

Ð˜Ð¼Ð¿Ð¾Ñ€Ñ‚ Ð² draw.io: **Arrange -> Insert -> Advanced -> PlantUML**.

```plantuml
@startuml
title XIIGen trainable hierarchy - machine / orchestrator / flow / unit / pair

skinparam classAttributeIconSize 0

class "ILearningMachine<TDomain>\nÐ¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ ÑÑ„ÐµÑ€Ð° Ñ†ÐµÐ»Ð¸ÐºÐ¾Ð¼" as Machine {
  input: domain request
  output: domain response
  asset: checkpoint + manifest
}

class "ILogicalOrchestrator\nÐ²Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ flow Ð¸ Ð¿Ð¾Ñ€ÑÐ´Ð¾Ðº" as Orchestrator {
  input: structured task
  output: selected flow route
}

class "ILogicalFlow<TInput>\nÑ†ÐµÐ¿Ð¾Ñ‡ÐºÐ° units" as Flow {
  input: flow input
  output: flow result
}

class "ILogicalUnit<TInput,TOutput>\nÐ¾Ð´Ð½Ð° Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ñ" as Unit {
  input: one typed slice
  output: one typed result
}

class "ILearningMachinePair<TPersonal,TGlobal>\ncommon leg + personal leg" as Pair {
  global/common model
  personal/adaptive model
  backup/export/restore/share
}

Machine --> Orchestrator : uses
Orchestrator --> Flow : selects
Flow --> Unit : runs ordered units
Machine --> Pair : must expose two legs
Pair --> Machine : loads common + personal state

@enduml
```

### A5.2 Ð§Ñ‚Ð¾ ÑÑ‚Ð¾ Ð·Ð½Ð°Ñ‡Ð¸Ñ‚ Ñ‡ÐµÐ»Ð¾Ð²ÐµÑ‡ÐµÑÐºÐ¸Ð¼ ÑÐ·Ñ‹ÐºÐ¾Ð¼

`ILearningMachine<TDomain>` - ÑÑ‚Ð¾ Ð½Ðµ Ð¾Ð´Ð¸Ð½ Ð¼ÐµÑ‚Ð¾Ð´ Ð¸ Ð½Ðµ Ð¾Ð´Ð¸Ð½ JSON. Ð­Ñ‚Ð¾ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ ÑÑ„ÐµÑ€Ð° Ñ†ÐµÐ»Ð¸ÐºÐ¾Ð¼: Ð½Ð°Ð¿Ñ€Ð¸Ð¼ÐµÑ€ `PromptToTaskMachine`, `OrchestrationReasoningMachine`, `SkillSelectionMachine` Ð¸Ð»Ð¸ `ResultGapMachine`. ÐžÐ½Ð° Ð¿Ð¾Ð»ÑƒÑ‡Ð°ÐµÑ‚ Ð·Ð°Ð¿Ñ€Ð¾Ñ ÑÐ²Ð¾ÐµÐ³Ð¾ Ð´Ð¾Ð¼ÐµÐ½Ð° Ð¸ Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°ÐµÑ‚ Ð¾Ñ‚Ð²ÐµÑ‚ ÑÐ²Ð¾ÐµÐ³Ð¾ Ð´Ð¾Ð¼ÐµÐ½Ð°.

`ILogicalOrchestrator` - ÑÑ‚Ð¾ ÑÐ»Ð¾Ð¹, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ñ€ÐµÑˆÐ°ÐµÑ‚, ÐºÐ°ÐºÐ¾Ð¹ Ð¿Ð¾Ñ‚Ð¾Ðº Ð½ÑƒÐ¶ÐµÐ½ ÑÐµÐ¹Ñ‡Ð°Ñ. Ð’ ÑÑ‚Ð¾Ð¹ extension-Ð·Ð°Ð´Ð°Ñ‡Ðµ Ð¾Ð½ Ð½Ðµ Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ Ð±Ð¾Ð»ÑŒÑˆÐ¸Ð¼ `if` Ð¸Ð»Ð¸ BM25 lookup. ÐžÐ½ Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð¾Ð¿Ð¸Ñ€Ð°Ñ‚ÑŒÑÑ Ð½Ð° Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ðµ ÑÐ¾ÑÑ‚Ð¾ÑÐ½Ð¸Ðµ, Ð° ÐµÑÐ»Ð¸ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ðµ ÑÐ¾ÑÑ‚Ð¾ÑÐ½Ð¸Ðµ Ð¾Ñ‚ÑÑƒÑ‚ÑÑ‚Ð²ÑƒÐµÑ‚, Ñ‡ÐµÑÑ‚Ð½Ð¾ Ð²ÐµÑ€Ð½ÑƒÑ‚ÑŒ "not trained / no semantic claim".

`ILogicalFlow<TInput>` - ÑÑ‚Ð¾ Ð¼Ð°Ñ€ÑˆÑ€ÑƒÑ‚ Ð²Ð½ÑƒÑ‚Ñ€Ð¸ Ð¾Ð´Ð½Ð¾Ð¹ Ð¼Ð°ÑˆÐ¸Ð½Ñ‹: Ð½Ð°Ð¿Ñ€Ð¸Ð¼ÐµÑ€ "Ð¿ÐµÑ€ÐµÐ²ÐµÑÑ‚Ð¸ prompt Ð² task", "Ð½Ð°Ð¹Ñ‚Ð¸ orchestration scenario", "Ð¿Ð¾Ð´Ð¾Ð±Ñ€Ð°Ñ‚ÑŒ skills", "Ð¿Ð¾ÑÐ»Ðµ Ð¾Ñ‚Ð²ÐµÑ‚Ð° AI Ð½Ð°Ð¹Ñ‚Ð¸ gap".

`ILogicalUnit<TInput,TOutput>` - ÑÑ‚Ð¾ Ð¼Ð°Ð»ÐµÐ½ÑŒÐºÐ°Ñ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ñ Ð²Ð½ÑƒÑ‚Ñ€Ð¸ flow: Ð²Ñ‹Ð´ÐµÐ»Ð¸Ñ‚ÑŒ intent, Ð²Ñ‹Ð´ÐµÐ»Ð¸Ñ‚ÑŒ constraints, Ð²Ñ‹Ð±Ñ€Ð°Ñ‚ÑŒ skill family, Ð¿Ñ€Ð¾Ð²ÐµÑ€Ð¸Ñ‚ÑŒ missing pattern, Ð¿Ð¾ÑÑ‚Ñ€Ð¾Ð¸Ñ‚ÑŒ DPO signal. Unit Ð¼Ð¾Ð¶ÐµÑ‚ Ð¸Ð¼ÐµÑ‚ÑŒ deterministic Ð¸Ð½Ñ„Ñ€Ð°ÑÑ‚Ñ€ÑƒÐºÑ‚ÑƒÑ€Ñƒ Ð²Ð¾ÐºÑ€ÑƒÐ³ ÑÐµÐ±Ñ: parse, validate, redact, save/load. ÐÐ¾ Ñ€ÐµÑˆÐµÐ½Ð¸Ðµ, ÐºÐ¾Ñ‚Ð¾Ñ€Ð¾Ðµ Ð¿Ð»Ð°Ð½ Ð½Ð°Ð·Ñ‹Ð²Ð°ÐµÑ‚ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ñ‹Ð¼, Ð½Ðµ Ð¼Ð¾Ð¶ÐµÑ‚ Ð±Ñ‹Ñ‚ÑŒ hardcoded lookup.

`ILearningMachinePair<TPersonal,TGlobal>` - ÑÑ‚Ð¾ Ð¿Ñ€Ð°Ð²Ð¸Ð»Ð¾ Ð´Ð²ÑƒÑ… Ð½Ð¾Ð³. Ð£ ÐºÐ°Ð¶Ð´Ð¾Ð¹ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð¾Ð¹ ÑÑ„ÐµÑ€Ñ‹ ÐµÑÑ‚ÑŒ common/global leg Ð² `llm_mvp_core` Ð¸ personal/adaptive leg Ñƒ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ. Common leg Ð¼Ð¾Ð¶Ð½Ð¾ Ð¾Ð±Ð½Ð¾Ð²Ð»ÑÑ‚ÑŒ Ð¸Ð· Ð½Ð°ÑˆÐµÐ³Ð¾ Ñ€ÐµÐ¿Ð¾Ð·Ð¸Ñ‚Ð¾Ñ€Ð¸Ñ. Personal leg Ð¼Ð¾Ð¶Ð½Ð¾ Ð¾Ð±ÑƒÑ‡Ð°Ñ‚ÑŒ Ð½Ð° ÑÑ‚Ð¸Ð»Ðµ Ð¸ Ð¸ÑÐ¿Ñ€Ð°Ð²Ð»ÐµÐ½Ð¸ÑÑ… Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ, Ð±ÑÐºÐ°Ð¿Ð¸Ñ‚ÑŒ, Ð¿ÐµÑ€ÐµÐ½Ð¾ÑÐ¸Ñ‚ÑŒ, Ð¸Ð¼Ð¿Ð¾Ñ€Ñ‚Ð¸Ñ€Ð¾Ð²Ð°Ñ‚ÑŒ Ð¸ ÑˆÐ°Ñ€Ð¸Ñ‚ÑŒ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ñ‡ÐµÑ€ÐµÐ· Ð±ÐµÐ·Ð¾Ð¿Ð°ÑÐ½Ñ‹Ð¹ user-approved/anonymised package.

### A5.3 Ð¢Ð°Ð±Ð»Ð¸Ñ†Ð° Ñ†ÐµÐ¿Ð¾Ñ‡ÐµÐº Ð¸ÑÐ¿Ð¾Ð»Ð½ÐµÐ½Ð¸Ñ

| Ð¡Ñ„ÐµÑ€Ð° | Machine input | Machine output | Flow / units that must exist | Pair legs |
|---|---|---|---|---|
| PromptToTask | user prompt + redacted context + current human authority | structured task with constraints, deliverables, permission boundary | intent unit, constraint unit, deliverable unit, human-authority boundary unit | common prompt-to-task model + user adaptive prompt preference model |
| OrchestrationReasoning | structured task + allowed reference summaries | orchestration scenario / plan shape / risk list | guide-pattern unit, plan-composition unit, risk/gate unit, no-hidden-determinism unit | common guide-reasoning model + user adaptive orchestration style model |
| SkillSelection | structured task + scenario + available skills | selected skills with reasons and when-not-to-use warnings | skill meaning unit, skill combination unit, wrong-skill rejection unit, user preference unit | common skill semantics model + user adaptive skill preference model |
| ResultGap | AI output + expected task/plan/skills + user correction if any | missing-pattern or correction DPO row | gap detector unit, correction-to-DPO unit, privacy/redaction unit, adaptive-training trigger unit | common result-gap schema + user adaptive feedback model |

### A5.4 A5 gate

Part B is invalid if it introduces a new trainable machine, flow, unit or pair that is not visible in A5. If implementation later discovers a different hierarchy, the plan must return to architect review before code execution continues.

---

# Part B - Ð¿Ð»Ð°Ð½ Ð¸ÑÐ¿Ð¾Ð»Ð½ÐµÐ½Ð¸Ñ Ð¿Ð¾ÑÐ»Ðµ Ð¿Ñ€Ð¸Ð½ÑÑ‚Ð¸Ñ Ð›ÑƒÐ±Ð¾Ð¹

## B0. Plan-only lock

Ð­Ñ‚Ð¾Ñ‚ Ð´Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚ Ð½Ðµ Ñ€Ð°Ð·Ñ€ÐµÑˆÐ°ÐµÑ‚ implementation. ÐŸÐ¾ÑÐ»Ðµ Ð¿Ñ€Ð¸Ð½ÑÑ‚Ð¸Ñ Ð›ÑƒÐ±Ð¾Ð¹ Ñ€Ð°Ð±Ð¾Ñ‚Ð° Ð¸Ð´Ñ‘Ñ‚ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ñ‡ÐµÑ€ÐµÐ· bounded sub-agent work orders, Ð¿Ð¾ Ð¾Ð´Ð½Ð¾Ð¼Ñƒ Ð·Ð° Ñ€Ð°Ð·. Parent/current session Ð¾ÑÑ‚Ð°Ñ‘Ñ‚ÑÑ architect/reviewer.

ÐšÐ°Ð¶Ð´Ñ‹Ð¹ work order Ð¾Ð±ÑÐ·Ð°Ð½ Ð¸Ð¼ÐµÑ‚ÑŒ:

| ÐŸÐ¾Ð»Ðµ | ÐŸÑ€Ð°Ð²Ð¸Ð»Ð¾ |
|---|---|
| Requested artifact type | plan/governance/code/runtime/training/asset/check |
| Allowed write scope | exact files or exact repo folder |
| Maximum active time | 15 minutes or less |
| Implementation authorized now | yes only after Luba accepts this plan |
| Luba approval quote | exact current conversation quote |
| Quoted material authority | evidence only |
| Answer-first interrupt | direct Luba question stops substantive work until answered |

ÐÐ¸ Ð¾Ð´Ð¸Ð½ work order Ð½Ðµ Ð¼Ð¾Ð¶ÐµÑ‚ Ð¾Ð±ÑŠÐµÐ´Ð¸Ð½ÑÑ‚ÑŒ Ð±Ð¾Ð»ÑŒÑˆÐµ Ð¾Ð´Ð½Ð¾Ð³Ð¾ trainable algorithm Ð¸Ð»Ð¸ Ð¾Ð´Ð½Ð¾Ð³Ð¾ lifecycle slice.

## B0.1 Reusable bounded sub-agent packet template

ÐšÐ°Ð¶Ð´Ñ‹Ð¹ Ð±ÑƒÐ´ÑƒÑ‰Ð¸Ð¹ executor packet Ð¾Ð±ÑÐ·Ð°Ð½ ÐºÐ¾Ð¿Ð¸Ñ€Ð¾Ð²Ð°Ñ‚ÑŒ ÑÑ‚Ð¸ Ð¿Ð¾Ð»Ñ Ð±ÑƒÐºÐ²Ð°Ð»ÑŒÐ½Ð¾. Ð•ÑÐ»Ð¸ Ñ…Ð¾Ñ‚Ñ Ð±Ñ‹ Ð¾Ð´Ð½Ð¾ Ð¿Ð¾Ð»Ðµ Ð¾Ñ‚ÑÑƒÑ‚ÑÑ‚Ð²ÑƒÐµÑ‚, packet Ð½Ðµ Ð´Ð°Ñ‘Ñ‚ authority Ð½Ð° Ð¸ÑÐ¿Ð¾Ð»Ð½ÐµÐ½Ð¸Ðµ.

```text
Requested artifact type: <plan|review|governance|code|runtime|training|asset|check>
Allowed write scope: <exact files / exact repo folder>
Maximum active time: <=15 minutes
Implementation authorized now: <yes/no>
Exact Luba approval quote if implementation follows plan-only artifact: <exact current-turn quote or NONE>
Quoted transcripts/state/old plans are evidence only: yes
Direct Luba question interrupt / answer-first rule included: yes
Forbidden commands/tools: <dotnet/build/test/run/training/host/browser/external AI/etc. as applicable>
DPO-KNOWLEDGE-MAP artifact required/applicable: <path or not_applicable_reason>
DPO-UNIVERSALIZATION-GATE artifact required/applicable: <path or not_applicable_reason>
TEST-STRATEGY-ADMISSION artifact required: <path>
UML-EXECUTABLE-CASE-MAP artifact required/applicable: <path or not_applicable_reason>
AUTHORITY-SELF-REFLECTION-GATE artifact required: <path>
CHECKPOINT-WRITER-GUARD artifact required/applicable: <path or not_applicable_reason>
METRIC-HONESTY-GATE artifact required/applicable: <path or not_applicable_reason>
HUMAN-AUTHORITY-BOUNDARY-GATE artifact required/applicable: <path or not_applicable_reason>
SUBAGENT-PACKET-QUALITY-GATE artifact required: <path>
TEST-EXECUTION-HYGIENE-GATE artifact required/applicable: <path or not_applicable_reason>
Required skill load list: <conversation-intent/self-reflection, architect-behavior-classifier, plan-review, skill-injection, implementation-integrity, phase-completion as applicable>
Drift prevention checks: executor drift, defensive freeze, action-as-relief, focus-loss, entropy tolerance
Review verdict options: <ALLOW_FINAL|CONTINUE_EXECUTION|REPAIR_REQUIRED|OPERATOR_INPUT_REQUIRED>
Self-approval forbidden: yes
Review stale after mutation: yes
Do not commit/push unless this packet is explicitly commit/push-only or the accepted phase gate requires it.
```

Current next action for this document is **WO-00 parent REVIEW_INTERNAL after Luba reads the plan**. WO-00 is review only. It does not authorize implementation. All implementation work orders below are dormant until Luba accepts this plan in the current conversation and issues a fresh implementation approval.

## B0.2 Gate wording and validator creation rule

Gate text must name a reviewable artifact and a concrete check phrase. Generic gates such as "looks good", "passes review", "runtime check", or "validator passes" are invalid unless the row names the exact artifact/report and the exact condition to inspect.

If a row needs a validator that does not exist yet, the plan must first add a bounded validator-creation work order. The plan must not invent a command for a tool that does not exist.

## B0.3 Mandatory admission layer for every work order

ÐšÐ°Ð¶Ð´Ñ‹Ð¹ work order Ð½Ð¸Ð¶Ðµ Ð¿Ð¾Ð»ÑƒÑ‡Ð°ÐµÑ‚ executor authority Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ð¿Ð¾ÑÐ»Ðµ admission review. Ð­Ñ‚Ð¾ Ð¿Ñ€Ð°Ð²Ð¸Ð»Ð¾ Ð¿Ñ€Ð¸Ð¼ÐµÐ½ÑÐµÑ‚ÑÑ Ð´Ð°Ð¶Ðµ Ðº Ð¼Ð°Ð»ÐµÐ½ÑŒÐºÐ¸Ð¼ work orders Ð´Ð¾ 15 Ð¼Ð¸Ð½ÑƒÑ‚: Ð¼Ð°Ð»ÐµÐ½ÑŒÐºÐ¸Ð¹ Ñ€Ð°Ð·Ð¼ÐµÑ€ Ð½Ðµ Ð¾Ñ‚Ð¼ÐµÐ½ÑÐµÑ‚ DPO knowledge, test strategy, UML executable cases Ð¸ skill-load/self-reflection.

### B0.3.1 `DPO-KNOWLEDGE-MAP`

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ Ð´Ð»Ñ Ð²ÑÐµÑ… work orders Ñ Ñ‚Ð¸Ð¿Ð¾Ð¼ `training`, `adaptive`, `DPO`, `asset`, `model`, Ð° Ñ‚Ð°ÐºÐ¶Ðµ Ð´Ð»Ñ code/check work orders, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ðµ Ð¼ÐµÐ½ÑÑŽÑ‚ trainable path.

Artifact: `artifacts/dpo-knowledge-map/<work_order>.json`.

ÐœÐ¸Ð½Ð¸Ð¼Ð°Ð»ÑŒÐ½Ñ‹Ðµ Ð¿Ð¾Ð»Ñ:

| ÐŸÐ¾Ð»Ðµ | ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÑŒÐ½Ð¾Ðµ ÑÐ¾Ð´ÐµÑ€Ð¶Ð°Ð½Ð¸Ðµ |
|---|---|
| `loaded_dpo_skills` | ÐšÐ°ÐºÐ¸Ðµ DPO/training-data/negative-sample skills Ð·Ð°Ð³Ñ€ÑƒÐ¶ÐµÐ½Ñ‹. |
| `guide_paths` | ÐÐ±ÑÐ¾Ð»ÑŽÑ‚Ð½Ñ‹Ðµ Ð¿ÑƒÑ‚Ð¸ Ðº strategy docs, source guides, prompt/example roots Ð¸Ð»Ð¸ hash-only refs. |
| `chosen_schema` | ÐŸÐ¾Ð»Ñ chosen side, Ð²ÐºÐ»ÑŽÑ‡Ð°Ñ provenance/ref/source safety. |
| `rejected_schema` | ÐŸÐ¾Ð»Ñ rejected side, including `[WRONG:]`/error taxonomy and provenance. |
| `taxonomy` | `ORCH-*`, `PromptError-*`, skill-selection taxonomy Ð¸Ð»Ð¸ adaptive taxonomy. |
| `registry_fields` | model id, dataset id, checkpoint id, manifest ref, locator ref, training-data ref. |
| `train_ready_validation` | ÐšÐ°Ðº gate Ð´Ð¾ÐºÐ°Ð·Ñ‹Ð²Ð°ÐµÑ‚, Ñ‡Ñ‚Ð¾ rows train-ready Ð¸ Ð½Ðµ ÑÐ²Ð»ÑÑŽÑ‚ÑÑ placeholders. |
| `held_out_evidence` | Held-out set, leakage/no-overlap gate, exact-memory separation. |
| `fresh_load_evidence` | ÐšÐ°Ðº fresh process/profile loads model/dataset/manifest. |
| `continue_training_evidence` | ÐšÐ°Ðº continue probe Ð´Ð¾ÐºÐ°Ð·Ñ‹Ð²Ð°ÐµÑ‚ no replay and same identity type. |

Ð—Ð°Ð¿Ñ€ÐµÑ‰ÐµÐ½Ð¾: Ð½Ð°Ñ‡Ð¸Ð½Ð°Ñ‚ÑŒ training/adaptive/model work, ÐµÑÐ»Ð¸ `DPO-KNOWLEDGE-MAP` Ð¾Ñ‚ÑÑƒÑ‚ÑÑ‚Ð²ÑƒÐµÑ‚ Ð¸Ð»Ð¸ ÑÐ¾Ð´ÐµÑ€Ð¶Ð¸Ñ‚ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ taxonomy Ð±ÐµÐ· chosen/rejected material. Ð’ Ñ‚Ð°ÐºÐ¾Ð¼ ÑÐ»ÑƒÑ‡Ð°Ðµ next action Ð½Ðµ "ÑÐ¿Ñ€Ð¾ÑÐ¸Ñ‚ÑŒ Ð›ÑƒÐ±Ñƒ", Ð° ÑÐ½Ð°Ñ‡Ð°Ð»Ð° Ð¸ÑÐºÐ°Ñ‚ÑŒ approved examples, strategy docs, local indexes Ð¸ Ð»Ð¸Ñ†ÐµÐ½Ð·Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð½Ñ‹Ðµ Ð¸ÑÑ‚Ð¾Ñ‡Ð½Ð¸ÐºÐ¸, Ð·Ð°Ñ‚ÐµÐ¼ Ð²Ð¾Ð·Ð²Ñ€Ð°Ñ‰Ð°Ñ‚ÑŒÑÑ Ñ Ñ‡ÐµÑÑ‚Ð½Ñ‹Ð¼ PASS/NOT_READY.

### B0.3.2 `DPO-UNIVERSALIZATION-GATE`

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ Ð´Ð»Ñ Ð²ÑÐµÑ… common Ð¸ adaptive DPO work orders, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ðµ Ð¾Ð±ÑƒÑ‡Ð°ÑŽÑ‚ `OrchestrationReasoningMachine`, `PromptToTaskMachine`, `SkillSelectionMachine`, `ResultGapMachine` Ð¸Ð»Ð¸ Ð»ÑŽÐ±ÑƒÑŽ Ð¸Ñ… personal/adaptive Ð½Ð¾Ð³Ñƒ.

Artifact: `artifacts/dpo-universalization/<work_order>.json`.

Ð¦ÐµÐ»ÑŒ gate: Ð²Ñ‹Ñ€ÐµÐ·Ð°Ñ‚ÑŒ Ð¸Ð· Ð¸ÑÑ‚Ð¾Ñ‡Ð½Ð¸ÐºÐ¾Ð² Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ñ‚Ñƒ Ñ‡Ð°ÑÑ‚ÑŒ, ÐºÐ¾Ñ‚Ð¾Ñ€Ð°Ñ ÑƒÐ½Ð¸Ð²ÐµÑ€ÑÐ°Ð»ÑŒÐ½Ð° Ð¸ Ð½Ðµ Ð¿Ñ€Ð¸Ð²ÑÐ·Ð°Ð½Ð° Ð½Ð°Ð¿Ñ€ÑÐ¼ÑƒÑŽ Ðº ÐºÐ¾Ð´Ñƒ, ÐºÐ¾Ð¼Ð°Ð½Ð´Ð°Ð¼, ÐºÐ¾Ð½ÐºÑ€ÐµÑ‚Ð½Ñ‹Ð¼ Ñ„Ð°Ð¹Ð»Ð°Ð¼ Ð¸Ð»Ð¸ host implementation. Ð¢ÐµÑ…Ð½Ð¸Ñ‡ÐµÑÐºÐ¾Ðµ Ð¸ÑÐ¿Ð¾Ð»Ð½ÐµÐ½Ð¸Ðµ Ð½Ðµ Ð²Ñ‹Ð±Ñ€Ð°ÑÑ‹Ð²Ð°ÐµÑ‚ÑÑ: Ð¾Ð½Ð¾ ÑÐ¾Ñ…Ñ€Ð°Ð½ÑÐµÑ‚ÑÑ ÐºÐ°Ðº `project_adapter_refs` Ð¸Ð»Ð¸ `excluded_technical_execution_refs`, Ñ‡Ñ‚Ð¾Ð±Ñ‹ Ð¿Ð¾Ð·Ð¶Ðµ Ð¾Ð±ÑƒÑ‡Ð°Ñ‚ÑŒ Ð¾Ñ‚Ð´ÐµÐ»ÑŒÐ½ÑƒÑŽ technical-execution model.

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÑŒÐ½Ð°Ñ ÐºÐ»Ð°ÑÑÐ¸Ñ„Ð¸ÐºÐ°Ñ†Ð¸Ñ ÐºÐ°Ð¶Ð´Ð¾Ð³Ð¾ source segment:

| Segment type | ÐœÐ¾Ð¶Ð½Ð¾ Ð»Ð¸ Ð² common/adaptive reasoning DPO ÑÐµÐ¹Ñ‡Ð°Ñ | Ð§Ñ‚Ð¾ ÑÑ‚Ð¾ Ð·Ð½Ð°Ñ‡Ð¸Ñ‚ |
|---|---|---|
| `universal_reasoning` | Ð”Ð° | Ð¢Ð¸Ð¿ Ð·Ð°Ð´Ð°Ñ‡Ð¸, Ñ€Ð°Ð·Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ Ð½Ð° ÑÑ‚Ð°Ð¿Ñ‹, Ñ€Ð¾Ð»Ð¸, gates, Ð³Ñ€Ð°Ð½Ð¸Ñ†Ñ‹ Ñ€Ð°Ð·Ñ€ÐµÑˆÐµÐ½Ð¸Ð¹, feedback loop, ÐºÑ€Ð¸Ñ‚ÐµÑ€Ð¸Ð¸ Ð¿Ð¾Ð»Ð½Ð¾Ñ‚Ñ‹. |
| `universal_skill_semantics` | Ð”Ð° | ÐšÐ¾Ð³Ð´Ð° Ð½ÑƒÐ¶ÐµÐ½ skill, ÐºÐ°ÐºÑƒÑŽ Ð¿Ñ€Ð¾Ð±Ð»ÐµÐ¼Ñƒ Ð¾Ð½ Ñ€ÐµÑˆÐ°ÐµÑ‚, Ð²Ñ…Ð¾Ð´Ñ‹/Ð²Ñ‹Ñ…Ð¾Ð´Ñ‹, Ð¾ÑˆÐ¸Ð±ÐºÐ¸, ÑÐ¾Ñ‡ÐµÑ‚Ð°Ð½Ð¸Ñ Ñ Ð´Ñ€ÑƒÐ³Ð¸Ð¼Ð¸ skills, when-not-to-use. |
| `project_adapter` | ÐÐµÑ‚ Ð² chosen/rejected, Ñ‚Ð¾Ð»ÑŒÐºÐ¾ ref | Ð›Ð¾ÐºÐ°Ð»ÑŒÐ½Ñ‹Ðµ Ð¸Ð¼ÐµÐ½Ð° gates, repo conventions, artifact paths, project-specific skill ids, source roots. |
| `technical_execution` | ÐÐµÑ‚, Ñ‚Ð¾Ð»ÑŒÐºÐ¾ excluded ref | ÐšÐ¾Ð¼Ð°Ð½Ð´Ñ‹, file edits, exact code paths, patch recipes, build/test filters, git/push steps, host-specific implementation details. |
| `evidence_example` | Ð¢Ð¾Ð»ÑŒÐºÐ¾ ÐºÐ°Ðº ref/redacted feature | ÐŸÑ€Ð¸Ð¼ÐµÑ€ Ð¸Ð· transcript/report, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ð¾Ð±ÑŠÑÑÐ½ÑÐµÑ‚ Ð¸ÑÑ‚Ð¾Ñ‡Ð½Ð¸Ðº, Ð½Ð¾ Ð½Ðµ Ð¾Ð±ÑƒÑ‡Ð°ÐµÑ‚ Ð¼Ð¾Ð´ÐµÐ»ÑŒ Ð¿Ð¾Ð²Ñ‚Ð¾Ñ€ÑÑ‚ÑŒ executor actions. |

Forbidden leakage Ð² common Ð¸ adaptive DPO Ð´Ð¾ Ð¿Ñ€Ð¸Ð½ÑÑ‚Ð¸Ñ `TECH-EXEC-LEARNING-GATE`:

- shell/git/dotnet/PowerShell/browser commands;
- ÐºÐ¾Ð½ÐºÑ€ÐµÑ‚Ð½Ñ‹Ð¹ Ñ„Ð°Ð¹Ð» Ð´Ð»Ñ edit Ð¸Ð»Ð¸ patch;
- direct patch recipe;
- host-specific implementation shortcut;
- repo path ÐºÐ°Ðº Ñ‡Ð°ÑÑ‚ÑŒ reasoning decision;
- deterministic command recipe presented as learned behavior.

Allowed universal content:

- "Ð½ÑƒÐ¶ÐµÐ½ DPO-map gate";
- "Ð½ÑƒÐ¶ÐµÐ½ e2e/host gate";
- "Ð½ÑƒÐ¶ÐµÐ½ no-hidden-determinism review";
- "Ð½ÑƒÐ¶ÐµÐ½ sub-agent review loop";
- "Ð½ÑƒÐ¶ÐµÐ½ answer-first boundary";
- "Ð½ÑƒÐ¶Ð½Ð° user/adaptive DPO row after correction";
- "Ð½ÑƒÐ¶ÐµÐ½ common/adaptive split and portability".

ÐœÐ¸Ð½Ð¸Ð¼Ð°Ð»ÑŒÐ½Ñ‹Ðµ Ð¿Ð¾Ð»Ñ ÑÑ‚Ñ€Ð¾ÐºÐ¸:

```json
{
  "source_row_id": "...",
  "target_machine": "OrchestrationReasoningMachine|PromptToTaskMachine|SkillSelectionMachine|ResultGapMachine",
  "model_leg": "common|adaptive",
  "abstraction_layer": "universal_reasoning|universal_skill_semantics",
  "chosen_ref": "...",
  "rejected_ref": "...",
  "project_adapter_refs": [],
  "excluded_technical_execution_refs": [],
  "contains_code_bound_execution": false,
  "contains_host_specific_logic": false,
  "universality_score": 0.0,
  "technical_execution_learning_allowed": false
}
```

Ð”Ð»Ñ skills ÑÑ‚Ð¾ Ð¿Ñ€Ð°Ð²Ð¸Ð»Ð¾ Ð´ÐµÐ¹ÑÑ‚Ð²ÑƒÐµÑ‚ Ð¾ÑÐ¾Ð±ÐµÐ½Ð½Ð¾ ÑÑ‚Ñ€Ð¾Ð³Ð¾: Ð¿Ð¾ÐºÐ° local wiki / technical execution model Ð½Ðµ Ð´Ð¾Ñ€Ð°Ð±Ð¾Ñ‚Ð°Ð½Ñ‹, common Ð¸ adaptive `SkillSelectionMachine` ÑƒÑ‡Ð°Ñ‚ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ semantic skill meaning and user preference, not executor hands. ÐŸÐ¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»ÑŒÑÐºÐ°Ñ adaptive-Ð¼Ð¾Ð´ÐµÐ»ÑŒ Ð¼Ð¾Ð¶ÐµÑ‚ ÑƒÑ‡Ð¸Ñ‚ÑŒ ÑÑ‚Ð¸Ð»ÑŒ Ð›ÑƒÐ±Ñ‹, Ð¿Ñ€ÐµÐ´Ð¿Ð¾Ñ‡Ñ‚ÐµÐ½Ð¸Ñ Ð³Ð»ÑƒÐ±Ð¸Ð½Ñ‹, gates, stop/answer boundaries and corrections, Ð½Ð¾ Ð½Ðµ exact files, shell commands, patch recipes Ð¸Ð»Ð¸ repo-specific execution habits.

#### B0.3.2.1 Seed positive/negative examples from existing references

Ð­Ñ‚Ð¸ Ð¿Ñ€Ð¸Ð¼ÐµÑ€Ñ‹ Ð¾Ð±ÑÐ·Ð°Ñ‚ÐµÐ»ÑŒÐ½Ñ‹ ÐºÐ°Ðº ÑÑ‚Ð°Ñ€Ñ‚Ð¾Ð²Ñ‹Ð¹ Ð½Ð°Ð±Ð¾Ñ€ Ð´Ð»Ñ DPO binding templates. ÐžÐ½Ð¸ Ð½Ðµ ÐºÐ¾Ð¿Ð¸Ñ€ÑƒÑŽÑ‚ ÑÑ‹Ñ€Ñ‹Ðµ Ð¿Ñ€Ð¸Ð²Ð°Ñ‚Ð½Ñ‹Ðµ prompts Ð¸Ð»Ð¸ Ð¿Ð¾Ð»Ð½Ñ‹Ðµ analyses Ð² Ñ€ÐµÐ¿Ð¾Ð·Ð¸Ñ‚Ð¾Ñ€Ð¸Ð¹. ÐšÐ¾Ð¼Ð¼Ð¸Ñ‚ÑÑ‚ÑÑ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ `source_ref`, `source_hash`, redacted feature summary, `chosen_ref`, `rejected_ref`, error code and universalization split.

| Seed id | Source reference | Target model | Positive / chosen signal | Negative / rejected signal | What is universalized | What is excluded as technical execution |
|---|---|---|---|---|---|---|
| `OR-SEED-001` | `sonnet local - analyzes\.clinerules-analysis.md`; also summarized in `ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md` | `OrchestrationReasoningMachine` common | Deep artifact analysis: identify audience, absence risk, linked artifacts, contradictions, operational consequences, and final action boundaries. | Superficial file summary that lists visible rules but misses linked evidence and hidden contradictions. Error class: `ORCH-02` + `ORCH-09`. | How to reason over an artifact: audience -> risk -> links -> contradictions -> evidence -> operational conclusion. | Exact repo files, exact command checks, exact line numbers, and patch actions. |
| `OR-SEED-002` | `sonnet local - analyzes\.gitignore-analysis.md`; also summarized in `ORCHESTRATION-REASONING-DPO-NEGATIVE-STRATEGY-v1.md` | `OrchestrationReasoningMachine` common | Risk analysis that detects safety gaps, encoding/format defects, mismatch with related repo policy, and concrete consequence. | "Looks mostly fine" summary that does not require evidence and does not surface safety impact. Error class: `ORCH-02` + `ORCH-09`. | Security/safety reasoning pattern: sensitive-data risk, policy mismatch, hidden format defect, evidence-before-claim. | Actual secret names/content, exact git commands, exact remediation patch. |
| `PTT-SEED-001` | `claude-sessions\sonnet-user-prompts\sonnet - accepted user prompts.md` + `sonnet - approved orchestration.md`; example family: "review/repair a plan, do not implement yet" | `PromptToTaskMachine` common/adaptive | Convert user prompt into structured task: mode=plan-review, implementation_authorized=false, sub-agent review loop, readable UML/algorithms/code refs, gates, no executor drift. | Starts implementation or repair commands before the plan is accepted. Error class: `PromptError-05` + `PromptError-07`. | Intent/constraints/deliverables extraction: "plan only", "readable for Luba", "sub-agent loop", "fix list not stop gate". | Any exact file edit, build/test command, branch/commit action. |
| `PTT-SEED-002` | Current conversation + `problem with authority.txt`; example family: direct "why/how did this happen?" question | `PromptToTaskMachine` and authority/adaptive leg | First action is answer/analyze the direct question, then find protocol/skill gap if needed; no repair as relief. | Runs repair or spawns executor before answering the question. Error class: `PromptError-07` + authority drift. | Conversation-intent boundary: question beats ongoing work, analysis before repair, no defensive freeze, no executor escape. | Any concrete patch to AGENTS/skills/plans until answer-first analysis is complete. |
| `SS-SEED-001` | `skills-inventory-universal-vs-project-specific.md` + current user correction "ÑÑ‚Ð¾ ÐºÐ°ÑÐ°ÐµÑ‚ÑÑ Ñ‚Ð°Ðº Ð¶Ðµ Ð¸ ÑÐºÐ¸Ð»Ð¾Ð²" | `SkillSelectionMachine` common/adaptive | Select skills by semantic job: planning, plan-review, DPO-authoring, no-product-decision, self-reflection; include when-not-to-use and combination rules. | Select executor/build/test/git skills because prompts mention files, code or commits. Error class: wrong skill semantics / executor drift. | Skill meaning: purpose, trigger, input/output, when to use, when not to use, compatible skills. | Exact repo commands, exact file edits, host/runtime operation. |
| `RG-SEED-001` | Current conversation + `problem with authority.txt`; examples: "if not found, after AI output build DPO and train user model"; "if changed, improve user model" | `ResultGapMachine` adaptive | Detect missing pattern or user correction; write adaptive DPO row for the personal leg while common model stays unchanged. | Store correction only as chat text or overwrite common model. Error class: `ORCH-06` + persistence gap. | Feedback loop: gap -> chosen/rejected adaptive signal -> personal checkpoint delta -> fresh-load/continue evidence. | Raw private chat text, external AI content without approval, common checkpoint mutation. |

For every seed row above, the `DPO-UNIVERSALIZATION-GATE` must produce two outputs:

```text
1. trainable_row: universal chosen/rejected signal used by the relevant common/adaptive model;
2. excluded_refs: technical execution, exact code/file/command, private raw text and host-specific details saved only as references for later technical-execution learning.
```

If the source is a mixed document that contains both architecture reasoning and executor operations, the row is invalid until the universal part and technical part are split. A mixed row must not be accepted as "better than nothing" because it trains the model to confuse thinking with doing.

### B0.3.3 `TEST-STRATEGY-ADMISSION`

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ Ð´Ð»Ñ ÐºÐ°Ð¶Ð´Ð¾Ð³Ð¾ work order, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ð¼ÐµÐ½ÑÐµÑ‚ Ð¿Ð¾Ð²ÐµÐ´ÐµÐ½Ð¸Ðµ, data readiness, model asset, API, host entrypoint Ð¸Ð»Ð¸ debug output.

Artifact: `artifacts/test-strategy-admission/<work_order>.json`.

ÐšÐ°Ð¶Ð´Ð°Ñ branch strategy Ð²Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ÑÑ ÑÐ²Ð½Ð¾:

| Branch | ÐšÐ¾Ð³Ð´Ð° Ð½ÑƒÐ¶Ð½Ð° | Ð§Ñ‚Ð¾ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÐµÑ‚ |
|---|---|---|
| unit | Pure unit/DTO/model component | ÐŸÐ¾Ð»Ñ, invariants, trainable route labels, no null/placeholder. |
| integration | ÐÐµÑÐºÐ¾Ð»ÑŒÐºÐ¾ core components Ð¸Ð»Ð¸ API boundary | Data movement between PromptToTask, Reasoning, SkillSelection, ResultGap, assets. |
| e2e/host | Claude/Cursor/Codex Ð¸Ð»Ð¸ API-to-host path | Real host entrypoint or honest downgrade to static readiness. |
| QA automation | ÐŸÐ¾Ð²Ñ‚Ð¾Ñ€ÑÐµÐ¼Ñ‹Ð¹ gate/script/check-set | ÐšÐ¾Ð¼Ð°Ð½Ð´Ð°/validator ÑÑƒÑ‰ÐµÑÑ‚Ð²ÑƒÐµÑ‚ Ð¸ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÑÐµÑ‚ named artifact. |
| negative | ÐžÑˆÐ¸Ð±Ð¾Ñ‡Ð½Ñ‹Ðµ inputs, forbidden tools, hidden deterministic shortcut | Gate fails for wrong behavior. |
| untrained honesty | Skeleton/untrained state | Response says not-trained/no semantic claim instead of pretending. |
| trained acceptance | Trained common/adaptive state | Held-out/generalization, fresh-load, continue and ablation pass. |
| regression | Existing accepted behavior | Previous accepted gates/artifacts stay valid or changed deliberately. |

Ð•ÑÐ»Ð¸ Ð²ÐµÑ‚ÐºÐ° Ð¿Ñ€Ð¾Ð¿ÑƒÑ‰ÐµÐ½Ð°, artifact Ð¾Ð±ÑÐ·Ð°Ð½ ÑÐ¾Ð´ÐµÑ€Ð¶Ð°Ñ‚ÑŒ `not_applicable_reason`. Parent review Ð½Ðµ Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°ÐµÑ‚ "small change" ÐºÐ°Ðº Ð¿Ñ€Ð¸Ñ‡Ð¸Ð½Ñƒ Ð¿Ñ€Ð¾Ð¿ÑƒÑÐºÐ°.

### B0.3.4 `UML-EXECUTABLE-CASE-MAP`

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ Ð´Ð»Ñ ÐºÐ°Ð¶Ð´Ð¾Ð³Ð¾ work order, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ñ€ÐµÐ°Ð»Ð¸Ð·ÑƒÐµÑ‚ Ð¸Ð»Ð¸ Ð¼ÐµÐ½ÑÐµÑ‚ UML edge, algorithm branch, host path, debug path Ð¸Ð»Ð¸ model route.

Artifact: `artifacts/uml-executable-case-map/<work_order>.json`.

ÐšÐ°Ð¶Ð´Ñ‹Ð¹ case Ð¾Ð±ÑÐ·Ð°Ð½ Ð¼Ð°Ð¿Ð¸Ñ‚ÑŒ:

```text
UML edge / algorithm branch
input fixture
expected request fields
expected response fields
expected asset touched
expected asset not touched
expected visible chat/debug output
negative case
```

Ð”Ð»Ñ host work orders visible output Ð¾Ð±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½: `xiigen <method> :`, Ð·Ð°Ñ‚ÐµÐ¼ Ð½Ð¾Ð²Ð°Ñ ÑÑ‚Ñ€Ð¾ÐºÐ° `input: <redacted payload>`, Ð·Ð°Ñ‚ÐµÐ¼ Ð½Ð¾Ð²Ð°Ñ ÑÑ‚Ñ€Ð¾ÐºÐ° `output: <redacted response>`.

### B0.3.5 `AUTHORITY-SELF-REFLECTION-GATE`

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ Ð´Ð»Ñ ÐºÐ°Ð¶Ð´Ð¾Ð³Ð¾ work order, Ð²ÐºÐ»ÑŽÑ‡Ð°Ñ plan/review/governance-only.

Artifact: `artifacts/authority-self-reflection/<work_order>.json`.

ÐœÐ¸Ð½Ð¸Ð¼Ð°Ð»ÑŒÐ½Ñ‹Ðµ Ð¿Ð¾Ð»Ñ:

| ÐŸÐ¾Ð»Ðµ | Ð§Ñ‚Ð¾ Ñ„Ð¸ÐºÑÐ¸Ñ€ÑƒÐµÑ‚ÑÑ |
|---|---|
| `loaded_authority_files` | AGENTS/CLAUDE/current plan/source ledgers/skills, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ðµ Ñ€ÐµÐ°Ð»ÑŒÐ½Ð¾ Ð¿Ñ€Ð¾Ñ‡Ð¸Ñ‚Ð°Ð½Ñ‹. |
| `conversation_intent_self_reflection_loaded` | Ð”Ð°/Ð½ÐµÑ‚ Ð¸ Ð¿ÑƒÑ‚ÑŒ/skill id. |
| `architect_behavior_classifier_loaded` | Ð”Ð°/Ð½ÐµÑ‚ Ð¸ Ð¿ÑƒÑ‚ÑŒ/skill id. |
| `plan_review_loaded` | Ð”Ð°/Ð½ÐµÑ‚ Ð¸ Ð¿ÑƒÑ‚ÑŒ/skill id. |
| `skill_injection_loaded` | Ð”Ð°/Ð½ÐµÑ‚, ÐµÑÐ»Ð¸ work order Ð¼ÐµÐ½ÑÐµÑ‚/Ð²Ñ‹Ð±Ð¸Ñ€Ð°ÐµÑ‚ skills. |
| `implementation_integrity_loaded` | Ð”Ð°/Ð½ÐµÑ‚, ÐµÑÐ»Ð¸ work order Ð¿Ð¸ÑˆÐµÑ‚ code/data/model/host. |
| `phase_completion_loaded` | Ð”Ð°/Ð½ÐµÑ‚, ÐµÑÐ»Ð¸ work order Ð·Ð°ÐºÑ€Ñ‹Ð²Ð°ÐµÑ‚ phase/gate. |
| `drift_checks` | ÐŸÑ€Ð¾Ð²ÐµÑ€ÐºÐ¸ Ð¿Ñ€Ð¾Ñ‚Ð¸Ð² executor drift, defensive freeze, action-as-relief, focus-loss, entropy tolerance. |
| `user_requirement_anchor` | ÐšÐ°ÐºÐ°Ñ Ñ„Ñ€Ð°Ð·Ð° Ð›ÑƒÐ±Ñ‹ Ð¸Ð»Ð¸ ledger row Ð´Ð°Ñ‘Ñ‚ authority. |

Ð•ÑÐ»Ð¸ gate Ð½Ð°Ñ…Ð¾Ð´Ð¸Ñ‚ drift Ð¸Ð»Ð¸ defensive freeze, ÑÑ‚Ð¾ Ð½Ðµ Ñ„Ð¸Ð½Ð°Ð»ÑŒÐ½Ñ‹Ð¹ ÑÑ‚Ð¾Ð¿: parent architect Ñ‡Ð¸Ð½Ð¸Ñ‚ work order/scope/skill load Ð¸ ÑÐ½Ð¾Ð²Ð° Ð¾Ñ‚Ð´Ð°Ñ‘Ñ‚ bounded sub-agent packet.

### B0.3.6 `CHECKPOINT-WRITER-GUARD`

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ Ð´Ð»Ñ ÐºÐ°Ð¶Ð´Ð¾Ð³Ð¾ work order, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ð¿Ð¸ÑˆÐµÑ‚, Ð¾Ð±Ð½Ð¾Ð²Ð»ÑÐµÑ‚, Ð¸Ð¼Ð¿Ð¾Ñ€Ñ‚Ð¸Ñ€ÑƒÐµÑ‚, ÑÐºÑÐ¿Ð¾Ñ€Ñ‚Ð¸Ñ€ÑƒÐµÑ‚, Ð¿Ñ€Ð¾Ð´Ð¾Ð»Ð¶Ð°ÐµÑ‚ Ð¸Ð»Ð¸ Ð²Ð°Ð»Ð¸Ð´Ð¸Ñ€ÑƒÐµÑ‚ model checkpoint, adaptive checkpoint, manifest Ð¸Ð»Ð¸ locator.

Artifact: `artifacts/checkpoint-writer-guard/<work_order>.json`.

ÐœÐ¸Ð½Ð¸Ð¼Ð°Ð»ÑŒÐ½Ñ‹Ðµ Ð¿Ð¾Ð»Ñ:

```text
checkpoint_id
writer_identity
single_writer_lock
write_allowed_only_when_gate_passed
accepted_pair_ids_or_source_refs
resume_rebuilds_from_accepted_refs
cursor_prefix_resume_forbidden
empty_accepted_refs_forbidden
corrupted_checkpoint_policy = rollback_or_forensic_only
fresh_start_masquerade_forbidden
```

Gate fails if a work order can write a checkpoint with unknown/failed pilot gate, if resume can be implemented as "take first N rows", or if a broken checkpoint can be silently accepted as continuation.

### B0.3.7 `METRIC-HONESTY-GATE`

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ Ð´Ð»Ñ ÐºÐ°Ð¶Ð´Ð¾Ð³Ð¾ work order, ÐºÐ¾Ñ‚Ð¾Ñ€Ñ‹Ð¹ Ð·Ð°ÑÐ²Ð»ÑÐµÑ‚ accuracy, readiness, trained quality, improvement, pass/fail promotion, or model acceptance.

Artifact: `artifacts/metric-honesty/<work_order>.json`.

ÐœÐ¸Ð½Ð¸Ð¼Ð°Ð»ÑŒÐ½Ñ‹Ðµ Ð¿Ñ€Ð°Ð²Ð¸Ð»Ð°:

- retrieval/routing metrics and trained-generalization metrics are reported separately;
- headline model quality can use only truly-novel held-out generalization, not retrieval or exact memory;
- near-duplicate, retrieval and bootstrap scores may be reported, but must be clearly lower-scope metrics;
- `AccuracyAfter`, `AccuracyAtExport`, `PassesGate` and similar fields must be runner-emitted or reproducible from runner/generator code;
- average/sliding-window metrics beat best single peak unless the plan explicitly names a peak-only diagnostic;
- parent review must compare report fields to the generator/runner path before accepting completion.

Gate fails if a static JSON report, hand-written number, peak-only success, or retrieval route is used as proof of trained generalization.

### B0.3.8 `HUMAN-AUTHORITY-BOUNDARY-GATE`

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ Ð´Ð»Ñ `PromptToTaskMachine`, plan-review, host-facing request parsing and every work order that reads attachments, old plans, state files or generated reports.

Artifact: `artifacts/human-authority-boundary/<work_order>.json`.

Priority order:

```text
1. current explicit Luba instruction in the active conversation
2. explicit constraints in the current work order
3. approved active plan
4. attached files / old plans / state / generated reports as evidence
5. inferred task or machine-selected scenario
```

Attachment content, old state, quoted transcript, model-selected flow or machine artifact name never overrides a current human instruction. Negative test required: a file or old plan says "execute", while Luba's current instruction says "answer/analyze/plan only"; expected behavior is answer/analyze/plan only.

### B0.3.9 `SUBAGENT-PACKET-QUALITY-GATE`

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ Ð´Ð»Ñ ÐºÐ°Ð¶Ð´Ð¾Ð³Ð¾ bounded sub-agent packet and parent review.

Artifact: `artifacts/subagent-packet-quality/<work_order>.json`.

Required checks:

- packet names the required reasoning tier/capability for the task;
- packet names exact allowed write scope and forbidden actions;
- packet cannot approve its own work as final;
- parent review verdict must be one of `ALLOW_FINAL`, `CONTINUE_EXECUTION`, `REPAIR_REQUIRED`, `OPERATOR_INPUT_REQUIRED`;
- any code/data/plan mutation makes previous review stale until rechecked;
- `completed:null`, metric-only response, no next-action analysis, or ungrounded "done" is rejected.

### B0.3.10 `TEST-EXECUTION-HYGIENE-GATE`

ÐžÐ±ÑÐ·Ð°Ñ‚ÐµÐ»ÐµÐ½ for any work order that runs tests, runtime probes, host commands or long-running training/check commands.

Artifact: `artifacts/test-execution-hygiene/<work_order>.json`.

Required checks:

- identify allowed command exactly before running it;
- separate pre-existing red from phase-induced regression when a focused command fails;
- before rerun, detect and remediate only exact orphan process roots related to the command;
- record timeout, cleanup and rerun evidence without calling timeout a blocker;
- long-running sub-agent work needs heartbeat/checkpoint handoff and replacement-worker packet with workspace/state continuity.

Gate fails if broad tests are run as ritual, if pre-existing red is mixed with current regression, if orphan processes are left to poison later runs, or if a sub-agent can disappear without evidence packet.

## B1. Authority Requirements Ledger

| ID | Ð¢Ñ€ÐµÐ±Ð¾Ð²Ð°Ð½Ð¸Ðµ | Ð“Ð´Ðµ Ð¿Ð¾ÐºÑ€Ñ‹Ñ‚Ð¾ | Gate/artifact |
|---|---|---|---|
| R1 | ÐŸÐ¾Ð»Ð½Ñ‹Ð¹ extension suite Ð´Ð»Ñ Claude, Cursor, Codex | A0, A1, B6-B8 | per-host real entrypoint gates |
| R2 | Prompt -> structured task | A1.1, B3 | PromptToTaskMachine trainable gate |
| R3 | Task -> orchestration plan | A1.2, B4 | OrchestrationReasoningMachine trainable gate |
| R4 | Task -> skills | A1.3, B5 | SkillSelectionMachine semantic selector gate |
| R5 | Missing pattern after AI answer -> DPO and user model training | A1.4, B9 | adaptive DPO + continue-training gate |
| R6 | User corrections improve user model | A1.5, B9 | feedback DPO + adaptive training gate |
| R7 | Common core logic/models/sets in `llm_mvp_core` | A3, B10 | asset location and manifest gate |
| R8 | User adaptive data/models on user side | A3, B10 | `.xiigen` preservation gate |
| R9 | ÐŸÐ»Ð°Ð½ Ñ‡Ð¸Ñ‚Ð°ÐµÐ¼ Ð›ÑƒÐ±Ð¾Ð¹ first | Part A | readable-plan review gate |
| R10 | UML importable to draw.io | A0 | PlantUML presence gate |
| R11 | Russian algorithms without code | A1 | algorithm section gate |
| R12 | Per-algorithm actual code refs and short quotes | A2 | exact file:line gate |
| R13 | No hidden deterministic shortcut | A1, A2, B11 | parent REVIEW_INTERNAL anti-determinism gate |
| R14 | External AI/unrelated tool ban | B11, B12 | external tool approval gate |
| R15 | Debug setting in each extension | A1.8, B6-B8 | host debug trace gate |
| R16 | Separate private repos under same owner | B6-B8 | repo inventory and package gate |
| R17 | No JSON/PowerShell-only substitute | A4, B6-B8, B11 | artifact shape gate |
| R18 | Ð›ÑŽÐ±Ð°Ñ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ `ILearningMachine`-ÑÑ„ÐµÑ€Ð° Ð¸Ð¼ÐµÐµÑ‚ common/global leg + personal/adaptive leg, user-model backup/share/restore/cross-device portability | A0, A1.7, A2, A3, B10, B16, B20 | two-leg contract gate + portability round-trip gate |
| R19 | DPO knowledge is loaded before any trainable/adaptive/DPO work | 0.1, 0.2, B0.3.1, B20 | `DPO-KNOWLEDGE-MAP` artifact gate |
| R20 | Common and adaptive DPO for orchestration/skills excludes code-bound technical execution until technical-execution learning has its own accepted gate | 0.2, B0.3.2, B7-B10, B16, B20 | `DPO-UNIVERSALIZATION-GATE` artifact + `TECH-EXEC-LEARNING-GATE` future boundary |
| R21 | Every implementation/model/host branch has a normal engineering test strategy | 0.2, B0.3.3, B20 | per-branch unit/integration/e2e-host/QA/negative/untrained/trained/regression strategy |
| R22 | UML simulations become executable test cases, not only prose | A0.1, A0.1.1, B0.3.4, B20 | `UML-EXECUTABLE-CASE-MAP` artifact gate |
| R23 | Architect/worker drift, defensive freeze, action-as-relief, focus loss and entropy tolerance are checked before every work order | 0.2, B0.3.5, B20 | `AUTHORITY-SELF-REFLECTION-GATE` artifact gate |
| R24 | Part A must show the `ILearningMachine` hierarchy from machine to orchestrator/flow/unit/two-leg pair, and Part B cannot introduce a trainable sphere/flow/unit/pair absent from that hierarchy | A5, B0.3.4, B16, B20 | A5 hierarchy/parity review + `UML-EXECUTABLE-CASE-MAP` artifact |
| R25 | Every training/adaptive/model asset work order must protect checkpoint writing from failed/unknown gates, multi-writer writes, replay/cursor-prefix resume and corrupted-checkpoint masquerade | 0.2, B0.3.6, B10, B16, B20 | `CHECKPOINT-WRITER-GUARD` artifact |
| R26 | Metrics must be honest: retrieval/routing/exact-memory results are separated from trained generalization, headline quality uses truly-novel held-out rows, and numbers are runner-emitted or reproducible | 0.2, B0.3.7, B16, B20 | `METRIC-HONESTY-GATE` artifact |
| R27 | Current Luba instruction beats attachments, older plans, state files, reports, inferred tasks and machine-selected scenarios | 0.2, B0.3.8, B1, B17, B20 | `HUMAN-AUTHORITY-BOUNDARY-GATE` artifact |
| R28 | Sub-agent packets and parent reviews must name capability/tier, forbid self-approval, invalidate review after mutation, and reject `completed:null` or ungrounded done claims | B0.1, B0.3.9, B20 | `SUBAGENT-PACKET-QUALITY-GATE` artifact |
| R29 | Test/runtime work must distinguish pre-existing red from phase-induced regression, clean exact orphan roots before rerun and provide heartbeat/replacement-worker handoff for long-running work | 0.2, B0.3.10, B20 | `TEST-EXECUTION-HYGIENE-GATE` artifact |
| R30 | Negative DPO rows must be plausible near-misses, and scaffolds such as position counters, exact retrieval or fixture bootstrap cannot pass as learned semantic behavior | B0.3.1, B0.3.2, B9, B16, B20 | DPO map + universalization + no-hidden-determinism review |

## B2. User Prompt Source Ledger

| Entry | Type | Source quote / resolution | Requirements |
|---|---|---|---|
| UPS-001 | user_request | "Ð¿Ð¾Ð»Ð½Ð¾Ñ†ÐµÐ½Ð½Ð¾Ð¹ extension Ð´Ð»Ñ cursor, claude, codex" | R1, R16 |
| UPS-002 | user_request | "Ð¿Ñ€Ð¸Ð½Ð¸Ð¼Ð°Ñ‚ÑŒ Ð·Ð°Ð¿Ñ€Ð¾Ñ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ Ð¸ Ð¿ÐµÑ€ÐµÐ²Ð¾Ð´Ð¸Ñ‚ÑŒ ÐµÐ³Ð¾ Ð² Ð·Ð°Ð´Ð°Ñ‡Ñƒ" | R2 |
| UPS-003 | user_request | "ÐÐ°Ñ…Ð¾Ð´Ð¸Ñ‚ÑŒ Ð¿Ð»Ð°Ð½ Ð¾Ñ€ÐºÐµÑÑ‚Ñ€Ð°Ñ†Ð¸Ð¸ Ð´Ð»Ñ Ð·Ð°Ð´Ð°Ñ‡Ð¸" | R3 |
| UPS-004 | user_request | "Ð½Ð°Ñ…Ð¾Ð´Ð¸Ñ‚ÑŒ Ð½ÑƒÐ¶Ð½Ñ‹Ðµ ÑÐºÐ¸Ð»Ñ‹ Ð´Ð»Ñ Ð·Ð°Ð´Ð°Ñ‡Ð¸" | R4 |
| UPS-005 | user_request | "Ð•ÑÐ»Ð¸ Ð½Ðµ Ð½Ð°Ñ…Ð¾Ð´Ð¸Ñ‚ - Ð¿Ð¾ÑÐ»Ðµ Ð¿Ð¾Ð»ÑƒÑ‡ÐµÐ½Ð¸Ñ from AI -> ÑÑ‚Ñ€Ð¾Ð¸Ñ‚ÑŒ Ð¾Ð±ÑƒÑ‡Ð°ÑŽÑ‰Ð¸Ðµ Ð”ÐŸÐž Ð¸ Ð¾Ð±ÑƒÑ‡Ð°ÑÑŒ Ð¼Ð¾Ð´ÐµÐ»ÑŒ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ" | R5 |
| UPS-006 | user_request | "Ð•ÑÐ»Ð¸ Ð¸Ð´ÐµÑ‚ Ð¸Ð·Ð¼ÐµÐ½Ð½ÐµÐ½Ð¸Ðµ - Ð£Ð»ÑƒÑ‡ÑˆÐ°Ñ‚ÑŒ Ð¼Ð¾Ð´ÐµÐ»ÑŒ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ" | R6 |
| UPS-007 | user_request | "Ð›Ð¾Ð³Ð¸ÐºÐ° Ð¼Ð¾Ð´ÐµÐ»Ð¸ Ð¸ ÑÐµÑ‚Ñ‹ Ð½Ð°ÑˆÐµÐ³Ð¾ core - Ð²ÑÐµ Ñƒ Ð½Ð°Ñ Ð² Ñ€ÐµÐ¿Ð¾" | R7 |
| UPS-008 | user_request | "Ð¡ÐµÑ‚Ñ‹ - Ð¸ Ð¼Ð¾Ð´ÐµÐ»Ð¸ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ Ñƒ Ð½ÐµÐ³Ð¾" | R8 |
| UPS-009 | user_request | "ÐŸÐ»Ð°Ð½ Ð² Ð¿ÐµÑ€Ð²ÑƒÑŽ Ð¾Ñ‡ÐµÑ€ÐµÐ´ÑŒ Ð´Ð¾Ð»Ð¶ÐµÐ½ Ð±Ñ‹Ñ‚ÑŒ Ñ‡Ð¸Ñ‚Ð°ÐµÐ¼ Ð¼Ð½Ð¾Ð¹" | R9-R12 |
| UPS-010 | user_request | "Ð½Ð¸Ð³Ð´Ðµ Ð½Ðµ ÑÐ¿Ñ€ÑÑ‚Ð°Ð½ deterministic shortcut" | R13 |
| UPS-011 | user_request | "Ð—Ð°Ð¿Ñ€ÐµÑ‚ Ð½Ð° Ð¸ÑÐ¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ð½Ð¸Ðµ Ð²Ð½ÐµÑˆÐ½Ð¸Ñ… AI..." | R14 |
| UPS-012 | user_request | "Ð´Ð¾Ð±Ð°Ð²Ð¸Ñ‚ÑŒ Ð² setting ÐºÐ°Ð¶Ð´Ð¾Ð¹ extension debug..." | R15 |
| UPS-013 | user_challenge | "ÐŸÐ¾Ñ‡ÐµÐ¼Ñƒ ... json & ps1 files, Ð²Ð¼ÐµÑÑ‚Ð¾ ÐºÐ¾Ð´Ð° Ð¿Ð»Ð°Ð³Ð¸Ð½Ð°..." | R17 |
| UPS-014 | user_question_and_requirement | "Ð»ÑŽÐ±Ð°Ñ Ð¾Ð±ÑƒÑ‡Ð°ÐµÐ¼Ð°Ñ ÑÑ„Ñ€Ð° Ð² ILearningMachine ... Ð¸Ð¼ÐµÐ»Ð° Ð´Ð²Ðµ Ð½Ð¾Ð³Ð¸ ... Ñƒ Ð½Ð°Ñ Ð² Ð¿Ñ€Ð¾ÐµÐºÑ‚Ðµ ... Ð¿Ð¾Ð´ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ ... Ð¼Ð¾Ð´ÐµÐ»ÑŒ ÑÐ¸Ð´Ð¸Ñ‚ Ñƒ Ð½ÐµÐ³Ð¾ Ð½Ð° ÐºÐ¾Ð¼Ð¿Ðµ ... Ð¼Ð¾Ð¶Ð½Ð¾ ÐµÑ‘ Ð±ÐµÐºÐ°Ð¿Ð¸Ñ‚ÑŒ, ÑˆÐµÑ€Ð¸Ñ‚ÑŒ, Ð¿ÐµÑ€ÐµÐ¿Ð¸ÑÑ‹Ð²Ð°Ñ‚ÑŒ Ð½Ð° Ð´Ñ€ÑƒÐ³Ð¸Ðµ ÑƒÑÑ‚Ñ€Ð¾Ð¹ÑÑ‚Ð²Ð°" | R18 |
| UPS-015 | user_review_gap | "DPO-KNOWLEDGE-MAP ÐµÑÑ‚ÑŒ, Ð½Ð¾ v3 Ð½Ðµ Ð¿Ð¾Ð´ÐºÐ»ÑŽÑ‡Ð°ÐµÑ‚ ÐµÐ³Ð¾ ÐºÐ°Ðº Ð¾Ð±ÑÐ·Ð°Ñ‚ÐµÐ»ÑŒÐ½Ñ‹Ð¹ gate" | R19 |
| UPS-016 | user_review_gap | "Ð³Ð´Ðµ Ñ‚ÐµÑÑ‚Ð¸Ñ€Ð¾Ð²Ð°Ð½Ð¸Ðµ, e2e, qa automation Ñ Ð°Ð¿Ð»Ð¸ÐºÐ°Ñ†Ð¸ÐµÐ¹" | R21 |
| UPS-017 | user_review_gap | "ÑÑ†ÐµÐ½Ð°Ñ€Ð¸Ð¸ Ð´Ð»Ñ Ð¿Ñ€Ð¾Ð²ÐµÑ€ÐºÐ¸ Ð±Ñ‹Ð»Ð¸ Ð´Ð¾Ð±Ð°Ð²Ð»ÐµÐ½Ñ‹ Ð² uml - Ð½Ð¾ Ð½Ðµ Ð¾Ñ„Ð¾Ñ€Ð¼Ð»ÐµÐ½Ñ‹ ÐºÐ°Ðº Ñ‚ÐµÑÑ‚ ÐºÐµÐ¹ÑÑ‹" | R22 |
| UPS-018 | user_review_gap | "Ð¿Ñ€Ð¾Ð±Ð»ÐµÐ¼Ñ‹ Ñ Ð°Ñ€Ñ…Ð¸Ñ‚ÐµÐºÑ‚Ð¾Ñ€Ð¾Ð¼... ÑƒÑ…Ð¾Ð´ Ð² Ð¸ÑÐ¿Ð¾Ð»Ð½Ð¸Ñ‚ÐµÐ»Ñ/Ð·Ð°Ñ‰Ð¸Ñ‚Ñƒ/Ð¿Ð¾Ñ‚ÐµÑ€Ñ Ñ„Ð¾ÐºÑƒÑÐ°/ÑÐ½Ñ‚Ñ€Ð¾Ð¿Ð¸Ñ" | R23 |
| UPS-019 | user_requirement | "Ð´Ð»Ñ Ð¾Ñ€ÐºÐµÑÑ‚Ñ€Ð°Ñ†Ð¸Ð¸ Ð²Ñ‹Ñ€ÐµÐ·Ð°Ñ‚ÑŒ Ñ‚Ð¾Ð»ÑŒÐºÐ¾ Ñ‡Ð°ÑÑ‚ÑŒ ÐºÐ¾Ñ‚Ð¾Ñ€Ð°Ñ ÑƒÐ½Ð¸Ð²ÐµÑ€ÑÐ°Ð»ÑŒÐ½Ð° Ð¸ Ð½Ðµ Ð²ÐºÐ»ÑŽÑ‡Ð°ÐµÑ‚ Ñ‚ÐµÑ…Ð½Ð¸Ñ‡ÐµÑÐºÐ¾Ðµ Ð¸ÑÐ¿Ð¾Ð»Ð½ÐµÐ½Ð¸Ðµ... ÑÑ‚Ð¾ ÐºÐ°ÑÐ°ÐµÑ‚ÑÑ Ñ‚Ð°Ðº Ð¶Ðµ Ð¸ ÑÐºÐ¸Ð»Ð¾Ð²... Ð½Ð° ÑƒÑ€Ð¾Ð²Ð½Ðµ Ð»Ð¾ÐºÐ°Ð»ÑŒÐ½Ð¾ Ð¾Ð±ÑƒÑ‡Ð°ÑŽÑ‰ÐµÐ¹ÑÑ Ð¼Ð¾Ð´ÐµÐ»Ð¸ ÑÑ‚Ð¾ Ð¿Ñ€Ð°Ð²Ð¸Ð»Ð¾ Ñ‚Ð¾Ð¶Ðµ Ð¿Ð¾ÐºÐ° Ð²ÐµÑ€Ð½Ð¾" | R20 |
| UPS-020 | external_review_gap | "Ð¿Ð¾Ð»ÐµÐ·Ð½Ñ‹Ðµ Ð²Ñ‹Ð²Ð¾Ð´Ñ‹ Ñ€ÐµÐ²ÑŒÑŽ: Ð½ÑƒÐ¶Ð½Ð° Ð¸ÐµÑ€Ð°Ñ€Ñ…Ð¸Ñ `ILearningMachine`, checkpoint writer guard, Ñ‡ÐµÑÑ‚Ð½Ñ‹Ðµ Ð¼ÐµÑ‚Ñ€Ð¸ÐºÐ¸, human authority boundary, ÐºÐ°Ñ‡ÐµÑÑ‚Ð²Ð¾ sub-agent packet, test execution hygiene Ð¸ near-miss DPO Ð²Ð¼ÐµÑÑ‚Ð¾ Ð·Ð°Ð³Ð»ÑƒÑˆÐµÐº" | R24-R30 |

## B3. Conversation Delta Ledger

| Delta | Ð§Ñ‚Ð¾ Ð¸Ð·Ð¼ÐµÐ½Ð¸Ð»Ð¾ÑÑŒ | Ð¡Ð»ÐµÐ´ÑÑ‚Ð²Ð¸Ðµ |
|---|---|---|
| D-001 | Coverage target changed to OrchestrationReasoning + PromptToTask source plans | v20 becomes reference only |
| D-002 | Readable Part A became admission gate | No execution without PlantUML, Russian algorithms, current code refs |
| D-003 | Host extension must be real code | Per-host rows must produce entrypoint/package/adapter code |
| D-004 | Manual DPO binding questions were rejected | Plan must include automatic seed materialization from local approved sources before asking Luba |
| D-005 | Architect must not become executor | Parent only reviews and issues bounded sub-agent work orders |
| D-006 | `.xiigen` folder convention alone is not a two-leg learning contract | Every trainable sphere needs `ILearningMachinePair` or equivalent pair adapter plus backup/export, restore/import, share/copy, and cross-device fresh-profile probes |

## B4. Replacement Mapping Register

| Ð—Ð°Ð¿Ñ€ÐµÑ‰Ñ‘Ð½Ð½Ð°Ñ Ð¿Ð¾Ð´Ð¼ÐµÐ½Ð° | Ð¢Ñ€ÐµÐ±ÑƒÐµÐ¼Ð°Ñ Ð·Ð°Ð¼ÐµÐ½Ð° |
|---|---|
| JSON/PowerShell proof bundle as extension | Real host entrypoint code + package/manifest + thin adapter |
| Host-side skill ranking | XIIGen core `SkillSelectionMachine` and semantic unit pipeline |
| Regex/prompt keyword task classifier | Trainable `PromptToTaskMachine` with checkpoint/manifest/export/import |
| Guide filename/BM25 lookup as orchestration plan | Trainable `OrchestrationReasoningMachine` with learned guide state |
| Missing-pattern if/else as learning | DPO row + adaptive user-model training + second-pass evidence |
| User correction stored as chat note only | User-local chosen/rejected DPO signal + adaptive training |
| Common upgrade overwrites user `.xiigen` | Upgrade planner preserves user state and emits patch proposals |
| External AI used for data generation without approval | Local sources and explicit Luba approval if external tool is needed |
| User adaptive model stored locally but not portable | User-model package with manifest/checkpoint hash/provenance/redaction + backup/export + restore/import + cross-device probe |

## B5. PRE and POST nine-check matrix

Ð­Ñ‚Ð° Ð¼Ð°Ñ‚Ñ€Ð¸Ñ†Ð° Ð·Ð°Ð¿ÑƒÑÐºÐ°ÐµÑ‚ÑÑ Ð´Ð²Ð°Ð¶Ð´Ñ‹: Ð´Ð¾ Ð¿Ñ€Ð¸Ð½ÑÑ‚Ð¸Ñ split Ð¸ Ð¿Ð¾ÑÐ»Ðµ Ð¸Ð·Ð¼ÐµÐ½ÐµÐ½Ð¸Ñ split.

| Check | Ð¢Ñ€ÐµÐ±Ð¾Ð²Ð°Ð½Ð¸Ðµ | Pre-split status | Post-split status |
|---|---|---|---|
| 1 | ÐŸÐ»Ð°Ð½ Ð¿Ð¾ÐºÑ€Ñ‹Ð²Ð°ÐµÑ‚ OrchestrationReasoning and PromptToTask source plans | mapped in B1/B12 | mapped in B13 |
| 2 | Ð’ÑÐµ Ð²Ð¸Ð´Ð¸Ð¼Ñ‹Ðµ Ñ‚Ñ€ÐµÐ±Ð¾Ð²Ð°Ð½Ð¸Ñ Ð¿Ð¾Ð»ÑŒÐ·Ð¾Ð²Ð°Ñ‚ÐµÐ»Ñ Ð² ledger and gates | mapped in B1/B2 | every work order links to R IDs |
| 3 | Mental implementation covers user tasks | B14 scenarios | B14 scenarios map to work orders |
| 4 | No deterministic algorithm hidden as trainable unit | B11 owner/gate | per work order anti-determinism check |
| 5 | Common/adaptive models and sets split, with two-leg `ILearningMachine` pair and user-model portability | A0/A1.7/A3/B10 | asset work orders split by common vs adaptive; every trainable sphere has pair + backup/share/restore/cross-device gates |
| 6 | User workstyle in agents/skills | **Ð¡Ð”Ð•Ð›ÐÐÐž: governance-ÐºÐ¾Ð¼Ð¼Ð¸Ñ‚ `a9b62903`** (OP-50 R1â€“R7, plan-review FC-20..24, architect FAIL-25..27, planning-startup gates) | done; future repair only if review finds a NEW gap |
| 7 | Core in `llm_mvp_core`, hosts thin clients | A0/A3/B6-B8 | per-host artifact shape gate |
| 8 | External AI/unrelated tools banned without permission | B11/B12 | every work order has forbidden actions |
| 9 | Claude/Cursor/Codex implementation and checks included | B6-B8 | one host per work-order group |

## B6. Foundation work orders

| Work order | Type | Goal | Allowed write scope | Maximum active time | Forbidden actions | Artifact | Gate/review | Commit boundary |
|---|---|---|---|---|---|---|---|---|
| WO-00 | plan/review | Parent review of this v3 plan after Luba reads it | none unless plan repair requested | <=15 min | implementation, code, tests, builds, host runtime | review notes | artifact check: reviewer marks all 9 checks, readable Part A, B9 11-unit coverage, two-leg `ILearningMachine` portability coverage, no-hidden-determinism, and OP-49/FC-19 Part A == Part B parity on architecture + product decisions + algorithms as PASS/FAIL with exact gaps | no commit unless plan repair changes repo |
| WO-01 | governance | If review finds missing rule, patch only governance/skills | AGENTS/CLAUDE/docs/skills only | <=15 min | product code, host repos, runtime commands | governance patch | artifact check: changed rule text + skills-gap-first simulation transcript shows architect does not execute | phase commit after review |
| WO-02 | documentation | Update current implementation docs to match actual code | docs only | <=15 min | product code/tests/runtime commands | implementation doc | artifact check: each claim has exact current code refs and no aspirational "implemented" claim without code evidence | phase commit after review |

## B7. PromptToTaskMachine work orders

| Work order | Type | Goal | Allowed write scope | Maximum active time | Forbidden actions | Artifact | Gate/review | Commit boundary |
|---|---|---|---|---|---|---|---|---|
| PTT-01 | code | Define trainable PromptToTask training pair and checkpoint state | `43.SmartSkillsDomain` contracts only | <=15 min | host repos, training runs, external AI | DTOs and state contracts | artifact check: DTO has source refs, chosen/rejected refs, checkpoint state fields, and OperationResult surface; no raw prompt text | PromptToTask phase group |
| PTT-02 | training-data | Build approved common seed corpus schema from prompt roots | `tools`, `training-data\prompt-to-task`, reports | <=15 min | external AI, raw private copy, host repos | redacted JSONL + manifest | artifact check: manifest contains source refs/hashes only and no raw private prompt text | same phase group |
| PTT-02U | training-data/check | Universalize PromptToTask DPO for common and adaptive legs before training | `tools`, `training-data\prompt-to-task`, reports | <=15 min | technical execution rows in chosen/rejected, host repos, external AI | DPO universalization report | artifact check: common/adaptive rows contain only prompt-to-task intent/constraint/deliverable semantics; code-bound execution refs are excluded or adapter-only | same phase group |
| PTT-02V | check | Create validator for PromptToTask corpus before any validator command is used | `tools`, reports only | <=15 min | training, host repos, external AI | validator script/report schema | artifact check: validator verifies required fields and private-text redaction; no runtime training claim | same phase group |
| PTT-03 | code | Implement trainable prompt-to-task inference replacing skeleton | `PromptToTaskMachine.cs` and tests named by plan | <=15 min | deterministic regex final authority, host repos, external AI | trained inference path | artifact check: ablation report shows no trained state -> not ready, trained state -> semantic task from checkpoint | same phase group |
| PTT-04 | asset | Export/import/fresh-load/continue PromptToTask model | models/training-data/tools only | <=15 min | host repos, external AI | checkpoint, manifest, locator report | artifact check: export/import/fresh-load/continue reports exist and reference current checkpoint hash | phase commit after gate |

## B8. OrchestrationReasoningMachine work orders

| Work order | Type | Goal | Allowed write scope | Maximum active time | Forbidden actions | Artifact | Gate/review | Commit boundary |
|---|---|---|---|---|---|---|---|---|
| OR-01 | code | Define learned guide-state training pair and checkpoint state | `43.SmartSkillsDomain` contracts only | <=15 min | host repos, training runs, external AI | DTOs and state contracts | artifact check: DTO has source refs and checkpoint state fields; no raw guide copy | OrchestrationReasoning phase group |
| OR-01U | training-data/check | Universalize OrchestrationReasoning DPO before corpus build | `tools`, `training-data\orchestration-reasoning`, reports | <=15 min | code-bound execution rows in chosen/rejected, host repos, external AI | DPO universalization report | artifact check: common/adaptive rows contain only universal reasoning; project adapter refs and technical execution refs are separated; technical execution leakage = 0 | OrchestrationReasoning phase group |
| OR-02 | training-data | Build guide-derived common corpus from `sonnet local - analyzes` | `tools`, `training-data\orchestration-reasoning`, reports | <=15 min | external AI, raw private copy, host repos | redacted JSONL + manifest | artifact check: manifest records hashes/source refs only and no raw private analysis text; depends on OR-01U PASS | same phase group |
| OR-02V | check | Create validator for OrchestrationReasoning corpus before any validator command is used | `tools`, reports only | <=15 min | training, host repos, external AI | validator script/report schema | artifact check: validator checks source refs, DPO error codes, redaction, and trainable-target fields | same phase group |
| OR-03 | code | Implement trainable task-to-plan inference replacing skeleton | `OrchestrationReasoningMachine.cs` and tests | <=15 min | BM25/file lookup final authority, host repos, external AI | learned scenario plan path | artifact check: ablation + held-out guide-pattern report proves decision comes from checkpoint | same phase group |
| OR-04 | asset | Export/import/fresh-load/continue OrchestrationReasoning model | models/training-data/tools only | <=15 min | host repos, external AI | checkpoint, manifest, locator report | artifact check: export/import/fresh-load/continue reports exist and reference current checkpoint hash | phase commit after gate |

## B9. Skill selection and semantic units

| Work order | Type | Goal | Allowed write scope | Maximum active time | Forbidden actions | Artifact | Gate/review | Commit boundary |
|---|---|---|---|---|---|---|---|---|
| SS-01 | code | Replace candidate-position final authority with semantic skill-selection pipeline boundary | `SkillSelectionMachine.cs`, semantic unit contracts only | <=15 min | host repos, training runs, external AI | semantic decision path skeleton | artifact check: candidate-position weight is marked scaffold only and cannot be final semantic authority | skill-selection phase group |
| SS-00U | training-data/check | Universalize Skill Semantics DPO for common and adaptive legs | `tools`, `training-data\skill-selection`, reports | <=15 min | exact file edits, shell commands, repo-specific execution habits in chosen/rejected, host repos, external AI | skill DPO universalization report | artifact check: skill rows teach purpose, when-to-use, when-not-to-use, inputs, outputs, failure modes and combinations; technical execution refs are excluded until `TECH-EXEC-LEARNING-GATE` passes | skill-selection phase group |
| SS-02 | code | Implement/trainable slice for `prompt_understanding` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `prompt_understanding` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-03 | code | Implement/trainable slice for `task_understanding` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `task_understanding` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-04 | code | Implement/trainable slice for `task_type` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `task_type` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-05 | code | Implement/trainable slice for `routing` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `routing` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-06 | code | Implement/trainable slice for `skill_candidates` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `skill_candidates` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-07 | code | Implement/trainable slice for `skill_selection` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `skill_selection` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-08 | code | Implement/trainable slice for `prompt_context` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `prompt_context` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-09 | code | Implement/trainable slice for `result_gap` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `result_gap` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-10 | code | Implement/trainable slice for `output_validation` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `output_validation` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-11 | code | Implement/trainable slice for `task_completion` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `task_completion` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-12 | code | Implement/trainable slice for `intermediate_checks` | one semantic unit + tests/reports | <=15 min | other semantic units, host repos, external AI | `intermediate_checks` checkpoint section | artifact check: held-out unit report + ablation prove checkpoint-backed decision | same phase group |
| SS-13 | asset | Export/import/fresh-load/continue full skill-selection model | models/training-data/tools only | <=15 min | host repos, external AI | manifest + locator + probes | artifact check: manifest lists all 11 unit ids and probes reference current checkpoint hash | phase commit after gate |

## B10. Adaptive user learning

| Work order | Type | Goal | Allowed write scope | Maximum active time | Forbidden actions | Artifact | Gate/review | Commit boundary |
|---|---|---|---|---|---|---|---|---|
| AD-00 | code/check | Define/verify two-leg pair contract for every trainable sphere: PromptToTask, OrchestrationReasoning, SkillSelection, ResultGap | `00.Core`, `43.SmartSkillsDomain` contracts/tools only | <=15 min | host repos, model training, external AI | pair coverage matrix + pair DTOs/adapters if missing | artifact check: every trainable sphere names common/global leg, personal/adaptive leg, backup/export, restore/import, and anonymised/share policy | adaptive phase group |
| AD-01 | code | Convert missing-pattern result gap into adaptive DPO signal | `43.SmartSkillsDomain` result-gap files/tools | <=15 min | common model mutation, host repos, external AI | user-local signal writer | artifact check: writer targets `.xiigen`, records chosen/rejected refs, and does not write common checkpoint | adaptive phase group |
| AD-02 | training | Train user adaptive model from missing-pattern DPO | adaptive training adapter only | <=15 min | common checkpoint overwrite, host repos, external AI | adaptive checkpoint/report | artifact check: common checkpoint hash unchanged and adaptive metric/report improved or explains no-op | same phase group |
| AD-03 | code | Convert user correction/change into adaptive DPO signal | feedback writer/training adapter | <=15 min | common model mutation, host repos, external AI | correction DPO writer | artifact check: chosen/rejected refs present and private raw text redacted | same phase group |
| AD-04 | training | Continue user adaptive model from correction signal | adaptive training adapter only | <=15 min | common checkpoint overwrite, host repos, external AI | adaptive checkpoint/report | artifact check: fresh-load + continue probe reports reference adaptive checkpoint hash | phase commit after gate |
| AD-05 | asset | Common model upgrade while preserving user adaptive state | upgrade planner/tools | <=15 min | user state overwrite, host repos, external AI | upgrade report + patch proposals | artifact check: adaptive state hash unchanged except accepted patches and rollback/load probe exists | phase commit after gate |
| AD-06 | asset | Export/backup user adaptive model package | adaptive asset tools only | <=15 min | common checkpoint overwrite, raw private text export, host repos, external AI | user-model backup package | artifact check: package contains manifest, checkpoint hash, schema version, source/provenance refs, redaction report, and no raw private prompt/corpus rows | same phase group |
| AD-07 | asset/check | Restore/import user adaptive model package into a fresh user profile/device path | adaptive asset tools/reports only | <=15 min | common checkpoint overwrite, host repos, external AI | restore/import report | artifact check: fresh profile loads adaptive checkpoint, verifies hash, and can continue training without mutating common model | same phase group |
| AD-08 | asset/check | User-approved share/copy path for adaptive model package | adaptive asset tools/reports only | <=15 min | raw private text sharing, silent sync, host repos, external AI | share/anonymised-sync report | artifact check: share package is either user-local copy or anonymised signal package; report proves no raw private text, secrets, or private corpus rows leave the device | same phase group |
| AD-09A | asset/check | Produce ResultGap missing-pattern adaptive package backup/restore/share evidence | adaptive assets/reports for ResultGap missing-pattern package only | <=15 min | common checkpoint overwrite, host repos, external AI, PromptToTask/OrchestrationReasoning implementation | ResultGap missing-pattern portability package report | artifact check: AD-01/AD-02 ResultGap adaptive checkpoint has backup/export, restore/import/fresh-profile continue probe, approved share/anonymised evidence, redaction report, and unchanged common checkpoint hash | adaptive portability repair group |
| AD-09B | plan/check | Split PromptToTask missing adaptive portability evidence into bounded rows before final gate | plan/state/reports only for PromptToTask gap | <=15 min | implementation, training, host repos, external AI, OrchestrationReasoning/SkillSelection/ResultGap scope, API-01+ | PromptToTask split plan report | artifact check: current PromptToTask evidence is classified as common-leg only; this row is plan repair, not portability pass; next action is PTT-AD-01 | adaptive portability repair group |
| PTT-AD-01 | code/check | Convert PromptToTask user correction or task-pattern feedback into user-local adaptive DPO signal | PromptToTask contracts/tools/tests/reports only | <=15 min | common checkpoint mutation, model training, host repos, external AI, OrchestrationReasoning/SkillSelection/ResultGap scope, API-01+ | PromptToTask adaptive signal writer | artifact check: writer targets `.xiigen/prompt-to-task`, records chosen/rejected structured-task refs, redacts raw private prompt text, and leaves common checkpoint unchanged | PromptToTask adaptive portability repair group |
| PTT-AD-02 | training/check | Train or no-op-train PromptToTask user adaptive model from PromptToTask DPO signals | PromptToTask adaptive training adapter/assets/reports only | <=15 min | common checkpoint overwrite, host repos, external AI, other sphere scope, API-01+ | PromptToTask adaptive checkpoint/report | artifact check: user-local adaptive checkpoint/report exists, common checkpoint hash unchanged, metric improves or no-op reason is explicit and replay is not silent | PromptToTask adaptive portability repair group |
| PTT-AD-03 | asset/check | Export/backup PromptToTask user adaptive package | PromptToTask adaptive assets/reports only | <=15 min | common checkpoint overwrite, raw private text export, host repos, external AI, other sphere scope, API-01+ | PromptToTask backup package | artifact check: package contains manifest, checkpoint hash, schema version, provenance refs, redaction report, and no raw private prompt/corpus rows | PromptToTask adaptive portability repair group |
| PTT-AD-04 | asset/check | Restore/import and user-approved share/anonymise PromptToTask adaptive package | PromptToTask adaptive assets/reports only | <=15 min | raw private text sharing, silent sync, common checkpoint overwrite, host repos, external AI, other sphere scope, API-01+ | PromptToTask restore/share report | artifact check: fresh profile loads checkpoint, verifies hash, can continue or reports explicit no-op/no replay, approved anonymised/share evidence exists, and common checkpoint remains unchanged | PromptToTask adaptive portability repair group |
| PTT-AD-Z | check | PromptToTask two-leg portability evidence gate before AD-09C | reports only | <=15 min | implementation, training, host repos, external AI, other sphere scope, API-01+ | PromptToTask two-leg matrix | artifact check: fails unless PromptToTask has common leg, personal/adaptive leg, backup/export, restore/import/cross-device probe, approved share/anonymised policy with runtime/probe evidence; no policy-only pass | PromptToTask adaptive portability repair group |
| AD-09C | plan/code/check | Produce OrchestrationReasoning adaptive portability evidence or split missing implementation/training rows before final gate | OrchestrationReasoning contracts/tools/assets/reports only as required by the gap | <=15 min each bounded row | host repos, external AI, PromptToTask/SkillSelection/ResultGap scope, API-01+ | OrchestrationReasoning two-leg portability evidence or honest sub-row gap report | artifact check: OrchestrationReasoning has common leg, personal/adaptive leg, backup/export, restore/import/cross-device probe, approved share/anonymised policy; if implementation/training is required, this row must split into bounded pre-gate work orders and cannot pass on policy-only evidence | adaptive portability repair group |
| OR-AD-01 | code/check | Convert OrchestrationReasoning user correction or plan-pattern feedback into user-local adaptive DPO signal | OrchestrationReasoning contracts/tools/tests/reports only | <=15 min | common checkpoint mutation, model training, host repos, external AI, PromptToTask/SkillSelection/ResultGap scope, API-01+ | OrchestrationReasoning adaptive signal writer | artifact check: writer targets `.xiigen/orchestration-reasoning`, records chosen/rejected scenario-plan refs, redacts raw private prompt/analysis text, and leaves common checkpoint unchanged | OrchestrationReasoning adaptive portability repair group |
| OR-AD-02 | training/check | Train or no-op-train OrchestrationReasoning user adaptive model from OrchestrationReasoning DPO signals | OrchestrationReasoning adaptive training adapter/assets/reports only | <=15 min | common checkpoint overwrite, host repos, external AI, PromptToTask/SkillSelection/ResultGap scope, API-01+ | OrchestrationReasoning adaptive checkpoint/report | artifact check: user-local adaptive checkpoint/report exists, common checkpoint hash unchanged, metric improves or no-op reason is explicit and replay is not silent | OrchestrationReasoning adaptive portability repair group |
| OR-AD-03 | asset/check | Export/backup OrchestrationReasoning user adaptive package | OrchestrationReasoning adaptive assets/reports only | <=15 min | common checkpoint overwrite, raw private text export, host repos, external AI, PromptToTask/SkillSelection/ResultGap scope, API-01+ | OrchestrationReasoning backup package | artifact check: package contains manifest, checkpoint hash, schema version, provenance refs, redaction report, and no raw private prompt/analysis/corpus rows | OrchestrationReasoning adaptive portability repair group |
| OR-AD-04 | asset/check | Restore/import and user-approved share/anonymise OrchestrationReasoning adaptive package | OrchestrationReasoning adaptive assets/reports only | <=15 min | raw private text sharing, silent sync, common checkpoint overwrite, host repos, external AI, PromptToTask/SkillSelection/ResultGap scope, API-01+ | OrchestrationReasoning restore/share report | artifact check: fresh profile loads checkpoint, verifies hash, can continue or reports explicit no-op/no replay, approved anonymised/share evidence exists, and common checkpoint remains unchanged | OrchestrationReasoning adaptive portability repair group |
| OR-AD-Z | check | OrchestrationReasoning two-leg portability evidence gate before AD-09Z | reports only | <=15 min | implementation, training, host repos, external AI, PromptToTask/SkillSelection/ResultGap scope, API-01+ | OrchestrationReasoning two-leg matrix | artifact check: fails unless OrchestrationReasoning has common leg, personal/adaptive leg, backup/export, restore/import/cross-device probe, approved share/anonymised policy with runtime/probe evidence; no policy-only pass | OrchestrationReasoning adaptive portability repair group |
| AD-09Z | check | Final pair coverage gate for all current and future `ILearningMachine` spheres | reports only | <=15 min | implementation, training, host repos, external AI | final two-leg coverage matrix | artifact check: matrix fails if any trainable sphere lacks common leg, personal leg, backup/export, restore/import, cross-device probe, or approved share/anonymised policy; current failed AD-09 matrix is evidence, not phase completion | phase commit after gate |

## B11. Core API boundary

| Work order | Type | Goal | Allowed write scope | Maximum active time | Forbidden actions | Artifact | Gate/review | Commit boundary |
|---|---|---|---|---|---|---|---|---|
| API-01 | code | Expand `XiigenExtensionApiService` to call trained PromptToTask/Reasoning/SkillSelection assets | `43.SmartSkillsDomain` API only | <=15 min | host repos, training runs, external AI | core API result | artifact check: response marks semantic quality only when trained-model gate refs are present | core API phase group |
| API-02 | code | Add method-level request/response contracts for prompt-to-task, plan, skills, feedback, debug | contracts/tools docs only | <=15 min | host repos, training runs, external AI | API contract schema | artifact check: hosts can send request and receive response without model internals or corpus data | same phase group |
| API-03 | check | Parent anti-determinism review | reports only | <=15 min | implementation, host repos, external AI | review matrix | artifact check: static + request trace evidence names every deterministic component and parent signs trainable decisions | no code commit unless report stored |
| CORE-API-INVOCATION-01 | code/check | Create or verify core-owned `XiigenExtensionRequest` executable/script endpoint for `XiigenExtensionApiService.ProcessHostExtensionRequest` | `llm_mvp_core` core API/tools only | <=15 min | host repos, training, external AI, old Phase40 substitution | XiigenExtensionRequest invocation endpoint report | artifact check: endpoint accepts `HostName`, `HostType`, `Method`, `UserPromptOrTaskText`, `HostFacts`, `DebugEnabled`, `RequestedSkills`, `AvailableSkills`, `CheckSetInputs`, `Provenance`, `CorrelationId`; returns `XiigenExtensionResponse`; redacts raw private prompt; calls service trained assets via API-01; does not claim host runtime | phase commit after gate |

## B12. Claude extension repo

CL-02 dependency note: if no valid core-owned endpoint accepts `XiigenExtensionRequest` and invokes `XiigenExtensionApiService.ProcessHostExtensionRequest`, CL-02 must not substitute the old `Phase40ExtensionApiRequest` path. Run `CORE-API-INVOCATION-01` first, then rerun CL-02.

Target: private repo `claude-xiigen-smart-skills` under the same owner.

| Work order | Type | Goal | Allowed write scope | Maximum active time | Forbidden actions | Artifact | Gate/review | Commit boundary |
|---|---|---|---|---|---|---|---|---|
| CL-01 | repo/setup | Verify/create private repo and host package shape | Claude repo only | <=15 min | core semantic logic, training, external AI | repo inventory report | artifact check: real Claude entrypoint source path exists; JSON/PS1-only bundle fails | host repo commit group |
| CL-02 | code | Implement Claude host entrypoint that sends `XiigenExtensionRequest` | Claude entrypoint files only | <=15 min | model logic, training, corpus code, external AI, old Phase40 contract substitution | entrypoint code | depends on CORE-API-INVOCATION-01 when endpoint is missing; artifact check: source refs show call to XIIGen exe/API endpoint for `XiigenExtensionRequest` and no semantic ranking in host | same host group |
| CL-03 | code | Add debug setting and chat display trace | Claude settings/UI files only | <=15 min | raw private trace display, model logic, external AI | debug display | artifact check: chat shape exactly contains `xiigen <method> :`, `input:`, `output:` with redaction evidence | same host group |
| CL-04 | runtime/check | Run bounded Claude smoke through host entrypoint | reports only unless fix needed | <=15 min | external AI unless approved, model training | diagnostic smoke trace report | artifact check: host-runtime invocation evidence exists, or claim is explicitly downgraded to static readiness; smoke is diagnostic evidence only and cannot satisfy full extension acceptance/completion without later source code review, QA automation, and scenario/e2e gates | commit after diagnostic report |

## B13. Cursor extension repo

Target: private repo `cursor-xiigen-smart-skills` under the same owner.

| Work order | Type | Goal | Allowed write scope | Maximum active time | Forbidden actions | Artifact | Gate/review | Commit boundary |
|---|---|---|---|---|---|---|---|---|
| CU-01 | repo/setup | Verify/create Cursor/VS Code extension package | Cursor repo only | <=15 min | core semantic logic, training, external AI | package inventory | artifact check: manifest + extension source entrypoint exist; JSON/PS1-only bundle fails | host repo commit group |
| CU-02 | code | Implement VS Code command/chat/panel entrypoint that sends XIIGen request | Cursor extension source only | <=15 min | model logic, training, corpus code, external AI | entrypoint code | artifact check: source refs show call to xiigen exe/API and no semantic ranking in host | same host group |
| CU-03 | code | Add debug setting and UI/chat trace | Cursor settings/UI files only | <=15 min | raw private trace display, model logic, external AI | debug display | artifact check: UI/chat shape exactly contains `xiigen <method> :`, `input:`, `output:` with redaction evidence | same host group |
| CU-04 | runtime/check | Run bounded Cursor Extension Host smoke request | reports only unless fix needed | <=15 min | external AI unless approved, model training | diagnostic smoke trace report | artifact check: Extension Host invocation evidence exists, or claim is explicitly downgraded to static readiness; smoke is diagnostic evidence only and cannot satisfy full extension acceptance/completion without later source code review, QA automation, and scenario/e2e gates | commit after diagnostic report |

## B14. Codex extension repo

Target: private repo `codex-xiigen-smart-skills` under the same owner.

| Work order | Type | Goal | Allowed write scope | Maximum active time | Forbidden actions | Artifact | Gate/review | Commit boundary |
|---|---|---|---|---|---|---|---|---|
| CX-01 | repo/setup | Verify/create Codex plugin/app package | Codex repo only | <=15 min | core semantic logic, training, external AI | package inventory | artifact check: manifest + plugin/app source entrypoint exist; JSON/PS1-only bundle fails | host repo commit group |
| CX-02 | code | Implement Codex plugin/app entrypoint that sends XIIGen request | Codex plugin source only | <=15 min | model logic, training, corpus code, external AI | entrypoint code | artifact check: source refs show call to xiigen exe/API and no semantic ranking in host | same host group |
| CX-03 | code | Add debug setting and chat trace | Codex settings/UI files only | <=15 min | raw private trace display, model logic, external AI | debug display | artifact check: chat shape exactly contains `xiigen <method> :`, `input:`, `output:` with redaction evidence | same host group |
| CX-04 | runtime/check | Run bounded Codex plugin/app smoke request | reports only unless fix needed | <=15 min | external AI unless approved, model training | diagnostic smoke trace report | artifact check: plugin/app host invocation evidence exists, or claim is explicitly downgraded to static readiness; smoke is diagnostic evidence only and cannot satisfy full extension acceptance/completion without later source code review, QA automation, and scenario/e2e gates | commit after diagnostic report |

## B15. Cross-host gates

| Gate | Ð§Ñ‚Ð¾ Ð´Ð¾ÐºÐ°Ð·Ñ‹Ð²Ð°ÐµÑ‚ | Evidence |
|---|---|---|
| HOST-01 | all three repos have real host entrypoint code | manifest/package + source file refs per host |
| HOST-02 | all three repos are thin clients | no model/training/corpus/ranking code in host repos |
| HOST-03 | all three can send prompt -> task -> plan -> skills request | host trace through XIIGen API as diagnostic evidence plus scenario/e2e test evidence for prompt->task, task->orchestration plan, and task->skills behavior |
| HOST-04 | all three show debug trace safely | exact chat shape with redaction |
| HOST-05 | no external AI/unrelated tools used without approval | explicit tool evidence report |
| HOST-06 | commits/pushes are per touched repo | core repo and each host repo git evidence |
| HOST-07 | host implementation is reviewed as code, not as artifacts | parent code review against real Claude/Cursor/Codex source entrypoints; JSON, PowerShell helpers, smoke traces, static probes, and reports are diagnostic inputs only |
| HOST-08 | host behavior has QA automation after smoke | QA automation and scenario/e2e tests exercise prompt->task, task->orchestration plan, task->skills, debug trace, no host semantic ranking, and no JSON/PS1/report-only substitute |

Smoke evidence rule:

- Runtime smoke, component probes, static reports, JSON evidence, and PowerShell helpers may prove that wiring started or identify a gap.
- They do not prove that a full Claude/Cursor/Codex extension is accepted or complete.
- Final/cross-host gates require source-code review of real entrypoints, QA automation, scenario/e2e behavior checks for prompt->task, task->orchestration plan, task->skills and debug trace, and trained/model-backed evidence whenever semantic behavior is claimed.
- Model-backed `codex exec` is authorized when the active work order says so, but it is still diagnostic smoke evidence only and is not a completion substitute.

No-stop intermediate evidence rule:

- If a real runtime/UI path cannot be invoked safely, the worker must not turn the gap into a product stop while a safe independent path exists.
- Prefer an honest test-only adapter or handler-level QA artifact when it can invoke the same production handler/core boundary and improve coverage without pretending to be UI/runtime evidence.
- If the missing step truly depends on a Luba product decision, record it in a deferred decision queue/end-of-plan queue and continue other independent bounded tasks from this plan.
- Every such artifact must state `handler_level_or_test_only=true` or equivalent and `full_runtime_ui_evidence=false`; final/cross-host matrices must preserve this distinction.

## B16. No-hidden-determinism review owner

Executor self-review is not enough. Parent `REVIEW_INTERNAL` owns final sign-off for every row where deterministic infrastructure could masquerade as learning.

Allowed deterministic infrastructure:

- parse;
- validate schema;
- hash;
- redact;
- serialize reports;
- load/save checkpoints;
- resolve manifests/locators;
- enforce hard policy such as "external AI forbidden".

Not allowed as trainable decisions:

- regex task classification;
- dictionary or exact prompt lookup;
- BM25/filename guide selection;
- host-side skill ranking;
- candidate-position weights as final full semantic skill selection;
- exact-source memory presented as generalization.

Metric honesty rule:

| Evidence type | Allowed claim | Forbidden claim |
|---|---|---|
| Known-route retrieval, exact prompt memory, filename/BM25 match, fixture bootstrap | retrieval/routing/fixture readiness only | trained semantic generalization |
| Near-duplicate held-out row | diagnostic similarity robustness | headline model quality |
| Truly-novel held-out row | trained generalization, if runner-emitted and reproducible | pass based on hand-written JSON |
| Peak epoch or best single row | diagnostic note | final quality unless the gate explicitly asks for peak |
| Average/sliding held-out metric from runner | final quality, if the plan names that metric | replacement by static report field |

If a report says `AccuracyAfter`, `passes_gate`, `trained`, `ready` or similar, parent review must trace the value back to the runner/generator that emitted it. A JSON file written by a script or sub-agent is evidence only when the runner path can reproduce the number from inputs/checkpoints.

Checkpoint writer rule:

- no checkpoint write after unknown/failed gate;
- one writer per checkpoint id;
- resume uses accepted pair ids or accepted source refs, not cursor prefix, path prefix or replay of private raw text;
- corrupted checkpoint is rollback/forensic evidence only, not continuation success;
- fresh-start after failed continuation must be labelled fresh-start and cannot satisfy a continue-training gate.

Required evidence per trainable path:

| Trainable path | Required proof |
|---|---|
| PromptToTaskMachine | checkpoint, manifest, export/import, fresh-load, continue-training, held-out prompt set, ablation without trained state |
| OrchestrationReasoningMachine | guide-state checkpoint, manifest, export/import, fresh-load, continue-training, held-out guide-pattern set, ablation |
| SkillSelectionMachine | all semantic units in checkpoint, manifest, export/import, fresh-load, continue-training, held-out skill-selection set, ablation |
| ResultGap/adaptive learning | user-local DPO signal, adaptive checkpoint delta, common checkpoint unchanged |

Every trainable path above must also pass the two-leg check:

| Two-leg proof | Required evidence |
|---|---|
| Common/global leg | repo-owned checkpoint/manifest/locator/training-data refs in `llm_mvp_core` |
| Personal/adaptive leg | user-owned `.xiigen` checkpoint/DPO/preference refs, loaded next to common state |
| Backup/export | portable user-model package with manifest, checkpoint hash, schema version, provenance refs, and redaction report |
| Restore/import | fresh user profile/device path loads the package and verifies the same adaptive checkpoint hash |
| Share/copy | user-approved local copy or anonymised signal package; no raw private prompts, secrets, or private corpus rows |
| Common upgrade safety | common checkpoint may change; adaptive checkpoint hash stays unchanged unless an explicit accepted patch changes it |

## B17. Mental Experiment Matrix

| Scenario | Expected behavior if plan is implemented | Covered by |
|---|---|---|
| Ð›ÑƒÐ±Ð° Ð¿Ñ€Ð¾ÑÐ¸Ñ‚ Ð½Ð°Ð¿Ð¸ÑÐ°Ñ‚ÑŒ plan for full extension | Extension sends request; PromptToTask extracts plan-only constraints; Reasoning chooses planning workflow; SkillSelection chooses planning/review skills | PTT, OR, SS, host gates |
| Host has no matching skills | ResultGap after AI output writes missing-pattern DPO and adaptive model trains | AD-01, AD-02 |
| Ð›ÑƒÐ±Ð° corrects chosen skills | Feedback writer creates chosen/rejected adaptive DPO and updates user model | AD-03, AD-04 |
| Common skill library upgraded | Common model loads new version; user `.xiigen` state preserved; patch proposals created | AD-05 |
| User wants to back up or move adaptive behavior to another device | User-model package exports, restores into a fresh profile/device path, verifies checkpoint hash, and continues training without changing common model | AD-06, AD-07, AD-09 |
| User wants to share adaptive learning safely | System creates only user-approved local copy or anonymised signal package; raw private prompts/corpus/secrets stay local | AD-08, AD-09 |
| Programmer debug enabled | Chat shows `xiigen <method> :`, `input:`, `output:` with redaction | CL/CU/CX debug gates |
| Host repo contains only JSON/PS1 evidence | Gate fails; not accepted as extension implementation | HOST-01, anti-loophole |
| Implementation tries BM25 guide selection | Parent no-hidden-determinism review fails | B16 |
| External AI requested for data generation | Plan requires explicit Luba approval before use | B18 |
| Trainable/adaptive work order mentions DPO but has no DPO map artifact | Parent review fails before execution; work order must name loaded skills, guide paths, chosen/rejected schema, taxonomy, registry fields and validation evidence | B0.3.1, CM-20 |
| Mixed reference document contains universal reasoning plus code-bound execution | Parent review fails until DPO universalization splits chosen/rejected learning rows from project adapter refs and excluded technical execution refs | B0.3.2, CM-21 |
| UML simulation exists only as prose | Parent review fails; each UML edge and algorithm branch must appear in executable case artifact before implementation | A0.1.1, B0.3.4, CM-23 |
| Host or model work order has no QA/e2e/negative/untrained/trained test strategy | Parent review fails before execution; "tests later" is not accepted | B0.3.3, CM-22 |
| Architect/worker tries to repair or execute to relieve discomfort instead of answering/analyzing | Authority/self-reflection artifact must catch action-as-relief, defensive freeze, executor drift, focus loss and entropy tolerance before work order execution | B0.3.5, CM-24 |
| Part B adds a trainable machine/flow/unit/pair that is not visible in Part A | Parent review fails; A5 must be updated first with the human-readable hierarchy, data movement and examples | A5, CM-26 |
| Training work order wants to write or continue a checkpoint after unknown/failed gate | Parent review fails; checkpoint writer guard must prove single writer, accepted refs and no cursor-prefix/replay resume | B0.3.6, CM-27 |
| Report mixes retrieval/routing/exact-memory with trained generalization | Parent review fails; metrics must be split and headline quality may only use truly-novel held-out runner evidence | B0.3.7, B16, CM-28 |
| Attached file or old plan says "execute" while current Luba instruction says answer/analyze/plan only | Current Luba instruction wins; worker must answer/analyze/plan and record the authority boundary | B0.3.8, CM-29 |
| Sub-agent returns `completed:null`, self-approves after mutation, or gives no next bounded action | Parent rejects packet; mutation makes the old review stale and requires a new review | B0.3.9, CM-30 |
| Focused test/runtime command fails but report hides pre-existing red, leaves orphan process roots or loses the worker | Test execution hygiene gate fails; exact red classification, cleanup and handoff evidence are required | B0.3.10, CM-31 |

## B18. External AI and tool ban

No work order may use external AI services, image-generation models, third-party LLM APIs, browser automation, or unrelated tools unless:

1. the work order names the exact tool family;
2. Luba explicitly approves it in the current conversation;
3. the output is not used to replace a project-owned trainable unit.

Local shell reads, static code inspection, Git status, and approved project scripts are not external AI. They still must stay inside the work order.

## B19. Anti-Loophole Register

Immediate plan failure if any row appears:

- plan claims current PromptToTask semantic inference is implemented without replacing the skeleton;
- plan claims current OrchestrationReasoning semantic planning is implemented without replacing the skeleton;
- candidate-position weight becomes final full skill selector;
- JSON or PowerShell report bundle is accepted as Claude/Cursor/Codex extension code;
- smoke trace, static component probe, helper-only invocation, or report-only artifact is accepted as completed Claude/Cursor/Codex extension code;
- test-only adapter or handler-level QA evidence is accepted as real runtime UI/chat/panel/command evidence or full extension completion;
- runtime smoke is treated as final/cross-host acceptance without later source-code review, QA automation, and scenario/e2e behavior tests;
- host repo owns semantic ranking, training, corpus, DPO materialization, or model checkpoint logic;
- common model upgrade overwrites user `.xiigen` state;
- any trainable `ILearningMachine` sphere lacks a common/global leg plus personal/adaptive leg;
- user adaptive model exists only as a local folder and cannot be backed up, exported, restored, imported, copied/shared, or loaded on a fresh user profile/device path;
- user-model share package contains raw private prompts, raw private analyses, secrets, or private corpus rows;
- missing AI-response pattern does not produce DPO/adaptive update path;
- trainable/adaptive/DPO work order starts without `DPO-KNOWLEDGE-MAP` artifact;
- common or adaptive DPO row contains code-bound technical execution before a separate `TECH-EXEC-LEARNING-GATE` is accepted;
- mixed reference material is used as DPO without splitting `universal_reasoning`, `universal_skill_semantics`, `project_adapter`, `technical_execution` and `evidence_example` segments;
- implementation/model/host work order starts without per-branch test strategy across unit, integration, e2e/host, QA automation, negative, untrained honesty, trained acceptance and regression, or an explicit not-applicable reason;
- UML edge or algorithm branch is implemented without an executable test-case row;
- work order starts without authority/self-reflection artifact covering executor drift, defensive freeze, action-as-relief, focus loss and entropy tolerance;
- Part B introduces a trainable machine, orchestrator, flow, unit or two-leg pair that is not first visible and understandable in A5;
- checkpoint is written by more than one writer, after an unknown/failed gate, from replay/cursor-prefix resume, or from a corrupted checkpoint accepted as continuation;
- exact retrieval, route lookup, fixture bootstrap, filename/BM25 match or exact-source memory is counted as trained semantic generalization;
- hand-written/static JSON metric is accepted without tracing it back to the runner/generator and checkpoint inputs;
- attachment, old plan, state file, report or inferred machine scenario overrides the current explicit Luba instruction;
- sub-agent packet self-approves, returns `completed:null`, hides missing next action, or reuses a review after mutating files;
- test/runtime evidence mixes pre-existing red with current regression, leaves exact orphan process roots, or lacks heartbeat/replacement-worker handoff for long-running work;
- negative DPO examples are generic refusal, absurd bad output, or checklist-only contrast instead of plausible near-miss errors with explicit taxonomy;
- user correction is stored only as chat text and not as adaptive DPO;
- debug mode omits exact `xiigen <method> :`, `input:`, `output:`;
- external AI is used without explicit current approval;
- execution starts before Luba accepts this plan.

## B20. Completion Matrix

| ID | Requirement | Completion evidence |
|---|---|---|
| CM-01 | Plan has readable Part A and OP-49/FC-19 Part A/Part B parity | PlantUML + Russian algorithms + code refs inside this plan; parent `REVIEW_INTERNAL` explicitly checks Part A == Part B for architecture, product decisions, and algorithms |
| CM-02 | PromptToTask trainable unit works | trained checkpoint + held-out prompt-to-task report |
| CM-03 | OrchestrationReasoning trainable unit works | trained guide-state checkpoint + held-out scenario plan report |
| CM-04 | Skill selection semantic unit works | all 11 semantic units represented in checkpoint + held-out report |
| CM-05 | Missing pattern creates adaptive DPO | user-local DPO row + adaptive continue report |
| CM-06 | User correction improves adaptive model | correction DPO row + adaptive checkpoint delta |
| CM-07 | Common/adaptive split preserved | common asset hashes + user state preservation report |
| CM-08 | Every trainable `ILearningMachine` sphere has two legs | two-leg coverage matrix lists PromptToTask, OrchestrationReasoning, SkillSelection, ResultGap with common/global leg and personal/adaptive leg |
| CM-09 | User adaptive model backup/export works | portable user-model package with manifest, checkpoint hash, schema version, provenance refs, and redaction report |
| CM-10 | User adaptive model restore/import works | fresh user profile/device path loads package, verifies adaptive checkpoint hash, and can continue training |
| CM-11 | User adaptive model share/copy is safe | user-approved local copy or anonymised signal package; no raw private prompts, secrets, private analyses, or private corpus rows |
| CM-12 | Common upgrade does not overwrite personal/adaptive leg | common asset hash may change; adaptive hash unchanged except explicit accepted patch |
| CM-13 | Claude extension real host entrypoint exists | Claude repo source refs + parent code review + QA automation/scenario evidence; runtime smoke trace is diagnostic input only |
| CM-14 | Cursor extension real host entrypoint exists | Cursor repo source refs + parent code review + QA automation/scenario evidence; Extension Host smoke trace is diagnostic input only |
| CM-15 | Codex extension real host entrypoint exists | Codex repo source refs + parent code review + QA automation/scenario evidence; plugin/app smoke trace is diagnostic input only |
| CM-16 | Host debug trace works | exact safe chat shape per host plus QA automation proving debug disabled/enabled behavior; component/static/test-only/handler-level probe alone is diagnostic only |
| CM-17 | Host repos are thin | anti-kitchen source sweep and code review for all three repos; report-only/static-only evidence is not enough |
| CM-18 | No hidden deterministic shortcut | parent `REVIEW_INTERNAL` static + runtime/request evidence + source review proving deterministic helpers are not semantic authority |
| CM-18A | Host scenario/e2e behavior works | QA automation and scenario/e2e tests prove prompt->task, task->orchestration plan, task->skills, and debug trace through real extension code paths; test-only adapters and handler-level QA are intermediate coverage and must remain distinct from full UI/runtime evidence |
| CM-18B | Semantic behavior is model-backed when claimed | trained-unit/model-backed evidence from accepted core assets/runners is present for any semantic quality claim; smoke output alone cannot claim semantic behavior |
| CM-19 | No external AI/unrelated tools without approval | tool usage evidence report |
| CM-20 | DPO knowledge map enforced | every trainable/adaptive/DPO work order has `artifacts/dpo-knowledge-map/<work_order>.json` with loaded skills, guide paths, chosen/rejected schema, taxonomy, registry fields, train-ready validation, held-out/fresh-load/continue evidence |
| CM-21 | DPO universalization enforced | every common/adaptive DPO work order has `artifacts/dpo-universalization/<work_order>.json` proving real reference examples were split into universal chosen/rejected rows, project adapter refs, excluded technical execution refs, and private raw text stayed out of committed rows |
| CM-22 | Per-branch test strategy exists | every branch/work order that changes code/model/data/API/host/asset has `artifacts/test-strategy-admission/<work_order>.json` covering unit, integration, e2e/host, QA automation, negative, untrained honesty, trained acceptance and regression, or explicit not-applicable reasons |
| CM-23 | UML simulations are executable test cases | every UML edge and algorithm branch maps to `artifacts/uml-executable-case-map/<work_order>.json` with input fixture, expected fields, expected touched/not-touched assets and expected visible chat/debug output |
| CM-24 | Authority/self-reflection gate ran for every work order | every work order has `artifacts/authority-self-reflection/<work_order>.json` proving required skills loaded and drift/defense/focus/entropy checks passed |
| CM-25 | DPO negatives are real near-misses | every DPO work order has rejected rows that are plausible, taxonomy-labelled, close enough to chosen to teach the model, and not generic refusal/absurd bad output |
| CM-26 | A5 trainable hierarchy is complete | every current and future trainable sphere maps to `ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit` and `ILearningMachinePair` before execution |
| CM-27 | Checkpoint writer guard enforced | every training/adaptive/model asset work order has `artifacts/checkpoint-writer-guard/<work_order>.json` proving single writer, accepted refs, no failed-gate write, no cursor-prefix/replay resume and corrupted-checkpoint policy |
| CM-28 | Metric honesty enforced | every quality/readiness/improvement claim has `artifacts/metric-honesty/<work_order>.json` separating retrieval/routing/exact-memory from trained generalization and tracing metrics to runner/generator evidence |
| CM-29 | Human authority boundary enforced | every plan-review/prompt-to-task/orchestration/host-facing work order has `artifacts/human-authority-boundary/<work_order>.json` proving current Luba instruction wins over attachments, old plans, state, reports and inferred tasks |
| CM-30 | Sub-agent packet quality enforced | every bounded sub-agent packet and parent review has `artifacts/subagent-packet-quality/<work_order>.json` proving capability/tier, no self-approval, stale-after-mutation behavior and accepted verdict vocabulary |
| CM-31 | Test execution hygiene enforced | every test/runtime work order has `artifacts/test-execution-hygiene/<work_order>.json` proving allowed command, red classification, exact orphan cleanup and heartbeat/replacement-worker handoff where needed |
| CM-32 | Commits/pushes complete | one accepted phase commit/push per touched repo |

## B21. Final acceptance gate

Full extension suite is implemented only when every completion row passes and parent architect review says:

```text
ARCHITECT_VERDICT_ALLOW_FINAL
```

Until then, allowed claims are narrower:

- plan written;
- core contract exists;
- host static package exists;
- contract-only core boundary works;
- host runtime smoke passed;
- diagnostic host smoke collected;
- trainable model asset lifecycle passed.

No one may claim "full extension complete" from static contracts, JSON artifacts, PowerShell helpers, smoke traces, test-only adapters, handler-level QA, component probes, or reports alone. Smoke and handler-level QA are diagnostic/intermediate inputs; final acceptance also requires real extension source entrypoints, code review against those sources, QA automation with app/extension behavior, scenario/e2e tests for prompt->task, task->orchestration plan, task->skills and debug trace, and real model-backed/trained-unit evidence where semantic behavior is claimed.
