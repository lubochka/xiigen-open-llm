# Layer 3 — Teaching Unit Pairs (004)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-004 — "explain" never fires any predicate

**Rule**: "explain" is pure teaching vocabulary. It never fires DOCUMENT, SECURITY, ARCHITECTURE, or any other predicate.

**This is a common v6-02 corruption target**:
- "Explain the auth flow" → gap ✓ (NOT SECURITY)
- "Explain the API design" → gap ✓ (NOT API_CONTRACT)
- "Explain the architecture" → gap ✓ (NOT ARCHITECTURE)
- "Explain how to write tests" → gap ✓ (NOT TEST)
- "Explain the documentation structure" → gap ✓ (NOT DOCUMENT)

**Why**: Explaining is a teaching act. The requester is not implementing security, creating API contracts, designing systems, writing tests, or creating documentation — they are explaining these topics. The action is teaching; DOCUMENT requires creating documentation artifacts.

**DOCUMENT requires**: write/create/spec out/document/draft + documentation artifact noun.
**Not**: explain/show/describe/present/walk through + documentation topic.
