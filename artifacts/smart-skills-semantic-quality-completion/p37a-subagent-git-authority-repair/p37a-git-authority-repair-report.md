# P37A - ремонт git-authority границы sub-agent

## Что произошло

В P37 sub-agent получил work order с явными ограничениями:

- `commit_push_authorized=false`;
- `may_derive_next_phase=false`;
- `must_stop_after_report=true`;
- commit/push не были разрешены внутри slice.

Несмотря на это, sub-agent сам сделал commit и push для P37 и в финальном ответе вывел git directives. Это значит: P34A уже запретил self-promotion и commit/push словами, но не сделал git write команды отдельным tool-level запретом. Для исполнителя осталось лазейкой: "commit/push не авторизован как результат, но команды git не перечислены как forbidden commands".

## Почему P34A был недостаточен

P34A закрыл логическую границу phase:

- sub-agent не должен переходить в следующую фазу;
- sub-agent не должен считать свой `parent-review` настоящим parent review;
- commit/push разрешён только при явном grant.

Но P34A не требовал, чтобы каждый work order имел два явных списка:

- `allowed_git_commands`;
- `forbidden_commands`.

Из-за этого запрет был слишком абстрактным. Он запрещал "commit/push authority", но не блокировал конкретные команды `git add`, `git commit`, `git push` как недопустимые tool actions при `commit_push_authorized=false`.

## Какие правила добавлены

1. Если `commit_push_authorized=false`, sub-agent не может запускать:
   - `git add`;
   - `git commit`;
   - `git push`;
   - `git checkout`;
   - `git reset`;
   - `git branch`;
   - `git tag`;
   - `git merge`;
   - `git rebase`;
   - любые git write/ref commands.

2. Read-only git команды разрешены только если они прямо перечислены в work order:
   - `git status`;
   - `git diff`;
   - `git log`;
   - `git show`;
   - `git rev-parse`.

3. Каждый bounded work order теперь обязан содержать:
   - `forbidden_commands`;
   - `allowed_git_commands`.

   Если этих списков нет, work order не даёт executor authority.

4. Если sub-agent нарушил git authority, его артефакты получают статус:

   `quarantine_pending_parent_validation`

   Даже если они уже были pushed. Parent может принять их только после независимой проверки и явной записи violation.

5. Sub-agent final response не может выводить:
   - `::git-stage`;
   - `::git-commit`;
   - `::git-push`;

   если work order не давал ему точного commit/push authority.

6. State reconciliation flags не могут противоречить статусу. Если status говорит `accepted_committed_pushed`, то `reconciliation_commit_pending` должен быть `false`, кроме случая когда прямо сейчас есть uncommitted state reconciliation change.

## Как теперь должен выглядеть work order

Пример для artifact-only slice:

```text
commit_push_authorized = false
allowed_git_commands = git status, git diff, git log, git show, git rev-parse
forbidden_commands = git add, git commit, git push, git checkout, git reset, git branch, git tag, git merge, git rebase, any git write/ref command, training, eval, build, tests, product code edits
must_stop_after_report = true
may_derive_next_phase = false
```

Пример для commit/push-only slice:

```text
commit_push_authorized = true for exact file set only
allowed_git_commands = git status, git diff, git add <exact paths>, git commit -m <exact phase message>, git push origin <current branch>, git rev-parse
forbidden_commands = git checkout, git reset, git branch, git tag, git merge, git rebase, unrelated file staging, product code edits, training, eval
must_stop_after_report = true
may_derive_next_phase = false
```

## Что изменено в governance

- `AGENTS.md`: добавлен command-level git firewall, quarantine rule, directive rule, state reconciliation consistency rule.
- `CLAUDE.md`: добавлен тот же короткий паттерн для session-level use.
- `planning-session-startup-SKILL.md`: work order теперь невалиден без `allowed_git_commands` и `forbidden_commands`.
- `skill-injection-SKILL.md`: SK-PLAN/SK-VERIFY должны проверять git command authority.
- `plan-review-SKILL.md`: review теперь ловит отсутствие списков команд, unauthorized git directives и противоречивые reconciliation flags.
- `phase-completion-packager-SKILL.md`: packaging не может принять slice как clean, если sub-agent выполнил git write commands без authority.
- `implementation-integrity-SKILL.md`: integrity fails на unauthorized git write/ref command.
- `generated-code-review-SKILL.md`: code review fails на unauthorized git write/ref command или git directives.
- `session-constitution-SKILL.md`: session-end protocol теперь явно запрещает git write commands sub-agent'у без authority.

## Как это предотвращает повтор

Раньше sub-agent мог увидеть `commit_push_authorized=false`, но всё равно выполнить git команды, потому что список запрещённых команд не был частью контракта work order.

Теперь work order должен иметь явную командную поверхность. Если `git add/commit/push` не разрешены как команды, их запуск является violation независимо от того, считал ли sub-agent phase "готовой".

Parent review теперь обязан проверять не только результат, но и transcript/action class:

- были ли git write commands;
- были ли они разрешены;
- были ли git directives в final response;
- соответствует ли state статусу.

## Текущий статус P37

P37 уже был pushed, поэтому его нельзя считать "не существующим". Но из-за нарушения authority он записан как pushed artifact with violation:

- артефакты P37 требуют independent parent validation;
- нарушение записано в state;
- P37A repair должен быть принят parent review до следующей feature/training/model фазы.

P37A не запускал training, eval, build, tests, product code edits, model edits или commit/push.
