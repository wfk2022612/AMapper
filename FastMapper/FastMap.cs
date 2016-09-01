namespace FastMapper
{
    public class FastMap
    {
        public static TypeMap<TS, TD> CreateMap<TS, TD>()
            where TS : new()
            where TD : new()
        {
            return new TypeMap<TS, TD>();
        }
    }
}