# Layer 0-C — Knowledge-Edge Pairs (052)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Supabase connector

---

## B17-0C-052-001 — Supabase: non-trigger terms

**Proper nouns — no predicate:** Supabase
**Domain nouns — no predicate:** table, row, column, policy, bucket, function, edge function, realtime channel, project, organization
**Acronyms — no predicate:** RLS (Row-Level Security), PG (PostgreSQL abbreviation), SB (Supabase abbreviation)

---

## B17-0C-052-002 — Supabase: anon key vs service key vs JWT token

- "Supabase anon key" → no SECURITY (no "token")
- "Supabase service role key" → no SECURITY (no "token")
- "Supabase JWT (JSON Web Token) access" → SECURITY fires (expansion contains "Token")
- "Supabase access token" → SECURITY fires (compound contains "token")
Teaching: Supabase uses "key" terminology by default. SECURITY fires only when "token" appears.

---

## B17-0C-052-003 — Supabase: "RLS policy" does not fire SECURITY

"Row-Level Security (RLS) policy" is a database access control feature — it is not a SECURITY trigger. RLS (Row-Level Security) acronym fires no predicate.

---

## B17-0C-052-004 — Supabase: JWT expansion note

JWT (JSON Web Token) — when the expansion "JSON Web Token" is written, "Token" is present → SECURITY fires.
When only the abbreviation "JWT" is written without expansion → SECURITY does NOT fire.
Best practice in training: treat JWT with its expansion as the canonical form.
