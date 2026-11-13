# Codex Pass151 Improver Non-Copy Closure

claim_scope:
One bounded T2/R5 DesignConversion improver non-copy slice. The local runtime improver now creates a deterministic changed HTML/CSS candidate when visual defect signals are present. This is not full product readiness, not external AI visual quality, not loop convergence, and not a trained learned-improver closure.

evidence_scope:
Current-code inspection, focused implementation, focused tests, targeted regression tests, diff check, and marker/mojibake scans inside C:\Projects\llm_mvp_core. The repair guide under C:\Users\lubao\Documents\xiigen was read only as evidence.

count_claimed:
1 bounded Pass151 product slice; 0 full readiness passes; 0 external provider runs; 0 commits; 0 pushes.

independent_evidence_count:
4 evidence classes: pre-fix code/guide diagnosis, focused anti-copy tests, impacted runtime-flow/machine regression tests, and post-change diff/marker verification.

subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT

requested_model: gpt-5.5

requested_reasoning_effort: xhigh

skill_load_status:
- AGENTS.md: found/read.
- docs/SESSION-START-PROMPT.md: found/read.
- docs/skills/planning-session-startup-SKILL.md: found/read.
- docs/skills/architect-behavior-classifier-SKILL.md: found/read.
- docs/skills/plan-review-SKILL.md: found/read.
- docs/skills/claim-verification-SKILL.md: found/read.
- docs/skills/trainable-architecture-decision-SKILL.md: found/read.
- docs/skills/no-product-decisions-SKILL.md: found/read.
- docs/skills/output-readability-gate-SKILL.md: found/read.
- docs/skills/qa-session-type-SKILL.md: found/read.
- docs/skills/generated-code-review-SKILL.md: found/read.
- docs/skills/test-integrity-SKILL.md: found/read.
- docs/skills/implementation-integrity-SKILL.md: found/read.
- docs/skills/verification-before-completion-SKILL.md: found/read.
- docs/skills/bug-to-tests-SKILL.md: found/read.
- docs/skills/test-failure-triage-SKILL.md: found/read.
- docs/skills/self-verification-SKILL.md: found/read.
- docs/skills/agent-output-format-SKILL.md: found/read.

active_task_sentence_source:
Current unquoted user message: "Pass151 product slice: close T2/R5 from the repair guide: DesignConversion ImproverUnit must not copy input HTML/CSS as the 'improved' output."

pre_fix_diagnosis:
- Repair guide evidence T2/R5 identifies DesignConversionRuntimeScaffoldImproverUnit as copying input HTML/CSS.
- Current pre-fix code confirmed the same: the improver built a candidate by carrying input evidence forward, then created DesignConversionImprovementOutput with Html: candidate.Html and Css: candidate.Css. It changed metadata/evidence labels but did not synthesize changed visible HTML/CSS.
- Existing learned-weight improver work before this pass wrapped the scaffold with learned evidence/confidence metadata. It did not drive improver HTML/CSS correction choice or candidate synthesis.

files_changed:
- 44.DesignConversionDomain/DesignConversionTrainedUnits.cs
- Tests/DesignConversionRuntimeImproverNonCopyTests.cs
- Tests/DesignConversionRuntimeFlowTests.cs
- docs/evidence/design-conversion/codex-pass151-improver-non-copy-closure.md

commands_run:
- rg over MEMORY.md for task-relevant prior context.
- Get-Content/read of AGENTS.md, CLAUDE.md, SESSION-START-PROMPT.md, required skills, repair guide evidence, and current DesignConversion code/tests.
- rg searches for ImproverUnit/currentHtml/currentCss/improved/candidate/copy/scaffold/LearnedWeightImproverUnit.
- git status --short, read-only.
- dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionRuntimeImproverNonCopyTests"
- dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionRuntimeFlowTests|FullyQualifiedName~DesignConversionMachineTests"
- dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionRuntimeFlowTests|FullyQualifiedName~DesignConversionMachineTests|FullyQualifiedName~DesignConversionRuntimeImproverNonCopyTests"
- git diff --check on touched files.
- rg marker/mojibake/conflict scans on touched files.

tests_run:
- DesignConversionRuntimeImproverNonCopyTests: passed 2/2.
- First impacted regression run exposed one stale test assertion that still expected copied HTML/CSS: failed 1, passed 6, total 7. This was not a product failure in the new improver; the test expected the old copy behavior.
- After updating that stale assertion, impacted regression run passed 9/9 for DesignConversionRuntimeFlowTests, DesignConversionMachineTests, and DesignConversionRuntimeImproverNonCopyTests.

improver_non_copy_proof:
- Defect case test asserts Candidate.Html != input Html and Candidate.Css != input Css.
- Defect case test asserts output HTML contains dcm-local-improved and output CSS contains visible declarations for contrast and spacing, including color:#111827, background-color:#ffffff, gap:12px, and padding:16px.
- Runtime flow test now asserts the default flow candidate differs from request HTML/CSS when QA/comparison defects are present.
- Evidence marker DESIGN_CONVERSION_IMPROVER_LOCAL_NON_COPY is emitted only when local defect-driven rules were applied.

learned_or_defect_signal_proof:
- Defect categories/descriptions drive deterministic local rules: contrast/color signals add foreground/background declarations; spacing/gap/padding/crowded signals add gap and padding; overflow/wrapping and typography signals have separate bounded rules.
- The current learned improver wrapper can carry sanitized learned-weight evidence, but learned-weight influence over HTML/CSS correction choice is not claimed in this pass.
- Metadata records learned_integration_status=remaining:learned_weights_not_used_for_local_rule_choice and learned_improvement_claimed=False.

verification_results:
- Focused anti-copy tests passed 2/2.
- Impacted flow/machine/non-copy tests passed 9/9 after updating the stale copy assertion.
- git diff --check passed for touched files.
- Marker/mojibake/conflict scan returned no matches for touched code/test/evidence files after evidence writing.

mergeability_status_or_not_checked:
Not checked. No branch, commit, push, checkout, reset, or merge operation was performed.

blockers:
- Full trained learned-improver inference is still not proven because learned improver weights are not yet used to choose the HTML/CSS correction rule in this slice.
- External AI visual quality, visual convergence loop success, and production deployment are not covered by this pass.

downgrade_needed:
Yes: evidence supports only deterministic local non-copy improvement mechanics from defect signals. It does not support full product readiness, external AI quality, trained learned improver quality, or loop convergence.

next_recommended_pass:
Wire trained improver weights into an allowed ILogicalUnit-level improver input/state path and prove a negative DPO/training signal changes the correction choice without external provider calls or raw/private payload leakage.
