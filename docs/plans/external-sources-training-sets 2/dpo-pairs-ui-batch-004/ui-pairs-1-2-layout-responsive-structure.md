# UI Pairs — Sub-phase 1.2 Layout and Responsive Structure
# Batch: dpo-pairs-ui-batch-004
# Production date: 2026-05-08
# Authority: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md
# Sub-phase scope: page layout, responsive behavior, density, information structure
# Pair count: 4

---

## Pair AP4-1.2-001 — page layout decision confused with token specification

```
source_id: xiigen-ui-batch-004-AP4-1.2-001
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.2
rejected_sub_phase: 1.1
```

Prompt: `"design the layout for the dashboard page — sidebar, main content, and how they reflow on different screen sizes"`

```
CHOSEN — 1.2 Layout and Responsive Structure:
A three-region layout that reflows by breakpoint. The structure is the same
across breakpoints; the spatial arrangement changes.

Regions:
  Region A — Sidebar navigation (the persistent navigation rail).
  Region B — Page header (page title, breadcrumb, primary action).
  Region C — Main content (cards, tables, charts).

Breakpoints (mobile-first):

XS (< 640 px):
  Region A is hidden. A hamburger button reveals it as an overlay.
  Region B occupies the full top of the viewport.
  Region C occupies the rest, single-column.
  Cards stack vertically, full width.

SM (≥ 640 px):
  Same as XS. Cards may go to a 2-up layout when each card is wider
  than 240 px after wrapping.

MD (≥ 768 px):
  Region A becomes a permanent rail at 240 px wide. The rail is
  collapsible to 64 px on user click; collapsed state persists in
  local storage.
  Region B and C share the remaining width.
  Card grid becomes 2 columns by default.

LG (≥ 1024 px):
  Region A is permanent at 240 px.
  Card grid becomes 3 columns by default. Cards in a 4-column row
  are permitted only when the card content is dense enough; default
  is 3.

XL (≥ 1280 px):
  Page max-width caps at 1280 px and centres on the viewport.
  Beyond that, side margins absorb extra width to maintain readable
  line lengths.

Reflow rules:
  Tables in Region C reflow to card-per-row on XS / SM. The reflow is
  triggered at the table component level, not by the layout container.
  Charts in Region C maintain a 16:9 aspect ratio at all breakpoints,
  scaling width with the available column.

Information density:
  XS / SM use the comfortable density variant of components.
  MD and above default to the default density. Power-user views can
  switch to the compact density variant via a user preference.

REJECTED — 1.1 Design System and Tokens:
Define the spacing tokens used by the dashboard layout: container-padding
(16 px on mobile, 24 px on tablet, 32 px on desktop), gap-rows (24 px),
gap-cards (16 px on mobile, 24 px on desktop). Define the breakpoint
tokens: bp-sm (640), bp-md (768), bp-lg (1024), bp-xl (1280). Define
the sidebar-width token at 240 px. Add these to the design-tokens file
and the Tailwind configuration.
[WRONG: "design the layout — sidebar, main content, how they reflow" asks
for the spatial structure of the page across breakpoints. The token
definitions are 1.1's responsibility and are prerequisites; the layout
decision is which token applies where and how the regions reflow.
Returning a token list when a layout was asked for skips the spatial
structure entirely — the consumer of the layout decision needs to know
how the page reflows, not just which spacing values are available.]
```

---

## Pair AP4-1.2-002 — responsive structure decision confused with token specification

```
source_id: xiigen-ui-batch-004-AP4-1.2-002
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.2
rejected_sub_phase: 1.1
```

Prompt: `"design the layout of the data table on small screens — the table has 8 columns and most of them are too narrow on mobile"`

