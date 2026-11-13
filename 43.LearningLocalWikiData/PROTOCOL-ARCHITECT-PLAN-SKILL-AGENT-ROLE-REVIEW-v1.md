# Протокол архитекторской проверки плана, скилов, агентских файлов и смены ролей

Статус: reference protocol для другого архитектора.
Дата: 2026-05-25.
Режим: архитекторский контроль, не execution report и не training report.

Этот документ нужен, чтобы следующий архитектор мог проверить не только сам
план, но и всю оркестрацию вокруг него: какие скилы должны быть загружены,
какие агентские правила управляют исполнителем, как меняются роли, где искать
ответы, как запрещаются мягкие остановки, и как возвращаться к исполнению после
ремонта протокола.

Этот документ не разрешает объявлять проект завершённым.
Этот документ не разрешает запускать новые batch-и сам по себе.
Этот документ не является stop gate.

## 1. Главная проверяемая цель

Архитектор проверяет, что план и правила удерживают одну цель:

```text
XIIGen должен уметь создать, обучить, экспортировать, импортировать,
валидировать, обновлять и использовать весь набор обучаемых units для
project-local wiki поверх единой Angular + .NET + SQL stored procedures системы,
а Visual Studio и VS Code плагины должны уметь запускать эту оркестрацию через
пользовательский entrypoint на реальных проектах и на задачах из prompt suite.
```

Любой план, скил, агентское правило или report, который проверяет меньшую цель,
должен быть исправлен до исполнения.

## 2. Когда применять этот протокол

Применять обязательно:

1. Перед принятием нового многошагового плана.
2. Перед переходом из архитектуры в исполнение.
3. После каждого executor slice, даже если slice сделал commit и push.
4. Когда исполнитель сказал `done`, `готово`, `complete`, `next step`, `can continue`,
   `waiting for approval`, `blocked`, или дал финальный report.
5. Когда Luba оспорила completion, формат report, роль, ветку, тесты, branch, commit,
   push, scope, plugin-readiness, model export/import, или покрытие prompts.
6. Когда найден gap в плане, скиле, `AGENTS.md`, `CLAUDE.md`, state или matrix.

## 3. Источники истины и порядок чтения

Архитектор читает в таком порядке:

1. Последнюю прямую инструкцию Luba в текущем разговоре.
2. `C:/Projects/llm_mvp_core/AGENTS.md`.
3. `C:/Projects/llm_mvp_core/CLAUDE.md`.
4. Активный план.
5. Активный state-файл плана.
6. Активную completion coverage matrix.
7. `C:/Projects/llm_mvp_core/docs/HOW-TO-USE-SKILLS.md`.
8. Именованные скилы, перечисленные в плане.
9. Историю сессии, если Luba дала transcript.
10. Runtime reports, registry, manifests, plugin evidence и git status.

Если источник ниже по списку противоречит источнику выше по списку, нижний
источник чинится. Исполнитель не выбирает удобный источник сам.

## 4. Обязательный цикл ролей

Роли являются внутренним циклом работы. Они не являются поводами остановиться
перед Luba.

```text
PLAN_AUTHOR_INTERNAL
  -> PLAN_REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL
  -> EXECUTOR_INTERNAL
  -> REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL_REPAIR
  -> EXECUTOR_INTERNAL_CONTINUE
```

Если `REVIEW_INTERNAL` находит gap, исполнитель не сообщает финальное `done`.
Он переходит в `ARCHITECT_INTERNAL_REPAIR`, чинит план, скилы, agent files,
state или matrix, затем возвращается в `EXECUTOR_INTERNAL_CONTINUE`.

Остановка перед Luba разрешена только если:

```text
goal_reached == true
AND completion matrix has zero failed rows
AND state.stop_allowed == true
AND persisted architect finish review verdict == ARCHITECT_VERDICT_ALLOW_FINAL
AND active state architect_finish_review_verdict == ARCHITECT_VERDICT_ALLOW_FINAL
AND executor_subagent_packet exists and was checked by ARCHITECT_INTERNAL
AND git boundary satisfied or explicitly impossible
```

или если есть настоящий blocker:

```text
next safe action cannot be determined from Luba instruction, AGENTS.md,
CLAUDE.md, active plan, state, matrix, skills, documents, reports, or repo.
```

Если next action есть в плане, state, matrix или report, blocker отсутствует.

## 5. Проверка роли PLAN_AUTHOR_INTERNAL

План считается пригодным только если в нём есть все пункты:

1. Главная цель записана человеческим языком и совпадает с Section 1 этого протокола.
2. Указано, что Angular client, .NET server и SQL stored procedures проверяются как
   одна бизнес-система.
3. Указано, что обучаемые units живут в проекте, а validation projects являются
   только validation target.
4. Указаны trainable units, import/export, locator, manifest, checkpoints и upgrade path.
5. Указана ветка: использовать текущую ветку, не менять branch без прямой команды.
6. Указано, какие проверки разрешены, а какие запрещены.
7. Указано, что `dotnet test` не запускается для documentation/governance phases без
   прямого Gate B или прямой команды Luba.
8. Указано, что time, session length, context compaction, commit, push, report, partial
   progress и passed child gate не являются stop boundary.
9. Указаны state updates после каждого slice.
10. Указана completion coverage matrix.
11. Указаны score rules.
12. Указан Document RAG lookup order.
13. Указаны exact plugin scenarios для Visual Studio и Visual Studio Code.
14. Указана проверка всех prompt tasks из prompt suite, а не только классов задач.
15. Указан переход ролей без user-facing stop.
16. Указано, как архитектор ремонтирует найденные лазейки и возвращает executor к плану.

Если хотя бы один пункт отсутствует, план не переходит в исполнение.

## 6. Проверка роли PLAN_REVIEW_INTERNAL

Review не проверяет красоту плана. Review ищет лазейки.

Обязательные вопросы:

1. Может ли исполнитель честно сказать `done`, если выполнен только один файл?
2. Может ли исполнитель остановиться после commit/push?
3. Может ли исполнитель остановиться после report?
4. Может ли исполнитель остановиться после child gate, когда parent goal не закрыт?
5. Может ли исполнитель назвать `blocker` то, для чего есть next action?
6. Может ли исполнитель заменить runtime evidence статическим build/readiness report?
7. Может ли исполнитель проверить только prompt class, а не каждый prompt?
8. Может ли исполнитель проверить только internal helper, а не plugin user-facing entrypoint?
9. Может ли исполнитель решить, что model weights embedded не нужны, без import/export proof?
10. Может ли исполнитель поменять branch, потому что plan упоминает `main`?
11. Может ли исполнитель запустить full tests, потому что skill так привык?
12. Может ли executor остаться executor, когда Luba явно просит architect?
13. Может ли review быть финальной остановкой вместо ремонта и возврата к executor?
14. Может ли documentation update быть забыта после code change?
15. Может ли state остаться stale после изменения plan, report, matrix или commit?

Если ответ на любой вопрос `да`, это gap governance, а не execution blocker.
Архитектор чинит governance и возвращает процесс в исполнение.

## 7. Проверка роли ARCHITECT_INTERNAL

Архитектор не пишет production code, пока открыт архитектурный gap.

Архитектор обязан:

1. Классифицировать gap: plan hole, skill hole, agent hole, state hole, matrix hole,
   report hole, evidence hole, role-loop hole, branch hole, test-policy hole.
2. Найти governing file, который позволил ошибку.
3. Исправить governing file, а не только текущий report.
4. Обновить state.
5. Обновить completion matrix.
6. Записать score impact.
7. Сформулировать next executor action из плана.
8. Вернуть процесс в `EXECUTOR_INTERNAL_CONTINUE`, если нет настоящего blocker.

Архитектор не имеет права завершить свою работу словами `теперь можно продолжать`
при `goal_reached=false` и `next_action` не пустой.

## 8. Проверка роли EXECUTOR_INTERNAL

Executor имеет право делать только next action, который выводится из:

1. Luba current instruction.
2. Active plan.
3. Active state `next_action`.
4. First failed row in completion matrix.
5. Report with explicit remediation.

Executor не имеет права:

