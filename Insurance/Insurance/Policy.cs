using System;
using System.Collections.Generic;

namespace Insurance
{
    public class Policy : IPolicy
    {
        public Policy(
            string nameOfInsuredObject,
            DateTime validFrom,
            DateTime validTill,
            decimal premium,
            IList<Risk> insuredRisks)
        {
            NameOfInsuredObject = nameOfInsuredObject;
            ValidFrom = validFrom;
            ValidTill = validTill;
            Premium = premium;
            InsuredRisks = insuredRisks;
        }

        public string NameOfInsuredObject { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTill { get; set; }
        public decimal Premium { get; set; }
        public IList<Risk> InsuredRisks { get; set; }
    }
}