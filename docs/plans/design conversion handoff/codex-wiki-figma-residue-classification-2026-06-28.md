# codex-wiki-figma-residue-classification-2026-06-28

must_stop_after_report=true

## Claim And Evidence Scope

claim_scope: bounded classification report for `.xiigen/wiki-domain/**` in platform/flows, Figma nested worktree residue from `.claude/worktrees/romantic-dewdney`, and Figma fixture report dirty status.

evidence_scope:
- Read-only file enumeration and size/count summaries for:
  - `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain`
  - `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain`
- Read-only git path status/tracked-file checks for both `.xiigen/wiki-domain` roots.
- Read-only manifest excerpts and JSONL record counts inside both `.xiigen/wiki-domain` roots.
- Read-only `git status`/`git diff` summary probes for:
  - `C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney`
  - `C:\Projects\FigmaToCode-design-gen\tests\fixtures\generated\*-layout-report.json`

count_claimed:
- wiki-domain roots inspected: 2
- wiki-domain files found: 2,243 total
- wiki-domain bytes found: 72,092,030 bytes total
- wiki-domain JSONL records counted: 39,984 total
- Figma nested worktree modified files found: 1
- fixture report paths checked: 3
- fixture report paths with content delta: 0

independent_evidence_count: 10 usable inspection command outputs. Two helper probes failed due PowerShell formatting/syntax and were superseded by successful reruns; they are not used as classification evidence.

subagent_id_or_NONE: bounded-wiki-figma-residue-classifier-worker; no nested sub-agent used by this worker.

downgrade_needed: no for this classification report. Yes for any cleanup, commit, stage, push, or repo-edit claim: none was performed and none is authorized in this work order.

## Executive Classification

No cleanup was performed. No repo files were edited. No staging, commit, or push was performed.

Primary finding: both `.xiigen/wiki-domain` trees are untracked generated domain-corpus outputs, not currently tracked repository content. Their manifests describe documentation/training corpus status rather than trained weight output, and their counts match the generated documentation/relations/questions/training corpus pattern.

Primary blocker: the Figma nested worktree is dirty and one commit ahead of its remote branch. It should be treated as likely owner/other-model work and must not be cleaned, staged, or folded into unrelated work without an explicit owner decision.

Fixture reports: three fixture report files are shown as modified by `git status`, but per-file `git diff --name-status`, `--numstat`, `--raw`, and `--quiet` show no content delta for all three. This classifies as index refresh/waive, not commit content.

## Wiki-Domain: Platform Repo

Root:
`C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain`

Git evidence:
- `git status --porcelain -- .xiigen/wiki-domain`: `?? .xiigen/wiki-domain/`
- `git ls-files -- .xiigen/wiki-domain`: 0 tracked files

Counts:
- files: 191
- bytes: 5,177,720
- extensions: 180 `.md`, 8 `.jsonl`, 3 `.json`
- JSONL records: 3,090

Top-level composition:
- `documentation`: 180 files, 414,581 bytes
- `relations`: 4 files, 942,421 bytes
- `training`: 2 files, 1,973,860 bytes
- `questions`: 1 file, 1,179,894 bytes
- `evidence`: 1 file, 601,427 bytes
- `eval`: 1 file, 64,709 bytes
- root manifests: 2 files, 828 bytes

Manifest evidence:
- `domain-manifest.json`: `project_id=xiigen-platform-dot-net-angular`, `documentation_version=post-completion-recheck-20260526101418`, `method_count=180`, `call_edge_count=285`, `value_flow_edge_count=285`, `negative_dpo_pair_count=180`, `question_database=question_seeds_generated`
- `model-manifest.json`: `model_status=documentation_ready_training_pending`, `weight_artifact_ref=not_trained_yet`, `local_cache_path=.xiigen/wiki-domain/weights/`

Representative paths:
- `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain\domain-manifest.json`
- `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain\model-manifest.json`
- `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain\documentation\xiigen-platform-dot-net-angular_CorrelationContext_17_76f41aceed574f5d.md`
- `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain\evidence\evidence-index.jsonl`
- `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain\questions\question-seeds.jsonl`
- `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain\relations\documentation-units.jsonl`
- `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain\training\batch-001\documentation-dpo-pairs.jsonl`
- `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain\training\batch-001\negative-dto-candidates.jsonl`

