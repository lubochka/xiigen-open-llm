# Phase 12 quality rubric

Scale: 0=not proven/fail, 1=partial evidence, 2=pass

Success rule: Adaptive token success is accepted only when adaptive total tokens are lower, no criterion regresses against baseline, and all adaptive rubric criteria pass.

| Criterion | Pass | Fail |
|---|---|---|
| correct_task_extraction | Response must contain structured task evidence or equivalent task extraction evidence for the same held-out prompt. | No task extraction evidence or only generic restatement. |
| universal_orchestration_plan_quality | Response must produce a universal plan with review/evidence/stop gates and no project-stack-specific trick. | No plan, only a placeholder, or a status saying no matching orchestration inference. |
| skill_selection_relevance | Relevant skills are actually selected or the skill selection artifact proves final selection authority without candidate-position leakage. | SelectedSkills is empty, only a requested-skill count is present, or final skill authority is absent. |
| no_project_specific_leakage | The scored user-visible answer does not contain repo paths, model paths, source ids, scenario ids, or pair ids as task authority. | Visible answer uses project paths/source ids/pair ids as answer authority. |
| no_hidden_deterministic_shortcut_claim | No hidden deterministic shortcut is introduced or claimed; candidate-position diagnostics are provenance-only if present. | Candidate order/position becomes selection authority or shortcut evidence is hidden. |
| no_overclaim | No claim of semantic quality, learnability, training, checkpoint promotion, full chat UI, token savings, or all-host readiness beyond evidence. | Any such claim is made without direct Phase 12 proof. |
