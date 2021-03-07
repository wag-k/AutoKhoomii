using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArrayOperation
{
    public class MinMax
    {
        public static T Max<T>(params T[] nums) where T : IComparable
        {
            if(nums.Length == 0) return default(T);

            T max = nums[0];
            for(int i = 1; i < nums.Length; i++)
            {
                max = max.CompareTo(nums[i]) > 0 ? max : nums[i];
                // Minの場合は不等号を逆にすればOK
            }
            return max;
        }
        public static T Min<T>(params T[] nums) where T : IComparable
        {
            if(nums.Length == 0) return default(T);

            T min = nums[0];
            for(int i = 1; i < nums.Length; i++)
            {
                min = min.CompareTo(nums[i]) < 0 ? min : nums[i];
                // Minの場合は不等号を逆にすればOK
            }
            return min;
        }
    }
}
