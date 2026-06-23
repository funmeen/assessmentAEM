using Microsoft.Extensions.Logging;
using PlatformWellSync.Data;
using PlatformWellSync.DTOs;
using PlatformWellSync.Models;

namespace PlatformWellSync.Services;

public class SyncService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SyncService> _logger;

    public SyncService(AppDbContext db, ILogger<SyncService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SyncAsync(List<PlatformApiResponse> platforms)
    {
        int platformsInserted = 0, platformsUpdated = 0;
        int wellsInserted = 0, wellsUpdated = 0;

        foreach (var platform in platforms)
        {
            if (!platform.Id.HasValue)
            {
                _logger.LogWarning("Skipping platform — Id is null.");
                continue;
            }

            // ---- PLATFORM UPSERT ----
            var existingPlatform = await _db.Platforms.FindAsync(platform.Id.Value);
            if (existingPlatform == null)
            {
                _db.Platforms.Add(new Platform
                {
                    Id = platform.Id.Value,
                    PlatformName = platform.UniqueName
                });
                platformsInserted++;
                _logger.LogInformation("INSERT Platform Id={Id} Name={Name}", platform.Id, platform.UniqueName);
            }
            else
            {
                existingPlatform.PlatformName = platform.UniqueName ?? existingPlatform.PlatformName;
                platformsUpdated++;
                _logger.LogInformation("UPDATE Platform Id={Id} Name={Name}", platform.Id, platform.UniqueName);
            }

            // ---- WELLS UPSERT ----
            if (platform.Wells == null || platform.Wells.Count == 0)
            {
                _logger.LogWarning("Platform {Name} has no wells.", platform.UniqueName);
                continue;
            }

            foreach (var well in platform.Wells)
            {
                if (!well.Id.HasValue || !well.PlatformId.HasValue)
                {
                    _logger.LogWarning("Skipping well — Id or PlatformId is null.");
                    continue;
                }

                var existingWell = await _db.Wells.FindAsync(well.Id.Value);
                if (existingWell == null)
                {
                    _db.Wells.Add(new Well
                    {
                        Id = well.Id.Value,
                        PlatformId = well.PlatformId.Value,
                        UniqueName = well.UniqueName,
                        Latitude = well.Latitude,
                        Longitude = well.Longitude,
                        CreatedAt = well.CreatedAt,
                        UpdatedAt = well.GetUpdatedAt()
                    });
                    wellsInserted++;
                    _logger.LogInformation("INSERT Well Id={Id} Name={Name}", well.Id, well.UniqueName);
                }
                else
                {
                    existingWell.PlatformId = well.PlatformId.Value;
                    existingWell.UniqueName = well.UniqueName ?? existingWell.UniqueName;
                    existingWell.Latitude = well.Latitude ?? existingWell.Latitude;
                    existingWell.Longitude = well.Longitude ?? existingWell.Longitude;
                    existingWell.CreatedAt = well.CreatedAt ?? existingWell.CreatedAt;
                    existingWell.UpdatedAt = well.GetUpdatedAt() ?? existingWell.UpdatedAt;
                    wellsUpdated++;
                    _logger.LogInformation("UPDATE Well Id={Id} Name={Name}", well.Id, well.UniqueName);
                }
            }
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Sync complete. Platforms: {PI} inserted, {PU} updated. Wells: {WI} inserted, {WU} updated.",
            platformsInserted, platformsUpdated, wellsInserted, wellsUpdated);
    }
}