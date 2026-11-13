# Независимая проверка KNN-пакета: границы структурного донора

Дата: 19.10.2026. Проверяющий: агент `units_restore`, отдельно от автора KNN-пакета. Режим — чтение первичных исходников и повтор арифметической Python-трассы; .NET, новые предметные опыты, обучение и вызовы teacher/judge не выполнялись.

**Вердикт: PASS_BOUNDED для трёх конкретных механизмов и приведённых результатов исходной трассы.** Прямых опровержений контрпримеров не найдено. Есть одно исправление воспроизводимости средней важности и два небольших уточнения формулировок/миниопытов. Эти замечания не превращают подтверждённое свойство исходника в доказанное ухудшение качества обучения. Полный ordinary serving trace и фактическое влияние на RP01 остаются неустановленными, как и заявляет автор.

## 1. Зафиксированный объект проверки

| Объект | SHA256 |
|---|---|
| `agents/knn/NEW-STRUCTURAL-KNN-DONOR-BOUNDARIES.md` | `9f31b0e21ce43cc8401e9eb19d84d1b5c57a868e8a06816a514bde5e1e2b0958` |
| `agents/knn/knn_structural_proxy.py` | `c7c5090f272b8cf43c9e182551a52d250b739ff8b7c8a390f5830079bd3896f4` |
| Исходный и независимо повторённый `knn_structural_proxy_results.json` | `2195eb46c222419be095cfff260d721969ac80848164042734a88208b2b99561` |
| Мой замороженный `agents/units/units-findings.md` | `463c1563c7945a7fdd6952e5f3df4de1c1379cfebd0828837926ca9a7e3925a9` |

Проверенный current — `358af351e22a92a3f7938d1f9221d5aeb7ddb053`, clean — `e346b88a3cc1fcce48aefa626bfc7b00509eefd6`. KNN draft, его исходный скрипт/receipt, оба реестра и remote не изменялись. Повтор выполнялся с отдельной точной копией скрипта в `agents/units`; результат записан только туда.

## 2. Замечания и точные исправления

### CORR-R1 — средняя важность: отдельный CLI не доказывает идентичность checkout указанным refs

В `knn_structural_proxy.py:86–104` проверяются несколько строк-маркеров и равенство clean/current; `:146` записывает фиксированные refs. Если пользователь передаст два одинаково изменённых checkout, эта проверка может пройти и прикрепить к ним прежние SHA. Это замечание к переносимой воспроизводимости, **не к уже полученным числам**: независимо пересчитаны Git blob SHA и SHA256 всех 26 файлов manifest, все совпали с записанными значениями; у 21 файла также присутствует совпадающий directory blob SHA. Ошибок локальных source bytes не найдено.

**Исправление:** перед расчётом проверять ожидаемый Git blob SHA для четырёх используемых пар файлов по immutable manifest; не ограничиваться взаимным равенством веток. Минимальные ожидаемые blobs: `PatternClassifier.cs` — `4d7a4c6ba2c9bb84bc915adebf7bd818135ebeba`; project-12 `PatternReorderer.cs` — `5f66485e0707fab6eede700525b9cfcceab9336e`; `MethodKnn.cs` — `e0ecebb6d513259a0930e8706737254f926197cd`; experimental helper — `560ad3dec3b96caed73e9b978341012da9a22e9d`. При несовпадении завершаться до формирования receipt с pinned refs. Нативный запуск для этого не нужен.

### CORR-R2 — низкая важность: сузить невозможность обучения While/Do до меток соседей

Фраза draft `:90` «больше данных с нынешней разметкой не научат отвечать Loop для while/do» слишком широка вне её ближайшего контекста. `Predict` предсказывает **следующий** метод и голосует по `CanonicalType(pair.Next.KindBag)`; запрос с `While` сам по себе не запрещает получить `Loop` от соседей с `For`. [Точный consumer][predict].

