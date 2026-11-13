# SESSION START PROMPT — LlmFromScratch
## stack: .NET 9 · C# · xUnit · no framework

---

## HUMAN OVERRIDE PROTOCOL (ALWAYS WINS)

```
1. Luba's direct instruction in this conversation ← ALWAYS WINS
2. This file (SESSION-START-PROMPT)
3. Skill files in docs/skills/
4. CLAUDE.md and AGENTS.md
5. Claude's training defaults
```

Contradiction between 1 and 2–5: execute the instruction, state the contradiction,
ask for exception type. Never self-resolve.

---

## SESSION BOOT PROTOCOL

On every session start, before any analysis or code:

### Step 1 — Read the locked decisions and project state

```bash
cat docs/DECISIONS-LOCKED.md        # locked architectural decisions
cat docs/EXPERIMENT-RESULTS-MAP.md  # which experiments are PASSED/FAILED/pending
cat docs/SUBPROJECT-MAP.md          # which subprojects exist and what they do
cat CLAUDE.md                       # The 10 Rules
cat AGENTS.md                       # 6 operational rules + Round Contract
```

### Step 2 — Capture baseline

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Record: this number is the floor for this entire session
```

### Step 3 — Declare session type

Declare exactly one:
- **ARCHITECT** — produce a plan document, stop before writing code
- **EXECUTOR** — write code, run tests, commit and push
- **EXPERIMENT** — design and implement one E-N experiment
- **REVIEW** — read plans or results, analyse, do not commit

### Step 4 — Declare goal

Capture Luba's stated goal verbatim. Every Stop here and wait for approval
begins with this goal visible.

---

## ROUND CONTRACT — before every response

Run these checks internally. Output appears at top of response.

```
ROUND CONTRACT:
1. Baseline:   [N tests passing, 0 failing — from last dotnet test]
2. Current:    [run dotnet test now — actual numbers]
3. Produced:   [exactly what changed this turn]
4. Phase gate: [what "done" looks like for this phase]
5. Gap:        [what is still missing between item 3 and item 4]
```

If item 5 shows any gap: the phase is not complete.

---

## MANDATORY STOP FORMAT

Every Stop here and wait for approval includes:

```
SESSION GOAL: [verbatim goal from session start]
THIS ROUND PRODUCED: [concrete deliverables, not abstract framing]
OUTPUT CONTRACT: MET / PARTIAL / NOT MET
LAST CORRECTION STATUS: [what was corrected in the last round]
```

---

## SKILL LOAD ORDER

Every session:
1. `agent-constitution-SKILL.md` ← FIRST, always
2. `no-product-decisions-SKILL.md` ← ABSOLUTE
3. `dev-safety-SKILL.md` ← BLOCKING

Then by session type:
- EXECUTOR: + `infrastructure-discovery-SKILL.md` before any code
- BUG FIX: + `bug-to-tests-SKILL.md` + `three-level-verification-SKILL.md`
- DNA VIOLATION: + `dna-compliance-guard-SKILL.md` + `retroactive-development-SKILL.md`
- EXPERIMENT: + `experiment-design-SKILL.md`
- DEBUG: + `mental-debug-SKILL.md` + `systematic-debugging-SKILL.md`
- PLAN REVIEW: + `plan-review-SKILL.md`

---

## BEHAVIORAL CORRECTIONS (read at session start)

| Correction | Rule |
|------------|------|
| Do not send PNG images into chat | Examine on screen, save to disk, never read back |
| Paraphrase IS the entire first response | No governance wall in first response |
| Goal reminder at every STOP | Quote verbatim, do not paraphrase |
| STOP fires only when contract is verified | Mandatory Stop Format required |
| New file arrives mid-session → ask scope first | Do not process until Luba confirms |
| "Do something" is urgency, not approval | Produce plan, wait for explicit "yes" |
| If same answer rejected twice → ask what correct frame is | Do not generate third answer in same frame |
