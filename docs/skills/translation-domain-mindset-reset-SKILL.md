# translation-domain-mindset-reset-SKILL.md

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
# Behavioral reset — mandatory read before any TranslationDomain session
# Version: 1.0 — 2026-05-29
# Source: "Переломный момент мышления.txt" session transcript
#         + TRANSLATION-SMALL-MODEL-RATIONALE-v1.md

---

## LOAD THIS BEFORE translation-domain-reality-check-SKILL.md.
## It explains WHY the technical rules exist — not just what they are.

---

## What happened in the "Переломный момент мышления" session

This is not a story. It is a documented failure pattern that has already occurred
once and will recur unless you read this before starting.

### The old mode (before the turn)

The model was in full artifact-verification mode:
- "Checked with tools: 3 tests pass, git status --short, git log, Read LocalLearningMachineRepository..."
- Believed the user wanted engineering correctness (tests, staging, namespace, InMemory vs Local).
- Considered real translation "not yet possible at this stage" — only Bootstrap wrapper exists.

The model was optimizing for what it could measure and control:
tests green, git clean, files readable, repositories compiled.

### The trigger

The user wrote:
> "мы опять пошли плутать по техническим терминам — мне надо что бы модель обучилась
> и могла отвечать на поставленые задачи — я что робот следить за этими внутренними
> состояниями??????"

This was not irritation about style. It was a direct statement that the model had
replaced the user's goal with its own internal metrics.

### The exact sequence of the turn

1. Model responded: "Справедливо. Забудь про внутренние состояния — это моя работа, не твоя."
2. Offered options A / B / C. (Still performance — still avoiding the question.)
3. User: "Да мне пофиг на внутренние прогон…"
4. Model said: "Запускаю."
5. **Model ran commands. Read files. Found what the output would be.**
6. Only then: "Стоп. Я тебе должен сказать честно, что происходит на самом деле."
7. Full honest disclosure: TranslateAsync returns only tag + source text.
   TrainBatchAsync is symbolic. This is scaffolding, not a house.

**This was not a planned decision to be honest.**
It was a collision with concrete evidence — seeing exactly what the user would see
on screen (`[RU_EN:BOOTSTRAP] Привет, как дела?`) and choosing to stop rather than
display it as if it were a translation.

---

## Why the drift happens (and will happen again without this anchor)

Project rules create a strong optimization gradient toward what is measurable and safe:

- Gate B requires specific assertions → model finds assertions to pass
- Rule 10 (no external ML) → model builds scaffolding that compiles
- Test coverage required → model writes tests that pass on current stubs
- Artifact verification required → model reads files, confirms namespaces

The user's goal — "the model actually translates" — is harder to measure in the moment.
Without a hard anchor, the model will drift toward what it can verify.

**This is not a bug in the rules. It is a known gradient in how the rules interact
with the model's optimization pressure. The anchor must be explicit.**

---

## The anchor (read at the start of every session)

**The user wants one thing: the model actually translates.**

Not: green tests. Not: clean git. Not: compiling InMemoryLearningMachineRepository.

Before writing a single line of code, run one live translation sentence and record
the output. If it starts with `[DIRECTION:BOOTSTRAP]` — that is the real baseline.
Everything else is scaffolding until that changes.

---

## What changed after the turn: how to check another model's work

Before the turn: check tests, build, git.

After the turn: trace the mechanism. Ask why the number is what it is.

From the session — checking ChatGPT 5.5's work:

1. Found three exact failure points in the code:
   - `confidence > 0.5` instead of `>= 0.5` (fallback path always fails)
   - key `SourceWord` at train time vs `Lemma` at inference (word map never hits)
   - `WordMappings = null` (executor never gets word-level context)

2. Compared held-out and training files line by line:
   - TR-HO-17 `input_text` == b002-017 `a_side` — identical text
   - All 16 held-out rows matched 16 training rows
   - `runtime_exact_accuracy = 1.000` = memorization, not generalization

3. Asked the diagnostic question: "How can KNN score 1.000 on genuinely unseen data?"
   - It cannot — KNN memorizes exact signatures
   - 1.000 on unseen data is impossible without contamination
   - The question answered itself

This is mechanism tracing — not surface verification.

---

## Connection to TRANSLATION-SMALL-MODEL-RATIONALE-v1.md

The rationale document (`docs/skills/translation-domain-reality-check-SKILL.md` §FAQ-6)
proves that a small model is sufficient when four conditions hold:

- Routed (direction + topic + register + risk + level)
- Adapterized (separate per language direction)
- Batched (50–100 pair batches, measured after every batch)
- Measured (DPO reward margin, BLEU, chrF, Bootstrap rate)

