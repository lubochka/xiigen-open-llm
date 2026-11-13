# Последняя сверка: LF-03 завершён, исправления учтены, очередь объединена

**Граница этого снимка:** ниже описано объединение40 ID до параллельного planningd7. Финальное состояние —42 ID,7 завершённых,35 будущих; порядок и новые NEXT заданы в [последнем дополнении](12-concurrent-planning-d7.md). Старое FROZEN→ATTN из этого снимка не применяется.

**Граница актуальности:** core `6e3e8864933e35fccdb4f21369c9912fa195aaf6`, planning `eaed843ad2b063fbb003f073deb69da19f0cb135`. Полный предыдущий корпус этого пакета закреплён на core827/planning53; новые дополнения проверены адресно. Новых сборок, обучения, inference и тестов в этой работе нет. Protected каталог остаётся неизменённым этой работой.

## 1. Эффективное состояние и приоритет доказательств

**Завершены семь первых опытов. NEXT — №08 N63-INGRESS-LENGTH.** В [core index](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md) осталось прежнее «шесть завершённых / NEXT LF-03», но [LF-03 report](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/LF-03-01/report.md) и actual A/B receipts подтверждают COMPLETED/error=null с одним issued/completed StepVector на плечо. Наличие результата важнее запоздавшей строки очереди. LF-03 не повторяется ради документации или недостающей диагностики.

В [объединённом реестре](04-execution-registry.md) **40 ID = 36 core + 2 peer + 2 этого пакета**, из них 7 завершённых и 33 будущих. Первые 34 core ID сохраняют порядок, затем GRAD-ACCUM-COPY-01 → FROZEN-BROADCAST-MAP-01 → ATTN-TRANSPOSE-STRIDES-01 → JSON-OPTIONS-REUSE-01 → IDENTITY-M1-01 → KN-ROUTER-LEARNED-KEY-01. Стабильным ключом остаётся ID, не номер.

**Конфликт перехода разрешён буквально:** peer FROZEN card/inline prompt/contract раньше указывал NEXT=IDENTITY; для объединённой программы NEXT=ATTN. BPE-REALIZED теперь ведёт к GRAD. Данные, source pins и бюджеты этих карточек не меняются. При отдельном поручении одного ID следующий эксперимент не запускается.

## 2. Полезный LF-03 результат и его точная область

Кандидат C-07: в `00.TensorCore/Core/Tensor.cs::Backward(Tensor)` убрать внешний `initialGradient.Data.ToArray()`, сохранив защитное копирование в TensorStorage. Это отдельный фактор от peer AccumulateGrad. Один настоящий DPO/q-v-LoRA update на плечо, четыре диагностических policy forwards суммарно, без EXAM/Generate/повторов.

| Ось | A | B | Изменение |
|---|---:|---:|---:|
| TRAIN allocated bytes | 1 848 491 176 | 1 847 844 360 | **−646 816; −0,0349916%** |
| TRAIN elapsed, мс | 15 270,2952 | 17 178,4596 | **+1 908,1644; хуже** |
| TRAIN CPU, мс | 14 406,25 | 15 015,625 | **+609,375; хуже** |
| GC0/1/2 | 93/8/6 | 90/5/3 | **−3/−3/−3** |
| Whole-process peak, байт | 830 795 776 | 787 300 352 | **−43 495 424** |
| Внешний arm lifecycle, с | 30,3123982 | 28,1212902 | −2,1911080 |

Внешний lifecycle и TRAIN window — разные области; один может улучшиться, когда другой ухудшился. Их нельзя подменять друг другом или суммировать с вложенными таймерами. Полные фазовые показатели сохранены в [core6e3 evidence](evidence/core6e3-late-delta.json) и [подробном readback](reviews/core6e3-late-delta.md).

Реальный учебный сигнал: в A и B first-token NLL изменился **6,814549566480718 → 6,814264388510441**, дельта −0,000285177970277. Это полезное измеренное улучшение обучения по данной метрике, хотя rank 221/629 и exact_first_token=false не изменились. **Разность этих учебных дельт между B и A равна нулю**: оптимизация не показала дополнительного качества B. Полная response NLL, полный teacher-forced exact и самостоятельные ответы этим raw не измерены.

Recorded final weights и readback совпадают; 16 adapter arrays изменились,0 из 39 base arrays. Аудит сравнивает JSON hashes/receipts, не утверждает самостоятельную десериализацию всех binary checkpoint payloads. Расчёт убираемых массивов 608 872 байта отличается от общей measured дельты 646 816 на 37 944; остаток не приписан конкретным объектам.

