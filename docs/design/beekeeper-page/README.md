# Beekeeper page – static design (demo)

`beekeeper-page.html` is the new design of the public beekeeper page (today: `Views/Storefront/Index.cshtml`).
It is plain HTML/CSS with inline SVG, no JavaScript. Open it in a browser to see it. All data in it is sample data.

## Files
- `beekeeper-page.html` – the page (all styles inline; fonts: Bricolage Grotesque + Instrument Sans from Google Fonts)
- `images/apiary.jpg`, `images/harvest.jpg`, `images/comb.jpg` – AI-generated demo photos (1600 px, JPEG)

## Sections and where the data comes from
| Section | Real data source | Demo-only today |
|---|---|---|
| Header tags (honey types, region) | honey types of the beekeeper's verified lots; region from province | – |
| Name, tagline, location, association, "on the platform since" | storefront + beekeeper + association membership; location = district + province only | tagline |
| Badges (AKS verified, accredited lab, all CCPs approved) | AKS enterprise no. verified; any approved report from an accredited lab; all CCPs of the lots approved | – |
| Recent activity (latest batch, latest lab report, CCP0) | latest finalised batch containing the honey; latest approved report; latest CCP0 status | – |
| Profile card stats (verified lots, batches, lab reports, kg traced) | counts/sums over verified lots, finalised batches, approved reports | – |
| About the apiary (story, 3 feature boxes) | storefront story text written by the association (with consent) | text |
| Photo gallery | storefront photos uploaded by the association with consent | all three photos |
| Map | inline SVG of Türkiye (viewBox 0 0 640 300); marker at the district/province centroid, never the apiary | – |
| Flowering and harvest calendar | honey types × production periods of the lots | flora rows |
| Latest verified lab results | values of the latest approved report vs. the active parameter limits | – |
| Batches with this honey | finalised batches using the beekeeper's lots, share = kg from her lots / batch kg; link = Honey Story | – |
| References | organisations with recorded relationships (association, processor, lab, retailer) + counts | association recommendation text |

## Map marker position
The SVG path uses a Mercator projection fitted to 640×300. For a centroid at (lat, lon) in degrees:
```
x = -821.154 + 1855.143 * lon * PI/180
y = 1524.674 - 1855.143 * ln(tan(PI/4 + lat * PI/360))
```
(Akçakoca: x ≈ 186.5, y ≈ 62.9; Ankara: x ≈ 242.5, y ≈ 112.3.)

## Rules that must stay
- Page is shown only when the storefront is published and consent is recorded; withdrawn consent hides it.
- Never show phone number, personal photo (faces), birth date or exact apiary location.
- Contact goes through the association, not directly to the beekeeper.
- No customer reviews or star ratings; references come only from recorded relationships.
- All visible text goes through the existing localisation (EN/TR/ES/PL/LV).
