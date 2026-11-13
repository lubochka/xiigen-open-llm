# 03 — Нейронное обучение: текущий тракт и малый парный caller

**Дата:** 20.10.2026. **Core:** `bf9b543e149ef197da85a767821d98c9b67916d2`. **Planning:** `1ad622fe1f6d1e8b0860265d8c208aeada3fefb0`. **Режим:** чтение исходников, сопоставление существующих находок и проект реализации. Native .NET, обучение, inference, численный proxy, CI и Git mutations в этом проходе **не выполнялись**. Защищённая папка `docs/plans/replanning-possible-diffs` не менялась.

**Граница текущего поручения:** подготовка этого доказательного документа разрешена пользователем и выполнена. Исторические отметки APPROVED/PENDING относятся к отдельным версиям прежних исполнительских паспортов; они не блокируют нынешнее чтение и подготовку документов и не подменяют актуальные указания пользователя. Core implementation и native runs в этот документный проход не входят. Ниже описан будущий implementation scope и условный промпт, а не начатое исполнение.

**Свежесть:** при финальной сверке core HEAD уже `de4ef0f14172dada4daf142a4c66ba19e96bb6d4`: после исследованного `bf9b543` добавлены два коммита документации. По сравнению этих версий C#-исходники не менялись; ссылки T01–T26 на зафиксированный код сохраняют применимость к алгоритму. Обновлённая D04 прямо снимает общую блокировку и повторное личное согласование каждого ID/SHA: B1–B6 — шесть задач подготовки исполнителя, а не требование получить ответы пользователя. Она по-прежнему сообщает, что micro-paired команды пока нет. Обновлённая active23 сохраняет точные PAD1/PAD2 и бюджет четырёх updates без Generate с необязательным расширением. Ссылки P01–P04 оставлены для исторического среза; актуальные утверждения здесь сверены с live-документами, а не перенесены из старых отметок PENDING. [F01][F02][F03][F04]

## 1. Вывод для очереди работ

Первый конкретный нейронный шаг — реализовать ограниченный **парный учебный caller** для уже описанного **ND-DPO-PAD / E-DPO-VALID-LENGTH**: два настоящих DPO-урока на плечо, тот же Student, те же данные и начальное состояние; единственный фактор — вычислять каждую сторону пары до её действительной длины. Основная проверяемая польза — время/память настоящего обучения. Это **уточнение существующей карточки**, не новый алгоритмический механизм и не установленная причина ухудшения RP01. Protected-карточка содержит материалы и честные счётчики; live-версия по-прежнему обозначает tiny exact owner, внутренний execution view и observer как ещё не реализованную связь. [P01][F04]

Выявлена конкретная дополнительная деталь его подключения: **два разных RunId не позволяют выполнить A и B на одном TRAIN hash через нынешний Run**. `issuedPath` строится из domain, Mode и InputSha256, без RunId; второй такой вызов откажет до загрузки модели. Для законного A/B нужен один заранее определённый paired-study owner с двумя изолированными arms и раздельными долговечными записями об их эффекте. Нельзя удалять marker, сбрасывать историю или выдавать второй RunId за разрешение повторить неизвестный эффект. Это **пробел готовности учебного эксперимента**, а не дефект математического обучения. [T01]

По оси прямого DPO/LoRA **новая причина деградации сверх existing findings в этом проходе не установлена**. FP32 cancellation, padding, EOS, порядок SFT/DPO, SFT mask/state, sparse AdamW age и warmup уже имеют следы в existing findings/protected-документах. Повторное описание не делает их новыми. Практический вклад здесь — current-source reconciliation, точные границы применимости и законченный проект отсутствующего caller.

## 2. Что уже исправлено или защищено

