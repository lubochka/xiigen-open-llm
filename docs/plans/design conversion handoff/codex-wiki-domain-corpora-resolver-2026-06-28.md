# WIKI_DOMAIN_CORPORA_RESOLVER - 2026-06-28

claim_scope:
- Bounded wiki-domain corpora resolver packet worker output for Packet Family 5 only.
- Classified safe document-only follow-up versus blocked corpora mutation/import/export/training/DPO actions.
- Created this resolver packet only; no corpora were read directly, copied, imported, exported, deleted, trained on, staged, committed, pushed, archived, or cleaned.

evidence_scope:
- Read `codex-preflight-resolution-sweep-2026-06-28.md`.
- Read `codex-designconversion-handoff-index-2026-06-28.md`.
- Read `codex-designconversion-completion-handoff-2026-06-28.md`.
- Read targeted Codex-authored handoff references for wiki-domain/corpora/training/DPO, including relevant sections of:
  - `codex-wiki-figma-residue-classification-2026-06-28.md`
  - `codex-approval-ready-execution-packets-2026-06-28.md`
  - `codex-selector-resolution-checklist-2026-06-28.md`
- Did not inspect `.xiigen/wiki-domain` corpus contents in the platform or flows repos during this worker run.
- Did not run tests, builds, browser/runtime probes, external AI, training, DPO generation, corpora import/export, git mutation, PR mutation, staging, commit, push, branch/ref mutation, archive, cleanup, or deletion.

count_claimed:
- resolver packets created: 1
- packet families covered by this resolver: 1
- safe doc-only action classes classified: 4
- blocked non-doc action classes classified: 10
- corpora roots cited from prior classifier evidence: 2
- current-turn corpora recounts/imports/exports/training/DPO actions: 0

independent_evidence_count:
- 7 current-turn read-only evidence records: folder listing, preflight sweep read, handoff index read, completion handoff read, targeted Codex-doc search, wiki/Figma classifier section read, approval packet/checklist section reads.

subagent_id_or_NONE:
- WIKI_DOMAIN_CORPORA_RESOLVER

downgrade_needed:
- Yes for any broader claim. The evidence supports only this doc-only resolver packet and cited prior classifier/report facts. It does not prove clean repo state, product completion, training completion, DPO generation, model weights, package readiness, runtime proof, visual fidelity, archive safety, sensitivity safety, or that the corpus trees are current beyond the cited prior classifier/report evidence.

required_literals:
- `WIKI_DOMAIN_CORPORA_RESOLVER`
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- `no training`
- `no DPO generation`
- `no corpora import/export`

## Resolver Summary

Packet Family 5 remains `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` for every non-default action. The conservative/default route is document-only deferral: keep platform and flows `.xiigen/wiki-domain` corpora in place as unresolved owner decisions, with no clean-repo or training-completion claim.

This resolver records `no training`, `no DPO generation`, and `no corpora import/export` as active boundaries. Corpus presence is documentation/corpus evidence only; it is not proof of learned weights, model promotion, or completed training.

## Cited Corpus State From Prior Classifier Evidence

Source: `codex-wiki-figma-residue-classification-2026-06-28.md`.

| Repo | Root | Prior classifier status | Prior classifier counts |
| --- | --- | --- | --- |
| platform | `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain` | untracked generated domain-corpus output; 0 tracked files | 191 files; 5,177,720 bytes; 3,090 JSONL records |
| flows | `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain` | untracked generated domain-corpus output; 0 tracked files | 2,052 files; 66,914,310 bytes; 36,894 JSONL records |
| combined | both roots | prior classifier total only, not re-counted by this worker | 2,243 files; 72,092,030 bytes; 39,984 JSONL records |

Manifest facts cited from prior classifier/report evidence:
- `model_status=documentation_ready_training_pending`
- `weight_artifact_ref=not_trained_yet`
- manifests mention generated documentation/training corpus material, but not trained weight output.

