# Document Hierarchy

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
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

Two documents with conflicting information about the same fact cause planning
errors. One document says the baseline is 916 tests; another says 928. One plan
says capability X is in subproject 21; another says it is in subproject 23.
The hierarchy navigator prevents new documents from being produced when an
authoritative one already exists, and routes every question to the single correct source.

---

## When to Invoke

- Before producing any new `.md` or `.json` document
- Before answering an architectural question that could reference stale data
- When two documents appear to conflict
- When deciding "where does this information live?"
- When adding a new entry to docs/ and unsure which canonical file to update

---

## Section 1 — Three Checks Before Producing Any Document

### Check 1: Does a canonical document already exist for this concern?

```
Read: docs/SUBPROJECT-MAP.md → document map section

If a Tier 1, 2, or 3 document already governs this concern:
  → Do NOT produce a new document
  → Instead: update the existing canonical document
  → Add a section, extend it, version it
  → Never create a competing parallel document

Only create a new document if:
  □ No existing document covers this concern
  □ The document is a new phase plan (PHASE-N.md)
  □ The document is a session execution log
  □ The document is a session state checkpoint (STATE.json equivalent)
```

### Check 2: Is the information consistent with existing documents?

```
Any new document must not contradict:
  □ docs/DECISIONS-LOCKED.md (any locked decision D-01..D-NN)
  □ docs/SUBPROJECT-MAP.md (current subproject statuses and baselines)
  □ The canonical Tier 1 documents (CLAUDE.md, AGENTS.md)

If a conflict is found: the existing document is right.
Investigate why the new information is different before writing anything.
```

### Check 3: Is a superseded document being accidentally referenced?

```
Superseded documents (do NOT use for new planning):
  Any file with -v1, -v2, -v3 suffix where a -v4 or higher exists
  → Use the highest version
  → Do not propagate superseded content
```

---

## Section 2 — Routing Table

"Where is the answer to this question?"

```
Architectural decision rationale  → docs/DECISIONS-LOCKED.md
Current test baseline count       → dotnet test --list-tests (live run, not memory)
Next available decision number    → read DECISIONS-LOCKED.md last D-NN + 1
Subproject current status         → docs/SUBPROJECT-MAP.md
Interface reference               → docs/COMPONENT-INTERFACES.md
Architecture and component diagram → docs/ARCHITECTURE.md
Experiment results E01-E50        → docs/EXPERIMENT-RESULTS-MAP.md
Which skill governs this work     → docs/HOW-TO-USE-SKILLS.md
Agent execution rules             → AGENTS.md
Core 10 rules + BaseService       → CLAUDE.md
```

---

## Section 3 — Document Tier Hierarchy

When two documents conflict, the higher tier wins:

```
Tier 0 (supreme):   CLAUDE.md + AGENTS.md
                    → Override everything else. Always current.

Tier 1 (primary):   docs/DECISIONS-LOCKED.md
                    docs/SUBPROJECT-MAP.md
                    → These are maintained after every session.

Tier 2 (reference): docs/COMPONENT-INTERFACES.md
                    docs/ARCHITECTURE.md
                    → Updated when interfaces or architecture change.

Tier 3 (results):   docs/EXPERIMENT-RESULTS-MAP.md
                    docs/HOW-TO-USE-SKILLS.md
                    → Updated when experiments complete or skills added.

Tier 4 (session):   Phase plans, execution logs, STATE.json equivalents
                    → Superseded by the corresponding Tier 1 updates.
```

**Resolution protocol when documents conflict:**
```
Step 1: Identify which documents conflict on which specific value
Step 2: Apply tier hierarchy — higher tier wins
Step 3: Update the lower-tier document to match the higher-tier document
Step 4: Note the reconciliation in docs/DECISIONS-LOCKED.md if architectural
```

---

## Section 4 — What Goes in Which Document

| Information type | Goes in |
|---|---|
| Locked architectural decisions (D-01..D-NN) | docs/DECISIONS-LOCKED.md |
| Current subproject status and test baselines | docs/SUBPROJECT-MAP.md |
| Interface contracts and method signatures | docs/COMPONENT-INTERFACES.md |
| Component diagram and dependency graph | docs/ARCHITECTURE.md |
| Experiment hypotheses and results | docs/EXPERIMENT-RESULTS-MAP.md |
| Skill activation triggers | docs/HOW-TO-USE-SKILLS.md |
| Rules that govern Claude Code behavior | CLAUDE.md |
| Multi-agent coordination rules | AGENTS.md |
| Phase execution details (what was done) | Execution log files (session-level) |

---

## Anti-Patterns

1. **"I'll create PHASE-4-ARCHITECTURE.md to document the architecture decisions for this phase"** —
   architecture decisions belong in `docs/DECISIONS-LOCKED.md`. Create a D-NN entry, not a new file.

2. **"The baseline was 916 last session"** — session state is ephemeral. Baselines live in
   `docs/SUBPROJECT-MAP.md` and are verified with `dotnet test`. Memory is not a baseline.

3. **"According to the PHASE-2-PLAN.md baseline..."** — phase plans are Tier 4 documents.
   The live test run or `docs/SUBPROJECT-MAP.md` is the authoritative baseline, not a plan file.

4. **"Let me document this decision in the phase plan"** — decisions that govern future
   sessions belong in `docs/DECISIONS-LOCKED.md`. Phase plans are Tier 4 and will be
   superseded. Locked decisions persist across sessions.

---

## Integration

- planning-session-startup-SKILL.md Q3 (governing documents) uses this skill's tier hierarchy
- decision-capture-SKILL.md routes decisions to docs/DECISIONS-LOCKED.md
- artifact-numbering-SKILL.md reads docs/DECISIONS-LOCKED.md to get the next D-NN number
- change-propagation-SKILL.md uses this skill to identify which canonical documents need updating
- audit-protocol-SKILL.md Point 6 (path verification) checks that referenced files exist in Tier 1+ documents
