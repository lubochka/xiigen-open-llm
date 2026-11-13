# 155. Адресный план исправлений после Q003

Основание: [аудит154](codex-154-postq003-artifact-and-transfer-audit-2026-11-03.md), source core7ce6b14c, просмотренная параллельная дельта доc1f3c448 (включая review579d и передачу завершённого Q004). План не создаёт новую очередь и не разрешает повтор старого обучения. Исполняется текущий прямо назначенный ID; продуктовый перенос/Q01 остаётся135-R01-CONCURRENCY. Сохранность конкретного донора — зависимость его существующего потребителя, а не основание глобально переставить131 опыт.

## 1. Что уже исправлено этим документным пакетом

1. Действующая строка Q002 отражает завершённый DPO recovery и обе стадии расхода; initial partial сохранён как история, transfer NOT_GRANTED.
2. P131 InputRoot исправлен на `continuation-0001/evidence` в150/49/карточке, с раздельными roots и ошибочным-parent negative.
3. Для P127 назначена внешняя authority:15 result files на7ce с Git blob/SHA256/bytes; historical и synthetic приёмка разведены. Это описание и manifest, **не реализованный native verifier**.
4.20 экспериментальных карточек, связанные transfer contracts и planning cards получают конкретные условия из156. Параллельные153/P03 review сохранены.

## 2. R154-A — завершить адресную сохранность binary artifacts без Train

**Существующие владельцы:** собственная карточка потребляемого состояния; общий producer/reader договор101 и fixtures02. P11/P12 относятся к29-D3/29-W1. Не переоткрывать уже исправленный алгоритм embedding/warmup из-за повреждения S1.

**Вход:** [inventory3502](../../../../experiments-v2/_audits/postq003-2026-11-03/codex-binary-path-inventory.json); old commit68f0ce4, changed187b54b, current7ce, актуальный branch HEAD перед записью; exact path и назначенный donor manifest. Original SHA выбирается по lineage конкретного path/attempt, не по близкому имени или большинству одинаковых файлов.

**Порядок внутри назначенного ремонта:**

1. Проверить, относится ли current input к19 conflict paths, двум shared S1 либо другой строке3401.49 восстановленных перепроверять только при их фактическом использовании или обнаруженном расхождении; не объявлять все3401 byte-corrupt до чтения.
2. Считать original LFS pointer и получить **его** bytes из доступного исходного LFS object/подлинного архива. Проверить SHA256/size до любых преобразований. При отсутствии оригинала — `ORIGINAL_BYTES_UNAVAILABLE`; не угадывать CR, не обнулять веса, не обучать замену.
3. Считать текущий materialized файл отдельно от Git pointer. Проверить отсутствие pointer/merge-conflict text вместо требуемого binary format, magic/schema/размеры/полноту до EOF. Не устанавливать глобальное «ровно320620»: P69 original119832, LoRA rank меняет размер. Сверять конкретный формат и manifest.
4. Восстановить bytes без текстового декодирования, нормализации строк или shell redirection через строку. Для19 путей сравнить с original, а не автоматически выбрать одну сторону merge. Проверить точный набор изменений и сохранить historical evidence прежней неисправности.
5. Проверить согласованность всего применимого bundle: weights, optimizer keyed moments/counters, RNG, tokenizer/config/coordinate schema. Не требовать нейронные поля от дискретной памяти; не называть неизвестные существующие части N/A. У shared P11/P12 исправить именно weights; не присваивать optimizer ошибку без доказательства.
6. Сверить recovered bytes→staged LFS OID/size→remote pointer→fresh checkout с реально загруженными LFS bytes. Свежий native Load и обычное чтение выполняются только в уже назначенной технической приёмке соответствующего owner, с отдельными counts и без Train. Не маскировать их под offline audit0.

**Приёмка:** manifest exact; все восстановленные paths равны назначенным originals; нельзя оставить pointer-only checkout и назвать его loaded. Честный полный bundle принимается. Правильный внешний LFS pointer, адресующий289-byte conflict text, отвергается до изменения live owner. Также отвергаются truncation, чужой payload того же размера, несовместимый tokenizer/coordinate schema, неполный moment/RNG state. Отказ сохраняет старый live state/version и старые ответы. Не требуется портить реальные originals: negative controls только на копиях.

**Терминал:** `PAYLOAD_IDENTITY_RESTORED`, `NATIVE_LOAD_VERIFIED` и `ORDINARY_RETENTION_VERIFIED` записываются раздельно с actual command/exit/source. Если сделан только первый уровень, остальные UNRUN. Любая ошибка получения или загрузки не разрешает дополнительный update.

## 3. R154-B — P03: граница конечного состояния и метрики

