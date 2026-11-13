# Два пред-существующих падения базового прогона — вне среза R34-FB

Это **не шум базы**. Оба падения сняты на неизменённом опубликованном коде ветки
`codex/dpo-implementation`, вершина `159368208ebee99678fad2f104e0d27490a434d9`, узким фильтром
`FullyQualifiedName~Translation|FullyQualifiedName~AgentAlgorithms`:
прошло 542, упало 2, всего 544. Я их **не чинил** — это вне назначенного среза, координатор
заводит отдельную работу. Ниже честное описание, чтобы строка работ была точной.

Дословный вывод прогона: `baseline-before-fix.stdout.txt`, квитанция `baseline-before-fix.trx`.

---

## Падение 1 — счётчик вхождений допущенного вызова обучения предпочтений

- **Полное имя проверки:**
  `LlmFromScratch.Tests.TranslationDpoTrainerBridgeApiBoundaryTests.TrainWithStepMany_HasNoUndocumentedPublicDirectTrainerMutationOverload`
- **Файл и строка ожидания:** `Tests/TranslationDpoTrainerBridgeApiBoundaryTests.cs`, строка **91**
  (сверка `Assert.Equal(2, CountOccurrences(runnerSource, "TranslationDpoTrainerBridge.TrainAdmittedWithStepMany("))`).
- **Ожидалось / фактически:** 2 / 3.
- **Что в продукте:** в файле `27.TranslationDomain/TranslationDpoTrainEvaluateRunner.cs`
  допущенный вызов стоит в трёх местах — строки **780**, **789** и **2915**; третье место
  названо `continueTraining`, то есть это путь продолжения обучения от существующего слепка.
  Запрещённый недопущенный вызов `TranslationDpoTrainerBridge.TrainWithStepMany(` по-прежнему
  встречается **0 раз**, то есть сам смысл границы не нарушен.
- **Одной фразой:** похоже на **устаревшее ожидание** — счётчик не обновили после появления
  третьего законного допущенного места (продолжение обучения), а охраняемое свойство
  «недопущенных вызовов нет» держится.

---

## Падение 2 — имя отказа при небезопасном родительском слепке модели

- **Полное имя проверки:**
  `LlmFromScratch.Tests.TranslationDpoTrainEvaluateRunnerTests.Run_RepositoryOwnedModelCheckpointUnsafeParentsFailBeforeTraining(caseName: "corrupt")`
- **Файл и строка ожидания:** `Tests/TranslationDpoTrainEvaluateRunnerTests.cs`, строка **1159**
  (сверка `Assert.Equal(expectedError, result.ErrorCode)`).
- **Ожидалось / фактически:**
  `REPOSITORY_OWNED_MODEL_CHECKPOINT_CANDIDATE_INVALID` /
  `REPOSITORY_OWNED_MODEL_CHECKPOINT_MANIFEST_INVALID`.
- **Что в продукте:** оснастка случая «corrupt» дописывает в файл весов слово `tampered`, поэтому
  отпечаток весов перестаёт совпадать с записанным в описи. Ожидаемое имя отказа объявлено в
  `14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointLoader.cs`, строка **56**;
  фактическое имя объявлено в соседнем файле
  `14.ContinuousLearning/Training/RepositoryOwnedModelCheckpointManifest.cs`, строка **140**,
  и загрузчик его не производит вовсе. То есть испорченные веса ловит **проверка целостности
  описи**, стоящая раньше разбора пригодности кандидата.
- **Важно:** сам отказ происходит. Проверка упала **только** на имени кода отказа; стоящая строкой
  выше сверка `Assert.False(result.IsSuccess)` прошла, обучение не началось, отчёт и слепок не
  созданы.
- **Одной фразой:** похоже на **устаревшее ожидание имени**, а не на пропущенный отказ, — но с
  оговоркой: называть испорченный файл весов «негодной описью» может быть и настоящей ошибкой
  классификации причины, и это решение владельца, а не моё.
