using BehaAtel.Application.Features.Charges.Common;
using MediatR;

namespace BehaAtel.Application.Features.Charges.GenerateMonthlyCharges;

public sealed record GenerateMonthlyChargesCommand(DateOnly Period) : IRequest<GenerateResultDto>;
