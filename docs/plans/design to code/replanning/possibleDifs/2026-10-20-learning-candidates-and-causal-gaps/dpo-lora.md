# Независимая проверка DPO / LoRA / TensorCore и положительных учебных результатов

## Основной вывод

Сохранённые `embedding-sft8-01` и `dpo-mask-live8-01` содержат **реальные успехи в обучении**, которые нельзя целиком отнести к «неудачным» только из-за 0/24 правильных переводов. Первый восстановил trainable embedding и уменьшил средний online SFT objective; второй исправил производную дробных масок, и её изменение дошло до LoRA optimizer moments и итоговых weights. Оба исправления **уже находятся в текущем pinned code**. Здесь добавлены доказательные кандидатные записи для их сохранения и корректной классификации; повторные product patches не предлагаются.

Новый найденный фактор — **локальность градиента правого MatMul-операнда** во время реальных LoRA updates. Текущая реализация gradB делает strided inner reads; однофакторная перестановка скалярных циклов может уменьшить стоимость обучения. Это **не выполненный опыт, не установленный speedup и не доказанное объяснение отсутствия правильных переводов**. Новая маленькая карточка фиксирует exact host, entrypoint, пути, данные, бюджет и измерения.

В этой работе выполнены только чтение pinned исходников/сохранённых receipts и арифметическая проверка JSON. Продукт не изменён; новых запусков обучения нет; защищённый remote-каталог `docs/plans/replanning-possible-diffs` не менялся. Downloaded snapshot содержит полученное текстовое содержимое; raw Git byte identity этим способом не аттестуется.

## Документы этого дополнения

| Файл | Назначение |
|---|---|
| [positive-embedding-sft8-candidate.md](positive-embedding-sft8-candidate.md) | Самостоятельная положительная candidate record EMB-SFT8-RETAIN, точные показатели/знаменатели, ограничения и current source status |
| [positive-dpo-mask-live8-candidate.md](positive-dpo-mask-live8-candidate.md) | Самостоятельная positive record DPO-MASK8-RETAIN; live LoRA updates, fractional legacy scope, все улучшения и ухудшения |
| [positive-neural-candidates.json](positive-neural-candidates.json) | Машиночитаемый пересчёт восьми online losses, времени, cost deltas, проверок материала и hashes источников |
| [new-gradb-locality-card.md](new-gradb-locality-card.md) | Новый документный эксперимент GRADB-LOCALITY-ENRU-01; результат `NOT_RUN`, не dispatch |
| Этот файл | Сводка текущего source review, counterevidence к устаревшим утверждениям и ограничения полноты |

**Pins:** core `9622b575a6aabb79939b12cc9f5d96313f1bf370`; planning `de270758087502a350c4fcf460acdf1638bdd318`. Все GitHub source links в кандидатах используют immutable SHA. Добавление документов в planning не должно переписывать защищённые первичные отчёты.

## Положительные результаты: что следует признать успешным

| Показатель | EMB-SFT8 A→B | DPO-MASK8 A→B |
|---|---|---|
| Native учебная работа | 8 SFT updates в каждом плече | 8 DPO updates в каждом плече |
| Обучаемый получатель | Full-model55 tensors, включая embedding | 16 q/v-LoRA tensors, frozen39 base/reference |
| Полезный подтверждённый успех | Reachability54/55→55/55; embedding coordinates0→38 400/322 048; FD0/24→24/24 | Native FD126/162→162/162; float64 oracle42/54→54/54; effect во всех16 optimizer first moments на каждом из8 updates |
| Средний online loss | **7.0169490625→6.9877417**, Δ−0.0292073625 | 0.69314783875→0.693147945, Δ+0.00000010625 |
| Собственная accuracy | 0/24→0/24, gained0/lost0 | 0/24→0/24, gained0/lost0 |
| Сумма времени учебных шагов | 24 505.3504→24 391.5929мс, локально−113.7575мс | 36 113.9900→34 532.0568мс, локально−1 581.9332мс |
| Total harness | 142 083.4058→149 435.7635мс, ухудшение | 155 791.2765→159 018.7596мс, ухудшение |
| Process peak working set | +89 014 272байт | +240 734 208байт |
| Текущий исправленный код | [Embedding.Forward:43–79][embedding] | [BackwardLogProbs:1072–1121][mask] |

Источники: [первичный EMB отчёт][emb-report], [первичный DPO отчёт][dpo-report], [EMB before steps][emb-before]/[after][emb-after], [DPO before steps][dpo-before]/[after][dpo-after]. Таблица не заменяет полные cost tables в двух candidate records.

Важные уточнения измерения:

