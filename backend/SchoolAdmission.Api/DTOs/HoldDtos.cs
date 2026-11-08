namespace SchoolAdmission.Api.DTOs;

/// <summary>POST /classes/{classId}/holds -> 201 {holdId,classId,studentId,status,expiresAt}</summary>
public sealed record HoldCreatedResponse(
    Guid HoldId, Guid ClassId, string StudentId, string Status, DateTimeOffset ExpiresAt);

/// <summary>GET /holds/{holdId} -> {holdId,classId,status,expiresAt}; status HELD|CONFIRMED|CANCELLED|EXPIRED</summary>
public sealed record HoldResponse(Guid HoldId, Guid ClassId, string Status, DateTimeOffset ExpiresAt);

/// <summary>GET /me/holds -> [{holdId,classId,className,status,expiresAt}]</summary>
public sealed record MyHoldResponse(Guid HoldId, Guid ClassId, string ClassName, string Status, DateTimeOffset ExpiresAt);

/// <summary>POST /holds/{holdId}/confirm -> 200 {holdId,status}</summary>
public sealed record HoldConfirmedResponse(Guid HoldId, string Status);