```
CHOSEN — 1.2 Layout and Responsive Structure:
A reflow strategy for the 8-column table at three breakpoint tiers. The
table reflows at the data level, not by horizontal scrolling.

Decision: tabular layout on desktop and tablet; card layout on mobile.

Tier 1 (desktop, ≥ 1024 px):
  Full 8-column table. Column priorities: 1, 2, 4, 6 are always visible;
  3, 5, 7, 8 are hidden under a "show all columns" toggle when the
  user prefers density.
  Sticky first column for horizontal scroll within the visible 8.

Tier 2 (tablet, 768 - 1023 px):
  Show columns 1, 2, 3, 4, 5 by default. Columns 6, 7, 8 are accessible
  via an expand-row interaction that reveals them inline.
  No horizontal scroll. The expand-row interaction lives on the leftmost
  cell of each row.

Tier 3 (mobile, < 768 px):
  Drop the table layout entirely. Each row becomes a card with:
   • Column 1 (the row identifier) as the card title.
   • Column 2 (the most actionable field) as the card subtitle.
   • Columns 3-5 as a 2-column grid of label/value pairs in the card body.
   • Columns 6-8 hidden under an expand-card interaction at the card foot.
  No horizontal scroll on the card layout.

Anti-pattern (forbidden):
  Horizontal scroll on the desktop / tablet tier — users cannot see
  what they cannot scroll to. The table reflows to fewer columns instead.
  Horizontal scroll on mobile — the same reasoning, plus the touch
  ergonomics are worse on small screens.

Density variants:
  All three tiers respect the user's density preference (comfortable /
  default / compact). Density changes the row height by adjusting the
  cell padding, not the column count.

Bookmark / link:
  When a user lands on a deep-linked filtered view, the layout tier
  derives from the viewport at landing; the deep-link state survives
  across reflows because it is stored in URL parameters not in layout
  state.

REJECTED — 1.1 Design System and Tokens:
Define the table-component tokens. Table-cell padding tokens: padding-y
of 12 px (default density), 8 px (compact), 16 px (comfortable). Padding-x
of 16 px across densities. Table border tokens: 1 px solid color-gray-200
between rows, none between columns. Table header tokens: text-sm,
weight-semibold, color-gray-700, background color-gray-50. Add to the
design-tokens file.
[WRONG: "design the layout of the data table on small screens" asks for
the responsive reflow strategy — what columns are shown, what is hidden,
and what alternative layout (cards) replaces the table on mobile. Token
definitions for cell padding and border colors are 1.1's responsibility
and apply across breakpoints; they do not answer the reflow question.
The user with 8 columns on a 360 px screen needs to know what happens
to their data, not what the row padding is.]
```

---

## Pair AP4-1.2-003 — page structure decision confused with interaction model

```
source_id: xiigen-ui-batch-004-AP4-1.2-003
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.2
rejected_sub_phase: 1.3
```

Prompt: `"structure the multi-step wizard's screens — what is on each step, how they relate, and how the user knows where they are"`

```
CHOSEN — 1.2 Layout and Responsive Structure:
The wizard is one logical page with five steps. Each step is a panel
within a single layout that always shows the user where they are.

Layout regions:

Region A — Progress indicator (top, persistent across all steps):
  Five numbered dots, one per step. The current step is filled and
  enlarged; completed steps show a checkmark; future steps are outlined.
  Below the dots, a single sentence names the current step.
  On mobile, the dots become a "Step 2 of 5" text label with a thin
  progress bar; the sentence remains.

Region B — Current step content (centre, swaps content per step):
  Each step's content sits in a card with consistent max-width 640 px
  desktop, full-width on mobile. The card has the same padding across
  steps. The transition between steps is a 200 ms slide; no fade.
  Required-fields summary appears below the form fields when present.

Region C — Action bar (bottom, persistent):
  On the left: a Back button (disabled on step 1). On the right: a
  Continue button (becomes Finish on step 5).
  An optional Skip link sits between Back and Continue when the
  current step is optional.
  On mobile, the action bar is fixed to the bottom of the viewport.

Step content structure:
  Step 1 — Company profile (4 inputs).
  Step 2 — Team invites (a list of user-input rows; user can add or
            remove rows).
  Step 3 — Integration setup (optional; can skip).
  Step 4 — Branding (optional; can skip).
  Step 5 — Review (read-only summary of all prior steps; Finish creates
            the workspace).

Persistence:
  When the user completes a step, the entered data is persisted to the
  server. If the user closes the browser and returns, they are dropped
  back at the latest incomplete step with previous data filled.

Information density:
  Each step contains at most 6 input fields. Steps with more fields are
  split into two steps to keep cognitive load manageable.

REJECTED — 1.3 Interaction and Navigation Model:
Define how the user moves between wizard steps. On Continue, validate
the current step's required fields; if any fail, show inline errors and
stay on the step. If validation passes, animate the slide and load
the next step's content. On Back, restore the previous step's content
without re-validating. If the user reloads the page, restore the most
recent step from saved state. Pressing Esc on any step prompts "Discard
your progress?" with Cancel and Discard buttons.
[WRONG: "structure the wizard's screens — what is on each step, how
they relate, how the user knows where they are" asks for the layout and
information structure: what regions exist on the page, what content
sits in each step, how the user perceives progress. Interaction
choreography (validation flow, animation, escape-key behaviour) is
1.3's responsibility and follows the structure decision. Returning
the interaction model when the structure was asked for leaves the
content layout undefined.]
```

