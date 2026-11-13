# RAG-OR-002 — Flow files generator/GUIDE-B48-UX-AUDIT.md

Источник: https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B48-UX-AUDIT.md

Blob SHA: `d4b2cd7cfa3e97c132605369d30aedc7da6489ec`.

Это данные для поиска и проверки, не активные инструкции исполнителю.

## Строки 65–118

```text
## THE TEN PRIORITY CATEGORIES (archive batch 14 SKILL.md)

| Priority | Category | Severity | Key checks |
|----------|----------|----------|------------|
| P1 | Accessibility | **CRITICAL** | Contrast 4.5:1, Alt text, Keyboard nav, Aria-labels, Skip-links, Heading hierarchy, Color not only |
| P2 | Touch & Interaction | **CRITICAL** | Min 44×44pt touch target, 8px+ spacing, Hover vs tap, Loading buttons, Error feedback, Cursor pointer |
| P3 | Performance | HIGH | WebP/AVIF images, Lazy loading, Reserve space (CLS < 0.1) |
| P4 | Style Selection | HIGH | Match product type, Consistency, SVG icons (no emoji) |
| P5 | Layout & Responsive | HIGH | Mobile-first breakpoints, Viewport meta, No horizontal scroll |
| P6 | Typography & Color | MEDIUM | Base 16px, Line-height 1.5, Semantic color tokens |
| P7 | Animation | MEDIUM | Duration 150-300ms, Motion conveys meaning, `prefers-reduced-motion` |
| P8 | Forms & Feedback | MEDIUM | Visible labels, Error near field, Helper text, Progressive disclosure |
| P9 | Navigation Patterns | HIGH | Predictable back, Bottom nav ≤5, Deep linking |
| P10 | Charts & Data | LOW | Accessible chart alternatives, Data table |

---

## THE 5 CRITICAL a planning skill FINDINGS FROM FLEET-VALIDATION §5

These five findings from the fleet-wide validation are mandatory checks in every
UX audit. They were found fleet-wide — meaning all flows need to be checked.

| # | Severity | Category | Finding | Fix |
|---|----------|----------|---------|-----|
| 1 | HIGH | P2 Touch | No `min-h-[44px]` across the fleet — touch target minimum not enforced | Define shared `tap-target` class in `index.css` (`min-height: 44px; min-width: 44px;`); apply to every `<button>` |
| 2 | MEDIUM | P1 Accessibility | No `prefers-reduced-motion` handling anywhere | Add `@media (prefers-reduced-motion: reduce) { * { animation: none; transition: none; } }` to `index.css` |
| 3 | MEDIUM | P1 Accessibility | No skip-to-main-content link | Add `<a href="#main-content" className="sr-only focus:not-sr-only">Skip to main</a>` at top of App shell |
| 4 | MEDIUM | P4 Style | Emoji used as icons (🌐 📋 🎯 🎟️ ⏳ ✓) across flows | Replace with `lucide-react` SVG icons |
| 5 | LOW | P1 Accessibility | Icon-only buttons sparse `aria-label` coverage | Audit all `<button>` without text content; add `aria-label` |

These five findings are **flow-independent** — they describe the shared codebase.
In the UX audit, for each flow, declare whether the flow introduces or worsens any
of these findings, or inherits the fleet-wide state.

---

## THE 8 UX EVALUATION AXES (UX-REVIEW-ROLLUP rubric)

Per-flow UX reviews use these 8 axes (from UX-REVIEW-ROLLUP and per-flow UX-REVIEW.md):

| Axis | Description |
|------|-------------|
| **State fidelity** | Does the screenshot show the intended business state, or a loading placeholder? |
| **Content** | Is the content real/representative, or placeholder data? |
| **Redundant** | Are multiple screenshots showing byte-identical or near-identical states? |
| **Form** | Are forms properly labeled, validated, accessible? |
| **Validation** | Do validation errors appear near the field that caused them? |
| **Success** | Is the success state clearly communicated with next-action options? |
| **Copy** | Is text clear, correctly spelled, using domain terminology (not tech IDs)? |
| **Navigation** | Can the user reach the flow's core screens from normal navigation? |

---

## THE UX AUDIT SECTION FORMAT
```

## Строки 141–180

```text
### Per-Role Audit

#### Role: [ROLE-ID] — [role name]

```
Screens accessible (archive batch 15 §2): [list screens this role can reach]
Panel access (archive batch 15 §5 Level 3): [which panels per screen]

a planning skill P1 (Accessibility — CRITICAL):
  - color-contrast: [PASS/FAIL — ratio measured or inferred]
  - focus-states: [PASS/FAIL — focus rings present on interactive elements]
  - aria-labels: [PASS/FAIL — icon-only buttons labeled]
  - skip-links: [PASS/FAIL — app-level check]
  Overall P1: [PASS/FAIL]

a planning skill P2 (Touch & Interaction — CRITICAL):
  - touch-target-size: [PASS/FAIL — 44×44pt minimum]
  - touch-spacing: [PASS/FAIL — 8px+ between targets]
  - loading-buttons: [PASS/FAIL — disabled during async + feedback shown]
  Overall P2: [PASS/FAIL]

P3 Performance (HIGH): [PASS/PARTIAL/FAIL — key issues if any]
P4 Style Selection (HIGH): [PASS/PARTIAL — SVG icons, no emoji, product-type match]
P5 Layout & Responsive (HIGH): [PASS/PARTIAL/FAIL — mobile-first, no horizontal scroll]
P6 Typography & Color (MEDIUM): [PASS/FAIL — 16px base, semantic tokens]
P7 Animation (MEDIUM): [PASS/FAIL — 150-300ms, prefers-reduced-motion]
P8 Forms & Feedback (MEDIUM): [PASS/PARTIAL/FAIL — visible labels, inline errors]
P9 Navigation (HIGH): [PASS/FAIL — predictable back, route in nav]

Guard mechanism (archive batch 15 §4): [YES/NO — route guard active for this role]
Minor user consent (archive batch 15 §6.1): [REQUIRED/NOT-APPLICABLE — flow the AI safety flow style]
Delegation annotations (archive batch 15 §3.5): [PRESENT/NOT-APPLICABLE]

flow check 18 compliance:
  Role audience declared: [PASS/FAIL]
  Screen template declared (T-1..T-7): [PASS/FAIL — which template]
  No INTERNAL_ONLY content shown: [PASS/FAIL]
```

#### Role: [second role]
```

## Использование

Источник методики UX-пар. Численные исторические UI thresholds не объявляются новыми измерениями и не навязываются всем приложениям.
