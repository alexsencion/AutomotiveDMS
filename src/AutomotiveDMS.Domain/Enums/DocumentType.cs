using System;
using System.Collections.Generic;
using System.Text;

namespace AutomotiveDMS.Domain.Enums
{
    public enum DocumentType
    {
        VehiclePhoto = 1,
        VehicleTitle = 2,

        CustomerIdFront = 10,
        CustomerIncome = 11,

        FinancingContract = 20,
        PaymentSchedulePdf = 21,
        PaymentReceipt = 22,

        PromissoryNote = 30,
        SignedPromissoryNote = 31

    }
}
