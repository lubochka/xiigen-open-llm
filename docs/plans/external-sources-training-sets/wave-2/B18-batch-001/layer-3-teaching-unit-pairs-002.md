# Layer 3 — Teaching Unit Pairs (002)
# Batch: B18-batch-001 | Scenario: B18 — Development — refactoring and data migration
# Production date: 2026-05-05 | pair_kind: teaching-unit

## B18-TU-002-001 — Teaching: "structure" always fires ARCHITECTURE

**What the model must learn:** The token "structure" ALWAYS fires the ARCHITECTURE predicate. Rule 3 is absolute. No context can suppress it.

**Question:** Which of these requests fires ARCHITECTURE?

A. "Migrate all product records to the new normalized table structure."
B. "Refactor the OrderService to improve the class structure."
C. "Implement the new data structure for the product catalog."
D. "Write seed data that matches the normalized file structure."

**Answer:** All four (A, B, C, D). Every request contains "structure" → ARCHITECTURE fires in all four.

---

## B18-TU-002-002 — Teaching: "structure" in compound nouns

**Q: Does "structure" in "table structure" still fire ARCHITECTURE?**
A: YES. The compiler matches the token "structure" regardless of compound noun context.

**Q: Does "structure" in "data structure" still fire ARCHITECTURE?**
A: YES. The compiler is literal. "data structure" contains "structure" → ARCHITECTURE fires.

**Q: Does "file structure" fire ARCHITECTURE?**
A: YES. Same rule.

**Q: Does "normalize the structure" fire ARCHITECTURE?**
A: YES. "structure" as a noun → ARCHITECTURE fires.

**Q: Does "restructure" fire ARCHITECTURE?**
A: NO. "restructure" is a different token (compound verb). "restructure" fires IMPLEMENT (refactoring). It does NOT contain "structure" as a separate token.

---

## B18-TU-002-003 — Teaching: why "structure" fires ARCHITECTURE in migration contexts

**Explanation of why this seems counterintuitive:**

A developer might think: "table structure" is just a way of describing the database layout — it's not an architectural design noun. The word "structure" is being used descriptively, not architecturally.

**Why the compiler fires anyway:**
Rule 3 is a LITERAL rule. The compiler does not perform semantic interpretation. It applies pattern matching to tokens. "structure" → ARCHITECTURE is an unconditional pattern match.

**Why this rule exists:**
In the AdaptiveLogicalOrchestrator's domain model, "structure" signals that schema design work is needed. Even "table structure" implies that the table design (a 3.1 architectural concern) is part of the task. The literal rule avoids false negatives (missing architecture when it's needed) at the cost of occasional over-firing.

---

## B18-TU-002-004 — Teaching: "structure" → ARCHITECTURE in B18 scenarios

**Every B18 scenario where "structure" appears:**

| Scenario | Contains "structure"? | ARCHITECTURE fires? |
|---|---|---|
| "migrate to normalized table structure" | YES | YES (3.1) |
| "data structure for the catalog" | YES | YES (3.1) |
| "encrypted structure" | YES | YES (3.1) |
| "schema structure" | YES | YES (3.1) |
| "file structure" | YES | YES (3.1) |
| "refactor the service" | NO | NO |
| "seed the database" | NO | NO |
| "build ETL pipeline" | NO | NO |

---

## B18-TU-002-005 — Teaching: what sub-phase does "structure" fire?

**"structure" → ARCHITECTURE → sub-phase 3.1 (default in B18)**

**Why 3.1 not 3.5?**
- 3.1 = Application/Domain Architecture: database schema design, data model, service architecture
- 3.5 = Infrastructure Architecture: deployment, cloud, network, containers

**"Table structure" is application architecture (3.1)** because it's about the database schema design within the application domain. It's not about the deployment infrastructure (3.5).

**In B18:** ARCHITECTURE always = 3.1 unless explicit infrastructure vocabulary ("deployment architecture", "cloud infrastructure") is present.

---

## B18-TU-002-006 — Teaching: v6-01 anti-example and correction

**v6-01 anti-example:**

Request: "Migrate all product records to the new normalized table structure."

**v6-01 classification (WRONG):**
- predicates: ["IMPLEMENT"] (ARCHITECTURE missing)
- reasoning: "table structure" is describing the database, not defining an architecture

**Correction:**
- predicates: ["IMPLEMENT", "ARCHITECTURE"]
- "structure" → ARCHITECTURE fires (Rule 3, absolute)
- "table structure" still contains "structure" → ARCHITECTURE fires

**How to avoid v6-01:**
Always check: does the request contain the exact token "structure" (standalone, not as part of "restructure")? If YES → add ARCHITECTURE to predicates. No reasoning required. No context check. Literal rule.