Boundary: this worker did not re-count, hash, import, export, read, archive, clean, delete, stage, commit, push, or train on the corpus trees.

## Safe Doc-Only Actions

Safe doc-only actions are allowed only as bounded documentation updates with explicit write scope:

| Safe action | Allowed output | Forbidden upgrade |
| --- | --- | --- |
| Defer decision | Record that selected corpora remain unresolved owner decisions. | Must not claim clean repo, training completion, model weights, or product readiness. |
| Decision packet | Prepare owner selectors for repo set, action, sensitivity, archive/cleanup, commit, and verification policies. | Must not execute the selected action. |
| Waiver packet draft | Draft exact forbidden claims and counts to preserve if Luba later chooses waiver. | Must not treat waiver as accepted without Luba approval. |
| Handoff/index update | Link this resolver packet and cite its boundaries. | Must not mutate repos, corpora, PRs, branches, or runtime evidence. |

Safe doc-only work must preserve these literals: `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`, `no training`, `no DPO generation`, `no corpora import/export`.

## Blocked Non-Doc Actions

The following actions remain blocked until the exact selectors below are supplied and a future bounded worker receives explicit authority:

| Blocked action class | Current classification | Required blocker literal |
| --- | --- | --- |
| Commit corpora | Blocked; would add generated corpus files to repo history. | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` |
| Stage corpora | Blocked; staging is repo mutation. | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` |
| Push corpora commit | Blocked; publishing requires separate approval. | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` |
| Archive/copy corpora | Blocked; destination and redaction/sensitivity policy missing. | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` |
| Cleanup/delete corpora | Blocked; destructive and may delete useful corpus/evidence. | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` |
| Waive corpora as accepted | Blocked until Luba explicitly accepts waiver terms. | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` |
| Direct corpus content inspection | Blocked except approved samples/metadata in a future scoped worker. | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` |
| Corpora import/export | Blocked; active boundary is `no corpora import/export`. | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` |
| Training / model update / weight promotion | Blocked; active boundary is `no training`. | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` |
| DPO generation or DPO dataset mutation | Blocked; active boundary is `no DPO generation`. | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` |

## Required Decisions And Selectors Before Any Corpora/Training Action

