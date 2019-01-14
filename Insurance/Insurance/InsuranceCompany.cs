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
            policy.ValidFrom < validFrom.AddMonths(validMonths) &&
            policy.ValidTill > validFrom))
                throw new ArgumentException("Impossible sell some policy to one insured object in overlapping period.");

            #endregion

            var validTill = validFrom.AddMonths(validMonths);
            var newPolicy = new Policy(
                nameOfInsuredObject,
                validFrom,
                validTill,
                CalculatePremium(selectedRisks, validFrom, validTill),
                selectedRisks);
            SoldPolicy.Add(newPolicy);

            return newPolicy;
        }

        private static decimal CalculatePremium(
            IList<Risk> risks,
            DateTime validFrom,
            DateTime validTill)
        {
            var sumRiskPremiumPerYear = risks.Sum(risk => risk.YearlyPrice);
            var riskPremiumPerDay = Math.Round(sumRiskPremiumPerYear / 365, 2, MidpointRounding.AwayFromZero);
            var countOfPolicyDays = (validTill - validFrom).Days;

            return riskPremiumPerDay * countOfPolicyDays;
        }

        public void AddRisk(string nameOfInsuredObject, Risk risk, DateTime validFrom, DateTime effectiveDate)
        {
            var policy = GetPolicy(nameOfInsuredObject, effectiveDate);

            #region Validation

            if (policy == null)
                throw new ArgumentException("Impossible add risk to nonexistent policy.");

            if (!AvailableRisks.Contains(risk))
                throw new ArgumentException("This risk not from avaliable list.");

            if (validFrom < DateTime.Now)
                throw new ArgumentException("Valid date must be equal to or greater than date now.");

            if (policy.ValidFrom > validFrom)
                throw new ArgumentException("Valid date must be equal to or greater than date of policy.");

            if (policy.ValidTill < validFrom)
                throw new ArgumentException("Valid date must be equal to or greater than date of policy.");

            #endregion

            policy.InsuredRisks.Add(risk);
            policy.Premium = CalculatePremium(policy.InsuredRisks, policy.ValidFrom, policy.ValidTill);
        }

        public void RemoveRisk(string nameOfInsuredObject, Risk risk, DateTime validTill, DateTime effectiveDate)
        {
            var policy = GetPolicy(nameOfInsuredObject, effectiveDate);

            #region Validation

            if (policy == null)
                throw new ArgumentException("Impossible delete risk from nonexistent policy.");

            if (validTill < DateTime.Now)
                throw new ArgumentException("Valid date must be equal to or greater than date now.");

            if (policy.ValidFrom > validTill)
                throw new ArgumentException("Valid date must be equal to or greater than date of policy.");

            if (policy.ValidTill < validTill)
                throw new ArgumentException("Valid date must be equal to or greater than date of policy.");

            if (!policy.InsuredRisks.Contains(risk))
                throw new ArgumentException("Can't remove nonexistent risk from policy.");
            
            #endregion

            policy.InsuredRisks.Remove(risk);
            policy.Premium = CalculatePremium(policy.InsuredRisks, policy.ValidFrom, policy.ValidTill);
        }

        public IPolicy GetPolicy(string nameOfInsuredObject, DateTime effectiveDate)
        {
            #region Validation

            if (nameOfInsuredObject == null)
                throw new ArgumentException("Name of insured object can't be null.");

            if (nameOfInsuredObject == string.Empty)
                throw new ArgumentException("Name of insured object can't be empty.");

            #endregion

            return SoldPolicy.FirstOrDefault(policy =>
                policy.NameOfInsuredObject == nameOfInsuredObject &&
                (policy.ValidFrom <= effectiveDate ||
                policy.ValidTill >= effectiveDate));
        }
    }
}