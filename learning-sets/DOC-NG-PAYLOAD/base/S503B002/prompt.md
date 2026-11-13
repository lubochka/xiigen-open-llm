<!-- LEGACY-PARALLEL-BOUNDARY-20261026 -->
## Область этого шаблона: явно выбранный legacy request

`xiigen.batch-request.v1` не входит в claims нового параллельного координатора. Используй этот шаблон только для отдельно выбранной последовательной старой попытки; не запускай параллельный дубль и не создавай здесь новый M001 вместо текущего D1000. Для новых 24 доменов используй [три согласованных промпта](MASTER-PROMPT-RU.md). Общая матрица D1000: `C:/Projects/llm_mvp_core/learning-sets/campaigns/D1000-20261025/status.json`, авторитет: `state.json`; их владелец — новый coordinator, старый runner не обновляет там claim/прогресс автоматически. Старые строки не засчитываются в D1000 без отдельного назначения.

В новом D1000 актуальные пути/команды находятся в возвращённых `coordination-*` файлах, с agent ID и token. Этот исторический шаблон не подменяет их. Подробности: [граница параллельности](../28-PARALLEL-GENERATION-AND-QUALITY.md).
<!-- /LEGACY-PARALLEL-BOUNDARY-20261026 -->

# Создание одной партии DPO в Kiro — CREATE_ONLY

<!-- CURRENT-LANGUAGE-AND-CAMPAIGN -->
Reason internally in English. Do not output private reasoning. Russian status; learner language follows the assigned profile. This prompt remains for an explicitly selected legacy request. New domain quotas and continuation use [campaign/registry.json](../campaign/registry.json); do not start a new legacy M001 instead of a pending campaign request.
<!-- /CURRENT-LANGUAGE-AND-CAMPAIGN -->

Рабочий порядок CREATE_ONLY берётся из текущего request/commands и этого входного промпта. Предметная карточка задаёт содержание и классы; сохранённые внутри неё ссылки на полные skills, старый шаблон 08 или будущий учебный пилот не требуют отдельного поиска, review или обучения при создании этой партии. Например, старое «Прочитай 06-SOURCES-AND-SKILLS» не отменяет уже выданное компактное назначение. Сами предметные правила, count и семейства сохраняются.

Перед составлением назначенной микротемы прочитай только соответствующий раздел `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/prompts/AUTHORING-SELF-CHECKS-2026-10-26.md`, если он есть. Там короткие хорошие/плохие примеры, а не новый этап review. Готовые строки и request не переписывай ради обновления памятки.


Роль человека при выборе учебного материала выполняет **CODEX с Astra 6 ultracode**. Оценку качества DPO также выполняет **CODEX с Astra 6 ultracode**. Внешняя открытая китайская модель через Kiro создаёт `chosen/rejected`. Сейчас выполняется CREATE_ONLY по уже назначенным карточкам: новые вызовы выбора/оценки не запускаются и не задерживают создание следующей партии. Назначение роли не является доказательством выполненного модельного вызова.

## Что ты создаёшь для обучения XIIGEN

Ты — автор учебных данных. Получатель данных — будущая обучаемая модель или Unit XIIGEN по назначенной предметной теме. Твой результат — реальные DPO-тройки `prompt/chosen/rejected` в `draft.jsonl`, пригодные по содержанию для последующего обучения предпочтению. Кандидатный статус обозначает стадию приёмки; он не разрешает заменять учебные примеры отчётом, схемой, списком тем или заглушками.

- `prompt` — полная задача, которую ученик должен решать при использовании модели: перевести текст, объяснить код, написать код либо выбрать действие по состоянию проекта.
- `chosen` — полный желаемый ответ ученика на этот prompt.
- `rejected` — полный правдоподобный ответ на тот же prompt с одной назначенной предметной ошибкой.
- Пара задаёт, какому ответу на тот же вход следует отдавать предпочтение. Фактическое изменение весов/LoRA или памяти выполняет соответствующий обучающий механизм позднее; генерация файла сама его не запускает.

Не помещай «создай DPO-набор», команды подготовки, пути Kiro, отчёт исполнителя или объяснение предпочтения в learner prompt/answers, если сама назначенная предметная задача явно этого не требует. При обычном использовании ученик получает prompt и выдаёт ответ; метки chosen/rejected не подсказываются ему как ответ. Обе альтернативы доступны обучающему механизму для задания предпочтения.

Сначала прочитай конкретные учебную цель, response contract и предметные reference-примеры назначенной карточки. До написания каждой строки должно быть ясно: «После будущего обучения на подобных примерах ученик должен уметь ___; в этой строке chosen выполняет ___, а rejected нарушает ___». Это рабочая постановка при авторинге, не отдельный отчёт или новый review-этап.

### Простая самопроверка на примерах

Для задачи удаления: `письмо` —6 букв, `писмо` —5: удалён `ь`, остальные буквы прежние. `пильмо` —6: это замена `с` на `л`, не удаление. Сначала проверь «длина стала меньше на1», затем «оставшиеся буквы в прежнем порядке и регистре». `Окно→кно` подходит для удаления первой буквы; `Окно→Кно` дополнительно меняет регистр. У `market` индексы `m0 a1 r2 k3 e4 t5`: `maket` получается удалением индекса2, не3. Даже правильная длина не доказывает правильный индекс.

Для перестановки бери слово прямо из предложения: в `Возьми ручку` это `ручку`. Обмен `ч/к` даёт `рукчу`,5→5 букв. `рукча` не подходит: изменилось ещё окончание. Обратная операция должна точно восстановить слово из предложения. Эти действия — часть составления строки, не отдельный audit/отчёт/запуск инструментов.

Для `TR07-VALIDWORD` исходное слово уже правильное: `She mentioned a sword.` → `Она упомянула меч.`; rejected `Она упомянула слово.`. Не добавляй строки «восстанови повреждение»: «Контрпример / граница» карточки не назначает квоту 5+5. Сравнение с конкурентом `sword→word` — удаление первой буквы (5→4), а learner source остаётся `sword`. Проверь и другие факты: `שולחנו` → `его стол`, не `наш стол`; `school entrance` → `вход школы`, без добавления «главный». Подробные разборы и источник форм иврита — в шпаргалке ниже.

При задаче с одной опечаткой обратная замена должна восстановить **всё предложение**, а не только токен: `She saw a cat.` → `She saw a cap.` сохраняет остальной текст; `She saw the cap near the door.` дополнительно меняет артикль и место.

Проверка пары: для `Она отправила писмо бабушке` chosen `She sent a letter to her grandmother.` и rejected `She sent a parcel to her grandmother.` различаются предметом. Rejected `She mailed a letter to her grandmother.` не создаёт такого контраста: это другой допустимый перевод. Rationale не может сделать правильный перевод ошибочным.

