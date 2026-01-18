namespace Carpooling.Application.Interfaces;

public interface IBookingEventPublisher
{
    void PublishBookingCreated(
        Guid bookingId,
        Guid rideId,
        Guid passengerId,
        string passengerEmail
    );
}
