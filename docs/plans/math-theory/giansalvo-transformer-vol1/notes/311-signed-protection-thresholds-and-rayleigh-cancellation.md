---
title: "311. Signed protection: проверка pass/fail и пределы Rayleigh-диагностики"
source_pdf_pages: [728, 730]
book_pages: [692, 694]
batch: "015"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 311. Signed protection: проверка pass/fail и пределы Rayleigh-диагностики

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Правильны ли решения Table 18.2 и означает ли малый Rayleigh quotient отсутствие значимого направления?

## Два разных порога

В source growth threshold равен $\theta_w=.4\Gamma_0$, а auxiliary protection threshold $\phi_g=.05\theta_w=.02\Gamma_0$. One-sided правило сохраняет голову при $\gamma_h>\phi_g$, signed-magnitude — при $|\gamma_h|>\phi_g$. Их нельзя подменять проверкой $|\gamma_h|>\theta_w$: это порог в 20 раз выше. В следующих описаниях $K_{\rm init}=8$ автор местами пишет обобщённо «gate threshold», но явная формула protection использует $\phi_g$. [Правила и Table 18.2](../source/0729-0731-ch18-signed-pruning-and-initial-head-count-sweep.md#section-18-1-2), [calibration](../source/0726-0728-ch18-warm-start-protocol-and-signed-protection.md#table-18-1).

## Независимый пересчёт всех решений

| Конфигурация | $\phi_g$ из напечатанного $\theta_w$ | One-sided: сохранены $h$ | Signed-magnitude: сохранены $h$ |
|---|---:|---|---|
| SST-2 BD MLM | 41.6985 | 0, 2, 3 | 0, 1, 2, 3 |
| MNLI BD MLM | 35.772 | 1 | 0, 1, 2, 3 |
| MNLI PCA MLM | 35.772 | 1, 2 | 0, 1, 2, 3 |
| MNLI BD no MLM | 57.5235 | нет | нет |

Все 16 строк и оба столбца pass/fail совпадают с арифметикой. В трёх MLM blocks правила расходятся на 1, 3 и 2 головах соответственно, суммарно на 6 из 12. Минимальный положительный signed margin среди них равен $44.76-35.772=8.988$ для MNLI PCA $h=2$. Для no-MLM максимальный модуль равен 38.32 и действительно ниже 57.5235. [Числа таблицы](../source/0729-0731-ch18-signed-pruning-and-initial-head-count-sweep.md#table-18-2).

Отношение spectral radius 1867.48 к четырём no-MLM модулям равно примерно 148.68, 876.75, 48.73 и 60.63. Поэтому фраза «two orders of magnitude smaller» является грубой характеристикой, а не точным свойством каждого отклика: два из четырёх меньше radius менее чем в 100 раз. На решения pass/fail это не влияет.

## Что sign и magnitude действительно означают

Для нормированного probe $u$ величина $u^\top Au$ измеряет суммарный signed quadratic response. Она не говорит, что $u$ является собственным вектором одного знакового класса. При indefinite $A$ положительные и отрицательные компоненты могут компенсироваться:

$$
A=\operatorname{diag}(1,-1),\quad u=(1,1)^\top/\sqrt2,
\quad u^\top Au=0,\quad\|Au\|_2=1.
$$

Поэтому пустой surviving set **по выбранным probes** не доказывает, что у оператора нет структурных modes, и не сертифицирует бесполезность encoder. Даже применение модуля после суммирования не устраняет это cancellation. Нормировка probes обязательна: без $\|u\|=1$ magnitude меняется при масштабировании $u$.

Верное преимущество signed rule — инвариантность решения к замене $A\mapsto-A$ при фиксированных probes и положительном пороге. One-sided rule этой инвариантности не имеет. Но большой модуль сам по себе не устанавливает, что отклик «не noise»: сильный шумовой outlier тоже может пересечь выбранный порог. Нужны uncertainty, повторяемость на независимых batches и связь с task loss. [Сильная интерпретация автора](../source/0729-0731-ch18-signed-pruning-and-initial-head-count-sweep.md#pdf-0730).

## Как сделать решение проверяемым

Если суммарная неопределённость $\gamma$ и $\phi$ ограничена $\delta_\gamma+\delta_\phi$, знак решения устойчив при margin $\bigl||\gamma|-\phi\bigr|>\delta_\gamma+\delta_\phi$. Здесь такого bound нет, поэтому вычисленные margins не являются statistical confidence. Отдельно требуется validation loss после удаления и проверка совместного эффекта нескольких голов.

Для проекта следует хранить signed quotient, его модуль, норму probe, projection residual, оба порога и конкретное действие controller. Удалённые головы и количество событий удаления — разные счётчики. Выполнена проверка таблицы и algebraic cancellation example; модельные эксперименты и pruning не запускались.
