# Создание одной партии DPO в Kiro — CREATE_ONLY

<!-- CURRENT-LANGUAGE-AND-CAMPAIGN -->
Reason internally in English. Do not output private reasoning. Russian status; learner language follows the assigned profile. This prompt remains for an explicitly selected legacy request. New domain quotas and continuation use [campaign/registry.json](../campaign/registry.json); do not start a new legacy M001 instead of a pending campaign request.
<!-- /CURRENT-LANGUAGE-AND-CAMPAIGN -->


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
  "stream_id": "OR-QA-02/base",
  "topic_id": "OR-QA-02",
  "objective_id": null,
  "domain_id": "or.qa",
  "profile_id": "base",
  "run_id": "V2P001",
  "split": "train",
  "count": 25,
  "family_ids": [
    "v3/OR-QA-02/base/F0000000",
    "v3/OR-QA-02/base/F0000001",
    "v3/OR-QA-02/base/F0000003",
    "v3/OR-QA-02/base/F0000004",
    "v3/OR-QA-02/base/F0000006",
    "v3/OR-QA-02/base/F0000007",
    "v3/OR-QA-02/base/F0000008",
    "v3/OR-QA-02/base/F0000011",
    "v3/OR-QA-02/base/F0000012",
    "v3/OR-QA-02/base/F0000013",
    "v3/OR-QA-02/base/F0000015",
    "v3/OR-QA-02/base/F0000016",
    "v3/OR-QA-02/base/F0000017",
    "v3/OR-QA-02/base/F0000018",
    "v3/OR-QA-02/base/F0000019",
    "v3/OR-QA-02/base/F0000020",
    "v3/OR-QA-02/base/F0000021",
    "v3/OR-QA-02/base/F0000022",
    "v3/OR-QA-02/base/F0000023",
    "v3/OR-QA-02/base/F0000024",
    "v3/OR-QA-02/base/F0000025",
    "v3/OR-QA-02/base/F0000027",
    "v3/OR-QA-02/base/F0000028",
    "v3/OR-QA-02/base/F0000029",
    "v3/OR-QA-02/base/F0000031"
  ],
  "family_contracts": {
    "v3/OR-QA-02/base/F0000000": {
      "origin_family_id": "v3/OR-QA-02/base/F0000000",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000001": {
      "origin_family_id": "v3/OR-QA-02/base/F0000001",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000003": {
      "origin_family_id": "v3/OR-QA-02/base/F0000003",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000003"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000004": {
      "origin_family_id": "v3/OR-QA-02/base/F0000004",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000004"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000006": {
      "origin_family_id": "v3/OR-QA-02/base/F0000006",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000006"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000007": {
      "origin_family_id": "v3/OR-QA-02/base/F0000007",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000008": {
      "origin_family_id": "v3/OR-QA-02/base/F0000008",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000008"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000011": {
      "origin_family_id": "v3/OR-QA-02/base/F0000011",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000011"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000012": {
      "origin_family_id": "v3/OR-QA-02/base/F0000012",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000012"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000013": {
      "origin_family_id": "v3/OR-QA-02/base/F0000013",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000015": {
      "origin_family_id": "v3/OR-QA-02/base/F0000015",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000015"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000016": {
      "origin_family_id": "v3/OR-QA-02/base/F0000016",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000016"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000017": {
      "origin_family_id": "v3/OR-QA-02/base/F0000017",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000017"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000018": {
      "origin_family_id": "v3/OR-QA-02/base/F0000018",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000018"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000019": {
      "origin_family_id": "v3/OR-QA-02/base/F0000019",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000019"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000020": {
      "origin_family_id": "v3/OR-QA-02/base/F0000020",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000020"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000021": {
      "origin_family_id": "v3/OR-QA-02/base/F0000021",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000021"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000022": {
      "origin_family_id": "v3/OR-QA-02/base/F0000022",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000022"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000023": {
      "origin_family_id": "v3/OR-QA-02/base/F0000023",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000023"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000024": {
      "origin_family_id": "v3/OR-QA-02/base/F0000024",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000024"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000025": {
      "origin_family_id": "v3/OR-QA-02/base/F0000025",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000025"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000027": {
      "origin_family_id": "v3/OR-QA-02/base/F0000027",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000027"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000028": {
      "origin_family_id": "v3/OR-QA-02/base/F0000028",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000028"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000029": {
      "origin_family_id": "v3/OR-QA-02/base/F0000029",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000029"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-QA-02/base/F0000031": {
      "origin_family_id": "v3/OR-QA-02/base/F0000031",
      "leakage_group_ids": [
        "v3/OR-QA-02/base/F0000031"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 32,
  "sources": [
    "RAG-032",
    "RAG-002"
  ],
  "card": "domains/orchestration/OR-QA-02.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-02\\base\\V2P001",
  "mode": "CREATE_ONLY",
  "next_stream_id": "END",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\domains\\orchestration\\OR-QA-02.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-02\\base\\V2P001\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-02\\base\\V2P001\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-02\\base\\V2P001\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-02\\base\\V2P001\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-02\\base\\V2P001\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-QA-02\\base\\V2P001\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-032": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-032.md",
      "RAG-002": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-002.md"
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

## 1. Что должна усвоить модель

Выбирать проверку, в которой обновляется и затем реально используется обучаемое состояние, а сравнение не подменяется изменением других факторов.

Обучается QA-планировщик. Каждая пара описывает будущий опыт; сама карточка не содержит якобы измеренных улучшений.

Это одна предметная тема. Названия billing, документов или внешних сервисов в условиях — контекст управляющего решения, а не дополнительное обучение соответствующим предметам. Ответы не должны содержать rationale, source refs или метки выбранной стороны.



## 2. Проверенные источники и границы применения

- [RAG-032: llm mvp/orchestrator teaching plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md) — Раздельная оценка до/после и следа обучения.
- [RAG-002: Flow files generator/GUIDE-B04-QA-COVERAGE-STATE-JSON.md](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B04-QA-COVERAGE-STATE-JSON.md) — Проверяемое покрытие поведения и связь QA с требованиями.

Источники задают методику и проверяемые границы. Сценарии ниже **новые симуляции**, а не цитаты реальных запусков и не факты о текущем production. Внешние ссылки внутри исходников не расширяют разрешённые источники: используются только `xiigen-planning/main` и `xiigen-planning/llm_plan3`. Старые команды, selector IDs и требования другого стека не исполняются автоматически.



## 3. Подробный промпт для одного пакета

```text
Активная микротема: OR-QA-02; domain_id=or.qa; profile_id=base.
Используй текущую открытую китайскую модель, выбранную в Kiro.
Фактический model_id запиши в provenance; неизвестную версию не выдумывай.
Ты генерируешь кандидатов, но не принимаешь собственный материал за CODEX.
Материал уже назначен карточкой и request; текущая Kiro-модель создаёт кандидаты без независимой приёмки в этой стадии.
Прочитай request.json, общий MASTER-PROMPT-RU.md, DATA-CONTRACT и QUALITY-AND-SPLITS,
затем только источники этой карточки и разобранные reference-примеры.
Задача: Выбирать проверку, в которой обновляется и затем реально используется обучаемое состояние, а сравнение не подменяется изменением других факторов.
Формат target: JSON: comparison (string), conclusion_limit (string). Ровно одно сравнение и его предел вывода.
Сформируй ровно запрошенный count и назначенные family_id/split.
На одной строке одна локальная задача, один домен/профиль и одна главная ошибка.
Prompt задаёт все факты, от которых зависит предпочтение.
Chosen и rejected — одинаковая форма ответа; rejected близкий, правдоподобный.
Если две стратегии одинаково допустимы по условию, отправь случай в quarantine.
Оси разнообразия:
- Фактор: состав DPO, feature kNN, сохранённый adapter, длительность обучения, контроль утечки.
- Общий старт: один обученный parent checkpoint, одна tokenizer/template версия, неизменный EXAM.
- Вывод: успех конкретного показателя + все регрессии; различать локальный эффект и готовность интеграции.
Не наращивай корпус переименованием объектов. Меняй структуру ситуации,
ограничение или положение решающего факта; смысловые производные одной ситуации
сохраняют family_id и один split. Reference строки нельзя копировать в TRAIN/DEV/EXAM.
Сохрани draft.jsonl в плоском формате xiigen.dpo.v1.
provenance.kind=synthetic_from_repo; metadata.simulation=true; source_role=methodology.
Source_refs/rationale/error_tag остаются только метаданными, вне target.
Не выводи истинный label из старого keyword predicate или имени source-файла.
Проверка JSON — проверка подготовки, не доказательство качества или обучения.
Отчёт по-русски: создано/принято/карантин, причины, покрытие, роли, фактическая модель.
Если пользователь указал следующий ID, не продолжай прошлую карточку.
При stop немедленно прекрати действия. Старые опыты и результаты не удаляй.
```



## 6. Разобранные reference-пары

### 1. `baseline_confound`

**Prompt:** Симуляция. Нужно проверить новую feature kNN. Есть совместимый обученный checkpoint P с ненулевым результатом. Два плеча должны отличаться только feature. Выбери сравнение.
Формат ответа: JSON: comparison (string), conclusion_limit (string). Ровно одно сравнение и его предел вывода.

**Chosen**

```json
{
  "comparison": "Восстановить P в обоих плечах; применить одинаковые уроки, feature включить только в B; оценить один неизменный EXAM.",
  "conclusion_limit": "Вывод относится к этой feature и данному профилю."
}
```

**Rejected**

```json
{
  "comparison": "Плечо A начать с P, а B обучить с нуля на другом наборе; сравнить итоговый EXAM.",
  "conclusion_limit": "Вывод относится к этой feature и данному профилю."
}
```

**Почему различаются:** Разные старты и наборы не позволяют приписать отличие одной feature.

Семья: `simulation.qa.common_parent`; pair_id: `OR-QA-02.base.REF2.F001.P01`.

### 2. `file_count_as_learning`

**Prompt:** Симуляция. Учебный шаг обновляет adapter B, но inference по конфигурации загружает adapter A. Цель — проверить влияние обучения B. Что нужно сравнить сначала?
Формат ответа: JSON: comparison (string), conclusion_limit (string). Ровно одно сравнение и его предел вывода.

**Chosen**

```json
{
  "comparison": "Связать inference с B и подтвердить hash/identity используемого adapter до и после его реального обновления.",
  "conclusion_limit": "Пока inference читает A, влияние обучения B не измерено."
}
```

**Rejected**

```json
{
  "comparison": "Сравнить число записанных файлов B до и после шага, не меняя inference A.",
  "conclusion_limit": "Пока inference читает A, влияние обучения B не измерено."
}
```

**Почему различаются:** Обновляемое состояние должно участвовать в получении проверяемого ответа.

Семья: `simulation.qa.consumer`; pair_id: `OR-QA-02.base.REF2.F002.P01`.

### 3. `zero_zero_as_success`

**Prompt:** Симуляция. На выбранном минимальном наборе до и после четырёх очень малых шагов правильных токенов 0 из 23. Margin изменился на 0.001. Есть общий обученный parent P с ненулевым усвоением. Выбери следующее сравнение.
Формат ответа: JSON: comparison (string), conclusion_limit (string). Ровно одно сравнение и его предел вывода.

**Chosen**

```json
{
  "comparison": "Восстановить P и повторить ограниченный опыт с достаточным для урока бюджетом.",
  "conclusion_limit": "Текущее 0→0 не доказывает учебный результат."
}
```

**Rejected**

```json
{
  "comparison": "Сохранить 0→0 как успешное обучение только по сдвигу margin на 0.001.",
  "conclusion_limit": "Текущее 0→0 объявить достаточным учебным результатом."
}
```

**Почему различаются:** Здесь конструкция опыта не показала усвоения; нельзя подменить его малым внутренним сдвигом.

Семья: `simulation.qa.zero`; pair_id: `OR-QA-02.base.REF2.F003.P01`.

### 4. `accuracy_only_success`

**Prompt:** Симуляция. В корректном учебном опыте один и тот же EXAM дал точность 8/10 до и 8/10 после. Время учебного шага снизилось с 100 до 80 мс, peak RAM вырос с 200 до 210 МБ; эти числа в условии — симуляция. Как оформить сравнение?
Формат ответа: JSON: comparison (string), conclusion_limit (string). Ровно одно сравнение и его предел вывода.

**Chosen**

```json
{
  "comparison": "Зафиксировать успех времени: −20 мс; рядом точность 8/10→8/10 и RAM +10 МБ.",
  "conclusion_limit": "Кандидат полезен по времени в этом профиле; интеграция требует отдельного решения."
}
```

**Rejected**

```json
{
  "comparison": "Объявить отсутствие любого успеха, потому что точность не выросла.",
  "conclusion_limit": "Кандидат отвергнут без учёта времени и RAM."
}
```

**Почему различаются:** Улучшение полезного показателя признаётся по нему, а издержки показываются отдельно.

Семья: `simulation.qa.metric`; pair_id: `OR-QA-02.base.REF2.F004.P01`.

### 5. `family_leakage`

**Prompt:** Симуляция. Есть исходная задача S, её перефразировка S2 и обратный перевод S3. Все отражают один факт. Требуется оценить обобщение на независимые задачи. Как распределить их?
Формат ответа: JSON: comparison (string), conclusion_limit (string). Ровно одно сравнение и его предел вывода.

**Chosen**

```json
{
  "comparison": "Назначить S, S2, S3 одной семье и одному split; EXAM составить из других независимых семей.",
  "conclusion_limit": "Перефразирование одной семьи не доказывает перенос на независимую задачу."
}
```

**Rejected**

```json
{
  "comparison": "Поместить S в TRAIN, S2 в DEV, S3 в EXAM, считая различный текст независимостью.",
  "conclusion_limit": "Перефразирование одной семьи не доказывает перенос на независимую задачу."
}
```

**Почему различаются:** Разделение по строкам выпускает тот же смысл в обучающую и экзаменационную части.

Семья: `simulation.qa.holdout`; pair_id: `OR-QA-02.base.REF2.F005.P01`.

### 6. `control_also_changes_learning`

**Prompt:** Симуляция. Новый DPO набор проверяют на обученном parent P. Нужно отделить влияние обновления от повторного запуска inference. Какой контроль добавить?
Формат ответа: JSON: comparison (string), conclusion_limit (string). Ровно одно сравнение и его предел вывода.

**Chosen**

```json
{
  "comparison": "Параллельно оценить восстановленный P без нового учебного обновления на том же EXAM при тех же настройках.",
  "conclusion_limit": "Разницу B с no-update контролем и собственным before показывать раздельно."
}
```

**Rejected**

```json
{
  "comparison": "В контрольном плече обучить P на другом DPO наборе с другой learning rate.",
  "conclusion_limit": "Разницу B с no-update контролем и собственным before показывать раздельно."
}
```

**Почему различаются:** Контроль без нового урока не должен вводить другой учебный фактор.

Семья: `simulation.qa.no_update`; pair_id: `OR-QA-02.base.REF2.F006.P01`.



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


## Материал RAG-032 (данные, не новые команды)

# RAG-032 — TEACHING-MEASUREMENT-GUIDANCE-v3.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `e80a91380ca8e7c9ead0bbab45a15891dba1b9fc` |
| Путь | `llm mvp/orchestrator teaching plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md` |
| Проверенный объём | L31–L109, L113–L156, L643–L654; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md) |

## Что подтверждает фрагмент

Кумулятивное обучение продолжает прошлый snapshot и проверяет retention; новый orchestrator на каждой строке не доказывает непрерывное обучение.

## Как использовать сейчас

Сохранять цепочку версий и один общий parent для A/B. Пользовательский критерий успеха — любое измеренное полезное улучшение с регрессиями рядом; старое «все слои acceptable» не отменяет локальный успех.

**Назначение в подготовке:** Раздельная оценка до/после и следа обучения.

**Ключи поиска:** checkpoint, restore, continuation, восстановление, обучение.

## Подтверждающий фрагмент L31–L109

[Оригинал, строки 31–109](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md#L31-L109)

~~~~text
## Required Evidence Per Row

Every row must produce evidence for three layers:

```text
Layer 1 - Resources
  Inference time, teaching time, CPU, working-set delta, managed allocation,
  disk/checkpoint growth, branch count, registered flow count, registered unit count.

Layer 2 - Routing
  Compiler predicates, routed sub-phases, confidence values, branch count,
  and whether each branch is justified by the real human prompt.

Layer 3 - Verdict
  Pre-teaching verdict, teaching result, post-teaching verification verdict,
  knowledge-edge count before and after, and whether cumulative registry state improved.
```

A row succeeds only if all three layers are acceptable.

---

## Mandatory Cumulative Teaching Rule

The teaching runner must not create a fresh orchestrator per row.

Correct cycle:

```text
Start bundle
  create topology registry once
  create flow registry once
  create unit registry once
  create capability trainer once
  create adaptive orchestrator once
  save active teaching snapshot

For row 1
  load active teaching snapshot
  compile prompt
  route to sub-phases
  teach every routed branch
  register learned unit/flow into the same registries
  verify row 1 against the same registries
  save active teaching snapshot
  write row evidence

For row 2
  load active teaching snapshot from row 1
  compile prompt
  route to sub-phases
  teach every routed branch, including sub-phases already seen before
  register newer learned unit versions
  verify row 2 against cumulative registry state
  re-test row 1 against the grown registry
  save active teaching snapshot
  write row evidence

For row N
  load active teaching snapshot from row N-1
  teach only row N
  verify row N
  re-test the newest 2-3 previously accepted rows
  accept row N only if the new row passes and the regression window still passes
  save active teaching snapshot
```

This deliberately avoids an "N rows in one long push" teaching run. The machine must show progress after one row at a time, because the value being measured is whether the logical unit grows without forgetting recent accepted behavior.

Wrong cycle:

```text
For each row
  create a fresh orchestrator
  train missing flows
  retry same row
```

The wrong cycle proves only local self-repair. It does not prove continuous learning, because row 2 cannot benefit from row 1 and row 3 cannot benefit from rows 1 and 2.
~~~~

## Подтверждающий фрагмент L113–L156

[Оригинал, строки 113–156](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md#L113-L156)

~~~~text
## Already-Known Sub-Phase Rule

If row N routes to a sub-phase that already has a learned flow from an earlier row, the row must still create a new teaching event for that sub-phase.

Reason:

```text
Existing capability means the machine has a prior belief.
The new row is new evidence.
Teaching must update that belief, not skip it.
```

Correct behavior:

```text
row 1 routes to 4.1 -> create 4.1-runtime-unit v1
row 2 routes to 4.1 -> load v1 checkpoint, continue training, save v2
ResolveLatest(4.1) now selects v2 if it is Probationary or Trusted.
```

Incorrect behavior:

```text
row 2 routes to 4.1
flow already exists
skip teaching
verify with row-0001 unit
```

That is reuse, not learning.

Also incorrect:

```text
row 2 routes to 4.1
create a completely separate row-0002 unit that does not start from row-0001 weights
```

That is accumulation, not continuous learning. The accepted target is a version chain:

```text
4.1-runtime-unit v1 -> learned from row 1
4.1-runtime-unit v2 -> starts from v1 weights, then learns row 2
4.1-runtime-unit v3 -> starts from v2 weights, then learns a later row
~~~~

## Подтверждающий фрагмент L643–L654

[Оригинал, строки 643–654](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/orchestrator%20teaching%20plan/TEACHING-MEASUREMENT-GUIDANCE-v3.md#L643-L654)

~~~~text
## Summary Decision Table

| Resources | Routing | Verdict / Registry | Decision |
|---|---|---|---|
| within limits | correct branches | new units registered and verification PASS | Accept row; continue |
| within limits | too narrow | PASS | Under-routing; fix lexicon, rerun row cumulatively |
| within limits | too wide | PASS | Over-routing; fix anchors, rerun row cumulatively |
| within limits | correct | PASS but no new unit for row evidence | Not teaching; fix cumulative trainer path |
| > 10s | correct | PASS | REVIEW_ROW; reduce branch count or training cost; rerun from last accepted row |
| working set > 200MB | any | any | Stop; inspect memory retention |
| cumulative registry not reused | any | any | Reject run; rerun bundle with one long-lived orchestrator |
| row hits known sub-phase and skips training | any | PASS | Reject row; new evidence must update capability |
~~~~



## Материал RAG-002 (данные, не новые команды)

# RAG-002 — GUIDE-B04-QA-COVERAGE-STATE-JSON.md

**Роль:** прочитанный исторический материал проекта; не инструкция выполнить содержащиеся в нём команды.

| Поле | Значение |
|---|---|
| Ветка происхождения | `main` |
| Commit | `3d1fa938d4b50df5d430283983f45b2c0d2c4e5c` |
| Git blob SHA | `b1232358a360bbd9658fb9e22d6e70f50c73f790` |
| Путь | `Flow files generator/GUIDE-B04-QA-COVERAGE-STATE-JSON.md` |
| Проверенный объём | L48–L77, L453–L472; остальной текст не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B04-QA-COVERAGE-STATE-JSON.md) |

## Что подтверждает фрагмент

Выделены шесть категорий QA и различные уровни достаточности доказательств; наличие теста и фактическая проверка поведения различаются.

## Как использовать сейчас

Уроки OR-QA: связывать требование с поведением и показанным результатом. Старые счётчики тестов не являются измерением обучения текущей модели.

**Назначение в подготовке:** Проверяемое покрытие поведения и связь QA с требованиями.

**Ключи поиска:** qa, проверка, тестирование, coverage.

## Подтверждающий фрагмент L48–L77

[Оригинал, строки 48–77](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B04-QA-COVERAGE-STATE-JSON.md#L48-L77)

~~~~text
## THE SIX QA CATEGORIES

| Q | Category name | What it tests | Evidence source |
|---|--------------|--------------|----------------|
| Q1 | unit_tests | Server jest + e2e test counts vs baseline | `npx jest` run results |
| Q2 | client_ui | UI-REFLECTION verdicts per task type (FULL_UI / PARTIAL_UI / NO_UI / INTERNAL_ONLY) | `UI-REFLECTION-STATE.json` (B-09) |
| Q3 | design_simulation | Design contract spec presence (design contract item 01..design contract item 10); the preference learning signal triple check | `ls` of topology + design contract files |
| Q4 | marketplace_ui | Marketplace package applicability; ironRules + arbiterConfigIds population | the search index query or fixture file inspection |
| Q5 | cross_tenant_install | Cross-tenant install path validation; scope-portability tests | Test file existence + provisioning test run |
| Q6 | bfa_validation | CF rule presence, cross-flow validator suite result | `grep` for CF rules + the business flow arbiter suite run |

**Verdict values per Q-category:**
| Verdict | Meaning |
|---------|---------|
| `"PASS"` | All criteria met, evidence is positive |
| `"PASS_WITH_CAVEATS"` | Criteria met but with acknowledged limitations (e.g., weakened assertions) |
| `"PARTIAL_GAP"` | Some criteria met, specific gaps identified |
| `"TBD"` | Not yet evaluated — insufficient evidence to set a verdict |
| `"FAIL"` | Critical criteria not met |

**overallReadiness values:**
| Value | Meaning |
|-------|---------|
| `"READY"` | All Q1-Q6 PASS or PASS_WITH_CAVEATS, no blockers |
| `"READY_WITH_GAPS"` | Some PARTIAL_GAP verdicts, no blocking items |
| `"BLOCKED"` | At least one FAIL or a BLOCKING severity item in blockerSummary |

---

## STEP-BY-STEP AUTHORING INSTRUCTIONS FOR CLAUDE CODE
~~~~

## Подтверждающий фрагмент L453–L472

[Оригинал, строки 453–472](https://github.com/lubochka/xiigen-planning/blob/3d1fa938d4b50df5d430283983f45b2c0d2c4e5c/Flow%20files%20generator/GUIDE-B04-QA-COVERAGE-STATE-JSON.md#L453-L472)

~~~~text
## TWO OBSERVED EXAMPLES

**Example A — the forty-seventh flow (partial implementation, READY_WITH_GAPS):**
- Q1: PASS_WITH_CAVEATS (86 tests pass but assertions weakened then partially restored)
- Q2: PARTIAL_GAP (a task type uses stub setTimeout instead of real the programming interface)
- Q3: TBD (no design contract item 01..design contract item 10 spec, the preference learning signal triple check skipped per KNOWN CONSTRAINTS)
- Q4: PARTIAL_GAP (ironRules + arbiterConfigIds zero-populated on all 30 packages)
- Q5: PARTIAL_GAP (a task type stub + cross-tenant install not validated live)
- Q6: PASS (7 CF rules present: an iron rule..an iron rule)
- overallReadiness: READY_WITH_GAPS
- blockerSummary: 6 items (Rule 16 violation, a task type stub, weakened assertions, missing topology, ironRules/arbiterConfigIds empty)

**Example B — the booking flow (early evaluation, TBD verdicts):**
- Q1: TBD (test files inventoried, no current count confirmed)
- Q2: TBD (5 React pages exist, but UI-REFLECTION inventory incomplete)
- Q3: TBD (topology present but QA spec and design-contract spec not confirmed)
- Q4: TBD (applicability noted but not evaluated)
- Q5: TBD (scope_isolation arbiter confirmed present but install not tested)
- Q6: PASS (implied from Phase A the business flow arbiter rules authored)
- Note: the booking flow uses an extended schema variant with `scaffold_phase` and `verdict_legend` fields
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\OR-QA-02\base\V2P001\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\OR-QA-02\base\V2P001\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\OR-QA-02\base\V2P001\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `END`. Не запускать review, accept, export, oracle или обучение.
