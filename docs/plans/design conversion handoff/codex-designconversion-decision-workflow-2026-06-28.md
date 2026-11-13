# DesignConversion Decision-Prep Workflow - 2026-06-28

claim_scope:
- Created a bounded owner decision-prep artifact for remaining DesignConversion owner decisions and hard blockers.
- This artifact asks decisions and defines bounded next work orders. It does not resolve product, repo, PR, branch, cleanup, runtime, or visual-fidelity work.

evidence_scope:
- Read existing handoff/index and classification reports in `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\`.
- Evidence source files read: `codex-designconversion-handoff-index-2026-06-28.md`, `codex-designconversion-completion-handoff-2026-06-28.md`, `codex-core-evidence-dirt-manifest-2026-06-28.md`, `codex-package-artifact-classification-2026-06-28.md`, `codex-figma-fixture-status-waiver-2026-06-28.md`, and `codex-wiki-figma-residue-classification-2026-06-28.md`.
- No GitHub PR mutation, git action, product test, product build, browser check, runtime probe, source edit, or nested agent invocation was performed for this artifact.

count_claimed:
- 10 decision sections prepared.
- 5 PRs covered in Section 1.
- 1 allowed decision artifact created.

independent_evidence_count:
- 1 directory listing command, 6 source file read outputs, 1 target absence check, and the allowed artifact create action.

subagent_id_or_NONE: SELF_AS_BOUND_SUBAGENT.

downgrade_needed:
- Yes for any live-current PR, remote-ref, runtime-readiness, visual-fidelity, or clean-repo claim. The evidence supports only decision-prep based on existing handoff reports.

## Governance Lines

`FINAL_ALLOWED_AFTER_LIVE_CONTINUATION_RECONCILIATION`

`premature final-stop must reopen bounded continuation`

`owner-decision leftovers require actionability classification before final`

`owner decision classification is not final-stop authority`

`external dependency classification is not final-stop authority`

`classification alone cannot close full-completion chain`

`draft PRs require bounded readiness audit before owner-decision final`

`repo dirt requires bounded cleanup-or-waive audit before owner-decision final`

`external visual/runtime gaps require bounded capability audit before owner-decision final`

`FINAL_ALLOWED_AFTER_OWNER_DECISION_HARD_BLOCKER_AUDIT`

`HARD_BLOCKER_REQUIRES_LUBA_DECISION`

`hard blocker is not final-stop authority`

`owner decision list is not final-stop authority`

`hard blockers must become bounded decision-prep workflow`

`FINAL_ALLOWED_AFTER_OWNER_DECISION_HARD_BLOCKER_AUDIT is not enough to stop`

`FINAL_ALLOWED_AFTER_DECISION_WORKFLOW_DELIVERED_OR_EXPLICIT_HOLD`

`decision-prep requires options consequences and next work orders`

`hard blocker is not final-stop authority`

This artifact is decision-prep, not product completion.

## Source Boundary

The handoff docs record five PRs, pushed refs, evidence summaries, residue classifications, and not-proven boundaries. This workflow does not independently re-query PRs or remotes. It turns the remaining owner decisions and hard blockers into exact questions, options, consequences, and next bounded work orders.

## 1. PR State For Five PRs

Exact decision question for Luba:
Should the five recorded DesignConversion PRs remain draft/open, be marked ready for review, be merged, or be closed?

PRs covered from the handoff index:
 - core: `https://github.com/lubochka/llm_mvp_core/pull/6`, branch `DesignToCode-clean`, latest recorded SHA `60dff3549dae44e60873d11182e86544f7884ddc`; historical previous governance SHA only: `739d7058041516e32d274797e5c572ffc7d0c1f1`; earlier historical previous governance SHA only: `9ea4d282c664a4c24949e8ef709150d7c3e82cf3`.