Полные хорошие/плохие разборы: `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation/prompts/TYPO-AUTHORING-EXAMPLES-RU.md`. Это публичная подсказка автору, не новые строки корпуса. Совпадающий F-номер у двух профилей не означает один сюжет. Автор плана подготовки вкладывает общий исходный сценарий или исходник для общей семьи; используй его, когда он дан. У старого request без такого сюжета сохраняй назначенные family/origin/leakage/split и кандидатный статус: родство не подтверждено, его нельзя объявить установленным по номеру. Не ищи чужие сессии, не выдумывай подтверждение origin и не переименовывай семьи. Это открытая часть задания для автора подготовки, а не новый блокер генерации: продолжай CREATE_ONLY с обычным finish и следующим ID, без отдельного review-процесса.

**CREATE_ONLY означает: сейчас создать данные для обучения, само обучение сейчас не запускать.** Он не отменяет предметной правильности, полного текста ответов, различающего смысла и требуемого объёма. `count` задаёт размер текущей партии, а не всей кампании; десять строк не означают, что большие наборы всех доменов готовы.

### Как выглядит учебное содержание

Публичный reference из TR07-OMISSION/ru-en (иллюстрация трёх текстовых полей; не копировать в новый TRAIN/DEV/EXAM):

```json
{
  "prompt": "Переведи с языка «русский» на язык «английский». Выдай только перевод. Контекст: документ для проезда в автобусе. В одном обычном слове случайно пропущена ровно одна буква; восстанови его по контексту.\nИсходный текст: Блет лежит на столе.",
  "chosen": "The ticket is on the table.",
  "rejected": "The receipt is on the table."
}
```

Учебное содержание здесь — восстановление значения по контексту и его перевод. Ответы сохраняют одну и ту же сцену; меняется перевод восстанавливаемого предмета. Это не урок о том, как оформлять JSON. Для текущей другой темы используй её собственный response contract и примеры: документация объясняет фактическое поведение, код реализует видимое требование, оркестрация выдаёт требуемое действие/план/маршрут.

`canonical_token`, `raw_token`, `lexical_family_ids`, origin/split и rationale служат разметкой и связностью. Чтобы учить переводу повреждённого слова, реальная повреждённая форма должна присутствовать в learner prompt, а правильный смысл — в выбранном ответе. Одна запись связи в metadata не обучает токенизатор, kNN или Unit и не доказывает, что native consumer использует это поле. Не раскрывай скрытый canonical-ответ ученику ради упрощения задачи.

Ты создаёшь учебные кандидаты одной маленькой темы в режиме **CREATE_ONLY**: запись draft → структурный finish → следующий stream. Предметное review, blind, accept, export, native import, Student и production-тесты не входят в эту сессию и не блокируют следующую партию. Используй текущую выбранную пользователем китайскую модель Kiro; помощник наследует её; допускается не более одного помощника одновременно. Не переопределяй модель и не называй её поставщика по предположению. Запиши наблюдаемую модель/selector, среду и идентификатор сессии. Если identity или session ID не раскрыты, запиши `unknown`; кандидаты можно сохранить, но нельзя утверждать, что установлен конкретный провайдер.

Этот пакет относится только к подготовке материала. Обязательные checksum, конкретный профиль CODEX, квитанции провайдера, проверка всего production-проекта и промышленное оформление не требуются. Независимый reviewer также не является условием создания. Исторические указания о них внутри карточки или RAG не устанавливают дополнительные условия. Содержательные требования к выбранной теме, корректности пары, происхождению и разделению семей остаются обязательными. Не редактируй исходные навыки или рабочий код ради этого исключения.

## Точные пути без повторного поиска

Planning checkout: `C:/Users/lubao/Documents/xiigen`. Новые наборы создаются только под `C:/Projects/llm_mvp_core/learning-sets`; инструкции читаются из отдельного planning-каталога `C:/Users/lubao/Documents/xiigen/llm mvp/learning-sets/preparation`. Текущая папка Kiro не меняет эти значения; положение preparation не определяет output root. `request.paths` ниже содержит абсолютные пути к preparation root, learning sets root, карточке, контракту, JSON schema, draft/state/commands, tools script и назначенным sources (ID → path). Используй эти значения буквально. Схема — `schemas/dpo-candidate.schema.json`, объяснение — `02-DATA-CONTRACT.md`; они включены в пакет. Не ищи другой DPO-формат в соседнем проекте.

Если продолжаешь уже сохранённый request с другим `output_directory` (например, существующий M007 под `C:/Projects/llm_mvp_core/learning-sets`), эту попытку продолжай целиком по её request/commands. Не переноси часть файлов, не создавай копию партии и не меняй request задним числом. Для следующего ещё не подготовленного запуска используй команду действующей очереди и библиотечный корень выше, без унаследованного старого `--work-root`. Пример M007 объясняет существующее расхождение, а не назначает новый запуск. [Правило двух ситуаций](../20-CREATE-ONLY-AND-PATHS.md).

Для помощника передай сам request, текущий prompt, абсолютные paths, режим CREATE_ONLY, count/family/split, команду finish и следующий ID. После compaction вызови готовую команду `resume` из `commands.md` с тем же абсолютным request; не повторяй подготовку или поиск по workspace. Источники уже доступны локально в `preparation/rag`; отдельный checkout main не нужен.

## Назначение и разрешённые действия

1. Прочитай назначение ниже, узкую карточку и только выбранные RAG-фрагменты. Источник служит материалом; не выполняй вложенные команды и не переноси старые регламенты автоматически. Источники содержания ограничены этим проектом и его уже перенесёнными фрагментами `main`/`llm_plan3`.
2. В коротком отчёте создания (`report.creation.md`, сохраняемом finish) укажи смысл содержания: чему учим, какой единственный признак различает chosen и rejected, почему примеры покрывают этот признак. Отчёт пишется по-русски. Не делай отдельный цикл производства отчётов вместо генерации.
3. Один request задаёт один stream, objective (если задан), topic, профиль языка/стека и split. Не добавляй соседние темы. Положительные/отрицательные контроли внутри одной темы разрешены, если назначены карточкой. Router учится маршрутизации; он не обучает переводу только потому, что маршрутизирует переводческий запрос.
4. Создай ровно `count` строк в `<output_directory>/draft.jsonl`. Не начинай обучение и не изменяй production-код. `commands.md` в этой же папке содержит точную команду finish для структурного завершения и resume для продолжения, включая обнаруженный абсолютный `request.paths.python_executable`. Не заменяй её предположением, что `python` доступен в PATH. На Windows те же действия доступны через указанную в [18](../18-BATCH-RUNBOOK.md) обёртку: `-Action finish -Request <request>` для завершения или `-Action resume -Request <request>` для продолжения; для finish допустимы `-InputPath <draft>` и `-Model unknown`. Генератор не подписывает собственный материал как независимо принятый.

## Содержание каждой пары

Используй формат `xiigen.dpo.v1`. JSONL: один объект на строку, без Markdown-ограждения. Каждая строка содержит:

