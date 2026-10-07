# Türkiye province map (D-72)

`turkey-provinces.json` holds the 81 provinces of Türkiye as SVG paths for the shared map component
(`Views/Shared/_TurkeyMap.cshtml`, `Services/TurkeyMap.cs`). It is embedded in the assembly; pages make no external requests.

- **Source:** Natural Earth, 1:10m Cultural Vectors, *Admin 1 – States, Provinces* (naturalearthdata.com;
  file `ne_10m_admin_1_states_provinces.geojson` from github.com/nvkelso/natural-earth-vector). Natural Earth is in the
  **public domain**; no permission or attribution is required, it is credited here anyway.
- **Processing:** `scripts/turkey-map.py` keeps the features with `adm0_a3 = TUR` (81 provinces, ISO 3166-2 `TR-01` … `TR-81`,
  i.e. the plate codes), simplifies them with mapshaper (`-simplify percentage=0.12 keep-shapes`), dissolves the outline,
  and projects everything to the 640 × 300 Mercator map of `ProvinceMap.ToMap`. Each province keeps its path and the
  centroid of its largest polygon.
- **Precision:** about 1–2 km at this scale. Good enough to place a point in a province for counts; borders are drawn,
  never used as a legal boundary.

The map never shows anything finer than a province (the beekeeper page names the district as before), and never an apiary's coordinates.