1. Придумывать новую цель.
2. Менять branch.
3. Запускать broad tests без Gate B.
4. Считать commit/push окончанием.
5. Считать `report written` окончанием.
6. Считать partial implementation окончанием.
7. Заменять trainable unit статическим helper-ом.
8. Заменять plugin runtime/user entrypoint проверку build-only проверкой.
9. Заменять prompt-by-prompt validation class-level validation.
10. Останавливаться после review, если есть next action.

После каждого executor slice обязательно включается `REVIEW_INTERNAL`.

## 9. Проверка роли REVIEW_INTERNAL

Review сравнивает claim scope с evidence scope.

Если claim говорит:

```text
plugin orchestration works
```

evidence должен показывать:

```text
Visual Studio user-facing command or tool-window invocation
AND Visual Studio Code user-facing command, panel, or chat invocation
AND per-prompt expected result
AND repo context
AND trainable unit path
AND import/export or locator proof
```

Если claim говорит:

```text
all prompts covered
```

evidence должен показывать каждый prompt item из source prompt suite, а не только
prompt class.

Если claim говорит:

```text
model is part of project
```

evidence должен показывать manifest, checkpoint reference, import path, export path,
locator entry, project registry row, and no embedded weights inside plugins.

Если evidence отсутствует, значение пишется `UNKNOWN`, не `PASS`.

## 10. Register лазеек из сессии

Каждая лазейка ниже должна иметь detector, repair и evidence row в плане или matrix.

| Лазейка | Симптом | Детектор | Обязательный ремонт |
|---|---|---|---|
| Premature stop | исполнитель остановился до цели | `goal_reached=false` и `next_action` не пустой | state + AGENTS/CLAUDE/plan repair, score penalty, architect-owned bounded work order |
| False positive done | `done/готово` без полной матрицы | matrix has failed row | completion claim forbidden; reopen review |
| Wrong report format | report без CPU/RAM/accuracy/time или wrong scope | report lacks numeric/evidence fields | runner/reporting protocol repair |
| Low accuracy stop | training stopped under threshold | accuracy below target and next batch known | continue training or source data |
| Local wiki/content confusion | wiki data treated as model capability | unit not exportable/importable | trainable-unit pattern repair |
| Artifacts outside project | accepted batch only in scratch folder | no project registry row | move metadata to project asset registry |
| No import/export | trained unit cannot be loaded elsewhere | no manifest/locator/checkpoint reference | add manifest, locator, repository reference |
| Unrequested tests | full test run during docs/governance | command history has broad test without Gate B | test policy repair; score penalty |
| Time limit stop | 120 seconds/session length used as reason | report says timeout or compaction as stop | time is metric only; continue/resume |
| Branch drift | executor switched branch | git branch differs without Luba command | record violation; return to requested/current branch only by explicit command |
| Commit/push gap | changes not committed/pushed when phase requires | git status not clean or no pushed commit | commit/push as checkpoint, not final stop |
| Executor instead of architect | code edit started while architecture gap open | Luba asked architect or plan review | stop code path, enter architect repair |
| Role transition stop | ARCHITECT -> EXECUTOR -> REVIEW became final stop | review found gap or next action exists | repair then continue executor |
| Soft stop | `I can continue`, `next step is`, `waiting` | phrase appears while next action known | -1000 score, repair protocol, continue |
| Custom final verdict | final review uses any verdict except `ARCHITECT_VERDICT_ALLOW_FINAL` | verdict contains PASSED/READY/COMPLETE but is not exact canonical value | final forbidden; patch review/state; rerun architect finish review |
| Missing architect/work-order evidence packet | architect final review accepts executor self-report or metrics only | no persisted architect evidence or bounded work-order packet checked by architect | collect architect evidence or issue bounded work order, persist packet, rerun review |
| Blocker misuse | `blocker` with known next action | state/matrix/report names action | blocker forbidden; continue |
| Static smart logic | helper chooses meaning/routing/unit | static class makes semantic decision | replace with trainable/exportable unit |
| Plugin readiness only | plugin build/readiness used as runtime proof | no user-facing invocation | add VS/VS Code user entrypoint probes |
| Prompt class only | prompt categories pass, individual prompts unknown | no row per prompt | create prompt-by-prompt expected results |
| Internal helper only | test calls helper directly | no plugin command/tool-window/chat | add host invocation harness |
| Stale state | state says done or old next_action | state conflicts with matrix | update state before any claim |
| Skills override AGENTS | skill STOP/test/branch wording wins | skill conflicts with OP rules | mark skill wording internal only |
| No review/repair loop | review found gap then process stopped | no ARCHITECT_INTERNAL_REPAIR state | patch loop and continue |
| User language drift | terse codes/schema instead of human answer | report unreadable to Luba | rewrite in Luba's language |

