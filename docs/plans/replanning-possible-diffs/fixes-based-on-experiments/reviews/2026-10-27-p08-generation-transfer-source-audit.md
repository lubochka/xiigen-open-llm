# P08: аудит результата и границы переноса генерации — 27 октября 2026

**Результат: P08 дал узкий положительный эффект распределения на уже обученном состоянии; автоматический перенос в продукт и выигрыш качества не доказаны.** В строке T1 вероятность правильного `+` после фильтрации выросла с 0,86879754 до 1, состав сократился с двух кандидатов до одного. На остальных трёх строках распределения A/B совпали; фактические ответы совпали на всех четырёх. Основной `Sampler` по-прежнему применяет K **или** p. Совместное применение существует только в экспериментальной копии. В этой проверке найдены недостаточность проверки A/B через генератор, неточная ресурсная формулировка и отдельный статический дефект выборки при draw=0.

Фаза — **READ_ONLY_RESEARCH + этот Markdown**. Не выполнялись сборка, тесты, inference, обучение, повтор P0801, Git-изменения или правки кода/старых результатов. Числа ниже прочитаны из уже сохранённого опыта, а не получены новым запуском. Автор пакета — единственный исследовательский worker `/root/p08_transfer_audit`; вложенные workers не запускались. Это адресный source/raw audit одного опыта, не полный аудит всех карточек и не повтор прежних82/83.

## 1. Идентичность источников и текущий статус

Локальные `.git/logs/HEAD` и `git show --format=fuller --stat` подтверждают commit **`f4dcf691cf239b14c428d2efc72a31738f18ec93`** от 27.10.2026 с результатом P08. В его дельте25 файлов:24 под `experiments-v2/RP01-MICRO/P0801` и запись в `docs/plans/replanning-possible-diffs/required-changes.md`. В этой дельте нет продуктовых файлов. Commit очереди Q009 — `051df1f444c0659a4cc0679b989e7b1d47fbbdb0`. Прочитанный позднейший локальный HEAD — `dd5d786879ea0e92d471eb84eb4fa0b252458127`; он не объявляется SHA запуска P08. Remote publication этой проверкой не подтверждалась.

Сохранённый `source-hashes.json` называет исходный HEAD опыта `8370a1f70a990e87e14a26a1f8cdc00aa37078f6` и дизайн-snapshot `6e58d068db4de36b82afff49fd8a8f98eba8eb9c`. Оба отличать от commit результата. Прямое чтение текущих байтов и SHA256 подтвердило совпадение всех10 перечисленных source-файлов с этим manifest: Sampler, TextGenerator, Trainer, AdamW, BpeTokenizer, LlmModel, bridge, B, diagnostics, Program. Это проверка сохранённых байтов, не выполнение поведения.

| Сохранённое состояние P0801 | Проверенный SHA256 | Что подтверждено |
|---|---|---|
| `S1.weights.bin` | `d4817625ffcd3d5cb5939d2793731881393dd814050f0d9fff6f0585647b1e78` | Байты совпадают с `metrics.learning.saved_state`;320620байт |
| `S1.optimizer.bin` | `7b35c9cf47f601f76d98e6cb8e4fc34a870ef35a5fd1ccc84c2e321f7d458aff` | Байты совпадают;7384байт |
| `S1.rng.json` | `fd5ebc3cc4be30392246ed07eed86c10dd97b1320a3e3ddf6a8e555c228d206e` | Байты совпадают;740байт |

Загрузка/продолжение этих артефактов в новой модели сейчас **не проверялись**. Наличие и совпадение hash не заменяют behavioral restore.

Основные первичные ссылки:

- [Паспорт P08, строки4–26](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/P08-answer-boundary.md:4).
- [Программа H64-RD, строки22–30](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/36-diagnostic-learning-programmes.md:22).
- [Первичный отчёт](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/report.md), [before](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/before.json), [after](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/after.json), [arms](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/arms.json), [metrics](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/metrics.json), [status](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/status.json), [source identities](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/source-hashes.json).
- [Кандидат в required-changes,1113–1117](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/required-changes.md:1113): прямо назван находкой, не правкой исходника.

