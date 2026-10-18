using EdNexusData.Broker.Core.Specifications;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EdNexusData.Broker.Core.Reports;

public class ProofOfRequestReport
{
    private readonly IRepository<Request> requestRepository;
    private readonly IReadRepository<ActivityLog> activityLogRepository;

    public ProofOfRequestReport(
        IRepository<Request> requestRepository,
        IReadRepository<ActivityLog> activityLogRepository
    )
    {
        this.requestRepository = requestRepository;
        this.activityLogRepository = activityLogRepository;
    }

    public async Task<byte[]> Generate(
        Guid id,
        User user,
        TimeZoneInfo timeZoneInfo)
    {
        return await Generate(id, $"{user.Name} ({user.Id})", timeZoneInfo);
    }

    public async Task<byte[]> Generate(
        Guid id,
        string generatedBy,
        TimeZoneInfo timeZoneInfo)
    {
        // Get request
        var request = await requestRepository.GetByIdAsync(id) ?? throw new NullReferenceException($"Unable to find request {id}");

        // Captured now so it survives even if this request's ActivityLog rows are later cascade-deleted
        // along with the request itself (e.g. by RequestCleanupJob).
        var activityLogs = await activityLogRepository.ListAsync(new ActivityLogsByRequestId(id));

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
                        col.Item().AlignCenter().Text($"Generated on {ResolveTime(DateTime.UtcNow, timeZoneInfo)} \n by {generatedBy}.");
                    });
                });

                // --- CONTENT ---
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().PaddingTop(20).PaddingBottom(5).Text("Student").SemiBold().FontSize(14);

                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(innerCol =>
                    {
                        // 1. Name stays on its own line at the top, using the same label width as
                        // Birthdate/Gender below so the values line up in a single column.
                        innerCol.Item().Table(t => {
                            t.ColumnsDefinition(cd => { cd.ConstantColumn(70); cd.RelativeColumn(); });

                            t.Cell().Text("Name:").SemiBold();
                            t.Cell().Text($"{request.RequestManifest?.Student?.LastName}, {request.RequestManifest?.Student?.FirstName} {request.RequestManifest?.Student?.MiddleName}");
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

                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(innerCol =>
                    {
                        // Request ID is a full GUID, so it gets its own line to avoid squeezing the columns below.
                        innerCol.Item().Text(t => {
                            t.Span("Request ID: ").SemiBold();
                            t.Span(request.Id.ToString());
                        });

                        innerCol.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem().Column(c => {
                                c.Item().Text("District:").SemiBold();
                                c.Item().Text($"{request.RequestManifest?.From?.District?.Name}");
                            });

                            row.ConstantItem(20);

                            row.RelativeItem().Column(c => {
                                c.Item().Text("School:").SemiBold();
                                c.Item().Text($"{request.RequestManifest?.From?.School?.Name}");
                            });

                            row.ConstantItem(20);

                            row.RelativeItem().Column(c => {
                                c.Item().Text("Sender:").SemiBold();
                                c.Item().Text($"{request.RequestManifest?.From?.Sender?.Name}");
                            });
                        });
                    });

                    col.Item().PaddingTop(20).PaddingBottom(5).Text("Sent To").SemiBold().FontSize(14);

                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(c => {
                            c.Item().Text("District:").SemiBold();
                            c.Item().Text($"{request.RequestManifest?.To?.District?.Name}");
                        });

                        row.ConstantItem(20);

                        row.RelativeItem().Column(c => {
                            c.Item().Text("School:").SemiBold();
                            c.Item().Text($"{request.RequestManifest?.To?.School?.Name}");
                        });

                        row.ConstantItem(20);

                        row.RelativeItem().Column(c => {
                            c.Item().Text("Broker Address:").SemiBold();
                            c.Item().Text($"{request.RequestManifest?.To?.BrokerAddress}");
                        });
                    });

                    col.Item().PaddingTop(20).PaddingBottom(5).Text("Note").SemiBold().FontSize(14);

                    col.Item().Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Text(t => {
                            t.Span(request.RequestManifest?.Note);
                        });
                    });

                    // Activity Log — last content section, so it reads as the closing history of the
                    // request before the footer disclaimer.
                    if (activityLogs.Count > 0)
                    {
                        col.Item().PaddingTop(20).PaddingBottom(5).Text("Activity Log").SemiBold().FontSize(14);

                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(cd =>
                            {
                                cd.ConstantColumn(130);
                                cd.ConstantColumn(120);
                                cd.RelativeColumn();
                            });

                            t.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Date/Time").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("User").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Activity").SemiBold();
                            });

                            foreach (var activityLog in activityLogs)
                            {
                                t.Cell().Padding(5).Text(ResolveTime(activityLog.CreatedAt.DateTime, timeZoneInfo));
                                t.Cell().Padding(5).Text(activityLog.User?.LastFirstName ?? "System");
                                t.Cell().Padding(5).Text(activityLog.Description ?? activityLog.Action);
                            }
                        });
                    }

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
