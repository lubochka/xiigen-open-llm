# 01 - UML Frame

## Active Nodes

1. Domain splitter: receives a human prompt and returns domain labels.
2. Domain translator array: receives one fragment and one domain label, then writes domain wording.
3. Scheme planner: receives all domain wordings and returns ordered work, parallel work, recovery paths, and reviews.
4. Domain processor array: processes one planned piece.
5. Applier array: turns one processed piece into action and outcome.
6. Result translator array: translates one formal result while seeing all results.
7. Arbiter selector: chooses review units from formal results.
8. Arbiter units: pass, flag, or send a fragment back for reprocessing.
9. Formatter: combines approved translated fragments.

## Array Nodes

Four nodes are arrays:

- Node 02 runs once per domain.
- Node 04 runs once per planned work piece.
- Node 05 runs once per planned work piece.
- Node 06 runs once per formal result.

This matters for Flow-10 because a CMS-commerce platform is not one linear flow. A runtime prompt may need public storefront, admin, identity, catalog, content, cart, checkout, order lifecycle, search, notifications, analytics, performance, and app extension work in parallel.

## Main Edges

### Edge 1 - Prompt to Domain Splitter

What moves: a human request about building or mapping a CMS, shop, storefront, admin, checkout, plugin, or design-to-commerce experience.

What the target receives: the raw prompt.

What the target produces: all module and flow domains the prompt touches.

What breaks if missing: commerce, content, back-office, and platform modules may be mixed or skipped.

### Edge 2 - Domain Splitter to Domain Translator Array

What moves: one domain label and matching prompt fragment.

What the target receives: one domain-shaped fragment.

What the target produces: precise domain wording.

What breaks if missing: planning may confuse public pages with admin actions, or product data with order lifecycle.

### Edge 3 - Domain Translator Array to Scheme Planner

What moves: all translated domain statements.

What the target receives: the full module map.

What the target produces: a plan that separates public experience, back office, shared platform modules, commerce modules, flows, extension points, and design-generation behavior.

What breaks if missing: the result becomes a list of modules rather than a working platform plan.

### Edge 4 - Scheme Planner to Domain Processor Array

What moves: one planned module or flow piece.

What the target receives: one piece and context.

What the target produces: full processed result for that piece.

What breaks if missing: the applier cannot handle lifecycle states, cache invalidation, webhooks, checkout validation, or plugin permissions.

### Edge 5 - Domain Processor Array to Applier Array

What moves: processed module or flow result with transformation instruction.

What the target receives: action-ready result.

What the target produces: concrete module behavior, flow outcome, or architecture mapping.

What breaks if missing: the platform remains theoretical and no module behavior is defined.

### Edge 6 - Applier Array to Result Translator Array

What moves: formal results.

What the target receives: one formal result and all siblings.

What the target produces: readable wording.

What breaks if missing: the user gets internal fragments rather than a clear platform explanation.

### Edge 7 - Applier Array to Arbiter Selector

What moves: formal results before formatting.

What the target receives: all results and context.

What the target produces: selected review units.

What breaks if missing: permission, checkout, inventory, payment, cache, webhook, app-scope, and data-privacy checks may not run.

### Edge 8 - Arbiter Selector to Arbiter Units

What moves: review responsibilities.

What the target receives: one rule and the relevant result.

What the target produces: pass, concern, or reprocess instruction.

What breaks if missing: module and flow errors are not corrected.

### Edge 9 - Arbiter Units to Reprocessing

What moves: rejected fragment and reason.

What the target receives: exact fragment to repair.

What the target produces: corrected routing.

What breaks if missing: detected issues cannot be fixed.

### Edge 10 - Approved Results to Formatter

What moves: approved translated fragments.

What the target receives: reviewed fragments.

What the target produces: one final platform answer.

What breaks if missing: the answer may mix rejected, missing, or contradictory module results.

