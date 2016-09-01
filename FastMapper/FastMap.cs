namespace FastMapper
{
    public class FastMap
    {
        public static TypeMap<TS, TD> CreateMap<TS, TD>()
            where TS : class ,new()
            where TD : class ,new()
        {
            return new TypeMap<TS, TD>();
        }
    }
}