## 2. Реальный учебный участок и допустимый вывод

Получатель обучения — q/v LoRA одного слоя H64, четыре обучаемых массива;12 массивов основы заморожены. Тема в отчёте названа «документация», но наблюдаемая задача значительно уже: один знак C#-разности. Это не доказательство составления документации, понимания всего C#, перевода или выбора домена. Уровни Unit/Flow не задействованы.

Переход общий до A/B: восстановление S0, AdamW step400 и RNG → T1,T2,T1,T2 → step404. В сохранённых metrics есть4updates,4Backward, изменения всех четырёх массивов LoRA и моментов, неизменность hash frozen state. Наибольшая абсолютная дельта LoRA —0,004357472. Учебные losses1,1465623 /1,2090048 /1,1422932 /1,2038769 считаются до собственного update и относятся к полному next-token objective; это не четыре измерения response accuracy.

| Строка | Gold | Greedy S0→S1 | ΔlogP(gold), S1−S0 | Δmargin(gold−other) | Что можно сказать |
|---|---|---|---:|---:|---|
| T1 `9-4` | `+` | `+`→`+` | +0,02023866 | +0,02913260 | Сохранён уже имевшийся ответ; новая acquisition в этих4шагах не показана |
| T2 `4-9` | `-` | `+`→`+` | +0,02497535 | +0,02633190 | Вероятность gold выросла, ответ остаётся неправильным;0→0 не retention |
| E1 `8-3` | `+` | `-`→`-` | +0,00043908 | **−0,01778531** | LogP(gold) и margin движутся по-разному; ухудшение margin сохраняется |
| E2 `3-8` | `-` | `-`→`-` | +0,02128723 | +0,01750445 | Узкое сохранение правильного ответа на этой строке |

Числа разностей вычислены из сохранённых `before.rows`/`after.rows`; это арифметика исторических данных, не новый модельный результат. До/после2/4, TRAIN1/2 и EXAM1/2. Восстановленный старт уже имел T1. Другие задания, seed, repeat, длинные ответы не измерялись. Положительная вероятность gold на T1/T2/E2 признаётся в своей области; она не превращается в доказательство улучшения от K→p: оба sampler читают одинаковый S1.

**Несогласованность паспорта с objective.** ПаспортP08:10 говорит `exact response masks`, тогда как [Program385–395](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/Program.cs:385) передаёт `Ids(prompt+answer)` в `TrainSupervisedNextTokenStep` без маски. [Trainer102–125](C:/Projects/llm_mvp_core/04.TrainingLoop/Trainer.cs:102) строит targets для всей последовательности и выполняет полный `CrossEntropyLoss`/`BackwardLoss`. Это соответствует bytes-рецепту H64-RD:28. Правка плана: назвать фактический objective full-sequence SFT и отдельно response-only readout; не менять на response-mask обучение, не переучивать donor ради исправления слов. Связать с [FIX-SEP/62](../62-supervised-framing-and-learning-readout.md), где различение framing/readout уже имеет владельца.

`VALID_FOR_NAMED_UNIT` — сохранённая метка автора опыта, не универсальная сертификация. [Program590–601](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/Program.cs:590) проверяет запись параметров,4updates и T1-before; вычисленный `t1After` не входит в validity predicate, хотя комментарий говорит «до и после». В данном raw T1-after=true, поэтому утраты T1 здесь не обнаружено. В следующих receipts отдельно сохранять qualification-before, retention-after и regressions; нельзя читать из одной метки VALID, что все after-навыки сохранены, либо запрещать честно измеренную регрессию ради PASS.

## 3. A/B: фактор, его польза и пределы

[Текущий Sampler20–38](C:/Projects/llm_mvp_core/09.Inference/Sampler.cs:20): temperature≤0 возвращает Argmax до фильтров; при положительной температуре сначала Scale, затем `if topK>0 … else if topP<1 …`. [Копия B32–66](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/src/B/P0801SequentialTopKTopPSampler.cs:32) сохраняет greedy-ветвь и применяет TopKMask→TopPFilter. TopP сам перенормирует оставшиеся logits, накапливает массу и оставляет пересекающий порог токен. Это изменение политики stochastic decoding, а не learning algorithm.

