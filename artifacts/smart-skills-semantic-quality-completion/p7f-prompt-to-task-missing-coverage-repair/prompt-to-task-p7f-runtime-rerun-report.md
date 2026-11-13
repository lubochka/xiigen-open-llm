# P7F runtime evidence for PromptToTask

## Verdict
P7F candidate was fresh-loaded through `PromptToTaskMachine.LoadCheckpointAsync` and checked on reserved positive rows plus shortcut controls. This is component evidence for parent review, not a full P7 PASS claim and not a semantic quality claim.

## Results
- Reserved positive rows: 4.
- Positives with learned pattern: 4.
- Shortcut controls: 24.
- Shortcut leaks: 0.

## Boundary
Eval/holdout rows were not used as training input. The check uses redacted surrogate prompts and does not pass raw private prompt text.
