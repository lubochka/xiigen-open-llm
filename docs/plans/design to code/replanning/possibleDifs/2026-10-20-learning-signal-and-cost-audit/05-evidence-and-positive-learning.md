# Что уже обучалось успешно и какие различия действительно относятся к RP01

**Первичные сохранённые отчёты, исходники и границы сравнения.** Это разбор уже существующих свидетельств, а не новые прогоны. Приведённые исторические числа принадлежат исходным авторам. Новый audit не выполнял обучение, inference, сборки или тесты; статические прогнозы LF-карточек вынесены отдельно и не смешаны с этими измерениями.

## 1. RP01: обновления были реальными, полезный margin вырос, свободные ответы не улучшились

| Сохранённый прогон | Действительное обучение и состояние | Показатель до → после | Что не улучшилось / ограничение |
|---|---|---|---|
| C# simulation main500 | 500 уникальных пар, 50 updates; 16 LoRA tensors изменились, 39 base tensors сохранились; после открытия сохранённого состояния подтверждены weights/optimizer/RNG | Mean normalized margin на 25 парах: 0.03733304977416992 → 0.03856058120727539, Δ +0.0012275314331054651 | 14 margins выросли, 11 снизились; chosen preferred 16/25 → 16/25. Свободные ответы 0/3 → 0/3, все три byte-identical и невалидны |
| EN→RU continuation | 500 пар, 50 новых updates C50→C100; 16 LoRA tensors изменились, 39 base сохранились; reference C50 заморожен, полное состояние повторно открыто | Mean normalized margin на 20 парах: 0.012293052673339844 → 0.012403059005737304, Δ +0.00011000633239746093 | 11 margins выросли, 9 снизились; chosen preferred 12/20 → 12/20. Свободные ответы 0/4 → 0/4, все невалидны |

