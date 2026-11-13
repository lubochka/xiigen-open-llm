# Реестр добавочного документного пакета

**Текущий режим — только документация.** Ни одна строка ниже не означает запуск в этой сессии. Это порядок будущих назначений после отдельного поручения пользователя. [Машинный реестр](registry.json) содержит те же ID, статусы, paths и budgets.

## Сначала устранить препятствия обучению

| Порядок | ID | Конкретное действие | Что сохраняется |
|---|---|---|---|
| P0.1 | EA-01 / LC-REF-CACHE | CLOSED BY LATE EVIDENCE: common1000, qualified weights, recovery и оба A/B raw уже сохранены | Consumed;0 новых model calls; независимый terminal exit не подтверждён; следующий защищённый ID RETENTION-M1-01 |
| P0.2 | EA-02 / trained S0 | MAPPING RESOLVED IN CORE33; применять canonical families и COMPLETE/qualification | Полные bundles NOT_MATERIALIZED; отсутствие не разрешает новый warm-up |
| P1 | EA-03 / карточка49 | Индекс29/30 исправлен; в тексте49 остаётся смешение topic с C#/.NET/Wiki context | Тема Оркестрация/D07; TRAIN и NEXT не меняются |
| P2 | EA-04 и контракт исполнения | Сохранить успехи вместе с ухудшениями; 0→0/full-answer цели не закрывать; применять STOP/NEXT и настоящее ожидание | Старые результаты не удаляются, consumed опыты не повторяются |

После [позднего согласования0cf3634](09-late-core-0cf3634-update.md) protected индекс имеет53 ID/49 файлов; исходный аудит50 ID остаётся привязан к43e2337. Проверенный issued-or-retained count14 не означает14 успешных опытов. Без прямого нового назначения первый невыданный ID остаётся **PARENT-ADOPTION-01**. Наша новая очередь не вставлена вместо него и не меняет нумерацию старых карточек. Прямое указание пользователя всегда имеет приоритет.

## Новые малые опыты: только после назначения

| Приоритет | ID / карточка | Готовность и зависимость | Бюджет новой пары | Следующий ID |
|---|---|---|---|---|
| 1 | [FILE-STATIC-RECEIVER-01](cards/FILE-STATIC-RECEIVER-01.md) | Рецепт и fixture готовы; host ещё не реализован; S0 native kNN6 создаётся ровно один раз |5Train,36 переданных пар памяти,55Predict; нейронных updates0 | RMS-MEAN-BACKWARD-01 |
| 2 | [RMS-MEAN-BACKWARD-01](cards/RMS-MEAN-BACKWARD-01.md) | Рецепт; host отсутствует; только COMPLETE qualified `h64-rms-row/v1` |8updates,24forward; common training0 | ATTN-SOFTMAX-COPY-01 |
| 3 | [ATTN-SOFTMAX-COPY-01](cards/ATTN-SOFTMAX-COPY-01.md) | Ниже по приоритету; собственная пара из того же `h64-rms-row/v1` S0, не из RMS-after |8updates,24forward; common training0 | END |

Canonical neural donor: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-row/v1/bundle.json`. Старый RD01 common — только источник экспорта; исполнитель не выбирает между двумя путями. Полные filenames и private RNG identity — в [каталоге](05-reusable-trained-states.md).

Каждый ID имеет одноимённый [промпт](prompts/). Полные команды — в JSON и карточках; наличие команды не означает существующий проверенный runner. Если named donor отсутствует, нужный следующий статус — `BLOCKED_DONOR_NOT_READY`; исполнитель не идёт искать другой и не запускает producer самостоятельно.

NEXT — указатель следующего назначения, не автоматическое разрешение исполнения всей цепи. При поручении только одного ID его завершение ведёт к END. При прямом NEXT/STOP прежнюю задачу прекращают сразу: никаких заключительных проверок, дописывания отчёта, commit или обновления памяти перед переходом. Существующий процесс и известный handle сообщаются без нового запуска.

Карточки с indirect learned-result consumer остаются после непосредственных learning blockers. Их наличие в старом индексе не оправдывает замещение текущего обучения metadata/schema-only проверками.
