---
title: "Chapter 16: spectral pairing, level-two pruning, and warm-start stabilisation"
source_pdf_pages: [643, 646]
book_pages: [607, 610]
batch: "013"
language: en
type: source-transcription
status: complete-for-page-range
continues_from_previous_file: true
continues_after_file: true
---

# Chapter 16: spectral pairing, level-two pruning, and warm-start stabilisation

[Library index](../README.md) · [Source policy](../SOURCE.md) · [Previous: signed matrices and pairing](0639-0642-ch16-signed-residual-matrix-and-spectral-pairing.md) · [Next: architecture initialisation and rank adaptation](0647-0650-ch16-architecture-initialisation-and-rank-adaptive-deflation.md)

> Editorial note: Full transcription of PDF 643–646 (printed pages 607–610). The original eigenvector argument after (16.76), the name “Polyak–Ruppert” for (16.78), the claimed reduction in Proposition 16.50, and all original conjecture/status tags are preserved. Separate notes identify the algebraic sign error, fixed-gain averaging issue, omitted $\varepsilon_\rho$ term, and limits of the one-seed experiment. Table 16.9 records the level-one rule, not a level-two validation. The six-step procedure introduced at the end begins on PDF 647.

<a id="pdf-0643"></a>
## PDF 643 · Book page 607

*approximately decomposes as a collection of paired eigenvalues $(+\lambda_i,-\lambda_i)$, with a small mean pairing error and a worst-case normalised discrepancy ranging from approximately 3.7% to 17.5% in the four diagnostic configurations considered here. The pairing error bound $\Delta_i^{(h)}\le\Delta_{\max}$ on the dominant magnitudes depends on the training setup; quantitative tightening of $\Delta_{\max}$ in the warm-started regime is part of the empirical agenda of Volume II.*

The conjecture is empirical: tracelessness alone does not force the pairing. An explicit counterexample is given by the diagonal symmetric matrix

<a id="display-0643-traceless-unpaired-counterexample"></a>
$$
A_{\mathrm{ce}}=\operatorname{diag}(2,1,-3)\in\mathbb R^{3\times3},
$$

which is symmetric and has $\operatorname{tr}(A_{\mathrm{ce}})=0$ but whose spectrum $\{2,1,-3\}$ is not paired: no entry is the negative of another. Hence the implication “trace zero $\Rightarrow$ paired spectrum” is false in general, and the pairing observed empirically across the four configurations above must be due to additional structure specific to the trained operators.

**A sufficient structural condition.** A condition under which the spectrum of $A_h^{\mathrm{sig}}$ is paired exactly is the existence of an orthogonal involution $J\in O(d)$ with $J^2=I$ and

<a id="equation-16-76"></a>
$$
JA_h^{\mathrm{sig}}J^\top=-A_h^{\mathrm{sig}}.
\tag{16.76}
$$

Under (16.76), every eigenvector $v$ of $A_h^{\mathrm{sig}}$ with eigenvalue $\lambda$ produces an eigenvector $Jv$ with eigenvalue $-\lambda$, since $A_h^{\mathrm{sig}}(Jv)=JA_h^{\mathrm{sig}}J^\top(Jv)=-J(\lambda v)=-\lambda(Jv)$ using $JJ^\top=I$. The conditions on $(\Sigma_X,K_h)$ under which such a $J$ exists are part of the open questions of this section, and a partial analysis is deferred to Volume II.

<a id="section-16-13-3"></a>
### 16.13.3 Signed spectral pruning: the level-two rule in matrix form

INCRT I introduced two diagnostic levels for the operational protection rule of the original INCRT formulation: the level-zero rule $\gamma_h>\phi_g$, which is sign-asymmetric; and the level-one rule $|\gamma_h|>\phi_g$, which is its minimal symmetric correction. Both rules are scalar. The matrix framework of the present section admits a strictly finer diagnostic, the *level-two rule*, which is the operational form of the four-channel signed gate of Section 16.12.

<a id="definition-16-49"></a>
**Definition 16.49 (Level-two diagnostic rule).** [NEW][DEF] For every head $h$ and every diagnostic time $t$, let $(u_h^+(t),u_h^-(t))$ denote a leading positive–negative eigenvector pair of $A_h^{\mathrm{sig}}(t)$, with corresponding eigenvalues $\lambda_h^+(t)>0$ and $\lambda_h^-(t)<0$, normalised so that $\|u_h^\pm\|=1$. Define the *signed-mode amplitude* of head $h$ by

