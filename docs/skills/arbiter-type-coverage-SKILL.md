# Arbiter Type Coverage

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (in any language), challenges an answer, or says a failure came
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

An arbiter fixture file can have records and still fail to cover the types of
checks a capability actually requires. A capability that reads session data may have
arbiters for domain logic but none for scope isolation — the most critical check for
multi-session storage correctness. This skill defines the minimum required arbiter
types based on capability characteristics, the detection commands to verify coverage,
and the severity of each gap.

---

## When to Invoke

- At Phase A gate, after arbiter fixtures are seeded but before Phase B begins
- When reviewing a capability's arbiter fixture file for completeness
- When `scope_isolation` count is 0 in any capability that reads or writes ISessionStorage
- When DPO triple quality audit identifies that arbiter coverage is incomplete

---

## The Two Arbiter Systems — Do Not Confuse

| System | Governed by | What it covers |
|--------|------------|----------------|
| C# `ArbiterConfig` in topology contracts | arbiter-panel-design-SKILL.md | AI pipeline judges during generation cycles |
| JSON fixture files `docs/arbiters/{capability-id}-arbiters.json` | **This skill** | Named checks registered in validate.handler; scored during each cycle |

A capability can have a correct ArbiterConfig and still have incomplete fixture coverage.
Both systems must be compliant independently.

---

## Section 1 — Minimum Type Matrix

| Capability characteristic | Required arbiter type | Minimum count | FAIL condition |
|--------------------------|----------------------|---------------|----------------|
| **Any capability** | `scope_isolation` | ≥1 per method that reads/writes ISessionStorage | 0 records = CRITICAL FAIL |
| Capability handles personal data (email, names, credentials) | `security` | ≥1 | Absent = CRITICAL FAIL |
| Capability has iron rule (CF-N) constraints | `iron_rules` | ≥1 per CF rule | Absent when CF rules exist = HIGH |
| Capability has domain business logic | `domain` | ≥3 | Fewer than 3 = HIGH |
| Capability uses store-before-publish ordering | `ordering_invariant` | ≥1 | Absent when pattern used = HIGH |
| Auth or credential-issuing capability | `security` | ≥3 | Fewer than 3 = CRITICAL FAIL |
| Cross-capability data access | `scope_isolation` | ≥2 | Fewer than 2 = HIGH |

**Universal rule:** every record in a fixture file must have the `CheckId` field
populated (non-empty string matching a registered named check). Records with null or
empty `CheckId` = HIGH finding.

---

## Section 2 — Detection Commands

Run against any capability's arbiter fixture file. Replace `{capability-id}` with the
actual capability identifier.

```bash
FIXTURE="docs/arbiters/{capability-id}-arbiters.json"

# Type distribution — shows what's present
cat "$FIXTURE" | \
  python3 -c "import sys,json; d=json.load(sys.stdin); \
    from collections import Counter; \
    c=Counter(x.get('CheckId','NO_CHECK_ID') for x in d); \
    [print(f'{k}: {v}') for k,v in sorted(c.items())]"

# Records without CheckId (must be 0)
cat "$FIXTURE" | \
  python3 -c "import sys,json; d=json.load(sys.stdin); \
    print(sum(1 for x in d if not x.get('CheckId')))"
# Expected: 0

# scope_isolation count (must be ≥1 for any capability)
cat "$FIXTURE" | \
  python3 -c "import sys,json; d=json.load(sys.stdin); \
    print(sum(1 for x in d if x.get('CheckId')=='session_id_scope_isolation'))"
# Expected: ≥1
```

For .NET, the equivalent grep-based detection:
```bash
# Scope isolation count in fixture
grep -c '"session_id_scope_isolation"' "docs/arbiters/{capability-id}-arbiters.json"

# All CheckId values in fixture
grep '"CheckId"' "docs/arbiters/{capability-id}-arbiters.json" | sort | uniq -c

# Records missing CheckId
grep -c '"CheckId": ""' "docs/arbiters/{capability-id}-arbiters.json"
```

---

## Section 3 — Assessment Protocol

### Step 1 — Identify capability characteristics

```
□ Does this capability read or write ISessionStorage?
□ Does this capability handle PersonalData fields (Email, Name, PasswordHash)?
□ Does this capability have iron rules (CF-N constraints in the topology contract)?
□ Does this capability contain domain business logic beyond simple routing?
□ Does this capability use IAgentMessageBus.Publish() after OperationResult.Ok()?
□ Is this capability authentication or credential-related?
□ Does this capability access data across multiple sessions?
```

### Step 2 — Run detection commands

Check CheckId distribution, count records without CheckId, count scope_isolation,
count security, check ordering_invariant presence.

### Step 3 — Apply minimum type matrix

For each characteristic that applies, verify the required type is present at the
required count. Record verdict per type.

### Step 4 — Issue findings

| Severity | Finding | Action |
|----------|---------|--------|
| CRITICAL | `session_id_scope_isolation` absent on any capability | Block Phase B |
| CRITICAL | `security` absent on credential or personal-data capability | Block Phase B |
| HIGH | Records with null/empty CheckId | Block Phase B |
| HIGH | `domain` arbiters < 3 on business-logic capability | Block Phase B |
| HIGH | `iron_rules` absent when CF constraints exist | Block Phase B |
| MEDIUM | `ordering_invariant` absent when store-before-publish used | Warn, proceed with risk note |

---

## Section 4 — Arbiter Type Vocabulary

These are the valid types for the arbiter type matrix. Each maps to a named check
family in the validate.handler registry (arbiter-registration-SKILL.md Section 1):

| Type | Covers | Named checks |
|------|--------|-------------|
| `scope_isolation` | Cross-session data access without scope check | `session_id_scope_isolation` |
| `security` | Credential handling, personal data exposure | `pii_not_in_event_payload` |
| `iron_rules` | DNA Rule 1-10 + CF-N constraints | `operation_result_everywhere`, `no_concrete_injection`, `no_throw_for_business_logic` |
| `domain` | Business logic correctness | capability-specific named checks |
| `ordering_invariant` | store-before-publish ordering | `store_before_enqueue` |

---

## Anti-Patterns

1. **"Only checking named check registration, not type coverage"** — a fixture file with
   all required named checks can still have no scope_isolation arbiter. Type coverage
   and named check registration are separate checks.

2. **"Record count implies coverage"** — a fixture with 20 records, all of type `domain`,
   has zero scope isolation coverage. Count does not imply coverage.

3. **"The arbiter-registration skill covers this"** — arbiter-registration-SKILL.md governs
   fixture format, named check registry, and replay gate. This skill governs whether the
   right types of checks are present for the capability's characteristics. Both must pass.

4. **"Scope isolation isn't needed because tests already verify it"** — the arbiter
   checks during pipeline cycles, not tests. Scope isolation must fire during every
   AI-generated code evaluation, not just in the test suite.

---

## Integration

- arbiter-registration-SKILL.md: governs fixture format and named check registration;
  this skill governs type coverage — both must pass independently
- scope-isolation-arbiter-SKILL.md: governs runtime isolation checking for ISessionStorage;
  this skill ensures the scope_isolation arbiter type is declared before Phase B
- capability-simulation-SKILL.md: L2 simulation should include arbiter type coverage
  as a SILENT_FAILURE check — a capability that runs but lacks scope_isolation arbiters
  will produce incorrectly scored cycles without any visible error
