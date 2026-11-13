---
title: "Chapter 17: signed-score conditioning, bilinear-routing bridge, and the numerical floor"
type: source-transcription
language: en
batch: "015"
source_pdf_pages: [718, 721]
book_pages: [682, 685]
---

# Chapter 17: signed-score conditioning, bilinear-routing bridge, and the numerical floor

[Library index](../README.md) · [Source and transcription policy](../SOURCE.md)

[Previous source](0714-0717-ch17-empirical-diagnostics-and-signed-gates.md#pdf-0717) · [Following source](0722-0725-ch17-adaptive-dispatch-and-optimiser-aware-gates.md#pdf-0722)

<a id="pdf-0718"></a>
## PDF 718 · Book page 682

**Alignment drift of the signed pair.** Let $A_h^{\rm sig}(t)$ be perturbed by a numerical error $E(t)=\widetilde A_h^{\rm sig}(t)-A_h^{\rm sig}(t)$ of spectral norm $\|E(t)\|_2\le\varepsilon_{\rm mach}\|A_h^{\rm sig}(t)\|$, arising from finite-precision arithmetic in the formation of $\Sigma_X$ and of $K_h$ and in the symmetrisation step. By the Davis–Kahan theorem applied to the signed eigendirections, the angular drift of the leading positive eigenvector $u_h^+(t)$ relative to its true counterpart satisfies

<a id="equation-17-53"></a>
$$
\sin\theta\bigl(u_h^+(t),\widetilde u_h^+(t)\bigr)
\le\frac{\|E(t)\|_2}{g_h^+(t)},
\qquad g_h^+(t)=\lambda_h^+(t)-\lambda_h^{++}(t),
\tag{17.53}
$$

where $\lambda_h^{++}(t)$ is the second-largest positive eigenvalue of $A_h^{\rm sig}(t)$. An analogous bound holds for the leading negative eigenvector with $g_h^-(t)=|\lambda_h^-(t)|-|\lambda_h^{--}(t)|$ the corresponding internal gap on the negative class. The drift bound of the level-two rule therefore decomposes into two class-specific Davis–Kahan factors, neither of which is the signed gap $\Delta_h^\pm$ of (17.50): the latter governs the existence of a stable signed pair, while $g_h^\pm$ govern the angular sensitivity within each class. In the warm-started stabilisation regime of Chapter 16, the empirical pairing reduces $g_h^+\approx g_h^-$ and the two factors are comparable; in the expansion regime of Chapter 15 they can be of distinct magnitudes, and the unified gate dispatches Type-A or Type-B independently on the two classes.

The accumulated alignment drift over a control window of length $w$ satisfies the standard sum-bound $\sum_{s=t-w+1}^t\sin\theta(u_h^+(s),\widetilde u_h^+(s))\le w\,\|E\|_2/g_h^+$, so the Polyak–Ruppert average $\bar\rho_h(t)$ inherits a relative error of order $\varepsilon_{\rm mach}w/\min(g_h^+,g_h^-)$. The window length $w$ must therefore satisfy $w\ll\min(g_h^+,g_h^-)/\varepsilon_{\rm mach}$, a condition that is automatic in the regular regime and that motivates the Type-B branch of the unified gate when violated.

**Class leakage.** Even when the signed gap $\Delta_h^\pm$ and the class-internal gaps $g_h^\pm$ are bounded away from zero, the absence of a structural involution $J\Sigma_XJ^\top=\Sigma_X$, $JK_hJ^\top=-K_h$ (open question of Chapter 16) admits a residual class leakage. We define the leakage with respect to the true sign-class projectors of $A_h^{\rm sig}(t)$, not the sign-class projectors of the computed $\widetilde A_h^{\rm sig}(t)$ (the latter would be zero by orthogonality of the eigenvectors of a symmetric matrix to itself):

<a id="equation-17-54"></a>
$$
\ell_h^\pm(t)=\bigl\|P_{\rm true}^\mp(t)\,\widetilde u_h^\pm(t)\bigr\|_2,
\qquad
P_{\rm true}^\mp(t)=\sum_{i:\,\operatorname{sign}(\lambda_i(t))=\mp}u_i(t)\,u_i(t)^\top,
\tag{17.54}
$$

the projection of the computed positive (negative) eigenvector $\widetilde u_h^\pm$ of $\widetilde A_h^{\rm sig}$ onto the opposite-class eigenspace of the unperturbed $A_h^{\rm sig}$. By the Davis–Kahan theorem applied class by class, the leakage is bounded by $\ell_h^\pm\le\|E\|_2/\Delta_h^\pm$, with $\Delta_h^\pm$ of (17.50), so the leakage is bounded by $\varepsilon_{\rm mach}\|A_h^{\rm sig}\|/\Delta_h^\pm$ in the regular regime. Two consequences for the level-two score $S_h(t)$ follow.

First, the signed-mode amplitude $\bar\rho_h$ inherits a leakage bias: the computed $\widetilde\rho_h=\tfrac12(\widetilde\lambda_h^+-\widetilde\lambda_h^-)$ can over-estimate or under-estimate the true $\rho_h$ by a relative amount of order $\varepsilon_{\rm mach}\|A_h^{\rm sig}\|/\Delta_h^\pm$. The bias is one-sided in any single batch but averages out on the Polyak–Ruppert window provided the perturbation is not systematic.

Second, the four-channel gate of §16.13 inherits an inter-class coupling that the symmetric analysis of the bilateral motor cannot detect: when the regularisation on the positive class is tuned, the leakage causes a small but systematic update on the negative class, and vice versa. The unified gate of §17.5 requires $\varepsilon_{\rm reg}\ge\sqrt{\varepsilon_{\rm mach}}\,\|A_h^{\rm sig}\|$ as the default regularisation amplitude; under this calibration the leakage bias is dominated by the regularisation bias and the four-channel gate decouples up to a controlled cross-class term.

<a id="pdf-0719"></a>
## PDF 719 · Book page 683

**Numerical condition of the score $S_h$.** The condition number of the level-two score, viewed as a function of the four operands $(\bar\rho_h,\rho(A_h^{\rm sig}),R_h,Q_h)$, decomposes multiplicatively. The factor $\rho(A_h^{\rm sig})$ is the spectral radius, which is well-conditioned with $\operatorname{cond}\approx1$ on the dominant pair; the redundancy index $R_h$ and the quality proxy $Q_h$ are bounded validation-time quantities with $\operatorname{cond}$ of order 1; the dominant contribution is from $\bar\rho_h$, with $\operatorname{cond}(\bar\rho_h)\sim1/\Delta_h^\pm$. Combining with the small-$\varepsilon_\rho$ floor of the denominator,

<a id="equation-17-55"></a>
$$
\operatorname{cond}\bigl(S_h(t)\bigr)
\lesssim\frac{\|A_h^{\rm sig}(t)\|}{\Delta_h^\pm(t)\cdot\bigl(\rho(A_h^{\rm sig}(t))+\varepsilon_\rho\bigr)}
\sim\frac1{\Delta_h^\pm(t)}\cdot\frac{\|A_h^{\rm sig}(t)\|}{\rho(A_h^{\rm sig}(t))+\varepsilon_\rho},
\tag{17.55}
$$

so the score is well-conditioned exactly when the signed gap is bounded away from zero on the same scale as the spectral radius. Choosing $\varepsilon_\rho\sim\sqrt{\varepsilon_{\rm mach}}\,\|A_h^{\rm sig}\|$, in analogy with the regularisation amplitude of §17.5, balances the numerator and the denominator contributions and keeps $\operatorname{cond}(S_h)$ bounded uniformly across the four configurations of Chapter 16.

**Operational summary.** The level-two protection rule of Chapter 16 is a mathematical object; computing it in finite arithmetic inherits the regime structure of the present chapter. In the regular regime $\Delta_h^\pm\gg\varepsilon_{\rm mach}\|A_h^{\rm sig}\|$, $S_h(t)$ is computed directly from its definition. In a Type-A or Type-B regime on one of the two classes, the unified gate of §17.5 is applied to that class to stabilise the extraction of $\widetilde u_h^\pm$ before the score is computed. In a Type-C regime on one or both classes, the fallback of Definition 17.15 routes the controller to the level-one rule of Chapter 15, or excludes the head from the protection set altogether. The level-two rule is therefore numerically stable in the same domain in which the unified gate is operationally well-defined, and degrades gracefully to the level-one and level-zero rules in the pathological regimes.

<a id="section-17-6-6"></a>
### 17.6.6 Bridge to Multi-Head Bilinear Gating

Chapter 19 introduces the Multi-Head Bilinear Gating (MHB-G) operator, which replaces the standard self-attention block with a bilinear multi-head construction. Two specific features of MHB-G connect to the unified gate of this chapter.

**Routing top-$k$ as spectral selection.** The MHB-G layer routes each query token to its top-$k$ key tokens via the bilinear scoring function $s_{ij}=x_i^\top W_sx_j/\sqrt d$, with the top-$k$ entries normalised by softmax. From a spectral perspective, the routing is a data-dependent allocation of attention weight along the dominant eigendirections of the matrix $W_s$ acting on the input pairs. The unified gate of Algorithm 7 provides exactly this type of allocation: a regime-dispatched extraction of the dominant $m$-eigenspace of a residual operator.

When the routing matrix $W_s$ is in a Type-B regime (near-degenerate top-$k$ scores, common in early training when token representations are not yet differentiated), the unified gate’s regularised block branch ensures stable subspace recovery. The Chapter 15 gate, in the same regime, would either oscillate between top-$k$ choices or saturate at the angular barrier of §17.6.3, producing erratic routing patterns.

**Rank stability of MHB-G.** Lemma ?? of Chapter 19 establishes that MHB-G with no residual connection preserves rank $\ge110$ for the first 4 layers, then collapses in deeper layers. The collapse is empirically observed to occur on samples where the routing matrix $W_s$ develops near-degenerate eigenvalues during training, exactly the Type-B regime. This is consistent with the framework of §17.6.3: in Type B, single-step methods (one forward pass through MHB-G without residual smoothing) cannot recover the dominant subspace, while

<a id="pdf-0720"></a>
## PDF 720 · Book page 684

methods with subspace-level memory (i.e., the residual connection accumulating updates over training steps) can.

The bridge is therefore: *the unified gate is the algorithmic analogue, at training time, of the residual-stabilised forward pass of MHB-G at inference time*. Both mechanisms address the same underlying phenomenon — subspace recovery in near-degenerate spectra — by appealing to block-level (rather than vector-level) geometric objects. This connection is not a coincidence but a structural consequence of the spectral nature of attention itself, established throughout this volume, and reaches its prescriptive form in MHB-G.

**Closing remark.** Chapter 15 introduced the three-level INCRT as a self-architecting algorithm whose gate dynamics determine the architecture from the data. The present chapter has refined the gate into a numerically robust form that dispatches between three spectral regimes, recovering the Chapter 15 algorithm in the regular regime and extending coverage to near-degeneracy and rank deficiency. Chapter 18 will validate the framework empirically; Chapter 19 will translate the spectral lessons into a prescriptive architecture. The unified gate of this chapter is the algorithmic lynchpin connecting the three.

<a id="section-17-7"></a>
## 17.7 Towards an optimiser-aware spectral gate

The preceding sections isolate three numerical regimes in which the ideal bidirectional gate must be refined: exact degeneracy, near-degeneracy, and numerical kernel directions. Several questions that arise from this classification are not independent. The numerical floor of the block gate controls the reliability of the relative gap test; the gap test controls the dispatch rule; the dispatch rule is stable only when the relevant invariant subspace drifts slowly; and the drift is partly induced by the optimiser. This section records a partial resolution of this chain. The results are deliberately stated as operational bounds rather than sharp asymptotic theorems, because their role is to make the unified gate usable inside the INCRT controller of Chapter 15 and consistent with the extensions of Chapter 16.

<a id="section-17-7-1"></a>
### 17.7.1 A practical estimate of the iteration floor

The empirical floor observed in Figure 17.6 is not an anomaly of the synthetic experiment. It is the expected consequence of three independent effects: finite arithmetic, finite iteration time, and the bias introduced by regularisation. A useful operational estimate is therefore obtained by decomposing the attainable subspace error as

<a id="display-0720-informal-error-decomposition"></a>
$$
\varepsilon_{\rm floor}\simeq\varepsilon_{\rm mach}+\varepsilon_{\rm iter}(T)+\varepsilon_{\rm reg}(\rho),
$$

where $\rho$ denotes the regularisation amplitude of the block gate. The symbol $\simeq$ is used here in its informal numerical sense: the terms on the right identify the dominant contributions, not an exact identity.

<a id="proposition-17-16"></a>
**Proposition 17.16 (Practical floor estimate for the regularised block gate).** *Let $A_{\rm res}$ be symmetric with $\|A_{\rm res}\|\le M$. Let $U_\star$ be an invariant subspace separated from the remaining spectrum by an outer gap $\delta_{\rm out}>0$. Assume that the regularised Block MCA–EXIN iteration is run with step size $\eta$, regularisation amplitude $\rho$, and $T$ effective iterations. Then the attainable subspace error admits the operational estimate*

<a id="display-0720-practical-subspace-error-estimate"></a>
$$
\operatorname{dist}(\widehat U_T,U_\star)
\lesssim C_1\kappa(U_\star)\varepsilon_{\rm mach}
+C_2\exp(-c\eta\delta_{\rm out}T)+C_3\rho.
$$

<a id="pdf-0721"></a>
## PDF 721 · Book page 685

*Consequently, increasing $T$ cannot reduce the error below the finite-precision and regularisation floor*

<a id="display-0721-claimed-iteration-floor"></a>
$$
\operatorname{dist}_{\rm floor}\lesssim C_1\kappa(U_\star)\varepsilon_{\rm mach}+C_3\rho.
$$

*The constants depend on the spectral norm bound, the block dimension, the normalisation convention, and the conditioning of the chosen coordinates.*

*Proof sketch.* The first term is the standard amplification of floating-point error by the conditioning of the computed invariant subspace. The second term is the contraction term inherited from the Stiefel-flow interpretation of the block iteration; the contraction rate is controlled by the external gap $\delta_{\rm out}$. The third term is the bias term already isolated in Lemma 17.10. Combining the three contributions yields the stated estimate. The result is intended as an a-priori scale estimate; a sharp non-asymptotic discretisation bound would require an independent analysis of the stochastic Stiefel flow. $\square$

The notation $\lesssim$ means “bounded above up to a harmless constant”. Thus $a\lesssim b$ abbreviates $a\le Cb$ for a constant $C>0$ that is not the focus of the argument. When the dependence on a parameter matters, it is written explicitly.

<a id="section-17-7-2"></a>
### 17.7.2 A floor-adaptive near-degeneracy threshold

The fixed value $\gamma_{\rm near}=0.05$ in Definition 17.6 is a robust default, but it is not intrinsic. A gap should be declared meaningful only when it is larger than the current numerical floor. This observation turns the floor estimate above into an adaptive dispatch rule.

<a id="definition-17-17"></a>
**Definition 17.17 (Floor-adaptive near-degeneracy threshold).** Given an online estimate $\widehat\varepsilon_{\rm floor}(t)$, define

<a id="display-0721-floor-adaptive-near-degeneracy-threshold"></a>
$$
\gamma_{\rm near}(t)=\max\left\{\gamma_{\min},\tau_\gamma\frac{\widehat\varepsilon_{\rm floor}(t)}{\widehat\lambda_1(t)+\epsilon}\right\},
$$

where $\gamma_{\min}>0$ is a conservative lower bound, $\tau_\gamma>1$ is a safety factor, and $\epsilon>0$ prevents division by a numerically vanishing dominant eigenvalue. A relative gap is treated as numerically meaningful only if it exceeds this adaptive threshold.

The fixed threshold of Definition 17.6 is recovered by taking $\widehat\varepsilon_{\rm floor}(t)o0$ and choosing $\gamma_{\min}=0.05$. In finite arithmetic, the adaptive version is more consistent with the Type-B/Type-C distinction: a small gap should not be trusted when it lies at the same scale as the subspace floor.

<a id="section-17-7-3"></a>
### 17.7.3 Slow drift of invariant subspaces

In the stationary analysis of §17.5, the residual operator is fixed during a growth event. In the full INCRT loop, however, $A_{\rm res}(t)$ changes with training. The relevant question is whether the target eigenspace moves slowly enough to be tracked from one dispatch event to the next. The appropriate scale is the ratio between operator movement and external spectral gap.

<a id="lemma-17-18"></a>
**Lemma 17.18 (Slow-drift condition for spectral-gate tracking).** *Let $A_t$ be a sequence of symmetric residual operators, and let $U_t$ be an invariant subspace separated from the rest of the spectrum by an outer gap $\delta_t>0$. If*

<a id="display-0721-slow-drift-hypothesis"></a>
$$
\|A_{t+1}-A_t\|\le\alpha\delta_t
$$

*for some $\alpha<1$, then*

<a id="display-0721-subspace-drift-conclusion"></a>
$$
\operatorname{dist}(U_{t+1},U_t)\lesssim\alpha.
$$

> Continuation: the concluding sentence and proof sketch of Lemma 17.18 begin on [PDF 722](0722-0725-ch17-adaptive-dispatch-and-optimiser-aware-gates.md#pdf-0722).
