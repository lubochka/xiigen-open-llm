# Независимый neural-аудит: новое наблюдение ENRU и гипотеза на границе доменов

18 октября 2026. Дополнение к восстановлению PrepareToGPL и аудиту RP01. Core и защищённый каталог не изменены. Новое обучение и новая предметная оценка не запускались.

## Главный результат

В сохранённых оценках ENRU после перехода C50→C100 **teacher-forced likelihood правильного chosen ответа снизилась на всех 20 из 20 вопросов**. У 19 из 20 одновременно снизилась likelihood rejected; у 11 из 20 относительный normalized margin всё же вырос. В число этих 11 входят все четыре вопроса, для которых дополнительно запускалась свободная генерация. Это воспроизводимый пересчёт уже существующих оценок, а не вывод из изменившихся весов.

Такой результат уточняет, *из чего именно сложилось* небольшое улучшение ENRU preference: в среднем rejected снизился сильнее, чем chosen. Он не доказывает ошибку формулы DPO или причинность конкретного optimizer state. Причинная гипотеза для различающего опыта — перенос первого момента AdamW с C# в ENRU при одновременной установке нового frozen reference C50. Проверка должна обнулять только первый момент, сохраняя второй момент, счётчик шага и остальные условия. Отдельный условный пробел нижнего checkpoint API — tensor identity без LoRA Alpha; фактический RP01 descriptor эту границу защищает.

## 1. Что зафиксировано и с чем сопоставлено

| Срез | Полный commit |
|---|---|
| Core, codex/dpo-implementation | a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6 |
| Успешная по сообщению пользователя PrepareToGPL | 31346d8928be4469b01aaf313780e8f950ff91e2 |
| Соседняя DesignToCode | 8f20ceb8a9bfbadab2ad309226c47dd76e714548 |

Применены пользовательские generated-code-review и trainable-architecture-decision skills. Проверены реальные owners 01.Tokenizer, 03.TransformerModel, 04.TrainingLoop, 10.LoRA, 11.DPO, их TensorCore dependencies и RP01 training/evaluation/continuation callers. Изучены ближайшие записи двух каталогов; локальный inventory содержит 260 protected + 272 planning Markdown-документа. Новизна ниже относится к этому срезу 532 файлов, а не ко всем возможным чужим сообщениям или незагруженным артефактам.

Старые blanket выводы не повторяются как новые: случайный B0, отсутствие языкового pretraining, сумма/среднее response logP, sparsity-only TeachingVector, отделение generation от preference, mask derivative, detached embedding и SFT warmup уже покрыты каталогами. Учтены реально исполненные 18 октября dpo-mask-live8 и embedding-sft8: исправления повлияли на native gradients/state, но в соответствующем фиксированном свободном экзамене осталось 0/24. Поэтому наличие нового gradient path нельзя объявлять восстановленным предметным обучением. См. [mask report][oct-mask] и [embedding report][oct-embedding].