1. **EMB loss сравним:** тот же tokenizer, одинаковые material bytes, порядок tr01…tr08, вес1, восемь updates, fresh AdamW, LR1e−5, WD0, dropout0. Значение каждого урока — CE всей prompt+chosen последовательности до его update; среднее по восьми урокам, не по всем490 targets и не heldout. Равенство начального loss при первом уже различающемся update согласуется с причинной последовательностью. [Harness:54–79][emb-harness]; [Trainer:102–147][trainer]
2. **DPO loss сравним:** те же восемь пар и masks, β/optimizer/objective и initial state одинаковы; различается только backward. Среднее online loss чуть ухудшилось. Online `DpoMetrics.Accuracy` 4/8→4/8 — знак preference margin, не правильность 24 переводов. [Step:668–691][step]; [live8 validation][dpo-validation]
3. **Дробные маски участвуют в реальном обучении.** Фактор не ограничен guard на logits: native moments16/16 различаются после каждого шага, policy weights после восьми updates различаются, frozen invariants сохранены. Но стандартный binary exact-v2 RP01 этим fractional legacy опытом не объяснён. [DPO report:9–22][dpo-report]
4. **Стоимость имеет точный scope.** EMB step timer включает issued receipt I/O; DPO timer окружает native Step вместе с checkpoint publication, но без последующего audit export optimizer. Полный harness, CPU, peak RAM и allocations показаны отдельно. Единичный измеренный выигрыш полезной метрики признаётся локальным успехом, без обещания устойчивого speedup.
5. **Нельзя выдать смену состояния за предметную точность.** Эти записи прямо сохраняют 0/24 и все ухудшения. Однако обратная ошибка — отвергнуть исправление градиента и train loss из-за отсутствия accuracy gain — тоже устранена.

## Текущие source-факты, которые ограничивают старые диагнозы

| Возможное старое утверждение | Факт pinned source | Вывод для нового аудита |
|---|---|---|
| «Embedding вообще оторван от autograd» | [Embedding:43–79][embedding] сохраняет ID, weight child и scatter-add backward, условно при RequiresGrad | Это уже исправленная историческая проблема. Сохранить положительный кандидат, не заново поручать patch |
| «DPO backward не соответствует fractional mask» | [BackwardLogProbs:1083–1116][mask] использует Σmask и mask multiplier | Успешное исправление уже есть. Не перенести fractional finding на binary exact без данных |
| «DPO в текущей ветке только двигает числа метрик, не делает neural backward» | [Step:665–681][step] строит policy graphs, вызывает BackwardLogProbs для обеих сторон и ApplyOptimizer; [BackwardLogProbs:1120][mask] вызывает logits.Backward | Такое общее утверждение текущему коду противоречит. Качество самого обучения всё ещё требует предметных результатов |
| «SFT caller неизбежно получает lr0 на первом шаге из стандартного warmup» | [IncrementalTrainingSession:405–406][warmup] явно ставит WarmupSteps=0; [Trainer:128–145][trainer] отвергает nonpositive LR | Нельзя считать default scheduler issue установленным блокером данного production caller |
| «Неучтённый LoRA dropout объясняет сохранённый RP01 simulation run» | [SimulationDpo:105–122][dropout] отвергает ненулевой dropout этого bounded path; positive harness также требует0 | Общий dropout drift остаётся profile-specific вопросом, не доказанным объяснением этих запусков |
| «Нулевой первый grad A у fresh LoRA означает сломанный backward» | [LoraLinear:92–101][lora-init] инициализирует A случайно и B нулём; forward — xAᵀBᵀ | По правилу цепи первый A-gradient при B=0 закономерно0; не путать с disconnected graph. C50 уже другой, обученный старт |
| «Любой current DPO обязательно использует полный prompt mean» | [ExactMaterial:19–22,41–46][material] задаёт response-sum objective и отдельные response masks; [Step][step] выбирает objective по material | Legacy и exact profiles нужно разделять. Fractional live8 специально legacy; обычный exact материал другой |
| «DpoMetrics после Step — post-update eval checkpoint» | [Step:668–691][step] вычисляет loss/rewards до ApplyOptimizer, затем сохраняет эти values | Это online train measure. Подпись «heldout/final model loss» недопустима |

Это адресное source review, не новая численная сертификация всех kernels. В частности, из этих проверок нельзя заключить, что вся восстановленная модель, tokenizer, optimizer resume и downstream generation полностью эквивалентны успешным историческим learners.

## Новый механизм: GRADB-LOCALITY

Факт: [TensorOps:679–698][gradb] реализует правый adjoint скалярным порядком batch→inner→column→row. Для каждой scalar output coordinate он повторно проходит страйдовые A/G строки. [LoraLinear:118–123][lora-forward] ставит trainable LoRA Aᵀ/Bᵀ справа от двух MatMul, поэтому участок является непосредственной частью DPO backward.

