---
title: "273. ICL sample complexity: статистическая ошибка не делится автоматически на глубину"
source_pdf_pages: [670, 671]
book_pages: [634, 635]
batch: "014"
language: ru
type: reading-note
status: source-linked-editorial-analysis
---

# 273. ICL sample complexity: статистическая ошибка не делится автоматически на глубину

[Индекс библиотеки](../README.md) · [Источник и политика](../SOURCE.md)

## Вопрос

Выводит ли proof Theorem 16.78 закон n = Θ(κ² log(1/ε)/T)?

## Два разных ограничения в одном proof sketch

[Theorem 16.78](../source/0671-0674-ch16-in-context-sample-complexity-and-test-time-adaptation.md#theorem-16-78) утверждает $n_{\rm ICL}=\Theta(\kappa_{A,\rm ICL}^2\log(1/\varepsilon)/T)$. Однако её proof сначала получает условие на число шагов

$$
T\ge\frac{\kappa_{A,\rm ICL}^2}{4\eta}\log\frac1\varepsilon,
$$

а затем условие на число **разных демонстраций**

$$
n\ge\frac{\Gamma_0^2}{\varepsilon^2\delta_0^2}
=\frac{\kappa_A^2}{\varepsilon^2}.
$$

Второе не исчезает при увеличении $T$. Предыдущая строка является ограничением на $T$, а не вторым числом samples, которое можно объединить максимумом с первым. В (16.121) нет ни операции max, ни члена $1/\varepsilon^2$, хотя proof называет итог «the displayed maximum of the two requirements». Переход к знаменателю $T$ не выведен.

Отдельно: верхняя граница variance может дать **достаточный** sample size для выбранного estimator и метрики; сама по себе она не доказывает необходимый нижний bound и тем более $\Theta$ с двумя сторонами. Для вероятностной гарантии ещё нужны confidence level, tail assumptions, тип нормы и размерностная зависимость. [Исходный proof](../source/0671-0674-ch16-in-context-sample-complexity-and-test-time-adaptation.md#pdf-0671).

## Простой noiseless пример

Пусть $x\in\{0,1\}$ с вероятностями $1/2,1/2$, метка детерминирована как $y=x$. Тогда по определению книги $Z=yx^2=x$, $B=1/2$, а $\widehat B$ — частота единиц. Метки имеют ненулевую variance $\sigma_y^2=1/4$, но label noise отсутствует: $\sigma_{\rm noise}^2=0$.

Точно:

$$
\mathbb E(\widehat B-B)^2=\frac1{4n},\qquad
\frac{\mathbb E(\widehat B-B)^2}{B^2}=\frac1n.
$$

Для relative RMS precision $\varepsilon$ этот estimator требует $n\ge1/\varepsilon^2$. Между тем (16.120) даёт $\kappa_{A,\rm ICL}^2=0$: (16.121) вырождается в нулевую sample scale, а фактор contraction $1-4\eta/\kappa_{A,\rm ICL}^2$ вообще не определён. Нулевой label noise не устраняет случайность входов. Этот пример удовлетворяет PSD population premise и условию noise-to-signal $\le c$; чтобы исключить его, нужны дополнительные условия, отсутствующие в формулировке. [Определение индекса](../source/0668-0670-ch16-deployment-frontier-and-implicit-task-operator.md#equation-16-120).

## Что может дать глубина

Полезно разложить ошибку алгоритмического выхода $\widetilde B_{n,T}$:

$$
\|\widetilde B_{n,T}-B\|
\le\|\widetilde B_{n,T}-\widehat B_n\|
+\|\widehat B_n-B\|.
$$

Первый член может уменьшаться с числом стабильных оптимизационных шагов при заданной геометрии objective. Второй описывает статистическую неопределённость конечного prompt. Его нельзя автоматически уменьшить повторной обработкой тех же наблюдений. Хороший prior, shrinkage или знание класса задач могут менять statistical estimator, но должны входить в постановку явно.

Даже геометрическое contraction требует диапазона шага, начального состояния, фиксированного или контролируемо меняющегося оператора и заданного preconditioner. При $0<4\eta/\kappa^2<1$ экспонента является обычной оценкой сверху, а не доказательством точного равенства скоростей.

Поэтому фраза «doubling the number of layers halves the sample complexity» не подтверждена данным proof. Для проекта следует отдельно измерять ошибку оценки задачи от длины контекста, ошибку вычисления этой оценки от глубины и конечную query loss. Проверка здесь алгебраическая; новые модели не обучались.
