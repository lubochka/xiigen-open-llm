# Layer 0-C — Knowledge-Edge Pairs (059)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-059-001 — Advanced edge: token co-occurrence matrix for 037-060

**High-frequency token co-occurrence patterns in 037-060:**

| Token A | Token B | Combined edge |
|---|---|---|
| "write migration" | "PII" | IMPLEMENT + SECURITY |
| "anonymize" | "GDPR" | IMPLEMENT + SECURITY (two SECURITY triggers → one predicate) |
| "design" | "structure" | ARCHITECTURE (two triggers → one predicate) |
| "pattern" | "architecture" | ARCHITECTURE (two triggers → one predicate) |
| "write migration" | "write tests" | IMPLEMENT + TEST |
| "write migration" | "document" | IMPLEMENT + DOCUMENT |
| "restructure" | "pattern" | IMPLEMENT (refactoring) + ARCHITECTURE |
| "PII" | "write tests" | SECURITY + TEST |
| "design" | "PII" + "write migration" | IMPLEMENT + ARCHITECTURE + SECURITY |

---

## B18-0C-059-002 — Advanced edge: negation and suppression

**Negation does NOT suppress absolute edges:**

| Phrasing | Edge still fires? | Reason |
|---|---|---|
| "no PII in this migration" | YES — SECURITY still fires | "PII" token present even in negation |
| "not a security concern, just a migration" | NO — SECURITY does NOT fire | No absolute security token present |
| "without GDPR requirements" | YES — SECURITY fires | "GDPR" token present |
| "non-structured data" | NO | "non-structured" — "structure" noun absent |
| "avoiding encryption" | NO | "encryption" absent as trigger; "avoiding" suppresses |
| "not a pattern, just an index" | YES — ARCHITECTURE fires | "pattern" noun present |

**Key edge:** The token fires the edge. Negation of the concept does not remove the token. Exception: negated compound forms where the token is part of a negative phrase that completely rewords the concept without containing the trigger token.

---

## B18-0C-059-003 — Advanced edge: ARCHITECTURE in passive voice

**ARCHITECTURE fires in passive voice constructions:**

| Passive construction | ARCHITECTURE fires? |
|---|---|
| "the schema structure is reviewed" | YES — "structure" noun present |
| "the pattern is applied" | YES — "pattern" noun present |
| "the architecture was designed" | YES — "architecture" noun + "designed" past tense of design |
| "the database is structured" | NO — "structured" = adjective |
| "the migration is patterned after X" | YES — "patterned" as adjective; "pattern" noun? Ambiguous. Typically NO — "patterned after" = adjective use |

---

## B18-0C-059-004 — Advanced edge: TEST in conditional constructions

**TEST in conditional or future constructions:**

| Construction | TEST fires? |
|---|---|
| "write tests to verify the migration" | YES |
| "we should write tests for this" | YES — "write tests" verb form present |
| "tests should be written after migration" | NO — "written" passive; "be written" ≠ "write tests" |
| "if we add tests later" | NO — conditional future; "add tests" in conditional form may not fire |
| "add integration tests for this feature" | YES — "add tests" verb form |
| "testing should follow migration" | NO — noun form |

---

## B18-0C-059-005 — Advanced edge: DOCUMENT in embedded clauses

**DOCUMENT fires in embedded clauses:**

| Construction | DOCUMENT fires? |
|---|---|
| "write migration and document the process" | YES — "document" verb |
| "implement the migration, which requires documenting the steps" | YES — "documenting" gerund = document verb |
| "document as you go" | YES — "document" verb |
| "the documented migration" | NO — "documented" = adjective |
| "documentation-driven migration" | NO — compound adjective |
| "spec out the requirements, then implement" | YES — "spec out" = DOCUMENT trigger |
| "requires full documentation" | YES — "documentation" noun but "requires full documentation" = DOCUMENT trigger pattern |
