using System;
using System.Diagnostics;
using Gosocket.Dian.Services.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Gosocket.Dian.TestProject.Utils
{
    [TestClass]
    public class StringUtilTest
    {
        [TestMethod]
        public void TestSuccessTextAfter()
        {
            try
            {
                var number = StringUtil.TextAfter("CFC1", "CFC").TrimStart('0');
                Assert.IsTrue(!string.IsNullOrEmpty(number));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Assert.IsTrue(false);
            }
        }
    }
}
