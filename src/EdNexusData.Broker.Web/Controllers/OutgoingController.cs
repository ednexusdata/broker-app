// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using EdNexusData.Broker.Domain;
using EdNexusData.Broker.SharedKernel;
using Microsoft.AspNetCore.Authorization;
using EdNexusData.Broker.Web.Models.OutgoingRequests;
using EdNexusData.Broker.Web.Models.Paginations;
using EdNexusData.Broker.Web.Specifications;
using System.Security.Claims;
using Ardalis.Specification;
using EdNexusData.Broker.Web.ViewModels.OutgoingRequests;
using EdNexusData.Broker.Web.Services.PayloadContents;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Web.Extensions.Genders;
using Ardalis.GuardClauses;
using System.Text.Json;
using EdNexusData.Broker.Connector.Payload;
using EdNexusData.Broker.Domain.Specifications;
using EdNexusData.Broker.Web.Utilities;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using EdNexusData.Broker.Service;
namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferOutgoingRecords)]
public class OutgoingController : AuthenticatedController<OutgoingController>
{
    private readonly IRepository<Request> _outgoingRequestRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IPayloadContentService _payloadContentService;
    private readonly FocusHelper _focusHelper;
    private readonly ICurrentUser _currentUser;
    private readonly ManifestService _manifestService;

    public OutgoingController(
        IHttpContextAccessor httpContextAccessor,
        IRepository<Request> outgoingRequestRepository,
        IRepository<PayloadContent> payloadContentRepository,
        IRepository<EducationOrganization> educationOrganizationRepository,
        IPayloadContentService payloadContentService,
        FocusHelper focusHelper,
        ICurrentUser currentUser,
        ManifestService manifestService)
    {
        _outgoingRequestRepository = outgoingRequestRepository;
        _payloadContentRepository = payloadContentRepository;
        _educationOrganizationRepository = educationOrganizationRepository;
        _payloadContentService = payloadContentService;
        _focusHelper = focusHelper;
        _currentUser = currentUser;
        _manifestService = manifestService;
    }

    public async Task<IActionResult> Index(
      OutgoingRequestModel model,
      CancellationToken cancellationToken)
    {
        RefreshSession();

        var searchExpressions = model.BuildSearchExpressions();

        searchExpressions.Add(x => x.IncomingOutgoing == IncomingOutgoing.Outgoing);

        var schools = await _focusHelper.GetFocusedSchools();
        searchExpressions.Add(x => schools.Contains(x.EducationOrganization!));

        var sortExpression = model.BuildSortExpression();

        var specification = new SearchableWithPaginationSpecification<Request>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(outgoingRequest => outgoingRequest.EducationOrganization)
                .ThenInclude(educationOrganization => educationOrganization!.ParentOrganization)
                )
            .Build();

        var totalItems = await _outgoingRequestRepository.CountAsync(
            specification,
            cancellationToken);

        var outgoingRequests = await _outgoingRequestRepository.ListAsync(
            specification,
            cancellationToken);

        var outgoingRequestViewModels = outgoingRequests
            .Select(outgoingRequest => new OutgoingRequestViewModel(outgoingRequest));

        totalItems = outgoingRequestViewModels.Count();

        if (!string.IsNullOrWhiteSpace(model.SearchBy))
        {
            outgoingRequestViewModels = outgoingRequestViewModels
                .Where(request => request.Student?.ToLower().Contains(model.SearchBy) is true || request.ReceivingSchool.ToLower().Contains(model.SearchBy)
                 || request.ReceivingSchool.ToLower().Contains(model.SearchBy));
            totalItems = outgoingRequestViewModels.Count();
        }

        var result = new PaginatedViewModel<OutgoingRequestViewModel>(
            outgoingRequestViewModels,
            totalItems,
            model.Page,
            model.Size,
            model.SortBy,
            model.SortDir,
            model.SearchBy);