Classification:
- corpus commit candidate: yes, if the intended repo policy is to preserve the generated wiki/training corpus with the project.
- generated cache: no concrete cache file observed in file enumeration; the manifest mentions `.xiigen/wiki-domain/weights/`, but `weight_artifact_ref=not_trained_yet`.
- archive candidate: yes, as an alternative to committing, because this is generated corpus output and may be too bulky/noisy for the product repo.
- cleanup candidate needing approval: yes only if Luba/owner confirms the corpus is accidental or reproducible and should not be retained.
- hard blocker: yes for repo hygiene until owner decides commit vs archive vs approved cleanup; not a content-corruption blocker.

Exact next bounded action:
Create a bounded owner-decision packet for this repo: commit generated corpus, archive generated corpus outside repo, or approve cleanup. Do not delete or commit from this classifier worker.

## Wiki-Domain: Flows Repo

Root:
`C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain`

Git evidence:
- `git status --porcelain -- .xiigen/wiki-domain`: `?? .xiigen/wiki-domain/`
- `git ls-files -- .xiigen/wiki-domain`: 0 tracked files

Counts:
- files: 2,052
- bytes: 66,914,310
- extensions: 2,041 `.md`, 8 `.jsonl`, 3 `.json`
- JSONL records: 36,894

Top-level composition:
- `documentation`: 2,041 files, 5,923,402 bytes
- `relations`: 4 files, 13,251,889 bytes
- `training`: 2 files, 26,598,273 bytes
- `questions`: 1 file, 12,018,680 bytes
- `evidence`: 1 file, 8,396,679 bytes
- `eval`: 1 file, 724,564 bytes
- root manifests: 2 files, 823 bytes

Manifest evidence:
- `domain-manifest.json`: `project_id=xiigen-flows-dot-net-angular`, `documentation_version=post-completion-recheck-20260526100601`, `method_count=2041`, `call_edge_count=4160`, `value_flow_edge_count=4160`, `negative_dpo_pair_count=2041`, `question_database=question_seeds_generated`
- `model-manifest.json`: `model_status=documentation_ready_training_pending`, `weight_artifact_ref=not_trained_yet`, `local_cache_path=.xiigen/wiki-domain/weights/`

Representative paths:
- `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain\domain-manifest.json`
- `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain\model-manifest.json`
- `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain\documentation\xiigen-flows-dot-net-angular_AdaptiveRagDeepResearchService_102_78098814dfe082a6.md`
- `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain\evidence\evidence-index.jsonl`
- `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain\questions\question-seeds.jsonl`
- `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain\relations\documentation-units.jsonl`
- `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain\training\batch-001\documentation-dpo-pairs.jsonl`
- `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain\training\batch-001\negative-dto-candidates.jsonl`

Classification:
- corpus commit candidate: yes, if this generated wiki/training corpus is intended as retained evidence or training material.
- generated cache: no concrete cache file observed in file enumeration; the manifest mentions `.xiigen/wiki-domain/weights/`, but `weight_artifact_ref=not_trained_yet`.
- archive candidate: yes, strongly, because the tree is large: 66,914,310 bytes and 2,052 files.
- cleanup candidate needing approval: yes only if Luba/owner confirms the corpus is accidental or reproducible and should not be retained.
- hard blocker: yes for repo hygiene until owner decides commit vs archive vs approved cleanup; not a content-corruption blocker.

Exact next bounded action:
Create a bounded owner-decision packet for this repo, with size impact called out: commit corpus, archive corpus, or approve cleanup. Do not delete or commit from this classifier worker.

## Figma Nested Worktree Residue

Root:
`C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney`

Status/diff summary evidence:
- branch/status: `## claude/romantic-dewdney...origin/claude/romantic-dewdney [ahead 1]`
- dirty file: `M src/ui/components/App.tsx`
- diff name-status: `M src/ui/components/App.tsx`
- diff numstat: `592 insertions, 592 deletions`
- diff stat: `1 file changed, 592 insertions(+), 592 deletions(-)`
- parent repo status marker: `m .claude/worktrees/romantic-dewdney`

