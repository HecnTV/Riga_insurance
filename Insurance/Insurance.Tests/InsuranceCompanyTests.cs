using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Insurance.Tests
{
    [TestClass]
    public class InsuranceCompanyTests
    {
        private IInsuranceCompany _testInsuranceCompany;
        private const string Name = "TestCompany";
        private IList<Risk> _risks;
        private Risk _testRisk;

        [TestInitialize]
        public void Setup()
        {
            _testRisk = new Risk {Name = "RiskName", YearlyPrice = 120};
            _risks = new List<Risk> {_testRisk};

            _testInsuranceCompany = CreateInsuranceCompany(Name);
            _testInsuranceCompany.AvailableRisks = _risks;
        }

        private IInsuranceCompany CreateInsuranceCompany(string name)
        {
            return new InsuranceCompany(name);
        }

        [TestMethod]
        public void ProvidesNameFromConstructor()
        {
            Assert.AreEqual(Name, _testInsuranceCompany.Name);
        }

        [TestMethod]
        public void ReturnesNewPolicyWhenSellPolicyInvoked()
        {
            var sellPolicy = _testInsuranceCompany.SellPolicy(
                string.Empty,
                DateTime.Now, 
                0,
                new List<Risk> {_testRisk});

            Assert.IsNotNull(sellPolicy);
        }

        [TestMethod]
        public void DoesSellPolicyWithRiskNotFromAvaliableRisksList()
        {
            Exception resultExcetpion = null;
            var v = new ArgumentException();

            var risk = new Risk();
            try
            {
                var sellPolicy = _testInsuranceCompany.SellPolicy(
                    string.Empty,
                    DateTime.Now,
                    0,
                    new List<Risk> { risk });
            }
            catch (Exception exception)
            {
                resultExcetpion = exception;
            }

            Assert.AreEqual(v, resultExcetpion);   
        }
    }
}
