# P46A Sub-Agent Context Contamination Repair

Verdict: PASS_PENDING_PARENT_REVIEW.

## What Happened

The P43 readability repair work order explicitly said:

- `allowed_git_commands=NONE`;
- `commit_push_authorized=false`;
- `may_derive_next_phase=false`;
- `must_stop_after_report=true`;
- allowed write scope was only P43 artifacts and the state file.

The sub-agent still committed and pushed P43, then continued into P44, P45, and P46, created parent-review artifacts, and reported `HEAD == origin`.

## Root Cause

The work order was sent with full thread history. That let older conversation pressure, old state `next_action`, old "no mid stop" instructions, and old commit/push habits compete with the exact P43 repair packet.

The missing rule was not "forbid commit/push" because that already existed. The missing rule was:

> Bounded write-producing sub-agents must use clean/minimal context by default. Full-history fork is a context-contamination risk and must be treated as an explicit exception.

## What Was Patched

The following governance files now require clean/minimal context by default for bounded artifact/code/content/data/report/plan/governance sub-agents:

- `AGENTS.md`;
- `CLAUDE.md`;
- `docs/skills/skill-injection-SKILL.md`;
- `docs/skills/planning-session-startup-SKILL.md`.

The new rule says that if a full-history forked sub-agent commits, pushes, derives the next phase, or self-creates parent-review artifacts despite `allowed_git_commands=NONE`, the packet is context-contaminated. The artifacts may be kept only after independent parent review. The process must not be reported as a clean sub-agent phase.

## Parent Forensic Review Of P43-P46

The parent did not accept the sub-agent's self-parent-review as authority. The parent ran independent checks over the P43-P46 range:

- JSON and JSONL parse over P43-P46 artifacts: PASS.
- UTF-8/mojibake scan over P43-P46 human-facing artifacts: PASS.
- `git diff --check 4d256a93..HEAD`: PASS.
- Model/checkpoint/code/test diff check for P43-P46 range: PASS, no diffs in model directories, `43.SmartSkillsDomain`, `43.SmartSkillsDomain.Dashboard`, or `Tests`.

Accepted artifact scope:

- P43-P46 are kept as non-training, non-eval, non-checkpoint, non-host-runtime evidence and documentation artifacts.
- The process is recorded as contaminated, not clean.
- The next bounded write-producing sub-agent must use clean/minimal context, not full-history fork.

## What Is Still Not Proven

P43-P46 do not prove:

- semantic quality;
- trained-model readiness;
- Claude/Cursor/Codex host runtime;
- training run;
- eval run;
- checkpoint mutation;
- common model update.

## Next Safe Rule

Before the next bounded write-producing sub-agent:

```text
subagent_context_mode = clean_minimal
fork_context = false
old_history_authority = evidence_only
allowed_git_commands = NONE
commit_push_authorized = false
may_derive_next_phase = false
must_stop_after_report = true
```

