# Skill: Implementation Documentation — Hierarchy and Chain Documentation (§5)

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

When a codebase uses a multi-level interface hierarchy (such as ILearningMachine → ILogicalOrchestrator → ILogicalFlow → ILogicalUnit), the implementation document must contain a dedicated section that shows how the levels connect, what data passes between them, and how the execution chain works end-to-end.

## Required subsections

### 5.1 Level diagram (ascii or mermaid)
Show all levels of the hierarchy with:
- Interface name at each level
- `Input` type → `Output` type for each level
- The role of each level in one sentence

Example format:
```
Уровень N: InterfaceName<TDomain>
  Принимает: InputType(field1, field2, ...)
  Возвращает: OutputType(field1, field2, ...)
  Роль: one sentence in Russian
```

### 5.2 Connection flowchart per machine
For each machine that implements ILearningMachine:
- Show direct call path (e.g., CLI runner → GeneratePlanAsync)
- Show orchestrator path (GetOrchestratorAsync → ILogicalOrchestrator → RunAsync)
- Show what data transforms at each step

### 5.3 Execution chain (for chains of ILogicalUnit)
When multiple ILogicalUnit implementations execute in sequence:
- Show each unit as a labeled box with Input type → Output type
- Show what each unit does internally (one sentence per unit)
- Note which units are deterministic (Bootstrap, Confidence=1.0) vs trained

### 5.4 Sequence diagram for plugin/host path
Show the full call chain from host entrypoint to final answer:
- Use `sequenceDiagram` with participant labels in Russian
- Label each arrow with the data type being passed (not just method name)
- Note required metadata fields (e.g., `active_repo_path`)

### 5.5 Skeleton orchestrators (if any)
For orchestrators that are contract-only stubs, show:
- What they always return (with actual constant values)
- What real implementation would return
- Why RequiresSecondPass or equivalent flag exists

## What NOT to do

- ❌ List interfaces without showing data types at each level
- ❌ Show chain without explaining what each unit does
- ❌ Sequence diagram where arrows only show method names (add data type labels)
- ❌ Skip stub/skeleton orchestrators — they must appear with their stub behavior documented

## Validation gate

- [ ] All levels of the hierarchy shown with Input/Output types
- [ ] Each machine has its connection diagram
- [ ] Each ILogicalUnit chain has labeled boxes showing Input→Output per unit
- [ ] Sequence diagram shows data types on arrows, not just method names
- [ ] Skeleton orchestrators documented with actual stub return values

## Integration

Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate), `plan-review-SKILL.md` (FC-18)
