"""Builds the Türkiye district map (D-98): TOP4HoneyChains.Web/Data/Geo/turkey-districts.json.

Source: geoBoundaries gbOpen, Türkiye ADM2 (973 districts; data from OpenStreetMap, Open Data Commons Open Database
License 1.0), e.g. the gjDownloadURL of https://www.geoboundaries.org/api/current/gbOpen/TUR/ADM2/

    python scripts/turkey-districts.py <geoBoundaries-TUR-ADM2.geojson> [work folder]

Each district is placed in its province by an interior point (mapshaper -points inner) tested against the province map
(turkey-provinces.json, built by scripts/turkey-map.py), then matched by name to the district list of
db/migrations/011_provinces_districts.sql (the Districts table): Turkish lower case, "<Province> merkez" -> "Merkez",
plus the RENAMES table below. The geometry is simplified with mapshaper (shared borders stay shared) and projected to the
640 x 300 Mercator map of ProvinceMap.ToMap, so the districts line up with the provinces. The script stops when a
district of the list has no shape or a shape has no district.
"""
import json
import math
import os
import re
import subprocess
import sys

INTERVAL = 700   # mapshaper -simplify interval in metres
HERE = os.path.dirname(os.path.abspath(__file__))
WEB = os.path.join(HERE, '..', 'TOP4HONEYCHAINS', 'TOP4HoneyChains.Web')
PROVINCES = os.path.join(WEB, 'Data', 'Geo', 'turkey-provinces.json')
SEED = os.path.join(HERE, '..', 'db', 'migrations', '011_provinces_districts.sql')
TARGET = os.path.join(WEB, 'Data', 'Geo', 'turkey-districts.json')

# geoBoundaries name (normalised by norm) -> (plate, name in the Districts table), where the spelling differs or the
# interior point falls into the neighbouring province on the simplified province map
RENAMES = {
    'karakeçeli': (71, 'Karakeçili'),
    'ibradi': (7, 'İbradı'),
    'şarkîkaraağaç': (32, 'Şarkikaraağaç'),
    'ılıç': (24, 'İliç'),
    'giresun district': (28, 'Merkez'),
    'ulukisla': (51, 'Ulukışla'),
    'ımbros': (17, 'Gökçeada'),
    'ayvacik': (17, 'Ayvacık'),
    'prince ıslands': (34, 'Adalar'),
}


def project(lon, lat):
    # the same projection as Services/ProvinceMap.cs ToMap
    x = -821.154 + 1855.143 * lon * math.pi / 180
    y = 1524.674 - 1855.143 * math.log(math.tan(math.pi / 4 + lat * math.pi / 360))
    return x, y


def tenths(v):
    s = f'{v / 10:g}'
    return s.replace('0.', '.', 1) if s.startswith('0.') else s.replace('-0.', '-.', 1)


def ring_path(ring):
    """'M12.3 45.6l.4-1.2 1.3…z': the first point absolute, the others relative to the previous rounded point (tenths of
    a map unit, so the relative steps add up exactly); returns the path, the projected points and the vertex count"""
    points = [project(lon, lat) for lon, lat in ring]
    steps, last = [], None
    for x, y in points:
        p = (round(x * 10), round(y * 10))
        if p != last:
            steps.append(p)
            last = p
    if len(steps) > 1 and steps[-1] == steps[0]:
        steps.pop()
    if len(steps) < 3:
        return '', points, 0
    numbers = [tenths(steps[0][0]), tenths(steps[0][1])]
    for (x0, y0), (x1, y1) in zip(steps, steps[1:]):
        numbers += [tenths(x1 - x0), tenths(y1 - y0)]
    out = 'M'
    for i, n in enumerate(numbers):
        if i == 2:
            out += 'l'
        elif i and not (n.startswith('-') or (n.startswith('.') and '.' in numbers[i - 1])):
            out += ' '   # a separator only where the SVG number grammar needs one
        out += n
    return out + 'z', points, len(steps)


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


