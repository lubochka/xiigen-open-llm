# Codex Pass167 - Final Product-Readiness Gap Audit

claim_scope: Pass167 audit-only final product-readiness gap classification after Pass164R, Pass165, Pass166, and Pass166R. This file audits current evidence and current implementation. It does not implement fixes and does not claim full production readiness.
evidence_scope: Fresh read-only inspection of required session and skill files, Pass146 through Pass166R evidence files, the three Claude DesignToCode evidence files, current DesignConversion and ModelServer source/test files, safe platform/FLOW worktree evidence, domain guide existence/readback, and read-only git status. No product code, Claude file, xiigen document, source repo, branch, commit, push, merge, rebase, reset, clean, external provider, cloud, deploy, or broad test action was performed.
count_claimed: 1 Pass167 audit evidence artifact only after readback and hygiene checks. This is not a product completion count, not a test count, and not a full readiness count.
independent_evidence_count: 8 current evidence groups: skill stack readback; Pass146-Pass166R evidence readback; Claude evidence readback; current DesignConversion source readback; current ModelServer source readback; current test source readback; safe platform/FLOW readback; git status and artifact hygiene checks.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
subagent_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true
downgrade_needed: yes. The evidence supports bounded local product capability and safe-worktree route evidence, but not full production readiness and not the full platform/FLOW route consuming browser-backed/promoted local composition.
pass167_verdict: NEEDS_FIX_IMPLEMENTABLE_GAPS

## Skill Load Status

| Group | Required file | Status |
|---|---|---|
| SK-SESSION | C:\Projects\llm_mvp_core\AGENTS.md | found/read |
| SK-SESSION | C:\Projects\llm_mvp_core\docs\SESSION-START-PROMPT.md | found/read |
| SK-SESSION | C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md | found/read |
| SK-INTENT | C:\Projects\llm_mvp_core\docs\skills\architect-behavior-classifier-SKILL.md | found/read |
| SK-PLAN | C:\Projects\llm_mvp_core\docs\skills\authority-requirement-binding-SKILL.md | found/read |
| SK-PLAN | C:\Projects\llm_mvp_core\docs\skills\reference-inventory-and-source-map-SKILL.md | found/read |
| SK-PLAN | C:\Projects\llm_mvp_core\docs\skills\phase-planning-SKILL.md | found/read |
| SK-PLAN | C:\Projects\llm_mvp_core\docs\skills\codex-planning-quality-GUIDE.md | found/read |
| SK-REVIEW | C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md | found/read |
| SK-REVIEW | C:\Projects\llm_mvp_core\docs\skills\goal-delivery-completeness-SKILL.md | found/read |
| SK-REVIEW | C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md | found/read |
| SK-REVIEW | C:\Projects\llm_mvp_core\docs\skills\audit-protocol-SKILL.md | found/read |
| SK-REVIEW | C:\Projects\llm_mvp_core\docs\skills\specificity-calibration-SKILL.md | found/read |
| SK-REVIEW | C:\Projects\llm_mvp_core\docs\skills\output-skepticism-SKILL.md | found/read |
| SK-REVIEW | C:\Projects\llm_mvp_core\docs\skills\reconnaissance-gate-SKILL.md | found/read |
| SK-ARCH | C:\Projects\llm_mvp_core\docs\skills\trainable-architecture-decision-SKILL.md | found/read |
| SK-ARCH | C:\Projects\llm_mvp_core\docs\skills\no-product-decisions-SKILL.md | found/read |
| SK-ARCH | C:\Projects\llm_mvp_core\docs\skills\output-readability-gate-SKILL.md | found/read |
| SK-QA/CODE | C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md | found/read |
| SK-QA/CODE | C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md | found/read |
| SK-QA/CODE | C:\Projects\llm_mvp_core\docs\skills\test-integrity-SKILL.md | found/read |
| SK-QA/CODE | C:\Projects\llm_mvp_core\docs\skills\e2e-test-matrix-SKILL.md | found/read |
| SK-QA/CODE | C:\Projects\llm_mvp_core\docs\skills\implementation-integrity-SKILL.md | found/read |
| SK-QA/CODE | C:\Projects\llm_mvp_core\docs\skills\verification-before-completion-SKILL.md | found/read |
| SK-QA/CODE | C:\Projects\llm_mvp_core\docs\skills\bug-to-tests-SKILL.md | found/read |
| SK-QA/CODE | C:\Projects\llm_mvp_core\docs\skills\test-failure-triage-SKILL.md | found/read |
| SK-QA/CODE | C:\Projects\llm_mvp_core\docs\skills\self-verification-SKILL.md | found/read |
| SK-QA/CODE | C:\Projects\llm_mvp_core\docs\skills\agent-output-format-SKILL.md | found/read |