- platform: `https://github.com/lubochka/xiigen-platform-dot-net-angular/pull/1`, branch `DesignToCode`, recorded SHA `bf62e291eca3c366641345781ebda85c7d255f90`.
- flows: `https://github.com/lubochka/xiigen-flows-dot-net-angular/pull/1`, branch `DesignToCode`, recorded SHA `5e6836e7328cd896c686ae083e8014b8b94db09b`.
- figma: `https://github.com/lubochka/FigmaToCode-design-gen/pull/40`, branch `DesignToCode`, recorded SHA `500fcab72e2df9ef7ce8999a63c786e26d24db47`.
- canva: `https://github.com/lubochka/CanvaToCode/pull/1`, branch `DesignToCode`, recorded SHA `2fa6629c9a1e3a70e5385fa4afa8109a8d264add`.

Recommended default if safe:
Leave all five PRs draft/open until Luba separately chooses readiness or merge policy, because remaining visual proof, residue, branch, and package decisions are unresolved.

Options, consequences, and next work orders:

| Option | Risk / consequence | Exact next bounded work order | Can execute without additional permission? |
| --- | --- | --- | --- |
| Leave draft/open | No repo merge happens; work remains visible but incomplete. Old branch heads can drift if not refreshed later. | Bounded PR monitor worker: re-query the five PRs read-only, report draft/open/head/base/state, and update the handoff only if Luba asks for a fresh status artifact. | Yes only as no-op status retention; no PR mutation. |
| Mark ready | Moves PRs into review flow before all owner hard blockers are settled. Reviewers may assume readiness that this packet does not prove. | Bounded PR readiness worker: after explicit Luba approval, verify current heads, confirm blocker decisions are accepted or waived, then mark selected PRs ready and report URLs and SHAs. | No. Requires explicit PR mutation permission. |
| Merge | Integrates branches into base repos and can make unresolved residue, branch naming, package artifacts, or incomplete proof harder to unwind. | Bounded merge worker: after explicit Luba approval per PR, re-fetch current PR state, verify checks and mergeability, merge selected PRs using the approved method, and report merge SHAs. | No. Requires explicit merge permission. |
| Close | Stops the PR path; may abandon useful handoff branches or require reopening/recreating later. | Bounded PR close worker: after explicit Luba approval per PR, preserve final head SHA evidence, close selected PRs, and update owner handoff. | No. Requires explicit PR mutation permission. |

## 2. Core Branch Naming

Exact decision question for Luba:
Should core continue using `DesignToCode-clean`, or should the evidence be moved/renamed so the active branch is exactly `DesignToCode` while preserving Luba evidence branch/history?

Recommended default if safe:
Keep `DesignToCode-clean` as the active core PR branch until Luba explicitly chooses a branch-history operation, because the handoff says local `DesignToCode` preserves Luba-authored evidence and `DesignToCode-with-luba-evidence` preserves `3e47d7a22`.

Options, consequences, and next work orders:

| Option | Risk / consequence | Exact next bounded work order | Can execute without additional permission? |
| --- | --- | --- | --- |
| Keep `DesignToCode-clean` | Branch name differs from the other repos, but evidence history is preserved and no risky ref move is needed. | Bounded handoff-update worker: record that core intentionally uses `DesignToCode-clean` and that no branch rename is authorized. | Yes only as documentation/no-op if Luba accepts this default; no branch mutation. |
| Move/rename to exact `DesignToCode` | Ref move could obscure or overwrite Luba evidence branch/history if done incorrectly. PR #6 may need retargeting or a new PR. | Bounded branch-reconciliation worker: after explicit Luba approval, fetch refs, map `DesignToCode`, `DesignToCode-clean`, and `DesignToCode-with-luba-evidence`, create preservation refs/tags if approved, update/rename branch, and report exact before/after SHAs. | No. Requires explicit branch/ref mutation permission. |
| Preserve both and create a new exact-name PR | Avoids deleting evidence but creates another branch/PR to track. | Bounded branch-copy worker: after explicit Luba approval, create exact `DesignToCode` from approved clean SHA while preserving evidence refs, push, and open/update PR as instructed. | No. Requires explicit branch, push, and PR permission. |
| Defer branch decision | Avoids risky branch operations now but leaves naming mismatch unresolved. | Bounded final-handoff worker: keep mismatch listed as owner decision and block final merge/cleanup claims until Luba decides. | Yes as a no-op documentation state; no repo action. |

