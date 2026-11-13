# Layer 0-C — Knowledge-Edge Pairs (057)
# Batch: B18-batch-001 | Scenario: B18 — Database migrations and data layer
# Production date: 2026-05-06 | pair_kind: knowledge-edge

## B18-0C-057-001 — Edge reference: DOCUMENT in migration context

**DOCUMENT edge activation in migration requests:**

| Token | DOCUMENT fires? | Notes |
|---|---|---|
| "document the migration" | YES | "document" = verb (Rule 4) |
| "document the rollback plan" | YES | "document" = verb |
| "write the runbook" | YES | "write runbook" = DOCUMENT trigger |
| "annotate the migration steps" | YES | "annotate" = DOCUMENT trigger |
| "spec out the migration process" | YES | "spec out" = DOCUMENT trigger |
| "write documentation for the schema" | YES | "write documentation" = DOCUMENT trigger |
| "write the policy for PII handling" | YES | "write the policy" = DOCUMENT trigger |
| "the documentation is complete" | NO | Noun form |
| "migration document" (noun) | NO | "document" as noun modifier |

---

## B18-0C-057-002 — Edge reference: "policy" and "runbook" as DOCUMENT triggers

**"policy", "runbook", "spec" as DOCUMENT context:**

| Token + DOCUMENT verb | DOCUMENT fires? |
|---|---|
| "document the PII policy" | YES |
| "write the data retention policy" | YES |
| "spec out the anonymization policy" | YES |
| "write the migration runbook" | YES |
| "annotate the rollback procedures" | YES |
| "the policy exists" | NO — noun form |
| "runbook complete" | NO — noun form |

---

## B18-0C-057-003 — Edge reference: "spec" disambiguation

**"spec" in different contexts:**

| Usage | Edge |
|---|---|
| "spec out the migration" | DOCUMENT (Rule 4: "spec out" = documentation) |
| "type-spec migration" | NOT DOCUMENT ("spec" as technical compound) |
| "spec file" | NOT DOCUMENT (file type) |
| "write a spec for the schema" | DOCUMENT (Rule 4) |
| "write a spec and implement the migration" | DOCUMENT + IMPLEMENT |

---

## B18-0C-057-004 — Edge reference: token interaction — "GDPR" + "compliance"

**"GDPR" and "compliance" in migration context:**

| Token | SECURITY fires? |
|---|---|
| "GDPR" | YES (absolute) |
| "GDPR compliance" | YES — "GDPR" token present |
| "GDPR-compliant migration" | YES — "GDPR" token present |
| "compliance" alone | YES — "compliance" fires SECURITY (absolute; regulatory compliance token) |
| "compliant migration" (without "GDPR") | NO — "compliant" alone ≠ SECURITY trigger |
| "compliance with schema requirements" | NO — "compliance" here = conformance, not regulatory |

**Key edge:** "compliance" fires SECURITY when it refers to regulatory compliance (GDPR, HIPAA, etc.). "compliance" in non-regulatory context (schema compliance, format compliance) does NOT fire SECURITY.

---

## B18-0C-057-005 — Edge reference: "authentication" and "authorization" in migrations

**"authentication"/"authorization" in migration context:**

| Token | SECURITY fires? | Notes |
|---|---|---|
| "authentication" | YES (absolute) | Rule 14 |
| "user authentication migration" | YES | "authentication" present |
| "authorization policies migration" | YES | "authorization" present |
| "API key authentication migration" | YES | "authentication" present |
| "migrate user accounts" | NO | "user accounts" ≠ authentication |
| "migrate session data" | NO | "session data" alone ≠ authentication |
| "migrate authentication tokens" | YES | "authentication" present |
