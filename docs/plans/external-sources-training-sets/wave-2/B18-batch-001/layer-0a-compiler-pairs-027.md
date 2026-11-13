# Layer 0-A — Compiler Pairs (027)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: compiler

## B18-0A-027-001 — PartitioningTable: predicate firing

**Request:** "Migrate the orders table to a partitioned table structure based on the created_at year."

**Token scan:**
- "migrate" → IMPLEMENT fires (Rule 5: data vocabulary)
- "orders table" → 4.4 vocabulary
- "partitioned table structure" → "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "based on the created_at year" → 4.4 DDL migration vocabulary
- No refactoring verbs → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-027-002 — PartitioningTable: "restructure" disambiguation

**Request:** "Restructure the products table into monthly partitions for better query performance."

**Token scan:**
- "restructure" → IMPLEMENT fires; restructure is a refactoring verb → TransitionType: refactoring (Rule 1)
- "products table" → 4.4 vocabulary
- "monthly partitions" → 4.4 DDL vocabulary
- No ARCHITECTURE token (no "structure", "architecture", "pattern")
- TransitionType: refactoring (restructure wins)

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: refactoring
- sub_phase: 4.4
- DomainId: software-development

---

## B18-0A-027-003 — PartitioningTable with IMPLEMENT + ARCHITECTURE

**Request:** "Design the partition pattern for the events table and create the partitioned table migration."

**Token scan:**
- "design" → ARCHITECTURE fires (Rule 11)
- "partition pattern" → "pattern" → ARCHITECTURE fires (Rule 7, absolute)
- "create the partitioned table migration" → IMPLEMENT fires (Rule 2: migration artifact)
- "events table" → 4.4 vocabulary
- No refactoring verbs in primary action → TransitionType: feature_addition

**Output:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 3.1
- DomainId: software-development

---

## B18-0A-027-004 — PartitioningTable with DOCUMENT

**Request:** "Create the table partitioning migration for the billing records and document the partitioning strategy."

**Token scan:**
- "create the table partitioning migration" → IMPLEMENT fires (Rule 2)
- "billing records" → 4.4 data vocabulary
- "document" → DOCUMENT fires (Rule 4)
- "partitioning strategy" → documentation target; no "structure" or "pattern" token → ARCHITECTURE does not fire
- No security tokens

**Output:**
- predicates: ["IMPLEMENT", "DOCUMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4 + 2.1
- DomainId: software-development

---

## B18-0A-027-005 — PartitioningTable: "partition" alone does not fire ARCHITECTURE

**Request:** "Add range partitioning to the audit_log table based on the event_date column."

**Token scan:**
- "add range partitioning" → IMPLEMENT fires (new partition structure is a new artifact)
- "audit_log table" → 4.4 data vocabulary
- "event_date column" → 4.4 column vocabulary
- "range partitioning" — "partitioning" alone does NOT fire ARCHITECTURE; "partition" is not an absolute trigger
- No "structure", "architecture", "pattern" → ARCHITECTURE does not fire

**Key rule:** "partitioning" vocabulary routes to 4.4 (DDL migration) but does NOT fire ARCHITECTURE unless "structure", "architecture", or "pattern" is also present.

**Output:**
- predicates: ["IMPLEMENT"]
- TransitionType: feature_addition
- sub_phase: 4.4
- DomainId: software-development