LOWER = str.maketrans({'I': 'ı', 'İ': 'i'})


def norm(name):
    return re.sub(r'\s+', ' ', name.translate(LOWER).lower().replace('(', ' ').replace(')', ' ')).strip()


def seed_districts():
    """{plate: [names]} from the INSERT dbo.Districts statements of 011"""
    with open(SEED, encoding='utf-8-sig') as f:
        sql = f.read()
    out = {}
    for code, values in re.findall(r"SELECT '(\d\d)', v\.Name FROM \(VALUES (.*?)\) v\(Name\)", sql, re.S):
        out[int(code)] = re.findall(r"N'((?:[^']|'')*)'", values)
    return out


def province_rings():
    with open(PROVINCES, encoding='utf-8') as f:
        data = json.load(f)
    rings = {}
    for p in data['provinces']:
        rings[int(p['code'])] = [[tuple(map(float, pt.split(' '))) for pt in re.split('[ML]', r) if pt]
                                 for r in p['d'].split('Z') if r]
    return data['source'], rings


def inside(rings, x, y):
    hit = False
    for ring in rings:
        j = len(ring) - 1
        for i in range(len(ring)):
            (xi, yi), (xj, yj) = ring[i], ring[j]
            if (yi > y) != (yj > y) and x < (xj - xi) * (y - yi) / (yj - yi) + xi:
                hit = not hit
            j = i
    return hit


def distance(rings, x, y):
    best = float('inf')
    for ring in rings:
        for (ax, ay), (bx, by) in zip(ring, ring[1:] + ring[:1]):
            length = (bx - ax) ** 2 + (by - ay) ** 2
            t = 0 if length == 0 else max(0, min(1, ((x - ax) * (bx - ax) + (y - ay) * (by - ay)) / length))
            best = min(best, (ax + t * (bx - ax) - x) ** 2 + (ay + t * (by - ay) - y) ** 2)
    return math.sqrt(best)


def match(plate, province, raw, names):
    key = norm(raw)
    for suffix in (' merkezi', ' merkez', ' merkez ilçe'):
        if key.endswith(suffix):
            key = key[: -len(suffix)].strip()
            if key == norm(province):
                key = 'merkez'
            break
    if key in ('merkez', norm(province)) and 'Merkez' in names:
        return 'Merkez'
    for n in names:
        if norm(n) == key:
            return n
    return None


