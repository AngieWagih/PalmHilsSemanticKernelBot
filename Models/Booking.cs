namespace PalmHilsSemanticKernelBot.Models
{
    public class Booking
    {
        public Booking(int id, DayOfWeek day, string date, ScheduleTimeSlot scheduleTimeSlot, bool isBooked, AppointmentType appointmentType, int? customerId, string? customerName, string? customerEmail, bool? hasAttended)
        {
            Id = id;
            Day = day;
            Date = date;
            ScheduleTimeSlot = scheduleTimeSlot;
            IsBooked = isBooked;
            AppointmentType = appointmentType;
            CustomerId = customerId;
            CustomerName = customerName;
            CustomerEmail = customerEmail;
            HasAttended = hasAttended;
        }

        public int Id { get; set; }
        public DayOfWeek Day { get; set; }
        public string Date { get; set; }
        public ScheduleTimeSlot ScheduleTimeSlot { get; set; }
        public bool IsBooked { get; set; }
        public AppointmentType AppointmentType { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public bool? HasAttended { get; set; }
    }

    public class ScheduleTimeSlot
    {
        public ScheduleTimeSlot(double startTime, double endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        public double StartTime { get; set; }
        public double EndTime { get; set; }
    }
    public enum AppointmentType
    {
        Survey,
        Viewing,
        Consultation
    }
}
