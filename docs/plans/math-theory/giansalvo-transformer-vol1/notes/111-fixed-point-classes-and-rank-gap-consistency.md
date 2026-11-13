---
title: "111. Четыре класса фиксированных точек: проверка размерностей и спектра"
source_pdf_pages: [377, 379]
book_pages: [341, 343]
batch: "008"
language: ru
type: reading-note
status: source-grounded-with-editorial-caveats
---

# 111. Четыре класса фиксированных точек: проверка размерностей и спектра

[Индекс библиотеки](../README.md) · [Proposition 11.14](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#proposition-11-14) · [Предыдущая карточка](110-skew-gradient-flow-and-symplectic-eigenplanes.md) · [Следующая карточка](112-gap-opening-threshold-and-perturbation-limits.md)

## Авторская классификация

Источник называет четыре класса неподвижных точек потока $\dot M_a=-\eta G_a$:

| Класс | Условие в тексте Proposition 11.14 | Заявленная интерпретация |
|---|---|---|
| (i) Sequence-silent | $K=0$, следовательно $M_a=\mathcal D=0$ | Нулевая directional energy |
| (ii) Reciprocal attractor | $K=1$, $\mu_1-\mu_2=0$ | Равная энергия на двух плоскостях $\mathcal D$ |
| (iii) Directional attractor | $K=1$, $\mu_1-\mu_2>0$ | Одна доминирующая плоскость, $\gamma\to\gamma^{**}>1$ |
| (iv) Composite attractor | $K\ge2$ | Несколько плоскостей и распределённая энергия |

Здесь $K=\operatorname{rank}M_a/2$, а $\mu_k$ — амплитуды блоков собственного нормального разложения $\mathcal D=XM_aX^\top$. Автор помечает доказательство как sketch и отсылает к неопределённым главам “??”. [Полная формулировка на PDF 377–378](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#proposition-11-14).

**Эту таблицу нельзя использовать как доказанную исчерпывающую классификацию.** Ниже — конкретные внутренние проблемы исходной формулировки; EN-транскрипция сохраняет её без исправлений.

## K = 1 не даёт две ненулевые плоскости D

Всегда $\operatorname{rank}\mathcal D\le\operatorname{rank}M_a$. Поэтому при $K=1$ ранг $\mathcal D$ не превосходит 2, а число её активных плоскостей $K'$ не превосходит 1. Две ненулевые плоскости с одинаковой положительной амплитудой, как требует объяснение класса (ii), невозможны. При $K'=1$ величина $\mu_2$ в списке положительных амплитуд вообще не определена; если дополнить список нулём, разрыв $\mu_1-0$ положителен. [Corollary 11.12](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#corollary-11-12), [класс (ii)](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#proposition-11-14).

Если же под $\mu_1,\mu_2$ понимать две обычные крупнейшие **сингулярные величины**, у любой ненулевой вещественной антисимметричной матрицы они равны из-за парности $\lambda_k,\lambda_k$. Такой разрыв всегда нулевой и не отделяет directional class. Для разрыва между eigenplanes надо брать по одной амплитуде на каждую пару и явно задавать дополнение нулями. [Нормальная форма](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#equation-11-4).

## Сравниваемые плоскости лежат в разных пространствах

В (11.8) записано

$$
\operatorname{span}(q_1,p_1)=\operatorname{span}(u_1,v_1),
\qquad\lambda_1=\mu>0.
$$

Но $q_1,p_1\in\mathbb R^d$, тогда как $u_1,v_1\in\mathbb R^n$. При $n\ne d$ буквальное равенство подпространств не определено. Для осмысленного сравнения нужно отображение через $X$ и учёт его ядра, нормировки и геометрии. Даже при $n=d$, $X=cI$ даёт $\mathcal D=c^2M_a$, поэтому $\mu_k=c^2\lambda_k$; равенство амплитуд не является общим свойством. [Формула (11.8)](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#equation-11-8).

Аналогично, при $K\ge2$ число наблюдаемых плоскостей $\mathcal D$ может быть меньше $K$. Утверждение класса (iv) о распределении энергии по тем же $K$ плоскостям требует дополнительных условий на $X$.

## Sequence silence не эквивалентно Ma = 0

Нулевой $M_a$ достаточен для $\mathcal D=0$, но не необходим. Если строки $X$ лежат в $\operatorname{span}(e_1,e_2)$, а $M_a$ действует только на $\operatorname{span}(e_3,e_4)$, то $M_a\ne0$, $K=1$, но $\mathcal D=0$. То же верно при входе ранга 1 независимо от величины $M_a$. Поэтому sequence-dependent silence шире класса $K=0$; заявленная идентификация с silence manifold не вытекает из выписанных условий. [Класс (i)](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#proposition-11-14), [карточка 109](109-layer-observables-and-activation-normalisations.md).

## Указанный в proof sketch градиент противоречит ненулевым классам

В sketch утверждается $G_a\propto P_a(X^\top\mathcal D X)$. Однако $X^\top\mathcal D X$ уже антисимметрична. Более того, при $\mathcal D=XM_aX^\top$ точное тождество сопряжённости даёт

$$
\left\langle M_a,X^\top\mathcal D X\right\rangle_F
=
\left\langle XM_aX^\top,\mathcal D\right\rangle_F
=
\lVert\mathcal D\rVert_F^2.
$$

Следовательно, $X^\top\mathcal D X=0$ влечёт $\mathcal D=0$. Ни равные амплитуды, ни совпадение eigenplanes не позволяют этому выражению исчезнуть при $\mathcal D\ne0$. При ненулевом коэффициенте пропорциональности заявленные объяснения классов (ii)–(iv) несовместимы с собственной формулой sketch. [Proof sketch на PDF 378](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#pdf-0378).

Для произвольного task loss настоящий adjoint имеет вид $G_M=X^\top G_LX$ с $G_L=\partial\mathcal L/\partial L$ и соответствующим score scaling; далее берётся $P_a(G_M)$. $G_L$ не равен самой $\mathcal D$ в общем случае. Чтобы вывести фиксированные точки, надо анализировать именно этот loss-dependent градиент. Даже $G_a=0$ означает только неподвижность $M_a$ при принятой модели flow; это ещё не устойчивость, притяжение или неподвижность остальных параметров. [Теорема 10.7](../source/0347-0350-ch10-softmax-adjoint-and-two-flow.md#theorem-10-7), [Proposition 11.7](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#proposition-11-7).

## Диаграмма и текст также расходятся

Figure 11.11 для класса (ii) пишет **$K\ge1$**, multiple equal $\lambda_k$ и $\gamma_h\to1$; Proposition 11.14 пишет **$K=1$** и произвольное конечное $\gamma^*>0$. Для класса (iv) диаграмма явно пишет $\gamma_h>1$, тогда как Remark 11.15 утверждает, что это возможно только для класса (iii). Все эти исходные надписи сохранены как изображение и поисковый текст. [Figure 11.11](../source/0379-0384-ch11-gap-opening-and-symmetry-breaking.md#figure-11-11), [Remark 11.15](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#remark-11-15).

Само $\gamma>1$ означает рост нормированного отношения относительно инициализации; оно возможно и при нескольких плоскостях, например при уменьшении $M_s$ с фиксированным многоплоскостным $M_a$. Алгебраическое число плоскостей также не доказывает исключительность класса (iii) для amplification. Нужны отдельные условия на входы, задачу и динамику, а не только название класса. [Определение $\gamma$](../source/0373-0378-ch11-layer-observables-and-symplectic-attractors.md#definition-11-4).

Практически полезно сохранять измеряемые $K,K'$, амплитуды блоков, выбранный gap, $\Gamma$, $\gamma$ и величину градиента. Присваивать им доказанный attractor label по этой proposition пока нельзя.