<a id="equation-16-77"></a>
$$
\rho_h(t)=\tfrac12\bigl(\lambda_h^+(t)-\lambda_h^-(t)\bigr)\ge0,
\tag{16.77}
$$

its Polyak–Ruppert moving average by

<a id="equation-16-78"></a>
$$
\bar\rho_h(t)=(1-\beta)\,\bar\rho_h(t-1)+\beta\,\rho_h(t),
\qquad\beta\in(0,1],
\tag{16.78}
$$

and the *level-two score*

<a id="equation-16-79"></a>
$$
S_h(t)=\frac{\bar\rho_h(t)}{\rho\bigl(A_h^{\mathrm{sig}}(t)\bigr)+\varepsilon_\rho}
\bigl(1-R_h(t)\bigr)\,Q_h(t),
\tag{16.79}
$$

where $\rho(A_h^{\mathrm{sig}}(t))=\max_i|\lambda_i^{(h)}(t)|$ is the spectral radius of $A_h^{\mathrm{sig}}(t)$, $R_h(t)$ is the redundancy index of head $h$ against its competitors as defined in Section 16.12, $Q_h(t)$ is a head-quality

<a id="pdf-0644"></a>
## PDF 644 · Book page 608

proxy computed from validation, and $\varepsilon_\rho>0$ is a small numerical constant. The *level-two protection rule* is

<a id="display-0644-level-two-protection-window"></a>
$$
\operatorname{protect}_h(t)\iff S_h(t)>\sigma_g
\quad\text{for at least }q\text{ consecutive control windows},
$$

for a scalar threshold $\sigma_g$. Equivalently, head $h$ is flagged for pruning only when $S_h(t)\le\sigma_g$ over the required window and the marginality and crowding conditions of the pruning and preservation rule of INCRT I are also satisfied.

The level-two rule depends on three quantities that the level-one rule does not access: the signed-mode amplitude $\bar\rho_h$, which couples the head response to the spread $\lambda_h^+-\lambda_h^-$ of the dominant signed pair rather than to a single scalar magnitude; the spectral radius $\rho(A_h^{\mathrm{sig}})$, which normalises the score by the maximal signed response of the head; and the redundancy index $R_h$, which couples each head to the collective configuration of the other heads. The signed eigenvector pair $(u_h^+,u_h^-)$ is recorded together with the score, and is the natural initialisation for the four-channel gate of Section 16.12.2. The rule retains the marginality and crowding tests of INCRT I unchanged. The level-two rule strictly refines the information used by the level-one rule; this is a refinement of the diagnostic input, not a dominance theorem on the protected set.

<a id="proposition-16-50"></a>
**Proposition 16.50 (Compatibility of level-two and level-one diagnostics).** [T-PROVED] *Under Definition 16.49, with $R_h\equiv0$, $Q_h\equiv1$, and $\rho(A_h^{\mathrm{sig}})\equiv1$, the level-two protection rule reduces to $\operatorname{protect}_h(t)\iff\bar\rho_h(t)>\sigma_g$, a Polyak–Ruppert average inequality on a non-negative scalar diagnostic. In particular, every head protected by the level-two rule under these specialisations satisfies the symmetric inequality of the level-one rule of INCRT I with $\phi_g=\sigma_g$ and with $\bar\rho_h$ playing the role of $|\gamma_h|$. This is a compatibility statement, not a dominance theorem: the level-two rule on the general score $S_h$ is not in general comparable, as a set inclusion of protected heads, with the level-one rule.*

*Proof.* Direct substitution in (16.79), using $\bar\rho_h\ge0$ from (16.77) and (16.78). $\square$

The proposition has no independent content beyond compatibility: its role is to certify that the level-two rule does not contradict the level-one rule it refines, which is necessary for the bridge between INCRT I and the present chapter to be self-consistent.

**Connection to the four-channel gate.** The four eigenvectors of the bilateral gate of Section 16.12.2—the captured and uncaptured directions on each sign class, $(u_+^+,u_-^+,u_+^-,u_-^-)$—are the natural inputs of the level-two rule when applied class-by-class. The score $S_h(t)$ specialises to a class-specific score $S_h^\pm(t)$ on the positive and negative classes, and the protection threshold $\sigma_g$ splits into $\sigma_g^\pm$, with $\sigma_g^+\approx\sigma_g^-$ in regimes where the class-balance ratio of Corollary 16.45 is close to one.