**Владелец:** существующий эксперимент/Q004, P03; текущая параллельная проверка579d уже опубликована. Не назначать повтор PRIMARY. Source-bound historical loss относится к21 позициям pre-update; answer NLL — к ответной позиции after. Сохранить T1 gain и E2 loss, не объявлять2 TRAIN rows двумя уроками.

**Текущая работа без модели:** проверить наличие подлинного полного final bundle вне опубликованного13-file P0301, если такой архив назначен/доступен. Weights сравнивать по реально записанным digests в той же именованной схеме исходного producer; hash serialized file не подставлять вместо hash массивов. Для keyed moments/RNG нужны отдельные подлинные архивные bytes и их provenance: совпавшие step/counts из after.json не доказывают тождество значений. Отсутствующее старое значение или доказательство оставить NOT_RECORDED/NOT_PROVEN. Если подлинного полного архива нет, сохранить `FINAL_STATE_NOT_PUBLISHED_IN_P0301` / пригодность для reuse NOT_PROVEN. Generated token IDs, full logits и gradients не реконструировать из U+FFFD или scalar loss.

**Профилактика для будущего уже назначенного producer:** до terminal success перечислить каждый фактически сериализованный applicable component и его path/hash/schema; hash/count-only receipt не удовлетворяет full-state requirement. Fresh load доказывается отдельным процессом/объектом и ordinary reader по собственному owner, а не повтором Train. Negative: заменить требуемый payload одним receipt с тем же named digest/count; completeness gate должен отказать. Positive: полный подлинный bundle восстанавливается, даже если научная точность ухудшилась. Целостность не требует победы learner.

## 4. R154-C — исправить реальный вход P131

**Владелец:** R131-04 /150§5 /49§5 / P131. Сохранить уже назначенные producer archive migration, native-output binding, exact128 keys,14 старых FQN плюс новые случаи. Не заменять их новым скриптом этого аудита.

От корня repository:

| Поле | Значение / обязанность |
|---|---|
| attempt_root | `experiments-v2/RP01-MICRO/P13101/results/attempt-0001/continuation-0001` |
| input_root | `experiments-v2/RP01-MICRO/P13101/results/attempt-0001/continuation-0001/evidence` |
| producer_archive_root | Отдельный каталог с подлинными historical source/DLL/PDB и проверенным mapping619; фактическое расположение назначает исполнитель по наличному архиву, это не input_root и не новая checker DLL |
| verification_output_root | Новый отсутствующий каталог вне input_root и producer archive; отдельный для каждого honest/mutant/honest запуска |

В таблице заданы repository-relative адреса. **Перед фактическим вызовом оба аргумента InputRoot/VerificationOutputRoot преобразовать в абсолютные пути.** `Invoke-Bounded` задаёт дочернему процессу WorkingDirectory=hostRoot; текущая VerifyOnly-ветка вычисляет inputFull/outputFull для проверки разделения, но в child передаёт исходные аргументы. Относительный путь от корня repo поэтому нельзя передавать напрямую. В49 добавлена подготовка абсолютных значений через Resolve-Path/GetFullPath.

Нативный runner должен получить evidence, а не родительский attempt. Положительный вход проходит layout/source checks до нужного binding; отрицательный родительский каталог должен дать явный missing-layout/nonzero exit без Train/Flow/Load и без создания файлов внутри input. Не создавать в неправильном корне пустые data/raw, чтобы формально пройти проверку.

Перед запуском нового checker сохранить originals; затем собрать Checks/Probe и host по49, discovery/TRX той же сборки. Далее full honest archive→адресная mutant copy→honest с нативным0→nonzero→0. Если producer archive неполон, provenance NOT_PROVEN и honest positive ещё не выполнен. Никакой новый PRIMARY для подготовки не нужен.

## 5. R154-D — P127: внешний historical authority и достижимые контроли

**Владелец:**150§6/49§6, KNN-ORDER-PERMUTATION-01; полезные числа и граница ordinary KNN integration остаются по153/105§5. Эти два ремонта не смешивать.

