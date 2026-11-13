# Skill Graph Sync

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

When a skill document is updated (new version, corrected guidance, new anti-pattern),
DomainModelRegistry edges that were seeded from that skill may silently diverge. After
15 runs have accumulated evidence for a different threshold than what the skill author
intended, neither the graph nor the skill document surfaces the conflict. This skill
defines the link mechanism between skill documents and graph edges, the drift detection
protocol, and the resolution paths based on evidence confidence.

---

## When to Invoke

- When a skill document version is bumped (any change to a skill file)
- When DomainModelRegistry confidence for an edge diverges more than 0.3 from its
  source skill's declared value
- When designing the SkillCompiler bootstrap process
- When a graph-learned threshold should update the skill, or vice versa

---

## Section 1 — The Link Mechanism

Every DomainModelRegistry edge seeded from a skill document carries a
`GOVERNS_SEED` relationship linking it back to its source skill:

```csharp
public record GovernsSeedEdge
{
    public string SkillId { get; init; }         // e.g., "arbiter-panel-design-SKILL"
    public string SkillVersion { get; init; }    // e.g., "1.0"
    public string GraphEntityId { get; init; }   // the DomainModelRegistry edge being governed
    public SyncPolicy SyncPolicy { get; init; }  // NOTIFY_ON_DRIFT | AUTO_RESEED
    public DateTimeOffset LastSyncedAt { get; init; }
}

public enum SyncPolicy
{
    NotifyOnDrift,   // surface to Luba when divergence > threshold
    AutoReseed,      // reseed from skill when graph confidence is low
}
```

This link is how `GraphDriftDetector` knows which skills to check when graph values
change, and which graph entities to update when skills are updated.

---

## Section 2 — Drift Detection Protocol

When a skill version is bumped, `GraphDriftDetector` runs:

```
1. Find all GOVERNS_SEED edges from this skill version
2. Compare skill's declared value against current graph edge confidence/weight
3. If divergence > 0.3:

   Graph confidence > 0.8 → GRAPH_LIKELY_CORRECT
     "Skill says X but N runs showed Y. Is the skill outdated?"
     → Surface to Luba as a structured question
     → Resolution: update the skill OR mark edge as EXCEPTION_ACKNOWLEDGED
       (stores the reason Luba accepts the divergence)

   Graph confidence < 0.5 → SKILL_LIKELY_CORRECT
     "Skill updated and graph has insufficient evidence"
     → Auto-reseed graph from skill (no human review needed)
     → Reset confidence to the ConfidencePreseeder value for new skill version

   Graph confidence 0.5..0.8 → CONFLICTING_EVIDENCE
     "Both have moderate support"
     → Schedule for investigation at next session
     → Neither auto-reseeds nor surfaces immediately
```

The 0.3 divergence threshold is configurable via FREEDOM config:
`graph.skillSync.driftThreshold`. Default 0.3.

---

## Section 3 — SkillCompiler — Bootstrap Boundary Rule

SkillCompiler compiles skill documents into DomainModelRegistry entities. It CANNOT
be a topology capability — it produces the graph that all topologies depend on.
SkillCompiler is Phase A bootstrap work, not a runtime topology.

**Two-pass extraction:**

```
Pass 1 — Structured fields (deterministic):
  Reads: skill file name, YAML frontmatter if present, section headers
  Extracts: skill ID, version, category, activation triggers
  Produces: TRIGGER_CONDITION nodes, archetype scope (for ConfidencePreseeder)

Pass 2 — Markdown body (AI-extracted):
  Reads: ## Anti-Patterns section (❌ prefixed lines)
  Reads: numbered step sequences in ordered lists
  Reads: bash/csharp code blocks marked as verification commands
  Produces: ANTI_PATTERN nodes with AVOID edges
             DECISION_STEP nodes with SEQUENCE edges
             FAILURE_MODE_PREVENTED nodes
```

Re-run triggers:
- Any skill version bump
- New skill file added to docs/skills/
- Drift detected with divergence > 0.3

