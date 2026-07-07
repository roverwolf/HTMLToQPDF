using HTMLQuestPDF.Extensions;
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
    public void TextOnly_GeneratesNonEmptyPdf()
    {
        var size = GeneratePdfSize("<p>hello world</p>");

        Assert.That(size, Is.GreaterThan(500));
    }

    private static long GeneratePdfSize(string html)
    {
        using var stream = new MemoryStream();

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.Content().Column(col =>
                {
                    col.Item().HTML(handler => handler.SetHtml(html));
                });
            });
        }).GeneratePdf(stream);

        return stream.Length;
    }
}
