# 41. Текущий exact DPO: градиент до q/v-LoRA по исходникам

Дата: 2026-10-16. Фаза: **READ_ONLY_RESEARCH**. Исполнитель: `/root/rp01_code_trace_astra`, один исследователь Astra ultra, без вложенных работников.

В четырёх заданных участках **конкретная математическая ошибка не обнаружена**. Исходники поддерживают отдельные chosen/rejected графы с суммированием в общие листья, передачу производной через замороженные веса, правильную производную текущего RmsNorm и правильные транспонирования LoRA. Это содержательные контрдоказательства простой гипотезе «градиент обрывается или теряется между ответами». Это ещё не численная проверка составной производной exact DPO по адаптерам и не объяснение качества свободных ответов.

Открыт один конкретный стык: совпадает ли **градиент взвешенного exact DPO-loss по q/v A/B до optimizer** с независимой finite difference, когда одновременно работают несколько пар и оба ответа каждой пары. Изменение 16 тензоров подтверждает движение разрешённых параметров, но не правильность величины и направления этого градиента. Ни один новый опыт в этом пакете не выполнялся.

## Версия и фактический путь

Исследование начато на `d5c2409b24292d361593b8262b0c54997b15f539`; при заключительном чтении HEAD был `79082340f01b744cdf08185616f2bd818dbd06b8`. Адресный `git diff` между этими SHA и отдельно между HEAD и working tree по перечисленным ниже исходникам не показал изменений. Это проверка выбранных файлов, а не заявление о чистоте всего репозитория. Содержимое исходников не является аттестацией бинарника уже завершённого RP01-run.

В [CLI LoadModels, строки 315–332](C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/Program.cs:315) две копии загружаются из сохранённого B0. [Production loader, строки 534–558](C:/Projects/llm_mvp_core/26.TrainableRouter/Training/CodeDocumentationRepositoryOwnedModelCheckpointStartService.cs:534) создаёт новый `LlmModel`, устанавливает адаптеры и импортирует checkpoint. Это свежие объекты модели, а не передача старого forward-графа.

[Конфигурация, строки 7–41](C:/Projects/llm_mvp_core/config/code-documentation/generation-v1.json:7): vocabulary 629, context 2048, hidden 512, 4 слоя, 8 Q-heads / 2 KV-heads, FFN 1408, RMS epsilon `1e-5`; LoRA rank 8, alpha 16, dropout 0, targets `q_proj`, `v_proj`. SHA256 прочитанных байтов конфигурации — `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`, совпадает с константой [CLI, строка 15](C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/Program.cs:15).

[Simulation caller, строки 108–131](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:108) требует независимые policy/reference, запрещает ненулевой dropout, замораживает reference, материализует exact-пары и захватывает действительно установленные LoRA recipients. Затем создаёт `DpoTrainer.CreateForProjectLora` и вызывает `StepVector` на выбранном minibatch: [строки 160–186](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:160). Проверка recipient допускает gradients только у разрешённых A/B, запрещает alias с reference и проверяет замороженную базу: [DpoTrainer, строки 198–261](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:198).

Здесь «exact-v2» обозначает material `segmented-byte-BPE-v2` / framing `prompt-response-segments-no-separator-v2`. Версия objective отдельно называется `response-sum-softplus-signal-weighted-v3`: [DpoExactPairMaterial, строки 19–22](C:/Projects/llm_mvp_core/11.DPO/Models/DpoExactPairMaterial.cs:19). Это не legacy mean-response ветка.

Каждая exact-пара содержит отдельные ids формы **`[1,T]`**, маски только 0/1; validator отвергает первый размер, отличный от 1: [строки 160–181](C:/Projects/llm_mvp_core/11.DPO/Models/DpoExactPairMaterial.cs:160). Поэтому vector batch здесь — список пар, а не один тензор `[N,T]`. Чтение первой последовательности в `BackwardLogProbs` не является пропуском остальных batch rows в этом caller. Число токенов T зависит от материала; 2048 — предел, не утверждение о длине каждой пары.

## 1. Отдельные графы и накопление между парами