---

## Section 4 — Skill Authoring for Compiler Compatibility

To maximize what SkillCompiler extracts from skill documents:

**Triggers:** Use specific phrases that map to graph TRIGGER_CONDITION nodes.
```
✓ "score below threshold for 3 consecutive runs" → specific, matchable
✗ "when things go wrong" → vague, unmatchable
```

**Anti-patterns:** Always prefix with ❌ on its own line (not inline in prose).
```
✓ ❌ Using the same arbiter role for two different evaluation dimensions
✗ "You shouldn't use the same arbiter role twice" (buried in prose — not extracted)
```

**Verification commands:** Always in bash/csharp code blocks with concrete commands.
```
✓ ```bash
  grep -rn "IAgentMessageBus" Src/12.ContinuousLearning/ --include="*.cs" | wc -l
  ```
✗ "Check that IAgentMessageBus is present" (not machine-executable)
```

**Prevention statements:** Always under `## Anti-Patterns` or `## Core Problem` as
a list or dedicated header. SkillCompiler reads these sections by header name.

---

## Section 5 — Resolution Record Format

When Luba reviews a `GRAPH_LIKELY_CORRECT` surface:

```csharp
public record SkillGraphResolution
{
    public string SkillId { get; init; }
    public string GraphEntityId { get; init; }
    public double SkillDeclaredValue { get; init; }
    public double GraphLearnedValue { get; init; }
    public double GraphConfidence { get; init; }
    public ResolutionDecision Decision { get; init; }
    public string Reasoning { get; init; }    // required for all decisions
    public DateTimeOffset ResolvedAt { get; init; }
}

public enum ResolutionDecision
{
    UpdateSkill,            // skill was wrong; graph evidence is correct
    AcceptException,        // both are valid in different contexts; exception acknowledged
    RejectGraphEvidence,    // graph accumulated on bad training data; reseed from skill
}
```

Every resolution record is stored in DomainModelRegistry as an
`EXCEPTION_ACKNOWLEDGED` edge when `Decision = AcceptException`. This prevents the
same divergence from re-surfacing after the next skill version bump.

---

## Anti-Patterns

❌ Auto-reseeding when graph confidence is > 0.8.
  The graph has accumulated genuine evidence. Overriding it without Luba's review
  discards that evidence. Resolution requires human judgment at high confidence.

❌ Ignoring drift when graph confidence is < 0.5.
  Low confidence means the graph doesn't have evidence to disagree. The skill
  author's knowledge should prevail automatically. Do not delay the reseed.

❌ Running SkillCompiler as a topology capability.
  SkillCompiler produces the graph that all topologies depend on. It is bootstrap
  infrastructure. If SkillCompiler fails, no topology can run. Never make it
  depend on itself.

❌ Marking GOVERNS_SEED edges as immutable.
  The governance relationship itself must be updatable when skill versions change.
  The governed entity may have immutable properties, but the link is not immutable.

❌ Omitting GOVERNS_SEED edges on new graph entities.
  An entity seeded without a GOVERNS_SEED edge cannot be drift-detected. New
  entities added by graph-extension-protocol-SKILL.md still need GOVERNS_SEED
  if they were proposed based on skill guidance.

---

## Integration

- graph-entity-schema-SKILL.md: defines NODE/EDGE/PROPERTY/IMMUTABLE classification;
  GOVERNS_SEED is an EDGE type connecting SKILL_NODE to graph entities
- confidence-seeding-SKILL.md: ConfidencePreseeder assigns initial confidence when
  skill updates auto-reseed graph (SKILL_LIKELY_CORRECT path)
- graph-extension-protocol-SKILL.md: new edges added through the mutation governance
  process may also have GOVERNS_SEED links if they originate from skill proposals
- learning-loop-closure-SKILL.md: a skill that consistently produces low-confidence
  graph edges (confidence never rises above 0.5) may indicate the skill's guidance
  does not match what the learning loop is observing
