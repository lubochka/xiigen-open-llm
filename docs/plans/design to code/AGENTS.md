# AGENTS.md - Design-to-Code Session Live Instructions

## Research scope boundary — direct user rule, 2026-10-16

The RP01 hypothesis-search and document-design assignment is READ_ONLY_RESEARCH plus the requested Markdown reports and study log in `C:\Projects\llm_mvp_core\docs\plans\replanning-possible-diffs`. This is the boundary of that research assignment, not a permanent phase for every later task in this directory.

During that research-only assignment, search the library, evidence and code and describe future experiments; do not launch them or change product/data/weights. “We will check everything experimentally” and a helper card are not execution permission. A new direct task can change the phase: Luba's clarification of2026-11-03 assigns necessary builds, test execution and justified test adaptation to executors of transfers, code QA/audits, acceptance and test repairs. Do not classify them as READ_ONLY_RESEARCH merely from the word "audit" and do not ask again for testing already included in the task. Preserve explicit current search-only restrictions and separate authorization for new scientific experiments. The live workflow is [QA and test repair46](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/46-qa-execution-test-repair-and-fresh-results-2026-11-03.md).

Carry this boundary into every sub-agent order; use only Astra ultra and no more than one active worker for this investigation. Autonomy and deliverable-first do not override the boundary: the requested evidence documents are the deliverable. Keep the study log updated with sources, conclusions, scope corrections, and proposed checks. This rule applies to the parent and workers and takes precedence over historical implementation/startup instructions below.

Cost/role rule from Luba: Astra ultra is reserved for research, architecture, hypothesis analysis, and checking primary evidence in this assignment. It must not implement experiment runners, repair product code, run builds/tests/inference/training, or act as the operational executor. Describe a concrete future work order that a cheaper model can execute, but do not launch it now. The parent orchestrates and reviews; the single Astra ultra sub-agent searches and writes evidence Markdown. Existing context or convenience does not override this budget boundary.

Continuation rule: an intermediate Markdown package or a completed bounded worker search is not completion of this investigation. Continue the next evidence-backed read-only search while requested axes or concrete primary-source leads remain. Do not end the task merely because a report has been written; do not switch into implementation. A narrower packet is an intermediate result, and its open questions stay active.

This file is the live Codex instruction surface for:

`C:\Users\lubao\Documents\xiigen\llm mvp\design to code session`

Canonical repository evidence for this session is under:

`C:\Projects\llm_mvp_core`

Do not copy the huge canonical `AGENTS.md` or `CLAUDE.md` into this folder. Use this file as the local first-screen rule set, then read the canonical files and required skill files when a task touches repo governance, planning, code, QA, or publication.

## Current Authority And Scope

Historical snapshot date below is2026-09-09. It does not select today's task or restrict a later QA assignment. Compute the actual local date and apply the latest unquoted user task.

Historical narrow task: rename/update the active library, plans and reports. It is not an active instruction for every new session. Old `26.08.26WorkLeft...` files and archive images remain historical evidence; unrelated deletion, quarantine or bulk copying is not authorized by a QA assignment.

Historical coordination note for the September session (not current ownership):

- `/root/governance_refresh` owns this local `AGENTS.md` and read-only validation preparation.
- `/root/library_update` owns library/document edits.
- `/root/commit_audit` owns git commit/status reconciliation.

If the newest live instruction conflicts with this file, Luba's current unquoted instruction wins.

## Honesty Guard

Before any status, review, done, ready, or final answer, check and use narrow wording from actual evidence:

```text
claim_scope:
evidence_scope:
count_claimed:
independent_evidence_count:
subagent_id_or_NONE:
downgrade_needed:
```

Rules:

- If you did not run, read, verify, receive, inspect, or edit it, say so.
- A ledger row, checklist row, generated table, readback, or summary is not a cycle/pass/review.
- "N cycles", "N reviews", or "N passes" requires N independently inspectable evidence records of that type.
- Parent/self review is not sub-agent review. A sub-agent claim must name the sub-agent id or `UNKNOWN_IN_SUBAGENT` when the platform hides it, plus the returned packet.
- Tests, builds, runtime probes, browser checks, git actions, and file edits cannot be claimed unless they actually ran in this turn or were directly verified from inspectable evidence.
- If evidence is narrower than the request, downgrade the answer.

Use direct wording when needed:

```text
I did not verify this.
I did not run this.
I did not use a sub-agent for this.
This is a ledger/checklist, not a completed cycle.
The evidence supports only: <narrow claim>.
```

## Deliverable First

Before any status/final, answer:

`is_the_assigned_deliverable_produced_in_a_live_path_this_turn?`

If the answer is no and a safe apply exists, producing or advancing the assigned deliverable is the only valid next action. Diagnosis, audits, status packets, readbacks, hashes, line counts, skill-load ledgers, and governance notes can support the task, but they are not the deliverable unless Luba explicitly ordered that process artifact as the deliverable.

Apply-not-package rule: a governance, skill, config, code, or documentation change counts only when written to the live path that the agent or product actually loads/uses. A supporting-file copy, handoff bundle, prefixed draft, archive, quarantine, or status-about-the-change counts as prepared, not applied.

