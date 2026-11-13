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
  "stream_id": "CODE-NG-DISABLED/base",
  "topic_id": "CODE-NG-01",
  "objective_id": "CODE-NG-DISABLED",
  "domain_id": "code.angular",
  "profile_id": "base",
  "run_id": "T436",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/CODE-NG-DISABLED/base/F0000026",
    "v3/CODE-NG-DISABLED/base/F0000028",
    "v3/CODE-NG-DISABLED/base/F0000029",
    "v3/CODE-NG-DISABLED/base/F0000030",
    "v3/CODE-NG-DISABLED/base/F0000031",
    "v3/CODE-NG-DISABLED/base/F0000032",
    "v3/CODE-NG-DISABLED/base/F0000033",
    "v3/CODE-NG-DISABLED/base/F0000035",
    "v3/CODE-NG-DISABLED/base/F0000037",
    "v3/CODE-NG-DISABLED/base/F0000038"
  ],
  "family_contracts": {
    "v3/CODE-NG-DISABLED/base/F0000026": {
      "origin_family_id": "v3/CODE-NG-DISABLED/base/F0000026",
      "leakage_group_ids": [
        "v3/CODE-NG-DISABLED/base/F0000026"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-NG-DISABLED/base/F0000028": {
      "origin_family_id": "v3/CODE-NG-DISABLED/base/F0000028",
      "leakage_group_ids": [
        "v3/CODE-NG-DISABLED/base/F0000028"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-NG-DISABLED/base/F0000029": {
      "origin_family_id": "v3/CODE-NG-DISABLED/base/F0000029",
      "leakage_group_ids": [
        "v3/CODE-NG-DISABLED/base/F0000029"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-NG-DISABLED/base/F0000030": {
      "origin_family_id": "v3/CODE-NG-DISABLED/base/F0000030",
      "leakage_group_ids": [
        "v3/CODE-NG-DISABLED/base/F0000030"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-NG-DISABLED/base/F0000031": {
      "origin_family_id": "v3/CODE-NG-DISABLED/base/F0000031",
      "leakage_group_ids": [
        "v3/CODE-NG-DISABLED/base/F0000031"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-NG-DISABLED/base/F0000032": {
      "origin_family_id": "v3/CODE-NG-DISABLED/base/F0000032",
      "leakage_group_ids": [
        "v3/CODE-NG-DISABLED/base/F0000032"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-NG-DISABLED/base/F0000033": {
      "origin_family_id": "v3/CODE-NG-DISABLED/base/F0000033",
      "leakage_group_ids": [
        "v3/CODE-NG-DISABLED/base/F0000033"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-NG-DISABLED/base/F0000035": {
      "origin_family_id": "v3/CODE-NG-DISABLED/base/F0000035",
      "leakage_group_ids": [
        "v3/CODE-NG-DISABLED/base/F0000035"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-NG-DISABLED/base/F0000037": {
      "origin_family_id": "v3/CODE-NG-DISABLED/base/F0000037",
      "leakage_group_ids": [
        "v3/CODE-NG-DISABLED/base/F0000037"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-NG-DISABLED/base/F0000038": {
      "origin_family_id": "v3/CODE-NG-DISABLED/base/F0000038",
      "leakage_group_ids": [
        "v3/CODE-NG-DISABLED/base/F0000038"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 39,
  "sources": [
    "RAG-037",
    "RAG-038",
    "RAG-039"
  ],
  "card": "curriculum/cards/CODE-NG-DISABLED.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-NG-DISABLED\\base\\T436",
  "mode": "CREATE_ONLY",
  "next_stream_id": "OR-AUTO-TERMINAL/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\CODE-NG-DISABLED.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-NG-DISABLED\\base\\T436\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-NG-DISABLED\\base\\T436\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-NG-DISABLED\\base\\T436\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-NG-DISABLED\\base\\T436\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-NG-DISABLED\\base\\T436\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-NG-DISABLED\\base\\T436\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-037": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-037.md",
      "RAG-038": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-038.md",
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
  "objective_id": "CODE-NG-DISABLED",
  "topic_id": "CODE-NG-01",
  "title_ru": "Написать boolean disabled binding",
  "learning_goal_ru": "Реализовать ровно заданный boolean контракт отключения native button средствами Angular property binding.",
  "generation_prompt_ru": "Ответ — один <button type=\"button\"> с текстом Save и [disabled] выражением. Prompt фиксирует типы полей и точное условие отключения. Все ветви truth table указываются в metadata для oracle, expected не выдаётся за наблюдённое. Rejected использует одну правдоподобную логическую ошибку: инверсия busy, потеря !canSave, && вместо || или потеря !force. Не добавляй event handlers, interpolation disabled=\"false\", атрибутные/доступностные изменения и CSS в том же контрасте.\n\nРаботай только с данным objective_id и назначенными family_id/split/count из request.json. Пиши одну смысловую пару на назначенное семейство; chosen и rejected должны иметь одинаковый формат и отличаться одной проверяемой ошибкой. Все сведения, версии, исходные данные и договор ответа помещай в prompt: metadata learner не получает. Примеры ниже — открытые авторские reference seeds, не закрытый EXAM, не сгенерированный китайской моделью корпус и не измеренные результаты. Не копируй reference-строки в TRAIN под новым ID. Переименование переменных/таблиц и замена чисел не делают шаблон новым независимым семейством. Для документации объясняй именно показанный код, для генерации возвращай только запрошенный код. Не вставляй в ответы labels, rationale, source ID и предпочтительную позицию. Oracle expected — заранее назначенное ожидание; статус остаётся NOT_RUN, пока соответствующий runtime действительно не выполнен. Runtime oracle сейчас не запускай; сохрани его ожидания как NOT_RUN, не выдавай их за наблюдённый результат. Не переходи к другой теме внутри пакета. При NEXT немедленно используй следующий ID из реестра, при STOP прекрати новые действия. Production и старые эксперименты не менять; отчёт на русском по 08-REPORT-TEMPLATE.md, фактическая текущая Kiro-модель без override.",
  "response_contract_ru": "Один HTML button с type=\"button\", текстом Save и единственным binding [disabled]; без объяснений, Markdown и других элементов.",
  "scenario_axes": [
    {
      "axis": "условие",
      "values": [
        "busy true",
        "canSave false",
        "busy или invalid",
        "busy и не force"
      ]
    },
    {
      "axis": "матрица",
      "values": [
        "все boolean комбинации"
      ]
    }
  ],
  "forbidden_mixing": [
    "HTTP/RxJS",
    "форма/валидаторы",
    "visibility",
    "события кнопки",
    "дизайн/RTL"
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "CODE-NG-DISABLED.REF.01",
      "profile_id": "base",
      "family_id": "ref.ng.disabled.busy.01",
      "prompt": "Авторская симуляция. Angular 16.2 / TypeScript 5.1. Поле busy:boolean. Кнопка должна быть отключена ровно когда busy=true. Напиши только один native button type=\"button\" с текстом Save и Angular [disabled] binding. Нет других bindings/host directives; после изменения полей вызывается fixture.detectChanges().",
      "chosen": "<button type=\"button\" [disabled]=\"busy\">Save</button>",
      "rejected": "<button type=\"button\" [disabled]=\"!busy\">Save</button>",
      "error_tag": "binding_polarity",
      "rationale": "Одна изменённая boolean операция нарушает явно заданную таблицу отключения; обе стороны имеют один формат.",
      "counterexample": "busy=false: expected disabled=false; rejected=true.",
      "source_refs": [
        "RAG-037",
        "RAG-038",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16.2 / TypeScript 5.1; TestBed DOM fixture; detectChanges after every assignment",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.ng.disabled.busy.01",
        "leakage_group_ids": [
          "ref.ng.disabled.busy.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "angular16.2-testbed",
          "assignments": [
            {
              "busy": false
            },
            {
              "busy": true
            }
          ],
          "expected_disabled": [
            false,
            true
          ],
          "assertion": "Render each template separately, assign component booleans, detectChanges, read HTMLButtonElement.disabled."
        }
      }
    },
    {
      "example_id": "CODE-NG-DISABLED.REF.02",
      "profile_id": "base",
      "family_id": "ref.ng.disabled.can-save.01",
      "prompt": "Авторская симуляция. Angular 16.2 / TypeScript 5.1. Поле canSave:boolean. Кнопка должна быть отключена ровно когда canSave=false. Напиши только один native button type=\"button\" с текстом Save и Angular [disabled] binding. Нет других bindings/host directives; после изменения полей вызывается fixture.detectChanges().",
      "chosen": "<button type=\"button\" [disabled]=\"!canSave\">Save</button>",
      "rejected": "<button type=\"button\" [disabled]=\"canSave\">Save</button>",
      "error_tag": "negation_dropped",
      "rationale": "Одна изменённая boolean операция нарушает явно заданную таблицу отключения; обе стороны имеют один формат.",
      "counterexample": "canSave=true: expected disabled=false; rejected=true.",
      "source_refs": [
        "RAG-037",
        "RAG-038",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16.2 / TypeScript 5.1; TestBed DOM fixture; detectChanges after every assignment",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.ng.disabled.can-save.01",
        "leakage_group_ids": [
          "ref.ng.disabled.can-save.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "angular16.2-testbed",
          "assignments": [
            {
              "canSave": false
            },
            {
              "canSave": true
            }
          ],
          "expected_disabled": [
            true,
            false
          ],
          "assertion": "Render each template separately, assign component booleans, detectChanges, read HTMLButtonElement.disabled."
        }
      }
    },
    {
      "example_id": "CODE-NG-DISABLED.REF.03",
      "profile_id": "base",
      "family_id": "ref.ng.disabled.or.01",
      "prompt": "Авторская симуляция. Angular 16.2 / TypeScript 5.1. Поля busy:boolean и invalid:boolean. Кнопка отключена, если хотя бы одно из них true. При обоих false она доступна. Напиши только один native button type=\"button\" с текстом Save и Angular [disabled] binding. Нет других bindings/host directives; после изменения полей вызывается fixture.detectChanges().",
      "chosen": "<button type=\"button\" [disabled]=\"busy || invalid\">Save</button>",
      "rejected": "<button type=\"button\" [disabled]=\"busy && invalid\">Save</button>",
      "error_tag": "or_changed_to_and",
      "rationale": "Одна изменённая boolean операция нарушает явно заданную таблицу отключения; обе стороны имеют один формат.",
      "counterexample": "busy=true, invalid=false: expected disabled=true; rejected=false.",
      "source_refs": [
        "RAG-037",
        "RAG-038",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16.2 / TypeScript 5.1; TestBed DOM fixture; detectChanges after every assignment",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.ng.disabled.or.01",
        "leakage_group_ids": [
          "ref.ng.disabled.or.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "angular16.2-testbed",
          "assignments": [
            {
              "busy": false,
              "invalid": false
            },
            {
              "busy": true,
              "invalid": false
            },
            {
              "busy": false,
              "invalid": true
            },
            {
              "busy": true,
              "invalid": true
            }
          ],
          "expected_disabled": [
            false,
            true,
            true,
            true
          ],
          "assertion": "Render each template separately, assign component booleans, detectChanges, read HTMLButtonElement.disabled."
        }
      }
    },
    {
      "example_id": "CODE-NG-DISABLED.REF.04",
      "profile_id": "base",
      "family_id": "ref.ng.disabled.force.01",
      "prompt": "Авторская симуляция. Angular 16.2 / TypeScript 5.1. Поля busy:boolean и force:boolean. Кнопка отключена ровно при busy=true и force=false. Force=true разрешает кнопку независимо от busy. Напиши только один native button type=\"button\" с текстом Save и Angular [disabled] binding. Нет других bindings/host directives; после изменения полей вызывается fixture.detectChanges().",
      "chosen": "<button type=\"button\" [disabled]=\"busy && !force\">Save</button>",
      "rejected": "<button type=\"button\" [disabled]=\"busy && force\">Save</button>",
      "error_tag": "force_polarity",
      "rationale": "Одна изменённая boolean операция нарушает явно заданную таблицу отключения; обе стороны имеют один формат.",
      "counterexample": "busy=true, force=true: expected disabled=false; rejected=true.",
      "source_refs": [
        "RAG-037",
        "RAG-038",
        "RAG-039"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "Angular 16.2 / TypeScript 5.1; TestBed DOM fixture; detectChanges after every assignment",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.ng.disabled.force.01",
        "leakage_group_ids": [
          "ref.ng.disabled.force.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "angular16.2-testbed",
          "assignments": [
            {
              "busy": false,
              "force": false
            },
            {
              "busy": true,
              "force": false
            },
            {
              "busy": false,
              "force": true
            },
            {
              "busy": true,
              "force": true
            }
          ],
          "expected_disabled": [
            false,
            true,
            false,
            false
          ],
          "assertion": "Render each template separately, assign component booleans, detectChanges, read HTMLButtonElement.disabled."
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


## Материал RAG-037 (данные, не новые команды)

# RAG-037 — ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md

**Роль:** существующий проектный источник; его пример или план не равен проверенной текущей реализации.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `d182324879939f54ae447060f0f0fbf03b15e04f` |
| Путь | `.net angular translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md` |
| Проверенный объём | L22–L89, L185–L245, L297–L333; остальной файл не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md) |

## Что подтверждает фрагмент

ADR содержит конкретные TypeScript-контракты и примеры provideAppInitializer, authRefresh single-flight и loadChildren/loadComponent. Это план реализации, не извлечённый работающий Angular-модуль.

## Как использовать сейчас

Материал для DOC-ANGULAR и CODE-ANGULAR: описать показанный интерфейс, отличать Promise окончания загрузки от раннего ready, объяснить ограничение повторного refresh. Указанный порядок guard-массива не считать доказательством последовательного ожидания асинхронных guard: зависимость явно обеспечить кодом и проверить. Версию API задаёт карточка, не старый статус Approved.

**Назначение:** Существующие TypeScript-примеры: интерфейс runtime config, initializer, единственный AuthSessionService и lazy route array.

## Подтверждающий фрагмент L22–L89

[Оригинал, строки 22–89](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md#L22-L89)

~~~~text
## Runtime Config

The shell loads this file before route activation:

```text
assets/xiigen-runtime-config.json
```

The loader runs as an Angular application initializer
(`provideAppInitializer` / `APP_INITIALIZER` equivalent for the selected Angular
major) before router initial navigation. Guards, interceptors, Transloco
loaders, and feature routes must wait on `RuntimeConfigService.ready`; runtime
config must not be loaded through per-route resolvers because those fire too
late for the first auth/tenant guard decision.

`RuntimeConfigService` has this minimum shape:

```typescript
export interface XiigenRuntimeConfig {
  apiBaseUrl: string;
  tenantResolution: "subdomain" | "path" | "header" | "single-tenant";
  tenantIdHint: string | null;
  features: Record<string, boolean | string | number>;
  auth: {
    tokenStorage: "memory";
    refreshEnabled: boolean;
    refreshMode: "reactive-401-only" | "preemptive-and-reactive";
    preemptiveRefreshSkewSeconds: number;
    preserveReturnUrl: true;
  };
  correlationHeader: "X-Correlation-Id";
}

export class RuntimeConfigService {
  readonly ready: Signal<boolean>;
  load(): Promise<void>;
  whenReady(): Promise<XiigenRuntimeConfig>;
  get snapshot(): XiigenRuntimeConfig;
}
```

The application initializer returns `runtimeConfigService.load()`. Guards and
interceptors call `whenReady()` when they may run before `ready()` is true.
`snapshot` throws a fail-closed configuration error if accessed before load
completion.

The Angular bootstrap sequence has one required initializer in the first
implementation:

```typescript
provideAppInitializer(() => inject(RuntimeConfigService).load())
```

No token refresh, tenant resolution network call, feature flag reload, or
context-role resolver may run as a bootstrap initializer. Those depend on the
runtime config and therefore run through guards/interceptors after
`RuntimeConfigService.ready` is true. `FeatureFlagService.reload(config)` is
called synchronously from the runtime config load success path. This keeps
initial navigation deterministic: config first, router second, auth/tenant
guards third.

`RuntimeConfigService` is a root singleton:

```typescript
@Injectable({ providedIn: "root" })
export class RuntimeConfigService { ... }
```

~~~~
## Подтверждающий фрагмент L185–L245

[Оригинал, строки 185–245](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md#L185-L245)

~~~~text
## HTTP Interceptors

Use functional interceptors:

- `authInterceptor: HttpInterceptorFn` adds `Authorization: Bearer <token>`
  when `AuthSessionService` has an in-memory access token.
- `tenantInterceptor: HttpInterceptorFn` adds `X-Tenant-Id` only when the
  server-side tenant resolution mode requires a header.
- `correlationInterceptor: HttpInterceptorFn` adds `X-Correlation-Id` from
  `CorrelationContextService` or creates one for the command/request.
- `authRefreshInterceptor: HttpInterceptorFn` performs one bounded refresh on
  401 when refresh is enabled; otherwise it clears session state and routes to
  login/denied.

`tenantInterceptor` reads `RuntimeConfigService.snapshot.tenantResolution`.
It adds the `X-Tenant-Id` header only when all of these are true:

- `tenantResolution` is `"header"`;
- `tenantIdHint` is present and non-empty;
- the request URL targets `apiBaseUrl` or a relative API URL owned by XIIGen;
- the request is not a third-party telemetry, asset, translation-bundle, or
  external provider request.

For `subdomain`, `path`, or `single-tenant`, the interceptor must not add
`X-Tenant-Id`; the server derives tenant scope from the approved resolution
source. A runtime config with `tenantResolution = "header"` and no
`tenantIdHint` fails schema validation before interceptors run.

JWT access tokens are stored in memory by default. Persistent token storage
requires a separate security ADR.

`AuthSessionService` is the only token source for interceptors:

```typescript
@Injectable({ providedIn: "root" })
export class AuthSessionService {
  readonly accessToken: Signal<string | null>;
  readonly refreshInProgress: Signal<boolean>;
  hasToken(): boolean;
  getAccessToken(): string | null;
  setSession(response: AuthLoginResponse): void;
  clearSession(reason: "logout" | "expired" | "refresh-failed"): void;
  refreshOnce(): Promise<boolean>;
}
```

Generated code must not read tokens from `RuntimeConfigService`, browser local
storage, session storage, cookies, or component state.

Refresh handling is single-flight and loop-safe:

1. `authRefreshInterceptor` catches a 401 only once per original request.
2. It marks the retried request with an internal `XIIGEN_REFRESH_RETRY` context
   token so the same request cannot trigger a second refresh loop.
3. If another refresh is already in progress, it awaits that in-memory promise
   instead of calling `/auth/refresh` again.
4. On refresh success, it updates `AuthSessionService` and retries the original
   request once.
5. On refresh failure, missing refresh token, or second 401 after retry, it
   clears the session and routes to login with the preserved `returnUrl`.
6. Refresh requests themselves are excluded from refresh interception.
~~~~
## Подтверждающий фрагмент L297–L333

[Оригинал, строки 297–333](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-RUNTIME-CONFIG-HTTP-SSR.md#L297-L333)

~~~~text
## Lazy Loading

The platform shell lazy-loads each flow as a route array:

```typescript
{
  path: "flows/FLOW-XX-<slug>",
  canActivate: [
    configLoadedGuard,
    tenantGuard,
    authGuard,
    roleGuard,
    contextRoleGuard
  ],
  loadChildren: () => import("./flows/FLOW-XX-<slug>/routes")
    .then(m => m.FLOW_XX_ROUTES)
}
```

Inside a flow route array, leaf screens use `loadComponent`. Use
`loadChildren` at the flow boundary and `loadComponent` for leaf views. This
keeps generated packages consistent while allowing fine-grained screen loading.

Guard activation order is fixed and fail-closed:

1. `configLoadedGuard` awaits `RuntimeConfigService.whenReady()` and redirects
   to a configuration-error route on validation/load failure.
2. `tenantGuard` resolves tenant context from the approved runtime-config mode.
3. `authGuard` verifies an in-memory access token or redirects to login with
   `returnUrl`.
4. `roleGuard` checks canonical role snapshots for route policies.
5. `contextRoleGuard` calls the server/context-role resolver when the policy
   requires business-state-derived roles.

Routes may omit `roleGuard` or `contextRoleGuard` only when the auth policy
registry marks the endpoint/screen as not requiring those gates. Routes must
not put auth, role, or context-role guards before `configLoadedGuard`.
~~~~



## Материал RAG-038 (данные, не новые команды)

# RAG-038 — ADR-ANGULAR-I18N-RTL-STRATEGY.md

**Роль:** существующий проектный источник; его пример или план не равен проверенной текущей реализации.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `57c6e5f8ace612eba5834310e7fe9029d8988764` |
| Путь | `.net angular translation/adr/ADR-ANGULAR-I18N-RTL-STRATEGY.md` |
| Проверенный объём | L17–L65, L67–L80; остальной файл не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-I18N-RTL-STRATEGY.md) |

## Что подтверждает фрагмент

Показан объект provideTransloco и требуемая синхронизация active language, html.lang и html.dir. Tenant overlay отделён от публичных bundled assets.

## Как использовать сейчас

Источник небольших задач по описанию конфигурации, fallback и RTL. Это UI-localization, не обучение переводческого домена. Сохранить явно заданные языки и версию fixture; не добавлять неподтверждённые библиотечные API и не называть эту выборку полным Angular-reference.

**Назначение:** Готовая конфигурационная иллюстрация provideTransloco, порядок bundle/overlay и изменение lang/dir.

## Подтверждающий фрагмент L17–L65

[Оригинал, строки 17–65](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-I18N-RTL-STRATEGY.md#L17-L65)

~~~~text
## Proposed Default

Use Transloco for Angular runtime translations.

- English is bundled with the application as the first-load fallback.
- Hebrew and other non-English bundles are fetched through the platform i18n
  endpoint or flow manifest bundle endpoint.
- Tenant-specific wording can be layered over base language bundles through the
  same runtime loader.
- Missing translation keys fail tests in CI for required screen states.
- The platform shell sets `document.documentElement.dir` to `rtl` for `he`,
  `ar`, `fa`, and `ur`, and to `ltr` for other locales.
- The platform shell also sets `document.documentElement.lang` to the resolved
  base or BCP-47 language tag used by Transloco.
- Angular components use logical CSS direction rules and layout utilities
  rather than physical left/right assumptions.

Root configuration uses `provideTransloco` in the standalone shell:

```typescript
provideTransloco({
  config: {
    availableLangs: ["en", "he", "ar", "fa", "ur"],
    defaultLang: "en",
    fallbackLang: "en",
    reRenderOnLangChange: true,
    prodMode: environment.production
  },
  loader: TranslocoHttpLoader
})
```

The loader resolves bundles in this order:

1. shell fallback bundle: `/assets/i18n/{lang}.json`;
2. flow bundle: `/assets/i18n/flows/{flowSlug}/{lang}.json`;
3. tenant overlay bundle:
   `/api/i18n/tenants/{tenantId}/flows/{flowSlug}/{lang}`;
4. English fallback when the requested non-English key is missing.

English shell strings are bundled for first paint. Flow bundles and
non-English bundles are lazy-loaded when a flow route activates. Tenant overlay
bundles are runtime data and are never built into the Angular package.

The language selector accepts BCP-47 tags and maps them to the supported base
language when safe, for example `he-IL` to `he`. Unsupported languages fail
closed to `en` and log a non-sensitive warning with the correlation id. The
selector updates Transloco active language, `document.documentElement.dir`, and
`document.documentElement.lang`.
~~~~
## Подтверждающий фрагмент L67–L80

[Оригинал, строки 67–80](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/.net%20angular%20translation/adr/ADR-ANGULAR-I18N-RTL-STRATEGY.md#L67-L80)

~~~~text
## Tenant Overlay Endpoint

Tenant overlay bundles are served by the platform i18n endpoint:

```text
GET /api/i18n/tenants/{tenantId}/flows/{flowSlug}/{lang}
```

The endpoint is implemented in the platform i18n module, not in each generated
flow service. It has auth posture `policy` with policy name `TenantI18nRead`.
It is tenant-scoped, requires an authenticated tenant context, and rejects
cross-tenant reads even when the route tenant id and active tenant id differ
only by case or alias. Public/anonymous screens use bundled shell or flow
assets only; tenant overlay runtime wording is never exposed anonymously.
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

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\CODE-NG-DISABLED\base\T436\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\CODE-NG-DISABLED\base\T436\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\CODE-NG-DISABLED\base\T436\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `OR-AUTO-TERMINAL/base`. Не запускать review, accept, export, oracle или обучение.
