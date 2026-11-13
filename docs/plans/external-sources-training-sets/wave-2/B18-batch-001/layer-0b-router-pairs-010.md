# Layer 0-B — Router Pairs (010)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: router

## B18-0B-010-001 — [IMPLEMENT, ARCHITECTURE, SECURITY, TEST, DOCUMENT] → 5 routing decisions

**Scenario:** GdprUserDataMigration (GDPR-compliant user data migration)

**Router input:**
- predicates: ["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]
- TransitionType: feature_addition
- Request: "Write the user data migration to the GDPR-compliant encrypted structure, design the target schema architecture, write migration tests, and document the data transformation rules."

**Routing logic:**

1. **IMPLEMENT** (feature_addition, 4.4 signals: write migration, user data, encrypted, database)
   → DataMigrationFlow44, sub-phase 4.4

2. **ARCHITECTURE** ("structure", "design", "schema architecture" vocabulary)
   → ArchitectureFlow31, sub-phase 3.1

3. **SECURITY** ("GDPR", "encrypted" vocabulary)
   → SecurityReviewFlow (security review sub-phase)

4. **TEST** ("write migration tests" vocabulary)
   → UnitTestingFlow51, sub-phase 5.x

5. **DOCUMENT** ("document", "transformation rules" vocabulary)
   → DocumentationFlow21, sub-phase 2.1

**Router output:**
- Flows: [DataMigrationFlow44, ArchitectureFlow31, SecurityReviewFlow, UnitTestingFlow51, DocumentationFlow21]
- Sub-phases: [4.4, 3.1, security, 5.x, 2.1]

---

## B18-0B-010-002 — Execution order for five-flow routing

**Ordered execution for GdprUserDataMigration:**

1. **ArchitectureFlow31** (3.1) — design target schema first
2. **DataMigrationFlow44** (4.4) — write migration (uses schema design)
3. **SecurityReviewFlow** — review encrypted implementation
4. **UnitTestingFlow51** (5.x) — test migration after it exists
5. **DocumentationFlow21** (2.1) — document after implementation complete

**Parallel opportunities:**
- ArchitectureFlow31 can start independently (schema design)
- DocumentationFlow21 can start for pre-migration documentation (field mapping docs)
- DataMigrationFlow44 and SecurityReviewFlow may overlap (security review during implementation)

---

## B18-0B-010-003 — SECURITY routing in B18

**SECURITY predicate routing:**
- SECURITY fires on: "GDPR", "encrypted", "encryption", "PII", "compliance", "security" (data context)
- Routes to: SecurityReviewFlow
- Does NOT suppress or replace DataMigrationFlow44

**SECURITY + IMPLEMENT routing:**
- Both flows are assigned independently
- DataMigrationFlow44 handles the implementation
- SecurityReviewFlow handles security review/verification
- They may run concurrently or SecurityReviewFlow follows DataMigrationFlow44

---

## B18-0B-010-004 — Five-predicate router trace full output

**ConnectorName:** GdprUserDataMigration

**Complete routing output:**
```
{
  "connector_name": "GdprUserDataMigration",
  "transition_type": "feature_addition",
  "predicate_routing": [
    {"predicate": "IMPLEMENT", "flow": "DataMigrationFlow44", "sub_phase": "4.4"},
    {"predicate": "ARCHITECTURE", "flow": "ArchitectureFlow31", "sub_phase": "3.1"},
    {"predicate": "SECURITY", "flow": "SecurityReviewFlow", "sub_phase": "security"},
    {"predicate": "TEST", "flow": "UnitTestingFlow51", "sub_phase": "5.x"},
    {"predicate": "DOCUMENT", "flow": "DocumentationFlow21", "sub_phase": "2.1"}
  ],
  "execution_order": [
    "ArchitectureFlow31",
    "DataMigrationFlow44",
    "SecurityReviewFlow",
    "UnitTestingFlow51",
    "DocumentationFlow21"
  ]
}
```

---

## B18-0B-010-005 — SECURITY does not suppress IMPLEMENT routing

**Correct behavior:**
- GDPR migration → IMPLEMENT routes to DataMigrationFlow44 (4.4) AND SECURITY routes to SecurityReviewFlow
- Both flows exist independently

**Wrong behavior (what v6 might do):**
- GDPR migration → SECURITY routes to SecurityReviewFlow only; DataMigrationFlow44 suppressed
- Or: SECURITY causes IMPLEMENT to route to a "security-hardened" 4.4 flow variant

**Rule:** Each predicate routes independently. No predicate suppresses another predicate's flow.

---

## B18-0B-010-006 — QA flow: not present in GdprUserDataMigration

**QA flow in B18:**
- QA fires only on "QA" abbreviation (Quality Assurance)
- NOT present in the GDPR migration scenario (no "QA" vocabulary)
- TEST (UnitTestingFlow51) is present but QA is a separate flow

**The five predicates in GdprUserDataMigration:**
["IMPLEMENT", "ARCHITECTURE", "SECURITY", "TEST", "DOCUMENT"]

**NOT included (absent vocabulary):**
- QA — no "QA" or "quality assurance" in request
- GapHandler — B18 has no GapHandler at all

**Result:** five flows, not six or seven.