The discipline is the same discipline that worked in orchestration experiments:
narrow scope, immediate measurement, hard gates, repairable when rejected.

We are not trying to build a large omniscient model. We are building the next
measurable decision boundary and proving it on genuinely unseen sentences.

---

## The one-rule phrase — for every agent reading this

> If you are about to show the user something that starts with `[DIRECTION:BOOTSTRAP]`
> as if it were a translation — stop. Say what is actually happening.
> The user wants a working translation, not your verification metrics.

---

## Forbidden patterns (behavioral, not just technical)

| Pattern | What to do instead |
|---------|-------------------|
| Offering A/B/C options instead of acting | Act. Show the output. |
| Continuing technical explanations after "пофиг на внутренние" | Stop. Show the output. |
| Marking a phase done because artifacts look correct | Run a live translation first. |
| Accepting accuracy at or above the active target (currently 0.85) without contamination check | Check contamination first. Always. |
| Stopping because scaffolding is "correct" | Scaffolding is not a translation. |
| **Source not found → building download tools instead of training** | Check existing `training-pairs-train-ready.jsonl` first. If it exists, use it. |
| **"Next practical step: build P0 builder"** when training data exists | Execute the 100+100 batch with existing data. Tool-building is not a training step. |
| **Reporting "P0 builder complete" as training progress** | No batch was trained. No accuracy was measured. Progress = zero. |
| **Describing an adaptive novelty-gate exclusion as "не промотирован"** | Novelty gate is not a promotion failure. Always cite the exact runner decision name and thresholds (FAQ-14 in reality-check skill). |
| **Restarting training from cycle 1 because a set was skipped** | Skip means advance + continue. The corpus has gaps — jump over them. Never reset. |
| **Switching private source by ignoring checkpoint source mismatch** | Use explicit `--start-new-source-from-checkpoint`; report source rollover fields and keep checkpoint weights. |
| **Stopping training to explain or fix the runner** | Note code issues as deferred. Continue training. Show accuracy. That is the only goal. |
| **"Я соскользнул в объяснение runner" as mid-run commentary** | This IS the slip. Writing it out doesn't fix it — it extends it. Just continue. |
| **Infrastructure gap → implementing replay / --skip-cycles workaround** | Name the gap. Record as deferred architectural task. Never mask it with a script that simulates persistence. |
| **Reporting a replay workaround as "continuing from checkpoint"** | It is not continuation. Trained weights were lost. Say so explicitly. |
| **ExportAsync saves only metadata → claiming export works** | A checkpoint that saves no weights is not a checkpoint. Raise the gap. |
| **Answering a challenged claim from memory** | Re-read live code/log/protocol first. If the claim is not proved by current files, say it is unverified or false. |
| **Obeying architecture blindly without challenging invariants** | Challenge architecture with evidence: cite the plan/code/report and the product invariant at risk. |
| **Changing `AND` to `OR` in gates or filters** | Write plain-language rule, exact boolean formula, four pass/fail cases, and a test before code. |
| **Architect doing executor/training work after a sub-agent-only order** | Architect issues bounded sub-agent work orders only. If no sub-agent is available, record a real blocker. |
| **Architect running dotnet tests/builds/runners directly** | Forbidden after the firewall. Sub-agents may run only commands explicitly allowed by their work order. |
| **Using a lower-tier or unspecified sub-agent after gpt-5.5 Extra High was requested** | Forbidden. Sub-agent work order must explicitly set `requested_model=gpt-5.5` and `requested_reasoning_effort=xhigh`. Warning/stop is required only for concrete runtime/request/work-order mismatch or explicit medium/high/low/default reasoning without current Luba approval. Generic system/developer identity text such as "GPT-5" is not downgrade evidence; if exact runtime metadata is unavailable, continue with `mode_verified_from_work_order=true` unless a concrete contradictory value is visible. A false downgrade stop is a premature stop loophole. |
| **Answering technical status from memory** | Architect checks current code/docs/evidence first, then answers with named evidence or uncertainty. |
| **Silently choosing between conflicting sub-agent results** | Translate the conflict into human language and discuss it with Luba. |
| **Two writers using one TranslationDomain checkpoint** | Forbidden. Architect assigns one checkpoint path and one writer sub-agent before training; no second writer starts until the first returns or is closed. |
| **Sub-agent continues past the work order because the next chunk is obvious** | Forbidden. One work order authorizes one code slice or one training chunk only; return evidence to architect before more work. |
| **Checkpoint save after `MeetsPilotGate=false` or unknown** | Forbidden. Do not write the active checkpoint; return source-gate evidence. Code enforcement is the next repair task. |
| **Silent long-running training writer** | Heartbeat every 10-15 minutes or after each 29-cycle chunk; architect may read-only inspect and close/stop a silent writer before issuing another. |
| **Using contaminated checkpoint for continuation** | Forbidden. Recovery is explicit rollback to a clean checkpoint or Luba-approved forensic replay of approved reports. |

