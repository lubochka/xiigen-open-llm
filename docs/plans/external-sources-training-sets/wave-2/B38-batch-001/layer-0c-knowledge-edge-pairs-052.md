# Layer 0-C — Knowledge-Edge Pairs (052)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B38-0C-052-001 — Changelog and release note: predicate disambiguation

Distinguishing DOCUMENT, IMPLEMENT, QA, and knowledge_edge in changelog/release note scenarios:

| Input | PredicateSet | Key trigger |
|---|---|---|
| "Create the release note template." | ["DOCUMENT"] | "create" in document context |
| "Build the release note template service." | ["IMPLEMENT"] | "build" + "service" |
| "Review the release note template." | ["QA"] | "review" |
| "Generate the release note template." | [] | "generate" gap |
| "Document the release note criteria." | ["DOCUMENT"] | "document" + "criteria" |
| "Build the changelog service and review the entries." | ["QA","IMPLEMENT"] | "build" + "service" (IMPLEMENT); "review" (QA) |

---

## B38-0C-052-002 — Migration guide: predicate disambiguation

| Input | PredicateSet |
|---|---|
| "Document the migration guide." | ["DOCUMENT"] |
| "Build the migration service." | ["IMPLEMENT"] |
| "Review the migration guide." | ["QA"] |
| "Prepare the migration guide." | [] |
| "Document the migration acceptance criteria." | ["DOCUMENT"] |
| "Build the migration service and document the guide criteria." | ["IMPLEMENT","DOCUMENT"] |

---

## B38-0C-052-003 — Deprecation and API breaking change: predicate disambiguation

| Input | PredicateSet |
|---|---|
| "Document the deprecation notice." | ["DOCUMENT"] |
| "Send the deprecation notice." | [] |
| "Build the deprecation notification service." | ["IMPLEMENT"] |
| "Review the deprecation notice." | ["QA"] |
| "Document the breaking change criteria." | ["DOCUMENT"] |
| "Introduce the breaking change." | [] |
| "Audit the breaking change security and document criteria." | ["QA","DOCUMENT","SECURITY"] |
