# Codex Pass64 - Visual QA Tier Boundary

## Claim Boundary

claim_scope: Pass64 implemented a bounded local S4 boundary for browser QA manifest coverage, visual judge provider-required status, tier policy stop reasons, and improvement loop trace.
evidence_scope: Local code inspection, focused unit tests with fake/in-memory providers, no external AI call, no real S3, no product readiness claim.
count_claimed: 1 sequential local Pass64 slice.
independent_evidence_count: 1 local implementation/evidence record plus focused test outputs from this pass.
subagent_id_or_NONE: Pass64_USER_ASSIGNED_NO_PLATFORM_ID_VISIBLE.
downgrade_needed: yes for product readiness, real browser screenshot readiness, external visual AI verification, and trained-feature claims.

## Inventory

- Existing render QA code already included `BrowserBackedDesignConversionRenderProvider` and fake-browser tests. That path can create render evidence when a local browser adapter exists, but this pass did not claim real browser screenshots.
- Existing visual judge code already included `NoConfiguredVisualJudge` and external HTTP provider boundaries. Before this pass, the missing-provider path used a legacy blocked status but did not expose the exact `visual_judge_provider_required` / `external_provider_not_configured` boundary in the new loop trace.
- Existing internal evaluation rendered up to `MaxRenderQaRuns` and called the visual judge, but the final trace did not explicitly validate multi-resolution render manifest coverage, tier budget/quality stop reason, or product-not-ready boundary.
- Pass60 through Pass63 evidence kept render QA, visual judge, and tier-policy loop work in the remaining queue; Pass62 mergeability was synthetic and required recheck after later edits.

## Repair

### multi_resolution_render

- Added `DesignConversionVisualQaBoundaryPolicy` to compare required render manifest viewports with observed render QA rows.
- Missing viewport evidence produces `render_manifest_incomplete`, records missing viewport ids, and keeps `product_not_ready`.
- Unit coverage proves a single desktop render cannot satisfy a required desktop-plus-mobile manifest.

### visual_judge_provider_required

- Updated `NoConfiguredVisualJudge` to return `provider_required` with `external_provider_not_configured` evidence while preserving the legacy blocker code as metadata/evidence.
- The boundary does not call or fake Claude/OpenAI/Gemini/Gemini-like visual AI.
- Unit coverage proves the no-provider path blocks AI-verdict claims.

### tier_policy

- Added tier stop computation for free/paid/pro/internal thresholds, estimated cost, and budget cap.
- Stop reasons are explicit: `continue_improvement`, `quality_reached`, `budget_cap_reached`, `provider_required`, and `render_manifest_incomplete`.
- Unit coverage proves budget cap, quality reached, and safe continue behavior with fake local verdicts.

### improvement_loop

- `DesignConversionInternalService` now records `VisualQaBoundaryTrace` containing render coverage, provider status, external-AI-executed flag, best score, estimated cost, tier decision, evidence refs, warnings, and hard false `ProductReady` / `TrainedFeatureClaimed`.
- QA rows now preserve the visual judge verdict instead of silently reporting only deterministic comparison verdicts.
- Deterministic image comparison remains local evidence, not hidden trainable authority.

## Tests

- First focused run failed in `DesignConversionVisualJudgeTests.VisualJudgeResult_CanFeedComparisonAndDpoVisualVerdictModel` because the current anti-leakage generator hashes raw evidence refs to `dcm-dpo-evidence-*`. That test was repaired to assert the anti-leakage-safe behavior instead of raw evidence leakage.
- Final focused post-artifact run passed 39/39:
  `DesignConversionVisualQaBoundaryPolicyTests`,
  `DesignConversionInternalServiceTests`,
  `DesignConversionVisualJudgeTests`,
  `DesignConversionBrowserQaProviderTests`,
  `DesignConversionDomainTests`,
  `DesignConversionThinClientJobRuntimeTests`,
  `DesignConversionTrainingPipelineTests`.
- These are local/fake/provider-boundary tests. They are not real browser E2E proof and not real external AI visual verification.

## mergeability

- Post-artifact non-mutating temp-index `git merge-tree` against `origin/main` and `origin/codex/chat-gpt-5.5` found 9 overall branch conflict headers for each target.
- The displayed conflict contexts were in existing unrelated branch/governance/evidence files (`AGENTS.md`, `CLAUDE.md`, `docs/plans/evidence/.../heldout.*`, `docs/skills/claim-verification-SKILL.md`, `docs/skills/output-readability-gate-SKILL.md`, `docs/skills/phase-completion-packager-SKILL.md`, `docs/skills/plan-review-SKILL.md`, `docs/skills/ui-testing-SKILL.md`).
- A path-focused scan showed no conflict-nearby hits for the Pass64 touched paths:
  `44.DesignConversionDomain/DesignConversionVisualQaBoundaryPolicy.cs`,
  `44.DesignConversionDomain/DesignConversionInternalService.cs`,
  `44.DesignConversionDomain/DesignConversionVisualJudge.cs`,
  `Tests/DesignConversionVisualQaBoundaryPolicyTests.cs`,
  `Tests/DesignConversionInternalServiceTests.cs`,
  `Tests/DesignConversionVisualJudgeTests.cs`,
  `docs/evidence/design-conversion/codex-pass64-visual-qa-tier-boundary.md`.
- Mergeability is therefore downgraded: the overall branch is not merge-clean, while this pass did not observe a conflict in the S4 paths.
- No merge, checkout, rebase, reset, commit, push, PR, or destructive cleanup was performed.

## Mental Simulation

### Free tier

Input: required desktop and mobile viewports, budget tier `free`, no configured external visual judge.

1. If only desktop render evidence exists, `multi_resolution_render` is incomplete and the loop stops as `render_manifest_incomplete`.
2. If desktop and mobile evidence exists but the visual judge provider is still unavailable, the loop stops as `provider_required` with `visual_judge_provider_required` and `external_provider_not_configured`.
3. With a fake local judge below threshold and cost still below the free cap, tier policy returns `continue_improvement`.
4. With a fake local judge below threshold and accumulated/estimated cost at or above the free cap, tier policy returns `budget_cap_reached`.

### Pro tier

Input: required desktop and mobile viewports, budget tier `pro`, fake provider verdicts for local tests only.

1. Complete render evidence plus fake score below pro threshold and remaining budget returns `continue_improvement`.
2. Complete render evidence plus fake score at or above pro threshold returns `quality_reached`.
3. Complete render evidence plus fake score below threshold but estimated cost beyond the pro cap returns `budget_cap_reached`.
4. None of those fake-provider paths prove real visual AI, real browser screenshot readiness, or trained feature readiness.

## product_not_ready

Product readiness remains blocked because this pass did not produce real render-image artifacts across target browsers, did not call a configured external visual AI provider, did not run a full browser E2E workflow, and did not train or promote a model.

## remaining_queue

- Configure and prove an external visual judge provider boundary with real provider evidence, redaction, cost accounting, and blocked-safe failure behavior.
- Produce real browser/render artifacts for the required viewport matrix and store inspectable image refs.
- Run full improvement-loop E2E with real render evidence and provider verdicts.
- Recheck mergeability after each future edit because the worktree is dirty and Pass62 mergeability was synthetic.