Гипотеза: скалярный порядок batch→row→inner→column и reuse одного A scalar на contiguous row G может уменьшить работу memory subsystem и elapsed cost real update. Смысл градиента и последовательность суммирования каждой координаты сохраняются; ни SIMD, ни новый objective не включены. Польза измеряется временем/CPU/allocations той же фактической учебной работы. Рост точности для такого успеха не нужен. При отсутствии измерений нельзя обещать экономию или объяснять плохие переводы этой стоимостью.

Новизна проверена полнотекстовым поиском по **662 planning md + 296 protected md**. Полнота protected Markdown сверена с remote inventory (`protected-corpus-inventory.json`); это полнота текстового поиска, не заявление полного ручного смыслового review всех 296 файлов. Точный поиск `gradB/right operand/AᵀG` дал **3 строки в 3 файлах**; ближайший protected N-ADJOINT-01 явно ограничен **gradA** и исключает gradB. P110 охватывает общую численную стоимость, но не данную reached loop-interchange причину. Полный запрос и ссылки prior art записаны в [карточке](new-gradb-locality-card.md). Это bounded novelty в данном корпусе, не заявление об отсутствии такой оптимизации в мировой литературе.

Карточка переиспользует существующий live8 DpoLive/Rp01SimulationDpo host, фиксирует те же8pair/24eval данные, настоящий admission и native DPO. Source-копии/host/output назначены вне защищённой папки. Архивный C50 и native runtime binaries исключены из GitHub, поэтому доступность этих точных inputs должна быть проверена будущей подготовкой. **Сегодня runner не создан, compilation/run отсутствуют, результаты NOT_RUN.** Это честно описанная новая гипотеза, не притворно готовая команда существующего generic CLI.

## Полнота и пределы работы

Скачаны и адресно исследованы 59 core source файлов из TensorCore, TransformerModel, TrainingLoop, LoRA, DPO и relevant ContinuousLearning цепочек. Полные computational regions DpoTrainer/MatMul/Embedding/LoRA/Trainer сопоставлены с вызывающими участками; giant source files не объявляются полностью проверенными лишь потому, что скачаны.

Для двух положительных кандидатов прочитаны protected reports, validations, harnesses, initial/trained state, material, восемь per-step receipts, parameter updates, costs, preregistration/finalize и архивные commands. Повторно не запускались генератор, scorer, finite differences или тренировка. SHA256 ключевых receipt-файлов сохранены в JSON с явным `hash_scope=local_materialized_download`: они относятся к локальным текстовым копиям, в которых download/apply_patch мог добавить terminal LF или нормализовать переводы строк. Эти hashes не объявлены идентичностью raw Git blob. Версию источника задают pinned GitHub URLs; численные результаты взяты из сохранённых receipts, нового измерения нет.

Исторические branch-vs-current различия KNN/units, domain data admission, restore ownership и полный каталог experiments покрываются другими частями общего аудита; этот dossier не объявляет их независимо проверенными. Текущая code presence исправлений подтверждена, но самостоятельная проверка commit lineage их внедрения и полная историческая binary-реконструкция здесь не выполнялись.

При подготовке документа использованы code-examination и improvement-measurement требования вместе с human-communication glossary. Прямые правила пользователя об experimental copies, полезных метриках и documentation-only имеют приоритет над старыми требованиями обязательного product test gate.

## Предложение по включению в основной пакет

Первые две candidate records добавить как **успешные измеренные исправления, уже присутствующие в source**, а не в очередь «ещё починить». Сохранить по ним формулировки точности 0/24 и scope профилей. Новую GRADB-LOCALITY карточку добавить в раздел **новых неподтверждённых гипотез стоимости настоящего обучения**, после разблокировки обязательных учебных путей. Никакого автоматического разрешения patch/launch/CI из этого статуса не следует.

[embedding]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/00.TensorCore/Layers/Embedding.cs#L43-L79
[mask]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/11.DPO/DpoTrainer.cs#L1072-L1121
[emb-report]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/report.md
[dpo-report]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/report.md
[emb-before]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/before/training-steps.json
[emb-after]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/after/training-steps.json
[dpo-before]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/before/training-steps.json
[dpo-after]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/after/training-steps.json
[emb-harness]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/harness/Program.cs#L54-L79
[dpo-validation]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/validation.json
[trainer]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/04.TrainingLoop/Trainer.cs#L102-L147
[step]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/11.DPO/DpoTrainer.cs#L650-L697
[warmup]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L400-L408
[dropout]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L105-L122
[lora-init]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/10.LoRA/LoraLinear.cs#L92-L101
[material]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/11.DPO/Models/DpoExactPairMaterial.cs#L19-L46
[gradb]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/00.TensorCore/Core/TensorOps.cs#L679-L698
[lora-forward]: https://github.com/lubochka/llm_mvp_core/blob/9622b575a6aabb79939b12cc9f5d96313f1bf370/10.LoRA/LoraLinear.cs#L112-L125

