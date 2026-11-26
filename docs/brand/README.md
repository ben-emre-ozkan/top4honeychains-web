# TOP4HoneyChains brand kit

**Current logo (2026-11-26):** see [logo-2026-11/README.md](logo-2026-11/README.md); it replaces the drawn marks below on every page.

The mark: a bee whose wings open behind three linked hexagons (the value chain and the honeycomb), with a turquoise head. `top4-mark-animated.svg` is the master drawing; every static file here is derived from it. Text in the logos is converted to outlines, so no font is needed.

## Files

| File | Use |
|---|---|
| `top4-mark-animated.svg` | Animated master (hover / `.is-active`: the bee lifts, wings flutter, a warm halo and a light sweep appear, pollen rises). Respects `prefers-reduced-motion`. |
| `top4-mark.svg` | Static mark with wing details. 48 px and larger. |
| `top4-mark-small.svg` | Static mark without wing details, for sizes under 48 px. |
| `top4-mark-mono.svg` | One-colour caramel version for stamps, PDFs and single-colour print. |
| `top4-mark-square.svg`, `top4-mark-small-square.svg` | Marks on a square canvas, for icon generation. |
| `top4-logo-w3.svg` | Mark + W3 wordmark (TOP4HONEYCHAINS with hexagon O's). Default horizontal logo. |
| `top4-logo-w2.svg` | Mark + W2 wordmark (TOP4 over HONEY ◆ CHAINS). Compact horizontal logo. |
| `top4-logo-stacked.svg` | Mark above W2. Square spaces, social media, posters, cover pages. |
| `top4-wordmark-w3.svg`, `top4-wordmark-w2.svg` | Wordmarks without the mark. |
| `favicon.ico` (16/32/48), `favicon-32.png` | Browser favicon. |
| `apple-touch-icon.png` (180, white) | iOS home screen. |
| `icon-192.png`, `icon-512.png`, `icon-maskable-512.png` | Web app manifest icons (the last one with `"purpose": "maskable"`). |
| `brand-preview.png` | Overview. |

## Colours

| Role | Hex |
|---|---|
| Rings | top #FFDC60 → #FFB52B, right #FFB538 → #F38B12, left #D39A50 → #B57B39 |
| Body | #FFBD31 → #EF9211, stripes #8C5D27, thorax #C79148 → #AB7636 |
| Head | #24BFB7 (turquoise) |
| Wings | #FFF0C9 with #B47C38 outline |
| Wordmark | caramel #B07838, deep honey #EE8A12, turquoise diamond #2BC4BC |

## Rules

- Do not recolour, stretch, rotate or redraw the mark in CSS.
- Clear space: at least the width of the bee's head on every side.
- White or light honey-foam backgrounds only.
- Logos are exempt from WCAG text-contrast rules; keep the colours as they are.
- When the animated SVG is inlined more than once on a page, give each copy its own id prefix (the ids inside it must be unique).