Перед публикацией дополнительно сверены11 новых planning Markdown из40–42 и новая версия README на `a4a3e709f8fb5310baab6daa16501b5d4394bd72`. Фактическаядекомпозиция 20/20 случаев и отдельный m50-only опыт там не найдены. Подробности и чужой приоритет legacy4→6 отражены в [проверке новизны](06-EVIDENCE-AND-NOVELTY.md#дополнение-перед-публикацией-учтены-параллельные-главы4042).

## 2. N-NEURAL-01 — абсолютный chosen score ENRU ухудшился на 20/20

**Статус:** новое наблюдение на сохранённых фактических Student scores. Причина ухудшения не установлена. **Получатель:** neural policy C50→C100, response-token scoring; не discrete translation memory и не KNN.

### Доказательства и воспроизводимость

Скрипт [recompute_likelihood_components.py](evidence/neural/recompute_likelihood_components.py) читает шесть сохранённых evaluation-receipt.json, не вызывает модель. Подробный результат — [likelihood-components.json](evidence/neural/likelihood-components.json); исходные JSON находятся в evidence/neural/measurements/. Для каждого A/B он проверяет совпадение panelSha256, набора ID, числа target tokens и отсутствие ошибки; у ENRU объединение 16+4 проверяется на непересечение ID. Исходный C# float восстанавливается через IEEE binary32 round-trip, затем разности считаются в binary64. Дополнительно проверено точное равенство normalizedMargin = chosenMean − rejectedMean и chosenSum = chosenMean × chosenTargets после round-trip.

| Панель и состояние | Δ mean chosen | Δ mean rejected | Δ normalized margin | chosen снизился | chosen снизился при выросшем margin |
|---|---:|---:|---:|---:|---:|
| C# 25, C0 saved-root→C50 | +0.002205772399902344 | +0.000978240966796875 | +0.0012275314331054688 | 4/25 | 1/25 |
| ENRU 20, C50→C100 | −0.0010038137435913086 | −0.0011138200759887695 | +0.00011000633239746093 | **20/20** | **11/20** |

На ENRU среднее исходное chosen −7.210505723953247 стало −7.211509537696839. Это около −0.10033% для геометрического среднего вероятности правильного target token; величина мала. При взвешивании chosen-разностей числом target tokens знак сохраняется: −0.0010258619067738358. Ни разные длины chosen/rejected, ни объединение панелей не меняют того факта, что каждый из двадцати paired chosen scores стал хуже.

Для четырёх вопросов с free generation сохранены такие **teacher-forced** разности:

| ID | Δ chosen | Δ rejected | Δ margin |
|---|---:|---:|---:|
| enru-e01 | −0.0008206367492675781 | −0.0016832351684570312 | +0.0008625984191894531 |
| enru-e02 | −0.0017580986022949219 | −0.0020084381103515625 | +0.0002503395080566406 |
| enru-e03 | −0.0007624626159667969 | −0.0009217262268066406 | +0.00015926361083984375 |
| enru-e04 | −0.0009322166442871094 | −0.0011925697326660156 | +0.00026035308837890625 |

Это не пересчёт semantic correctness свободного текста. Receipt явно помечает qualityVerdict=NOT_GRADED_BY_CODE_EXECUTOR, optimizerCalls=0 и externalTeacherCalls/externalEvaluatorCalls=0. Новых вызовов при нашем анализе также ноль.

### Primary sources

- [ENRU C50 score16][enru-c0-score] и [C100 score16][enru-t-score]: строки 2–5 фиксируют panel/weights, 14–190 содержат исходные scores.
- [ENRU C50 free4][enru-c0-free] и [C100 free4][enru-t-free]: строки 14–58 содержат четыре сопоставленных score, 60–64 — ограничения.
- [C# C0 score25][cs-c0-score] и [C50 score25][cs-t-score]. В раннем C# receipt поле domain отсутствует, candidateSha256 у C0 равно null: это saved-root branch без candidate override, а не самостоятельно сохранённый SHA внутри receipt. Главный ENRU вывод не зависит от этой ранней неполноты.
- ENRU исходные weights SHA: C50 6f4d0284ea823ba99932b0f28576855c14205703115b48c983c7a9f03749179b; C100 3acb5a9080d0454942c261eca4c1b886aab011db83729947509be4463785fab5.
- [Evaluation caller, L79–88][score-owner] вызывает response-normalized scorer отдельно для chosen/rejected, затем сохраняет обе компоненты и difference. [DPO exact owner, L891–904][dpo-margin] оптимизирует relative-to-reference difference и применяет противоположные upstream знаки. Ненулевого самостоятельного слагаемого, требующего повышения absolute chosen likelihood, в этой формуле нет.

Математически если среднее изменение c = −a, а изменение r = −b при b>a>0, то Δ(c−r)=b−a>0. Поэтому улучшенный margin совместим с нашим наблюдением. Это свойство относительного критерия; оно само по себе не доказывает неверный backward.

### Новизна относительно каталогов

Planning 32-MICRO-ONE-FACTOR-LEARNING-CONTRAST-AND-E09 уже предлагает chosen absolute logP как secondary metric. Protected P03.1 требует восстанавливать exact score lineage; protected 12-student-update-to-generation-path уже сообщает маленькое ENRU margin gain и неуспешную generation. **Новое здесь — фактическая декомпозиция этих самых сохранённых scores, 20/20 отрицательных chosen-разностей и разница с C#**, а не изобретение требования «проверять absolute score».

### Короткий опыт

Этап 0 уже выполнен: offline paired component analysis. Он показал наблюдаемость явления на старой панели с нулём model calls.

Этап 1 — выбрать ровно N-NEURAL-02 (m50-only) как причинное вмешательство на новом разрешённом ENRU corpus. После каждого из 1–8 updates логировать chosen/rejected logP раздельно на TRAIN; на новом source-family-disjoint DEV измерить обе компоненты и одинаковый ordinary free output. Старые 20 известных вопросов остаются диагностическими наблюдениями, их нельзя объявлять новым независимым экзаменом. Если обе ветки сохраняют одинаковое падение chosen и одинаковое качество, результат против momentum-объяснения, хотя это не отменяет зафиксированное наблюдение.

Предметная рубрика ENRU: полярность, модальность, участники и числа; C# retention рубрика: ветвление null/empty и границы сравнений. Тот факт, что DPO margin меняется, не подменяет правильно переведённую фразу или объяснение метода. Не добавлять SFT, другой tokenizer, LR, target modules либо KNN в то же сравнение.

**Сверка нового параллельного пакета:** DLT-K1 на planning `412d3364cc94b5aa1cdf516f829960da152826cb` независимо описывает конфликт перенесённых moments и измерение cosine. Поэтому новизной здесь считается только дополнительный причинный контраст **m50→0 при прежних v50/step50**, а не идея reset optimizer или само предположение old-direction interference. В этих новых документах actual score decomposition 20/20, 19/20, 11/20 не найдена. [Итоговая проверка](06-EVIDENCE-AND-NOVELTY.md#сверка-параллельной-публикации-412d3364).

## 3. N-NEURAL-02 — первый момент прошлого домена при новом reference C50

**Статус:** source-grounded уточнение известной гипотезы отдельным first-moment-only опытом; конфликт действительных gradients не измерен. Общая тема переноса moments уже описана другим автором в [DLT-K1](https://github.com/lubochka/xiigen-planning/blob/412d3364cc94b5aa1cdf516f829960da152826cb/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md#6-полезные-уточнения-известных-тем--не-увеличивают-число-новых-находок). **Достижимость RP01:** подтверждена для последовательности C#→ENRU, не объясняет первоначальный провал C# из B0.

### Точная разница lifecycle

| До / другой срез | Фактический текущий RP01 | Возможное следствие |
|---|---|---|
| PrepareToGPL [DpoTrainer constructor L28–35][base-trainer] каждый новый trainer создаёт new AdamW; DesignToCode [L21–28][design-trainer] делает то же. Полного current continuation lifecycle в этих constructor нет. | [Simulation L108–112][sim-load] загружает C50 в policy и независимый frozen reference. [L160–171][sim-restore] создаёт trainer и восстанавливает фактические optimizer m/v, step50 и RNG. | DPO reference anchor начинается заново на C50, а накопленная update direction принадлежит предыдущим C# batches. Это может помочь удержанию или мешать первым ENRU updates. Сам факт корректного restore не определяет качество transfer. |

Нельзя переносить вывод «new AdamW» в утверждение, что PrepareToGPL успешно училась именно из-за сброса moments. Старые нейросетевые paths имеют другие крупные отличия; успех memory learner также не аттестует этот DpoTrainer.

[AdamW L72–106][adamw] на первом ENRU update вычисляет:

m51 = 0.9·m50 + 0.1·gENRU;

v51 = 0.95·v50 + 0.05·gENRU²;

Δw = −lr·m51/(1−0.9^51)/(sqrt(v51/(1−0.95^51))+eps), отдельно от decoupled weight decay.

Если old first moment и new gradient направлены противоположно, знак update может определяться m50. Пример *только математической возможности*: m50=1, gENRU=−1 ⇒ m51=0.8; zero-first-moment arm ⇒ m51=−0.1. Это не извлечённые из RP01 реальные числа. Ненулевой второй момент и bias-correction не меняют знак.

[SimulationEnRuContinuation L141–155][source-pins] фиксирует реальный optimizer artifact SHA 1779a760d1b3a74edb158785304f99e1823ee1a16eb3562849c50549931f2fb3, length426788. GitHub file retrieval в этой сессии вернул **LFS pointer**, а не slots. Сохранённый pointer подтверждает адрес требуемого объекта; из него невозможно вывести нормы, signs или cosines m50. Не выдаём это за измеренный stale momentum.

### Отличие от уже существующих гипотез

P119 меняет LR, beta или batch policy, оставляя initial moments фиксированными. P12 содержит SFT schedule/partial-state/clip, а не first-moment switch C50. P15 меняет replay или order; P25 проверяет точное восстановление; P105 — отдельный frozen reference. N-NEURAL-02 сохраняет весь material/order/reference/second moment/step и меняет только m50. Позднее DLT-K1 уже предложил сравнить перенос moments и новый optimizer с тем же reference и весами; это покрывает общую гипотезу и cosine-диагностику. Полный новый optimizer одновременно меняет m, v и step, поэтому наш m-only contrast остаётся отдельным уточнением причинного эксперимента. Planning 26 и protected30 объясняют scale-cancellation AdamW и упоминают накопленные moments, но не задают это конкретное вмешательство на observed C#→ENRU boundary.

### Минимальный A/B и критерий опровержения

**Старт:** две независимые копии одного полного C50; тот же tokenizer, exact-v2 IDs/masks, q/v rank8 alpha16 dropout0, same seeds/RNG, learningRate1e−5, beta.1, β1=.9, β2=.95, decay.01, eps1e−8. Reference в обоих случаях независимая копия C50. Одни 8 новых ENRU train pairs, до 8 одинаковых updates, отдельные 8–12 DEV семей и C# retention cases. Возможный минимальный TRAIN contrast: “Mira must open the gate” / “Mira must not open the gate”; перевод обязан сохранить обязанность/запрет. В новом DEV — другие действия и сущности, не только замена ID.

- **A:** настоящие m50, v50, step50.
- **B:** m50 обнулён; настоящие v50 и step50 сохранены. Никакого сброса LR schedule, B matrices или RNG.
- **Отрицательный контроль:** один и тот же A state под двумя именами; zero-update parity. Компонентный знак-контроль с aligned old/new gradients нужен для проверки измерителя, а не как второй training фактор.

Перед первым optimizer update сохранить текущий g и состояние. Измерить по фактическим tensors m50·g, cosine(m50,g), долю координат с противоположными знаками g и preconditioned m51. Более важный критерий — g·Δw без decay и с decay отдельно: положительное значение означает ascent именно для локальной линеаризации текущего batch loss. Оно не равно автоматической ошибке optimizer или предметному quality verdict. При B должны совпадать все неизменяемые hashes/step/second-moment values; first moment отличается по объявлению.

**Подтверждающий сигнал:** A имеет выраженную old-direction interference, B её устраняет, и на одинаковой новой предметной панели повышается chosen score и/или meaningful free correctness без потери исходного ненулевого навыка. Report каждой оси отдельно. Если C50 old free correctness=0, preservation нуля не считается удержанием навыка.

**Опровержение/предел:** реальные m50 малы или согласованы с g; g·Δw и chosen dynamics практически равны; либо механическая разница возникает, но весь free output остаётся одинаково неправильным. Последний исход подтверждает только trajectory sensitivity, не восстановленное обучение. Такой опыт не относится к независимому KNN/memory owner без AdamW.

**Исполнение:** нужен отдельный исследовательский host на копиях, который явно представляет изменённое m50. Существующая continuation proof обязана отвергать выдачу такого состояния за точное C50 restore. Не обходить proof checks и не менять исторический applied-once main500. Quality experiment по этой гипотезе в данной сессии не выполнен.

## 4. N-NEURAL-03 — full checkpoint weights не кодируют LoRA Alpha; RP01 защищён descriptor

**Статус:** условный API/restore gap, установлен по source, native import A/B не запускался. **Приоритет:** дополнение, не объяснение текущего RP01.

[FullCheckpointLoraTopology L15–29][topology] содержит Rank/TargetModules — schema tensors. [WriteKeyedCheckpoint L1449–1473][writer] пишет version/kind/base model fingerprint/key/shape/float payload. [ComputeConfigFingerprint L1900–1916][fingerprint] включает девять ModelConfig полей, не LoRA Alpha/Dropout. [LoadCheckpoint L1140–1160][loader] сравнивает этот fingerprint и keyed records, затем копирует floats в уже сконструированные live slots.

При тех же A/B tensor bytes и rank r изменение live Alpha меняет функцию LoRA: output = base + (Alpha/r)·xAᵀBᵀ. Это [фактический LoraLinear Forward L115–125][lora-forward], не предположение о внешней библиотеке. Поэтому bare full-checkpoint import в slot, созданный с другой Alpha, может пройти tensor/schema проверки, но восстановить другую функцию. B=0 скрывает эту разницу в zero-step test.

Существенное **контрдоказательство к обвинению RP01**: [RepositoryOwnedFullLoraCheckpointDescriptor L34–48][descriptor-check] проверяет полный topology hash, включая Alpha/Dropout/seeds, и копирует canonical config; [L102–111][descriptor-create] создаёт adapters из сохранённого descriptor. [Simulation L140–171][sim-restore] дополнительно связывает RNG/optimizer с этим identity. В фактическом RP01 alpha16 и immutable descriptor; произвольный alpha32 до него не дошёл. Общий метод намеренно может полагаться на своего владельца; недостаток standalone metadata нельзя автоматически называть production bug.

**Компактный опыт без обучения:** один уже обученный checkpoint с ненулевым B, один input/prompt, same model config. A — bare import в alpha16, B — bare import в alpha32 при тех же tensors/targets/rank; никакой иной разницы. Сначала сравнить projection residual: если xAᵀBᵀ≠0, B residual должен быть ровно 2× A с обычной погрешностью вычисления. Затем обычные logits/output, которые не обязаны сменить argmax. Отдельно descriptor-controlled import mismatch должен быть отклонён; это положительный контроль существующей защиты. Zero B negative control обязан скрыть разницу. Domains: ENRU и C# используют тот же neural API; эффект metadata не зависит от языка, предметная тяжесть различна. Нулевые training calls. Falsifier — сохранённая effective Alpha фактически входит в иной обязательный owner contract всех доступных callers либо реальный importer отвергает mismatch до функции; тогда остаётся только условная boundary документация.

**PrepareToGPL:** старый base-only checkpoint и внешний unattached LoRA path не являются эталоном переносимого full-LoRA lifecycle. Этот пункт предлагает сохранить семантику нынешнего корректного owner, а не восстановить старый codec.

## 5. Дополнение — Alpha0 допускается, weight hash может измениться без LoRA learning

[ValidateAndCanonicalize L707–718][lora-validation] требует finite Alpha, но принимает 0. [LoraConfig L10–17][lora-config] даёт scaling=Alpha/Rank; [Forward][lora-forward] умножает весь adapter output на scaling. При Alpha0 dA=dB=0; если graph создаёт ненулевые по наличию, но численно нулевые Grad arrays, AdamW всё равно применит decay к A, поскольку [L79][adamw] пропускает только Grad==null, а [L102–106][adamw] decays независимо от величины g. Поэтому altered A bytes не гарантируют altered function.

Это **не новый диагноз RP01**: там Alpha16; Oct18 mask report дополнительно подтверждает ненулевые конечные first moments всех16 tensors на каждом шаге, что исключает decay-only объяснение именно его live8. Не повышать приоритет на основании общего API. Если нужен отдельный компонентный опыт: A Alpha0, B Alpha16 из одного A≠0/B0=0; одинаковая пара, один update, frozen base, same decay. Логировать actual gradients, optimizer first moments, effective residual и logits, а не только weight digest. Отрицательный контроль Alpha0/decay0 проверяет природу изменившегося digest. Это два последовательных самостоятельных сравнения; не смешивать два фактора в одном A/B.

## 6. Что нельзя «восстанавливать из PrepareToGPL» вслепую

| Проверенный механизм | PrepareToGPL / DesignToCode | Current | Решение |
|---|---|---|---|
| DPO log-softmax chain | [Prepare L312–318][base-sign] использует (softmax−onehot)×dL/dlogP, то есть обратный нужному знак. DesignToCode сохраняет этот механизм. | [L1110–1120][current-sign] использует (onehot−softmax) и connected backward; [L1135–1150][objective] устойчивый exact objective. | Не переносить старый sign как успешный алгоритм. Успех другой Unit не аттестует этот code path. |
| RmsNorm backward | [Prepare L42–65][base-rms] возвращает new Tensor после ручных data loops, без связей graph. Аналогичный DesignToCode. | [L29–40][current-rms] использует TensorOps.MultiplyBroadcast. | Возврат старого слоя оборвёт gradient path; это контрдоказательство к blanket restoration. |
| LoraApplicator.Apply | [Prepare L11–44][base-apply] замораживает base и возвращает внешние adapters; comment прямо говорит, что base forward unchanged. | RP01 захватывает реально attached projections и keyed recipient ([Simulation L128–131][sim-recipient]). | Не считать одинаковое имя LoRA одинаковым обучаемым механизмом. Проверять historical caller конкретного успешного Unit. |
| BPE/EOS и masks | Ранние BPE train/encode похожи; старый EOS last-vocab token не доказывает наличие stop identity. | exact-v2 masks/segments и актуальный generation caller уже аудированы; этот срез не даёт нового regression обвинения. | Приоритет новым source-specific различиям, не повторам P05/P07/P08. |

## 7. Передача для итогового пакета

Основной новый результат — N-NEURAL-01 (реальный offline result). N-NEURAL-02 — отдельный first-moment-only microexperiment, уточняющий уже описанную в DLT-K1 гипотезу переноса optimizer state. N-NEURAL-03/Alpha0 оставить в разделе conditional boundaries с явной защитой RP01. Приоритет восстановления конкретного PrepareToGPL translation state из аудита root выше этих общих conditional API тем.

В итоговом опыте фиксировать кроме качества actual updates, examples/target tokens, milliseconds/update, full process RAM/VRAM если применимо, parameter counts, total/trainable model layers и отдельные Unit/Flow levels. У m50-only архитектура не меняется: 4 model blocks, q/v adapters в каждом; не объявлять16 trainable tensors «16 layers». Для offline recalc updates/calls=0 и native training cost отсутствует. CODEX Astra6 ultracode выбирает/оценивает DPO; внешний open Chinese model генерирует альтернативы только в отдельной реально разрешённой программе. Ни такой generator, ни отдельный evaluator в данном read-only анализе не вызывались.

## Ссылки на pinned sources

[enru-c0-score]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/training-artifacts/experiments/code-documentation-simulation/simulation-enru-C0-score16-20261016-01/evaluation-receipt.json#L2-L197
[enru-t-score]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/training-artifacts/experiments/code-documentation-simulation/simulation-enru-T-score16-20261016-01/evaluation-receipt.json#L2-L197
[enru-c0-free]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/training-artifacts/experiments/code-documentation-simulation/simulation-enru-C0-free4-20261016-01/evaluation-receipt.json#L14-L65
[enru-t-free]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/training-artifacts/experiments/code-documentation-simulation/simulation-enru-T-free4-20261016-01/evaluation-receipt.json#L14-L65
[cs-c0-score]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/training-artifacts/experiments/code-documentation-simulation/simulation-csharp-c0-score25-20261015-01/evaluation-receipt.json#L2-L295
[cs-t-score]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/training-artifacts/experiments/code-documentation-simulation/simulation-csharp-t-score25-20261015-01/evaluation-receipt.json#L2-L295
[score-owner]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoEvaluation.cs#L79-L88
[dpo-margin]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L891-L904
[sim-load]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L108-L112
[sim-restore]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L140-L171
[sim-recipient]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L128-L131
[adamw]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/04.TrainingLoop/Optimizers/AdamW.cs#L72-L106
[source-pins]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/SimulationEnRuContinuation.cs#L141-L155
[base-trainer]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/11.DPO/DpoTrainer.cs#L28-L35
[design-trainer]: https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/11.DPO/DpoTrainer.cs#L21-L28
[topology]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/FullCheckpointLoraTopology.cs#L15-L29
[writer]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/LlmModel.cs#L1449-L1473
[fingerprint]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/LlmModel.cs#L1900-L1916
[loader]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/LlmModel.cs#L1140-L1160
[lora-forward]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/10.LoRA/LoraLinear.cs#L115-L125
[lora-validation]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/10.LoRA/LoraApplicator.cs#L707-L718
[lora-config]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/10.LoRA/LoraConfig.cs#L10-L17
[descriptor-check]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/RepositoryOwnedFullLoraCheckpointDescriptor.cs#L34-L48
[descriptor-create]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/RepositoryOwnedFullLoraCheckpointDescriptor.cs#L102-L111
[base-sign]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/11.DPO/DpoTrainer.cs#L312-L318
[current-sign]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1110-L1120
[objective]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1135-L1150
[base-rms]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/03.TransformerModel/Layers/RmsNorm.cs#L42-L65
[current-rms]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/Layers/RmsNorm.cs#L29-L40
[base-apply]: https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/10.LoRA/LoraApplicator.cs#L11-L44
[oct-mask]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/dpo-mask-live8-01/report.md
[oct-embedding]: https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/embedding-sft8-01/report.md