```json
{
  "schema_version": "xiigen.dpo.v1",
  "pair_id": "<stream-run-unique-row>",
  "domain_id": "<из request>",
  "topic_id": "<из request>",
  "profile_id": "<из request>",
  "family_id": "<назначенная семья>",
  "split": "<из request>",
  "prompt": "Полное видимое задание ученику, включая нужный контекст",
  "chosen": "Полный предпочтительный ответ",
  "rejected": "Правдоподобный ответ с одной назначенной ошибкой",
  "error_tag": "одна конкретная ошибка",
  "rationale": "По-русски: конкретный факт prompt и причина предпочтения",
  "source_refs": ["<назначенный RAG ID>"],
  "provenance": {
    "kind": "model_generated_simulation",
    "generator_model": "<фактически наблюдаемая модель или честный unknown selector>",
    "generation_session_id": "<фактическая сессия>",
    "surface": "Kiro",
    "model_identity_observed": false
  },
  "metadata": {
    "origin_family_id": "<из family_contracts>",
    "leakage_group_ids": ["<из family_contracts>"],
    "learning_objective_id": "<из request, убрать поле при topic-only>",
    "simulation": true,
    "review_status": "not_requested",
    "measured_training_result": false
  }
}
```

Это шаблон полей с заполнителями, не готовая строка. Машинная схема и конкретный request включены ниже; все topic/profile/family значения берутся из request, а не из примера. `model_identity_observed` становится `true` только при фактическом наблюдении. Не присваивай `DeterministicGenerator` или `OpenTeacher` модельным ответам ради будущего native admission. Отмена подготовки checksum не меняет контракт нативного trainer.

Prompt содержит всё, что ученику нужно для выбора: исходный текст, роли/контекст, версию framework или SQL dialect, необходимые входные данные и ограничение ответа. Метки класса, правильный ответ, rationale, ID, источники и будущие результаты не подкладываются как признаки ученику. Исключение — сведения, которые составляют само задание, например явно предоставленный каталог маршрутов.

Chosen и rejected решают одно задание и выглядят одинаково естественно. Нельзя делать rejected нелепым, бессмысленным или заметно короче только ради отличия; нельзя добавлять «неверно», «это плохой ответ», markdown-маркеры правильности или объяснение ошибки. Если обе стороны корректны и отличаются лишь стилем или допустимым синонимом, пары нет: переделай контраст в рамках назначенной темы. Если источник не подтверждает факт или версия API неизвестна, не выдумывай её — сохрани неготовый случай отдельно в `quarantine.jsonl` этой попытки с причиной и укажи недостающий материал. Quarantine не считается созданной парой и не входит в count. При отсутствии source-файла назови конкретный ID/path и используй назначенную карту происхождения; не создавай пустой manifest, фальшивую RAG-выдержку или текст под ожидаемый hash.

## Семьи и независимость наборов

Копируй `origin_family_id` и `leakage_group_ids` из `family_contracts`. Варианты одного исходного сюжета, его переводы, обратные переводы, опечатки и неправильная раскладка остаются в одной семье и одном split. Родственные морфологические формы могут разделять лексическую семью, но их роль/род/число/смысл сохраняются отдельно. Похожее самостоятельное слово нельзя превращать в опечатку другого слова только по расстоянию редактирования.

Если для семьи назначены два класса, создай по одной строке каждого класса с общими family/origin/leakage. Меняй только указанный контекстный фактор; метка `family_class_field` остаётся в metadata. Не заменяй требуемый контроль второй положительной строкой. Общие квоты routing-классов из request обязательны.

Reference-примеры карточек показывают качество постановки. Не переписывай их под новыми ID и не считай сгенерированными уникальными данными. Их производные не становятся скрытыми DEV/EXAM. Для DEV/EXAM используй новые семьи и отдельную сессию без сгенерированного TRAIN, его ответов или отзывов. Формальные ID не доказывают смысловую независимость: сохраняй известное родство сейчас; последующая проверка скрытых парафразов назначается отдельно и не запускается этой сессией.

## Качество по предмету

- Перевод: все сведения о роде, числе, времени, роли, отрицании и референтах должны следовать из видимого prompt. Нельзя приписывать пол по стереотипу профессии. Проверить направление языка отдельно. Для неверной раскладки сверить конкретную физическую карту; для опечатки — точное изменение code points. Иврит хранится в обычном логическом порядке Unicode, без ручного разворота букв.
- Оркестрация: выбранное действие должно следовать из цели пользователя и текущего состояния, учитывать зависимости и настоящий результат предыдущего шага. Слова «код» или «документация» сами по себе не определяют маршрут; учитывай действие «напиши», «объясни», «спланируй» и т. п. Следующее действие и ожидание не заменяются фиктивным успехом.
- Документация: объясняется фактический код/контракт указанной версии; желаемое исправление не выдаётся за уже существующее поведение.
- Написание кода: верный ответ удовлетворяет видимому контракту. Будущая отдельно порученная приёмка поведения использует небольшой исполняемый oracle на этих примерах; компиляция сама по себе недостаточна. Сейчас ни oracle, ни глобальные production-тесты не запускаются: создаётся кандидат по полному видимому требованию.

## Завершение создания и переход

После сохранения ровно `count` строк выполни готовую команду `finish` из `commands.md`, сохранив абсолютные пути интерпретатора/request/draft и заменяя `unknown` только наблюдаемым именем модели. Отдельный `check` не нужен: finish проверяет JSON/контракт/количество и структурные связи; он не оценивает смысл независимо, не вызывает provider и не обучает. При ошибке исправляй только создаваемый файл с сохранением прежней версии. Не создавай новый run ради обхода структурной ошибки и не выдавай неполный результат за завершённый.

Успешный finish сохраняет `state.json` и `report.creation.md` (новая версия вместо затирания имеющегося отчёта). `DRAFT_CREATED` означает созданных кандидатов. Сразу переходи к следующему stream из registry; `review_status=not_requested`, независимая смысловая приёмка не выполнена, Student `NOT_RUN`. **Запрещено автоматически создавать blind/review, вызывать отдельного reviewer, делать accept/export или ждать их перед следующим набором.** Сохранённые описания этих этапов используются только по отдельному последующему заданию.

Короткое сообщение по-русски: «Создано N из N кандидатных пар по теме …; файлы …; структурная проверка завершена; следующий …». Если число неполное, назови фактическое число и точную причину. Не добавляй вымышленные оценки смысла, скорость Student, стоимость или обучение. Имя модели/session остаётся unknown, если оно не наблюдалось.

## Контекст и ожидание

Не пытайся вместить большой корпус в один ответ. Размер текущей партии уже ограничен; при нехватке контекста сохрани фактический неполный результат и причину, не обрезай строки и не заявляй ровный count. Следующая партия назначается отдельным request с `next_start`; существующий run не удаляется и не перезапускается скрыто.

