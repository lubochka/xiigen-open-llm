# PACKAGE_TRACE_CORPORA_FIXTURE_SELECTOR - 2026-06-29

claim_scope:
- Bounded doc-only selector packet for package artifacts, trace JSON, wiki/corpora/DPO, and Figma nested worktree/fixture decision lanes.
- Created a human-readable routing packet that states what Luba must select, the safe default, future worker type, and expected evidence for each lane.
- This packet performs no package build, no trace replay, no corpora import/export, and no fixture cleanup.

evidence_scope:
- Read `codex-package-artifact-resolver-2026-06-28.md`.
- Read `codex-package-artifact-classification-2026-06-28.md`.
- Read `codex-trace-json-resolver-2026-06-28.md`.
- Read `codex-wiki-domain-corpora-resolver-2026-06-28.md`.
- Read `codex-figma-nested-worktree-resolver-2026-06-28.md`.
- Read `codex-figma-fixture-residue-resolver-2026-06-28.md`.
- Read `codex-proof-task-scenario-queue-2026-06-29.md`.
- Read `codex-preflight-resolution-sweep-2026-06-28.md`.
- Checked that this target packet did not exist before creation.
- Did not inspect package binaries, trace JSON contents, corpus contents, fixture JSON contents, nested worktree file contents, secrets, runtime, browser, tests, builds, git refs, PRs, or external services.

count_claimed:
- selector packet files created: 1
- lanes covered: 4
- source documents read: 8
- package builds executed: 0
- trace replays executed: 0
- corpora import/export actions executed: 0
- fixture cleanup actions executed: 0

independent_evidence_count:
- 9 current-turn evidence records: 8 source document reads plus 1 target nonexistence check before creation.

subagent_id_or_NONE:
- PACKAGE_TRACE_CORPORA_FIXTURE_SELECTOR

downgrade_needed:
- Yes for any broader claim. The evidence supports only this document-only selector packet. It does not prove package provenance, trace safety, trace replay readiness, corpora disposition, training completion, DPO generation, fixture correctness, nested worktree owner intent, clean repo state, merge readiness, runtime/browser readiness, PR readiness, or product readiness.

## Required Boundaries

- `PACKAGE_TRACE_CORPORA_FIXTURE_SELECTOR`
- `no package build`
- `no trace replay`
- `no corpora import/export`
- `no fixture cleanup`
- `Proof readiness items are tasks, not stop blockers`
- `Work happens on DesignToCode in a separate folder or worktree`

These lanes are tasks and decision points, not stops. Missing proof, missing selectors, or deferred artifact policy should become the next bounded worker packet, not a final halt, unless a future worker proves a true external blocker with no safe workaround.

## Safe Overall Default

Safe default for all four lanes:
- defer non-document action;
- preserve current artifacts in place;
- do not inspect sensitive contents unless explicitly selected;
- do not mutate files, repos, branches, PRs, registries, archives, corpora, traces, fixtures, worktrees, or runtime state;
- ask Luba for exact selectors before any future worker does more than document or re-check approved metadata.

This packet is a selector map only. It does not run proof work.

## Lane 1 - Package Artifacts

What it means:
- Package artifacts are `.nupkg` outputs or related package evidence that may be useful as handoff evidence, cleanup candidates, provenance candidates, or publish candidates.
- Prior source packets identify 8 package artifacts: 6 tracked modified platform packages and 2 untracked FLOW-47 package outputs.
- Existing evidence classifies them as unresolved owner/package-policy decisions, not as ready-to-commit, ready-to-delete, ready-to-publish, or source-equivalent binaries.

What Luba must select:
- Package set: `all-8`, `platform-6`, `flows-2`, or an exact path list.
- Ecosystem: NuGet, NPM, mixed, or exact other ecosystem. Current source evidence only names NuGet package candidates.
- Action: defer, restore from HEAD, archive/copy, commit after provenance, cleanup/delete, regenerate/build, or publish/upload.
- Provenance policy: source ref, build recipe, expected package ids/versions, hash rule, and allowed package metadata inspection.
- Mutation policy: whether restore, archive, delete, build, stage, commit, push, or publish is allowed. Default is none.
- Verification policy: exact commands, expected manifest fields, hash algorithm, destination path, and stop conditions.

Safe default:
- Defer package mutation and preserve the package artifacts as unresolved owner decisions.
- Keep `no package build`, no publish, no artifact deletion, no restore, no archive/copy, no stage/commit/push.

Future worker after selection:
- `bounded-package-artifact-policy-selector-worker` for owner choice capture.
- `bounded-package-provenance-worker` if Luba wants commit/publish/provenance decisions.
- `bounded-package-build-worker` only after explicit build selectors and write/run authority.
- `bounded-package-archive-worker` only after exact destination and manifest policy.
- `bounded-package-publish-worker` only after exact registry/feed/credential policy and publish approval.
- `bounded-artifact-deletion-worker` only after explicit destructive approval and backup policy.

Expected evidence:
- Package selector packet with selected paths and action.
- Package manifest with path, package id, version, size, and hash if metadata/hash checks are approved.
- Provenance report if source/package equivalence is checked.
- Build transcript only if a future worker is explicitly authorized to build.
- Publish transcript only if a future worker is explicitly authorized to publish.
- Explicit boundary statement for any non-executed action, especially `no package build`.

## Lane 2 - Trace JSON

What it means:
- Trace JSON is request/response/trace evidence such as `xiigen-codex-request.json`, `xiigen-codex-response.json`, and `xiigen-codex-trace.json`.
- These files can be sensitive. They may contain prompts, responses, local paths, account details, or other private data.
- Existing source packets keep trace work in doc-only defer state because sensitivity and no-secret-print policy are not selected.

What Luba must select:
- Trace purpose: diagnostic, replay fixture, regression evidence, audit archive, import/export bridge, or defer.
- Exact trace file set.
- Sensitivity policy and no-secret-print rule.
- Whether content inspection, redaction, generation, replay, import/export, archive, cleanup/delete, commit, or defer is selected.
- Exact command/script entrypoint if generation or replay is selected.
- Runtime/network/secret policy if replay touches a service, adapter, browser, or external system.
- Output evidence path, schema/version, validation command, and overwrite/merge behavior.

Safe default:
- Defer trace action and preserve the files as unresolved sensitivity decisions.
- Keep no trace JSON generation, `no trace replay`, no evidence mutation, no import/export, no redaction write, no archive, no cleanup/delete, no stage/commit/push.

Future worker after selection:
- `bounded-trace-json-sensitivity-worker` after sensitivity inspection and no-secret-print policy are supplied.
- `bounded-trace-json-generation-worker` only after explicit generation authority and output path.
- `bounded-trace-json-replay-worker` only after explicit replay/runtime authority and expected behavior.
- `bounded-trace-json-import-export-worker` only after exact source/destination, schema, redaction, and validation selectors.
- `bounded-trace-json-archive-cleanup-worker` only after archive/delete approval and backup policy.

Expected evidence:
- Trace selector or sensitivity packet.
- Redacted sensitivity report if content inspection is approved.
- Schema/validation report if generation or import/export is approved.
- Replay transcript only if a future worker is explicitly authorized to replay.
- Exact statement of what was not run, especially `no trace replay`.

## Lane 3 - Wiki / Corpora / DPO

What it means:
- Wiki-domain corpora are generated documentation/training-corpus trees under platform and flows repos.
- Prior classifier evidence cites `.xiigen/wiki-domain` roots for platform and flows with generated corpus counts, but the resolver explicitly says this is not trained weight evidence.
- DPO/training is a separate future path and is not authorized by corpus presence.

What Luba must select:
- Repo set: platform, flows, or both.
- Action: defer, recount metadata, archive, cleanup/delete, commit, waive, or training/DPO prep.
- Sensitivity/provenance policy and whether any corpus content may be sampled or printed.
- Archive destination, cleanup backup policy, commit scope, or waiver wording if relevant.
- For training/DPO prep only: exact corpus roots, command/script entrypoint, output directory, checkpoint/weight policy, evaluation criteria, compute/runtime permission, and promotion rules.

Safe default:
- Defer corpus disposition and preserve both corpus roots in place.
- Keep no training, no DPO generation, `no corpora import/export`, no archive/copy, no cleanup/delete, no stage/commit/push, no waiver acceptance, no model-promotion claim.

Future worker after selection:
- `bounded-wiki-domain-corpora-decision-packet-worker` for owner selection.
- `bounded-wiki-domain-corpora-recount-worker` if Luba approves read-only metadata recount.
- `bounded-wiki-domain-corpora-archive-worker` if archive destination and manifest policy are selected.
- `bounded-wiki-domain-corpora-cleanup-worker` only after explicit destructive approval and backup policy.
- `bounded-wiki-domain-corpora-commit-scope-worker` if commit scope is selected, before any staging/commit approval.
- `bounded-wiki-domain-training-dpo-preflight-worker` only if Luba explicitly asks for training or DPO preparation.

Expected evidence:
- Corpora decision packet naming selected repos and action.
- Metadata recount report if approved.
- Archive manifest if archive is later approved and executed.
- Cleanup before/after verification only if destructive cleanup is later approved and executed.
- Training/DPO preflight only if explicitly requested.
- Explicit boundary statement: no training, no DPO generation, `no corpora import/export`.

## Lane 4 - Figma Nested Worktree / Fixtures

What it means:
- This lane has two related but separate Figma residue decisions:
  - nested worktree: `C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney`, previously observed as ahead and dirty;
  - fixtures: three generated layout report JSON files previously observed as status-only `M` with empty content diff checks in source reports.
- The nested worktree may contain owner or other-model work. The fixture reports may be status/index/metadata residue. Neither condition proves safe cleanup, safe restore, or fixture correctness.

What Luba must select:
- Nested worktree policy: preserve/exclude, owner-inspect, stash, restore, cleanup/reset, delete, move, commit, or push.
- Fixture policy: preserve status-only, refresh index, waive, restore, owner-inspect, regenerate fixtures, cleanup/delete, move, or stash.
- Exact targets: nested worktree path, selected dirty file paths, and/or selected fixture paths.
- Owner intent: preserve, inspect, discard, commit, archive, or unknown.
- Content inspection policy for nested file contents or fixture JSON contents.
- Index/working-tree mutation policy: whether `git update-index --refresh`, restore, delete, move, stash, regenerate, stage, commit, push, or PR mutation is allowed.
- Backup/preservation policy and post-action verification commands.

Safe default:
- Preserve and exclude nested worktree from unrelated cleanup.
- Preserve fixture status-only boundary.
- Keep `no fixture cleanup`, no nested worktree cleanup, no delete/move/stash/restore, no branch/ref mutation, no fixture regeneration, no index refresh, no stage/commit/push/PR mutation, no tests/builds/browser/runtime.

Future worker after selection:
- `bounded-figma-nested-worktree-owner-intent-decision-prep-worker` for nested owner decision options.
- `bounded-figma-nested-worktree-preserve-exclude-worker` for read-only re-check if selected.
- `bounded-figma-nested-worktree-mutation-worker` only after exact mutation approval.
- `bounded-figma-fixture-owner-decision-prep-worker` for fixture options.
- `bounded-figma-fixture-preserve-status-only-worker` for read-only re-check if selected.
- `bounded-figma-fixture-mutation-worker` only after exact fixture mutation approval.

Expected evidence:
- Nested worktree decision matrix and current read-only metadata if re-check is approved.
- Fixture decision matrix and current status/diff/index snapshot if re-check is approved.
- Before/after status only if a future mutation is explicitly approved and executed.
- Backup, stash, branch, restore, regeneration, or cleanup evidence only if that exact action is selected later.
- Explicit boundary statement: `no fixture cleanup` and no delete/move/stash/restore unless separately evidenced.

## Decision Summary For Luba

| Lane | Luba selects | Safe default | Future worker | Expected evidence |
| --- | --- | --- | --- | --- |
| Package artifacts | package set, action, provenance, mutation, verification | defer, preserve, `no package build` | package selector/provenance/build/archive/publish/delete worker depending on action | selector packet, manifest, provenance/build/publish evidence only if authorized |
| Trace JSON | file set, sensitivity, purpose, action, replay/generation/import/export policy | defer, preserve, `no trace replay` | trace sensitivity/generation/replay/import-export/archive-cleanup worker | selector or sensitivity report; replay/generation transcript only if authorized |
| Wiki/corpora/DPO | repo set, action, sensitivity, archive/cleanup/commit/waive/training-prep policy | defer, preserve, `no corpora import/export` | corpora decision/recount/archive/cleanup/commit/training-DPO-preflight worker | decision packet, recount/archive/cleanup/training-preflight evidence only if authorized |
| Figma nested/fixtures | owner intent, nested policy, fixture policy, exact targets, mutation/inspection/backup policy | preserve/exclude, status-only boundary, `no fixture cleanup` | nested/fixture decision or mutation worker depending on action | decision matrix, read-only snapshot, before/after evidence only if authorized |

## Continuation Rule

`Proof readiness items are tasks, not stop blockers`.

For this packet, that means:
- Package ambiguity is a package selector/provenance/build/archive/publish/delete task.
- Trace ambiguity is a sensitivity/generation/replay/import-export/archive-cleanup task.
- Corpora ambiguity is a corpus disposition or training/DPO-preflight task.
- Fixture ambiguity is a nested-worktree or fixture owner-decision task.

`Work happens on DesignToCode in a separate folder or worktree` remains the branch/workspace boundary for future implementation/proof work. This selector packet does not change branches, does not create a worktree, and does not perform any merge or mutation.

## Final Boundary

This packet does not execute any selected lane.

No package build.
No package publish.
No artifact deletion.
No trace JSON generation.
No trace replay.
No evidence mutation.
No corpora import/export.
No training.
No DPO generation.
No fixture cleanup.
No nested worktree cleanup.
No delete/move/stash/restore.
No git, branch, PR, merge, commit, push, stage, test, build, browser, runtime, external AI, or secret action.

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-package-trace-corpora-fixture-selector-2026-06-29.md`

verification:
- Required follow-up verification is a readback/literal check for this packet only.
- No tests/builds/browser/runtime/git/external-AI/package/trace/corpora/fixture execution should be inferred from this document.

Class C yes-no:
- yes, if Class C means doc-only decision-prep/selector work with no product/runtime/artifact mutation.