## 11. Required plan governance block template

Every multi-step plan and child plan must contain this block in its own words:

```text
main_focus_goal:
  The exact user-facing outcome this plan must deliver.

branch_rule:
  Use current branch. Do not switch branch without Luba's explicit current command.

allowed_verification:
  List exact commands or artifact checks allowed in this phase.

forbidden_verification:
  No full dotnet test for docs/governance/registry phases unless Gate B or Luba asks.

role_loop:
  PLAN_AUTHOR_INTERNAL -> PLAN_REVIEW_INTERNAL -> ARCHITECT_INTERNAL ->
  EXECUTOR_INTERNAL -> REVIEW_INTERNAL -> ARCHITECT_INTERNAL_REPAIR ->
  EXECUTOR_INTERNAL_CONTINUE.

skills_by_lifecycle_stage:
  plan creation, plan review, execution, code review, documentation sync,
  completion verification, loophole repair.

document_rag_lookup_order:
  Where to look before asking Luba.

anti_loophole_register:
  Explicit list of forbidden soft stops and false completion paths.

state_update_points:
  Before execution, after every slice, after review, after repair, after commit/push.

completion_matrix_rows:
  Each literal user expectation mapped to evidence and pass/fail.

score_rules:
  +1 for real progress without stopping, -100 premature incomplete stop,
  -1000 severe violation or soft stop.

commit_push_boundary:
  Commit/push is required when phase gate is met but is not a stop gate.

repair_returns_to_executor:
  After governance repair, continue the plan from state.next_action.
```

If a plan lacks this block, execution cannot start.

## 12. Required state fields

The active state must include:

```json
{
  "main_focus_goal": "",
  "goal_reached": false,
  "stop_allowed": false,
  "gate_status": "IN_PROGRESS_NEXT_STEP_KNOWN",
  "next_action": "",
  "next_action_plan_source": "",
  "actionable_gaps": [],
  "current_internal_role_state": "",
  "last_role_transition_reason": "",
  "return_to_executor_required": true,
  "architect_finish_review_verdict": "",
  "executor_subagent_packet_ref": "",
  "executor_subagent_packet_checked_by_architect": false,
  "post_push_architect_recheck_passed": false,
  "loaded_skills": [],
  "plan_completion_hole_register": [],
  "completion_matrix_path": "",
  "score_audit_latest": {
    "current_score": 0,
    "last_delta": 0,
    "last_delta_reason": ""
  },
  "premature_stop_review": {
    "required_if_stop_attempted_before_goal": true,
    "loophole_found": "",
    "governing_files_to_patch": []
  },
  "commit_push_boundary": {
    "commit_required_for_phase": true,
    "push_required_for_phase": true,
    "commit_is_stop_gate": false,
    "push_is_stop_gate": false
  }
}
```

If state says `stop_allowed=true` while matrix has failed rows, state is wrong.

## 13. Required completion matrix governance rows

The completion matrix must include rows for:

1. Main focus goal.
2. Branch rule.
3. Verification policy.
4. Role loop.
5. Skill orchestration.
6. Document RAG lookup order.
7. State invariant.
8. Score rules.
9. Soft stop ban.
10. Blocker definition.
11. Commit/push boundary.
12. Documentation sync.
13. Trainable unit import/export.
14. Plugin user-facing invocation.
15. Prompt-by-prompt coverage.
16. Advanced wiki questions.
17. Runtime CPU/RAM/time/accuracy metrics.
18. Child plan governance inheritance.
19. Review/architect repair returns to architect-owned bounded work order.
20. Final evidence scope equals final claim scope.

