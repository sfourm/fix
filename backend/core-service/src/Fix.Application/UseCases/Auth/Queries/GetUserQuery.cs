using Fix.Application.Abstractions.Context;
using Fix.Application.Abstractions.Messaging;
using Fix.Application.Auth.Dtos;

namespace Fix.Application.Auth.Queries;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserDto>, IUserRequest;
