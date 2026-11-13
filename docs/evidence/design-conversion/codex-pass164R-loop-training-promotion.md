# Codex Pass164R - Loop-created source-disjoint count gate and local promotion

claim_scope: Pass164R focused local proof that loop-created/runtime DesignConversion DPO candidates can be approved through the local artifact DB/review API, pass the existing default dataset count gate with source-disjoint train/held-out rows, run local training, locally promote a checkpoint, and let ModelServer use the promoted local checkpoint. No cloud upload, external provider call, deployment, public endpoint, commit, branch, merge, or full product readiness is claimed.
evidence_scope: Current-turn reads of AGENTS/session/skill files, Pass164/163/157/162/155 evidence, current local artifact DB/training/promotion/ModelServer code, one focused test addition in `Tests/DesignConversionModelServerImprovementLoopTests.cs`, two focused dotnet test outputs, and this evidence file.
count_claimed: 1
independent_evidence_count: 2 focused dotnet test outputs; this evidence file is a summary artifact, not an extra test.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
subagent_identity_self_check: spawned_worker=true; used_NONE=false; parent_must_reconcile=true
downgrade_needed: no for the bounded Pass164R local count-gate/training/local-promotion/ModelServer promoted-checkpoint-use proof after focused tests; yes for full product readiness, cloud upload, external deployment, external provider behavior, production DB readiness, Pass165 learned-improver correction-choice, and Pass166 browser-backed rendering.
pass164R_verdict: PASS

## skill_load_status

| Group | Required file | Status |
|---|---|---|
| SK-SESSION | AGENTS.md | found/read; lines=6913; sha12=1783F2465B18 |
| SK-SESSION | docs/SESSION-START-PROMPT.md | found/read; lines=621; sha12=13EC383411F8 |
| SK-SESSION | docs/skills/planning-session-startup-SKILL.md | found/read; lines=2041; sha12=FDCF254396B7 |
| SK-INTENT | docs/skills/architect-behavior-classifier-SKILL.md | found/read; lines=1469; sha12=397D7D5D1FC1 |
| SK-PLAN | docs/skills/authority-requirement-binding-SKILL.md | found/read; lines=330; sha12=6CF359C0AAFE |
| SK-PLAN | docs/skills/reference-inventory-and-source-map-SKILL.md | found/read; lines=337; sha12=29ACEF3ACC14 |
| SK-PLAN | docs/skills/phase-planning-SKILL.md | found/read; lines=1152; sha12=6A75E636AD33 |
| SK-PLAN | docs/skills/codex-planning-quality-GUIDE.md | found/read; lines=1236; sha12=1C4F5196ABC2 |
| SK-REVIEW | docs/skills/plan-review-SKILL.md | found/read; lines=3747; sha12=AF9764553AB1 |
| SK-REVIEW | docs/skills/goal-delivery-completeness-SKILL.md | found/read; lines=613; sha12=920EE886A13B |
| SK-REVIEW | docs/skills/claim-verification-SKILL.md | found/read; lines=766; sha12=B230CB92DFD3 |
| SK-REVIEW | docs/skills/audit-protocol-SKILL.md | found/read; lines=497; sha12=7B966599925A |
| SK-REVIEW | docs/skills/specificity-calibration-SKILL.md | found/read; lines=307; sha12=810183E959C6 |
| SK-REVIEW | docs/skills/output-skepticism-SKILL.md | found/read; lines=371; sha12=19246E097571 |
| SK-REVIEW | docs/skills/reconnaissance-gate-SKILL.md | found/read; lines=468; sha12=178781622794 |
| SK-ARCH | docs/skills/trainable-architecture-decision-SKILL.md | found/read; lines=738; sha12=FF8B2CF13B00 |
| SK-ARCH | docs/skills/no-product-decisions-SKILL.md | found/read; lines=159; sha12=2E9DCE24F1BA |
| SK-ARCH | docs/skills/output-readability-gate-SKILL.md | found/read; lines=725; sha12=1A57680DFFBF |
| SK-QA/CODE | docs/skills/qa-session-type-SKILL.md | found/read; lines=231; sha12=114DA97E4FC8 |
| SK-QA/CODE | docs/skills/generated-code-review-SKILL.md | found/read; lines=643; sha12=576EF9881E8A |
| SK-QA/CODE | docs/skills/test-integrity-SKILL.md | found/read; lines=776; sha12=AD81C685807E |
| SK-QA/CODE | docs/skills/e2e-test-matrix-SKILL.md | found/read; lines=266; sha12=AAD3DB7880EA |
| SK-QA/CODE | docs/skills/implementation-integrity-SKILL.md | found/read; lines=794; sha12=45074FED65FF |
| SK-QA/CODE | docs/skills/verification-before-completion-SKILL.md | found/read; lines=696; sha12=BDEC446CFC62 |
| SK-QA/CODE | docs/skills/bug-to-tests-SKILL.md | found/read; lines=202; sha12=F17D9190523E |
| SK-QA/CODE | docs/skills/test-failure-triage-SKILL.md | found/read; lines=215; sha12=1A76AFC338DF |
| SK-QA/CODE | docs/skills/self-verification-SKILL.md | found/read; lines=192; sha12=9D604FD1246E |
| SK-QA/CODE | docs/skills/agent-output-format-SKILL.md | found/read; lines=498; sha12=65EADCA1D63C |

