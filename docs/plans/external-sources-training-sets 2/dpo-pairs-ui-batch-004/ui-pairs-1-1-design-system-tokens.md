# UI Pairs — Sub-phase 1.1 Design System and Tokens
# Batch: dpo-pairs-ui-batch-004
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: visual tokens, component primitives, spacing, typography, color, reusable UI rules
# Pair count: 4

---

## Pair AP4-1.1-001 — token specification confused with layout placement

```
source_id: xiigen-ui-batch-004-AP4-1.1-001
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.1
rejected_sub_phase: 1.2
```

Prompt: `"define the typography system for the new product — fonts, sizes, line heights, and weights — that every team can use consistently"`

```
CHOSEN — 1.1 Design System and Tokens:
A typography token table that applies across every product surface. Each
token has a name, a value, and a use-case constraint.

Type-family tokens:
  font-family-display    → "Inter Display, sans-serif"
                           use only for hero headlines and feature
                           announcements; never in body content.
  font-family-body       → "Inter, sans-serif"
                           the default for all running text.
  font-family-mono       → "JetBrains Mono, monospace"
                           use for code blocks and inline code only.

Type-size tokens (with line height):
  text-xs    → 12 px / 16 px line  → captions, footnotes, chip labels.
  text-sm    → 14 px / 20 px       → secondary UI text, table cells.
  text-base  → 16 px / 24 px       → body, default paragraph text.
  text-lg    → 18 px / 28 px       → emphasised paragraph or lead text.
  text-xl    → 20 px / 28 px       → small section headings.
  text-2xl   → 24 px / 32 px       → page section headings.
  text-3xl   → 30 px / 36 px       → page titles.
  text-4xl   → 36 px / 40 px       → top-level page titles only.
  text-5xl   → 48 px / 52 px       → hero / display use only.

Weight tokens:
  weight-regular   → 400
  weight-medium    → 500
  weight-semibold  → 600
  weight-bold      → 700
  Forbidden: weight 800 / 900 — they read as decorative and conflict
  with the brand's quiet voice.

Tracking (letter-spacing) tokens:
  Default tracking is 0. The only allowed deviation is for all-caps
  labels (-0.02em). No other tracking values are permitted.

Use-case rules:
  Body content uses text-base + weight-regular at minimum.
  Never use a display weight (semibold / bold) for body content.
  Never combine more than two weights in a single screen.

REJECTED — 1.2 Layout and Responsive Structure:
Define how typography lays out on the page. On mobile, paragraph text takes
the full width with 16px side margins. On tablet, paragraphs are 80% width
centred. On desktop, paragraphs are constrained to a max-width of 720px
for readability. Page headings appear with 32px bottom margin separating
them from the next paragraph. Section headings appear with 24px top margin
and 16px bottom margin. Inline code keeps its monospace alignment within
running text.
[WRONG: "define the typography system" asks for the token table — font
families, sizes, weights, and the rules for which token goes where.
Layout decisions about widths and margins are 1.2's responsibility; they
follow from the tokens but they do not replace them. A team trying to
apply consistent typography needs the token names and values; the layout
decisions are downstream and surface-specific.]
```

---

## Pair AP4-1.1-002 — color token specification confused with interaction states

```
source_id: xiigen-ui-batch-004-AP4-1.1-002
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.1
rejected_sub_phase: 1.3
```

Prompt: `"define the color tokens for the application — primary, semantic, and neutral palettes, plus rules for when to use each"`

```
CHOSEN — 1.1 Design System and Tokens:
A token table for color, organised by purpose. Each token has a name, a
hex value, a contrast-ratio note, and a use-case rule.

Primary palette:
  color-primary-50   → #EFF6FF  (background tint, very subtle)
  color-primary-100  → #DBEAFE  (informational background)
  color-primary-500  → #3B82F6  (default primary action)
  color-primary-600  → #2563EB  (hover / pressed of primary action)
  color-primary-700  → #1D4ED8  (active / focused)
  color-primary-900  → #1E3A8A  (text on light primary backgrounds)

Semantic palette:
  color-success-500  → #10B981
  color-warning-500  → #F59E0B
  color-error-500    → #EF4444
  color-info-500     → #3B82F6  (alias for primary-500 — semantic info
                                 uses the brand primary; this is
                                 deliberate and documented).

Neutral palette:
  color-gray-50  → #F9FAFB
  color-gray-100 → #F3F4F6
  color-gray-200 → #E5E7EB
  color-gray-500 → #6B7280   (default body text on white background)
  color-gray-700 → #374151
  color-gray-900 → #111827   (high-contrast headings)

Use-case rules:
  Body text: color-gray-500 minimum on white (4.5:1 contrast).
  Primary action: color-primary-500 background, white foreground (5.7:1).
  Error message text: color-error-500 only on white background; on tinted
   backgrounds, use color-error-700 (#B91C1C) for sufficient contrast.
  Forbidden: any color outside the token list. No bespoke hex values
   per screen.

Contrast-ratio constraint:
  Every color pair specified for text must meet WCAG AA (4.5:1 for
  normal text, 3:1 for large text). The accessibility review (1.4)
  validates real combinations.

REJECTED — 1.3 Interaction and Navigation Model:
Define how colors change in response to user interaction. On hover, the
primary button transitions from blue-500 to blue-600 over 150ms with an
ease-out curve. On press, it transitions to blue-700 instantly (no easing).
On focus (keyboard), an outline appears at blue-300 width 3px. On disabled,
the button uses gray-200 background and gray-500 text. Error states pulse
red-500 to red-600 once on validation failure to draw attention.
[WRONG: "define the color tokens" asks for the palette and the use-case
rules — the static definitions every team consumes. Interaction transitions
between states are 1.3's responsibility (how the system reacts to user
actions). The token table is the prerequisite; transitions reference token
names. Returning interaction-state choreography when token definitions were
asked for collapses two distinct sub-phases into one and leaves the
palette undefined.]
```