| Старое подозрение | Текущее доказательство | Решение |
|---|---|---|
| Embedding возвращает detached tensor | `Embedding.Forward` передаёт RequiresGrad, добавляет weight в граф и делает scatter-add градиента, включая повторённые token IDs. [T02] | **Не заказывать ремонт заново.** Protected active map это уже отмечает. Для frozen-base RP01 base embedding вообще не является обучаемым получателем. [P02] |
| Linear bias не получает сумму градиентов | `ExpandBias` сейчас связывает bias и суммирует gradient по повторениям. [T03] | **Не заказывать ремонт заново.** Это не доказанная причина q/v-LoRA RP01, чей LlmModel создаёт основные Linear без bias. [T04] |
| Fractional mask расходится между scalar loss и gradient | Current backward суммирует mask, затем умножает каждый token gradient на maskVal; exact RP01 использует бинарные response masks. [T05] | **Исторический дефект уже исправлен.** Protected active07 и map это признают. [P03] |
| Любой DPO обрезает prompt/response до 480 символов | Default `LogicalDpoTrainingRequest.MaterialVersion` действительно legacy; `BuildBatch` делает Compact только в legacy-ветви. Exact-ветвь возвращает `DpoExactPairMaterial.CreateBatch`. [T06][T24] | Отдельно рассматривать legacy callers. Нельзя приписывать Compact480 текущему simulation RP01. |
| RP01 оптимизирует prompt tokens и среднее всей строки | Simulation явно использует exact materializer, проверяет tokenizer SHA, response masks, capacity; exact objective — response **sum**. [T07][T08] | Это не current дефект указанного RP01 path. Замена sum на mean — самостоятельная гипотеза, а не восстановление отсутствующей mask. |
| Reference или базовые веса могут незаметно обучаться вместе с LoRA | Run замораживает reference, захватывает project recipient; optimizer получает его разрешённые bindings; readback сравнивает base/reference и полный candidate. [T09][T10] | Существуют реальные guards. Их наличие не доказывает качество, но опровергает безусловное утверждение об отсутствии защиты. |
| В ENRU продолжение забывает C50 moments/RNG | Run читает и импортирует C50, восстанавливает optimizer/RNG и сверяет байты состояния до C51. [T11] | Нельзя заменять этот текущий факт общим старым тезисом «restore только весов». Другие владельцы обучения проверяются отдельно. |

Автоградные подтверждения — **source-подтверждения**, не результаты нового runtime-теста. Старые P11/P86 подпункты содержат исторические формулировки; действующая карта и текущий код должны иметь приоритет при выборе работы. Отдельные Pool/Normalize/Slice/Concat проблемы нельзя автоматически объявлять причиной обучения без действительного trainable caller. [P02][P03]

## 3. Подтверждённая цепочка настоящего обучения

| Звено | Current owner и действие |
|---|---|
| Приём уроков | `Inspect` читает exact JSONL, проверяет hash/count/schema и разные prompt/triplet identities. Это не обучение. [T12] |
| Разрешённый учебный материал | Run создаёт typed `DpoPair` с существующим DeterministicGenerator authorship, проводит governance, `DpoPairTrainingContentAuthority.Project`, затем exact materializer. [T07] |
| То, что получает Student | Prompt/Chosen/Rejected кодируются отдельными byte-BPE segments; masks выделяют ответы; padding идёт до max длин. Admission и digest привязаны к исходному материалу. [T08] |
| Получатель изменения | Установленные q/v `LoraLinear`, фактические bindings `DpoProjectLoraRecipient`; один `CreateForProjectLora` на весь run. [T09][T11] |
| Обновление | На каждую пару два frozen-reference forward, два policy forward, два backward; `StepVector` вызывает AdamW один раз на minibatch. [T13] |
| Сохраняемое состояние | Full keyed weights + AdamW moments/step + RNG; fresh load сравнивается с live candidate. Это необходимая доставка результата обучения. [T10][T11] |
| Читатель обученного состояния | C# `Evaluate` проверяет candidate hash/full checkpoint, загружает именно candidate, вызывает общий scorer и `CodeDocumentationStudentGeneration.Generate`. [T14] |
| Измеряемый результат | Сопоставимые затраты actual training steps; отдельно heldout response likelihood и собственные ответы Student. Сам факт изменения LoRA или роста train margin — свидетельство действия, не доказательство ответа на новую задачу. |

