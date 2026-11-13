# Наборы и сохранённое состояние для будущих продуктовых тестов

**4 ноября — новые источники данных, не готовые checkpoints.** [158](codex-158-learning-sets-transfer-coverage-2026-11-04.md) связывает выбранные `learning-sets` с владельцами переносов. Производные данные располагаются только в `learning-sets/simulations/transfer-coverage-2026-11-04/`; будущие квалифицированные старты — отдельно в `experiments-v2/RP01-MICRO/_shared/learning-sets/<profile>/v1/`. Сейчас наличие пригодного старта для этих новых пакетов не установлено. Review данных, native admission, приобретение ответа и проверка восстановления — отдельные результаты. Существующие доноры и их ремонт 155/157 этим не заменяются; source hashes и профили нового набора нельзя подставить в старый manifest.

<!-- POSTQ011-2026-11-04:START -->
## POSTQ011 — 2026-11-04: native importer и полнота continuation

**Основание:** [аудит POSTQ011](codex-158-postq011-transfer-and-experiment-audit-2026-11-04.md) и [адресный план](codex-159-postq011-addressed-repair-plan-2026-11-04.md). Срез исходников `45bd4e371f511e29c053f8117d61f43365abb460`. Это уточнение действующего владельца и приёмки; очередь, статусы и исторические результаты сохранены. Текущая нативная приёмка — **UNRUN**; документный аудит не запускал обучение.

