using HTMLQuestPDF.Extensions;
using HTMLToQPDF.Components;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HTMLToQPDF.Tests;

[TestFixture]
public class MultipleImageRenderingTests
{
    private const string TinyPngBase64 =
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==";

    private static string TinyPngDataUri => $"data:image/png;base64,{TinyPngBase64}";

    [SetUp]
    public void SetUp()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Test]
    public void TwoSiblingImages_RendersLargerPdfThanSingleImage()
    {
        var singleImageSize = GeneratePdfSize($"""<div><img src="{TinyPngDataUri}"/></div>""");
        var twoImagesSize = GeneratePdfSize(
            $"""<div><img src="{TinyPngDataUri}"/><img src="{TinyPngDataUri}"/></div>""");

        Assert.That(twoImagesSize, Is.GreaterThan(singleImageSize));
    }

    [Test]
    public void TwoLargeSiblingImages_BothAppearInRenderedPages()
    {
        const string html = """
            <div>
              <img src="first"/>
              <img src="second"/>
            </div>
            """;

        byte[] First() => Placeholders.Image(1260, 750);
        byte[] Second() => Placeholders.Image(800, 600);

        var twoImageBytes = RenderPageBytes(html, src => src == "first" ? First() : Second()).Sum(p => p.Length);
        var singleImageBytes = RenderPageBytes("""<div><img src="first"/></div>""", _ => First()).Sum(p => p.Length);

        Assert.That(twoImageBytes, Is.GreaterThan(singleImageBytes * 1.15),
            "Second sibling image should contribute visible content.");
    }

    [Test]
    public void TextWithImage_RendersNonEmptyPdf()
    {
        var size = GeneratePdfSize($"""<p>before<img src="{TinyPngDataUri}"/>after</p>""");

        Assert.That(size, Is.GreaterThan(500));
    }

    [Test]
    public void SingleImage_RendersNonEmptyPdf()
    {
        var size = GeneratePdfSize($"""<div><img src="{TinyPngDataUri}"/></div>""");

        Assert.That(size, Is.GreaterThan(500));
    }

    [Test]
    public void TableCellTwoImages_RendersLargerPdfThanSingleImage()
    {
        var singleImageSize = GeneratePdfSize(
            $"""<table><tr><td><img src="{TinyPngDataUri}"/></td></tr></table>""");
        var twoImagesSize = GeneratePdfSize(
            $"""<table><tr><td><img src="{TinyPngDataUri}"/><img src="{TinyPngDataUri}"/></td></tr></table>""");

        Assert.That(twoImagesSize, Is.GreaterThan(singleImageSize));
    }

    [Test]
    public void ImageWithMaxWidthContainerStyle_FitsOnFirstPage()
    {
        const string html = """<div><img src="photo"/></div>""";

        var pageCount = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.Content().Column(col =>
                {
                    col.Item().HTML(handler =>
                    {
                        handler.SetHtml(html);
                        handler.OverloadImgReceivingFunc(_ => Placeholders.Image(1260, 750));
                        handler.SetContainerStyleForHtmlElement("img", c => c.MaxWidth(12, Unit.Centimetre));
                    });
                });
            });
        }).GenerateImages().Count();

        Assert.That(pageCount, Is.EqualTo(1));
    }

    [Test]
    public void ImageWithHtmlWidthAttribute_RendersSmallerThanWiderHtmlWidth()
    {
        // CKEditor stores display size as width/height attributes (CSS px).
        var small = RenderPageBytes(
            """<div><img src="photo" width="80" height="60"/></div>""",
            _ => Placeholders.Image(800, 600)).Sum(p => p.Length);
        var large = RenderPageBytes(
            """<div><img src="photo" width="500" height="375"/></div>""",
            _ => Placeholders.Image(800, 600)).Sum(p => p.Length);

        Assert.That(small, Is.LessThan(large),
            "Smaller HTML width/height should produce a visibly smaller rendered image.");
    }

    [Test]
    public void TextOnly_GeneratesNonEmptyPdf()
    {
        var size = GeneratePdfSize("<p>hello world</p>");

        Assert.That(size, Is.GreaterThan(500));
    }

    [Test]
    public void SecondImageNearPageBottom_AppearsOnFollowingPage()
    {
        // width/height match CKEditor output; values chosen so FitWidth height ≈ 278pt
        // after px→pt, leaving a thin strip after a 480pt spacer on A4.
        const string html = """
            <div>
              <div style="height: 480pt; background-color: #cccccc;">spacer</div>
              <img src="first" width="555" height="370"/>
              <img src="second" width="555" height="370"/>
            </div>
            """;

        var withBoth = RenderPageBytes(html, _ => Placeholders.Image(800, 600));
        var withFirstOnly = RenderPageBytes(
            """
            <div>
              <div style="height: 480pt; background-color: #cccccc;">spacer</div>
              <img src="first" width="555" height="370"/>
            </div>
            """,
            _ => Placeholders.Image(800, 600));

        Assert.That(withBoth.Count, Is.GreaterThanOrEqualTo(2), "Second image should force an extra page.");
        Assert.That(withBoth.Sum(p => p.Length), Is.GreaterThan(withFirstOnly.Sum(p => p.Length) * 1.1),
            "Second image near the page bottom must still be drawn on a later page.");
    }

    [Test]
    public void SecondImageInDivNearPageBottom_StillRenders()
    {
        const string html = """
            <div>
              <div style="height: 480pt; background-color: #cccccc;">spacer</div>
              <div class="article-image">
                <img src="first" width="555" height="370"/>
                <img src="second" width="555" height="370"/>
              </div>
            </div>
            """;

        var withBoth = RenderPageBytes(html, _ => Placeholders.Image(800, 600));
        var withFirstOnly = RenderPageBytes(
            """
            <div>
              <div style="height: 480pt; background-color: #cccccc;">spacer</div>
              <div class="article-image">
                <img src="first" width="555" height="370"/>
              </div>
            </div>
            """,
            _ => Placeholders.Image(800, 600));

        Assert.That(withBoth.Count, Is.GreaterThanOrEqualTo(2));
        Assert.That(withBoth.Sum(p => p.Length), Is.GreaterThan(withFirstOnly.Sum(p => p.Length) * 1.1),
            "Second image in a div near page bottom must still render after the page break.");
    }

    private static List<byte[]> RenderPageBytes(string html, GetImgBySrc getImgBySrc)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.Content().Column(col =>
                {
                    col.Item().HTML(handler =>
                    {
                        handler.SetHtml(html);
                        handler.OverloadImgReceivingFunc(getImgBySrc);
                    });
                });
            });
        }).GenerateImages(new ImageGenerationSettings { RasterDpi = 72 }).ToList();
    }

    private static long GeneratePdfSize(string html, GetImgBySrc? getImgBySrc = null)
    {
        using var stream = new MemoryStream();
        CreateDocument(html, getImgBySrc).GeneratePdf(stream);
        return stream.Length;
    }

    private static Document CreateDocument(string html, GetImgBySrc? getImgBySrc = null)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.Content().Column(col =>
                {
                    col.Item().HTML(handler =>
                    {
                        handler.SetHtml(html);
                        if (getImgBySrc is not null)
                            handler.OverloadImgReceivingFunc(getImgBySrc);
                    });
                });
            });
        });
    }
}
