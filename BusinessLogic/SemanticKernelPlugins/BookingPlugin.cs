using Dapper;
using Microsoft.SemanticKernel;
using PalmHilsSemanticKernelBot.Models;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;

namespace PalmHilsSemanticKernelBot.BusinessLogic.SemanticKernelPlugins
{
    public class BookingPlugin
    {
        private readonly IDbConnection Connection;

        public BookingPlugin(IDbConnection connection)
        {
            Connection = connection;
        }

        [KernelFunction]
        [Description("Validates that the user's requested booking date is valid by checking it is not in the past and not on a vacation/holiday")]
        public async Task<bool> IsValidBooking(
             [Description("The booking date requested by the user (in string format)")] string userRequestedBookingDate)
        {
            try
            {
                if (!DateTime.TryParse(userRequestedBookingDate, out DateTime bookingDate))
                {
                    return false;
                }
                DateTime currentDate = DateTime.Today;
                DayOfWeek dayOfWeek = bookingDate.DayOfWeek;

                if (bookingDate < currentDate || (dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday))
                {
                    return false;
                }
                return true;
            }
            catch (Exception)
            {

                throw;
            }

        }

        /* [KernelFunction]
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
 */

        [KernelFunction]
        [Description("Saves a new booking/appointment to the database for a specific customer")]
        public async Task<bool> SaveBookingForCustomer(
     [Description("The booking date requested by the user")] string userRequestedBookingDate,
     [Description("Unique identifier for the customer making the booking")] string customerId,
     [Description("Type of appointment being booked")] AppointmentType appointmentType)
        {
            try
            {
                // Parse the booking date
                if (!DateTime.TryParse(userRequestedBookingDate, out DateTime bookingDate))
                {
                    return false; // Invalid date format
                }
                var updateQuery = @"
                      UPDATE Bookings 
                      SET CustomerID = @CustomerId, 
                      IsBooked = @IsBooked
                      Where Date(Bookings.Day) = @BookingDate  ";

                var parameters = new
                {
                    CustomerId = customerId,
                    IsBooked = true,
                    BookingDate = bookingDate.Date
                };

                var rowsAffected = await Connection.ExecuteAsync(updateQuery, parameters);

                return rowsAffected > 0;
            }
            catch (Exception)
            {

                throw;
            }

        }


    }
}
