# Lexting in-app tutorial (pinch / grab-first)

Replaces pop-gun flow on `TextOutputCanvas`. **One label** per step: copy **alpha off → replace text → alpha on** (~22 transitions). Tutorial prompt, **two-zone widget**, and **output field** are **visible together** from Step 3 onward (not the old “tutorial OR widget” split).

**Floating menu** = mid-air **HighlightableZoneController** mode picker (typing tutorial / speed test / freestyle). **Not shown** during this tutorial; user goes straight to live widget + output after Step 3. A single **Skip** control (3D zone below the widget) appears from Step 3 onward; **Don’t show again** is not offered in the shipped UI (PlayerPref opt-out remains available via `EndTutorialFromSkip(dontShowAgain: true)` if a scene node is authored).

**Design intent:** Selection = **index–thumb pinch** / **grab–release** (same as live typing). Demo hands: `TutorialProceduralHandPose` (open ↔ closed references).

---

## Master step table

| Step | Title (short) | Advance when | Wrong input / revert | Demo hands & chrome |
|------|-----------------|--------------|----------------------|---------------------|
| **1** | Ready position | Both palms facing each other: `dot(n_L, n_R) ≤ -0.7` held **2 s**; **reset timer** if tracking/pose lost | — | Both **open** |
| **2** | Left hand grab | After **2 s** from step start, play **left closed** demo; advance when user **matches left grab** (live `Insert`/grab) | — | Left **closed** anim; **left checkmark** on success |
| **3** | Right hand grab | **2 s** after step start, play **right closed** demo; advance on **right grab**; **summon widget + output**; show **Skip** below widget (scene nodes or runtime **SKIP** zone) | — | Right **closed**; **right checkmark** only; then **fade hands + checkmarks to 0** |
| **4** | Type `t` | `CurrentOutputValue == "t"` | Else **red ~0.25 s**, restore **step-start output** (`""`) | Hands/checkmarks **hidden** |
| **5** | Palm up (supinate) | Either hand classified **`supinate`** (same **CubeRotationState** / palm-normal mapping as the two-zone cube) | — | — |
| **6** | Type `h` | `CurrentOutputValue == "th"` | Revert to **`"t"`** (step 6 start) | — |
| **7** | Palm down (pronate) | Either hand classified **`pronate`** | — | — |
| **8** | Type `e` | `CurrentOutputValue == "the"` | Revert to **`"th"`** while still on this step | — |
| **9** | FOX / palm-forward intro | **Scripted at step start:** **2 s** after **`"the"`** → clear to **`""`** → **1 s** → **`"fo"`**; then **7 s** auto-advance (read the cue before the pose) | If output is not **`"fo"`** or a completed **`"fox"`**: red + restore **`"fo"`**. Completing **`"fox"`** here **skips steps 10–11** and goes to **Back-Word (12)** | — |
| **10** | Palm forward | Either hand classified **`forward`** (cube “toward screen” pose) | Same as step 9: wrong output → red + **`"fo"`**; **`"fox"`** → skip to **12** | — |
| **11** | Type `x` (after `fo`) | User completes **`"fox"`** from **`"fo"`** | Wrong char while output should be **`"fo"`**: red + restore **`"fo"`**. If **`"fox"`** is already present (e.g. skilled user), no penalty — advance when output matches | — |
| **12** | Back-Word | **`"fox"`** present at step entry; after step, output **empty** via Back-Word (gesture = existing **right middle → left wrist** mapping; **output-only** validation) | Any change other than clearing **fox** → reset to **`"fox"`** | — |
| **13** | Word Mode intro | **7 s** auto-advance | — | — |
| **14** | WM: start `the` | **L** holding **t**, **R** holding **h** (word-mode hold detection) | On release before valid: reset step; wrong pairing: **red + revert on release only** | — |
| **15** | WM: autofill `the` | Both release → word completes as **`the`** | Wrong on release: red + revert to step 14 start state | — |
| **16** | WM: start `tantalize` | **L** **t**, **R** **a** held | Wrong on release: red + revert; if **`ta`** correct, **may advance while still holding** | — |
| **17** | WM: `tantalize` + N | **L** dragged to **N** while word-mode hold continues (assembly toward **`tantalize`**) | Early **release** during await → reset to **step 16**; wrong letter **while still holding**: no flash (drag-through); wrong on **release**: red + revert | — |
| **18** | WM: `tantalize` + T | **R** dragged to **T** while hold continues | Same hold/release rules as 17 | — |
| **19** | WM: finalization entry | **R** still holding, **L** opens → **finalization mode** for current word stem | After **dual release**, if output already has a **conjugated** tantalize commit (not bare lemma `tantalize`) since step 19 began → **21**; early release without valid stem → reset tantalize block | — |
| **20** | Finalization | User **commits** any **accepted conjugated form of `tantalize`** (dictionary); **punctuation optional** | Wrong word on commit: red + **reset to step 16** (tantalize block) | — |
| **21** | Wrap-up | **7 s** on copy, then **freestyle**: dismiss tutorial UI; fire **`OnTutorialCompleted`**; hide Skip control; show **floating menu** if desired for mode pick | — | — |

