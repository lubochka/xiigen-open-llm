# 50. Сохранённые bytes, исторические свидетельства и приёмка переноса

3 ноября 2026. Дополнение к39/49/101 по [аудиту154](../fixes-based-on-experiments/codex-154-postq003-artifact-and-transfer-audit-2026-11-03.md) и [плану155](../fixes-based-on-experiments/codex-155-postq003-addressed-repair-plan-2026-11-03.md). Это конкретизация найденных ошибок; она не меняет данные, фактор, уроки, gold, native API, очередь или научную дозу карточки.

**Уточнение4 ноября:** [157](../fixes-based-on-experiments/codex-157-postq003-executor-context-repair-2026-11-04.md) закрепляет текущие hashes двух S1 и отдельный readback без обучающих fixtures. Счёт49/3401 ниже — исторический7ce; на67af Git-сверка дала57 original/3393 прежних изменённых после ремонта8 Q005 файлов. Это не57 native Load. Старые результаты не повторять, статусы и NEXT обновлять в основной строке индекса.

## 1. Идентичность файла имеет несколько разных носителей

При использовании сохранённого состояния записать `repository/commit/path`, Git blob SHA1, LFS OID/size если применяется, фактические materialized SHA256/bytes, тип/schema и исходный donor manifest. Совпадение Git pointer не означает скачивание backing object; совпадение размера не означает checkpoint. Два SHA считаются равными только в одной схеме сериализации: hash named floating arrays и hash serialized file — разные величины.

На19 paths P68/P69/P75/P76/P78/P81 опубликован корректный внешний LFS pointer, адресующий289-byte conflict text. Поэтому до Load/acceptance проверять **содержимое материализованного файла**, включая ожидаемый формат и EOF. `git diff --check`, отсутствие markers в pointer и успешный upload не достаточны. Positive — exacttrusted payload; negative — correct pointer к conflict text/чужому same-size payload; ошибочный live import не меняет старое состояние. Проверка native loader обязательна там, где она уже назначена собственным owner; offline-проверку не называть ею.

## 2. Восстанавливать подлинное состояние, не похожее

Использовать exactoriginal bytes по lineage конкретного attempt. Режим чтения/записи binary сохраняет все bytes; не выполнять CR/LF нормализацию, замену по regex или text roundtrip. Если original недоступен, статус `ORIGINAL_BYTES_UNAVAILABLE`; новый Train не является восстановлением. Staged pointer, remote pointer и fresh materialized readback получают отдельные receipts.49 repaired pointers Q002/Q003 — подтверждённая часть, не автоматическое доказательство остальных3401.

Shared P11/P12 S1.weights сейчас расходятся с собственными manifest. Перед их назначенным reuse восстановить donor; не обновлять manifest под повреждённые bytes. Исправность algorithm и пригодность текущего сохранённого файла — отдельные статусы. Этот вывод не распространяется автоматически на optimizer этих bundles или общий h64-rms-row S0.

## 3. Полный final state — это данные, а не описание данных

Для применимого нейронного state нужны фактические weights, keyed moments/counters, RNG, tokenizer/config/schema и lineage. Для KNN/discrete memory — её реальный typed payload и обязательные metadata; нейронные поля не навязываются. Hash/count/step-only after.json не заменяет сериализацию значений. P03 historical final bundle не опубликован в собственном поддереве; обозначить этот предел, не строить новый из повторного обучения.

При будущей назначенной записи final state перечислить exactfiles и сохранить пригодный набор до terminal receipt. Positive load того же bundle в новый owner возвращает назначенные ordinary ответы. Negative удаление одного требуемого компонента или замена его receipt-only файлом обнаруживается. Если loader отсутствует, `NO_NATIVE_RESTORE`; сохранившийся hash не даёт fresh-loadPASS.

## 4. Historical authority хранится вне проверяемой копии

У P127 использовать [15-file manifest](../../../../experiments-v2/_audits/postq003-2026-11-03/codex-p127-trusted-historical-output-manifest.json) из `manifest_commit=f3b64f87159193c2897092d6e571b2b65e3b75c4`, SHA256 `df98d7354296396164b51ce583370f4d10dbc46f2e3527e1b67a5f265b1fca84`. Отдельный `results_commit=7ce6b14ca7bb353ed5f57179f0561053496d36ae` обозначает исторические результаты. InputRoot не выбирает свою authority. До/после одинаковые hashes показывают отсутствие изменения во время проверки; они не доказывают неизменность с момента настоящего запуска. Source/DLL identity не автоматически связывает metrics output.

Historical mode сверяет выбранную попытку с externally trusted exactoutputs; synthetic mode имеет собственную identity, не засчитывается как historicalrun. Контроль согласованно изменённых route+steps+summary+local hashes должен отказать по authority. Полный synthetic C=B/неверный B нужен для проверки, что technical integrity не требует scientific success. Его native finalizer/outer exit должны реально выполняться при ремонте; Python-проекция этого не заменяет.15 исходных files подлинны по закреплённому Git-срезу; ошибочные выводы исторического report исправлены отдельно153, а не считаются истинными из-за hash.

## 5. Каждый root должен соответствовать реальному reader

P131 `attempt_root=…/results/attempt-0001/continuation-0001`, **`input_root=attempt_root/evidence`**. В фактическую команду передать абсолютные InputRoot и VerificationOutputRoot: child работает в hostRoot и не разрешает repository-relative аргументы от исходного cwd. `producer_archive_root` — отдельный подлинный source/DLL архив с mapping619; `verification_output_root` — новый внешний каталог. Неправильный parent root отвергается по layout до каких-либо model calls. Сохранить current49 requirements:14 прежних tests + новые controls, отдельные Checks/Probe/host builds,128 native keys, producer/verifier schemas, explicit outer exit. Никакого PRIMARY для saved verification.

## 6. Число принадлежит конкретному состоянию и попытке

Для метрики писать `attempt/arm/phase/state/row-id`, единицы, формулу и знаменатель. P03 T1 NLL уменьшилась, средняя panelNLL и accuracy ухудшились; обе стороны остаются. Pre-update loss по21 positions не называется after-answer loss. P127 p2 — TRAIN, h2 — EXAM; ресурсы0001 не подставляются в0002. Valid negative result остаётся научным результатом, не отказом integrity. Отсутствующие token IDs/gradients/native receipts — NOT_RECORDED, не восстановленные из текста значения.

Q002 old partial + recovery учитываются вместе: известны800+800SFT и8DPO, другие historical costs отдельно unknown. Q003 известны24updates. Binary repair0updates не переписывает historicaldose. Последующее исправление документа не становится новым обучением.

## 7. Копируемое задание и русский терминальный отчёт

Передавать собственный адресный блок POSTQ003 карточки вместе с50 и существующим120/49, а не только ссылку на общий аудит. Назвать назначенный owner, exactinputroot/donor/attempt, sourceauthority, positive и negative, ожидаемый failure reason, nativecommand/result или UNRUN, полный выполненный расход и следующий разрешённый ID. Не сокращать criticalcase до «проверь hashes» или «все тесты должны пройти».

Авторский срез3 ноября — документы и offline-пересчёт. Эта историческая фаза не запрещает C#/PowerShell builds/tests и fresh readback уже назначенному исполнителю ремонта. Перед фильтром проверить constructor/setup/fixtures; отдельный reader допускается в пределах назначенного технического ремонта по46/157. Отдельный запрос нового обучения не создаётся из отсутствующего файла; если восстановить оригинал нельзя, сообщить конкретную нехватку, а не повторять оплаченный опыт.
