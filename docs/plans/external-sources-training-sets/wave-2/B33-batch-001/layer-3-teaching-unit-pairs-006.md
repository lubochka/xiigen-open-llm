# Layer 3 — Teaching Unit Pairs (006)
# Batch: B33-batch-001 | Scenario: B33 — AI agent adaptation gap
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-006 — DomainId is always software-development for AI gaps

**Teaching goal**: No special domain exists for AI capability gaps. DomainId is always software-development.

**Rule**: All B33 AI agent gap requests log DomainId: software-development.
There is no: ai-development, ml-development, llm-development, or agent-development domain.

**Why**: The SCHEDULE_CAPABILITY_LEARNING system does not have specialized AI domains in its registry. All capability gaps — regardless of whether they involve AI, ML, or traditional software — are classified under software-development.

**GapHandler logging format** (canonical):
```
{
  gapId: "gap-ai-agent-adapt-001" | "gap-ai-agent-skill-001" | "gap-ai-agent-llm-001",
  domainId: "software-development",
  request: "[original request text]",
  event: "SCHEDULE_CAPABILITY_LEARNING"
}
```

**Common mistake to avoid**:
- v6 corruption: DomainId: "ai-development" — does not exist
- v6 corruption: DomainId: "llm-development" — does not exist
- Correct: DomainId: "software-development" — always, for all B33 gaps