**Dictionary:** Target word **`tantalize`** (and verb forms **`tantalizes`**, **`tantalizing`**, **`tantalized`**) is in `Lexting_Unity_Plugin/Runtime/Scripts/EnglishDictionaries/Resources/longer_words_2x2x2.json`, loaded by `LextingDictionary` for finalization. Partial stems during assembly (e.g. **`ta`**, **`tan`**, **`tant`**) are **in-progress UI state**, not standalone dictionary entries.

**Skip:** Shown from Step 3+. Move the **hand cursor** onto **SKIP** (highlight + center hover click; **grab–release** plays selection click and bump), then jump to **wrap-up (21)**—**7 s** on copy, then freestyle dismiss (same as completing the tutorial); **Success_Mid** plays on skip confirm (not again at dismiss). **Grab must start while hovering SKIP** (carried-over grabs from earlier steps do not count). **This session only** (no PlayerPrefs). While a cursor hovers SKIP, that hand’s cube hover is paused until the cursor leaves. Skip visibility widens cursor outer range so SKIP sits in reach. If no scene-authored skip nodes exist, `TutorialManager` clones the widget **CenterZone** geometry (same materials/label stack as the center cell) and places it **below the midpoint between the left and right center keys**. The label reads **SKIP** (white at rest, black on hover, like cube cells).

---

## Step copy (normalized 1–21)

### Step 1 — Ready position

**Title:** Lexting tutorial  

**Body:** Hold your hands in front of you, palms facing each other, fingers **open and relaxed**.

---

### Step 2 — Left hand grab

**Body:** **Great!** Now do the **GRAB** gesture with the **left** hand like this.

---

### Step 3 — Right hand grab

**Body:** **Great!** Now the right hand.

---

### Step 4 — Type the letter t

**Body:** Locate the letter **T** with either hand. Hover your cursor into it and **grab–release** to type it.

---

### Step 5 — Palm up (supinate)

**Body:** **Great!** Now **supinate** your wrist (turn palm upward).

---

### Step 6 — Type the letter h

**Body:** The letters changed for palm-up position. Grab **H**.

---

### Step 7 — Palm down (pronate)

**Body:** **Great!** Now **pronate** your wrist (turn palm downward).

---

### Step 8 — Type the letter e

**Body:** The letters changed again for palm-down. Find **E** and grab it.

---

### Step 9 — FOX / palm-forward intro (timed)

**Body:** **Great!** You typed a word! Now let's finish typing the word **FOX**. 

---

### Step 10 — Palm forward

**Body:** Notice the **X** is nowhere to be found in palm-sideways, palm-up, or palm-down. But try turning your palm **forward** toward the screen.

---

### Step 11 — Type the letter x (after `fo`)

**Body:** **X** and **Z** just showed up in the UI! Grab **X**.

---

### Step 12 — Back-Word

