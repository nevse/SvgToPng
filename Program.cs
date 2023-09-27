using SkiaSharp;
using Svg.Skia;

if (args.Length == 0) {
    Console.WriteLine("Usage: SvgToPng <path to the project root>");
    return;
}

string basePath = args[0];

string svgFilesPath = Path.Combine(basePath, "Resources", "SvgImages");
string outputPath = basePath;

foreach (var svgFilePath in Directory.GetFiles(svgFilesPath, "*.svg")) {
    SKSvg svg = new SKSvg();
    svg.Load(svgFilePath);

    //ios
    List<(string suffix, float scale)> iosScales = new() {
        ("", 1.0f),
        ("@2x", 2.0f),
        ("@3x", 3.0f),
    };
    foreach (var scaleInfo in iosScales) {
        string fileName = Path.GetFileNameWithoutExtension(svgFilePath);
        string path =
            $@"{outputPath}/Resources/Images/{fileName}{scaleInfo.suffix}.png";
        ScaleSvgTo(svg, scaleInfo.scale, path);
        Console.WriteLine($"Generated {path}");
    }

    //android
    List<(string pathPart, float scale)> androidScales = new() {
        ("drawable", 1.0f),
        ("drawable-mdpi", 1.0f),
        ("drawable-hdpi", 1.5f),
        ("drawable-xhdpi", 2.0f),
        ("drawable-xxhdpi", 3.0f),
        ("drawable-xxxhdpi", 4.0f),
    };
    foreach (var scaleInfo in androidScales) {
        string fileName = Path.GetFileNameWithoutExtension(svgFilePath);
        string path =
            $@"{outputPath}/Platforms/Android/Resources/{scaleInfo.pathPart}/{fileName}.png";
        ScaleSvgTo(svg, scaleInfo.scale, path);
        Console.WriteLine($"Generated {path}");
    }
}

void ScaleSvgTo(SKSvg svg, float scale, string outputFileName) {
    if (svg.Picture == null) throw new NotSupportedException("Svg.Picture is null");
    SKSize svgSize = svg.Picture.CullRect.Size;
    SKSizeI svgScaledSize = new SKSizeI((int)Math.Round(svgSize.Width * (double)scale),
        (int)Math.Round(svgSize.Height * (double)scale));
    using SKBitmap tempBitmap = new SKBitmap(svgScaledSize.Width, svgScaledSize.Height);
    using SKCanvas canvas = new SKCanvas(tempBitmap);
    var canvasSize = tempBitmap.Info.Size;
    canvas.Clear(SKColors.Transparent);
    canvas.Scale(scale, scale);

    canvas.DrawPicture(svg.Picture, null);
    using FileStream stream = File.Create(outputFileName);
    tempBitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
    stream.Flush();
}