[StepVectorCore, строки 864–923](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:864) сначала нормализует веса пар, затем для каждой пары выполняет два reference forward и два отдельных policy forward. Reference используется как постоянный score. Каждый policy forward возвращает новый граф и новые logits: [строки 1002–1030](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1002).

Для пары i проверяемый exact objective имеет вид:

```text
w_i = max(0.01, weight_i) / sum_j max(0.01, weight_j)
c_i = w_i * VectorSignal(teachingVector_i)
m_i = beta * ((S_policy_chosen - S_ref_chosen)
              - (S_policy_rejected - S_ref_rejected))
L = sum_i c_i * softplus(-m_i)
S = сумма log p(response_token | предшествующие tokens)
```

`ObjectiveTerms` даёт производную softplus по margin; chosen получает `beta * c_i * dLoss/dMargin`, rejected — её противоположность: [строки 891–904](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:891), [1130–1140](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1130). `BackwardLogProbs` для exact использует upstream без деления на число targets; row i предсказывает token i+1, prompt/padding исключаются маской: [1036–1119](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1036). На этом участке нет дополнительного среднего, которое уменьшало бы градиент длинного ответа вопреки objective.

Далее существенна разница между временным графом и общими параметрами:

- [Tensor.Backward, строки 75–102](C:/Projects/llm_mvp_core/00.TensorCore/Core/Tensor.cs:75) обходит граф в обратном топологическом порядке; visited сравнивает объекты по identity. Начальный gradient **копируется** в output Grad. Вызов `logits.Backward(logits.Grad)` не прибавляет его ещё раз к самому output.
- [AccumulateGrad, строки 983–997](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:983) копирует первый вклад в leaf Grad и затем выполняет `+=`. Второй ответ и следующие пары добавляют производную к тем же A/B. Лист параметра не распространяет свой уже накопленный gradient обратно в другой forward-граф.
- `ApplyOptimizer` вызывается **после всего foreach**, затем `_policy.ZeroGrad()`: [строки 907–923](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:907). [Module.ZeroGrad, строки 60–63](C:/Projects/llm_mvp_core/00.TensorCore/Layers/Module.cs:60) обходит параметры; [Tensor.ZeroGrad, строки 56–61](C:/Projects/llm_mvp_core/00.TensorCore/Core/Tensor.cs:56) очищает существующий Grad. Обнуления между chosen/rejected или между парами нет — здесь оно уничтожило бы нужную сумму.

**Граница вывода.** `ExecuteWithPublication` не делает начальный `ZeroGrad`: [строки 1227–1244](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1227). Если внешний caller передаст policy с заранее ненулевыми trainable gradients, первый шаг включит их. Это свойство API, но не установленная причина RP01: указанный caller загружает свежие модели, конструктор Tensor оставляет Grad пустым, нормальные предыдущие шаги его очищают. Повторный backward по одному и тому же уже использованному графу тоже потребовал бы отдельного контроля intermediate gradients; фактический caller строит новые графы. Не следует переносить эти условные сценарии на наблюдавшийся run без свидетельства такого входа.

При ошибке до завершения публикации существует rollback: [DpoTrainer, строки 1238–1250](C:/Projects/llm_mvp_core/11.DPO/DpoTrainer.cs:1238). Snapshot сохраняет и восстанавливает также Grad, включая null: [DpoTrainerStepSnapshot, строки 154–187](C:/Projects/llm_mvp_core/11.DPO/DpoTrainerStepSnapshot.cs:154). Это контрдоказательство предположению, что любая неудачная попытка обязательно оставляет частичный gradient. Runtime-сценарий ошибки здесь не воспроизводился.

## 2. Замороженные lm_head и base не отсекают gradient к входу

Путь обучения идёт через блоки, final norm и lm_head: [LlmModel.Forward, строки 219–241](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:219). Head создан без bias: [строка 109](C:/Projects/llm_mvp_core/03.TransformerModel/LlmModel.cs:109). Q/K/V/O projections также без bias: [CausalSelfAttention, строки 62–65](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/CausalSelfAttention.cs:62).

