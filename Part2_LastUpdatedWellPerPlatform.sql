SELECT p.PlatformName, w.Id, w.PlatformId,
       w.UniqueName, w.Latitude, w.Longitude,
       w.CreatedAt, w.UpdatedAt
FROM Well w
INNER JOIN Platform p ON p.Id = w.PlatformId
INNER JOIN (
    SELECT PlatformId, MAX(UpdatedAt) AS LastUpdatedAt
    FROM Well
    GROUP BY PlatformId
) latest ON w.PlatformId = latest.PlatformId
       AND w.UpdatedAt = latest.LastUpdatedAt
ORDER BY p.PlatformName;