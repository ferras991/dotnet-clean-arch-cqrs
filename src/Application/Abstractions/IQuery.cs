using Domain;

namespace Application.Abstractions;

public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }