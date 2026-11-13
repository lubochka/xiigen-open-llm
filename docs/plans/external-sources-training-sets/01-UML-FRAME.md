# 01-UML-FRAME — Pipeline Edge Inventory

This file is the binding frame for all node pair files in this corpus run.
A pair in any node file that cannot be connected to an edge listed here does not belong.

---

## The Nine Nodes

| Node | Name | Input | Output |
|---|---|---|---|
| 1 | Domain Splitter | User prompt (natural language) | Array of domain labels |
| 2 | Domain Translator (array) | One prompt fragment + one domain label | Formal description in domain vocabulary |
| 3 | Scheme Planner | All translated domain descriptions together | Ordered sub-task list with transformational prompts |
| 4 | Domain Processor (array) | One sub-task + domain context | All success, failure, and wait-state outcomes for that sub-task |
| 5 | Applier (array) | One sub-task + transformational prompt + processed result | Concrete action taken and observable outcome |
| 6 | Result Translator (array) | One formal result + full results collection (for consistency checking) | Natural-language fragment readable by the user |
| 7 | Arbiter Selector | Full collection of formal results + flow context | Names of specific review units that must fire |
| 8 | Arbiter Units | One review responsibility + one result or result group | Pass (silent) / Concern (rule + location) / Reprocess (exact fragment + required change) |
| 9 | Formatter | All approved translated results + audience type | One combined user-readable output shaped for that audience |

---

## Edge Inventory

### Edge 1→2
- What moves: the user prompt, split by domain territory
- What Node 2 receives: one fragment from the prompt paired with one domain label from Node 1's output array
- What Node 2 must produce: a formal, precise description of what that fragment requires, in the vocabulary of that specific domain
- What breaks if missing: Node 3 receives ambiguous natural-language input and cannot produce a precise sub-task plan; the planner works from colloquial phrasing instead of formal domain terms

### Edge 2→3
- What moves: all formal domain descriptions produced by the Node 2 array
- What Node 3 receives: the complete set of translated domain descriptions together
- What Node 3 must produce: an ordered sequence of sub-tasks, each with a transformational prompt explaining how to approach that sub-task, including parallel steps, wait states, recovery paths, and review points
- What breaks if missing: the processor and applier arrays receive no plan; they cannot begin

### Edge 3→4
- What moves: one sub-task plus its domain context
- What Node 4 receives: exactly one sub-task (not the whole plan)
- What Node 4 must produce: all success states, all failure states, all wait-state outcomes for that one sub-task; enough information for the next stage to act without re-reading the original input
- What breaks if missing: the applier has no result to apply; the result translator has no formal result to translate

### Edge 3→5 (transformational prompt)
- What moves: the transformational prompt attached to the sub-task
- What Node 5 receives: the sub-task + the transformational prompt + the processed result from Node 4
- What Node 5 must produce: the concrete action taken and its observable outcome (what was written, returned, published, and in what order)
- What breaks if missing: the applier acts without the method directive; it may produce valid output for the wrong approach

### Edge 4→5
- What moves: the processed result for one sub-task
- What Node 5 receives: the processed result alongside the transformational prompt and sub-task
- What Node 5 must produce: concrete, observable outcome
- What breaks if missing: Node 5 has no result to apply; it operates on an empty input

### Edge 5→6
- What moves: one formal result from one applier unit
- What Node 6 receives: that one result, plus the full collection of all results for consistency checking
- What Node 6 must produce: a natural-language fragment the user can read and act on; internal system details stay internal unless the user explicitly needs them
- What breaks if missing: the user receives untranslated formal system output

### Edge 5→7 (parallel with 5→6)
- What moves: the full collection of formal results (before translation)
- What Node 7 receives: all formal results plus the flow context
- What Node 7 must produce: the specific names of review units that must fire before formatting; uses the formal pre-translation results, not the translated user-facing text
- What breaks if missing: the formatter receives results that have not been reviewed; errors, contradictions, and policy violations reach the user

### Edge 7→8
- What moves: the names of review units selected by Node 7
- What Node 8 receives: one review responsibility + one result or result group
- What Node 8 must produce: exactly one of — pass (silent), concern (naming the rule violated and where the source states it), or reprocess (naming the exact fragment to send back and what must change)
- What breaks if missing: the formatter receives unreviewed results; or results are reviewed by the wrong unit

### Edge 8→9 (pass path only)
- What moves: results that passed all required arbiters
- What Node 9 receives: approved translated results (never raw results, never unapproved results) plus the target audience type
- What Node 9 must produce: one combined user-readable output shaped for that audience; the same facts produce different shapes for different audiences
- What breaks if missing: the user receives no output; or the formatter receives unapproved content

### Edge 8→1 (reprocess path)
- What moves: a reprocess instruction naming the exact fragment to send back and what must change
- What Node 1 receives: the fragment as a new input to re-enter the pipeline
- What happens: the fragment traverses the full pipeline again
- What breaks if missing: errors identified by arbiters reach the formatter without correction

---

## Array Node Rule

Nodes 2, 4, 5, and 6 are array nodes. Each unit in the array receives exactly one element.
A pair for any of these nodes covers one unit receiving one element — never one large unit receiving all elements.

## Formatter Audience Types

The formatter produces different output for at least: end user, product manager, security reviewer, devops engineer.
At minimum two audience types must be covered in any node-09 file.
