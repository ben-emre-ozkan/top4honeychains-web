# Pictures over 400 KB (photos and gradients in the background) are stored with a 256-colour palette; text stays sharp.
# Called by capture.mjs at the end of a run: python shrink.py <screens folder>
import os, sys
from PIL import Image
folder = sys.argv[1]
for name in sorted(os.listdir(folder)):
    path = os.path.join(folder, name)
    if not name.endswith('.png') or os.path.getsize(path) <= 400_000:
        continue
    before = os.path.getsize(path)
    Image.open(path).convert('RGB').quantize(colors=256, method=Image.Quantize.FASTOCTREE, dither=Image.Dither.FLOYDSTEINBERG).save(path, optimize=True)
    print(f'  {name}: {before // 1024} KB -> {os.path.getsize(path) // 1024} KB')