Это прямой нейронный путь. Здесь нет доказательства, что DPO автоматически породил корректные subunits, обучил KNN, граф или доменный исполнитель. Для такого утверждения нужен отдельный consumer того же обученного состояния. Успех другой memory-реализации не заменяет результат этого Student.

## 4. Почему две пары пока нельзя просто запустить

Protected D04 **на фиксированном `bf9b543`** оставляла B1–B6 незаполненными. В live-версии это прямо названы задачи исполнителя: он выбирает фактор, находит данные и исходное состояние, готовит native путь и фиксирует команду в рамках порученной программы; отдельного личного согласования каждого поля не требуется. При этом утверждение об отсутствующем двухпарном micro-paired caller осталось: ниже конкретизировано именно недостающее подключение, а не разрешительный блокер. Protected-текст этим документом не меняется. [P04][F02][F03]

1. `ValidOptions` допускает только calibration с одной парой/одним update либо main500 с одним проходом. ENRU ещё уже: конкретный hash, batch10 и 50 новых updates. Это ограничение именно данного runner, не минимальная математическая ёмкость DpoTrainer. [T15]
2. CLI проверяет конкретные CalibrationHash/MainHash/ENRU hash. Подставить маленький произвольный JSONL недостаточно. [T16]
3. Даже добавление Mode=`micro-paired`, count2 и updates2 само по себе не решает A/B: обе стороны на одном TRAIN hash столкнутся на нынешнем `issuedPath`. [T01]
4. `DpoTrainer` без capability предназначен только для чтения/оценки. Вызывать его «напрямую» без existing lease означает отказ до изменения, а не дешёвую проверку обучения. Фактическая выдача разрешения находится у existing server-owned authority и domain lease broker. [T09]
5. A и B должны стартовать от независимых копий **одних** исходных weights/optimizer/RNG. Два запуска calibration с повторным обучением или B из A1 не образуют ремонтный A/B.

## 5. Минимальный проект caller в оригинальном коде

Это **проект реализации**, не уже существующие команды/API. Разрешённый результат данной работы — описание; код ниже не реализован и ничего не исполняет.

| Исходный файл/метод | Минимальное необходимое изменение | Что остаётся общей реализацией |
|---|---|---|
| `tools/Rp01SimulationDpo/Program.cs`: dispatch около 116–130, `ReadConfiguration/LoadModels` около 307–332 | Добавить отдельный явный `micro-paired` dispatch только для утверждённого C#-паспорта: exact TRAIN hash и, только при выборе расширения, PANEL hash; один study ID, два фиксированных arms, factor ID. Не принимать через этот режим произвольный новый baseline. [T16][T17] | Тот же verified saved-root loader, tokenizer/config checks, отсутствие random fallback. |
| `CodeDocumentationSimulationDpoExperiment.cs`: `SimulationDpoRunOptions`, `Run`, `ValidOptions` | Добавить внутренний bounded paired owner. Его контракт: два arms, в каждом ровно 2 пары, batch1, 2 updates, один проход PAD1→PAD2. Общий внутренний исполнитель переиспользует нынешние materialization/admission/trainer/publication/readback участки. Не создавать второй DPO trainer implementation. [T01][T07][T11][T15] | Старые calibration/main/ENRU guards не ослабляются. Режим не меняет действующую политику продолжения C50. |
| Тот же owner: `issuedPath`/issued journal | Связать study identity с factor/source/TRAIN; B0 manifest и weights hashes; hash независимого reference; tokenizer/config hashes; initial optimizer settings/state и RNG snapshot hashes; явным отображением arm → build hash и treatment flag; выбранным режимом измерения и optional PANEL/settings hashes. Это две **заранее объявленные** стороны одного порученного опыта. Хранить долговечные issued/completed для каждой стороны под одной study identity. Повтор arm с возможным эффектом запрещён; неизвестный эффект требует reconciliation существующей записи. | Не удалять старые markers; не использовать новый RunId как обход. Обе стороны используют один бинарник, содержащий только выбранный A/B фактор, и одинаковые исходные состояния; разные treatment flags не разрешают менять прочие настройки. Две стороны — первоначально объявленный парный опыт, не ретрай. |
| Тот же owner: lease/trainer composition | Для каждой стороны создать свои policy/reference, один trainer и свой live domain training lease для того же content fingerprint. Выдавать capability через существующие `AcquireCodeDocumentationTrainingLease` → `IssueDpoTrainerMutationAdmissionCapability`. [T09] | Никаких фиктивных teacher, licence, approval tokens; actual source — simulation. |
| `11.DPO/DpoTrainer.cs`: `BatchLogProbs`, `ForwardPolicyBatchLogProbs` и вызывающий `StepVectorCore` | После полного `CaptureBatch` и exact validation создать внутренний **рабочий вид** стороны: A использует исходные padded IDs/mask; B копирует prefix IDs **и соответствующей response mask** до ChosenLength/RejectedLength. Разрешать B только для exact material и названного experiment path; public/default поведение A. [T08][T13][T18] | Сохраняются authoritative batch/digest/admission, токены, response targets, β, sum objective, learning rate, weights и порядок. Не искать длину по ID0. |
| `CodeDocumentationSimulationDpoEvaluation.Evaluate` | Только для заранее выбранного необязательного расширения использовать существующий reader для четырёх состояний, с одним неизменным scorer и decoder в A/B. В базовом ресурсном опыте Evaluate/Generate не вызываются. Candidate A1/B1 — реальные разные checkpoints обучения. [T14] | Evaluation не получает treatment-specific scorer и не обучает gold. |