---

## Pair AP4-1.1-003 — component primitive specification confused with layout

```
source_id: xiigen-ui-batch-004-AP4-1.1-003
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.1
rejected_sub_phase: 1.2
```

Prompt: `"specify the button component primitive — variants, sizes, spacing, and the visual rules every button must follow"`

```
CHOSEN — 1.1 Design System and Tokens:
The Button primitive token specification. Variants and sizes are tokens;
visual rules are constraints that every consumer must obey.

Variants (style):
  primary       → color-primary-500 background, white text, no border.
  secondary     → white background, color-gray-300 border, gray-700 text.
  tertiary      → transparent background, no border, color-primary-600 text.
  destructive   → color-error-500 background, white text, no border.
  Forbidden: any variant outside this list. No "warning button". No
   "cta button".

Sizes (padding + height + text token):
  sm  → height 32 px, padding-x 12 px, text-sm    weight-medium.
  md  → height 40 px, padding-x 16 px, text-base  weight-medium.
  lg  → height 48 px, padding-x 24 px, text-base  weight-semibold.
  Forbidden: custom heights between sizes. Use the closest size token.

Visual rules:
  Border radius: button-radius token = 6 px. No exceptions.
  Icon spacing: when a button has both icon and text, the gap is 8 px.
  Icon-only button: square aspect ratio, padding-x equals padding-y.
  Disabled button: 50% opacity. Apply at the css opacity layer; do not
   create a separate disabled color token.

Forbidden combinations:
  Primary + sm — primary actions are not 32-px tall; they are at least
   md. Reasoning: primary actions are decisions that benefit from
   easy clicking.
  Destructive + tertiary style — destructive actions must use the
   filled variant so the user does not miss them.

Affordance rule:
  Every Button must have an accessible label even when icon-only (the
  aria-label attribute). The accessibility review (1.4) validates this
  in real components.

REJECTED — 1.2 Layout and Responsive Structure:
Specify how buttons sit on a page. Primary CTAs sit in the bottom-right
of dialog footers and at the bottom of forms, after the last input.
Secondary buttons sit to the left of primary buttons in dialog footers.
On mobile, all buttons in a button group stretch to full width and stack
vertically; on desktop, they sit in a row with 12px gap. In navigation
bars, buttons sit on the right side of the bar. In page headers, buttons
sit aligned to the right edge of the page container.
[WRONG: "specify the button primitive — variants, sizes, spacing, and
visual rules" asks for the component-token specification: what the button
looks like in each variant and size, and what is forbidden. Layout choices
about where buttons sit on a page are 1.2's responsibility — they apply
the primitive but they do not define it. The primitive must be specified
first; layout decisions can then reference button-md, button-primary, etc.,
without reinventing them per surface.]
```

---

## Pair AP4-1.1-004 — token specification confused with frontend implementation

```
source_id: xiigen-ui-batch-004-AP4-1.1-004
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.1
rejected_sub_phase: 4.2
```

Prompt: `"define the spacing token system — base unit, scale, and the rules for which token to use where"`

```
CHOSEN — 1.1 Design System and Tokens:
A spacing scale built on a 4-pixel base. Every token references multiples
of the base. Bespoke pixel values are forbidden.

Base unit:
  space-base = 4 px. All tokens are multiples.

Scale:
  space-0   → 0
  space-1   → 4 px
  space-2   → 8 px
  space-3   → 12 px
  space-4   → 16 px
  space-5   → 20 px
  space-6   → 24 px
  space-8   → 32 px
  space-10  → 40 px
  space-12  → 48 px
  space-16  → 64 px
  space-20  → 80 px

Use-case rules:
  Component internal padding: space-2 to space-4.
  Component-to-component spacing within a section: space-4 to space-6.
  Section-to-section spacing within a page: space-8 to space-12.
  Page-edge padding: space-4 on mobile, space-8 on tablet, space-12 on
   desktop (the only place where the token varies by surface).

Forbidden:
  Custom values between tokens. If a designer feels the gap should be
  18 px, use space-4 (16 px) or space-5 (20 px); do not introduce a
  bespoke 18-px value.
  Padding/margin values outside the scale are a code-review block.

The scale is the contract. Layout rules (1.2) reference the tokens by
name; layout never specifies pixel values directly.

REJECTED — 4.2 Frontend and Client Integration:
Implement the spacing system in CSS. Define CSS custom properties
--space-1 through --space-20 on :root. Add Tailwind config entries
mapping spacing.1 through spacing.20 to the same values. Update the
existing components to replace hardcoded margin and padding values with
the new tokens. Add a Stylelint rule that fails the build on any
hardcoded pixel value in margin or padding properties. Document the
migration in the team wiki.
[WRONG: "define the spacing token system" asks for the design-system
specification — the base unit, the scale, the use-case rules. Implementing
the system in CSS, Tailwind, and Stylelint is the next sub-phase (4.2)
where the design tokens are turned into runtime values. The token
definition must exist before the implementation can begin; the rejected
response presupposes the tokens and goes straight to the wiring,
collapsing two sub-phases.]
```