Domain guides/skills for original requirement 12 were also found/read: `dcm-training-GUIDE.md`, `dcm-dpo-construction-GUIDE.md`, `dcm-measurement-GUIDE.md`, `design-conversion-ilearningmachine-architecture-GUIDE.md`, `design-conversion-model-lifecycle-and-promotion-GUIDE.md`, `design-conversion-training-runbook-GUIDE.md`, `design-conversion-negative-dpo-quality-GUIDE.md`, `design-conversion-tier-budget-policy-GUIDE.md`, `html-css-render-qa-automation-GUIDE.md`, and `design-conversion-dpo-authoring-SKILL.md`.

## Files Read

- Governing files and required skills listed in Skill Load Status.
- Evidence files present from `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass146*.md` through `codex-pass166R*.md`.
- Claude evidence read-only: `claude-SCAFFOLD-TO-TRAINED-FEATURE-PLAN-2026-06-29.md`, `claude-FIX-GUIDE-scaffold-to-implemented-v1.md`, and `claude-AUTONOMY-EXTENSIONS-skills-guides-agents-v1.md`.
- Current core implementation, including `44.DesignConversionDomain\DesignConversionInternalService.cs`, `DesignConversionLocalServiceComposition.cs`, `DesignConversionBrowserQaProviders.cs`, `DesignConversionImprovementLoopCoordinator.cs`, `DesignConversionLocalArtifactDb.cs`, `DesignConversionDpoGenerator.cs`, `DesignConversionDpoUsefulnessApi.cs`, `DesignConversionTrainingPipeline.cs`, `DesignConversionTrainingRunner.cs`, `DesignConversionTrainingMeasurementGate.cs`, `DesignConversionMachine.cs`, `DesignConversionPromotedLocalModel.cs`, and related contracts.
- Current ModelServer implementation under `25.ModelServer\DesignConversion`, including API, caller contract, improvement loop handler, endpoint registry, status/readback, DPO usefulness, local caller adapter, local HTTP host adapter, and ASP.NET endpoint/startup files.
- Current tests including `DesignConversionModelServerImprovementLoopTests.cs`, `DesignConversionRuntimeImproverNonCopyTests.cs`, `DesignConversionBrowserQaProviderTests.cs`, `DesignConversionInternalServiceTests.cs`, `DesignConversionServiceCompositionTests.cs`, `DesignConversionModelServerImprovementLoopCompositionTests.cs`, `DesignConversionModelServerImprovementLoopBrowserIntegrationTests.cs`, and related DPO/training/visual policy tests.
- Safe worktree evidence under `C:\Projects\design-to-code-worktrees\xiigen-platform-dot-net-angular-DesignToCode-pass75` and `C:\Projects\design-to-code-worktrees\xiigen-flows-dot-net-angular-DesignToCode-pass70`.

## Files Changed

- `C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass167-final-product-gap-audit.md`

No product code, Claude file, xiigen document, safe platform/FLOW file, source repo file, git metadata action, or external provider was changed or invoked by this pass.

## Commands Run

