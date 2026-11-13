# Client Adaptive Learning Loop Skill

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

## Purpose

Use this skill for any plan, work order, review, or implementation that says a
user-side model learns from user prompts, host AI answers, result gaps, or user
corrections.

The plan must describe the full local adaptive learning loop. "The user model
learns" is not enough.

## Required Algorithm

```text
capture:
  source = host prompt | AI answer | user correction | missing result
  redaction = apply configured privacy policy
  provenance = host, method, timestamp, model/domain, consent setting

diagnose_gap:
  choose domain = PromptToTask | OrchestrationReasoning | SkillSelection |
                  ResultGap | ArchitectBehavior
  identify expected behavior and observed behavior
  mark whether common model, user model, or both failed

draft_dpo_candidate:
  load domain-specific DPO guides
  create chosen/rejected pair
  annotate rejected side with domain error code
  include source_row_id, approved_pair_ref, chosen_ref, rejected_ref

review_train_ready:
  reject generic refusal negatives
  reject weak margin
  reject missing provenance
  reject wrong domain taxonomy

train_adaptive:
  update only user-adaptive checkpoint
  preserve common model and common sets
  emit before/after behavior probe

verify_and_persist:
  run before/after scenario
  run fresh-load probe
  write manifest/checkpoint pointer
  update backup/share/import bundle
```

## Gates

- No hidden learning: background learning requires explicit settings and
  redaction policy.
- No common overwrite: user-adaptive training never mutates common model assets.
- No DPO without guides: each domain loads its DPO strategy and rejected
  taxonomy.
- No "learned" claim without before/after, fresh-load, and changed-decision
  evidence.
- Full Extension Definition Gate: a Claude, Cursor, or Codex extension cannot
  claim a local adaptive learning path unless it proves capture, gap diagnosis,
  DPO candidate drafting, train-ready review, adaptive checkpoint update,
  fresh-load, export/import/backup/share, and continue-training from the
  existing checkpoint through that host or an explicitly scoped handler path.

## Per-Domain DPO Expectations

| Domain | Chosen | Rejected |
| --- | --- | --- |
| PromptToTask | correct structured task from prompt | weak/missing task translation |
| OrchestrationReasoning | correct orchestration plan | wrong stage/order/scope/verification |
| SkillSelection | correct skill bundle | irrelevant/missing skills |
| ResultGap | correct missing-pattern diagnosis | missed or false gap |
| ArchitectBehavior | correct mode/first action | executor drift, defense, freeze |
