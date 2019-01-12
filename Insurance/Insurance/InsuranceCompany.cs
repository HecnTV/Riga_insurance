using System;
using System.Collections.Generic;
using System.Linq;

namespace Insurance
{
    public class InsuranceCompany : IInsuranceCompany
    {
        public InsuranceCompany(string name)
        {
            Name = name;

            SoldPolicy = new List<IPolicy>();
        }

        public string Name { get; }
        public IList<Risk> AvailableRisks { get; set; }

        private IList<IPolicy> SoldPolicy { get; } 

        public IPolicy SellPolicy(string nameOfInsuredObject,
            DateTime validFrom,
            short validMonths,
            IList<Risk> selectedRisks)
        {
            #region validation
            if (selectedRisks == null)
                throw new ArgumentException("Selected risks can't be null.");

            if (!selectedRisks.Any())
                throw new ArgumentException("Selected risks can't be empty.");

            if (!selectedRisks.All(risk => AvailableRisks.Contains(risk)))
                throw new ArgumentException("This risk not from avaliable list.");

            if (validFrom < DateTime.Now)
                throw new ArgumentException("Valid from date can't be less than now.");

            if (validMonths < 1)
                throw new ArgumentException("Valid mounth can't be less than one.");

            if (nameOfInsuredObject == null)
                throw new ArgumentException("Name of insured object can't be null.");

            if (nameOfInsuredObject == string.Empty)
                throw new ArgumentException("Name of insured object can't be empty.");

            if (SoldPolicy.Any(policy => policy.NameOfInsuredObject == nameOfInsuredObject &&
            policy.ValidFrom < validFrom &&
            policy.ValidTill > validFrom.AddMonths(validMonths)))
                throw new ArgumentException("Impossible sell some policy to one insured object in same period.");
            #endregion

            var newPolicy = new Policy(
                nameOfInsuredObject,
                validFrom,
                validFrom.AddMonths(validMonths),
                selectedRisks.Sum(risk => risk.YearlyPrice),
                selectedRisks);
            SoldPolicy.Add(newPolicy);

            return newPolicy;
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