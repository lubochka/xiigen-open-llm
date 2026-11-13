# 78. Найдено сохранённое продолжение обучения памяти, но его «generalization» имеет узкий смысл

16 октября2026. READ_ONLY_RESEARCH. Поиск продолжает75: historical Tatoeba caller и его raw receipt. Именованный private Tatoeba report из STATE39 на этой машине отсутствует. Однако найден другой самостоятельный положительный путь — `continue-translation-training` с сохранёнными исходным и продолженным checkpoint. Ни команды, ни модели повторно не запускались.

**Что подтверждено:** архив показывает добавление новой корпусной пары после загрузки прежней памяти, сохранение старых записей и ответ через kNN на неидентичную строку. **Чего он не показывает:** прироста качества от новой пары, русского→английского перевода или neural DPO/LoRA. Это полезный успешный механизм продолжения памяти, который следует сохранить как отдельный архитектурный донор.

## Первичная привязка

- [continue-report.json](C:/Projects/llm_mvp_core/artifacts/phase38/translation-v1-continued-probe/continue-report.json) прочитан полностью.
- [Состояние после продолжения](C:/Projects/llm_mvp_core/artifacts/phase38/translation-v1-continued-probe/translation-v1-continued-checkpoint.json) разобрано как JSON; inspected KNN entries, history, identities, executor maps и общие поля.
- Источник до продолжения: `models/translation/translation-v1/translation-v1-checkpoint.json` **в commit** `5fa0f242b9f24c2045f4ca07742bfe6baaf48bb5`, не нынешний одноимённый файл.
- В том же commit `training-data/translation/translation-v1/corpus.jsonl.gz` содержит две строки. Его SHA256 `53559e02f6f0a5eb54d85e6e424afb97c8bd1d364f5a7c8aa10c891430abd9ac` заново вычислен по Git blob и совпал с report. Gzip только прочитан/распакован в памяти; данные не импортировались и не переписывались.
- Нынешний training-manifest имеет12 пар и другой hash `dac67097…`; подставлять его в историческое доказательство было бы ошибкой. Retained continuation artifact между5fa0 и нынешним HEAD имеет пустой diff.

## Что изменилось в сохранённом состоянии

| Поле | До | После |
|---|---|---|
| Версия |0.1.1 |0.1.2 |
| Structured identities |safe-001 |safe-001 и safe-002 |
| KNN entries |3 представления одного урока |6 представлений двух уроков |
| Exact-source memory |`add null check ValidateToken` |Сохранена, добавлен `launch worker thread` |
| История |regenerate-001, rows1 |Дополнена continue-001, rows1 |
| accuracy_after |0.3333333333 |0.3333333333 |
| Tatoeba loop state |Не используется этим доказательством |null |

Три прежние KNN записи присутствуют в after в точности; три новые принадлежат второму уроку и разным signatures. Это не6 независимых уроков. Новая пара: `launch worker thread → Launch worker thread.` Старый урок: `add null check ValidateToken → Add a null check to ValidateToken.` Rejected каждой строки задан отдельно в корпусе; дальнейший CLI создаёт executor example ([BuildTranslationContinueExample4677–4694](C:/Projects/llm_mvp_core/27.TranslationDomain.Tools/Program.cs:4677)).

[ContinueTranslationTraining4073–4125](C:/Projects/llm_mvp_core/27.TranslationDomain.Tools/Program.cs:4073) сверяет hash корпуса, загружает checkpoint, собирает прежние ExampleId и исключает их из выбранных строк. Затем вызывает TrainStructuredBatchAsync, сохраняет checkpoint и собирает route evidence ([4163–4191](C:/Projects/llm_mvp_core/27.TranslationDomain.Tools/Program.cs:4163)). Это реальная цепочка изменения памяти по source. Сопоставленные saved states и report поддерживают конкретное историческое продолжение; исполнявшийся бинарник отдельным hash в прочитанном report не аттестован.

## Что означает удачный ответ через kNN

Report сохраняет две строки:

| Запрос | Adapter | Ответ |
|---|---|---|
| `add null check ValidateToken` |executor-ru_en-exact-source |Add a null check to ValidateToken. |
| `add a null check before ValidateToken` |executor-ru_en-knn |Add a null check to ValidateToken. |

Вторая строка отличается от exact-source и проходит через kNN. Это содержательное свидетельство использования выученной памяти на варианте формулировки, а не только успешного сохранения файла.

Но оба probe относятся **к старому safe-001**, который уже был в исходном checkpoint. Они не проверяют новый safe-002 и не содержат парного before/after результата. Значит, успешный probe нельзя выдавать за прирост качества вследствие только что добавленного урока.

Кроме того, корпус и probe передают английские source/target при `source_language=ru`, `target_language=en`, direction RU_EN. В [RunTranslationRouteProbe4753–4772](C:/Projects/llm_mvp_core/27.TranslationDomain.Tools/Program.cs:4753) эти метки выставлены явно. Это проверка технического маршрута и памяти; она не доказывает понимание русского источника или корректный автоматический выбор языка.

Поле `non_exact_probe_proves_generalization` вычисляется как Success && !BootstrapOutput && !ExactSourceHit. Сравнения с gold и независимости семьи в этом предикате нет ([4721–4725](C:/Projects/llm_mvp_core/27.TranslationDomain.Tools/Program.cs:4721)). Тот же предикат подтверждён в Program@5fa0,968–972. Поле `exact_memory_leakage_proven` здесь означает «разделение маршрутов признано контролируемым», а не буквально доказанную утечку; имя не заменяет прочтение формулы. Положительный факт route не отбрасываем, широкий термин generalization сужаем.

## Связь с Tatoeba и другими доменами

В [STATE39](C:/Projects/llm_mvp_core/docs/plans/STATE-PHASE-39-EN-RU-SCRIPT-AWARE-COMPOSITION.json:46) есть точный адрес private report и признание contaminated-панели с exact1.0. Проверенный адрес `C:/Users/lubao/Documents/xiigen/private-corpora/tatoeba-2026-05-23/pilot-private-2500w80-phase39` не существует. Поэтому не подтверждаем его raw строки и не используем вторичную запись как новый успех. В найденном continuation-checkpoint `tatoeba_loop=null`, а report называет другой command: это не missing Tatoeba receipt из75.

Новый положительный материал относится к памяти executor и восстановлению состояния. Он поддерживает отдельный путь «урок → exact/signature memory → обычный adapter response». Он не устанавливает исторический перенос неудачной матрицы в успешный kNN и не объясняет RP01 q/v-LoRA. Сохранённые записи полезно сопоставлять с этими нейронными механизмами без подмены одного вида обучения другим.

## Один будущий ограниченный контроль — не выполнен

Взять неизменённый исходный checkpoint5fa0 и подготовить заранее два запроса к **новому** уроку safe-002: точный и содержательно эквивалентный неидентичный, плюс контраст действия/объекта, который нельзя отвечать прежним текстом. Сначала определить правильные ответы и реальные язык/направление. После отдельной авторизации сравнить состояния до/после добавления только safe-002, маршруты, правильность на новых контрастах и сохранение safe-001. Число signatures не считать числом уроков. Флаг !ExactSourceHit не заменяет смысловую оценку. Это контроль памяти/переноса, не эксперимент LoRA.

Охват: точные JSON и исторический gzip/два corpus rows, scoped Git history/diff, адресные CLI методы и исторический предикат; остальные private corpora и весь translation archive не обследованы. Изменён только этот Markdown. Документ является продолжением поиска, не завершением исследования.

