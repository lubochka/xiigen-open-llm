# Codex Pass165 - Learned improver correction-choice proof

claim_scope: Pass165 bounded local DesignConversion proof that learned DPO/improver weights can change which visible HTML/CSS correction rule is selected for the same visual defect context. This is not browser-backed rendering, external-provider behavior, cloud upload, deployment, or full product readiness.
evidence_scope: Current-turn reads of AGENTS/session/skill files, Pass150/151/162/164R evidence, read-only Claude DesignToCode files, current DesignConversion improver/machine/loop/code paths, focused production/test edits, and focused dotnet test runs in C:\Projects\llm_mvp_core.
count_claimed: 1
independent_evidence_count: 2 passing focused dotnet test outputs; the first failing diagnostic run is recorded as repair evidence only and is not counted as a pass.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
subagent_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true
downgrade_needed: no for the bounded Pass165 learned correction-choice proof after focused tests; yes for full product readiness, browser-backed rendering, external visual AI/provider execution, cloud upload/deploy, production DB readiness, and general model quality.
pass165_verdict: PASS

## skill_load_status

| Group | Required file | Status |
|---|---|---|
| SK-SESSION | AGENTS.md | found/read |
| SK-SESSION | docs/SESSION-START-PROMPT.md | found/read |
| SK-SESSION | docs/skills/planning-session-startup-SKILL.md | found/read |
| SK-INTENT | docs/skills/architect-behavior-classifier-SKILL.md | found/read |
| SK-PLAN | docs/skills/authority-requirement-binding-SKILL.md | found/read |
| SK-PLAN | docs/skills/reference-inventory-and-source-map-SKILL.md | found/read |
| SK-PLAN | docs/skills/phase-planning-SKILL.md | found/read |
| SK-PLAN | docs/skills/codex-planning-quality-GUIDE.md | found/read |
| SK-REVIEW | docs/skills/plan-review-SKILL.md | found/read |
| SK-REVIEW | docs/skills/goal-delivery-completeness-SKILL.md | found/read |
| SK-REVIEW | docs/skills/claim-verification-SKILL.md | found/read |
| SK-REVIEW | docs/skills/audit-protocol-SKILL.md | found/read |
| SK-REVIEW | docs/skills/specificity-calibration-SKILL.md | found/read |
| SK-REVIEW | docs/skills/output-skepticism-SKILL.md | found/read |
| SK-REVIEW | docs/skills/reconnaissance-gate-SKILL.md | found/read |
| SK-ARCH | docs/skills/trainable-architecture-decision-SKILL.md | found/read |
| SK-ARCH | docs/skills/no-product-decisions-SKILL.md | found/read |
| SK-ARCH | docs/skills/output-readability-gate-SKILL.md | found/read |
| SK-QA/CODE | docs/skills/qa-session-type-SKILL.md | found/read |
| SK-QA/CODE | docs/skills/generated-code-review-SKILL.md | found/read |
| SK-QA/CODE | docs/skills/test-integrity-SKILL.md | found/read |
| SK-QA/CODE | docs/skills/e2e-test-matrix-SKILL.md | found/read |
| SK-QA/CODE | docs/skills/implementation-integrity-SKILL.md | found/read |
| SK-QA/CODE | docs/skills/verification-before-completion-SKILL.md | found/read |
| SK-QA/CODE | docs/skills/bug-to-tests-SKILL.md | found/read |
| SK-QA/CODE | docs/skills/test-failure-triage-SKILL.md | found/read |
| SK-QA/CODE | docs/skills/self-verification-SKILL.md | found/read |
| SK-QA/CODE | docs/skills/agent-output-format-SKILL.md | found/read |

## files_read

- C:\Projects\llm_mvp_core\AGENTS.md
- C:\Projects\llm_mvp_core\docs\SESSION-START-PROMPT.md
- Required docs/skills files listed in skill_load_status.
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass150-real-training-noop-closure.md
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass151-improver-non-copy-closure.md
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass162-product-gap-audit.md
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass164R-loop-training-promotion.md
- C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\claude-SCAFFOLD-TO-TRAINED-FEATURE-PLAN-2026-06-29.md (read-only)
- C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\claude-FIX-GUIDE-scaffold-to-implemented-v1.md (read-only)
- C:\Users\lubao\Documents\xiigen\llm mvp\design to code session\claude-AUTONOMY-EXTENSIONS-skills-guides-agents-v1.md (read-only)
- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionTrainedUnits.cs
- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionMachine.cs
- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionImprovementLoopCoordinator.cs
- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionCheckpointContracts.cs
- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionUnitContracts.cs
- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionContracts.cs
- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionRoutingFlow.cs
- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionDpoGenerator.cs
- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionDpoDatasetBuilder.cs
- C:\Projects\llm_mvp_core\Tests\DesignConversionRuntimeImproverNonCopyTests.cs
- C:\Projects\llm_mvp_core\Tests\DesignConversionMachineTests.cs
- C:\Projects\llm_mvp_core\Tests\DesignConversionThinClientJobRuntimeTests.cs

## files_changed

- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionTrainedUnits.cs
- C:\Projects\llm_mvp_core\44.DesignConversionDomain\DesignConversionMachine.cs
- C:\Projects\llm_mvp_core\Tests\DesignConversionRuntimeImproverNonCopyTests.cs
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass165-learned-improver-correction-choice.md

No claude-* file, C:\Users\lubao\Documents\xiigen file, platform repo file, branch, commit, push, merge, rebase, reset, clean, browser-backed renderer, external provider, cloud, or deploy action was performed.

## commands_run

