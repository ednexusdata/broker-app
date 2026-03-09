using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EdNexusData.Broker.Core.Reports;

public class ProofOfRequestReport
{
    private readonly IRepository<Request> requestRepository;

    public ProofOfRequestReport(
        IRepository<Request> requestRepository
    )
    {
        this.requestRepository = requestRepository;
    }

    public async Task<byte[]> Generate(
        Guid id, 
        User user,
        TimeZoneInfo timeZoneInfo)
    {
        // Get request
        var request = await requestRepository.GetByIdAsync(id) ?? throw new NullReferenceException($"Unable to find request {id}");
        
        // ... inside your generation method ...
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(1, Unit.Inch);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Verdana));

                // --- HEADER ---
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().AlignCenter().Text("PROOF OF RECORDS REQUEST").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().AlignCenter().Text($"Generated on {ResolveTime(DateTime.UtcNow, timeZoneInfo)} \n by {user.Name} ({user.Id}).");
                    });
                });

                // --- CONTENT ---
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().PaddingTop(20).PaddingBottom(5).Text("Student").SemiBold().FontSize(14);

                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(innerCol =>
                    {
                        // 1. Name stays on its own line at the top
                        innerCol.Item().Text(t => {
                            t.Span("Name: ").SemiBold();
                            t.Span($"{request.RequestManifest?.Student?.LastName}, {request.RequestManifest?.Student?.FirstName} {request.RequestManifest?.Student?.MiddleName}");
                        });

                        innerCol.Item().Row(row =>
                        {
                            // Left side: Birthdate & Gender
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Table(t => {
                                    t.ColumnsDefinition(cd => { cd.ConstantColumn(70); cd.RelativeColumn(); });
                                    
                                    t.Cell().Text("Birthdate:").SemiBold();
                                    t.Cell().Text($"{request.RequestManifest?.Student?.Birthdate}");

                                    t.Cell().Text("Gender:").SemiBold();
                                    t.Cell().Text($"{request.RequestManifest?.Student?.Gender}");
                                });
                            });

                            row.ConstantItem(20); // Spacing between the two "columns"

                            // Right side: Student ID & Grade
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Table(t => {
                                    t.ColumnsDefinition(cd => { cd.ConstantColumn(80); cd.RelativeColumn(); });

                                    t.Cell().Text("Student ID:").SemiBold();
                                    t.Cell().Text($"{request.RequestManifest?.Student?.StudentNumber}");

                                    t.Cell().Text("Grade:").SemiBold();
                                    t.Cell().Text($"{request.RequestManifest?.Student?.Grade}");
                                });
                            });
                        });

                    });

                    col.Item().PaddingTop(20).PaddingBottom(5).Text("Requested From").SemiBold().FontSize(14);

                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Text(t => {
                            t.Span("Request ID: ").SemiBold();
                            t.Span(request.Id.ToString());
                        });
                        row.RelativeItem().Text(t => {
                            t.Span("District: ").SemiBold();
                            t.Span(request.RequestManifest?.From?.District?.Name);
                        });
                        row.RelativeItem().Text(t => {
                            t.Span("School: ").SemiBold();
                            t.Span(request.RequestManifest?.From?.School?.Name);
                        });

                        row.RelativeItem().Text(t => {
                            t.Span("Sender: ").SemiBold();
                            t.Span(request.RequestManifest?.From?.Sender?.Name);
                        });
                    });

                    col.Item().PaddingTop(20).PaddingBottom(5).Text("Sent To").SemiBold().FontSize(14);

                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Text(t => {
                            t.Span("District: ").SemiBold();
                            t.Span(request.RequestManifest?.To?.District?.Name);
                        });

                        row.RelativeItem().Text(t => {
                            t.Span("School: ").SemiBold();
                            t.Span(request.RequestManifest?.To?.School?.Name);
                        });

                        row.RelativeItem().Text(t => {
                            t.Span("Broker Address: ").SemiBold();
                            t.Span(request.RequestManifest?.To?.BrokerAddress);
                        });
                    });

                    col.Item().PaddingTop(20).PaddingBottom(5).Text("Note").SemiBold().FontSize(14);

                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Text(t => {
                            t.Span(request.RequestManifest?.Note);
                        });
                    });

                    // 3. Footer Note
                    col.Item().PaddingTop(30).Text("This document serves as an automated confirmation that the request was transmitted successfully to the destination server.").Italic().FontSize(9).FontColor(Colors.Grey.Medium);
                });

                page.Footer().AlignCenter().Text(x => {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private string ResolveTime(DateTime time, TimeZoneInfo? timeZoneInfo = null)
    {
        // Resolve timezoneinfo
        if (timeZoneInfo is null)
        {
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("UTC");
        }
        
        return TimeZoneInfo.ConvertTimeFromUtc(time, timeZoneInfo).ToString("M/dd/yyyy h:mm tt");
    }
}