**Ключевая ошибка возможной реализации:** нельзя заменить authoritative exact carrier укороченными tensors и затем просить прежний `CaptureAndValidate` принять их. Он обязан сравнивать исходные SequenceLength, данные и digest. Обрезание — внутреннее представление для forward/backward **после** полной проверки, не изменение оценки или provenance материала. [T08]

Один paired owner не требует смешивать память A и B. Для ресурсного сравнения предпочтительны два изолированных процесса одного бинарника под этим owner, по одному на arm; каждый имеет свою запись issued, собственный allocator/JIT baseline и независимый пик RAM. Если используются последовательные интервалы одного процесса, счётчик cumulative PeakWorkingSet нельзя называть отдельным пиком B. Нельзя складывать outer `training_step_total` с вложенными forward/backward/serialization как независимые затраты.

## 6. Паспорт конкретного ND-DPO-PAD уточнения

### 6.1. Student, baseline и состояние

Student — **`TransformerModel.LlmModel`, `rp01-csharp-small/v1`, предметная тема C#-документация**. Current config задаёт vocab629, context2048, hidden512, 4 слоя, 8 query heads/2 KV heads, FFN1408, RoPE theta10000, RMS epsilon1e-5; LoRA rank8, alpha16, dropout0, q_proj/v_proj. Это прочитанные параметры, не измеренная способность модели. [T19]

Начальный источник — тот же **saved B0** текущего `LoadModels`: Step1, root без parent, `InitialBaselineNoUpdate`; weights SHA256 `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`, manifest SHA256 `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`, 55 full checkpoint entries, 39 base и 16 adapter tensors. Exact path берётся из verified manifest через existing loader, не угадывается. В этом аудите бинарные weights не открывались; перед выполнением нужно независимо подтвердить actual bytes и пути, а не считать константы runtime-доказательством. [T17]

Config artifact SHA256 в host — `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`; tokenizer SHA256 в config — `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066`. Обе стороны используют одни bytes; LoRA не пересоздаётся между уроками. [T17][T19]

Для этого **C# B0** профиля предлагается точно существующий старт: fresh AdamW на плечо, initial step0/без накопленных m/v; lr1e-5, β1=.9, β2=.95, weightDecay=.01, epsilon1e-8. Это следует из factory/defaults и отсутствия C# continuation, но actual exported state/settings всё равно записываются до первого урока. ENRU C50 moments к C# не подмешиваются. [T20][T25][T26][T11] RNG capture из загруженного Student сравнивается между сторонами; seed20261014 не заменяет actual cursors/hash. Dropout0 остаётся фиксирован.

