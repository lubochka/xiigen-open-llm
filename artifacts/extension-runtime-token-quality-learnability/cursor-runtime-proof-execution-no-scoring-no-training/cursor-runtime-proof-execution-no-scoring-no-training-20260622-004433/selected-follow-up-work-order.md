# Selected follow-up work order

Selected: DOCS-QA-CURSOR-RUNTIME-PROOF-REPAIR-DISCOVERY-NO-SCORING-NO-TRAINING-01

Reason: Cursor GUI/Extension Host launch and extension registration were observed, but positive and negative command invocation did not produce fresh llm_mvp_core traces.

Scope: bounded repair/discovery only for deterministic Cursor extension command invocation. No scoring, training, model mutation, checkpoint promotion, Claude runtime proof, Codex runtime proof, git staging, commit, or push. Parent performs commit/push after review if applicable.
