using Carpooling.Application.DTOs.Ride;
using FluentValidation;

namespace Carpooling.API.Validators;

public class CreateRideDtoValidator : AbstractValidator<CreateRideDto>
{
    public CreateRideDtoValidator()
    {
        RuleFor(x => x.FromCity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ToCity).NotEmpty().MaximumLength(100);
        RuleFor(x => x.AvailableSeats).GreaterThan(0);
        RuleFor(x => x.PricePerSeat).GreaterThanOrEqualTo(0);
        RuleFor(x => x.RideDate).GreaterThan(DateTime.UtcNow);
    }
}
