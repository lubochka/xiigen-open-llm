# Протокол текущего задания и адресные дополнения к промптам

Снимок core `883496bca18113bf37e0afe683c75b05a0075048`; текущая фаза — документирование результатов и малых будущих экспериментов. Protected только читается. Здесь разрешаются противоречия инструкций, чтобы исполнитель не выбирал сам модель, путь, приоритет или трактовку успеха.

## Применённые навыки и границы

В работе использованы роли архитектора, планирования, проверки плана и кода, авторства промптов и человеческой коммуникации из указанного пользователем `docs/skills`. Основные точные источники для передачи:

- [docs/skills/architect-behavior-classifier-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/skills/architect-behavior-classifier-SKILL.md)
- [docs/skills/planning-session-startup-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/skills/planning-session-startup-SKILL.md)
- [docs/skills/phase-planning-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/skills/phase-planning-SKILL.md)
- [docs/skills/plan-review-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/skills/plan-review-SKILL.md)
- [docs/skills/code-examination-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/skills/code-examination-SKILL.md)
- [docs/skills/generated-code-review-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/skills/generated-code-review-SKILL.md)
- [docs/skills/prompt-patch-authoring-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/skills/prompt-patch-authoring-SKILL.md)
- [docs/skills/output-readability-gate-SKILL.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/skills/output-readability-gate-SKILL.md)
- [docs/skills/human-communication-GLOSSARY.md](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/skills/human-communication-GLOSSARY.md)

