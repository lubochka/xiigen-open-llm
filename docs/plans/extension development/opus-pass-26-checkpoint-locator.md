# Pass P26 — Жизненный цикл checkpoint / locator (реальное состояние)

Автор: Opus 4.8 REAL-STATE ASSESSOR. Дата: 2026-07-08. Ветка репо: DesignToCode-clean.
Оценка ведётся по ЖИВОМУ коду `C:\Projects\llm_mvp_core`, а не по copy/status пакета `extension development`.

## Что именно проверялось в этом проходе

Фокус P26 — РЕАЛЬНЫЙ механизм: (а) promotion checkpoint'а в `models/`, (б) update локатора,
(в) rollback — через `25.ModelServer` (`IModelRepository`, `IRemoteUnitLocator`, `IModelServer`,
`ModelEndpointRegistry`, `RemoteLogicalUnitProxy`), `models/index.json`, publisher'ы
`42.ProjectWikiDomain/*ManifestPublisher.cs`, потребление ассетов в
`43.SmartSkillsDomain/XiigenExtensionApiService.cs` (`LoadCommonAssets`), lifecycle
`Phase08CommonUserTwoLegLifecycle.cs`, и промоушен-гейт домена `44.DesignConversionDomain`
(`DesignConversionPromotion.cs`, `DesignConversionCheckpointContracts.cs`).

Прочитано целиком/по срезу: `25.ModelServer/Local/LocalModelRepository.cs` (86),
`25.ModelServer/Remote/InMemoryRemoteUnitLocator.cs` (45), `.../Contracts/IModelRepository.cs`,
`.../Contracts/IModelServer.cs`, `.../Http/ModelEndpointRegistry.cs`,
`.../Remote/RemoteLogicalUnitProxy.cs`, `models/index.json` (131),
`44.DesignConversionDomain/DesignConversionPromotion.cs` (410),
`DesignConversionCheckpointContracts.cs` (62), срез
`XiigenExtensionApiService.cs:1389-1475` (`LoadCommonAssets`), срез
`Phase08CommonUserTwoLegLifecycle.cs:740-800` (rollback). Плюс bounded-грепы связей.

## Дедупликация против предыдущих проходов

НЕ повторяю уже занесённое: P05 (`IModelRepository.UploadAsync` no-op транспорт;
`IRemoteUnitLocator` in-memory непереживающий рестарт; `.xiigen`-writer существует),
P09 (test-only promotion локатор Claude), P11 (Claude `activated_asset_root` конфляция;
runtime-load checkpoint ломается `..._CHECKPOINT_REF_MISMATCH`; `production_locator_update_done:false`),
P19 (Codex `TargetLocator` = load-root selector, не реестр; Codex rollback = directory-swap;
«locator update» = закрытый non-claim на границе), P22 (OrchestrationReasoning continue = append;
shipped stateId удвоен; нет runner-report), P25 (промоутнутые ассеты без loss/reward-margin;
continue-probe = no_op; тренировка регрессировала). P26 добавляет НОВОЕ на уровне САМОГО
механизма promotion/locator/rollback (проводка `25.ModelServer` ↔ рантайм, integrity-гейт,
наличие/отсутствие promotion-гейта, исполняемость rollback-решения).

## Ключевой вывод P26 (человеческим языком)

Вся инфраструктура «модель-сервера» из `25.ModelServer` (репозиторий, локатор, публикация,
proxy) **не подключена к рантайму трёх расширений вообще**. Три промоутнутые модели
(`prompt-to-task-v1`, `orchestration-reasoning-v1`, `skill-selection-v1`) грузятся расширением
по ЖЁСТКО ЗАШИТЫМ файловым путям (`Path.Combine(root,"models","prompt-to-task","prompt-to-task-v1",...)`),
минуя `IModelRepository`/`IRemoteUnitLocator`/`index.json`. Значит:

- **«promotion»** = ручная выкладка файлов в `models/` (программного writer'а нет; в `index.json`
  видны цепочки `metadata_hash_repair` — руками правленные sha);