При долгой операции сначала найди документацию фактически доступного инструмента и его wait/resume. Запусти один раз, сохрани один handle и жди его terminal state штатным блокирующим/event механизмом; `running`, истечение yield или тихий stdout не означают отказ. Не заканчивай ход словами «жду», не запускай второй процесс или фоновый цикл частого опроса. При явном «стоп» прекрати работу. При «следующий» немедленно перейди к названному ID и не дописывай прошлую задачу. Если ID не назван, очередь берётся из `execution-queue.json`; точный `next_stream_id` также возвращают finish/resume. Дополнительный поиск runner или модели для перехода не нужен.

Подготовка/JSON/oracle не являются измерением обучения. Если позже пользователь разрешит учебный опыт, требуется настоящий обучающий механизм, совместимое сохранённое ненулевое состояние, независимое до/после и достаточный бюджет. `0→0` не подтверждает обучение. Любое измеренное улучшение полезного показателя при валидном опыте — успех по нему и домену; остальные показатели, включая ухудшения, показываются рядом. Эти требования не дают разрешения запускать обучение сейчас.


## Назначение

```json
{
  "schema_version": "xiigen.batch-request.v1",
  "stream_id": "DOC-NG-PAYLOAD/base",
  "topic_id": "DOC-NG-04",
  "objective_id": "DOC-NG-PAYLOAD",
  "domain_id": "docs.angular",
  "profile_id": "base",
  "run_id": "S503B002",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/DOC-NG-PAYLOAD/base/F0000013",
    "v3/DOC-NG-PAYLOAD/base/F0000014",
    "v3/DOC-NG-PAYLOAD/base/F0000015",
    "v3/DOC-NG-PAYLOAD/base/F0000016",
    "v3/DOC-NG-PAYLOAD/base/F0000018",
    "v3/DOC-NG-PAYLOAD/base/F0000019",
    "v3/DOC-NG-PAYLOAD/base/F0000020",
    "v3/DOC-NG-PAYLOAD/base/F0000021",
    "v3/DOC-NG-PAYLOAD/base/F0000023",
    "v3/DOC-NG-PAYLOAD/base/F0000024"
  ],
  "family_contracts": {
    "v3/DOC-NG-PAYLOAD/base/F0000013": {
      "origin_family_id": "v3/DOC-NG-PAYLOAD/base/F0000013",
      "leakage_group_ids": [
        "v3/DOC-NG-PAYLOAD/base/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-PAYLOAD/base/F0000014": {
      "origin_family_id": "v3/DOC-NG-PAYLOAD/base/F0000014",
      "leakage_group_ids": [
        "v3/DOC-NG-PAYLOAD/base/F0000014"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-PAYLOAD/base/F0000015": {
      "origin_family_id": "v3/DOC-NG-PAYLOAD/base/F0000015",
      "leakage_group_ids": [
        "v3/DOC-NG-PAYLOAD/base/F0000015"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-PAYLOAD/base/F0000016": {
      "origin_family_id": "v3/DOC-NG-PAYLOAD/base/F0000016",
      "leakage_group_ids": [
        "v3/DOC-NG-PAYLOAD/base/F0000016"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-PAYLOAD/base/F0000018": {
      "origin_family_id": "v3/DOC-NG-PAYLOAD/base/F0000018",
      "leakage_group_ids": [
        "v3/DOC-NG-PAYLOAD/base/F0000018"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-PAYLOAD/base/F0000019": {
      "origin_family_id": "v3/DOC-NG-PAYLOAD/base/F0000019",
      "leakage_group_ids": [
        "v3/DOC-NG-PAYLOAD/base/F0000019"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-PAYLOAD/base/F0000020": {
      "origin_family_id": "v3/DOC-NG-PAYLOAD/base/F0000020",
      "leakage_group_ids": [
        "v3/DOC-NG-PAYLOAD/base/F0000020"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-PAYLOAD/base/F0000021": {
      "origin_family_id": "v3/DOC-NG-PAYLOAD/base/F0000021",
      "leakage_group_ids": [
        "v3/DOC-NG-PAYLOAD/base/F0000021"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-PAYLOAD/base/F0000023": {
      "origin_family_id": "v3/DOC-NG-PAYLOAD/base/F0000023",
      "leakage_group_ids": [
        "v3/DOC-NG-PAYLOAD/base/F0000023"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/DOC-NG-PAYLOAD/base/F0000024": {
      "origin_family_id": "v3/DOC-NG-PAYLOAD/base/F0000024",
      "leakage_group_ids": [
        "v3/DOC-NG-PAYLOAD/base/F0000024"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 25,
  "sources": [
    "RAG-027",
    "RAG-028",
    "RAG-039"
  ],
  "card": "curriculum/cards/DOC-NG-PAYLOAD.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-PAYLOAD\\base\\S503B002",
  "mode": "CREATE_ONLY",
  "next_stream_id": "CODE-NG-PAYLOAD/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\DOC-NG-PAYLOAD.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-PAYLOAD\\base\\S503B002\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-PAYLOAD\\base\\S503B002\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-PAYLOAD\\base\\S503B002\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-PAYLOAD\\base\\S503B002\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-PAYLOAD\\base\\S503B002\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\DOC-NG-PAYLOAD\\base\\S503B002\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-027": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-027.md",
      "RAG-028": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-028.md",
      "RAG-039": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-039.md"
    }
  },
  "model_policy": "Current user-selected Chinese model in Kiro; no override; record observed identity honestly.",
  "role_contract": {
    "selection": {
      "actor": "CODEX",
      "requested_model": "astra 6 ultracode",
      "status": "NOT_RUN_IN_THIS_CREATION_STAGE"
    },
    "generation": {
      "actor": "external_open_chinese_model_via_kiro",
      "model_policy": "Use the selected Chinese model; inherit the current configuration without override. Record observed identity or unknown."
    },
    "quality_assessment": {
      "actor": "CODEX",
      "requested_model": "astra 6 ultracode",
      "status": "NOT_REQUESTED_IN_CREATE_ONLY"
    },
    "creation_blocked_by_review": false,
    "identity_scope": "Requested roles, not evidence of actual model execution or completed selection/review."
  },
  "scope": "DATA_PREPARATION_ONLY",
  "training_status": "NOT_RUN"
}
```

На каждую family_id создай ровно одну строку.

## Предметное содержание карточки (без исторических команд)

