# UI Quality Standard

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

A Windows Forms interface produced without design quality criteria looks
like every other auto-generated interface: identical card grids, generic
layouts, no intentional hierarchy. The same pattern of obvious choices
applies regardless of context. This skill defines the absolute bans that
prevent AI-slop UI patterns, the design principles that make an interface
memorable rather than generic, and the AI slop test that validates the
result before claiming the UI is done.

---

## When to Invoke

- Before writing or reviewing any Windows Forms control, layout, or panel
- When ui-testing-SKILL.md passes but the UI still "feels wrong"
- When designing a new MainForm layout or adding a stage to the demo explorer
- When reviewing a UI change before it enters a release commit

---

## Section 1 — Absolute Bans

These patterns are never acceptable. They are the most recognizable
AI-generated design tells. If any of these appear in a Windows Forms
form: stop and redesign the element with a different approach.

```
BAN 1: Side-stripe borders on panels, list items, or status indicators
  Pattern: BorderStyle with a colored 3px+ stripe on the left or right side
  Why banned: The single most overused "design touch" in tool/admin UIs.
              Never looks intentional regardless of color.
  Rewrite: Use full borders, background tints, or no visual indicator at all.

BAN 2: Gradient text
  Pattern: Using BackColor + ForeColor gradient patterns on labels for "impact"
  Why banned: Decorative rather than meaningful; top AI design tell.
  Rewrite: Use a single solid foreground color. Use font weight or size for emphasis.

BAN 3: Identical control grids
  Pattern: N panels of identical size, each with icon + label + description,
           repeated with no variation in size or visual weight.
  Why banned: Communicates no hierarchy. Every item looks equally important.
  Rewrite: Vary sizes based on importance. Use primary/secondary groupings.

BAN 4: Generic drop shadows on every panel
  Pattern: Every panel has the same shadow offset and opacity regardless of depth intent.
  Why banned: Safe, forgettable, could be any generated output.
  Rewrite: Use shadow only where actual depth relationship is intended.

BAN 5: Modal dialogs for low-stakes interactions
  Pattern: Showing a modal MessageBox.Show() or Form for confirmations
           that don't require interrupt-level attention.
  Why banned: Lazy interaction design; breaks flow for non-critical decisions.
  Rewrite: Use inline status indicators or confirmation within the existing panel.
```

---

## Section 2 — Design Hierarchy Principles

### Typography

```
- Use at most 2 font families per form
- Body text: sans-serif (Segoe UI 9pt is the Windows Forms baseline — do not vary)
- Heading text: vary weight (Bold) and size, not font family
- Do NOT set all labels to the same font size. Hierarchy requires at least
  a 1.3× ratio between heading and body sizes.
- Do NOT use ALL_CAPS for long labels. Reserve for 2-4 word status indicators.
```

### Color

```
- Use a primary accent color from the project's existing SystemColors palette
- Tint neutral backgrounds toward the accent hue (even SystemColors.ControlLight
  can be shifted slightly toward the accent by drawing custom backgrounds)
- The 60-30-10 rule: 60% neutral surface, 30% secondary text/borders, 10% accent
- Functional colors must follow conventions: red = error, green = success,
  amber = warning. Do not use these colors for decorative purposes.
- Never use pure Black (0,0,0) or pure White (255,255,255) for text backgrounds.
  Use SystemColors.ControlText and SystemColors.Control respectively.
```

### Layout and Spacing

```
- Use a 4pt spacing unit. Space values: 4, 8, 12, 16, 24, 32, 48, 64.
- Do NOT apply identical padding to every panel. Vary for hierarchy.
- Establish one dominant visual anchor per form (the primary action or primary
  result display). Everything else is secondary.
- Left-aligned text with asymmetric compositions feels more designed than
  everything centered.
- Do NOT wrap labels beyond ~80 characters. Add MaximumSize with WrapContents.
```

---

## Section 3 — AI Slop Test

Before claiming any UI change is complete, apply this test:

```
If you showed this interface to someone and said "AI made this," would they
believe you immediately?

If yes: that is the problem. A distinctive interface should make someone ask
"how was this made?" — not "which AI template was used?"
```

Review this list. If any of these apply: redesign before committing:

```
□ Every panel has the same padding
□ Every heading is the same font size
□ Controls are arranged in an identical grid pattern
□ The primary action is not visually distinct from secondary actions
□ All colors are from the SystemColors defaults with no project-specific accent
□ There is no visual anchor — no single element clearly dominates
□ Every status indicator uses the same visual style regardless of severity
```

---

## Section 4 — Checker Protocol

When reviewing an existing Windows Forms form:

```
Phase 0: Context
  □ What is this form for? Who uses it? What is the primary action?
  □ What aesthetic direction was intended (clinical/tool/explorer/dashboard)?

Phase 1: Absolute Bans
  □ No BAN 1 side-stripe borders
  □ No BAN 2 gradient text
  □ No BAN 3 identical control grids
  □ No BAN 4 generic drop shadows on every panel
  □ No BAN 5 modal dialogs for low-stakes interactions

Phase 2: Hierarchy
  □ One dominant visual element (the anchor)
  □ At least 2 size levels for text (heading vs body)
  □ Varied spacing communicates grouping and importance

Phase 3: Color
  □ Accent color is used selectively (10% rule)
  □ Functional colors (red/green/amber) reserved for semantic use only
  □ Neutral surfaces tinted toward project accent

Phase 4: AI Slop Test
  □ Run the test above — would an observer immediately identify this as AI output?
```

---

## Anti-Patterns

1. **"It passes ui-testing-SKILL.md, so it's done"** — passing tests verifies
   correctness. This skill verifies quality. Both are required.

2. **"SystemColors is fine — it's standard Windows Forms"** — SystemColors
   provides a correct baseline, not a design direction. Apply the 10% accent
   rule on top of the baseline.

3. **"The form is tool-facing, not user-facing — design doesn't matter"** —
   tool-facing forms with poor hierarchy create cognitive overhead for the
   developer using the tool. The AI slop test applies to all forms.

4. **"I used different colors, so it's distinct"** — color variation without
   hierarchy and visual anchor produces noise, not design. Anchor first,
   then vary color.

---

## Integration

- ui-testing-SKILL.md handles functional correctness; this skill handles design quality
- verification-before-completion-SKILL.md fires before any completion claim — apply this skill as part of that verification for UI phases
- dna-compliance-guard-SKILL.md handles code structure; this skill handles visual structure