A действительно вызывает native sampler через [bridge30–38](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/src/P0801SamplerBridge.cs:30). Обе руки получают клоны одинаковых S1 logits; настройки temp1,K2,p0,6 и `new Random(20261027)` одинаковы для каждой строки ([Program473–493](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/Program.cs:473)). Единственное использованное число —0,12251277506468482. Это paired comparison, но не несколько независимых случайных выборок и не оценка частоты изменений.

| Строка | A: token ID и вероятность | B | Фактический ответ A/B |
|---|---|---|---|
| T1 |43 `+`:0,86879754;45 `-`:0,13120241 |43:1 |`+`/`+`, оба верны |
| T2 |43 `+`:0,54291773;45 `-`:0,45708230 |То же |`+`/`+`, оба неверны |
| E1 |45 `-`:0,53978211;182:0,46021786 |То же |`-`/`-`, оба неверны; gold43 уже отсутствует в topK |
| E2 |45 `-`:0,53939402;182:0,46060604 |То же |`-`/`-`, оба верны; посторонний byte остаётся возможным |

Измеренная локальная польза — на T1 убрана масса неправильного `-` и выполнено выбранное требование cumulative p внутри topK. На E1 фильтр не способен восстановить исключённый ранее правильный `+`. На T2/E2 правильный альтернативный/главный кандидат не получил нового преимущества. «Стало меньше кандидатов» само по себе не является качеством: если главный кандидат ошибочен, сокращение может удалить правильную альтернативу. Нужен countercontrol с независимо заданным gold у удаляемого кандидата, без переобъявления gold по победителю.

Для фиксированных конечных logits, положительной температуры и уникального максимума topK и p>0 сохраняют максимум; его **ранг** неизменен. Greedy здесь вообще обходит фильтры. Это не означает равенство stochastic outputs при любом draw: T1 может дать A=`-`,B=`+` при draw внутри интервала A для `-`. Использованное маленькое draw выбрало первую положительную массу во всех строках, поэтому совпадение ответов ожидаемо для данного draw. Нельзя из этого выводить «фактор не влияет на ответы» или «обязательно улучшит генерацию».

Два component controls `[5,4,0]` и `[10,0,-10]` подтверждают сужение2→1 приK2,p0,6. p1 parity записана для **двух component vectors**, не для всех будущих logits/доменов. [one-factor.diff](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/one-factor.diff) строится из решающего блока; [Program712–761](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/Program.cs:712) не делает полный semantic diff helper-методов. Самостоятельно прочитаны B TopKMask74–88, TopP91–118, Multinomial121–131 и native аналоги42–99: дополнительной намеренной смены семантики в них не установлено. Генератор отчёта блока один этот вывод не доказывает.

Состав A реконструирует отдельная [диагностика23–36,63–99](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/src/P0801FilterDiagnostics.cs:23). Проверка predicted token при одном draw в Program486–488 недостаточна для доказательства полного равенства распределений: разные распределения могут вернуть один token. Источник чисел support/probabilities поэтому обозначать как **реконструкция с сопоставленным кодом**, а не прямое наблюдение внутреннего native tensor. Для будущего допуска нужен независимый численный oracle и проверка interval boundaries/отсечённых ID, не только один token.

## 4. Подтверждённые дефекты и пробелы

