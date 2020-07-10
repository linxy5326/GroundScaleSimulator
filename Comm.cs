using System.Text.RegularExpressions;

namespace xabg.Core
{

    /// <summary>
    /// 公共类
    /// </summary>
    public class Common
    {

        /// <summary>   
        /// 匹配decimal(n,m)  
        /// </summary>   
        /// <param name="tb"></param>
        /// <returns></returns>     
        public static bool StringIsDecimal(string number)
        {
            return Regex.IsMatch(number, @"^\d{1,n-m}(?:\.\d{1,m})?$");
        }


    }
}
