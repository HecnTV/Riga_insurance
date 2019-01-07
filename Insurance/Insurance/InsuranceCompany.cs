using System;
using System.Collections.Generic;

namespace Insurance
{
    public class InsuranceCompany : IInsuranceCompany
    {
        public InsuranceCompany(string name)
        {
            //Name = name;
        }

        public string Name { get; }
        public IList<Risk> AvailableRisks { get; set; }

        public IPolicy SellPolicy(string nameOfInsuredObject,
            DateTime validFrom,
            short validMonths,
            IList<Risk> selectedRisks)
        {
            throw new NotImplementedException();
        }

        public void AddRisk(string nameOfInsuredObject, Risk risk, DateTime validFrom, DateTime effectiveDate)
        {
            throw new NotImplementedException();
        }

        public void RemoveRisk(string nameOfInsuredObject, Risk risk, DateTime validTill, DateTime effectiveDate)
        {
            throw new NotImplementedException();
        }

        public IPolicy GetPolicy(string nameOfInsuredObject, DateTime effectiveDate)
        {
            throw new NotImplementedException();
        }
    }
}