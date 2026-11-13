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
  "stream_id": "OR-ARCH-DECOMPOSE/base",
  "topic_id": "OR-ARCH-01",
  "objective_id": "OR-ARCH-DECOMPOSE",
  "domain_id": "or.architecture",
  "profile_id": "base",
  "run_id": "O401",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/OR-ARCH-DECOMPOSE/base/F0000000",
    "v3/OR-ARCH-DECOMPOSE/base/F0000001",
    "v3/OR-ARCH-DECOMPOSE/base/F0000002",
    "v3/OR-ARCH-DECOMPOSE/base/F0000004",
    "v3/OR-ARCH-DECOMPOSE/base/F0000005",
    "v3/OR-ARCH-DECOMPOSE/base/F0000006",
    "v3/OR-ARCH-DECOMPOSE/base/F0000007",
    "v3/OR-ARCH-DECOMPOSE/base/F0000009",
    "v3/OR-ARCH-DECOMPOSE/base/F0000013",
    "v3/OR-ARCH-DECOMPOSE/base/F0000014"
  ],
  "family_contracts": {
    "v3/OR-ARCH-DECOMPOSE/base/F0000000": {
      "origin_family_id": "v3/OR-ARCH-DECOMPOSE/base/F0000000",
      "leakage_group_ids": [
        "v3/OR-ARCH-DECOMPOSE/base/F0000000"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ARCH-DECOMPOSE/base/F0000001": {
      "origin_family_id": "v3/OR-ARCH-DECOMPOSE/base/F0000001",
      "leakage_group_ids": [
        "v3/OR-ARCH-DECOMPOSE/base/F0000001"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ARCH-DECOMPOSE/base/F0000002": {
      "origin_family_id": "v3/OR-ARCH-DECOMPOSE/base/F0000002",
      "leakage_group_ids": [
        "v3/OR-ARCH-DECOMPOSE/base/F0000002"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ARCH-DECOMPOSE/base/F0000004": {
      "origin_family_id": "v3/OR-ARCH-DECOMPOSE/base/F0000004",
      "leakage_group_ids": [
        "v3/OR-ARCH-DECOMPOSE/base/F0000004"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ARCH-DECOMPOSE/base/F0000005": {
      "origin_family_id": "v3/OR-ARCH-DECOMPOSE/base/F0000005",
      "leakage_group_ids": [
        "v3/OR-ARCH-DECOMPOSE/base/F0000005"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ARCH-DECOMPOSE/base/F0000006": {
      "origin_family_id": "v3/OR-ARCH-DECOMPOSE/base/F0000006",
      "leakage_group_ids": [
        "v3/OR-ARCH-DECOMPOSE/base/F0000006"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ARCH-DECOMPOSE/base/F0000007": {
      "origin_family_id": "v3/OR-ARCH-DECOMPOSE/base/F0000007",
      "leakage_group_ids": [
        "v3/OR-ARCH-DECOMPOSE/base/F0000007"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ARCH-DECOMPOSE/base/F0000009": {
      "origin_family_id": "v3/OR-ARCH-DECOMPOSE/base/F0000009",
      "leakage_group_ids": [
        "v3/OR-ARCH-DECOMPOSE/base/F0000009"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ARCH-DECOMPOSE/base/F0000013": {
      "origin_family_id": "v3/OR-ARCH-DECOMPOSE/base/F0000013",
      "leakage_group_ids": [
        "v3/OR-ARCH-DECOMPOSE/base/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/OR-ARCH-DECOMPOSE/base/F0000014": {
      "origin_family_id": "v3/OR-ARCH-DECOMPOSE/base/F0000014",
      "leakage_group_ids": [
        "v3/OR-ARCH-DECOMPOSE/base/F0000014"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 15,
  "sources": [
    "RAG-OR-004"
  ],
  "card": "curriculum/cards/OR-ARCH-DECOMPOSE.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-ARCH-DECOMPOSE\\base\\O401",
  "mode": "CREATE_ONLY",
  "next_stream_id": "OR-QA-REVIEWER-COVERAGE/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\OR-ARCH-DECOMPOSE.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-ARCH-DECOMPOSE\\base\\O401\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-ARCH-DECOMPOSE\\base\\O401\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-ARCH-DECOMPOSE\\base\\O401\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-ARCH-DECOMPOSE\\base\\O401\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-ARCH-DECOMPOSE\\base\\O401\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\OR-ARCH-DECOMPOSE\\base\\O401\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-OR-004": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-OR-004.md"
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
  "objective_id": "OR-ARCH-DECOMPOSE",
  "topic_id": "OR-ARCH-01",
  "title_ru": "Полезная декомпозиция одного узла с сохранением обязательств",
  "learning_goal_ru": "Выбирать keep либо принятие конкретного split по полноте, непересечению обязанностей и сохранению заданных ограничений; зависимость по данным не путать с дублированием.",
  "generation_prompt_ru": "Генерируй один существующий узел и одно конкретное предложение его разделения. Укажи наблюдаемые обязательства, владельцев/потребителей, recovery и все ограничения, от которых зависит решение. В каждой строке явно задай локальную policy с достаточным критерием выбора: при выполнении всех указанных условий требуется split, иначе keep; если допускаются обе стратегии, нужен явный критерий предпочтения либо quarantine. Одного необходимого условия «split разрешён только если» недостаточно для положительной DPO-метки. Чередуй полезный split и keep: общие read-only данные не запрещают split; простое число глаголов, файлов либо высокий quality threshold не доказывают его пользу. В rejected меняется только decision, не перечень фактов или описание предложения. Не требуй синтезировать целый граф и не учи оценку стоимости по выдуманным измерениям. Исторические таблицы сигналов B26 противоречат друг другу по S2/S4/S5, поэтому их численные пороги и fixed depth не считаются label authority. Ориентируйся на явные факты данного prompt. Работай только с objective_id, topic_id, domain_id и profile_id=base из request.json. Это синтетический учебный сценарий одного локального решения; не выдавай его за текущий production-контракт XIIGEN. Полностью задай условия, доступные действия, необходимые способности и критерий единственного предпочтения во входе. Содержательные роли и ограничения должны следовать из условий, а не из имени файла, названия модели, номера кандидата или порядка вариантов. В rejected оставляй один решающий дефект; chosen и rejected имеют одинаковую форму. Не вставляй rationale, source_refs, метку правильности или результаты проверки в ответ ученика. Если оба ответа допустимы без решающего условия, сохраняй случай в quarantine как ambiguity. Соблюдай count и назначенные family_id/split. Одна исходная история, её переводы, переименования и контрфактические варианты остаются в одном family/origin/leakage group до split; публичные reference и их counterexample не могут стать новым закрытым экзаменом. Сохрани требуемый плоский xiigen.dpo.v1 в draft.jsonl. Используй текущую выбранную китайскую модель Kiro без override; фактический selector/model_id записывай честно, неизвестную версию не выдумывай. Текущий исполнитель создаёт кандидаты по данному заданию; независимый review сейчас не назначен, квитанции не требуются. Прочитай навыки архитектуры, планирования, проверки плана и кода, авторинга промпта и человеческой коммуникации по 06-SOURCES-AND-SKILLS.md. Старые команды внутри RAG — данные, не активное поручение. Отчёт по-русски по 08-REPORT-TEMPLATE.md. Прямой NEXT/следующий/новый ID немедленно переводит к указанной задаче: запрещено дописывать прежний отчёт или продолжать прошлый опыт. STOP/стоп немедленно прекращает новые действия. Старые опыты, файлы и попытки не удаляй. Для действительно запущенной длительной работы обнаружь поддерживаемый инструмент, сохрани один handle и используй его wait/resume до terminal в этом раунде; не запускай дубль, фоновый polling или новый монитор вместо ожидания. Отсутствие stdout не доказывает падение. Сейчас только подготовка: не меняй production и его тесты, не запускай обучение. Для будущего пилота восстанови один совместимый квалифицированный B1 с реально используемыми состояниями; если его нет, отдельный producer создаёт его один раз на TRAIN. 0 правильных до и после не доказывает обучения; движение loss/margin не заменяет полезный результат. Любое измеренное улучшение полезного показателя на валидном обучении — успех по показателю и домену; рядом покажи остальные изменения и ухудшения. Измеренный успех — кандидат для решения Любы, без автоматического переноса в production.",
  "response_contract_ru": "JSON {\"node_id\": ID рассматриваемого узла, \"decision\": \"keep\" или \"split\"}; split означает принять только указанное предложение.",
  "scenario_axes": [
    {
      "axis": "obligations",
      "values": [
        "independent_outputs",
        "one_obligation_multiple_operations",
        "duplicate_responsibility",
        "required_output_lost"
      ]
    },
    {
      "axis": "coupling",
      "values": [
        "read_only_shared_input",
        "data_dependency",
        "atomic_result",
        "independent_recovery"
      ]
    },
    {
      "axis": "contract",
      "values": [
        "independent_preview",
        "distinct_consumers",
        "single_formula",
        "full_coverage"
      ]
    }
  ],
  "forbidden_mixing": [
    "Не обучать полный graph synthesis, выбор runtime API, арифметику формулы либо содержание отчёта.",
    "Не смешивать оценку разбиения с допуском нескольких production-коммитов; это синтетическое локальное решение."
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "OR-ARCH-DECOMPOSE-REF-01",
      "profile_id": "base",
      "family_id": "v4-ref.or-arch-decompose.f01",
      "prompt": "Симуляция архитектурного планирования. Узел N1 делает два пользовательских результата: сохраняет отчёт и отдельно рассылает уведомление. Политика: если предложенное разбиение покрывает все обязательства без перекрытия и сохраняет ограничения, требуется split; иначе требуется keep. Частичный успех разрешён, уведомление можно повторить без пересоздания отчёта. Предложены дочерние узлы Report (только сохранение отчёта) и Notice (только доставка уведомления); оба используют один общий read-only расчёт. keep оставляет исходный узел; split принимает это предложение. JSON node_id=N1, decision=keep или split.",
      "chosen": "{\"node_id\":\"N1\",\"decision\":\"split\"}",
      "rejected": "{\"node_id\":\"N1\",\"decision\":\"keep\"}",
      "error_tag": "shared_read_only_input_as_shared_responsibility",
      "rationale": "Два независимо восстанавливаемых обязательства разделены; общий вход сам по себе не означает дублирование ответственности.",
      "counterexample": "Если заказчик требует единый атомарный эффект, а предложенное разбиение его не сохраняет, тот же split недопустим.",
      "source_refs": [
        "RAG-OR-004"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-ARCH-DECOMPOSE-REF-02",
      "profile_id": "base",
      "family_id": "v4-ref.or-arch-decompose.f01",
      "prompt": "Симуляция архитектурного планирования. Узел N2 сохраняет отчёт и подтверждает доставку уведомления. Политика: принять предложенное разбиение только при полном покрытии без перекрытия и сохранении ограничений. По контракту пользователя оба результата публикуются как один атомарный эффект: ни отчёт, ни уведомление не должны стать видимы отдельно. Предложенные Report и Notice публикуют каждый свой результат сразу, общей транзакции или компенсации нет. keep сохраняет существующий атомарный узел; split принимает именно это предложение. JSON node_id=N2, decision=keep или split.",
      "chosen": "{\"node_id\":\"N2\",\"decision\":\"keep\"}",
      "rejected": "{\"node_id\":\"N2\",\"decision\":\"split\"}",
      "error_tag": "decomposition_breaks_atomic_obligation",
      "rationale": "Сами обязанности различаются, но указанное предложение нарушает единый атомарный эффект. Отвергается этот split, а не возможность любой иной архитектуры.",
      "counterexample": "Если контракт разрешит независимые результаты и отдельный retry уведомления, тот же разрез может стать полезным; это семья f01.",
      "source_refs": [
        "RAG-OR-004"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-ARCH-DECOMPOSE-REF-03",
      "profile_id": "base",
      "family_id": "v4-ref.or-arch-decompose.f03",
      "prompt": "Симуляция. Узел N3 выдаёт одно заключение о допустимости формы. Рассматриваемый split создаёт CheckForm и ValidateFields. По описанию оба проверяют все те же поля по тем же правилам и независимо выдают итоговое заключение. Отдельного пользовательского результата, владельца ошибки или нового ограничения у второго узла нет. Политика принимает split только при непересекающихся обязательствах; иначе keep. JSON node_id=N3, decision=keep или split.",
      "chosen": "{\"node_id\":\"N3\",\"decision\":\"keep\"}",
      "rejected": "{\"node_id\":\"N3\",\"decision\":\"split\"}",
      "error_tag": "renamed_duplicate_responsibility",
      "rationale": "Разные имена не создают разных обязанностей: предложено дважды принять одно заключение.",
      "counterexample": "Если CheckForm проверяет только структуру, а второй узел проверяет бизнес-ограничение с отдельным контрактом и без повторной структурной проверки, покрытие может стать непересекающимся.",
      "source_refs": [
        "RAG-OR-004"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-ARCH-DECOMPOSE-REF-04",
      "profile_id": "base",
      "family_id": "v4-ref.or-arch-decompose.f04",
      "prompt": "Симуляция. Узел N4 выполняет импорт документа. Его два проверяемых обязательства: Parser превращает байты в нормализованные записи; Indexer строит индекс только из уже нормализованных записей. Ошибка формата устраняется повтором Parser, ошибка индекса — повтором Indexer из сохранённых записей. Предложенный split сохраняет границу данных и эту зависимость, не повторяет parsing в Indexer. Политика требует split для непересекающихся контрактов с отдельным восстановлением; зависимость по данным разрешена. JSON node_id=N4, decision=keep или split.",
      "chosen": "{\"node_id\":\"N4\",\"decision\":\"split\"}",
      "rejected": "{\"node_id\":\"N4\",\"decision\":\"keep\"}",
      "error_tag": "data_dependency_as_responsibility_overlap",
      "rationale": "Последовательная зависимость не делает контракты одинаковыми; каждое восстановление остаётся у своего шага.",
      "counterexample": "Если Indexer повторно выполняет весь Parser и у него нет отдельного обязательства, основание для этого split исчезает.",
      "source_refs": [
        "RAG-OR-004"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-ARCH-DECOMPOSE-REF-05",
      "profile_id": "base",
      "family_id": "v4-ref.or-arch-decompose.f05",
      "prompt": "Симуляция. N5 переводит одну температуру из градусов Цельсия в Фаренгейты и возвращает единственный результат. Порог требуемой точности 0.999; все ошибки относятся к одной формуле. Предлагается split на Multiply (внутреннее умножение) и Add (внутреннее сложение), без новых контрактов, отдельного повторного использования или восстановления. Правило проекта: если предложенное разбиение лишь выделяет арифметические подоперации одной формулы без самостоятельных контрактов, требуется keep; split требуется только при наличии самостоятельных контрактов и сохранении всех ограничений. Высокий порог качества сам по себе не создаёт самостоятельного контракта. JSON node_id=N5, decision=keep или split.",
      "chosen": "{\"node_id\":\"N5\",\"decision\":\"keep\"}",
      "rejected": "{\"node_id\":\"N5\",\"decision\":\"split\"}",
      "error_tag": "quality_threshold_as_decomposition_trigger",
      "rationale": "Порог относится к качеству одного результата; он не доказывает пользу двух новых границ.",
      "counterexample": "Если две части станут независимо вызываемыми контрактами с отдельными владельцами и требованиями, это будет другая архитектурная ситуация той же семьи.",
      "source_refs": [
        "RAG-OR-004"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-ARCH-DECOMPOSE-REF-06",
      "profile_id": "base",
      "family_id": "v4-ref.or-arch-decompose.f06",
      "prompt": "Симуляция. N6 рассчитывает срок хранения, а затем удаляет истёкшие записи. Расчёт срока должен быть доступен отдельно в режиме preview без удаления; правила сроков меняются независимо от механизма удаления. Предложенный split: Policy возвращает границу срока, Cleanup принимает эту границу и удаляет только подходящие записи. Policy не удаляет; Cleanup не пересчитывает срок. Политика требует split, когда разные независимо используемые обязательства имеют непересекающиеся контракты и сохранённые ограничения. JSON node_id=N6, decision=keep или split.",
      "chosen": "{\"node_id\":\"N6\",\"decision\":\"split\"}",
      "rejected": "{\"node_id\":\"N6\",\"decision\":\"keep\"}",
      "error_tag": "independent_preview_contract_hidden_in_combined_node",
      "rationale": "Независимо вызываемый preview — явная отдельная обязанность; предложенный разрез не дублирует расчёт.",
      "counterexample": "Если preview не требуется, а предложенные части не имеют отдельных контрактов или восстановления, нельзя объявлять split обязательным только из-за двух глаголов.",
      "source_refs": [
        "RAG-OR-004"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-ARCH-DECOMPOSE-REF-07",
      "profile_id": "base",
      "family_id": "v4-ref.or-arch-decompose.f07",
      "prompt": "Симуляция. N7 принимает заказ: проверяет допустимость, резервирует ресурс и сохраняет обязательный журнал результата. Предложен split Validate + Reserve. Validate проверяет допустимость; Reserve резервирует ресурс. Ни один дочерний узел и ни общий wrapper не сохраняют журнал. Политика принимает конкретный split только при полном покрытии обязанностей и сохранённых ограничениях; keep оставляет прежний узел, который пишет журнал. JSON node_id=N7, decision=keep или split.",
      "chosen": "{\"node_id\":\"N7\",\"decision\":\"keep\"}",
      "rejected": "{\"node_id\":\"N7\",\"decision\":\"split\"}",
      "error_tag": "decomposition_drops_required_output",
      "rationale": "Непересечение двух частей недостаточно: обязательное журналирование потеряно.",
      "counterexample": "Если журнал получает отдельного явного владельца или остаётся в wrapper без дублирования, предложение нужно проверить заново, а не отвергать всякую декомпозицию.",
      "source_refs": [
        "RAG-OR-004"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
      }
    },
    {
      "example_id": "OR-ARCH-DECOMPOSE-REF-08",
      "profile_id": "base",
      "family_id": "v4-ref.or-arch-decompose.f08",
      "prompt": "Симуляция. N8 анализирует один текст для двух независимых клиентов: редактору возвращает список терминов, службе доступа — признак наличия персональных данных. Каждый клиент вызывает свой контракт отдельно; ошибки и версии критериев различны. Предложены Terms и PersonalData, каждый выдаёт только свой результат и читает один исходный текст. Политика принимает split для таких независимых обязанностей, если общая обработка read-only и нет атомарного общего результата. JSON node_id=N8, decision=keep или split.",
      "chosen": "{\"node_id\":\"N8\",\"decision\":\"split\"}",
      "rejected": "{\"node_id\":\"N8\",\"decision\":\"keep\"}",
      "error_tag": "shared_source_document_as_single_user_contract",
      "rationale": "Один документ может обслуживать разные независимые пользовательские контракты; здесь разбиение сохраняет их.",
      "counterexample": "Если оба ответа лишь два названия одного и того же вывода для того же клиента, одного общего входа мало для вывода о двух обязанностях.",
      "source_refs": [
        "RAG-OR-004"
      ],
      "metadata": {
        "response_language": "ru",
        "target_format": "json",
        "split": "reference",
        "provenance_kind": "synthetic_from_repo",
        "status": "REFERENCE_ONLY_NOT_GENERATED_NOT_TRAINED",
        "observed_learning_result": false,
        "counterexample_is_training_row": false,
        "reference_only": true,
        "training_allowed": false,
        "review_status": "semantic_review_required",
        "source_role": "methodology_not_runtime_contract",
        "simulation": true
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


## Материал RAG-OR-004 (данные, не новые команды)

# RAG-OR-004 — GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md

**Роль:** прочитанный исторический материал проекта; команды внутри него не являются активными инструкциями.

| Поле | Значение |
|---|---|
| Ветка | `main` |
| Commit | `f16e68bc662bc629bc45bd0e6a46011c12865199` |
| Git blob SHA | `f2d5f72f4275cb8323421f642232f032235682c4` |
| Путь | `Flow files generator/GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md` |
| Источник | [Закреплённый файл](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md) |
| Проверенные фрагменты | L66–L107, L157–L188, L424–L444 |

## Что прочитано

В источнике требуется полезность расширения, непересечение обязанностей и проверяемая причина. Есть несовпадающие определения S2/S4/S5 в двух таблицах.

## Границы использования

Источник методический, не gold labels: не переносить fixed depth=3/5, signal thresholds, score=0.85 или правило по слову and. Историческая фраза об удалении зависимого узла неоднозначна; в новых симуляциях явно различены зависимость данных и дублирование ответственности.

## Подтверждающий фрагмент L66–L107

[Оригинал, строки 66–107](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md#L66-L107)

~~~~text
## THE 5 COMPLEXITY SIGNALS — UNIVERSAL

The same 5 signals apply to all flows. Each references a specific NODE field so the
evaluation is observable, not subjective:

| Signal | NODE field | Triggers EXPAND when |
|--------|-----------|---------------------|
| S1: Multi-purpose intent | `node.intent.purpose` | Contains 2+ independent outcomes connected by "and", "then", or "also" |
| S2: Multiple independent input types | `node.structure.inputShape` | More than 3 distinct input types where no two share a schema |
| S3: Independent failure modes | `node.quality.scoringCriteria` | More than 2 failure modes with divergent handling mechanisms |
| S4: Cross-domain constraints | `node.constraints` | Constraints span more than 2 domain areas (security + data + scheduling + external + business + tenant) |
| S5: High threshold + broad scope | `node.quality.acceptanceThreshold` + `node.intent.purpose` | Threshold > 0.90 AND purpose covers more than one user-visible outcome (weak — never sufficient alone) |

**Rules for signal evaluation:**
- All 5 signals must be evaluated for every non-terminal NODE (LEAF without evaluating
  all 5 signals is rejected — Step 7 Check 2 specifically enforces this)
- At termination bound: no signal evaluation — verdict is always LEAF
- A single signal triggers a possibility for EXPAND, not a mandate. The Depth Decider
  must cite the signal evidence AND conclude that expansion adds structural value.
- S5 alone is never sufficient — it must combine with at least one other signal

---

## THE TERMINATION BOUND — MACHINE CONSTRAINT

```
default_depth:  3
max_depth:      5
```

The termination bound is a MACHINE constraint — not configurable per tenant or per
flow. At depth = termination_bound, the verdict is always LEAF. No signal evaluation
runs. Justification format: "depth = [N] = termination_depth — bound enforced."

**Why MACHINE?** If one tenant's flow expands beyond the termination bound, it creates
a recursive decomposition that blocks all other tenants' execution on the same engine.
The bound prevents infinite recursion across the entire platform.

**Choosing termination_depth:**
Most flows use the default of 3. Use a higher value (4 or 5) only when the flow spec
explicitly contains capabilities that require 4+ levels of decomposition. Document
the reasoning in state (see the user registration flow's reasoning example above).
~~~~

## Подтверждающий фрагмент L157–L188

[Оригинал, строки 157–188](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md#L157-L188)

~~~~text
CONSTRAINTS:
  - MACHINE CONSTRAINT: if current_depth >= termination_depth -> LEAF verdict, no AI call
  - the content-type rendering pattern through the ninth architecture DNA pattern still apply to any sub-nodes if EXPAND
  - Sub-flows from EXPAND must not overlap in responsibility
  - Justification required for every verdict (empty = grade 0)

PRIOR_ART:
  - [Reference to which prior flow established the LEAF/EXPAND protocol]
  - Termination bound = [N] validated across [prior flows]

SUCCESS:
  verdict_accepted_when:
    - LEAF: single responsibility confirmed, justification present, depth <= termination_bound
    - EXPAND: 2+ complexity signals triggered, sub-flow decomposition non-overlapping
    - Grade >= 0.85 (justification_present x non_overlap)
```
```

### Step 4 — Write the 5 COMPLEXITY SIGNALS table

```markdown
---

## 5 COMPLEXITY SIGNALS (a planning skill Section 2)

| Signal | NODE Field | Triggers EXPAND when |
|--------|-----------|---------------------|
| S1: Multi-purpose intent | intent.purpose | Contains 2+ independent outcomes |
| S2: Compound input shape | structure.inputShape | Requires data from 2+ independent sources |
| S3: Independent failure modes | quality.scoringCriteria | Contains more than 2 independent failure modes with divergent handling |
| S4: Multiple quality criteria | quality.testCriteria | 4+ unrelated test criteria |
| S5: Sub-flow naming | intent.outcome | Outcome names a process that is itself a flow |
~~~~

## Подтверждающий фрагмент L424–L444

[Оригинал, строки 424–444](https://github.com/lubochka/xiigen-planning/blob/f16e68bc662bc629bc45bd0e6a46011c12865199/Flow%20files%20generator/GUIDE-B26-STEP-6-CYCLE3-CONTEXT.md#L424-L444)

~~~~text
**QUESTION YOURSELF** *(Depth Decider AI: verify before returning verdict)*

```
Q1 — TERMINATION BOUND FIRST: Is current_depth = termination_depth?
     YES → verdict is LEAF, state bound enforced, skip Q2-Q4.
     NO → proceed to Q2.

Q2 — SIGNAL EVIDENCE: Have I evaluated all 5 signals against specific NODE fields?
     S1: intent.purpose — "and"/"then" connecting distinct actions?
     S2: structure.inputShape — more than 3 distinct input types?
     S3: quality.scoringCriteria — more than 2 independent failure modes?
     S4: constraints — span more than 2 domain areas?
     S5: acceptanceThreshold > 0.90 AND multiple outcomes in purpose?
     LEAF without evaluating all 5 → justification rejected.

Q3 — JUSTIFICATION TRACEABLE: Can the cited NODE field value be verified?
     If justification references a field value not in the actual NODE → revise.

Q4 — EXPAND COHERENCE (if EXPAND): Does sub_flow_decomposition have ≥2 entries
     where removing one doesn't break the other's coverage?
     If not → they share responsibility → merge → re-evaluate whether EXPAND is valid.
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\OR-ARCH-DECOMPOSE\base\O401\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\OR-ARCH-DECOMPOSE\base\O401\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\OR-ARCH-DECOMPOSE\base\O401\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `OR-QA-REVIEWER-COVERAGE/base`. Не запускать review, accept, export, oracle или обучение.
