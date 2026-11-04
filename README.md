# Lexting: Type Words, Not Letters
## Rapid Spatial Gesture Text Input for XR

[![OpenXR](https://img.shields.io/badge/OpenXR-00897B.svg)](https://www.khronos.org/openxr/)
[![Unity](https://img.shields.io/badge/Unity-6000.0.60f1-black.svg)](Lexting_Unity/ProjectSettings/ProjectVersion.txt)
[![Version](https://img.shields.io/badge/version-0.3.0-blue.svg)](docs/implementation_plan.md)

![Lexting matrix palm control — dual-hand typing volumes](images/header.png)

<img src="images/youtube-icon.webp" alt="Alt text" width="50" style="vertical-align: middle;"> 3-Minute Tutorial Coming Soon


**Lexting** is an **OpenXR** spatial text-entry interface controlled with either hand or both hands. Each hand gets a **2×2×2 typing volume** (eight letter **nodes** each) floating in front of the wrist. To type faster than any other mid-air XR text entry method, you **grab**, **drag**, and **turn your wrists** in a single group gesture that maps to a whole word in English in under 0.5 seconds, yielding theoretical average speed limit above 120WPM for power users. Each gesture includes instant conjugation and punctuation capability. Hotkeys, numbers, special characters, and other ASCII features are avilable, and streamed keystrokes are currently able to provide remote input to Windows PCs in addition to game engine integration via SDKs. The **Unity SDK** is avaialble in 1.0. **Unreal SDK** and **OpenXR Direct SDK** will be avaialble in 2.0.

<hr style="border: 1px solid gray;">

<a id="introduction"></a>
<details>
<summary><b>🧊 Introduction</b></summary>
<hr style="border: 1px solid gray;">

<blockquote>
<details>
<summary><b>🌐 What is Lexting?</b></summary>
<hr style="border: 1px solid gray;">

Lexting is a rapid volumetric isotropic **hands-first XR text entry** system with no physical keyboard or furniture necessary. It is designed for competitive-speed text entry in spatial computing that works universally for users while sitting, standing, or even walking. It is especially needed as a replacement for ridiculously slow floating QWERTY UIs in XR since it is the only alternative that needs no tabletop, yet it can rival physical QWERTY throughput. Instead of typing keys one at a time on a flat QWERTY layout, you:

1. **Hover** into **nodes** inside each hand’s typing **volume** using an isotropic **hand cursor** so the real hand does not have to intersect and occlude the 3D widget.
2. **Grab** to type a letter or grab-hold to activate a whole word.
3. **Drag** across **nodes** to assemble up to **four letters** across both hands in one rapid dual-hand gesture.
4. **Filter** against an English dictionary in real time. Other languages will be supported in the future.
5. Release one hold of two to begin Finalization. **Finalize** the root word in the left hand volume, **conjugate** with dual-hand wrist orientations, and **punctuate** from the right-hand **volume**.
6. **Emit** text to subscribed `InputField` / `Text` components, or synthesize **Windows keyboard** input when running in PC passthrough / keyboard-replacement modes.

The finalized reference implementation is the **dual-volume 2x2x2 typing widget** (`TwoZoneCubeController`). Several layouts of various node counts, matrix types, and letter positions were tried and then deprecated in favor of this 2x2x2 matrix for each hand.

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>📜 Background</b></summary>
<hr style="border: 1px solid gray;">

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">The Point of Inception</span></summary>

Lexting was invented in January 2020 by AJ Campbell (scifiuiguy on GitHub). It was originally born out of his attempt to build a 3D visual programming language called Atomic. In the first week of developing Atomic, it became very clear that an iconographic node-based 3D programming language (a 3D VPL) was going to need a lot of text entry capability despite trying to turn all text-based code into 3D clusters of simple geometry, especially regarding value assingment for string variables. Like many other structures that form complex clusters in 3D, the Atomic project exposed the need a new kind of XR interface. 3D VPLs will feature synapse-like content throughout the field, requiring a user to engage with complex groups of 3D objects all at once while standing up in an open roomscale space without a desktop in the way. This use case laid bare the weakness of floating QWERTY, and of the entire raycast+WIMP paradigm.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Floating QWERTY, A Familiar Problem</span></summary>

Having worked at Magic Leap, AJ was familiar with how poorly floating QWERTY keyboards perform in spatial computing. He struggled with floating QWERTY jank for years, building multiple typing alternatives along the way. No previous prototypes felt competitive with physical QWERTY, but AJ knew **SOMETHING** was out there in the ether, some overlooked idea that would unlock competitive typing speed via 6DOF hand tracking. When he had the spark of the idea for Lexting, he had no idea it would change his entire view on XR interface design, but that change in mindset is crucial for everyone to prepare for the faster throughput XR needs to compete with desktop computers across the board.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">XR 1.0 Interfaces</span></summary>

AJ finds **raycasting** useful solely as a backwards compatibility tool for 2D legacy app support in XR. Most of his modern interface design work is inspired by the Lexting project to now feature **volumetric isotropic elements** instead of raycast cursors on flat planes. AJ believes **raycast cursors on flat-surface-based WIMP metaphors** are the key poor design choice that will make XR 1.0 interfaces look incredibly dated in a few years as they are replaced by **rapid-throughput volumetric isotropic interfaces in XR 2.0**.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Lexting was the First 3D Isotropic Text Entry Solution</span></summary>

Lexting was the first fully volumetric isotropic interface AJ ever designed. The idea is not new, but it **IS** a forgotten art. Volumetric isotropic techniques were popular in 1990s XR HCI research, but they fell out of fashion before XR achieved consumer-friendliness and were forgotten by the industry. Certain false assumptions led to every HMD OEM constraining control to X and Y with raycast+WIMP in every OS interface, and then application developers took their lead. We all built raycast+WIMP into the core of almost all XR interfaces. The enitre community (Academia and Industry) went **down the raycast+WIMP rabbit hole**. Lexting is a prime example to show why **it was the wrong rabbit hole**.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Highest-Throughput Need = 1st Domino to Fall</span></summary>

Text entry is the quintessential use case for which throughput is most needed. The XR industry put floating QWERTY UIs up as the champion interface. AJ stumbled upon long-forgotten isotropic techniques from 90s Academia without realizing it when he created Lexting. The isotropic nature of the UI is the ingredient that absolutely destroys raycast+WIMP UIs on throughout. Floating QWERTY vs. Lexting is just one early example of how EVERY raycast+WIMP UI will eventually be trounced by isotropic equivalents in apples-to-apples throughput competition, hence the inevitable transition to XR 2.0 interfaces.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">We Can't Do The Greatest Things in XR Without Higher Throughput</span></summary>

3D VPLs and other complex data visualization tools require a better interface than raycast+WIMP. Floating QWERTY and any raycast-based UI will ruin the flow of any fast-paced enterprise system that contains dozens-to-thousands of interactable items per field. XR 2.0 will bring **data cluster management capability** because newer interfaces will enable the prerequisite higher level of throughput. 

But the cart can't come before the horse. That's why AJ sidelined the Atomic project years ago to solve the QWERTY problem instead. Little did AJ know COVID was about to hit the world like a bomb, leaving an opening for Lexting to take over his every waking thought for two years.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">All For Nothing?</span></summary>

Unfortunately, after two years of changing the layout over and over until it felt perfect, COVID had sent the entire XR industry running for the hills. It felt as though very few people were interested in trying the private beta once it was finally approaching a stable state. The disinterest was a wild contrast to the fervor of the 20-teens, but AJ assumed the XR market would roar to life back one day, as it always has in past cycles. He decided to shelve Lexting until that day felt closer.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Are We There Yet?</span></summary>

The next XR hype cycle feels closer now in 2026. We have world-meshing HMDs that are **almost sunglasses form factor** now, whispers of **XR app multitasking** on the horizon (an absolute necessity for XR adoption in offices), and **containerization** has taken the cloud by storm, unlocking edge compute bottlenecks. The technical ingredients are lining up for XR 2.0. Now we just need wicked-fast throughput in all of our UIs. AJ told a few people he'd open-source Lexting one day, since there were dozens who expressed interest without any marketing. In Q4 2026, it's finally time. Let's do this!

</details>

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>⚙️ How Does It Work?</b></summary>
<hr style="border: 1px solid gray;">

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Warm Up: Breathe In The Nature of Your Arm's Flexing Capabilities</span></summary>

The elbows should always be near the ribs, but not tucked or pressed into the body. Just like the hundreds of drum students AJ taught when he was in college, he encourages anyone using Lexting to start by rolling those shoulders out and shaking those elbows loose so you can feel them flopping lifelessly. That looseness is how the elbows should stay. Hold on to that relaxation as you put your hands into starting position. It's scientifically proven that we all flex a whole bunch of our muscles for no reason whenever we try something new, so it's best to start each Lexting session with a quick reminder to shake all of that tension out before you begin. Every shoulder muscle should be ready for bed through the entire session. Just remember: floppy elbows are good. Only a tiny flex of the biceps is needed to bring hands up into starting pose.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Starting Pose</span></summary>

The starting pose is elbows completely neutral, and the elbows are never meant to move more than three inches forward or three inches back during Lexting use, for a total z-depth range of 6". Neutral elbow should put cursors directly into the center core of each hande node in the UI. If not, then the UI or the body needs to adjust. The default body-relative position of the UI will be customizable to suit each user's preferences in a future version of Lexting, but for now the body offset is globally set per-application. The elbow never moves laterally away from the ribs, only forward and backward. Lateral elbow motion flexes the deltoid (shoulder cap) for absolutely no reason.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Starting Hands</span></summary>

From neutral elbows, you point both wrists and all fingers out straight ahead, palms facing each other. This palms-sideways position is the absolute most relaxed/unflexed position for your ulnar wrist muscles, which is important for Lexting, since you twist the wrists to activate modes.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Wrist Warm-up</span></summary>

Before trying to type, release all finger tension. The fingers will curl slightly, pointing toward each other. Now feel the wrist muscles flex as you turn both palms downward (pronate). Feel them release back to sideways. Feel them flex to turn palms up (supinate). Feel them release to sidways. Sideways is home because sideways is muscle release.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">The UI</span></summary>

Each hand alone has an 8-cell 2x2x2 cube to itself, which displays eight letters at once, one in each cell. Pronate to change to a different set of 8 letters. Supinate to change to yet a different set. These three wrist positions give you 24 letters. The final two letters in the English alphabet are the least common in the dictionary (X and Z) so we place them in the UI's overflow area, which you can access by turning the palm slightly forward with fingers pointings slightly upward. X and Z become avaialble in the overflow area (the bottom rear two cells).

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Basic Activation</span></summary>

To activate a node, you can set Lexting in either **Pinch Mode (index-to-thumb)** or **Grab Mode (close all four fingers against the palm)**. AJ prefers Grab Mode because it enables all four inline fingers to work together, which requires less coordination. You can tell it's easier because moving any one inline finger alone is actually quite challenging, even the index finger. All four inline fingers WANT to move together, and you have to train muscle memory to coordinate them apart. It's why baby's can whole-hand-grab things even before they're out of the womb but a single finger pinch is hard for an infant. Our most fundamental motor cortex synapses trained the whole-hand-grab first. It might seem like it requires more muscle, but take it from a drum teacher... you can move all four fingers rapidly in tandem all day long without fatigue as long as you don't flex too hard.  In Pinch Mode, a pinch types a single letter. In Grab Mode, a grab types a single letter. The README from here on will only mention grab for activation. That said, if you prefer Pinch Mode over Grab Mode, it is a toggleable option.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Word Mode</span></summary>

Lexting is built around two phases: **word mode** (assemble a letter prefix) and **finalization mode** (pick the dictionary word type, grammar, and punctuation).

Start with a **grab** in both hands when you're hovering over the first to letters of your word. Tentative output appears above the **volumes**. You can add a **third and fourth** letter by moving either hand. The dictionary filters to the top result in real time.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Finalization Mode</span></summary>

When eight or fewer roots match, matching words fill the **left volume** as **nodes**. Hover a root, set **conjugation** with **two-hand wrist orientation** (`LextingConjugationState`), and pick **punctuation** from **nodes** on the **right volume**. **Soft-tap** (open-hand tap) cycles alternate forms in the same conjugation group. Release the remaining grab to **commit** the word.

</details>

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>⌨️ Why not QWERTY?</b></summary>
<hr style="border: 1px solid gray;">

The real sole competitor is not physical QWERTY nor projected virtual QWERTY nor stenography, although Lexting draws deep influence from them all. The sole competitor is floating virtual QWERTY, since floating QWERTY is the only option other than Lexting that works in mid-air.


<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">QWERTY versus Stenography</span></summary>
Flat floating **QWERTY in XR** is a pain even for the occasional one-off keyword search, but it is an absolute deal-breaker for enterprise productivity typing, whereas **Stenography** is to go-to professional tool that requires years of training for best-in-class throughput, yet most office workers can't cross its huge learning curve or deal with its expensive closed-source hardware. Lexting aims to borrow techniques from both QWERTY and steno to create a best-of-both-worlds on speed versus ease to learn. Lexting uses steno-style chording techniques to destroy floating QWERTY. It probably competes neck-and-neck with physical QWERTY, yet it's easier to learn than QWERTY, let alone MUCH easier than Stenography. 

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Spatial Computing in Mid-Air</span></summary>

Productive rapid spatial text entry **IS** possible within the volume in front of your upper torso—but not on a virtual floating QWERTY panel. Vice versa, You **COULD** use Lexting at a physical desk, but there will tyipcally be little need when physical QWERTY is already there and familiar in most cases. Lexting provides a 3D widget **attached to the upper body** so it works **sitting, standing, or walking,** making it the most flexible competitive option. That said, roomscale standing with no table in the way is its default use case.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Occlusion and Periphery</span></summary>

Poking individual keys with tracked fingers means your hand covers the very target whose hover and selection highlights are crucial peripheral indicators for picking up speed. Mobile devices use popover tooltips to reveal whether you hit the right key, but that only works when the line of sight is inches away from the output text that is supposed to retain eye focus. Lexting uses an offset **hand cursor** and **volumetric nodes** so selection stays perfectly perceptible, making "touch-typing" feasible even though there's nothing physical to touch. It's periphery typing instead of touch-typing, but the goal is identical: let the eyes stay on the output. Volumetric isotropic controls let you place the cursors IN the nodes, not on them (volume-based not surface-based), and unique hover/selection aesthetics plus audio cues are geared toward aiding peripheral control. Lexting expects sightline to stay on the output while the widget provides crucial peripheral feedback that doesn't even need a glance.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Isotropic 3D Layout</span></summary>

Unlike QWERTY, an inherently 2D layout, Lexting letters live on a **cubic 3D lattice** around each wrist, not on a 2D row copied from a typewriter. That matches wrist rotation and grab-and-drag motion instead of forcing every symbol onto a flat plane. This maximizes adjacency, leading to smallest possible hand travel distance, which yields top throughput and ergo comfort.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">The Layout is Designed For Error Prevention</span></summary>

The eight-cell multi-choice widget with a neutral core was chosen after many iterations of various layouts, many of which performed at high speed but killed their own total throughput by being too error-prone. The eight-cell system is that perfect Goldilocks arrangement for 3D hand motion. It eliminates all overshoot and undershoot errors because, for each node-to-node motion, there are essentially no intermediate stopping point options, which means it's nearly impossible to overshoot your target even without looking. Beginners can move their hands in big, exaggerated actions and still not miss. Power users can practice shrinking the total travel distance over time to pick up speed. In contrast, there is no way to type with floating XR QWERTY without looking.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Chordal Throughput</span></summary>

Floating QWERTY in VR is still largely **one symbol per discrete action**. Without a tactile surface to work against, it can never match even a fraction of physical QWERTY speed. Chordal alternatives are a necessity to get that speed back. Lexting assembles up to **four letters per dual-hand gesture** in Word Mode (which engages as soon as the user begins using two hands in tandem). It's like snatching a word out of the air by grabbing four letters all in one quick motion. The word resolves from a **dictionary**—often appearing immediately, or presenting a short list of word choices. This is a hybrid best-of-both-worlds between QWERTY and Stenograophy. It's a chordal system with all the speed of QWERTY without the years of memorization needed for stenography.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Language and form, not just alphabet</span></summary>

Conjugation and punctuation are **first-class gestures** (wrist pose + right-hand **volume**) that can be baked into each word in the same gesture that created the word, making extra prefixed gestures, postfixed gestures, extra letters, and modifier keys unnecessary.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Lexting Knows Its Niche</span></summary>

Lexting does **not** claim physical QWERTY is useless in XR. AJ assumes physical QWERTY will remain king as long as there is a flat desktop available on which to place the physical keyboard. That said, since Lexting Word Mode requires (at most) one small wrist rotation plus one finger gesture per word with no physical backstops causing impact micro-traumas, Lexting is theoretically much ergonomically safer than physical QWERTY despite having competitive speed. That doesn't mean everyone will switch overnight, but it MIGHT mean that it would be healthier for their bodies to do so.

AJ also finds projected virtual QWERTY (tabletop QWERTY without the physical keys) to be an interesting solution as well, although it seems like a vast majority of its use cases are better served by physical QWERTY since both require a desktop. The author of Lexting imagines a future for most offices where rolling standing desks enable everyone to bring a physical QWERTY keyboard anywhere in the room while engaging with volumetric content.

Sometimes, even a small rolling desk will feel "in-the-way." There must be a way to continue typing rapidly in mid-air. AJ considers Floating QWERTY to be a poor interface that Lexting should replace entirely, and then it can be an optional alternative to the other QWERTY forms. If Lexting gains only 20 million power users by 2080, it will have grown faster than QWERTY did. By then, who knows if we'll still have 2D screens at all, let alone QWERTY keyboards? If everything has become 100% spatial by then, how text at all? Floating QWERTY only? Yikes. We need something better to replace floating QWERTY.

</details>

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>📈 Isn't There A Big Learning Curve?</b></summary>
<hr style="border: 1px solid gray;">

There **IS** a learning curve, but it's a shorter curve than **QWERTY** and a MUCH shorter curve than **Stenography** even though it performs like a best-of-both-worlds between the two.

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Far fewer total hand motions</span></summary>

Unlike QWERTY, you don't seek ONE-letter-with-ONE-stroke. You grab four letters all at once in a single stroke. Lexting is much closer to Stenography in this fashion, since it is intended to be chordal by default for most gestures. Via **word mode** (i.e. two hands working together for one gesture), you snatch just enough info to steer the **dynamic dictionary** toward your word, and then that word is always in the same spot in the UI. Finalization (subtle twists of the wrist) enables you to postfix your word before you let go of that same gesture. One stroke gets you a whole word every time.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">The eight-node volume is deliberately forgiving</span></summary>

Each hand’s **2×2×2** grid prevents overshoot, which is common in almost any other layout in 3D space, including QWERTY. Beginners can use **large, exaggerated** hand paths and still land in the correct node; speed comes from recognizing those paths can be shortened over time.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Chordal speed without memorization tax</span></summary>

Stenography pays off with extreme speed, but it demands **years of chord memorization** before it feels natural. With Lexting, you only memorize the letter positions, just like QWERTY, but then Lexting let's you chord like steno right away. Instead of requiring you to know thousands of chords beforehand, you just need to know the first four letters, and your word appears. If you've never typed it before, you can glance down to see where it is among the eight node positions. It will always be in that spot, so your hand will automatically go there after you've typed that word a few times. Lexting takes chordal patterns from a stenography book and bakes them into your spatial muscle memory as you practice typing, rather than making you study a stenography book for years like learning a whole new language.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Honest expectations</span></summary>

Lexting is much like QWERTY in that you can peck a few letters out at a time right away, but unlocking rapid speed takes practice. It will feel slow at first, then **step-change faster** once word mode, finalization, and peripheral hover feedback start to click as fluid gestures, then gesture sequences. Lexting is geared toward early adopters of enterprise XR who love the idea of working with the whole room as their canvas, on their feet, rather than trapped in a cubicle staring at a 2D screen.

</details>

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>🧪 What exists in this repo?</b></summary>
<hr style="border: 1px solid gray;">

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Core dual-volume typing UI</span></summary>

Main scene: [`Lexting_Unity_Plugin/Scenes/main_01_twozone.unity`](Lexting_Unity_Plugin/Scenes/main_01_twozone.unity) (same content as the legacy monolith scene). In-editor documentation for architecture and input setup is stored on **`Descriptor`** components in that scene (large `description` text areas—summarized under [Repository Layout](#repository-layout) and [Architecture](#architecture)).

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Input boundary (OpenXR target)</span></summary>

**Target design:** One **OpenXR** input boundary feeds **`SpatialGestureInput`** (hand/controller poses, gestures, session lifecycle). Everything above that layer—two-volume typing UI, dictionary, hotkeys, output—is runtime-agnostic.

**This repo today:** `LextingInputModuleManager` still exposes legacy **`QUEST_*`** paths alongside **`OPEN_XR`**. Passthrough modes forward text to a Windows PC over the network. The **target** is **OpenXR-first** wiring end-to-end (see [Platform targets](#platform-strategy)).

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">English dictionary & conjugation</span></summary>

JSON word lists under `Scripts/EnglishDictionaries/Resources/` feed `LextingDictionary` (four-tier prefix lookup). `LextingConjugationState` maps two-hand wrist poses to grammatical forms during finalization. See [`docs/conjugation_algorithm.md`](docs/conjugation_algorithm.md) for the designer-facing word mode and finalization workflow. It was determined during early R&D that an approximate 10K-word dictionary fits in the default Lexting interface without paging. Even though 100s of 1000s of exotic words exist in English, the first prototype of Lexting is intended to achieve conversational texting with the most popular 10K words in English, along with the ability to add custom words to the dictionary. Larger dictionaries that support paging will be implementaed in the future.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">Demos & tooling</span></summary>

* **QWERTY_Sim** — visualization / comparison demo (`_Scenes/QWERTY_Sim.unity`).
* **Streaming / MLAPI** — experimental Quest→PC streaming (much of the MLAPI code is commented out; see [Streaming & PC passthrough](#streaming)).
* **Tutorial** — in-scene tutorial driven by `TutorialManager` and pop-gun gestures.
* **Speed tests** — `SpeedTestManager` for throughput experiments.
* **Implementation plan** — [`0.2.0` → `1.0.0`](docs/implementation_plan.md) active path (Unity plugin → OpenXR refresh → Meta Start); **`0.1.0`** Quest baseline complete.

</details>

<details>
<summary style="font-size: 1.15em; display: list-item;"><span style="font-weight: 600;">BETA Limitations</span></summary>

Lexting is in Beta. A few primary features have yet to be implemented:

**Long-Word Drill-Down** A small number of words have more than eight potential results once the first four characters are selected, but they don't have more than eight different options for the 5th letter, so if there is only one option for a given 5th letter, then that full word will display in the UI during Finalization. If there is more than one remaining option for that 5th letter, then the first five letters plus "..." will be displayed. If the user hovers on that node and releases Finalization, the first of that group will print, but if the user wants to drill down to other options, they can grab-hold that node to display all the options and move to a different option. The current 1.0 Beta build has no drill-down support yet, so only the first eight options will appear, which means a small number of English words may be impossible to type using Word Mode until Lexting is out of Beta.

**2D Cursor/Caret Control** Control of the 2D caret in a future version of Lexting will turn Lexting into a full-blown word-processing tool rather than just a rapid text input tool. The Caret Control is accessed via Hotkey, and while some of the other Hotkeys e.g. COPY are quite easy to implement, they will not be very useful without a Caret Control to highlight text to copy first. Therefore, most of the Hotkey systems are considered Pending in Beta as they await Caret Control.

**PC Streaming** There is an operational PC streaming service in the Beta, but the Streaming Service itself is brand new in Alpha. It will be throughly tested before Lexting comes out of Beta. 

</details>
</blockquote>

</details>

<hr style="border: 1px solid gray;">

<a id="architecture"></a>
<details>
<summary><b>🏗️ Architecture</b></summary>
<hr style="border: 1px solid gray;">

<blockquote>
<a id="platform-strategy"></a>
<details>
<summary><b>🎯 Platform Targets</b></summary>
<hr style="border: 1px solid gray;">

Lexting is built for **near-field, two-hand** input—the form factor where spatial word entry pays off. OpenXR is the integration layer emerging across the OEMs that will enable long-term cross-platform compatibility long into the future.

The long-term product shape is the **Lexting OpenXR SDK**: hand-tracked typing logic, gesture semantics, dictionary/word pipeline, and output events implemented **once**, on top of **OpenXR** session and hand/controller APIs. **Engine projects are front-end integrations** of that SDK—not separate platform forks.

* **Today (`0.2.0`):** **`Lexting_Unity_Plugin/`** UPM package (reference scene **`main_01_twozone`** under **Packages → Lexting**). Legacy monolith [`Lexting_Unity/`](Lexting_Unity/) may still be used for Quest builds until the greenfield host replaces it.
* **Goal:** a matching **Unreal Engine** reference prototype that uses the **same SDK**, so teams can evaluate Lexting in either editor without reimplementing typing behavior.
* **Beyond engines:** any **app or ecosystem that already supports OpenXR** should be able to host the SDK with comparatively thin glue (native, custom engines, enterprise shells, etc.)—the SDK is the portable core; Unity and Unreal are the first-class **sample hosts**.

<blockquote>
<details>
<summary><b>Why OpenXR?</b></summary>

* **One runtime contract** for head pose, hands, controllers, and (where exposed) session and composition—instead of N duplicate SDK forks in the typing stack.
* **OEM convergence:** Meta, Samsung, Valve, XReal, and others ship or plan **OpenXR runtimes**; successful hardware roadmaps align with the spec, not proprietary input silos.
* **Portable core:** Two-volume typing, word assembly, dictionary, and hotkeys belong in the **Lexting OpenXR SDK**; Unity/Unreal layers supply scene graph, UI widgets, and editor workflow only.
* **Integrator freedom:** If your product already loads an OpenXR runtime, you integrate **Lexting** at the SDK boundary—not by porting a whole Unity project.

</details>
</blockquote>

<blockquote>
<details>
<summary><b>Layered architecture (target)</b></summary>

```mermaid
flowchart TB
  OEM[OEM headsets and glasses] --> OXR[OpenXR runtime]
  OXR --> SDK[Lexting OpenXR SDK]
  SDK --> UnityFE[Unity front-end prototype]
  SDK --> UnrealFE[Unreal front-end prototype]
  SDK --> Host[Any OpenXR-capable host app]
  UnityFE --> UXu[Unity UI and scenes]
  UnrealFE --> UXue[Unreal UI and levels]
  Host --> UXh[Host-native UI]
```

**Meta Quest** remains the **primary dev kit and perf baseline**, accessed as **Meta’s OpenXR runtime**—not as a parallel long-lived platform branch in Lexting.

In this repository, the SDK boundary is **still co-located with the Unity C# codebase** (`SpatialGestureText.*`). The **target** is a hardened SDK boundary that an Unreal front-end (and other hosts) can call through the same OpenXR-facing API.

</details>
</blockquote>

<blockquote>
<details>
<summary><b>Honest gap — what this repo still does</b></summary>

The open-source snapshot predates the consolidation pass:

* **Only the Unity front-end** is present; **Unreal** and standalone **SDK packaging** are roadmap deliverables, not folders in this tree yet.
* **`InputMode.QUEST_*`** and **`OculusHandInput`** still drive most Unity scenes and tests.
* **`InputMode.OPEN_XR`** exists but is not yet the single blessed path in docs, prefabs, and CI.
* Unity packages in [`manifest.json`](Lexting_Unity/Packages/manifest.json) need alignment with **current OpenXR** feature sets (hand tracking, interaction profiles, extensions).

That mismatch is intentional to document: **this README states the OpenXR-first architecture; the tree reflects the current Unity prototype.**

</details>
</blockquote>

<blockquote>
<details>
<summary><b>OEM runtimes we optimize for (via OpenXR)</b></summary>

Hand tracking quality and SKU availability vary; Lexting does **not** fork the UI per vendor. Each row is the **same OpenXR integration path** unless noted.

| OEM / device line | OpenXR notes |
|-------------------|--------------|
| **Meta Quest** (current gen and successors) | Default **development and perf** runtime; validate every release here first. |
| **Samsung Galaxy XR** | **Android XR / OpenXR**; same adapter as other OpenXR Android runtimes when hand APIs stabilize. |
| **Steam Frame** | **OpenXR on SteamVR**; PC VR runtime, shared with other Steam OpenXR devices. |
| **Meta VR Glasses** | **Meta OpenXR stack** when retail glasses expose standard hand or companion-controller input. |
| **XReal Aura** | **OpenXR** plus documented vendor hand extensions mapped into the adapter; glasses fit Lexting’s offset hand cursors well. |
| **Snap Specs** | **Not OpenXR-compatible yet.** The **Lexting OpenXR SDK** will support **Snap Specs as soon as Specs conforms to OpenXR** (or exposes equivalent standard hand/head data). Until then: **roadmap only**—no Specs-specific plugin in this repository. |

</details>
</blockquote>

<blockquote>
<details>
<summary><b>Integration principle</b></summary>

**Do not rewrite typing logic for new hardware or new hosts.** Extend the **Lexting OpenXR SDK** so OpenXR session/hand/controller data flows into the same gesture and word-entry state machine; **Unity** and **Unreal** front-ends only render UI and forward engine-specific text output.

Third-party **apps that already support OpenXR** should integrate the SDK directly when they do not need a full engine sample—same OpenXR boundary, thinner host layer.

Bespoke non-OpenXR shims are **exception-only** (e.g. pre-OpenXR Specs). They belong outside the SDK core and should be removed when the runtime ships conformant OpenXR hand interaction.

</details>
</blockquote>

</details>
</blockquote>

---

<blockquote>
<a id="input-modes"></a>
<details>
<summary><b>🎛️ Input Modes & Setup</b></summary>
<hr style="border: 1px solid gray;">

`LextingInputModuleManager` (`[INPUTS]` object in the hierarchy) selects **`InputMode`**. **Integrators should treat `OPEN_XR` as the long-term default** (see [Platform targets](#platform-strategy)). Legacy **`QUEST_*`** modes remain in the prototype tree for compatibility with existing scenes.

| Mode | Purpose |
|------|---------|
| **`OPEN_XR`** | **Target path:** any conformant runtime—Meta Quest (OpenXR), PC VR, Android XR, **Steam Frame**, **Galaxy XR**, **XReal Aura**, **Meta VR Glasses**, etc. |
| `QUEST_HANDS` | *Legacy prototype:* Quest embedded UI via Oculus-specific hand path. |
| `QUEST_HANDS_PASSTHROUGH` | *Legacy:* Quest passthrough; streams text to a **local PC** over Wi‑Fi. |
| `QUEST_TOUCH` | *Legacy:* Quest Touch controllers. |
| `QUEST_TOUCH_PASSTHROUGH` | *Legacy:* Touch + PC passthrough. |

**Default integrator path:** run **Lexting → Setup Scene** in your open scene ([Getting Started](#getting-started)) **after** you already have an **XR player rig** in that scene (e.g. **XR Origin** / XRI rig with head and hand tracking targets). The wizard writes **serialized** scene content only: **`[INPUTS]`** + **`LextingInputModuleManager`** with **Transform** references into **your** rig. The dual-volume typing widget is **not** saved into your scene hierarchy (see Getting Started).

**Advanced / manual:** assign **head**, **left hand**, and **right hand** on **`LextingInputModuleManager`** yourself only if you are customizing beyond what the wizard provides. Legacy prefabs under [`Lexting_Unity/Assets/Lexting/Resources/Prefabs/InputControllers/`](Lexting_Unity/Assets/Lexting/Resources/Prefabs/InputControllers/) (e.g. **`Lexting_OVRCameraRig`**) remain for prototype Quest workflows; new projects should rely on **OpenXR** + the setup wizard.

OEM coverage—including **Snap Specs** once they ship **OpenXR**—is summarized under [Platform targets](#platform-strategy).

</details>
</blockquote>

---

<blockquote>
<a id="typing-workflow"></a>
<details>
<summary><b>✍️ Word Mode & Finalization</b></summary>
<hr style="border: 1px solid gray;">

The following is adapted from [`docs/conjugation_algorithm.md`](docs/conjugation_algorithm.md) and the word-assembly / finalizer class documentation.

<details>
<summary><b>Word mode Basics</b></summary>

* Word mode starts when **both hands begin a grab in rapid succession**. The first two chosen letters produce dictionary candidates (invalid pairs such as `"XX"` do not enter word mode).
* The tentative word appears above the typing **volume(s)**. The **first and second** characters come from the initial grabs on each hand.
* **Third and/or fourth** characters come from moving dragging into a new **node** with one or both hands.
* Releasing one hand but not the other activates **Finalization Mode**.
* **Popular four-letter roots** (`comp`, `cons`, `cont`, `inte`, `port`, `tran`) require **two extra letters** before finalization—with these special cases, releasing one hand and holding the other will continue to show letters rather than entering finalization mode. This is the system's way of telling you it needs a couple more letters to hone in on your word. These situations are quite rare, but you just need pick one extra letter per hand in this case.

</details>


<details>
<summary><b>Word Mode Composition</b></summary>

**Four letters** merge into a grab cluster as follows:

1. First letter can be grabbed by either right or left hand.
2. Second letter must be grabbed by the opposite hand to begin Word Mode.
3. Either hand moves to a third letter.
4. The opposite hand moves to the fourth letter.

The assembler autocorrects accidntal swaps of the 3rd/4th letters when a mistake does not yield a valid dictionary word, which means the user can blast through a sequence at high speed, and the assembler will almost always give the correct combo the user performed the correct order or not. Only the first two letters need to be ordered correctly in most cases.

To lock in selection, the user releases one hand and holds the other, which activates Finalization Mode.

</details>

<details>
<summary><b>Finalization mode</b></summary>

* When eight or fewer roots match the chosen word cluster, the **left** typing **volume** lists root words (alphabetical).
* Hover the desired root **node**. Optionally set **conjugation** with two-hand wrist orientation (see Conjugation section).
* If multiple conjugations exist for the same orientation, **soft-tap open hand** to cycle (paging dots show group size).
* The **right** **volume** offers **punctuation** while finalizing:

| Node layout (right volume) | Characters |
|-------------|------------|
| Top back | `?` `!` |
| Top front | space, no-space |
| Bottom front | `.` `,` |
| Bottom back | `:` `;` |
| Center | `"` |

* Hover a punctuation **node** with the right hand, then **release the remaining soft hold** to commit the conjugated, punctuated word.

</details>


</details>
</blockquote>

---

<blockquote>
<a id="conjugation"></a>
<details>
<summary><b>🔄 Conjugation</b></summary>
<hr style="border: 1px solid gray;">

During Finalization, while the user is choosing the final word root on the left and optionally selecting a punctuation on the right, they can also turn the wrists at the same time to apply suffixes to turn the word from noun to verb, singular to plural, or even adjective to adverb.

| Left hand | Right hand | `ConjugationState` |
|-----------|------------|---------------------|
| relax | relax | 1st-person singular verb |
| pronate | relax | 3rd-person singular verb |
| supinate | relax | present progressive verb |
| supinate | supinate | past verb |
| relax | pronate | singular noun |
| pronate | pronate | plural noun |
| supinate | pronate | possessive noun |
| relax | supinate | adjective |
| pronate | supinate | adverb |

It is possible for two different wrist gestures to spell the same word because English often takes oddball twists. For example, we commonly use an 's' suffix for both 3rd-person verbs and plural nouns **most of the time** but **not always**, which is why we must have distinct gestures for each. Good examples are "ends-in-f-but-nouns-become-'ves'" words like leaf, wolf, loaf, and scarf.

During finalization, `TwoZoneWordListController` reads the active conjugation key from the dictionary JSON for the hovered root **node**.

</details>
</blockquote>

---

<blockquote>
<a id="hotkeys"></a>
<details>
<summary><b>⌨️ Hotkeys</b></summary>
<hr style="border: 1px solid gray;">

Lexting exposes common word processing commands (copy, paste, backspace, etc.) by using the center zone as a drag modifier for all the other zones in the widget. 

### Activating hotkeys (current build)

1. **Grab + Hold** the Hotkey zone in the center of the UI.
2. **Drag** into any other zone. Blue text will show which Hotkey is available for that zone.
3. **Rotate** the wrist palm down or palm up during drag to access other Hotkeys.
4. **Release** to activate the chosen hotkey.

**Alt**, **Shift**, and **Ctrl** are toggleable, latching on and off.



| Matrix Position | pronate | relax | supinate |
|-----|---------|-------|----------|
| Top-Left-Front | PASTE* | COPY* | CUT* |
| Top-Right-Front | ITALICS* | BOLD* | UNDERLINE* |
| Top-Left-Back | TAB FORWARD* | TAB* | TAB BACKWARD* |
| Top-Right-Back | BACKSPACE | BACKWORD | DELETE |
| Bottom-Left-Back | SHIFT | ALT | CTRL |
| Bottom-Right-Back | UNDO* | CAPITALIZE ONE | REDO* |
| Bottom-Left-Front | SELECT LETTERS* | CARET* | SELECT WORDS* |
| Bottom-Right-Front | MOUSE* | SELECT ALL* | MACROS & EMOJIS* |
| Center | ENTER | NUMBERS | ESCAPE |

* Any Hotkey above with a * is **Non-operational in Beta**.

</details>
</blockquote>

---

<blockquote>
<a id="streaming"></a>
<details>
<summary><b>📡 Input Streaming Passthrough</b></summary>
<hr style="border: 1px solid gray;">

Input Streaming Passthrough is intended for an XR client that drives text entry on a Windows desktop or a TV from across a room via network-delivered keystrokes.

Although Lexting is in Beta, the Streaming Service is brand new in Alpha. It is experimental and not to be relied upon for real productive work yet.

For PC-side reception, install the Streaming Service on a PC on the same network as the HMD. You can manually assign that PC's IP and port in Unity Editor, and vice versa for the serivce on PC. The complete version of this service will feature automatic punchtrough. The PC and HMD will communicate over UDP.

</details>
</blockquote>

</details>

<hr style="border: 1px solid gray;">

<a id="control-basics"></a>
<details>
<summary><b>🎮 Control Basics</b></summary>
<hr style="border: 1px solid gray;">

<blockquote>
<details>
<summary><b>🖐️ Wrist Rotation States</b></summary>
<hr style="border: 1px solid gray;">

* **`relax`** — palm sideways
* **`pronate`** — palm down
* **`supinate`** — palm up

![Lexting matrix palm control — 24 letters in 3 palm positions](images/matrix-palm-control.png)

Rotation changes swap which character or command appears on each **node**. The relaxed pose shows the eight most frequently used characters in the alphabet. The pronated and supinated positions show less frequent characters. Turning palm slightly forward causes the rear bottom two letters to toggle to X and Z, which completes the alphabet.

**Finalization Mode** takes over wrist functionality at the end of a Word Mode gesture because it assumes the desired letters have all been chosen already, thus it's time to use the wrists to specify conjugation. You can do so with combinations of palm state shared between the two hands:

![Lexting matrix palm control — dual-hand typing volumes](images/wrist_functions.png)

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>✊ Hand Gesture States</b></summary>
<hr style="border: 1px solid gray;">

| State | Description |
|-------|-----------------------------------------------|
| `Neutral` | Open hand / not grabbing |
| `Activate` | Grab Gesture (all four inline finger close against the palm) |
| `Drag` | Grab / Hold / Move from one node to another |

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>🎯 Hand Cursors</b></summary>
<hr style="border: 1px solid gray;">

`HandCursor` tracks an offset point in front of each hand. Lexting Hand Cursors are 100% isotropic, not perspective-projection-based, meaning they move 1-to-1 on X, Y, and Z in identical mapping rather than casting out of the hand like a laser pointer.

![Lexting Cursor Offset Diagram](images/Cursor_Offset_Diagram.png)

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>↩️ Back-word Gesture</b></summary>
<hr style="border: 1px solid gray;">

Touching the **opposite hand’s middle finger** to the base of the wrist activates **back-word**  A.K.A. it deletes all the way back to the previous space.

![Lexting Cursor Offset Diagram](images/backword_function_infographic.png)

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>↩️ Caret Control Gesture (TBD)</b></summary>
<hr style="border: 1px solid gray;">

The following is not implementated but planned for a future version of Lexting. Better Caret Control will allow Lexting to become not just a text entry system, but a rapid word processing system as well.

Just as the Back-Word function is a Hotkey that also has a default hand gesture mapping for quicker access, the **Caret Control** will also have a **rapid-access hand gesture interface** in a future version of Lexting. Rather than use the Hotkey menu to toggle Caret Control on, you'll be able to take instant caret control using **Secondary-Pinch-Hold (middle finger to thumb) + Drag** in either hand. 

The mapping will allow the caret to move to each position across an entire line of text with X hand motion where one line of 50-100 characters can be swept across ~8" of hand motion by default. Y hand motion will jump to newlines of text in a multiline block.

By default, a **left-hand Secondary-Pinch-Tap-Quick-Release** will act identically to the **Back Arrow Key**, retreating the caret by one position. Likewise, the same from the right hand will advance the caret by one position like the **Right Arrow Key**. 

**Supination (palm-up) + Secondary-Tap** will act as **Up Arrow Key**, jumping to the newline above. **Pronation (palm-down) + Secondary-Tap** will act as **Down Arrow Key**, jumping to the newline below. 

**Secondary-Pinch-Hold-Drag** with either hand to the left or right will highlight the entire word immediately to the left or right of the caret, and futher drag will jump the highlight to the entire next word progressively. During this drag, pronation will change from full-word highlight to caret-placed-at-the-end-of-the-word. Supination will change from full-word to caret-placed-at-the-front-of-the-word.

To highlight an entire passage, **Secondary-Pinch-Hold** in both hands at once, and drag either the left hand or the right hand. The left hand marks the beginning of the highlighted section and the right hand marks the end, so dragging with left allows highlight of entire text blocks before the caret and dragging with the right allows highlight of entire text blocks after the caret.

To highlight every character on the current line before the caret all the way up to the position directly above the caret, **Secondary-Pinch-Hold** in one hand, and **Supinate + Secondary-Tap** in the other. To do the same for the characters ahead of the caret all the way to the newline position below, **Secondary-Pinch-Hold** in one hand, and **Pronate + Secondary-Tap** in the other.

If you drag the caret to the middle of a word and then activate **Word Mode**, Lexting will jump the caret to the end of that word to insert a new word after it. This is true whether the caret has no highlight. If the caret is highlighting multiple characters when Word Mode activates, Lexting will delete the highlighted portion and prefix a space before the new word.

By default, the **Back-Word gesture** deletes the entire word the caret currently marks no matter which character position it holds in that word, including deletion of the space following that word if there is one. To backspace-remove a single character instead, **Secondary-Pinch-Hold** with left and then 

None of the gestures in this Caret Control section are mapped in the current version of Lexting. All are pending for the future.

</details>
</blockquote>

</details>

<hr style="border: 1px solid gray;">

<a id="repository-layout"></a>
<a id="project-layout"></a>
<details>
<summary><b>📁 Repository Layout</b></summary>
<hr style="border: 1px solid gray;">

<blockquote>
<details>
<summary><b>📂 Package Folder Tree</b></summary>

The Unity package in [`Lexting_Unity_Plugin/`](Lexting_Unity_Plugin/) is what you add to a host project. In the Editor it appears under **Packages → Lexting Spatial Gesture Text** (folder name follows the package id).

```text
Lexting_Unity_Plugin/
├── package.json              UPM manifest (version, Unity minimum, XR/Input dependencies)
├── README.md                 Short install notes for manifest.json
├── ThirdPartyNotices.md      Licenses for embedded third-party source
├── Runtime/                  Player assemblies and assets
│   ├── Scripts/              Lexting C# (typing UI, dictionary, hotkeys, streaming hooks)
│   │   ├── TwoZoneCube/      Dual-volume controllers, nodes, hand cursors
│   │   ├── LetterCube/       Output manager, word assembly, UI receivers
│   │   ├── Input/            Hand/XR adapters, LextingInputModuleManager
│   │   ├── Hotkeys/          Dual-hand hotkey matrix
│   │   ├── EnglishDictionaries/   Word JSON + LextingDictionary
│   │   ├── Tutorial/         In-scene tutorial helpers
│   │   ├── Streaming/        Windows UDP client (desktop passthrough experiments)
│   │   └── ThirdParty/       SimpleJSON, Surge (Pixelplacement subset)
│   ├── Prefabs/              XRRig, letter cubes, output canvases, widgets
│   ├── Resources/            Runtime-loaded assets (macros, conjugation notes, etc.)
│   ├── ThirdParty/           UniRx, WindowsInput, vendored XR Hands HandVisualizer (XRRig)
│   └── (materials, shaders, fonts, audio, models, …)
├── Editor/                   Inspector editors, Surge editor scripts, define-symbol postprocessor
├── Scenes/
│   └── main_01_twozone.unity Reference scene (+ baked lighting data alongside)
└── Releases/                 Quest demo APK notes; download `tutorial_demo.apk` from GitHub Releases
```

Host projects install **OpenXR / Meta Quest / Input System** (and related XR packages) via **their** `Packages/manifest.json`. Those platform packages are declared as dependencies in the plugin’s `package.json` but are not copied into this tree.

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>🧩 Core Components</b></summary>

| Shortcut | Class | Role |
|----------|--------|------|
| `Lexting.input` | `SpatialGestureInput` | Consolidates input adapters (OpenXR target); publishes gesture and rotation changes. |
| `Lexting.printer` | `LextingOutputManager` | Receives completed characters/words/commands; updates UI fields or Windows key simulation. |
| `Lexting.controller(right)` | `TwoZoneCubeController` | Per-hand typing **volume** (2×2×2 **nodes**): hover, insert, drag, hotkey interaction. |
| `Lexting.controllers` | `TwoZoneCubeControllers` | Parent of both hands; shared trigger state and bounds. |
| *(per node)* | `TwoZoneTrigger` | One letter **node** in the volume; exposes `currentRightHighlightedLetter` / `currentLeftHighlightedLetter`. |
| `Lexting.assembler` | `CubeWordAssemblyController` | Builds the candidate word from grab/drag data; queries the dictionary. |
| `Lexting.finalizer` | `TwoZoneWordListController` | **Finalization mode**: word choice (left **volume**), punctuation (right **volume**), paging. |
| `Lexting.conjugator` | `LextingConjugationState` | Maps two-hand wrist orientation to conjugation during finalization. |
| *(static)* | `LextingDictionary` | Loads English JSON and provides fast prefix lookup. |
| `Lexting.line(right)` | `CubeDragLineVisualizer` | Line renderer from drag start **node** to current **node** while dragging. |
| *(per hand)* | `HandCursor` | Point cursor offset from the hand; hover sensing without embedding the hand in the UI. |
| `Lexting.rHandMarker` / `lHandMarker` | `HandMarker` | Hand transform references for the rest of the stack. |
| `HeadMarker` | `HeadMarker` | Head transform reference. |
| `Lexting.hotkeys` | `LextingHotkeyManager` | Dual-hand hotkeys (both hands on the same **node**, matching rotation). |

**Pingleton vs singleton:** Some types use a **Pingleton** (property-named singleton) so inheritance is not blocked. **`Lexting`** collects the common entry points.

**UniRx:** Extension methods in `EventUtils` and elsewhere subscribe to events and spawn update loops without boilerplate `OnEnable` / `Update` / coroutines. If lifecycle methods seem “missing,” check UniRx usage ([UniRx](https://github.com/neuecc/UniRx)).

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>🔀 Data Flow</b></summary>

```mermaid
flowchart LR
  OXR[OpenXR input adapter] --> SGI[SpatialGestureInput]
  SGI --> TZC[TwoZoneCubeController]
  TZC --> TZT[TwoZoneTrigger nodes]
  TZC --> TGS[TwoZoneGrabState]
  TGS --> CWA[CubeWordAssemblyController]
  CWA --> LD[LextingDictionary]
  CWA --> TWL[TwoZoneWordListController]
  TWL --> LOM[LextingOutputManager]
  LOM --> UI[InputField / Text receivers]
  LOM --> WIN[Windows InputSimulator]
```

</details>
</blockquote>

---

<blockquote>
<a id="scenes"></a>
<details>
<summary><b>🎬 Sample Scenes</b></summary>
<hr style="border: 1px solid gray;">

Lexting is designed to be integrated into your scenes via the Setup Wizard (file menu `Lexting->Setup`), but the following scene allows quick testing and comparison because Lexting is already preconfigured here.

| Scene | Notes |
|-------|--------|
| `main_01_twozone.unity` | **Primary** reference scene (package: **`Lexting_Unity_Plugin/Scenes/`**) — dual-volume UI + in-scene Descriptor docs |

</details>
</blockquote>

</details>

<hr style="border: 1px solid gray;">

<a id="getting-started"></a>
<details>
<summary><b>🚀 Getting Started</b></summary>
<hr style="border: 1px solid gray;">

<blockquote>
<details>
<summary><b>📲 Sideload Tutorial Demo</b></summary>
<hr style="border: 1px solid gray;">

Sideload the provided `tutorial_demo.apk` on Meta Quest to try Lexting directly. Find it in `Lexting_Unity_Plugin/Releases/tutorial_demo.apk` or on the repo’s [GitHub Releases](https://github.com/scifiuiguy/Lexting/releases) page.

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>📦 Install the Unity Package and Build</b></summary>
<hr style="border: 1px solid gray;">

1. Install **Unity 6** LTS or higher and open **`Lexting_Unity/`**.
3. Add **`Lexting_Unity_Plugin`** to **`Packages/manifest.json`** or use the file menu option `Window -> Package Manager -> + Dropdown -> Install package from git URL -> paste "git@github.com:scifiuiguy/Lexting.git"`.
4. **`main_01_twozone`** is the fully wired reference scene displayed in tutorial_demo.apk. It can be found in **Packages → Lexting → Scenes**.
5. Open your own scene with an **XR-ready player rig** (OpenXR + XRI). Create a player rig if you don't have one (see Unity OpenXR documentation).
6. Choose the file menu option **Lexting → Setup Scene** — The Lexting Wizard will configure components (especially the `LextingInputModuleManager`) in your open scene, as long as you have an OpenXR-compliant rig in that scene. If you don't, the wizard will throw warnings of incomplete setup. Compare your configuration to the main_01_twozone scene if in doubt. Lexting components have detailed descriptors to help you along.
7. Add a **`ZoneFieldOutputReceiver`** on each **`InputField`** you wish to type into at runtime.
8. Configure Player, Project, and Build Settings per the target platform's documentation (e.g. use Meta's OpenXR documentation for Quest).
9. **File → Build Settings → Android** (main_01_twozone enabled); build, and sideload or Build And Run if you have a live link to Unity.

</details>
</blockquote>

---

<blockquote>
<details>
<summary><b>🔌 Subscribe to Lexting output in code (optional)</b></summary>
<hr style="border: 1px solid gray;">

Call **`this.GetLextingOutput`** (all events) or **`this.Subscribe`** (one event) in **`Awake`** on any `MonoBehaviour` to receive text output from Lexting. Call these Lexting output subscriptions in **`Awake`**, not in `OnEnable`, not in `Start`, and definitely not in `Update`. You do not need to unsubscribe to these. Lexting event objects self-sub and self-unsub during OnEnable and OnDisable in the background. Call once in Awake to activate the entire Sub/Unsub workflow.

If you call `GetLextingOutput` directly, you must declare **every required callback as below**. Pass no-op lambdas (`() => { }`) for events you don't intend to implement immediately. Use this kitchen-sink call only if you intend to implement most/all of the callbacks:

```csharp
void Awake()
{
    this.GetLextingOutput(
        tabCallback: () => { /* TAB happened - DO STUFF HERE */ },
        altTabCallback: () => { /* ALT+TAB happened - etc. */ },
        altShiftTabCallback: () => { },
        enterCallback: () => { },
        deleteCallback: () => { },
        backspaceCallback: () => { },
        backWordCallback: (numCharsRemoved) => { },
        undoCallback: () => { },
        redoCallback: () => { },
        cursorMoveCallback: (direction) => { },
        cursorShiftMoveCallback: (direction, highlightedString) => { },
        cursorShiftWordMoveCallback: 
          (direction, numCharsJustSelected, highlightedString) => { },
        altCallback: (on) => { },
        ctrlCallback: (on) => { },
        shiftCallback: (on) => { },
        capsCallback: (on) => { },
        keyCallback: (key) => { }
    );
}
```

If you prefer to sub to **One event at at time** — use `this.Subscribe` instead:

```csharp
void Awake()
{
    this.Subscribe(Lexting.printer.OnEnter, () => { /* ENTER happened - DO STUFF HERE */ });
}
```

</details>
</blockquote>

</details>

<hr style="border: 1px solid gray;">

<a id="deprecated"></a>
<details>
<summary><b>🗄️ Deprecations</b></summary>
<hr style="border: 1px solid gray;">

* **`ControllerType.ThreeZone`** and scripts under `LetterCube/Deprecated/` — superseded by the two-volume typing widget (`TwoZoneCubeController`).
* **`CubeHandTrackingVisualizer`** — fully commented out; **`HandCursor`** replaces it for two-volume mode.
* **MLAPI streaming** — largely commented; treat as work-in-progress snapshot.
* **Retired `InputMode` values and descriptor stubs** — older experiments remain in the codebase but are unmaintained; new work should follow the [OpenXR-first platform targets](#platform-strategy).
* **`QUEST_*` / Oculus-specific input** — legacy prototype paths; prefer **`OPEN_XR`** and avoid extending these for new features.

Third-party bundles shipped for demos (Holoplay Looking Glass, Pixelplacement Surge, WindowsInput, UniRx, etc.) retain their own licenses inside those folders.

</details>

<hr style="border: 1px solid gray;">