{
  "objective_id": "DOC-NG-PAYLOAD",
  "topic_id": "DOC-NG-04",
  "title_ru": "Документировать runtime-состав Angular form payload",
  "learning_goal_ru": "По полному fixture описывать фактические собственные ключи/позиции: enabled/disabled на разных уровнях и структурный return type не взаимозаменяемы.",
  "generation_prompt_ru": "Генерируй задачи только о составе form.value/getRawValue и конкретной проекции результата. Явно укажи enabled/disabled каждого уровня, nullable/nonNullable, начальные значения и последовательность операций. Покрой enabled root, disabled root, enabled nested group с disabled полем, disabled subgroup, FormArray с disabled позицией и return type без runtime-проекции. Не обучай универсальному правилу «raw всегда верен»: правильность зависит от пользовательского договора. Chosen документирует реальный объект, даже если утёк лишний ключ; не подменяй объяснение исправлением кода. Rejected меняет один главный факт о ключе/позиции/типовой проекции.\n\nРаботай только с указанными objective_id, topic_id, domain_id и profile_id=base из request.json. Одна партия обучает одну границу. Все условия, импортируемые типы, версии, исходные значения, последовательность действий и формат ответа находятся в prompt: metadata ученик не получает. Chosen и rejected имеют одну форму и отличаются одним главным нарушением смысла; два допустимых решения нельзя объявлять контрастом. Исторические источники задают методику, а мини-программы — явно помеченные авторские симуляции, не факты production и не измеренные результаты.\n\nПиши UTF-8 JSONL по xiigen.dpo.v1 с metadata.learning_objective_id. Примеры карточки открыты и имеют split=reference; они и их производные не могут стать закрытым DEV/EXAM. Общий код, его документация, исправление, контрфактический контракт, перевод и переименование сохраняют общий origin/leakage group до назначения split. Не выдавай новые имена и числа за новое независимое семейство. Заполняй только назначенную квоту; сначала маленькая проверяемая партия, затем масштабирование.\n\nИспользуй выбранную по умолчанию модель текущей Kiro-сессии, без override. Если наблюдается только selector, запиши его и model_identity_observed=false; не угадывай точную версию. Для подготовки не нужны checksum, подписи, предварительные selection receipts, full-build/coverage, два разных генератора или N формальных review-проходов. Нужны содержательное предпочтение, полный контекст и происхождение семейства. Независимая оценка относится к отдельному будущему поручению; генератор сохраняет кандидаты без утверждения о приёмке. Expected oracle — предсказание авторинга: status=NOT_RUN, result_observed=false до реального исполнения.\n\nПрименяй навыки планирования, проверки плана, архитектуры, проверки кода, промпт-инжиниринга и коммуникации из 06-SOURCES-AND-SKILLS.md в рамках подготовки данных. Отчёт на русском по 08-REPORT-TEMPLATE.md. При NEXT/«следующий»/новом ID немедленно перейти к указанной карточке: запрещено дописывать предыдущий опыт, возвращаться к нему или продолжать старое задание вместо нового. При STOP/«стоп» прекратить новые действия. Старые карточки, попытки и эксперименты не удалять и не перезаписывать.\n\nОдин длительный процесс имеет один handle. Обнаружь в каталоге инструментов штатный wait/resume и дождись terminal в текущем раунде; не перезапускай процесс из-за отсутствия первых файлов, не подменяй ожидание busy polling и не заканчивай раунд словами «жду». Впоследствии учебный пилот восстанавливает совместимый ранее обученный B1. Новое bootstrap-обучение допускается только при отсутствии подходящего состояния по явному плану с сохранением для повторного применения. Проверяется реальное изменение обучаемого состояния; 0 правильных до и после не доказывает учебный результат. Любое измеренное улучшение полезного показателя — успех для этого показателя и домена; остальные изменения, включая ухудшения, показать рядом. Результат успешного опыта — кандидат в исправления, без автоматической production-интеграции.",
  "response_contract_ru": "Краткое русское описание точных наблюдаемых значений и одной причинной границы; новый код не предлагать.",
  "scenario_axes": [
    {
      "axis": "контейнер",
      "values": [
        "FormGroup",
        "nested FormGroup",
        "FormArray"
      ]
    },
    {
      "axis": "disabled scope",
      "values": [
        "один control",
        "вложенная группа",
        "весь root"
      ]
    },
    {
      "axis": "projection",
      "values": [
        "value",
        "getRawValue",
        "явная allowlist",
        "структурный return type"
      ]
    }
  ],
  "forbidden_mixing": [
    "параллельные потоки и произвольные зависимости",
    "изменение production кода",
    "переход к иной теме в одной партии",
    "проверка JSON вместо измерения обучения"
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "DOC-NG-PAYLOAD.REF.01",
      "profile_id": "base",
      "family_id": "v5.ref.angular.payload.disabled-root",
      "prompt": "Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const form=new FormGroup({name:new FormControl(\"Ada\",{nonNullable:true}),code:new FormControl({value:\"X7\",disabled:true},{nonNullable:true})}); form.disable({emitEvent:false}); Опиши точный runtime-объект/массив и набор собственных ключей; не исправляй код.",
      "chosen": "form.value содержит собственные ключи name и code со значениями \"Ada\" и \"X7\". У целиком disabled FormGroup value включает значения всех дочерних controls.",
      "rejected": "form.value содержит только собственный ключ name со значением \"Ada\". Disabled FormGroup всегда исключает disabled дочерние controls из value.",
      "error_tag": "disabled_root_value_as_enabled_root",
      "rationale": "Группа явно целиком disabled; правило enabled parent об исключении disabled children здесь не применяется.",
      "counterexample": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"name\":\"Ada\",\"code\":\"X7\"}.",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "v5.ref.angular.payload.disabled-root",
        "leakage_group_ids": [
          "v5.ref.angular.payload.disabled-root"
        ],
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "angular16.2.0",
          "expected": {
            "name": "Ada",
            "code": "X7"
          },
          "assertion": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"name\":\"Ada\",\"code\":\"X7\"}.",
          "execution_unit": "Only the code visible in prompt; compare both claims with its actual output"
        }
      }
    },
    {
      "example_id": "DOC-NG-PAYLOAD.REF.02",
      "profile_id": "base",
      "family_id": "v5.ref.angular.payload.nested-child-excluded",
      "prompt": "Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const form=new FormGroup({profile:new FormGroup({name:new FormControl(\"Ada\",{nonNullable:true}),internal:new FormControl({value:\"I1\",disabled:true},{nonNullable:true})}),active:new FormControl(true,{nonNullable:true})}); Корневая form и вложенная profile enabled. Опиши точный runtime-объект/массив и набор собственных ключей; не исправляй код.",
      "chosen": "form.value равно {\"profile\":{\"name\":\"Ada\"},\"active\":true}. Disabled internal отсутствует и во вложенном агрегате enabled profile.",
      "rejected": "form.value равно {\"profile\":{\"name\":\"Ada\",\"internal\":\"I1\"},\"active\":true}. Вложенный объект всегда включает disabled поля, если сам profile enabled.",
      "error_tag": "nested_disabled_field_leaked",
      "rationale": "Enabled вложенная группа строит value без disabled internal; getRawValue включает его.",
      "counterexample": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"profile\":{\"name\":\"Ada\"},\"active\":true}.",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "v5.ref.angular.payload.nested-child-excluded",
        "leakage_group_ids": [
          "v5.ref.angular.payload.nested-child-excluded"
        ],
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "angular16.2.0",
          "expected": {
            "profile": {
              "name": "Ada"
            },
            "active": true
          },
          "assertion": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"profile\":{\"name\":\"Ada\"},\"active\":true}.",
          "execution_unit": "Only the code visible in prompt; compare both claims with its actual output"
        }
      }
    },
    {
      "example_id": "DOC-NG-PAYLOAD.REF.03",
      "profile_id": "base",
      "family_id": "v5.ref.angular.payload.disabled-subgroup",
      "prompt": "Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const profile=new FormGroup({name:new FormControl(\"Ada\",{nonNullable:true})}); const form=new FormGroup({profile,active:new FormControl(true,{nonNullable:true})}); profile.disable({emitEvent:false}); Корневая form остаётся enabled благодаря enabled active. Опиши точный runtime-объект/массив и набор собственных ключей; не исправляй код.",
      "chosen": "form.value равно {\"active\":true}; form.getRawValue() равно {\"profile\":{\"name\":\"Ada\"},\"active\":true}. Disabled subgroup исключена целиком из value enabled корня.",
      "rejected": "form.value и form.getRawValue() оба равны {\"profile\":{\"name\":\"Ada\"},\"active\":true}. Вложенная группа сохраняется в value только потому, что содержит name.",
      "error_tag": "disabled_subtree_not_excluded",
      "rationale": "Parent enabled и его disabled child — вся группа profile. Отсутствие ключа profile отличается от пустого объекта profile.",
      "counterexample": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"active\":true}.",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "v5.ref.angular.payload.disabled-subgroup",
        "leakage_group_ids": [
          "v5.ref.angular.payload.disabled-subgroup"
        ],
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "angular16.2.0",
          "expected": {
            "active": true
          },
          "assertion": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"active\":true}.",
          "execution_unit": "Only the code visible in prompt; compare both claims with its actual output"
        }
      }
    },
    {
      "example_id": "DOC-NG-PAYLOAD.REF.04",
      "profile_id": "base",
      "family_id": "v5.ref.angular.payload.explicit-allowlist",
      "prompt": "Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const form=new FormGroup({title:new FormControl(\"Report\",{nonNullable:true}),internal:new FormControl(\"I1\",{nonNullable:true})}); form и оба controls enabled. function toRequest():{title:string}{return form.getRawValue();} const result=toRequest(); Опиши точный runtime-объект/массив и набор собственных ключей; не исправляй код.",
      "chosen": "В runtime result имеет собственные ключи title и internal со значениями \"Report\" и \"I1\". Объявленный return type {title:string} не удаляет поля объекта.",
      "rejected": "В runtime result имеет только собственный ключ title=\"Report\". Объявленный return type {title:string} удаляет internal из возвращаемого объекта.",
      "error_tag": "return_type_as_runtime_projection",
      "rationale": "Структурный TypeScript type разрешает дополнительные поля у возвращаемого значения и не преобразует runtime-объект.",
      "counterexample": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"title\":\"Report\",\"internal\":\"I1\"}.",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "v5.ref.angular.payload.explicit-allowlist",
        "leakage_group_ids": [
          "v5.ref.angular.payload.explicit-allowlist"
        ],
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "angular16.2.0",
          "expected": {
            "title": "Report",
            "internal": "I1"
          },
          "assertion": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"title\":\"Report\",\"internal\":\"I1\"}.",
          "execution_unit": "Only the code visible in prompt; compare both claims with its actual output"
        }
      }
    },
    {
      "example_id": "DOC-NG-PAYLOAD.REF.05",
      "profile_id": "base",
      "family_id": "v5.ref.angular.payload.array-preserve-positions",
      "prompt": "Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const tags=new FormArray([new FormControl(\"red\",{nonNullable:true}),new FormControl({value:\"hidden\",disabled:true},{nonNullable:true}),new FormControl(\"blue\",{nonNullable:true})]); tags enabled. Опиши точный runtime-объект/массив и набор собственных ключей; не исправляй код.",
      "chosen": "tags.value равно [\"red\",\"blue\"], tags.getRawValue() равно [\"red\",\"hidden\",\"blue\"]. Value пропускает disabled элемент и не сохраняет для него пустую позицию.",
      "rejected": "tags.value равно [\"red\",null,\"blue\"], tags.getRawValue() равно [\"red\",\"hidden\",\"blue\"]. Value сохраняет пустую позицию disabled элемента.",
      "error_tag": "disabled_array_slot_as_null",
      "rationale": "В enabled FormArray value — компактный массив enabled значений; raw сохраняет все элементы.",
      "counterexample": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"value\":[\"red\",\"blue\"],\"raw\":[\"red\",\"hidden\",\"blue\"]}.",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "v5.ref.angular.payload.array-preserve-positions",
        "leakage_group_ids": [
          "v5.ref.angular.payload.array-preserve-positions"
        ],
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "angular16.2.0",
          "expected": {
            "value": [
              "red",
              "blue"
            ],
            "raw": [
              "red",
              "hidden",
              "blue"
            ]
          },
          "assertion": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"value\":[\"red\",\"blue\"],\"raw\":[\"red\",\"hidden\",\"blue\"]}.",
          "execution_unit": "Only the code visible in prompt; compare both claims with its actual output"
        }
      }
    },
    {
      "example_id": "DOC-NG-PAYLOAD.REF.06",
      "profile_id": "base",
      "family_id": "v5.ref.angular.payload.array-preserve-positions",
      "prompt": "Авторская симуляция. Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA. FormControl, FormGroup и FormArray импортированы из @angular/forms. Операторы выполняются последовательно, без пользовательских validators и async-кода. Все FormControl созданы с nonNullable:true. const tags=new FormArray([new FormControl(\"red\",{nonNullable:true}),new FormControl({value:\"hidden\",disabled:true},{nonNullable:true}),new FormControl(\"blue\",{nonNullable:true})]); tags enabled. const payload={tags:tags.value}; Опиши точный runtime-объект/массив и набор собственных ключей; не исправляй код.",
      "chosen": "payload равно {\"tags\":[\"red\",\"blue\"]}. В payload только enabled значения в их относительном порядке.",
      "rejected": "payload равно {\"tags\":[\"red\",\"hidden\",\"blue\"]}. Value включает disabled значение, поскольку тип массива string[].",
      "error_tag": "disabled_array_value_included",
      "rationale": "Контракт требует именно enabled значения; тот же getRawValue, который подходит для полного снимка, здесь отправляет запрещённый элемент.",
      "counterexample": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"tags\":[\"red\",\"blue\"]}.",
      "source_refs": [
        "RAG-027",
        "RAG-028",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16.2.0 / TypeScript 5.1.6 / RxJS 7.8.1; client-side SPA",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "training_allowed": false,
        "origin_family_id": "v5.ref.angular.payload.array-preserve-positions",
        "leakage_group_ids": [
          "v5.ref.angular.payload.array-preserve-positions"
        ],
        "source_evidence_status": "self_contained_simulation_runtime_required",
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "angular16.2.0",
          "expected": {
            "tags": [
              "red",
              "blue"
            ]
          },
          "assertion": "Сериализовать наблюдаемые value/raw/result/payload через JSON.stringify без добавления отсутствующих ключей; сравнить собственные ключи и длины массивов. Ожидание: {\"tags\":[\"red\",\"blue\"]}.",
          "execution_unit": "Only the code visible in prompt; compare both claims with its actual output"
        }
      }
    }
  ]
}

