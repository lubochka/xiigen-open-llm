# Кандидат EMB-SFT8: успешное восстановление обучаемого embedding

## Решение и границы

**Зафиксировать как успешное исправление по двум учебным показателям: доступность корректного градиента embedding и уменьшение среднего online TRAIN loss в профиле перевода EN→RU.** Рост точности не является условием признания этих успехов. Предметная точность осталась 0/24; расходы по нескольким другим показателям выросли. Исправление уже присутствует в зафиксированном текущем коде: статус `SUCCESS_RECORDED / SOURCE_ALREADY_PRESENT`. Повторный production patch по этой карточке не нужен.

Это добавочная интерпретация сохранённого опыта `embedding-sft8-01` от 2026-10-18. Защищённый первичный отчёт и его `NO_OBSERVED_GAIN` по точности сохраняются без изменений. Новых обучающих запусков, исправлений продукта или тестовых конвейеров при этом аудите не выполнялось. Здесь «до/после» означает историческое A/B двух вариантов программы, а не повторный запуск сегодня. Источники зафиксированы на core SHA `9622b575a6aabb79939b12cc9f5d96313f1bf370`.

**Предметная область:** перевод. **Профиль:** EN→RU, полная нейронная LlmModel, SFT всех 55 тензоров. LoRA, SFT и направление EN→RU не объявляются отдельными предметными областями. Обычный RP01 с замороженной базой и обучением только q/v-LoRA этим опытом не проверен: там embedding намеренно не обучается. [Первичный отчёт, строки 3–7][report]

## Что именно участвовало в обучении

В A и B загружен один C50 checkpoint, создан свежий AdamW, выполнено ровно восемь `Trainer.TrainSupervisedNextTokenStep`. В каждой последовательности native tokenizer отдельно кодирует prompt и chosen, затем соединяет ID. Все 55 тензоров переведены в trainable. Фактор B — только связь результата `Embedding.Forward` с его таблицей весов и scatter-add производной для повторяющихся ID. `weightDecay=0` исключает объяснение движения embedding одним decay. Реальные вызовы, настройка параметров и материал записаны в [harness, строки 50–79][harness].

Причинная цепочка: prompt/response IDs → `LlmModel.Forward` → gather embedding → next-token CE → `BackwardLoss` → `TensorOps.AccumulateGrad` таблицы → clipping → `AdamW.Step` → изменённые веса → следующие SFT уроки и собственные ответы генератора. Это не отдельный тест матрицы без ученика. Диагностический backward после восьми updates не вызывает optimizer и не является девятым уроком. [Trainer, строки 102–147][trainer]; [первичный отчёт, строки 26–28][report]

Текущий исправленный оригинал — [`00.TensorCore/Layers/Embedding.cs:43–79`][embedding]. При trainable weight он сохраняет token IDs, присоединяет weight к children, суммирует производные по строкам таблицы и накапливает градиент. При frozen weight связь не создаётся. **Это подтверждение наличия исправления в источнике сегодня, не утверждение о новом его внедрении этим аудитом.**

## Сопоставимость плеч и определения метрик

| Условие | Проверенное значение |
|---|---|
| C50 SHA256 | `6f4d0284ea823ba99932b0f28576855c14205703115b48c983c7a9f03749179b` |
| Начальные параметры | Одинаковые; content hash `89d4beadb50634a3b8988c49293af638fdfb047c630893ea59eab4d9760953e8` |
| Топология | vocab 629, hidden 512, 4 Transformer-блока, 8 attention heads / 2 KV heads, FFN 1408, context 2048 |
| Обучаемый получатель | 55/55 тензоров, 39 base + 16 LoRA, всего 11 974 144 скалярных параметра |
| Оптимизатор | AdamW β₁=.9, β₂=.95, ε=1e−8, weightDecay=0; свежий step 0 |
| Скорость и clipping | MaxLr=MinLr=1e−5, warmup=0, clip norm=1 |
| Данные | Те же 8 уроков, порядок tr01…tr08; `training-material.json` побайтно совпал |
| Масштаб loss | weight=1 в обоих плечах; один update на урок, без накопления батчей |
| Стохастика | Начальные RNG и tokenizer равны; LoRA dropout=0 |
| Генерация | 24 одинаковых входа, native TextGenerator, temperature=0, topK=0, topP=1, seed=20261014, maxNewTokens=48 |

Равенство материала и последовательности ID дополнительно проверено чтением сохранённых JSON при этом аудите; `validation.json` также фиксирует равенство config, RNG, исходных весов и неизменность входов. Отличается только TensorCore assembly. [Validation][validation]; [начальное состояние][initial]; [исходный материал][material]

