using System.Collections.Generic;
using System.Linq.Expressions;

namespace BaseSolution.Core.Commons.Yesmarket.Support
{
    internal interface IExpressionCollection : IEnumerable<Expression>
    {
        void Fill();
    }
}