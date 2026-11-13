# Поздний core669: завершённый R2, устранённые конфликты и оставшиеся gaps

> **Исторический промежуточный срез.** Окончательный827:01–06 completed, NEXT=LF-03; [текущая сводка](08-final-core827.md). Старые next/counts ниже не управляют запуском.


**Актуальный следующий ID — E-OPT-STEP, исходный №06. Объединённая очередь:38 ID,5 завершённых,33 pending.** [Core index669](https://github.com/lubochka/llm_mvp_core/blob/66947785d1318f7cbbc88118f3156cdc75001e16/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#L3-L7) запрещает повтор №01–05. Документная сессия новых запусков не назначает.

Этот commit `66947785d1318f7cbbc88118f3156cdc75001e16` появился после исходного аудита883496 и согласования planning53cb. Его автором был другой исполнитель. В данной работе core/protected не менялись. Старые receipts747/334/153/470 и review35/38 сохранены с границами snapshot; они не выдают прежние статусы за текущие.

## 1. Что изменилось в выводах

| Предмет | На исходном883 | На669 и действие в этом пакете |
|---|---|---|
| N62-R2 | Следующий невыполненный | Сохранены две COMPLETED arms по2 updates. Теперь completed, без retry; ближайший E-OPT-STEP |
| N62-R1 report | Неполные оговорки и ошибочная трактовка scalars | Current report уже исправлен. Наш corrigendum — история, не новое требование повторить или переписать его |
| P54.1–P54.7 | Общий допуск противоречит7 blanket запретам | Запреты сняты. Все7 остаются NEEDS_INPUT_FOR_RECIPE: конкретные snapshots/tokenization/masks/relations не назначены. Реальный learned reader допустим без искусственного нового Train |
| NG01 | Exact рецепт есть только в planning, общий N48/C2 недостаточен | Полная карточка и промпт теперь protected; DESIGNED_NOT_RUN. Внутренний host заменил C:/xg, документный gap переноса закрыт; результат не появился |
| E-METRIC-PHASE | Точный phase-only контраст не покрыт P78/P89 | P78 по-прежнему window1→5, P89 normalization. Эти факторы не закрывают phase-only intervention; конкретный минимальный паспорт остаётся незавершённым |
| GRAD / FROZEN | Два новых точных source фактора | Оба Tensor-файла byte-identical на883/669. R2 меняет expVals в другом методе, новизну этих факторов не поглощает |

[Подробная P54/NG01/P78/P89 сверка](history/06-core669-gap-status.md), [Tensor delta](learning/CORE669-DELTA.md), [R2 readback](experiments/CURRENT669-N62-R2-READBACK.md).

## 2. R2: успех allocations вместе с ухудшениями

Из actual A/B JSON: allocated3 764 947 024→3 764 634 696 байт, **−312 328 /−0,008295681%**. Одновременно process peak949 817 344→980 291 584, **+30 474 240 байт**, GC188/12/9→189/13/10. Elapsed40 459,8146→34 658,2724мс и CPU34 593,750→30 546,875мс — полезные наблюдения, но причинность speedup не изолирована: fixed A→B и ускорение нетронутых фаз.

Это успех измеренной полезной оси, без обязательного accuracy gain. Совпадающие state hashes/optimizer receipts и16 изменённых LoRA не доказывают улучшения ответов. Step metrics принадлежат pre-update двух разных уроков; after-panel/TRAINv2 отсутствует. Её не добавляют задним числом.

Study A/B trainer hashes одинаковы и сняты до B-edit; историческая binary custody B неполна. Текущий source B-factor известен, но точный исполнявшийся B binary не аттестован этим readback. Raw указывают executed core62bcc8f, а669 — commit публикации. [Данные, расчёты и существующий кандидат](experiments/CURRENT669-N62-R2-READBACK.md) сохраняют эти ограничения. Второго опыта/кандидата с новым ID не создаётся; protected required-changes только прочитан.

## 3. Очередь и единственное назначение NG01

[Root04](04-execution-registry.md) — действующий реестр. Все38 ID сохранены; R2 переведён в completed, NG01 не продублирован при его добавлении в core. Исходные source-номера в общей таблице относятся к883: на669 NG01=32, KN-ROUTER=33. Их stable IDs и назначенная последовательность прямых learners перед readers сохраняются.

[Root05](05-executor-contract.md) содержит33 активных output assignments. Из32 inherited planning53cb assignments30 остались active без изменения, R2 сохранён как completed archive, NG01 получил явный core669 supersession; добавлены2 наших tensor assignments. Прежний NG01 assignment сохранён для provenance, **не для альтернативного запуска**.

NG01 host=`C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/NG01`; results=`host/r/attempt-0001`, `report.md`/`candidate.md`. Read-only source pin0b76ab950d82534f5904005dda096545505ae307;2 Train/8 Predict; одна Run.ps1 цепочка. [Точный mapping с line evidence](history/evidence/ng01-core669-output-mapping.json). Запись в protected required-changes, указанная в самой карточке, отменяется прямым запретом пользователя; здесь разрешены experiment outputs и planning possibleDifs. Runtime-проверки не было.

## 4. Граница дополнительной проверки

Прочитаны только затронутые статус/протокол, R2 actual results/study, R1 report, P54/NG01/P78/P89, required-changes и два Tensor-файла. Это адресный delta readback, не повторная полная научная аттестация всех карточек новогоHEAD. Compare API вернул300 путей и мог обрезать file list: [receipt](coverage/core669-concurrent-change-receipt.json) прямо отмечает эту границу; он не называется полным tree diff.

Ни один новый Train/build/inference не выполнен. Исходные source-copy pins сохранены там, где обеспечивают воспроизводимость вмешательства. Обязательные default model/skills, русский отчёт, useful metric success, регрессии рядом и немедленный NEXT/STOP сохраняются; новое состояние очереди не разрешает завершать оформление прежнего опыта после команды перейти дальше.
