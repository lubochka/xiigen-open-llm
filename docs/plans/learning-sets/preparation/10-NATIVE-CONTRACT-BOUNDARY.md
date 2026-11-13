# Нативные входы DPO: граница подготовки и обучения

## Повторная сверка по результату ce76f597

Для [разбора конкретной генерации](22-DPO-LEARNING-TASK-AND-CE76.md) 25.10.2026 повторно прочитаны те же четыре файла на `llm_mvp_core@ce76f597a81221abe0224d4d09d5a77d99e0b47e`:

| Файл | Git blob на ce76 | Подтверждение |
|---|---|---|
| `11.DPO/DpoDataset.cs` | `008da881258dd18d4975058d2f13ea9a5a35ac66` | Native `Prompt/Chosen/Rejected` и отдельный admission |
| `11.DPO/Models/DpoDatasetRow.cs` | `f2552ccf269f918f9d16cdd982c344f37465c65c` | Пара и admission хранятся раздельно |
| `11.DPO/Models/DpoTrainerPairAdmission.cs` | `9da211a523ae7428555c9deae52cdeac90e678f9` | Авторство обеих сторон нельзя выдумывать ради допуска |
| `training-data/dpo/README.md` | `aaebabe17b1db6367e6ec9f954c83011f838ebfc` | Отдельные native authoring и маршрутная память |

Эти blobs совпадают с уже зафиксированной ниже ограниченной сверкой. Получатель богатой metadata не появляется от её наличия в строке. Данные создаются **для будущего обучения**, а CREATE_ONLY ограничивает действия нынешнего генератора. Сам loader/adapter и алгоритмы обучения в этой документной правке не изменены. Реальные строки M001 через LFS не получены; эта сверка не удостоверяет их содержание.