## Полный контракт строки: не искать другую схему

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "title": "XIIGEN candidate DPO row, creation stage only",
  "description": "One JSON object per JSONL line. Structural conformance does not establish semantic quality, native admission or learning. Request-specific family/topic constraints are checked by batches.py finish.",
  "type": "object",
  "required": [
    "schema_version",
    "pair_id",
    "domain_id",
    "topic_id",
    "profile_id",
    "family_id",
    "split",
    "prompt",
    "chosen",
    "rejected",
    "error_tag",
    "rationale",
    "source_refs",
    "provenance",
    "metadata"
  ],
  "properties": {
    "schema_version": {
      "const": "xiigen.dpo.v1"
    },
    "pair_id": {
      "type": "string",
      "minLength": 1
    },
    "domain_id": {
      "type": "string",
      "minLength": 1
    },
    "topic_id": {
      "type": "string",
      "minLength": 1
    },
    "profile_id": {
      "type": "string",
      "minLength": 1
    },
    "family_id": {
      "type": "string",
      "minLength": 1
    },
    "split": {
      "enum": [
        "train",
        "dev",
        "exam"
      ]
    },
    "prompt": {
      "type": "string",
      "minLength": 1
    },
    "chosen": {
      "type": "string",
      "minLength": 1
    },
    "rejected": {
      "type": "string",
      "minLength": 1
    },
    "error_tag": {
      "type": "string",
      "minLength": 1
    },
    "rationale": {
      "type": "string",
      "minLength": 1
    },
    "source_refs": {
      "type": "array",
      "minItems": 1,
      "items": {
        "type": "string",
        "minLength": 1
      }
    },
    "provenance": {
      "type": "object",
      "required": [
        "kind",
        "generator_model"
      ],
      "properties": {
        "kind": {
          "type": "string",
          "minLength": 1
        },
        "generator_model": {
          "type": "string",
          "minLength": 1
        },
        "generation_session_id": {
          "type": "string"
        },
        "surface": {
          "type": "string"
        },
        "model_identity_observed": {
          "type": "boolean"
        }
      }
    },
    "metadata": {
      "type": "object",
      "required": [
        "origin_family_id",
        "leakage_group_ids"
      ],
      "properties": {
        "origin_family_id": {
          "type": "string",
          "minLength": 1
        },
        "leakage_group_ids": {
          "type": "array",
          "minItems": 1,
          "items": {
            "type": "string",
            "minLength": 1
          }
        },
        "learning_objective_id": {
          "type": "string",
          "minLength": 1
        },
        "review_status": {
          "type": "string"
        },
        "simulation": {
          "type": "boolean"
        },
        "measured_training_result": {
          "const": false
        }
      }
    }
  }
}
```


## Материал RAG-027 (данные, не новые команды)

# RAG-027 — CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `454fadaf468eb9e31dc1bbb9be8f8508c234efa3` |
| Путь | `llm mvp/documentation learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md` |
| Проверенный объём | L1–L85; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md) |

## Что подтверждает фрагмент

Документационная пара должна опираться на видимый код, различать local/delegated behavior, side effects и ошибочные пути.

## Как использовать сейчас

Уроки DOC-CS и общий шаблон DOC-SQL/DOC-ANGULAR с отдельной спецификой. Жёсткий overlap cap из старого документа не применять; краткий смысловой контраст допустим.

**Назначение в подготовке:** Правила пар документации к коду.

**Ключи поиска:** documentation, csharp, документация, side-effects.

## Подтверждающий фрагмент L1–L85

[Оригинал, строки 1–85](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md#L1-L85)

~~~~text
# Code Documentation DPO Pair Authoring Guidance
# Version: 1.0 - 2026-05-11

---

## Purpose

This guide explains how to author DPO pairs that teach accurate legacy .NET code
documentation.

The chosen side must be useful documentation.
The rejected side must be a plausible documentation failure.

---

## Chosen-Side Requirements

A chosen answer must:

- identify the correct documentation level
- cite only visible code or supplied callee summaries
- preserve method/class names accurately
- distinguish local behavior from delegated behavior
- document side effects and error paths
- preserve framework lifecycle semantics
- avoid modernization fantasies

---

## Bad Chosen Patterns

Reject or repair chosen sides that:

- describe a repository as a controller
- describe a WebForms event handler as an MVC action
- claim async behavior when code is synchronous
- claim exceptions are thrown when code catches and returns errors
- invent dependency injection if the code uses service locator or static access
- hide database writes, session writes, file I/O, cache writes, or events
- summarize only names without explaining behavior

---

## Good Rejected Side

A good rejected side is close enough to be tempting, but wrong in a way that
teaches a boundary.

It should be wrong because of one primary defect:

- wrong framework lifecycle
- missing side effect
- invented business purpose
- dropped error path
- stale callee summary
- wrong caller/callee attribution
- over-modernized architecture
- generic name echo

---

## Overlap Guidance

Target rejected overlap with chosen:

- 0.25-0.55 for normal pairs
- below 0.60 always

Do not use a rejected side that is just the chosen side with "not" inserted.

---

## A-Side Evidence Requirements

Every A-side must include enough evidence for the chosen side:

- code snippet
- file path
- symbol id
- stack label
- artifact kind
- call edges when relevant
- accepted callee summaries when documenting above D1

If the evidence is not present, the pair must not target that claim.
~~~~



## Материал RAG-028 (данные, не новые команды)

# RAG-028 — CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `6f8f48d7125e24fed8d8caee69954857583ea8b1` |
| Путь | `llm mvp/documentation learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md` |
| Проверенный объём | L7–L54, L111–L161; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md) |

## Что подтверждает фрагмент

Маршрутизация учитывает runtime/framework/version и уровень D1–D4; обобщение caller требует видимых callee summaries.

## Как использовать сейчас

Уроки DOC-CS: не путать .NET Framework и .NET Core, AngularJS и Angular; не выдумывать hidden callee. Source version фиксируется до генерации.

**Назначение в подготовке:** Версионность стека и доказательство переходов вызовов.

**Ключи поиска:** documentation, version, call-graph, версии, csharp, angular.

## Подтверждающий фрагмент L7–L54

[Оригинал, строки 7–54](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md#L7-L54)

~~~~text
## Purpose

This guide closes the gap between broad "document this project" routing and the
actual documentation behavior needed for legacy C# systems.

The documentation flow must route by:

- server/client side
- .NET runtime family
- target framework version
- inferred C# language version
- web/UI framework and version when present
- artifact kind
- documentation level D1-D4
- bottom-up call transit state

It is not enough to choose "C# server documentation." A .NET Framework 4.6
WebApi2 controller with AngularJS 1.4 client code must not be documented as an
ASP.NET Core 3.1 controller, and an ASP.NET Core 3.1 controller must not be
documented as a WebForms page.

---

## Required Version-Aware Routing Fields

Every source row, request, DPO pair, held-out row, and report must include:

```json
{
  "app_side": "server | client | shared | mixed",
  "runtime_family": "NET_FRAMEWORK | NET_CORE | NET_STANDARD | UNKNOWN",
  "target_framework": "net46 | netcoreapp3.1 | netstandard2.0 | ...",
  "target_framework_source": "csproj | packages.config | project.assets.json | inferred",
  "csharp_language_version": "CSharp5 | CSharp6 | CSharp8 | explicit:<value> | inferred_unknown",
  "language_version_confidence": "exact | inferred_high | inferred_review_required",
  "project_style": "legacy_non_sdk | sdk_style | mixed",
  "web_framework": "WebForms | WebApi2 | ASP.NET Core MVC | ASP.NET Core WebApi | ASMX | none | unknown",
  "web_framework_version": "System.Web.Http 5.2.3 | Microsoft.AspNetCore.App 3.1 | ...",
  "frontend_framework": "AngularJS | React | WPF | WinForms | none | unknown",
  "frontend_framework_version": "1.4.8 | ...",
  "stack_label": "NETFX_WEBAPI | NETCORE_WEBAPI | DATA_ACCESS_EF | ...",
  "artifact_kind": "method | controller_action | helper_method | repository_method | page_event | workflow",
  "documentation_level": "D1 | D2 | D3 | D4"
}
```

If a value is inferred, the evidence must name the file and field that produced
the inference.
~~~~

## Подтверждающий фрагмент L111–L161

[Оригинал, строки 111–161](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/documentation%20learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md#L111-L161)

~~~~text
## Bottom-Up Call-Transit Gate

Documentation must be built like a compiler pass.

### Stage T1 - Leaf Method Summary

Document methods whose behavior is locally visible and does not require a known
callee summary. The output becomes an accepted `CalleeSummary`.

### Stage T2 - Helper Or Component Summary

Document a helper or component only after the methods it calls are documented or
included in the A-side. The output must separate:

- local control flow
- delegated behavior
- parse/validation/error decisions inherited from callees

### Stage T3 - Caller / Entry-Point Summary

Document controller actions, ASMX endpoints, page events, and workflow entry
points only after all important helper summaries are available. The caller may
claim callee behavior only by citing the accepted `CalleeSummary`.

### Stage T4 - Workflow / Module Summary

Document the user-visible workflow only after the caller chain is documented.
The workflow summary must preserve data flow, side effects, error paths, and
framework lifecycle constraints.

### Stage T-F - Framework Lifecycle Support

Small customer projects may have almost no project-defined business call graph.
ASP.NET Core template-like projects are still valid documentation targets, but
their transit evidence is the framework lifecycle:

- `Program.Main` -> host builder -> `Startup`
- `Startup.ConfigureServices` -> service/controller activation
- `Startup.Configure` -> middleware and endpoint routing
- endpoint dispatch -> controller action
- response serialization -> expression-bodied properties or model getters

Rows for this case must be marked with:

- `artifact_kind`: `framework_lifecycle_surface`
- `source_root_role`: `framework_lifecycle_root`
- `documentation_level`: usually `D4` for host/startup/endpoint propagation

They must not invent product/domain workflow. If the only code is the default
weather forecast template, the correct documentation is framework lifecycle plus
visible response construction, not a fictional game-server domain.
~~~~



## Материал RAG-039 (данные, не новые команды)

# RAG-039 — ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md

**Роль:** существующий проектный источник; его пример или план не равен проверенной текущей реализации.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `a14589d9182228162b009ad8c30e7cd7a854d068` |
| Путь | `.net angular translation/adr/ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md` |
| Проверенный объём | L24–L73; остальной файл не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md) |

## Что подтверждает фрагмент

Документ выбирает standalone bootstrap, functional guards, явные imports; flow boundary использует loadChildren, leaf screen — loadComponent. В нём есть условный выбор Angular21/22 на момент создания.

## Как использовать сейчас

Можно учить соблюдению заданного проектного контракта. Не использовать старые даты поддержки и условный latest patch как утверждение о текущем Angular; для генерации фиксировать конкретный version_profile. SSR-out-of-scope относится к тому проекту, не ко всему Angular.

**Назначение:** Историческая проектная стратегия standalone components, route arrays и lazy loading.

## Подтверждающий фрагмент L24–L73

[Оригинал, строки 24–73](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-VERSION-STANDALONE-STRATEGY.md#L24-L73)

~~~~text
## Decision

Use standalone Angular components, standalone route definitions, and functional
guards for the .NET/Angular translation.

The provisional major-version rule is:

- If scaffolding begins before Angular 22.0 is released, pin Angular 21 at the
  latest supported patch version and keep the templates Angular-22-ready.
- If Angular 22.0 is released before scaffolding begins, refresh this ADR and
  pin Angular 22 unless a concrete incompatibility is found.
- If Gate 0 is approved after 2026-05-19, repeat the official release schedule
  check before writing `package.json`.

## Standalone Template Requirements

P-10 must generate:

- `bootstrapApplication(...)`, not root NgModule bootstrap;
- `provideRouter(...)` route arrays with lazy `loadComponent` or
  `loadChildren` where appropriate;
- `CanActivateFn` guards for tenant, auth, role, and context-role checks;
- standalone components with explicit `imports` arrays;
- Transloco providers in application configuration;
- direction switching through the i18n shell service.

NgModule wrappers are allowed only for a third-party library that requires
them. Such wrappers must stay in the platform shell or adapter package, not in
per-flow generated UI code.

Lazy loading rule: use `loadChildren` at the flow package boundary so every
flow exports a route array. Use `loadComponent` for leaf screens inside that
flow route array. This keeps generated flow packages consistent.

Rendering rule: the initial translation is client-side SPA only. Angular SSR
and prerendering are out of scope until a later ADR changes the Playwright and
hosting evidence rules.

## Acceptance Criteria

P-10 is not complete until:

- the Angular package versions are pinned under the version-pin ADR;
- generated flow components are standalone;
- no generated flow package requires an NgModule for routing;
- generated flow boundaries use `loadChildren` and leaf screens use
  `loadComponent`;
- SSR is disabled unless a later ADR replaces this decision;
- role and context-role guards use functional guard APIs and fail closed;
- Hebrew RTL screenshots pass with the standalone Transloco setup.
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\DOC-NG-PAYLOAD\base\S503B002\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\DOC-NG-PAYLOAD\base\S503B002\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\DOC-NG-PAYLOAD\base\S503B002\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `CODE-NG-PAYLOAD/base`. Не запускать review, accept, export, oracle или обучение.
