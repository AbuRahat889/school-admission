namespace SchoolAdmission.Api.DTOs;

/// <summary>POST /classes/{classId}/holds -> 201 {holdId,classId,studentId,status,expiresAt}</summary>
public sealed record HoldCreatedResponse(
    Guid HoldId, Guid ClassId, string StudentId, string Status, DateTimeOffset ExpiresAt);
