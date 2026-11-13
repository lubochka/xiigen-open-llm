# Несколько ошибок на один вопрос и границы файлов E09

16 октября2026. READ_ONLY_RESEARCH. Источники помощников N32 сопоставлены с текущим кодом. Скрипт и JSON прочитаны как чужие материалы; native Inspect, Roslyn, обучение и proxy здесь не запускались.

**Два разных результата.** Simulation importer действительно запрещает повтор prompt, даже если rejected другой. Исторический E09 действительно создаёт переходы между файлами. Оба условия подтверждаются исходниками, но их влияние на качество ещё предстоит измерять в отдельной разрешённой фазе. Они не объясняют автоматически нулевое улучшение RP01.

## 1. Importer запрещает несколько контрастов одного вопроса

Первичный [CodeDocumentationSimulationDpoExperiment.Inspect,39–75](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:39) проверяет отдельно уникальные id, prompt и content fingerprint. Повтор точного prompt приводит к отказу всего input. Никакие строки молча не выбрасываются.

Таким образом `(P,chosen,rejected1)` и `(P,chosen,rejected2)` с разными rejected и id не проходят вместе. Это отдельное ограничение от защиты от полного дубля. Комментарий строки51 объясняет намерение предотвращать перевес повторённых записей. Однако разные предметные ошибки на одном вопросе могут быть самостоятельными учебными контрастами; желательность такого взвешивания — вопрос дизайна будущего набора, а не основание молча снять guard.

Настоящий caller есть: [offline CLI,77](C:/Projects/llm_mvp_core/tools/Rp01SimulationDpo/Program.cs:77), затем [Run,106–119](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:106) снова использует Inspect до подготовки exact material. Этот путь относится к bounded simulation исследованию, а не ко всем production DPO authorities.

**Важное уточнение границы EN→RU.** В [44–45](C:/Projects/llm_mvp_core/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs:44) EN→RU дополнительно привязан к одному DatasetSha256 и ровно500 строкам. Поэтому маленький двухстрочный EN→RU fixture через данный метод откажет раньше проверки prompt. Предложенные помощником native fixtures правильно используют C# domain; перенос их на EN→RU без учёта этой границы не изолирует нужное условие.

Нынешние C#500/ENRU500 уже прошли этот importer. Значит, найденный guard не является тихой потерей строк внутри состоявшегося обучения. Возможный эффект — ограничение того, какие наборы вообще можно выразить и принять. Нет данных, что исключённые таким образом multi-negative пары реально готовились для прежнего опыта.

Будущая проверка, не запуск: сначала три валидных C# input — разные negatives при одинаковом prompt; разные prompt; точный дубль. Только после разрешения отдельного учебного опыта сравнивать два разных вида ошибки с повторением одного вида при одинаковом числе updates, весе вопроса, chosen, исходном состоянии и независимом экзамене. **Повтор в control — специально определённое вмешательство будущего опыта, не разрешение переиспользовать уже потреблённую текущую пару.** Допуск данных/число optimizer steps и предметное качество — разные результаты.

## 2. E09 учит переходы через границы файлов

Первичный [E09StructuralKnn,102–126](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E09StructuralKnn.cs:102) складывает AST nodes всех files в одну последовательность. [Train,30–47 и64–67](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E09StructuralKnn.cs:30) строит соседние пары без проверки границы. Baseline делает то же; validation [50–77](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E09StructuralKnn.cs:50) тоже считает соседние targets по объединённому списку. Переход «последний узел файлаA → первый узел файлаB» не является связью внутри AST одного файла.

**Поправка к формулировке помощника:** FileId не потерян полностью. Он есть внутри `TokenWithVector.Vector`: [StructuralVector.FileId,23](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:23), [Extract,93](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:93), и E09 передаёт fileId в Extract. Граница доступна, но при создании пар не используется. DistanceTo [27–35](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:27) также не использует FileId. Последнее само по себе не ошибка: исключение произвольного ID из расстояния может помогать переносу на новые файлы. Для правильной training-target boundary не требуется добавлять ID как семантическую координату.