Возвращаемый SFT loss рассчитывается **до optimizer update** как средний next-token CE для всей последовательности prompt+chosen, затем умножается на 1. В отчёте усредняются восемь таких значений с равным весом каждого урока. Это сопоставимая A/B метрика одной учебной траектории: одни и те же примеры, порядок, objective, масштаб и восемь шагов. Это **не heldout loss**, **не final-model loss**, **не response-only loss** и **не среднее всех 490 токенов с одинаковым весом**. [Trainer:117–147][trainer]; [CrossEntropyLoss:376–398][crossentropy]

| Шаг / ID | Next-token targets | Loss A | Loss B | B−A |
|---|---:|---:|---:|---:|
| 1 / tr01 | 54 | 7.7864347 | 7.7864347 | 0 |
| 2 / tr02 | 48 | 7.2864030 | 7.2747480 | −0.0116550 |
| 3 / tr03 | 59 | 7.1144695 | 7.0949630 | −0.0195065 |
| 4 / tr04 | 74 | 7.1006760 | 7.0740876 | −0.0265884 |
| 5 / tr05 | 70 | 6.8708870 | 6.8410950 | −0.0297920 |
| 6 / tr06 | 58 | 6.7864523 | 6.7477670 | −0.0386853 |
| 7 / tr07 | 63 | 6.5873437 | 6.5384083 | −0.0489354 |
| 8 / tr08 | 64 | 6.6029263 | 6.5444300 | −0.0584963 |
| Сумма восьми loss | 490 targets всего | 56.1355925 | 55.9019336 | −0.2336589 |
| Среднее: сумма / 8 уроков | 8 | **7.0169490625** | **6.9877417000** | **−0.0292073625** |

Все числа пересчитаны из [before/training-steps.json][steps-a] и [after/training-steps.json][steps-b], без нового обучения. Одинаковый первый loss ожидаем при одинаковом начальном forward; изменение первых весов влияет на следующие уроки. Семь последующих online значений ниже в B. Это полезное измеренное уменьшение учебного objective в данном профиле, около 0.416%, при указанном ограничении его смысла.

## Успехи, нейтральные результаты и ухудшения

| Показатель / знаменатель | A | B | Классификация |
|---|---:|---:|---|
| Тензоры, достижимые из native logits / 55 | 54/55 | 55/55 | Успех: embedding включён в настоящий учебный граф |
| Тензоры с изменёнными весами / 55 | 54/55 | 55/55 | Подтверждает реальное действие исправления |
| Изменённые координаты embedding / 322 048 | 0 | 38 400 | Реальное обновление таблицы; величина сама по себе не accuracy |
| Максимальное `abs(Δembedding)` | 0 | 0.0000797659158706665 | Подтверждает ненулевое изменение |
| Корректность finite difference / 24 строк | 0/24, Grad=null | 24/24 | Успех производной |
| Средний online TRAIN loss / 8 уроков | 7.0169490625 | 6.9877417 | Успех по учебному objective |
| Правильные ответы / 24 | 0/24 | 0/24 | Нет роста предметной точности |
| Правильные TRAIN recall / 8 | 0/8 | 0/8 | Нет роста |
| Правильные неучебные / 16 | 0/16 | 0/16 | Нет роста |
| Неучебные: близкие / композиции / края | 0/3, 0/8, 0/5 | 0/3, 0/8, 0/5 | Все категории сохранены |
| Gained / lost IDs | 0 / 0 | 0 / 0 | Изменение полноты правильных ответов отсутствует |
| Совпавшие тексты между плечами | — | 3/24 | Тексты изменились, но правильными не стали |

Знаменатель finite difference — **8 скалярных координат × 3 размера конечной разности**, а не 24 независимых опыта. Восемь строк таблицы включают repeated-ID и unused-row случаи; frozen guard проверяет отсутствие градиента замороженной таблицы. В первом B-update изменились 18 944 координаты и появился optimizer state embedding; в A этого нет. После восьми updates отдельный native backward также увидел ненулевой градиент. [Harness:103–108][guard]; [отчёт:16–28][report]

Восемь recall-вопросов открыты обучению, поэтому они не объявлены независимым экзаменом. Оставшиеся 16 в SFT не подавались. Исторический успех памяти Executor 19/24 относится к другому получателю и не заменён этим нейросетевым 0/24. Для этого аудита точность взята из сохранённого scorer/report; новый scorer и новое внешнее судейство не запускались. [Первичная оценка][report]

## Полные расходы: выигрыш одного показателя не скрывает цену

