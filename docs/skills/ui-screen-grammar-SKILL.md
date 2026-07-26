# UI Screen Grammar Types

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

A Windows Forms form designed without declaring its grammar type defaults to
whatever pattern is easiest to implement: a data grid, a tab panel, a flat list.
The result looks "like a form" without being designed for the specific interaction
the user must perform. Grammar types are the seven fundamental interaction patterns
that every form belongs to — declaring one before writing any control forces the
design to commit to an interaction model, not just a layout.

---

## When to Invoke

- Before writing any new Windows Forms form, panel arrangement, or multi-control layout
- When a form passes ui-quality-standard-SKILL.md checks but the interaction model
  is unclear ("what is the user here to DO?")
- When deciding how to arrange controls for a new stage in Demo Explorer
- As part of interface-design-craft-SKILL.md Section 1 (Intent First — WHAT must
  they accomplish determines the grammar type)

---

## The Seven Grammar Types

### G1 — PROGRESS_STRIP
**User's question:** "Where is this in its lifecycle?"

The form visualizes a sequence of phases or stages for a single subject. The primary
visual element is an ordered sequence with status per step — not a list of items.

```
Windows Forms implementation:
  Primary: horizontal or vertical Panel chain (one panel per phase)
  Each panel: phase name (Label bold) + status chip (Panel with colored BackColor)
              + elapsed time (Label secondary) + optional action Button if step is current
  Progress indicator: ProgressBar or custom Panel showing N of M complete
  Status colors: Green = COMPLETE, Blue = IN_PROGRESS, Gray = NOT_STARTED, Red = FAILED

Applies to:
  - ContinuousLearningPipeline stage viewer
  - Capability promotion lifecycle (SEED → LEARN → PROMOTE → GRADUATE)
  - Training run phase tracking
  - Phase-completion progress tracker
```

### G2 — VERDICT_GRID
**User's question:** "What did each evaluator decide, and why?"

The form shows a two-dimensional grid: items (rows) × evaluators (columns). Each cell
is a verdict. The dominant visual element is the verdict grid itself, not a summary.

```
Windows Forms implementation:
  Primary: DataGridView with custom cell rendering per verdict type
  Columns: one per arbiter role (Role 1, Role 2 ... Role N)
  Rows: one per generation cycle or candidate output
  Cell content: APPROVED / BLOCK / CONCERN chip + brief reasoning (tooltip)
  Row summary: consensus verdict in the leftmost or rightmost column

Applies to:
  - Arbiter panel verdict display
  - Multi-model comparison results (Cycle A vs Cycle B)
  - Score component breakdown (iron_rules / dna_compliance / testability / ...)
```

### G3 — CARD_LIST
**User's question:** "Which items need my attention, in what state?"

The form renders a list of items where each item has a state and may require
action. The user scans the list to find items in specific states.

```
Windows Forms implementation:
  Primary: FlowLayoutPanel or ListView with custom item rendering
  Each item: identity (bold Label) + state chip + key metric + optional action Button
  Sort: by state priority (BLOCKED first) then by date
  Filter: ComboBox or CheckBoxes for state filtering
  Empty state: Label with explanation + guidance CTA (never empty Panel)

Applies to:
  - DPO triple index (each triple: state chip + quality score + curriculum tier)
  - Capability registry view (each capability: state chip + confidence score)
  - Test failure list (each failure: class + method + error summary)
  - Training data gap inventory
```

### G4 — TOPOLOGY_CANVAS
**User's question:** "How do the parts connect?"

The form renders a graph or pipeline: nodes connected by edges. The user inspects
the structure and relationships, not a list.

```
Windows Forms implementation:
  Primary: custom Panel with manual drawing (Graphics.DrawLine, Graphics.DrawRectangle)
           OR third-party graph control if available
  Nodes: labeled rectangles with color coding by handler type
  Edges: directional arrows with label (data type name)
  Side panel: selected node detail (inputs/outputs/wiring status)
  Selection: click node to highlight and populate side panel

Applies to:
  - AI pipeline topology viewer (rag-retrieve → decompose → ai-generate → ... → feedback)
  - DomainModelRegistry edge visualization
  - Capability dependency graph
  - Event flow diagram
```

### G5 — KIOSK
**User's question:** "I have one task, one decision."

