using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extension.tests
{
    public class FakeTypeLocator<T> : ITypeLocator
    {
        public IReadOnlyList<Type> GetTypes()
        {
            return new Type[] { typeof(T) };
        }
    }
}