<a id="section-16-13-4"></a>
### 16.13.4 Signed spectral stabilisation on a warm-started backbone

The framework so far has been algebraic and operational. The single empirical regime that has been documented at length in this section, and that motivates the architecture-initialisation extension of Section 16.13.5, is the regime of a warm-started backbone on a natural-language classification task. We refer to it as the *stabilisation regime* of the dynamic controller, and denote it Regime II following the terminology of INCRT I.

<a id="pdf-0645"></a>
## PDF 645 · Book page 609

**Setup.** The MNLI-50k task was run with a two-layer Q/K-split attention block on a backbone warm-started with masked language modelling. The bidirectional embedding (BD) was used at $\theta=0.40$ with seed 42. Four values of the initial head count, $K_{\mathrm{init}}\in\{1,2,4,8\}$, were swept; the level-one rule of INCRT I was used as the protection rule. The diagnostic state was logged at the end of training, including the number of accepted growth events $n_{\mathrm{growth}}$, the number of accepted pruning events $n_{\mathrm{pruning}}$, the observed head count $K_{\mathrm{obs}}$, the validation accuracy, and the initial residual energy $\Gamma_0$.

**Result.** The table reports the sweep.

<a id="table-16-9"></a>
| $K_{\mathrm{init}}$ | $K_{\mathrm{obs}}$ | $n_{\mathrm{growth}}$ | $n_{\mathrm{pruning}}$ | Accuracy | $\Gamma_0$ |
|---:|---:|---:|---:|---:|---:|
| 1 | 1 | 0 | 0 | 0.4413 | 3628.31 |
| 2 | 2 | 0 | 0 | 0.4561 | 2446.91 |
| 4 | 4 | 0 | 0 | 0.4692 | 1788.60 |
| 8 | 5 | 0 | 3 | 0.4585 | 1508.88 |

**Table 16.9:** Sweep of the initial head count on MNLI-50k with bidirectional embedding at $\theta=0.40$, seed 42, level-one protection rule.

*The controller did not grow in any configuration ($n_{\mathrm{growth}}=0$); in the $K_{\mathrm{init}}=8$ configuration, three heads were pruned by the level-one rule, yielding an observed count $K_{\mathrm{obs}}=5$, which is consistent with the compressed-sensing prediction $K_{\mathrm{pred}}^*\approx5.7$ obtained by plugging the empirical $\Gamma_0$ and a calibrated $\kappa_A$ into the total-count formula of Section 16.3.*

This sweep falsifies, for the warm-started regime under consideration, the interpretation of the present implementation of INCRT as a self-expanding controller. Starting from $K_{\mathrm{init}}=1$ or $K_{\mathrm{init}}=2$, the controller does not grow toward the empirically best value $K_{\mathrm{init}}=4$. Conversely, when over-initialised at $K_{\mathrm{init}}=8$, the controller prunes to $K_{\mathrm{obs}}=5$. The observed dynamics is therefore one-sided: retention and pruning, but not bidirectional adaptation, on the warm-started backbone. Three observations follow from the sweep.

<a id="conjecture-16-51"></a>
**Conjecture 16.51 (No-growth in the stabilisation regime).** [CONJ] *For a warm-started backbone on a representative natural-language classification task, the INCRT controller in the protection-only regime does not trigger any growth event ($n_{\mathrm{growth}}=0$) when initialised at any $K_{\mathrm{init}}$ in the range studied. The diagnostic state on which the growth trigger of INCRT I is built does not satisfy the residual-amplification threshold $\Gamma_{\mathrm{res}}>\theta_w$ during the warm-started trajectory.*

<a id="conjecture-16-52"></a>
**Conjecture 16.52 (Upper-selection at $K_{\mathrm{init}}=8$).** [CONJ] *For the configuration $K_{\mathrm{init}}=8$ of Table 16.9, the controller prunes from the top, reducing the observed count to $K_{\mathrm{obs}}=5\approx K_{\mathrm{pred}}^*=5.7$. The mechanism is selection from above by the level-one rule, in agreement with the compressed-sensing prediction of Section 16.3: heads whose response magnitude falls below the protection threshold are flagged and pruned, and the surviving count converges to the compressed-sensing target.*

