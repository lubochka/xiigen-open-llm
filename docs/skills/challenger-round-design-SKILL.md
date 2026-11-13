# Challenger Round Design

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

## Core Problem

A multi-model convergence session without defined challenger roles produces
inconsistent challenges — one reviewer checks intent, another checks security,
both check both, neither checks completeness. The challenge history contains
noise. The final design carries unresolved ambiguity. This skill defines how to
compose challenger roles for convergence sessions, what each challenger receives
(context isolation), and when to declare consensus vs escalate.

---

## When to Invoke

- Before writing any multi-reviewer convergence session for a capability design
- When multi-reviewer-design-SKILL.md requires challenger prompt templates
- When a convergence session is STALLED and the escalation type needs diagnosis
- Before Gate B of any phase that uses multi-model design review

---

## Section 1 — Challenger Role Selection

### Minimum challenger set by capability archetype

| Archetype | Minimum challengers |
|---|---|
| ROUTING | Domain + DNA Compliance |
| DATA_PIPELINE | Domain + Security + DNA Compliance |
| VALIDATION | Domain + Completeness + DNA Compliance |
| SCORING | Domain + DNA Compliance + Iron Rules |
| TRAINING | Domain + Security + Business + Completeness + DNA Compliance |
| ORCHESTRATION | Domain + Security + Business + Completeness + DNA Compliance |

### Add optional challengers when:
- Capability emits cross-capability events → add **Business** challenger
- Capability crosses subproject boundaries → add **Business** challenger
- Capability involves session data or user context → add **Security** challenger
- Capability has multi-subproject deployment requirement → add **Stack Portability** challenger

### Never add challengers for noise

Do not add a Security challenger to a read-only query capability that touches
no session data. Do not add a Business challenger to a single-subproject capability
with no cross-system contracts. Noise challengers inflate rounds without adding signal.

---

## Section 2 — Challenger Prompt Templates

Every challenger receives a prompt that defines its role, context package, and
response format. Context isolation is mandatory: each challenger receives ONLY
its defined subset.

### Domain Challenger

```
ROLE:   Domain Challenger in a capability design convergence session.
EXPERTISE: Intent correctness and iron rule completeness for this capability class.

RECEIVES:
- The proposed capability design (NODE representation)
- The capability's iron rules
- The domain context (what this capability does for the subproject)

DOES NOT RECEIVE: security patterns, DNA principles text, RAG patterns,
                  other challenger verdicts.

For each challenge, provide:
1. Which field in the design is incorrect or incomplete
2. Why (trace to a specific domain invariant)
3. What the correct representation should be

RESPONSE FORMAT:
{
  "verdict": "APPROVED" | "CHALLENGE" | "CONTEXT_INSUFFICIENT",
  "challenges": [
    { "field": "intent.failureModes", "reason": "...", "correction": "..." }
  ],
  "contextRequests": [
    { "type": "DOWNSTREAM_CONTRACT | INTERFACE_SPEC | ...", "target": "...", "question": "..." }
  ]
}

A challenge is GENUINE if removing it would change what code gets generated.
A challenge is NOISE if the correction would produce functionally identical code.
Raise only genuine challenges.
```

### Security Challenger

```
ROLE:   Security Challenger in a capability design convergence session.
EXPERTISE: Rule 1 compliance (no throws), Rule 2 compliance (fabric-first),
           failure mode coverage for this capability class.

RECEIVES:
- The proposed capability design
- CLAUDE.md Rule 1 and Rule 2 full text
- The capability's declared failure modes

DOES NOT RECEIVE: iron rules, domain context, other challenger verdicts, RAG patterns.

Check:
- Does intent.failureModes[] cover all Rule 1 (OperationResult<T>) failure paths?
- Are there missing failure modes related to: session isolation, interface contract violations?
- Does intent.invariants[] enforce the critical boundary properties?

RESPONSE FORMAT: same as Domain Challenger
```

### Business Challenger

```
ROLE:   Business Challenger in a capability design convergence session.
EXPERTISE: Cross-subproject consistency — contracts between this capability
           and other capabilities, subprojects, or services.

RECEIVES:
- The proposed capability design
- The list of events this capability emits and consumes
- Upstream capability designs from earlier in the same pipeline

DOES NOT RECEIVE: iron rules, DNA principles text, security patterns.

You are looking for:
- Field values that may be transformed by a prior capability before reaching this one
- Event schema fields that downstream consumers may interpret differently
- Dependencies on an assumption about an upstream subproject

RESPONSE FORMAT: same as Domain Challenger
```

### DNA Compliance Challenger (context-isolated — mandatory)

