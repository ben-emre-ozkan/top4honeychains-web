"""Builds the Türkiye province map (D-72): TOP4HoneyChains.Web/Data/Geo/turkey-provinces.json.

Source: Natural Earth 1:10m "Admin 1 - States, Provinces" (public domain), e.g.
https://raw.githubusercontent.com/nvkelso/natural-earth-vector/master/geojson/ne_10m_admin_1_states_provinces.geojson

    python scripts/turkey-map.py <ne_10m_admin_1_states_provinces.geojson> [work folder]

Keeps Türkiye's 81 provinces (adm0_a3 = TUR), simplifies them with mapshaper (npx, keep-shapes, 12 %),
dissolves the outline, and projects both to the 640 x 300 Mercator map of ProvinceMap.ToMap. Each province keeps
its plate code (ISO 3166-2 TR-01 .. TR-81), its SVG path and the centroid of its largest polygon.
"""
import json
import math
import os
import subprocess
import sys

SIMPLIFY = '12%'
HERE = os.path.dirname(os.path.abspath(__file__))
TARGET = os.path.join(HERE, '..', 'TOP4HONEYCHAINS', 'TOP4HoneyChains.Web', 'Data', 'Geo', 'turkey-provinces.json')


def project(lon, lat):
    # the same projection as Services/ProvinceMap.cs ToMap
    x = -821.154 + 1855.143 * lon * math.pi / 180
    y = 1524.674 - 1855.143 * math.log(math.tan(math.pi / 4 + lat * math.pi / 360))
    return x, y


def ring_path(ring):
    points = [project(lon, lat) for lon, lat in ring]
    out, last = [], None
    for x, y in points:
        p = (round(x, 1), round(y, 1))
        if p == last:
            continue
        out.append(('M' if not out else 'L') + f'{p[0]:g} {p[1]:g}')
        last = p
    return ''.join(out) + 'Z', points


def centroid(points):
    a = cx = cy = 0.0
    for i in range(len(points) - 1):
        x0, y0 = points[i]
        x1, y1 = points[i + 1]
        c = x0 * y1 - x1 * y0
        a += c
        cx += (x0 + x1) * c
        cy += (y0 + y1) * c
    a /= 2
    return (cx / (6 * a), cy / (6 * a), abs(a)) if a else (points[0][0], points[0][1], 0.0)


def polygons(geometry):
    return geometry['coordinates'] if geometry['type'] == 'MultiPolygon' else [geometry['coordinates']]


def geometries(obj):
    if obj['type'] == 'FeatureCollection':
        return [f['geometry'] for f in obj['features']]
    if obj['type'] == 'GeometryCollection':
        return obj['geometries']
    return [obj.get('geometry', obj)]


def main():
    source = sys.argv[1]
    work = sys.argv[2] if len(sys.argv) > 2 else os.path.dirname(os.path.abspath(source))
    with open(source, encoding='utf-8') as f:
        world = json.load(f)
    turkey = [f for f in world['features'] if f['properties'].get('adm0_a3') == 'TUR']
    if len(turkey) != 81:
        sys.exit(f'expected 81 provinces, found {len(turkey)}')
    raw, simple, outline = (os.path.join(work, n) for n in ('tr_admin1.geojson', 'tr_simple.geojson', 'tr_outline.geojson'))
    with open(raw, 'w', encoding='utf-8') as f:
        json.dump({'type': 'FeatureCollection', 'features': turkey}, f)
    npx = 'npx.cmd' if os.name == 'nt' else 'npx'
    # relative names in the work folder: mapshaper derives layer names from the paths; percentage=… (a '%' is lost in npx.cmd)
    subprocess.run([npx, '--yes', 'mapshaper', os.path.basename(raw), '-simplify', 'percentage=' + str(int(SIMPLIFY[:-1]) / 100), 'keep-shapes',
                    '-filter-fields', 'iso_3166_2,name', '-o', 'format=geojson', 'precision=0.0001', os.path.basename(simple), 'force'], check=True, cwd=work)
    subprocess.run([npx, '--yes', 'mapshaper', os.path.basename(simple), '-dissolve', '-o', 'format=geojson', 'precision=0.0001',
                    os.path.basename(outline), 'force'], check=True, cwd=work)

    with open(simple, encoding='utf-8') as f:
        features = json.load(f)['features']
    provinces = []
    for feature in features:
        code = feature['properties']['iso_3166_2'].split('-')[1]
        paths, best = [], None
        for polygon in polygons(feature['geometry']):
            for k, ring in enumerate(polygon):
                d, points = ring_path(ring)
                paths.append(d)
                if k == 0:
                    c = centroid(points)
                    if best is None or c[2] > best[2]:
                        best = c
        provinces.append({'code': code, 'd': ''.join(paths), 'cx': round(best[0], 1), 'cy': round(best[1], 1)})
    provinces.sort(key=lambda p: p['code'])
    with open(outline, encoding='utf-8') as f:
        border = ''.join(ring_path(polygon[0])[0] for g in geometries(json.load(f)) for polygon in polygons(g))

    data = {
        'source': 'Natural Earth 1:10m Admin 1 - States, Provinces (public domain, naturalearthdata.com), Türkiye only '
                  f'(81 provinces, ISO 3166-2 TR-01..TR-81), simplified to {SIMPLIFY} with mapshaper (keep-shapes), '
                  'projected to the 640x300 Mercator map of ProvinceMap.ToMap; built by scripts/turkey-map.py',
        'viewBox': '0 0 640 300',
        'outline': border,
        'provinces': provinces,
    }
    os.makedirs(os.path.dirname(TARGET), exist_ok=True)
    with open(TARGET, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(data, f, ensure_ascii=False, separators=(',', ':'))
    print(f'{len(provinces)} provinces -> {os.path.normpath(TARGET)} ({os.path.getsize(TARGET)} bytes)')


if __name__ == '__main__':
    main()