## files_read

- `AGENTS.md`; `docs/SESSION-START-PROMPT.md`; required skill files listed above.
- `docs/evidence/design-conversion/codex-pass164-source-disjoint-loop-training.md`.
- `docs/evidence/design-conversion/codex-pass163-same-scenario-local-chain.md`.
- `docs/evidence/design-conversion/codex-pass157-retrain-promotion-boundary.md`.
- `docs/evidence/design-conversion/codex-pass162-product-gap-audit.md`.
- `docs/evidence/design-conversion/codex-pass155-dpo-approval-training-intake-boundary.md`.
- Existing `codex-pass146` through `codex-pass161` evidence was listed/searched as read-only context.
- Current implementation/test files inspected included `44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs`, `44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs`, `44.DesignConversionDomain/DesignConversionTrainingPipeline.cs`, `44.DesignConversionDomain/DesignConversionTrainingRunner.cs`, `44.DesignConversionDomain/DesignConversionPromotedLocalModel.cs`, `25.ModelServer/DesignConversion/DesignConversionModelServerApi.cs`, `Tests/DesignConversionModelServerImprovementLoopTests.cs`, and `Tests/DesignConversionDpoStorageTests.cs`.

## files_changed

- `Tests/DesignConversionModelServerImprovementLoopTests.cs`: added `Pass164R_LoopCreatedRuntimeCandidatesPassDefaultCountGatePromoteAndModelServerUsesCheckpoint` plus local helper methods for the focused proof.
- `docs/evidence/design-conversion/codex-pass164R-loop-training-promotion.md`: this evidence packet.
- No production code was changed. The existing dataset count gate and source-disjoint gates were not weakened.

## commands_run

- `rg` memory/context lookups and repo discovery.
- `Get-Content` reads for required guidance, Pass164/163/157/162/155 evidence, and current implementation/test files.
- Required skill read/hash command over AGENTS/session/skills. A first hash helper failed because PowerShell did not expose `SHA256.HashData`; it is failed helper evidence only. The compatible `SHA256.Create().ComputeHash(...)` rerun produced the skill status table above.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionModelServerImprovementLoopTests.Pass164R_LoopCreatedRuntimeCandidatesPassDefaultCountGatePromoteAndModelServerUsesCheckpoint" --logger "console;verbosity=minimal"` passed: Failed 0, Passed 1, Total 1. Existing unrelated warnings were emitted from other tests during compilation.
- `dotnet test Tests\LlmFromScratch.Tests.csproj --filter "FullyQualifiedName~DesignConversionModelServerImprovementLoopTests.Pass164_SourceDisjointLoopCreatedRuntimeCandidatesPassTrainingIntakeBoundary" --logger "console;verbosity=minimal"` passed: Failed 0, Passed 1, Total 1.

## loop_training_promotion_trace