### 6.2. Один фактор и точные уроки

**A:** сегодняшний padded execution. **B:** только side-valid-length execution после той же admission. Гипотеза: исключение пустого хвоста уменьшит стоимость двух настоящих updates; влияние на численную траекторию и ответы измеряется отдельно. Экономия заранее не гарантирована: prefix-copy и небольшой масштаб могут перекрыть её.

Точный prefix: `Explain in one short English sentence what the method returns. Describe argument order. Code: `. Код сразу после этого prefix. Материал — уже описанный PAD1/PAD2 protected23; здесь он воспроизведён как вход конкретного caller, не объявлен новым датасетом или новым механизмом. [P01]

| ID | Код | Chosen | Rejected |
|---|---|---|---|
| PAD1 | `int Delta(int first,int second){return first-second;}` | `Returns the first argument minus the second argument.` | `Returns the second argument minus the first argument and then takes the absolute value of that result.` |
| PAD2 | `int Delta(int first,int second){return second-first;}` | `Returns the second argument minus the first argument without taking the absolute value or changing the sign afterward.` | `Returns the first argument minus the second argument.` |

Обе стороны получают **одни JSONL bytes/hash, authorship, pair fingerprints, exact masks, порядок PAD1→PAD2**. Tokenizer не обучается. До первого update owner подтверждает, что response lengths действительно различаются, что обе стороны помещаются в context2048 и что roundtrip точен. Эти реальные длины пока неизвестны. Если contrast не возник, материал не подходит для этого опыта; нельзя после результата дописывать padding или менять фразы, сохраняя прежнее имя опыта.

### 6.3. Измерение до/после и полезный результат

Плановый бюджет обучения: **2 arms × 2 StepVector = 4 вызова StepVector и 4 optimizer updates**. Внутри — **16 model forwards (8 policy, 8 reference) и 8 policy backwards**. Эти вложенные операции не суммируются с 4 updates как дополнительные уроки. В каждом arm — один trainer; после первого урока state переносится во второй естественно, без reset. Состояния обозначаются A0/A1 и B0/B1, где индекс1 — после обоих уроков. **Базовый ресурсный опыт: 0 Evaluate, 0 Generate и 0 внешних scoring panels.** Наблюдение численного действия происходит внутри тех же actual training steps. Без панели answer accuracy — NOT_MEASURED, а не unchanged или 0.

Основная величина — стоимость **одинаковых двух настоящих updates**: training-step elapsed, раздельные forward/backward/optimizer-phase и serialization intervals, CPU, независимый peak RAM, allocated/state bytes при доступном измерителе. Текущий timing ApplyOptimizer включает recipient checks и не равен инструментированному чистому kernel времени; текущий receipt это честно различает. [T13][T21]

Внутри этих же steps фиксировать actual valid-target counts, train loss/margin и изменение LoRA/AdamW. Если нужен gradient comparison, общий observer должен стоять до ApplyOptimizer/ZeroGrad, а не после возврата StepVector. Наличие/конечность градиентов — механизм; экономия фактического update или изменение полезного ответа — результат.

### 6.4. Необязательное расширение: heldout reader

Расширение выбирается явно до запуска; оно не является условием признания ресурсного успеха и не выполняется автоматически. При его выборе используется регрессионная панель текущего C# reader: четыре Evaluate на A0/A1/B0/B1, каждый на четырёх нижеописанных случаях. Дополнительный бюджет — **16 генераций и 32 candidate-side scoring calls**; autoregressive forwards считаются отдельно. Число учебных updates остаётся 4. Baseline ресурсного опыта соответствует protected23: 0 Generate. Optional PANEL hash, settings и бюджет фиксируются в той же study identity до первого эффекта. [P01][P04][T14]

