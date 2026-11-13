# Изменения соседних сессий, учтённые перед публикацией

Этот документ имеет приоритет над численными выводами исходного census, где версии явно закреплены. Содержимое старых реестров не переписано под новый hash: сохранены и исходный срез, и адресная поздняя проверка.

## Последний учтённый срез: ce890a / 3b625a

Эта секция и [coverage/06](coverage/06-FINAL-CORE-DELTA-ce890a.md) имеют приоритет над срезами 7c5718/64aa3c ниже. Зафиксирована последняя ограниченная дельта перед публикацией; дальнейшее движение веток не превращается в бесконечный новый аудит.

**Core 7c5718 → ce890a48efda7f00d0ee040f2383cc2d06428ea6:** один commit, пять документов — три изменены, две таблицы ответов добавлены. Native code не менялся. Число protected Markdown выросло 289→291; card-файлов осталось 128. Все пять файлов прочитаны адресно. N-KU теперь имеет опубликованные A0=B0=0/4, A1=1/4, B1=3/4. False ADD_LOOP уменьшились 3→0 при потере KQ4. Узкий успех не отбрасывается; structural generalization не доказан. Две таблицы имеют один blob и не доказывают два независимых запуска; raw/exit/counters отсутствуют в этих новых файлах. N60-E2 уже определяет SourceSpanFrames, writer, reader, checkpoint и бюджет; прежний OPEN_STATE_AND_READER больше не актуален. Полный native host всё ещё требует подготовки. N60-E1 fixture-недостижимость не исправлена.

**Planning 64aa3c → 3b625a83ee878906d42544516741849a16c04689:** два commit, 67 путей, из них 31 новый Markdown и 2 изменённых. Все 33 Markdown получены и сверены по Git blob; новые findings прочитаны адресно. Число Markdown получается 602+31=633; это обновлённый inventory, не заявление о новом семантическом аудите каждого файла.

| Поздний фактор другого пакета | Атрибуция и отличие |
|---|---|
| [N-HEAD-01][head-new] — LM output projection только нужных DPO позиций | Другой фактор, чем кэш frozen Linear transpose: меняет область вычисления head, требует сохранить индексы и градиентную связность. Не присваивается этому пакету |
| [LF-01][lf-new] — majority shift N-KU и отдельная шестистрочная постановка | Известное ограничение карточки, не новая наша гипотеза. Дополнительная постановка не назначается обязательным повтором уже начатого опыта |
| [LF-02][lf-new] — expVals softmax buffer | Сам автор указал точное совпадение с OCT20-R2. Это одна гипотеза, не два открытия |
| [LF-03][lf-new] — лишний ToArray в Tensor.Backward(initialGradient) | Другая allocation-операция: копирование входного градиента. Не совпадает с transpose матрицы или BPE chunk copy; не добавляем к ним как скрытый второй фактор |
| [LF-04][lf-new] — быстро/тихо не достигают writer N60-E1 | То же уточнение fixture, что наша correction. У LF-04 нежно/горестно, у нас спокойно/громко: это альтернативные панели одной гипотезы, не причина запускать обе. Новизна correction не заявляется |

Четыре наши точные гипотезы Wiki taxonomy, Wiki margin, frozen Linear transpose и BPE no-match copy в этих новых findings не повторены; это независимо сверено вторым reviewer. Их эффект остаётся NOT_RUN. Защищённая папка не редактируется, чужие результаты и источники не копируются поверх прежних. [Машинная запись адресного сравнения](evidence/final-concurrent-delta.json).

[head-new]: https://github.com/lubochka/xiigen-planning/blob/3b625a83ee878906d42544516741849a16c04689/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/02-NEURAL-CURRENT-STATE-AND-NOVEL-HEAD.md
[lf-new]: https://github.com/lubochka/xiigen-planning/blob/3b625a83ee878906d42544516741849a16c04689/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-signal-and-cost-audit/01-new-findings-and-small-experiments.md

## Предыдущий срез core: код тот же, карточки дополнены

GitHub compare `62bcc8f → 7c57183670ccaf554a47c0a4f44cabadde8c303d` содержит пять commit и **148 изменённых файлов**, все внутри защищённой документации; native исходники исследуемых механизмов не менялись. Поэтому source-copy и цитаты native кода остаются привязанными к 62bcc8f. Это не повод заново читать все навыки или перезапускать подготовленный опыт.

Protected tree на срезе 7c5718 — `40b4f0750e9a44edbf26e416a20ae39492d66d34`, полный tree без truncation: **289 Markdown**, **128 card-файлов**. В 125 P-семействах остаются 470 подслучаев. Добавлены две карточки и одна preregistration; одна из новых карточек содержит два разных опыта. Число файлов не равно числу запусков.

