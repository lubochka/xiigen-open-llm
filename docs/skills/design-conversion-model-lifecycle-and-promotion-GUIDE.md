# Design Conversion Model Lifecycle And Promotion Guide

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

Этот guide описывает lifecycle для trained DesignConversionMachine: checkpoint,
manifest, export/import, locator/server upload, promote/rollback,
common/adaptive split, and user approval before retrain/upload.

## When to use / Когда использовать

Используй этот guide when a plan/review/work order mentions model asset,
checkpoint, manifest, export, import, backup, server upload, locator update,
promotion, rollback, common model, adaptive user model, retraining, or
continue-training for DesignConversionDomain.

## Inputs / Входы

- Validated training run report with held-out metrics and ablation result.
- Current checkpoint and manifest if continuing from an existing model.
- Common/global asset policy and personal/adaptive asset policy.
- User approval record when private or adaptive data is used.
- Server/locator destination for 25.ModelServer or approved model repository.
- Rollback candidate and previous promoted version.

## Outputs/artifacts / Выходы и артефакты

Every lifecycle pass must produce:

- `checkpoint_ref`: trained state artifact with version and hash.
- `manifest_ref`: machine id, domain tag, version, source corpus refs, metrics,
  gates, privacy scope, and compatibility.
- `repository_owned_model_checkpoint_ref`: when the model belongs to the common
  training program, the checkpoint is not a scratch file and not a user cache.
  It lives under
  `training-artifacts/models/<domain>/<design-pattern>/<model>/<version>/checkpoints/<step>/`.
  A resumed run must load the latest compatible sealed parent from that tree,
  apply the next admitted batch, and publish the child checkpoint back to the
  same tree with the parent manifest and the DPO dataset manifest recorded.
- `export_package_ref`: portable package for backup/share/import.
- `import_report_ref`: proof that package can be imported.
- `fresh_load_report_ref`: proof that a fresh machine loads without retraining.
- `continue_training_report_ref`: proof that training can resume from the
  imported checkpoint.
- `locator_update_ref`: server/locator entry or explicit not-uploaded reason.
- `promotion_report_ref`: promote/keep/rollback decision.
- `rollback_report_ref`: previous version restore path.
- `approval_ref`: user approval for retrain/upload when needed.

## Required gates / Обязательные gates

- Checkpoint gate: no trained claim without checkpoint ref and hash.
- Manifest gate: checkpoint must have manifest with domain, version, data refs,
  metrics, gates, privacy scope, and compatibility notes.
- Export/import gate: the model must export and import through project
  infrastructure, not only exist as a local file.
- Fresh-load gate: a new machine instance loads exported state and runs held-out
  inference without retraining.
- Continue-training gate: imported state accepts an approved next batch and
  records metric continuity.
- Repository-owned continuation gate: if a DesignConversion training run is part
  of the repo-owned learning program, it must use
  `DesignConversionRepositoryOwnedModelCheckpointLifecycle` instead of a
  throwaway temp checkpoint. The loader opens the latest compatible checkpoint
  from `training-artifacts/models`; the publisher writes the next step there
  with a parent pointer and with the exact `training-data/dpo` manifest identity
  that supplied the new preference rows. This is the human rule Luba asked for:
  a model resumes from disk and improves; it does not silently start over.
- Repository-owned run-context factory: before production wiring passes contexts
  into `DesignConversionTrainingPipeline`, it should use
  `DesignConversionRepositoryOwnedTrainingRunContextFactory` to connect one
  already sealed `training-data/dpo/<domain>/<design-pattern>/<version>` dataset
  with one `training-artifacts/models/<domain>/<design-pattern>/<model>/<version>`
  checkpoint line. The factory is intentionally boring: it opens the saved DPO
  dataset, checks the pair files through the sealer, validates the model address
  through the shared layout, snapshots provenance refs, and returns enabled DPO
  plus model contexts. It does not ask Qwen to create new DPO rows, does not
  train, does not write weights, and does not create a model directory. In plain
  language: this is the adapter that proves the next run will reuse repository
  assets. The production ModelServer training-intake handler now owns that
  adapter before `run`, `promote-local`, or `continue-training-upload-status`
  reaches the artifact DB: server configuration opens the sealed DPO dataset
  through the factory and overwrites caller-supplied repository contexts in
  `DesignConversionTrainingPipelineOptions`. If the server has not configured
  repository-owned training, caller-supplied repository contexts fail closed.
  A missing, corrupt, or incomplete sealed DPO dataset fails before the
  training call and before any model checkpoint directory is created.
  The executable local-loopback Program path uses the typed server entrypoint
  `Program.LocalLoopbackRepositoryOwnedTrainingConfiguration.Resolve`, enabled
  only by
  `DESIGN_CONVERSION_MODELSERVER_REPOSITORY_OWNED_TRAINING_ENABLED=true`.
  Repository ownership is mandatory for training actions: missing config or
  `...ENABLED=false` is a fail-closed state, not a legacy-training bypass. The
  local executable bootstrap pins the repository authority to the canonical
  `C:\Projects\llm_mvp_core` checkout; the optional
  `DESIGN_CONVERSION_MODELSERVER_REPOSITORY_ROOT` value is only a deploy-time
  assertion that must match that trusted root and cannot redirect training to a
  different clone, disk, or client-provided path. The public executable Program
  entrypoints are sealed and do not accept repository-owned training options
  from callers; internal bootstrap/test seams that pass explicit options must
  also validate those options against the same trusted root. Lower-level
  `DesignConversionModelServerAspNetHostStartup` overloads remain server
  composition APIs, not authority to bypass the executable Program boundary.
  Deploy-time configuration must also provide the domain slug, design-pattern
  slug, dataset version, model id slug, model version, five runtime SHA-256
  fingerprints, runtime compatibility key, provenance refs, and, when cold
  start is allowed, an explicit cold-start reason. These values are server
  env/config values; client training-intake JSON paths and caller-supplied
  repository contexts are not authority.