**Body:** **Great!** Now delete the word **fox** by tapping your **left wrist** with your **right middle finger**.

---

### Step 13 — Word Mode intro

**Body:** **Great job!** You’ve accessed the whole alphabet, but typing one letter at a time is slow — let’s go faster with **Word Mode**!

---

### Step 14 — Word Mode: `the` (start)

**Body:** Word Mode is **GRAB + HOLD + DRAG** with both hands. GRAB+HOLD **T** with the left and **H** with the right.

---

### Step 15 — Word Mode: `the` (autofill)

**Body:** Now **let go** with both hands — Lexting will autofill the word.

---

### Step 16 — Word Mode: `tantalize` (start)

**Body:** **Great!** Try a longer word: **tantalize**. Grab **T** with the left and **A** with the right. **Hold both.**

---

### Step 17 — Word Mode: `tantalize` (continue)

**Body:** **Great!** Keep holding. Now drag your left hand to **N**.

---

### Step 18 — Word Mode: `tantalize` (continue)

**Body:** **Great!** Now drag your right hand to **T**.

---

### Step 19 — Word Mode: open left (finalization)

**Body:** **Great!** Keep holding with the **right**, but **open the left** hand to enter finalization.

---

### Step 20 — Finalization

**Body:** **Great!** You’re in **Finalization Mode**. Pick the word **tantalize** on the left. Rotate your wrists to explore your suffix choices. Choose any suffix and punctuation, then release.

---

### Step 21 — Wrap-up

**Body:** **Amazing!** You’re ready to freestyle. Think of a sentence and type it with Word Mode.

---

## Engineering notes

- **UI pages:** **22** step indices (0–21); single shared label; **`TutorialManager`** master-fades **`TutorialGroup` CanvasGroup** to `tutorialOpacity` (default 0.4) between steps; **`TutorialPageHandler`** swaps copy only.
- **Output gating:** Match **`CurrentOutputValue`** exactly per typing step until freestyle; **no blocking** of widget mechanics — only **red flash ~0.25 s** + **restore step-start output** on error (including Back-Word that removes required prefix).
- **Rotation / posture steps (5, 7, 10):** Advance when **`Lexting.input.currentLeftRotationState`** or **`currentRightRotationState`** matches **`CubeRotationState.supinate`**, **`pronate`**, or **`forward`** (same classifier as **`OpenXRHandInput` / Quest palm normal). **No** timed transition for those poses — user must achieve them. **Exception:** step **9** (index 8) runs the **scripted `the` → clear → `fo`** sequence **at step start**, then **7 s** read-ahead before the palm-forward pose (step **10** / index **9**).
- **Word Mode (indices 13–20):** Validate **progressive** assembly per step table; advance **while still holding** when the step’s letter pairing is correct; **wrong only on release** (except early release → reset to the `tantalize` block start at index **15**); **don’t** flash on wrong letter **during** hold (drag-through).
- **Finalization:** **One** interactive commit step (index **20**); validate **any conjugated form of `tantalize`**, ignore punctuation requirement.
- **Hands:** 1 open both → 2 left closed → 3 right closed → **4+ fade out** hands/checkmarks when widget live.
- **Prefab/scene:** `TutorialHand_Left` / `TutorialHand_Right` with `TutorialProceduralHandPose` + open/closed ref children; **`TutorialPageHandler` on existing `Pages`** drives **`step1`** body text + canvas **`Title`** (step2–4 hidden); optional Skip node under `TextOutputCanvas` with `TutorialDismissControls` + `TutorialSkipGrabZone` — at step 3 **`TutorialManager`** reparents under the widget and **`TutorialSkipControlsLayout`** positions it **`skipControlsMarginBelowWidget`** (world **−Y**) below the **`TwoZoneCube`** origin (resync on **`ControllerActivated`** / **`OnWidgetRepositioned`**). Default **Skip-only**; runtime provisioning when no nodes are present.
- **Code:** `TutorialManager` (22-step state machine), `TutorialStepContent` (copy), pop-gun disabled while tutorial is incomplete (`OpenXRHandInput`).
