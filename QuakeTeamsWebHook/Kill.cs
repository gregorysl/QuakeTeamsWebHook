using System;

namespace QuakeTeamsWebHook
{
    public class Kill
    {
        public int Killer { get; set; }
        public int Killed { get; set; }
        public int Means { get; set; }

        public MeansOfDeath Meanenum()
        {
            MeansOfDeath aaa;
            var a = Enum.TryParse(Means.ToString(), out aaa);
            return aaa;
        }
    }
}