        return View(result);
    }

    public async Task<IActionResult> Update(Guid requestId)
    {
        var outgoingRequest = await _outgoingRequestRepository.FirstOrDefaultAsync(new RequestByIdWithPayloadContents(requestId));
        if (outgoingRequest is null) return NotFound();

        var viewModel = new CreateOutgoingRequestViewModel
        {
            RequestId = outgoingRequest.Id,
            RequestReceived = outgoingRequest.CreatedAt,
            StudentUniqueId = outgoingRequest.Student?.Student?.StudentNumber,
            Additional = JsonSerializer.Serialize(outgoingRequest.Student?.Connectors),
            FirstName = outgoingRequest.Student?.Student?.FirstName,
            MiddleName = outgoingRequest.Student?.Student?.MiddleName,
            LastSurname = outgoingRequest.Student?.Student?.LastName,
            Grade = outgoingRequest.Student?.Student?.Grade,
            Gender = outgoingRequest.Student?.Student?.Gender,
            BirthDate = outgoingRequest.Student?.Student?.Birthdate!.Value.ToString("yyyy-MM-dd"),
            ReceivingStudent = outgoingRequest.RequestManifest?.Student,
            ReceivingDistrict = outgoingRequest.RequestManifest?.From?.District,
            ReceivingSchool = outgoingRequest.RequestManifest?.From?.School,
            ReceivingSchoolContact = outgoingRequest.RequestManifest?.From?.Sender,
            ReceivingNotes = outgoingRequest.RequestManifest?.Note,
            ReleasingNotes = outgoingRequest.ResponseManifest?.Note,
            Contents = outgoingRequest.ResponseManifest?.Contents?.Select(x => x.FileName.ToString()).ToList(),
            RequestStatus = outgoingRequest.RequestStatus,
            Genders = Genders.GetSelectList(),
            ReceivingAttachments = outgoingRequest.PayloadContents?.Where(x => outgoingRequest.RequestManifest!.Contents!.Select(y => y.FileName).Contains(x.FileName)).ToList(),
            ReleasingAttachments = (outgoingRequest.ResponseManifest is not null) ? outgoingRequest.PayloadContents?.Where(x => outgoingRequest.ResponseManifest!.Contents!.Select(y => y.FileName).Contains(x.FileName)).ToList() : null,
            DraftAttachments = outgoingRequest.PayloadContents?.Where(x => x.MessageId == null).ToList()
        };

        return View(viewModel);
    }

    [HttpPut]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(CreateOutgoingRequestViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            var userId = Guid.Parse(User.FindFirstValue(claimType: ClaimTypes.NameIdentifier)!);

            var outgoingRequest = await _outgoingRequestRepository.GetByIdAsync(viewModel.RequestId);

            Guard.Against.Null(outgoingRequest);

            var student = new Student()
            {
                LastName = viewModel.LastSurname,
                FirstName = viewModel.FirstName,
                MiddleName = viewModel.MiddleName,
                Gender = viewModel.Gender,
                Grade = viewModel.Grade,
                Birthdate = (viewModel.BirthDate is not null) ? DateOnly.Parse(viewModel.BirthDate) : null,
                StudentNumber = viewModel.StudentUniqueId
            };

            // Resolve Student
            var jsonConnector = (viewModel.Additional is not null) ? JsonSerializer.Deserialize<Dictionary<string, object>>(viewModel.Additional) : null;

            outgoingRequest.Student = new StudentRequest()
            {
                Student = student,
                Connectors = (jsonConnector is not null) ? new Dictionary<string, object>
                    { 
                        [jsonConnector.Keys.First()] = jsonConnector.GetValueOrDefault(jsonConnector.Keys.First())!
                    } : null
            };

            outgoingRequest.ResponseManifest = new Manifest() {
                RequestType = typeof(StudentCumulativeRecord).FullName!,
                Student = student,
                Note = viewModel.ReleasingNotes,
                To = new RequestAddress()
                {
                    District = outgoingRequest?.RequestManifest?.From?.District,
                    School = outgoingRequest?.RequestManifest?.From?.School
                }
            };

            await _outgoingRequestRepository.UpdateAsync(outgoingRequest!);

            await _payloadContentService.AddPayloadContentsAsync(viewModel.Files, viewModel.RequestId);

            TempData[VoiceTone.Positive] = $"Updated request for {viewModel.FirstName} {viewModel.LastSurname} ({outgoingRequest!.Id}).";

            return RedirectToAction(nameof(Update), new { requestId = outgoingRequest.Id });
        }

        return View(viewModel);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ViewAttachment(Guid id)
    {
        var payloadContent = await _payloadContentRepository.GetByIdAsync(id);

        Guard.Against.Null(payloadContent);
        Guard.Against.Null(payloadContent.ContentType, "ContentType", "ContentType missing from payload content.");

        if (payloadContent.JsonContent is not null)
        {
            return Ok(payloadContent.JsonContent.ToJsonString());
        }

        var stream = new MemoryStream();
        if (payloadContent.XmlContent is not null)
        {
            payloadContent.XmlContent.Save(stream);
        }
        if (payloadContent.BlobContent is not null)
        {
            await stream.WriteAsync(payloadContent.BlobContent);
            stream.Seek(0, SeekOrigin.Begin);
        }

        return new FileStreamResult(stream, payloadContent.ContentType);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(List<IFormFile> files, Guid requestId)
    {
        var incomingRequest = await _outgoingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(requestId));

        Guard.Against.Null(incomingRequest);

        foreach (var formFile in files)
        {
            if (formFile.Length > 0)
            {
                var file = await FileHelpers
                        .ProcessFormFile<BufferedSingleFileUploadDb>(formFile, ModelState, new string[] { ".png", ".txt", ".pdf", ".json" }, 2097152);
                
                var payloadContent = new PayloadContent()
                {
                    Request = incomingRequest,
                    ContentType = formFile.ContentType,
                    FileName = formFile.FileName
                };

                if (payloadContent.ContentType == "application/json")
                {
                    payloadContent.JsonContent = JsonDocument.Parse(System.Text.Encoding.Default.GetString(file));
                }
                else
                {
                    payloadContent.BlobContent = file;
                }
                
                await _payloadContentRepository.AddAsync(payloadContent);
            }
        }

        return RedirectToAction(nameof(Update), new { requestId = requestId });
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(Guid requestId, Guid payloadContentId)
    {
        var incomingRequest = await _outgoingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(requestId));

        Guard.Against.Null(incomingRequest);

        var payloadContentToDelete = await _payloadContentRepository.GetByIdAsync(payloadContentId);

        Guard.Against.Null(payloadContentToDelete);

        await _payloadContentRepository.DeleteAsync(payloadContentToDelete);

        return RedirectToAction(nameof(Update), new { requestId = requestId });
    }

    [HttpPut]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Load(Guid id)
    {
        var outgoingRequest = await _outgoingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(id));

        Guard.Against.Null(outgoingRequest);

        outgoingRequest.RequestStatus = RequestStatus.WaitingToLoad;

        await _outgoingRequestRepository.UpdateAsync(outgoingRequest);

        TempData[VoiceTone.Positive] = $"Request set to load outgoing payload ({outgoingRequest.Id}).";
        return RedirectToAction(nameof(Update), new { requestId = id });
    }

    [HttpPut]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(Guid id)
    {
        var outgoingRequest = await _outgoingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(id));

        Guard.Against.Null(outgoingRequest);

        outgoingRequest.RequestStatus = RequestStatus.WaitingToSend;
        outgoingRequest.ResponseProcessUserId = _currentUser.AuthenticatedUserId();

        var manifestWithFrom = await _manifestService.AddFrom(outgoingRequest, _currentUser.AuthenticatedUserId()!.Value);

        outgoingRequest.ResponseManifest = manifestWithFrom;

        // Copy over receiving school's request ID
        outgoingRequest.ResponseManifest!.RequestId = outgoingRequest.RequestManifest?.RequestId;

        await _outgoingRequestRepository.UpdateAsync(outgoingRequest);

        TempData[VoiceTone.Positive] = $"Request marked to send ({outgoingRequest.Id}).";
        return RedirectToAction(nameof(Update), new { requestId = id });
    }

}