## 3. External AI Visual Judge

Exact decision question for Luba:
Does this chain require an external AI visual judge, and if yes, what provider, endpoint, model, secret convention, and run scope should be used?

Recommended default if safe:
Waive external AI visual judging for the handoff-only boundary, but require a separate Luba-approved judge configuration before claiming full visual fidelity or merge-ready visual acceptance.

Options, consequences, and next work orders:

| Option | Risk / consequence | Exact next bounded work order | Can execute without additional permission? |
| --- | --- | --- | --- |
| Configure and run external judge | Requires provider credentials and may upload screenshots/design artifacts to an external service. Results are only as good as prompt/rubric and model choice. | Bounded visual-judge-config worker: ask Luba for provider, endpoint, model, secret environment variable name, data-sharing approval, screenshots/design references, rubric, and pass threshold; then run only after secrets and scope are approved. | No. Requires explicit provider/secret/data-use permission. |
| Use local/offline judge only | Avoids external data transfer but may be weaker or unavailable; may not satisfy intended acceptance gate. | Bounded local-judge-discovery worker: inspect available local tools only after approval, propose an offline rubric/run plan, then await execution approval. | No. Requires tool/runtime discovery permission. |
| Waive external judge | Faster and avoids secrets, but full AI visual-judge evidence remains not proven. | Bounded waiver-record worker: record that external AI visual judge is waived for this handoff and cannot be cited as product readiness or full visual-fidelity proof. | Yes only after Luba explicitly accepts waiver; no external run. |
| Defer judge decision | Keeps blocker open and prevents full visual-fidelity completion claim. | Bounded decision-tracker worker: keep the judge decision as an unresolved hard blocker in the handoff. | Yes as no-op documentation; no product claim. |

## 4. Full Runtime / Design Fidelity Proof

Exact decision question for Luba:
Should full runtime and source-design fidelity proof be required before readiness/merge, or should the saved-output browser proof boundary be accepted for this handoff only?

Recommended default if safe:
Accept saved-output browser proof only for the handoff boundary, not for product readiness; require a separate full runtime/design-fidelity proof before any merge or release claim that depends on it.

Options, consequences, and next work orders:

| Option | Risk / consequence | Exact next bounded work order | Can execute without additional permission? |
| --- | --- | --- | --- |
| Require more proof | More time and tool work, but avoids claiming readiness from saved HTML/CSS output only. | Bounded runtime-visual-proof planner: create exact Path A automated and Path B host/browser scenario with inputs, commands, screenshots, expected UI state, pass/fail thresholds, and evidence filenames; do not execute until approved. | Planning may be safe if authorized; execution requires explicit test/browser/runtime permission. |
| Accept saved-output boundary for handoff | Allows decision workflow to proceed, but cannot support full runtime pipeline, source design fidelity, or external AI judge claims. | Bounded handoff-boundary worker: record acceptance wording that saved-output browser proof is evidence archive only, not full runtime or design-fidelity proof. | Yes only as documentation after Luba accepts this boundary. |
| Waive additional proof entirely | Fastest, but highest risk if PRs are merged or marketed as visually correct without real runtime/design validation. | Bounded waiver worker: record explicit waiver, list claims that remain forbidden, and require Luba quote before any final readiness claim. | No unless Luba explicitly waives. |
| Defer proof decision | Keeps readiness and merge blocked by unresolved proof policy. | Bounded blocker-tracker worker: keep full runtime/design proof as hard blocker and prevent final product completion wording. | Yes as no-op documentation; no proof claim. |

## 5. Core Evidence / Package / Wiki Residue Summary

Exact decision question for Luba:
For broad residue across core evidence, package artifacts, wiki-domain corpora, caches, generated output, and request/response traces, should each category be committed, cleaned, archived, waived, or deferred?

