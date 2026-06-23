-- ============================================================
-- PART 2: Last Updated Well for Each Platform
-- ============================================================
-- Returns the most recently updated well per platform,
-- matching the expected result format from the assessment.
-- ============================================================

SELECT
    p.PlatformName,
    w.Id,
    w.PlatformId,
    w.UniqueName,
    w.Latitude,
    w.Longitude,
    w.CreatedAt,
    w.UpdatedAt
FROM Well w
INNER JOIN Platform p ON p.Id = w.PlatformId
INNER JOIN (
    -- Subquery: find the MAX UpdatedAt per platform
    SELECT
        PlatformId,
        MAX(UpdatedAt) AS LastUpdatedAt
    FROM Well
    GROUP BY PlatformId
) latest ON w.PlatformId = latest.PlatformId
        AND w.UpdatedAt = latest.LastUpdatedAt
ORDER BY p.PlatformName;


-- ============================================================
-- ALTERNATIVE using ROW_NUMBER (handles ties cleanly)
-- ============================================================

SELECT
    PlatformName,
    Id,
    PlatformId,
    UniqueName,
    Latitude,
    Longitude,
    CreatedAt,
    UpdatedAt
FROM (
    SELECT
        p.PlatformName,
        w.Id,
        w.PlatformId,
        w.UniqueName,
        w.Latitude,
        w.Longitude,
        w.CreatedAt,
        w.UpdatedAt,
        ROW_NUMBER() OVER (
            PARTITION BY w.PlatformId
            ORDER BY w.UpdatedAt DESC
        ) AS rn
    FROM Well w
    INNER JOIN Platform p ON p.Id = w.PlatformId
) ranked
WHERE rn = 1
ORDER BY PlatformName;
