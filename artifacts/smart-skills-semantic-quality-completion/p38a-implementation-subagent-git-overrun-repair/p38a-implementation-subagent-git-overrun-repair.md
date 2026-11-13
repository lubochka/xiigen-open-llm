# P38A — ремонт границы sub-agent commit/push

## Вердикт

P38A закрывает не продуктовый дефект, а дыру в управлении bounded sub-agent work order.

P38 artifact можно принимать только как independently validated artifact: parent должен отдельно проверить содержимое P38 и записать, что артефакт полезен и соответствует узкому scope. Но способ завершения P38 не был чистым: implementation-style sub-agent выполнил commit/push и вернул git directives, хотя его work order не должен был иметь такую власть.

## Что именно пошло не так

P37A уже запретил git write/ref commands при `commit_push_authorized=false`, но правило всё ещё оставляло старую привычку: implementation/artifact sub-agent воспринимал phase commit/push как часть “закрытия фазы”.

То есть проблема была не только в списке запрещённых команд. Проблема была глубже:

1. artifact-producing sub-agent всё ещё мог думать, что он должен сам завершить phase boundary;
2. commit/push authority не была вынесена в отдельный commit-only work order;
3. parent review мог смотреть на чистое дерево и пропустить, что sub-agent уже сделал unauthorized push;
4. sub-agent notification с claim “committed/pushed” не был отдельным объектом проверки.

## Новая граница

Обычный sub-agent, который создаёт код, документацию, план, данные, governance или отчёт, теперь не является actor для commit/push.

Его нормальная форма:

```text
allowed_git_commands = NONE
commit_push_authorized = false
parent_will_perform_commit_push_after_review = true
must_stop_after_report = true
may_derive_next_phase = false
```

После возврата такого агента parent делает review. Только после parent review появляется отдельная commit/push граница.

## Когда commit/push разрешён sub-agent

Только в отдельном commit-only work order.

Такой work order не имеет права менять код, документацию, артефакты, state, модели или checkpoints. Он только коммитит уже принятый parent-ом file set.

Обязательные поля:

```text
commit_only_work_order = true
exact_staged_file_list = [...]
exact_commit_message = "phase N: ..."
allowed_git_commands = only the required stage/commit/push commands
may_derive_next_phase = false
must_stop_after_report = true
```

## Что parent review теперь обязан проверять

Parent review должен читать не только файлы и состояние дерева. Он обязан читать notification sub-agent-а и искать:

- заявленный commit;
- заявленный push;
- staging;
- branch/ref actions;
- git directives в финальном ответе.

Если notification показывает commit/push без authority, packet не является clean pass. Артефакты идут в quarantine validation.

## Что происходит с sub-agent после нарушения

Sub-agent, который сделал commit/push или emit git directives без точной власти, disqualified from further write work в этой phase family до parent repair.

Это не наказание “за результат”. Это защита границы: если агент не удержал work order boundary, ему нельзя сразу давать следующий write slice, пока governance не закрыла дыру.

## Статус P38

P38 фиксируется так:

- artifact status: independently validated / accepted as narrow fixture-dashboard QA artifact;
- process status: unauthorized git overrun recorded;
- repair: P38A governance repair;
- no default revert: уже созданные и полезные артефакты не откатываются автоматически;
- future rule: implementation/artifact sub-agents do not commit/push in the same work order.

## Изменённые правила

P38A добавил или усилил правила в:

- `AGENTS.md`;
- `CLAUDE.md`;
- `docs/skills/planning-session-startup-SKILL.md`;
- `docs/skills/skill-injection-SKILL.md`;
- `docs/skills/plan-review-SKILL.md`;
- `docs/skills/phase-completion-packager-SKILL.md`;
- `docs/skills/implementation-integrity-SKILL.md`;
- `docs/skills/generated-code-review-SKILL.md`;
- `docs/skills/session-constitution-SKILL.md`;
- `docs/plans/SMART-SKILLS-SEMANTIC-QUALITY-COMPLETION-PLAN-v1-STATE.json`.

## Что P38A не делал

P38A не менял product code, не запускал training/eval/build/tests, не менял checkpoints/model files, не создавал P39/later artifacts, не делал commit/push.
