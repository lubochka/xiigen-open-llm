# Investigation Handoff

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.

## When to Invoke

At the end of any session that:
- Analyzed the codebase to understand current state
- Compared two approaches or designs
- Debugged a complex issue across multiple classes
- Discovered facts not in STATE-Pn.json or docs

Write to `docs/investigations/` AND update STATE-Pn.json `discoveries[]` field.

**Write discoveries to STATE-Pn.json during the session — not just at the end.**
A session that compacts before writing the handoff loses everything discovered in the last hour.

---

## Required Output Format

File: `docs/investigations/INVESTIGATION-HANDOFF-[topic]-[YYYY-MM-DD].md`

```markdown
# INVESTIGATION-HANDOFF — [topic] — [YYYY-MM-DD]
## Session type: [CODE | PLANNING | ANALYSIS | DEBUG]
## For: [next session / Luba]

## CONTEXT
[2-3 sentences: what problem was investigated, what approach was taken, what was the starting state]

---

## DISCOVERIES

DISCOVERY-1:
  fact:        [precise factual statement — no hedging, no "probably"]
  verified_by: [exact command or file:line that confirmed it]
  implication: [what this means for any session that touches this area]
  action:      RESOLVED: [what was done]
             | DEFERRED: [why and what triggers resolution]
             | OPEN: [what needs to happen next]

DISCOVERY-2: ...

---

## REJECTED CLAIMS

REJECTED-1:
  claim:       [exact wrong statement that was assumed]
  source:      [where the assumption came from]
  truth:       [what is actually true]
  verified_by: [command or file:line that disproved it]
  risk:        [what would have gone wrong if accepted]

---

## OPEN QUESTIONS REQUIRING LUBA'S DECISION

OPEN-1:
  question:  [precise question requiring Luba's decision]
  context:   [why this cannot be decided automatically]
  options:   [2-3 concrete options with tradeoffs]
  blocked:   [what cannot proceed until this is answered]

---

## NUMBERS ESTABLISHED THIS SESSION

| Artifact | Verified Value | Verified By |
|----------|---------------|-------------|
| Test count at session start | 916 | dotnet test --no-build \| tail -3 |
| Test count at session end | 924 | dotnet test --no-build \| tail -3 |
| Next subproject number | 25 | ls Src/ \| sort -n \| tail -3 |

---

## WHAT COMES NEXT

1. [task] — [CODE SESSION | PLANNING SESSION | ANALYSIS]
2. ...
```

---

## discoveries[] in STATE-Pn.json

Write discoveries to STATE-Pn.json continuously during investigation:

```json
{
  "phase": "P2",
  "status": "IN_PROGRESS",
  "discoveries": [
    {
      "foundAt": "step-3",
      "fact": "IVectorIndex.Search() returns ALL vectors when query.Length == 0, not 0 results",
      "verifiedBy": "grep -n 'query.Length' Src/09.VectorSearch/BruteForceIndex.cs",
      "implication": "Callers that pass empty queries will get unexpected results — guard needed",
      "actionTaken": "OPEN — guard required in BruteForceIndex.Search()"
    }
  ],
  "rejectedClaims": [
    {
      "claim": "HybridRetriever handles null IVectorIndex gracefully",
      "source": "assumption from reading the interface",
      "truth": "HybridRetriever constructor throws ArgumentNullException immediately",
      "verifiedBy": "Read Src/12.HybridRetriever/HybridRetriever.cs:15 — no null guard",
      "risk": "Would have relied on graceful degradation that does not exist"
    }
  ]
}
```

---

## Real Example

```
DISCOVERY-1:
  fact:        BruteForceIndex.Search() with topK > document count returns all documents, not error
  verified_by: dotnet test --filter "BruteForceIndex_TopK_ExceedsCount" → passes (test already exists)
               File: Tests/VectorSearch/BruteForceIndexTests.cs:145
  implication: HybridRetriever callers that specify topK=100 on small corpora get all docs — OK behavior
  action:      RESOLVED — documented in COMPONENT-INTERFACES.md

REJECTED-1:
  claim:       "HnswIndex is faster than BruteForceIndex for all dataset sizes"
  source:      assumption from HNSW literature
  truth:       For N<500 vectors, BruteForceIndex is faster due to HNSW graph overhead
  verified_by: Experiment E-07 in docs/EXPERIMENT-RESULTS-MAP.md
  risk:        Would have defaulted to HnswIndex everywhere, harming performance on small indexes
```

---

## Anti-Patterns

1. **Writing the handoff only at session end** — context compaction destroys discoveries made hours earlier
2. **"See above" as verified_by** — not a command; not reproducible; not useful
3. **Discoveries without implications** — "T count is 924" with no implication = incomplete
4. **Omitting rejected claims** — wrong assumptions resurface in future sessions if not recorded
5. **Vague facts** — "IVectorIndex might have issues with empty input" is not a discovery; "BruteForceIndex returns 0 results for empty query" is
