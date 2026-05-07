using System;
using System.IO;
using HTMLQuestPDF.Extensions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length is < 1 or > 2)
        {
            PrintUsage();
            return 2;
        }

        var inputPath = args[0];
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            PrintUsage();
            return 2;
        }

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Input file does not exist: {inputPath}");
            return 3;
        }

        var outputPath = args.Length == 2 ? args[1] : GetDefaultOutputPath(inputPath);
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            Console.Error.WriteLine("Output path is empty.");
            return 2;
        }

        try
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var html = File.ReadAllText(inputPath);

            Document.Create(container =>
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
                        });
                    });
                });
            }).GeneratePdf(outputPath);

            Console.WriteLine($"Generated: {outputPath}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static string GetDefaultOutputPath(string inputPath)
    {
        // Replace .html/.htm extension (or any extension) with .pdf
        return Path.ChangeExtension(inputPath, ".pdf") ?? (inputPath + ".pdf");
    }

    private static void PrintUsage()
    {
        Console.WriteLine("HTMLToQPDF.Cli");
        Console.WriteLine("Usage:");
        Console.WriteLine("  HTMLToQPDF.Cli <input.html> [output.pdf]");
        Console.WriteLine("If output is omitted, it defaults to the input filename with a .pdf extension.");
    }
}

