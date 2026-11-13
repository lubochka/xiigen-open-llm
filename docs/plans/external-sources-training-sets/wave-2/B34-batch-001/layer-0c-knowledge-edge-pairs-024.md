# Layer 0-C — Knowledge Edge Pairs (024)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-024-001 — Complete B34 guidance vocabulary knowledge edge summary

**Gap verbs (never fire any predicate):**
mentor, coach, teach, train (human), guide, explain, onboard, shadow, upskill, advise, facilitate, pair-with, walk-through, workshop (event), review-with, ramp-up, demonstrate, show (guidance sense), clarify, instruct, sponsor, cross-train, observe, hand-off (knowledge), share (knowledge)

**Embedded verb rule (guidance frame + code verb):**
- Code verb fires its predicate if requester performs the action
- Guidance verb becomes gap warning (not full gap log)
- GapHandler suppressed when any predicate fires
- SCHEDULE_CAPABILITY_LEARNING suppressed when any predicate fires

**Full gap (GapHandler + SCHEDULE_CAPABILITY_LEARNING):**
- Triggered when predicate set = [] after full scan
- No code-action verbs anywhere in request

---

## KE-024-002 — Exception registry for B34

| Exception | Rule | Example |
|-----------|------|---------|
| "integrate" in "teach to integrate" | "teach to X" exception; infinitive = learning target → gap | "teach to integrate systems" → gap |
| "explain" never fires DOCUMENT | explain = gap even next to DOCUMENT nouns | "explain the documentation" → gap |
| "design [curriculum]" → gap | noun type determines: curriculum → gap | "design workshop curriculum" → gap |
| "design [architecture]" → ARCHITECTURE | structural noun → ARCHITECTURE | "design system architecture" → ARCHITECTURE |
| third-party action not counted | only requester's actions fire predicates | "shadow while she builds" → gap |
| "facilitate integration" → API_CONTRACT | integrate fires in any context including facilitate wrapper | "facilitate integration of A with B" → API_CONTRACT |

---

## KE-024-003 — QA abbreviation rule reminder

"QA" abbreviation fires QA predicate.
"quality assurance" spelled out → gap (gap-skills-guidance-train-001 in training context).
"quality of training" → gap.
"ensure quality" → gap.
"teach to QA" → gap ("teach to X" pattern).
"QA the portal" → QA fires.
"conduct QA" → QA fires (QA abbreviation in any position).
