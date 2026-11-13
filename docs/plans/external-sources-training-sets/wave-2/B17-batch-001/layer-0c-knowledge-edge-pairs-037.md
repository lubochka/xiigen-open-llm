# Layer 0-C — Knowledge-Edge Pairs (037)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Jira connector

---

## B17-0C-037-001 — Jira: non-trigger terms

**Proper nouns — no predicate:** Jira, Atlassian
**Domain nouns — no predicate:** issue, ticket, sprint, epic, backlog, board, project, workflow, transition, assignee, reporter
**Acronyms — no predicate:** JIRA (treated as proper noun), REST, API (alone)

---

## B17-0C-037-002 — Jira: "issue tracker" and "workflow" are not integration vocabulary

"Issue tracker" and "workflow transition" are domain phrases. They fire no predicates.
Only "connector", "webhook", "API" (in direct compound with connector vocabulary), "external service" supply integration vocabulary to route IMPLEMENT to 4.3.

---

## B17-0C-037-003 — Jira: OAuth trigger confirms

"OAuth" fires AUTHORIZATION regardless of whether the OAuth flow is Jira's specific OAuth 1.0a or OAuth 2.0. The rule checks for the word "OAuth" in any form — not for a specific OAuth version or flow type.

---

## B17-0C-037-004 — Jira: "API key" and "personal access token" distinction

- "Jira API key" alone → no SECURITY (API key, no "token")
- "Jira personal access token" → SECURITY fires (compound contains "token")
- "API token" → SECURITY fires ("token" present)

The distinction is the word "token" in the noun phrase — not the security classification of the credential.
