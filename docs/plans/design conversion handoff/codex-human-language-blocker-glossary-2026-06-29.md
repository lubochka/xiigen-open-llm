# HUMAN_LANGUAGE_BLOCKER_GLOSSARY - 2026-06-29

Цель: когда Codex пишет Luba-facing report, сначала надо сказать по-человечески, что происходит, что мешает, что можно сделать дальше, и только потом добавлять машинный label.

`Human-language blocker reports before machine labels`

## Главное правило

`Proof readiness items are tasks, not stop blockers`

Пункты готовности к доказательству - это задачи, которые надо подготовить и выполнить. Их нельзя превращать в туманное "мы заблокированы" или финальную остановку, если можно безопасно сделать планирование, проверку, dry-run, сценарий или decision-prep.

`Do not call product proof tasks blockers when they are executable planning or verification work.`

## Словарь простыми словами

Source branch - ветка-источник. Это ветка, с которой сверяются и в которую Luba потом может решать мержить. Для Codex это ориентир и read-only authority, а не место для рабочих коммитов.

Work branch - рабочая ветка Codex. Это отдельная ветка, например `DesignToCode`, где Codex может делать изменения только если это прямо разрешено текущим work order.

Mergeability check - проверка, сможет ли работа аккуратно влиться в source branch. Безопасная форма - read-only проверка или local dry-run. Это не настоящий merge, не push и не PR mutation.

Blocker - реальное препятствие, которое нельзя безопасно обойти без решения Luba или внешнего условия. Например: нужны учетные данные, запрещено трогать ветку, нет доступа к сервису.

Task - работа, которую можно безопасно продолжить. Например: написать сценарий проверки, подготовить decision packet, сделать read-only анализ, описать варианты, подготовить evidence filenames.

Class B - обычно это не "все остановилось", а задача с понятным следующим шагом. Надо объяснить: что не готово, какой безопасный work order это чинит, какой результат будет доказательством.

Class C - обычно это внешний или owner-decision вопрос. Надо объяснить, какое решение нужно от Luba, какие есть варианты, что можно подготовить до решения, и чего нельзя делать без разрешения.

Readiness - готовность к следующему доказательству. Это не само доказательство. Если readiness не закрыта, следующий шаг - подготовить или выполнить проверочную задачу.

Proof - доказательство, которое можно показать: команда, лог, screenshot, файл evidence, readback, dry-run result, browser trace или другой конкретный артефакт. Proof должен соответствовать claim scope.

External AI judge - внешний AI-оценщик качества или visual fidelity. Это нельзя запускать через секреты, аккаунты или provider без отдельного разрешения. Если нужен внешний judge, сначала нужен decision-prep для Luba.

Visual fidelity - насколько результат визуально совпадает с целевым дизайном. Это доказывается screenshots, сравнением, checklist или approved judge flow. Просто сказать "выглядит хорошо" недостаточно.

Clean repo - понятное состояние репозитория: что изменено, что staged/unstaged/untracked, что принадлежит текущей работе, что нельзя чистить без решения Luba. Dirty repo не всегда blocker; часто это task на classification или decision-prep.

Runtime proof - запуск кода или сценария с конкретным ожидаемым результатом. Если запуск не разрешен, это не proof, а задача на prep или approval.

Browser proof - проверка UI в браузере с точными шагами: куда нажать, что ввести, что должно быть видно, какие screenshots сохранить. Если нужен login, сначала session/auth preflight, без захвата секретов.

Clean-worktree proof - доказательство, что рабочее дерево в ожидаемом состоянии. Если есть чужие или generated файлы, следующий шаг - owner decision packet, а не автоматический cleanup.

## Как писать Luba-facing blocker report

1. Сначала по-русски: что именно мешает или не доказано.
2. Затем: это blocker или task.
3. Затем: что можно сделать безопасно прямо сейчас.
4. Затем: что требует решения Luba, если требует.
5. Только после этого: machine label, Class B/Class C, state code или packet name.

Пример:

"Визуальное совпадение еще не доказано. Это не стоп-блокер: можно подготовить сценарий screenshots и список ожидаемых состояний. Внешний AI judge требует отдельного решения Luba, потому что может потребовать provider/account/secrets. Machine label: Class B для локальной visual proof task, Class C только для external AI judge approval."

## Next safe followups

- Подготовить очередь proof tasks: runtime, browser, visual, clean repo, mergeability dry-run.
- Для каждого proof task указать allowed tools, forbidden actions, expected evidence files и downgrade rules.
- Для external AI judge подготовить отдельный decision-prep: зачем нужен, какие данные уйдут наружу, какие есть локальные альтернативы, какая точная approval phrase нужна.
- Для clean repo подготовить owner-facing classification: какие изменения относятся к текущей работе, какие чужие, какие generated, что можно оставить, что требует решения Luba.

## Boundary

Этот glossary не доказывает product readiness, visual fidelity, clean repo, mergeability, runtime/browser success или PR readiness. Он только переводит машинные labels в простой язык и задает правило: proof readiness items are tasks, not stop blockers.