[Linear.Forward, строки 45–60](C:/Projects/llm_mvp_core/00.TensorCore/Layers/Linear.cs:45) вычисляет `MatMul(input, Transpose(weight))`. В [MatMul, строки 644–698](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:644) результат требует gradient, если его требует **хотя бы один** operand. `gradA = gradOutput * B^T` вычисляется при `a.RequiresGrad`, независимо от `b.RequiresGrad`; для этого доступны замороженные `b.Data`. Запрет gradient к весам не означает запрета gradient через их линейное преобразование.

Для head `H[1,T,512] * W^T[512,629]` это даёт `dH = dLogits * W`. Замороженная база, расположенная после обучаемого адаптера, аналогично передаёт производную назад. Если вход первого адаптера сам заморожен, производные A/B всё равно вычисляются по его значениям. Поэтому «frozen lm_head делает q/v-LoRA необучаемой» противоречит проверенной реализации.

Это не утверждение, что случайный замороженный head является хорошим декодером или что q/v-only rank 8 достаточно для обучения языка. Такие ограничения выразительности и исходного знания остаются отдельными от связности autograd.

## 3. RmsNorm: проверка производной для `[B,T,D]`

Текущий [RmsNorm, строки 29–40](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/RmsNorm.cs:29) использует связанные TensorOps: `Pow(x,2) → Mean(axis:-1,keepDims:true) → AddScalar(eps) → RSqrt → MultiplyBroadcast(x,rms) → MultiplyBroadcast(normalized,gamma)`.

Для одной строки `(b,t)` обозначим `s = (sum_j x_j² / D + eps)^(-1/2)`, `y_j = gamma_j*x_j*s`, `g_j = dL/dy_j`. Тогда:

```text
dL/dx_j = g_j*gamma_j*s
           - x_j*s^3/D * sum_k(g_k*gamma_k*x_k)
dL/dgamma_j = sum_(b,t)(g_(b,t,j)*x_(b,t,j)*s_(b,t))
```

Чтение операций даёт именно эту формулу:

| Участок | Проверенное соответствие формуле |
|---|---|
| [Pow, строки 284–319](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:284), [AddScalar, 74–96](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:74) | `2*x` для квадрата; единичная производная добавления epsilon. |
| [Mean, строки 532–581](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:532), [broadcast backward, 1049–1060](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:1049) | Форма `[B,T,1]`, распределение upstream по D с множителем `1/D`. Проверена используемая ветка `keepDims:true`. |
| [RSqrt, строки 398–426](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:398) | `-0.5 * upstream * result³`; после Pow и Mean получается отрицательный второй член. |
| [MultiplyBroadcast, строки 145–208](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:145) | Индексы выравниваются справа; обратный вклад в broadcast operand суммируется через `+=`. Для `[B,T,1]` это сумма по D, для `[D]` — по B и T. |

У x есть прямой и нормирующий пути; топологический обход и additive accumulation складывают оба до обработки x. Frozen gamma отключает только `dGamma`, сохраняя первый operand и зависимость от gamma.Data. В RP01 D=512 и B=1; формула выше также описывает проверенную реализацию с общим B.

Полезное историческое изменение действительно существует: `git show 8a8cad4a23026b7f007f99f5a979ce4bfe21d547 -- 03.TransformerModel/Layers/RmsNorm.cs` показывает замену частных helpers, возвращавших `new Tensor(data, shape)` без backward, на `TensorOps.MultiplyBroadcast`. Коммит от 2026-07-28 называется `fix: connect LoRA adapters to live training path`. **Это исправление уже перенесено в текущий source**; майский head-only донор не доказывает нынешний обрыв в RmsNorm. В этом пакете проверен diff данного файла, а не весь исторический коммит как работающая сборка.

## 4. LoRA: формы, transpose и исполнитель MatMul

Реальный [LoraLinear.Forward, строки 112–125](C:/Projects/llm_mvp_core/10.LoRA/LoraLinear.cs:112) вычисляет:

```text
X [...,I], A [r,I], B [O,r], scale = alpha/r = 2
Z = X * A^T
DeltaY = scale * Z * B^T
Y = BaseLinear(X) + DeltaY
dB = scale * G^T * Z
dA = scale * (G*B)^T * X
dX_lora = scale * G * B * A
```

