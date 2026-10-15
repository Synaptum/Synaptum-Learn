-- Verify grade auditing and identify the user
-- responsible for the most recent modification.

SELECT
    g.Id,
    g.Name,
    g.CreatedByUserId,
    g.LastModifiedByUserId,
    g.LastModifiedAt,
    u.FirstName,
    u.LastName
FROM dbo.Grades AS g
    LEFT JOIN dbo.Users AS u
    ON g.LastModifiedByUserId = u.Id
ORDER BY g.Name;