## 14. Skill audit protocol

Architect must search loaded skills for:

```text
STOP
hard stop
wait for answer
approval
must be on main
run dotnet test
done
complete
```

Any such wording is interpreted as internal only unless it matches Luba's current
instruction, `AGENTS.md`, `CLAUDE.md`, the active plan, and the active state.

If skill text conflicts with continuation, patch:

1. The active plan.
2. `docs/HOW-TO-USE-SKILLS.md`.
3. The specific skill file if needed.
4. State and matrix.

Then continue. Do not stop at the skill repair.

## 15. Agent and CLAUDE audit protocol

Architect must verify `AGENTS.md` and `CLAUDE.md` contain active rules for:

1. Luba current instruction outranks all.
2. No branch switch without explicit instruction.
3. No broad tests without Gate B.
4. Trainable units are project assets.
5. Continue while actionable gap exists.
6. Blocker means no safe next action.
7. Mandatory state.
8. Completion matrix.
9. Premature stop self-diagnosis and repair.
10. Score penalties.
11. Runtime evidence firewall.
12. Partial progress stop firewall.
13. Post-completion challenge.
14. Skills cannot override AGENTS/CLAUDE.
15. Review/architect repair loop.
16. Plan governance orchestration.

If one is absent, patch the files before continuing execution.

## 16. Report validation protocol

Every report is checked by claim scope.

Allowed result values:

```text
PASS
FAIL
UNKNOWN
NOT_APPLICABLE
IN_PROGRESS_NEXT_STEP_KNOWN
```

Forbidden:

```text
PASS by inference
PASS by readiness only
PASS by build only when runtime was required
PASS by class coverage when prompt-by-prompt was required
PASS by internal helper when plugin entrypoint was required
```

If CPU, RAM, elapsed time, accuracy, reward margin, loss, or held-out metric was
not emitted by the runner and persisted as artifact, the value is `UNKNOWN`.

`UNKNOWN` does not mean failure. It means no fake number.

## 17. Project wiki and plugin-specific checks

Architect must verify:

1. Angular + .NET + SQL stored procedures are treated as one linked business system.
2. Validation projects are validation targets only.
3. Generic model logic lives in `42.ProjectWikiDomain`, `43.LearningLocalWikiData`,
   `25.ModelServer`, and `15.OrchestratorModel`.
4. Plugins do not embed model weights.
5. Plugins can load or reference model manifests/checkpoints.
6. Visual Studio evidence uses command/tool-window or equivalent user-facing host path.
7. Visual Studio Code evidence uses command/panel/chat or equivalent user-facing host path.
8. Every prompt in `orchestrator prompts.txt` has expected orchestration output.
9. Prompt expected outputs include unit A-to-B structure, fanout, arbiter path,
   reprocessing path, and authority source.
10. Advanced wiki questions include entity impact, flow trace, stored procedure impact,
    Angular client impact, .NET server impact, and missing evidence abstention.

## 18. Stop permission gate

Before any final answer, architect must run this logic. Executor may prepare the
input packet, but executor may not grant final permission.

First, verify the finish-review authority fields:

```text
IF persisted architect finish review verdict != ARCHITECT_VERDICT_ALLOW_FINAL:
  stop_allowed = false
  next_action = architect repair or execution continuation
  final response forbidden

ELSE IF state.architect_finish_review_verdict != ARCHITECT_VERDICT_ALLOW_FINAL:
  stop_allowed = false
  next_action = reconcile state with architect review
  final response forbidden

ELSE IF architect evidence packet or bounded executor work-order packet is missing:
  stop_allowed = false
  next_action = run architect-only evidence collection or issue bounded work order and persist packet
  final response forbidden
```

Then run the ordinary stop logic:

```text
IF completion matrix has failed row:
  stop_allowed = false
  next_action = first failed row remediation
  continue

ELSE IF state.goal_reached != true:
  stop_allowed = false
  next_action = derive from plan/state
  continue

ELSE IF git boundary is required and not satisfied:
  stop_allowed = false
  next_action = commit/push or record explicit impossibility
  continue

ELSE:
  stop_allowed = true
  final report allowed
```