Суммы в dA/dB проходят по ведущим строкам X. [HeadDim](C:/Projects/llm_mvp_core/03.TransformerModel/Config/ModelConfig.cs:24) равен 512/8=64; из конфигурации и [конструкторов projections](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/CausalSelfAttention.cs:62) следуют q: A `[8,512]`, B `[512,8]`; v: A `[8,512]`, B `[128,8]`. Четыре слоя × два targets × A/B дают 16 разрешённых тензоров.

В [MatMul, строки 593–700](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:593) leading matrices входа обходятся последовательно; для двумерного правого operand одна матрица весов используется для всех. Backward по левому operand вычисляет `G*right^T`; по правому — `left^T*G`, суммируя строки и leading matrices. Это соответствует формам LoRA выше. Проверка не объявляет корректным произвольный broadcasting всех возможных MatMul shapes.

[Transpose, строки 707–783](C:/Projects/llm_mvp_core/00.TensorCore/Core/TensorOps.cs:707) меняет две оси, а backward транспонирует upstream обратно теми же осями. Для A/B это двумерный случай. Новые A-transpose/B-transpose узлы создаются в каждом forward, общими остаются только листья A/B. [Dropout, строки 54–57](C:/Projects/llm_mvp_core/00.TensorCore/Layers/Dropout.cs:54) при p=0 возвращает вход; случайный dropout не вводит различий chosen/rejected в данном RP01 path.

Исполнитель здесь установлен по вызовам, а не по названию режима запуска. TensorOps напрямую вызывает **managed `SimdOps`**: [MatVecRowCore, строки 99–146](C:/Projects/llm_mvp_core/00.TensorCore/Core/SimdOps.cs:99) использует `float[]`, `System.Numerics.Vector<float>`, `Vector.Dot` и скалярный хвост. [Transpose2DCore, строки 241–270](C:/Projects/llm_mvp_core/00.TensorCore/Core/SimdOps.cs:241) записывает `dst[col*rows+row] = src[row*cols+col]`. В этой цепочке нет переключения на отдельный C++/BLAS kernel; JIT может исполнять .NET SIMD аппаратными инструкциями. Комментарий caller о «native execution» сам по себе не свидетельствует о другом математическом backend.

Начальная B=0 при ненулевой A означает **нулевой dA на первом backward и потенциально ненулевой dB**. После изменения B появляется вклад в dA. Это следует из формулы и соответствует [инициализации A/B, строки 92–101](C:/Projects/llm_mvp_core/10.LoRA/LoraLinear.cs:92); такой первый шаг не является доказательством мёртвой A. Для проверки A нужна отдельно зафиксированная ненулевая B.

## Что уже предусмотрено тестовыми исходниками — без запуска

| Прочитанный тест | Какое контрдоказательство даёт его конструкция | Чего он не закрывает |
|---|---|---|
| [ResponseSumAndBackward…, строки 173–202](C:/Projects/llm_mvp_core/Tests/DpoExactPairMaterialTests.cs:173) | Независимый score и центральная разность по всем toy logits; 1/2 response targets; исключение prompt/padding; отличие суммы от среднего. | Производную по q/v A/B и суммирование отдельных пар. |
| [MatMulBroadcastBackward…, строки 399–458](C:/Projects/llm_mvp_core/Tests/LoraTests.cs:399) | Независимые loop-reference gradients для A `[2,3,2]` и общей B `[2,2]`, finite difference по B. | Составной DPO-loss и реальные RP01 размеры. |
| [RmsNorm_BackwardMatchesReference…, строки 795–875](C:/Projects/llm_mvp_core/Tests/LoraTests.cs:795) | Независимая формула и finite difference input/gamma, B=2,T=3,D=4, неоднородная gamma, случай x=0. | Полный pipeline и качество свободного ответа. |
| [LoraAttention_AllProjectionBGradients…, строки 878–927](C:/Projects/llm_mvp_core/Tests/LoraTests.cs:878) | Каждый q/k/v/o отдельно, logits → final norm → adapter, конечная разность B для weighted-logits objective. | В каждом случае rank 1, один target, B=0; сравнивается координата максимального численного B-gradient. Нет A-gradient, совместных q/v, exact DPO или paired/minibatch accumulation. |

