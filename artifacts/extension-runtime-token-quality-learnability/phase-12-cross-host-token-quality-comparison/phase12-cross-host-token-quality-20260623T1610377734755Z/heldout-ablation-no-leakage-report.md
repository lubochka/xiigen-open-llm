# Phase 12 held-out, ablation, no-leakage checks

Общий статус: PASS_WITH_LIMITATIONS.
Полная disjoint-проверка против всего training/source corpus: False.

| Check | Status | Evidence |
|---|---|---|
| heldout-phase12-heldout-prompt-to-task-01-claude | PASS_WITH_LIMITATION | Hand-authored Phase 12 universal workflow prompt; no project, repo, model path, host path, pair id, or source id embedded. Full training corpus disjointness not proven by Phase 12. |
| ablation-phase12-heldout-prompt-to-task-01-claude | PASS_SIGNAL_CHANGED_NOT_SEMANTIC_PROOF | Adaptive requested_skills_count=3; ablation requested/available skills removed. |
| no-runtime-authority-leakage-phase12-heldout-prompt-to-task-01-claude | PASS_NO_SCENARIO_OR_PAIR_ID_IN_RUNTIME_REQUEST | Pair/scenario ids are used in file paths and outer report rows, not intentionally passed as request authority. |
| candidate-position-authority-phase12-heldout-prompt-to-task-01-claude | PASS_NO_CANDIDATE_POSITION_EVIDENCE | authority_detected=False; provenance_only=False. |
| heldout-phase12-heldout-prompt-to-task-01-cursor | PASS_WITH_LIMITATION | Hand-authored Phase 12 universal workflow prompt; no project, repo, model path, host path, pair id, or source id embedded. Full training corpus disjointness not proven by Phase 12. |
| ablation-phase12-heldout-prompt-to-task-01-cursor | PASS_SIGNAL_CHANGED_NOT_SEMANTIC_PROOF | Adaptive requested_skills_count=3; ablation requested/available skills removed. |
| no-runtime-authority-leakage-phase12-heldout-prompt-to-task-01-cursor | PASS_NO_SCENARIO_OR_PAIR_ID_IN_RUNTIME_REQUEST | Pair/scenario ids are used in file paths and outer report rows, not intentionally passed as request authority. |
| candidate-position-authority-phase12-heldout-prompt-to-task-01-cursor | PASS_NO_CANDIDATE_POSITION_EVIDENCE | authority_detected=False; provenance_only=False. |
| heldout-phase12-heldout-prompt-to-task-01-codex | PASS_WITH_LIMITATION | Hand-authored Phase 12 universal workflow prompt; no project, repo, model path, host path, pair id, or source id embedded. Full training corpus disjointness not proven by Phase 12. |
| ablation-phase12-heldout-prompt-to-task-01-codex | PASS_SIGNAL_CHANGED_NOT_SEMANTIC_PROOF | Adaptive requested_skills_count=3; ablation requested/available skills removed. |
| no-runtime-authority-leakage-phase12-heldout-prompt-to-task-01-codex | PASS_NO_SCENARIO_OR_PAIR_ID_IN_RUNTIME_REQUEST | Pair/scenario ids are used in file paths and outer report rows, not intentionally passed as request authority. |
| candidate-position-authority-phase12-heldout-prompt-to-task-01-codex | PASS_NO_CANDIDATE_POSITION_EVIDENCE | authority_detected=False; provenance_only=False. |
| heldout-phase12-heldout-orchestration-plan-01-claude | PASS_WITH_LIMITATION | Hand-authored Phase 12 universal orchestration prompt; no project-stack-specific trick or training-source pointer embedded. Full training corpus disjointness not proven by Phase 12. |
| ablation-phase12-heldout-orchestration-plan-01-claude | PASS_SIGNAL_CHANGED_NOT_SEMANTIC_PROOF | Adaptive requested_skills_count=4; ablation requested/available skills removed. |
| no-runtime-authority-leakage-phase12-heldout-orchestration-plan-01-claude | PASS_NO_SCENARIO_OR_PAIR_ID_IN_RUNTIME_REQUEST | Pair/scenario ids are used in file paths and outer report rows, not intentionally passed as request authority. |
| candidate-position-authority-phase12-heldout-orchestration-plan-01-claude | PASS_NO_CANDIDATE_POSITION_EVIDENCE | authority_detected=False; provenance_only=False. |
| heldout-phase12-heldout-orchestration-plan-01-cursor | PASS_WITH_LIMITATION | Hand-authored Phase 12 universal orchestration prompt; no project-stack-specific trick or training-source pointer embedded. Full training corpus disjointness not proven by Phase 12. |
| ablation-phase12-heldout-orchestration-plan-01-cursor | PASS_SIGNAL_CHANGED_NOT_SEMANTIC_PROOF | Adaptive requested_skills_count=4; ablation requested/available skills removed. |
| no-runtime-authority-leakage-phase12-heldout-orchestration-plan-01-cursor | PASS_NO_SCENARIO_OR_PAIR_ID_IN_RUNTIME_REQUEST | Pair/scenario ids are used in file paths and outer report rows, not intentionally passed as request authority. |
| candidate-position-authority-phase12-heldout-orchestration-plan-01-cursor | PASS_NO_CANDIDATE_POSITION_EVIDENCE | authority_detected=False; provenance_only=False. |
| heldout-phase12-heldout-orchestration-plan-01-codex | PASS_WITH_LIMITATION | Hand-authored Phase 12 universal orchestration prompt; no project-stack-specific trick or training-source pointer embedded. Full training corpus disjointness not proven by Phase 12. |
| ablation-phase12-heldout-orchestration-plan-01-codex | PASS_SIGNAL_CHANGED_NOT_SEMANTIC_PROOF | Adaptive requested_skills_count=4; ablation requested/available skills removed. |
| no-runtime-authority-leakage-phase12-heldout-orchestration-plan-01-codex | PASS_NO_SCENARIO_OR_PAIR_ID_IN_RUNTIME_REQUEST | Pair/scenario ids are used in file paths and outer report rows, not intentionally passed as request authority. |
| candidate-position-authority-phase12-heldout-orchestration-plan-01-codex | PASS_NO_CANDIDATE_POSITION_EVIDENCE | authority_detected=False; provenance_only=False. |
| heldout-phase12-heldout-skill-selection-01-claude | PASS_WITH_LIMITATION | Hand-authored Phase 12 universal skill-selection prompt; no candidate position, pair id, path, or source id embedded. Full training corpus disjointness not proven by Phase 12. |
| ablation-phase12-heldout-skill-selection-01-claude | PASS_SIGNAL_CHANGED_NOT_SEMANTIC_PROOF | Adaptive requested_skills_count=4; ablation requested/available skills removed. |
| no-runtime-authority-leakage-phase12-heldout-skill-selection-01-claude | PASS_NO_SCENARIO_OR_PAIR_ID_IN_RUNTIME_REQUEST | Pair/scenario ids are used in file paths and outer report rows, not intentionally passed as request authority. |
| candidate-position-authority-phase12-heldout-skill-selection-01-claude | PASS_NO_CANDIDATE_POSITION_EVIDENCE | authority_detected=False; provenance_only=False. |
| heldout-phase12-heldout-skill-selection-01-cursor | PASS_WITH_LIMITATION | Hand-authored Phase 12 universal skill-selection prompt; no candidate position, pair id, path, or source id embedded. Full training corpus disjointness not proven by Phase 12. |
| ablation-phase12-heldout-skill-selection-01-cursor | PASS_SIGNAL_CHANGED_NOT_SEMANTIC_PROOF | Adaptive requested_skills_count=4; ablation requested/available skills removed. |
| no-runtime-authority-leakage-phase12-heldout-skill-selection-01-cursor | PASS_NO_SCENARIO_OR_PAIR_ID_IN_RUNTIME_REQUEST | Pair/scenario ids are used in file paths and outer report rows, not intentionally passed as request authority. |
| candidate-position-authority-phase12-heldout-skill-selection-01-cursor | PASS_NO_CANDIDATE_POSITION_EVIDENCE | authority_detected=False; provenance_only=False. |
| heldout-phase12-heldout-skill-selection-01-codex | PASS_WITH_LIMITATION | Hand-authored Phase 12 universal skill-selection prompt; no candidate position, pair id, path, or source id embedded. Full training corpus disjointness not proven by Phase 12. |
| ablation-phase12-heldout-skill-selection-01-codex | PASS_SIGNAL_CHANGED_NOT_SEMANTIC_PROOF | Adaptive requested_skills_count=4; ablation requested/available skills removed. |
| no-runtime-authority-leakage-phase12-heldout-skill-selection-01-codex | PASS_NO_SCENARIO_OR_PAIR_ID_IN_RUNTIME_REQUEST | Pair/scenario ids are used in file paths and outer report rows, not intentionally passed as request authority. |
| candidate-position-authority-phase12-heldout-skill-selection-01-codex | PASS_NO_CANDIDATE_POSITION_EVIDENCE | authority_detected=False; provenance_only=False. |