Delete-test: if deleting the artifact would change nothing the agent auto-loads or the product runs, the deliverable count is zero.

Done/fixed/implemented/ready for a task is invalid unless this turn produced or verified the requested live-path delta. If the task was only read-only inventory, say inventory only.

## Sub-Agent And Worker Boundary

For parent/orchestrator product or implementation work, the standing default from the canonical governance remains: route implementation slices through bounded sub-agents unless Luba explicitly says parent-direct.

A spawned worker is already the executor. A worker must edit the assigned files directly within its bounded work order and must not spawn another worker, invoke `codex.exe`, or search for a "sub-agent inside a sub-agent". `Access is denied`, no nested sub-agent tool, or no callable sub-agent is not a blocker for a worker; the worker should write the file itself or report the exact filesystem error.

Do not invent special N-pass requirements; only count pass/review floors that Luba explicitly required and that have independent evidence records.

## Mandatory Reload List

After every cleanup and after every published code slice, re-read these before the next slice:

- `C:\Projects\llm_mvp_core\AGENTS.md`
- `C:\Projects\llm_mvp_core\CLAUDE.md`
- `C:\Projects\llm_mvp_core\docs\skills\architect-behavior-classifier-SKILL.md`
- `C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md`
- `C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md`
- `C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md`
- `C:\Projects\llm_mvp_core\docs\skills\output-readability-gate-SKILL.md`

Missing any one of the five skill files blocks a "reload complete" claim. Targeted excerpt reading supports only targeted-read claims; do not call it a complete skill load.

When architecture/readability artifacts are the deliverable, also load:

- `C:\Projects\llm_mvp_core\docs\skills\human-readable-architecture-audit-SKILL.md`

## Daily Documentation Rule

At the start of each calendar day or after a published commit boundary, run a fresh read-only status pass before changing planning docs:

1. Read current repository evidence from `C:\Projects\llm_mvp_core`, including relevant git state supplied by the commit-audit owner.
2. Re-read the mandatory reload list above.
3. Re-walk active planning/report files for current status from scratch.
4. Put unfinished/open work first, with exact next actions and blockers.
5. Put completed/published work at the end, with evidence and commit references only when verified by the commit-audit owner or by a same-turn git read.
6. Label old statuses as historical/pre-repair/evidence-only when they remain visible.
7. Do not claim a daily update is done until the live active document path for the actual local calendar date has been written and read back.

If no current-date active document exists, say that plainly. When the assigned slice is active-library rename/update, create, rename, or update the exact active document paths for the current local date. Do not silently treat August-named files as the current daily surface.

## Documentation Follows Code

Code changes that alter behavior, contracts, lifecycle, reports, manifests, runner output, trainable routing, import/export, plugin readiness, or active documentation consumers must update the matching project documentation in the same implementation slice when the doc target is known.

Documentation is not a pause gate. If the needed documentation update is known and authorized, make it and continue. Do not claim done while known documentation drift remains.

## Worktree And Cleanup Hygiene

Do not create new worktrees, sibling clones, temporary repo copies, quarantine directories, or archive copies from this local session unless Luba explicitly orders that exact action in current unquoted text.

Cleanup means physical deletion only for exactly identified, authorized noncanonical worktrees or garbage paths after a resolved absolute-path proof. Never delete `C:\Projects\llm_mvp_core` or this planning cwd. Never delete by glob. Use one PowerShell shell end-to-end with `-LiteralPath` for Windows deletion. If ownership is doubtful, skip that path, report it, and continue only safe confirmed cleanup. A move/rename/copy/quarantine is not cleanup.

The historical library-rename task authorized only its assigned documentation changes. A later QA/repair task uses its own current scope, including the builds/tests/test adaptation described above. Neither task authorizes unrelated source/worktree deletion or project-file renames.

## Read-Only QA Preparation

For link/reference/integrity validation, produce an inventory before any edits:

- Active-document inventory: list current-date active docs and any historical `26.08.26WorkLeft...` files they reference.
- Repository consumer map: in `C:\Projects\llm_mvp_core`, find tracked docs/skills/agent/governance files that reference the active library names or old `26.08.26WorkLeft...` names.
- Markdown link check: verify local relative/absolute paths exist and report broken links with exact file and line.
- NDJSON check: parse each `.ndjson` line as JSON, count bad lines, and verify referenced files/ids where the schema makes that possible.
- Integrity check: compare active status claims against published commit evidence supplied by `/root/commit_audit`; do not claim commit publication from doc text alone.
- Readability/encoding check: changed current docs must be human-readable to Luba, with no mojibake in changed Russian sections.

Historical evidence readback prose is not automatically rewritten. After rename or promotion work is assigned, update only live consumer links and repository plan mirrors that point to active files; leave historical prose alone unless it is part of the current live surface.

## Reporting Shape

User-visible status should start with the human result, not raw machine fields. Use compact evidence. For long plan execution status in Russian, show total, done, current, remaining, result type, accepted evidence, open gaps, next bounded action, and the rule that status/report/commit is not a stop while authorized work remains.

For final answers from this worker, report:

- live paths changed;
- exact readback evidence;
- read/load coverage limits;
- read-only maps produced;
- anything not run or not verified.
