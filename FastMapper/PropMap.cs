using System;
using System.Linq.Expressions;
using System.Reflection;

namespace FastMapper
{
    public class PropMap
    {
        public bool IsIgnore { get; private set; }

        public void Ignore()
        {
            IsIgnore = true;
        }

        public string MapProp { get; set; }
        public MethodInfo MethodInfo { get; internal set; }
        public object Target { get; internal set; }
        public Expression ValueExpression { get; internal set; }
    }
}