| Изменение | Что теперь верно | Чего из этого не следует |
|---|---|---|
| [N61 Mode1](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N61-mode1-retention-and-identity.md) | RETENTION-M1-01 и IDENTITY-M1-01 получили точные уроки, состояния, source repair, outputs и команды | Нельзя продолжать писать «не покрыто». Полный Program остаётся заданием подготовки; факт компиляции и измеренного эффекта не доказан |
| [N61 reference reuse](https://github.com/lubochka/llm_mvp_core/blob/7c57183670ccaf554a47c0a4f44cabadde8c303d/docs/plans/replanning-possible-diffs/experimets/cards/N61-dpo-reference-reuse.md) | Frozen-reference reuse теперь прямо описан в protected; native updates и before/after оценки отделены | Не назван существующим готовым host; зависимости/admission должны быть материализованы по карточке |
| N-KU-01 и N60 | Убран дубль recipe из N60, оставлена ссылка на полный N-KU паспорт; описана сохранность уже начатого опыта | Не новая гипотеза и не новый результат. Majority-shift ограничение остаётся |
| Общие заголовки | Для оболочки малого опыта разрешены простые DTO, throw/try-catch/void без product API-переделки; старые исходники/results/bin/obj сохраняются | Это не разрешение менять factor, данные, budget или проглатывать сбой |
| N-KU preregistration | Есть зарегистрированный план и source hashes | Preregistration сама по себе не training report |

В карточках исходного среза обнаружено 180 prompt-labelled секций. Поздняя дельта добавляет три промпта в N61 и удаляет один дублирующий N-KU prompt из N60: **182 секции, 181 с поручением**, P133 неизменны. Это счёт текста, не число независимых опытов и не доказательство их исполнения. Адресный разбор — [late card audit](coverage/05-LATE-CORE-DELTA-7c5718.md).

Из четырёх inherited coverage gaps исходного среза остаётся отсутствие точного protected recipe для learned-key canonicalization. У Mode1/reference вопрос уже в готовности и корректности конкретного host, а не в отсутствии идеи. Новые Wiki taxonomy/margin и два наши cost-фактора в просмотренной поздней дельте не добавлены.

## Предыдущий срез planning: новые результаты других агентов сохранены

Два новых commit `c38a2a8 → 64aa3c75e378ddf32af517ab139f17dea1f0ed15` добавили 60 файлов и изменили master navigation. Получены и сверены по Git blob SHA **26 новых/изменённых Markdown**; выполнен поиск новых механизмов по их тексту. Адресно прочитаны постановки R1, R2, E-OPT-STEP и уточнение фаз метрики. Исходный корпус 577 Markdown расширился до 602, поскольку один файл из этих 26 уже существовал.

| Поздняя гипотеза другого пакета | Точное место фактора | Отличие от наших новых карточек |
|---|---|---|
| [OCT20-R1](https://github.com/lubochka/xiigen-planning/blob/64aa3c75e378ddf32af517ab139f17dea1f0ed15/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-update-effect-audit/05-new-R1-frozen-base-comparison.md) | Побитовое сравнение frozen base в `DpoProjectLoraRecipient.RequireValid`; byte spans вместо per-float LINQ | Не кэширует транспонированную базовую матрицу внутри `Linear.Forward`; guard и матричный forward — разные участки |
| [OCT20-R2](https://github.com/lubochka/xiigen-planning/blob/64aa3c75e378ddf32af517ab139f17dea1f0ed15/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-update-effect-audit/06-new-R2-softmax-buffer.md) | Срок жизни `expVals` в `BackwardLogProbs` | Другой buffer/операция, не BPE chunks и не Linear transpose |
| [E-OPT-STEP](https://github.com/lubochka/xiigen-planning/blob/64aa3c75e378ddf32af517ab139f17dea1f0ed15/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/62-LEARNING-METRICS-AND-REFERENCE-COST-AUDIT-2026-10-20/OPTIMIZER-STEP-READ-COST-2026-10-20.md) | Portable optimizer-state export только ради номера шага при RNG callback | Другой owner и allocation; snapshot/rollback validity должны сохраниться. Не frozen-reference cache и не наши four factors |
| [Training metric phase](https://github.com/lubochka/xiigen-planning/blob/64aa3c75e378ddf32af517ab139f17dea1f0ed15/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/62-LEARNING-METRICS-AND-REFERENCE-COST-AUDIT-2026-10-20/TRAINING-METRIC-PHASE-REPAIR-2026-10-20.md) | Returned StepVector metric вычислена до optimizer; weighted evaluator и monitor должны измерять ту же функцию | Это развитие известного phase вопроса. Его нельзя приписывать нашей новой taxonomy/margin; общий after-probe не тождественен training forward |

Наличие этих пакетов не делает наши находки автоматически дубликатами, но они включены в attribution и современную карту. Отдельные cost-гипотезы не соединяются в один «ускоряющий патч»: иначе нельзя узнать, какой фактор дал наблюдаемый эффект. Они также не вытесняют приоритет прямых блокеров приобретения знания.

## Как учтена параллельность

Защищённая папка не редактировалась этим аудитом. Поздние исправления выполнены другой сессией; их не записываем в наши достижения. Публикуемый пакет добавляется поверх последнего planning head с проверкой коллизий. Ни чужие commits, ни исторические эксперименты не откатываются.

При появлении после этого среза новых чужих Markdown фактический source review остаётся воспроизводимым по версиям. Новый документный HEAD не создаёт необходимости бесконечного перечитывания и не доказывает изменение native механизма. В отчёте явно названы версии, а не неопределённое «проверено всё последнее».