| EXAM | Новый код с тем же prefix | Точный Chosen для scoring | Точный Rejected для scoring |
|---|---|---|---|
| D1 | `int Gap(int a,int b){return a-b;}` | `Returns the first argument minus the second argument.` | `Returns the second argument minus the first argument.` |
| D2 | `int Gap(int a,int b){return b-a;}` | `Returns the second argument minus the first argument.` | `Returns the first argument minus the second argument.` |
| D3 | `int Diff(int left,int right)=>left-right;` | `Returns the first argument minus the second argument.` | `Returns the second argument minus the first argument.` |
| D4 | `int Diff(int left,int right)=>right-left;` | `Returns the second argument minus the first argument.` | `Returns the first argument minus the second argument.` |

Gold определяется операндами до run; в генератор поступают только prefix и код. Scoring получает ровно указанные Chosen/Rejected. Панель не используется для градиента, подбора lr, lengths или выбора удачного checkpoint. Это **малый близкий перенос**, а не четыре независимых семейства задач: обе стороны одной subtraction-семантики родственны TRAIN. Положительный результат не доказывает освоение C# или других доменов. Более строгая family-disjoint панель — отдельное последующее поручение, не требование незаметно увеличить этот опыт.

**Нормализация optional scoring фиксируется до запуска.** Для каждого состояния и случая сохранить native `chosenMean`, `rejectedMean` и counts сдвинутых response targets; primary heldout NLL — равновесное по четырём случаям среднее `−chosenMean` в nat/response-token. Дополнительно показать per-case `chosenNllSum = −chosenMean × chosenTargets`, сумму по панели и token-weighted NLL `ΣchosenNllSum / ΣchosenTargets`, а также `chosenMean − rejectedMean`. Native reader уже выдаёт counts, mean и восстановленные из mean суммы; последние не выдавать за независимо накопленную высокоточную сумму logP. Все величины вычисляются из тех же 32 side scores, без дополнительных forward. Для каждой метрики публикуются уровни A0/A1/B0/B1, изменения `ΔA=A1−A0`, `ΔB=B1−B0` и разность изменений `ΔB−ΔA`; снижение NLL полезно, увеличение normalized margin полезно. Training objective остаётся response **sum**, heldout token normalization его не меняет. [T14]

Фиксированное декодирование только optional версии: temperature0/topK0/topP1/seed17/MaxNewTokens64; MaxAnswerChars160 — передаваемое поле, **не доказанный hard cap**. `Generate` передаёт остальные настройки TextGenerator, но не применяет этот character cap. [T22] Все raw ответы и chosen/rejected scores сохраняются; likelihood не подменяет свободный ответ.

**Правило успеха в теле карточки:** любое фактически измеренное полезное улучшение считается успехом по этой метрике и предметному домену; рост accuracy не обязателен. Например, подтверждённая экономия времени настоящих updates — успех и без измерения ответов, а при выбранной панели — также при неизменной доле правильных объяснений. Рядом показать все фактически измеренные изменения и потери: Δпамяти, численные расхождения, а при наличии панели — Δкачества и ΔNLL. Неизмеренные величины пометить NOT_MEASURED. При неразличимом времени писать «неразличимо». Изменение hash/цифр margin само по себе не объявлять пользой. Failure до update — blocked/invalid, а не accuracy0.

### 6.5. Домены и пределы

Прямо подтверждён C# exact neural path. При unequal side lengths тот же вычислительный механизм может относиться к другим exact DPO доменам, но ENRU сейчас жёстко связан с C50→C100 и конкретным500 dataset; этот caller нельзя объявить готовым двухпарным ENRU API. SQL, AST/KNN/order-aware memory и semantic Unit learners данным опытом не проверяются.

Ни ресурсный успех, ни две новые правильные строки не доказывают, что padding был причиной исторического ухудшения качества RP01. Эксперимент отвечает на более узкий вопрос: изменился ли полезный показатель **того же действительного обучения** от одного изменения выполняемой длины.

## 7. FP32 margin — известная гипотеза, второй refinement

В current коде response logP суммируются во float; PolicyLogProbGraph и BatchLogProbs также возвращают float; далее вычитаются близкие policy/reference totals. Stable softplus сохраняет хвост derivative, но не восстанавливает информацию, потерянную до вычисления margin. Это source-факт и численная гипотеза, не наблюдённый эффект этого run. [T18][T23]

