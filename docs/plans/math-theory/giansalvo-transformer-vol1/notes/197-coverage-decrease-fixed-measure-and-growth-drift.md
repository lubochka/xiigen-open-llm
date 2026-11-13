---
title: "197. H2: уменьшение coverage при фиксированной мере и при изменении популяции голов"
source_pdf_pages: [556, 557]
book_pages: [520, 521]
batch: "012"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 197. H2: уменьшение coverage при фиксированной мере и при изменении популяции голов

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Что утверждает H2

Assumption 15.4 ограничивает средний квадрат ошибки новой сигнатуры относительно uncovered maximiser. Theorem 15.5 заявляет decrease

$$
\mathbb E\rho_\ell(t+\Delta)
\le\rho_\ell(t)-c_\ell r_\ell^2
+O(\xi_{\mathrm{init}}^2+\tau_{\mathrm{opt}}^2+\epsilon_{\mathrm{stab}}).
$$

В proof neighbourhood $U$ получает новый атом, а остальная часть support считается неизменной с точностью до stability error. [Условия](../source/0556-0559-ch15-stability-cover-preservation-and-termination.md#assumption-15-4), [теорема](../source/0556-0559-ch15-stability-cover-preservation-and-termination.md#theorem-15-5), [доказательство](../source/0556-0559-ch15-stability-cover-preservation-and-termination.md#pdf-0557).

## Точное основание для фиксированной задачи

При фиксированной мере $\mu$ и сохранении старых атомов расширение $\mathcal D^+=\mathcal D\cup\{a\}$ всегда даёт

$$
d(\sigma,\mathcal D^+)^2\le d(\sigma,\mathcal D)^2
$$

для каждой $\sigma$. Поэтому интегральный residual не возрастает. Строгое уменьшение требует положительной массы области, для которой новый атом действительно ближе.

Количественная редакторская версия: пусть $r=d(\sigma^\star,\mathcal D)$, область $U$ лежит в шаре радиуса $ar$ вокруг $\sigma^\star$, а $\|a_{\mathrm{new}}-\sigma^\star\|\le br$. Тогда на $U$

$$
d(\sigma,\mathcal D)\ge(1-a)r,\qquad
d(\sigma,\mathcal D^+)\le(a+b)r.
$$

При $a+b<1-a$ получаем decrease не меньше

$$
\mu(U)\bigl[(1-a)^2-(a+b)^2\bigr]r^2.
$$

Таким образом, коэффициент содержит и массу, и геометрический margin. В (15.27) $c_\ell$ назван самой empirical mass, а доказательство говорит о «constant multiple». При использовании численного bound нужно явно согласовать эту константу с радиусом neighbourhood и ошибкой реализации.

## В книге меняется сама мера

Исходное $\mu_\ell(t)$ — эмпирическая мера **существующих голов**, а не независимое распределение task demand. Добавление головы меняет и число атомов этой меры, и её нормировку. [Определение (15.2)](../source/0547-0550-ch15-dynamic-variables-and-controller-introduction.md#equation-15-2), [разбор наблюдаемости](193-dynamic-head-measure-and-residual-observability.md).

Если старые $H$ сигнатур вообще не двигаются, новая голова становится новым словарным атомом, а её собственный distance равен нулю, то

$$
\mu^+=\frac{H}{H+1}\mu+\frac1{H+1}\delta_{\sigma_{\mathrm{new}}},
\qquad
\rho^+\le\frac{H}{H+1}\rho.
$$

Часть наблюдаемого падения возникает от изменения весов, даже без улучшения покрытия внешнего спроса задачи. Поэтому до и после события полезно дополнительно сравнивать coverage на одной фиксированной reference measure. Это отдельная редакторская рекомендация.

Во время warm-up могут измениться старые сигнатуры, assignments и словарь. Assumption 15.4 контролирует только новую сигнатуру; общий drift интеграла ею не ограничен. Упомянутый в (15.27) $\epsilon_{\mathrm{stab}}$ должен иметь определённое отношение к изменениям меры и словаря, с нужным контролем расстояний или моментов.

## Связь с loss и residual amplification остаётся условной

Неувеличение quantisation distortion — геометрическое свойство множества атомов. Из него не следует уменьшение task loss или residual amplification, определённой как нормированная величина layer update. Источник ограничивает соответствующий переход случаем, когда signature distance определяется подходящей residual directional coordinate. Для практического использования нужны сама эта coordinate map и проверка полезности после warm-up. [Авторская связь с deflation](../source/0556-0559-ch15-stability-cover-preservation-and-termination.md#pdf-0557).
