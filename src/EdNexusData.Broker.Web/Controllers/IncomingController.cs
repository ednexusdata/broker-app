// Copyright: 2023 Education Nexus Oregon
// Author: Makoa Jacobsen, makoa@makoajacobsen.com

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EdNexusData.Broker.Domain;
using System.Security.Claims;
using EdNexusData.Broker.SharedKernel;
using EdNexusData.Broker.Web.Models.Paginations;
using Ardalis.Specification;
using EdNexusData.Broker.Web.Specifications;
using EdNexusData.Broker.Web.Models.IncomingRequests;
using EdNexusData.Broker.Web.ViewModels.IncomingRequests;
using Microsoft.EntityFrameworkCore;
using EdNexusData.Broker.Web.Services.PayloadContents;
using System.Linq.Expressions;
using static EdNexusData.Broker.Web.Constants.Claims.CustomClaimType;
using EdNexusData.Broker.Web.Extensions.States;
using EdNexusData.Broker.Web.Extensions.Genders;
using EdNexusData.Broker.Connector.Payload;
using EdNexusData.Broker.Web.Helpers;
using EdNexusData.Broker.Domain.Specifications;
using Ardalis.GuardClauses;
using EdNexusData.Broker.Web.Constants.DesignSystems;
using System.Text.Json;
using EdNexusData.Broker.Web.Utilities;
using System.ComponentModel.DataAnnotations;
using EdNexusData.Broker.Service;

namespace EdNexusData.Broker.Web.Controllers;

[Authorize(Policy = TransferIncomingRecords)]
public class IncomingController : AuthenticatedController<IncomingController>
{
    private readonly IReadRepository<EducationOrganization> _educationOrganizationRepository;
    private readonly IRepository<PayloadContent> _payloadContentRepository;
    private readonly IRepository<Request> _incomingRequestRepository;
    private readonly IPayloadContentService _payloadContentService;
    private readonly FocusHelper _focusHelper;
    private readonly ICurrentUser _currentUser;
    private readonly ManifestService _manifestService;

    public IncomingController(
        IReadRepository<EducationOrganization> educationOrganizationRepository,
        IRepository<PayloadContent> payloadContentRepository,
        IRepository<Request> incomingRequestRepository,
        IPayloadContentService payloadContentService,
        FocusHelper focusHelper,
        ICurrentUser currentUser,
        ManifestService manifestService)
    {
        _educationOrganizationRepository = educationOrganizationRepository;
        _payloadContentRepository = payloadContentRepository;
        _incomingRequestRepository = incomingRequestRepository;
        _payloadContentService = payloadContentService;
        _focusHelper = focusHelper;
        _currentUser = currentUser;
        _manifestService = manifestService;
    }

    public async Task<IActionResult> Index(
        IncomingRequestModel model,
        CancellationToken cancellationToken)
    {
        RefreshSession();
        var searchExpressions = new List<Expression<Func<Request, bool>>>(); 
        searchExpressions = model.BuildSearchExpressions();

        var schools = await _focusHelper.GetFocusedSchools();

        searchExpressions.Add(x => schools.Contains(x.EducationOrganization!));

        searchExpressions.Add(x => x.IncomingOutgoing == IncomingOutgoing.Incoming);

        var sortExpression = model.BuildSortExpression();

        var organizationName = GetFocusOrganizationDistrict();

        var specification = new SearchableWithPaginationSpecification<Request>.Builder(model.Page, model.Size)
            .WithAscending(model.IsAscending)
            .WithSortExpression(sortExpression)
            .WithSearchExpressions(searchExpressions)
            .WithIncludeEntities(builder => builder
                .Include(incomingRequest => incomingRequest.EducationOrganization)
                .ThenInclude(educationOrganization => educationOrganization!.ParentOrganization)
                )
            .Build();

        var totalItems = await _incomingRequestRepository.CountAsync(
            specification,
            cancellationToken);

        var incomingRequests = await _incomingRequestRepository.ListAsync(
            specification,
            cancellationToken);

        var incomingRequestViewModels = incomingRequests
            .Select(incomingRequest => new IncomingRequestViewModel(incomingRequest));
        
        if (!string.IsNullOrWhiteSpace(model.SearchBy))
        {
            incomingRequestViewModels = incomingRequestViewModels
                .Where(request => request.Student?.ToLower().Contains(model.SearchBy) is true || request.ReleasingDistrict.ToLower().Contains(model.SearchBy)
                 || request.ReleasingSchool.ToLower().Contains(model.SearchBy));
            totalItems = incomingRequestViewModels.Count();
        }

        var result = new PaginatedViewModel<IncomingRequestViewModel>(
            incomingRequestViewModels,
            totalItems,
            model.Page,
            model.Size,
            model.SortBy,
            model.SortDir,
            model.SearchBy);

        return View(result);
    }

    public async Task<IActionResult> Create()
    {
        var viewModel = new CreateIncomingRequestViewModel
        {
            EducationOrganizations = await _focusHelper.GetFocusedSchools(),
            States = States.GetSelectList(),
            Genders = Genders.GetSelectList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateIncomingRequestViewModel viewModel)
    {
        var district = JsonSerializer.Deserialize<District>(Request.Form["ToDistrict"]!, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (district is not null)
        {
            district.Schools = null;
        }
        var school = JsonSerializer.Deserialize<School>(Request.Form["ToSchool"]!, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        if (ModelState.IsValid)
        {
            var userId = Guid.Parse(User.FindFirstValue(claimType: ClaimTypes.NameIdentifier)!);

            var edOrg = await _educationOrganizationRepository.FirstOrDefaultAsync(new OrganizationByIdWithParentSpec(viewModel.EducationOrganizationId!.Value));

            var student = new Student() {
                LastName = viewModel.LastSurname,
                FirstName = viewModel.FirstName,
                MiddleName = viewModel.MiddleName,
                StudentNumber = viewModel.StudentUniqueId,
                Grade = viewModel.Grade,
                Gender = viewModel.Gender,
                Birthdate = DateOnly.Parse(viewModel.BirthDate!)
            };

            var jsonConnector = (viewModel.Additional is not null) ? JsonSerializer.Deserialize<Dictionary<string, object>>(viewModel.Additional) : null;

            var incomingRequestId = Guid.NewGuid();
            var incomingRequest = new Request
            {
                Id = incomingRequestId,
                EducationOrganizationId = viewModel.EducationOrganizationId.Value,
                Student = new StudentRequest() {
                    Student = student,
                    Connectors = (jsonConnector is not null) ? new Dictionary<string, object>
                    { 
                        [jsonConnector.Keys.First()] = jsonConnector.GetValueOrDefault(jsonConnector.Keys.First())!
                    } : null
                }, 
                RequestManifest = new Manifest() {
                    RequestType = typeof(StudentCumulativeRecord).FullName!,
                    RequestId = incomingRequestId,
                    Student = student,
                    Note = viewModel.Note,
                    To = new RequestAddress()
                    {
                        District = district,
                        School = school
                    }
                },
                ResponseManifest = null,
                RequestProcessUserId = userId,
                RequestStatus = RequestStatus.Draft,
                IncomingOutgoing = IncomingOutgoing.Incoming,
                Payload = typeof(StudentCumulativeRecord).FullName!
            };

            await _incomingRequestRepository.AddAsync(incomingRequest);

            TempData[VoiceTone.Positive] = $"Created request for {viewModel.FirstName} {viewModel.LastSurname} ({incomingRequest.Id}).";
            return RedirectToAction(nameof(Update), new { id = incomingRequest.Id });
        }

        viewModel.EducationOrganizations = await _focusHelper.GetFocusedSchools();
        return View(viewModel);
    }

    [Route("/incoming-requests/edit/{id:guid}")]
    public async Task<IActionResult> Update(Guid id)
    {
        var incomingRequest = await _incomingRequestRepository.FirstOrDefaultAsync(new RequestByIdWithPayloadContents(id));
        if (incomingRequest is null) return NotFound();

        var requestManifest = incomingRequest.RequestManifest;

        var viewModel = new CreateIncomingRequestViewModel
        {
            RequestId = incomingRequest.Id,
            EducationOrganizations = await _focusHelper.GetFocusedSchools(),
            EducationOrganizationId = incomingRequest.EducationOrganizationId,
            StudentUniqueId = incomingRequest.Student?.Student?.StudentNumber,
            FirstName = incomingRequest.Student?.Student?.FirstName,
            MiddleName = incomingRequest.Student?.Student?.MiddleName,
            LastSurname = incomingRequest.Student?.Student?.LastName,
            BirthDate = incomingRequest.Student?.Student?.Birthdate!.Value.ToString("yyyy-MM-dd"),
            Gender = incomingRequest.Student?.Student?.Gender,
            Grade = incomingRequest.Student?.Student?.Grade,
            Additional = JsonSerializer.Serialize(incomingRequest.Student?.Connectors),
            ToDistrict = requestManifest?.To?.District,
            ToSchool = requestManifest?.To?.School,
            Note = requestManifest?.Note,
            Contents = requestManifest?.Contents?.Select(x => x.FileName).ToList(),
            RequestStatus = incomingRequest.RequestStatus,
            States = States.GetSelectList(),
            Genders = Genders.GetSelectList(),
            ReceivingAttachments = incomingRequest.PayloadContents?.Where(x => incomingRequest.RequestManifest!.Contents!.Select(y => y.FileName).Contains(x.FileName)).ToList(),
            ReleasingAttachments = (incomingRequest.ResponseManifest is not null) ? incomingRequest.PayloadContents?.Where(x => incomingRequest.ResponseManifest!.Contents!.Select(y => y.FileName).Contains(x.FileName)).ToList() : null,
            DraftAttachments = incomingRequest.PayloadContents?.Where(x => x.MessageId == null).ToList()
        };

        return View(viewModel);
    }

    [HttpPut]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(CreateIncomingRequestViewModel viewModel)
    {
        var district = JsonSerializer.Deserialize<District>(Request.Form["ToDistrict"]!, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (district is not null)
        {
            district.Schools = null;
        }
        var school = JsonSerializer.Deserialize<School>(Request.Form["ToSchool"]!, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        if (ModelState.IsValid)
        {            
            var incomingRequest = await _incomingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(viewModel.RequestId));

            Guard.Against.Null(incomingRequest);

            incomingRequest.EducationOrganizationId = viewModel.EducationOrganizationId!.Value;

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

            incomingRequest.Student = new StudentRequest()
            {
                Student = student,
                Connectors = (jsonConnector is not null) ? new Dictionary<string, object>
                    { 
                        [jsonConnector.Keys.First()] = jsonConnector.GetValueOrDefault(jsonConnector.Keys.First())!
                    } : null
            };

            incomingRequest.RequestManifest = new Manifest() {
                RequestType = typeof(StudentCumulativeRecord).FullName!,
                Student = student,
                Note = viewModel.Note,
                To = new RequestAddress()
                {
                    District = district,
                    School = school
                }
            };
            incomingRequest.RequestStatus = viewModel.RequestStatus;

            await _incomingRequestRepository.UpdateAsync(incomingRequest);

            await _payloadContentService.AddPayloadContentsAsync(viewModel.Files, viewModel.RequestId);

            TempData[VoiceTone.Positive] = $"Updated request for {viewModel.FirstName} {viewModel.LastSurname} ({incomingRequest.Id}).";

            return RedirectToAction(nameof(Update), new { id = incomingRequest.Id });
        }

        viewModel.EducationOrganizations = await _focusHelper.GetFocusedSchools();
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
        var incomingRequest = await _incomingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(requestId));

        Guard.Against.Null(incomingRequest);

        foreach (var formFile in files)
        {
            if (formFile.Length > 0)
            {
                var file = await FileHelpers
                        .ProcessFormFile<BufferedSingleFileUploadDb>(formFile, ModelState, new string[] { ".png", ".txt", ".pdf" }, 2097152);
                
                var payloadContent = new PayloadContent()
                {
                    Request = incomingRequest,
                    ContentType = formFile.ContentType,
                    BlobContent = file,
                    FileName = formFile.FileName
                };
                
                await _payloadContentRepository.AddAsync(payloadContent);
            }
        }

        return RedirectToAction(nameof(Update), new { id = requestId });
    }

    [HttpDelete]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(Guid requestId, Guid payloadContentId)
    {
        var incomingRequest = await _incomingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(requestId));

        Guard.Against.Null(incomingRequest);

        var payloadContentToDelete = await _payloadContentRepository.GetByIdAsync(payloadContentId);

        Guard.Against.Null(payloadContentToDelete);

        await _payloadContentRepository.DeleteAsync(payloadContentToDelete);

        return RedirectToAction(nameof(Update), new { id = requestId });
    }

    [HttpPut]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(Guid id)
    {
        var incomingRequest = await _incomingRequestRepository.FirstOrDefaultAsync(new RequestByIdwithEdOrgs(id));

        Guard.Against.Null(incomingRequest);

        incomingRequest.RequestStatus = RequestStatus.WaitingToSend;
        incomingRequest.RequestProcessUserId = _currentUser.AuthenticatedUserId();

        var manifestWithFrom = await _manifestService.AddFrom(incomingRequest, _currentUser.AuthenticatedUserId()!.Value);

        incomingRequest.RequestManifest = manifestWithFrom;

        await _incomingRequestRepository.UpdateAsync(incomingRequest);

        TempData[VoiceTone.Positive] = $"Request marked to send ({incomingRequest.Id}).";
        return RedirectToAction(nameof(Update), new { id = id });
    }
}

public class BufferedSingleFileUploadDb
{
    [Required]
    [Display(Name="File")]
    public IFormFile? FormFile { get; set; }

    [Display(Name="Note")]
    [StringLength(50, MinimumLength = 0)]
    public string? Note { get; set; }
}