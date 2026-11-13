# Governance Repair Evidence Packet - Inline Script Timeout

packet_id: governance-repair-inline-script-timeout-20260627-060712
subagent_id: current_bounded_subagent
status: PASS

## Scope

Active unquoted Luba work order: bounded sub-agent governance/skill repair after
Row10-S5c executor timeout.

Allowed write scope used:
- AGENTS.md
- CLAUDE.md
- docs/skills/code-execution-SKILL.md
- docs/skills/ui-testing-SKILL.md
- docs/skills/phase-planning-SKILL.md
- this evidence packet folder

Forbidden actions not performed:
- product code edits
- product execution
- Cursor UI launch/control
- tests/builds
- commit/push
- web/external AI
- nested subagents

## Rule Changes

1. UI/runtime/executor work orders must not use huge inline shell scripts.
   Threshold added: more than 20 nonblank lines, more than 4000 characters, or
   more than one function/class requires a scoped `script-file runner` under the
   evidence/run folder.

2. Node REPL execution must avoid `Node REPL top-level redeclaration` using
   `var`, scoped blocks, or fresh names. A declaration collision is a
   work-order/tooling failure, not a product blocker.

3. Windows error 206, oversized inline shell command, timeout, or
   tool-command construction failure is not a product blocker. The next step is
   preflight-only script-file prep/execution, not another huge inline command
   retry.

4. Parent/subagent-only boundaries and no-stop rules were preserved. The repair
   only extends existing governance and skill rules.

## Exact Snippets

AGENTS.md:

```text
- instruction that UI/runtime/executor commands use a `script-file runner` when
  the command would exceed 20 nonblank lines, 4000 characters, or include more
  than one function/class; huge inline shell scripts are forbidden in work
  orders.
```

```text
- Windows error 206, oversized inline shell command, or other tool-command
  construction failure: do not retry the huge command. Split the next action
  into preflight-only script-file prep/execution, write the scoped runner under
  the evidence/run folder, and execute that file.
```

CLAUDE.md:

```text
Runtime/UI tool-command construction pattern: UI/runtime/executor work orders
must not use huge inline shell scripts.
```

docs/skills/code-execution-SKILL.md:

```text
## Runtime/Tool Command Construction Gate
```

docs/skills/ui-testing-SKILL.md:

```text
UI/runtime scenario work orders must not use huge inline shell scripts.
```

docs/skills/phase-planning-SKILL.md:

```text
Runtime/UI work-order command gate: UI/runtime/executor work orders must not use
huge inline shell scripts.
```

## Verification

Targeted phrase check:

```text
rg -n "huge inline shell scripts|script-file runner|Node REPL top-level redeclaration|preflight-only script-file prep/execution|tool-command construction failure|Windows error 206" AGENTS.md CLAUDE.md docs\skills\code-execution-SKILL.md docs\skills\ui-testing-SKILL.md docs\skills\phase-planning-SKILL.md
```

Result: PASS. The command returned matches in all five changed governance/skill
files.

Changed tracked files check:

```text
git diff --name-only -- AGENTS.md CLAUDE.md docs\skills\code-execution-SKILL.md docs\skills\ui-testing-SKILL.md docs\skills\phase-planning-SKILL.md
```

Result: PASS. The command returned exactly the five changed governance/skill
files.

Final packet checks:

```text
Get-Content -Raw "docs\plans\evidence\extension-full-readiness-dual-path\governance-repair-inline-script-timeout-20260627-060712\governance-repair-inline-script-timeout-20260627-060712.json" | ConvertFrom-Json | Out-Null; Write-Output "PASS json parse"
```

Result: PASS.

```text
git diff --check -- AGENTS.md CLAUDE.md docs\skills\code-execution-SKILL.md docs\skills\ui-testing-SKILL.md docs\skills\phase-planning-SKILL.md
```

Result: PASS, exit code 0, no whitespace output.

Packet whitespace check for untracked packet files:

```text
Select-String -Path <packet md/json> -Pattern "[ \t]$"
```

Result: PASS, no trailing whitespace matches.