- Same-source negative control: the first loop-created runtime candidate was created through `DesignConversionModelServerLocalCallerAdapter -> DesignConversionModelServerImprovementLoopHandler -> DesignConversionImprovementLoopCoordinator -> JsonlDesignConversionLocalArtifactDb`, then approved through `DesignConversionDpoUsefulnessApi.ApproveRuntimeCreatedDpoCandidateAsync`.
- The one-candidate intake stayed blocked with `SourceDisjointStatus=blocked_same_source`, blocker `source_disjoint_blocked_same_source`, `model_upload_ready=false`, and `product_ready=false`.
- Count-gate path: the focused test created and approved 100 loop-created runtime candidates from 100 distinct source manifests and 100 distinct source structures, all via the same local loop/DB/review API path.
- Handoff proof: `ApprovedCandidateCount=100`, row count 100, distinct `SourceManifestRef` count 100, distinct `SourceStructureHash` count 100, and every row remained `training_handoff_only=true`, `runtime_created_dpo_candidate=true`, `production_dpo=false`, `product_not_ready=true`, and `model_readiness_claimed=false`.
- Intake/preflight proof: `AcceptedCandidateCount=100`, distinct source identity hash count 100, `SourceManifestStatus=proven`, `SourceDisjointStatus=proven`, no preflight blockers, `source_manifest_candidate_count=100`, `source_manifest_hash_count=100`, `model_upload_ready=false`, and `product_ready=false`.
- Count gate/local training proof: local promotion used a `DesignConversionTrainingPipelineOptions` with default dataset count requirements still intact. Runner evidence asserted `RequiredTrainRecords=50`, `RequiredHeldOutRecords=50`, `ActualTrainRecords=50`, `ActualHeldOutRecords=50`, `DatasetCountGatePassed=true`, and `SourceDisjoint=true`.
- Local weight/promotion proof: training runner invoked local lifecycle, training started, checkpoint mutated, checkpoint candidate created, real weights produced, measurement gate passed, promotion gate status was `PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS`, local promotion ready was true, promoted checkpoint path existed, promoted checkpoint sha differed from a fresh baseline checkpoint, promoted version was not `0.1.0`, learned weight artifact/observation counts were greater than zero, and promoted local model fresh load/real-weights/local-only flags were true.
- ModelServer boundary proof: `DesignConversionPromotedLocalModelLoader.LoadAsync` loaded the promoted checkpoint, `CreateInternalService` created the local service from the promoted machine, and `DesignConversionModelServerApi.SubmitAndEvaluateAsync` returned `LearnedInferenceApplied=true`, `ModelTrainingVerified=true`, promoted model id/version metadata, `cloud_model_upload_attempted=false`, `ModelUploadReady=false`, `DeploymentVerified=false`, and `ProductReady=false`.

## mental_simulation_result

One loop-created runtime candidate cannot form a source-disjoint train/held-out split, so the existing same-source blocker still prevents training. After 100 loop-created candidates with independent source manifest refs and source structures are approved through the local review API, the training intake has enough source identities for source-disjoint preflight. The default dataset builder then splits the 100 source groups into 50 held-out rows and 50 train rows, satisfying the unchanged count gate. The local runner trains in process, produces real local learned weights, and the existing measurement/promotion gate allows a local-only promotion. The promoted checkpoint is fresh-loaded into the local ModelServer service and used at the boundary while cloud upload, deployment, public endpoint, external provider, and product readiness remain false/not claimed.

## verification_results

- PASS: Pass164R focused test, 1 passed, 0 failed.
- PASS: prior Pass164 focused source-disjoint boundary regression, 1 passed, 0 failed.
- PASS: same-source blocker remains asserted in the Pass164R test through `source_disjoint_blocked_same_source`.
- PASS: dataset count gate is not weakened; asserted required train/held-out counts are 50/50 and actual train/held-out counts are 50/50.
- PASS: local promotion and ModelServer promoted checkpoint use are asserted with local-only/no-cloud/no-deploy/product-not-ready boundary metadata.
- PASS: evidence-file readback had exact identity-line count 1, any identity-line count 1, and forbidden identity value count 0.
- PASS: line-start conflict marker scan over changed test/evidence files returned no matches.
- PASS: mojibake marker scan over changed test/evidence files returned no matches.
- PASS: `git diff --check -- Tests\DesignConversionModelServerImprovementLoopTests.cs docs\evidence\design-conversion\codex-pass164R-loop-training-promotion.md` returned exit code 0 with no output.
- PASS: because the changed files are untracked in this worktree, `git diff --no-index --check -- /dev/null <file>` was also run for each changed file; each returned exit code 1 with empty output, expected for no-index differences and with no whitespace warnings.

## remaining_gaps_after_pass164R

- Full product readiness remains not claimed.
- Cloud upload, external provider calls, deployment, public endpoint proof, and production DB readiness remain not proven.
- Pass165 learned-improver correction-choice is not implemented or claimed.
- Pass166 browser-backed rendering is not implemented or claimed.

next_recommended_pass: Pass165 learned-improver correction-choice, unless Luba wants a broader same-scenario package/API-to-promoted-local-ModelServer proof with the 100-candidate count-gate path first.
