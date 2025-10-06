using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaAssistant.Core.Entities;
using SocialMediaAssistant.Core.Enums;
using SocialMediaAssistant.Core.Interfaces;
using System.Security.Claims;
using SocialMediaAssistant.Application.Interfaces;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ContentPostsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly IContentGenerationService _contentGenerationService;
    
    private readonly ICoinService _coinService;


    public ContentPostsController(IUnitOfWork unitOfWork,IContentGenerationService contentGenerationService,ICoinService coinService)
    {
        _unitOfWork = unitOfWork;
        _contentGenerationService = contentGenerationService;
        _coinService = coinService;
    }

    [HttpPost("generate-preview")]
    public async Task<IActionResult> GeneratePreview(CreateContentPostDto postDto)
    {
        var userId = GetCurrentUserId();
        var workspace = await _unitOfWork.Workspaces.GetByIdAsync(postDto.WorkspaceId);
        if (workspace == null || workspace.UserId != GetCurrentUserId())
            return Forbid("Bu çalışma alanına içerik ekleme yetkiniz yok.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (!await _coinService.HasSufficientCoins(userId, postDto.ContentType))
        {
            return BadRequest(new { message = "Bu içerik türü için yeterli krediniz (coin) bulunmamaktadır." });
        }

        var generatedContent = await _contentGenerationService.GenerateContentAsync(postDto.UserPrompt, workspace);

        return Ok(generatedContent);
    }

     [HttpPost("save")]
    public async Task<IActionResult> SavePost(SaveContentPostDto postDto)
    {
        var userId = GetCurrentUserId();
        var workspace = await _unitOfWork.Workspaces.GetByIdAsync(postDto.WorkspaceId);
        if (!await IsUserOwnerOfWorkspace(postDto.WorkspaceId))
            return Forbid("Bu çalışma alanına içerik kaydetme yetkiniz yok.");

        try
        {
            await _coinService.DeductCoinsForAction(userId, postDto.ContentType);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        var newPost = new ContentPost
        {
            WorkspaceId = postDto.WorkspaceId,
            UserPrompt = postDto.UserPrompt,
            GeneratedText = postDto.GeneratedText,
            GeneratedAssetUrl = postDto.GeneratedAssetUrl,
            ContentType = postDto.ContentType,
            AssetType = AssetType.Image,
            Status = PostStatus.Draft
        };

        await _unitOfWork.ContentPosts.AddAsync(newPost);
        await _unitOfWork.CompleteAsync();

        var postToReturn = new ContentPostDto
        {
            Id = newPost.Id,
            WorkspaceId = newPost.WorkspaceId,
            UserPrompt = newPost.UserPrompt,
            GeneratedText = newPost.GeneratedText,
            GeneratedAssetUrl = newPost.GeneratedAssetUrl,
            AssetType = newPost.AssetType,
            ContentType = newPost.ContentType,
            Status = newPost.Status,
            ScheduledAt = newPost.ScheduledAt,
            CreatedAt = newPost.CreatedAt,
        };

        return Ok(postToReturn);
    }
    [HttpPatch("{id}/schedule")]
    public async Task<IActionResult> SchedulePost(Guid id, [FromBody] SchedulePostDto scheduleDto)
    {
        if (scheduleDto.ScheduledAt <= DateTime.UtcNow)
        {
            return BadRequest(new { message = "Zamanlama tarihi geçmiş bir tarih olamaz." });
        }

        var post = await _unitOfWork.ContentPosts.GetByIdAsync(id);
        if (post == null) return NotFound();

        if (!await IsUserOwnerOfWorkspace(post.WorkspaceId))
            return Forbid("Bu içeriği zamanlama yetkiniz yok.");

        if (post.Status != PostStatus.Draft)
        {
            return BadRequest(new { message = "Sadece 'Taslak' durumundaki içerikler zamanlanabilir." });
        }

        // Post'un durumunu ve zamanlama tarihini güncelle
        post.Status = PostStatus.Scheduled;
        post.ScheduledAt = scheduleDto.ScheduledAt;

        _unitOfWork.ContentPosts.Update(post);
        await _unitOfWork.CompleteAsync();

        return NoContent(); // Başarılı, geri döndürülecek içerik yok.
    }
    [HttpGet("workspace/{workspaceId}")]
    public async Task<IActionResult> GetPostsForWorkspace(Guid workspaceId)
    {
        if (!await IsUserOwnerOfWorkspace(workspaceId))
            return Forbid("Bu çalışma alanına ait içerikleri görme yetkiniz yok.");

        var posts = await _unitOfWork.ContentPosts.GetPostsByWorkspaceIdAsync(workspaceId);
        var postsDto = posts.Select(p => new ContentPostDto
            {
                Id = p.Id,
                WorkspaceId = p.WorkspaceId,
                UserPrompt = p.UserPrompt,
                GeneratedText = p.GeneratedText,
                GeneratedAssetUrl = p.GeneratedAssetUrl,
                AssetType = p.AssetType,
                ContentType = p.ContentType,
                Status = p.Status,
                ScheduledAt = p.ScheduledAt,
                PublishedAt = p.PublishedAt,
                CreatedAt = p.CreatedAt,
            });

        return Ok(postsDto);
    }

    private async Task<bool> IsUserOwnerOfWorkspace(Guid workspaceId)
    {
        var userId = GetCurrentUserId();
        var workspace = await _unitOfWork.Workspaces.GetByIdAsync(workspaceId);
        return workspace != null && workspace.UserId == userId;
    }

    private Guid GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdString!);
    }
}