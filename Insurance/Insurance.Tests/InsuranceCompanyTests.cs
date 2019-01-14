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
        private DateTime _testDate;

        [SetUp]
        public void Setup()
        {
            _testRisk = new Risk {Name = "RiskName", YearlyPrice = 120};
            _risks = new List<Risk> {_testRisk};

            _testDate = DateTime.Now.AddHours(1);

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
                _testDate, 
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
                      _testDate,
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
                      _testDate,
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
                      _testDate,
                      1,
                      new List<Risk> { risk }));

            Assert.That(ex.Message, Is.EqualTo("This risk not from avaliable list."));
        }

        [Test]
        public void DoesNotSellPolicyWhenValidFromDateLessThanNow()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.SellPolicy(
                      InsuredObjectName,
                      _testDate.AddDays(-1),
                      1,
                      new List<Risk> {_testRisk}));

            Assert.That(ex.Message, Is.EqualTo("Valid from date can't be less than now."));
        }

        [Test]
        public void DoesNotSellPolicyWhenValidMounthLessThanOne()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.SellPolicy(
                      InsuredObjectName,
                      _testDate,
                      0,
                      new List<Risk> { _testRisk }));

            Assert.That(ex.Message, Is.EqualTo("Valid mounth can't be less than one."));
        }

        [Test]
        public void DoesNotSellPolicyWithNullInsuredObject()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.SellPolicy(
                      null,
                      _testDate,
                      1,
                      new List<Risk> { _testRisk }));

            Assert.That(ex.Message, Is.EqualTo("Name of insured object can't be null."));
        }

        [Test]
        public void DoesNotSellPolicyWithEmptyInsuredObjectName()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.SellPolicy(
                      string.Empty,
                      _testDate,
                      1,
                      new List<Risk> { _testRisk }));

            Assert.That(ex.Message, Is.EqualTo("Name of insured object can't be empty."));
        }

        [Test]
        public void ProvidesPolicyParamsWhenSold()
        {
            var validFrom = _testDate;

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
            var validFrom = _testDate;
            var validMonthCount = (short)2;
            var validTill = validFrom.AddMonths(validMonthCount);

            var soldPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                validFrom,
                validMonthCount,
                new List<Risk> { _testRisk });

            Assert.AreEqual(validTill, soldPolicy.ValidTill);
        }

        //140+120=260 per year
        //260/365=0.71 per day
        [Test]
        public void ProvidesCorrectPremiumBySoldPolicy()
        {
            var fireRisk = new Risk
            {
                Name = "Fire rist",
                YearlyPrice = 140M
            };
            var timeSpan = _testDate.AddMonths(1) - _testDate;
            var expectedPremium = Convert.ToDecimal(0.71*timeSpan.Days);

            _testInsuranceCompany.AvailableRisks.Add(fireRisk);
            var validFrom = _testDate;

            var soldPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                validFrom,
                1,
                new List<Risk> { _testRisk, fireRisk });

            Assert.AreEqual(expectedPremium, soldPolicy.Premium);
        }

        [Test]
        public void DoesNotSellTwoPolicyForOneInsuredObjectInOverlappingPeriod()
        {
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                2,
                new List<Risk> { _testRisk });

            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.SellPolicy(
                      InsuredObjectName,
                      _testDate,
                      1,
                      new List<Risk> { _testRisk }));

            Assert.That(ex.Message, Is.EqualTo("Impossible sell some policy to one insured object in overlapping period."));
        }

        [Test]
        public void FindsPolicyByInsuredObjectNameAndEffectiveDate()
        {
            var validFromDate = _testDate;
            var effectiveDate = validFromDate.AddDays(2);
            var soldPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                validFromDate,
                2,
                new List<Risk> { _testRisk });

            var foundPolicy = _testInsuranceCompany.GetPolicy(InsuredObjectName, effectiveDate);

            Assert.AreEqual(soldPolicy, foundPolicy);
        }

        [Test]
        public void ProvidesExceptionWhenTryFindPolicyWithNullName()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.AddRisk(
                      null,
                      _testRisk,
                      _testDate.AddDays(2),
                      _testDate.AddDays(1)));

            Assert.That(ex.Message, Is.EqualTo("Name of insured object can't be null."));
        }

        [Test]
        public void ProvidesExceptionWhenTryFindPolicyWithEmptyName()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.AddRisk(
                      string.Empty,
                      _testRisk,
                      _testDate.AddDays(2),
                      _testDate.AddDays(1)));

            Assert.That(ex.Message, Is.EqualTo("Name of insured object can't be empty."));
        }

        [Test]
        public void ReturnsNullWhenTryFindsPolicyAndDoNotFindIt()
        {
            var validFromDate = _testDate;
            var effectiveDate = validFromDate.AddDays(2);
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                validFromDate,
                2,
                new List<Risk> { _testRisk });

            var foundPolicy = _testInsuranceCompany.GetPolicy("Empty", effectiveDate);

            Assert.IsNull(foundPolicy);
        }

        [Test]
        public void ProvidesExceptionWhenTryAddRiskToNonexistentPolicy()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.AddRisk(
                InsuredObjectName,
                _testRisk,
                _testDate.AddDays(2),
                _testDate.AddDays(1)));

            Assert.That(ex.Message, Is.EqualTo("Impossible add risk to nonexistent policy."));
        }

        [Test]
        public void ProvidesExceptionWhenTryAddRiskNotFromAvaliableRisksList()
        {
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                2,
                new List<Risk> { _testRisk });

            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.AddRisk(
                      InsuredObjectName,
                      new Risk(), 
                      _testDate.AddDays(2),
                      _testDate.AddDays(1)));

            Assert.That(ex.Message, Is.EqualTo("This risk not from avaliable list."));
        }

        [Test]
        public void ProvideExceptionWhenTryAddRiskWithValidDateLesserThanNow()
        {
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                2,
                new List<Risk> { _testRisk });

            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.AddRisk(
                      InsuredObjectName,
                      _testRisk,
                      DateTime.Now.AddDays(-2),
                      _testDate.AddDays(1)));

            Assert.That(ex.Message, Is.EqualTo("Valid date must be equal to or greater than date now."));
        }

        [Test]
        public void ValidDateMustBeGreaterOrEqualThenPolicyDate()
        {
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate.AddDays(3),
                2,
                new List<Risk> { _testRisk });

            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.AddRisk(
                      InsuredObjectName,
                      _testRisk,
                      _testDate.AddDays(2),
                      _testDate.AddDays(4)));

            Assert.That(ex.Message, Is.EqualTo("Valid date must be equal to or greater than date of policy."));
        }

        [Test]
        public void ValidDateMustBeLessesThenPolicyValidTillDate()
        {
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                2,
                new List<Risk> { _testRisk });

            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.AddRisk(
                      InsuredObjectName,
                      _testRisk,
                      _testDate.AddMonths(3),
                      _testDate.AddDays(1)));

            Assert.That(ex.Message, Is.EqualTo("Valid date must be equal to or greater than date of policy."));
        }

        [Test]
        public void AddsRiskToPolicy()
        {
            var fireRisk = new Risk {Name = "Fire", YearlyPrice = 140M};
            _testInsuranceCompany.AvailableRisks.Add(fireRisk);
            var soldPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                2,
                new List<Risk> { _testRisk });

            _testInsuranceCompany.AddRisk(
                InsuredObjectName,
                fireRisk,
                _testDate,
                _testDate);

            Assert.IsTrue(soldPolicy.InsuredRisks.Contains(fireRisk));
        }

        [Test]
        public void ReculculatesPremiumAfterAddRiskToPolicy()
        {
            var fireRisk = new Risk { Name = "Fire", YearlyPrice = 140 };
            var validMonths = (short)2;
            var timeSpan = _testDate.AddMonths(validMonths) - _testDate;
            var expectedPremium = Convert.ToDecimal(0.71*timeSpan.Days);
            _testInsuranceCompany.AvailableRisks.Add(fireRisk);

            var soldPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                validMonths,
                new List<Risk> { _testRisk });

            _testInsuranceCompany.AddRisk(
                InsuredObjectName,
                fireRisk,
                _testDate,
                _testDate);

            Assert.AreEqual(expectedPremium, soldPolicy.Premium);
        }

        [Test]
        public void ProvidesExceptionWhenTryRemoveRiskToNonexistentPolicy()
        {
            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.RemoveRisk(
                InsuredObjectName,
                _testRisk,
                _testDate.AddDays(2),
                _testDate.AddDays(1)));

            Assert.That(ex.Message, Is.EqualTo("Impossible delete risk from nonexistent policy."));
        }

        [Test]
        public void ProvideExceptionWhenTryRemoveRiskWithTillDateLesserThanNow()
        {
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                2,
                new List<Risk> { _testRisk });

            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.RemoveRisk(
                      InsuredObjectName,
                      _testRisk,
                      DateTime.Now.AddDays(-2),
                      _testDate.AddDays(1)));

            Assert.That(ex.Message, Is.EqualTo("Valid date must be equal to or greater than date now."));
        }

        [Test]
        public void TillDateMustBeGreaterOrEqualThenPolicyFromDate()
        {
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate.AddDays(3),
                2,
                new List<Risk> { _testRisk });

            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.RemoveRisk(
                      InsuredObjectName,
                      _testRisk,
                      _testDate.AddDays(2),
                      _testDate.AddDays(4)));

            Assert.That(ex.Message, Is.EqualTo("Valid date must be equal to or greater than date of policy."));
        }

        [Test]
        public void TillDateMustBeLessesThenPolicyValidTillDate()
        {
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                2,
                new List<Risk> { _testRisk });

            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.RemoveRisk(
                      InsuredObjectName,
                      _testRisk,
                      _testDate.AddMonths(3),
                      _testDate.AddDays(1)));

            Assert.That(ex.Message, Is.EqualTo("Valid date must be equal to or greater than date of policy."));
        }

        [Test]
        public void ProvidesExceptionWhenTryRemoveNonexistentRisk()
        {
            var fireRisk = new Risk { Name = "Fire", YearlyPrice = 140M };
            _testInsuranceCompany.AvailableRisks.Add(fireRisk);
            _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                2,
                new List<Risk> { _testRisk });

            var ex = Assert.Throws<ArgumentException>(() => _testInsuranceCompany.RemoveRisk(
                InsuredObjectName,
                fireRisk,
                _testDate,
                _testDate));

            Assert.That(ex.Message, Is.EqualTo("Can't remove nonexistent risk from policy."));
        }

        [Test]
        public void RemovesRiskFromPolicy()
        {
            var fireRisk = new Risk { Name = "Fire", YearlyPrice = 140M };
            _testInsuranceCompany.AvailableRisks.Add(fireRisk);
            var soldPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                2,
                new List<Risk> { _testRisk, fireRisk });

            _testInsuranceCompany.RemoveRisk(
                InsuredObjectName,
                fireRisk,
                _testDate,
                _testDate);

            Assert.IsFalse(soldPolicy.InsuredRisks.Contains(fireRisk));
        }
        
        //120=260 per year
        //120/365=0.33 per day
        [Test]
        public void ReculculatesPremiumAfterRemoveRiskFromPolicy()
        {
            var fireRisk = new Risk { Name = "Fire", YearlyPrice = 140M };
            var validMonths = (short)2;
            var timeSpan = _testDate.AddMonths(validMonths) - _testDate;
            var expectedPremium = Convert.ToDecimal(0.33*timeSpan.Days);
            _testInsuranceCompany.AvailableRisks.Add(fireRisk);

            var soldPolicy = _testInsuranceCompany.SellPolicy(
                InsuredObjectName,
                _testDate,
                2,
                new List<Risk> { _testRisk, fireRisk });

            _testInsuranceCompany.RemoveRisk(
                InsuredObjectName,
                fireRisk,
                _testDate,
                _testDate);
            
            Assert.AreEqual(expectedPremium, soldPolicy.Premium);
        }
    }
}