Recommended default if safe:
Defer destructive or publishing actions. Use category-specific manifests first, then act only after Luba selects commit, archive, cleanup, waiver, or defer for each category.

Options, consequences, and next work orders:

| Option | Risk / consequence | Exact next bounded work order | Can execute without additional permission? |
| --- | --- | --- | --- |
| Commit residue | May publish sensitive logs, request/response traces, generated corpora, screenshots, package binaries, or large artifacts. | Bounded commit-scope manifest worker: for selected categories only, inspect provenance/sensitivity as approved, create exact file list, then stage/commit only after explicit Luba commit approval. | No. Requires explicit commit scope and sensitivity policy. |
| Clean residue | May delete evidence or other-model work that is not safely reproducible. | Bounded cleanup-plan worker: prepare exact delete/restore list with reversible backup/archive step, then wait for explicit cleanup approval. | No. Requires explicit destructive-action approval. |
| Archive residue | Preserves evidence without committing to repos, but needs an approved archive location and possible redaction. | Bounded archive worker: after Luba selects archive destination and redaction policy, copy selected artifacts, create archive manifest, verify checksums, and only then propose cleanup if requested. | No. Requires archive destination/policy approval. |
| Waive residue | Fast but leaves dirty/generated state unresolved for repo hygiene; cannot claim clean repos. | Bounded waiver-record worker: document waived categories and forbidden claims, with exact affected paths and counts from existing reports. | Yes only after Luba accepts waiver; no repo mutation. |
| Defer residue | Safest for unknown ownership, but leaves hard blockers open for final completion. | Bounded decision-tracker worker: keep each residue category in owner decision ledger with no final product completion claim. | Yes as no-op documentation; no repo mutation. |

## 6. Figma Nested Worktree `romantic-dewdney`

Exact decision question for Luba:
For `C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney`, should the nested worktree be preserved, committed/pushed, stashed, or explicitly approved for cleanup?

Evidence boundary from handoff docs:
The nested worktree is recorded as dirty and ahead of remote by one commit, with `src/ui/components/App.tsx` modified and `592 insertions, 592 deletions`.

Recommended default if safe:
Preserve and exclude the nested worktree from unrelated cleanup until a bounded worktree-owner handoff decides its fate.

Options, consequences, and next work orders:

| Option | Risk / consequence | Exact next bounded work order | Can execute without additional permission? |
| --- | --- | --- | --- |
| Preserve/exclude | Avoids data loss but leaves parent repo status marker unresolved. | Bounded worktree-handoff worker: record exclusion, current branch/ahead/dirty facts, and owner needed; no changes to nested worktree. | Yes as no-op documentation; no worktree mutation. |
| Commit/push | Preserves work remotely, but may publish unreviewed source changes from another model/owner. | Bounded Figma worktree-owner worker: after explicit Luba approval, inspect diff, identify owner intent, run allowed checks if approved, commit/push or update PR as instructed. | No. Requires worktree owner and git permission. |
| Stash | Reduces dirty status but may hide important work and complicate recovery. | Bounded stash worker: after explicit Luba approval, record current SHA/status, create named stash, verify stash contents, and report recovery command. | No. Requires git mutation permission. |
| Cleanup/reset | Highest risk; can discard owner/other-model work and unpushed commit state. | Bounded cleanup worker: only after explicit Luba cleanup approval and backup/ref preservation, reset/remove exact worktree state and report before/after evidence. | No. Requires explicit destructive-action permission. |

## 7. Figma Fixture Status-Only Residue

Exact decision question for Luba:
For the three fixture reports that remain `M` with empty content diff and empty numstat, should the next action be refresh index, waive, restore, or inspect with the owning model?

Paths from the waiver report:
- `tests/fixtures/generated/cart-layout-report.json`
- `tests/fixtures/generated/login-layout-report.json`
- `tests/fixtures/generated/mail-layout-report.json`

Recommended default if safe:
If clean status is required, refresh/recheck the index for exactly these three paths. If clean status is not required for this handoff, waive as status-only/index/metadata residue.

Options, consequences, and next work orders:

