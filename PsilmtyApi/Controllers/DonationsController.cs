using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PsilmtyApi.Helpers;
using PsilmtyApi.Interfaces.Repositories;

namespace PsilmtyApi.Controllers;

[Authorize]
[ApiController]
[Route("api/donaciones")]
public sealed class DonationsController(IDatabaseRepository repository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get() => Ok(await repository.QueryAsync<object>("""
        SELECT d.id Id,d.user_id UserId,CONCAT_WS(' ',u.first_name,u.last_name) UserName,
               d.amount Amount,d.currency Currency,d.type Type,d.concept Concept,
               d.payment_method PaymentMethod,d.is_anonymous IsAnonymous,d.status Status,d.donated_at DonatedAt
        FROM donations d LEFT JOIN users u ON u.id=d.user_id
        WHERE d.parish_id=@ParishId AND d.is_active=1 ORDER BY d.donated_at DESC
        """, new { ParishId = User.GetParishId() }));

    [HttpGet("resumen")]
    public async Task<IActionResult> GetSummary() => Ok(await repository.QuerySingleAsync<object>("""
        SELECT COALESCE(SUM(amount),0) Total,
               COALESCE(SUM(IF(type='tithe',amount,0)),0) TotalTithe,
               COALESCE(SUM(IF(type='offering',amount,0)),0) TotalOffering,
               COUNT(*) Count
        FROM donations WHERE parish_id=@ParishId AND is_active=1 AND status='completed'
        """, new { ParishId = User.GetParishId() }));

    [HttpPost]
    public async Task<IActionResult> Create(Dictionary<string, object?> request)
    {
        var id = await repository.ExecuteScalarAsync<uint>("""
            INSERT INTO donations
                (parish_id,user_id,amount,currency,type,concept,payment_method,is_anonymous,status,donated_at,is_active,created_at,created_by)
            VALUES
                (@ParishId,@UserId,@Amount,'MXN',@Type,@Concept,@PaymentMethod,@IsAnonymous,'completed',UTC_TIMESTAMP(),1,UTC_TIMESTAMP(),@CreatorId);
            SELECT LAST_INSERT_ID();
            """, new
        {
            ParishId = User.GetParishId(), UserId = request.GetValueOrDefault("userId"),
            Amount = request.GetValueOrDefault("amount"), Type = request.GetValueOrDefault("type") ?? "offering",
            Concept = request.GetValueOrDefault("concept"),
            PaymentMethod = request.GetValueOrDefault("paymentMethod") ?? "cash",
            IsAnonymous = request.GetValueOrDefault("isAnonymous") ?? false, CreatorId = User.GetUserId()
        });
        return Ok(new { id });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(uint id)
    {
        await repository.ExecuteAsync(
            "UPDATE donations SET is_active=0,updated_at=UTC_TIMESTAMP(),updated_by=@UserId WHERE id=@Id AND parish_id=@ParishId",
            new { Id = id, UserId = User.GetUserId(), ParishId = User.GetParishId() });
        return NoContent();
    }
}