**Исправление:** «добавление пар, у которых `Next.KindBag` содержит только While/Do как вид цикла, не создаёт из этих пар голосов Loop; они продолжают голосовать Mixed». Условие о согласованном широком значении Loop и возможном намеренно узком legacy-контракте уже написано правильно. Менять эту гипотезу на безусловный баг нельзя.

### CORR-R3 — низкая важность: явно закончить маленькие учебные карточки N-K2/N-K3

N-K1 `:54` задаёт два разрешённых урока, четыре новых задания и A/B до/после — 16 ответов. N-K2 содержит корректный четырёхстрочный механический preflight и отдельное вмешательство, но явно не повторяет размер и состав учебного опыта. N-K3 содержит пять прямых API-контролей и frozen память, но не завершает отдельную карточку двух уроков/четырёх новых входов. Пять API-контролей не проблема; их нужно отделить от последующего опыта обучения.

**Исправление при интеграции:** для N-K2 явно сослаться на размер N-K1, указав два урока с новыми словами/структурами и четыре непересекающихся проверочных случая: два многословных подлежащих и два однословных контроля. Для N-K3 оставить пять API-входов как preflight; затем два добавляемых урока и четыре новых запроса на одной ранее разрешённой frozen памяти, уже удовлетворяющей порогу `TrainingSize >= 4`. Проверять `PredictedNextMethodType`, а не приписывать этому полю уже найденного генератора документации. Не создавать дополнительные пары только ради искусственного обхода порога. DPO-пары для реального учебного шага создаёт внешний teacher, а CODEX с Astra 6 UltraCode выбирает и оценивает их; диагностические таблицы не являются такими парами.

## 3. Что независимо подтверждено первичными исходниками

| Проверка | Решение и основание |
|---|---|
| E27 actual owner | **PASS.** Runner находится в `ExperimentsV2`, вызывает `PatternReorderer.Reorder`; одноимённый helper тоже в `ExperimentsV2`. Global alias относится к `SentencePatternClassifier = SemanticCompiler.PatternClassifier`, а не к `PatternReorderer`. Ошибочной подмены E27 проектом 12 в draft нет. [Runner][e27-runner], [helper][helper], [aliases][aliases]. |
| E25 обучение и E27 готовая метка | **PASS.** E25 вызывает `Train(train)` и `Predict` на отдельном `val`; его learner хранит примеры и сравнивает направленные POS-биграммы. E27 получает `SourcePattern` из семи фиксированных кейсов и E25 не обучает. Исторические 34,38%→87,5% и 2/7→7/7, дополнительное IsSVO 4/7 соответствуют прочитанным receipts. Это не новые измерения. [E25 runner][e25-runner], [learner][learner], [E25 receipt][e25-receipt], [E27 receipt][e27-receipt]. |
| N-K1 удаление PRON | **PASS.** Фильтр удаляет все токены с `PosTag == 3`; `Send you it to her` даёт `Send to`, `Tell you him about it` даёт `Tell about`. Донорский `Add you check to method` и отдельный `IMPERATIVE_V` контроль корректны. Примеры являются размеченным промежуточным представлением, не утверждением о грамматичности сырого source. [Правило][pronoun], [донорский контроль][e27-cases]. |
| N-K2 движение одного слова | **PASS.** `FindIndex(1, ...)`, `RemoveAt`, `Insert(0, ...)` переносят только noun/pronoun. `reads old server file` превращается в `server reads old file`; determiner-пример аналогичен. Обе ошибочные строки удовлетворяют ограниченному IsSVO. Автор отделяет новые связи определения от уже известной слабости метрики. [VSO][vso], [IsSVO][svo]. |
| N-K3 While/Do | **PASS_BOUNDED.** `WhileStatement→While`; суффиксный fallback даёт `DoStatement→Do`. Оба входят в KindBag, но отсутствуют в Loop branch. Публичный результат facade действительно включает предсказание при C# + включённом MethodKnn + ≥4 пары. Это условный недостаток онтологии следующего метода, не универсальное правило для программирования и не доказательство ущерба free-answer. [Extractor][extractor], [label][label], [facade][facade]. |
| clean/current | **PASS.** Для заявленных идентичных файлов байты действительно совпадают; возвращение этих файлов само по себе не исправит указанные ограничения. Они унаследованы от успешного донора, а не вновь созданы RP01. |
| Область профилей | **PASS_BOUNDED.** N-K1/N-K2 относятся к переводу/перестановке при английской SVO-цели и требуют источника ролей/spans. Перенос на EN→RU, SQL, C# или иной порядок языка явно ограничен. Дальнейшее уточнение реального направления перевода необходимо только в конкретном маленьком опыте. |