The form is a focused single-action surface. One primary CTA dominates. Everything
else is context that supports making that one decision.

```
Windows Forms implementation:
  Primary: large CTA Button centered or prominently placed
  Supporting context: Labels with facts that inform the decision
  Secondary actions: smaller, visually subordinate Buttons
  No navigation within the form — this is the whole task
  Cancel/Back: always present and visually subordinate

Applies to:
  - Capability promotion approval (approve/reject with reason)
  - Manual arbiter override (confirm override + mandatory reason field)
  - Training run launch (confirm parameters + start)
  - Gap resolution confirmation
```

### G6 — DASHBOARD
**User's question:** "What are my key metrics right now?"

The form shows multiple metrics simultaneously. The user reads the state of the system,
not performs an action. Action buttons are secondary to information display.

```
Windows Forms implementation:
  Primary: grid of metric cards (TableLayoutPanel, fixed layout)
  Each card: metric name (Label secondary) + value (Label large bold) + trend indicator
  Color coding: Green = healthy, Amber = watch, Red = action needed
  Refresh: automatic or manual refresh button (never requires navigation to see current state)
  No primary CTA — the purpose is observation, not action

Applies to:
  - Engine independence dashboard (DPO count, gap scores, graduation progress)
  - Session health overview (test count, capability status, blocker count)
  - Training pipeline status overview
  - Phase completion summary
```

### G7 — SETTINGS_TABS
**User's question:** "Which setting do I need to change?"

The form organizes configuration options by domain, with each domain in a Tab.
The user knows what they want to change and navigates to the right Tab.

```
Windows Forms implementation:
  Primary: TabControl with descriptive Tab names (not Tab1, Tab2)
  Each tab: related settings in labeled GroupBox panels
  Input controls: TextBox, ComboBox, NumericUpDown with validation
  Save: explicit Save Button per Tab (not global save) OR unsaved indicator
  Validation: inline Label for each field in error state

Applies to:
  - FREEDOM config editor (tabs by domain: Scoring, Routing, Training, ...)
  - Pipeline parameter configuration
  - AI model selection and routing settings
  - Session governance settings
```

---

## Section 2 — Pre-Code Declaration

Before writing any form control, declare:

```
Grammar type: G[N] — [TYPE_NAME]
User question answered: "[exact question from grammar definition]"
Primary control structure: [control type from implementation guide above]
Why this grammar, not the alternatives: [one sentence — what makes G1 wrong for this form]
```

A form that cannot answer "why NOT G2?" for a form that serves one evaluator verdict
has not committed to a grammar. Generic justifications ("G6 seems right") are not
declarations.

---

## Section 3 — Grammar Violation Signals

Signs that a form was built without declaring a grammar type:

| Signal | Likely cause |
|--------|-------------|
| Form has both a DataGridView AND a progress bar as primary elements | G1/G2 conflict — pick one grammar |
| Primary CTA is hidden below a large table | G5 mixed with G3 — restructure |
| Dashboard cards all show the same type of metric | Not G6 — the cards are a G3 list of items |
| Topology canvas has a submit button as the primary action | G4 mixed with G5 — separate concerns |
| Settings tabs contain a progress indicator | G7 mixed with G1 — settings and monitoring are different grammars |

---

## Anti-Patterns

1. **"The form has multiple sections so it doesn't fit one grammar"** — a form that
   genuinely cannot commit to one grammar type should be split into two forms. A form
   that shows "the topology AND the settings" is two forms sharing a container.

2. **"G6 Dashboard is the default for complex forms"** — G6 means the user is observing
   metrics. If the user is here to take an action, G6 is wrong. Most operational forms
   are G5 or G3, not G6.

3. **"Grammar type is cosmetic"** — the grammar type determines which control is primary,
   how information is organized, and what the user can DO on the form. Declaring the wrong
   grammar and then "making it work visually" produces forms that confuse users about what
   they are supposed to accomplish.

---

## Integration

- interface-design-craft-SKILL.md: Section 1 (Intent First) answers WHO/WHAT/FEEL —
  the WHAT answer determines the grammar type; declare grammar at the end of Section 1
- ui-quality-standard-SKILL.md: bans apply after grammar is declared; they are not
  grammar-specific
- ui-examination-round-SKILL.md: GRAMMAR field in the examination record is populated
  from the grammar declared here before implementation