- `rg` over memory registry for Pass167/DesignConversion context. No direct relevant memory hit was used as evidence.
- `rg --files`, `Get-ChildItem`, `Get-Content -Raw`, `Select-String`, and `Get-FileHash` over required skills, evidence files, Claude files, current source/tests, domain guides, and safe platform/FLOW worktrees.
- `git status --short` and `git status --short -- <audit file>` read-only.
- `rg`/`Select-String` for current implementation evidence, especially browser-backed composition, promoted local model use, platform/FLOW payload routing, deterministic/provider boundary markers, and remaining gap wording.
- No `dotnet test` command was run during Pass167. Test evidence cited here is from fresh-read Pass164R, Pass165, Pass166, Pass166R packets and current test source, not current execution.
- No git write command, branch operation, external provider call, cloud/deploy action, or approval prompt was run.

## Coverage Table

| # | Original requirement | Current evidence status | Pass/current evidence refs | Gap classification |
|---|---|---|---|---|
| 1 | User sends HTML+CSS result, original tree, original/source tree, and design screenshots to server/API. | proven_bounded_local_and_safe_worktree | Pass161 route/config; safe platform tests for `/conversion/submit` and `/conversion/decision-envelope`; FLOW contracts for `sourceKind`, `html`, `css`, `originalTree`, `screenshots`, `designScreenshots`; ModelServer caller contract. | proven bounded local/safe-worktree evidence; source repo materialization remains external/not-authorized. |
| 2 | Server routes tree/result to trained/learnable ILearningMachine subproject/API boundary. | partially_proven_with_current_product_gap | FLOW safe worktree wires `RealDesignConversionCoreClient` to `DesignConversionInternalService`; Pass164R proves local promoted checkpoint use by ModelServer; current ModelServer API exposes promoted local model metadata. | implementable product gap: FLOW/default route still uses deterministic render/comparison providers and is not proven to consume promoted local ModelServer/browser-backed composition. |
| 3 | Server generates result images in multiple resolutions using QA automation. | proven_bounded_local | Pass166 proves four local Edge-backed PNG render refs and DB persistence; current `BrowserBackedDesignConversionRenderProvider`, PNG inspector, and runtime tests exist. | implementable product gap only for platform/FLOW default consumption; production browser farm is external/not-authorized. |
| 4 | Result images and expected/exported design images are visually checked. | proven_bounded_local | Pass166 local comparison/judge path; current `DesignConversionInternalService` sends expected and actual render refs into comparison and visual judge; `DesignConversionVisualJudgeTests` covers local and redacted external-provider seams. | external visual AI/provider execution not authorized/proven. |
| 5 | Visual check identifies what is wrong and stops/continues by quality threshold and user tier/cost budget. | proven_bounded_local | Pass160; current `DesignConversionVisualQaBoundaryPolicy` and tests cover missing viewport, provider-required, budget cap, continue improvement, and quality reached decisions. | no current implementable local policy gap found. |
| 6 | Result, HTML, comparison images/findings are sent into improvement loop. | proven_bounded_local | Pass152 loop coordinator; Pass163 same-scenario local package to DB/intake blocker; current `DesignConversionModelServerImprovementLoopHandler` and coordinator map refs/findings into loop attempts. | platform/FLOW default composition gap remains under #2/#3. |
| 7 | Loop repeats until required quality/cost stop. | proven_bounded_local | Pass152 focused loop proof; current loop coordinator and visual policy tests include attempt/budget/quality stop behavior. | no current implementable local loop-stop gap found. |
| 8 | Results are saved in special DB for training processing. | proven_bounded_local | Pass153/154 local JSONL artifact DB; Pass163/164R runtime candidates persisted through `JsonlDesignConversionLocalArtifactDb`. | production DB deployment is external/not-authorized. |
| 9 | Local project opens DB and builds DPO by DPO principles for ILearningMachine. | proven_bounded_local | Pass164R: 100 loop-created runtime candidates, source-disjoint train/held-out split, default count gate, local training, promotion; current DB/usefulness/training APIs support handoff. | no current implementable local DPO build gap found. |
| 10 | DPO vector includes internal machine part, source structure, and result. | proven_bounded_local | Pass158; current `DesignConversionDpoGenerator`/dataset/training code carries unit/source/result features and source-structure/source-manifest hashes. | no current implementable local vector gap found. |
| 11 | Negative DPO pairs are quality-created, not fake shortcuts. | proven_bounded_local | Pass164 quality-pair assertions; Pass164R uses runtime-created candidates; current tests assert chosen/rejected visual score ordering, distinct candidates, no quarantine, and no source-disjoint shortcut. | no current implementable local negative-pair gap found. |
| 12 | Guides/skills for DPO/training/measurement exist or are patched/planned. | proven_current_repo | Required skills found/read; domain DCM/training/DPO/measurement/render/tier/lifecycle guides found/read. | no required skill missing in current repo; no edit made in this audit. |
| 13 | DPOs are displayed to user with usefulness indicators. | proven_bounded_local_and_safe_worktree | Pass147-Pass149 safe platform UI/API/browser proof; Pass156 and current `DesignConversionDpoUsefulnessApi`/handler/display rows. | Angular/product deployment remains external/not-authorized. |
| 14 | User can request architect/session to train further and upload/use new weights on server. | proven_bounded_local_for_train_and_use; external_upload_not_proven | Pass163 local training-intake request; Pass164R local training/promotion and ModelServer promoted checkpoint use; current `RunTrainingIntakeAndPromoteLocalAsync` and `DesignConversionPromotedLocalModelLoader`. | cloud upload/public deployment not authorized/proven; platform/FLOW promoted-model route gap under #2. |
| 15 | Branch/mergeability/latest authority constraints respected. | partially_proven_bounded | Pass161 safe worktree patch-apply checks to main/UpdateUniversalSkills/DesignToCode; current Pass167 read-only `git status` saw dirty/untracked local worktree and no git write actions. | source repo materialization, CI, commit, PR, and merge are external/not-authorized in this audit. |
| 16 | No hidden deterministic acceptance or provider success is claimed. | proven_current_boundary | Pass146-Pass166R downgrade language; current code keeps `ProductReady=false`, deployment/cloud/provider false markers, local-only promotion refs, external provider not executed markers, and no production browser farm claim. | continue preserving boundary wording; no current implementable honesty gap found. |

