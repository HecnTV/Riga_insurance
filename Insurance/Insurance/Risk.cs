using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance
{
    public struct Risk
    {
        /// <summary>
        /// Unique name of the risk
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Risk yearly price
        /// </summary>
        public decimal YearlyPrice { get; set; }
    }
}
