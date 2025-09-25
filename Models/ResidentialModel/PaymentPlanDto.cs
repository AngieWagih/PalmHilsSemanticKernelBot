namespace PalmHilsSemanticKernelBot.Models.ResidentialModel
{
    public class PaymentPlanDto
    {
        public int PaymentPlanId { get; set; }
        public string PlanName { get; set; }
        public double? DownPaymentAmount { get; set; }
        public double? DownPaymentPercentage { get; set; }
        public int? InstallmentMonths { get; set; }
        public double? MonthlyInstallment { get; set; }
        public double? InterestRate { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }
}
