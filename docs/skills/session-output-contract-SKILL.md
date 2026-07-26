# Session Output Contract

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or the same in the operator's language, e.g. "почему"), challenges an answer, or says a failure came
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

## Core Problem

A session that starts without defining what "done" means will either over-deliver
(produces things not requested, consuming time) or under-deliver (misses implied
deliverables, causing rework). Both failure modes recur across sessions. The output
contract is the zeroth deliverable — it must exist before any code is written or any
file is read.

---

## When to Invoke

- Before any session starts — produce the contract before reading any files
- Before writing any phase plan (to scope what the plan must contain)
- Before claiming a session complete (verify against the contract)
- When a session begins with "write me a plan for..." or "implement phase N"

---

## Section 1 — What an Output Contract Is

A machine-verifiable specification of exactly what "done" means for this session.

Three parts:

### Part 1 — Exact File List

```markdown
## Output Contract — [Session Name]

### Deliverables
| # | File | Must contain |
|---|---|---|
| 1 | docs/skills/cycle-budget-prediction-SKILL.md | Budget card format, MAE recalibration section, integration references |
| 2 | docs/HOW-TO-USE-SKILLS.md | New activation trigger + skill index row for above |
| 3 | docs/DECISIONS-LOCKED.md | D-NN entry for any new locked decision |
```

No ambiguity. No "and any other relevant files." Every deliverable is named,
or the session is not scoped correctly.

### Part 2 — Verification Commands

For each deliverable, one verification command that produces a binary result:

```bash
# Skill file exists and has the required section:
grep -n "MAE recalibration" docs/skills/cycle-budget-prediction-SKILL.md
# Expected: 1 hit

# HOW-TO-USE-SKILLS.md updated with new skill:
grep -n "cycle-budget-prediction" docs/HOW-TO-USE-SKILLS.md
# Expected: 2 hits (activation trigger + skill index row)

# Test baseline preserved:
dotnet test --list-tests 2>/dev/null | grep -c "Test"
# Expected: baseline count or higher
```

### Part 3 — Specification-Level Question

After verification commands pass, answer this before claiming done:

> "Does this deliverable match what was originally requested?"

Not: "Are the file internals consistent?"
Not: "Does the content make sense?"
The question is about the original request. Re-read the request. Does the deliverable satisfy it exactly?

---

## Section 2 — Contract Format