**Частичное несоответствие самому рецепту:** [протокол24, строки11 и42–49](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/24-neural-learning-signal-protocol.md) требовал весь response, exact target IDs/weights, per-position показатели и отдельную стоимость диагностики. [Фактический bridge, строки400–443](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/35.ExperimentsV2/Experiments/Rp01Micro/LF-03/Common/ContinuousLearning/N62ExperimentBridge.cs#L400-L443) берёт только `goldIds[0]`. Это недостаточность панели, а не основание отвергать допустимый learning-cost опыт. В будущих назначенных hosts точное teacher-forced оценивание всего response и стоимость diagnostic forwards должны быть реализованы согласно карточке; в LF03 отсутствующие поля остаются NOT_MEASURED. Дополнительного LF03 запуска и нового ID для этой правки нет.

### Provenance: что стало известно дополнительно

Сохранённый `study.sourceManifestSha256` правильно совпадает с bytes манифеста. Однако внутри этого манифеста B/Core/Tensor.cs всё ещё имеет `APPLIED_AFTER_FACTOR_EDIT`, а `bridgeSha256=d0035590…` не совпадает с сохранённым LF03 bridge `eb765f3a…`. Current A/B source diff действительно удаляет только внешний ToArray; source props выбирают соответствующий A/B Tensor, DpoTrainer одинаков. Это подтверждает видимый замысел и нынешний текст, но не закрывает историческую цепочку «отредактированный B source → собранная → реально загруженная binary».

Поэтому полезные allocations/GC/peak и меньший внешний lifecycle сохранены как **наблюдённые успехи своих осей**, с отдельным ограничением атрибуции. Формулировки исходного report «детерминированно647КБ», «не меняет обучение совсем» шире имеющихся данных. Разная направленность времени в нескольких разных опытах также не доказывает отсутствие эффекта фактора. Raw не переписывается; повтор не назначен. [Source custody и полные hashes](evidence/core6e3-late-delta.json), [независимый пересчёт arm JSON](evidence/LF03-saved-results-recomputed.json).

## 3. Какие прежние замечания уже исправлены

| Замечание снимка827 | Состояние на 6e3 | Действие будущего исполнителя |
|---|---|---|
| E-OPT raw labels от R1, пропущены GC1/2, слишком широкий вывод | Добавлен [measurement-erratum](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/E-OPT-STEP-01/measurement-erratum.md); raw сохранён, происхождение ограничено | Читать erratum вместе с raw и нашим независимым 08 addendum; не переисполнять EOPT. |
| KN Train timer не охватывал изменяемый reader | [Карточка 6e3](https://github.com/lubochka/llm_mvp_core/blob/6e3e8864933e35fccdb4f21369c9912fa195aaf6/docs/plans/replanning-possible-diffs/experimets/cards/KN-ROUTER-LEARNED-KEY-01.md) уже содержит readerCosts внутри Read before/after и process-wide allocation counter | Использовать этот observer один раз, с прежним бюджетом 2 Train / 16 Route. Архивный 09 overlay повторно не применять. |
| Разные верхние и нижние output paths | 18 pending карточек получили выравнивание нижних literals; уже назначенные корни 29 будущих core опытов и entry не изменились | Следовать текущей карте; не создавать альтернативную папку и не обходить прежние claims. AST candidate называется `candidate-fix.md`. |
| P54 blanket exclusion как «не обучение» | Запрет уже снят ранее; latest подтверждает допустимость learned-state readers | Семь подслучаев всё ещё `NEEDS_INPUT_FOR_RECIPE`; не требовать от исполнителя самому найти snapshots/query/masks/host и не превращать это в блокер прямого обучения. |

Проверка всех 11 использованных native поддеревьев между 827 и 6e3 показала одинаковые tree SHA. Source-гипотезы TensorCore/LoRA/DPO/translation и witness существующего Tests поэтому не исчезли из-за позднего изменения документации: [identity receipt](evidence/native6e3-source-identity.json).

## 4. Почему четыре новых фактора не являются повторами

| ID | Меняемый участок | Различие |
|---|---|---|
| Peer GRAD-ACCUM-COPY-01 | `TensorOps.AccumulateGrad`, первый вклад в каждый достигнутый узел | LF-03 меняет только начальный gradient в `Tensor.Backward`, до обхода графа. |
| Peer FROZEN-BROADCAST-MAP-01 | `MultiplyBroadcast`, mappedBIndices при обоих RequiresGrad=false | Не меняет FlatToIndices, градиентную ветвь и Transpose. |
| Новый ATTN-TRANSPOSE-STRIDES-01 | `Transpose`, rank4 axes1↔2 в attention forward/backward | Убирает coordinate arrays прямой адресацией; не frozen-weight cache и не broadcast map. |
| Новый JSON-OPTIONS-REUSE-01 | `ExecutorUnit.TrainOnExample`, lifetime JsonSerializerOptions | Настоящее EN→RU обучение памяти с теми же DTO/targets/ordinary consumer; autograd не меняет. |

Все четыре пока **NOT_RUN**, measured benefit не приписан. Peer hosts AGC1/FBM1 ещё не созданы; ATTN host — заданный recipe; JSON source-copy host написан в приложении, но не собран. Точные команды, выходы, pins, бюджет и inline prompt peer карточек прочитаны и включены в metadata. [Peer audit](reviews/planning-eaed-peer-delta.md), [машинная карта](evidence/planning-eaed-peer-delta.json).

Peer rank использует competition convention, ATTN — ordinal с tie-break по меньшему ID. Обе метрики явно определены; их нельзя объединять в одну rank-колонку. Старая conditional E07-гипотеза не включена 41-м ID: repeated-Train caller для её переноса на RP01 не установлен.

## 5. Объём проверки и сохранность

Полные прежние snapshots: 799 planning MD на 53 и 345 protected MD на 827. Поздний адресный readback: 52 изменённых/новых protected файла, 17 выбранных source/manifest файлов и 10 peer документов; полученные Git blob SHA проверены. Это не full semantic review всех новых документов, всех 133 исторических prompts или всех binary payloads. Источники и текущие ограничения перечислены в receipts; подписанные download URLs не публикуются.

Публикуется новый пакет в possibleDifs и добавочная шапка его общего README. Ранее опубликованные peer файлы, старые эксперименты, protected документы, production код, tests и веса не меняются этой работой. Успешные измерения сохранены как семь кандидатов; выбор переноса в продукт остаётся за пользователем. Конкретные поздние недостатки описания и source custody добавлены как evidence-кандидаты к существующим опытам, без дополнительных запусков.