| Option | Risk / consequence | Exact next bounded work order | Can execute without additional permission? |
| --- | --- | --- | --- |
| Refresh index | May clear stale metadata status without content change, but still mutates git index. | Bounded fixture-index worker: after explicit Luba approval, run scoped index refresh/status recheck for exactly the three paths and report whether `M` clears. | No. Requires git index mutation permission. |
| Waive | Leaves status marker visible but records no content delta from the checked evidence. | Bounded waiver-record worker: record waived status-only residue and forbid fixture correctness or clean-repo claims from this waiver. | Yes only after Luba accepts waiver. |
| Restore | Produces cleaner status, but may touch working-tree metadata/content if Git considers files modified. | Bounded fixture-restore worker: after explicit Luba approval, restore exactly the three paths, then verify diff/status. | No. Requires working-tree mutation permission. |
| Inspect with owning model | More accurate ownership diagnosis, but takes another bounded worker and may still require later action. | Bounded Figma fixture-owner worker: inspect why Git status reports `M` with empty diff/numstat and recommend refresh/waive/restore. | No unless Luba authorizes the owning-model inspection. |

## 8. Package Artifact Decisions

Exact decision question for Luba:
For the 8 package artifacts classified in platform and flows, should they be restored, archived, committed after provenance verification, cleaned up, or deferred?

Evidence boundary from package classification:
Six platform `.nupkg` artifacts are tracked modified cleanup candidates requiring approval. Two flows FLOW-47 `.nupkg` artifacts are untracked evidence archive candidates. No package source provenance review, build, or test was performed by the classifier.

Recommended default if safe:
Defer mutation and run a provenance/scope worker before any commit, restore, cleanup, or archive operation.

Options, consequences, and next work orders:

| Option | Risk / consequence | Exact next bounded work order | Can execute without additional permission? |
| --- | --- | --- | --- |
| Restore platform packages | Removes tracked package modifications but may discard intended deliverables. | Bounded package-restore worker: after explicit Luba approval, restore exactly the six platform package paths and verify git status/diff. | No. Requires working-tree mutation permission. |
| Archive package artifacts | Preserves binaries outside repo history, but needs destination and checksum/provenance policy. | Bounded package-archive worker: after Luba approves destination, copy selected `.nupkg` files, record SHA256/metadata, and report archive manifest. | No. Requires archive destination/policy approval. |
| Commit after provenance verification | Keeps binaries in repo history, but risks publishing stale or unreviewed build outputs if provenance fails. | Bounded package-provenance worker: verify binaries correspond to accepted source/build outputs, then request explicit commit approval with exact file list. | No. Requires provenance plus commit permission. |
| Cleanup untracked flows packages | Reduces residue but may delete FLOW-47 evidence artifacts. | Bounded package-cleanup worker: after explicit cleanup approval, back up if required, delete exact selected untracked package paths, and verify status. | No. Requires destructive-action permission. |
| Defer | Avoids accidental loss/publish but leaves repo hygiene blocker open. | Bounded decision-tracker worker: keep package artifacts listed as owner decision and forbid clean-repo claim. | Yes as no-op documentation; no mutation. |

## 9. Wiki-Domain Generated Corpus Residue

Exact decision question for Luba:
For `.xiigen/wiki-domain` generated corpora in platform and flows, should the corpora be committed, archived, cleaned, waived, or deferred?

Evidence boundary from wiki/Figma classification:
Platform wiki-domain has 191 files and 5,177,720 bytes. Flows wiki-domain has 2,052 files and 66,914,310 bytes. Both are untracked generated domain-corpus outputs with training pending and no trained weights reported.

Recommended default if safe:
Archive or defer before cleanup. Do not delete or commit these generated corpora without explicit owner policy, because they may be evidence/training material and flows is large.

Options, consequences, and next work orders:

| Option | Risk / consequence | Exact next bounded work order | Can execute without additional permission? |
| --- | --- | --- | --- |
| Commit corpora | Preserves generated documentation/training corpus in repo history, but adds thousands of files and large data volume. | Bounded wiki-commit-scope worker: after Luba approval, inspect file list/sensitivity, propose exact commit scope per repo, then stage/commit only with explicit commit approval. | No. Requires commit and content policy permission. |
| Archive corpora | Preserves material outside repo history; needs archive destination and checksum manifest. | Bounded wiki-archive worker: after Luba approves destination, copy both wiki-domain trees, generate manifest/checksums, and report exact archive path. | No. Requires archive destination approval. |
| Cleanup corpora | Removes repo dirt but may delete useful training/evidence corpus. | Bounded wiki-cleanup worker: only after explicit cleanup approval and optional archive backup, delete exact wiki-domain roots and verify status. | No. Requires destructive-action permission. |
| Waive corpora | Leaves untracked corpora in place; clean-repo and full-completion claims remain forbidden. | Bounded wiki-waiver worker: document waiver and exact forbidden claims, preserving file/byte counts from classifier. | Yes only after Luba accepts waiver. |
| Defer | Avoids wrong action now but remains hard blocker for repo hygiene. | Bounded decision-tracker worker: keep wiki-domain corpora as unresolved owner decision. | Yes as no-op documentation; no mutation. |

## 10. Root `xiigen-codex-*.json` Trace / Request / Response Files

Exact decision question for Luba:
For root core files `xiigen-codex-request.json`, `xiigen-codex-response.json`, and `xiigen-codex-trace.json`, should they be committed, archived outside repo, cleaned, waived, or deferred after sensitivity review?

Evidence boundary from core dirt manifest:
The files are classified as evidence archive candidates but hard blockers before commit because request/response/trace content was not inspected for sensitivity.

Recommended default if safe:
Do not commit. Run a bounded sensitivity/provenance inspection first, or archive outside repo only after Luba approves destination and redaction policy.

Options, consequences, and next work orders:

| Option | Risk / consequence | Exact next bounded work order | Can execute without additional permission? |
| --- | --- | --- | --- |
| Commit | Could publish prompts, traces, credentials, internal paths, or sensitive data if unreviewed. | Bounded trace-sensitivity worker: after Luba approval, inspect exactly the three JSON files for secrets/sensitivity, redact or reject, then request explicit commit approval. | No. Requires sensitivity inspection and commit permission. |
| Archive outside repo | Preserves evidence while avoiding repo history, but archive must be secured and possibly redacted. | Bounded trace-archive worker: after Luba approves destination/redaction policy, archive exact files with checksums and report location. | No. Requires archive policy permission. |
| Clean/delete | Removes risk from workspace but may destroy evidence. | Bounded trace-cleanup worker: after explicit cleanup approval and optional archive, delete exact files and verify status. | No. Requires destructive-action permission. |
| Waive | Leaves files in workspace and blocks clean-repo claim; sensitivity remains unresolved. | Bounded trace-waiver worker: record waiver and forbidden claims. | Yes only after Luba accepts waiver. |
| Defer | Safest absent policy, but keeps hard blocker open. | Bounded decision-tracker worker: keep trace files as unresolved owner decision and do not claim final completion. | Yes as no-op documentation; no mutation. |

## Decision Execution Rule

No repo-mutating, PR-mutating, branch-mutating, cleanup, archive, test, build, browser, runtime, or external-provider action may execute from this artifact alone. Each selected option becomes a bounded work order with exact scope, allowed reads/writes, verification, and git boundary. A no-op documentation state can be recorded only when Luba explicitly accepts the relevant default or waiver.

## Section Coverage Check

Covered required topics:
- PR state for five PRs: Section 1.
- Branch naming: Section 2.
- External AI visual judge: Section 3.
- Full runtime/design fidelity proof: Section 4.
- Core evidence/package/wiki residue: Section 5.
- Figma nested worktree `romantic-dewdney`: Section 6.
- Figma fixture status-only residue: Section 7.
- Package artifact decisions: Section 8.
- Other remaining hard blockers from handoff docs: Sections 9 and 10.

must_stop_after_report=true