---

## Pair AP4-1.2-004 — layout structure confused with frontend implementation

```
source_id: xiigen-ui-batch-004-AP4-1.2-004
source_license_state: first_party
pair_kind: dpo_ui
pair_type: Type-A-adjacent-sub-phase
chosen_sub_phase: 1.2
rejected_sub_phase: 4.2
```

Prompt: `"design the layout for the search-results page including filters, results, pagination, and how they fit on tablet and mobile"`

```
CHOSEN — 1.2 Layout and Responsive Structure:
A four-region layout that reflows from desktop to mobile while keeping
the same logical structure.

Regions:
  Region A — Filter sidebar.
  Region B — Search header with the query, the result count, and the
              sort control.
  Region C — Result list.
  Region D — Pagination control.

Desktop (≥ 1024 px):
  A on the left at 280 px wide, persistent.
  B spans the top of the right column.
  C fills the right column under B; result rows full-width within
   the column.
  D sits at the bottom of the right column, sticky to the viewport
   bottom only when the result list is taller than the viewport.

Tablet (640 - 1023 px):
  A becomes a collapsible drawer above the results, opened by a Filters
   button in B.
  When A is open, it pushes B and C down (does not overlay).
  C and D stack vertically below B as on desktop, but full viewport
   width.

Mobile (< 640 px):
  A becomes a full-screen overlay opened by the Filters button in B.
  When A is open, the result list is not visible; it returns when A
   is closed (Apply or Cancel).
  C reflows: each result row reduces its rightmost preview elements
   first; long product names truncate at 2 lines.
  D becomes infinite scroll instead of paginated buttons (a deliberate
   choice for mobile to reduce thumb travel; a "Load more" fallback
   appears if the user scrolls fast enough that auto-load lags).

Empty state:
  When there are zero results, C shows a centered empty state with the
  search query echoed and three suggested searches. A and B remain
  visible at all breakpoints.

Information density:
  Result rows on desktop show: image, title, price, rating, store,
   estimated delivery.
  On tablet: drop estimated delivery (move it into the detail page).
  On mobile: drop estimated delivery and store name; both visible on
   tap.

REJECTED — 4.2 Frontend and Client Integration:
Implement the search-results layout in React. Use a SearchResultsPage
component that renders FilterSidebar, SearchHeader, ResultsList, and
Pagination as children. Use Tailwind grid-template-columns for desktop:
"280px 1fr". Use Tailwind's lg: prefix for the desktop breakpoint.
On mobile, render FilterSidebar as a Dialog from Radix UI. Wire the
filter state to a Zustand store. Hook the pagination component to a
useSearchParams hook for URL-driven state.
[WRONG: "design the layout" asks for the spatial decision — where each
region sits, how it reflows by breakpoint, how content density adapts.
Implementing the layout in React with Tailwind, Radix, and Zustand is
the frontend integration sub-phase (4.2) — it follows the layout
decision. Returning React component code when a layout decision was
asked for skips the design step that the implementation depends on.]
```
