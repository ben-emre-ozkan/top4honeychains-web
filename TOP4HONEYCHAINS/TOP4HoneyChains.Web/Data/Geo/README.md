# Türkiye province and district maps (D-72, D-98)

Both files are embedded in the assembly; pages make no external requests and load no map tiles.

## Provinces: `turkey-provinces.json`

The 81 provinces of Türkiye as SVG paths for the shared map component
(`Views/Shared/_TurkeyMap.cshtml`, `Services/TurkeyMap.cs`).

- **Source:** Natural Earth, 1:10m Cultural Vectors, *Admin 1 – States, Provinces* (naturalearthdata.com;
  file `ne_10m_admin_1_states_provinces.geojson` from github.com/nvkelso/natural-earth-vector). Natural Earth is in the
  **public domain**; no permission or attribution is required, it is credited here anyway.
- **Processing:** `scripts/turkey-map.py` keeps the features with `adm0_a3 = TUR` (81 provinces, ISO 3166-2 `TR-01` … `TR-81`,
  i.e. the plate codes), simplifies them with mapshaper (`-simplify percentage=0.12 keep-shapes`), dissolves the outline,
  and projects everything to the 640 × 300 Mercator map of `ProvinceMap.ToMap`. Each province keeps its path and the
  centroid of its largest polygon.
- **Precision:** about 1–2 km at this scale. Good enough to place a point in a province for counts; borders are drawn,
  never used as a legal boundary.

## Districts: `turkey-districts.json` (D-98)

The 973 districts, keyed by plate and by the district name of the `Districts` table (migration 011), for the drill-down
of the province map, the district outline on the Honey Story and the district counts of the dashboard
(`Services/TurkeyDistricts.cs`, `Views/Shared/_DistrictOutline.cshtml`).

- **Source:** geoBoundaries, gbOpen, Türkiye ADM2 (geoboundaries.org, API `api/current/gbOpen/TUR/ADM2/`, build of
  Nov 2026, boundary year 2021). geoBoundaries takes this layer from **OpenStreetMap**; its licence is the
  **Open Data Commons Open Database License (ODbL) 1.0**, not CC BY.
- **Licence obligations:** every map that shows the districts carries the attribution line (`Map.DistrictCredit`,
  "District boundaries: geoBoundaries, © OpenStreetMap contributors (ODbL)", linked to openstreetmap.org/copyright).
  `turkey-districts.json` is a derived database: it is made available under the ODbL 1.0 as well
  (https://opendatacommons.org/licenses/odbl/1-0/); the rest of the application is not affected.
- **Processing:** `scripts/turkey-districts.py <geoBoundaries-TUR-ADM2.geojson> [work folder]` places each shape in a
  province by an interior point (mapshaper `-points inner`) on the province map, matches it by name to the district list
  of `db/migrations/011_provinces_districts.sql` (Turkish lower case; "<Province> merkez" → "Merkez"; a `RENAMES` table for
  nine shapes whose spelling differs or whose point falls over a border, e.g. "Prince Islands" → Adalar, "Imbros" →
  Gökçeada), simplifies with mapshaper (`-simplify interval=700 keep-shapes`; shared borders stay shared) and projects to
  the same 640 × 300 map, so districts line up with the provinces. Paths use relative steps of 0.1 map unit
  (`M x y l dx dy …z`). The script stops if any district of the list has no shape or a shape has no district.
- **Precision:** about 0.7 km. Natural Earth and OpenStreetMap draw some province borders differently by up to a few km;
  a point is placed in the district of its province that holds it, else the nearest within about 30 km.
- **Rebuild:** download the `gjDownloadURL` named by the geoBoundaries API into an empty folder, run the script with
  Python 3 and Node (npx mapshaper), and check `TurkeyDistrictsTests`.

The maps show provinces and districts only, never an apiary's coordinates: positions are turned into a province or a
district on the server.