1. Закрепить `manifest_commit=f3b64f87159193c2897092d6e571b2b65e3b75c4`, SHA256 manifest `df98d7354296396164b51ce583370f4d10dbc46f2e3527e1b67a5f265b1fca84`; `results_commit=7ce6b14ca7bb353ed5f57179f0561053496d36ae` и получить из manifest_commit [p127-trusted-historical-output-manifest.json](../../../../experiments-v2/_audits/postq003-2026-11-03/codex-p127-trusted-historical-output-manifest.json). Он перечисляет15 files на source7ce:15 Git blobSHA1,15 SHA256, exactbytes. Во входном пакете authority не выбирать и не пересоздавать. Корректировка manifest допускается только отдельным явным source-bound решением, не автоматически из supplied input.
2. В historical mode до интерпретации проверить exact file set **выбранной попытки** относительно соответствующего среза manifest; чужая попытка не заполняет missing files. Версия verifier, trusted source authority и historical producer проверяются раздельно. Наличие двух source metrics digests в153 полезно, но не заменяет эту реализацию.
3. После byte identity проверить68 unique read keys и12 graph keys на каждую выбранную попытку, включая A-perm/B-perm; отдельные cold observations/receipts по фактическому набору. Затем DTO route/orderedsteps, trace binding и scientific gold scoring. Historical bytes могут содержать настоящий неверный ответ: это не integrity error.
4. Negative: в копии изменить route+steps, A/B trace, сводные поля и её локальный hash inventory согласованно; оставить прежний historical attempt. Приёмка должна отказать по внешней authority, даже если вход не менялся во время проверки. Отдельно проверить missing, duplicate,extra,подмену permutation row и попытку выдать synthetic identity за historical.
5. Positive historical: настоящие15 files, выбранные поattempt, проходят identity/layout; научные scores вычисляются без требования успеха B/C. Positive validator: полный synthetic fixture C=B/неверный B имеет собственную identity, явно отдельный test mode и все применимые records; через тот же finalizer даёт procedureVALID, scientificfalse, не historical acceptance. Нельзя сделать этот positive переклейкой historical proof.
6. Реализовать и испытать настоящий finalizer и outer process по150/49. Python hash-control этого аудита только демонстрирует внешний identity predicate; не проверяет C# finalizer/CLI. Все эти controls — saved-only, без Train/Predict/graph-node execution. Отдельного native-flow для P127 historical rows нет: сохранить NOT_RECORDED, не изготовлять его из DTO.

## 6. R154-E — держать продуктовые переносы на существующих адресах

| Работа | Реальные файлы/владелец | Что должно различать исправление |
|---|---|---|
| CAS conflict identity и late cancellation | `27.TranslationDomain/TranslationMachine.cs`;135-R01 | В конфликте observed version относится к тому же CAS; отмена на publish barrier не публикует accuracy и не меняет live version. Позднее чтение CurrentVersion не годится. Сохранить исправленную Train-ветку |
| Durable candidate и обычная composition | Q01/R17; TranslationLostCandidateArtifact, TatoebaPilotLoopRunner и назначенный composition root | Реально подключённый store пишет именно проигравшего staged candidate; новый процесс восстанавливает его без Train. Unit test отдельного store не заменяет обычного caller; отсутствие store имеет явный результат |
| Project owner → mutable recipient | R128-02; DpoTrainer, DpoMutationAdmissionCapability, DpoMutationOwnerLedger, реальный issuer из14.ContinuousLearning; точная матрица146 | Два разных recipient из одинаковых bytes различаются. Новая capability не забывает прежнего владельца занятого recipient; public admitted constructor с null не обходится без project binding. Два законных шага своего owner и разрешённый shared/legacy путь остаются положительными контролями |
| Lineage и законные повторяющиеся IDs | R128-03; CapabilityMaterialLineageGuard и CapabilityMaterialLineageTests,150§4/146 | Два lawful Train ID одного content принимаются независимо от порядка SourceRows; чужой ID/content и несколько семей отвергаются. Запрещённое Train/Test пересечение достигает FORBIDDEN_CONTENT_OVERLAP, а не случайного раннего LINEAGE_CONTRADICTS_CONTENT. Dev/Test flags проверяются отдельно |
| Null/empty Output до framing | R117-04,153§1 | Существующий producer `DescribeOutput` различает законные null/empty; ordinary trained unit и reuse подтверждают фактические identities, не только nullable helper fixture |
| Shared S1 repeatability |29-D3/29-W1,02/101/103 | Тот же сохранённый donor читается без Train; file repair не выдаётся за новый успех алгоритма |

По назначенному техническому ремонту исполнитель выполняет сборку, discovery, позитивные/адресно отрицательные проверки, фиксирует фактические FQN/TRX/source/DLL и исправляет неверные fixture/expected по независимому договору. Не оставлять failed-test под видом запрета из исторического документа. Новое научное обучение не возникает из такого поручения автоматически. Полный дорогой набор повторяется по действующему gate или конкретному остающемуся риску, не ради оформления отчёта.

## 7. Передача результата человеку

В конце каждого ремонта: что реально исправлено; exactpaths/commit; что измерено до/после на каком attempt/arm/phase; что проверено native и что только offline; цена всех уже выполненных стадий; оставшийся owner и текущий STOP/NEXT. «Повреждён19-й файл» или «тесты зелёные» без этой связи не закрывают задачу. Сначала доказательство, затем статус; принятое ухудшение метрики не прятать за словом VALID.

Этот пакет уже исправляет документы; R154-A/B/C/D/E — конкретные обязанности существующих исполнителей, где реализация ещё отсутствует. Нет нового blanket запрета на все результаты и нет назначения повторять131 опыт. Подробный состав изменения и review —156.
