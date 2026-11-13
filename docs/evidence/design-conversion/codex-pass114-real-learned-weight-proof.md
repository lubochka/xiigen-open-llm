claim_scope:
Pass114 implemented and verified the narrow local DesignConversion learned-weight proof path. A controlled valid distinct-source local training run now persists non-empty learned-weight artifacts derived from sanitized DPO training fields, fresh-load verifies the artifacts after checkpoint reload, and continue-training appends/updates learned-weight state in a separate checkpoint without mutating the source checkpoint. This does not claim external model-server training, upload readiness, deployment, UI, server API completion, or full product completion.

evidence_scope:
Current-turn code inspection, current-turn edits in allowed DesignConversion files, focused DesignConversion tests, required test command output, Pass112/Pass113 evidence readback, and post-edit scans/readbacks. The proof is local to DesignConversion checkpoint/fresh-load/continue-training behavior.

count_claimed:
1 narrow Pass114 product slice.

independent_evidence_count:
9 current-turn evidence records: required skill/context load, code/search inspection, Pass112/Pass113 evidence readback, implementation edits, DesignConversionTrainingPipelineTests run, DesignConversionDpoStorageTests run, required focused DesignConversion test run, static hygiene scans, and this evidence packet/readback.

subagent_id_or_NONE:
UNKNOWN_IN_SUBAGENT

subagent_identity_self_check:
spawned_worker=true; used_NONE=false; parent_must_reconcile=true

downgrade_needed:
no for the narrow Pass114 local learned-weight proof path after tests and scans pass. yes for broader product readiness, external training, upload readiness, production promotion, UI/API/deployment, and full completion.

skill_load_status:
AGENTS.md found/read; docs/SESSION-START-PROMPT.md found/read; docs/skills/planning-session-startup-SKILL.md found/read; docs/skills/architect-behavior-classifier-SKILL.md found/read; docs/skills/authority-requirement-binding-SKILL.md found/read; docs/skills/reference-inventory-and-source-map-SKILL.md found/read; docs/skills/phase-planning-SKILL.md found/read; docs/skills/plan-review-SKILL.md found/read; docs/skills/goal-delivery-completeness-SKILL.md found/read; docs/skills/claim-verification-SKILL.md found/read; docs/skills/audit-protocol-SKILL.md found/read; docs/skills/specificity-calibration-SKILL.md found/read; docs/skills/output-skepticism-SKILL.md found/read; docs/skills/reconnaissance-gate-SKILL.md found/read; docs/skills/trainable-architecture-decision-SKILL.md found/read; docs/skills/no-product-decisions-SKILL.md found/read; docs/skills/output-readability-gate-SKILL.md found/read; docs/skills/implementation-doc-uml-SKILL.md found/read; docs/skills/implementation-doc-algorithms-SKILL.md found/read; docs/skills/implementation-doc-code-refs-SKILL.md found/read; docs/skills/implementation-doc-status-SKILL.md found/read; docs/skills/implementation-doc-hierarchy-SKILL.md found/read.

files_read:
- 44.DesignConversionDomain/DesignConversionCheckpointContracts.cs
- 44.DesignConversionDomain/DesignConversionMachine.cs
- 44.DesignConversionDomain/DesignConversionTrainingPipeline.cs
- 44.DesignConversionDomain/DesignConversionTrainingRunner.cs
- 44.DesignConversionDomain/DesignConversionLocalFreshLoadProbe.cs
- 44.DesignConversionDomain/DesignConversionLocalContinueTrainingProbe.cs
- 44.DesignConversionDomain/DesignConversionPromotion.cs
- 44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs
- 44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs
- 44.DesignConversionDomain/DesignConversionContracts.cs
- 00.Core/LearningMachineContracts.cs
- Tests/DesignConversionTrainingPipelineTests.cs
- Tests/DesignConversionDpoStorageTests.cs
- Tests/DesignConversionDpoUsefulnessApiTests.cs
- Tests/DesignConversionThinClientJobRuntimeTests.cs
- Tests/DesignConversionCheckpointLifecycleTests.cs read as relevant compile/test context only; not edited.
- docs/evidence/design-conversion/codex-pass112-source-manifest-disjoint-proof.md
- docs/evidence/design-conversion/codex-pass113-training-execution-semantics.md

files_changed:
- 44.DesignConversionDomain/DesignConversionCheckpointContracts.cs
- 44.DesignConversionDomain/DesignConversionMachine.cs
- 44.DesignConversionDomain/DesignConversionTrainingPipeline.cs
- 44.DesignConversionDomain/DesignConversionTrainingRunner.cs
- 44.DesignConversionDomain/DesignConversionLocalFreshLoadProbe.cs
- 44.DesignConversionDomain/DesignConversionLocalContinueTrainingProbe.cs
- 44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs
- Tests/DesignConversionTrainingPipelineTests.cs
- Tests/DesignConversionDpoStorageTests.cs
- docs/evidence/design-conversion/codex-pass114-real-learned-weight-proof.md