| Находка | Доказательство | Классификация и действие |
|---|---|---|
| A/B генераторы только созданы | [Program462–467](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/Program.cs:462); [bridge40–50](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/src/P0801SamplerBridge.cs:40); report39 | Подтверждённое расхождение с passport18: native путь `_sampler` для этих A/B не исполнен. Сохранить полезный sampler-result, label `A_B_GENERATOR_ROUTE_NOT_RUN`; constructor != effect |
| Категорическое отрицание ресурсного эффекта | [report126](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/attempt-0001/report.md:126) говорит «не даёт и не может давать»; B42–47/96–117 выполняет дополнительный sort/softmax/cumsum и выделения | Неверное обобщение. Верно: sampler latency/allocation A/B **не измерены**. Отсутствие model Forward не делает sampler бесплатным; знак/размер resource delta неизвестен |
| Нулевая вероятность может быть выбрана при draw0 | [Sampler89–98](C:/Projects/llm_mvp_core/09.Inference/Sampler.cs:89), B121–130: `acc += p[i]; if (r <= acc) return i` | Подтверждённый **статическим разбором** отдельный defect. Для logits `[0,3,1]`,temp1,K1,p1 остаётся id1; при injected draw0 первая итерация имеет `0 <= 0`, возвращает исключённый id0. Контрпример не запускался; это не измеренный результат P08 и не следствие нового фактора |
| Framing/mask слова расходятся с consumer | P08:10, Program385–395, Trainer102–125 | Подтверждённая неточность документации objective. Исправлять формулировку по donor; не менять paid training задним числом |
| Общий VALID может быть прочитан как retention-all | Program590–601, metrics acquired_t1_before/after и zero_to_zero_rows | Ограничение oracle/комментария; observed T1 сохранён, скрытая утрата не обнаружена. Разнести qualification и результат по строкам |
| Отсутствует замер sampler A/B и execution через выбранные генераторы | report39/132, metrics.cost | Блокирует заявленный общий перенос по качеству/ресурсам, не отменяет наблюдённую пользу T1 |

EOS в P08 задан null, maxNewTokens=1. У [TextGenerator213–249](C:/Projects/llm_mvp_core/09.Inference/TextGenerator.cs:213) есть stop/EOS/decode path, но он не покрыт этим A/B. Длинные ответы, EOS-preservation, multiple draws, UTF-8 byte182 как единственный token, границы topK ties и экстремальные параметры остаются вне результата. Не объявлять их сломанными без отдельного первичного свидетельства.

## 5. Что уже покрыто продуктом — не создавать дубликаты

Прочитаны полностью следующие три тестовых файла; ни один тест сейчас не запускался:

| Существующий source | Уже проверяет | Адресный пробел P08 |
|---|---|---|
| [SamplerTests16–78](C:/Projects/llm_mvp_core/Tests/SamplerTests.cs:16) | Greedy,topK1 с уникальным максимумом,диапазон при temperature,очень малый p | Нет совместногоK>0,p<1 с независимым expected support/token; нет draw0 при ведущей нулевой вероятности |
| [SamplerTests82–136](C:/Projects/llm_mvp_core/Tests/SamplerTests.cs:82) | `SequenceRandom` с несколькими draws,изоляция seeded/unseeded,greedy не потребляет RNG,null-RNG guard | Переиспользовать эту инфраструктуру для draw0 и порогов,не создавать второй RNG framework |
| [TextGeneratorFirstDecisionTests18–55](C:/Projects/llm_mvp_core/Tests/TextGeneratorFirstDecisionTests.cs:18) | Реальный вызов injected sampler,ordinary/captured receipt parity,immutable snapshot,Forward counts при budget1/3 | Fixture обнулённая/temp0; не является learned-S1,K+p bridge proof |
| [TextGeneratorFirstDecisionTests59–67](C:/Projects/llm_mvp_core/Tests/TextGeneratorFirstDecisionTests.cs:59) | Cancellation до cache/sample | Сохранить существующий guard |
| [TextGeneratorSeededSamplingTests22–112](C:/Projects/llm_mvp_core/Tests/TextGeneratorSeededSamplingTests.cs:22) | Exact replay двух seeds,один Random на decode,injected sampler,null seam,изоляция | Существующая seam рабочая по исходнику тестов; P0801 всё равно её не вызвал с A/B |

