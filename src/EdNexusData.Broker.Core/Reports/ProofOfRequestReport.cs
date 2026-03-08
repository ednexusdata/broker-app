using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EdNexusData.Broker.Core.Reports;

public static class ProofOfRequestReport
{
    public static byte[] Generate(Guid id)
    {
        // ... inside your generation method ...
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Inch);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Verdana));

                // --- HEADER ---
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PROOF OF RECORD REQUEST").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text($"Generated on: {DateTime.Now:MMMM dd, yyyy HH:mm}");
                    });

                    // Status Badge
                    row.ConstantItem(80).Background(Colors.Green.Lighten4).Padding(5).AlignCenter().Column(c => {
                        c.Item().AlignCenter().Text("STATUS").FontSize(8).SemiBold().FontColor(Colors.Green.Darken3);
                        c.Item().AlignCenter().Text("SENT").FontSize(14).Bold().FontColor(Colors.Green.Darken3);
                    });
                });

                // --- CONTENT ---
                page.Content().PaddingVertical(20).Column(col =>
                {
                    // 1. Request Info Box
                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Text(t => {
                            t.Span("Request ID: ").SemiBold();
                            t.Span(id.ToString());
                        });
                        row.RelativeItem().AlignRight().Text(t => {
                            t.Span("Sender: ").SemiBold();
                            t.Span("Records Dept.");
                        });
                    });

                    col.Item().PaddingTop(20).PaddingBottom(5).Text("Requested Records").SemiBold().FontSize(14);
                    
                    // 2. The Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(30); // #
                            c.RelativeColumn(3);  // Record Name
                            c.RelativeColumn(2);  // Type
                        });

                        table.Header(h =>
                        {
                            h.Cell().Element(HeaderStyle).Text("#");
                            h.Cell().Element(HeaderStyle).Text("Record Name");
                            h.Cell().Element(HeaderStyle).Text("Category");
                            
                            static IContainer HeaderStyle(IContainer c) => c.BorderBottom(1).PaddingVertical(5).DefaultTextStyle(x => x.SemiBold());
                        });

                        // Example Data Rows
                        AddRow(table, "1", "Employment_Contract_2025.pdf", "Legal");
                        AddRow(table, "2", "Annual_Performance_Review.docx", "HR");
                        AddRow(table, "3", "Tax_Withholding_Forms.pdf", "Finance");
                    });

                    // 3. Footer Note
                    col.Item().PaddingTop(30).Text("This document serves as an automated confirmation that the request was transmitted successfully to the destination server.").Italic().FontSize(9).FontColor(Colors.Grey.Medium);
                });

                page.Footer().AlignCenter().Text(x => {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }
    

    // Helper for table rows
    private static void AddRow(TableDescriptor table, string index, string name, string type)
    {
        table.Cell().PaddingVertical(5).Text(index);
        table.Cell().PaddingVertical(5).Text(name);
        table.Cell().PaddingVertical(5).Text(type);
    }
}