Повтор Python-трассы дал ровно прежние байты JSON: два расхождения среди четырёх pronoun cases; два среди четырёх constituent cases; обе последние ошибочные строки проходят IsSVO; два изменённых label среди пяти KindBag. Скрипт — транскрипция нужных ветвей, не полный эквивалент C# для любых inputs: например, иные patterns здесь намеренно не моделируются. В рамках выбранных cases формулы совпадают. Код proposed span-варианта показывает диагностическую альтернативу с заранее указанным диапазоном; он не проверяет parser и не реализует полный production-контракт `roles_unknown`.

## 4. Новизна относительно двух существующих реестров

Независимый ограниченный поиск повторён по **689 Markdown**. Расширенные запросы по удалению объектных местоимений и While/Do→Loop/Mixed не нашли предшествующего точного механизма. По constituent/именным группам найдено одно общее упоминание domain/role-aware rerank в planning package 30; оно не описывает single-token move после правильного VSO. Прочитаны ближайшие N26, Core33, N38 expression-body, N38 composite-label и релевантные секции P16.

Семантический вывод: N-K1/N-K2 — новые точные контрпримеры к известной семье переноса E27, а не новая общая идея «порядок/роли важны». N-K3 — конкретное расширение проверки label coverage с условным downstream-контрактом; не повтор expression-body и не multi-label Guard+Async. Глобальная уникальность во всех документах/ветках не доказана. Текстовый поиск не заменяет полного semantic review; draft правильно ограничивает собственное заявление о новизне.

Сопутствующие записи проверки: `review-knn-source-verification.json`, `review-knn-novelty.json`, точная копия скрипта `knn_structural_proxy_review_copy.py` и независимо полученный `knn_structural_proxy_results.json` находятся в этой папке. Это свидетельства review, не новые результаты обучения.

[e27-runner]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E27PatternReorderer.cs#L6-L44
[helper]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L6-L16
[aliases]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/GlobalUsings.cs#L6-L11
[e25-runner]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L20-L48
[learner]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/12.SemanticCompiler/Translation/PatternClassifier.cs#L34-L86
[e25-receipt]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json#L2-L18
[e27-receipt]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/results/E27-PATTERN-REORDERER/results/run-2026-05-01-001.json#L2-L18
[pronoun]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L51-L53
[e27-cases]: https://github.com/lubochka/llm_mvp_core/blob/e346b88a3cc1fcce48aefa626bfc7b00509eefd6/35.ExperimentsV2/Experiments/E27PatternReorderer.cs#L84-L100
[vso]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L33-L45
[svo]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/35.ExperimentsV2/Infrastructure/PatternReorderer.cs#L78-L84
[extractor]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/Prediction/MethodKnn.cs#L61-L104
[label]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/Prediction/MethodKnn.cs#L171-L184
[predict]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/Prediction/MethodKnn.cs#L133-L146
[facade]: https://github.com/lubochka/llm_mvp_core/blob/358af351e22a92a3f7938d1f9221d5aeb7ddb053/12.SemanticCompiler/SemanticCompilerFacade.cs#L313-L323
