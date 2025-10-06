using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Interfaces;
using System.Security.Claims;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ConnectedAccountsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ConnectedAccountsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet("workspace/{workspaceId}")]
    public async Task<IActionResult> GetAccountsForWorkspace(Guid workspaceId)
    {
        var userId = GetCurrentUserId();

        var workspace = await _unitOfWork.Workspaces.GetByIdAsync(workspaceId);
        if (workspace == null || workspace.UserId != userId)
        {
            return Forbid("Bu çalışma alanına erişim yetkiniz yok.");
        }

        var accounts = await _unitOfWork.ConnectedAccounts.GetAccountsByWorkspaceIdAsync(workspaceId);

        var accountsDto = accounts.Select(a => new ConnectedAccountDto
        {
            Id = a.Id,
            WorkspaceId = a.WorkspaceId,
            Platform = a.Platform,
            PlatformUsername = a.PlatformUsername,
            CreatedAt = a.CreatedAt
        });

        return Ok(accountsDto);
    }

    [HttpPost]
    public async Task<IActionResult> ConnectAccount(ConnectAccountDto connectAccountDto)
    {
        var userId = GetCurrentUserId();

        var workspace = await _unitOfWork.Workspaces.GetByIdAsync(connectAccountDto.WorkspaceId);
        if (workspace == null || workspace.UserId != userId)
        {
            return Forbid("Bu çalışma alanına hesap ekleme yetkiniz yok.");
        }

        var newAccount = new ConnectedAccount
        {
            WorkspaceId = connectAccountDto.WorkspaceId,
            Platform = connectAccountDto.Platform,
            PlatformUsername = connectAccountDto.PlatformUsername,
            AccessToken = $"simulated_token_for_{connectAccountDto.PlatformUsername}"
        };

        await _unitOfWork.ConnectedAccounts.AddAsync(newAccount);
        await _unitOfWork.CompleteAsync();

        var accountToReturn = new ConnectedAccountDto
        {
            Id = newAccount.Id,
            WorkspaceId = newAccount.WorkspaceId,
            Platform = newAccount.Platform,
            PlatformUsername = newAccount.PlatformUsername,
            CreatedAt = newAccount.CreatedAt
        };
        
        return Ok(accountToReturn);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DisconnectAccount(Guid id)
    {
        var userId = GetCurrentUserId();
        var account = await _unitOfWork.ConnectedAccounts.GetByIdAsync(id);

        if (account == null)
        {
            return NotFound();
        }

        var workspace = await _unitOfWork.Workspaces.GetByIdAsync(account.WorkspaceId);
        if (workspace == null || workspace.UserId != userId)
        {
            return Forbid("Bu hesabı silme yetkiniz yok.");
        }

        _unitOfWork.ConnectedAccounts.Delete(account);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdString!);
    }
}