# Layer 0-A — Compiler Pairs (034)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: compiler

---

## B34-compiler-034-001 — "brief" + security context → gap (not SECURITY)

A: "Brief the development team on our security protocols and patching schedule."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: "brief on" = information/knowledge-sharing vocabulary → gap. "security protocols" = knowledge topic being communicated, not a security action being performed.

---

## B34-compiler-034-002 — "present" → gap even with implementation topic

A: "Present the refactored payment module to the engineering leadership for sign-off."
B: Predicates: [] | Gap: gap-skills-guidance-train-001
Evidence: "present to [audience]" = communication/teaching vocabulary → gap. No code action; presenting for review = knowledge transfer.

---

## B34-compiler-034-003 — "familiarize with" + API topic → onboard gap

A: "Familiarize the new backend engineers with our REST (Representational State Transfer) API conventions."
B: Predicates: [] | Gap: gap-skills-guidance-onboard-001
Evidence: "familiarize with" + new engineers + conventions/standards = onboarding vocabulary → gap-skills-guidance-onboard-001.

---

## B34-compiler-034-004 — guidance + UI: show → gap, implement → IMPLEMENT

A: "Show the mobile team how the design tokens work and implement the dark mode toggle feature."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: "show how [tokens] work" = gap warning (gap-skills-guidance-train-001); "implement the dark mode toggle feature" = requester's direct code action → IMPLEMENT fires.

---

## B34-compiler-034-005 — multiple guidance verbs, no code verbs → single gap

A: "Brief, discuss, and run through the containerization strategy with the DevOps team."
B: Predicates: [] | Gap: gap-skills-guidance-mentor-001
Evidence: brief, discuss, run through = all guidance vocabulary → single gap (most-specific: gap-skills-guidance-mentor-001). Multiple guidance verbs do not multiply gap entries. DomainId: software-development.
