# Layer 0-A — Compiler Pairs (028)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-028-001 — MaterializedView: predicate firing

**Request:** "Create a materialized view for the monthly sales summary aggregated from the orders and line_items tables."

**Token scan:**
- "create a materialized view" → IMPLEMENT fires (Rule 2: new artifact — materialized view is a new database object)
- "orders and line_items tables" → 4.4 data vocabulary
- "monthly sales summary" → 4.4 aggregation vocabulary
- No "structure", "architecture" → ARCHITECTURE does not fire
- No security tokens, no TEST verb

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-028-002 — MaterializedView with ARCHITECTURE

**Request:** "Design the materialized view architecture for the reporting layer before creating the view migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "materialized view architecture" → "architecture" → ARCHITECTURE fires (Rule 7, absolute)
- "creating the view migration" → IMPLEMENT fires (Rule 2: migration artifact)
- "reporting layer" → 4.4 data layer vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-028-003 — MaterializedView: "view" alone does not fire special predicates

**Request:** "Build the product inventory view to support real-time reporting queries."

**Token scan:**
- "build" (data artifact target) → IMPLEMENT fires; "build" with database object = feature_addition + 4.4
- "product inventory view" → 4.4 data vocabulary
- "real-time reporting queries" → 4.4 query vocabulary
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire
- "view" alone is not an ARCHITECTURE trigger

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-028-004 — MaterializedView with DOCUMENT

**Request:** "Create the user activity summary view and document how the refresh schedule works."

**Token scan:**
- "create the user activity summary view" → IMPLEMENT fires (new artifact)
- "user activity" → 4.4 data vocabulary
- "document" → DOCUMENT fires (Rule 4)
- "refresh schedule" → documentation subject; does not independently fire any additional predicate
- No ARCHITECTURE or SECURITY tokens

**Output:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 2.1
- DomainId: software-development

---

## B18-0A-028-005 — MaterializedView with SECURITY

**Request:** "Create a materialized view of customer PII fields for the compliance reporting dashboard."

**Token scan:**
- "create a materialized view" → IMPLEMENT fires (Rule 2)
- "customer PII fields" → "PII" → SECURITY fires (Rule 14, absolute)
- "compliance reporting" → "compliance" → SECURITY fires (Rule 14, absolute; reinforcement)
- "dashboard" → 4.4 reporting vocabulary
- SECURITY fires twice → single SECURITY predicate

**Output:**
- predicates: ["IMPLEMENT", "SECURITY"]
- TransitionType: feature_addition
- sub_phase: 4.4 + security
- DomainId: software-development