- Repository-owned DPO transaction identity factory:
  `DesignConversionRepositoryOwnedDpoTransactionContextFactory` builds only the
  internal identity/context for the DesignConversion repository-owned DPO
  transaction. The trusted repository root is chosen only by the ModelServer
  Program boundary; this factory is not an external authority and must not be
  exposed to arbitrary callers as a way to choose roots, declare material
  trusted, or inject caller-provided hashes. It receives the Program-trusted
  repository root and a separate asserted root, fails closed unless they
  normalize to the same checkout, and builds the DPO address only from the
  trusted root. It accepts the fixed
  `figma-html-css` / `design-to-code` address, dataset version, already-known
  local Qwen/Ollama teacher identity with model artifact digest, generation
  policy identity, creator identity, and paid reference HTML+CSS material whose
  content shape has been canonicalized for hashing. The factory recomputes the
  paid-reference SHA-256 values from canonical in-memory JSON bytes and
  delegates the final request identity to
  `RepositoryOwnedDpoDatasetManifestSealer.CreateRequestIdentity`; caller
  hashes, caller contexts, and alternate roots are not authority. It returns a
  validated `DesignConversionRepositoryOwnedDpoTransactionContext` with the
  canonical `RepositoryOwnedTrainingArtifactLayout` DPO address and
  `RepositoryOwnedDpoDatasetRequestIdentity`. The factory still does not call
  Qwen, does not open or seal a manifest, does not write pair files, and does
  not create a model directory; it only creates the identity that downstream
  storage and authoring code must use.
- ModelServer improvement-loop DPO context wiring: the public route now gets
  repository-owned DPO authority from server composition, not from caller JSON.
  `Program` supplies the trusted `C:\Projects\llm_mvp_core` root and the
  already validated local Qwen/Ollama TP1 teacher configuration through
  `DesignConversionModelServerAspNetHostStartup` and
  `DesignConversionModelServerEndpointRegistry` to
  `DesignConversionModelServerImprovementLoopHandler`. The handler hashes the
  paid HTML/CSS reference from `InitialPackage`, builds deterministic
  generation-policy and creator identities, attaches the repository-owned DPO
  transaction context to the coordinator request, and keeps raw reference
  material out of the manifest, pair rows, metadata, and public response. If
  repository-owned training is configured but the trusted root or server-owned
  teacher configuration is missing or invalid, the route fails before candidate
  work, teacher calls, `training-data/dpo`, or `training-artifacts` writes. The
  clean-missing DPO path has one extra rule: the first identical handler request
  may call the permitted local Qwen/Ollama authoring teacher once, seal the rich
  fixture pairs under the repository-owned `training-data/dpo` address, and
  reopen those bytes through the sealer. The second byte-identical handler
  request must stop on the existing sealed dataset, leave the teacher-call count
  unchanged, and leave every regular file under that dataset root byte-for-byte
  unchanged. That byte comparison is the plain proof that the ModelServer seam
  reused the repository asset instead of regenerating or rewriting it. The
  focused tests use a temporary repo-shaped fixture and prove composition only:
  they do not run real Qwen, do not claim real DPO/model bytes, and do not
  create real trained model weights.
- Pipeline-owned checkpoint call: `DesignConversionTrainingPipeline` is the live
  DesignConversion path that now calls that repository lifecycle. When its
  options include a repository-owned DPO transaction context and a
  repository-owned model checkpoint context, it first opens the last compatible
  sealed checkpoint from
  `training-artifacts/models/<domain>/<design-pattern>/<model>/<version>/`.
  That loaded machine replaces the empty caller machine before the first
  admitted training mutation. After the mutation succeeds, the same pipeline
  seals the next checkpoint step back into that repository tree and records the
  exact sealed DPO manifest from `training-data/dpo`. In plain language: the
  pipeline keeps learning from the saved model on disk; it does not train a new
  throwaway model each time.
