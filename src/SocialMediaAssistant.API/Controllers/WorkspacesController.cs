using Microsoft.AspNetCore.Mvc;
using SocialMediaAssistant.API.Dtos;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;



namespace SocialMediaAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkspacesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public WorkspacesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllForAdmin()
        {
            var workspaces = await _unitOfWork.Workspaces.GetAllAsync();
            var workspacesDto = workspaces.Select(w => new WorkspaceDto
            {
                Id = w.Id,
                UserId = w.UserId,
                BrandName = w.BrandName,
                Industry = w.Industry,
                TargetAudience = w.TargetAudience,
                BrandTone = w.BrandTone,
                Keywords = w.Keywords,
                CreatedAt = w.CreatedAt,
                User = w.User == null ? null : new UserDto
                {
                    Id = w.User.Id,
                    Name = w.User.Name,
                    Email = w.User.Email
                }
            });
            return Ok(workspacesDto);
        }
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyWorkspaces()
        {
            var userId = GetCurrentUserId();
            var workspaceSummaries = await _unitOfWork.Workspaces.GetSummariesByUserIdAsync(userId);

            var workspacesDto = workspaceSummaries.Select(s => new WorkspaceDto
            {
                Id = s.Id,
                BrandName = s.BrandName,
                Industry = s.Industry,
                PostCount = s.PostCount,
                AccountCount = s.AccountCount
            });

            return Ok(workspacesDto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdAsync(id);
            if (workspace == null) return NotFound();
            if (workspace.UserId != GetCurrentUserId()) return Forbid();


            var workspaceDto = new WorkspaceDto
            {
                Id = workspace.Id,
                UserId = workspace.UserId,
                BrandName = workspace.BrandName,
                Industry = workspace.Industry,
                TargetAudience = workspace.TargetAudience,
                BrandTone = workspace.BrandTone,
                Keywords = workspace.Keywords,
                CreatedAt = workspace.CreatedAt,
                User = workspace.User == null ? null : new UserDto
                {
                    Id = workspace.User.Id,
                    Name = workspace.User.Name,
                    Email = workspace.User.Email
                },
                ConnectedAccounts = workspace.ConnectedAccounts.Select(acc => new ConnectedAccountDto
                {
                    Id = acc.Id,
                    WorkspaceId = acc.WorkspaceId,
                    Platform = acc.Platform,
                    PlatformUsername = acc.PlatformUsername,
                    CreatedAt = acc.CreatedAt
                }).ToList()
            };
            return Ok(workspaceDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateWorkspaceDto workspaceDto)
        {
            var userIdString = GetCurrentUserId();

            var newWorkspace = new Workspace
            {
                BrandName = workspaceDto.BrandName,
                UserId = userIdString,
                Industry = workspaceDto.Industry ?? "",
                TargetAudience = workspaceDto.TargetAudience ?? "",
                BrandTone = workspaceDto.BrandTone ?? "",
                Keywords = workspaceDto.Keywords ?? ""
            };

            await _unitOfWork.Workspaces.AddAsync(newWorkspace);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetById), new { id = newWorkspace.Id }, newWorkspace);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var workspace = await _unitOfWork.Workspaces.GetByIdAsync(id);
            if (workspace == null) return NotFound();

            _unitOfWork.Workspaces.Delete(workspace);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkspaceDto workspaceDto)
        {
            var userId = GetCurrentUserId();

            var workspaceToUpdate = await _unitOfWork.Workspaces.GetByIdAsync(id);

            if (workspaceToUpdate == null || workspaceToUpdate.UserId != userId)
            {
                return Forbid("Bu çalışma alanını güncelleme yetkiniz yok.");
            }

            workspaceToUpdate.BrandName = workspaceDto.BrandName;
            workspaceToUpdate.Industry = workspaceDto.Industry ?? workspaceToUpdate.Industry;
            workspaceToUpdate.TargetAudience = workspaceDto.TargetAudience ?? workspaceToUpdate.TargetAudience;
            workspaceToUpdate.BrandTone = workspaceDto.BrandTone ?? workspaceToUpdate.BrandTone;
            workspaceToUpdate.Keywords = workspaceDto.Keywords ?? workspaceToUpdate.Keywords;

            _unitOfWork.Workspaces.Update(workspaceToUpdate);

            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdString!);
        }
    }
}