Поиск `Sampler`,P08/P0801,topK/topP в обоих каталогах `fixes-based-on-experiments` и историческом `fixes-based-on-experimentsperiments`, плюс read реестра, не выявил ранее назначенного владельца composition-transfer. Найден кандидат required-changes1113. Не присваивать новый FIX только по этому отсутствию: родитель связывает новый bounded transfer-plan с существующим P08, а общие framing/learning-readout ошибки — с FIX-SEP/62 и paired evidence/63. Полный каталог raw здесь не просматривался.

## 6. Конкретное усиление будущей программы — всё NOT_RUN

1. **Сначала разрешить контракт продукта.** Зафиксировать, поддерживает ли public API композицию K→p или legacy precedence. P08 показывает различие и локальное улучшение массыT1, но не даёт права бесшумно менять default для всех callers. Записать version/settings identity; до решения и недостающего resource evidence — HOLD общего переноса. Сужение кандидатов не превращать в универсальный qualityPASS.

2. **Разделить три результата.** `learning_state_transition` = S0→S1; `learning_behavior` = построчные actual outputs/logP/margin до/после; `decoder_factor` = A/B на одномS1. Для after-only A/B не заполнять «A-before/B-before sampling accuracy» одинаковым greedy baseline без явной маркировки другого режима. Actual response2/4 хранить отдельно от reconstructed candidate probability. E1-regression, T2/E1 errors и посторонний182 обязательны.

3. **Использовать уже оплаченный S1.** Для допроверки native consumer восстановить этот `S1.weights.bin` и относящиеся к нему model/tokenizer/RNG identities; сохранить optimizer404 как provenance, без новых updates. Исходные4updates остаются учебным участком этой оплаченной попытки; read-only recovery не называется новой acquisition-программой. Перед возможным исполнением проверить текущие receipts, чтобы не повторить уже проведённое восстановление. Ни400prepare,ни4SFT заново ради отчёта не назначать. При несовместимости записать конкретный missing artifact,не подбирать checkpoint/seed.

4. **Закрыть именно отсутствующий consumer-вызов.** Предложение отдельного ограниченного recovery после прямого разрешения: четыре исходных prompts×A/B, `GenerateWithFirstDecision(max1,temp1,K2,p.6,seed20261027,EOS=null)` через injected native generator;8Generation/8prefillForward/8samples,0Backward/0updates. Сохранить callback count1 на каждую generation, actual settings/RNG draw/token/receipt, source logits и равенство логитам сохранённогоS1. Не подменять вызов конструктора проверкой. Никакого расширения до длинных ответов в этом бюджете. Результат не обязан показать разные ответы: для этого seed ожидается прежнее совпадение. Если запусков не разрешено — этот шаг остаётся NOT_RUN.

5. **Защитные тесты без повторного neural Train.** Дополнить существующий `SamplerTests` детерминированными компонентными случаями, сохранив старые asserts: `[5,4,0]`,K2,p.6; p1 control; K0/p-only;K1;greedy(temp0) с ThrowingRandom;draw0 при ведущих нулях;draw по разные стороны cumulative boundary;равенство logits на границеK. До реализации oracle определить отдельно, должна ли политика ties сохранять всех равных порогу (нынешний source именно так делает: `>= threshold`,Sampler48–53). Не утверждать support≤K для ties по названию метода. Для learned evidence использовать сохранённые четыре after logits; сравнить независимый float64 oracle и actual native selected IDs на заранее заданных draws0,.1,.5,.9,`1−2^-24`, без seed search и без вызовов модели. Новые assertions до исполнения — proposals, не PASS.

6. **Негативный контроль пользы.** На component logits `[5,4,0]` взять независимую учебную меткуid1:K2,p.6 отсекает её в B. Это не новая модельная regression и не gold для T1; это алгебраический контроль, запрещающий scorer объявлять уменьшение support автоматическим улучшением ответа. На savedE1 явно считать gold-in-support=false у обеих рук.