There is no separate stop permission for:

```text
I wrote the protocol
I committed
I pushed
I found the next step
I need more time
The session is long
The context was compacted
The review is done
The architecture repair is done
```

Those are continuation points.

## 19. Architect review output

Architect review must output:

```text
verdict: PASS_TO_EXECUTION | REPAIR_REQUIRED_CONTINUE | REAL_BLOCKER
goal_alignment: yes/no with reason
holes_found: exact list
files_to_patch: exact list
state_update_required: yes/no
matrix_update_required: yes/no
score_delta: number and reason
next_internal_role: EXECUTOR_INTERNAL_CONTINUE unless REAL_BLOCKER
next_action: exact plan-derived action
```

`REPAIR_REQUIRED_CONTINUE` is not a stop. It means repair and continue.

## 20. Two-pass loop

The architect repeats these passes until no new governance hole appears:

1. Plan coverage pass.
2. Skill and agent conflict pass.
3. State and matrix invariant pass.
4. Role-transition simulation pass.
5. Evidence-scope pass.
6. Soft-stop and blocker-misuse pass.

If a pass finds a hole, patch the governing file, update state/matrix, and rerun
the pass. Do not hand the problem back to Luba unless it is a real blocker.

## 21. Reference verification commands

Allowed governance verification commands:

```powershell
git branch --show-current
git status --short
git diff --check
rg -n "OP-28|Plan governance|role loop|soft stop|stop_allowed" AGENTS.md CLAUDE.md docs/HOW-TO-USE-SKILLS.md
rg -n "PLAN_AUTHOR_INTERNAL|ARCHITECT_INTERNAL_REPAIR|EXECUTOR_INTERNAL_CONTINUE" 43.LearningLocalWikiData
```

Allowed JSON verification:

```powershell
Get-Content -Raw <state-or-report>.json | ConvertFrom-Json | Out-Null
```

Forbidden during this protocol unless a Gate B explicitly requires it:

```powershell
dotnet test
dotnet build
npm test
npm run build
```

## 22. Acceptance for this protocol

This protocol is accepted only if:

1. It exists in `43.LearningLocalWikiData`.
2. `AGENTS.md` references it.
3. `CLAUDE.md` references it.
4. `docs/HOW-TO-USE-SKILLS.md` references it.
5. State records the new protocol coverage hole as closed.
6. Completion matrix records the new protocol row as passed.
7. `git diff --check` passes.
8. JSON state and matrix parse.
9. The change is committed and pushed if git is available.

After acceptance, this protocol becomes the reference for future architects, but
it still does not complete the project-local wiki plan. The next executor action
continues from the active state and first failed completion matrix row.

## 22.1. Architect-owned execution review

This protocol must verify that executor never owns the session.

Allowed control chain:

```text
ARCHITECT_INTERNAL_SESSION_OWNER
  -> ARCHITECT_ISSUES_BOUNDED_WORK_ORDER
  -> EXECUTOR_INTERNAL_WORKER
  -> EXECUTOR_RETURNS_WORK_ORDER_RESULT
  -> ARCHITECT_INTERNAL_REVIEW
  -> ARCHITECT_DECIDES_NEXT_WORK_ORDER_OR_FINAL_VERDICT
```

Forbidden control chains:

```text
EXECUTOR_INTERNAL -> final response
EXECUTOR_INTERNAL -> self review -> done
EXECUTOR_INTERNAL -> vague next step -> pause
ARCHITECT_INTERNAL -> hidden JSON review only -> final response
ARCHITECT_INTERNAL -> product/runtime/training code edit without work order
```

Every plan, skill, agent file, and state file must make this visible:

1. Architect is the session owner for multi-step work.
2. Executor authority exists only inside one bounded work order.
3. Work order scope names files, commands, forbidden actions, expected artifacts,
   state updates, commit/push expectation, return condition, and evidence packet.
4. Executor authority expires after the work order result or any pause/final
   attempt.
