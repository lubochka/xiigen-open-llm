# FIX-KR — читатель должен узнавать ключи, которые записал обучатель

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход в выбранный перенос или owner-раздел

Обязателен [договор120](../fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

**Область:** этот вход относится к каждому выбираемому ниже FIX/TEST/R/RF/C и его собственной приёмке, а не превращает весь файл в одно поручение. Уже принятые части не переоткрывать; сначала сверить current source и сохранённые receipts именно выбранного владельца.

**Применимые риски ROUTE/FLOW:** На названном маршрутизаторе/каталоге проверить writer→обученная память→ordinary route/payload и собственные positive/negative строки; label без выбранного payload не закрывает маршрут. Exact recall, простой baseline и предметная тема раздельно. Дополнительный dynamic same-functions фактор допустим только если назначен паспортом и проверяет скорость native Train. Правильная строка Unit/FlowRef не заменяет исполненный результат: typed refs→вызванные nodes, learned edge→Retry/Stop, graph order→sink. Положительный контроль использует обученный payload, отрицательный выявляет его потерю/чужое тело; trivial baseline и exact recall показаны отдельно.

**Ожидания тестов:** собственные данные/positive/negative этой карточки и названный ordinary caller имеют приоритет над примерами120. Пример другого семейства не добавляет fixture, Train или новый API. Полный набор outer/nested Train/BPE/Grammar, readouts, restore и этапов остаётся в существующем бюджете. Структурный guard сTrain0 не объявляется новым обучением.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.
<!-- EXECUTOR-CONTEXT-120:END -->


Дата: 23.10.2026. Новый кандидат; [общий контракт](02-native-transfer-and-test-repair.md) обязателен. Не дублирует FIX-WT/WK/WC/RET/E2E: здесь иной learner, `RoutingProfileTrainer`, и иной дефект — несовпадающая нормализация записи и чтения.

## Основание и границы

Опыт [KN-ROUTER-LEARNED-KEY-01](../../../../experiments-v2/RP01-MICRO/KR01/results/report.md), сырые [A](../../../../experiments-v2/RP01-MICRO/KR01/results/A.json) / [B](../../../../experiments-v2/RP01-MICRO/KR01/results/B.json), точный [harness](../../../../experiments-v2/RP01-MICRO/KR01/A/Program.cs). Тема — оркестрация; получатель — обучаемые UnitScoringProfile, consumer `TrainableRoutingFlow.RouteAsync`. Это не нейросетевая DPO и не `OrchestrationPlanMachine`.

A: 0/4→2/4, B: 0/4→4/4; wrong route=0, отказы после обучения 2→0, профили обеих рук одинаковы,426 байт. На `C# .NET` 0→2 совпадения выученных ключей, confidence .20→.44. Только два урока и четыре близких запроса; широкое обобщение не показано. Разницу затрат обучения нельзя приписывать reader-only factor.

## Куда и что переносить

[BaselineKeywordCandidateGenerator.cs:36](../../../../26.TrainableRouter/Training/BaselineKeywordCandidateGenerator.cs) сейчас только вызывает ToLowerInvariant. [RoutingProfileTrainer.cs:93](../../../../26.TrainableRouter/Training/RoutingProfileTrainer.cs) до токенизации заменяет C#→csharp и .NET→dotnet без учёта регистра. Добавить в ScoreAsync эти две замены в том же порядке перед lower-case. Метод `NormalizeSemanticText` в текущем продукте отсутствует: не поручать исправлять несуществующий метод. Не расширять таблицу синонимов и не менять learner, score, DomainBoost, DirectionBoost, tie order и threshold.

Математическая опора: если обучение хранит N(x), то сравнение с сырым x может дать 0 совпадений даже для учебной строки. Один и тот же N на чтении возвращает доступ к уже записанному ключу. Это эквивалентность конкретных двух написаний, не семантическое доказательство общих тем. Слово `csharp` само не назначает Unit: его связь записана уроком.

## Перенос опыта в тест

Class/fixture по 02. `train.json` фиксирует T1: `C# .NET` → code, rejected docs; T2: `manual guide` → docs, rejected code. Метаданные code/executor/programming и docs/executor/documentation, confidenceFloor .20; candidate IDs code,docs; TaskSlice пуст, RequiredDirection и DomainHint null. EXAM: `C# .NET`, `.NET C#`, `csharp dotnet` → code; `manual guide` → docs. Источник строк — указанный Program.cs:13–25; gold отдельно от входа RouteAsync.

Capture: пустые profiles →4Route →1 native Train на 2 строках →4Route, по каждому arm. Сохранить profiles/rows/confidence/authority и digest. Итого 2Train/4 учебные строки/16Route; отсутствие подстановки prebuilt profiles обязательно. Нет нового warmup. Compare читает receipts: A2/4,B4/4, profiles equality, unknown-before, отсутствие неверных маршрутов. Экспериментальный BEFORE=0 здесь допустим, потому что AFTER ненулевой и содержит оба класса; 0→0 не принимать.

Три существующих guards в [DistributedModelArchitectureTests.cs](../../../../Tests/DistributedModelArchitectureTests.cs): `BaselineKeywordCandidateGenerator_UnknownProfileStillStampsBaselineAuthority`, `..._NonSaturatedDirectionFormulaRemainsExact`, `..._EqualScoresUseOrdinalUnitIdTieBreak`. Они используют готовые profiles, без Train, и сохраняются без изменения expected; одна проверка ScoreAsync на guard. Если меняются 3.01/.49 или ordinal tie — это регрессия фактора, не повод переписать test. Новый capture использует настоящую native сборку 26, не историческую упрощённую копию Flow.

## Связь с большим prompt→domain E2E

[Предыдущий FIX-E2E](../fixes-based-on-experiments/20-prompt-domain-e2e.md) уже назначил learned outer selector, typed handoff, каталог bindings, assistant и loopback API. Не добавлять второй dispatcher и не объявлять KR заменой этого плана.

В текущем [TrainableRoutingFlow.cs:38–47,70–80](../../../../26.TrainableRouter/Training/TrainableRoutingFlow.cs) результат помечен `baseline_candidate_generator`. Успех KR чинит чтение обученных profiles, но **не выдаёт ему learned authority**. В будущем E2E trace хранит отдельные `candidate_unit_id`, `candidate_authority_role`, `accepted_learned_handoff`, `actual_target_binding`, `actual_result`. Baseline candidate не разрешает вызов Unit без действующего learned handoff. Это уточнение проверки существующего пути, не новая экспериментальная гипотеза.

Для интеграции использовать уже назначенные E2E fixtures/checkpoints и бюджеты старого 20; не дообучать outer selector на двух KR строках и не смешивать их с предметными уроками перевода. На существующих E2E случаях проверить: payload передан фактическому получателю; gold/domain не пришёл от теста как hint; caller не повысил authority; unknown/несовместимый handoff дал 0target calls. По KR отдельно доказан только компонент до выбранного candidate, не API→ответ продукта. Нейронные и downstream улучшения остальных новых планов не выбирают тему за этот selector.
