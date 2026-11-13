# Layer 3 — Teaching Unit Pairs (022)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU-022 — Mixed code predicates in AI context teaching unit

**Teaching goal**: Demonstrate that code predicates fire normally in AI agent contexts, and gap vocabulary is silently skipped when predicates fire.

**Rule 1**: Code predicates fire in AI context. "implement the agent skill router and write integration tests" → [IMPLEMENT, TEST]. Both fire. AI context ("agent skill") does not reduce predicate count.

**Rule 2**: Multiple code verbs fire multiple predicates. "design the multi-agent architecture, build the orchestration service, and integrate with the registry API" → [ARCHITECTURE, IMPLEMENT, API_CONTRACT]. Three verbs, three predicates.

**Rule 3**: Gap suppression rule. "The agent needs chain-of-thought and we should integrate with the feedback API" → [API_CONTRACT]. "chain-of-thought" = gap (silently skipped). "integrate" fires API_CONTRACT. Result contains only predicates.

**Rule 4**: SECURITY fires in AI context when prompt injection present. "build the agent response filter and secure against prompt injection" → [IMPLEMENT, SECURITY]. AI context does not suppress SECURITY.

**Rule 5**: QA abbreviation fires even in AI context. "Implement the skill router, write tests, and QA sign-off" → [IMPLEMENT, TEST, QA]. All three fire; QA is last regardless.

**Rule 6**: DOCUMENT fires in AI context. "document the agent's self-healing behavior" → [DOCUMENT]. "document" verb fires regardless of AI vocabulary in the object.

**Summary**: AI context never suppresses code predicates. Code verbs fire their predicates regardless of AI vocabulary in subject or object positions. Mixed gap+code: gap is silently skipped.