Уже прочитанное не обходить повторно ради формального счётчика. Число исследовательских проходов не заменяет результат проверки; этот аудит не заявляет неисполненных reviews или test passes. Модель исполнителя — текущая модель сессии по умолчанию. Термины: домен — предметная тема (C#, перевод, Wiki); EN→RU — профиль направления; LoRA и kNN — механизмы обучения; Unit и Flow — архитектурные сущности. Ни то ни другое не является названием домена.

## Все четырнадцать уточнений пользователя

| № | Обязательное решение в этом пакете |
|---|---|
| 1 | Каждый новый опыт имеет один фактор, буквальные данные/панель, точный native learner, маленький бюджет, исходный и изменённый варианты; доменная применимость обозначена. |
| 2 | Любое измеренное улучшение полезного показателя — успех этого показателя/домена. Accuracy не gate. Все прочие измеренные изменения, включая ухудшения, рядом. |
| 3 | Самостоятельный замер несвязанной функции не принимается. Требуется настоящий update/Train, фактический TRAIN_INPUT либо доказанный learned-state consumer. |
| 4 | Loader/restore/reader допустимы при конкретной зависимости от результатов обучения: writer→state→consumer и original code-to-fix. Два декоративных урока перед benchmark не создают связь. |
| 5 | Учебные блокировки идут раньше зависимых проверок; документация сейчас не превращается в новый runtime запуск. |
| 6 | Сначала изолированная experimental copy и сохранённый результат. Полный перенос, исправление product tests/CI и прямое изменение original source не назначаются до решения пользователя. |
| 7 | В новых карточках точные paths/project/SDK/package/source bodies/CLI/budget/retry rules. Default executor model не требуется выбирать. Неоттестованный host прямо назван будущим. |
| 8 | Старые эксперименты, raw, неудачные попытки и partial files сохраняются. Изменения добавочные; никаких удалений. |
| 9 | Порядок: разблокировать обучение → разрешить конфликты карточек → применить присланные уточнения → зависимые readers в конце. NEXT из свежего индекса —05, не04. |
| 10 | В изолированном эксперименте допустимы throw/try-catch/void, простые DTO и минимальный host. Это не разрешение переносить экспериментальные исключения в продукт. |
| 11 | Сейчас документируем измеренные улучшения C-01…C-04 как кандидаты. Пользователь решает, что внедрять. Новые гипотезы не называются удачными без измерений. |
| 12 | Отчёт — русский Markdown, один report.md в назначенном result-root; точные навыки указаны. До350 слов плюс таблицы/raw, кандидат до120 слов, без повторных запусков ради оформления. |
| 13 | STOP/NEXT записан в каждой новой карточке, каждом новом промпте и пяти дополнениях. Команда перехода/остановки выше оформления, review, hash, commit и других остатков. Полная запись инцидента — в13. |
| 14 | Единый [реестр](05-execution-registry.md) сохраняет завершённые01–04, NEXT05, существующие ROPE/AST/NG01. Новый BPE идёт после NG01, dependent readers16/32 — в конце. SKNN не добавляет запуск. |

## Три допустимые связи с обучением

| Связь | Пример | Что именно измерять |
|---|---|---|
| Настоящий учебный переход | DPO/LoRA update; kNN Train; BPE merge learning | Fixed panel A0/A1/B0/B1, actual calls/state; время/allocations на самом переходе |
| Подготовка учебного входа | Мета do-while; AST vectors; tokenizer; admission/replay | Тот же фактический downstream Train плюс цена/качество подготовленных уроков |
| Чтение выученного результата | Restore, phrase maps, learned router key | Происхождение trained state, его идентичность, реальные ответы reader; новый Train необязателен |

Для resource-only improvement достаточно настоящего обучения и сохранения нужной семантики; Generate не обязателен. Для утверждения о полезном learning signal нужны собственные результаты на фиксированной панели. Train loss двух разных уроков, число поменявшихся тензоров и проверка равенства хэшей сами по себе качество ответов не измеряют.

## Пять готовых адресных дополнений

В этих карточках уже есть before/after и процессные правила; добавление точных skills не означает, что всё остальное в них отсутствовало. Protected файлы не переписываются.

| № | Исходная карточка | Добавочный промпт | Далее |
|---|---|---|---|
| 18 | [E-SFT-RESPONSE-MASK](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-RESPONSE-MASK.md) | [prompts/E-SFT-RESPONSE-MASK-addendum.md](prompts/E-SFT-RESPONSE-MASK-addendum.md) | 19 E-SFT-MOMENT-CONTINUITY |
| 19 | [E-SFT-MOMENT-CONTINUITY](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/E-SFT-MOMENT-CONTINUITY.md) | [prompts/E-SFT-MOMENT-CONTINUITY-addendum.md](prompts/E-SFT-MOMENT-CONTINUITY-addendum.md) | 20 E-DPO-VALID-LENGTH |
| 20 | [E-DPO-VALID-LENGTH](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/E-DPO-VALID-LENGTH.md) | [prompts/E-DPO-VALID-LENGTH-addendum.md](prompts/E-DPO-VALID-LENGTH-addendum.md) | 21 E-DPO-SFT-ORDER |
| 21 | [E-DPO-SFT-ORDER](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/E-DPO-SFT-ORDER.md) | [prompts/E-DPO-SFT-ORDER-addendum.md](prompts/E-DPO-SFT-ORDER-addendum.md) | 22 KH-COST-01 |
| 31 | [METHOD-KNN-TARGET-SCOPE-01](https://github.com/lubochka/llm_mvp_core/blob/883496bca18113bf37e0afe683c75b05a0075048/docs/plans/replanning-possible-diffs/experimets/cards/METHOD-KNN-TARGET-SCOPE-01.md) | [prompts/METHOD-KNN-TARGET-SCOPE-01-addendum.md](prompts/METHOD-KNN-TARGET-SCOPE-01-addendum.md) | S01 ROPE-TRAIN-TABLE-01 |

Дополнения также задают будущие result-root вне protected и прямо отменяют требование записывать туда отчёт/candidate из прежних шапок. Это точное разрешение конфликта места записи; бюджет/фактор/данные исходных рецептов сохраняются. Дополнения не запускают эти опыты сейчас.

## Присланные обновления и конфликт продуктовых правил

Прочитан пользовательский `03-WHAT-TO-DO-WHAT-NOT-TO-DO-XIIGEN-v2.md` (версия2, 15.05.2026) и пакеты skills/session. В нём различаются Lite (NestJS/React) и Enterprise (.NET/Angular), а общий DNA/контракты/данные не делают реализации одной и той же системой. Текущий аудит относится к учебному core и planning; он не назначает переделку одного из продуктовых репозиториев.

Требования из присланного v2 о полном product gate/repair-until-green/commit-only-full-green действуют в описанном там продуктовом внедрении. Текущие прямые уточнения пользователя №6/10/11 ограничивают нашу фазу документированием и отдельным экспериментом. Поэтому полный CI, перенос production changes и расширение опыта до всех функций здесь не назначаются. Сохранены относящиеся к данной задаче правила: правильный репозиторий, разделение реализации/контракта, происхождение учебных источников, недопустимость выдуманных результатов.

Правило default executor model не устанавливает модель Student и не переписывает metadata chosen/rejected/teacher в исторических DPO. При проверке provenance не присваивать исполнителю чужую модель и не считать ручные учебные fixtures автоматически прошедшим внешним teacher/guardrail pipeline. Исследовательский профиль можно обозначить честно; продуктовая пригодность — отдельное решение.

**Немедленный переход:** команда пользователя «заканчивай и переходи» прекращает прежнюю задачу без дополнительного отчёта, hash, review, retry или commit. «Остановить всё» прекращает всё. Невыполненное оформление можно только кратко назвать как состояние; оно не разрешает продолжение. Старые следы не удалять.


**Обязательная поправка будущих выходных путей:** [15-output-path-contract.md](15-output-path-contract.md) задаёт точные result-root для каждого ещё не выполненного активного ID, включая NEXT05 N62-R2, и сохраняет protected только для чтения. Это дополнение не разрешает запуск сейчас.


Финальная очередь после9d311 —36distinct/32pending: [05](05-execution-registry.md). ДляMETHOD31 NEXT=S01ROPE. SKNNне отдельный запуск. Все5addenda сохраняют точные существующиеR из63/14; новые конкурирующие пути не назначаются.
