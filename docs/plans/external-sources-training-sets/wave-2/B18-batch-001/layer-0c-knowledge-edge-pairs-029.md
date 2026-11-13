# Layer 0-C — Knowledge-Edge Pairs (029)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-029-001 — "trigger" disambiguation: database object vs security vocabulary

**"trigger" token in different contexts:**

| Phrase | Context | Edge fired |
|---|---|---|
| "database trigger" | DDL database object | IMPLEMENT + 4.4 only |
| "trigger migration" | DDL migration artifact | IMPLEMENT + 4.4 only |
| "trigger for GDPR audit" | GDPR present | SECURITY fires via GDPR, not via "trigger" |
| "audit trigger" alone | DDL, no PII/GDPR | IMPLEMENT + 4.4 only |
| "trigger compliance check" | compliance present | SECURITY fires via "compliance", not "trigger" |

**Rule:** "trigger" is a database DDL object token. It NEVER independently fires SECURITY. SECURITY fires only if PII, GDPR, encrypted, credentials, authentication, compliance, or sensitive data tokens are present.

---

## B18-0C-029-002 — "index" token: no ARCHITECTURE edge

**"index" in different compound contexts:**

| Phrase | ARCHITECTURE fires? | Reason |
|---|---|---|
| "index structure" | YES | "structure" fires (Rule 3) |
| "index architecture" | YES | "architecture" fires (Rule 7) |
| "index pattern" | YES | "pattern" fires (Rule 7) |
| "search index" | NO | no absolute trigger |
| "rebuild index" | NO (ARCHITECTURE) | data rebuild → IMPLEMENT + 4.4 only |
| "indexed column" | NO | adjective form |
| "create an index" | NO (ARCHITECTURE) | IMPLEMENT fires; no structural token |

---

## B18-0C-029-003 — "sequence" token: no ARCHITECTURE edge by itself

**"sequence" in different compound contexts:**

| Phrase | ARCHITECTURE fires? | Reason |
|---|---|---|
| "sequence structure" | YES | "structure" fires (Rule 3) |
| "sequence architecture" | YES | "architecture" fires (Rule 7) |
| "sequence pattern" | YES | "pattern" fires (Rule 7) |
| "primary key sequence" | NO | no absolute trigger |
| "reset sequence" | NO (ARCHITECTURE) | IMPLEMENT fires; no structural token |
| "sequence cache" | NO | operational vocabulary |

---

## B18-0C-029-004 — "enum" token: no ARCHITECTURE edge by itself

**"enum" in different compound contexts:**

| Phrase | ARCHITECTURE fires? | Reason |
|---|---|---|
| "enum type structure" | YES | "structure" fires (Rule 3) |
| "enum architecture" | YES | "architecture" fires (Rule 7) |
| "state machine pattern" | YES | "pattern" fires (Rule 7) |
| "order_status enum" | NO | no absolute trigger |
| "add enum value" | NO (ARCHITECTURE) | IMPLEMENT fires; no structural token |
| "rename enum" | NO (ARCHITECTURE) | refactoring IMPLEMENT; no structural token |

---

## B18-0C-029-005 — "procedure" and "view" tokens: no ARCHITECTURE edge by themselves

**"procedure" and "view" in compound contexts:**

| Phrase | ARCHITECTURE fires? | Reason |
|---|---|---|
| "procedure structure" | YES | "structure" fires (Rule 3) |
| "stored procedure architecture" | YES | "architecture" fires (Rule 7) |
| "view architecture" | YES | "architecture" fires (Rule 7) |
| "reporting view structure" | YES | "structure" fires (Rule 3) |
| "stored procedure" alone | NO | database object only |
| "materialized view" alone | NO | no absolute trigger |
| "create a procedure" | NO (ARCHITECTURE) | IMPLEMENT fires; no structural token |