| Измерение | A | B | B−A |
|---|---:|---:|---:|
| Сумма elapsedMs восьми уроков, мс | 24 505.3504 | 24 391.5929 | **−113.7575** |
| Полный native harness, мс | 142 083.4058 | 149 435.7635 | +7 352.3577 |
| Генерация 24 ответов, мс | 112 946.0235 | 118 968.0467 | +6 022.0232 |
| CPU всего процесса, мс | 141 593.75 | 149 609.375 | +8 015.625 |
| Peak working set процесса, байт | 1 737 625 600 | 1 826 639 872 | +89 014 272 |
| Максимальная наблюдённая сумма RAM группы, байт | 1 745 084 416 | 1 836 838 912 | +91 754 496 |
| Cumulative managed allocations, байт | 175 607 527 112 | 175 687 971 504 | +80 444 392 |
| Сохранённый checkpoint, байт | 47 899 519 | 47 899 519 | 0 |

Источник: [A cost][cost-a], [B cost][cost-b], [первичный отчёт расходов][report]. Время урока начинается **перед** записью `issued-step.json`; поэтому приведённая сумма включает receipt I/O и native SFT, а не только численные kernels. Это видно в [harness:70–72][harness]. Полный harness включает загрузку, диагностику, обучение, ответы и сохранение. Cumulative allocations — накопленные выделения за процесс, а не одновременная RAM. Групповой sampler мог пропустить короткие процессы; это наблюдённый максимум, не гарантированный полный peak. GPU/VRAM N/A, Unit/Flow не создавались.

Уменьшение суммы времени уроков зарегистрировано как **локальный успех измеренного показателя** в этой единственной паре запусков. JIT, порядок исполнения и receipt I/O не позволяют приписать разницу только исправлению или обещать устойчивое ускорение. Эта оговорка не отменяет успех корректного градиента и train loss. Выросшие CPU, память, время ответов и полного harness остаются видимой ценой наблюдённого исполнения.

## Запись в кандидаты исправлений

| Поле | Значение |
|---|---|
| Candidate ID | EMB-SFT8-RETAIN |
| Эксперимент-доказательство | `embedding-sft8-01`, 2026-10-18 |
| Где чинить оригинал | `00.TensorCore/Layers/Embedding.cs`, `Embedding.Forward` |
| Какая правка | Связать gather с weight и scatter-add backward, сохраняя frozen semantics |
| Текущее состояние | Правка уже находится в pinned source; новый patch не назначен |
| Что одобряется как результат | Успешный ремонт обучения trainable embedding и локальное уменьшение online TRAIN objective |
| Что не установлено | Рост точности перевода, улучшение exact RP01 с frozen embedding, эффект в иных доменах, устойчивое ускорение |
| Действие этого аудита | Добавить этот положительный результат в реестр возможных исправлений/сохранённых исправлений; первичный отчёт не переписывать |
| Новые запуски | 0; карточка не является dispatch нового эксперимента |
| Исторические команды | Существующий [`commands.md`][commands] с build/launch/scorer receipts; это архивная инструкция, не новое поручение запускать её |

Для будущего переноса на другой subject domain нужен отдельный однофакторный опыт в экспериментальной копии с тем же native learner. Ни разморозка базы в обычном RP01, ни изменение LoRA topology, ни починка Linear bias не входят в доказанный фактор.

**Промпт документирующему исполнителю:** «Перенеси этот измеренный результат в additive candidate record. Признай любой измеренный выигрыш полезной метрики успехом по этой метрике и профилю; рост accuracy не требуется. Сохрани 0/24 и все ухудшения рядом. Укажи, что loss — среднее восьми pre-update online SFT losses по полным последовательностям, не heldout. Не запускай обучение, не изменяй продукт или защищённую папку и не делай повторный patch уже имеющегося Embedding backward».

[report]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/report.md#L3-L44
[harness]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/harness/Program.cs#L50-L89
[guard]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/harness/Program.cs#L103-L108
[trainer]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/04.TrainingLoop/Trainer.cs#L102-L147
[crossentropy]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/04.TrainingLoop/Trainer.cs#L376-L398
[embedding]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/00.TensorCore/Layers/Embedding.cs#L43-L79
[validation]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/validation.json
[initial]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/initial-state.json
[material]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/training-material.json
[steps-a]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/training-steps.json
[steps-b]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/after/training-steps.json
[cost-a]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/cost.json
[cost-b]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/after/cost.json
[commands]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/commands.md


## Оговорка о хешах локального аудита

В companion `positive-neural-candidates.json` поле `hash_scope=local_materialized_download` означает SHA256 полученной локальной текстовой копии. Download/apply_patch мог добавить terminal LF или нормализовать переводы строк; raw Git blob identity по этим hashes не заявляется. Версию задают immutable source URLs. Побайтное равенство учебного материала во время выполненного A/B подтверждается первичным `validation.json`; локальный аудит дополнительно проверил полученные копии и порядок/содержимое token IDs. Это не новая runtime-проверка.
