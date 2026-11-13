# Добавление узких тем оркестрации: источники, ограничения и проверка

Дата: 25.10.2026. Работа добавляет пять микроцелей к существующим карточкам. В текущем снимке было семь узких целей оркестрации; функциональные критерии, БД, server и mobile имели родительские карточки, но не отдельные curriculum-цели. Пятая новая цель расширяет автоматизацию от одной длительной работы к завершению группы.

| Цель | Поддомен | Первый run | Reference-пары |
|---|---|---|---|
| OR-FUNC-ACCEPTANCE — Наблюдаемый критерий без изменения требования | `or.functional` | `O301` | 6 |
| OR-DB-ATOMIC-CLAIM — Следующий шаг по доказанному владению операцией | `or.database` | `O302` | 6 |
| OR-SERVER-OUTBOX — Серверный переход между сохранением и доставкой события | `or.server` | `O303` | 6 |
| OR-MOBILE-ACK — Закрытие локальной операции только по подходящему подтверждению | `or.mobile` | `O304` | 6 |
| OR-AUTO-FANIN — Сбор группы только по её составу и политике завершения | `or.automation` | `O305` | 6 |

## Использованные источники

Прочитаны закреплённые выдержки существующего RAG, перечисленные ниже. Полные документы вне этих фрагментов не объявлены проверенными. Новые внешние источники не добавлялись. Отрывки берутся из разрешённого репозитория `lubochka/xiigen-planning`, веток `main` и `llm_plan3`.

