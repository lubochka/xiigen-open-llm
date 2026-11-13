# Pass240 Dirty Worktree Mergeability Evidence

claim_scope: Post-Pass239 no-write dirty-worktree mergeability check for `C:\Projects\llm_mvp_core` on branch `DesignToCode-clean` against `origin/main` and `origin/codex/chat-gpt-5.5`.
evidence_scope: Local git command evidence from this Pass240 session only. Parent-provided full-suite result `Failed 0, Passed 2339, Total 2339` was not rerun here. No ProductReady, cloud, deploy, public endpoint, external-provider, PR, push, or CI claim is made.
count_claimed: 1 dirty snapshot commit object created with a temporary index; 2 no-write merge-tree target checks run.
independent_evidence_count: 2 target merge-tree command results, plus branch/ref/status/temp-index/index-preservation checks.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: No for the scoped no-write mergeability evidence. Yes for any broader product/cloud/deploy/public endpoint/external-provider readiness claim, which is not claimed here.

## Skill Load Status

| Required file | Status | Evidence |
|---|---:|---|
| `C:\Projects\llm_mvp_core\AGENTS.md` | found/read | `Get-Content -LiteralPath 'C:\Projects\llm_mvp_core\AGENTS.md' -Raw`, exit 0 |
| `C:\Projects\llm_mvp_core\docs\skills\verification-before-completion-SKILL.md` | found/read | `Get-Content -LiteralPath 'C:\Projects\llm_mvp_core\docs\skills\verification-before-completion-SKILL.md' -Raw`, exit 0 |
| `C:\Projects\llm_mvp_core\docs\skills\phase-git-report-SKILL.md` | found/read | `Get-Content -LiteralPath 'C:\Projects\llm_mvp_core\docs\skills\phase-git-report-SKILL.md' -Raw`, exit 0 |
| `C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md` | found/read | `Get-Content -LiteralPath 'C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md' -Raw`, exit 0 |

## Preflight Git State

| Field | Value |
|---|---|
| current_branch | `DesignToCode-clean` |
| head | `0d4a879298772dfad76111ad1301cdfc74047a33` |
| origin/main | `ded0a6d8946db888f8398b8477c9c66c60d3e37e` |
| origin/codex/chat-gpt-5.5 | `5084f355e7185e2c2cf7928ae5c972208c226117` |
| cached_status_before | empty output, exit 0 |
| worktree_status_before | dirty; `git status --porcelain=v1 -uall` exit 0; terminal output was large and included modified tracked files plus untracked files |
| fetch_performed | no; both required remote-tracking refs resolved locally |

## Dirty Snapshot

| Field | Value |
|---|---|
| temporary index path | `C:\Projects\llm_mvp_core\.git\codex-pass240-temp-index` |
| temporary lock path | `C:\Projects\llm_mvp_core\.git\codex-pass240-temp-index.lock` |
| dirty_snapshot_tree | `202c4f7d45f86fac86fe60427d7b3712cfc73f62` |
| dirty_snapshot_commit | `2157b0b64e05e9aaeb2186eeedebc3142e0ffffa` |
| snapshot method | `GIT_INDEX_FILE` set only to the temp index for `git read-tree`, `git add -A -- .`, `git write-tree`, and `git commit-tree` |
| real index staging used | no |

The final successful snapshot was created before this evidence artifact existed, because a complete self-referential artifact cannot be included in the commit object that records its own final commit id. This artifact is evidence-only and does not add a product/source/test change.

## Mergeability Results

| Target | Command | Exit | Output | Verdict |
|---|---|---:|---|---|
| `origin/main` | `git merge-tree --write-tree origin/main 2157b0b64e05e9aaeb2186eeedebc3142e0ffffa` | 0 | `4d613707f7a44abc7ce6801db31ce0754cd5924a` | PASS |
| `origin/codex/chat-gpt-5.5` | `git merge-tree --write-tree origin/codex/chat-gpt-5.5 2157b0b64e05e9aaeb2186eeedebc3142e0ffffa` | 0 | `4d613707f7a44abc7ce6801db31ce0754cd5924a` | PASS |

## Real Index Preservation