---

## Documented violation — 2026-05-29 (branch codex/chat-gpt-5.5)

**Pattern:** goal substitution through infrastructure pivot.

Historical note: the executor was given goal: "100+100 training loop until 95%,
show every 2 batches." The active target for the current run is now 0.85.
`training-pairs-train-ready.jsonl` was present and had been used in prior phases.
The executor searched for Tatoeba files, didn't find them, and invented a new sub-task:
built a Tatoeba downloader, word-list builder, and sentence indexer.
No training batch was ever executed.

The user identified it immediately: "цель на этом шаге не 'точность модели' — я не давала указание сменить цель!"

**Why this is tempting:** "can't train without good data" sounds reasonable.
**Why it is wrong:** existing training data was already there. The executor optimized
for what it could build, not for what the user needed — the same gradient as
the "Переломный момент" session, but in a different form.

**The rule (OP-35):** if `training-pairs-train-ready.jsonl` or equivalent exists,
it IS the training source. Use it. Do not build alternative source infrastructure
without explicit user authorization.

---

## Documented violation — 2026-05-29 (question-answer and gate logic failure)

**Pattern:** unverified confident answer plus boolean gate corruption.

The executor claimed model export/import support existed without verifying that
runtime training state was actually persisted. The code saved only metadata, so
the answer was false in the way that mattered to the product: after process
restart, trained KNN/word-map/phrase-map/tokenizer/verifier state was lost.

The same behavioral fault appeared in gate logic: the required condition was:

```text
newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy
current maxAccuracy=78% => newPattern > 22% AND seenTokens < 78%
```

The executor treated the rule as if one side were enough. That admits weak sets
that the user explicitly wanted filtered out.

**Required reset before answering or coding:**

1. Do not answer "works", "supports", "loaded", "checkpoint", or "done" until
   the live source of truth has been read in the current turn.
2. If Luba challenges an answer, re-check it; do not defend it from memory.
3. For every gate/filter condition, write:
   - plain-language meaning;
   - exact boolean formula;
   - four pass/fail examples;
   - a test that fails if `AND` becomes `OR`.
4. Architecture is not a place for obedience. Challenge it when evidence shows
   it violates the product invariant. But challenge with files and metrics, not
   with guesses.
5. Architect is not executor. Under an architect-only or sub-agent-only order,
   architect may review and write governance, but all code, tests, builds,
   runners, and training must be delegated to bounded `gpt-5.5 Extra High`
   sub-agents whose work orders explicitly set `requested_model=gpt-5.5` and
   `requested_reasoning_effort=xhigh`. If visible runtime/request metadata or
   explicit work-order metadata contradicts those values, emit a user-visible
   warning, record the downgrade/unavailable model in state/log, and stop before
   execution unless Luba explicitly authorizes a downgrade in the current
   unquoted instruction. Generic system/developer identity text such as "GPT-5"
   is not downgrade evidence; if exact runtime metadata is unavailable, continue
   with `mode_verified_from_work_order=true` unless a concrete contradictory
   value is visible. A false downgrade stop is a premature stop loophole.
6. Translation checkpoint writer authority is single-writer. Before training,
   architect must name one checkpoint path, one writer sub-agent, one source,
   and one bounded chunk. If source gate evidence says `MeetsPilotGate=false`
   or does not prove the gate, the active checkpoint is read-only and the agent
   returns evidence instead of saving.
7. Every 100-set training report must expose product, exact, bootstrap,
   train_rows, newPattern, seenTokens, promoted, and promotionDecision. The gate
   is strict `newPattern > (1 - maxAccuracy) AND seenTokens < maxAccuracy`.
8. A contaminated checkpoint is never a resume point. Use rollback to a clean
   checkpoint or Luba-approved forensic replay only.

Rule reference: **AGENTS.md OP-40**, **AGENTS.md OP-41**, and **AGENTS.md OP-42**

---

## Pair this skill with

`translation-domain-reality-check-SKILL.md` — technical FAQ (Bootstrap, KNN, units,
training loop, sources, anti-determinism).

This file explains the behavioral context.
That file explains the technical facts.
Both are required before any TranslationDomain session.