- Memory lookup in C:\Users\lubao\.codex\memories\MEMORY.md for relevant DPO/planning governance context.
- Required skill/session read/hash command over AGENTS.md, docs/SESSION-START-PROMPT.md, and the requested docs/skills files.
- Get-Content and rg read-only inspection over Pass150/151/162/164R evidence, read-only Claude DesignToCode files, and current DesignConversion code/tests.
- dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionRuntimeImproverNonCopyTests" --logger "console;verbosity=minimal" failed once before fixture repair: failed 1, passed 2, total 3. The failure showed the test phrase "Text" also triggered the existing typography rule, so the fixture wording was tightened without changing production code.
- dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionRuntimeImproverNonCopyTests" --logger "console;verbosity=minimal" passed: failed 0, passed 3, total 3.
- dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionRuntimeImproverNonCopyTests|FullyQualifiedName~DesignConversionMachineTests" --logger "console;verbosity=minimal" passed: failed 0, passed 7, total 7.
- git diff read-only inspection over the changed allowed code/test files.

## learned_choice_trace

- Existing deterministic path before Pass165: `DesignConversionRuntimeScaffoldImproverUnit` derived local rules from visual defects. With both contrast and spacing defects, fallback selected `contrast|spacing` and emitted HTML with `dcm-local-improved` plus CSS containing `color:#111827`, `background-color:#ffffff`, `gap:12px`, and `padding:16px`. It remained labelled `learned_integration_status=remaining:learned_weights_not_used_for_local_rule_choice` and `DETERMINISTIC_LOGIC_CLAIMED_AS_LEARNING_FALSE`.
- Pass165 training feature path: `DesignConversionMachine.TrainBatchAsync` now reads sanitized improver DPO chosen/rejected correction categories. A chosen category receives a positive `correction_category:<rule>` learned weight; a rejected category receives a negative `correction_category:<rule>` learned weight. Raw HTML/CSS/tree/screenshot payloads are not required for this feature.
- Learned state A: chosen response `correction_category=spacing`, rejected response `correction_category=contrast`, chosen visual score 0.92, rejected visual score 0.12. Checkpoint weights include `correction_category:spacing > 0` with derivation `chosen_correction_category_preference` and `correction_category:contrast < 0` with derivation `rejected_correction_category_penalty`.
- Learned state B: chosen response `correction_category=contrast`, rejected response `correction_category=spacing`, same score margin. Checkpoint weights flip the positive/negative preference.
- Runtime feature path: for the same contrast+spacing visual defect input, `BuildImproverRuntimeFeatures` exposes `correction_category:contrast=1.0` and `correction_category:spacing=1.0`.
- Selection path: `LearnedWeightImproverUnit` builds learned correction preferences from matching checkpoint contributions and passes them to the local improver seam. The local improver selects the highest positive learned matching rule instead of applying all deterministic rules.
- Output proof: learned state A selected only `spacing`, with CSS containing `gap:12px` and `padding:16px` and not `color:#111827`. Learned state B selected only `contrast`, with CSS containing `color:#111827` and `background-color:#ffffff` and not `gap:12px`. Both changed `Output.Css`, both preserved `Output.Css == Candidate.Css`, and the two learned outputs differed for the same defect context.
- Evidence labels: learned outputs include `DESIGN_CONVERSION_IMPROVER_LEARNED_CORRECTION_CHOICE_APPLIED`, `DESIGN_CONVERSION_RUNTIME_LEARNED_INFERENCE_APPLIED`, `learned_correction_choice_source=local_checkpoint_sanitized_features`, and `learned_integration_status=bounded:learned_weights_ranked_local_correction_choice`.

## mental_simulation_result

Chosen/rejected DPO correction categories are converted into local learned checkpoint weights. A fresh runtime machine imports those checkpoint weights. During the same visual defect context, the critic still supplies the same contrast and spacing defects. Runtime feature extraction maps those defects to matching correction-category features. The learned improver wrapper calculates category contributions and passes only sanitized category preferences to the local improver. The improver ranks available local correction rules by positive learned contribution, selects spacing or contrast depending on the checkpoint, and emits visibly different CSS output. Without learned weights, the deterministic fallback stays stable, applies both local rules, and remains labelled local/deterministic. This proves bounded learned correction-choice behavior, not browser-rendered visual quality or full product readiness.

## verification_results

- PASS: focused Pass165 improver test class passed after fixture repair: failed 0, passed 3, total 3.
- PASS: impacted improver+machine regression filter passed: failed 0, passed 7, total 7.
- PASS: deterministic fallback assertion remains stable and labelled local/deterministic.
- PASS: same input defect/context plus learned state A and learned state B produced different selected correction rules and different CSS output.
- PASS: learned choice is tied to `correction_category:*` weights produced by `TrainBatchAsync` and imported through a fresh checkpoint, not to a no-op marker.
- PASS: post-write evidence readback showed exact identity-line count 1 and any identity mention count 1.
- PASS: mojibake marker scan over changed files returned 0 matches.
- PASS: line-start conflict-marker scan over changed files returned 0 matches.
- PASS: git diff --check over changed tracked domain files returned exit code 0 with no output.
- PASS: git diff --no-index --check over the untracked focused test file returned only the expected difference exit code and no whitespace warning output.
- PASS: git diff --no-index --check over the new untracked evidence file returned only the expected difference exit code and no whitespace warning output.

## remaining_gaps_after_pass165

- Full product readiness remains not claimed.
- Browser-backed rendering remains Pass166 and is not implemented here.
- External visual AI/provider execution, cloud upload, deployment, public endpoint proof, and production DB readiness remain not proven.
- This pass proves bounded local learned correction choice, not general model quality or visual-fidelity convergence.

next_recommended_pass: Pass166 browser-backed/default render QA path proof, unless Luba asks for a narrower same-scenario chain extension first.