- **«locator update»** физически не может повлиять на то, какой checkpoint грузит рантайм
  (рантайм локатор не читает; версия зашита в код — v2 потребует пересборки);
- **integrity на загрузке отсутствует**: рантайм записывает объявленный `CheckpointSha256`
  как provenance, но НЕ пересчитывает хеш файла и не сверяет его с манифестом/индексом;
- **promotion-гейта у трёх расширений нет** (в отличие от `44.DesignConversionDomain`, где
  есть настоящий `DesignConversionPromotionGateEvaluator`), а сам этот сильный гейт —
  **test-only** (нет ни продового uploader'а, ни продового вызывателя) и по построению
  никогда не заявляет model-server promotion (`ModelServerUploadAttempted=false`,
  `local_receipt_only`);
- **rollback как ОПЕРАЦИЯ отсутствует**: `DesignConversion` умеет РЕШИТЬ `Rollback`, но
  publisher его не ИСПОЛНЯЕТ; единственный исполняемый core-rollback (`Phase08`) восстанавливает
  ТЕКСТ манифеста, без post-rollback load/inference проверки, и достижим только из тестов;
- **`IModelServer`** (Publish/OpenWeights) — мёртвый контракт (0 реализаций), а
  `RemoteLogicalUnitProxy` просто ЭХОит вход (не тянет веса, не делает inference).

## Таблица НОВЫХ дыр (P26)

| # | Компонент | Реальный статус | Доказательство (file:line) | Дыра | Шаги закрытия |
|---|---|---|---|---|---|
| G1 | Потребление promoted-checkpoint'ов расширениями минует весь `25.ModelServer` | MISSING WIRING | `XiigenExtensionApiService.cs:1403-1404,:1422-1424,:1442-1444` грузит по `Path.Combine(root,"models",...)`; `git grep -E "IModelRepository\|IRemoteUnitLocator\|index.json\|UploadAsync\|LocateAsync\|LoadWeightsAsync" -- 43.SmartSkillsDomain/*.cs`→0 | Слой model-server (repository/locator/publish/proxy) для трёх расширений НЕ используется ни на записи, ни на чтении. «Promotion через 25.ModelServer» и «discoverable через IRemoteUnitLocator» (Trainable Unit pattern) не выполняются: рантайм читает сырые файлы по жёсткому пути. Отдельно от P05 (транспорт no-op) и P19 (`TargetLocator`=выбор папки): здесь весь слой обойдён. | Подключить загрузку расширений через `IRemoteUnitLocator.LocateAsync(unitId)`→манифест→checkpoint путь; publisher, пишущий `models/`+`index.json`+регистрацию; доказать, что смена записи локатора меняет активный checkpoint без пересборки. |
| G2 | Версия checkpoint'а зашита в код; переключателя активной версии нет | STUB / no version indirection | `XiigenExtensionApiService.cs:1403` `"prompt-to-task-v1"`, `:1422` `"orchestration-reasoning-v1"`, `:1442` `"skill-selection-v1"` — литералы; нет чтения «current version»/pointer | «Checkpoint promotion» не может на рантайме сменить активный checkpoint: имя версии — строковый литерал в пути. Промоушен `v2` требует правки кода и пересборки, а не записи в реестр/индекс. Отдельно от G1: тут именно отсутствие version-indirection. | Ввести указатель активной версии (в `index.json`/манифесте/локаторе), резолвить версию на рантайме; e2e: положить `v2`, обновить указатель, доказать переключение без правки кода. |
| G3 | `models/index.json` — рукотворный каталог; продовым рантаймом не читается | PARTIAL / unwired hand-maintained | `models/index.json:52-67,:100-119` цепочки `metadata_hash_repair` (руками правленные sha); `:21,:50,:99` `locator_proof_type:"json_index_file_resolver_proof"`; читатели = `27.TranslationDomain.Tools/Program.cs:2285,:3480,:3496` + `artifacts/*/dotnet-probe/Program.cs` (одноразовые пробы); программного writer'а нет (`git grep index.json ... WriteAll/Serialize`→0, пишется только `evidence-index.jsonl`) | Единственный persistent «реестр» промоушена не связан ни с writer'ом (правится руками), ни с рантаймом (расширения его не читают, G1); `json_index_file_resolver_proof` гоняют лишь выброшенные пробы. Отдельно от P05 (in-memory локатор не персистентен): здесь наоборот — persistent, но unwired и hand-maintained. | Сделать `index.json` программно-генерируемым артефактом промоушена (sha считается кодом); подключить рантайм-резолвер, который его читает; тест паритета `index.json`↔файлы на диске↔загруженный рантаймом checkpoint. |
| G4 | Нет integrity-гейта на загрузке checkpoint'а | MISSING | `XiigenExtensionApiService.cs:1389-1465` `LoadCommonAssets`: пишет `refs["*_checkpoint_sha256"]=Manifest.CheckpointSha256` (provenance), но НЕ пересчитывает хеш файла и не сверяет; единственное sha-сравнение `PromptToTaskMachine.cs` import-путь сверяет две ОБЪЯВЛЕННЫЕ sha (manifest vs export), не байты файла | Повреждённый/подменённый checkpoint загрузится молча, если десериализуется: рантайм не пересчитывает sha файла против манифеста/`index.json` (чьи sha и так правлены руками, G3). Нарушение integrity-гейта жизненного цикла. Ново (P11 — про ref-mismatch манифеста, не про recompute-and-compare файла). | В `LoadCommonAssets` пересчитывать sha256 файла checkpoint и сверять с манифестом И `index.json`; при mismatch — fail-load; добавить load-integrity-probe в CI как гейт перед continue-training/fresh-load. |
| G5 | У трёх расширений нет промоушен-гейта в проде (в отличие от DesignConversion) | MISSING GATE | `git grep -E "PromotionDecision\|PromotionEligib\|MeetsPromotion\|PromotionGate" -- 43.SmartSkillsDomain/*.cs`→0; `ExtensionEvalOnlyQualityRunner.cs` без гейта; контраст — `44.DesignConversionDomain/DesignConversionPromotion.cs:57-121` `DesignConversionPromotionGateEvaluator.Decide` (held-out≥порог, source-disjoint, shortcut-detect, fresh-load, budget-tier, quarantine, required-artifacts) | Промоушен трёх extension-моделей в `models/` — ручная выкладка без гейт-решения: нет проверки порога/дизъюнктности/shortcut/fresh-load ПЕРЕД публикацией. Проект УМЕЕТ строить promotion-гейт (DesignConversion), но shipped-расширения его не используют. Отдельно от P25 (у ассетов нет loss/margin) — тут отсутствует сам механизм решения о промоушене. | Ввести `PromotionGateEvaluator` для трёх машин (порог held-out, source-disjoint, ablation-shortcut, fresh-load); гейтить запись в `models/` его решением; персистить gate-report на каждый промоушен. |
| G6 | Сильный гейт DesignConversion — test-only; продового uploader'а нет, model-server promotion by-design не заявляется | STUB / test-only, local-receipt-only | `git grep -E "IDesignConversionPromotionUploader\|new DesignConversionPromotionPublisher" -- *.cs` (без Tests)→0 реализаций/вызывателей; `DesignConversionPromotion.cs:276-288` `_uploader is null`→`DESIGN_CONVERSION_PROMOTION_UPLOADER_REQUIRED`+`MODEL_SERVER_UPLOAD_NOT_ATTEMPTED`; `:326-330` `ModelServerUploadAttempted=false`/`ModelServerPromotionClaimed=false`/`PromotionReceiptKind="local_receipt_only"`; `:372-380` `MODEL_SERVER_PROMOTION_NOT_CLAIMED_LOCAL_RECEIPT` | Даже единственный настоящий promotion-гейт: (а) не вызывается ни из одного продового раннера (только тесты), (б) не имеет продового `IDesignConversionPromotionUploader`, (в) по построению всегда падает в `local_receipt_only` и сам штампует «upload не пытались / promotion не заявляем». Значит model-server promotion не реализован даже там, где гейт лучший. Отдельно от P05 (no-op в 25.ModelServer): тут publisher домена честно отказывается claim'ить. | Подключить гейт к продовому промоушен-раннеру; реализовать реальный uploader (durable store/HTTP) ИЛИ честно закрепить local-only и убрать «model-server promotion» из описания lifecycle; e2e: gate PASS→upload→locate-after-upload из другого процесса. |
| G7 | Решения `Rollback`/`ContinueTraining`/`Quarantine` ПРИНИМАЮТСЯ, но не ИСПОЛНЯЮТСЯ | STUB / decision-without-executor | `DesignConversionPromotion.cs:96` `DecisionKind.Rollback` — единственная ссылка (только производится); `:270-274` при `!AllowsUpload` возвращается отчёт+манифест с `null` receipt, никакого restore/quarantine/continue действия; исполняется только ветка `Promote`(`AllowsUpload`) | Промоушен-гейт умеет вернуть `Rollback`/`ContinueTraining`/`Quarantine`, но publisher на них не реагирует — нет вызова восстановления предыдущего checkpoint, карантина строк или дообучения; «rollback» здесь = метка отчёта, не операция. Отдельно от P19 (Codex directory-swap) и Phase08 (см. G8): тут решение без исполнителя. | Реализовать исполнителей на каждый DecisionKind: `Rollback`→восстановить предыдущий promoted checkpoint+доказать load; `Quarantine`→изолировать строки; `ContinueTraining`→запустить дообучение; тест, что решение приводит к side-effect. |
| G8 | `IModelServer` — мёртвый контракт; `RemoteLogicalUnitProxy` эхоит вход; `LoadWeightsAsync` игнорирует serverBaseUrl | STUB / dead contract + echo proxy | `git grep ": IModelServer" -- *.cs`→0 (Publish/OpenWeights не реализованы); `RemoteLogicalUnitProxy.cs:34` output=`request.Input` (эхо, confidence 1.0, tag "REMOTE_UNIT") — веса не тянет, inference не делает; `LocalModelRepository.cs:10-30` `LoadWeightsAsync` игнорирует `serverBaseUrl`, всегда open локального файла | Контракт публикации/открытия весов `IModelServer` не реализован; «remote unit» через локатор→proxy вернул бы ЭХО входа, а не результат модели; загрузка весов не имеет пути «скачать с сервера». Совокупно «remote/marketplace»-нога lifecycle — фикция на уровне контрактов. Отдельно от P05 (там UploadAsync/локатор): тут `IModelServer`+proxy+load-путь. | Либо реализовать `IModelServer` (durable/HTTP) + настоящий proxy, тянущий веса и выполняющий remote-inference, + download-путь в `LoadWeightsAsync`; либо удалить мёртвые контракты и переклассифицировать lifecycle как local-only без «remote». |

## Дополнительная заметка (не отдельная дыра — cross-ref)

Единственный ИСПОЛНЯЕМЫЙ core-rollback — `Phase08CommonUserTwoLegLifecycle.RollbackCommonUpgrade`
(`:789`), но он восстанавливает ТЕКСТ common-манифеста из rollback-записи (`WriteJson` записи `:769`,
`RollbackRecordCreated=File.Exists :785`), сверяет sha записи, и НЕ выполняет post-rollback
load/inference проверки, что откатанный checkpoint функционально активен; вызыватели Phase08 —
только `Tests/*` (см. P11). Это класс P19 «directory-swap != model rollback», уточнённый для
core-two-leg: свёрнут в G7/G8 и не считается отдельной новой дырой, чтобы не задваивать claim.

## Итог

8 НОВЫХ дыр (G1–G8). Общий смысл: checkpoint/locator LIFECYCLE у трёх расширений
не механизирован — promotion ручной, locator к рантайму не подключён, integrity на загрузке нет,
promotion-гейт для расширений отсутствует (а имеющийся в DesignConversion — test-only и by-design
не заявляет model-server promotion), rollback как операция отсутствует, `IModelServer`/proxy —
мёртвые/эхо. Ни одна из строк не объявляет фазу закрытой; каждая привязана к file:line живого кода.