**Новизны нет:** PG-09 уже описывает те же carriers, double через subtraction и различие между numeric proxy и actual neural evidence. [P05] Поэтому нельзя публиковать это как новую N-гипотезу. Поскольку пользователь исключил самостоятельный zero-update опыт, применимое уточнение — захватывать эти diagnostics **внутри настоящего парного обучения**, затем измерять полезный эффект/расходы и одинаковый heldout.

Если выбирается этот фактор вместо PAD, ремонт должен сохранять double от суммы **тех же per-token float logP** через BatchLogProbs, PolicyLogProbGraph, policy/reference subtraction и итоговый margin/derivative до последнего scalar cast на границе float32 tensor gradient. Ранний cast totals обратно во float оставит исходную потерю. Не менять вместе с этим log-sum-exp, sum→mean, β, rank, data или LoRA scaling. Длинные/короткие и byte-heavy языковые strata должны быть обозначены заранее, а не выбраны по удачным deltas. Это другой A/B и другой материал; **не добавлять его одновременно к PAD**. Два коротких D04 урока могут не иметь полезного разрешения для этой гипотезы.

## 8. Практический промпт реализации и последующего исполнения

> Этот промпт относится к последующей явно разрешённой стадии реализации; текущая работа ограничена подготовкой документов. Реализуй только описанный paired native caller ND-DPO-PAD на указанном core SHA в разрешённых исходных файлах. Это уточнение существующего E-DPO-VALID-LENGTH, не новая причина деградации. Не редактируй `docs/plans/replanning-possible-diffs`.
>
> Сначала сделай reviewable patch: отдельный bounded paired-study owner, ровно два arms с независимыми состояниями, две exact пары PAD1/PAD2, batch1 и два StepVector на arm. Один и тот же TRAIN hash/provenance и один заранее выбранный factor. Переиспользуй verified LoadModels и существующие materialization, project recipient, server-owned domain lease, CreateForProjectLora, transaction/publication и fresh readback. Точный baseline — rp01-csharp-small/v1 saved B0 с указанными SHA, без random fallback; reference — отдельная frozen копия того же B0, fresh AdamW step0 на arm с одинаковыми settings, actual RNG snapshots сравниваются.
>
> Учти нынешний anti-replay key domain+mode+input hash. Сделай отдельную первоначально объявленную paired-study identity с двумя arm records в рамках порученной программы. Привяжи её к factor/source/TRAIN, B0 manifest/weights, reference, tokenizer/config, initial optimizer settings/state и RNG snapshot hashes, а также к отображению arm → build hash/treatment flag; один бинарник и одинаковые прочие настройки. При выбранном расширении добавь PANEL/settings hashes. Не удаляй marker, не сбрасывай историю, не обходи отказ новым RunId. Старые calibration/main/ENRU modes сохраняют прежние guards.
>
> Treatment B изменяет только рабочую длину стороны в настоящих forward/backward после полной exact admission: копируй IDs и response mask до ChosenLength/RejectedLength. Authoritative batch/digest остаются прежними. A использует текущий padding. Не меняй границу prompt/response, objective sum, β, lr, tokenizer, rank, LoRA weights или данные.
>
> До запуска внеси в точный паспорт фактически разрешённые пути, bytes/hashes TRAIN/B0, build identity, существующую после patch команду и разрешённые outputs. Базовый бюджет — 4 actual updates, без Evaluate/Generate; PANEL hash требуется только при заранее выбранном расширении. Не выдавай проектируемое имя micro-paired за готовую native команду. Руководствуйся текущим разрешённым scope: исторические отметки отдельных паспортов не блокируют разрешённую подготовку документов и не дают дополнительных прав на запуск.
>
> При разрешённом исполнении базового опыта измеряй реальные четыре updates; Evaluate/Generate/scoring panels не вызывай. Сохраняй исходные и конечные keyed weights/optimizer/RNG, реальные costs, input lengths, finite numerics и frozen base/reference. Только если до запуска отдельно выбрано optional расширение, получи A0/A1/B0/B1 через один и тот же C# reader на неизменной четырёхзадачной панели с буквальными Chosen/Rejected из раздела 6.4: дополнительно 16 генераций/32 side scores с указанными настройками. Заранее зафиксируй primary NLL как равновесное по случаям среднее отрицательного native chosenMean; рядом покажи counts, восстановленные суммы, token-weighted NLL, normalized margin и уровни/дельты по формуле раздела 6.4. Не смешивай inference cost с training cost. Любое измеренное полезное улучшение — успех по его метрике и домену, без требования accuracy↑; остальные изменения и ухудшения покажи рядом. Без панели accuracy — NOT_MEASURED. Не заменяй real training proxy/zero-update или дополнительными фиктивными уроками. Не делай автоматических повторов и дополнительных абляций.

