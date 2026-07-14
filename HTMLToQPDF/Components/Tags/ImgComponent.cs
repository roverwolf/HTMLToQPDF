using System.Globalization;
using HtmlAgilityPack;
using HTMLQuestPDF.Utils;
using HTMLToQPDF.Components;
using HTMLToQPDF.Utils;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components.Tags
{
    internal class ImgComponent : BaseHTMLComponent
    {
        /// <summary>
        /// Soft cap so portrait images taller than a page scale down instead of
        /// throwing DocumentLayoutException with AspectRatio FitWidth.
        /// </summary>
        private const float MaxImageHeightPoints = 700f;

        private readonly GetImgBySrc getImgBySrc;

        public ImgComponent(HtmlNode node, HTMLComponentsArgs args) : base(node, args)
        {
            this.getImgBySrc = args.GetImgBySrc;
        }

        protected override void ComposeSingle(IContainer container)
        {
            var src = node.GetAttributeValue("src", "");
            var img = getImgBySrc(src) ?? Placeholders.Image(200, 100);

            var htmlWidth = TryGetHtmlSizeAttribute(node, "width");
            var htmlHeight = TryGetHtmlSizeAttribute(node, "height");

            float aspect;
            if (htmlWidth is > 0 && htmlHeight is > 0)
            {
                aspect = htmlWidth.Value / htmlHeight.Value;
            }
            else if (ImageDimensionUtils.TryGetDimensions(img, out var pixelWidth, out var pixelHeight)
                     && pixelWidth > 0 && pixelHeight > 0)
            {
                aspect = pixelWidth / (float)pixelHeight;
                // No HTML size: don't upscale past the image's intrinsic CSS-pixel size.
                htmlWidth ??= pixelWidth * 0.75f;
                htmlHeight ??= pixelHeight * 0.75f;
            }
            else
            {
                container.AlignCenter().Image(img).FitArea();
                return;
            }

            container = container.AlignCenter();

            // Honor HTML width/height (and intrinsic size fallback) as maxima so images
            // match the UI and never grow beyond their authored size. They can still
            // shrink when the page is narrower (like CSS max-width: 100%).
            if (htmlWidth is > 0)
                container = container.MaxWidth(htmlWidth.Value);
            if (htmlHeight is > 0)
                container = container.MaxHeight(Math.Min(htmlHeight.Value, MaxImageHeightPoints));
            else
                container = container.MaxHeight(MaxImageHeightPoints);

            var heightAtHtmlWidth = (htmlWidth ?? 500f) / aspect;
            if (heightAtHtmlWidth > MaxImageHeightPoints)
            {
                container
                    .AspectRatio(aspect, AspectRatioOption.FitArea)
                    .Image(img)
                    .FitArea();
            }
            else
            {
                container
                    .AspectRatio(aspect, AspectRatioOption.FitWidth)
                    .Image(img)
                    .FitArea();
            }
        }

        /// <summary>
        /// HTML width/height attributes are CSS pixels when unitless (CKEditor output).
        /// </summary>
        private static float? TryGetHtmlSizeAttribute(HtmlNode node, string name)
        {
            var raw = node.GetAttributeValue(name, string.Empty);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            raw = raw.Trim();
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var pixels))
                return pixels * 0.75f; // same px→pt as CssParser

            return CssParser.ParseLength(raw);
        }
    }
}
