# Что фактически создано прогоном внутри каталога испытаний

Прочитано обратно с диска после обоих прогонов, 2026-11-09.

Корень каталога испытаний: `C:\Projects\llm_mvp_core\.codex-test-temp`

Состояние до прогона: каталога НЕ существовало (проверено обращением к пути,
результат «False»). Поэтому всё перечисленное ниже создано именно этим
прогоном, а не осталось от прежних запусков.

Имена папок выглядят как `pos-<32 знака>`, потому что общая оснастка обрезает
имя области до трёх знаков и добавляет уникальный номер прогона. Все 19 папок
созданы проверками класса допуска к обучению после оценки
(DesignConversionPostEvaluationTrainingEligibilityTests): их 14 проверок дали
22 исполненных случая, и каждый случай заводил свой корень.

| Полный путь созданной папки | Вложенных папок | Файлов | Байт |
|---|---|---|---|
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-03ffea00bbc74528aca4ae8eb80d59fa` | 2 | 2 | 0 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-0646d5daa36949e895d8772e7950cbbc` | 0 | 5 | 46279 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-15d9065d5c374a2693a3dfa7a5054c23` | 0 | 3 | 27479 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-2ab0e2b9e688405bb5d538f3ef54adaa` | 0 | 3 | 14665 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-32c9a31ce7e443fd83edd064373b30a7` | 2 | 3 | 796 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-34ab1e8eb191496aa0bd6246ebeb17c5` | 2 | 4 | 796 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-4bd7fe44fdd14104ab3fd52068de0759` | 0 | 3 | 28646 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-4f301d207b7a470790903d4ca9582e20` | 2 | 6 | 22741 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-575483887eba4e4dbab122eb0276a1aa` | 0 | 3 | 28928 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-5d3dc17f77dc47fea36ba6f7334a1893` | 2 | 3 | 811 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-8251b49175e44a4eb8b963af7ea596fc` | 1 | 2 | 898 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-a0ca164ffd9a4fa389771412d43c5966` | 1 | 2 | 33 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-ad89354ece4e4b2cbfb447b45ec60d75` | 2 | 8 | 39993 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-add7a39b58154ad2851b20a113ff798e` | 2 | 6 | 15603 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-d6d78723eb874731ab0f862e356ea764` | 2 | 9 | 45880 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-e0281c5c7a8e440ba7208bc29af78eed` | 2 | 3 | 868 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-e8483855e990468c966305e95557da92` | 2 | 6 | 15742 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-ef62763a192f47ccbdb2eeb469910f8a` | 2 | 7 | 29573 |
| `C:\Projects\llm_mvp_core\.codex-test-temp\pos-f8c0d2a783514b439b3d00d214821ab6` | 2 | 6 | 15558 |

Итого: папок верхнего уровня 19, вложенных папок 26, файлов 84, суммарный
размер 335 289 байт (327,4 КБ).

## Какое содержимое внутри

Это настоящие рабочие хранилища проверок, а не пустышки. Пример содержимого
папки `pos-0646d5daa36949e895d8772e7950cbbc`:

- `conversion-results.index.json` — 2056 байт, указатель записей;
- `conversion-results.jsonl` — 26 402 байта, построчный журнал записей
  результатов преобразования;
- `conversion-results.jsonl.mutation.lock` — 0 байт, замок на изменение
  журнала;
- `training-intake-requests.jsonl` — 17 821 байт, журнал заявок на приём в
  обучение;
- `training-intake-requests.jsonl.mutation.lock` — 0 байт, замок на изменение
  этого журнала.

Папка `pos-03ffea00bbc74528aca4ae8eb80d59fa` содержит 2 файла нулевой длины —
это замки, созданные до записи содержимого в том случае проверки, который
намеренно обрывается раньше записи. Нулевой размер здесь ожидаем, а не дефект.

## Чего прогон НЕ оставил

Названная в поручении проверка хранения кандидатов
(`CandidatePayloadCustodyStore_RepoRootTempRootPreparesCommitsAndResolves`,
полное имя `Tests.DesignConversionDpoCorpusAdmissionTests.
CandidatePayloadCustodyStore_RepoRootTempRootPreparesCommitsAndResolves`)
прошла успешно и свою папку создала, но её оснастка одноразовая и при
освобождении удаляет собственный корень с повторами. Поэтому после прогона её
папки на диске нет. Сам каталог испытаний она при этом не удаляет: он остаётся.
Это поведение самой проверки, а не пропуск с нашей стороны.