commands_run:
- Select-String memory lookup for Pass114/Pass113/DesignConversion/real learned weights/governance markers.
- Required skill/context file inventory and full file read command for AGENTS.md, docs/SESSION-START-PROMPT.md, and required docs/skills files.
- rg searches for LearnedWeightsPresent, CheckpointContainsRealTrainedWeights, ExportAsync, SaveCheckpointAsync, FreshLoad, ContinueTraining, weights, learned, real_trained_weights_not_proven, WeightProofStatus, WeightsProduced, and ModelUploadReady.
- Get-Content reads for the required DesignConversion implementation and test files, plus Pass112/Pass113 evidence files.
- dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionTrainingPipelineTests" --no-restore --logger "console;verbosity=minimal"
- dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionDpoStorageTests" --no-restore --logger "console;verbosity=minimal"
- dotnet test Tests\LlmFromScratch.Tests.csproj -p:TargetFrameworks=net9.0 -p:TargetFramework=net9.0 --filter "FullyQualifiedName~DesignConversionThinClientJobRuntimeTests|FullyQualifiedName~DesignConversionDpoUsefulnessApiTests|FullyQualifiedName~DesignConversionDpoStorageTests|FullyQualifiedName~DesignConversionLocalFullLoopE2eTests|FullyQualifiedName~DesignConversionTraining" --no-restore --logger "console;verbosity=minimal"
- post-write conflict marker scan on changed files
- post-write mojibake marker scan on changed files
- git diff --check over changed allowed files
- required evidence/status readback for subagent_id_or_NONE, subagent_identity_self_check, LearnedWeights, CheckpointContainsRealTrainedWeights, WeightsProduced, FreshLoad, ContinueTraining, and ModelUploadReady

tests_result:
- DesignConversionTrainingPipelineTests focused run passed: Failed 0, Passed 13, Skipped 0, Total 13.
- DesignConversionDpoStorageTests focused run passed after stale warning repair: Failed 0, Passed 14, Skipped 0, Total 14.
- Required focused DesignConversion command passed: Failed 0, Passed 40, Skipped 0, Total 40.
- Post-write conflict-marker scan passed with no matches.
- Post-write mojibake marker scan passed with no matches.
- git diff --check over changed allowed files exited 0 with no output.
- Required readback found subagent_id_or_NONE, subagent_identity_self_check, LearnedWeights, CheckpointContainsRealTrainedWeights, WeightsProduced, FreshLoad, ContinueTraining, and ModelUploadReady in changed code/tests/evidence.

implementation_summary:
- Added DesignConversionLearnedWeightCheckpoint and optional learned-weight artifacts to DesignConversionRuntimeUnitCheckpoint.
- DesignConversionMachine now accumulates learned feature weights only from structured DesignConversion DPO training items. Plain text/ad hoc batches remain metadata-only and do not produce weights.
- Learned weights are derived from sanitized fields: unit id mapping, visual score margin, chosen/rejected acceptance delta, usefulness score, internal-vector aggregate shape, pair family, source kind, and SHA-256 source-manifest/source-identity/source-structure hashes. The checkpoint stores feature keys, numeric weights, observation counts, last batch id, derivation labels, source SHA-256 hashes, and evidence markers. It does not store raw tree/html/css/screenshots or record ids in learned-weight artifacts.
- Import rejects a checkpoint that claims LearnedWeightsPresent=true without a valid persisted artifact.
- Fresh-load proof now uses DesignConversionMachine.CheckpointHasRealLearnedWeights(checkpoint) after reload and metadata continuity, not a standalone boolean.
- Continue-training proof writes a separate continued checkpoint, verifies the source checkpoint SHA is unchanged, appends the continuation batch, and reports RealTrainedWeightsContinued=true only when the continued checkpoint contains valid learned-weight artifacts.
- Training lifecycle evidence sets CheckpointContainsRealTrainedWeights=true only from fresh-load proof. WeightProofStatus becomes real_trained_weights_proven for the controlled local path while ModelUploadReady remains false.
- User-facing training intake output removes stale preflight weights_not_produced warnings after the actual run proves weights, while retaining model_upload_not_ready and real_training_not_claimed boundaries.

mental_simulation_result:
Approved distinct-source training intake passes the Pass112 source proof, then the runner enters the Pass113 local lifecycle when the dataset count gate is satisfied. The pipeline converts approved train-split records into structured training items with sanitized source hashes and preference fields. DesignConversionMachine.TrainBatchAsync derives learned numeric feature weights and exports them in unit checkpoint artifacts. SaveCheckpointAsync writes the checkpoint; LoadCheckpointAsync reloads it; LocalFreshLoadProbe verifies metadata continuity and valid learned-weight artifacts after reload, setting CheckpointContainsRealTrainedWeights=true. LocalContinueTrainingProbe loads the source checkpoint, rejects held-out/replay cases, trains a continuation batch into a separate checkpoint, verifies the source checkpoint SHA before/after is unchanged, verifies the continued checkpoint has appended history and learned weights, and keeps PromotionUploadAllowed=false. The final local intake run can report WeightsProduced=true and WeightProofStatus=real_trained_weights_proven while still reporting ModelUploadReady=false.

boundaries_not_proven:
- External model-server training is not proven.
- Upload readiness is not proven.
- Promotion to 25.ModelServer is not proven.
- Held-out semantic quality/measurement gate remains not run in this pass.
- UI, public server API, deployment, production database readiness, and full product completion are not proven.
- The runtime scaffold units still do not execute learned visual behavior at inference time; this pass proves persisted local learned-weight artifacts and lifecycle survival, not full learned runtime authority.

remaining_queue:
- Wire learned weights into runtime inference behavior if product scope requires learned visual decisions, with separate tests.
- Run/implement measurement gate and held-out quality proof before any promotion/upload claim.
- Prove promotion/upload readiness behind the existing measurement and real-weight gates.
- Continue server/API/UI/deployment/full-product work separately.

packet_self_check:
subagent_id_or_NONE appears as UNKNOWN_IN_SUBAGENT, not NONE. subagent_identity_self_check appears with spawned_worker=true; used_NONE=false; parent_must_reconcile=true. The packet claims one narrow Pass114 slice only. It does not claim full product completion or upload readiness.