Источники: [C# scientific-result.json][rp01-cs], [ENRU scientific-result.json][rp01-enru]. Это достаточное основание не писать «LoRA вообще не обучалась» или «не было ни одного полезного изменения». По правилу пользователя рост margin — успех **этой метрики на этой панели**, а сохранение нулевого числа правильных свободных ответов — отдельное наблюдение. Идентичность трёх ответов не означает идентичности всех logits/весов.

Панели близки к учебным шаблонам; они не доказывают широкого переноса на независимые семейства задач. C# retention 0/3→0/3 в продолжении также не доказывает сохранения ранее работавшего навыка: исходный показатель уже нулевой. Случайная/необученная исходная база — важная гипотеза пригодности Student, но сама по себе не доказанная единственная причина провала. Нужен сравнимый опыт выбранного training-фактора на действительном состоянии, а не название «готовая модель». [Те же первичные отчёты][rp01-cs] и [профиль Student][config].

Стоимость C# owner: 6272.808539 с; вложенный training total — 6238.0783751 с, в который входят сохранения (serialization 16.5683 с и optimizer/RNG save 2.04767 с). Эти интервалы нельзя складывать как независимые расходы или называть чистым optimizer time. ENRU owner 2330.4531116 с относится к другому материалу; разницу нельзя объявлять ускорением C# алгоритма. [C#][rp01-cs], [ENRU][rp01-enru].

## 2. Самое прямое доказательство потери учебного сигнала при загрузке

### Приобретение знания, восстановление и ремонт loader — три разных результата

| Свидетельство | Сравнение | Подтверждённый смысл |
|---|---|---|
| P125 ENRU memory | Пустой learner → 8 настоящих уроков; ordinary Flow 0/24 → 19/24 | Acquisition памяти Executor/Units, нейронных updates нет; три ошибочно принятых ответа после обучения |
| P25 cold restore | Ранее обученная память → сохранение → новый процесс → те же 24 запроса | Сохранено 19/24 и маршрут/ответы; 0 новых Train — законная проверка сохранности результата обучения |
| translation-memory-ingress-01 | Два одинаковых свежих learner; каждый получает те же 8 уроков через старый/исправленный реальный loader | 11/24 → 19/24 — matched repair входа обучения, а не сравнение trained с empty |

Источники: [P125 report][p125], [P25 report][p25], [ingress report][ingress]. Не складывать эти три опыта в одно «0→11→19» и не выдавать cold readback за второй этап приобретения знания.

В ingress-паре оба корпуса заранее содержали 16 соответствий частей. Старый ReadTranslationCorpusRowsAsync их не читал, а BuildTranslationContinueExample не переносил в ExecutorOutput. После исправления уроки доставляют 16 mappings в ту же обучаемую память; сами Executor, поиск, Flow, verifier и пороги не менялись. Действительная цепь: private reader/builder Tools через reflection → TrainStructuredBatchAsync → GetOrchestratorAsync().RunAsync. Это обычный переводческий Flow после реального обучения; полный внешний CLI/registry не запускался. [Описание изменений и цепи][ingress].

| Метрика matched ingress | A: старый loader, после 8 уроков | B: исправленный loader, после тех же 8 уроков |
|---|---:|---:|
| Учебные примеры | 8/8 | 8/8 |
| Близкие формулировки | 3/3 | 3/3 |
| Новые комбинации знакомых частей | 0/8 | 8/8 |
| Краевые случаи | 0/5 | 0/5 |
| Правильные среди всех | 11/24 | 19/24 |
| Принятые ответы | 12/24 | 22/24 |
| Ошибочно принятые | 1 | 3 |
| Правильные среди принятых | 11/12 (91.7%) | 19/22 (86.4%) |
| Сохранённое состояние | 7 515 байт | 8 954 байт |

Это одновременно успех строгой correctness/покрытия комбинаций и ухудшение false acceptance, precision и размера состояния. Среди принятых ошибок — потеря отрицания и превращение вопроса о прошлом в команду. Вывод «из-за этих ошибок весь ingress был неуспешным» скрыл бы восемь реально приобретённых ответов. Обратный вывод «перевод теперь правильный вообще» также неверен. [Все 24 raw ответа и стоимость][ingress].

**Этот ремонт уже есть в текущем коде:** [reader, строки 4442–4472][ingress-reader] читает phrase_mappings, [builder, строки 4682–4702][ingress-builder] передаёт row.PhraseMappings в ExecutorOutput. Не ставить его в очередь повторного исправления. Использовать как доказательство того, насколько важен транспорт sub-unit материала. Neural RP01 использует другой adapter; успех памяти не доказывает, что починен его DPO путь.

Времена одного batch и 24 ответов сохранены, но это одиночные процессы с JIT и разным порядком. Process peak уменьшился, максимум наблюдённых сумм RAM группы вырос, snapshots sampler было разное число. Поэтому отчёт честно не доказывает воспроизводимое ускорение или полный peak RAM группы. Trainer accuracyAfter=0 там относится к другой coverage-метрике/набору Units, а не к этой EN→RU панели 19/24. [Раздел стоимости и provenance][ingress].

## 3. Успешные соседние ветки: не потерять механизм при переносе

| Ветка и опыт | Что действительно училось / измерялось | Сохранённый полезный эффект | Доменные и измерительные границы |
|---|---|---|---|
| DesignToCode, E09 Structural KNN | AST-признаки C# → native KNN по последовательным парам structural tokens | Accuracy 0.3368 → 0.3472, +1.04 процентного пункта | Старый FAILED возник из порога +5% relative. Это полезный локальный прирост, но не способность генерировать C#. Сохранённые timed blocks 1→74 мс — отдельный регресс/цена |
| language_translate, E25 Pattern Classifier | Train SentencePatternClassifier на синтетических POS-последовательностях; val 32 примера | 0.3438 → 0.875, +53.12 п.п. | 160 synthetic examples всего; не raw-text перевод и не LoRA. Train находится до timers, поэтому 0.4→15.4 мс характеризует prediction, а не стоимость обучения |
| PrepareToGPL, E43 Replay Buffer | Таблица bigram counts; new-only против new+old replay | Retention 0.3226 → 0.3978, +7.52 п.п. | Не neural adapter. Old465/new414/replay177 — различие предъявленного материала является фактором и расходом. 0.1→5.8 мс не переносить на DPO optimizer |

Первичные результаты и исполняемые реализации: [E09 report][e09-report], [E09 source][e09-code]; [E25 JSON][e25-report], [E25 source][e25-code]; [E43 JSON][e43-report], [E43 source][e43-code]. Ветки закреплены полными SHA. Сравнение A/B внутри одного такого опыта информативно для его механизма; сравнение абсолютной accuracy E09 с E25 или RP01 между разными панелями не является оценкой переноса.

Вывод для архитектуры: AST-порядок/вид узла полезно проверять на коде; POS/порядок предложения — на соответствующем языковом consumer; replay — на actual learner с памятью прежних уроков. Нельзя буквально переносить successful table в neural матрицу и ожидать того же эффекта, если изменились представление, target, функция соседства или reader. Старый PASS/FAILED не заменяет чтение numerator/denominator, роли Train и места таймера. E09/E25/E43 обосновывают **сохранение гипотез**, но не универсальный k, размер replay или один tokenizer на все домены.

Точная непрерывная цепочка «один и тот же фактор был неудачно реализован в матрице → затем перенесён в функцию KNN и стал успешным» в этом аудите не установлена. Это остаётся вопросом происхождения реализации, а не опровержением пользовательского свидетельства. Найденные первичные успехи KNN/POS/replay и ошибочная трактовка FAILED сохранены отдельно; недостающий migration trace не превращает их в отрицательный результат.

### V309: первичный учебный отчёт найден, но final accuracy не heldout

В архиве сохранён [V309 summary][v309-report]: 1404 пары, batch16, 88 batches, context96, vocab2048; AverageLoss0.6789156693, FinalLoss0.6676091552, AverageAccuracy0.8380681818, FinalAccuracy1; AverageRewardMargin0.0289275967, FinalRewardMargin0.0517829508; TotalElapsedMs860816.1058, checkpoint bytes4 766 262.

Прочитан [runner][v309-runner]: он формирует batches, вызывает trainer.StepMany на каждом, а summary берёт `Average(...)` и `Last()` из batchSummaries. Следовательно, FinalAccuracy1 — значение **последнего учебного батча**, не 100% на независимом экзамене. При 1404/16 последний батч содержит 12 пар; AverageAccuracy — простое среднее 88 батчей, не взвешенная по 1404 парам доля. AverageLoss→FinalLoss тоже не matched before/after: меняются батчи. Это сохраняемый исторический учебный результат, но не доказательство полной исходной компетентности современного RP01.

Runner использует другой Student (H64,1 layer, vocab2048), word hashing, контекстное усечение и разные Chosen:/Rejected: заголовки. Данный snapshot runner поясняет структуру отчёта; он не является аттестацией того, что нынешний bf9 DpoTrainer побайтно совпадает с майским binary. SourceSha256 в JSON относится к исходному корпусу. Для повторного использования historical state нужны реальные compatible weights/tokenizer/adapters и provenance; не заменять их реконструкцией по final score. [Runner config, batching и ToBatch][v309-runner].

## 4. Карта механизмов: что проверять и что не откатывать

| Механизм / возможная потеря | Точный исходный путь и current статус | Малый способ проверки / домен |
|---|---|---|
| Exact DPO материал и ответная граница | [DpoExactPairMaterial][exact], [DpoTrainer.StepVector][dpo] используют IDs/masks и исходные тексты; raw pair, teacher judge и измерение полезности — разные сущности | Один semantic contrast с проверкой model-visible target positions; C# и перевод раздельно. Не удалять negative только из-за его намеренной неправильности |
| Знак градиента, same-forward autograd, adapter recipient | [DpoTrainer][dpo], [LoraLinear][lora] и реальные RP01 receipts подтверждают нынешний actual backward/LoRA путь | Историческую ошибку не объявлять действующей без current reachability. Rank/alpha/dropout/target set — отдельные A/B; не переносить целиком старый trainer |
| Prompt в supervised loss | [IncrementalTrainer][incremental] кодирует prompt+LF+response; [Trainer.TrainSupervisedNextTokenStep][sft] обучает все shifted targets | MASK: одинаковые IDs и denominator T−1, меняется только prompt mask. Already covered, new host preparation в 03 |
| Сброс SFT optimizer между уроками | [IncrementalTrainingSession.Create][session] создаёт fresh Trainer/AdamW | STATE: carry m/v/step против fresh при одинаковом LR и переносимых weights; не менять одновременно scheduler/reference |
| Последовательность DPO/SFT | [IncrementalTrainer][incremental] и отдельные owners; CARD23 задаёт ORDER | Общий frozen R0 до обоих stages, одинаковые stage optimizers/уроки; это гипотеза порядка, не доказанная причина RP01-only run |
| PAD и лишние allocations actual backward | [DpoTrainer][dpo], [Tensor.Backward][tensor] | PAD уже покрыт; LF-02/LF-03 — новые отдельные ресурсные контрасты. Качество генерации можно оставить NOT_MEASURED |
| Смысл и плотность teaching vector | [DpoTrainer.StepVector][dpo] и существующие P06/P89/N54 | Число активных координат/масштаб шага не равно семантическому credit. Фиксировать objective и actual weight update отдельно; это уже известная тема |
| Vocab/framing/усечение/position | [current profile][config], [exact material][exact], V309 runner с другим hashing | Сравнивать model-visible IDs и targets одного learner; historical vocab2048 не численный рецепт для BPE629. Смена tokenizer требует совместимого state |
| AST label/features/KNN majority | [FileActionKnn][file-knn] / [Facade][facade] | Do label bug; constant4/k3 уже признан. LF-01 отделяет label producer от features/k; не универсализировать C# AST для перевода |
| Parent DPO→typed Units | [TranslationStructuredBatchBuilder][builder] и [TranslationDpoTrainerBridge][bridge] | Три строки имеют разные цели и recipients. LF-04 проверяет конкретный verifier writer; количество 3 не доказывает semantic correctness |
| Phrase mappings не доходят до обучения | [Tools reader][ingress-reader] / [builder][ingress-builder] | FIXED_ALREADY с matched11/24→19/24. Не повторять исправление; проверить остальные declared carriers адресно |
| Кириллица/контекст verifier | [TranslationTrainedUnits][units] | N57 — известная тема; LF-04 выявляет её воздействие на новую E1 панель без второго code patch |
| Learned Unit versus deterministic oracle | [исходная архитектура][task] требует обучаемые Units и явную композицию | Число классов/Flow/ILearningUnit не доказательство learning. Точный writer→state→consumer; fixed snippet либо gold injection не заменяет Student |
| Restore модели/памяти | [AdamW portable state][adam], [P25][p25], актуальные loader recipes в 02 | Тождество base/adapters/tokenizer/moments/RNG и actual reader. Cold output допустим; если restore блокирует continuation, это первая очередь |
| KNN eligibility/negative neighbors/арбитр | [KNN authority amendment][knn-authority] задаёт domain pool и positive/negative/warning evidence | Изменение neighbor mix релевантно только actual trained table/consumer; proximity сама по себе не acceptance. Домены и планы/код/перевод не смешивать |

Для всех строк подробные существующие P IDs, рецепты и открытые связи собраны в [02](02-experiment-coverage-audit.md). Готовность != положительный результат. Что уже исправлено, что является source-confirmed gap и что остаётся hypothesis, указано отдельно; однозначная главная причина плохого RP01 free generation по этим свидетельствам не установлена.

## 5. Какой вывод использовать при продолжении работы

Сначала подготовить один выбранный actual learning путь и сохранить достигнутые improvements: реальную LoRA mutation, переданные phrase mappings, малые native KNN/Unit writers. Затем сравнить конкретный потерянный training signal или состояние при одинаковом исходном материале. Ресурсные LF-02/LF-03 полезны сами по себе, но не должны вытеснить MASK/STATE/ORDER и смысловые Unit-вопросы только потому, что их легче реализовать. Потребление и восстановление learned output исследуются по зависимости; unrelated отчётные проверки остаются вне учебной программы. [Конкретный порядок и original owners](03-learning-first-work-order.md).

Любой измеренный полезный сдвиг признаётся успехом своей оси и предметной темы. Все потери показателей остаются в том же отчёте. Исторические цифры здесь дают основания выбирать малые опыты; они не заменяют их и не разрешают приписывать новые результаты данному аудиту.

[rp01-cs]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/scientific-result.json
[rp01-enru]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json
[config]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/config/code-documentation/generation-v1.json
[p125]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md
[p25]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md
[ingress]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md
[ingress-reader]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain.Tools/Program.cs#L4442-L4472
[ingress-builder]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain.Tools/Program.cs#L4682-L4702
[e09-report]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/experiments-v2/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.md
[e09-code]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E09StructuralKnn.cs
[e25-report]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json
[e25-code]: https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/Experiments/E25PatternClassifier.cs
[e43-report]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json
[e43-code]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs
[v309-report]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/experiments-v2/INTERNAL-TRAIN-READY-V191/results/train-ready-v309-v307-plus-v308-plan-head16-full-20260512-004000/internal-train-ready-dpo-summary.json
[v309-runner]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/experiments-v2/INTERNAL-TRAIN-READY-V191/runner/InternalTrainReadyDpoRunner/Program.cs
[exact]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/Models/DpoExactPairMaterial.cs
[dpo]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L832-L1120
[lora]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/10.LoRA/LoraLinear.cs
[incremental]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/IncrementalTrainer.cs
[sft]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/04.TrainingLoop/Trainer.cs#L102-L148
[session]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L403-L419
[tensor]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Core/Tensor.cs#L94-L103
[file-knn]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/Prediction/FileActionKnn.cs
[facade]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/12.SemanticCompiler/SemanticCompilerFacade.cs
[builder]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L80-L141
[bridge]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationDpoTrainerBridge.cs
[units]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/27.TranslationDomain/TranslationTrainedUnits.cs#L1093-L1303
[adam]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/04.TrainingLoop/Optimizers/AdamW.cs#L182-L307
[task]: https://github.com/lubochka/xiigen-planning/blob/1ad622fe1f6d1e8b0860265d8c208aeada3fefb0/llm%20mvp/design%20to%20code%20session/original%20task.md
[knn-authority]: https://github.com/lubochka/xiigen-planning/blob/1ad622fe1f6d1e8b0860265d8c208aeada3fefb0/llm%20mvp/llm%20dynamic%20flow/ARBITER-KNN-AUTHORITY-AMENDMENT-v1.md