## Mental Simulation Result

Simulated chain:
1. FigmaToCode/CanvaToCode-like package sends HTML/CSS, tree refs/payloads, screenshots/design renders, tier, and budget through safe platform/FLOW and local ModelServer contracts.
2. Safe platform/FLOW can forward payload shape to a real core boundary, and local ModelServer can process package/API-style requests.
3. Local core can render multiple browser-backed PNGs with Edge, compare expected and actual render refs, run the local visual judge, apply tier/budget decisions, and persist refs/findings without external provider calls.
4. The local improvement loop can create runtime DPO candidates, persist them in JSONL artifact DB, display usefulness/readback rows, approve them, and build train-ready handoff.
5. With 100 source-disjoint loop-created candidates, local training passes default count/source gates, produces real local learned weights, promotes a local checkpoint, fresh-loads it, and ModelServer can use it with `LearnedInferenceApplied=true` and `ModelTrainingVerified=true`.
6. Learned DPO/improver weights can change visible HTML/CSS correction rule selection for the same defect context.
7. The chain remains bounded local: product ready, cloud upload, deployment, external provider execution, production browser farm, production DB, and source-repo materialization are false/not proven.

Mental simulation verdict: NEEDS_FIX_IMPLEMENTABLE_GAPS. The local core/ModelServer product slices are strong enough for bounded local capability, but the full platform/FLOW route is not proven or wired to consume the explicit browser-backed/promoted local composition. Therefore this is not full production readiness and not even a clean bounded platform/FLOW product pass.

## Implementable Product Gaps