## 9. Что остаётся неизвестным до выполнения

Actual weights bytes/paths на Windows, hashes новых материализованных TRAIN/PANEL, lengths после actual tokenizer, переданные RNG cursors, build/runtime identity, actual command после реализации, timing resolution и результаты A/B в этом аудите **не измерены**. Эти поля должны заполняться наблюдениями, не предположениями. Ограничение не мешает подготовить конкретный patch, но не позволяет назвать эксперимент уже выполненным или доказанным улучшением.

Protected-папка уже отражает большинство найденных механизмов, поэтому нынешняя работа не должна дублировать сотни карточек. По этой оси приоритет: **paired caller → один PAD factor → actual learning before/after → честный useful-metric вывод**. Общие операторные проверки без установленного learning caller в активную очередь не добавляются.

## Источники с фиксированными версиями

[F01]: https://github.com/lubochka/llm_mvp_core/compare/bf9b543e149ef197da85a767821d98c9b67916d2...de4ef0f14172dada4daf142a4c66ba19e96bb6d4
[F02]: https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L3-L11
[F03]: https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L60-L93
[F04]: https://github.com/lubochka/llm_mvp_core/blob/de4ef0f14172dada4daf142a4c66ba19e96bb6d4/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L154-L166
[T01]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L80-L102
[T02]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Layers/Embedding.cs#L38-L81
[T03]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/00.TensorCore/Layers/Linear.cs#L72-L100
[T04]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/03.TransformerModel/Layers/CausalSelfAttention.cs#L56-L72
[T05]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L1072-L1120
[T06]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L2351-L2429
[T07]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L106-L124
[T08]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/Models/DpoExactPairMaterial.cs#L19-L145
[T09]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L128-L142
[T10]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L192-L209
[T11]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L145-L189
[T12]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L39-L78
[T13]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L832-L939
[T14]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoEvaluation.cs#L53-L106
[T15]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L224-L230
[T16]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/tools/Rp01SimulationDpo/Program.cs#L116-L130
[T17]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/tools/Rp01SimulationDpo/Program.cs#L307-L332
[T18]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L991-L1068
[T19]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/config/code-documentation/generation-v1.json#L1-L49
[T20]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L364-L386
[T21]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L211-L220
[T22]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/14.ContinuousLearning/Training/CodeDocumentationStudentGeneration.cs#L61-L76
[T23]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L891-L904
[T24]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L466-L482
[T25]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/11.DPO/DpoTrainer.cs#L450-L480
[T26]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/04.TrainingLoop/Optimizers/AdamW.cs#L16-L39

[P01]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md#L142-L154
[P02]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/06-findings-map.md#L102-L108
[P03]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/07-conditional-probes.md#L23-L29
[P04]: https://github.com/lubochka/llm_mvp_core/blob/bf9b543e149ef197da85a767821d98c9b67916d2/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md#L58-L143
[P05]: https://github.com/lubochka/xiigen-planning/blob/1ad622fe1f6d1e8b0860265d8c208aeada3fefb0/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/PREPARETOGPL-EXPERIMENTS-2026-10-18.md#L131-L139

