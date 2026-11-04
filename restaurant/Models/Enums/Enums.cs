namespace restaurant.Models.Enums
{
    public enum ReservationStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
        NoShow = 4
    }

    public enum OrderStatus
    {
        Placed = 0,
        Preparing = 1,
        ReadyToServe = 2,
        Served = 3,
        Completed = 4,
        Cancelled = 5
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Paid = 1,
        Refunded = 2,
        Failed = 3,
        // Used by Staff/Admin billing flows in the shared Azure DB
        Cancelled = 4
    }

    public enum PaymentMethod
    {
        Cash = 0,
        Card = 1,
        Online = 2,
        // Accept Staff/Admin values stored as E-Wallet / EWallet
        EWallet = 3
    }

    public enum NotificationType
    {
        Reservation = 0,
        Order = 1,
        Payment = 2,
        Promotion = 3,
        System = 4
    }
}
