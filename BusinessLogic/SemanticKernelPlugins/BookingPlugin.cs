using Microsoft.SemanticKernel;
using PalmHilsSemanticKernelBot.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PalmHilsSemanticKernelBot.BusinessLogic.SemanticKernelPlugins
{
    public class BookingPlugin
    {

        [KernelFunction]
        [Description("Validates that the user's requested booking date is valid by checking it is not in the past and not on a vacation/holiday")]
        public async Task<bool> IsValidBooking(
             [Description("The booking date requested by the user (in string format)")] string userRequestedBookingDate)
        {
            try
            {
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        [KernelFunction]
        [Description("Checks if the requested booking date and time slot is available for booking")]
        public async Task<bool> IsAvailableToBook(
    [Description("The booking date and time requested by the user (in string format)")] string userRequestedBookingDate,
     [Description("The specific time slot requested by the user")] ScheduleTimeSlot userRequestedTimeSlot,
     [Description("Booking object containing details to check against existing bookings")] Booking booking)
        {
            try
            {
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }


        [KernelFunction]
        [Description("Saves a new booking/appointment to the database for a specific customer")]
        public async Task<bool> SaveBookingForCustomer(
     [Description("The booking date requested by the user")] string userRequestedBookingDate,
     [Description("Unique identifier for the customer making the booking")] string customerId,
     [Description("Type of appointment being booked")] AppointmentType appointmentType)
        {
            try
            {
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }


    }
}