**Новое доказательство в прежней области state admission.** В [native-result-002.json Q007](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/experiments-v2/RP01-MICRO/P0601/results/codex-review-2026-11-04/native-result-002.json) оба случая `optimizer-empty` и `optimizer-nan` имеют `NativeAccepted=true`, `FullStateAccepted=false`, а optimizer digest после прямого native import отличается от исходного. [native-reader/Program.cs:159–188](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/experiments-v2/RP01-MICRO/P0601/results/codex-review-2026-11-04/native-reader/Program.cs#L159-L188) проверяет protected target своим внешним guard, а на другом, диагностическом target намеренно ожидает принятие этих двух payload продуктом. Поэтому 9/9 означает успешную диагностику reader; закрывать по нему продуктовую проверку импорта нельзя. Восстановление исходных файлов Q007 уже выполнено; эта находка не назначает его повтор.

**Граница реального владельца.** [AdamW.TryImportPortableState:234–307](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/04.TrainingLoop/Optimizers/AdamW.cs#L234-L307) проверяет только предъявленные keys/shapes/длины, затем заменяет `_state`/`_step`; finite-проверки m/v и запрета `v<0` нет. Путь потребителя — [DpoTrainer.TryRestoreOptimizerState / TryRestoreContinuationState:572–676](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/11.DPO/DpoTrainer.cs#L572-L676). Trusted continuation proof дополнительно связывает bytes; обход уже выданного proof этой находкой не доказан. Проверка идентичности bytes сама не подтверждает численную допустимость изначально принятого state.

| Обязанность существующего владельца | Различающая будущая приёмка | Сохраняемый положительный случай |
|---|---|---|
| Native AdamW проверяет значения до публикации | NaN и ±Infinity в каждом виде момента, отрицательный second moment: отказ до swap; полный старый state и step неизменны сразу после отказа | Конечные m, конечные неотрицательные v, корректный повтор импорта |
| Continuation consumer проверяет применимую полноту по доверенному договору | Для исходного Q007 Step401 с четырьмя moment entries удалить одну, затем все записи: отказ. Ожидаемый initialized keyset, step и optimizer family берутся из trusted bundle authority вне проверяемой копии | Законный sparse state с его ожидаемым initialized keyset; не требовать моменты для ещё не обновлённых параметров |
| Настоящий restore consumer подтверждает атомарность | Тот же `DpoTrainer` admission/restore, отдельные before/after для optimizer, весов и применимых RNG/cache/version; внешний guard не заменяет вызов продукта | Подлинный сохранённый Q007 bundle восстанавливается; отказ не меняет прежнего live owner |

**Защита действующего sparse/fresh договора.** [PortableState_SparseAndKeyContractsRemainStable:286–300](https://github.com/lubochka/llm_mvp_core/blob/45bd4e371f511e29c053f8117d61f43365abb460/Tests/AdamWOptimizerStateRecoveryTests.cs#L286-L300) требует принять честный empty Step0 и state одного обновлённого параметра при двух bindings. Эти два controls обязательны вместе с негативами. Полнота конкретного continuation не равна универсальному all-parameters keyset; сам предъявленный пакет не вправе объявить себя полным. Infinity/отрицательный v выведены из текущего кода как будущие controls, в историческом Q007 они не измерялись.

**Передача следующему исполнителю.** Существующие владельцы — AdamW/DpoTrainer и state-договоры 02/101, связанные карточки P06/P12. Записывать раздельно `native_import_accepted`, `external_guard_accepted`, contract authority, before/after и фактический consumer. Native controls исполняются в назначенном техническом ремонте с сохранённым state и без нового Train/Step; этот документ их не объявляет выполненными. `Q02` — PAIR; `Q04/FIX-MC` остаётся приостановленным, численный дефект не является разрешением возобновить его обучение.
<!-- POSTQ011-2026-11-04:END -->

**4 ноября, фактический риск fixture:** [157](codex-157-postq003-executor-context-repair-2026-11-04.md) задаёт отдельный reader для damaged P11/P12 S1. Проверить constructor/setup/общую fixture до выбора теста: существующие save-тесты сначала обучают. В артефактном ремонте Train0 не означает Load0, если readback прямо назначен; Load/readouts считать отдельно. Переподготовка S1 и новый manifest под изменённые bytes не восстанавливают старое состояние. Эти ограничения не запрещают сборку/технические тесты назначенного ремонта по46.

<!-- POSTQ003-2026-11-03:START -->
## Уточнение после Q003 — DONOR-IS-THE-MATERIALIZED-ORIGINAL

Перед назначенным reuse конкретного donor применять50: path/attempt/manifest→Git pointer→фактические bytes→native loader→ordinary consumer. P11/P12 S1.weights расходятся с manifest; восстановить точный donor в существующих29-D3/29-W1, не Train и не reseal manifest. Положительный: подлинный полный bundle; отрицательный: указатель на конфликтный текст или receipt-only component. Историческую правильность algorithm и текущую доступность fixture учитывать раздельно.

Основание: [154](codex-154-postq003-artifact-and-transfer-audit-2026-11-03.md), действие существующего владельца: [155](codex-155-postq003-addressed-repair-plan-2026-11-03.md), обязательный [договор50](../experimets/codex-50-postq003-binary-and-historical-evidence-contract-2026-11-03.md). В передаваемое поручение включить этот адресный случай, positive/negative и критерий отказа. Сохранить фактические source/bytes/commands/exit, отдельные статусы artifact/native/science/product; не повторять PRIMARY ради файла или отчёта.
<!-- POSTQ003-2026-11-03:END -->


<!-- CODEX-POST128-NAV:START -->
## Уточнение после P128: личность, состояние, настоящий ответ

[Аудит 145](codex-145-post128-transfer-and-experiment-audit-2026-11-03.md) и [ремонт 146](codex-146-post128-addressed-repair-plan-2026-11-03.md) задают конкретные дополнительные контроли:

| Граница | Проходящий пример | Обязательный различающий отказ |
|---|---|---|
| Trusted source row → consumed lesson | Два разных Train IDs одной семьи с одинаковым содержанием и независимый Test сохраняют оба IDs | Чужой ID или запрещённый overlap; порядок SourceRows не выбирает личность через rows[0] |
| Project → mutable recipient | Два шага своего владельца на своём объекте; явное законное sharing | Разные mutable objects одного checkpoint, чужая capability или потеря recipient в другом конструкторе |
| Bundle → restored state | Файловые bytes/hash и полный stable state сверены с независимым producer | Пустая exact map, same-count KNN target, lineage, namespace или значимый order |
| Restored state → consumer → итог | Полный набор IDs и обычных ответов сохраняется | Один неверный output или отсутствующая строка меняют mandatory verdict и внешний exit |

Поле, сохранённое без потребителя, не считается проверенным. Отсутствующие поля API остаются NOT_EXPOSED; история уроков не подменяет checkpoint. Дозы первичных опытов не меняются; saved reanalysis не становится новым обучением.
<!-- CODEX-POST128-NAV:END -->

## Текущее уточнение после P125: actual state и его независимое ожидание

[Аудит142](codex-142-post125-transfer-and-experiment-audit-2026-11-03.md)/F125-01/05/08/12: candidate store уже написан, но подтверждает не те UTF-8 bytes; expected KNN journal не заменяет actual export; восемь пустых optimizer/RNG fingerprints не доказывают изоляцию. Читать полный checkpoint, точные keys/topic/target, кратность, provenance, значимый порядок и фактические bytes. Один comparer принимает честный snapshot и отвергает одну содержательную порчу с согласованным self-hash при прежнем count. Для Q125 сохраняется реальное24/24 совпадение в новом процессе использованной DLL; это не приёмка свежегоHEAD. Отсутствующее старое состояние назвать UNKNOWN, не восстанавливать из expected и не оплачивать новым PRIMARY. [дополнение codex45 о доказательствах](../experimets/codex-45-post125-evidence-consumer-contract-2026-11-03.md) задаёт точные уровни доказательства; [план143](codex-143-post125-addressed-repair-plan-2026-11-03.md) сохраняет очередь. Нижние датированные инструкции применяются с этим уточнением.

<!-- POST113-CURRENT-2026-11-02:START -->
## После P113 — состояние, его версия и проверенный payload

Translation уже публикует согласованный holder, но N01/N02 требуют общего writer gate и принадлежности оценки измеренному state. Полное сохранение/восстановление связывает фактические значения/maps/owners, а не counts или заявленные hashes. Positive читает новый target без Train; negative меняет одну карту при прежнем export/count и обязан нарушить обычный ответ. Каждый механизм перечисляет собственных mutable owners; отсутствующие optimizer/RNG не добавляются по шаблону.

Основание: [адресный план 135](codex-135-post113-addressed-repair-plan-2026-11-02.md). Срез `21007e25ec3f6f6634dd8733fc8631ae267e518c`, 2 ноября 2026. Этот блок уточняет указанные условия; исторические результаты, собственные данные, единственный фактор, учебный бюджет и очередь сохраняются. Порча копии квитанции не назначает новое обучение или дополнительное чтение модели. Нижние датированные указания применяются в своей исходной области; новые требования не делают старые ненаблюдённые поля измеренными.


**Уточнение перед публикацией, 1aaa3bc:** канонический основной builder, PairId, ряд readers/imports, collision preflight и удаление PAIR-обхода floor уже реализованы в исходнике; native gate UNRUN. Открыты version/default, достижение кандидата N16, RULE/restore и новая N18 (non-object JSON в collision preflight). Действующий остаток — [135/R09](codex-135-post113-addressed-repair-plan-2026-11-02.md), подробная сверка — [отдельный документ](../../../../experiments-v2/_audits/2026-11-02-post113-audit/publication-reconciliation.md). Повторять реализованные части M03 не требуется.
<!-- POST113-CURRENT-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход перед новым аудитом или заданием

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

До действий прочитать выбранную карточку целиком и её локальную секцию120, source/raw и фактический маршрут producer→ordinary caller→learner→consumer; local HEAD, published SHA и dirty bytes различаются. Списки навыков ниже — дополнительные, не освобождение от полного свежего стека120. Исторические Tier3/vocab/split/UI/Git/model правила не расширяют нынешний паспорт и авторизацию.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.

**Сохранение постановки:** exact recorded result не повторять, old NOT_RUN не выше текущей Q-строки. Полный outer/nested бюджет и независимый gold остаются у карточки; пересчёт raw/структурный guard сTrain0 не заменяет обязательный учебный цикл и не получает фиктивный Train. Команды, API и фильтры не додумывать исполнителю. Вход обязателен также в передаваемом промпте выбранного ID.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

Уточнение состояния после P103/P107: retention требует одного lineage S0→S1 и полного содержания прежних ответов. Два независимых Train — INDEPENDENT_TRAINING_OBJECT; Save и чтение прежнего Unit — SAME_LIVE_OBJECT_READ. Для fresh load нужны новый объект, точные bytes, Load и ordinary consumer. LFS pointers не удостоверяют backing weights. Фикстура выбирает и payload, и lesson identity; metadata без тела ответа не закрывает исполнение. Условия и бюджет branch-dependent действий задаются формулой, отказы не дополняются вызовами до желаемого числа.

Основание: [аудит 123](codex-123-post107-transfer-and-experiment-audit-2026-11-02.md), [план 124](codex-124-post107-addressed-repair-plan-2026-11-02.md), [контекст 41](../experimets/codex-41-post107-executor-checks-2026-11-02.md). Это уточнение приёмки будущего переноса; прежние результаты и назначенная доза сохраняются.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее уточнение после P102 — Состояние и чтение после update

**Договор.** Проверять named S0/S1, фактического loader, state ownership и keyed optimizer payload только у механизмов с optimizer. Для native memory нейронные поля N/A по контракту. После step1 отсутствие moment fingerprint называется NOT_MEASURED, не «не применяется до первого шага». Parameter delta не доказывает nonzero data gradient. Fresh child без нового Train читает тот же accepted bundle; отсутствие native restore API обозначается явно, surrogate не подменяет его.

**Ошибка и ожидаемая проверка.** Wrong case: новый объект/повторный Train выдан за cold restore. Correct case: exact payload/config/keys и обычный consumer подтверждены child receipt; missing payload — typed incomplete. Late report failure восстанавливается от сохранённого S1.

Источник: [аудит120](120-post102-transfer-and-executor-audit-2026-11-02.md); действующие роли skills, WRONG/CORRECT, testing/recovery и границы полномочий — [40](../experimets/40-effective-executor-context-2026-11-02.md). Дозы, факторы и исторические raw сохраняются. Этот договор не запускает PRIMARY и не объявляет будущие tests выполненными.
<!-- POST102-AUDIT-2026-11-02:END -->


<!-- POST101-AUDIT-2026-11-02:START -->
## Дополнение после P101: Состояние, прочитанное конкретным владельцем

Основание — [117](117-post101-transfer-and-experiment-audit-2026-11-02.md) / [118](118-post101-addressed-repair-plan-2026-11-02.md). **Действующий договор.** Remote blob, скачанный payload, десериализованный state и использованный обычным learner/consumer state — четыре разных наблюдения. Для каждого owner перечислить только его применимые weights/memory/index/tokenizer/moments/scheduler/RNG/cursor/metadata. P101 recorded weights hash и readback подтверждают своё соответствие, но не новую загрузку аудитором; отсутствие локального бинарного файла не означает утрату. Фактические optimizer0 уP89 и400 уP101 не заменять общей подписью sharedS0.

**Адресная приёмка.** Negative: чужой digest/схема/неполный обязательный bundle отказывает до live publication. Positive: доступный сохранённый state читается тем же ordinary owner без нового Train. Staging партии проверяется до restore по всему применимому состоянию; syntax-only preflight не закрывает late typed/held-out failure.

Это требование к будущему исполнению/приёмке применимой границы. Нового PRIMARY, увеличения бюджета или повторного обучения оно не назначает; исторические evidence и исходные recipe сохраняются.
<!-- POST101-AUDIT-2026-11-02:END -->


**Дополнение 25 октября 2026:** для нового FIX-SEP используются два уже обученных weights-only состояния SEP01; точный профиль чтения и границы восстановления — [§19](#sep133-fixtures). Предыдущие состояния и бюджеты не заменяются.

**Актуализация23октября2026:** источники состояний новых для единой очереди внешних ID перечислены в§18; это ссылки на существующие recipes, без второго bootstrap. Статус общей build-предпосылки§10 уточнён по755a8cd: Remove уже есть, явный NKu01 Include сохраняется.

## Актуальное дополнение 23 октября: FBC, LFT и N63C

Точные назначения ниже дополняют прежние профили, не разрешая повторное обучение ради каждого теста.

| Consumer | Состояние и место | Восстановление и предел |
|---|---|---|
| [FIX-FBC54](54-frozen-base-check-scope.md) | Готовый `_shared-learned-starts/h512-dt-segmented/v1/S0.weights.bin` в `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/` | Особый FBQ профиль: **policy и reference=S0**, DPO optimizer fresh0; producer moments/RNG не импортируются. Это не profileTS/NH с referenceB0. По10DPOupdates/arm, никакого нового acquisition |
| [FIX-LFT55](55-frozen-linear-transpose-cache.md) | Будущий `C:/Projects/llm_mvp_core/Tests/Fixtures/ExperimentFixes/v1/FTR01/S0/` | На диске FTR results есть hashes, но полногоS0нет. Один ограниченный producer250SFT **до baseline inputs/intent**, затем portable optimizer250→254 и model/RNG restore в каждом плече. Если пригодный bundle уже есть — producer0. Подробные APIs/данные/бюджеты в55 |
| [FIX-N63C17](17-dpo-sft-correction-collision.md) | `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/CDB01/results-01/authority-acquired.bin` | Policy weights-only donor: 22/23 в no-LF панели; тот же payload имеет 23/23 в SFT/LF-профиле SEP. Reference B0; свежие DPO/SFT optimizers одинаковы A/B, без заявления восстановления acquisition moments. Всего 2 DPO + 40 SFT, 42 диагностических Forward, 0 acquisition; точные bindings в [17§11](17-dpo-sft-correction-collision.md#n63c-fixed-gold-readout) |

Совместимость означает совпадение named weights, tokenizer/framing, topology, learner mode, optimizer policy и readout; одинаковый размер файла или model family недостаточен. Полный FTR optimizer нельзя заменить weights-only FBQ/CDB, а SFT moments нельзя молча импортировать в DPO. При несовместимости сначала адресно разобрать identity/loader; не предлагать пользователю повторное обучение как равноправную альтернативу исправлению загрузки уже подходящего состояния. Кандидат остаётся документацией до разрешения исполнения.

Этот документ связывает 24 кандидата и шесть дополнений TEST-TCTX/TEST-TMEM/TEST-ORDER/TEST-E22/TEST-POS/TEST-SK с данными и состояниями; зависимый план FIX-E2E использует отдельно назначенные состояния получателей. Первоначальные 8 кандидатов сохранены; семьи MC/MS/PD закреплены в разделе7, TS01 — в карточке28, reuse segmented S0 для FIX-GB, готовый RMS400 и два BPE corpus — в [разделе11](#new-resource-fixtures). Он **дополняет**, а не заменяет [контракт общих стартов33](../experimets/33-shared-learned-starts.md) и [контракт продолжения29](../experimets/29-learned-start-and-parent-continuation.md). Новые нейронные producers и повторные 1000/400-шаговые подготовки здесь не назначены. Для небольших дискретных learner карточки явно задают fixed cold-fit/prefix; FIX-E2E задаёт будущую интеграционную fixture, ещё не квалифицированную как успешный опыт. Это конкретные исключения из требования готового checkpoint, не разрешение скрытого bootstrap. Текущая работа — только план.

## 1. Где будут находиться тестовые входы

Корень будущих небольших fixture-паспортов: `Tests/Fixtures/ExperimentFixes/v1/`. Отдельные карточки назначают им подпапки; источник является неизменяемым файлом по зафиксированному commit, не последним отчётом из рабочей директории. Будущий helper `Tests/ExperimentFixes/ExperimentFixtureLoader.cs` читает паспорт, проверяет SHA-256 исходных bytes и возвращает неизменяемые TRAIN/EXAM объекты. Это новый файл по плану; сейчас его нет и его команда не запускалась.

В паспорт входят `fixture_id`, `source_commit`, `source_paths`, `sha256`, `topic`, `profile`, `learner`, `train_ids`, `exam_ids`, `model_facing_material_digest`, `state_kind`, `donor_path`, `model_identity`, `tokenizer_identity`, `optimizer_policy`, `rng_policy`, `qualified_metric`, `historical_limitations`. Хэши вычисляют из реальных bytes при переносе, не из строки пути. Незахваченные исторические поля получают `not_captured`, не выдуманное значение.

TRAIN и EXAM сериализуются раздельно. Ожидаемые ответы EXAM доступны только оценщику. Случайное перемешивание вместо авторского состава, добавление новых тем, изменение обозначений классов, нормализация регистра, подсказки expected в prompt и выбор seed/checkpoint по EXAM запрещены.

## 2. Точная карта источников

| ID | Источник данных и состояния | Как использовать в новом тесте |
|---|---|---|
| FIX-BPE | `docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/`, полный A/B raw; fixture/runner paths в [карточке](12-bpe-no-match-copy.md) | Обучить BPE на фиксированном TRAIN каждого профиля отдельно, сравнить 64 слияния, словарь, IDs, exact roundtrip и allocations. Не загружать нейронный checkpoint для проверки обучения самого BPE |
| FIX-LC | `experiments-v2/RP01-MICRO/N61-REF-CACHE/results/common/S0.weights.bin` и `preparation.json`, `results/pair-summary.json` | Один сохранённый источник весов для двух новых независимых экземпляров. **NEW_DPO_EPOCH:** fresh DPO optimizer step0 и одинаковый DPO RNG при dropout0. Не объявлять продолжением исторического SFT |
| FIX-N60 | `experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/` — точные fixtures/Programs и `results/A/arm-result.json`, `results/B/arm-result.json` | Две изначально пустые независимые таблицы Unit; A/B получают исходные две пары. Дополнительный отрицательный контроль из [плана](11-translation-verifier-admission.md) имеет отдельный ID и не переписывает старый A/B |
| FIX-NKU | `docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/inputs.json`, `gold.json`, `arm-A.json`, `arm-B.json` | 4 TRAIN + 4 EXAM, k=3, исходные 30 признаков. Нулевой старт обоснован проверкой приобретения: после настоящего fit результат ненулевой. Один fit на плечо, без предварительного разогрева учебными ответами |
| FIX-RET | `experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/A/states.json`, `B/states.json`, raw answers и runner | Общий prefix из первых двух уроков, независимые копии состояния до третьего урока; после него проверить каждый старый/новый маршрут. Исторические states — свидетельство; не выдавать их автоматически за принятый производственный checkpoint |
| FIX-WT | `experiments-v2/RP01-MICRO/WT01/results/A/report.json`, `B/report.json`, fixture/runner из [плана](14-wiki-routing-taxonomy.md) | Раздельные cold A/B и зафиксированный 4-row TRAIN/6-row EXAM. Миграция label space проверяется отдельным сохранённым старым snapshot. Исторический B с failed publish gate не подставляется в live route |
| FIX-N63C | **Действующий donor:** `experiments-v2/RP01-MICRO/CDB01/results-01/authority-acquired.bin`, raw `CDB01/results-01/n63c-probe.json`; correction-collision fixture из [17](17-dpo-sft-correction-collision.md). Прежний `N63C-PROBE/results/authority-acquired.bin` — только historical evidence, не вход нового теста | H512; назначенная SHA-256 весов `59810def905c4ead2f22985c5e1d525f59feb45590256eca8b492a293206d44f`, 47 899 519 bytes. До воздействия: no-LF 22/23 и SFT/LF 23/23 по тому же состоянию SEP Abs. Проверить actual payload/config/tokenizer/targets до чтения. 2 DPO + 40 SFT; 36 no-LF + 6 LF = 42 scoring Forward. Optimizers fresh, acquisition 0 |
| FIX-N63R | `experiments-v2/RP01-MICRO/N63R-PROBE/results/wa-trained.bin`, `wa-trained-optimizer.bin`; `experiments-v2/RP01-MICRO/N63R/fixtures/input.json` | H32 CODE_Q, прежние два урока; контроль их удержания после новых CODE_Q-уроков. BUG-примеры нагружают общий буфер, но не попадают в обучение CODE_Q. Сохранённые moments используются только с совпавшими binding/identity |
| TEST-TCTX | `experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/Program.cs`, `results/A/raw.json`, `results/B/raw.json` на2297566; будущая fixture `Tests/Fixtures/ExperimentFixes/v1/TTrainContext01/` | Две записи слова book в пустой native ContextualDictionary, four-before/four-after; Q1/Q3 одинаковы. Cold acquisition проверяет писатель AddEntry, поэтому BuildDefault или готовый наполненный словарь не заменяет этот переход; нейронный checkpoint не нужен |
| TEST-TMEM | P125 exact `training-batch-B/C.json`,24-row `eval-inputs.json`/gold; P25 `input-checkpoint.json`, все пути/bytes в [19](19-translation-memory-coverage.md) | Две8-row native memory acquisitions только для writer-test; один готовый исторический checkpoint для cold consumer,0новых Train. Один shared receipt,168ordinary calls суммарно; EN→RU technical не заменяет RU→EN FIX-E2E |
| [TEST-E22](27-predicate-dataset-router-learning-coverage.md) | Положительный E22 raw от 7 мая; новый точный fixture из 6 TRAIN и 4 EXAM C#-исходников задан в карточке 27. Будущие файлы `Tests/Fixtures/ExperimentFixes/v1/PredicateDatasetRouter/` | Historical corpus из 450 строк и обученная таблица не сохранены; у native learner нет save/load API. Один fit 6 пар на одном экземпляре, 4 before + 4 after, reuse единой квитанции. Это новое покрытие усвоения двух шаблонов, не replay исторического corpus или выбор предметного домена |
| [FIX-TS](28-dpo-tokenizer-snapshot.md) | `experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/`; material `experiments-v2/RP01-MICRO/TS01/in/train.jsonl`; raw `TS01/results/attempt-0001/A/metrics.json` и `B/metrics.json` на `87284fc734af2c3e57d5486d8f0b13a7aab949cf` | Отдельная готовая H512 семья: две TRAIN строки по 33/33 response targets. Producer уже выполнил 1000 SFT updates и не повторяется. В каждой руке одна новая DPO update со свежим DPO optimizer и восстановленным S0 RNG; только policy получает S0, reference остаётся frozen B0. Это не продолжение SFT moments и не donor LC/MC/OR. A использует native per-pair CreateBatch, B — native Materializer.Prepare: проверяется parity, но разные preparation envelopes не дают нового causal performance benchmark. Exact identities, source hashes, material и общий тестовый бюджет заданы в карточке 28 |

Пути нейронных весов — адреса сохранённых артефактов в исходных материалах, **не заявление, что их бинарные bytes были материализованы или восстановлены в этой документной сессии**. LFS pointer не проходит проверку входного артефакта. Loader обязан отличить указатель от содержимого и проверить digest до model load.


## 3. Чем веса отличаются от продолжения обучения

Полное продолжение требует не только weights, но и точных параметров модели/LoRA, tokenizer IDs, optimizer moments и step, состояний RNG/cursor, порядка данных и framing/masks. Отсутствующее поле нельзя восстанавливать «по seed», если исходный курсор неизвестен. Нельзя замаскировать несовместимость ручным изменением manifest/hash.

LC — документированное исключение **для нового DPO-этапа**: пригодные ранее обученные веса загружаются, затем в обоих плечах создаётся одинаковый новый DPO optimizer/RNG. В [preparation.json](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/common/preparation.json) исторические SFT optimizer/RNG не захвачены. Такой источник не разрешает тест непрерывности SFT moments. Повторять подготовку, чтобы задним числом заполнить квитанцию, нельзя.

N63C и N63R имеют другие модели, framing и учебные строки. Они не заменяются H512 LC-стартом только потому, что у него лучше показатели или файл доступнее. Их частичные навыки и ограничения зафиксированы в отдельных планах. Если будущий тест начинает новую SFT/DPO-эпоху, это отдельный явно названный контракт теста, не «точный повтор» исторического прогона.

## 4. Переиспользование без изменения общего источника

При подготовке тестовой коллекции один раз прочитать и проверить bytes донора. Каждый изменяющий состояние тест получает собственные model/optimizer objects, восстановленные из тех же bytes; общий файл не открыт на запись. После восстановления сравнить идентичности и начальные state hashes. Сами model objects не разделять между A/B или параллельными тестами.

Для маленьких табличных Unit, где опыт честно исследовал cold-start fit, фиксированный prefix можно обучить один раз в fixture и экспортировать через существующий model save/load API, если этот API существует. Если product API сериализации нет, его отсутствие прямо указано в карточке: использовать независимые fit одного маленького TRAIN на экземпляр, а не придумывать checkpoint и не добавлять неоснованную систему сериализации ради теста. У N-KU это 4 строки, не скрытый нейронный bootstrap.

Для production-публикации выбранного обученного артефакта действуют существующие resolver/publisher и `training-artifacts/models/...`; экспериментальный donor не становится canonical parent перемещением файла. В текущей задаче ни перемещение весов, ни новая запись в `training-artifacts` не выполняются.

## 5. Квалификация и бюджет

В первой строке будущего отчёта назвать, **какому состоянию и какой метрике** соответствует «до». Для нейронного learner требуется наблюдаемый ненулевой навык из его собственной фиксированной TRAIN-панели. Отдельно показать constant baseline там, где он может объяснить результат. `0→0` запрещено использовать как доказательство обучения; одинаковые полные неверные ответы не скрывать за изменением margin.

Точный бюджет находится в каждой карточке. Producer-обучение из контракта33 не запускается consumer-тестом автоматически. Пригодный существующий donor используется прежде любой подготовки. Отсутствие или несовместимость данных даёт `missing_or_incompatible_fixture` с конкретным путём/полем; исполнитель не подбирает другой набор и не увеличивает steps до успеха. Это препятствие данной будущей проверки, а не основание переписать исторические результаты.

Расход фиксируется отдельно: подготовительные updates, проверочные forwards, DPO updates, SFT updates, новые/повторяемые TRAIN tokens, Generate calls, внешние вызовы. Ошибка позднего экспорта не обнуляет уже выполненное обучение. Для N63R показываются A/B/C по отдельности: одинаковое число updates не равно одинаковому числу tokens и не снимает разницу curriculum.

## 6. Проверка происхождения нужна только как часть обучающего пути

Проверка digest, tokenizer identity, индексов классов или optimizer binding допускается здесь потому, что она предохраняет конкретное восстановление или передаваемый learner вход. В отчёте указывается вызывающий метод продукта. Отдельная «проверка наличия JSON» вне потребителя не считается экспериментом и не запускается как самостоятельная дорогая задача.

Исполнитель применяет общие [правила модели, skills, ожидания, STOP/NEXT и отчёта](03-test-and-execution-contract.md). При `NEXT` общие fixtures не становятся предлогом продолжать предыдущую карточку.

## 7. Две уже квалифицированные семьи для MC01, MS01 и PD01

Дополнение 2026-10-21 UTC по source `4e993a33c9396f155a4aaeb42170e0251f34aa77`. Это входы новых планов, не назначение новых producers. В обеих семьях producer исторически выполнил1000SFT updates; consumer их не повторяет. Корень `experiments-v2/RP01-MICRO/_shared-learned-starts/`.

| План / будущий passport | Семья / способ продолжения | Фиксированный material и квалификация |
|---|---|---|
| [FIX-MC](21-sft-optimizer-continuity.md), `Tests/Fixtures/ExperimentFixes/v1/Mc01/manifest.json` | `h512-dt-joint/v1`; SFT optimizer1000 + RNG, первый урок, затем reset/carry только между уроками | D-T1→D-T2,125tokens на carrier,33response targets; before33/33 обе строки. EXAM D1–D4 отдельный |
| [FIX-MS](23-sft-response-mask.md), `Tests/Fixtures/ExperimentFixes/v1/Ms01/manifest.json` | Та же `h512-dt-joint/v1`; SFT optimizer1000 в обоих плечах, до1002 без reset | Тот же TRAIN/EXAM; mask denominator124,active33; меняется только objective mask. Внедрение MC не является условием test loader |
| [FIX-PD](22-dpo-valid-length.md), `Tests/Fixtures/ExperimentFixes/v1/Pd01/manifest.json` | `h512-pad-segmented/v1`; восстановить SFT-derived weights/RNG, **fresh DPO optimizer0** | PAD1→PAD2, segmented no-separator carrier; before33/33 и69/69. EXAM/Generate нет; SFT moments не импортируются |

Данные и контекст одного family не заменяются другим. Внутри code-documentation темы используются классы/контроли одной задачи; outer routing тема «оркестрация» не добавляется в эти TRAIN.

### Точные bytes и идентичности

Первичные manifests: [dt-joint bundle](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-joint/v1/bundle.json), [pad-segmented bundle](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-pad-segmented/v1/bundle.json). Их схемы различаются (`familyId/file` и `family_id/name`); loader явно поддерживает эти два закреплённых формата, не угадывает поля и не переписывает исходные manifests.

| Файл | dt-joint SHA256 | pad-segmented SHA256 |
|---|---|---|
| S0.weights.bin,47899519bytes | `99325ae2d693e1b73c3bcf653b5f5744117b2bd30e405211b27fe06b6a3c04c1` | `f404f8eedc6f03edd60103d13d05440083088990fbb728630b750b5902f03987` |
| S0.optimizer.bin,426788bytes | `7e3f3fa2b98259325725110bc901ff82f5acb634cbb422b74d72f2cad72cbda2` | `a7dd7d747f71bff858e073d92cf7fdf7d803db94d9d16330d004bab51e3b1136` |
| S0.rng.json,1573bytes | `f819248e615f50e41c22db16d5fac5116809857d195d3b9d149c9b6ec580ba9b` | То же значение; это не делает семьи взаимозаменяемыми |
| data.source.jsonl | `efedf046d90e69b3eda5f5200c412ae200a28917396147683b94278c89d58573`,610bytes | `b8dc0bc759983ef45b960b2ea6cb684db698dbfd1c3b38982931bd90906e98c9`,724bytes |
| data.material.json | `af2ec74db4323d98033811583fdf1cbd9438f2cb2844f167aec2dc5675097204`,9140bytes | `668afba066a3600d6ea1ee58226f6a0fcf0a5f0448628537e001fb86faf75f63`,17461bytes |

Общий tokenizer.json: source-manifest20867bytes, file SHA256 `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066`. Для [generation.json](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-joint/v1/generation.json) source manifest указывает1520bytes и **file SHA256** `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`; внутри файла **runtimeCompatibility.configSha256** — `8cf38d269f806356191c614ccbf3bcafdb3c7e339f02e9d77bdade4e9fcd68ae`. Первый hash проверяет exact bytes файла, второй — runtime config identity. Они не взаимозаменяемы; нормализованная локальная копия текста не доказывает file hash. Та же пара identity относится к PD family и используется FIX-PD по этой ссылке. Модель `rp01-csharp-small/v1`, q/v LoRA rank8/alpha16/dropout0; topology и16keys зафиксированы в authority.

PD raw `578a2915015381f21a204d140bd7836b4a4e5d23375b04b9d216c67a8bae248d` — digest сериализованного массива adapter values после load, не SHA файла weights. Конкретный источник вычисления — [PD01ExperimentBridge:494–495](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/Common/ContinuousLearning/PD01ExperimentBridge.cs#L494-L495). File SHA f404f8 проверяется до load; adapter digest578a после load. Одна проверка не заменяет другую.

### Применение loader и отсутствие скрытого старта

Будущий общий `ExperimentFixtureLoader` из раздела1 читает family passport, проверяет полные bytes/lengths/hash, отвергает LFS pointer, создает независимый instance A/B и проверяет ненулевой TRAIN before по конкретному recipe карточки. Проверка before входит в diagnostic budget, не добавляет лишние readouts. Source files общие и read-only; MC/MS не создают собственные копии47MBдонора в каждой attempt. Мутирующие checkpoints принадлежат только `artifacts/fbe/<ID>/attempt-0001`.

[dt authority](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-joint/v1/authority.json) прямо называет private research start, не canonical DPO parent; обе authority не содержат transferable admission/lease/capability. Loader не присваивает `InitialBaselineNoUpdate` обученной policy, не переименовывает domain/unit и не запечатывает research artifacts как production authorization. Native product test admission создаётся отдельно своим existing test path, production gate остаётся действующим.

Для native resume MC требуется собственный typed SFT continuation, записанный будущим **SupervisedOnly** consumer→SFT admission/transaction→v3 publication; существующая DPO transaction для этого не подходит. Exact SFT dataset находится в планируемом `training-data/sft/{domain}/{designPattern}/{datasetVersion}`; tiny consumer test21 проверяет два реальных обновления и retry0updates с собственным разрешённым material. Настоящий H512 donor нужен только фикстуре исходного A/B. MC сохраняет session scheduler semantics и запрещает ResumeSft при промежуточном/scheduled DPO, смене target mode/framing или parent conflict. Existing DPO→SFT и его authority сохраняются. PD начинает отдельную DPO-эпоху; MS test loader восстанавливает моменты непосредственно без скрытого внедрения MC. Missing artifact/нет LFS objects/identity mismatch — конкретный blocked input выбранной проверки. Не bootstrap, не альтернативная ветка, не новое обучение до успеха.

Все hashes в этом документном дополнении прочитаны из первичных manifests; бинарные LFS bytes здесь не загружались и native load не выполнялся. Их фактическая проверка — часть будущего назначенного теста.


## 8. Новые входы FIX-KHC и TEST-ORDER

Дополнение 22 октября 2026, source `213f0c9b0568384632ab6bb5a67c57d6f17b3df9`. Все прежние семьи и бюджеты раздела7 сохранены.

| План | Источник и будущий passport | Использование |
|---|---|---|
| [FIX-KHC](24-file-action-shared-parse.md) | `experiments-v2/RP01-MICRO/KHC/results/arm-A.json`, `arm-B.json`; `Tests/Fixtures/ExperimentFixes/v1/Khc01/manifest.json` | Ровно K1–K6 и KQ1–KQ4. Основной native A/B: 2 FileAction fit / 12 предъявлений / 16 Compile; 0 neural. Native checkpoint API отсутствует: fixture хранит данные и receipts, не притворяется восстановлением модели. Компаньоны и побочные E05 Train учтены отдельно в24 |
| [TEST-ORDER](25-dpo-sft-order-learning-coverage.md) | `experiments-v2/RP01-MICRO/_shared-learned-starts/h512-order-mixed/v1/`; `Tests/Fixtures/ExperimentFixes/v1/Or01/manifest.json` | Третий, отдельный H512 family: weights + SFT optimizer1000 + RNG, четыре усвоенных текста. Никакого нового1000-step producer. DPO optimizer свежий; frozen reference остаётся исходной B0, не подменяется policy S0. Точные hashes, TRAIN/EXAM, order и расход — в25 |

KHC не использует нейронный checkpoint: измеряемый фактор — подготовка самой маленькой таблицы. Последующие assertions переиспользуют один receipt/экземпляр каждой руки. OR, напротив, обязан прочитать готовый family bundle и не может заново обучать модель, чтобы восстановить отсутствующий файл. Если exact LFS bytes отсутствуют или не совпал digest, соответствующая native проверка имеет конкретный недоступный вход; другого donor и автоматической подготовки нет.

SFT optimizer OR1000 — исследовательский fixture для двух порядков, не новый производственный continuation policy. TEST-ORDER не ослабляет запрет FIX-MC на ResumeSft после DPO. Целевые production dataset/admission/transaction/publisher остаются прежними; источник reference и полное состояние момента начала теста записываются отдельно.


## 9. Свежий вход FIX-KHE и повторное использование памяти после T1

Дополнение по `c8b0d8a029ce721ecd906d3ff747e740b524be23`. [FIX-KHE](26-method-knn-training-admission.md) использует `experiments-v2/RP01-MICRO/KHE/results/arm-A.json` и `arm-B.json`. Будущая fixture — `Tests/Fixtures/ExperimentFixes/v1/Khe01/{manifest,train,exam,historical-readouts}.json`; точные поля и тексты назначены в26, источник и SHA-256 фиксируются в manifest.

T1 — один C# файл с пятью методами и четырьмя парами; T2 — четыре файла с одним методом и нулём пар. Четыре EXAM-запроса имеют gold Loop. После T1 обязательны четыре реальные пары и 4/4 правильных ответов; только затем проверяется эффект T2 на **том же** экземпляре. Constant Loop тоже4/4: fixture доказывает удержание обученной памяти, не классовое обобщение. EXAM не становится MethodKnn TRAIN.

У MethodKnn нет установленного native checkpoint API. T1 выполняется один раз на плечо; дальнейшие assertions используют полученное состояние, без повторного acquisition и без новой нейронной модели. Снимок пар в receipt — наблюдение, не восстановимый checkpoint. К KHC и OR этот набор не подмешивается.

Основной native A/B: три MethodKnn.Train, 14 методов, переданных Train, восемь вставленных пар и 24 Compile / 24 побочных E05 fit. Четыре вызова фасада извлекают18 методов; B не передаёт четыре метода T2 в Train. Полный назначенный расход с двумя compatibility-проходами — 11 MethodKnn.Train / 76 предъявлений методов / 50 вставленных пар / 30 Compile-E05. Три boundary cases после B дополнительно извлекают9 методов, отклоняют6 пар, не вызывают native Train или Compile. Эти contract cases участвуют перед обучением, но не объявляются успехом «0→0». Никаких скрытых retries, нового donor или BPE/SFT/DPO обучения.


<a id="native-build-prerequisite"></a>
## 10. Общая предпосылка сборки для назначенных native тестов пакета

**Проверить и при необходимости устранить до обеих рук, одинаково для A и B.** Это ограниченный ремонт состава сборки, участвующий до запуска обучения. Он не является новым успешным экспериментом, не меняет learning factor и не означает, что сегодняшние тесты запускались и упали. Выполнение — только внутри отдельно назначенного переноса или учебного покрытия; нынешняя документная сессия эти команды не запускает.

Область: любая назначенная команда `dotnet test Tests/LlmFromScratch.Tests.csproj`, в том числе baseline, `--list-tests` и FIX-E2E. Предпосылка не ограничена карточками 24–26. Её область задаёт ссылка Tests→35, а не номер опыта. Самостоятельный resource host без этой зависимости не получает дополнительных запусков, обучения или правок. Общий запрет скрывать падения исключением тестов сохраняется; [03§2](03-test-and-execution-contract.md#2-как-перестраивать-падающие-тесты) допускает только описанный ниже архивный Remove.

### Текущее состояние предпосылки

На `755a8cd2adad7727596c259b3330ae8249f18a8a` [ExperimentsV2.csproj](../../../../35.ExperimentsV2/ExperimentsV2.csproj) уже содержит `Compile Remove="Experiments/Rp01Micro/**/*.cs"` **и** отдельный `Compile Include="Experiments/Rp01Micro/NKu01/**/*.cs"`. Поэтому приведённый ниже ремонт не назначается повторно автоматически. Проверить фактический состав как common baseline; сохранить необходимый NKu01 Include. Удаление этой явной зависимости под видом общего Remove могло бы сломать потребителей. Сборка сейчас не исполнялась, новый compiler FAIL не заявлен.

### Исторический источник конфликта

На проверенном [`7eab1172`](https://github.com/lubochka/llm_mvp_core/commit/7eab1172bb3787eed9c1d966931a856df95f4976) [Tests/LlmFromScratch.Tests.csproj:67](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/Tests/LlmFromScratch.Tests.csproj#L67) ссылается на [35.ExperimentsV2/ExperimentsV2.csproj:1–29](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/35.ExperimentsV2/ExperimentsV2.csproj#L1-L29). Последний использует Microsoft.NET.Sdk, не отключает default Compile items и не исключает `Experiments/Rp01Micro`. По [правилу SDK](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview#default-includes-and-excludes) в Compile входят вложенные C# файлы.

В этом дереве уже до KHE находились архивные [E-OPT-STEP/A/DpoTrainer.cs](https://github.com/lubochka/llm_mvp_core/blob/c8b0d8a029ce721ecd906d3ff747e740b524be23/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/A/DpoTrainer.cs) и [B/DpoTrainer.cs](https://github.com/lubochka/llm_mvp_core/blob/c8b0d8a029ce721ecd906d3ff747e740b524be23/35.ExperimentsV2/Experiments/Rp01Micro/E-OPT-STEP/B/DpoTrainer.cs), определяющие одинаковые типы namespace DPO. KHE добавил копию типов SemanticCompiler.Prediction, принадлежащих native проекта 12. По исходникам это конфликт состава сборки/выбора типов, существовавший независимо от новых тестов; фактическая диагностика компилятора в данной сессии не получена. Пропуск к native learning нельзя заменять запуском копии алгоритма из проекта 35.

### Предписанный ограниченный ремонт

До первой назначенной команды Tests один раз получить ProjectReference этого проекта и Compile проекта 35. Если текущий Tests больше не ссылается на проект 35, записать это отличие источника и не применять Remove автоматически. Если ссылка сохраняется и Compile включает `Experiments/Rp01Micro/**/*.cs`, добавить **только в35.ExperimentsV2/ExperimentsV2.csproj**:

```xml
<ItemGroup>
  <Compile Remove="Experiments/Rp01Micro/**/*.cs" />
</ItemGroup>
```

Это исключает архивные копии из данной сборки. Не удалять файлы, не исключать Tests или штатные E01–E53, не менять project references native библиотек, assertions, namespaces, экспериментальные hosts или их источники. Если exact exclusion уже принят в новом HEAD и evaluated Compile его подтверждает, повторно править файл не требуется. Сохранить одну и ту же сборочную предпосылку в A и B; не считать её частью измеренного выигрыша любого выбранного FIX/TEST.

Архивы остаются доступны собственным hosts. [KHC/Runner.csproj](https://github.com/lubochka/llm_mvp_core/blob/c8b0d8a029ce721ecd906d3ff747e740b524be23/experiments-v2/RP01-MICRO/KHC/Runner.csproj) и [KHE/Runner.csproj](https://github.com/lubochka/llm_mvp_core/blob/c8b0d8a029ce721ecd906d3ff747e740b524be23/experiments-v2/RP01-MICRO/KHE/Runner.csproj) отключают default Compile items и явно включают свои файлы; Remove в другом проекте на них не распространяется. [OR01/Runner.csproj](https://github.com/lubochka/llm_mvp_core/blob/c8b0d8a029ce721ecd906d3ff747e740b524be23/experiments-v2/RP01-MICRO/OR01/Runner.csproj) использует собственные Libs/Sources props. Ни один из этих hosts ради проверки исключения не запускать. Планируемый resource host FIX-BPE размещается вне проекта 35, в `Tests.Support/BpeNoMatchCopy01/`, по [карточке 12](12-bpe-no-match-copy.md), чтобы не создать такой же конфликт заново; это будущие файлы, не перемещение существующего опыта.

### Команды и квитанция без обучения

Из `C:/Projects/llm_mvp_core`, до необходимого ремонта:

```powershell
dotnet msbuild Tests/LlmFromScratch.Tests.csproj -nologo -p:Configuration=Release -getProperty:AssemblyName -getItem:ProjectReference
dotnet msbuild 35.ExperimentsV2/ExperimentsV2.csproj -nologo -p:Configuration=Release -getProperty:AssemblyName -getItem:Compile
```

После фактического Remove повторяется **только второй** вызов. Это evaluation-only: [MSBuild17.8+](https://learn.microsoft.com/en-us/visualstudio/msbuild/evaluate-items-and-properties?view=visualstudio) возвращает JSON без выполнения targets, когда `-target`/`-getTargetResult` не указаны. Не добавлять `-restore` или `-target:Build`. Если ремонт не нужен, сохранить один evaluated output35 и сослаться на него как на текущее состояние; не запускать второй тождественный вызов. Изменение ссылок/SDK/default items, которого нет на проверенном срезе, фиксируется как конкретная новая несовместимость, а не исправляется широким обходом solution.

| Назначенный план | Каталог квитанции |
|---|---|
| FIX-KHC | `artifacts/fbe/FIX-KHC/attempt-0001/build/` |
| TEST-ORDER | `artifacts/fbe/TEST-ORDER/attempt-0001/build/` |
| FIX-KHE | `artifacts/fbe/FIX-KHE/attempt-0001/build/` |
| Любой другой назначенный ID из 01 при использовании Tests | `artifacts/fbe/<точный-ID>/attempt-0001/build/`; ID берётся из назначения, не придумывается |

Сохранить `project-references.json`, `compile-before.json`, при ремонте `compile-after.json`, исходный/итоговый csproj SHA и фактические exits. Запись CreateNew, не overwrite. Для сравнения привести evaluated пути к одному полному пути и единому разделителю; итоговое множество должно быть **точно исходное множество минус архивные Rp01Micro sources**. Все прочие Compile items сохраняются, включая `Experiments/E13MethodLevelKnn.cs` и `Experiments/E14ContextActionKnn.cs`; native project references не меняются. Для архивных исходников сохранить список и SHA-256 до/после и требовать совпадения: исключение их из проекта 35 не удаляет и не переписывает сами файлы.

Файлы каталога результата не стираются; существующий terminal receipt сначала читается и тот же завершённый action не запускается повторно. Если текущие входные csproj/SDK не совпадают с квитанцией, она сохраняется как историческая: не перезаписывать её и не считать подтверждением нового состава; следующая адресная проверка назначается явно в пределах карточки. Не менять SDK/version, не создавать ещё один project и не начинать массовый solution repair. Другая ошибка имеет точный diagnostic и статус недоступного входа; она не превращается в обучение 0→0 или разрешение ослабить guard.

### Происхождение проверяемого learner

Общая предпосылка выше касается всех назначенных Tests-команд. Следующие точные assembly-проверки сохраняют область карточек 24–26 и выполняются в уже назначенных helpers; для других карточек сохраняются их собственные native-origin проверки. Не добавлять отдельный learner, H512 load или Train только ради проверки metadata.

Имена native assembly подтвердить evaluation-only командами, по одному вызову только для библиотек выбранного плана:

```powershell
dotnet msbuild 12.SemanticCompiler/SemanticCompiler.csproj -nologo -p:Configuration=Release -getProperty:AssemblyName
dotnet msbuild 11.DPO/DPO.csproj -nologo -p:Configuration=Release -getProperty:AssemblyName
dotnet msbuild 14.ContinuousLearning/ContinuousLearning.csproj -nologo -p:Configuration=Release -getProperty:AssemblyName
```

Для KHC/KHE нужен только первый вызов: Facade и соответствующий FileActionKnnPredictor/MethodKnnPredictor должны происходить из `SemanticCompiler`. Для ORDER нужны второй и третий: DpoTrainer — `DPO`, ProductionSupervisedTrainingStepExecutor — `ContinuousLearning`. В текущих native csproj это соответственно явный AssemblyName и стандартные имена проектов. Имена и exits сохранить в `assembly-identities.json` той же квитанции. В уже назначенном helper, **до первого Train/Forward/Compile**, проверить `typeof(...).Assembly.GetName().Name`, записать Assembly.Location и SHA-256 реально загруженного DLL; основным проверяемым B learner не может быть копия из ExperimentsV2, archived Runner или test-defined класса. Несовпадение останавливает эту руку до обучения. Это 0 дополнительных learning updates/Compile/Generate; отдельный тестовый процесс для таких assertions не назначен.

Существующие legacy controls, явно проверяющие E13/E14/E26 из ExperimentsV2, сохраняют свой исходный смысл и assembly. Их не переписывают и не выдают за новое native-learning доказательство. Ограниченный baseline A остаётся baseline с отдельным происхождением; это не разрешает заменить основной продуктовый B копией алгоритма.

После этого исполняется ровно адресная команда выбранной карточки с её прежним бюджетом. При отдельно назначенном внедрении, использующем Tests, допустимый diff включает только этот общий csproj-ремонт и собственные файлы карточки; алгоритмы, данные и assertion-контракты меняются только в перечисленных там местах. Сохранённые исторические результаты и будущая сборка native tests — разные свидетельства.


<a id="new-resource-fixtures"></a>
## 11. Готовые состояния и fixed corpus для FIX-GB / FIX-RMS / FIX-BW

Дополнение по `7a9fa58d55127ae88e93cd5d71c24a579f730ca3`. Это новые постоянные native tests успешных факторов, **не повтор historical hosts**. Векторы/tokenizer/optimizer каждой семьи остаются своими. Метаданные и hashes прочитаны из первичных результатов; бинарные LFS bytes этой документной сессией не загружались и не восстанавливались.

| ID / будущий manifest | Готовый источник / state policy | Приобретённый результат и бюджет |
|---|---|---|
| [FIX-GB](29-matmul-right-gradient-locality.md), `GradBLocality01/manifest.json` под общим fixture root | `experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/`; тот же источник S0/B0 identity, что FIX-TS, **не** joint/LC/OR. Policy=S0+RNG, reference=frozen B0, fresh DPO0 | D-T1/D-T2 по33/33 до/после; два DPO updates/arm. SFT producer1000 не повторяется, moments1000 не импортируются |
| [FIX-RMS](30-broadcast-coordinate-allocation.md), `RmsCoordinate01/manifest.json` | `experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/`; weights+AdamW400+RNG | Усвоен только T1; TRAIN1/2 и EXAM1/2, различение знаков не доказано. Четыре SFT updates/arm до404; producer400 не повторяется |
| [FIX-BW](31-bpe-weighted-chunks.md), `BpeWeightedChunks01/manifest.json` | Exact `BW01/in/fixture.json`, два раздельных corpus5×8; byte256→BPE320 | 64 приобретённых merges/профиль, compression172→87 и192→87, одинаковые A/B states. Четыре writer fits нужны для проверки изменённого Train; готовый after-tokenizer не заменяет writer-test. Последующий cold Load использует сохранённый результат без fit |

### FIX-GB: reuse паспорта segmented S0, не новый producer

Полные identities уже закреплены в [28 §3](28-dpo-tokenizer-snapshot.md#3-готовое-состояние-и-точные-данные). Новый fixture ссылается на ту же семью и сохраняет собственные KGB dataset metadata/digests из29. Сверенные hashes на7a9fa58 совпадают с опубликованным TS-паспортом.

| Input | Bytes | SHA256 |
|---|---:|---|
| `S0.weights.bin` | 47 899 519 | `a3cad45680c0953ffd03ca52a32d80f0afa5547015e493907af6811cf85cd9c5` |
| `S0.rng.json` | 1 573 | `f819248e615f50e41c22db16d5fac5116809857d195d3b9d149c9b6ec580ba9b` |
| `tokenizer.json` | 20 867 | `e51c91d85ec486863f1862e49b378fa9f34873ee31a993114ba41995f38e5066` |
| `generation.json` | 1 520 | `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40` |
| `data.source.jsonl` | 848 | `91f9409b03dce081e9cd8f431f52fa4157715c44c9d8d22d561fe87fd79a8a95` |
| `data.material.json` | 16 377 | `b4e35793c0051261e25d4aba12097d1ff71a271b15efc64519e96765dcbb2233` |

Runtime config hash `8cf38d269f806356191c614ccbf3bcafdb3c7e339f02e9d77bdade4e9fcd68ae` — не file hash generation.json. После restore adapter-value digest `9cf24bf8b5e13144c20c28ed91215683cd58c7a94c116be1b2a75056c3568439` — не file hash weights.

B0 source: `training-artifacts/models/csharp-code-documentation/ilearning-unit/rp01-csharp-small/v1/checkpoints/000001/weights.bin`, 47 899 519 bytes, SHA `a65887cc9d97c793d00e77632013adeba5e2d36efd7e6cebe7e49ba8a9b37911`; manifest `c5f4f7da80f427b8e9aac1dabded5849f0cbc6cb444b99d2bf7daac44ec0246d`, Step1/Parent=null/InitialBaselineNoUpdate. Полные source manifests — [bundle](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/bundle.json), [authority](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/authority.json).

Topology hash `f220de4520adbec3ddb8c23eabb0d485f6bb052f0c0dc9a50a8a5d9c3448cbfb`, LoRA identity `4df9bab2d5115d8368c3f36fdf6c4f31ba8ed43bb65200c41a5c563795d02c35`; seeds root20261014, policy stream `rp01-policy`, reference `rp01-reference`. Новые независимые objects в каждом arm, только policy получает S0. Готовый `S0.optimizer.bin` SFT1000 (426788bytes, SHA `a99aefd66bc756280a45361b58225b974dd8ed2abf8f435885974f9ecd5f29c5`) остаётся provenance, не input нового DPO optimizer.

### FIX-RMS: точный portable continuation400

[Bundle](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/_shared-learned-starts/h64-rms-coordinate/v1/bundle.json) и [Program restore](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/RMS01/a001/Program.cs#L350-L402) задают совместимые topology/bindings/framing.

| Input в h64-rms-coordinate/v1 | Bytes | SHA256 |
|---|---:|---|
| `S0.weights.bin` | 320 620 | `8721dd57b11fa61b11acf37c337611129891a1d3096ae0392a89455d6d58fdec` |
| `S0.optimizer.bin` | 7 384 | `539d11e925bbff676d9e5969d8c7bfb4326ff93aa1b93f934e8ff9bbbafc15d0` |
| `S0.rng.json` | 610 | `6215ff74662fb24ab856ffd149a243d4e5d07c5fa94fc16eb7ca1f7ec7793616` |
| `data.source.jsonl` | 153 | `b4b8e72cc4107c4aaec37adbde297c2b52e413cb714a5555c9b57785874ac9d1` |
| `data.material.json` | 5 225 | `3e73abfc521ca5c0615ef702ca39e467a1157822e002e85227165316b4eb4e73` |
| `tokenizer.json` | 283 | `569a552aa3fe3328aa3203c92bc18fcc6d5837fc30928dd2a244d3c38e5608c7` |

Topology identity `67ae4bbc81e1db3d99c0d3480bd3ee1aba6a4c33db9d809b6ef67796708b2674`, LoRA identity `901123df2486c06a87fd7585f481b481af96ca9d4ca755f14f513e2387a34a6e`; точные hash literals и seeds —30 §3. Tokenizer здесь байтовое отображение, **не** BPE320/V629; BPE.Train не вызывается. Import optimizer по4 именованным adapter bindings обязателен, fresh0 не допускается. Квалификация поля learning_qualified читается вместе с поштучным исходным report: она ограничена T1, не всем sign task.

### Запись и повторное использование

Общий planned loader проверяет реальные bytes/length/hash/LFS-pointer, keys/shapes/tokenizer/RNG/bindings и точный источник до training call. ReadCompleteBundle из historical host, проверяющий лишь COMPLETE/boolean, не заменяет этот loader. Missing/incompatible input блокирует только соответствующую будущую проверку; не обнуляет historical result и не назначает producer.

В двух последовательных стадиях одного canonical checkout источники читаются через явный `SourceRoot=C:/Projects/llm_mvp_core` по03 §9, без записи в общую семью. A receipt сохраняется до product diff, B после него получает те же проверенные donor bytes в новых native objects. Final test artifacts пишутся один раз в `artifacts/fbe/<ID>/attempt-0001/learning/<arm>/state` (BW ещё profile), с actual identity и source reference; B не перезаписывает A и не делает его повторный fit. Это test state, не автоматически опубликованная production model. GB/RMS after-assertions используют тот же loaded model и сохранённые values; BW cold-readout использует canonical Save/Load. Ни один новый downstream assertion не повторяет acquisition. Отдельного обучения «для репорта» нет.

<a id="wiki-feature-key-state"></a>
## 12. FIX-WK: неизменяемый legacy donor и один native v2 writer

Поздний source `32d8887983c347f3bf3d674a5e9414d57eaec75b`, точный4TRAIN/6EXAM набор, original-byte hashes и reuse — [32§3](32-wiki-feature-key-isolation.md). Профиль D07/оркестрация. Сохранённый `experiments-v2/RP01-MICRO/WK01/results/attempt-001/A/after-snapshot.json` используется для действительного legacy restore без Train. Historical B содержит уже namespaced keys, но не имеет явной версии: это только pinned числовой oracle, не загружаемый production donor.

Новый паспорт `Tests/Fixtures/ExperimentFixes/v1/WikiFeatureKeys01/manifest.json` разделяет file SHA original CRLF bytes, canonical parameter digest, source origin, execution core и retrieval commit. Fixture bytes3154, SHA `90d70b9c6998d921465720ac743e095aade7d3f5126b937ae7dd2ee6305ce63d`; не добавлять конечный LF при копировании. Полный набор и состояние небольшие; исходные results не переписываются и не удаляются.

Для проверки **самого native writer** назначены одна empty init и один4row/18epoch fit на плечо. Этого достаточно для измеренного0→3/6; отдельный bootstrap/готовый neural checkpoint не нужен. Результат сохраняется один раз в `artifacts/fbe/FIX-WK/attempt-0001/learning/{A,B}/state/after.snapshot.json`. Contracts читают legacy A и новый native B из этих назначенных источников; не делают дополнительный fit для каждой проверки или отчёта. Вся цепочка с existing guards содержит10Train calls, только2непустых/144presentations — точный бюджет32, не четыре calls по видимому main без hidden helpers.

Новый native B snapshot явно хранит `FeatureKeyCodecVersion=namespaced-length-v2`; отсутствующее/null поле означает legacy-joined-v1. FromSnapshot/ToSnapshot/WithGateEvidence сохраняют интерпретацию weights и не переносят passed gate на другую модель. Codec version независима от label contract15/17 FIX-WT. Для unknown version — явный отказ до чтения; по форме keys формат не угадывается. Ограниченный17label oracle/его preflight назначен в32, не оставлен на решение исполнителя. E2E20 использует codec того же выбранного saved state и добавляет0Train.

<a id="wiki-once-pos-fixtures"></a>
## 13. FIX-WC и TEST-POS: новые writer-проверки и точное повторное использование

Текущий source — `1fa9bac16ff209646f7abbd29dad2461df3ed538`. [FIX-WC33](33-wiki-training-feature-reuse.md) имеет готовые JSON snapshots, а у [TEST-POS34](34-pos-bigram-learning-coverage.md) native save/load отсутствует. Эти случаи не смешиваются; обе карточки заранее назначают единственный минимальный fit, необходимый для проверки самого writer. Новых нейронных producers нет.

| План / будущий fixture | Источник и способ использования | Ограниченный расход |
|---|---|---|
| FIX-WC / `Tests/Fixtures/ExperimentFixes/v1/WikiFeaturesOnce01/` | Exact `experiments-v2/RP01-MICRO/WC01/fixture.json` и `results/attempt-001/A/{before-snapshot.json,after-snapshot.json}`, raw A/B. До native fit восстановить готовый zero snapshot через FromSnapshot; после fit сохранить native состояние. Cold/compatibility assertions читают готовые historical/native after snapshots, не вызывают Train повторно | Основные A/B: по1Train8строк×18эпох, по4before/4after. Полный назначенный пакет с existing empty guards и cross-call isolation:10Train=6empty+4nonempty,324presentations,38reader calls; детали33. Historical7mistake updates не считать отсутствующим native counter |
| TEST-POS / `Tests/Fixtures/ExperimentFixes/v1/PosBigram/` | Новый exact6TRAIN/4EXAM из34, отдельный gold; historical E25 corpus/table не восстановлены. Один native PatternClassifier(k3) в общем IClassFixture, одна неизменяемая before/after-квитанция используется обоими tests. JSON уроков не называется checkpoint | 1Train6строк,8Predict; дополнительные6existing rules дают0Train. Native export/import нет, отдельный process не называется restore |

Historical WC использует прежние15labels и legacy feature codec, восемь маршрутизационных уроков одной темы и четыре входа EXAM. Для native A/B codec и labels закрепляются одинаково в обеих руках по33; уже отдельно принятые WK/WT сохраняются на общей базе. Historical hash — oracle только для исходных15labels/legacy; при иной принятой базе oracle — завершённый native A, не откат чужого изменения. Сохранённый after годится для reader/restore, но загрузка послеобученного состояния вместо изменённого Train не проверила бы экономию подготовки. Поэтому две новые native writer-руки назначены явно; архивный WC01 host не запускается. Готовый zero snapshot устраняет дополнительные пустые fit в основном learning test. Cache живёт только внутри Train, не входит в state; gate остаётся missing/false. Изменение label space или codec — отдельный фактор, не автоматическое условие WC.

Для POS массивы передаются без мутации после Train, поскольку native AddRange сохраняет ссылки. Fixture содержит `train.json`, `eval-inputs.json`, `panel-gold.json`, `provenance.json`; exact ordered IDs, labels, POS schema, k и реальные hashes закрепляются до вызова learner. Аналитически ожидаемые2/4→4/4 ещё не измерены; это критерий нового теста, не пересчёт historical87,5%. При отсутствии файла/несовпадении hash/namespace — конкретный отказ до Train, без выбора другого набора, скрытого fit или подстановки иной модели.

Общий loader02 проверяет эти малые fixtures в выбранной ветви, не требует неотносящегося H512/optimizer/RNG. Пути выходов фиксированы в карточках; родительские source snapshots остаются read-only. Исходные reports/эксперименты не редактируются и не удаляются. Потребители уже полученных receipts не запускают новое обучение ради формирования отчёта.


<a id="ks-nh-sk-fixtures"></a>
## 14. FIX-KS / FIX-NH / TEST-SK: три разные учебные памяти

Дополнение проверено на `3acae42f7ef474e449737a57f0cf064921c4bbc7`. Карточки35–37 содержат точные литералы, hashes, model identities и terminal-правила; эта карта связывает их с общим loader и исключает скрытую повторную подготовку.

| План / точный будущий паспорт | Источник, состояние и проверяемый переход | Повторное использование и полный расход |
|---|---|---|
| [FIX-KS35](35-method-knn-target-scope.md), `Tests/Fixtures/ExperimentFixes/v1/MethodScope/provenance.json` | Historical MKSC01:4TRAIN-файла/8методов,4direct inputs и отдельный gold. Для настоящего Compile отдельный `facade-inputs.json`: ко всем четырём historical source добавлен одинаковый exact `// csharp\n`. Все8 raw/derived source hashes закреплены в35;4readonly Extract до fit подтверждают неизменность PackedMethod/KindBag. k=1 явно, defaultk5 сохранён | Main:1pinned-A fit и1native-B fit; B direct/Compile читают ту же таблицу. С compatibility/contracts всего11successful fits,81method presentations,52pairs,28Predict,14Compile/ожидаемых14E05fits; отдельно2rejected Train без замены памяти. Нет native Save/Load: `state_kind=single-fit-in-memory-table`; receipt/pairs JSON не checkpoint |
| [FIX-NH36](36-dpo-scored-row-head.md), `Tests/Fixtures/ExperimentFixes/v1/NHeadSelectedRows01/manifest.json` | Готовый `experiments-v2/RP01-MICRO/_shared-learned-starts/h512-dt-segmented/v1/`; тот же S0 hash `a3cad456…`, independent frozen B0 и fresh DPO0. Exact `NH01/train.jsonl`,848bytes/SHA `91f9409b…`, D-T1/D-T2 по33targets. Полные hashes/LoRA/RNG —36§4 и11 выше;33/33 на каждой строке требуется до первого update | Два последовательных этапа в одном canonical checkout по03§13. В каждом arm отдельные native objects:2DPOupdates; A+B4updates/16trainingforward/8backward/8diagnosticforward/4optimizer. Контракты0updates; old tiny tests24additionalforward A+B. Producer1000/SFTmoments не повторять/не импортировать; reports/E2E читают те же receipts |
| [TEST-SK37](37-structural-knn-learning-coverage.md), `Tests/Fixtures/ExperimentFixes/v1/StructuralKnn/manifest.json` | Новый фиксированный6TRAIN/4EXAM C# fixture из37; historical E09 corpus/table не восстановлены. Native `TrainStructuralKnn` один раз хранит96within-file pairs; query labels не передаются learner. Gold открывается scorer только после сохранения всех outputs | Один IClassFixture,4Compile BEFORE+4AFTER; два Facts читают одну квитанцию. Вместе с existing guards12Compile/ожидаемых12E05fits,68structuralPredict и ровно1structuralTrain. Save/Load API отсутствует: reuse same instance, не fake checkpoint; ожидаемое0→4/4 ещё не измерено |

Для KS точный паспорт — `provenance.json`, указанный в таблице; loader получает этот путь явно, не ищет ещё один manifest. Пять JSON35 и их назначение не меняются. Для SK `manifest.json` ссылается на `train.json`, `eval-inputs.json`, `panel-gold.json`, `provenance.json`. Состав/порядок уроков закреплены автором; scorer может знать gold, native learner — только TRAIN и текущий input. Ни fixture hashes, ни expected label не подаются как обучающий признак.

KS и SK проверяют writer обучаемой таблицы, у которой нет native persistence. Поэтому назначен один минимальный новый fit основного native экземпляра; для последующих assertions используется уже полученный результат. Restore, экспорт таблицы через reflection или сериализация свидетельства под видом checkpoint не добавляются. У NH существует пригодный сохранённый старт: его отсутствие/LFS pointer/несовпадение hash блокирует конкретный этап до model call, без нового acquisition producer или выбора другого веса.

Все три loaders используют явные `XIIGEN_FBE_SOURCE_ROOT`/`XIIGEN_FBE_OUTPUT` по карточке; ownership создаётся атомарно до первого учебного действия. Нейронные starts остаются read-only. Будущие test states/receipts пишутся в `artifacts/fbe/<ID>/attempt-0001` по03 и карточке, не в historical results. Уже завершённая квитанция не запускается повторно ради отчёта, следующего assertion или E2E.

<a id="ast-sibling-fixtures"></a>
## 15. FIX-AST: фиксированные AST01 данные и одноразовые native состояния

[Карточка38](38-structural-knn-sibling-cache.md) — единственное новое назначение на срезе `d2ec9b309162deab975ef25435f5314a9567d456`. Паспорт: **планируемый** `Tests/Fixtures/ExperimentFixes/v1/AstSiblingCache/provenance.json`; источник — exact narrow/wide/heldout из `experiments-v2/RP01-MICRO/AST01/data/`, все размеры/hashes закреплены в38§4. Данные под Tests как `.cs` не копировать. Topic=программирование, profile=C# AST next-kind, learner=`SemanticCompiler.Prediction.StructuralKnnPredictor`, k=5, Roslyn4.12.0, codec=`SyntaxKind%200`.

`state_kind=single-fit-in-memory-table`: у native learner нет Save/Load. Historical state-after является свидетельством, а не checkpoint для private-field import. Для измерения source→learned table нельзя заменить обучение загрузкой готовых vectors. Создаются только назначенные native A/B состояния, затем snapshots/readout повторно используются assertions/scorer без fit. Cache содержит SyntaxNode только внутри одного дерева/Train и никогда не сохраняется.

Direct narrow/wide: по одному fit на профиль/руку,5122/4106пар и16BEFORE+16AFTER queries. Facade A/B: по одному batch `[narrow," ",wide]`,9228пар с FileId0/2, один Compile до/после на том же instance. Полный бюджет38:6structural fits/36912pair writes/184Predict/8Compile/8побочных E05 fits с учётом old guards. Output: `artifacts/fbe/FIX-AST/attempt-0001/resources/<A|B>/<narrow|wide|facade>/`; отдельный ownership до каждого capture. TEST-SK, его96пар, neural donors и E2E producer не подключаются автоматически.

Loader использует тот же явный root/output contract и hashes до обучения. `comparison.json` исторического AST01 на этом commit — LFS pointer; читать четыре per-arm raw и compare-stdout либо получить **точный** LFS payload. Pointer не передавать JSON parser; не переписывать исходный результат. В будущих reports различать costs обучения, acquired compact-kind ответы и ещё не измеренную semantic quality.

<a id="ng-br-fixtures"></a>
## 16. FIX-NG / FIX-BR: два новых фиксированных учебных входа по70062c

Источники впервые опубликованы на `70062c090a55d7350e5eadb2606dd3f07953692f`; это дополнение к прежним состояниям, без их замены. [Карточка39](39-null-guard-training-label.md) и [карточка40](40-bpe-realized-merge-objective.md) содержат полные literals, native API, hashes и бюджеты.

| ID | Точный future fixture root | Начало и сохранение |
|---|---|---|
| FIX-NG | `Tests/Fixtures/ExperimentFixes/v1/NullGuardLabel01/`: manifest.json, train.tsv, exam.tsv, historical-A.json, historical-B.json | 12 ordered C# TRAIN /4 EXAM, k3/dim30. Native persistence нет: один пустой facade и один настоящий fit на arm, затем тот же instance и readonly receipts. Не импортировать rows в private поле |
| FIX-BR | `Tests/Fixtures/ExperimentFixes/v1/BpeRealizedMerge01/`: manifest.json, fixture.json, initial-tokenizer.json | Готовый byte-only S0 из BR01 A-initial-state.json. Три captures A-legacy/B-legacy/B-realized загружают его, каждый делает один содержательный Train257 и сохраняет собственный after для native cold Load без Train |

NG: TRAIN1213bytes/`21bad78046eba694b560b7ff28aef5b034fb0439e30c85332ca28f95e4e5c1c4`; EXAM443bytes/`22de0f5c81e48a95d7f258c02dc814d315d8662bbb97632ae491cab69f77413b`. Три примера на семейство обязательны при k3; gold не подменяет native Label. Семьи, относительный порядок и3:9 gold не перебалансируются. Captured state-after отражает реальные метки и пригоден для отчёта, не является checkpoint для восстановления API, которого нет.

BR: fixture458bytes/`979c0820a3681360e1f75027a0ab40bbe859cfd116010da914bd76559ea6435e`; S0 3261bytes/`3bba3bc0d53fb07a5de4d28839f14c2b296b8034de37e5d561b123a9dcf95bf4`; corpus27bytes/`b07ec7b8dbd7f319c2997554312f70853ba0cd10264dd3e329b33424284d684c`. LF/no final LF задан в40. Byte initialization не повторяется. After загружается для проверки persistence, но не подставляется вместо writer-test. Lambda/comment/neutral — профили одной темы; BR257 не заменяет другие BPE320 states и не меняет их fingerprints.

Static manifests сохраняют данные, recipe и budgets; фактические A/B commit и assembly identity принадлежат intent/binding/receipts. Наличие общего output не разрешает перезаписать A или переучить его. Полные бюджеты include existing и B-only boundaries в39/40, E2E получает0 дополнительных fits.

<a id="new-measured-fixtures"></a>
## 17. Тринадцать новых кандидатов: записать один раз, восстановить назначенное состояние

Source одиннадцати сверок — ebaf7760a36d43172bd88317ff469edfd09a615b; поздний PQ01 — a87fef91ba52e81c69dd63fbc094daa776f57095, HCL01 —38c05f2e9a0fd5ec5da269ea078daf298b637d3b. Native recipients не менялись между этими source-срезами. Ниже перечислены **будущие** fixture directories внутри Tests/Fixtures/ExperimentFixes/v1/. Файлы, receipts, hashes, literals и loader API назначены соответствующей карточкой; их существование после этой документной правки не заявляется. Бинарные shared starts не копируются в тринадцать новых каталогов. Каждый manifest ссылается на свой неизменяемый donor и проверяет реальный payload, а не текст Git LFS pointer.

| ID / карточка | Fixture directory | Откуда взять обученное состояние и как использовать |
|---|---|---|
| [FIX-AGC41](41-grad-accum-copy.md) | GradAccumCopy01 | experiments-v2/RP01-MICRO/_shared-learned-starts/h512-postfix-one/v1; восстановить весь назначенный Student/adapter/tokenizer/RNG. DPO reference=B0, DPO optimizer свежий0→1; SFT producer1000 не повторяется |
| [FIX-FBM42](42-frozen-broadcast-map.md) | FrozenBroadcastMap01 | Тот же h512-postfix-one/v1, отдельные экземпляры A/B и отдельные receipts. Один DPO update на руку; AGC не включается тайно в B |
| [FIX-VA43](43-verifier-apostrophe-learning.md) | VerifierApostrophe01 | QVA01: две точные строки verifier из native builder, восемь независимых EXAM. По два UnitTrain на руку нужны для проверки writer; сохранённый unit snapshot затем импортируется без нового fit. Парафраза и typographic apostrophe сохраняют gold=true |
| [FIX-RRD44](44-rms-row-reduce.md) | RmsRowReduce01 | _shared-learned-starts/h64-rms-row/v1: полные weights, AdamW и RNG400; четыре SFT updates400→404. Это не donor старого RMS01 |
| [FIX-ATS45](45-attention-transpose-strides.md) | AttentionTranspose01 | _shared-learned-starts/h64-attn-transpose/v1: отдельная семья с теми же обязательными полями continuation400→404 |
| [FIX-JO46](46-training-json-options-reuse.md) | TrainingJsonOptions01 | Сохранённый empty checkpoint JO1 и точные P125 TRAIN8/EXAM24 из карточки. Одна native batch/руку измеряет ingress; after checkpoint используется для restore/readout без нового Train |
| [FIX-SSR47](47-silu-sigmoid-reuse.md) | SiluSigmoidReuse01 | _shared-learned-starts/h64-silu/v1: полный learned start400 и четыре назначенных updates. READOUT training=false; cache только при training=true |
| [FIX-IM48](48-learned-route-body-identity.md) | IdentityRoute01 | Полные /0/raw и /1/raw из docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/IDENTITY-M1-01/A/states.json. Проверить хеши и schema3; штатные Import/Save/Load, **0 новых Train** |
| [FIX-KR49](49-router-learned-key-normalization.md) | RouterLearnedKey01 | profiles из experiments-v2/RP01-MICRO/KR01/results/A.json, сверенные с B.json; прежние426B. Явный empty-profile control и обученный snapshot читаются native scorer; **0 новых Train** |
| [FIX-HNQ50](50-hnsw-training-query-norm.md) | HnswQueryNorm01 | Исторический index-only снимок не содержит tokenizer/labels всего Unit. Назначенные warm4 и main8 native fits используют сохранённые уроки и один test-owned RNG7331 на arm; subsequent readouts берут тот же instance. Native default RNG не меняется |
| [FIX-PQ51](51-pos-query-bigram-reuse.md) | PosQueryBigram01 | Сохранённый PQ01 fixture128TRAIN/32EXAM; генератор не повторяется. У native PatternClassifier нет Save/Load: один Train/руку, затем чтение того же instance и raw, без восстановления private-памяти |
| [FIX-IDX52](52-translation-training-identity-index.md) | TrainingIdentityIndex01; общий TranslationMemory | Использовать опубликованное полное состояние до изменения учебной памяти, затем только назначенный Train; 128batch preparation не повторяется. После writer сохранить state для дальнейшего чтения без нового fit |
| [FIX-HCL53](53-hierarchical-local-history.md) | HierarchicalLocalHistory01 | Полного checkpoint BigramMarkov в raw нет, Save/Load отсутствует. Один S0 heartbeat и два назначенных S1 canonical/permuted на руку; модели повторно читаются без Train для assertions. Счётные таблицы не выдаются за neural/LoRA state |

Учебные темы не смешиваются: нейронные code-sign/postfix профили не становятся данными selector; EN→RU — направление внутри перевода; LAB-A/LAB-B и code/docs — классы маршрутов внутри оркестрации. Отрицательные запросы проверяют тот же router, а не обучают его постороннему предмету. Writer-тест VA/JO нельзя заменить простым импортом after: тогда изменённый участок не выполнится. Reader-тест IM/KR, напротив, не получает нового fit ради повторения уже сохранённого результата.

H64 состояния содержат приобретённый ответ, но только1/2 TRAIN и1/2 EXAM: они квалифицируют реальное обучение для ресурсного сравнения, не освоение общего правила. Диагностическая пустая память IM/KR — абляция; её0-показатель не выдаётся за успех. Missing/corrupt donor означает конкретный отсутствующий вход и запрет объявлять измерение; это не разрешение тайно заменить модель, бюджет, данные или запустить producer.

После завершённой A один и тот же output artifacts/fbe/<ID>/attempt-0001 сохраняется для B, сравнения и русского report.md. Уже существующий completed A не является конфликтом занятого каталога. Ошибку восстановления исправляют по точному loader-контракту карточки, не удаляя результаты и не выбирая следующий results-NN. Семантические hashes не вычисляют от вручную сокращённого state; из JO убираются только заранее перечисленные runtime-поля для сравнения, а raw сохраняется целиком.

## 18. Состояния связанных владельцев и отсутствие повторного acquisition

[Карта56](56-measured-delta-and-existing-coverage.md) и [контракт57](57-native-transfer-contract-gaps.md) дополняют единую очередь, не создавая девять новых donor families.

| ID | Назначенный источник | Что сохраняется |
|---|---|---|
| IL / SEXP | PA01/repos/B, accepted checkpoint000002; [внешний IL§3](../fixes-based-on-experimentsperiments/55-supervised-ingress-length.md) и [41§3](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md) | H16;weights/moments/step1000 и RNG принятия. Не запускать PA1000 ещё раз. Private manifest не получает product authority. IL mask включает LF:42цели; SEXP40 — другой scorer, не приобретение двух токенов |
| SC | RESTORE-MINIMAL-SET/results/trained-weights.bin, тот же фиксированный donor из внешнего41§5 | H16 weights-only; policy/reference одинаковы, fresh DPO0. CalibrateA200 не является compareparent и не повторяется |
| MB / AC | _shared-learned-starts/h64-rms-row/v1 | Полные weights/AdamW400/RNG своей семьи;4SFT/arm, acquiredT1. Не заменять на новый seed или h64RPE |
| RPE / CS | h64-rope-table/v1 и h64-causal-scores/v1 соответственно | Отдельные полные400-step bundles; совпавшие topology sizes или stream name не объединяют identity |
| FBC | h512-dt-segmented/v1; точный loader [54§4](54-frozen-base-check-scope.md) | **S0 и в policy, и в reference**; новый DPO optimizer0. Не заимствовать профиль policyS0/referenceB0 из TS/PD/LC |
| LFT | Tests/Fixtures/ExperimentFixes/v1/FTR01/S0/, [55§4](55-frozen-linear-transpose-cache.md) | Исторического полногоS0 нет. Сначала читать точный назначенный bundle; только при отсутствии один bounded producer250, сохранить weights/moments/RNG, требовать2/2. Потом8updates A+B. Непригодный bundle не перезаписывается, подготовка не продлевается |
| FSR / SVD | Frozen source fixtures прежних внешних51/53 | Native persistence нет. FSR4fits/30rows/44Compile и companion расходы —57§4; SVD6fits/9000tokens/30predict —57§6. Matrix/receipt JSON не называется восстановленным native checkpoint |

Все terminal native результаты сохраняются один раз под `artifacts/fbe/<канонический ID>/` в назначенной владельцем leaf; readback/report/E2E читают их без повторного fit. CreateNew ownership, raw/source hashes, ABI/keyed state и точное framing проверяются до воздействия. Отсутствующий обязательный donor даёт конкретный failure; единственное заранее назначенное исключение producer — LFT55, а не общий допуск учить заново любую модель.

### Позднее дополнение FIX-OSR / 58

OPT01 использует уже квалифицированный H512 segmented donor: S0 восстанавливается в policy и reference, а DPO начинает со свежего optimizer step 0. Подготовительное обучение не нужно. Точные байты TRAIN, metadata, loader и паспорт состояния назначены в [плане 58](58-optimizer-step-metadata-read.md). Сходство с профилем FBQ не разрешает заменить хеш набора OPT01. Snapshot и rollback по-прежнему сохраняют полные moments; меняется только способ чтения step для RNG capture.


### Дополнение восстановления после согласования 82860557

У PA donor `ResetAtAdoptionDropoutZero` означает сохранённый RNG snapshot принятия, а не доказанную непрерывность RNG прежнего тысячного шага. Checkpoint ordinal2 и AdamW step1000 — разные идентификаторы. Хэш Git LFS pointer не заменяет хэш model payload; metadata без доступных bytes не подтверждают native restore. Эти проверки входят в назначенный loader, с0новых fits. Для LFT полный portable AdamW и typed JSON RNG, проверки cursor/state и единственный producer назначены в [55§7](55-frozen-linear-transpose-cache.md#7-недостающие-условия-защищённого-native-переноса); остальные donors не получают право на скрытый bootstrap.

<a id="sep133-fixtures"></a>

## 19. FIX-SEP: два готовых состояния и явный профиль учебного readout

Единственный новый владелец — [62 / FIX-SEP](62-supervised-framing-and-learning-readout.md). Источник — завершённый SEP01/133: по 60 настоящих SFT updates для Abs и Sign; новые product tests используют сохранённые результаты этого обучения. Это **две независимо обученные модели**, а не одна модель, выучившая обе строки.

| Назначение | Существующее состояние | Использование |
|---|---|---|
| Abs | `experiments-v2/RP01-MICRO/SEP01/results-01/producer-abs.bin` | A без LF: 22/23; B в SFT-формате: 23/23 |
| Sign | `experiments-v2/RP01-MICRO/SEP01/results-01/producer-sign.bin` | A без LF: 20/21; B в SFT-формате: 21/21 |
| Новый manifest | Планируемый `Tests/Fixtures/ExperimentFixes/v1/SupervisedFraming01/manifest.json` | Ссылки на эти payload, config/tokenizer identity, две строки, profile/digest и historical raw; веса не копировать |

Точные SHA256, размер каждого payload 47 899 519 байт, config и полные тексты закреплены в [62§5](62-supervised-framing-and-learning-readout.md#5-сохранённые-данные-и-восстановление-без-нового-обучения). В этой сессии проверены pointers и raw, а не выполнена загрузка больших весов. LFS pointer не является моделью. Перед будущим readout нужны настоящие payload с указанными OID; отсутствие файла не разрешает скрытое acquisition.

Восстановление повторяет native topology preparation из SEP01, затем `LlmModel.LoadCheckpoint`; adapters после загрузки не инициализировать заново. Эти файлы не содержат подтверждённого продолжения AdamW/scheduler/RNG после 60 updates. Поэтому разрешено read-only чтение learned state, но не объявление полного продолжения обучения и не создание «восстановленных» moments из нулей.

Профиль SFT — `sft-joint-lf-v1`, один joint Encode строки `prompt + LF + response`; exact DPO сохраняет `prompt-response-segments-no-separator-v2`. Response targets извлекаются только после проверки actual IDs и префикса. Несопоставимые targets запрещают A/B вердикт; диагностическая проблема границы не удаляет строку из действующего full-sequence SFT.

Квалификация переноса: один ограниченный native readout fixture, **6 Forward, 0 SFT/DPO updates, 0 Generate**; включены A/B двух моделей и два cold readout. Никаких новых producer, сетов, доменов или повторов 120 updates. Это проверка результата уже проведённого обучения, непосредственно используемого условием остановки в исходном приборе. Полный E2E получает только metadata этого профиля по [20§17](20-prompt-domain-e2e.md#sep133-e2e), без дополнительных вызовов ради заполнения отчёта.

## 20. Уточнение 25 октября: один сохранённый Abs-state для N63C и SEP

Проверка на `7e79eb156f418dc76d699966ca948d4616b36cdd` установила одинаковые Git LFS pointers у назначенного CDB01 donor и `SEP01/results-01/producer-abs.bin`: Git blob `bdac5a6faced90cb8dc35618f737a3e69db955af`, payload SHA-256 `59810def905c4ead2f22985c5e1d525f59feb45590256eca8b492a293206d44f`, размер 47 899 519 bytes. Это отсутствовавшая связь между двумя существующими планами, а не новое приобретение навыка. [Доказательства и граница проверки](04-evidence-and-novelty.md#n63c-sep-shared-state-evidence).

Обе действующие таблицы этого документа теперь назначают CDB01. Старое имя N63C-PROBE сохранено только для исторического результата. Совпадение показателя 22/23 само по себе не разрешает замену состояния: у старого и нового опытов различаются acquisition recipes.

В локальных fixture-паспортах 17 и 62 можно ссылаться на один проверенный неизменяемый payload; большие веса не копировать и не переучивать. Путь CDB01 остаётся назначенным входом 17, путь SEP — входом 62; общий OID не разрешает автоматически выбрать другой файл при отсутствии назначенного. После проверки payload bytes загружать независимые model instances. N63C создаёт новую учебную эпоху с fresh optimizers, SEP выполняет только чтение. Передавать изменённый экземпляр из N63C в SEP запрещено.

Для N63C фиксируется `readout_role=retention_gold`: исходный правильный `T1.Chosen`, один `probe_digest` и одинаковые target IDs во всех состояниях A/B. Фактически поданный `Rejected` в A хранится как training evidence, но не становится gold проверки удержания. `sequence_origin=acquired_gold` отличает этот readout от проверки текущего supervised input. [Точный oracle, бюджеты и ремонт тестов — 17§11](17-dpo-sft-correction-collision.md#n63c-fixed-gold-readout).
