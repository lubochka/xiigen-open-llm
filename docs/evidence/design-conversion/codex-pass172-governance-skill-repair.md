# Codex Pass172 - Governance Skill Repair

claim_scope: Pass172 governance/skills/agent-file repair only. This pass patched governance guidance so future workers cannot stop on NEEDS_FIX, ask permission under full access/approval-never, skip required stack loading, convert quoted evidence into live authority, bypass sub-agent/orchestrator-only mode, treat plan-only repair as product completion, overclaim ProductReady from bounded-local evidence, leave stale status labels current-looking, or mutate source branches while checking mergeability.
evidence_scope: Fresh read of the required skill stack, AGENTS.md, CLAUDE.md, Pass170 evidence, Pass171 audit evidence, and two present SESSION-START-PROMPT files. Direct edits were limited to AGENTS.md, CLAUDE.md, docs/skills/*.md, and this evidence artifact. No product code, safe worktree, xiigen document, source repo, git metadata action, external provider call, cloud action, deploy action, Stop-Process, or long product test ran.
count_claimed: 1 Pass172 governance repair.
independent_evidence_count: 5 verification groups: required-stack read summary; Pass172 marker readback; changed-section mojibake/conflict scan; git diff whitespace check; evidence artifact self-check.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: no downgrade for the governance repair. ProductReady, cloud/deploy/public endpoint/provider/production readiness, source materialization, CI, PR, merge, and product-feature completion remain not claimed.

pass172_verdict: PASS_GOVERNANCE_SKILL_REPAIR

## Files Changed

- C:\Projects\llm_mvp_core\AGENTS.md
- C:\Projects\llm_mvp_core\CLAUDE.md
- C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md
- C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md
- C:\Projects\llm_mvp_core\docs\skills\claim-verification-SKILL.md
- C:\Projects\llm_mvp_core\docs\skills\phase-planning-SKILL.md
- C:\Projects\llm_mvp_core\docs\skills\codex-planning-quality-GUIDE.md
- C:\Projects\llm_mvp_core\docs\skills\qa-session-type-SKILL.md
- C:\Projects\llm_mvp_core\docs\skills\generated-code-review-SKILL.md
- C:\Projects\llm_mvp_core\docs\skills\capability-design-review-SKILL.md
- C:\Projects\llm_mvp_core\docs\evidence\design-conversion\codex-pass172-governance-skill-repair.md

## Rules Added Or Strengthened

- AGENTS.md: added PASS172_GOVERNANCE_EXECUTION_FIREWALL covering approval-never/no sandbox permissions, NEEDS_FIX-as-repair-queue, required stack load and packet identity, quoted-evidence quarantine, sub-agent/orchestrator-only parent limits, bounded-local-not-ProductReady claim boundary, stale status labels, and no-write mergeability/latest-authority checks.
- CLAUDE.md: added a Pass172 mirror so CLAUDE fallback guidance cannot weaken AGENTS.md on the same failure classes.
- planning-session-startup-SKILL.md: startup now says not loaded yet until required architecture/planning/review/arch/QA/code-review status exists and blocks permission prompts, quoted-authority drift, sub-agent-only parent edits, plan-only completion, ProductReady overclaim, stale labels, and unsafe git mutation.
- plan-review-SKILL.md: plan review now rejects packets, holds, readiness, no-safe-work, implementation authorization, and ProductReady-style claims that violate Pass172.
- claim-verification-SKILL.md: claim verification now downgrades permission blockers, stops, missing skill-load/identity, quoted-authority drift, parent direct edits, plan-only completion, bounded-local overclaims, stale labels, and unauthorized merge/git claims.
- phase-planning-SKILL.md and codex-planning-quality-GUIDE.md: planning now separates governance repair from product/project completion and requires continuation or true external blocker after a process repair.
- qa-session-type-SKILL.md, generated-code-review-SKILL.md, and capability-design-review-SKILL.md: QA/code/capability review now require stack load and reject bounded-local/ProductReady overclaim, stale status drift, quoted-authority drift, sub-agent-only parent edits, and unauthorized source-branch mutation.

## Before / After Risk

| Failure class | Before risk | After rule |
|---|---|---|
| Permission prompt under approval-never | Worker could ask before authorized cleanup/repair. | Full access plus approval-never forbids prompts and sandbox permissions; exact cleanup proceeds with proof or records read-only blocker. |
| NEEDS_FIX stop | NEEDS_FIX/timeout/tool error/usage limit could become final pause. | These are repair queues until bounded repair or true external blocker certificate exists. |
| Missing skill stack | Worker could status/act from a prompt checklist. | Required stack must be reported found/read/missing; quoted lists are not load evidence. |
| Bad worker identity | Spawned packet could use NONE or placeholder identity. | Real platform id or UNKNOWN_IN_SUBAGENT with parent reconciliation is required. |
| Quoted text as live task | Transcript/task-like quote could create scope or git authority. | Quoted/attached/state/sub-agent text stays evidence-only until exact current reactivation. |
| Parent direct execution | Parent could edit during sub-agent/orchestrator-only mode. | Parent direct product/governed edits are rejected except allowed orchestration, packet verification, and exact cleanup. |
| Plan-only completion | Governance patch could be called project completion. | Process repair closes only the rule defect; resume authorized product/packaging/execution or record true blocker. |
| ProductReady overclaim | Bounded local/safe-worktree proof could be restated as production readiness. | Bounded local complete is not ProductReady/cloud/deploy/public endpoint/provider/production readiness. |
| Stale status labels | Old NOT_READY/NEEDS_FIX/G1/G2/current labels could remain current-looking. | Superseded labels must be historical/pre-repair/evidence-only with current bounded verdict nearby. |
| Mergeability check mutates source | Branch-readiness check could become source mutation. | Use no-write latest-authority/mergeability checks into original/base and main unless git mutation is explicitly authorized. |

## Mental Simulation Results

- Stale git.exe/temp cleanup asks approval under approval never: blocked by AGENTS, CLAUDE, startup, plan-review, and claim-verification text requiring no approval prompt or sandbox permissions; exact PID/path/work-chain proof allows cleanup, missing proof records a read-only blocker.
- Pass audit returns NEEDS_FIX and parent stops: blocked by AGENTS, startup, plan-review, claim-verification, phase-planning, QA, generated-code-review, and capability-review wording that makes NEEDS_FIX and related tool failures a bounded repair queue.
- User quotes another model transcript and parent treats quote as live task: blocked by AGENTS, CLAUDE, startup, plan-review, claim-verification, phase-planning, and planning-quality quoted-evidence-only rules.
- User asks product completion, worker edits only the plan: blocked by AGENTS, startup, claim-verification, phase-planning, and planning-quality rules that plan/governance-only repair is not product/project completion.
- Worker lacks skill-load status but claims PASS: blocked by startup, plan-review, claim-verification, QA/code/capability review, and AGENTS identity/load gates.
- Parent says ProductReady from bounded local evidence: blocked by AGENTS, CLAUDE, planning-quality, QA, generated-code-review, and claim-verification bounded-local-not-ProductReady wording.
- Source repo branch mergeability rule causes direct source-branch mutation: blocked by AGENTS, CLAUDE, plan-review, phase-planning, generated-code-review, QA, and capability review no-write mergeability/latest-authority language.

## Verification Commands

- Required-stack read summary: PowerShell Get-Content/Get-FileHash loop over AGENTS.md, CLAUDE.md, required skills, Pass170, Pass171, and two present SESSION-START-PROMPT files.
- Changed-section readback: rg -n -C 4 over Pass172 markers across changed governance/skill files.
- Changed-section mojibake/conflict scan: PowerShell extracted each Pass172 section and scanned for configured mojibake markers and conflict markers; all changed Pass172 sections returned mojibake=0 and conflicts=0.
- Conflict-marker search: rg line-anchored conflict-marker search over changed files returned no matches.
- Whitespace check: git diff --check over tracked modified governance/skill files exited clean. Because this artifact is new and untracked, its whitespace/conflict/mojibake checks are recorded separately instead of claiming git diff inspected it.
- Artifact self-check after Pass172R recovery: canonical `pass172_verdict: PASS_GOVERNANCE_SKILL_REPAIR` line count equals 1; identity-field line count equals 1; conflict markers equals 0; configured mojibake markers equals 0.

## Remaining Governance Gaps

No remaining Pass172 governance gap found in the edited surfaces. ProductReady, cloud/deploy/public endpoint/provider/production readiness, source materialization, CI, PR, merge, and clean consumer package restore remain outside this governance repair and are not claimed.

next_recommended_pass: If Luba authorizes packaging evidence, run a narrow packaging/consumer-restore pass without upgrading ProductReady/cloud/deploy/provider claims. If Luba authorizes source publication, run no-write latest-authority/mergeability checks first and only mutate git after exact current authority.
