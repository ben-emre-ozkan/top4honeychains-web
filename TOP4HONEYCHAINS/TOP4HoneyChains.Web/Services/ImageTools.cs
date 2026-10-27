using SkiaSharp;

namespace TOP4HoneyChains.Web.Services
{
    // D-85: image work on SkiaSharp (MIT). ImageSharp 3.2 refuses Release builds without a Six Labors license key, and 3.1.12 has
    // open security advisories. Every output is a fresh JPEG encode: EXIF (GPS, camera), IPTC, XMP and ICC data are never copied.
    public static class ImageTools
    {
        public record Info(string Format, int Width, int Height);

        // format and size from the file's own content; null when it is not an image SkiaSharp can decode
        public static Info? Identify(Stream stream)
        {
            using var codec = SKCodec.Create(stream);
            if (codec == null)
                return null;
            var format = codec.EncodedFormat switch
            {
                SKEncodedImageFormat.Jpeg => "JPEG",
                SKEncodedImageFormat.Png => "PNG",
                SKEncodedImageFormat.Webp => "WEBP",
                var other => other.ToString().ToUpperInvariant()
            };
            return new Info(format, codec.Info.Width, codec.Info.Height);
        }

        // decoded, turned upright by its EXIF orientation, cropped to fill width × height around the centre, as JPEG; null when
        // the bytes are not an image
        public static byte[]? CropToJpeg(Stream stream, int width, int height, int quality)
        {
            using var source = DecodeUpright(stream);
            if (source == null)
                return null;
            var scale = Math.Max((float)width / source.Width, (float)height / source.Height);
            var cropWidth = width / scale;
            var cropHeight = height / scale;
            var crop = SKRect.Create((source.Width - cropWidth) / 2f, (source.Height - cropHeight) / 2f, cropWidth, cropHeight);
            using var target = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(target))
            {
                canvas.Clear(SKColors.White);
                using var image = SKImage.FromBitmap(source);
                canvas.DrawImage(image, crop, SKRect.Create(width, height), new SKSamplingOptions(SKCubicResampler.Mitchell));
            }
            return Jpeg(target, quality);
        }

        public static byte[]? CropToJpeg(byte[] data, int width, int height, int quality)
        {
            using var stream = new MemoryStream(data);
            return CropToJpeg(stream, width, height, quality);
        }

        // the same picture re-encoded as JPEG without any metadata; null when the bytes are not an image
        public static byte[]? CleanJpeg(byte[] data, int quality)
        {
            using var stream = new MemoryStream(data);
            using var source = DecodeUpright(stream);
            return source == null ? null : Jpeg(source, quality);
        }

        private static SKBitmap? DecodeUpright(Stream stream)
        {
            using var codec = SKCodec.Create(stream);
            if (codec == null)
                return null;
            var info = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            var decoded = new SKBitmap(info);
            var result = codec.GetPixels(info, decoded.GetPixels());
            if (result != SKCodecResult.Success && result != SKCodecResult.IncompleteInput)
            {
                decoded.Dispose();
                return null;
            }
            var origin = codec.EncodedOrigin;
            if (origin == SKEncodedOrigin.TopLeft)
                return decoded;
            var upright = Orient(decoded, origin);
            decoded.Dispose();
            return upright;
        }

        // the EXIF orientation applied to the pixels (the encoded JPEG carries no orientation tag)
        private static SKBitmap Orient(SKBitmap bitmap, SKEncodedOrigin origin)
        {
            var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
            var result = new SKBitmap(swap ? bitmap.Height : bitmap.Width, swap ? bitmap.Width : bitmap.Height, bitmap.ColorType, bitmap.AlphaType);
            using var canvas = new SKCanvas(result);
            switch (origin)
            {
                case SKEncodedOrigin.TopRight: canvas.Scale(-1, 1, result.Width / 2f, 0); break;
                case SKEncodedOrigin.BottomRight: canvas.RotateDegrees(180, result.Width / 2f, result.Height / 2f); break;
                case SKEncodedOrigin.BottomLeft: canvas.Scale(1, -1, 0, result.Height / 2f); break;
                case SKEncodedOrigin.LeftTop: canvas.Translate(result.Width, 0); canvas.RotateDegrees(90); canvas.Scale(1, -1, 0, bitmap.Height / 2f); break;
                case SKEncodedOrigin.RightTop: canvas.Translate(result.Width, 0); canvas.RotateDegrees(90); break;
                case SKEncodedOrigin.RightBottom: canvas.Translate(0, result.Height); canvas.RotateDegrees(-90); canvas.Scale(1, -1, 0, bitmap.Height / 2f); break;
                case SKEncodedOrigin.LeftBottom: canvas.Translate(0, result.Height); canvas.RotateDegrees(-90); break;
            }
            canvas.DrawBitmap(bitmap, 0, 0);
            return result;
        }

        private static byte[] Jpeg(SKBitmap bitmap, int quality)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            return data.ToArray();
        }
    }
}