G1 - Platform/FLOW default route does not consume explicit browser-backed/promoted local composition.
- Evidence: FLOW safe worktree `DesignConversionIntakeServiceCollectionExtensions.cs` registers `DeterministicDesignConversionRenderProvider` and `DeterministicDesignConversionComparisonProvider` by default, while Pass166R proves browser-backed rendering only when explicitly injected into local composition/runtime. FLOW manifest still says real core with deterministic providers until authorized external visual AI.
- Impact: requirements 2 and 3 are not proven through the platform/FLOW route in the same browser-backed/promoted local mode. A reader could only claim bounded local ModelServer/browser proof plus separate platform/FLOW payload proof.
- Next product pass: wire or prove a safe platform/FLOW route that uses `DesignConversionLocalServiceComposition.CreateBrowserBackedLocal` or an equivalent explicit local browser-backed ModelServer composition, while preserving `ProductReady=false`, no production browser farm, no external provider, no cloud/deploy, and no raw payload leakage.

G2 - One all-up local same-scenario product proof remains missing across platform/FLOW intake, browser-backed render, loop-created 100-candidate training, local promotion, and promoted ModelServer use.
- Evidence: Pass163 proves same-scenario package to DB/intake blocker; Pass164R proves 100 loop-created local candidates to training/promotion/ModelServer use; Pass166 proves browser-backed render/runtime persistence; Pass166R reconciles browser markers. These are inspectable but segmented records, not one all-up scenario from safe platform/FLOW intake through promoted browser-backed ModelServer use.
- Impact: the bounded local components are present, but full product-readiness wording would overcombine separate proofs.
- Next product pass: add a focused integration proof or minimal wiring that carries a trace id/source family across platform/FLOW payload intake, browser-backed loop attempts, 100 source-disjoint runtime candidates, local training/promotion, and promoted ModelServer evaluation, still no cloud/deploy/external provider.

## External Or Not Authorized Gaps

- External visual AI/provider execution is not authorized and not proven.
- Cloud model upload, public deployment, public endpoint readiness, production browser farm readiness, and production DB deployment are not authorized and not proven.
- Source repo materialization into `C:\Projects\xiigen-platform-dot-net-angular`, commits, PRs, merges, CI, and deployment checks are forbidden by this audit work order and remain not proven.
- Broad solution tests were not run in Pass167; current test-pass claims are limited to fresh-read prior evidence packets and current test source inspection.

## Governance Skill Repair Queue

- No required Pass167 skill file was missing in `C:\Projects\llm_mvp_core`.
- No domain DCM guide was missing after current readback.
- Optional future governance cleanup: if any active status board or plan consumes Pass162/Pass163 NEEDS_FIX language as current, label it historical/pre-Pass164R-Pass166R evidence and point to this Pass167 verdict. This audit did not edit governance files because the current task permits only this evidence file.

## Verification Results

- Evidence file readback exact identity line count: 1.
- Evidence file readback any identity-token count: 1.
- Forbidden spawned-worker identity value count: 0.
- Line-start conflict marker count: 0.
- Configured mojibake marker count: 0.
- Plain `git diff --check -- docs\evidence\design-conversion\codex-pass167-final-product-gap-audit.md` returned exit code 0 with no output.
- New-file `git diff --no-index --check -- NUL docs\evidence\design-conversion\codex-pass167-final-product-gap-audit.md` returned expected diff exit code 1 with no whitespace warning output.
- `git status --short -- docs\evidence\design-conversion\codex-pass167-final-product-gap-audit.md` reports the audit file as untracked/new. No staging was performed.

## Next Recommended Pass

next_recommended_pass: Pass168 safe platform/FLOW browser-backed promoted-local composition proof. Scope should be read/modify only a safe worktree or explicitly authorized target, not source repo materialization unless Luba separately authorizes it. The pass should preserve no-external-provider/no-cloud/no-deploy/product-not-ready boundaries while proving or wiring the platform/FLOW route to explicit local browser-backed/promoted ModelServer composition.
