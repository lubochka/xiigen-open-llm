# Точный fixture SC01 и измеряемые targets

Статус: **зафиксированные входы будущего эксперимента; обучения в этом аудите не было**. Эта запись дополняет [карточку](02-DPO-SHIFTED-CONTEXT-01-CARD.md), не является командой запуска.

## Что означает пара

Код функции сам задаёт правильную цифру в завершающем комментарии. Chosen и rejected различаются только последней цифрой. Это искусственный механический simulation fixture для проверки next-token обучения и границы контекста. Здесь нет вызова внешней модели, нет скрытого teacher и нет утверждения о качестве authored production DPO.

Непосредственный источник исходных One/Two функций — [RESTORE-MINIMAL-SET/Program.cs, 51–72](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/RESTORE-MINIMAL-SET/Program.cs#L51-L72). Новый комментарий задаётся ниже явно. CODEX Astra 6 ultracode проверяет смысл пары; executor остаётся моделью по умолчанию. Если позднее создаётся отдельный generated corpus, внешняя открытая китайская модель может только генерировать, а выбор/оценка остаются CODEX; это другой corpus и другой результат.

## Boundary JSONL: точно 364 UTF-8 bytes

Файл будущего host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/fixture-boundary.jsonl`.

Копировать только две строки внутри code block. UTF-8 без BOM, LF между строками и один LF после второй строки; `\n` внутри JSON остаётся двумя байтами backslash+n. SHA256: **692be1ce627f76b459910738a9bdd98d6ffc55ecf0e9ac0f5046350cd26e60cb**.

```jsonl
{"id":"SC01-boundary-one","prompt":"// file: One.cs\n","chosen":"static int One()=>1;\n// expected numeric value: 1","rejected":"static int One()=>1;\n// expected numeric value: 2"}
{"id":"SC01-boundary-two","prompt":"// file: Two.cs\n","chosen":"static int Two()=>2;\n// expected numeric value: 2","rejected":"static int Two()=>2;\n// expected numeric value: 1"}
```

После JSON decode:

| Поле | One и Two |
|---|---:|
| Prompt bytes | 16 |
| Исходный source response bytes | 20 |
| Suffix bytes | 29 |
| Полный chosen/rejected response | 49 |
| Prompt + response | 65 |
| Первый различающийся absolute target index, 0-based | 64 |
| Последний input index B | 63 |
| Последний logit index B | 63 |
| Scored response targets | indices16…64, всего49 |
| Первый различающийся target при ошибочном отсечении | отсутствует; пара потеряла весь contrast |

При native vocab256 и нуле merges каждому байту соответствует token ID. Кодировать prompt и каждую response отдельно, как exact material; потом конкатенировать. Подтвердить это native Encode/Decode в readiness. Гипотеза не зависит от «угаданного» Unicode token count.

Первые 36 полных byte/tokens обеих сторон совпадают с донорским One/Two текстом. Благодаря causal context первые 20 response predictions могут сверяться с положительными TRAIN anchors донора. Эта причинная совместимость — основание проверки; до фактического native readout она не заменяет результат восстановления.

## Контроль JSONL: точно 358 UTF-8 bytes

Файл будущего host: `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/SC01/fixture-control.jsonl`.

Та же сериализация; SHA256: **649df762a811b645b3fb67f35d705031f848ff205d6dec8f5e0f471714dfe048**.

```jsonl
{"id":"SC01-control-one","prompt":"// file: One.cs\n","chosen":"static int One()=>1;\n// expected return value: 1","rejected":"static int One()=>1;\n// expected return value: 2"}
{"id":"SC01-control-two","prompt":"// file: Two.cs\n","chosen":"static int Two()=>2;\n// expected return value: 2","rejected":"static int Two()=>2;\n// expected return value: 1"}
```

Prompt16 + source20 + suffix28 =64. Первый различающийся absolute target63, соответствующая logit row62. Полный carrier остаётся64 в обоих путях; вход A64, вход B63. Этот контроль находится в текущей допустимой области и отделяет изменение математического objective от изменения допуска на границе.

**Контроль и boundary — разные fixture IDs.** Нельзя сравнивать final metrics контрольного source64 с boundary source65 как будто это один train set. У каждого собственные before/after и denominators; только внутри одной пары A/B входы идентичны.

## Anchor / EXAM / CONTROL readout

Anchor score — первые 20 response токенов исходных One/Two; суммарный denominator40. Сами boundary пары обучаются полностью по exact response mask, включая комментарий; anchor — отдельный измерительный срез той же policy.

Оставить donor held-out/readout тексты неизменными:

| Role | Prompt | Gold response |
|---|---|---|
| EXAM | `// file: Three.cs\n` | `static int Three()=>3;` |
| EXAM | `// file: Four.cs\n` | `static int Four()=>4;` |
| CONTROL | `// file: Pos.cs\n` | `static bool Pos(int n)=>n>0;` |

Они не поступают в optimizer, не выбирают LR, не выбирают checkpoint и не определяют пары. Их назначение — показать сопутствующие изменения после actual DPO updates, включая ухудшения. Это допустимые measurements результата обучения, а не самостоятельный «эксперимент без обучения».

Для финальной цифры записывать NLL, probability, rank, strict two-candidate win, all-vocab top1 и ties отдельно. При подсчёте по двум кандидатам нельзя назвать победу полной генерацией правильного ответа. При source-prefix top1>0 нельзя назвать правильной всю response. Общий `teacherForcedResponseExact` остаётся отдельной метрикой по всем response targets при gold prefix; это не свободная autoregressive генерация, которая здесь не измеряется (N/A).

Любое измеренное улучшение полезного показателя считается успехом его показателя/домена; рост accuracy не нужен. Положительный anchor доказывает token-level память, не полноту ответа. Для этого fixture смысловой terminal подответ проверяется отдельно: two-candidate correctness 0→0 не выдаётся за решённый подответ даже при positive anchor3/40. Полезная NLL/probability дельта сохраняется как успех показателя с пометкой NO_TASK_PROOF / NEEDS_CALIBRATION. Terminal0→1 валиден; terminal1→1 с улучшившимся NLL тоже может быть успешным. Terminal1→0 показывается регрессией.

## Сохранность и завершение

Существующие fixture/result не удалять. Нет автоповторов, скрытого warm-up, смены бюджета или «ещё одного полезного» опыта. На STOP прекратить немедленно; на NEXT закончить только назначенную фазу коротким статусом и перейти к следующему ID из реестра. Не продолжать SC01 ради доработки старого отчёта или тестов.

