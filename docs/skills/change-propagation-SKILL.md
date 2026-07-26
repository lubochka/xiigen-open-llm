# Change Propagation

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (in any language), challenges an answer, or says a failure came
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

## The Prior Question

Before any task begins — not after it appears done — ask:

**"What are all the files that will be stale the moment this change is made?"**

That list is the blast radius. Writing it down before starting means nothing
is discovered reactively at the end. The task is not done until every item
on the list is checked off.

---

## The Dependency Map

Six change types, each with a complete blast radius.

---

### Change Type 1: SKILL ADDED (new docs/skills/*.md file)

**Blast radius — ALL of the following must update:**

```
docs/HOW-TO-USE-SKILLS.md
  - Add to "Activation triggers" table (when to load it)
  - Add to "Skill index" table at the bottom
  - Update version number (v5.0 → v6.0, etc.)
  - Update skill count in the header line

docs/COMPONENT-INTERFACES.md (if skill introduces a new interface)
  - Add interface signature block

CLAUDE.md Quick Reference (if skill creates a new canonical doc)
  - Add to Quick Reference table

AGENTS.md (if skill changes agent behavior rules)
  - Add to load_order or trigger section
```

**Before starting:** verify HOW-TO-USE-SKILLS.md exists and note the current
version number and skill count. These are the fields to update.

---

### Change Type 2: SKILL UPDATED (content change, no new file)

**Blast radius:**

```
docs/HOW-TO-USE-SKILLS.md
  - Only if the update changes WHEN the skill fires or WHAT it prevents

docs/HOW-TO-USE-SKILLS.md skill index table
  - Update the "What it governs" description if behavior changed
```

**Verification:**
```bash
grep -n "skill-file-name-SKILL.md" docs/HOW-TO-USE-SKILLS.md
# Confirm the entry reflects the new behavior
```

---

### Change Type 3: INTERFACE ADDED OR CHANGED

**Blast radius:**

```
docs/COMPONENT-INTERFACES.md
  - Add interface signature block (or update existing)
  - Add implementing classes

docs/SUBPROJECT-MAP.md
  - Update subproject status (Planned → Active)
  - Add interface to "Interfaces" column

docs/ARCHITECTURE.md
  - If interface changes the architecture diagram, update text description

CLAUDE.md
  - If interface is a primary fabric interface, add to BaseService pattern
    section or OperationResult namespaces section

Src/*/Program.cs (or DI registration file)
  - Add DI registration (services.AddSingleton<INew, Impl>())
```

**Verification commands:**
```bash
# Confirm COMPONENT-INTERFACES.md has the new interface
grep -n "INewInterface" docs/COMPONENT-INTERFACES.md

# Confirm SUBPROJECT-MAP.md reflects current status
grep -n "NN.SubprojectName" docs/SUBPROJECT-MAP.md

# Confirm DI registration present
grep -rn "INewInterface" Src/ --include="*.cs" | grep -i "AddSingleton\|AddTransient\|AddScoped"
```

---

### Change Type 4: SUBPROJECT ADDED

**Blast radius:**

```
docs/SUBPROJECT-MAP.md — add new row

docs/ARCHITECTURE.md — add to text description

CLAUDE.md
  - Update Quick Reference if subproject adds a canonical doc
  - Update test baseline count if subproject adds tests

LlmFromScratch.sln — add project reference

The new .csproj file — created at subproject start
```

---

### Change Type 5: PLAN PRODUCED OR AMENDED

**Blast radius:**

```
The REFERENCE-PLAN or EXECUTION file itself

docs/DECISIONS-LOCKED.md
  - Any new locked decisions from this plan/amendment

STATE-Pn.json
  - Update to reflect new plan if phase ordering changed

Any other EXECUTION file that references the amended plan's phase count
  - (grep the phase number; update cross-references)
```

---

### Change Type 6: DECISION LOCKED

**Blast radius:**

```
docs/DECISIONS-LOCKED.md — add D-NN entry

docs/COMPONENT-INTERFACES.md (if decision governs an interface contract)
  - Add reference: "See D-NN"

CLAUDE.md (if decision affects the 10 Rules or BaseService pattern)
  - Add reference to the decision or update the relevant rule

All EXECUTION files that were drafted before the decision was locked
  - Grep for the affected component; update if the decision changes the approach
```

**Renumbering verification (claim-verification-SKILL.md):**
```bash
grep -rn "D-[0-9][0-9]" docs/DECISIONS-LOCKED.md | tail -5
# Confirms the highest existing D-NN — next = max + 1
```

---

## How to Use This Skill

### At task start (not end):

```
1. Identify the change type(s) above
2. Write out the blast radius for that type
3. Cross off files not applicable to this specific change
4. What remains is the definition of "done" for this task
```

### At task end:

```
For each file in the blast radius:
  □ Was it updated?
  □ Can the update be verified? (grep, dotnet build, dotnet test)
  □ If not updated: explicit reason why not needed for this change
```

---

## The Structural Problem This Addresses

Information about interfaces, skills, and architecture is duplicated across
multiple files. Some files are sources of truth; most are derived views.

**Sources of truth** (one owner, one writer):
- Each `Src/*.cs` implementation file (owns its own code)
- Each `docs/skills/*-SKILL.md` (owns its own content)
- `docs/DECISIONS-LOCKED.md` (owns locked decisions)

**Derived views** (must be kept in sync):
- `docs/HOW-TO-USE-SKILLS.md` — aggregates from skill files
- `docs/COMPONENT-INTERFACES.md` — aggregates from interface files
- `docs/SUBPROJECT-MAP.md` — aggregates from subproject status
- `docs/ARCHITECTURE.md` — aggregates from system structure
- `CLAUDE.md` — aggregates conventions from all of the above

**The rule:** when a source changes, every derived view that references it
must update in the same commit. Not the next session. Not when noticed.
Same commit. (See documentation-sync-SKILL.md.)

---

## Anti-Patterns

1. **Closing a "skill added" task after updating only the skill file** —
   HOW-TO-USE-SKILLS.md is stale. The next session will be missing the
   skill in the activation triggers.

2. **"I'll update the other files in the next session"** — partial updates
   are the root cause of documentation drift. The next session doesn't have
   the context to know what was deferred.

3. **Discovering the blast radius at the end of a task** — the blast radius
   is identified at the START. It defines what "done" means, not what was
   forgotten.

4. **Updating a derived view instead of the source** — if COMPONENT-INTERFACES.md
   is stale because the source interface changed, update the interface (source)
   and then regenerate the doc entry (derived). Do not edit the doc to paper over
   a stale source.
