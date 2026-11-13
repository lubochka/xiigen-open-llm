# DPO, LoRA, токены и начальное состояние: сравнение PrepareToGPL с текущим кодом

Дата проверки: 18 октября 2026. Эта записка — результат чтения кода и одной отдельно обозначенной численной проверки редукции FP32. Нативное обучение, предметные A/B и оценка внешних DPO в этой работе не запускались.

## 1. Что действительно сопоставлено

- База пользователя: `PrepareToGPL`, commit [31346d8928be4469b01aaf313780e8f950ff91e2](https://github.com/lubochka/llm_mvp_core/tree/31346d8928be4469b01aaf313780e8f950ff91e2).
- Текущий исходник: `codex/dpo-implementation`, commit [a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6](https://github.com/lubochka/llm_mvp_core/tree/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6).
- Граница: `11.DPO`, `10.LoRA`, `01.Tokenizer`, `03.TransformerModel`, используемые TensorCore/AdamW, сборка модели в `18.CapabilityTraining`, текущая композиция C#→ENRU.
- Прочитаны `generated-code-review-SKILL.md` и `planning-dpo-authoring-SKILL.md`. Применён code-first подход: реализация → вызывающий путь → наблюдаемая граница → эксперимент. Отчёт не является разрешением менять core.
- Сопоставлены оба существующих каталога, включая 367 документов planning, верхний core-каталог и карточки P01/P10/P12/P105/P119, а также результаты P11 и `dpo-mask-live8-01` от 18 октября. Поиск новизны был адресным по механизмам; число просмотренных файлов не означает чтение каждого их абзаца.

**Выбор в роли человека выполняет CODEX; оценку качества chosen/rejected также выполняет CODEX, в обоих случаях с указанной пользователем моделью `astra 6 ultracode`. Внешняя открытая китайская модель создаёт DPO.** Это требуемое разделение ролей, а не утверждение, что такой генератор или оценщик вызывался в данном аудите.

**Перед публикацией перечитаны N40–N42 на planning a4a3e709.** DLT-1 и DLT-2 сохраняют статус новых конкретных механизмов в просмотренном корпусе. DLT-3 классифицирован консервативно как новое уточнение P01/P10, без увеличения числа самостоятельных дефектов. Старое/новое подключение LoRA уже также разобрано в [N41 LoRA parity](41-LORA-BRANCH-PARITY-AND-REAL-NEURAL-QUALITY-BOUNDARY-2026-10-18.md); таблица ниже является подтверждением, а не новой находкой.

## 2. В начало очереди восстановления поведения PrepareToGPL

В моей области самый конкретный новый кандидат на нарушение сравнимости с PrepareToGPL — **DLT-1: изменившийся порядок расходования общего RNG при холодном восстановлении моделей**. Он меняет начальные веса следующего нового семейства, даже когда конфигурация и номинальный seed выглядят одинаково.

При проверке успешного механизма ветки сначала необходимо сохранить **реальный получатель обучения**: конкретный Unit/KNN/матрицу или Transformer. Простой откат текущего DpoTrainer/LoRA к PrepareToGPL не воспроизводит доказанную компетенцию ветки. В старой `LoraApplicator.Apply` адаптеры создаются и возвращаются, но комментарий и реализация прямо оставляют forward базовой модели прежним. В текущем коде LoRA уже встроена в attention и регистрацию параметров. См. таблицу ниже. Успешный обученный Unit другой ветки нельзя автоматически объявлять успешным именно этим старым нейронным трактом.

### Изменения на уровне деталей модели, которые уже есть в current

| Деталь | PrepareToGPL | Current a392b6b | Что из этого следует |
|---|---|---|---|
| Включение LoRA в модель | `Apply` замораживает base, возвращает отдельные adapters; base forward не заменяется | Установленные q/k/v/o adapters вызываются в attention через `ForwardProjection`, регистрируются в слотах модели | Старое отсутствие подключения адаптера не является текущим дефектом. Без дополнительного старого caller нельзя приписать историческую полезность обновлениям этих адаптеров |
| Знак производной log-prob | `(sm - oh) * scale` при upstream от log-prob | `(oh - sm) * targetScale` | Ошибка знака в старом файле не основание ломать правильный текущий знак |
| Граф forward/backward | Для backward вызывается ещё один forward | Возвращается `PolicyLogProbGraph`, backward использует его `Logits` | Повторное стохастическое вычисление не является установленной текущей причиной |
| Exact objective и маски | Среднее по full-sequence mask | Для exact-v2 response-only mask и сумма target log-prob; stable softplus | Маска prompt и нестабильный sigmoid-tail уже не актуальное описание exact-v2 |
| Fractional mask | Счётчик ненулевых mask вместо суммы; mask не входит в каждый gradient | Сумма mask и множитель mask для каждой позиции | Результат 18 октября уже отражён в exact current исходнике; это не новая находка |
| Embedding и bias gradient | В старых листовых forward есть разрывы графа | Current связывает Embedding scatter-add и expanded bias reduce-sum с параметрами | P11/P86 и текущие изменения надо сохранять; их наличие отдельно от предметного learning gain |
| Маленький generic capability config | vocab512, context64, dim32, 4 слоя; rank2/alpha4/q,v/dropout0 | Те же величины | Само это сочетание гиперпараметров — не новая регрессия относительно PrepareToGPL |

Прямые исходники: [старый LoraApplicator L11–48](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/10.LoRA/LoraApplicator.cs#L11-L48), [текущий attention L129–170](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/Layers/CausalSelfAttention.cs#L129-L170), [слоты модели L520–543](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/LlmModel.cs#L520-L543), [старый DPO L276–322](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/11.DPO/DpoTrainer.cs#L276-L322), [текущий DPO L1002–1150](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1002-L1150), [current Embedding L59–78](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/00.TensorCore/Layers/Embedding.cs#L59-L78), [current Linear L83–97](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/00.TensorCore/Layers/Linear.cs#L83-L97), [старый capability config L846–895](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L846-L895), [current config L4047–4087](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L4047-L4087).

## 3. DLT-1 — Восстановление чужого домена меняет будущую начальную модель

**Приоритет:** первым в группе восстановления численной сравнимости с PrepareToGPL.  
**Статус:** изменение цепи и общий RNG доказаны исходниками; изменение предметного качества не измерено.  
**Новизна:** новый конкретный эффект warm-cache→cold-construction в capability path. Общее существование `Random(42)` уже замечено в core/02 на другом старом V191 runner; HNSW shared RNG уже известен N33/P22. Эти темы не выдаются за новые.

### Что было

В PrepareToGPL `DefaultLogicalCapabilityTrainer` создаёт `_referenceModel` один раз при создании объекта ([L36](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L30-L39)) и повторно использует её ([L204–208](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L204-L208)). При совпадении активного parent с cache `BuildTeachingStack` возвращает `cached.PolicyModel`, без создания новой модели ([L492–506](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L492-L506)).

### Что стало

Current для parent создаёт новый `LlmModel(parentProvenance.ParentConfig)` ([L2359–2372](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L2359-L2372)). После построения каждого успешного stack создаёт ещё один `referenceModel = new LlmModel(...)` ([L639–648](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L639-L648)). Загрузка parent weights происходит **позже**, после создания объектов и адаптеров: [L1240–1269](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1240-L1269).

Обычные конструкторы Linear/Embedding выбирают числа из `Initializers`. В current сохранён статический `Random(42)`; если `ScopedRng` отсутствует, Kaiming/Normal берут следующий участок общего потока ([L18–55](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/00.TensorCore/Init/Initializers.cs#L18-L55), [L101–115](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/00.TensorCore/Init/Initializers.cs#L101-L115)). Unseeded LoRA-A тоже расходует этот поток ([L53–59](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/10.LoRA/LoraLinear.cs#L53-L59)).

### Почему это может ухудшить обучение

Загрузка существующей модели затем перезаписывает её случайные веса; однако уже израсходованные случайные числа не возвращаются. Следующий **новый** domain получает другие embedding, attention и LM-head при тех же параметрах config и том же надписанном seed42. Если для этого домена используются случайная frozen base и KNN на её embeddings, меняется геометрия и начальная обучаемость. Таким образом, равные данные и гиперпараметры двух веток ещё не обеспечивают одинаковую исходную функцию.

Возможен эффект уже в первом новом семействе: old создаёт общую reference перед policy, current — policy перед новой reference. Дополнительные cold reload усиливают различие. Это **не утверждение о порче восстановленного checkpoint**: после полного load его веса могут совпадать точно.

Проверен доступный обычный вход: [Assistant.RunWithCapabilityGenerationAsync L954–995](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs#L954-L995) → [AdaptiveLogicalOrchestrator L480–510](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs#L480-L510) → DefaultTrainer. В этих прочитанных файлах и DefaultTrainer собственного `UseDeterministicSeed` нет. **Внешний caller может установить AsyncLocal seed scope**; тогда этот конкретный global-stream механизм требует отдельной проверки и не считается автоматически активным. Сам scope API существует [L26–30](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/00.TensorCore/Init/Initializers.cs#L26-L30).

### Домены

Новые generic capability families, для которых действующий путь создаёт tiny model: классификация запросов, планирование, выбор навыка, предметная память на Transformer embeddings. Риск зависит от числа ранее созданных/восстановленных моделей в том же процессе. **Прямой RP01 simulation загружает сохранённый B0/C50, поэтому DLT-1 не объявляется объяснением его уже измеренных нулевых ответов.**

### Маленький эксперимент

1. **Ноль optimizer updates:** в двух свежих процессах одинаково подготовить окружение. A создаёт целевую новую capability model; B предварительно выполняет ровно те дополнительные конструкторы, которые использует cold reload, затем создаёт ту же целевую модель. Записать named base hashes, LoRA-A hashes, реально активный seed scope и logits/embeddings до обучения. Данные, конфигурация, порядок target examples одинаковы. Это проверка зависимости начального состояния, не learning gain.
2. **Контроль механизма:** повторить A/B с локальным независимым scope вокруг создания именно целевой модели и её адаптеров. Использовать существующий API seed scope; seed, namespace и version зафиксировать. Не сбрасывать глобальный RNG процесса. Если целевые hashes всё ещё отличаются, этот механизм не изолирован.
3. **Проверка качества:** 8–16 допущенных уроков одного нового домена и 12 закрытых перефразированных/отрицательных запросов через тот же ordinary Unit. Взять **ровно два полных начальных состояния, сохранённых в диагностическом A/B шага 1**, и продолжить каждое на одинаковых данных с одинаковым дальнейшим stochastic schedule. Основной фактор здесь — полное состояние, изменившееся из-за порядка инициализации: оно может различаться и в base, и в LoRA-A. Не подменять один вариант произвольным saved checkpoint и не приписывать суммарный эффект только base. Если нужно выделить влияние именно base, отдельный второй опыт удерживает одинаковые начальные LoRA-A/B при двух наблюдавшихся base states. Считать correct, critical errors, abstain, query→neighbors, число обновлений и время. Различие hashes без различия ответов подтверждает только численную несравнимость.
4. **Восстановление PrepareToGPL:** для donor-first сравнения взять именно сохранённое исходное состояние успешного пути или воспроизводимо восстановить его исходные tensors. `new LlmModel(sameConfig)` не заменяет это доказательство. Новый scoped seed обеспечивает воспроизводимость будущих запусков, но сам по себе не восстанавливает старые полезные веса.

## 4. DLT-2 — Суммирование FP32 теряет малый DPO-контраст до stable softplus

**Приоритет:** P2, диагностика численного сигнала; после восстановления правильного получателя/состояния.  
**Статус:** код редукции доказан; выполнен только численный proxy этой редукции; нативная incidence и предметный эффект неизвестны.  
**Новизна:** конкретная локализация потери точности **перед** stable loss, отличная от известной отмены масштаба в AdamW (N26), clipping overflow (P12) и общей finite-difference композиции (P10).

### Было и стало

И старый, и current DpoTrainer накапливают `sum` и `logSumExp` как float. Current exact-v2 возвращает сумму target log-prob, затем отдельно вычитает policy/reference totals и chosen/rejected rewards ([L1036–1068](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1036-L1068), [L891–896](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L891-L896)). Старый path суммирует таким же float и потом делит на count ([L239–273](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/11.DPO/DpoTrainer.cs#L239-L273)). Поэтому это **унаследованный риск, не доказанная новая регрессия**, и возврат к старому mean не является его исправлением.

Текущий `ExactObjective` устойчиво считает softplus и его хвост ([L1135–1150](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1135-L1150)). Если маленькая разность уже округлилась в ноль в суммах, эта устойчивость её не вернёт.

### Конкретный численный результат, выполненный в этом аудите

Численная проверка повторяла sequential FP32 addition через Python `struct.pack/unpack('f')`; нативный TensorCore не запускался. Обе последовательности содержат N представимых float log-prob `−6`; в policy последний элемент увеличен на представимое `0.000010013580322265625`. Rejected side оставлена одинаковой. Воспроизводимый скрипт: `audit/dpo_fp32_proxy.py`; сохранённый результат: `audit/dpo_fp32_proxy_result.json`, статус `SOURCE_NUMERIC_PROXY_ONLY`. Команда: `python audit/dpo_fp32_proxy.py > audit/dpo_fp32_proxy_result.json`.

| N target-токенов | Разность FP32 сумм | Разность double сумм тех же float входов |
|---:|---:|---:|
| 8 | 0.000011444091796875 | 0.000010013580322265625 |
| 32 | 0.0000152587890625 | 0.000010013580322265625 |
| 64 | 0 | 0.000010013580322265625 |
| 256 | 0 | 0.000010013580322265625 |

При N64 current-style margin с beta0.1 становится 0 вместо приблизительно 0.00000100136. Это доказательство возможности потери представимого сигнала при такой редукции, **не измеренные logits RP01**.

### Почему важно и где предел

Могут квантоваться очень небольшие улучшения, pair ordering и finite-difference оценка. Однако при таком маленьком margin ошибка производной мала. Один этот пример не объясняет отсутствие общей языковой компетенции и не доказывает значимый quality loss. В generated capability существует пороговый stop, но RP01 finite loop его не вызывает; связывать эту гипотезу с ранней остановкой RP01 нельзя.

Уязвимее длинные ответы и фрагментированные Unicode targets, а также tiny early deltas и почти совпадающие chosen/rejected. Для короткого scalar Unit ожидаемый риск меньше.

### Маленький эксперимент

**Сначала 0 updates:** снять реальные policy/reference logits для 8 длинных и 8 коротких новых пар и вычислить один сохранённый набор **float per-token log-prob** текущим кодом. A накапливает эти значения в FP32, B1 — в double. Это изолирует именно редукцию totals, показанную proxy; log-sum-exp, tokenizer, masks, веса и sampling одинаковы. Double значения B1 должны сохраняться через промежуточные carriers до вычитания policy/reference и chosen/rejected, с единственным приведением итогового margin к float. Возврат double суммы через прежний `float` output или свойство снова теряет проверяемый сигнал: соответствующие текущие границы — [BatchLogProbs L1007](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1007) и [PolicyLogProbGraph L1157](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1157). Zero-step diagnostic может хранить эти totals отдельно от trainer, без изменения его контрактов. Записать response token counts, per-token differences, float/double margin, sign flips, zero-vs-nonzero и разность objective derivative.

**B2 — самостоятельная проверка:** если нужно исследовать точность log-sum-exp, сравнить B1 с double log-sum-exp при одинаковой double редукции totals на тех же float logits. Этот второй фактор не смешивать с первым A/B; настоящий proxy не проверяет ошибку log-sum-exp.

Включить exact common-prefix пару, пару с различием в начале и пару с различием в конце. Если реальный error меньше заранее выбранного допуска и не меняет direction/decision, закрыть DLT-2 как объяснение данного запуска.

Только при существенном наблюдаемом отличии — 4–8 одинаковых разрешённых обновлений в двух независимых состояниях с одним изменением precision выбранной редукции, сохраняя её через промежуточные carriers до margin; потом новые закрытые C#/ENRU ответы. Для такого native продолжения изменение carrier precision должно быть явной частью одного вмешательства, а не скрытой потерей double при возврате из функции. Если изменились только последние десятичные цифры, результат `NUMERIC_DIFFERENCE_ONLY`, а не полезное обучение. Zero-step double diagnostic не требует переписывать обучение на другой framework.

## 5. DLT-3 — Для атомарного урока q-LoRA может быть математически бездействующей

**Приоритет:** P2, только для точного атомарного профиля; не общая гипотеза о текущем RP01.  
**Статус:** математическое следствие прочитанной causal attention, не измеренный native gradient.  
**Новизна:** конкретная область нулевой достижимости q/K, уточняющая широкие P01/P10. Это не новое доказательство неверного backprop.

### Область, в которой утверждение верно

Exact material допускает prompt из одного token и response из одного token: [ограничения длины L120–124](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/Models/DpoExactPairMaterial.cs#L120-L124). Framing не добавляет separator/BOS: [версия L19–22](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/Models/DpoExactPairMaterial.cs#L19-L22), [раздельное кодирование L64–78](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/Models/DpoExactPairMaterial.cs#L64-L78), [сборка последовательности L171–176](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/Models/DpoExactPairMaterial.cs#L171-L176). Тогда единственный обучаемый target предсказывается из позиции0. В causal attention эта позиция видит только один key; attention weights равны [1,0,…] независимо от Q/K. Производная softmax по score нулевая. Формула именно так реализована: [attention L271–291](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/03.TransformerModel/Layers/CausalSelfAttention.cs#L271-L291), [Softmax backward L852–877](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/00.TensorCore/Core/TensorOps.cs#L852-L877), [DPO next-token shift L1044–1049](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/11.DPO/DpoTrainer.cs#L1044-L1049).

При таком уроке q-LoRA не получает содержательного gradient независимо от числа query heads. V-путь может учиться. На первом шаге при B=0 **A-производная любого LoRA тоже равна нулю** — это нормальная инициализация, не поломка; B-производная является корректной первоначальной проверкой. AdamW weight decay способен изменить A даже при нулевом gradient, поэтому одного diff weights недостаточно.

### Что сопоставлять с PrepareToGPL

Математика одноэлементного attention не введена RP01. Отличие применимости возникает, когда успешный атомарный learned Unit/матрицу заменяют единым q/v-LoRA recipient. Для ранее обученного KNN по словам отсутствие обновления q вообще не было критерием. Нельзя из этой заметки вывести, что именно такой перенос уже произошёл в конкретном успешном donor: caller требуется подтвердить отдельно.

**Не применимо автоматически:** если настоящий materializer добавляет instruction/BOS, source кодируется несколькими BPE tokens либо response содержит больше одного target, граница n=1 не выполнена. В рассмотренных C#/ENRU500 prompt длиннее одного token; DLT-3 не предъявляется как объяснение их результата.

### Домены и маленький эксперимент

Атомарные категории/метки, короткие коды действий, числа, операции token→token — только после проверки настоящих IDs. Для предложений и AST зависимостей нужна обычная многотокенная проверка.

1. Создать 4 новые законные пары, у которых фактические `PromptLength=1`, `ChosenLength=RejectedLength=2`; использовать существующий exact materializer. Не подменять длинные реальные примеры ручными ID и не называть это их production path.
2. Один native forward/backward без optimizer: записать по каждому q/v A/B `Grad is null`, norm, maxabs; fixed dropout0. Для q ожидается нулевой gradient; для v.B подобрать заранее невыраженную в zero input допустимую пару и проверить ненулевой контроль. Finite difference q-coordinate должен дать ноль.
3. Положительный контроль — тот же тип урока с двухтокенным контекстом, в котором второй token меняет правильную метку; q-gradient может стать ненулевым. Отсутствие q-gradient и в нём требует другой локализации.
4. Если атомарные примеры действительно существенны в домене, отдельный quality A/B: текущие q/v targets против v-only с **тем же v-rank и теми же initial v tensors**. Это уменьшение числа параметров, не равный parameter budget; оно проверяет, нужны ли q именно этому профилю. Увеличение v-rank или добавление head — следующее самостоятельное сравнение, не второй скрытый фактор. Считать предметные ответы, parameter bytes и время; не обещать, что одно отключение q улучшит качество.

## 6. Полезные уточнения известных тем — не увеличивают число новых находок

### DLT-K1. Перенос AdamW moments между C# и ENRU надо проверять отдельно от правильного восстановления

В текущем simulation ENRU policy/reference загружаются из C50, а moments/step50 продолжаются: [Run L160–171](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L160-L171). AdamW считает `m=.9*m+.1*g`, `v=.95*v+.05*g²`: [L72–106](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/04.TrainingLoop/Optimizers/AdamW.cs#L72-L106). При конфликтующем градиенте первые ENRU updates могут продолжать старое направление. Это ожидаемое свойство переноса optimizer state, а не ошибка loader.

Тема moments/retention уже есть в `06-CHECKPOINT-RESTORE-ADOPTION.md`. Новое полезное уточнение эксперимента: до step измерить cosine(new-domain gradient, carried first moment), fraction координат с противоположным знаком и predicted update; сравнивать перенос moments и новый optimizer как явно разные **стратегии нового домена**. Нельзя обнулять moments и называть результат точным continuation C50; runtime proof намеренно требует прежнее состояние. A/B должен иметь собственное разрешённое research continuation, одно frozen reference C50, одни веса/данные, закрытую C# retention панель после достижения ненулевой C# компетенции.

### DLT-K2. Legacy [B>1,T] нельзя считать автоматическим neural minibatch

Current exact validator требует [1,T], и RP01 подаёт список таких пар. Цикл DpoTrainer читает только T-позиции первой строки; legacy carrier сам по себе не содержит такого shape-отказа. Это условная граница публичного legacy API, уже оговорённая core/41; не причина пропуска RP01 rows. Сравнение двух batch1 с одним tensor batch2 имеет смысл только после доказательства поддерживаемого legacy caller. Повторно объявлять current RP01 batch implementation сломанной нельзя.

## 7. Что нужно проверяющему до интеграции

- Для DLT-1 проверить полный внешний caller seed scope и фактический момент расходования RNG; отсутствие внутреннего scope в прочитанных трёх слоях не доказывает отсутствие scope в любом host.
- Не смешивать исправление reference independence с изменением base identity: current копирование reference может быть правильным, а дополнительные конструкторы всё равно меняют следующий cold start.
- Для DLT-2 прочитать именно float totals до stable objective. Proxy отделить от native run и от предметного gain. Возврат к mean или изменение beta не является чистым precision intervention.
- Для DLT-3 получить token counts настоящего доменного caller. Нулевой q-gradient в одноэлементном attention — ожидаемая математика; дефектом может быть только необоснованное ожидание, что этот recipient обучает требуемое решение.
- В готовом пользовательском перечне новых механизмов считать **DLT-1 и DLT-2**; DLT-3 — новое уточнение области применимости известных P01/P10, а K1/K2 — crosswalk к существующим темам. Не наращивать число новых гипотез переименованием старых.
- Продуктовый код, core-каталог возможных различий и git refs этим агентом не изменены. Локальные исходники для независимой сверки находятся под `audit/dpo_src`; профильные capability исходники — в общем `audit/units_src`. Единственный смысловой deliverable агента — эта записка.

