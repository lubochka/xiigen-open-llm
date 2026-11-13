# Claude Export Development-Cycle Bundle Map

Source: `C:\Users\lubao\Documents\xiigen\old\data-6308de18-3e10-4d75-9d43-35c6c315a9ad-1777178009-8b897c61-batch-0000.zip`  
Index: `C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\claude-export\batch-0000\index\conversation-index.jsonl`  
Version: 1.0 - 2026-05-05

---

## What Was Checked

The export was streamed and indexed by conversation metadata, dates, prompt count, correction count, delivery evidence, and file references.

```text
Total conversations: 968
XIIGen-relevant conversations: 462
Date buckets with relevant signal: 92
```

The first automatic grouping was too broad because generic terms like `flow`, `phase`, and `architecture` appear in many non-orchestrator projects. The corrected grouping below prioritizes real development cycles that produced deliverables from existing or old code.

---

## Priority Finding

The early sessions are not secondary. They are among the highest-value teaching sources because they represent a common real-world demand:

```text
Given an existing/old codebase, produce documentation, skills, plans,
tests, and reusable development guidance.
```

This is exactly the kind of orchestration task the machine must learn:

```text
legacy/current code input
  -> understand project structure
  -> split into documentation / skills / architecture / testing work
  -> produce deliverables
  -> accept correction feedback
  -> improve the next cycle
```

Therefore the first teaching pass should not start only from the latest orchestrator documents. It should include these early development-cycle bundles.

---

## Bundle 1 - Legacy Documentation And Skills From Existing Code

```yaml
bundle_id: "legacy-docs-skills-from-old-code"
label: "Documentation and skills library from existing/old project code"
date_window: "2026-03-10 -> 2026-03-11"
conversation_count: 2
messages: 158
human_messages: 77
valid_a_sides: 74
correction_events: 35
delivery_hits: 109
```

Conversations:

```yaml
- name: "Documentation and skills for Claude Code"
  uuid: "647878b2-ac25-4780-a623-30fb422f8f75"
  created: "2026-03-10"
  updated: "2026-03-10"
  valid_a_sides: 3
  correction_events: 1
  delivery_hits: 6
- name: "Documentation and skills for Claude Code"
  uuid: "2b4b55b1-382f-4dc3-a235-70a813f7b8a1"
  created: "2026-03-10"
  updated: "2026-03-11"
  valid_a_sides: 71
  correction_events: 34
  delivery_hits: 103
```

Primary teaching value:

```text
This teaches the orchestrator the "old/current code -> documentation + skills"
development cycle. This is a high-demand real task and should be a first-priority
bundle, not historical noise.
```

Expected covered areas:

```text
1. functional understanding of existing code
2. architecture extraction
3. documentation generation
4. skill-library generation
5. bug/context discovery while reading code
6. correction learning from rejected documentation/skill outputs
```

---

## Bundle 2 - Figma-To-Code Skills Adapted Into XIIGen

```yaml
bundle_id: "figma-to-code-skills-to-xiigen"
label: "Adapt Figma-to-code skills into XIIGen platform skills"
date_window: "2026-03-18 -> 2026-03-19"
conversation_count: 2
messages: 425
human_messages: 209
valid_a_sides: 163
correction_events: 69
delivery_hits: 317
```

Conversations:

```yaml
- name: "Adapting figma to code skills for xiigen platform"
  uuid: "33797d24-0e59-42dd-9090-f349af79ce51"
  valid_a_sides: 87
  correction_events: 39
  delivery_hits: 158
- name: "Adapting figma to code skills for xiigen platform"
  uuid: "64051905-0315-480b-9416-91e59372c8a0"
  valid_a_sides: 76
  correction_events: 30
  delivery_hits: 159
```

Primary teaching value:

```text
This teaches adaptation of an existing skill library into a target platform.
The machine must compare old skill assets, target project structure, tests,
logical units, automation expectations, and delivery constraints.
```

Expected covered areas:

```text
1. source skill inventory
2. target platform mapping
3. reusable skill extraction
4. compile/test requirements
5. per-logical-unit testing
6. correction of wrong stack assumptions
```

---

## Bundle 3 - Self-Learning Cycle Validation

```yaml
bundle_id: "self-learning-cycle-validation"
label: "Validate self-learning cycles against existing code and deliverables"
date_window: "2026-03-19 -> 2026-03-20"
conversation_count: 3
messages: 315
human_messages: 155
valid_a_sides: 131
correction_events: 35
delivery_hits: 207
```

Primary teaching value:

```text
This teaches the machine to evaluate whether a proposed learning cycle is
realistic against existing code, current documents, and required deliverables.
```

Expected covered areas:

```text
1. plan validation
2. code-state checking
3. deliverable review
4. learning-cycle sequencing
5. feedback incorporation
```

---

## Bundle 4 - Flow-01 Engine Execution And Station Testing

```yaml
bundle_id: "flow-01-station-testing"
label: "Run Flow-01 through engine, station testing, and improvement cycles"
date_window: "2026-03-23 -> 2026-03-24"
conversation_count: 5
messages: 478
human_messages: 231
valid_a_sides: 209
correction_events: 79
delivery_hits: 311
```

Primary teaching value:

```text
This teaches full flow execution: station outputs, feedback, cycle-to-cycle
improvement, master-plan updates, and correction of false assumptions.
```

Expected covered areas:

```text
1. flow execution
2. station-level testing
3. cycle improvement
4. generated vs manually written code distinction
5. plan update from execution evidence
```

---

## Bundle 5 - Session Prompt And Learning Protocol

```yaml
bundle_id: "session-prompt-and-learning-protocol"
label: "Session prompt, learning protocol, and multi-session issue synthesis"
date_window: "2026-03-25"
conversation_count: 4
messages: 336
human_messages: 169
valid_a_sides: 145
correction_events: 71
delivery_hits: 221
```

Primary teaching value:

```text
This teaches how a session itself should behave as a learning process:
maintaining goal focus, reading prior evidence, synthesizing issues from
multiple sessions, and generating an execution protocol.
```

---

## Bundle 6 - Value Proposition And Code Gaps

```yaml
bundle_id: "value-proposition-code-gaps"
label: "XIIGen value proposition and code gap analysis"
date_window: "2026-03-24 -> 2026-03-26"
conversation_count: 8
messages: 562
human_messages: 275
valid_a_sides: 229
correction_events: 104
delivery_hits: 409
```

Primary teaching value:

```text
This teaches the orchestrator to connect product value, implementation gaps,
roadmap planning, and code-state evidence.
```

---

## Recommended First Teaching Order

Use this order, not latest-first:

```text
1. legacy-docs-skills-from-old-code
2. figma-to-code-skills-to-xiigen
3. self-learning-cycle-validation
4. flow-01-station-testing
5. session-prompt-and-learning-protocol
6. value-proposition-code-gaps
7. April orchestrator-machine-cycle bundles
8. May implementation/teaching bundles
```

Reason:

```text
The early bundles teach the base market-demand pattern:
old/current code -> documentation -> skills -> tests -> plan -> corrected output.

The later bundles teach the specialized XIIGen orchestrator implementation pattern.
The machine needs both, but the early pattern should come first because it is the
broader reusable product behavior.
```

---

## Required Use In The Export Addendum

`XIIGEN-EXPORT-TEACHING-ADDENDUM-v2.md` should treat this map as the current first-pass bundle ordering.

The executor must not discard early conversations simply because they predate the current orchestrator implementation. If they produced documentation, skills, tests, plans, or correction evidence from an existing codebase, they are valid orchestration teaching sources.

---

# Last updated: 2026-05-05
