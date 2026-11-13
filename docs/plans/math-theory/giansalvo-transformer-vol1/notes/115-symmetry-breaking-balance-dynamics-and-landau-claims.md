---
title: "115. Symmetry breaking: нормировка α, знак дрейфа и Landau-аргумент"
source_pdf_pages: [383, 384]
supplementary_source_pdf_pages: [373, 382]
book_pages: [347, 348]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 115. Symmetry breaking: нормировка α, знак дрейфа и Landau-аргумент

[Индекс библиотеки](../README.md) · [§11.6.2](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#section-11-6-2) · [Предыдущая карточка](114-gaussian-initialisation-moments-and-concentration.md) · [Следующая карточка](116-bimodal-emergence-and-stochastic-basin-claims.md)

## Предлагаемая цепочка Stage B

После Gaussian init автор пытается доказать, что $\alpha=1/\sqrt2$ — неустойчивое состояние с двумя устойчивыми фазами $\alpha=0$ и $\alpha=1$. Аргумент проходит через balance equation (11.13), Lemma 11.25 о нулевом ожидаемом дрейфе, Proposition 11.27 о saddle и Remark 11.28 о Landau potential. Затем на PDF 384 начинается Theorem 11.29 о бимодальной популяции. [§§11.6.2–11.6.3](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#section-11-6-2).

В этой цепочке есть отдельные алгебраические ошибки. Их нельзя устранить одним добавлением слова «conditional»: сначала нужно согласовать переменную и динамику.

## Формула (11.13) использует другую α

Ранее глава определила **отношение норм**

$$
\alpha=\frac{\lVert M_a\rVert_F}{\lVert M\rVert_F}.
$$

Положим $A=\lVert M_a\rVert_F^2$, $S=\lVert M_s\rVert_F^2$, $B=A+S$ и $r=A/B=\alpha^2$. Для прямого flow $\dot M_a=-\eta G_a$, $\dot M_s=-\eta G_s$ обозначим $a_g=\langle M_a,G_a\rangle_F$, $s_g=\langle M_s,G_s\rangle_F$. Тогда при $B>0$:

$$
\dot r
=-\frac{2\eta}{B}
\left[(1-r)a_g-rs_g\right].
$$

Именно эту формулу источник пишет в (11.13), заменяя $r$ символом $\alpha$. Для настоящей unsquared $\alpha$ при $\alpha>0$ получается

$$
\boxed{
\dot\alpha
=-\frac{\eta}{\alpha B}
\left[(1-\alpha^2)a_g-\alpha^2s_g\right].
}
$$

Для дискретного Euler step соответствующая разность имеет остаток второго порядка при гладкости; непрерывный flow и шаг SGD следует обозначать раздельно. Эти формулы также предполагают прямой flow по $M$; factorised QK training требует индуцированного обновления. [Definition 11.4](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#definition-11-4), [исходное (11.13)](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#equation-11-13), [карточка 110](110-skew-gradient-flow-and-symplectic-eigenplanes.md).

Сохранить (11.13) можно было бы для переменной $r$, но тогда равные энергии соответствуют $r=1/2$, а не $1/\sqrt2$. Нельзя одновременно использовать производную squared balance и значение равновесия unsquared balance.

## Одновременная смена знака не обнуляет inner product

Lemma 11.25 опирается на symmetry $M_a\mapsto-M_a$, которая для чётного loss даёт $G_a\mapsto-G_a$. Но

$$
\langle -M_a,-G_a\rangle_F
=\langle M_a,G_a\rangle_F,
$$

а не её отрицание. Поэтому инвариантность совместного закона не заставляет $\mathbb E\langle M_a,G_a\rangle_F$ быть нулём. Дополнительная «orientation symmetry on $M_s$» также не следует из сформулированной E2. [Lemma 11.25 и proof](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#lemma-11-25).

Редакторский тест достаточности symmetry: для $\mathcal L=\frac12\lVert M_a\rVert_F^2$ loss чётен по $M_a$, но $G_a=M_a$ и

$$
\langle M_a,G_a\rangle_F=\lVert M_a\rVert_F^2>0
$$

при $M_a\ne0$. Для корректной unsquared координаты $\dot\alpha=-\eta\alpha(1-\alpha^2)$, поэтому при $\alpha=1/\sqrt2$ дрейф равен $-\eta/(2\sqrt2)$, не нулю. Этот пример проверяет использованную логическую импликацию «чётность ⇒ нулевой дрейф»; он не подменяет конкретную MLM-задачу квадратичной.

Есть и более принципиальное различие: преобразование $M_a\mapsto-M_a$ **оставляет $\alpha$ неизменной**. Оно меняет ориентацию, но не переставляет симметричную и антисимметричную энергетические фазы $\alpha=0$ и $\alpha=1$. Поэтому E2 сама не задаёт симметрию между этими двумя бассейнами.

## Средний нулевой дрейф не равен неподвижной точке каждой траектории

Даже если бы ожидаемый дрейф удалось обнулить, отдельные состояния могли бы двигаться в разные стороны. Нулевое ensemble mean в одной координате не гарантирует стационарности полного состояния, инвариантности распределения или существования автономного уравнения $\dot\alpha=f(\alpha)$. В выражении для производной остаются $B$, два inner product и остальные параметры сети. [Remark 11.26](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#remark-11-26).

## Знак κ и отсутствующие члены линеаризации

Proposition 11.27 печатает

$$
\kappa=-\frac2B(a_g+s_g)(-\eta)
=\frac{2\eta}{B}(a_g+s_g).
$$

Если $a_g<0$ доминирует, как далее предполагает proof, то эта собственная формула даёт **$\kappa<0$**, тогда как текст заключает $\kappa>0$. Это явная ошибка знака, независимо от предшествующей проблемы нормировки. [Proposition 11.27](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#proposition-11-27).

Кроме того, реальная линеаризация должна учитывать изменение $B$, $M_s,M_a$, градиентов и связанных переменных; нельзя дифференцировать только явные множители $(1-\alpha)$ и $\alpha$, считая всё остальное постоянным, без определённого пути возмущения. Положительная дисперсия случайного коэффициента не доказывает нужный знак его реализации. Даже установленная локальная неустойчивость одной точки ещё не доказывает два глобальных устойчивых attractors, их бассейны или экспоненциальную сходимость к концам отрезка.

Упоминание E3 также наследует проблемы Gap-Opening Proposition 11.16: наличие gap само не задаёт знак $a_g$ и не закрывает missing terms. [Карточка 112](112-gap-opening-threshold-and-perturbation-limits.md).

## Проверка напечатанного Landau potential

Remark 11.28 использует

$$
\mathcal F(\alpha)=a(\Gamma-\Gamma^*)\alpha^2+b\alpha^4+O(\alpha^6).
$$

Для показанной **квадратично-квартической части**, обозначив $c=a(\Gamma-\Gamma^*)$, получаем

$$
\mathcal F'(1/\sqrt2)=\sqrt2(c+b),
\qquad
\mathcal F''(1/\sqrt2)=2c+6b.
$$

Это не заявленный в источнике знак $c-6b/4$. Точка $\alpha=1/\sqrt2$ также не является критической без условия $c+b=0$ для этой усечённой модели. При gradient descent положительная вторая производная в критической точке означает локальное притяжение, а не неустойчивость. [Remark 11.28](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#remark-11-28).

Остаток $O(\alpha^6)$ задан около нуля; подстановка фиксированного $\alpha\approx0.707$ требует контроля его производных, которого здесь нет. Следовательно, даже исправленная арифметика усечённого potential не заменит доказательства полной динамики и двух заявленных бассейнов.

## Точная граница Theorem 11.29

На PDF 384 перенесены вступление Stage C, название Theorem 11.29 и первая строка постановки, заканчивающаяся словом **“Under”**. Гипотезы, пункты и доказательство находятся в следующем source-файле. [Начало theorem](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#theorem-11-29), [продолжение на PDF 385](../source/0385-0390-ch11-emergence-and-residual-amplification.md#theorem-11-29-continued).

При чтении популяционного вывода необходимо переносить сюда выявленные ограничения Stage A и Stage B. Полная транскрипция сохраняет утверждения автора; библиотека не превращает их в независимо проверенную гарантию bimodal emergence.