Первичные файлы: [DpoDataset](https://github.com/lubochka/llm_mvp_core/blob/ce76f597a81221abe0224d4d09d5a77d99e0b47e/11.DPO/DpoDataset.cs), [DpoDatasetRow](https://github.com/lubochka/llm_mvp_core/blob/ce76f597a81221abe0224d4d09d5a77d99e0b47e/11.DPO/Models/DpoDatasetRow.cs), [Admission](https://github.com/lubochka/llm_mvp_core/blob/ce76f597a81221abe0224d4d09d5a77d99e0b47e/11.DPO/Models/DpoTrainerPairAdmission.cs), [native README](https://github.com/lubochka/llm_mvp_core/blob/ce76f597a81221abe0224d4d09d5a77d99e0b47e/training-data/dpo/README.md).

## Свежая ограниченная сверка входа на 25.10.2026

Для нынешнего дополнения повторно получены и прочитаны четыре файла на core commit `6726b27b1e6b34d538bc3f1010781b4d2944bbc9`:

| Файл | Git blob | Подтверждённая граница |
|---|---|---|
| [11.DPO/DpoDataset.cs](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/11.DPO/DpoDataset.cs) | `008da881258dd18d4975058d2f13ea9a5a35ac66` | Native поля `Prompt`, `Chosen`, `Rejected`; admission читается отдельно. Lowercase interchange не объявляется нативным входом. |
| [11.DPO/Models/DpoDatasetRow.cs](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/11.DPO/Models/DpoDatasetRow.cs) | `f2552ccf269f918f9d16cdd982c344f37465c65c` | Тексты пары и отдельный Admission; произвольные metadata не становятся обучаемым текстом. |
| [11.DPO/Models/DpoTrainerPairAdmission.cs](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/11.DPO/Models/DpoTrainerPairAdmission.cs) | `9da211a523ae7428555c9deae52cdeac90e678f9` | Реальные категории авторства и отказ для PaidExternalModel; фактическое авторство не переименовывается ради допуска. |
| [training-data/dpo/README.md](https://github.com/lubochka/llm_mvp_core/blob/6726b27b1e6b34d538bc3f1010781b4d2944bbc9/training-data/dpo/README.md) | `aaebabe17b1db6367e6ec9f954c83011f838ebfc` | Канонический набор имеет manifest/pairs; описание оркестрации отдельно указывает механизм памяти/маршрутов. |

Это проверка существующего входа, не реализация нового adapter и не обучение. Нет причины останавливать создание кандидатов: они сохраняются с честным provenance. Вопрос фактического native admission решается при будущем binding выбранного получателя, без обхода проверок. Полная историческая сверка остальных runner/checkpoint файлов ниже остаётся закреплённой на прежнем commit; она не объявлена заново выполненной на свежем head.

## Сохранённая полная сверка предыдущего пакета

Проверено 25 октября 2026. Код: `lubochka/llm_mvp_core`, ветка `codex/dpo-implementation`, commit **51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7**. Ссылки ниже закреплены на этом commit. Если core изменился, binding проверяется заново по конкретному diff, без автоматической смены данных или модели.

Текущая библиотека подготавливает задания, reference seeds и проверяемые кандидаты. Она не утверждает, что все новые темы подключены к нативному trainer. Команды `tools/sets.py` относятся к подготовке данных. Новый consumer получает статус `ADAPTER_IMPLEMENTATION_REQUIRED`, пока его существующий вход не проверен или отдельный экспериментальный adapter не реализован.

## 1. Три разных входных формата

| Получатель | Реальный контракт | Что должен сделать будущий adapter |
|---|---|---|
| `DpoDataset.Load` | `Prompt`, `Chosen`, `Rejected` с точным регистром. Если admission присутствует, обязательны все три поля: `chosenAuthor`, `rejectedAuthor`, `admissionIssuerName`. | Отобразить lowercase поля подготовки в native поля, сохранить точные тексты, установить проверенное фактическое авторство и пройти нормальный допуск. |
| `CodeDocumentationSimulationDpoExperiment.Inspect` | Специальный lowercase simulation JSONL: `id,dataset_version,domain,pair_kind,provenance,prompt,chosen,rejected,metadata`; узкие значения domain/pair_kind, ограничения hashes и числа пар. | Не использовать как общий импорт китайских ответов: этот путь предназначен для конкретных программно созданных simulation fixtures. |
| Repository-owned authoring/resolver/sealer | `training-data/dpo/<domain>/<design-pattern>/<version>/manifest.json` и `pairs/<pair-id>.dpo-pair.json`, паспорт запроса, hashes, сохранение и повторное чтение. | Создать нативный набор по настоящему authoring contract, не подменять его произвольным release.json подготовки. |

Источники: [DpoDataset, строки 19–39 и 77–180](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/11.DPO/DpoDataset.cs#L19-L180), [DpoDatasetRow, строки 8–27](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/11.DPO/Models/DpoDatasetRow.cs#L8-L27), [simulation importer, строки 39–78](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L39-L78), [канонические наборы, строки 1–33](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/training-data/dpo/README.md#L1-L33).

Legacy пара без admission загружается в контейнер, но получает `Admission=null`. Это не train-ready. Native `DpoDataset` проверяет регистр/дубликаты известных полей; он не является валидатором всей богатой схемы подготовки. Дополнительные metadata не становятся обучаемым текстом автоматически.

Обменный экспорт подготовки сохраняет lowercase `prompt/chosen/rejected` и полное происхождение отдельно. Он остаётся `INTERCHANGE_FORMAT_ONLY`. Не добавлять rationale, error_tag, gold, route metadata или источник к model-facing ответу ради упрощения импорта.

## 2. Китайская модель, симуляция и авторство

Сюжет может быть вымышленной симуляцией, а его обе стороны — ответами модели. Это разные свойства. Нельзя назвать ответ модели `DeterministicGenerator` только потому, что задача симуляционная, и нельзя назначить `OpenTeacher` только по стране происхождения модели или интерфейсу Kiro.

Native enum содержит `User`, `LocalModel`, `OpenTeacher`, `DeterministicGenerator`, `PaidExternalModel`. Указание платной внешней модели на любой стороне ведёт к специальному отказу `DPO_PAIR_ADMISSION_PAID_MODEL_SIDE`; паспорт сам по себе не обещает принятия trainer. [Исходник типов и fingerprint](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/11.DPO/Models/DpoTrainerPairAdmission.cs#L14-L152).

Для генерации записать фактический Kiro selector, наблюдаемую точную model identity либо `model_identity_observed=false`, поверхность исполнения и run ID. Отсутствие точного provider identity не мешает сохранить кандидаты, но не позволяет выдумать native admission issuer. Исполнитель сохраняет текущую модель сессии без override. Установленный пользовательский выбор модели не заменяется самостоятельно.

Reference seeds этой библиотеки — исходный материал для постановки и проверки, а не фактические ответы китайской модели. Не переименовывать их в сгенерированный корпус. Native происхождение обеих сторон проверяется на стадии adapter; никакая страна модели, новый ID или формат JSON не меняет фактическое авторство.

## 3. Почему старый Rp01SimulationDpo нельзя запускать на любом новом наборе

[Реальный Program.cs](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/tools/Rp01SimulationDpo/Program.cs#L23-L130) имеет команды `inspect`, `calibrate`, `train`, `evaluate` и отдельные ENRU режимы. Это обнаруженные команды конкретного исторического опыта, не универсальный runner новых тем.

Его действительные ограничения:

- C# calibration — одна пара; основной режим — 500.
- Есть зафиксированные разрешённые hashes входных наборов.
- ENRU принимает конкретный набор, batch10 и 50 новых updates.
- C# начинает с закреплённого B0; ENRU продолжает специальный C50.
- Importer ожидает `domain=csharp,pair_kind=CODE_TO_DOC` либо `domain=en-ru,pair_kind=TRANSLATION`.
- Программное simulation authoring назначает обеим сторонам `DeterministicGenerator`.
- Уже выданный dataset/arm нельзя повторить обходом через новый run ID; сначала выясняется состояние существующего журнала.

Подтверждение: [importer](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L39-L78), [параметры и replay, строки 224–234](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L224-L234).

Поэтому в этой библиотеке нет выдуманной команды `train --dataset <любой-new-domain>`. Команды generation/validate/export конкретны; native learning новых topics требует отдельного binding. Это не причина останавливать текущую подготовку материалов.

## 4. Сохранённое состояние и повторное использование

В коде существует специальная реализация продолжения C50: `training-artifacts/experiments/code-documentation-simulation/simulation-csharp-main500-20261015-01/`. Она закрепляет weights, optimizer, RNG, state и receipts точными hashes/размерами. Комментарий в исходнике прямо указывает, что это не общий resume API. [C50 source, строки 138–158](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/14.ContinuousLearning/Training/SimulationEnRuContinuation.cs#L138-L158).

Это исторический пример полного состояния, **не назначение B1 всем новым доменам**. Совместимость зависит от native topology, tokenizer и token IDs, LoRA, цели обучения, накопленной памяти и происхождения набора.

Для continuation надо восстановить веса, optimizer slots/step, RNG, порядок данных и прочие фактические state artifacts получателя. Если берутся только веса с новым optimizer, это отдельный режим; не называть его точным continuation. Native readback в действующем опыте проверяет свежий независимый объект, base+LoRA keys, optimizer и RNG, неизменность frozen reference и соответствие persisted/live состояния. [Readback, строки 192–207](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L192-L207).

`RepositoryOwnedDpoRandomStateFile` требует имя, построенное его `BuildFileName(step)`; не передавать ему `after-sft.rng.json`. Разные состояния размещать в разных каталогах, сохранять родные имена. [Writer](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs#L70-L112), [проверка имени](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/14.ContinuousLearning/Training/RepositoryOwnedDpoRandomStateFile.cs#L417-L430).

[33-shared-learned-starts.md](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/docs/plans/replanning-possible-diffs/experimets/33-shared-learned-starts.md) уже задаёт будущие producers/consumers и прямо сообщает, что общий новый runtime и snapshots там не созданы. Его будущие `Prepare.ps1` и флаги нельзя выдавать за существующий инструмент. Путь B1 в [05](05-LEARNING-AND-CHECKPOINTS.md) тоже назначенный будущий адрес, пока нет квалифицированного COMPLETE bundle.

## 5. Механизм получателя должен быть назван точно

В текущем продолжении оркестрации `OrchestrationPlanMachine` обновляет маршрутные метки и память ближайших примеров. README прямо отделяет это от градиентного DPO общей нейросети. [Описание оркестрации, строки 78–114](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/training-data/dpo/README.md#L78-L114).

Поэтому binding для каждого objective содержит конкретный `learning_mechanism`, например gradient DPO, SFT или memory/kNN update, и код consumer. Наличие слова DPO в имени файла не доказывает вызов optimizer. Router обучается оркестрации, даже когда маршрутизирует запросы о переводе или документации; примеры конечных доменов внутри routing-задачи не являются обучением всех этих доменов.

[Контракт состава данных](https://github.com/lubochka/llm_mvp_core/blob/51c19ba2de32bbd6a5144b8bd215d6e0ba1c77d7/docs/plans/replanning-possible-diffs/experimets/32-training-set-and-inference-contract.md) различает repeats/unique cases, acquisition/discrimination/generalization, запрещает EXAM-driven настройку и causal вывод из несопоставимых WT01/WM01. Все контроли назначаются до результатов. Ненулевые updates и confidence не доказывают, что ответы различают нужный признак.

## 6. Конкретное содержание следующего binding-задания

Для одного `topic_id/profile_id/objective_id` будущий исполнитель получает файл `training-binding.json` рядом с run. До реализации его статус — `ADAPTER_IMPLEMENTATION_REQUIRED`, точные неизвестные поля — `null` с причиной, а не guessed path:

| Поле | Что зафиксировать |
|---|---|
| source identity | preparation release SHA, native sealed dataset path/hash, точная карта prompt/chosen/rejected и provenance |
| recipient | существующий native class/method, assembly и commit, механизм обучения |
| execution | проверенная существующая команда или отдельно реализованный experiment adapter, ограниченный автором budget |
| state | полный совместимый B1 path/hash, родные codecs, continuation/new-optimizer режим |
| isolation | один objective; один split на входе генерации; EXAM из других семейств не доступен TRAIN generator |
| validation | до/после на реальных outputs, qualification, fresh restore и все регрессии |
| lifecycle | один собственный process handle, штатный wait/resume до terminal, readback receipt; без нового запуска вместо ожидания |
| result | русский отчёт с измеренными counters/outputs и candidate; без автоматической интеграции production |

В этой сессии такой adapter не запускается. Никакое место этого документа не объявляет измеренный учебный успех. JSON/компиляция/oracle проверяют вход или выход обучения; результат обучения фиксируется только при фактическом уроке и до/после. Если результат нулевой до и после, ошибка постановки не переименовывается в удачный опыт по изменению margin. При валидном уроке любое измеренное улучшение полезного показателя — успех по данному показателю и домену, с ухудшениями остальных показателей рядом.

