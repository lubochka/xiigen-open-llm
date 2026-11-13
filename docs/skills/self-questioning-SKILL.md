# Self-Questioning

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

An artifact produced without questioning its own assumptions will contain
gaps the author could have caught — but didn't because the production
process never paused to ask. This skill defines the QUESTION YOURSELF
protocol: the minimum question set, the required output format, and the
downstream validation checks that confirm the protocol was applied. A
topology, contract, or RAG pattern that reaches the validator without a
self-questioning record is treated as incomplete, regardless of how
correct it appears.

---

## When to Invoke

- Before finalizing any topology design (ai-pipeline-topology-SKILL.md)
- Before submitting any EngineContract record
- Before submitting any RAG pattern for DomainModelRegistry
- After producing any artifact that will feed IncrementalTrainer
- When the validate.handler downstream check SELF-Q-001 fires (missing questions)

---

## Section 1 — QUESTION YOURSELF Protocol

**Minimum: 3 questions. Default: apply all 5 standard questions.**

```
Standard Question Set:

Q1 — Capability classification:
  "Have I classified the capability class correctly, or could this be
   a different archetype that changes which iron rules apply?"
  (prevents misclassification that routes to the wrong named checks)

Q2 — Learning signal:
  "Does this artifact produce a learning signal? If not: why not,
   and is that acceptable for this capability class?"
  (ensures DPO triple generation is not accidentally omitted)

Q3 — Bootstrap boundary:
  "At what point does this artifact require real data vs. synthetic data?
   Have I marked the boundary explicitly?"
  (prevents synthetic training data from being treated as production signal)

Q4 — Threshold configuration:
  "Are any thresholds in this artifact hardcoded? Should they be loaded
   from IConfiguration instead?"
  (machine_constant sub-score prevention: any hardcoded threshold here
   will fire score-zero-investigation-SKILL.md)

Q5 — Completeness:
  "Is there a named check in this artifact that I cannot verify will
   fire on a real violation? If so: I need a test for that check before
   this artifact enters the corpus."
  (prevents named checks that look correct but never actually fire)
```

---

## Section 2 — Output Format

Every artifact that passes through the self-questioning protocol must
include these three fields in its output record:

```csharp
public record SelfQuestionedArtifact<T>
{
    public T Artifact { get; init; }
    public List<SelfQuestion> SelfQuestions { get; init; }   // minimum 3
    public List<string> Answers { get; init; }               // one per question, in order
    public List<string> ModificationsFromQuestioning { get; init; }
    // ModificationsFromQuestioning: what changed because of the questioning.
    // If no modifications: include at least one entry: "No modifications required — [reason]"
    // Empty list is not valid: it means the questions were not genuinely applied.
}

public record SelfQuestion
{
    public string QuestionId { get; init; }  // "SELF-Q-001" style or custom
    public string QuestionText { get; init; }
    public string Domain { get; init; }      // TOPOLOGY / CONTRACT / RAG / GENERAL
}
```

The `ModificationsFromQuestioning` field is the most important. If it
is empty, the downstream validator rejects the artifact. The field must
contain either a concrete change made OR an explicit statement of why
no change was needed.

---

## Section 3 — Downstream Validation Checks

The validate.handler in the AI pipeline MUST verify three checks on any
artifact submitted through this protocol:

```
SELF-Q-001: selfQuestions.Count >= 3
  FAIL: artifact has fewer than 3 questions
  Action: return artifact to origin with SELF_QUESTIONING_INCOMPLETE signal

SELF-Q-002: answers.Count == selfQuestions.Count
  FAIL: not all questions have answers
  Action: return artifact to origin with UNANSWERED_QUESTIONS signal
  (a question listed but unanswered is worse than no question — it signals
   the protocol was started but abandoned)

SELF-Q-003: modificationsFromQuestioning is not empty
  FAIL: ModificationsFromQuestioning.Count == 0
  Action: return artifact to origin
  Note: an explicit "No modifications required — [reason]" entry PASSES this check.
        An empty list does not.
```

---

## Section 4 — Domain-Specific Question Sets

For artifact types that recur frequently, replace or extend the standard
five questions with domain-specific ones:

### Topology Artifacts

```
T1: "Does this topology have at least 2 iron rules? (mandatory element)"
T2: "Does the feedback.handler appear as the final node? (mandatory wiring)"
T3: "If this uses multi-generate: does the arbiter-panel produce a DPO triple?"
T4: "Is there a quality gate that verifies dotnet test before proceeding?"
```

### EngineContract Records

```
E1: "Does every public method in this contract return OperationResult<T>? (Rule 1)"
E2: "Does this contract take concrete types instead of interfaces? (Rule 2 check)"
E3: "Is there a test in this contract's test class that verifies the Fail path,
     not just the Ok path?"
```

### RAG Pattern Generation

```
R1: "Does this RAG pattern target a specific capability class, or is it too broad
     to be useful for DomainModelRegistry routing?"
R2: "Does this pattern have an ObservationCount? Can the count be verified?"
R3: "Does the confidence seed in this pattern match the risk tier from
     confidence-seeding-SKILL.md for this capability class?"
```

---

## Anti-Patterns

1. **"Filling in selfQuestions[] after writing the artifact"** — the
   purpose of self-questioning is to change the artifact before it is
   finalized, not to document questions after the fact. Questions must
   be asked before the artifact is complete.

2. **"ModificationsFromQuestioning: []"** — an empty list signals the
   protocol was performed mechanically without genuine application.
   The downstream SELF-Q-003 check will fail this. Write "No modifications
   required — [specific reason]" if nothing changed.

3. **"Using Q1-Q5 verbatim when domain-specific questions apply"** —
   a topology artifact that uses the generic Q1 instead of T1 ("has this
   topology ≥ 2 iron rules?") misses the topology-specific mandatory check.
   Use domain-specific questions when available.

4. **"Answering questions without changing anything for 3+ sessions"** —
   consistent zero-modification records signal that the protocol is being
   applied but is not finding genuine gaps. Rotate question sets or
   escalate to meta-arbiter-SKILL.md for review.

5. **"Skipping self-questioning for 'simple' artifacts"** — the artifacts
   that appear simple are the ones most likely to contain silent errors
   (hardcoded thresholds, missing iron rules, incomplete test coverage).
   Apply the protocol regardless of perceived complexity.

---

## Integration

- ai-pipeline-topology-SKILL.md requires self-questioning before finalizing any topology
- score-zero-investigation-SKILL.md uses Q4 (threshold configuration) as a machine_constant prevention step
- validate.handler in the AI pipeline enforces SELF-Q-001/002/003 downstream
- planning-dpo-authoring-SKILL.md benefits from Q2 (learning signal check) being applied before DPO authoring
- meta-arbiter-SKILL.md may identify missing self-questioning as a retrieval gap (Step 1 diagnosis)
