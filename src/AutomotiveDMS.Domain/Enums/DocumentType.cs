using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Enums
{
    public enum DocumentType
    {
        VehiclePhoto = 1,
        VehicleInspection = 2,
        VehicleTitle = 3,

        CustomerId = 10,
        CustomerIncome = 11,
        CustomerAddress = 12,

        FinancingContract = 20,
        PaymentSchedulePdf = 21,
        PaymentReceipt = 22,

        PromissoryNote = 30,
        SignedPromissoryNote = 31

    }
}