5. Architect final review must be visible in the user-facing response when Luba
   asks for architect, review, coverage, loopholes, settings, or why work
   stopped.
6. Persisted JSON is evidence only. It is not a role handoff and not a visible
   architect verdict.
7. Architect may edit governance directly but may not implement
   product/runtime/training code directly.

Detector:

```text
Any response, state, plan, or protocol lets executor choose finality, keep
session control after a slice, or replace visible architect review with a hidden
artifact.
```

Mandatory repair:

```text
1. Revoke executor authority.
2. Switch to ARCHITECT_INTERNAL_REPAIR.
3. Patch AGENTS.md, CLAUDE.md, protocol, plan, skill, or state where the loophole lives.
4. Record score_delta = -1000 for executor self-finalization or wrong-role finality.
5. Record the repaired hole in state and matrix.
6. Continue only by architect-owned bounded work order or architect final review.
```

Acceptance:

```text
session_owner_role == ARCHITECT_INTERNAL
executor_standing_authority == false
bounded_work_order_required == true
executor_final_response_forbidden == true
visible_architect_review_required == true
hidden_json_review_is_not_final == true
architect_product_code_execution_forbidden == true
```

## 23. HOLE-034 final repair summary stop

Detector:

```text
A user-facing final response reports that governance repair, commit, push, or diagnosis was completed while the active state has goal_reached=false, stop_allowed=false, or architect_finish_review_verdict != ARCHITECT_VERDICT_ALLOW_FINAL.
```

Why it is a loophole:

```text
The repair report becomes a soft stop even though the plan still has a next action. It converts architecture repair into a hidden stop gate.
```

Mandatory repair:

```text
1. Record HOLE-034 in state and completion matrix.
2. Apply -1000 score_delta for final_repair_summary_stop.
3. Patch AGENTS.md, CLAUDE.md, this protocol, and the active plan.
4. Keep ARCHITECT_INTERNAL as session owner.
5. Issue a bounded EXECUTOR_INTERNAL work order only if implementation is needed.
6. Persist the returned work-order result packet or architect-only evidence packet.
7. Update state.next_action from the packet or from the active plan if the packet is too vague.
8. Continue execution; do not send a final repair report unless exact ARCHITECT_VERDICT_ALLOW_FINAL is present after post-mutation review.
```

Acceptance:

```text
final_repair_summary_stop_forbidden == true
bounded_work_order_or_architect_evidence_after_repair_required == true
repair_checkpoint_not_final == true
state_goal_reached_false_blocks_final == true
```

## 24. HOLE-035 continuation without concrete next action

Detector:

```text
state.architect_finish_review_verdict == ARCHITECT_VERDICT_CONTINUE_EXECUTION
AND state.goal_reached == false
AND matrix.failed_count == 0
AND state.next_action does not name OP-30 final review or a concrete failed row/remediation.
```

Why it is a loophole:

```text
The executor is told to continue but has no concrete work item. This silently becomes a stop or vague status loop.
```

Mandatory repair:

```text
Either run canonical OP-30 final finish review immediately, or add a concrete failed matrix row and set state.next_action to that row's remediation. A generic continuation sentence is invalid.
```

Acceptance:

```text
continue_verdict_has_concrete_next_action == true
zero_failed_rows_routes_to_OP30_final_review == true
empty_or_generic_next_action_forbidden == true
```

## 25. HOLE-036 next-action source divergence

Detector:

```text
After a user challenge, state.next_action, last_plan_reminder.next_action, execution_attempt_latest.next_action, completion_matrix.next_action, first failed row, and latest report next_action fields do not converge.
```

Why it is a loophole:

```text
The executor can stop by choosing the most convenient next_action source and ignoring another source that still names work.
```

Mandatory repair:

```text
1. Reopen prior ALLOW_FINAL.
2. Set goal_reached=false and stop_allowed=false.
3. Record HOLE-036 in state and matrix.
4. Reconcile stale next_action fields to a single concrete next action.
5. Rerun OP-30 only after convergence, or continue the first concrete failed row.
```

Acceptance:

```text
next_action_sources_compared == true
next_action_sources_converged == true
stale_next_actions_invalidated == true
```