Наличие этих исходников полезно: локальные производные и прежний разрыв не забыты. **В текущем исследовании тесты не запускались; результат PASS на текущем HEAD не заявляется.** Их смысл также нельзя расширить до численной проверки всего RP01 только по названиям тестов.

## Минимальная будущая различающая проверка — НЕ ВЫПОЛНЕНА

Цель — закрыть один составной numerical link, а не ещё раз проверить факт изменения checkpoint. Только после отдельного разрешения на исполнение можно поручить следующую ограниченную работу более дешёвому исполнителю.

1. Зафиксировать текущий source/config, независимую frozen reference, dropout 0, exact material и две короткие допущенные пары с разной длиной response и padding. Сохранить actual RP01 topology/q-v recipients; не менять корпус и не оценивать обучение языка по этим двум парам. Задать веса/vector signals заранее и использовать ровно objective `L` выше.
2. До optimizer получить аналитический gradient двух chosen/rejected графов каждой пары. При изолированном измерении сохранить коэффициенты c_i, рассчитанные для полного набора: отдельный вызов StepVector с одной парой перенормирует вес и не даст нужного сравнения. Сравнить сумму отдельно измеренных вкладов с общим minibatch gradient и с обратным порядком пар. Сравнение выполнять при одинаковых исходных весах и явной нулевой стартовой Grad; оно различает потерю, удвоение и перенос вклада между графами.
3. На заранее выбранных координатах q/v A/B первого и последнего слоя сравнить analytic gradient с центральной разностью **полного скалярного L**, а не с изменением веса после AdamW. Reference и остальные параметры неизменны. Отдельные состояния B=0 и малая фиксированная B≠0 различают ожидаемый нулевой dA в старте и действительно неверную производную A.
4. До получения результата зафиксировать h, абсолютный/относительный допуск и повтор с h/2 для устойчивости float32. Координата с численно неразличимой производной не подтверждает правильность всей матрицы. Не подбирать только удачную координату после вычисления. Отдельно прочитать границу Grad после успешного шага, если исполнение шага входит в будущую авторизацию; для проверки производной optimizer вообще не требуется.

Расхождение этой проверки локализовало бы ошибку композиции, даже если 16 тензоров продолжают меняться. Совпадение дало бы более сильное контрдоказательство данной гипотезе, но не доказало бы обученность случайной базы, достаточность q/v-only LoRA, качество данных или связность free generation.

## Остаточная граница и следующий точный источник

В пределах четырёх заданных участков незамкнутый внешний native kernel не найден: владелец рассмотренных MatMul/Transpose установлен до managed реализации. Численные результаты их композиции не получены. Остальная attention-цепочка не превращена в тотальный аудит: например, вызовы `RotaryEmbedding.Apply` и `RepeatInterleave` находятся в [CausalSelfAttention, строки 149–158](C:/Projects/llm_mvp_core/03.TransformerModel/Layers/CausalSelfAttention.cs:149). Их полная backward-математика в этом пакете не проверялась; это предел вывода, а не найденный дефект.

Если потребуется следующий **source-only** пакет именно по gradient-chain, точный первичный вход — вызов `RotaryEmbedding.Apply` на q/k в указанном `CausalSelfAttention` и его реализация `03.TransformerModel/Layers/RotaryEmbedding.cs`. Это предложение следующей границы; файл не расширял текущий аудит. Исторический learned donor остаётся отдельным открытым вопросом отчёта 39 и здесь новыми архивами не дополнялся.

Coverage: адресно прочитаны названные caller, exact material, Tensor/операции, LoRA, relevant loader/config, rollback-часть и четыре тестовых фрагмента; проверен один исторический RmsNorm diff. Canonical AGENTS/CLAUDE и семь запрошенных docs/skills использовались по ранее прочитанным и дополнительным адресным выдержкам. Полная загрузка тысяч строк, полный аудит ядра, независимый review другим агентом и runtime-проверка не заявляются. Создан только этот Markdown; product/code/data/weights/runtime этим исследованием не изменялись. Git-команды использовались только для чтения. Все будущие действия выше остаются дизайном, а не выполненными результатами.
