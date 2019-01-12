using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace Insurance.Tests
{
    [TestFixture]
    public class InsuranceCompanyTests
    {
        private IInsuranceCompany _testInsuranceCompany;
        private const string InsuredObjectName = "TestCompany";
        private IList<Risk> _risks;
        private Risk _testRisk;

        [SetUp]
        public void Setup()
        {
            _testRisk = new Risk {Name = "RiskName", YearlyPrice = 120};
            _risks = new List<Risk> {_testRisk};

            _testInsuranceCompany = CreateInsuranceCompany(InsuredObjectName);
            _testInsuranceCompany.AvailableRisks = _risks;
        }

        private IInsuranceCompany CreateInsuranceCompany(string name)
        {
            return new InsuranceCompany(name);
        }

        [Test]
        public void ProvidesNameFromConstructor()
        {
            Assert.AreEqual(InsuredObjectName, _testInsuranceCompany.Name);
        }

        [Test]
        public void ReturnesNewPolicyWhenSellPolicyInvoked()
        {
            var sellPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                DateTime.Now, 
                1,
                new List<Risk> {_testRisk});

            Assert.IsNotNull(sellPolicy);
        }

        [Test]
        public void DoesNotSellPolicyWithoutRisks()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
            _testInsuranceCompany.SellPolicy(
                      InsuredObjectName,
                      DateTime.Now,
                      1,
                      null));

            Assert.That(ex.Message, Is.EqualTo("Selected risks can't be null."));
        }

        [Test]
        public void DoesNotSellPolicyWithEmptyRisks()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
            _testInsuranceCompany.SellPolicy(
                      InsuredObjectName,
                      DateTime.Now,
                      1,
                      new List<Risk>()));

            Assert.That(ex.Message, Is.EqualTo("Selected risks can't be empty."));
        }

        [Test]
        public void DoesNotSellPolicyWithRiskNotFromAvaliableRisksList()
        {
            var v = new ArgumentException();
            var risk = new Risk();

            var ex = Assert.Throws<ArgumentException>(() =>
            _testInsuranceCompany.SellPolicy(
                      InsuredObjectName,
                      DateTime.Now,
                      1,
                      new List<Risk> { risk }));

            Assert.That(ex.Message, Is.EqualTo("This risk not from avaliable list."));
        }

        [Test]
        public void DoesNotSellPolicyWhenValidFromDateLessThanNow()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.SellPolicy(
                      InsuredObjectName,
                      DateTime.Now.AddMinutes(-1),
                      1,
                      new List<Risk> {_testRisk}));

            Assert.That(ex.Message, Is.EqualTo("Valid from date can't be less than now."));
        }

        [Test]
        public void DoesNotSellPolicyWhenValidMounthLessThanOne()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.SellPolicy(
                      InsuredObjectName,
                      DateTime.Now,
                      0,
                      new List<Risk> { _testRisk }));

            Assert.That(ex.Message, Is.EqualTo("Valid mounth can't be less than one."));
        }

        [Test]
        public void DoesNotSellPolicyWithNullInsuredObject()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.SellPolicy(
                      null,
                      DateTime.Now,
                      1,
                      new List<Risk> { _testRisk }));

            Assert.That(ex.Message, Is.EqualTo("Name of insured object can't be null."));
        }

        [Test]
        public void DoesNotSellPolicyWithEmptyInsuredObjectName()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.SellPolicy(
                      string.Empty,
                      DateTime.Now,
                      1,
                      new List<Risk> { _testRisk }));

            Assert.That(ex.Message, Is.EqualTo("Name of insured object can't be empty."));
        }

        [Test]
        public void ProvidesPolicyParamsWhenSold()
        {
            var validFrom = DateTime.Now;

            var soldPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                validFrom,
                1,
                new List<Risk> { _testRisk });

            Assert.AreEqual(InsuredObjectName, soldPolicy.NameOfInsuredObject);
            Assert.AreEqual(validFrom, soldPolicy.ValidFrom);
            Assert.AreEqual(1, soldPolicy.InsuredRisks.Count);
            Assert.AreEqual(_testRisk, soldPolicy.InsuredRisks.First());
        }

        [Test]
        public void ProvidesCorrectValidTillDateInSoldPolicy()
        {
            var validFrom = DateTime.Now;
            var validMonthCount = (short)2;
            var validTill = validFrom.AddMonths(validMonthCount);

            var soldPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                validFrom,
                validMonthCount,
                new List<Risk> { _testRisk });

            Assert.AreEqual(validTill, soldPolicy.ValidTill);
        }

        [Test]
        public void ProvidesCorrectPremiumInSoldPolicy()
        {
            var fireRisk = new Risk
            {
                Name = "Fire rist",
                YearlyPrice = 140M
            };
            var expectedPremium = _testRisk.YearlyPrice + fireRisk.YearlyPrice;
            _testInsuranceCompany.AvailableRisks.Add(fireRisk);
            var validFrom = DateTime.Now;

            var soldPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                validFrom,
                1,
                new List<Risk> { _testRisk, fireRisk });

            Assert.AreEqual(expectedPremium, soldPolicy.Premium);
        }

        [Test]
        public void DoesNotSellTwoPolicyForOneInsuredObjectInSamePeriod()
        {
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                DateTime.Now,
                2,
                new List<Risk> { _testRisk });

            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.SellPolicy(
                      InsuredObjectName,
                      DateTime.Now,
                      1,
                      new List<Risk> { _testRisk }));

            Assert.That(ex.Message, Is.EqualTo("Impossible sell some policy to one insured object in same period."));
        }
    }
}
