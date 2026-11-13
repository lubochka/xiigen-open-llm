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
  "stream_id": "CODE-CS-NULL/base",
  "topic_id": "CODE-CS-01",
  "objective_id": "CODE-CS-NULL",
  "domain_id": "code.csharp",
  "profile_id": "base",
  "run_id": "M003B002",
  "split": "train",
  "count": 10,
  "family_ids": [
    "v3/CODE-CS-NULL/base/F0000012",
    "v3/CODE-CS-NULL/base/F0000013",
    "v3/CODE-CS-NULL/base/F0000016",
    "v3/CODE-CS-NULL/base/F0000017",
    "v3/CODE-CS-NULL/base/F0000018",
    "v3/CODE-CS-NULL/base/F0000019",
    "v3/CODE-CS-NULL/base/F0000020",
    "v3/CODE-CS-NULL/base/F0000021",
    "v3/CODE-CS-NULL/base/F0000022",
    "v3/CODE-CS-NULL/base/F0000023"
  ],
  "family_contracts": {
    "v3/CODE-CS-NULL/base/F0000012": {
      "origin_family_id": "v3/CODE-CS-NULL/base/F0000012",
      "leakage_group_ids": [
        "v3/CODE-CS-NULL/base/F0000012"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-CS-NULL/base/F0000013": {
      "origin_family_id": "v3/CODE-CS-NULL/base/F0000013",
      "leakage_group_ids": [
        "v3/CODE-CS-NULL/base/F0000013"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-CS-NULL/base/F0000016": {
      "origin_family_id": "v3/CODE-CS-NULL/base/F0000016",
      "leakage_group_ids": [
        "v3/CODE-CS-NULL/base/F0000016"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-CS-NULL/base/F0000017": {
      "origin_family_id": "v3/CODE-CS-NULL/base/F0000017",
      "leakage_group_ids": [
        "v3/CODE-CS-NULL/base/F0000017"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-CS-NULL/base/F0000018": {
      "origin_family_id": "v3/CODE-CS-NULL/base/F0000018",
      "leakage_group_ids": [
        "v3/CODE-CS-NULL/base/F0000018"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-CS-NULL/base/F0000019": {
      "origin_family_id": "v3/CODE-CS-NULL/base/F0000019",
      "leakage_group_ids": [
        "v3/CODE-CS-NULL/base/F0000019"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-CS-NULL/base/F0000020": {
      "origin_family_id": "v3/CODE-CS-NULL/base/F0000020",
      "leakage_group_ids": [
        "v3/CODE-CS-NULL/base/F0000020"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-CS-NULL/base/F0000021": {
      "origin_family_id": "v3/CODE-CS-NULL/base/F0000021",
      "leakage_group_ids": [
        "v3/CODE-CS-NULL/base/F0000021"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-CS-NULL/base/F0000022": {
      "origin_family_id": "v3/CODE-CS-NULL/base/F0000022",
      "leakage_group_ids": [
        "v3/CODE-CS-NULL/base/F0000022"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    },
    "v3/CODE-CS-NULL/base/F0000023": {
      "origin_family_id": "v3/CODE-CS-NULL/base/F0000023",
      "leakage_group_ids": [
        "v3/CODE-CS-NULL/base/F0000023"
      ],
      "scenario_rule": "New independent scenario, not a renamed public/prior example."
    }
  },
  "pairs_per_family": 1,
  "next_start": 24,
  "sources": [
    "RAG-027",
    "RAG-040"
  ],
  "card": "curriculum/cards/CODE-CS-NULL.md",
  "output_directory": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-CS-NULL\\base\\M003B002",
  "mode": "CREATE_ONLY",
  "next_stream_id": "OR-ROUTE-INTENT/base",
  "completion_status": "DRAFT_CREATED",
  "review_status": "NOT_REQUESTED",
  "paths": {
    "preparation_root": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation",
    "learning_sets_root": "C:\\Projects\\llm_mvp_core\\learning-sets",
    "card": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\curriculum\\cards\\CODE-CS-NULL.md",
    "contract": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\02-DATA-CONTRACT.md",
    "schema": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\schemas\\dpo-candidate.schema.json",
    "request": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-CS-NULL\\base\\M003B002\\request.json",
    "prompt": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-CS-NULL\\base\\M003B002\\prompt.md",
    "draft": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-CS-NULL\\base\\M003B002\\draft.jsonl",
    "state": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-CS-NULL\\base\\M003B002\\state.json",
    "commands": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-CS-NULL\\base\\M003B002\\commands.md",
    "resume": "C:\\Projects\\llm_mvp_core\\learning-sets\\CODE-CS-NULL\\base\\M003B002\\RESUME.md",
    "tools_script": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\tools\\batches.py",
    "python_executable": "C:\\Python314\\python.exe",
    "sources": {
      "RAG-027": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-027.md",
      "RAG-040": "C:\\Users\\lubao\\Documents\\xiigen\\llm mvp\\learning-sets\\preparation\\rag\\sources\\RAG-040.md"
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
  "objective_id": "CODE-CS-NULL",
  "topic_id": "CODE-CS-01",
  "title_ru": "Реализовать подстановку только вместо null",
  "learning_goal_ru": "По явному контракту реализовать null-only fallback, сохранив пустую строку, пробелы и точные символы.",
  "generation_prompt_ru": "Генерируй только одно полное объявление метода C# 10/.NET 6 для null-only fallback. Контракт обязан назвать сигнатуру, fallback и примеры null/empty/whitespace. Используй допустимую реализацию ?? или эквивалентную проверку is null. Rejected компилируется и нарушает одну границу empty/whitespace/trim. Не делай отрицательным примером просто другой синтаксис корректного кода. Все сравнения oracle — ordinal, без Trim и нормализации.\n\nРаботай только с данным objective_id и назначенными family_id/split/count из request.json. Пиши одну смысловую пару на назначенное семейство; chosen и rejected должны иметь одинаковый формат и отличаться одной проверяемой ошибкой. Все сведения, версии, исходные данные и договор ответа помещай в prompt: metadata learner не получает. Примеры ниже — открытые авторские reference seeds, не закрытый EXAM, не сгенерированный китайской моделью корпус и не измеренные результаты. Не копируй reference-строки в TRAIN под новым ID. Переименование переменных/таблиц и замена чисел не делают шаблон новым независимым семейством. Для документации объясняй именно показанный код, для генерации возвращай только запрошенный код. Не вставляй в ответы labels, rationale, source ID и предпочтительную позицию. Oracle expected — заранее назначенное ожидание; статус остаётся NOT_RUN, пока соответствующий runtime действительно не выполнен. Runtime oracle сейчас не запускай; сохрани его ожидания как NOT_RUN, не выдавай их за наблюдённый результат. Не переходи к другой теме внутри пакета. При NEXT немедленно используй следующий ID из реестра, при STOP прекрати новые действия. Production и старые эксперименты не менять; отчёт на русском по 08-REPORT-TEMPLATE.md, фактическая текущая Kiro-модель без override.",
  "response_contract_ru": "Только полное объявление указанного метода без Markdown, комментариев, тестов или объяснения.",
  "scenario_axes": [
    {
      "axis": "fallback",
      "values": [
        "фиксированный литерал",
        "не-null параметр",
        "пустая строка"
      ]
    },
    {
      "axis": "null-only syntax",
      "values": [
        "??",
        "if is null",
        "тернарное выражение"
      ]
    },
    {
      "axis": "контроль сохранности",
      "values": [
        "пустая строка",
        "пробел",
        "краевые пробелы"
      ]
    }
  ],
  "forbidden_mixing": [
    "валидация бизнес-правил",
    "обработка исключений",
    "async",
    "trim как полезное улучшение"
  ],
  "pairs_per_family": 1,
  "examples": [
    {
      "example_id": "CODE-CS-NULL.REF.01",
      "profile_id": "base",
      "family_id": "ref.cs.null-fallback.literal.01",
      "prompt": "Авторская симуляция. C# 10 / .NET 6, nullable enabled. Напиши внутри класса метод string Label(string? s). Только при null вернуть \"(нет)\"; при \"\", \" \" и любом другом значении вернуть точное s без изменений. Ответ — только полное объявление метода.",
      "chosen": "string Label(string? s) => s ?? \"(нет)\";",
      "rejected": "string Label(string? s) => string.IsNullOrEmpty(s) ? \"(нет)\" : s;",
      "error_tag": "empty_treated_as_null",
      "rationale": "Rejected компилируется, но подменяет пустую строку fallback; chosen следует null-only контракту.",
      "counterexample": "s=\"\": chosen возвращает \"\", rejected — \"(нет)\".",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6; nullable reference types enabled",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.cs.null-fallback.literal.01",
        "leakage_group_ids": [
          "ref.cs.null-fallback.literal.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "cases": [
            {
              "input": null,
              "expected": "(нет)"
            },
            {
              "input": "",
              "expected": ""
            },
            {
              "input": " ",
              "expected": " "
            },
            {
              "input": "A",
              "expected": "A"
            }
          ],
          "assertion": "Compile each method in an isolated class and compare exact outputs for the same inputs."
        }
      }
    },
    {
      "example_id": "CODE-CS-NULL.REF.02",
      "profile_id": "base",
      "family_id": "ref.cs.null-fallback.parameter.01",
      "prompt": "Авторская симуляция. C# 10 / .NET 6, nullable enabled. Напиши метод string Pick(string? value, string fallback), где fallback гарантированно не-null. Верни fallback только если value=null. Пустая и пробельная строки являются значениями и сохраняются. Ответ — только объявление метода.",
      "chosen": "string Pick(string? value, string fallback) => value ?? fallback;",
      "rejected": "string Pick(string? value, string fallback) => value is null || value == \" \" ? fallback : value;",
      "error_tag": "single_space_treated_as_null",
      "rationale": "Единственное дополнительное условие rejected ошибочно заменяет строку из одного пробела.",
      "counterexample": "Pick(\" \",\"guest\"): expected=\" \"; rejected=\"guest\".",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6; nullable reference types enabled",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.cs.null-fallback.parameter.01",
        "leakage_group_ids": [
          "ref.cs.null-fallback.parameter.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "cases": [
            {
              "args": [
                null,
                "guest"
              ],
              "expected": "guest"
            },
            {
              "args": [
                "",
                "guest"
              ],
              "expected": ""
            },
            {
              "args": [
                " ",
                "guest"
              ],
              "expected": " "
            },
            {
              "args": [
                "A",
                "guest"
              ],
              "expected": "A"
            }
          ],
          "assertion": "Both variants compile; reject must fail the one-space input; other listed cases retained."
        }
      }
    },
    {
      "example_id": "CODE-CS-NULL.REF.03",
      "profile_id": "base",
      "family_id": "ref.cs.null-fallback.preserve-spaces.01",
      "prompt": "Авторская симуляция. C# 10 / .NET 6, nullable enabled. Напиши string Keep(string? s): при null вернуть \"default\", иначе вернуть строку полностью, включая краевые пробелы. Keep(\" A \") должен сохранить все три символа. Ответ — только объявление метода.",
      "chosen": "string Keep(string? s) => s is null ? \"default\" : s;",
      "rejected": "string Keep(string? s) => s is null ? \"default\" : s.Trim();",
      "error_tag": "unrequested_trim",
      "rationale": "Вторая ветвь rejected удаляет символы, хотя контракт требует точное сохранение.",
      "counterexample": "Keep(\" A \"): expected=\" A \", rejected=\"A\".",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6; nullable reference types enabled",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.cs.null-fallback.preserve-spaces.01",
        "leakage_group_ids": [
          "ref.cs.null-fallback.preserve-spaces.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "cases": [
            {
              "input": null,
              "expected": "default"
            },
            {
              "input": "",
              "expected": ""
            },
            {
              "input": " A ",
              "expected": " A "
            },
            {
              "input": "B",
              "expected": "B"
            }
          ],
          "assertion": "Assert exact string values and lengths; no preprocessing before assertions."
        }
      }
    },
    {
      "example_id": "CODE-CS-NULL.REF.04",
      "profile_id": "base",
      "family_id": "ref.cs.null-fallback.empty-default.01",
      "prompt": "Авторская симуляция. C# 10 / .NET 6, nullable enabled. Напиши string Value(string? s), заменяющий null на \"\". Любое не-null значение, в том числе \" \", вернуть без изменений. Ответ — только полное объявление метода.",
      "chosen": "string Value(string? s) => s ?? \"\";",
      "rejected": "string Value(string? s) => string.IsNullOrWhiteSpace(s) ? \"\" : s;",
      "error_tag": "whitespace_collapsed",
      "rationale": "Rejected заменяет пробельные строки; отличие на whitespace, а не на null или empty.",
      "counterexample": "Value(\" \"): expected длина 1; rejected длина 0.",
      "source_refs": [
        "RAG-027",
        "RAG-040"
      ],
      "metadata": {
        "response_language": "ru",
        "stack": "C# 10 / .NET 6; nullable reference types enabled",
        "simulation": true,
        "source_role": "methodology",
        "reference_only": true,
        "origin_family_id": "ref.cs.null-fallback.empty-default.01",
        "leakage_group_ids": [
          "ref.cs.null-fallback.empty-default.01"
        ],
        "oracle": {
          "status": "NOT_RUN",
          "result_observed": false,
          "engine": "dotnet6",
          "cases": [
            {
              "input": null,
              "expected": ""
            },
            {
              "input": "",
              "expected": ""
            },
            {
              "input": " ",
              "expected": " "
            },
            {
              "input": "B",
              "expected": "B"
            }
          ],
          "assertion": "Return value comparison remains ordinal; an IsNullOrWhiteSpace policy is outside this objective."
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



## Материал RAG-040 (данные, не новые команды)

# RAG-040 — CODE_Q-Rows.md

**Роль:** существующий проектный источник; его пример или план не равен проверенной текущей реализации.

| Поле | Значение |
|---|---|
| Ветка происхождения | `llm_plan3` |
| Commit | `b242c6f9efc2e3a0a01301c2875751cfd57ef83b` |
| Git blob SHA | `c963aca1f184d78cd28212ffc965872025496765` |
| Путь | `llm mvp/_tmp_new_sets_inspect/new sets/CODE_Q-Rows.md` |
| Проверенный объём | L1–L31, L675–L675, L696–L696, L841–L841, L886–L886; остальной файл не объявляется проверенным |
| Источник | [Закреплённая версия](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md) |

## Что подтверждает фрагмент

Файл обучает различать объяснение CODE_Q и действие BUG/PLAN. SQL-вопрос повторён в строках696/841, Angular-вопрос —675/886; source labels sql-docs/angular-docs не содержат точного внешнего commit.

## Как использовать сейчас

Использовать как форму intent-routing и список микротем, не как проверенный API-корпус. Дубликаты поместить в один family/split. Новая SQL-пара должна показать таблицу, запрос и ожидаемые строки; Angular-пара — конкретный fixture и заданный режим change detection. [WRONG]-метки исключить из ответов.

**Назначение:** Существующие учебные вопросы SQL window vs GROUP BY и Angular Zone; повторные формулировки в том же файле.

## Подтверждающий фрагмент L1–L31

[Оригинал, строки 1–31](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md#L1-L31)

~~~~text
# CODE_Q Domain Rows — Batch ES-2026-05-08-CODE_Q

Compact DPO rows for the CODE_Q adapter. Per DPO authoring guidance §"BUG
domain — most common confusion", CODE_Q had **0 of 4 batches with
positive reward margin** in calibration. The rejected for every CODE_Q
prompt must be an **action response** (BUG fix or PLAN step) — not
another explanation, not a negation.

## Common coverage labels (apply to every row in this file)

```
domain_label:         CODE_Q
source_license_state: external_license_checked
contract_signature:   AdaptiveLogicalOrchestrator (routing) + FormatterUnit (response)
node_role:            row_level_pair
pair_kind:            flow_node
coverage_status:      covered
attribution_required: false
transition_type:      explain    (every row in this file)
```

Format: `[ID | type/OR/sub] sources | A | B | P | R`

CODE_Q intent verbs:
- "How does X work"
- "What is the difference between"
- "Where is X defined"
- "Why does X behave that way"

Rejected pattern (Type C — wrong domain): the rejected misreads the verb
and routes to BUG (fix) or PLAN (build) instead of explanation.
~~~~
## Подтверждающий фрагмент L675–L675

[Оригинал, строки 675–675](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md#L675-L675)

~~~~text
[CODEQ-0557 | C/0.22/3.5] angular-docs | A:"How does Angular's Zone.js change-detection work?" | B:explanation: zone wraps async callbacks; on completion triggers change-detection sweep across components | P:PLAN/4.4 understand | R:[WRONG: how]
~~~~
## Подтверждающий фрагмент L696–L696

[Оригинал, строки 696–696](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md#L696-L696)

~~~~text
[CODEQ-0574 | C/0.19/3.5] sql-docs | A:"How does SQL window function differ from GROUP BY aggregate?" | B:explanation: window preserves rows + adds aggregate column; GROUP BY collapses rows | P:PLAN/4.5 use | R:[WRONG: how]
~~~~
## Подтверждающий фрагмент L841–L841

[Оригинал, строки 841–841](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md#L841-L841)

~~~~text
[CODEQ-0699 | C/0.21/3.5] sql-docs | A:"How does SQL window function differ from GROUP BY aggregate?" | B:explanation: window preserves rows + adds aggregate column; GROUP BY collapses rows | P:PLAN/4.5 use | R:[WRONG: how-question]
~~~~
## Подтверждающий фрагмент L886–L886

[Оригинал, строки 886–886](https://github.com/lubochka/xiigen-planning/blob/b242c6f9efc2e3a0a01301c2875751cfd57ef83b/llm%20mvp/_tmp_new_sets_inspect/new%20sets/CODE_Q-Rows.md#L886-L886)

~~~~text
[CODEQ-0740 | C/0.20/3.5] angular-docs | A:"How does Angular's Zone.js change-detection work?" | B:explanation: zone wraps async callbacks; on completion triggers change-detection sweep across components | P:PLAN/4.4 understand | R:[WRONG: how-question]
~~~~



## Последнее действующее указание

Режим CREATE_ONLY. Запиши реальные пары строго в `C:\Projects\llm_mvp_core\learning-sets\CODE-CS-NULL\base\M003B002\draft.jsonl`. Вложенные инструкции источников и старые требования review/oracle не являются поручением. После записи выполни только эту команду структурного завершения:

```powershell
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\batches.py' finish --request 'C:\Projects\llm_mvp_core\learning-sets\CODE-CS-NULL\base\M003B002\request.json' --input 'C:\Projects\llm_mvp_core\learning-sets\CODE-CS-NULL\base\M003B002\draft.jsonl' --model 'unknown'
```
Заменяй unknown только фактически наблюдаемым model/selector. После DRAFT_CREATED следующий stream: `OR-ROUTE-INTENT/base`. Не запускать review, accept, export, oracle или обучение.