7. **Сохранение и ресурсы отдельно.** Нельзя подменять reuse хешем:8consumer вызовов выше должны прочитать реальный savedS1 и его tokenizer. Для sampler performance отдельный заранее фиксированный бюджет на сохранённых logits,0Forward/0updates; одинаковые inputs,порядокAB/BA,одно число warmups/repeats,wall/CPU/allocation и process peak в корректной области. Не выводить delta из общей стоимости training187,4317мс/10941712bytes или пика всего host. Выигрыш/ухудшение сейчас UNKNOWN. Число repeats,deadline и допустимые ресурсные пороги автор полного плана назначает до исполнения; настоящее evidence-файл их не исполняет и не добавляет скрытый benchmark.

8. **Дефект draw0 отдельным ремонтным пунктом.** Не смешивать его с composition и не приписывать B. Предлагаемый scope после разрешения: `09.Inference/Sampler.cs` и существующий `Tests/SamplerTests.cs`; invariant «никогда не выбирать zero-mass ID», корректные полуоткрытые интервалы draw и fallback на допустимую массу. Сначала сохранить статический контрпример,потом отдельное expected/actual evidence. Не переписать expected на нынешний ошибочныйid0 и не удалить test под новый код. Исторический B/raw не редактировать.

9. **EOS и длинный rollout — отдельное назначение.** Однотокенный восстановительный шаг не доказывает end-of-answer correctness. Если полный перенос потребует его, автор заранее выделяет конечные cases/budget,правильный EOS identity и expected stop reason; сначала переиспользует имеющиеся generator/cache tests. Не расширять P0801 на лету и не объединять предметные темы по имениD01.

## 7. Охват, ограничения и прочтение правил

Прочитаны первая экранная часть canonical AGENTS/CLAUDE, RTK.md, первые относящиеся к authority/readout фрагменты семи проектных skills: architect-behavior-classifier,planning-session-startup,plan-review,generated-code-review,output-readability-gate,human-readable-architecture-audit,task-slice-golden-rule; human-communication-GLOSSARY прочитан целиком. Это **targeted read**, не полная загрузка 8640/4282строк governance или всей библиотекиskills. Действующий research work order ограничил Git действия чтением; historical push/build предписания не исполнялись.

Полностью прочитаны Sampler,bridge,B,filter diagnostics,первичный report,три существующих test sources; Program прочитан по полным относящимся определениям/блокам восстановления,readout,учебного участка,sampler,validity и diff-builder, а не целиком932строки. Native TextGenerator прочитан по consumer/constructor/receipt/stop/capture блокам; Trainer — определение текущего supervised step. Все before/after logits и JSON-объекты прочитаны машинно при разборе,пользователю выведены отобранные строки и поля; не запускалась независимая численная реализация модели или фильтров. Прочитаны также оба csproj: ссылки направлены на настоящие продуктовые проекты; прочитаны сохранённые build.log и run.log. В build.log:17 записано `Build succeeded`, в run.log:49–52 — старый COMPLETED и exit0; это не запуск в текущем аудите. Счётчики training/Forward являются сохранёнными показаниями старого host; новый процесс исполнения не наблюдался.

Поиски исходников выполнены через `rg`; первый поиск required-changes с ошибочным `docs/plans/required-changes.md` вернул ошибку отсутствия пути, после чего повторён по настоящему `docs/plans/replanning-possible-diffs/required-changes.md`. Ошибка пути не интерпретировалась как отсутствие кандидата. Context-mode в текущей среде не использовался; после задержек чтение выполнялось Node fs и ограниченным shell output. Не заявляется экономия токенов/денег или полный raw-аудит всей очереди.

```text
claim_scope: один адресный P08 source/raw audit и предложения ремонта
evidence_scope: сохранённый P0801 attempt-0001, native Sampler/TextGenerator/Trainer, три существующих test sources, локальный commit/stat и hashes
count_claimed: один пакет аудита; ноль новых runtime-проверок
independent_evidence_count: не серия циклов; отдельный внешний review этого пакета ещё не получен
subagent_id_or_NONE: /root/p08_transfer_audit
downgrade_needed: product transfer NOT_APPLIED; A/B generator route NOT_RUN; sampler resources UNKNOWN; другие домены/полный каталог NOT_AUDITED
```
