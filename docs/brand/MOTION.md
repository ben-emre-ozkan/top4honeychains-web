# TOP4HoneyChains motion system

Motion has one job: show that something happened, or welcome someone at a brand moment. It never decorates a screen where people are entering data. Recorded as D-70; the code lives in `wwwroot/css/honey-mark.css`, `wwwroot/js/honey-mark.js`, `wwwroot/js/landing.js` and the "D-70 motion system" block of `wwwroot/css/app-skin.css`.

## Three levels

| Level | Where | Motion | Timing |
|---|---|---|---|
| **Brand moments**: expressive | Logo, landing page, sign-in, loader, empty states, success moments | The animated mark (lift, wing flutter, halo, light sweep, pollen), scroll reveals, the honey line, the hero honeycomb, the background video | Logo moment 620 ms with a 1.35 s sweep; loops 2.4–5.4 s; reveals 700 ms |
| **Feedback**: subtle | Buttons, toasts, the My work badge glow, a status pill that just changed, row hover | One short move or fade: lift by 1–2 px, scale to 1.04–1.15 and back, fade a glow in and out | **150–250 ms** (`--b-fast` = 160 ms); the badge glow rises once |
| **Data entry**: still | CCP values, lab results, tables, every form | Nothing decorative. Fields, rows and colours change at once. Nothing moves while someone types, and counters in forms are never animated | none |

## Rules

1. **Only `transform` and `opacity` are animated.** Colours, borders, shadows, sizes and filters change instantly. A glow that should appear is a pseudo-element or layer with a fixed shadow whose opacity changes.
2. **`prefers-reduced-motion: reduce` is respected everywhere.** The mark is static; the loader shows the static mark with its text line; reveals are shown at once; the honey line is drawn; the video shows its poster only.
3. **Loops pause when nobody can see them**: off screen (IntersectionObserver), in a hidden tab (`html.is-hidden-tab`), and while focus is in a form field (`html.is-typing`). The honeycomb canvas and the video stop drawing as well.
4. **Nothing moves while someone types.** Loops pause, and no feedback animation is attached to typing.
5. **One brand moment at a time.** The top-bar mark plays once per session after sign-in, and again only on a success moment.

## The animated mark

`<honey-mark mode="…" size="…" label="…" />` (tag helper `TagHelpers/HoneyMarkTagHelper.cs`) inlines `docs/brand/top4-mark-animated.svg`, with a prefix on every id and `url(#…)` so copies can share a page. Without a `label` the mark is decorative (inside a link that has a name).

| Mode | Use | Behaviour |
|---|---|---|
| `hover` (default) | Top bars | The master's moment on hover, and on keyboard focus of the surrounding `.th-logo-trigger` link |
| `once` | Landing hero, sign-in panel | Plays once after load, then on hover; `data-hm-once="(media query)"` limits it, e.g. the sign-in form logo plays only on phones, where the panel is hidden |
| `loop` (`is-looping`) | Page loader | Wings flutter, the bee bobs, the halo breathes, the light sweep repeats about every 3 s |
| `slow` (`is-looping is-slow`) | Empty states, 403/404/500 pages | The same, calmer (sweep about every 5 s) |

The top-bar mark (`data-hm-intro`) plays once per browser session after sign-in (the sign-in page resets it) and once on a success moment: a status pill approved just now (CCP or lab approval) or a freshly sealed batch. The static files (`top4-mark*.svg`, the logos and the favicon set) stay for the favicon, print, PDFs and e-mails.

## The loader

`_Loader` partial, in the app and sign-in layouts. It shows the looping mark centred on cream with the honey glow and a short localised line:

- only when a page load or a form post takes longer than **400 ms**;
- at once for long actions: lab PDF upload (`data-loader="upload"`), report submission (`report`), batch finalisation (`seal`) and the PDF export of the admin tables (`pdf`);
- `aria-busy="true"` on the body while it shows, and the line in a polite live region;
- not for new tabs, downloads (`download`, `data-no-loader`), hash links, menus, or a submit that client validation stopped; hidden again on back and forward navigation.

## Canvas animations (D-71, the landing hero)

- Draw what does not change once (an offscreen canvas) and copy it each frame; look cells up through a grid, never by searching every cell.
- Build after `load`, in idle time, and split layout reads from heavy drawing so no task exceeds 50 ms (check with 4× CPU throttling).
- Cap the device pixel ratio at 2; no `shadowBlur` per frame (draw a wide soft stroke under the core stroke instead).
- Pointer effects for mouse and pen only; touch gets the tap effect. Stop the loop off screen and in a hidden tab; reduced motion draws one static frame.
- Text over a canvas keeps its contrast: damp the effect behind the copy, or give the text a light plate.

## The map, the admin home and the Honey Story (D-72)

- **Türkiye map** (`_TurkeyMap`, `tmap.css`, `tmap.js`): the outline draws once when the map first scrolls into view (stroke-dashoffset, the one
  exception to rule 1; it runs once), a Honey Story map fills in its provinces by opacity; hover, tap or keyboard focus fades in a lifted copy
  of the province over a fixed glow (opacity and transform); the honey dots pulse with transform and opacity and pause off screen, in a hidden
  tab and while typing.
- **Admin home**: cards rise once from a visible resting state (opacity 0.55, 10 px; CSS only, so nothing can stay hidden); figures count up
  once when visible (the final value is in the HTML); the chain strip runs the landing hero engine at a smaller scale (`data-comb-r`).
- **Honey Story**: the bee plays once, the hero glow fades in once; journey steps light up as they scroll in and the light path grows with them;
  meters and share bars move to their values once; the verified band stamps its seal once. Step photos are decorative backgrounds only.
- Reduced motion: all of it is shown at rest (the page is not armed).

## Checklist for new screens

- Is it a data-entry screen? Then add no motion.
- Feedback? 150–250 ms, transform or opacity, and a reduced-motion rule that turns it off.
- A loop? It must pause off screen, in a hidden tab and while typing.
- Is a colour, shadow or size animated? Use a layer whose opacity changes instead.