```markdown
## Output Contract — [Session Name]
Requested by: [reference to user instruction]
Session type: IMPLEMENTATION | PLANNING | SKILL_TRANSLATION | REVIEW

### Deliverables
| # | File | Verification command | Expected output |
|---|---|---|---|
| 1 | [path] | [bash command] | [expected result] |
| 2 | [path] | [bash command] | [expected result] |

### Specification gate (run last, before claiming done)
- [ ] Re-read the original request
- [ ] Every named deliverable in the request exists
- [ ] No deliverable was silently merged into another file
- [ ] Files updated from memory of prior content were verified against the original source
- [ ] Every count claim is backed by matching independent evidence records
- [ ] Every sub-agent claim names the sub-agent id/nickname and returned packet
- [ ] `SILENT_SUBAGENT_NO_PACKET_RECOVERY_GATE` passed: a wait timeout,
      heartbeat nonresponse, running/shutdown/interrupted/not_found notice, or
      no-output wait is not completion, not a blocker, and not accepted work.
      If no packet arrives after a heartbeat/status request and bounded wait,
      classify `LOST_WORKER_UNACCEPTED`, set `count_claimed=0` and
      `independent_evidence_count=0`, close/interrupt if safe, and route to a
      smaller recovery work order or allowed orchestration/cleanup.
- [ ] Running/shutdown/interrupted/not_found without returned packet and without
      parent-visible allowed-file diff is zero accepted work; do not ask Luba to
      debug agents or present the silent/lost worker as a product blocker.
- [ ] Work orders request early micro-packets after initial reads, first
      compile/test/runtime error, filesystem denial, or scope uncertainty, and
      require human-readable `NEEDS_FIX` instead of silent continuation.
- [ ] Ledger/checklist rows are not described as cycles
- [ ] Spawned bounded sub-agent is already the sub-agent executor when a work
      order gives allowed write scope; the sub-agent directly creates or edits
      scoped artifacts and reports evidence instead of invoking nested agents.
- [ ] `codex.exe Access is denied is not a blocker` for a spawned sub-agent
      artifact task; if writing scoped files fails, the packet names the exact
      path, attempted operation, and filesystem error.
- [ ] Parent direct execution remains forbidden, and sub-agent execution is not
      parent direct execution.
- [ ] Verification literals: `Spawned bounded sub-agent is already the sub-agent executor`; `codex.exe Access is denied is not a blocker`; `Parent direct execution remains forbidden`; `sub-agent execution is not parent direct execution`.
- [ ] Every execution report/status board row is in the operator's language (plain language; Russian for the original author) and includes:
      `Phase X of Y`, `phase meaning`, `Step X.Y`, `step meaning`,
      and exact status literals in the operator's language. Example literals
      for a Russian-speaking operator: `Фаза N из M: <человеческое название>. Смысл фазы: <что эта фаза решает для проекта/пользователя>.`
      and `Шаг N.K из N.T: <человеческое название>. Смысл шага: <что этот шаг доказывает/чинит>.`;
      actual reports replace N, M, N.K, and N.T with concrete phase/step
      numbers,
      `Status: DONE | IN_PROGRESS | NEEDS_FIX | REVIEWING | BLOCKED_WITH_REASON`,
      a plain-language status explanation in the operator's language, total project position, `Done`,
      `In progress`, numbered `Remaining`, optional `Review found`,
      `What was fixed`, `What remains`, `Next action`, and whether
      commit/continuation is allowed. Opaque codes may appear only after the
      human explanation, never instead of it. Bare `QSG-28`,
      `07.06/07.07 NEEDS_FIX`, `packet accepted`, `boundary pass`, JSON, or
      STATE labels fail the contract unless translated with meaning and counts.
- [ ] Machine-only statuses are invalid: the packet includes phase/step
      meaning, counts, done/in-progress/remaining, review findings, fixes, and
      next action before any machine label, path, packet id, JSON, STATE, or
      validator code.
- [ ] `STATUS_FORMAT_HUMAN_GATE_ALWAYS_REQUIRED` passed: every visible progress/
      status/review/final/worker packet uses the canonical operator-language human-gate
      block before machine evidence, or is rejected as
      `STATUS_FORMAT_HUMAN_GATE_FAILED`. A returned sub-agent packet is
      translated by the parent before being shown to Luba as status.
- [ ] `HUMAN_BOARD_BEFORE_CLAIM_SCOPE_SUPPORT_BLOCK` passed: the
      `claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed`
      block is mandatory evidence support, but it appears only after the human
      board or remains internal. Raw `claim_scope`, JSON/STATE/path/packet id,
      ALL_CAPS verdict, compact machine table, or raw sub-agent excerpt before
      the human board is `STATUS_FORMAT_HUMAN_GATE_FAILED`.
- [ ] `ROUTINE_PROGRESS_COMPACT_EVIDENCE_GATE` passed: ordinary commentary
      heartbeat/progress starts with a short human paragraph or compact board,
      may omit the full claim/evidence schema, and uses at most one short
      `Evidence:` sentence or two short support lines when evidence is visible.
      Full claim/evidence schema is reserved for worker return packets,
      acceptance/rejection reviews, final summaries, and direct audit/evidence
      answers.
- [ ] Parent translated worker machine fields before Luba-facing status. A raw
      worker `claim_scope/evidence_scope` block is not the main status, and a
      formal phase/status opening plus long raw evidence dump is still
      `STATUS_FORMAT_HUMAN_GATE_FAILED`.
- [ ] `HYBRID_STATUS_PACKET_DUMP_FORBIDDEN` passed: no routine Luba-facing
      status uses a formal human board plus long raw `claim_scope`,
      `evidence_scope`, `count_claimed`, `independent_evidence_count`,
      `subagent_id_or_NONE`, `downgrade_needed`, raw worker packet, or machine
      inventory as the main body. Parent summarized worker packet fields in
      human language first; routine support is capped at one short `Evidence:`
      sentence or 1-2 compact lines. Full schemas appear only in worker return
      packets, acceptance/rejection reviews, final summaries, or explicit
      audit/evidence answers.
- [ ] formal human opening plus raw claim/evidence/inventory/subagent packet
      dump is `STATUS_FORMAT_HUMAN_GATE_FAILED` when the raw packet is the main
      visible content or longer than 1-2 compact evidence lines.
- [ ] If Luba challenged readability with an operator-language phrase like `не читабельно`, `нечитабельно`,
      `format lost`, or equivalent, the response answered first, classified the
      prior output as `STATUS_FORMAT_HUMAN_GATE_FAILED` or
      `status_format_governance_gap`, patched or verified the governing rule,
      and resumed the nearest authorized product work after verification.
- [ ] Plan-readiness continuation is explicit: do not stop is not executor
      authorization; plan-not-ready returns to planning repair, not execution;
      a premature executor must be stopped and replaced by bounded governance
      repair; and the Luba-facing packet shows human-readable phase counts
      before execution continuation: total phases/steps, done, in-progress,
      remaining, and plain-language phase/step meaning before QSG/row/state/
      machine labels.
- [ ] Exact session literals are present when this checklist is used for
      propagation verification: `schema != training`; `current-state invariants + evidence packet`.
- [ ] Review did not overclaim: boundary/API evidence != host readiness;
      marker-only != full local desktop pass; trace != savings; schema !=
      training; checkpoint candidate != promotion. Broader claims have
      `claim=false` unless exact evidence proves them.
- [ ] For untracked new artifacts, review may use current-state invariants +
      evidence packet instead of an impossible git baseline only when exact
      paths, explicit counts/claim=false, and no readiness claim are recorded.
- [ ] `NON_HEAVY_MERGEABILITY_NOT_FULL_PROOF` and
      `GENERATED_HEAVY_OUTPUT_BOUNDARY` passed: status labels proof that excludes
      `TestResults/**`, `artifacts/**`, `bin/**`, or `obj/**` as non-heavy/
      scoped proof, records excluded counts and policy, and does not imply full
      dirty mergeability or copy generated-heavy output unless the current task
      explicitly selected exact curated files.
- [ ] No stop gates from protocol repair: after bounded protocol/governance/
      skill repair review, the next bounded phase remains the next action
      unless GOLD GATE, missing authority, unsafe action, or a real blocker
      remains.
- [ ] Downgrade, blocker, not proven, not ready, needs verification, no claim,
      or status-only artifact is not completion. It creates a next concrete work
      item unless all product requirements are already proven or Luba explicitly
      stops.
- [ ] Product-readiness progress uses scenario counters, not phase artifacts:
      total scenarios, fully joined proven, partial, not moved, blocked with
      next action, and exact scenario moved.
- [ ] Every scenario attempts dual-path proof where possible: automated/core/API
      path plus local real-program or closest honest local/test-only adapter
      path. If direct host UI is impossible, the next action is a feasible
      alternate proof path, not terminal stop.
- [ ] Completion output separates `evidence_accounting_done` from
      `product_requirement_proven`.
- [ ] Operator-language status wording includes examples like (for a Russian-speaking operator):
      `Сценарии: всего 12; полностью доказано 3; частично доказано 5; не сдвинуто 2; заблокировано с понятным следующим действием 2.`
      and
      `Сдвинулся сценарий Codex prompt-to-task: automated boundary доказан, local proof частичный; продуктовая готовность ещё не доказана.`
- [ ] Parent Architect Subagent-Only Firewall phrases are present in every
      scoped governance/skill file: `parent local sidecar work ban`,
      `read-only verification after a sub-agent packet returns`,
      `sub-agent waiting is not a stop`, and
      `no local non-overlapping work escape`.
- [ ] Repeated self-block drift is recorded as governance drift and repaired or
      avoided in later work orders; nested sub-agent/codex CLI searching after
      scoped write authority is not treated as a blocker.
- [ ] dotnet test baseline ≥ pre-session baseline (zero regression)

### What this session does NOT produce
[Explicit list of in-scope-but-not-this-session items]
```

The "does NOT produce" section is as important as the deliverables list. It prevents
scope creep ("while I'm here I'll also fix...") and prevents the next session from
assuming the current session already did something it didn't.

For extension/runtime/product readiness sessions, the contract must include the
Dual-Path Host Verification Rule.
Deliverables and verification commands must separately name: Automated boundary
path evidence for adapter/API/core request-response, model logic, DPO/training,
token measurement, quality scoring, backup/export/import/share/continue-
training/rollback, checkpoint/locator gates; and Local desktop path evidence
from an agent on the local computer in the real installed host with visible
marker plus screenshot/video/log evidence. JSON-only, adapter/core-only,
test-only hook, process/window-list, or synthetic helper UI cannot replace Local
desktop proof; screenshot-only cannot replace Automated boundary proof. A
blocked Local desktop path records the exact blocker, downgrades the claim,
continues other proof paths, and is not a stop gate. The contract must require
`phase.step`, Automated boundary status, Local desktop status, evidence,
remaining work, and downgraded claims in the final packet.

For product-readiness sessions, the final packet must also include scenario
counters: total scenarios, fully joined proven, partial, not moved, blocked with
next action, and exact scenario moved. A report may say evidence accounting is
done only when it also says whether the product requirement is proven. If it is
not proven, the report must name the next concrete proof work item.

---

## Section 3 — Read-Before-Write Rule

If a deliverable updates an existing file, the source file must be read completely
before any character is changed. Updating from memory produces the prior version,
not the current state.

```
□ For each file in the deliverables list that is an UPDATE (not a new file):
  → Read the complete file before writing
  → Record one specific fact from the end of the file as proof (Rule 5)
  → Only then: apply the update
```

---

## Section 4 — Two-Deliverable Minimum Verification

A session that produces one deliverable can verify it by reading it.
A session that produces multiple deliverables must verify each independently.

If any deliverable cannot be verified independently — either it is incomplete,
or it is not a real deliverable (it should be part of another file).

For skill translation batches specifically: each skill file and the HOW-TO-USE-SKILLS.md
update are two independent deliverables. Verify them separately:
```bash
# New skill file:
ls docs/skills/[new-skill]-SKILL.md
# Expected: file exists

# HOW-TO-USE-SKILLS.md updated:
grep -c "[new-skill]" docs/HOW-TO-USE-SKILLS.md
# Expected: 2 (activation trigger + index row)
```

---

## Anti-Patterns

1. **"Starting to write code before the output contract exists"** — the contract is the
   zeroth step. It exists before reading any files. Code is at least the second step.

2. **"Output contract written as prose: 'This session will produce updated skills...'"** —
   prose cannot be verified. Name the files. Write the verification commands.

3. **"'And any other relevant files' in the deliverables list"** — this is not a contract.
   It is permission to produce anything. If it's relevant, name it. If unknown, that's a
   scoping problem to resolve before starting.

4. **"Claiming done before running verification commands"** — run them. If they don't
   pass, the session is not done.

5. **"Two deliverables merged into one file without noting it"** — the contract must
   list each requested deliverable separately. Silent merging is a scope violation.

6. **"N ledger rows means N cycles"** — false. The contract must require N
   independent evidence records for N cycles/reviews/passes.

7. **"Sub-agent did it" without a sub-agent packet** — false. Name the
   sub-agent id/nickname and returned packet, or downgrade to parent/self work.

8. **Opaque execution status as the user report** — false. A row such as
   `Phase 01.01 Scenario And Claim Inventory`, `PASS_FOR_LUBA_REVIEW`, or a
   JSON/STATE code is not the output contract unless it is preceded by the
   human-readable phase/step/count/review/fix/next-action contract above.
   `QSG-28`, `07.06/07.07 NEEDS_FIX`, `packet accepted`, and `boundary pass`
   have the same failure mode when untranslated.

---

## Integration

- agent-output-format-SKILL.md governs what SESSION files look like internally; this skill governs whether the right files are produced
- verification-before-completion-SKILL.md enforces the specification gate at session end
- plan-self-validation-SKILL.md Phase 0 produces the output contract for a multi-phase plan
- documentation-sync-SKILL.md uses the contract's deliverables list to verify sync completeness
- Rule 5 (read before touching) is enforced by Part 3 of this skill's Read-Before-Write rule