Чужой [JS proxy](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/32-MICRO-ADMISSION-E09-SOURCE-PROXY.mjs>) и [receipt](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/32-MICRO-ADMISSION-E09-SOURCE-PROXY-RESULT.json>) используют три искусственных файла по4 узла. Из9 train nodes получаются8 переходов,2 межфайловых. Это согласованная арифметика данного примера, **не измеренные25% плохих пар исторического C# корпуса**. Proxy не вызывает Roslyn, StructuralKnnPredictor или Inspect и не проверяет качество пользователя. Записанные7 assertions — утверждение чужого receipt, не наши выполненные тесты.

Положительное контрдоказательство против широкого переноса проблемы: production [StructuralKnnPredictor.TrainFromSource,123–141](C:/Projects/llm_mvp_core/12.SemanticCompiler/Prediction/StructuralKnn.cs:123) строит пары в пределах одного переданного source. Межфайловая склейка находится в experiment E09, а не автоматически во всех consumers StructuralKnn. Различие label/tie/opt-in переноса уже разобрано в [38](38-e09-transfer-labels-and-opt-in-consumer.md), не считается новой находкой.

## 3. Какой source фактически читает E09

`csharpSource` [18–26](C:/Projects/llm_mvp_core/35.ExperimentsV2/Experiments/E09StructuralKnn.cs:18) используется только для проверки непустоты; AST строится из `files[i].Text`. [ExperimentsV2Dispatcher,48–53](C:/Projects/llm_mvp_core/35.ExperimentsV2/ExperimentsV2Dispatcher.cs:48) передаёт оба аргумента. Поэтому смена одного непустого csharpSource при неизменных files не меняет этот расчёт метрик. Время может отличаться по внешним причинам.

Это контракт входа, не обязательно дефект: если files — канонический корпус, поведение может быть намеренным. Полная цепочка формирования обоих аргументов CLI в этом пакете не прослежена. Два предположенных пути Program.cs/ExperimentV2Runner.cs отсутствовали; отрицательный вывод по всему репозиторию из этой ошибки не сделан. Ранее [38](38-e09-transfer-labels-and-opt-in-consumer.md) отдельно устанавливает production consumer; его не подменяет API E09.

## 4. Поправка к будущему эксперименту помощника

[MICRO-N32](<C:/Users/lubao/Documents/xiigen/llm mvp/design to code session/replanning/possibleDifs/32-MICRO-ONE-FACTOR-LEARNING-CONTRAST-AND-E09-2026-10-16.md>) правильно предлагает менять только cross-file training pairs и оценивать обе ветки на одних внутрифайловых случаях. При этом предложенная отдельная перестановка файлов **не будет чистой проверкой порядка**, если после неё заново взять первые80%: одновременно меняется train/validation membership. Для такого контраста нужно заранее закрепить membership и target каждой пары по `(file,span)` и сохранить те же записи в обоих плечах.

Перестановка также может менять порядок равных соседей при `OrderBy(distance).Take(k)`; эта tie-зависимость должна быть отдельным наблюдением. Нельзя одновременно исправлять boundary, local kindMap, target IDs, split и tie-policy, а затем приписывать результат одному исправлению.

Для первого будущего bounded опыта достаточно: одинаковый frozen corpus/split, A со стыковыми training pairs, B без них; одна fixed внутрифайловая val-панель, per-file denominators, реальные predictions и стоимость. Изменение числа записей — прямое следствие удаления нежелательных переходов, которое следует показать. Сохранять исторические положительные E09 receipts; matched проверка уточнит их механизм, не перепишет прошлое.

## Источники и предел

Прочитаны N32-A, N32-B, общий N32 register, микро-протокол, весь JS и JSON. Основные source-фрагменты перечислены выше. Core сменился внешним действием с79082340f01b на32ac6e741ce4; read-only diff этих HEAD содержит только исследовательские Markdown, не рассматриваемые product-файлы. Родитель commit/push не делал. Уникальность этих гипотез среди всех файлов не заявляется. Причинное влияние на качество, native поведение и будущее обучение **не проверены запуском**.