```
ROLE:   DNA Compliance Arbiter in a capability design convergence session.
EXPERTISE: Alignment with CLAUDE.md Rules 1–10 and BaseService pattern.

RECEIVES (ONLY these — isolation is the enforcement mechanism):
- Full text of CLAUDE.md Rules 1–10 with concrete violation examples
- The proposed capability design

DOES NOT RECEIVE: domain context, iron rules, security patterns,
                  other challenger verdicts.

Check: does the capability design, as written, violate any Rule 1–10?

BLOCK threshold: ANY Rule 1–10 violation is automatic BLOCK.
A CHALLENGE from this role means the design cannot be accepted until resolved.

RESPONSE FORMAT: same as Domain Challenger
```

### Completeness Challenger

```
ROLE:   Completeness Challenger in a capability design convergence session.
EXPERTISE: Verifying that nothing essential is missing from the design.

RECEIVES:
- The proposed capability design
- The capability's structure fields (inputShape, outputShape, emits)

Check:
- Are all declared outputs reachable from the declared inputs?
- Are all declared emits traceable to a condition in intent.invariants or intent.failureModes?
- Is intent.purpose specific enough that a second implementor could build the same thing?

RESPONSE FORMAT: same as Domain Challenger
```

---

## Section 3 — Termination Conditions

### CONSENSUS_REACHED (happy path)

All challengers return APPROVED in the same round.

Verify:
```
□ Every challenger returned APPROVED (not just a majority)
□ Final design was validated against the last round's challenges
□ No pending CONTEXT_INSUFFICIENT requests
□ convergenceHistory.consensusReachedAt is set
```

### DEFERRED_CONSTRAINT (acceptable provisional)

A CONTEXT_INSUFFICIENT request could not be resolved (requires human judgment
or external information not available in the session).

Mark the design as provisional:
```markdown
## DEFERRED_CONSTRAINT
Request type: DOWNSTREAM_CONTRACT
Target: IVectorIndex.Query() return shape after normalization
Question: Does IVectorIndex.Query() return normalized scores or raw distances?
Deferred at: [date]
Unblocks: iron rule "normalize before merge" cannot be finalized
```

Implementation proceeds with provisional design. The generated code is marked
with the DEFERRED_CONSTRAINT and may need revision when resolved.

### STALLED (escalation required)

Max rounds exceeded without consensus. Escalate to planning session.

Stalling usually means:
- Genuine architectural ambiguity → escalate to Luba
- Circular challenges (challenger scope too broad) → narrow the challenger's context package
- Missing upstream design (convergence order violated) → check dependency graph

---

## Section 4 — Genuine Disagreement Requirement

A convergence session produces learning signal only when challengers genuinely
disagree. The session can produce syntactically valid designs that teach nothing.

**Genuine disagreement requires:**
```
1. At least one CHALLENGE verdict from a challenger
   (all APPROVED on round 1 = either the design is trivial or challengers didn't try)

2. The CHALLENGE traces to a specific field and specific invariant
   (vague challenges like "this needs more detail" = noise)

3. The correction, if applied, changes what code gets generated
   (cosmetic challenges = noise)
```

**Pre-convergence gate — run before starting rounds:**
```
□ Design has at least one domain-specific constraint
  (generic designs give challengers nothing to challenge)

□ At least one legitimate implementation path exists that a challenger might object to
  (if there is only one correct approach, disagreement is impossible)

□ Intent purpose is specific enough to distinguish from similar capabilities
  (too vague → challengers have nothing to compare against)
```

---

## Anti-Patterns

1. **"Challenger prompt without context isolation"** — DNA Compliance challenger
   receiving iron rules is a contamination. Each challenger receives ONLY its
   defined context package.

2. **"Adding challengers to appear thorough"** — a Security challenger on a
   read-only query adds noise. Minimum panel by archetype. Add only when justified.

3. **"APPROVED after round 1 with no challenges"** — either the design is trivial
   or the challengers didn't challenge hard enough. Zero challenges in round 1
   warrants re-running with more specific challenge criteria.

4. **"STALLED treated as equivalent to CONSENSUS"** — STALLED means genuine
   disagreement exists. Escalate it. A STALLED design used for implementation
   carries unresolved ambiguity into the code.

---

## Integration

- multi-reviewer-design-SKILL.md uses these challenger roles for phase plan reviews
- arbiter-panel-design-SKILL.md governs the parallel evaluation panel (runtime)
- diagnostic-escalation-SKILL.md governs escalation when STALLED status is reached
- context-package-design-SKILL.md governs the structure of each challenger's context package
- convergence training signals feed into DomainModelRegistry via ContinuousLearningPipeline