Before any corpora mutation, import/export, training, or DPO action, Luba must supply an exact, internally consistent selector packet. Missing, vague, stale, or contradictory fields keep the work blocked by `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

Required selectors for corpus disposition:
- `packet_family_id`: `5`
- selected repo set: platform, flows, or both
- selected action: `commit`, `archive`, `cleanup`, `waive`, or `defer`
- approval target: exact repo roots and exact outcome
- allowed mutation: exact file paths and operation class
- forbidden mutations: all repo/corpus/runtime/PR/git actions outside the selected action
- sensitivity/provenance policy: whether content inspection is allowed, which paths/samples, and what must not be printed
- verification target: expected file counts, byte counts, git status paths, archive manifest, commit SHA, or waiver report fields
- stop conditions: count drift, path mismatch, sensitivity concern, missing archive destination, missing backup, branch/state drift, or any unapproved file outside selected roots

Additional selectors for archive:
- approved archive destination
- redaction policy
- checksum/hash manifest requirement
- retention/access policy
- whether source cleanup is separately approved after archive

Additional selectors for cleanup/delete:
- explicit destructive-action approval
- backup/archive requirement before delete
- exact roots to remove
- recovery expectation if backup is absent
- post-cleanup status and count verification

Additional selectors for commit/stage/push:
- exact repo set
- exact path list or path roots
- sensitivity/provenance check approval
- commit message
- whether push is allowed, and target remote/branch
- rollback/revert policy

Additional selectors before any training/model/DPO action:
- explicit statement that Luba is authorizing training or DPO work despite this resolver's default `no training` and `no DPO generation` boundary
- exact corpus source roots and version/hash/count snapshot
- exact training or DPO generation command/script entrypoint
- allowed output directory
- checkpoint/weight artifact policy
- data retention and publication policy
- compute/runtime permission
- secret/credential policy, if any
- evaluation/promotion criteria
- rollback/preservation plan

Without those additional selectors, the only allowed current state is: `no training`; `no DPO generation`; `no corpora import/export`.

## Current Default Resolution

selected_action:
- `defer-both-repos`

selected_targets:
- `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain`
- `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain`

mutation_policy:
- no commit
- no stage
- no push
- no archive/copy
- no cleanup/delete
- no waiver acceptance
- no direct content inspection
- no training
- no DPO generation
- no corpora import/export

preservation_policy:
- Leave both corpus roots in place until Luba supplies exact corpus disposition selectors.
- Preserve the evidence boundary that prior reports classify these as generated corpus/evidence, not trained weights.

forbidden_claims:
- clean repository state
- full completion
- product readiness
- training completed
- model weights produced
- model promoted
- DPO generation completed
- corpora safely archived
- corpora safely deleted
- corpora sensitivity reviewed

## Future Bounded Work Orders

1. `bounded-wiki-domain-corpora-decision-packet-worker`
   - Write scope: a new `codex-*` decision packet in the handoff folder only.
   - Task: collect Luba's selected repo set, selected action, archive/cleanup/commit/waive/defer policy, sensitivity/provenance policy, verification evidence names, and stop conditions.
   - Forbidden: repo mutation, corpus content inspection, corpora import/export, training, DPO generation, git/PR mutation.

2. `bounded-wiki-domain-corpora-recount-worker`
   - Allowed only after Luba approves read-only metadata recount.
   - Task: re-count selected corpus roots and compare to prior classifier counts.
   - Forbidden: content printing, archive/copy, cleanup/delete, staging, commit, push, training, DPO generation, corpora import/export.

3. `bounded-wiki-domain-corpora-archive-worker`
   - Allowed only after Luba supplies selected repo set, approved archive destination, redaction policy, and checksum manifest policy.
   - Task: copy selected corpus roots to the approved archive destination and generate verification manifest.
   - Forbidden unless separately approved: source cleanup/delete, commit, push, training, DPO generation.

4. `bounded-wiki-domain-corpora-cleanup-worker`
   - Allowed only after explicit destructive cleanup approval and backup/archive policy.
   - Task: remove only approved selected roots and verify targeted status/counts.
   - Forbidden: deleting any path outside selected roots, training, DPO generation, unrelated repo cleanup.

5. `bounded-wiki-domain-corpora-commit-scope-worker`
   - Allowed only after Luba selects repo set and commit policy.
   - Task: produce exact file list, sensitivity/provenance result, commit message proposal, and staging scope for approval.
   - Forbidden until a separate commit approval: staging, commit, push.

6. `bounded-wiki-domain-corpora-waiver-worker`
   - Allowed only after Luba explicitly chooses waiver.
   - Task: record accepted waiver counts and exact forbidden claims.
   - Forbidden: clean-repo or product-readiness upgrade, repo mutation, training, DPO generation.

7. `bounded-wiki-domain-training-dpo-preflight-worker`
   - Allowed only if Luba explicitly asks for future training or DPO preparation.
   - Task: create a preflight plan naming exact corpus inputs, command/script entrypoints, output policy, evaluation criteria, and safety gates.
   - Forbidden unless a later separate execution order approves it: training execution, DPO generation, checkpoint/weight mutation, corpora import/export.

## Final Boundary

This packet is a resolver document only. It does not execute Packet Family 5. All non-doc corpus disposition paths remain `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` until exact selectors and policies are supplied.

file_created:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-wiki-domain-corpora-resolver-2026-06-28.md`

verification:
- File content should be verified by reading this document and checking for the required literals.
- No tests/builds/browser/runtime/git/PR/external-AI/training/DPO/corpora import/export verification was run or claimed.
