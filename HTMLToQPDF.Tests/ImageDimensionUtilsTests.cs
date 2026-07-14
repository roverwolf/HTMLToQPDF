using HTMLToQPDF.Utils;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HTMLToQPDF.Tests;

[TestFixture]
public class ImageDimensionUtilsTests
{
    [SetUp]
    public void SetUp() => QuestPDF.Settings.License = LicenseType.Community;

    [Test]
    public void TryGetDimensions_ReadsPlaceholderImageSize()
    {
        var bytes = Placeholders.Image(400, 200);
        Assert.That(ImageDimensionUtils.TryGetDimensions(bytes, out var w, out var h), Is.True);
        // QuestPDF placeholders may not match requested pixel size exactly, but aspect should.
        Assert.That(w, Is.GreaterThan(0));
        Assert.That(h, Is.GreaterThan(0));
        Assert.That((double)w / h, Is.EqualTo(2.0).Within(0.05));
    }
}