def main():
    source = sys.argv[1]
    work = sys.argv[2] if len(sys.argv) > 2 else os.path.dirname(os.path.abspath(source))
    with open(source, encoding='utf-8') as f:
        features = json.load(f)['features']
    for i, feature in enumerate(features):
        feature['properties'] = {'idx': i, 'name': feature['properties']['shapeName']}
    raw, points, simple = (os.path.join(work, n) for n in ('tr_adm2.json', 'tr_adm2_points.json', 'tr_adm2_simple.json'))
    with open(raw, 'w', encoding='utf-8') as f:
        json.dump({'type': 'FeatureCollection', 'features': features}, f)
    npx = 'npx.cmd' if os.name == 'nt' else 'npx'
    subprocess.run([npx, '--yes', 'mapshaper', os.path.basename(raw), '-points', 'inner', '-o', 'format=geojson',
                    os.path.basename(points), 'force'], check=True, cwd=work)
    subprocess.run([npx, '--yes', 'mapshaper', os.path.basename(raw), '-simplify', f'interval={INTERVAL}', 'keep-shapes',
                    '-o', 'format=geojson', 'precision=0.0001', os.path.basename(simple), 'force'], check=True, cwd=work)

    _, provinces = province_rings()
    seed = seed_districts()
    if sum(len(v) for v in seed.values()) != 973 or len(seed) != 81:
        sys.exit('011_provinces_districts.sql: expected 973 districts in 81 provinces')
    names = {p: v for p, v in seed.items()}
    province_names = {}
    with open(SEED, encoding='utf-8-sig') as f:
        for code, name in re.findall(r"\('(\d\d)', N'([^']+)'\)", f.read()):
            province_names[int(code)] = name

    with open(points, encoding='utf-8') as f:
        where = {}
        for pt in json.load(f)['features']:
            lon, lat = pt['geometry']['coordinates']
            x, y = project(lon, lat)
            plate = next((p for p, r in provinces.items() if inside(r, x, y)), 0)
            if not plate:
                plate = min(provinces, key=lambda p: distance(provinces[p], x, y))
            where[pt['properties']['idx']] = plate

    with open(simple, encoding='utf-8') as f:
        shapes = json.load(f)['features']
    out, problems, used = {}, [], set()
    vertices = 0
    for feature in shapes:
        idx, raw_name = feature['properties']['idx'], feature['properties']['name']
        plate = where[idx]
        if norm(raw_name) in RENAMES:
            plate, name = RENAMES[norm(raw_name)]
        else:
            name = match(plate, province_names[plate], raw_name, names[plate])
        if name is None:
            # the interior point fell into the neighbouring province on the simplified map: try the provinces nearby by name
            for other, other_names in names.items():
                candidate = match(other, province_names[other], raw_name, other_names)
                if candidate and (other, candidate) not in used:
                    x, y = project(*feature['geometry']['coordinates'][0][0][0] if feature['geometry']['type'] == 'MultiPolygon'
                                   else feature['geometry']['coordinates'][0][0])
                    if distance(provinces[other], x, y) < 15 or inside(provinces[other], x, y):
                        plate, name = other, candidate
                        break
        if name is None:
            problems.append(f'no district for shape "{raw_name}" ({norm(raw_name).encode("unicode_escape").decode()}) in {province_names[plate]} ({plate:02d})')
            continue
        if (plate, name) in used:
            problems.append(f'two shapes for {name}, {province_names[plate]} ({plate:02d}): "{raw_name}"')
            continue
        used.add((plate, name))
        paths, best = [], None
        for polygon in polygons(feature['geometry']):
            for k, ring in enumerate(polygon):
                d, pts, n = ring_path(ring)
                if n < 3:
                    continue
                vertices += n
                paths.append(d)
                if k == 0:
                    c = centroid(pts)
                    if best is None or c[2] > best[2]:
                        best = c
        out.setdefault(f'{plate:02d}', []).append({'name': name, 'd': ''.join(paths), 'cx': round(best[0], 1), 'cy': round(best[1], 1)})
    for plate, list_ in names.items():
        for n in list_:
            if (plate, n) not in used:
                problems.append(f'no shape for {n}, {province_names[plate]} ({plate:02d})')
    if problems:
        print('\n'.join(problems))
        sys.exit(f'{len(problems)} problems; extend RENAMES')

    data = {
        'source': 'geoBoundaries gbOpen TUR ADM2 (geoboundaries.org; data from OpenStreetMap contributors, Open Data Commons '
                  'Open Database License 1.0), 973 districts matched by name to the Districts table, simplified with mapshaper '
                  f'(interval {INTERVAL} m, keep-shapes), projected to the 640x300 Mercator map of ProvinceMap.ToMap; '
                  'built by scripts/turkey-districts.py',
        'licence': 'ODbL-1.0',
        'attribution': 'District boundaries: geoBoundaries, © OpenStreetMap contributors, ODbL',
        'provinces': {k: sorted(v, key=lambda d: d['name']) for k, v in sorted(out.items())},
    }
    with open(TARGET, 'w', encoding='utf-8', newline='\n') as f:
        json.dump(data, f, ensure_ascii=False, separators=(',', ':'))
    print(f'{len(used)} districts, {vertices} vertices -> {os.path.normpath(TARGET)} ({os.path.getsize(TARGET)} bytes)')


if __name__ == '__main__':
    main()