- **RAG-001 — Flow files generator/GUIDE-B11-FLOW-UI-AUTOMATION-JSON.md**. [main, commit 3d1fa938d4b50df5d430283983f45b2c0d2c4e5c](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B11-FLOW-UI-AUTOMATION-JSON.md); blob `563babdd273065251b1bea86a858e1c7e822b7f3`; фрагменты 38–84, 106–163, 240–257. Использовать для OR-AUTO: выбор следующего шага, UI↔server, завершение и ожидание. Имена инструментов и жёсткие UI-гейты относятся к историческому проекту, не к новому учебному runner.
- **RAG-004 — Flow files generator/GUIDE-B12-DESIGN-SIMULATION-R1-MD.md**. [main, commit 3d1fa938d4b50df5d430283983f45b2c0d2c4e5c](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B12-DESIGN-SIMULATION-R1-MD.md); blob `6ce0a6981c8e12cd36cb028ee4e5b361676966f9`; фрагменты 273–315, 418–430. Уроки OR-DESIGN/ARCH: один изменяемый фактор, полные входы/выходы, объяснимая зависимость. Исторический qualityScore — аннотация документа, не новый измеренный результат.
- **RAG-005 — Flow files generator/GUIDE-B17-IMPLEMENTATION-PLAN.md**. [main, commit 3d1fa938d4b50df5d430283983f45b2c0d2c4e5c](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B17-IMPLEMENTATION-PLAN.md); blob `73e234e3df128e8d976af0ec15ded7bf7d15aba1`; фрагменты 81–105, 124–143, 333–349. Уроки OR-CODE: исполнимый ограниченный work order. Фазы полного внедрения, approval и глобальные тестовые гейты не запускать для подготовки данных или ещё не доказанного опыта.
- **RAG-006 — Flow files generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md**. [main, commit 3d1fa938d4b50df5d430283983f45b2c0d2c4e5c](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B28-STEP-8-HANDOFF-CONTRACT.md); blob `2f37cc0b56f1a11579fdd8fcbae9cf8b44f0b97f`; фрагменты 66–74, 137–181. Уроки OR-ARCH: не терять поля и потребителя при передаче. Не объявлять Nest-декораторы универсальным C#/.NET-контрактом.
- **RAG-008 — Flow preparation/XIIGEN-GAP-SIMULATION-PROTOCOL.md**. [main, commit 3d1fa938d4b50df5d430283983f45b2c0d2c4e5c](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20preparation/XIIGEN-GAP-SIMULATION-PROTOCOL.md); blob `1cd891e2b68454c1af2d3a0ea565f4c3ec26dee1`; фрагменты 8–62, 425–454. Контрасты OR-AUTO: ожидать все необходимые handle, затем собирать результат. Упомянутые модели и недостающий handler описывают исторический снимок, не текущую среду Kiro.
- **RAG-017 — llm mvp/learning-sets/FLOW-01-REGISTRATION-RUNTIME-TRAINING-DATA-v2.md**. [llm_plan3, commit b242c6f9efc2e3a0a01301c2875751cfd57ef83b](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/learning-sets/FLOW-01-REGISTRATION-RUNTIME-TRAINING-DATA-v2.md); blob `ab3eb51064898956f736277e45c7033bd392c33e`; фрагменты 212–227, 493–529, 1631–1664. Симуляции OR-MOBILE/BACKEND/DB и контрасты порядка событий. Числа/конкретные правила регистрации задавать в scenario input, а не превращать историческое бизнес-правило в универсальное.
- **RAG-047 — Flow files generator/GUIDE-B40-LIVE-RUN.md**. [main, commit 8d3b4c600f458363e4a54ddce83e91000efd0e8a](https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B40-LIVE-RUN.md); blob `68f5dcf5fd1c77e7ccfc3ff07b50bc85e2b90e55`; фрагменты 182–218, 282–304. Для OR-CODE-PROMPT-CONTEXT обучать выбор следующего шага подготовки handoff при известном потерянном поле, не угадывая его. Для автоматизации различать запрошенный run и чужой результат. Список провайдеров, старые модели, пороги и синтетические шаблонные числа не наследуются. Наличие отчёта не доказывает обучение.
- **RAG-OR-001 — Flow files generator/GUIDE-B21-STEP-1-INVARIANTS.md**. [main, commit 8d3b4c600f458363e4a54ddce83e91000efd0e8a](https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B21-STEP-1-INVARIANTS.md); blob `a32f0f667f73d85c205aff262c7b06942ca6527a`; фрагменты 82–123, 180–195, 257–297. Метод построения синтетических условий OR-FUNC/DB/SERVER. Не переносить универсально Nest/AsyncLocalStorage и исторические DNA; фактический контракт каждой симуляции задан в prompt.
- **RAG-OR-003 — Flow files generator/GUIDE-B46-DESIGN-SIM-CLIENT-SCREENS.md**. [main, commit 8d3b4c600f458363e4a54ddce83e91000efd0e8a](https://github.com/lubochka/xiigen-planning/blob/8d3b4c600f458363e4a54ddce83e91000efd0e8a/Flow%20files%20generator/GUIDE-B46-DESIGN-SIM-CLIENT-SCREENS.md); blob `c4027c2bcb8634348900112596554f31ccde3789`; фрагменты 77–200. Метод синтетических OR-CLIENT/DESIGN. Требования прежнего UI-стека не превращаются в API новой модели; request/version правила даны отдельно в условии пары.

## Как разрешены конфликты

- Старые форматы с `[WRONG]` не попадают в learner output: причина ошибки хранится в metadata/rationale.
- Указание старых навыков о разных моделях chosen/rejected не отменяет текущую задачу генерации одной выбранной китайской моделью Kiro. Происхождение записывается честно, независимость генераторов не выдумывается.
- Исторические fixed confidence, обязательные production-гейты и автоматическое продвижение не используются как новые измерения или разрешение на production-изменение.
- Track A/Track B из старых документов не смешиваются: новые задачи — предметные решения в своих поддоменах; роутер обучается отдельно. Новые JSON действия — контракты симуляций, а не проверенные имена native API.
- Источник с правилом store-before-enqueue не доказывает атомарный outbox или exactly-once. Атомарность, смысл ACK и поведение повторов явно задаются внутри конкретного prompt.
- Mobile accepted/applied и DB lease/CAS ниже не объявлены существующим кодом XIIGEN. Это прозрачные причинные условия новых симуляций, полезные для последующей проверки обучения.
- Один узкий набор содержит разные классы своего решения. Наличие слова SQL/перевод/мобильный в запросе само по себе не меняет учебную тему; выбор домена принадлежит отдельному router-набору.
- Не создаются результаты обучения, фиктивные метрики или кандидаты fixes из структурной проверки пакета. После реального полезного учебного результата кандидат будет документирован отдельно.
- Старая история не удаляется. Новые карточки ссылаются на стабильный stream ID и каноническую очередь, не копируют меняющийся порядковый номер следующей задачи.

## Проверка этих материалов

Локально проверены уникальность 30 новых IDs, наличие пяти целей по шесть примеров, разбор JSON обеих альтернатив, одинаковый набор полей target, ровно одно различающее поле в каждой паре, отсутствие равных chosen/rejected, новые run IDs O301–O305 и неизменность source snapshot. Это проверка подготовки данных; семантическая аттестация реального сгенерированного корпуса, запуск китайской модели, native oracle и обучение не выполнялись.

В каждой паре решающий факт доступен во входе. Контрпримеры задают предел применимости; история контрфактического случая остаётся одним семейством. Результаты и старые reference не перезаписываются.

## Исправление происхождения после независимого ревью

У OR-AUTO-FANIN-REF-04 и OR-AUTO-FANIN-REF-05 один исходный двухветочный сценарий success/failure, меняется только политика частичного результата. Обе пары имеют общее семейство `v3-ref.or-auto-fanin.f04`. OR-AUTO-FANIN-REF-01 и OR-AUTO-FANIN-REF-02 также являются контрфактической парой running/terminal одной группы и разделяют `v3-ref.or-auto-fanin.f01`. Для каждой группы совпадают origin/leakage group в плоском JSONL. Имена G1/G2 и G4/G5 не доказывают независимости.

OR-MOBILE-ACK-REF-01 сохраняет origin/derived_from прежнего OR-MOBILE-01-REF-001, OR-MOBILE-ACK-REF-03 — прежнего OR-MOBILE-01-REF-003, OR-DB-ATOMIC-CLAIM-REF-02 — прежнего OR-SERVER-01-REF-001. Эти уточнения существующих опор не выдаются за независимые исходные истории. Всего 30 пар и 28 объявленных family ID; число ID не является числом независимых источников.
