# Перевод EN/RU/HE: материал для DPO

Каталог: 19 родительских тем и 18 микроцелей. Полный текущий порядок и NEXT — [EXECUTION-QUEUE.md](../../EXECUTION-QUEUE.md); прямое указание ID выше очереди. [Общий промпт Kiro](../../14-GENERATION-CAMPAIGN.md) и [контракт формата](../../02-DATA-CONTRACT.md). Подготовка нового дополнения не означает выполненную генерацию, native oracle или обучение.

Целевые домены tr.en/tr.ru/tr.he; отдельные профили ru-en, he-en, en-ru, he-ru, en-he, ru-he. Каноническое слово, опечатка и неправильная раскладка связаны лексической семьёй; семейство исходной сцены управляет split отдельно. Параллельные, обратные и контрфактические переводы одного случая не распределяются по разным split. Правильные варианты перевода не объявляются rejected.

## Родительские темы

| ID | Тема | Домены |
|---|---|---|
| [TR01](TR01.md) | Сохранение отрицания | tr.en, tr.ru, tr.he |
| [TR02](TR02.md) | Кто совершил действие над кем | tr.en, tr.ru, tr.he |
| [TR03](TR03.md) | Время и завершённость действия | tr.en, tr.ru, tr.he |
| [TR04](TR04.md) | Число и явно заданный род | tr.en, tr.ru, tr.he |
| [TR05](TR05.md) | Значение слова по контексту | tr.en, tr.ru, tr.he |
| [TR06](TR06.md) | Точные числа, метки и заполнители | tr.en, tr.ru, tr.he |
| [TR07](TR07.md) | Опечатки внутри одной лексической семьи | tr.en, tr.ru, tr.he |
| [TR08](TR08.md) | Слово набрано в неверной раскладке | tr.en, tr.ru, tr.he |
| [TR09](TR09.md) | Перевод вокруг кода и технических имён | tr.en, tr.ru, tr.he |
| [TR10](TR10.md) | Уточнение при неразрешённой неоднозначности | tr.en, tr.ru, tr.he |
| [TR-EN-IF-01](TR-EN-IF-01.md) | Условие действия и направление условной связи — английский | tr.en |
| [TR-EN-SCOPE-01](TR-EN-SCOPE-01.md) | Область действия отрицания, «только» и количества — английский | tr.en |
| [TR-EN-ORDER-01](TR-EN-ORDER-01.md) | Временной порядок событий и одновременный интервал — английский | tr.en |
| [TR-RU-IF-01](TR-RU-IF-01.md) | Условие действия и направление условной связи — русский | tr.ru |
| [TR-RU-SCOPE-01](TR-RU-SCOPE-01.md) | Область действия отрицания, «только» и количества — русский | tr.ru |
| [TR-RU-ORDER-01](TR-RU-ORDER-01.md) | Временной порядок событий и одновременный интервал — русский | tr.ru |
| [TR-HE-IF-01](TR-HE-IF-01.md) | Условие действия и направление условной связи — иврит | tr.he |
| [TR-HE-SCOPE-01](TR-HE-SCOPE-01.md) | Область действия отрицания, «только» и количества — иврит | tr.he |
| [TR-HE-ORDER-01](TR-HE-ORDER-01.md) | Временной порядок событий и одновременный интервал — иврит | tr.he |

## Узкие задания

| ID | Учебная граница | Reference |
|---|---|---:|
| [TR07-OMISSION](../../curriculum/cards/TR07-OMISSION.md) | Ровно одна пропущенная буква известного слова | 6 |
| [TR07-TRANSPOSE](../../curriculum/cards/TR07-TRANSPOSE.md) | Перестановка двух соседних букв одного слова | 6 |
| [TR07-VALIDWORD](../../curriculum/cards/TR07-VALIDWORD.md) | Корректное похожее слово нельзя исправлять в другое | 6 |
| [TR08-LAYOUT](../../curriculum/cards/TR08-LAYOUT.md) | Одно слово в известной неверной раскладке | 6 |
| [TR10-CONDITIONAL](../../curriculum/cards/TR10-CONDITIONAL.md) | Уточнить только тогда, когда контекста действительно недостаточно | 12 |
| [TR01-NEGATION-SCOPE](../../curriculum/cards/TR01-NEGATION-SCOPE.md) | Область действия отрицания в короткой фразе | 12 |
| [TR02-ROLE-ORDER](../../curriculum/cards/TR02-ROLE-ORDER.md) | Смысловые роли при изменении порядка слов | 12 |
| [TR03-COMPLETION](../../curriculum/cards/TR03-COMPLETION.md) | Завершённое действие и ещё продолжающийся процесс | 12 |
| [TR04-EXPLICIT-GENDER](../../curriculum/cards/TR04-EXPLICIT-GENDER.md) | Род участника, явно указанный в тексте | 12 |
| [TR05-CONTEXT-SENSE](../../curriculum/cards/TR05-CONTEXT-SENSE.md) | Значение многозначного слова при достаточном контексте | 12 |
| [TR06-PLACEHOLDER](../../curriculum/cards/TR06-PLACEHOLDER.md) | Точное сохранение имени заполнителя в переводе | 12 |
| [TR09-IDENTIFIER](../../curriculum/cards/TR09-IDENTIFIER.md) | Сохранение точного программного идентификатора | 12 |
| [TR02-COREFERENCE](../../curriculum/cards/TR02-COREFERENCE.md) | Референт местоимения в соседнем предложении | 24 |
| [TR05-MODALITY](../../curriculum/cards/TR05-MODALITY.md) | Разрешение и физическая способность слова «может» | 24 |
| [TR08-RESTORE-OR-PRESERVE](../../curriculum/cards/TR08-RESTORE-OR-PRESERVE.md) | Восстановить неверную раскладку или сохранить буквальный идентификатор по видимому контексту | 12 |
| [TR07-REPEAT](../../curriculum/cards/TR07-REPEAT.md) | Ровно одна лишняя повторённая буква внутри слова | 12 |
| [TR06-NUMBER-UNIT](../../curriculum/cards/TR06-NUMBER-UNIT.md) | Сохранение точной пары «число — единица» при переводе | 12 |
| [TR03-EVENT-ORDER](../../curriculum/cards/TR03-EVENT-ORDER.md) | До и после: порядок двух завершённых событий | 12 |

Код, документация, перефразирование и перевод одного источника сохраняют общие origin/leakage до split. Публичные reference и их потомки не являются закрытым EXAM. Точные first-run/dataset/B1 пути задаёт [pilot-queue.json](../../curriculum/pilot-queue.json); метрики, восстановление и достаточный ненулевой старт — [05](../../05-LEARNING-AND-CHECKPOINTS.md). Выбор/оценка выполняются CODEX, генерация — китайской моделью Kiro по [10](../../10-CODEX-AND-CHINESE-ROLES.md). NEXT/STOP соблюдаются немедленно, старые версии сохраняются.