Classification:
- likely owner/other-model work: yes.
- generated cache: no evidence; this is a nested worktree with a real source file modification and one unpushed commit.
- cleanup candidate: no, not without explicit owner approval and worktree-owner handoff.
- hard blocker: yes for any parent/root repo cleanup, commit, or push that would need a clean nested-worktree state.

Exact hard blocker:
The nested worktree is both ahead of origin by 1 commit and dirty in `src/ui/components/App.tsx`. This worker is forbidden to modify, clean, stage, commit, or push it.

Exact safe next action:
Delegate a bounded worktree-owner review/handoff worker for `romantic-dewdney` to decide whether to preserve, commit/push, stash, or explicitly approve cleanup. Until then, exclude `.claude/worktrees/romantic-dewdney` from unrelated repo actions.

## Fixture Report Status

Root:
`C:\Projects\FigmaToCode-design-gen`

Paths checked:
- `tests/fixtures/generated/cart-layout-report.json`
- `tests/fixtures/generated/login-layout-report.json`
- `tests/fixtures/generated/mail-layout-report.json`

Status/diff summary evidence:
- `git status --porcelain` reports all three as `M`.
- Per-file `git diff --name-status` is empty for all three.
- Per-file `git diff --numstat` is empty for all three.
- Per-file `git diff --raw` is empty for all three.
- Per-file `git diff --quiet` exit code is `0` for all three.

Content delta verification:
- `cart-layout-report.json`: content_delta=false
- `login-layout-report.json`: content_delta=false
- `mail-layout-report.json`: content_delta=false

Classification:
- commit: no content delta to commit based on inspected diff evidence.
- cleanup: no cleanup indicated.
- index refresh: yes, likely stale stat/index/EOL metadata or equivalent no-content status residue.
- waive: yes after a bounded index refresh/status recheck if no content diff remains.

Exact next bounded action:
Run a bounded index-refresh/status recheck worker for these three fixture paths only. If the `M` statuses clear, no further action. If status remains `M` with no raw/name/numstat diff, waive as no-content fixture residue. If a later probe finds real content delta, reclassify as fixture update requiring review before commit.

## Category-To-Action Matrix

corpus commit candidate:
- Platform wiki-domain corpus: 191 files, 5,177,720 bytes.
- Flows wiki-domain corpus: 2,052 files, 66,914,310 bytes.
- Next bounded action: owner-decision packet for commit vs archive vs cleanup approval.

generated cache:
- No concrete generated-cache files observed in inspected file enumerations.
- The manifests mention `.xiigen/wiki-domain/weights/`, but report `weight_artifact_ref=not_trained_yet`.
- Next bounded action: no cache cleanup; only verify empty/absent weights path if a future owner-decision worker needs it.

archive candidate:
- Both wiki-domain trees, especially flows due size.
- Next bounded action: if owner declines repo commit, archive exact trees to an approved artifact location and then request explicit cleanup approval separately.

cleanup candidate needing approval:
- Both wiki-domain trees only after explicit owner confirmation that they are accidental/reproducible and not needed.
- Nested Figma worktree is not a cleanup candidate for this worker; it requires owner/worktree-worker handoff first.
- Fixture reports are not cleanup candidates because no content delta was found.

hard blocker:
- Wiki-domain: untracked corpus trees block clean repo-hygiene completion until owner decision.
- Figma nested worktree: dirty and ahead branch blocks root cleanup/commit/push decisions until owner/worktree handoff.
- Fixture reports: no hard blocker after no-content diff verification; remaining item is index refresh/waive.

## Recommendations

1. Treat `.xiigen/wiki-domain` as generated corpus/evidence, not disposable cache, unless Luba explicitly approves cleanup.
2. Use a bounded owner-decision worker to choose commit vs archive vs cleanup approval for each wiki-domain tree.
3. Treat `romantic-dewdney` as likely owner/other-model work. Do not clean or overwrite it from unrelated work.
4. For fixture reports, perform an index refresh/status recheck only. Do not commit the three fixture reports unless a later diff shows actual content delta.

must_stop_after_report=true