| Check | Command | Exit | Output | Verdict |
|---|---|---:|---|---|
| cached status before | `git diff --cached --name-status` | 0 | empty | PASS |
| cached status after final merge-tree checks | `git diff --cached --name-status` | 0 | empty | PASS |
| temp index cleanup | exact `codex-pass240` temp index and lock existence checks before/after cleanup | 0 | temp index existed before cleanup; temp lock did not; both absent after cleanup | PASS |

## Final Successful Command Log

| Step | Command | Exit | Key output |
|---|---|---:|---|
| branch | `git branch --show-current` | 0 | `DesignToCode-clean` |
| head | `git rev-parse HEAD` | 0 | `0d4a879298772dfad76111ad1301cdfc74047a33` |
| target ref | `git rev-parse --verify origin/main` | 0 | `ded0a6d8946db888f8398b8477c9c66c60d3e37e` |
| target ref | `git rev-parse --verify origin/codex/chat-gpt-5.5` | 0 | `5084f355e7185e2c2cf7928ae5c972208c226117` |
| cached before | `git diff --cached --name-status` | 0 | empty |
| worktree status | `git status --porcelain=v1 -uall` | 0 | dirty output in terminal |
| temp read tree | `$env:GIT_INDEX_FILE='C:\Projects\llm_mvp_core\.git\codex-pass240-temp-index'; git read-tree HEAD` | 0 | empty |
| temp add all | `$env:GIT_INDEX_FILE='C:\Projects\llm_mvp_core\.git\codex-pass240-temp-index'; git add -A -- .` | 0 | empty |
| temp write tree | `$env:GIT_INDEX_FILE='C:\Projects\llm_mvp_core\.git\codex-pass240-temp-index'; git write-tree` | 0 | `202c4f7d45f86fac86fe60427d7b3712cfc73f62` |
| temp commit tree | `$env:GIT_INDEX_FILE='C:\Projects\llm_mvp_core\.git\codex-pass240-temp-index'; git commit-tree 202c4f7d45f86fac86fe60427d7b3712cfc73f62 -p HEAD -m 'codex-pass240 dirty snapshot after whitespace cleanup'` | 0 | `2157b0b64e05e9aaeb2186eeedebc3142e0ffffa` |
| merge main | `git merge-tree --write-tree origin/main 2157b0b64e05e9aaeb2186eeedebc3142e0ffffa` | 0 | `4d613707f7a44abc7ce6801db31ce0754cd5924a` |
| merge original | `git merge-tree --write-tree origin/codex/chat-gpt-5.5 2157b0b64e05e9aaeb2186eeedebc3142e0ffffa` | 0 | `4d613707f7a44abc7ce6801db31ce0754cd5924a` |
| cached after | `git diff --cached --name-status` | 0 | empty |

## Discarded Attempt Notes

An earlier temp-index attempt reached `git commit-tree` but failed with exit 129 because the PowerShell command string quoted the commit message incorrectly. The exact temp index left by that failed attempt was checked and removed:

| Check | Result |
|---|---|
| `git diff --cached --name-status` after failed attempt | empty, exit 0 |
| temp index before cleanup | true |
| temp lock before cleanup | false |
| temp index after cleanup | false |
| temp lock after cleanup | false |

A second attempt produced snapshot commit `ff0dbb0930e4588a55e8d73bb68c2e4dab4501d1` and both merge checks exited 0, but its transcript was too terse for the final evidence standard. It is not used as the claimed final snapshot.

## Changed Files By This Pass

| Path | Change |
|---|---|
| `docs/evidence/design-conversion/codex-pass240-dirty-mergeability-after-whitespace-cleanup.md` | added Pass240 evidence artifact |

## Verdict

verdict: PASS

`DesignToCode-clean` dirty snapshot commit `2157b0b64e05e9aaeb2186eeedebc3142e0ffffa` is no-write mergeable with both `origin/main` and `origin/codex/chat-gpt-5.5` by `git merge-tree --write-tree`, with both target checks exiting 0. The real cached index was empty before and after the merge-tree checks. No real staging, commit, merge, rebase, checkout, reset, clean, stash, push, PR, deployment, or external provider/API call was performed.

## Remaining Risks

- Remote-tracking refs were not fetched in this pass; evidence uses the locally resolved `origin/main` and `origin/codex/chat-gpt-5.5` refs listed above.
- The parent-provided post-Pass239 full-suite result was not rerun in this pass.
- This is local no-write mergeability evidence only, not ProductReady, cloud, deploy, public endpoint, external-provider, CI, PR, or publication readiness.
