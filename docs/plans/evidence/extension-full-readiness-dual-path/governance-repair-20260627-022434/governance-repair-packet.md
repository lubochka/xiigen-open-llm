# Пакет evidence по governance/skills repair

## Honesty guard

```text
claim_scope: только governance/skill/guide repair; product readiness не заявляется
evidence_scope: изменены AGENTS.md, CLAUDE.md, пять docs/skills файлов и два файла evidence packet; выполнены targeted textual checks, git diff --check, проверка пробелов в packet и JSON parse check
count_claimed: 7 governance/skill файлов изменены + 2 evidence packet файла созданы
independent_evidence_count: 4
subagent_id_or_NONE: bounded-governance-repair-subagent
downgrade_needed: product readiness не заявлялась; evidence поддерживает только governance/skill repair
```

## Вердикт

PASS для этого bounded governance/skills repair.

Этот packet не заявляет product readiness, host readiness, runtime execution,
tests, build success, commit, push или plan execution.

## Диагноз

Дырка была не в полном отсутствии plan-readiness правила. В файлах уже было
написано, что unready plan возвращается в repair loop, и что старое continuation
wording не достаточно.

Точная проблема была уже: правила не связывали старое давление "do not stop" /
"continue" с явным запретом выдавать или продолжать executor work order до того,
как активный план закрыт, readable, exact, и пока product-readiness plan gate не
прошел.

Из-за этого executor/sub-agent мог принять continuation pressure как runtime
permission, а не как planning/governance repair loop. Repair закрывает это через
hard executor-authorization gate, правило остановки преждевременного executor и
обязательный human-readable status-count gate перед execution continuation.

## Измененные файлы

- `C:\Projects\llm_mvp_core\AGENTS.md`
- `C:\Projects\llm_mvp_core\CLAUDE.md`
- `C:\Projects\llm_mvp_core\docs\skills\planning-session-startup-SKILL.md`
- `C:\Projects\llm_mvp_core\docs\skills\plan-review-SKILL.md`
- `C:\Projects\llm_mvp_core\docs\skills\output-readability-gate-SKILL.md`
- `C:\Projects\llm_mvp_core\docs\skills\verification-before-completion-SKILL.md`
- `C:\Projects\llm_mvp_core\docs\skills\session-output-contract-SKILL.md`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\governance-repair-20260627-022434\governance-repair-packet.md`
- `C:\Projects\llm_mvp_core\docs\plans\evidence\extension-full-readiness-dual-path\governance-repair-20260627-022434\governance-repair-packet.json`

## Exact rule snippets added

`AGENTS.md`:

```text
Plan-readiness executor authorization gate: do not stop is not executor
authorization.
```

```text
plan-not-ready returns to planning repair, not execution
```

```text
the premature executor must be stopped and replaced by bounded governance repair
```

```text
human-readable phase counts before execution continuation
```

`CLAUDE.md`:

```text
Plan-readiness before executor continuation pattern: do not stop is not executor
authorization.
```

```text
the premature executor must be stopped and replaced by bounded governance repair
```

`planning-session-startup-SKILL.md`:

```text
Startup executor authorization guard: do not stop is not executor
authorization.
```

```text
Startup returns to bounded planning/governance/clarity repair and then resumes
the plan-readiness workflow.
```

`plan-review-SKILL.md`:

```text
Executor-before-plan-readiness review blocker: do not stop is not executor
authorization.
```

```text
Plan review must reject any work order, continuation verdict, or status packet
that treats "do not stop", old "continue", old `next_action`, or "nearest safe
already-authorized" pressure as permission to run an executor before the active
plan is closed/readable/exact and the product-readiness plan gate has passed.
```

`output-readability-gate-SKILL.md`:

```text
Executor-readiness visibility rule: do not stop is not executor authorization.
```

```text
Before any execution continuation is visible as allowed, the message must
include human-readable phase counts before execution continuation.
```

`verification-before-completion-SKILL.md`:

```text
Plan-readiness continuation verification: do not stop is not executor
authorization.
```

```text
Completion/continuation verification must fail any packet that uses old "do not
stop", "continue", `next_action`, or "nearest safe already-authorized" pressure
to start or continue executor work before the active plan is closed/readable/exact.
```

`session-output-contract-SKILL.md`:

```text
Plan-readiness continuation is explicit: do not stop is not executor
authorization; plan-not-ready returns to planning repair, not execution; a
premature executor must be stopped and replaced by bounded governance repair.
```

## Почему это закрывает loophole

1. Executor authorization теперь отделен от continuation pressure. "Do not stop"
   удерживает planning/governance loop живым, но не разрешает product execution.
2. Для unready plan next action назван явно: planning/governance/clarity repair,
   не executor runtime work и не final pause.
3. Если executor уже стартовал слишком рано, правило теперь требует остановить
   его, считать output quarantine evidence only, провести governance repair
   через bounded sub-agent и затем вернуться в plan-readiness workflow.
4. До execution continuation Luba должна видеть human-readable phase counts:
   total phases/steps, done, in-progress, remaining и plain-language meaning
   текущей фазы/шага перед любым machine-only label.

## Verification performed

Commands run:

```powershell
rg --files -g 'AGENTS.md' -g 'CLAUDE.md' -g 'docs/skills/*.md' -g 'docs/plans/evidence/**'
rg -n "do not stop|plan-not-ready|Plan-Not-Ready|premature|executor|human-readable|phase counts|QSG|plan readiness|ready|готов|исполнитель|не стоп|останов" AGENTS.md CLAUDE.md docs/skills -S
git status --short
Get-Date -Format 'yyyyMMdd-HHmmss'
rg -n --fixed-strings "<required phrase>" AGENTS.md CLAUDE.md docs/skills/planning-session-startup-SKILL.md docs/skills/plan-review-SKILL.md docs/skills/output-readability-gate-SKILL.md docs/skills/verification-before-completion-SKILL.md docs/skills/session-output-contract-SKILL.md
git diff --check -- AGENTS.md CLAUDE.md docs/skills/planning-session-startup-SKILL.md docs/skills/plan-review-SKILL.md docs/skills/output-readability-gate-SKILL.md docs/skills/verification-before-completion-SKILL.md docs/skills/session-output-contract-SKILL.md
rg -n "[ \t]+$" docs/plans/evidence/extension-full-readiness-dual-path/governance-repair-20260627-022434/governance-repair-packet.md docs/plans/evidence/extension-full-readiness-dual-path/governance-repair-20260627-022434/governance-repair-packet.json
Get-Content -Raw docs/plans/evidence/extension-full-readiness-dual-path/governance-repair-20260627-022434/governance-repair-packet.json | ConvertFrom-Json | Select-Object -ExpandProperty verdict
```

Verification result:

- Required phrase check по 7 governance/skill файлам до создания packet: PASS
  после добавления explicit central literals.
- Required phrase check по 9 измененным файлам после создания packet: PASS.
- `git diff --check` по 7 governance/skill файлам: PASS, no output.
- `git diff --stat`: 7 files changed, 90 insertions до создания packet.
- Packet `.md` / `.json` trailing-whitespace text check: PASS, no matches.
- Packet JSON parse check: PASS, parsed verdict `PASS`.

## Remaining next action

Parent/current session должен review this bounded packet, принять или отклонить
repair, затем resume plan-readiness workflow. Этот packet не authorizes product
execution.
