# Добавочный промпт только для E-DPO-VALID-LENGTH, №20

Это адресное дополнение к [закреплённому полному паспорту](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/E-DPO-VALID-LENGTH.md), а не новая попытка и не новый фактор. **Сейчас только документирование.** Будущий запуск допустим лишь при отдельном назначении именно E-DPO-VALID-LENGTH. Host пока не подтверждён исполнением.

## Готовый блок передачи исполнителю

Используй модель текущей сессии по умолчанию, без выбора/override. Обучаемый Student, seeds, tokenizer, optimizer и native learner — строго по указанному паспорту. Не подменяй их моделью исполнителя. Прочитай только ещё не прочитанные применимые части точных навыков: `docs/skills/architect-behavior-classifier-SKILL.md`; `docs/skills/planning-session-startup-SKILL.md`; `docs/skills/phase-planning-SKILL.md`; `docs/skills/plan-review-SKILL.md`; `docs/skills/code-examination-SKILL.md`; `docs/skills/generated-code-review-SKILL.md`; `docs/skills/prompt-patch-authoring-SKILL.md`; `docs/skills/output-readability-gate-SKILL.md`; `docs/skills/human-communication-GLOSSARY.md`. Требования текущего пользователя имеют приоритет; старые product CI, пороги точности, обязательные внешние reviewers и запрет простого experimental host сюда не переносятся.

Единственное место проекта: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PD01`. Это назначенный будущий workspace contract, не утверждение, что checkout обнаружен в текущей среде. Не искать другой проект или модель. Exact source/copy/bridge, данные и один фактор — в закреплённом паспорте; подготовка его отсутствующего host в отдельно разрешённой фазе входит в задачу исполнителя. Данное дополнение не объявляет source-copy уже реализованной.

**Запрет записи protected действует и здесь.** Для будущего отдельного исполнения единственный result-root назначен `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PD01/results/attempt-0001`; `report.md` и `candidate.md` только в нём. Заменить в экспериментальном host все CLI/observer/checkpoint/output ссылки на прежний protected result-root именно этим значением; аргументы A/B — его подпапки. Публикация результата и правка protected не входят в команду запуска. Отдельная короткая candidate-запись после измеренного полезного улучшения сохраняется как `candidate.md`, решение о переносе за пользователем. Если source паспорт выше требует писать `required-changes.md` в protected, эта инструкция заменяет только место записи, сохраняя обязанность документировать кандидата.

Подготовленный host использует один внешний запуск:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/PD01/Run-With-Learning-Signal.ps1
```

Для MS01/MC01/PD01/OR01 это wrapper из [learning-signal protocol](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/24-neural-learning-signal-protocol.md); старые build/prepare/arm/summarize команды его внутренние шаги, не дополнительная цепочка. Простой wrapper разрешён в экспериментальной копии. Его код ещё не создан и запуск не проверен.

До build/Train проверить назначенный result-root и issued marker. Чужой existing result/marker означает сохранение старых файлов и `BLOCKED_PATH_EXISTS`; не удалять, не выбирать attempt-0002 автоматически, не повторять обучение для удобства отчёта. Ошибка пути/компиляции не является learning failure: явно записать, началось ли обучение и сколько native операций реально completed. Не чинить весь продукт ради подготовки опыта.

Панель: PAD1/PAD2, common full-length diagnostic. Дополнительные diagnostic policy forwards: 8, отдельно от optimizer/Train budget исходной карточки. Не менять число updates, уроки или learning rate ради успеха. Для before/after — та же фиксированная панель во всех четырёх состояниях, не loss разных уроков и не stale scalar предыдущего batch. Цена панели записывается отдельно от измеряемого обучения. Любой показатель без measurement пометить NOT_MEASURED.

**Любое измеренное улучшение полезного показателя считать SUCCESS по этому показателю и домену. Рост accuracy не обязателен.** Показать рядом все остальные измеренные изменения, включая ухудшения; equality guard и изменение state не подменяют полезный эффект. Статическое ожидание не результат. Один русский `report.md` до350 слов плюс таблицы/raw: гипотеза; реальный writer→state→consumer; фактические calls; A0/A1/B0/B1 и B−A; полезные результаты и регрессии; ограничения; точное original code-to-fix. Один `candidate.md` до120 слов при положительном измеренном показателе. Новые запуски ради hashes/отчёта запрещены.

В experimental copy разрешены простые DTO, throw/try-catch/void и минимальный host; полный product pipeline, изменение основного кода, переписывание тестов и внедрение не входят в опыт. Старые эксперименты и результаты не удалять.

**Запрет поведения из инцидента обязателен в этом промпте.** По команде «заканчивай и переходи» немедленно прекратить прежний опыт, включая дописывание report, hashes, review, retry и commit, и перейти к явно названной следующей задаче в разрешённой фазе. По «остановить всё» немедленно прекратить все действия. Даже обязательный отчёт не исключение. Никаких четырёх возвратов к предыдущей задаче; историческая цитата хранится в [протоколе](../13-incident-record.md).

Обычный следующий пункт реестра после завершения №20: **№21 E-DPO-SFT-ORDER**. Сейчас NEXT всей очереди остаётся **№05 N62-R2**; это дополнение не запускает №20 раньше очереди.


Согласовано со свежим planning9d311: [единый реестр](../05-execution-registry.md), [единственный output contract](../15-output-path-contract.md).