- External-transaction publication rule: when DesignConversion consumes a
  prepared external DPO transaction, the child checkpoint is not written to
  `training-artifacts/models` until that external transaction has finished
  successfully. In plain language: a failed external handoff may leave evidence
  that training was attempted, but it must not leave a new repository-owned
  child model that looks ready to continue.
- DPO integrity before mutation: the pipeline opens the DPO dataset with the
  full sealed reader before the first training mutation, so damaged pair files
  cannot be hidden behind a healthy-looking manifest identity.
- Cold-start rule for this pipeline: if no compatible parent checkpoint exists,
  the run must fail before training unless the request carries an explicit
  cold-start reason. A missing, corrupted, ambiguous, or incompatible parent is
  not a warning and is not allowed to fall through into fresh training by
  accident.
- Persistence rule for Luba's repo: DPO datasets generated by the approved local
  open-weight teacher are project training data and belong under
  `training-data/dpo`. Trained checkpoints and model manifests belong under
  `training-artifacts/models`. These folders are repository assets for the
  `codex/dpo-implementation` branch, not scratch cache and not another drive.
- Checkpoint mutation admission gate: promoted-parent continuation may mutate
  DPO weights only with a live verifier-minted
  `DesignConversionDpoCheckpointMutationAdmission` that binds exact checkpoint
  SHA-256, byte length, file/relative identity, current source/training
  manifest SHA, legal-teacher commitment fingerprint and base checkpoint SHA.
  The bridge, not a provider-returned `ModelSet`, owns the mutable-base proof:
  promoted-parent continuation clones the already verified snapshot bytes into
  two independent one-shot custody roles, one hidden bridge-proof source and
  one provider-transfer source. The bridge opens only the hidden proof source,
  recomputes the admitted checkpoint bytes, loads its own
  policy/reference/fresh-load models, fingerprints that state, and compares the
  provider's actual returned weights. Provider acquisition itself is only a
  load/read of an untrusted candidate and must not acquire the server-owned
  mutation permit; the first mutation permit is post-validation provider-result,
  followed by LoRA, trainer step or artifact publication.
  Typed prepared evaluated execution may clone/revalidate captured batches for
  exact binding, but that capture is non-mutating bookkeeping and also cannot
  acquire a server-owned mutation permit before actual-state validation.
  Legacy or unattested checkpoints remain readable/exportable/evaluable but are
  quarantined from mutable continuation until rebuilt or re-attested through the
  controlled verifier; a JSON sidecar or custom provider cannot self-mint this
  authority.
- Locator/server upload gate: promotion must update an approved locator or
  server entry, or record why upload is intentionally blocked.
- Promote/rollback gate: promotion must be reversible to the previous accepted
  checkpoint.
- Common/adaptive split gate:
  - common/global model assets live in `llm_mvp_core` approved model/training
    asset paths;
  - personal/adaptive assets live in the user-approved local state path;
  - personal data cannot silently enter common/global assets.
- User approval gate: retraining on private/user material and uploading an
  adaptive checkpoint require explicit user approval and recorded scope.

## Forbidden substitutes / Запрещенные подмены

- A saved checkpoint without manifest is not lifecycle completion.
- A manifest without import/fresh-load/continue-training probes is not enough.
- Uploading a file without locator/server registration is not promotion.
- Promotion without rollback path is incomplete.
- Common/global checkpoint cannot contain private user screenshots, raw design
  payloads, user ids, project paths, or unapproved proprietary data.
- Adaptive user model cannot be overwritten by common model promotion without
  conflict handling and user approval.

## How it feeds the future plan / Как это питает будущий план

Детали 8a должны include lifecycle work orders after training and before any
trained readiness claim. The plan must name checkpoint, manifest, export/import,
fresh-load, continue-training, locator/server upload, promote/rollback,
common/adaptive split, and user approval paths. This guide prevents "saved
model" from being reported as usable project asset before lifecycle proof.

## Pass40/41 Real-S3 Approval-Validation Boundary

For real-S3 lifecycle work, user/source approval is an intake gate, not a
promotion gate. The approval packet must be classified by `approval-validation`
before any private/adaptive source material can feed lifecycle artifacts.

Routes:

- `approval_packet_missing`
- `template_not_approval`
- `approval_validation_failed`
- `approval_valid_but_not_product_ready`

Only `approval_valid_but_not_product_ready` permits continuation into real-data
artifact construction. It still requires `real_data_still_required` because
approval does not create or validate `source-manifest.json`, `dpo-rows.jsonl`,
`heldout-measurements.json`, or `checkpoint-manifest.json`. A template, scaffold,
schema, or focused validator test is not a checkpoint, not a locator update, not
a rollback path, and not a product-ready promotion; `schema/template/scaffold/focused tests do not prove`
lifecycle readiness.
