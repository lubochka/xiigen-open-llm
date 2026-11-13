# MICRO-N28: быстрый проверочный эксперимент для TranslationProductHeldOutRunner

**Дата:** 2026-10-16. **Статус:** Python logic proxy EXECUTED; native C# NOT_RUN; real-model learning NOT_MEASURED; independent CODEX/Astra 6 UltraCode review NOT_RUN. Это подготовленный опыт, а не объявленный рост качества.

## Почему этот опыт важен

Текущий `TranslationProductHeldOutRunner` проверяет маркеры и письменность, но его `ProductMatch` не проверяет семантическую эквивалентность ответа эталону. Даже когда смысловые роли переставлены или пропало отрицание, оценка может оказаться истинной. Само по себе это не является причиной RP01 neural 0/3, 0/4; это отдельный observer для structured translation Unit.

Привязка: core `d5c2409b24292d361593b8262b0c54997b15f539`, [основной source](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L141-L198) и [другой direction runner](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L324-L380).

## Стадия 0: 4 коротких контрпримера

Приложены [исполняемый Python script](28-PRODUCT-HELDOUT-SCORER-PROXY.py) и [машинная квитанция](28-PRODUCT-HELDOUT-SCORER-PROXY-RESULT.json). Команда из `possibleDifs`: `python 28-PRODUCT-HELDOUT-SCORER-PROXY.py`. Python стандартная библиотека; ни native C#, ни Student не запускаются.

| ID | Вход RU→EN | Эталон | Предположенный неправильный результат | Маркеры | Проверяемый закон |
| --- | --- | --- | --- | --- | --- |
| A | Привет мир | Hello world | banana | нет | предмет ответа |
| B | Не удаляй файл | Do not delete the file | Delete the file | required delete, file | отрицание |
| C | Собака кусает человека | dog bites man | man bites dog | required dog, man | subject/object order |
| D | Привет 🙂 | Hello 🙂 | Hello | emoji markers не заданы | сохранение emoji |

Результат Python proxy: 4/4 `product_match=true` и 0/4 `exact=true`. Для C оба bag-of-words `bleu1` и char bag `chrf1` дают 1.0. Для D условие `(EmojiMarkers??[]).All` с пустым набором возвращает `true`. Это демонстрирует расхождение формальных критериев, а не измеренный процент ошибок живой системы. Ceiling sanity: `AccuracyBefore=0.99, After=1.00` не удовлетворяет условию `After>=Before+0.02`.

## Стадия 1: один native C# scorer A/B, без обучения

**Actor:** CODEX с моделью Astra 6 UltraCode. Исследователь сначала перечитывает source на точном SHA и проверяет действительную вызывающую цепочку. В существующем тестовом проекте или разрешённом изолированном harness воспроизвести чистую функцию `ProductMatch` на 4 исходных и ещё 4 заранее утверждённых позитивных перефразированиях. Если score вычисляется внутри runner, извлечь **побитно сопоставимую формулу** без изменения основного кода, чтобы не выдать refactor за причинный результат.

Сравниваются две оценки на **тех же неизменных восьми outputs**:
- A = текущие `ProductMatch, ExactMatch, Bleu1, ChrF1`.
- B = law-aware scorer, независимо проверяющий сохранение negation, subject/object, чисел, emoji и сущностей, допускающий истинно эквивалентные перефразы. Вывод CODEX/Astra UltraCode по закрытым labels обязателен, если утверждается семантическое качество.

Главный фальсификатор: A принимает четыре неверных отрицательных outputs, B их отклоняет, при этом B принимает четыре допустимых положительных перефразирования. Отдельно проверить `IsTargetScriptOk`, `CopiedSource` и `BOOTSTRAP` как сохранённые baseline инварианты, не ослаблять их при починке смыслового слоя. Если native A не совпал с proxy — `SOURCE_PROXY_DISAGREEMENT` и остановка; если B принимает смысловую ошибку или отвергает правильные перефразирования — `FIX_NOT_ACCEPTABLE`.

Фиксировать для каждой строки `caseId/source/expected/actual/markers/exact/A/B/why/trace`; два независимых denominators false accept и false reject; версии source, evaluator, SHA, elapsed/memory, commands/tests. **Меняется только scorer, не KNN, веса или training data.**

## Стадия 2: условный miniature end-to-end Unit, не RP01 Student

Только если native стадия 1 подтвердилась и consumer реально используется, взять 6–12 согласованных train-ready Unit rows и 8–12 blind holdout, разделённых **по исходным смысловым семьям**, а не только по точному тексту. Выполнить одинаковые law-exams на `TranslationMachine` до и после `TrainStructuredBatchAsync` и после fresh-process `Export/Restore`, проверить полный путь `learned Unit → TranslationRoutingFlow → ordinary output → scorer A/B → CODEX judge`. Включить отрицание, порядок ролей, однозначный словарь и unrelated-domain retention. Если реальный consumer не связан — установить `OBSERVER_ONLY` вместо выдуманного RP01 root cause. Не запускать Windows full CI/500 пар ради этого вопроса.

**Обязательное распределение ролей, если понадобится НОВЫЙ DPO:** выбор предмета/урока и человеческой предпочтительной задачи выполняет **CODEX/Astra 6 UltraCode**; разрешённая внешняя **открытая китайская модель создаёт обе окончательные стороны `chosen` и `rejected`**; отдельный **CODEX/Astra 6 UltraCode независимо оценивает качество именно этой пары**. Стадия 0 — исключительно диагностический fixture, не генерация DPO и не работа внешней модели. Никакого фиктивного agent receipt.

## Work order и ограничения

- **CURRENT TASK:** source pin, native score reproduction, один фактор scorer A/B, семантический blind exam, небольшой consumer trace при наличии вызова.
- **NOT THE TASK:** обучение новой LoRA, 500 DPO, массовая CI, переписывание frozen исторических results, автоматическая promotion.
- **REQUIRED SKILLS:** `docs/skills/human-readable-architecture-audit-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `generated-code-review-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `conversation-intent-and-self-reflection-SKILL.md`, `experiment-design-SKILL.md`, `claim-verification-SKILL.md`.
- **ALLOWED DOC WRITES:** только `xiigen-planning/llm_plan3/llm mvp/design to code session/replanning/possibleDifs`. Для последующего изменения кода нужен отдельный авторизованный executor.
- **FORBIDDEN WRITES:** `llm_mvp_core/docs/plans/replanning-possible-diffs`, замороженные эксперименты/checkpoints и несвязанный продуктовый код.
- **REQUIRED EVIDENCE:** exact commit, source:line, 8 frozen cases SHA, output × scorer table, positive/negative controls, source-family split, actual consumer path, current/gold semantic judge, независимый review receipt или `NOT_RUN`, restore identity/metrics если стадия 2 выполнена.
- **PLAN REVIEW:** без согласованного native результата и independent CODEX review карточка `DRAFT_FOR_REVIEW`, не `ACCEPTED`.

**Честная граница:** `PassesGate` здесь может быть ложно высоким/ложно низким, но [`PassesPromotionGate=false`](https://github.com/lubochka/llm_mvp_core/blob/d5c2409b24292d361593b8262b0c54997b15f539/27.TranslationDomain/TranslationProductHeldOutRunner.cs#L249-L267) — нет доказательства прямой автоматической promotion. Попадание этого scorer в neural RP01 также `UNVERIFIED`.
