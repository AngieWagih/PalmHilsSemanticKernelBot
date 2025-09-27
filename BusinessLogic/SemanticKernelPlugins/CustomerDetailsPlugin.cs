using Microsoft.SemanticKernel;
using PalmHilsSemanticKernelBot.Models;
using System.ComponentModel;

namespace PalmHilsSemanticKernelBot.BusinessLogic.SemanticKernelPlugins
{
    public class CustomerDetailsPlugin
    {

        /// <summary>
        /// Performs a specific operation
        /// </summary>
        /// <param name="input">Input parameter</param>
        [KernelFunction]
        [Description("Performs a specific operation")]
        public async Task<Customer> GetCustomerDetails(
            [Description("Input parameter")] string customerName, string customerPhoneNumber, string customerEmail)
        {
            try
            {
                return new();
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}
