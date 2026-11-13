# MICRO-N33 — три сверхмалых нативных проверки и два причинных учебных контраста

<!-- AUDIT-2026-11-09-3161:START -->
## Уточнение применимости после аудита переноса — 9 ноября 2026

Ниже сохранён паспорт на коде от16 октября, а не описание всех текущих дефектов. Перед новым поручением сопоставить каждый owner и oracle с core `31612135c97e15be64e1b2a4a21efd3ffd58e443`; уже исправленные pre-step gradients не реализовывать заново. Конкретный остаток [C04](https://github.com/lubochka/llm_mvp_core/blob/31612135c97e15be64e1b2a4a21efd3ffd58e443/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/105-addressed-repair-plan-2026-10-30.md) касается возвращаемого loss: ограничение `-log(max(p,1e-7))` не соответствует производной `p-onehot` при насыщении. Это отдельная граница от правильного порядка backward и от численной конечности.

Для назначенных математических проверок дополнить независимый oracle случаями по обе стороны1e−7, logits `[10,0,-10]`/target2 и общим сдвигом logits; scalar loss и все производные относятся к одному состоянию до update. Устойчивый эталон — `logsumexp(logits)-targetLogit`; новый ожидаемый результат не берётся из самого текущего TrainStep. Контроль насыщенного loss отличает≈20,000045 от≈16,118095 и ненулевую производную от нулевой производной ограничения. Эта чистая проверка формулы не назначает повтор SFT/DPO или изменение исходных доз ниже.

Протокол разделяет четыре вывода: вызовы выполнены; значения конечны; цель и производная согласованы; обычный потребитель улучшил ответы. Ни одно из первых трёх не заменяет четвёртое. Для копии алгоритма и реальной сборки сохранить отдельные статусы; Python-повтор формулы не выдавать за native PASS. В промпт переноса включить именно отрицательный насыщенный случай и его положительный обычный контроль, а не общее пожелание «проверить градиенты».
<!-- AUDIT-2026-11-09-3161:END -->

**16.10.2026 · EXECUTOR-READY DESIGN, NOT_EXECUTED.** Протокол относится к [новому N33 source-разбору](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md). Цель — быстро фальсифицировать точные алгоритмические условия и **только затем** измерить перенос на смысл ответа. Никаких 500 DPO, Windows CI, полной перестройки Unit, обновления production/checkpoint до решения CODEX. Изменения кода возможны только в отдельном санкционированном рабочем контуре; **текущий документ не меняет core**.

## Эксперимент 0: различить ошибку градиента и отсутствие способности модели

Структура каждого шага: **вопрос → гипотеза → источник/вход → A/B с одним фактором → наблюдаемая метрика → критерий опровержения → scope**. Логировать core SHA, .NET runtime, seed, model config, tokenizer SHA, parameter/optimizer SHA до и после, consumed task ID, и статус `NATIVE_RUN` vs `SOURCE_ONLY`.

### N33.M0.A — детерминированный probe настоящего Embedding

- **Гипотеза:** собственная весовая матрица Embedding не получает градиента, хотя аналитическая производная ненулевая. Это проверка вычислительного графа без language model, DPO и GPU.
- **Данные:** `Embedding(vocabSize:8, hiddenDim:3)`; ID `[3,3,1]`, Tensor shape `[1,3]`; функция `L = sum(Embedding.Forward(ids).Data)`. Базовая матрица фиксирована любыми конечными float значениями; выбор конкретных чисел не влияет на производную. Убедиться, что `emb.Weight.Value.RequiresGrad == true`.
- **План:**
  1. `var output = emb.Forward(ids);` сохранить `output.RequiresGrad` и наличие ребра до `Weight` через наблюдение `Grad` (без reflection).
  2. Сформировать `dL/dOutput = ones` той же формы и вызвать `output.Backward(ones)`. Сохранить `emb.Weight.Grad` (ожидается null **на текущем source**).
  3. Независимо изменить `emb.Weight.Value.Data[3 * 3 + 0]` на `+eps` и `-eps`; считать `(Lplus-Lminus)/(2*eps)`. Затем аналогично ID=1. Установить `eps=1e-3`; допускается `1e-2` при float noise.
  4. **A:** неизменённый исходный Forward; **B:** в disposable local harness правильный differentiable gather, где output связывается с `Weight.Value` и `BackwardFunc` делает `gradWeight[tokenId, dim] += gradOutput[position, dim]` для каждого токена. Сравнить native gradients для 3×8 таблицы, включая повторённый ID.
- **Предсказание:** аналитическое и finite-difference `dL/dW[3,0]=2`, `dL/dW[1,0]=1`; A даёт `Grad null`, B даёт **2 и 1** (по всем 3 hidden dimensions). Если A native уже даёт 2 и 1, исходная N33-A **DISCONFIRMED** и необходимо обнаружить иную связь.
- **Фактическое исполнение:** `NOT_RUN`. Source уже прочитан; этот пункт — требуемый executable test design, а не присвоение результата.

**Примечание по тесту:** `Tensor.Backward(initialGradient)` устанавливает initial grad даже для выхода без `RequiresGrad`; оно не создаёт недостающее `BackwardFunc`/Children задним числом. Проверка лишь `output.Grad != null` — ложноположительная; критично именно `emb.Weight.Grad`.

### N33.M0.B — нулевой LR и различие supervised/pretraining

- **Гипотеза:** `WarmupSteps=100` по умолчанию ведёт к `GetLr(0)=0`; SFT запрещает LR<=0 и никогда не совершает первый update, pretraining засчитывает update с нулевой дельтой весов.
- **M0 исходники:** `04.TrainingLoop/Schedulers/CosineWithWarmup.cs:GetLr`, `TrainingConfig.cs`, `Trainer.cs:TrainSupervisedNextTokenStep`, `Trainer.cs:RunPretrainingCoreAsync`, `AdamW.cs:Step`.
- **Вход:** создать валидную маленькую `LlmModel(ModelConfig)`, optimizer `AdamW`, список 4–8 token IDs в диапазоне vocab; один валидный native SFT call и один pretraining step `TotalSteps=1, WarmupSteps=100`. Внешние DPO не требуются.
- **A (source defaults):** использовать `WarmupSteps=100`, логировать `scheduler.GetLr(0)`, исключение и `W_before/W_after` SFT; во втором отдельном мини-run pretraining логировать completed, parameter hash, AdamW.step/moments. **Не смешивать разные методы.**
- **B (минимальный repair):** изменить **только** SFT-интерпретацию начального индекса schedule на 1-based (`GetLr(_supervisedStep+1)`), сохранить всю остальную модель/данные/seed/optimizer; теперь первый SFT-update должен доходить до optimizer при конечном LR>0. Отдельно pretraining сравнить исходный `GetLr(0)` с 1-based start.
- **Критерий:** в A native SFT throw before optimizer и `supervisedStep` не увеличен; в B throw отсутствует по причине LR, есть изменение разрешённого trainable веса при ненулевом градиенте. Pretraining A может увеличить moment/step без weight change. Если A не падает при зафиксированном `WarmupSteps=100` и валидном forward — гипотеза N33-C **опровергнута**.
- **Граница:** этот runtime не является `DpoTrainer.StepMany`; даже точный `M0.B PASS` не объясняет напрямую RP01 C#500/ENRU500.

### N33.M0.C — bias обычного Linear

- `Linear(inFeatures:2,outFeatures:1,useBias:true)`, вход shape `[1,2,2]` из нулей, scalar loss=sum(output). Аналитически `dLoss/dbias[0]=2`.
- A: `Forward→Backward(ones)` текущего `Linear`; B: `ExpandBias` c differentiable broadcast/reduction, сумма по batch/seq. Заморозить вход, веса. Проверить `Bias.Grad` и central finite difference, ожидаем 2.
- **Не тестировать как bug обычной LoRA**, где основные linear-проекции без bias. Отдельно доказать фактический trainable biased consumer, прежде чем давать P0 продуктовый приоритет.

## Эксперимент 1: учится ли базовая нейросеть после восстановления графа?

**Цель:** не остановиться на `Grad exists`. Небольшая нативная проверка `neural self-learning`, а не имитация смыслового обучения JS.

- **Фиксация:** один B0 full-checkpoint, одинаковые tokenizer/vocab, RNG seed, границы ответа, 8 коротких source-family-disjoint учебных случаев (например, 4 rule families C#, 4 EN→RU; не смешивать одновременно для первого мини-опыта), отдельные 8–16 невиденных перефразировок и 4 ранее известных retained laws. Стратифицировать по семье, длине и negation/operator. Holdout исключить из tokenizer training и вариантов negative/chosen.
- **Исключение независимого блокера:** оба плеча A/B используют один и тот же заранее проверенный положительный LR (только N33-C repair), иначе нулевая learning rate маскирует влияние Embedding.
- **A:** train на те же 8 текстов, текущий `Embedding` without gradient. **B:** единственная разница — native corrected Embedding `BackwardFunc` с повторными token ID; другие параметры/optimizer config/steps идентичны. 8–32 маленьких supervised steps; логировать только реальную длительность при исполнении. Не обучать DPO на этом этапе.
- **Механизм:** `max_abs(embedding_W_after-before)`, checksum, grad norm, teacher-forced next-token NLL; должно измениться только в B. Если другой слой мешает forward — `BLOCKED`, не объявлять `NO_EFFECT`.
- **Качество:** frozen prompts `BEFORE/AFTER` ordinary `TextGenerator`, token-level NLL на heldout, семейная accuracy, critical mistakes/false accepts, количество abstain, exact stop reason, regression прежних 4 законов. Нужен слепой semantic judge CODEX/Astra 6 UltraCode для продуктового `QUALITY_GAIN`. Если B лучше на train-loss, но не на heldout, вывод `GRADIENT_REPAIRED_NO_GENERALIZATION_YET`; не раздувать pilot на 500 пар.
- **Causal verdict:** B показывает преимущество над A при одинаковом шаге и новых семьях + нет критической регрессии; только тогда принимать влияние N33-A на **SFT**. Прямой RP01 DPO impact возможен, если доказана цепочка `warm base→DPO→same consumer`; не делать скачок.

## Эксперимент 2: не сбрасывается ли LR после cold restore?

Фиксировать одинаковую модель, tokenizer, optimizer state, batch, scheduler config; выбирать `WarmupSteps=0` **только для снятия первого throw N33-C**; `TotalSteps=8`, `MaxLr>MinLr`. A = два последовательных SFT updates одним `Trainer`. B = после первого (тот же optimizer state и model weights) создать новый `Trainer`, второй update на идентичной паре. Проверить `GetLr(1)` vs `GetLr(0)`, `weight SHA` и heldout output; при отсутствии реального lifecycle recreate — `CONDITIONAL_ONLY`. Отдельно оформить перенос scheduler position в future state schema, не менять production при одной source-догадке.

## Если потребуется перейти к DPO / Unit: строгие границы ролей

**CODEX / Astra 6 UltraCode** = выбор учебных случаев в роли человека, затем **независимая** проверка качества `chosen` и `rejected` как разных смысловых ответов и blind оценка выученного закона. **Внешняя разрешённая OPEN китайская модель** = автор **обеих финальных** DPO сторон. Neural M0/M1 без новой DPO-пары не требуют учителя; не подменять CODEX статическим классификатором либо самой китайской моделью. При любом DPO опыте фиксировать `selected_case_id, teacher_model_id, exact_pair_sha, grade_actor_id, recipient_id, checkpoint_id, actual_consumer_id` и независимое право veto.

## Рубрика отчёта после запуска (пока пустая)

| Task | Source branch/commit | L0 mechanism | L1 actual learning | L2 cold serving | CODEX/Astra judge | Decision |
| --- | --- | --- | --- | --- | --- | --- |
| N33.M0.A Embedding | `codex/dpo-implementation@32ac6e7` | NOT_RUN | NOT_RUN | n/a | n/a | OPEN |
| N33.M0.B warmup | same | NOT_RUN | NOT_RUN | n/a | n/a | OPEN |
| N33.M0.C bias | same | NOT_RUN | NOT_RUN | n/a | n/a | OPEN |
| N33.M1 SFT quality | same | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | OPEN |
| N33.M2 LR restore | same | NOT_RUN | NOT_RUN | NOT_RUN | NOT_RUN | OPEN |

**Запрещённые заключения:** тест asserts pass ⇒ модель научилась; LoRA changed ⇒ user answers better; old kNN E09 positive ⇒ repair SFT bug доказан; runner model changed ⇒ source of human-role DPO correct. **Проверка плана:** source contract → single intervention → independent observation → domain-specific falsification → real consumer → blind verdict. Правки только в `xiigen-planning/llm_plan3/.../possibleDifs`; `llm_mvp_core/docs/plans/replanning-possible-diffs` никогда не модифицировать.