<a id="conjecture-16-53"></a>
**Conjecture 16.53 (Asymmetric attractor).** [CONJ] *The fixed point $K_{\mathrm{obs}}=K_{\mathrm{pred}}^*$ of the warm-started controller is approached asymmetrically: from above by pruning and not from below by growth. In the stabilisation regime, the controller has access to a deactivation mechanism but not to an activation mechanism on the warm-started backbone, and the architecture is therefore selected by selection from above on an over-parametrised initialisation rather than by selection from below on an under-parametrised initialisation.*

<a id="pdf-0646"></a>
## PDF 646 · Book page 610

The three conjectures are concurrent, and a complete explanation requires a separate convergence argument for the warm-started flow that we do not give here. Their compatibility with the compressed-sensing framework of Section 16.3 is operational: the target $K_{\mathrm{pred}}^*\approx5.7$ is the standard prediction of the total-count formula, and the observed approach from above is consistent with the deflation interpretation of the compressed-sensing argument run in reverse. A complete treatment, including the conditions under which the asymmetric-attractor behaviour is robust, is deferred to Volume II.

**Why the operator $A_h^{\mathrm{sig}}$ matters for the regime.** The level-one rule is sufficient to explain Conjectures 16.51–16.53 operationally: the warm-started backbone produces head responses whose magnitudes are clustered around the protection threshold, and the symmetric inequality $|\gamma_h|>\phi_g$ classifies correctly the heads to retain and the heads to remove. What the level-one rule does not explain is why the spectrum of $A_h^{\mathrm{sig}}$ should have very few dominant signed modes on the warm-started trajectory in the first place. The empirical pairing of Conjecture 16.48 suggests an answer: the spectrum is approximately paired in this regime, so that the $K_{\mathrm{pred}}^*\approx5.7$ target counts pairs of signed modes, and the level-two rule of Section 16.13.3 would naturally protect pairs rather than individual heads. Verifying this prediction directly requires matched empirical access to $(u_h^+,u_h^-)$ for every head in the sweep, which is part of the empirical agenda of Volume II.

<a id="section-16-13-5"></a>
### 16.13.5 INCRT as architecture initialisation: the third regime

The dynamic controller of INCRT I is run *during* training: the diagnostic state is computed on a moving window of validation batches, growth and protection events are accepted only when the estimates are stable, and termination is reached when the four tolerances of finite termination hold simultaneously. This is the natural mode of operation, and is the one assumed by every theorem of this chapter so far.

The matrix framework of the present section opens a different mode. The head-level signed operator $A_h^{\mathrm{sig}}$ is defined directly from the weights $W_q^{(h)},W_k^{(h)}$ and the empirical input covariance $\Sigma_X$; it does not require training to be in progress. In particular, $A_h^{\mathrm{sig}}$ can be evaluated on a candidate untrained backbone at initialisation time, and its spectrum analysed before any gradient step has been taken. The architecture produced by this analysis is then fixed for the subsequent training run.

<a id="definition-16-54"></a>
**Definition 16.54 (Architecture-initialisation use of INCRT; Regime III).** [NEW][OUTLOOK] The *architecture-initialisation regime* of INCRT consists of running the dynamic controller of INCRT I as a spectral pre-pass on the head-level signed operators $\{A_h^{\mathrm{sig}}\}_h$ of a candidate backbone, prior to training, in order to output a fixed architecture $(L^\star,\{K_\ell^\star\},\{(u_h^+,u_h^-)\}_h)$ that is then frozen for training. The pre-pass uses the level-two rule of Definition 16.49 as its protection and pruning rule, and the four-channel gate of Section 16.12 as its growth-event template.

The architecture-initialisation regime is the third operational specialisation of the controller, after the expansion regime of INCRT I (Regime I) and the stabilisation regime of Section 16.13.4 (Regime II) above.

<a id="algorithm-0646-architecture-initialisation"></a>
**The architecture-initialisation procedure.** The procedure that produces the architecture $(L^\star,\{K_\ell^\star\},\{(u_h^+,u_h^-)\}_h)$ from a candidate backbone at initialisation time, on an input distribution sampled by $\Sigma_X$, has six steps.

> Boundary: The six steps of this unnumbered procedure follow on [PDF 647](0647-0650-ch16-architecture-initialisation-and-rank-adaptive-deflation.md#pdf-0647).
