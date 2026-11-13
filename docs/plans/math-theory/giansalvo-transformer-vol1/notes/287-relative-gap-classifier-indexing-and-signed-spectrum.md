---
title: "287. Relative-gap test: индексы kernel, signed spectrum и устойчивость решения"
source_pdf_pages: [694, 695]
book_pages: [658, 659]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 287. Relative-gap test: индексы kernel, signed spectrum и устойчивость решения

## Вопрос

Можно ли реализовать Definition 17.6 буквально и гарантирует ли Lemma 17.7 неизменность классификации?

**Нужны исправления порядка проверок и индексов, а для устойчивости дискретной ветви необходим margin до threshold.** Полезная часть lemma — непрерывность relative gap на положительной structural band — не решает эти вопросы. [Definition 17.6](../source/0693-0695-ch17-kernel-detection-and-relative-gap-test.md#definition-17-6), [Lemma 17.7](../source/0693-0695-ch17-kernel-detection-and-relative-gap-test.md#lemma-17-7).

## Ошибки буквального алгоритма

Первый шаг вычисляет $\gamma_k=(\widehat\lambda_k-\widehat\lambda_{k+1})/\widehat\lambda_k$ **для всех** индексов, включая нулевые, хотя последующая проза утверждает, что kernel отделён заранее. Возможны $0/0$ и деление на ноль. Rank filtering должно предшествовать такому относительному диагностированию.

Второй шаг выбирает **наибольший** $k_C$ с $|\widehat\lambda_{k_C+1}|<\theta_{\mathrm{rank}}$. Если разрешён $k_C=\tau_{\max}$, введённый sentinel $\widehat\lambda_{\tau_{\max}+1}=0$ при положительном пороге делает этот выбор автоматическим: kernel tail пуст. Даже при ограничении $k_C<\tau_{\max}$ наличие нескольких малых tail values сдвигает границу к последнему из них, а не к началу малого блока. Для PSD-спектра нужно определить число значений выше порога или первый малый индекс и отдельно обработать пустые множества.

Для signed spectrum kernel вообще может быть **в середине** упорядоченного спектра. Таков собственный пример автора:

$$
2.51,\quad0,\quad-0.37,\quad-2.14.
$$

Удалять tail после нуля значит ошибочно удалить два значимых отрицательных eigenvalues. Нужна маска $|\widehat\lambda_j|\le\theta$, а диагностику обоих endpoints следует задать раздельно. Небольшой набор только верхних Ritz values не сертифицирует полный kernel или нижний endpoint. [Спектр (17.15)](../source/0693-0695-ch17-kernel-detection-and-relative-gap-test.md#equation-17-15).

## Корректное ядро Lemma 17.7

Пусть $a=\lambda_k\ge b=\lambda_{k+1}\ge c\lambda_1>0$, $|\delta a|,|\delta b|\le\eta\lambda_1$ и $\eta<c/2$. Тогда

$$
\widetilde\gamma-\gamma
=\frac{b\,\delta a-a\,\delta b}{a(a+\delta a)},\qquad
|\widetilde\gamma-\gamma|
\le\frac{2\eta}{c-\eta}
=\frac{2\eta}{c}+O_c(\eta^2).
$$

Это редакторское уточнение доказывает заявленный порядок и допускает авторскую более слабую константу $2/c+1$. Прямая грубая triangle estimate последней строки напечатанного proof даёт $3/c$, поэтому нужная константа не извлечена там явно; само утверждение на указанной положительной band поправимо приведённой точной дробью. [Структурная band и expansion](../source/0693-0695-ch17-kernel-detection-and-relative-gap-test.md#display-0694-relative-gap-expansion).

Из малого изменения $\gamma$ не следует сохранение ответа $\gamma<\gamma_{\mathrm{near}}$. Для этого требуется

$$
|\gamma-\gamma_{\mathrm{near}}|
>\text{доказанная граница ошибки }\gamma.
$$

Аналогичный margin нужен около rank threshold. При точном попадании на границу сколь угодно малое perturbation может изменить discrete branch. Распознавание точной кратности Type A также не дано тремя шагами: они выделяют near cluster, не отделяя равные eigenvalues от близких.

## Проверка четырёхмерного примера

Объявленная классификация — regular dominant, regular trailing, один kernel vector — соответствует приведённому спектру. Она **не** демонстрирует одновременное присутствие всех трёх A/B/C, вопреки вводному предложению. После исключения нуля верхний gap равен $(2.51+0.37)/2.51\approx1.1474$, тогда как источник оставляет $1.0$, вычисленное до исключения. Нижнее значение $0.83$ получается как $(2.14-0.37)/2.14\approx0.8271$; это отдельная absolute-endpoint нормировка, не буквальная формула с отрицательным знаменателем. [Worked example](../source/0693-0695-ch17-kernel-detection-and-relative-gap-test.md#section-17-3-5).

После точного удаления направлений $2.51$ и $-2.14$ остаётся eigenvalue $-0.37$. Поэтому одна bidirectional head сама по себе не доказывает остановку при произвольном residual threshold; надо задать, какой residual и какой порог используются и как head реализует две spectral components. Kernel-aware учёт полезен, но из одной классификации численный бюджет $K^\star=1$ не сертифицирован.
