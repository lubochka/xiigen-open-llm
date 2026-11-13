# Interface Design Craft

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

## Core Problem

A Windows Forms interface designed without intentional craft produces the same
output as every other generated interface. The visual patterns emerge from
defaults: standard SystemColors palette, uniform padding, grid layout, nothing
memorable. This happens because defaults are invisible — they feel like
infrastructure rather than design decisions. The process below ensures every
visual decision is a genuine choice with a stated reason, not an unexamined
default. Use alongside ui-quality-standard-SKILL.md (which governs bans and
validation) — this skill governs the design process that precedes the code.

---

## When to Invoke

- Before writing any new Windows Forms form, panel arrangement, or multi-control layout
- When a form passes ui-quality-standard-SKILL.md checks but still "feels generic"
- Before designing a new demo explorer stage or dashboard view
- When the user asks "why does this look the same as everything else?"

---

## Section 1 — Intent First

Before touching any control or layout property, answer these three questions.
Not as internal assumptions — stated explicitly before any design work begins.

```
Question 1: WHO is this human?
  Not "users" — the actual person.
  What are they doing when they open this form?
  What did they do 5 minutes before? What will they do after?
  A developer debugging at midnight is not a researcher reviewing patterns
  at 9am. Their context shapes what must be immediate vs secondary.

Question 2: WHAT must they accomplish?
  Not "use the form" — the specific action.
  Find the broken pipeline stage. Approve the promotion. Inspect the RAG result.
  The answer determines what leads visually, what follows, what hides.

Question 3: WHAT should this feel like?
  Say it in words that mean something. "Clean and modern" means nothing.
  Clinical like a diagnostic tool? Dense like a trading terminal?
  Quiet like a research reader? The answer shapes color temperature, spacing
  density, font weight choices — everything.
```

If you cannot answer all three with specifics: stop and ask Luba. Do not
guess. Defaults fill in when answers are vague.

---

## Section 2 — Domain Exploration

This is where defaults get caught — or don't.

Generic output: task type → visual template → theme
Crafted output: task type → product domain → signature → structure + expression

Before any visual or structural thinking, explore the product domain:

```
Domain:    List 5+ concepts, metaphors, and vocabulary from this product's world.
           Not features — territory.
           Example for Stage Explorer: "learning curve", "confidence threshold",
           "promotion gate", "decay", "gradient toward mastery"

Color world:  What colors exist naturally in this domain?
           Not "warm" or "cool" — specific to this product's context.
           Example: stage explorer: deep midnight blue (late-night debugging),
           amber (threshold warning), cold green (promotion confidence)

Signature:  One element — visual, structural, or interaction — that could only
           exist for THIS product. If you can't name one specific element:
           keep exploring.

Defaults:  Name 3 obvious choices for this interface type — BOTH visual and
           structural. You cannot avoid patterns you haven't named.
           Example: identical metric cards, system accent color, top-aligned tabs
```

**The test:** Read your domain exploration. Remove the product name.
Could someone identify what this is for? If not: explore deeper.

---

## Section 3 — Per-Component Checkpoint

Before writing any Windows Forms control, state the following:

```
Intent:    [who + what they're doing + how it should feel — from Section 1]
Palette:   [colors from domain exploration — and WHY they fit this product]
Depth:     [borders only | subtle shadows | none — and WHY this fits the intent]
Surfaces:  [elevation approach — and WHY this color temperature]
Typography:[font weight + size — and WHY it fits the intent, not just "readable"]
Spacing:   [base unit: 4pt — specific values: 4 / 8 / 12 / 16 / 24 / 32 / 48]
```

This checkpoint is mandatory before writing layout code. It forces every
technical choice to trace back to intent. If you cannot explain WHY for each
choice: you are defaulting. Stop and think.

---

## Section 4 — Craft Foundations

### Subtle layering

Surfaces stack. A status panel sits above the canvas. An active row sits above
inactive rows. Build an elevation system, not a single background color.

```
In light mode:  higher elevation = slightly lighter background
Level 0:  SystemColors.Control (base canvas)
Level 1:  slightly lighter (active panel, focused section)
Level 2:  visible foreground card (modal-equivalent area)
```

Each jump should be barely perceptible in isolation. When surfaces stack,
the hierarchy emerges. Whisper-quiet shifts you feel rather than see.

**Key decisions:**
- Side panels: same background as canvas, not different — use a subtle border
- Active rows: one level above inactive — not a highlight color, a surface shift
- Input areas: slightly darker than surroundings (inputs are "inset")

### Typography hierarchy

Build distinct levels. Size alone is not hierarchy — combine size, weight, spacing.

```
Heading:   Segoe UI 12pt Bold — tight line spacing
Section:   Segoe UI 10pt Semibold (or Bold)
Body:      Segoe UI 9pt Regular — this is the Windows Forms baseline, do not vary font
Label:     Segoe UI 9pt Regular with reduced opacity or secondary color
Data:      Consolas 9pt — for values, counts, numeric indicators
```

Hierarchy requires at least 1.3× ratio between heading and body sizes.

### Color with intention

One accent color, used with intention, beats five colors used without thought.

```
60% neutral surface (SystemColors.Control and near variants)
30% secondary text, borders, inactive indicators
10% accent color (project's primary: cold green for confidence, amber for warning)

Functional colors must follow conventions:
  Red   = error or failed state (never decorative)
  Green = success or promoted state (never decorative)
  Amber = warning, degraded confidence, approaching threshold
```

The accent color should feel like it came FROM the domain, not like it was
applied TO the interface. A confidence-tracking tool's accent green should
carry the specific tone of "growing evidence," not generic system green.

---

## Section 5 — The Swap Test

Before presenting any form to Luba:

```
The swap test: If you swapped your font weight choice for the most common
alternative, would anyone notice? If you swapped the layout for a standard
grid template, would it feel different? The places where swapping wouldn't
matter are the places you defaulted.

The signature test: Can you point to five specific controls or layout
decisions where your stated intent is visible? Not "the overall feel" —
actual components. A signature you can't locate doesn't exist.

The squint test: Blur your eyes. Can you still perceive hierarchy? Is
anything jumping out harshly? Craft whispers. No harsh lines. No jarring
color shifts. Quiet structure.
```

If any test fails: iterate before presenting. Your first pass is probably
generic. The work is catching it yourself before Luba has to.

---

## Anti-Patterns

1. **"SystemColors is fine — it's standard Windows Forms"** — SystemColors
   provides a correct baseline, not a design direction. The domain exploration
   determines which direction to tint from the baseline.

2. **"Clean and modern = the design is done"** — "clean and modern" is not
   an intent. It is the absence of deciding. Answer Section 1 with specifics.

3. **"Different colors = distinct design"** — color variation without hierarchy
   and a visual anchor produces noise, not design. Anchor first (Section 1
   Question 1), then vary color.

4. **"Process alone guarantees craft"** — following the process produces better
   output than no process. But process does not guarantee you caught all
   the defaults. Run the swap test after the process.

---

## Integration

- ui-quality-standard-SKILL.md governs BANS and validation (what not to do);
  this skill governs the design process (how to produce something non-generic)
- ui-examination-round-SKILL.md governs the structured round of improvement
  after the form is built; this skill governs the design before building begins
- ui-testing-SKILL.md handles functional correctness; this skill handles craft
